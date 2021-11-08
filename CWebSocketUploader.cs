using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using System.Net.WebSockets;

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
        private Manifest DeploymentFileManifest, ItemSlideManifest;
        private int ClientID;
        private IATConfigFileNamespace.ConfigFile CF;
        private ManualResetEvent UploadSuccess = new ManualResetEvent(false), UploadFailed = new ManualResetEvent(false), UploadCancelled = new ManualResetEvent(false);
        private bool bAwaitingUpdates = false;
        private CRSAKeyPair keyPair = null;
        private object transmissionLock = new object();
        private String multipartBoundry = "===" + DateTime.Now.ToBinary().ToString() + "===";
        private CancellationToken AbortToken = new CancellationToken();
        private ArraySegment<byte> ReceiveBuffer = new ArraySegment<byte>(new byte[8192]);
        private CEnvelope IncomingMessage;
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
                CEnvelope env = new CEnvelope(trans);
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
                        System.Drawing.Image img = imgMedia.Image;
                        img.Save(memStream, imgMedia.Format);
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
                IATSurveyFile.Survey s = new IATSurveyFile.Survey(IAT.BeforeSurvey[ctr1].Name);
                s.Timeout = (int)(IAT.BeforeSurvey[ctr1].Timeout * 60000);
                s.HasCaption = IAT.BeforeSurvey[ctr1].Items[0].IsCaption;
                if (s.HasCaption)
                    s.SetCaption(IAT.BeforeSurvey[ctr1].Items[0]);
                s.SetItems(IAT.BeforeSurvey[ctr1].Items.Where(si => si.ItemType == SurveyItemType.Item).ToArray());
                s.NumItems = IAT.BeforeSurvey[ctr1].Items.Where(si => (si.ItemType == SurveyItemType.Item) &&
                    (si.Response.ResponseType != CResponse.EResponseType.Instruction)).Count();
                XmlSerializer ser = new XmlSerializer(typeof(IATSurveyFile.Survey));
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
                        System.Drawing.Image img = imgMedia.Image;
                        MemoryStream memStream = new MemoryStream();
                        img.Save(memStream, imgMedia.Format);
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
                IATSurveyFile.Survey s = new IATSurveyFile.Survey(IAT.AfterSurvey[ctr1].Name);
                s.Timeout = (int)(IAT.AfterSurvey[ctr1].Timeout * 60000);
                s.HasCaption = IAT.AfterSurvey[ctr1].Items[0].IsCaption;
                if (s.HasCaption)
                    s.SetCaption(IAT.AfterSurvey[ctr1].Items[0]);
                s.SetItems(IAT.AfterSurvey[ctr1].Items.Where(si => si.ItemType == SurveyItemType.Item).ToArray());
                s.NumItems = IAT.AfterSurvey[ctr1].Items.Where(si => (si.ItemType == SurveyItemType.Item) &&
                    (si.Response.ResponseType != CResponse.EResponseType.Instruction)).Count();
                XmlSerializer ser = new XmlSerializer(typeof(IATSurveyFile.Survey));
                ser.Serialize(xmlWriter, s);
                xmlWriter.Flush();
                SASurveys.Add(afterSurveyStream);
            }
        }

        private void BuildFileManifest()
        {
            DeploymentFileManifest = new Manifest();
            DeploymentFileManifest.Type = Manifest.EType.DeploymentFiles;
            CF.UploadTimeMillis = UploadTimeMillis;
            CF.ClientID = ClientID;
            ConfigFileXML = new MemoryStream();
            XmlTextWriter xWriter = new XmlTextWriter(ConfigFileXML, Encoding.Unicode);
            xWriter.WriteStartDocument();
            CF.WriteXml(xWriter);
            xWriter.WriteEndDocument();
            xWriter.Flush();
            ManifestFile configFile = new ManifestFile(IATName + ".cf", ConfigFileXML.Length);
            ProcessSurveys(Properties.Resources.sDefaultIATServerDomain, Convert.ToInt32(Properties.Resources.sDefaultIATServerPort));
            int nFiles = 2 + CF.DisplayItems.Count + (Surveys.Count * 2) + ((UniqueRespXML == null) ? 0 : 1);
            DeploymentFileManifest.AddFile(configFile);
            if (UniqueRespXML != null)
            {
                ManifestFile urf = new ManifestFile("UniqueResponse.xml", UniqueRespXML.Length);
                DeploymentFileManifest.AddFile(urf);
            }
            String surveyFNameBase;
            Regex r = new Regex("[^a-zaA-Z0-9]");
            for (int ctr2 = 0; ctr2 < Surveys.Count; ctr2++)
            {
                if (ctr2 < IAT.BeforeSurvey.Count)
                    surveyFNameBase = IAT.BeforeSurvey[ctr2].Name;
                else
                    surveyFNameBase = IAT.AfterSurvey[ctr2 - IAT.BeforeSurvey.Count].Name;
                surveyFNameBase = r.Replace(surveyFNameBase, "");
                DeploymentFileManifest.AddFile(new ManifestFile(String.Format("{0}.xml.survey", surveyFNameBase), Surveys[ctr2].Length));
                ManifestFile SurveyFileWithSchema = new ManifestFile();
                DeploymentFileManifest.AddFile(new ManifestFile(String.Format("{0}.xml", surveyFNameBase), SASurveys[ctr2].Length));
            }
            DeploymentFileManifest.AddFiles(CF.DisplayItemImages.ConstructFileManifest());
            foreach (Tuple<String, MemoryStream> tup in SurveyImages)
                DeploymentFileManifest.AddFile(new ManifestFile(tup.Item1, tup.Item2.Length));
        }

        private bool SendDeploymentFiles(CUploadRequest upReq)
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
            byte[][] ImageData = CF.DisplayItemImages.GetImageData();
            for (int ctr = 0; ctr < ImageData.Length; ctr++)
                memStream.Write(ImageData[ctr], 0, ImageData[ctr].Length);
            foreach (Tuple<String, MemoryStream> tup in SurveyImages)
            {
                memStream.Write(tup.Item2.ToArray(), 0, (int)tup.Item2.Length);
                tup.Item2.Dispose();
            }
            try
            {
                WebClient webClient = new WebClient();
                webClient.UploadData(String.Format(Properties.Resources.sUploadURI, upReq.DeploymentID, upReq.DataUploadKey, DeploymentFileManifest.TotalSize), "POST", memStream.ToArray());
                return true;
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.ProtocolError)
                {
                    CReportableException reportable = new CReportableException(ex.Message, ex);
                    IATConfigMainForm.ShowErrorReport("Failed to upload IAT", reportable);
                    UploadFailed.Set();
                }
                return false;
            }
            catch (Exception ex)
            {
                CReportableException reportable = new CReportableException(ex.Message, ex);
                IATConfigMainForm.ShowErrorReport("Failed to upload IAT", reportable);
                UploadFailed.Set();
                return false;
            }
            finally
            {
                memStream.Dispose();
            }
        }

        private bool SendItemSlides(CUploadRequest upReq)
        {
            byte[][] itemSlideData = CF.ItemSlides.GetImageData();
            MemoryStream memStream = new MemoryStream();
            for (int ctr = 0; ctr < itemSlideData.Length; ctr++)
                memStream.Write(itemSlideData[ctr], 0, itemSlideData[ctr].Length);
            try
            {
                WebClient webClient = new WebClient();
                webClient.UploadData(String.Format(Properties.Resources.sUploadURI, upReq.DeploymentID, upReq.ItemSlideUploadKey, ItemSlideManifest.TotalSize), "POST", memStream.ToArray());
                return true;
            }
            catch (WebException ex)
            {
                if (ex.Status != WebExceptionStatus.ProtocolError)
                {
                    CReportableException reportable = new CReportableException(ex.Message, ex);
                    IATConfigMainForm.ShowErrorReport("Failed to upload IAT", reportable);
                }
                UploadFailed.Set();
                return false;
            }
            catch (Exception ex)
            {
                CReportableException reportable = new CReportableException(ex.Message, ex);
                IATConfigMainForm.ShowErrorReport("Failed to upload IAT", reportable);
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
            CEnvelope transmission = new CEnvelope(trans);
            transmission.SendMessage(UploadTestWebSocket, AbortToken);
        }

        private void ShakeHands(INamedXmlSerializable handshake)
        {
            HandShake hs = (HandShake)handshake;
            CEnvelope transmission = new CEnvelope(HandShake.CreateResponse(hs));
            transmission.SendMessage(UploadTestWebSocket, AbortToken);
            deploymentStage++;
        }

        private void OnDeploymentProgressUpdate(INamedXmlSerializable dpu)
        {
            CDeploymentProgressUpdate update = (CDeploymentProgressUpdate)dpu;
            if (update.DeploymentException != null)
            {
                IATConfigMainForm.ShowErrorReport("Error deploying IAT", update.DeploymentException);
                UploadFailed.Set();
                return;
            }
            if (update.Stage == CDeploymentProgressUpdate.EStage.mismatchedDeploymentDescriptors)
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

        private void UploadData(INamedXmlSerializable upReq)
        {
            CUploadRequest uploadRequest = (CUploadRequest)upReq;
            BackgroundWorker worker = new BackgroundWorker();
                SetStatusMessage("Uploading test");
                if (!SendDeploymentFiles(uploadRequest))
                    return;
                if (!SendItemSlides(uploadRequest))
                    return;
        }

        private void OnUploadProgress(String data)
        {
            MainForm.BeginInvoke(new Action<int>(MainForm.ProgressIncrement), Convert.ToInt32(data));
        }

        private void TransactionReceived(INamedXmlSerializable T)
        {
            TransactionRequest trans = (TransactionRequest)T;
            TransactionRequest outTrans = null;
            EDeploymentStage dStage = deploymentStage;
            DialogResult dResult;
            CEnvelope transmission = null;
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
                        transmission = new CEnvelope(outTrans);
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
                            CEnvelope env = new CEnvelope(trans);
                            env.SendMessage(UploadTestWebSocket, AbortToken);
                            UploadCancelled.Set();
                            return;
                        }
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                        transmission = new CEnvelope(outTrans);
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
                            CEnvelope env = new CEnvelope(outTrans);
                            env.SendMessage(UploadTestWebSocket, AbortToken);
                            UploadCancelled.Set();
                            return;
                        }
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.HaltTestDeployment;
                        outTrans.LongValues["DeploymentId"] = trans.LongValues["DeploymentId"];
                        transmission = new CEnvelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.DeploymentHalted:
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.QueryRemainingIATS;
                        deploymentStage++;
                        transmission = new CEnvelope(outTrans);
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
                        transmission = new CEnvelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.PasswordInvalid:
                        OperationFailed("The password you supplied does not match the original password for this IAT.", "Invalid Password");
                        break;

                    case TransactionRequest.ETransaction.PasswordValid:
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IATName;
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestIATRedeploy;
                        transmission = new CEnvelope(outTrans);
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
                        transmission = new CEnvelope(outTrans);
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
                        transmission = new CEnvelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.RemainingIATS:
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestIATUpload;
                        transmission = new CEnvelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.DeploymentDescriptorMismatch:
                        OperationFailed("The test you are attempting to upload differs from the test already on the server in ways that would cause discrepencies in the result set format. It cannot be deployed atop the existing test.", "Incompatible Result Sets");
                        break;

                    case TransactionRequest.ETransaction.TokenDefinitionReceived:
                        transmission = new CEnvelope(DeploymentFileManifest);
                        SetStatusMessage("Uploading file manifest");
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;                        
                    case TransactionRequest.ETransaction.EncryptionKeysReceived:
                        try
                        {
                            BuildFileManifest();
                            if (IAT.TokenType != ETokenType.NONE)
                            {
                                transmission = new CEnvelope(new TokenDefinition(IAT.TokenType, IAT.TokenName));
                                transmission.SendMessage(UploadTestWebSocket, AbortToken);
                            }
                            else
                            {
                                SetStatusMessage("Uploading file manifest");
                                transmission = new CEnvelope(DeploymentFileManifest);
                                transmission.SendMessage(UploadTestWebSocket, AbortToken);
                            }
                        }
                        catch (NotImplementedException ex)
                        {
                            OperationFailed(ex.Message, "Operation Not Implemented");
                        }
                        break;

                    case TransactionRequest.ETransaction.DeploymentFileManifestReceived:
                        ItemSlideManifest = new Manifest();
                        ItemSlideManifest.AddFiles(CF.ItemSlides.ConstructFileManifest());
                        ItemSlideManifest.Type = Manifest.EType.ItemSlides;
                        transmission = new CEnvelope(ItemSlideManifest);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.ItemSlideManifestReceived:
                        outTrans = new TransactionRequest();
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestDataUpload;
                        outTrans.LongValues["DeploymentId"] = DeploymentSessionID;
                        SetStatusMessage("Waiting for test upload signal");
                        transmission = new CEnvelope(outTrans);
                        transmission.SendMessage(UploadTestWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.RequestIATUpload:
                        SetStatusMessage("Initializing result encryption data");
                        UploadTimeMillis = trans.LongValues["DeploymentStartTime"];
                        DeploymentSessionID = trans.LongValues["DeploymentId"];
                        if (this.keyPair == null)
                        {
                            CPartiallyEncryptedRSAKey dataKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
                            dataKey.Generate(DataPassword);
                            CPartiallyEncryptedRSAKey adminKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Admin);
                            adminKey.Generate(AdminPassword);
                            this.keyPair = new CRSAKeyPair(dataKey, adminKey);
                        }
                        transmission = new CEnvelope(this.keyPair);
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
            catch (Exception ex)
            {
                CReportableException reportable = new CReportableException(ex.Message, ex);
                IATConfigMainForm.ShowErrorReport("Failed to upload IAT", reportable);
                if (DeploymentSessionID != -1)
                {
                    trans.Transaction = TransactionRequest.ETransaction.AbortDeployment;
                    trans.LongValues["DeploymentId"] = DeploymentSessionID;
                }
                else
                    trans.Transaction = TransactionRequest.ETransaction.AbortTransaction;
                CEnvelope env = new CEnvelope(trans);
                env.SendMessage(UploadTestWebSocket, AbortToken);
                UploadFailed.Set();
            }
        }

        private void OnDeploymentException(INamedXmlSerializable ex)
        {
            CServerException deployEX = (CServerException)ex;
            IATConfigMainForm.ShowErrorReport("Error deploying IAT", deployEX);
            UploadFailed.Set();
        }

        public bool Upload(String iatName, String password)
        {
            ConnectCancel = false;
            IncomingMessage = null;
            UploadSuccess.Reset();
            UploadFailed.Reset();
            UploadCancelled.Reset();
            DataPassword = password;
            AdminPassword = password;
            _IATName = iatName;
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(TransactionReceived);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.DeploymentProgress] = new Action<INamedXmlSerializable>(OnDeploymentProgressUpdate);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(OnRSAKeyPairReceipt);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.UploadRequest] = new Action<INamedXmlSerializable>(UploadData);
            MainForm.Invoke(new Action<EventHandler, IATConfigMainForm.EProgressBarUses>(MainForm.BeginProgressBarUse), new EventHandler(OnCancel), IATConfigMainForm.EProgressBarUses.Upload);
            UploadTestWebSocket = new ClientWebSocket();
            CancellationToken connectCancellation = new CancellationToken();
            SetStatusMessage("Preparing Upload");
            CF = new IATConfigFileNamespace.ConfigFile(IAT);
            CF.ServerDomain = Properties.Resources.sDefaultIATServerDomain;
            CF.ServerPath = Properties.Resources.sDefaultIATServerPath;
            CF.ServerPort = Convert.ToInt32(Properties.Resources.sDefaultIATServerPort);
            if (CF.HasUniqiueResponses)
            {
                UniqueRespXML = new MemoryStream();
                XmlWriter xWriter = new XmlTextWriter(UniqueRespXML, Encoding.UTF8);
                IATConfigFileNamespace.UniqueResponseItem uri = new IATConfigFileNamespace.UniqueResponseItem(IAT.UniqueResponse);
                uri.WriteXmlDocument(xWriter);
                xWriter.Flush();
            }
            bool connectionMade = false;
            SetStatusMessage("Establishing connection");
            if (!UploadTestWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), connectCancellation).ContinueWith((t) =>
            {
                if (!t.IsFaulted)
                {
                    connectionMade = true;
                    StartMessageReceiver();
                }
                else
                {
                    WebException webException = null;
                    Exception innerEx;
                    foreach (Exception ex in t.Exception.InnerExceptions)
                    {
                        innerEx = ex;
                        while (!(innerEx is WebException))
                        {
                            innerEx = ex.InnerException;
                            if (innerEx == null)
                                break;
                        }
                        if (innerEx != null)
                            if (innerEx is WebException)
                            {
                                webException = innerEx as WebException;
                                break;
                            }
                    }
                    HttpStatusCode code = (webException.Response as HttpWebResponse).StatusCode;
                    if ((code == HttpStatusCode.BadGateway) || (code == HttpStatusCode.InternalServerError))
                        OperationFailed(Properties.Resources.sServerDown, Properties.Resources.sServerDownCaption);
                    else
                        OperationFailed(Properties.Resources.sConnectionError, Properties.Resources.sConnectionErrorCaption);
                }
            }).Wait(15000))
            {
                OperationFailed(Properties.Resources.sConnectionTimeoutMessage, Properties.Resources.sConnectionTimeoutCaption);
                UploadTestWebSocket.Dispose();
                return false;
            }
            if (!connectionMade)
                return false;
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            trans.IATName = IATName;
            CEnvelope env = new CEnvelope(trans);
            env.SendMessage(UploadTestWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { UploadSuccess, UploadFailed, UploadCancelled });
            CEnvelope.Shutdown();
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
            Task<WebSocketReceiveResult> receiveTask = UploadTestWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
            receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
        }

        private void ReceiveMessage(Task<WebSocketReceiveResult> t)
        {
            if (t.IsCanceled)
                return;
            if (t.IsFaulted)
                return;
            try
            {
                if (t.Result.Count != 0)
                {
                    try
                    {
                        lock (transmissionLock)
                        {
                            WebSocketReceiveResult receipt = t.Result;
                            if (receipt.EndOfMessage)
                            {
                                if (IncomingMessage == null)
                                    IncomingMessage = new CEnvelope();
                                if (IncomingMessage.QueueByteData(ReceiveBuffer.Array.Take(receipt.Count).ToArray(), true))
                                    IncomingMessage = null;
                            }
                            else
                            {
                                if (IncomingMessage == null)
                                    IncomingMessage = new CEnvelope();
                                IncomingMessage.QueueByteData(ReceiveBuffer.Array.Take(receipt.Count).ToArray(), false);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        CReportableException reportable = new CReportableException(ex.Message, ex);
                        IATConfigMainForm.ShowErrorReport("Error receiving server transmission", reportable);
                        UploadFailed.Set();
                    }
                }
                Task<WebSocketReceiveResult> receiveTask = UploadTestWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
                receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
            }
            catch (Exception ex) {
                CReportableException reportable = new CReportableException(ex.Message, ex);
                IATConfigMainForm.ShowErrorReport("Communication Error", reportable);
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
                    CEnvelope env = new CEnvelope(trans);
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
