using IATClient.Messages;
using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Xml;
using System.Xml.Serialization;

namespace IATClient
{
    class CWebSocketUploader
    {
        private CancellationTokenSource cancellationTokenSource { get; set; }
        private IATConfigMainForm MainForm;
        private String DataPassword, AdminPassword, _IATName;
        private CIAT IAT;
        private List<MemoryStream> Surveys = new List<MemoryStream>(), SASurveys = new List<MemoryStream>();
        private MemoryStream ConfigFileXML, UniqueRespXML;
        private ClientWebSocket UploadTestWebSocket;
        private enum EDeploymentStage { requestingConnection = 0, shakingHands = 1, queryingDuplicateIAT = 2, requestingDeployment = 3, performingDeployment = 4 };
        private EDeploymentStage deploymentStage;
        private Manifest Manifest;
        private int ClientID { get; set; } = -1;
        private IATConfig.ConfigFile CF;
        private ManualResetEvent UploadSuccess = new ManualResetEvent(false), UploadFailed = new ManualResetEvent(false), UploadCancelled = new ManualResetEvent(false);
        private CRSAKeyPair keyPair = null;
        private object transmissionLock = new object();
        private CancellationToken AbortToken = new CancellationToken();
        private ArraySegment<byte> ReceiveBuffer = new ArraySegment<byte>(new byte[8192]);
        private Envelope IncomingMessage;
        private long UploadTimeMillis = -1, DeploymentSessionID = -1;
        private List<Tuple<String, MemoryStream>> SurveyImages = new List<Tuple<String, MemoryStream>>();

        public String IATName
        {
            get
            {
                return _IATName;
            }
        }

        public void OnCancel(object sender, EventArgs e)
        {
            try
            {
                if (UploadTestWebSocket.State == WebSocketState.Open)
                {
                    TransactionRequest trans = new TransactionRequest();
                    if (DeploymentSessionID != -1)
                    {
                        trans.Transaction = TransactionRequest.ETransaction.HaltTestDeployment;
                        trans.LongValues["DeploymentId"] = DeploymentSessionID;
                    }
                    else
                        trans.Transaction = TransactionRequest.ETransaction.AbortTransaction;
                    Envelope env = new Envelope(trans);
                    env.SendMessage(UploadTestWebSocket, AbortToken);
                } else
                    UploadCancelled.Set();
            }
            catch (Exception) { }
        }

        void OperationFailed(String msg, String caption)
        {
            MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), msg, caption);
            UploadFailed.Set();
        }

        public CWebSocketUploader(CIAT iat, IATConfigMainForm mainForm)
        {
            IAT = iat;
            MainForm = mainForm;
        }

        private void ConstructDeploymentStream()
        {
            CF.UploadTimeMillis = UploadTimeMillis;
            CF.Name = IATName;
            CF.ClientID = ClientID;
            foreach (var s in CIAT.SaveFile.IAT.BeforeSurvey)
                CF.BeforeSurveys.Add(s);
            foreach (var s in CIAT.SaveFile.IAT.AfterSurvey)
                CF.AfterSurveys.Add(s);
            CF.UniqueResponse = CIAT.SaveFile.IAT.UniqueResponse;
            ConfigFileXML = new MemoryStream();
            XmlTextWriter xWriter = new XmlTextWriter(ConfigFileXML, Encoding.UTF8);
            xWriter.WriteStartDocument();
            CF.WriteXml(xWriter);
            xWriter.Flush();
        }

        private void BuildFileManifest()
        {
            Manifest = new Manifest();
            Manifest.ClientId = ClientID;
            Manifest.AddFile(new ManifestFile(IATName, ConfigFileXML.Length)
            {
                ResourceType = ManifestFile.EResourceType.testConfiguration,
                ResourceId = 0
            });
            Manifest.AddFiles(CF.IATImages.ConstructFileManifest(ManifestFile.EResourceType.image));
            Manifest.AddFiles(CF.SlideImages.ConstructFileManifest(ManifestFile.EResourceType.itemSlide));
            CF.ErrorMarkID = Manifest.Contents.Cast<ManifestFile>().Where(f => f.ResourceType == ManifestFile.EResourceType.errorMark)
                .Select(f => f.ResourceId).ToList().First();
        }

        private bool SendDeploymentFiles(long deploymentId, String sessionId)
        {
            MemoryStream[] streams = new MemoryStream[3];
            streams[0] = ConfigFileXML;
            /*            for (int ctr = 0; ctr < Surveys.Count; ctr++)
                        {
                            memStream.Write(Surveys[ctr].ToArray(), 0, (int)Surveys[ctr].Length);
                            memStream.Write(SASurveys[ctr].ToArray(), 0, (int)SASurveys[ctr].Length);
                        }*/
            var memStream = new MemoryStream();
            byte[][] ImageData = CF.IATImages.GetImageData();
            for (int ctr = 0; ctr < ImageData.Length; ctr++)
                memStream.Write(ImageData[ctr], 0, ImageData[ctr].Length);
            streams[1] = memStream;
            memStream = new MemoryStream();
            ImageData = CF.SlideImages.GetImageData();
            for (int ctr = 0; ctr < ImageData.Length; ctr++)
                memStream.Write(ImageData[ctr], 0, ImageData[ctr].Length);
            streams[2] = memStream;
            try
            {
                WebClient c = new WebClient();
                c.Headers["deploymentId"] = deploymentId.ToString();
                c.Headers["sessionId"] = sessionId;
                c.UploadData(Properties.Resources.sDeploymentUploadURL + "/configuration", streams[0].ToArray());
                c.UploadData(Properties.Resources.sDeploymentUploadURL + "/images", streams[1].ToArray());
                c.UploadData(Properties.Resources.sDeploymentUploadURL + "/itemSlides", streams[2].ToArray());
                foreach (var s in streams)
                    s.Dispose();
                return true;
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.ProtocolError)
                {
                    ErrorReporter.ReportError(new CReportableException("Failed to upload IAT", ex));
                    UploadFailed.Set();
                }
                return false;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Failed to upload IAT", ex));
                UploadFailed.Set();
                return false;
            }
            finally
            {
                memStream.Dispose();
            }
        }

        public void OnRSAKeyPairReceipt(INamedXmlSerializable keyPair)
        {
            this.keyPair = (CRSAKeyPair)keyPair;
            TransactionRequest trans = new TransactionRequest();
            trans.IATName = IATName;
            trans.Transaction = TransactionRequest.ETransaction.RequestDataPasswordVerification;
            Envelope transmission = new Envelope(trans);
            transmission.SendMessage(UploadTestWebSocket, AbortToken);
        }

        private void ShakeHands(INamedXmlSerializable handshake)
        {
            HandShake hs = (HandShake)handshake;
            Envelope transmission = new Envelope(HandShake.CreateResponse(hs));
            transmission.SendMessage(UploadTestWebSocket, AbortToken);
            deploymentStage++;
        }

        private void OnDeploymentProgressUpdate(INamedXmlSerializable dpu)
        {
            DeploymentProgressUpdate update = (DeploymentProgressUpdate)dpu;
            if (update.DeploymentException != null)
            {
                ErrorReporter.ReportError(update.DeploymentException);
                UploadFailed.Set();
                return;
            }
            if (update.Stage == DeploymentProgressUpdate.EStage.mismatchedDeploymentDescriptors)
            {
                OperationFailed("The test you are attempting to upload differs from the test already on the server in ways that would cause discrepencies in the result set format. It cannot be deployed atop the existing test.", "Incompatible Result Sets");
                return;
            }
            MainForm.StatusMessage = update.StatusMessage;
            MainForm.SetProgressRange(update.ProgressMin, update.ProgressMax, update.CurrentProgress);
        }

        private void TransactionReceived(INamedXmlSerializable T)
        {
            TransactionRequest trans = (TransactionRequest)T;
            TransactionRequest outTrans = null;
            EDeploymentStage dStage = deploymentStage;
            DialogResult dResult;
            Envelope transmission = null;
            try
            {
                switch (trans.Transaction)
                {
                    case TransactionRequest.ETransaction.ClientFrozen:
                        OperationFailed(Properties.Resources.sClientFrozen, "Account Frozen");
                        break;

                    case TransactionRequest.ETransaction.RequestTransmission:
                        MainForm.StatusMessage = "Connection established";
                        ClientID = trans.ClientID;
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.IATExists;
                        transmission = new Envelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.ClientDeleted:
                        OperationFailed(Properties.Resources.sClientDeleted, "Account Deleted");
                        break;

                    case TransactionRequest.ETransaction.IATExists:
                        dResult = MainForm.DisplayYesNoMessageBox("An IAT with this name associated with your account already exists on the server. If you have made costmetic changes to your " +
                            "test that do not effect the format of the result set, such as changing the wording of instructions or correcting typos, you may attempt to redeploy your IAT " +
                            "atop the one on the server. Do you wish to try this?", "IAT Exists");
                        if (dResult == DialogResult.No)
                        {
                            MainForm.StatusMessage = String.Empty;
                            MainForm.SetProgressRange(0, 0, 0);
                            if (DeploymentSessionID != -1)
                            {
                                trans.Transaction = TransactionRequest.ETransaction.HaltTestDeployment;
                                trans.LongValues["DeploymentId"] = DeploymentSessionID;
                            }
                            else
                                trans.Transaction = TransactionRequest.ETransaction.AbortTransaction;
                            Envelope env = new Envelope(trans);
                            env.SendMessage(UploadTestWebSocket, AbortToken);
                            UploadCancelled.Set();
                            return;
                        }
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                        transmission = new Envelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.TestBeingDeployed:
                        dResult = MainForm.DisplayYesNoMessageBox("An IAT with this name is currently in a state of deployment. If you have recently tried to upload a test " +
                            "with this name, likely that deployment has been suspended. You may only upload this test if you abandon the previous deployment. Do you wish to do so?", "Test Being Deployed");
                        if (dResult == DialogResult.No)
                        {
                            MainForm.StatusMessage = String.Empty;
                            MainForm.SetProgressRange(0, 0, 0);
                            outTrans = new TransactionRequest();
                            outTrans.Transaction = TransactionRequest.ETransaction.AbortDeployment;
                            outTrans.LongValues["DeploymentId"] = trans.LongValues["DeploymentId"];
                            Envelope env = new Envelope(outTrans);
                            env.SendMessage(UploadTestWebSocket, AbortToken);
                            UploadCancelled.Set();
                            return;
                        }
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.HaltTestDeployment;
                        outTrans.LongValues["DeploymentId"] = trans.LongValues["DeploymentId"];
                        transmission = new Envelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.DeploymentHalted:
                        UploadCancelled.Set();
                        break;

                    case TransactionRequest.ETransaction.VerifyPassword:
                        String encValue = trans.StringValues["EncryptedTestString"], value;
                        try
                        {
                            this.keyPair.DataKey.DecryptKey(DataPassword);
                            RSAParameters RSAParams = this.keyPair.DataKey.GetRSAParameters();
                            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
                            RSA.ImportParameters(RSAParams);
                            value = Convert.ToBase64String(RSA.Decrypt(Convert.FromBase64String(encValue), false));
                        }
                        catch (Exception ex)
                        {
                            OperationFailed("The password you supplied does not match the original password for this IAT.", "Invalid Password");
                            return;
                        }
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IATName;
                        outTrans.Transaction = TransactionRequest.ETransaction.VerifyPassword;
                        outTrans.StringValues["DecryptedTestString"] = value;
                        transmission = new Envelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.PasswordInvalid:
                        OperationFailed("The password you supplied does not match the original password for this IAT.", "Invalid Password");
                        break;

                    case TransactionRequest.ETransaction.PasswordValid:
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IATName;
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestIATRedeploy;
                        transmission = new Envelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.InsufficientIATS:
                        OperationFailed("You have met your maximum quota for the number of IATs you may have deployed on the server.", "Insufficient IATs Remaining");
                        break;

                    case TransactionRequest.ETransaction.NoSuchIAT:
                        MainForm.StatusMessage = "Requesting upload";
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.QueryRemainingIATS;
                        deploymentStage++;
                        transmission = new Envelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.QueryPublicityIAT:
                        outTrans = new TransactionRequest();
                        var publicityIATDialogResult = MainForm.DisplayYesNoMessageBox(Properties.Resources.sQueryPublicityIAT,
                            Properties.Resources.sQueryPublicityIATCaption);
                        if (publicityIATDialogResult == DialogResult.Yes)  
                        {
                            outTrans.Transaction = TransactionRequest.ETransaction.PublicityIAT;
                        }
                        else
                        {
                            outTrans.Transaction = TransactionRequest.ETransaction.RequestIATUpload;
                            outTrans.IATName = IAT.Name;
                        }
                        transmission = new Envelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.RemainingIATS:
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestIATUpload;
                        transmission = new Envelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.DeploymentDescriptorMismatch:
                        OperationFailed("The test you are attempting to upload differs from the test already on the server in ways that would cause discrepencies in the result set format. It cannot be deployed atop the existing test.", "Incompatible Result Sets");
                        break;

                    case TransactionRequest.ETransaction.TokenDefinitionReceived:
                        transmission = new Envelope(Manifest);
                        MainForm.StatusMessage = "Uploading file manifest";
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;
                    case TransactionRequest.ETransaction.EncryptionKeysReceived:
                        try
                        {
                            ConstructDeploymentStream();
                            BuildFileManifest();
                            if (IAT.TokenType != ETokenType.NONE)
                            {
                                transmission = new Envelope(new TokenDefinition(IAT.TokenType, IAT.TokenName));
                                transmission.SendMessage(UploadTestWebSocket, AbortToken);
                            }
                            else
                            {
                                MainForm.StatusMessage = "Uploading file manifest";
                                transmission = new Envelope(Manifest);
                                transmission.SendMessage(UploadTestWebSocket, AbortToken);
                            }
                        }
                        catch (NotImplementedException ex)
                        {
                            OperationFailed(ex.Message, "Operation Not Implemented");
                        }
                        break;

                    case TransactionRequest.ETransaction.DeploymentFileManifestReceived:
                        MainForm.StatusMessage = "Uploading test data";
                        SendDeploymentFiles(trans.LongValues["DeploymentId"], trans.StringValues["SessionId"]);
//                        SendItemSlides(trans.LongValues["DeploymentId"], trans.StringValues["SessionId"]);
                        break;

                    case TransactionRequest.ETransaction.RequestIATUpload:
                        throw new Exception("Bullshit Error");
                        MainForm.StatusMessage = "Initializing result encryption data";
                        UploadTimeMillis = trans.LongValues["DeploymentStartTime"];
                        DeploymentSessionID = trans.LongValues["DeploymentId"];
                        if (this.keyPair == null)
                        {
                            PartiallyEncryptedRSAData dataKey = new PartiallyEncryptedRSAData(PartiallyEncryptedRSAData.EKeyType.Data);
                            dataKey.Generate(DataPassword);
                            PartiallyEncryptedRSAData adminKey = new PartiallyEncryptedRSAData(PartiallyEncryptedRSAData.EKeyType.Admin);
                            adminKey.Generate(AdminPassword);
                            this.keyPair = new CRSAKeyPair(dataKey, adminKey);
                        }
                        transmission = new Envelope(this.keyPair);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.TransactionFail:
                        OperationFailed("Deployment failed", "Deployment failed");
                        break;

                    case TransactionRequest.ETransaction.BackupRestored:
                        OperationFailed("The redeployment of your test atop the test on the server failed but the existing test was preserved.", "Deployment Failed");
                        break;

                    case TransactionRequest.ETransaction.CannotRestoreBackup:
                        OperationFailed("The redeployment of your test atop the test on the server failed. Moreover, the backup of the test already on the server could not be restored.", "Deployment Failed");
                        break;

                    case TransactionRequest.ETransaction.InsufficientDiskSpace:
                        OperationFailed("You do not have sufficient remaining disk space on the server to upload your IAT", "Insufficient Disk Space");
                        break;

                    case TransactionRequest.ETransaction.TestFilesMissing:
                        OperationFailed("The deployment for this test is corrupt, likely due to a previous incomplete deployment. If this is not the case and if you have result data, it is likely you can still retrieve it. " +
                            "After doing so, you can delete this test and upload another.", "Test Deployment Corrupt");
                        break;

                    case TransactionRequest.ETransaction.TransactionSuccess:
                        CIATSummary IATSummary;
                        IATSummary = new CIATSummary(CF);
                        switch (IAT.TokenType)
                        {
                            case ETokenType.NONE:
                                IATSummary.IATLink = "http://" + Properties.Resources.sDefaultIATServerDomain + Properties.Resources.sDefaultIATServerPath + String.Format(Properties.Resources.sIATServletURLPart, IATName, ClientID);
                                break;

                            case ETokenType.VALUE:
                                IATSummary.IATLink = String.Format("http://{0}{1}{2}{3}", Properties.Resources.sDefaultIATServerDomain, Properties.Resources.sDefaultIATServerPath,
                                    String.Format(Properties.Resources.sIATServletURLPart, IATName, ClientID), String.Format("&{0}={1}", IAT.TokenName, "{text value}"));
                                break;

                            case ETokenType.HEX:
                                IATSummary.IATLink = String.Format("http://{0}{1}{2}{3}", Properties.Resources.sDefaultIATServerDomain, Properties.Resources.sDefaultIATServerPath,
                                    String.Format(Properties.Resources.sIATServletURLPart, IATName, ClientID), String.Format("&{0}={1}", IAT.TokenName, "{hexadecimal digits}"));
                                break;

                            case ETokenType.BASE64:
                                IATSummary.IATLink = String.Format("http://{0}{1}{2}{3}", Properties.Resources.sDefaultIATServerDomain, Properties.Resources.sDefaultIATServerPath,
                                    String.Format(Properties.Resources.sIATServletURLPart, IATName, ClientID), String.Format("&{0}={1}", IAT.TokenName, "{base64 digits}"));
                                break;

                            case ETokenType.BASE64_UTF8:
                                IATSummary.IATLink = String.Format("http://{0}{1}{2}{3}", Properties.Resources.sDefaultIATServerDomain, Properties.Resources.sDefaultIATServerPath,
                                    String.Format(Properties.Resources.sIATServletURLPart, IATName, ClientID), String.Format("&{0}={1}", IAT.TokenName, "{base64 encoded utf-8 text}"));
                                break;
                        }

                        IATSummary.DataRetrievalPassword = DataPassword;
                        IATSummary.AdminPassword = AdminPassword;
                        MainForm.OperationComplete(IATSummary);
                        UploadSuccess.Set();
                        break;

                }
            }
            catch (WebException)
            {
                UploadFailed.Set();
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Failed to upload IAT", ex));
                if (DeploymentSessionID != -1)
                {
                    trans.Transaction = TransactionRequest.ETransaction.AbortDeployment;
                    trans.LongValues["DeploymentId"] = DeploymentSessionID;
                }
                else
                    trans.Transaction = TransactionRequest.ETransaction.AbortTransaction;
                Envelope env = new Envelope(trans);
                env.SendMessage(UploadTestWebSocket, AbortToken);
                UploadFailed.Set();
            }
        }

        private void OnDeploymentException(INamedXmlSerializable ex)
        {
            UploadFailed.Set();
        }

        public bool Upload(String iatName, String password)
        {
            cancellationTokenSource = new CancellationTokenSource();
            IncomingMessage = null;
            UploadSuccess.Reset();
            UploadFailed.Reset();
            UploadCancelled.Reset();
            var hasher = new Rfc2898DeriveBytes(password,
                Convert.FromBase64String(LocalStorage.Activation[LocalStorage.Field.Salt]), 1000);
            var bytes = hasher.GetBytes(8);
            DataPassword = password;
            AdminPassword = password;
            _IATName = iatName;
            Envelope.ClearMessageMap();
            Envelope.OnReceipt[Envelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            Envelope.OnReceipt[Envelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(TransactionReceived);
            Envelope.OnReceipt[Envelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            Envelope.OnReceipt[Envelope.EMessageType.DeploymentProgress] = new Action<INamedXmlSerializable>(OnDeploymentProgressUpdate);
            Envelope.OnReceipt[Envelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(OnRSAKeyPairReceipt);
            MainForm.Invoke(new Action<EventHandler, IATConfigMainForm.EProgressBarUses>(MainForm.BeginProgressBarUse), new EventHandler(OnCancel), IATConfigMainForm.EProgressBarUses.Upload);
            UploadTestWebSocket = new ClientWebSocket();
            MainForm.StatusMessage = "Packaging Test";
            CF = new IATConfig.ConfigFile(IAT);
            CF.ServerDomain = Properties.Resources.sDefaultIATServerDomain;
            CF.ServerPath = Properties.Resources.sDefaultIATServerPath;
            CF.ServerPort = Convert.ToInt32(Properties.Resources.sDefaultIATServerPort);
            MainForm.StatusMessage = "Establishing connection";
            Task connectTask = null;
            try
            {
                connectTask = UploadTestWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), cancellationTokenSource.Token);
                connectTask.Wait(10000);
            }
            catch (AggregateException aggEx)
            {
                foreach (var ex in aggEx.InnerExceptions)
                    ErrorReporter.ReportError(new CReportableException("Error on test upload", ex));
                return false;
                if (UploadTestWebSocket.State == WebSocketState.Connecting)
                    cancellationTokenSource.Cancel();
            }
            if (!connectTask.IsCompleted)
            {
                if (UploadTestWebSocket.State == WebSocketState.Connecting)
                    cancellationTokenSource.Cancel();
                ErrorReporter.ReportError(new CReportableException("Timeout connecting to server for test upload", new TimeoutException()));
                return false;
            }
            Receive();
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            trans.IATName = IATName;
            Envelope env = new Envelope(trans);
            env.SendMessage(UploadTestWebSocket, AbortToken);
            int nTrigger = Task.Run<int>(new Func<int>(() =>
            {
                try
                {
                    return WaitHandle.WaitAny(new WaitHandle[] { UploadSuccess, UploadFailed, UploadCancelled });
                }
                catch (TaskCanceledException) { return 2; }
            }), CIAT.GetCancellationToken()).Result;
            Envelope.Shutdown();
            MainForm.Invoke(new Action(MainForm.EndProgressBarUse));
            String closeReason = String.Empty;
            switch (nTrigger)
            {
                case 0:
                    closeReason = "Test Deployed";
                    break;

                case 1:
                    closeReason = "Test Upload Failed";
                    break;

                case 2:
                    closeReason = "Test Upload Cancelled";
                    break;
            }
            if (UploadTestWebSocket.State == WebSocketState.Open)
            {
                try
                {
                    if (!UploadTestWebSocket.CloseAsync(WebSocketCloseStatus.InternalServerError, String.Empty, cancellationTokenSource.Token).Wait(1000))
                        cancellationTokenSource.Cancel();
                }
                finally
                {
                    UploadTestWebSocket.Dispose();
                }
            }
            MainForm.StatusMessage = String.Empty;
            return (nTrigger == 0);
        }
        private void Receive()
        {
            UploadTestWebSocket.ReceiveAsync(ReceiveBuffer, cancellationTokenSource.Token).ContinueWith(t => ReceiveMessage(t.Result), cancellationTokenSource.Token);
        }

        private void ReceiveMessage(WebSocketReceiveResult receipt)
        {
            try
            {
                if ((receipt.MessageType == WebSocketMessageType.Close) && (UploadTestWebSocket.State == WebSocketState.Open))
                    UploadTestWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close Requested by server", cancellationTokenSource.Token);
                if (receipt.Count != 0)
                {
                    lock (transmissionLock)
                    {
                        if (receipt.EndOfMessage)
                        {
                            if (IncomingMessage == null)
                                IncomingMessage = new Envelope();
                            if (IncomingMessage.QueueByteData(ReceiveBuffer.Array.Take(receipt.Count).ToArray(), true))
                                IncomingMessage = null;
                        }
                        else
                        {
                            if (IncomingMessage == null)
                                IncomingMessage = new Envelope();
                            IncomingMessage.QueueByteData(ReceiveBuffer.Array.Take(receipt.Count).ToArray(), false);
                        }
                    }
                }
                if (UploadTestWebSocket.State == WebSocketState.Open)
                    Receive();
            }
            catch (WebSocketException ex)
            {
                UploadFailed.Set();
                return;
            }
            catch (System.Net.Sockets.SocketException)
            {
                UploadFailed.Set();
                return;
            }
            catch (OperationCanceledException ex)
            {
                return;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Communication Error", ex));
                UploadFailed.Set();
            }
        }
    }
}
