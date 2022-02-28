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
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace IATClient
{
    class CWebSocketUploader
    {
        class DeploymentProcessException : Exception
        {
            public DeploymentProcessException() { }
        }


        private Action<int, int> SetProgressRange;
        private Func<String, String, DialogResult> DisplayYesNoMessageBox;
        private Action<CIATSummary> OperationComplete;
        private Action<int> ProgressIncrement;
        private Action ResetProgress;
        private Action<EventHandler, IATConfigMainForm.EProgressBarUses> BeginProgressBarUse;
        private Action EndProgressBarUse;
        private Action<String, String> DisplayMessageBox;
        private Action<int> OnSetProgressValue;
        private IATConfigMainForm MainForm;
        private object lockObject = new object();
        private String DataPassword, AdminPassword, _IATName;
        private CIAT IAT;
        private List<MemoryStream> Surveys = new List<MemoryStream>(), SASurveys = new List<MemoryStream>();
        private MemoryStream ConfigFileXML, UniqueRespXML;
        private ClientWebSocket UploadTestWebSocket;
        private enum EDeploymentStage { requestingConnection = 0, shakingHands = 1, queryingDuplicateIAT = 2, requestingDeployment = 3, performingDeployment = 4 };
        private EDeploymentStage deploymentStage;
        private Manifest Manifest;
        private int ClientID;
        private IATConfig.ConfigFile CF;
        private ManualResetEvent UploadSuccess = new ManualResetEvent(false), UploadFailed = new ManualResetEvent(false), UploadCancelled = new ManualResetEvent(false);
        private bool bAwaitingUpdates = false;
        private CRSAKeyPair keyPair = null;
        private object transmissionLock = new object();
        private String multipartBoundry = "===" + DateTime.Now.ToBinary().ToString() + "===";
        private CancellationToken AbortToken = new CancellationToken();
        private ArraySegment<byte> ReceiveBuffer = new ArraySegment<byte>(new byte[8192]);
        private Envelope IncomingMessage;
        private bool ConnectCancel = false;
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
                UploadCancelled.Set();
                ConnectCancel = true;
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
            }
            catch (Exception) { }
        }

        void OperationFailed(String msg, String caption)
        {
            MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), msg, caption);
            UploadFailed.Set();
        }

        private void SetStatusMessage(String msg)
        {
            MainForm.BeginInvoke(new Action<String>(MainForm.SetStatusMessage), msg);
        }

        public CWebSocketUploader(CIAT iat, IATConfigMainForm mainForm)
        {
            IAT = iat;
            MainForm = mainForm;
            SetProgressRange = new Action<int, int>(mainForm.SetProgressRange);
            DisplayYesNoMessageBox = new Func<String, String, DialogResult>(mainForm.OnDisplayYesNoMessageBox);
            OperationComplete = new Action<CIATSummary>(mainForm.OperationComplete);
            ProgressIncrement = new Action<int>(mainForm.ProgressIncrement);
            ResetProgress = new Action(mainForm.ResetProgress);
            BeginProgressBarUse = new Action<EventHandler, IATConfigMainForm.EProgressBarUses>(mainForm.BeginProgressBarUse);
            EndProgressBarUse = new Action(mainForm.EndProgressBarUse);
            DisplayMessageBox = new Action<String, String>(mainForm.OnDisplayMessageBox);
            OnSetProgressValue = new Action<int>(mainForm.SetProgressValue);
        }

        private void ProcessSurveys(String URL, int port)
        {
            Surveys.Clear();
            SASurveys.Clear();
            int surveyImgCtr = 0;
            for (int ctr1 = 0; ctr1 < IAT.BeforeSurvey.Count; ctr1++)
            {
                // store the schema-less XML used by the XSLT code
                MemoryStream beforeSurveyStream = new MemoryStream();
                XmlTextWriter xmlWriter = new XmlTextWriter(beforeSurveyStream, Encoding.Unicode);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Survey");
                xmlWriter.WriteAttributeString("IAT", IATName);
                xmlWriter.WriteAttributeString("Type", "Before");
                xmlWriter.WriteAttributeString("FileName", IAT.BeforeSurvey[ctr1].FileNameBase);
                xmlWriter.WriteAttributeString("SurveyName", IAT.BeforeSurvey[ctr1].Name);
                xmlWriter.WriteAttributeString("TimeoutMillis", (IAT.BeforeSurvey[ctr1].Timeout * 60000).ToString());
                xmlWriter.WriteAttributeString("ServerURL", URL);
                xmlWriter.WriteAttributeString("ServerPort", port.ToString());
                xmlWriter.WriteAttributeString("ClientID", ClientID.ToString());
                if (IAT.UniqueResponse.SurveyUri == null)
                    xmlWriter.WriteElementString("UniqueResponseItem", "-1");
                else if (IAT.UniqueResponse.SurveyUri.Equals(IAT.BeforeSurvey[ctr1].URI))
                    xmlWriter.WriteAttributeString("UniqueResponseItem", IAT.UniqueResponse.ItemNum.ToString());
                else
                    xmlWriter.WriteAttributeString("UniqueResponseItem", "-1");
                xmlWriter.WriteElementString("ServerDomain", Properties.Resources.sDefaultIATServerDomain);
                xmlWriter.WriteElementString("ServerPort", Properties.Resources.sDefaultIATServerPort);
                xmlWriter.WriteElementString("ServerPath", Properties.Resources.sDefaultIATServerPath);
                xmlWriter.WriteElementString("UploadTimeMillis", UploadTimeMillis.ToString());
                for (int ctr2 = 0; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++)
                {
                    if (IAT.BeforeSurvey[ctr1].Items[ctr2].ItemType == SurveyItemType.SurveyImage)
                    {
                        Images.IImageMedia imgMedia = (IAT.BeforeSurvey[ctr1].Items[ctr2] as CSurveyItemImage).SurveyImage.IImage.OriginalImage;
                        MemoryStream memStream = new MemoryStream();
                        System.Drawing.Image img = imgMedia.Img;
                        img.Save(memStream, imgMedia.ImageFormat.Format);
                        img.Dispose();
                        (IAT.BeforeSurvey[ctr1].Items[ctr2] as CSurveyItemImage).OnlineFilename = String.Format("survey-image{0}.{1}", ++surveyImgCtr, imgMedia.FileExtension);
                        SurveyImages.Add(new Tuple<String, MemoryStream>(String.Format("survey-image{0}.{1}", surveyImgCtr, imgMedia.FileExtension), memStream));
                    }
                    IAT.BeforeSurvey[ctr1].Items[ctr2].WriteXml(xmlWriter);
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                Surveys.Add(beforeSurveyStream);

                // store the schema-ed XML used for result file processing
                beforeSurveyStream = new MemoryStream();
                xmlWriter = new XmlTextWriter(beforeSurveyStream, Encoding.Unicode);
                Survey s = new Survey(IAT.BeforeSurvey[ctr1].Name);
                s.Timeout = (int)(IAT.BeforeSurvey[ctr1].Timeout * 60000);
                s.HasCaption = IAT.BeforeSurvey[ctr1].Items[0].IsCaption;
                if (s.HasCaption)
                    s.SetCaption(IAT.BeforeSurvey[ctr1].Items[0]);
                s.SetItems(IAT.BeforeSurvey[ctr1].Items.Where(si => si.ItemType == SurveyItemType.Item).ToArray());
                s.NumItems = IAT.BeforeSurvey[ctr1].Items.Where(si => (si.ItemType == SurveyItemType.Item) &&
                    (si.Response.ResponseType != CResponse.EResponseType.Instruction)).Count();
                XmlSerializer ser = new XmlSerializer(typeof(Survey));
                ser.Serialize(xmlWriter, s);
                xmlWriter.Flush();
                SASurveys.Add(beforeSurveyStream);
            }
            for (int ctr1 = 0; ctr1 < IAT.AfterSurvey.Count; ctr1++)
            {
                // store the schema-less XML used by the XSLT code
                MemoryStream afterSurveyStream = new MemoryStream();
                XmlTextWriter xmlWriter = new XmlTextWriter(afterSurveyStream, Encoding.Unicode);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Survey");
                xmlWriter.WriteAttributeString("IAT", IATName);
                xmlWriter.WriteAttributeString("Type", "After");
                xmlWriter.WriteAttributeString("FileName", IAT.AfterSurvey[ctr1].FileNameBase);
                xmlWriter.WriteAttributeString("SurveyName", IAT.AfterSurvey[ctr1].Name);
                xmlWriter.WriteAttributeString("TimeoutMillis", (IAT.AfterSurvey[ctr1].Timeout * 60000).ToString());
                xmlWriter.WriteAttributeString("ServerURL", URL);
                xmlWriter.WriteAttributeString("ServerPort", port.ToString());
                xmlWriter.WriteAttributeString("ClientID", ClientID.ToString());
                if (IAT.UniqueResponse.SurveyUri == null)
                    xmlWriter.WriteAttributeString("UniqueResponseItem", "-1");
                else if (IAT.UniqueResponse.SurveyUri.Equals(IAT.AfterSurvey[ctr1].URI))
                    xmlWriter.WriteAttributeString("UniqueResponseItem", IAT.UniqueResponse.ItemNum.ToString());
                else
                    xmlWriter.WriteAttributeString("UniqueResponseItem", "-1");
                xmlWriter.WriteElementString("ServerDomain", Properties.Resources.sDefaultIATServerDomain);
                xmlWriter.WriteElementString("ServerPort", Properties.Resources.sDefaultIATServerPort);
                xmlWriter.WriteElementString("ServerPath", Properties.Resources.sDefaultIATServerPath);
                xmlWriter.WriteElementString("UploadTimeMillis", UploadTimeMillis.ToString());
                for (int ctr2 = 0; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
                {
                    if (IAT.AfterSurvey[ctr1].Items[ctr2].ItemType == SurveyItemType.SurveyImage)
                    {
                        (IAT.AfterSurvey[ctr1].Items[ctr2] as CSurveyItemImage).OnlineFilename = String.Format("survey-image{0}", ++surveyImgCtr);
                        Images.IImageMedia imgMedia = (IAT.AfterSurvey[ctr1].Items[ctr2] as CSurveyItemImage).SurveyImage.IImage.OriginalImage;
                        System.Drawing.Image img = imgMedia.Img;
                        MemoryStream memStream = new MemoryStream();
                        img.Save(memStream, imgMedia.ImageFormat.Format);
                        img.Dispose();
                        SurveyImages.Add(new Tuple<String, MemoryStream>(String.Format("survey-image{0}", surveyImgCtr), memStream));
                    }
                    IAT.AfterSurvey[ctr1].Items[ctr2].WriteXml(xmlWriter);
                }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                Surveys.Add(afterSurveyStream);

                // store the schema-ed XML used for result file processing
                afterSurveyStream = new MemoryStream();
                xmlWriter = new XmlTextWriter(afterSurveyStream, Encoding.Unicode);
                Survey s = new Survey(IAT.AfterSurvey[ctr1].Name);
                s.Timeout = (int)(IAT.AfterSurvey[ctr1].Timeout * 60000);
                s.HasCaption = IAT.AfterSurvey[ctr1].Items[0].IsCaption;
                if (s.HasCaption)
                    s.SetCaption(IAT.AfterSurvey[ctr1].Items[0]);
                s.SetItems(IAT.AfterSurvey[ctr1].Items.Where(si => si.ItemType == SurveyItemType.Item).ToArray());
                s.NumItems = IAT.AfterSurvey[ctr1].Items.Where(si => (si.ItemType == SurveyItemType.Item) &&
                    (si.Response.ResponseType != CResponse.EResponseType.Instruction)).Count();
                XmlSerializer ser = new XmlSerializer(typeof(Survey));
                ser.Serialize(xmlWriter, s);
                xmlWriter.Flush();
                SASurveys.Add(afterSurveyStream);
            }
        }

        private void BuildFileManifest()
        {
            Manifest = new Manifest();
            CF.UploadTimeMillis = UploadTimeMillis;
            CF.ClientID = ClientID;
            ConfigFileXML = new MemoryStream();
            XmlTextWriter xWriter = new XmlTextWriter(ConfigFileXML, Encoding.Unicode);
            xWriter.WriteStartDocument();
            CF.WriteXml(xWriter);
            xWriter.WriteEndDocument();
            xWriter.Flush();
            Manifest.AddFile(new ManifestFile(IATName + ".xml", ConfigFileXML.Length)
            {
                ResourceType = ManifestFile.EResourceType.DeploymentFile,
                ResourceId = 1
            });
            ProcessSurveys(Properties.Resources.sDefaultIATServerDomain, Convert.ToInt32(Properties.Resources.sDefaultIATServerPort));
            String surveyFNameBase;
            Regex r = new Regex("[^a-zaA-Z0-9]");
            for (int ctr2 = 0; ctr2 < Surveys.Count; ctr2++)
            {
                if (ctr2 < IAT.BeforeSurvey.Count)
                    surveyFNameBase = IAT.BeforeSurvey[ctr2].Name;
                else
                    surveyFNameBase = IAT.AfterSurvey[ctr2 - IAT.BeforeSurvey.Count].Name;
                surveyFNameBase = r.Replace(surveyFNameBase, "");
                Manifest.AddFile(new ManifestFile(String.Format("{0}", surveyFNameBase), Surveys[ctr2].Length)
                {
                    ResourceType = ManifestFile.EResourceType.DeploymentFile,
                    ResourceId = 2 * ctr2 + 2
                });
                Manifest.AddFile(new ManifestFile(String.Format("{0}-DataRetrieval.xml", surveyFNameBase), SASurveys[ctr2].Length)
                {
                    ResourceType = ManifestFile.EResourceType.DeploymentFile,
                    ResourceId = 2 * ctr2 + 3
                });
            }
            if (UniqueRespXML != null)
            {
                ManifestFile urf = new ManifestFile("UniqueResponse.xml", UniqueRespXML.Length)
                {
                    ResourceType = ManifestFile.EResourceType.DeploymentFile,
                    ResourceId = Surveys.Count * 2 + 2
                };
                Manifest.AddFile(urf);
            }
            foreach (Tuple<String, MemoryStream> tup in SurveyImages)
                Manifest.AddFile(new ManifestFile(tup.Item1, tup.Item2.Length)
                {
                    ResourceId = Manifest.NumEntities + 1,
                    ResourceType = ManifestFile.EResourceType.DeploymentFile
                });
            Manifest.AddFiles(CF.IATImages.ConstructFileManifest(ManifestFile.EResourceType.DeploymentImage));
            Manifest.AddFiles(CF.SlideImages.ConstructFileManifest(ManifestFile.EResourceType.ItemSlide));
        }

        private bool SendDeploymentFiles(long deploymentId, String sessionId)
        {
            MemoryStream memStream = new MemoryStream();
            memStream.Write(ConfigFileXML.ToArray(), 0, (int)ConfigFileXML.Length);
            if (UniqueRespXML != null)
                memStream.Write(UniqueRespXML.ToArray(), 0, (int)UniqueRespXML.Length);
            for (int ctr = 0; ctr < Surveys.Count; ctr++)
            {
                memStream.Write(Surveys[ctr].ToArray(), 0, (int)Surveys[ctr].Length);
                memStream.Write(SASurveys[ctr].ToArray(), 0, (int)SASurveys[ctr].Length);
            }
            byte[][] ImageData = CF.IATImages.GetImageData();
            for (int ctr = 0; ctr < ImageData.Length; ctr++)
                memStream.Write(ImageData[ctr], 0, ImageData[ctr].Length);
            foreach (Tuple<String, MemoryStream> tup in SurveyImages)
            {
                memStream.Write(tup.Item2.ToArray(), 0, (int)tup.Item2.Length);
                tup.Item2.Dispose();
            }
            try
            {
                WebClient c = new WebClient();
                c.Headers["deploymentId"] = deploymentId.ToString();
                c.Headers["sessionId"] = sessionId;
                c.UploadData(Properties.Resources.sDeploymentUploadURL, memStream.ToArray());
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

        private void SendItemSlides(long deploymentId, String sessionId)
        {
            CF.SlidesProcessed.WaitOne();
            byte[][] itemSlideData = CF.SlideImages.GetImageData();
            MemoryStream memStream = new MemoryStream();
            for (int ctr = 0; ctr < itemSlideData.Length; ctr++)
                memStream.Write(itemSlideData[ctr], 0, itemSlideData[ctr].Length);
            WebClient web = new WebClient();
            web.Headers["deploymentId"] = deploymentId.ToString();
            web.Headers["sessionId"] = sessionId;
            web.UploadData(Properties.Resources.sItemSlideUploadURL, memStream.ToArray());
            memStream.Dispose();
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
            SetStatusMessage(update.StatusMessage);
            MainForm.Invoke(ResetProgress);
            if (update.ProgressMax > 0)
            {
                if (update.CurrentProgress == 0)
                    MainForm.Invoke(SetProgressRange, update.ProgressMin, update.ProgressMax);
                MainForm.Invoke(ProgressIncrement, update.CurrentProgress);
            }
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
                        SetStatusMessage("Connection established");
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
                        dResult = (DialogResult)MainForm.Invoke(DisplayYesNoMessageBox, "An IAT with this name associated with your account already exists on the server. If you have made costmetic changes to your " +
                            "test that do not effect the format of the result set, such as changing the wording of instructions or correcting typos, you may attempt to redeploy your IAT " +
                            "atop the one on the server. Do you wish to try this?", "IAT Exists");
                        if (dResult == DialogResult.No)
                        {
                            MainForm.Invoke(EndProgressBarUse);
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
                        dResult = (DialogResult)MainForm.Invoke(DisplayYesNoMessageBox, "An IAT with this name is currently in a state of deployment. If you have recently tried to upload a test " +
                            "with this name, likely that deployment has been suspended. You may only upload this test if you abandon the previous deployment. Do you wish to do so?", "Test Being Deployed");
                        if (dResult == DialogResult.No)
                        {
                            MainForm.Invoke(EndProgressBarUse);
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
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.QueryRemainingIATS;
                        deploymentStage++;
                        transmission = new Envelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
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
                        SetStatusMessage("Requesting upload");
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.QueryRemainingIATS;
                        deploymentStage++;
                        transmission = new Envelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.QueryPublicityIAT:
                        outTrans = new TransactionRequest();
                        if (MainForm.OnDisplayYesNoMessageBox(Properties.Resources.sQueryPublicityIAT, Properties.Resources.sQueryPublicityIATCaption) == DialogResult.Yes)
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
                        SetStatusMessage("Uploading file manifest");
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;
                    case TransactionRequest.ETransaction.EncryptionKeysReceived:
                        try
                        {
                            BuildFileManifest();
                            if (IAT.TokenType != ETokenType.NONE)
                            {
                                transmission = new Envelope(new TokenDefinition(IAT.TokenType, IAT.TokenName));
                                transmission.SendMessage(UploadTestWebSocket, AbortToken);
                            }
                            else
                            {
                                SetStatusMessage("Uploading file manifest");
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
                        SendDeploymentFiles(trans.LongValues["DeploymentId"], trans.StringValues["SessionId"]);
                        SendItemSlides(trans.LongValues["DeploymentId"], trans.StringValues["SessionId"]);
                        break;

                    case TransactionRequest.ETransaction.RequestIATUpload:
                        SetStatusMessage("Initializing result encryption data");
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
                        MainForm.BeginInvoke(OperationComplete, IATSummary);
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

        private CancellationToken WebSocketCancellationToken { get; set; }

        public bool Upload(String iatName, String password)
        {
            var CancellationSource = new CancellationTokenSource();
            WebSocketCancellationToken = CancellationSource.Token;
            ConnectCancel = false;
            IncomingMessage = null;
            UploadSuccess.Reset();
            UploadFailed.Reset();
            UploadCancelled.Reset();
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
            SetStatusMessage("Preparing Upload");
            CF = new IATConfig.ConfigFile(IAT);
            CF.ServerDomain = Properties.Resources.sDefaultIATServerDomain;
            CF.ServerPath = Properties.Resources.sDefaultIATServerPath;
            CF.ServerPort = Convert.ToInt32(Properties.Resources.sDefaultIATServerPort);
            if (CF.HasUniqiueResponses)
            {
                UniqueRespXML = new MemoryStream();
                XmlWriter xWriter = new XmlTextWriter(UniqueRespXML, Encoding.UTF8);
                IATConfig.UniqueResponseItem uri = new IATConfig.UniqueResponseItem(IAT.UniqueResponse);
                uri.WriteXmlDocument(xWriter);
                xWriter.Flush();
            }
            SetStatusMessage("Establishing connection");
            Task connectTask = null;
            try
            {
                connectTask = UploadTestWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), WebSocketCancellationToken);
                connectTask.Wait(10000);
            }
            catch (AggregateException aggEx)
            {
                foreach (var ex in aggEx.InnerExceptions)
                    ErrorReporter.ReportError(new CReportableException("Error on test upload", ex));
                return false;
            }
            if (!connectTask.IsCompleted)
            {
                ErrorReporter.ReportError(new CReportableException("Timeout connecting to server for test upload", new TimeoutException()));
                return false;
            }
            StartMessageReceiver();
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
                catch (TaskCanceledException) { return 1; }
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
            return (nTrigger == 0);
        }
        private void StartMessageReceiver()
        {
            UploadTestWebSocket.ReceiveAsync(ReceiveBuffer, WebSocketCancellationToken).ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), WebSocketCancellationToken);
        }

        private void ReceiveMessage(Task<WebSocketReceiveResult> t)
        {
            try
            {
                if (t.IsCanceled)
                    return;
                if (t.IsFaulted)
                    return;
                WebSocketReceiveResult receipt = t.Result;
                if (receipt.MessageType == WebSocketMessageType.Close)
                    UploadTestWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Close Requested by server", WebSocketCancellationToken);

                if (t.Result.Count != 0)
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
                    UploadTestWebSocket.ReceiveAsync(ReceiveBuffer, WebSocketCancellationToken).ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), CIAT.GetCancellationToken());
            }
            catch (WebSocketException ex)
            {
                UploadTestWebSocket.Dispose();
                UploadFailed.Set();
                return;
            }
            catch (System.Net.Sockets.SocketException)
            {
                UploadTestWebSocket.Dispose();
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
                try
                {
                    TransactionRequest trans = new TransactionRequest();
                    if (DeploymentSessionID != -1)
                    {
                        trans.Transaction = TransactionRequest.ETransaction.AbortDeployment;
                        trans.LongValues["DeploymentId"] = DeploymentSessionID;
                    }
                    else
                        trans.Transaction = TransactionRequest.ETransaction.AbortTransaction;
                    Envelope env = new Envelope(trans);
                    env.SendMessage(UploadTestWebSocket, AbortToken);
                }
                finally
                {
                    UploadFailed.Set();
                }
            }
        }
    }
}
