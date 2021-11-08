using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Net;
using WebSocket4Net;

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
        private Action<String, String> OperationFailed;
        private Action<int> ProgressIncrement;
        private Action ResetProgress;
        private Action<String> SetStatusMessage;
        private Action<EventHandler, IATConfigMainForm.EProgressBarUses> BeginProgressBarUse;
        private Action EndProgressBarUse;
        private Action<String, String> DisplayMessageBox;
        private Action<int> OnSetProgressValue;
        private Action<Form> ShowForm;
        private IATConfigMainForm MainForm;
        private object lockObject = new object();
        private String DataPassword, AdminPassword, _IATName;
        private CIAT IAT;
        private List<MemoryStream> Surveys = new List<MemoryStream>(), SASurveys = new List<MemoryStream>();
        private MemoryStream ConfigFileXML, UniqueRespXML;
        private WebSocket webSocket;
        private enum EDeploymentStage { requestingConnection = 0, shakingHands = 1, queryingDuplicateIAT = 2, requestingDeployment = 3, performingDeployment = 4 };
        private EDeploymentStage deploymentStage;
        private Manifest fileManifest;
        private int ClientID;
        private IATConfigFileNamespace.ConfigFile CF;
        private ManualResetEvent TransactionComplete = new ManualResetEvent(false);
        public enum ETransactionResult { negotiationComplete, uploadComplete, success, fail, exception, cancel, unset };
        private ETransactionResult _TransactionResult = ETransactionResult.unset;
        private Exception _TransactionException = null;
        private CPacketTransmission PacketTransmission = null;
        private Boolean webSocketClosed = true;
        private bool bAwaitingUpdates = false;
        private CRSAKeyPair keyPair = null;
        private String reconnectionKey;
        private long deploymentID;
        private long uploadTimeMillis = -1;

        public Exception TransactionException
        {
            get
            {
                return _TransactionException;
            }
        }

        public String IATName
        {
            get
            {
                return _IATName;
            }
        }

        public void HaltPacketTransmission()
        {
            if (PacketTransmission != null)
                PacketTransmission.Halt();
        }

        public void OnCancel(object sender, EventArgs e)
        {
            try
            {
                if (PacketTransmission != null)
                {
                    PacketTransmission.Halt();
                    PacketTransmission = null;
                }
                webSocket.Close();
            }
            catch (Exception) { }
        }

        public CWebSocketUploader(CIAT iat, IATConfigMainForm mainForm)
        {
            IAT = iat;
            MainForm = mainForm;
            SetProgressRange = new Action<int, int>(mainForm.SetProgressRange);
            DisplayYesNoMessageBox = new Func<String, String, DialogResult>(mainForm.OnDisplayYesNoMessageBox);
            OperationComplete = new Action<CIATSummary>(mainForm.OperationComplete);
            OperationFailed = new Action<String, String>(mainForm.OperationFailed);
            ProgressIncrement = new Action<int>(mainForm.ProgressIncrement);
            ResetProgress = new Action(mainForm.ResetProgress);
            SetStatusMessage = new Action<String>(mainForm.SetStatusMessage);
            BeginProgressBarUse = new Action<EventHandler, IATConfigMainForm.EProgressBarUses>(mainForm.BeginProgressBarUse);
            EndProgressBarUse = new Action(mainForm.EndProgressBarUse);
            DisplayMessageBox = new Action<String, String>(mainForm.OnDisplayMessageBox);
            OnSetProgressValue = new Action<int>(mainForm.SetProgressValue);
            ShowForm = new Action<Form>(mainForm.ShowForm);
        }

        private void ProcessSurveys(String URL, int port)
        {
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
                if (IAT.UniqueResponse.SurveyName == IAT.BeforeSurvey[ctr1].Name)
                    xmlWriter.WriteAttributeString("UniqueResponseItem", IAT.UniqueResponse.ItemNum.ToString());
                else
                    xmlWriter.WriteAttributeString("UniqueResponseItem", "-1");
                xmlWriter.WriteElementString("ServerDomain", Properties.Resources.sDefaultIATServerDomain);
                xmlWriter.WriteElementString("ServerPort", Properties.Resources.sDefaultIATServerPort);
                xmlWriter.WriteElementString("ServerPath", Properties.Resources.sDefaultIATServerPath);
                xmlWriter.WriteElementString("UploadTimeMillis", uploadTimeMillis.ToString());
                for (int ctr2 = 0; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++)
                    IAT.BeforeSurvey[ctr1].Items[ctr2].WriteToXml(xmlWriter);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                Surveys.Add(beforeSurveyStream);

                // store the schema-ed XML used for result file processing
                beforeSurveyStream = new MemoryStream();
                xmlWriter = new XmlTextWriter(beforeSurveyStream, Encoding.Unicode);
                IATSurveyFile.Survey s = new IATSurveyFile.Survey(IAT.BeforeSurvey[ctr1].Name);
                s.Timeout = (int)(IAT.BeforeSurvey[ctr1].Timeout * 60000);
                if (IAT.BeforeSurvey[ctr1].Items[0].IsCaption)
                {
                    s.SetCaption(IAT.BeforeSurvey[ctr1].Items[0]);
                    s.HasCaption = true;
                    CSurveyItem[] surveyItems = new CSurveyItem[IAT.BeforeSurvey[ctr1].Items.Count - 1];
                    int itemCtr = 0;
                    for (int ctr2 = 1; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++)
                    {
                        if (IAT.BeforeSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                            itemCtr++;
                        surveyItems[ctr2 - 1] = IAT.BeforeSurvey[ctr1].Items[ctr2];
                    }
                    s.SetItems(surveyItems);
                    s.NumItems = itemCtr;
                }
                else
                {
                    s.HasCaption = false;
                    CSurveyItem[] surveyItems = new CSurveyItem[IAT.BeforeSurvey[ctr1].Items.Count];
                    int itemCtr = 0;
                    for (int ctr2 = 0; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++)
                    {
                        if (IAT.BeforeSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                            itemCtr++;
                        surveyItems[ctr2] = IAT.BeforeSurvey[ctr1].Items[ctr2];
                    }
                    s.SetItems(surveyItems);
                    s.NumItems = itemCtr;
                }
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
                if (IAT.UniqueResponse.SurveyName == IAT.AfterSurvey[ctr1].Name)
                    xmlWriter.WriteAttributeString("UniqueResponseItem", IAT.UniqueResponse.ItemNum.ToString());
                else
                    xmlWriter.WriteAttributeString("UniqueResponseItem", "-1");
                xmlWriter.WriteElementString("ServerDomain", Properties.Resources.sDefaultIATServerDomain);
                xmlWriter.WriteElementString("ServerPort", Properties.Resources.sDefaultIATServerPort);
                xmlWriter.WriteElementString("ServerPath", Properties.Resources.sDefaultIATServerPath);
                xmlWriter.WriteElementString("UploadTimeMillis", uploadTimeMillis.ToString());
                for (int ctr2 = 0; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
                    IAT.AfterSurvey[ctr1].Items[ctr2].WriteToXml(xmlWriter);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                Surveys.Add(afterSurveyStream);

                // store the schema-ed XML used for result file processing
                afterSurveyStream = new MemoryStream();
                xmlWriter = new XmlTextWriter(afterSurveyStream, Encoding.Unicode);
                IATSurveyFile.Survey s = new IATSurveyFile.Survey(IAT.AfterSurvey[ctr1].Name);
                s.Timeout = (int)(IAT.AfterSurvey[ctr1].Timeout * 60000);
                if (IAT.AfterSurvey[ctr1].Items[0].IsCaption)
                {
                    s.SetCaption(IAT.AfterSurvey[ctr1].Items[0]);
                    s.HasCaption = true;
                    CSurveyItem[] surveyItems = new CSurveyItem[IAT.AfterSurvey[ctr1].Items.Count - 1];
                    int itemCtr = 0;
                    for (int ctr2 = 1; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
                    {
                        if (IAT.AfterSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                            itemCtr++;
                        surveyItems[ctr2 - 1] = IAT.AfterSurvey[ctr1].Items[ctr2];
                    }
                    s.SetItems(surveyItems);
                    s.NumItems = itemCtr;
                }
                else
                {
                    s.HasCaption = false;
                    CSurveyItem[] surveyItems = new CSurveyItem[IAT.AfterSurvey[ctr1].Items.Count];
                    int itemCtr = 0;
                    for (int ctr2 = 0; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
                    {
                        if (IAT.AfterSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                            itemCtr++;
                        surveyItems[ctr2] = IAT.AfterSurvey[ctr1].Items[ctr2];
                    }
                    s.SetItems(surveyItems);
                    s.NumItems = itemCtr;
                }
                XmlSerializer ser = new XmlSerializer(typeof(IATSurveyFile.Survey));
                ser.Serialize(xmlWriter, s);
                xmlWriter.Flush();
                SASurveys.Add(afterSurveyStream);
            }
        }

        private void BuildFileManifest()
        {
            fileManifest = new Manifest();
            fileManifest.Type = Manifest.EType.DeploymentFiles;
            ManifestFile configFile = new ManifestFile(IATName + ".cf", ConfigFileXML.Length);
            int nFiles = 2 + CF.DisplayItems.Count + (Surveys.Count * 2) + ((UniqueRespXML == null) ? 0 : 1);
            fileManifest.AddFile(configFile);
            if (UniqueRespXML != null)
            {
                ManifestFile urf = new ManifestFile("UniqueResponse.xml", UniqueRespXML.Length);
                fileManifest.AddFile(urf);
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
                fileManifest.AddFile(new ManifestFile(String.Format("{0}.xml.survey", surveyFNameBase), Surveys[ctr2].Length));
                ManifestFile SurveyFileWithSchema = new ManifestFile();
                fileManifest.AddFile(new ManifestFile(String.Format("{0}.xml", surveyFNameBase), SASurveys[ctr2].Length));
            }
            fileManifest.AddFiles(CF.DisplayItemImages.ConstructFileManifest());
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
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(String.Format(Properties.Resources.sUploadURI, upReq.DeploymentID, upReq.DataUploadKey));
            req.SendChunked = true;
            req.ContentLength = memStream.Length;
            req.ContentType = "application/octet-stream";
            req.Method = "POST";
            req.KeepAlive = false;
            req.Timeout = 600000;
            Stream s = req.GetRequestStream();
            s.Write(memStream.ToArray(), 0, (int)memStream.Length);
            s.Flush();
            memStream.Dispose();
            try
            {
                HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            }
            catch (WebException ex) {
                MainForm.Invoke(OperationFailed, "Server returned: " + ex.Status, "Failed to upload IAT");
                TransactionComplete.Set();
                return false;
            }
            return true;
            
            /*
            PacketTransmission = new CPacketTransmission(webSocket, CPacket.EType.DeploymentData);
            PacketTransmission.QueueFile(ConfigFileXML);
            if (UniqueRespXML != null)
                PacketTransmission.QueueFile(UniqueRespXML);
            for (int ctr = 0; ctr < Surveys.Count; ctr++)
            {
                PacketTransmission.QueueFile(Surveys[ctr]);
                PacketTransmission.QueueFile(SASurveys[ctr]);
            }
            byte[][] ImageData = CF.DisplayItemImages.GetImageData();
            for (int ctr = 0; ctr < ImageData.Length; ctr++)
                PacketTransmission.QueueFile(ImageData[ctr]);
            PacketTransmission.BuildPacketList();
            MainForm.Invoke(SetStatusMessage, "Uploading IAT");
            MainForm.Invoke(SetProgressRange, 0, PacketTransmission.NumPackets);
            CPacketTransmission.ETransmissionResult result = PacketTransmission.Transmit(MainForm, ProgressIncrement, Properties.Resources.sDataTransactionWebsocketURI);
            PacketTransmission = null;
            MainForm.Invoke(ResetProgress);
            return (result == CPacketTransmission.ETransmissionResult.Success);
             * */
        }

        private bool SendItemSlides(CUploadRequest upReq)
        {
            byte[][] itemSlideData = CF.ItemSlides.GetImageData();
            MemoryStream memStream = new MemoryStream();
            for (int ctr = 0; ctr < itemSlideData.Length; ctr++)
                memStream.Write(itemSlideData[ctr], 0, itemSlideData[ctr].Length);
            HttpWebRequest req = (HttpWebRequest)WebRequest.Create(String.Format(Properties.Resources.sUploadURI, upReq.DeploymentID, upReq.ItemSlideUploadKey));
            req.SendChunked = true;
            req.ContentLength = memStream.Length;
            req.ContentType = "application/octet-stream";
            req.Method = "POST";
            req.KeepAlive = false;
            req.Timeout = 600000;
            Stream s = req.GetRequestStream();
            s.Write(memStream.ToArray(), 0, (int)memStream.Length);
            s.Flush();
            HttpWebResponse response = (HttpWebResponse)req.GetResponse();
            memStream.Dispose();
            if (response.StatusCode != HttpStatusCode.OK)
            {
                MainForm.Invoke(OperationFailed, "Server returned: " + response.StatusCode.ToString(), "Failed to upload IAT");
                TransactionComplete.Set();
                return false;
            }
            return true;
/*            byte[][] itemSlideData = CF.ItemSlides.GetImageData();
            PacketTransmission = new CPacketTransmission(webSocket, CPacket.EType.ItemSlide);
            for (int ctr = 0; ctr < itemSlideData.Length; ctr++)
                PacketTransmission.QueueFile(itemSlideData[ctr]);
            PacketTransmission.BuildPacketList();
            MainForm.Invoke(SetStatusMessage, "Uploading Item Slides");
            MainForm.Invoke(SetProgressRange, 0, PacketTransmission.NumPackets);
            CPacketTransmission.ETransmissionResult result = PacketTransmission.Transmit(MainForm, ProgressIncrement, Properties.Resources.sDataTransactionWebsocketURI);
            PacketTransmission = null;
            return (result == CPacketTransmission.ETransmissionResult.Success); */
        }

        public void OnRSAKeyPairReceipt(INamedXmlSerializable keyPair)
        {
            this.keyPair = (CRSAKeyPair)keyPair;
            TransactionRequest trans = new TransactionRequest();
            trans.IATName = IATName;
            trans.Transaction = TransactionRequest.ETransaction.RequestDataPasswordVerification;
            CEnvelope.SendMessage(trans, webSocket);
        }

        private void ShakeHands(INamedXmlSerializable handshake)
        {
            HandShake hs = (HandShake)handshake;
            CEnvelope.SendMessage(HandShake.CreateResponse(hs), webSocket);
            deploymentStage++;
        }

        private void OnDeploymentProgressUpdate(INamedXmlSerializable dpu)
        {
            CDeploymentProgressUpdate update = (CDeploymentProgressUpdate)dpu;
            MainForm.Invoke(SetStatusMessage, update.StatusMessage);
            MainForm.Invoke(ResetProgress);
            if (update.ProgressMax > 0)
            {
                MainForm.Invoke(SetProgressRange, update.ProgressMin, update.ProgressMax);
                MainForm.Invoke(ProgressIncrement, update.CurrentProgress);
            }
        }

        private void OnUploadRequest(INamedXmlSerializable upReq)
        {
            _TransactionResult = ETransactionResult.negotiationComplete;
            webSocket.Close();
            CUploadRequest uploadRequest = (CUploadRequest)upReq;
            reconnectionKey = uploadRequest.ReconnectionKey;
            deploymentID = uploadRequest.DeploymentID;
            bAwaitingUpdates = true;
            if (!SendDeploymentFiles(uploadRequest))
            {
                _TransactionResult = ETransactionResult.fail;
                TransactionComplete.Set();
                return;
            }
            if (!SendItemSlides(uploadRequest))
            {
                _TransactionResult = ETransactionResult.fail;
                TransactionComplete.Set();
                return;
            }
            _TransactionResult = ETransactionResult.uploadComplete;
            TransactionComplete.Set();
        }

        private void TransactionReceived(INamedXmlSerializable T)
        {
            TransactionRequest trans = (TransactionRequest)T;
            TransactionRequest outTrans = null;
            EDeploymentStage dStage = deploymentStage;
            try
            {
                switch (trans.Transaction)
                {
                    case TransactionRequest.ETransaction.ClientFrozen:
                        MainForm.BeginInvoke(OperationFailed, Properties.Resources.sClientFrozen, "Account Frozen");
                        TransactionComplete.Set();
                        break;

                    case TransactionRequest.ETransaction.RequestTransmission:
                        ClientID = trans.ClientID;
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.IATExists;
                        CEnvelope.SendMessage(outTrans, webSocket);
                        break;

                    case TransactionRequest.ETransaction.ClientDeleted:
                        MainForm.BeginInvoke(OperationFailed, Properties.Resources.sClientDeleted, "Account Deleted");
                        TransactionComplete.Set();
                        break;

                    case TransactionRequest.ETransaction.IATExists:
                        DialogResult result = (DialogResult)MainForm.Invoke(DisplayYesNoMessageBox, "An IAT with this name associated with your account already exists on the server. If you have made costmetic changes to your " +
                            "test that do not effect the format of the result set, such as changing the wording of instructions or correcting typos, you may attempt to redeploy your IAT " +
                            "atop the one on the server. Do you wish to try this?", "IAT Exists");
                        if (result == DialogResult.No)
                        {
                            _TransactionResult = ETransactionResult.cancel;
                            MainForm.Invoke(EndProgressBarUse);
                            TransactionComplete.Set();
                            webSocket.Close();
                        }
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                        CEnvelope.SendMessage(outTrans, webSocket);
                        break;

                    case TransactionRequest.ETransaction.VerifyPassword:
                        String encValue = trans.StringValue, value;
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
                            MainForm.Invoke(OperationFailed, "The password you supplied does not match the original password for this IAT.", "Invalid Password");
                            webSocket.Close();
                            TransactionComplete.Set();
                            return;
                        }
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IATName;
                        outTrans.Transaction = TransactionRequest.ETransaction.VerifyPassword;
                        outTrans.StringValue = value;
                        CEnvelope.SendMessage(outTrans, webSocket);
                        break;

                    case TransactionRequest.ETransaction.PasswordInvalid:
                        MainForm.Invoke(OperationFailed, "The password you supplied does not match the original password for this IAT.", "Invalid Password");
                        TransactionComplete.Set();
                        break;

                    case TransactionRequest.ETransaction.PasswordValid:
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IATName;
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestIATRedeploy;
                        CEnvelope.SendMessage(outTrans, webSocket);
                        break;

                    case TransactionRequest.ETransaction.InsufficientIATS:
                        MainForm.BeginInvoke(OperationFailed, "You have met your maximum quota for the number of IATs you may have deployed on the server.", "Insufficient IATs Remaining");
                        TransactionComplete.Set();
                        break;

                    case TransactionRequest.ETransaction.NoSuchIAT:
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestIATUpload;
                        deploymentStage++;
                        CEnvelope.SendMessage(outTrans, webSocket);
                        break;

                    case TransactionRequest.ETransaction.RequestIATUpload:
                        uploadTimeMillis = trans.LongValue;
                        if (this.keyPair == null)
                        {
                            CPartiallyEncryptedRSAKey dataKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
                            dataKey.Generate(DataPassword);
                            CPartiallyEncryptedRSAKey adminKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Admin);
                            adminKey.Generate(AdminPassword);
                            this.keyPair = new CRSAKeyPair(dataKey, adminKey);
                        }
                        CEnvelope.SendMessage(this.keyPair, webSocket);
                        CF = new IATConfigFileNamespace.ConfigFile(IAT);
                        CF.ServerDomain = Properties.Resources.sDefaultIATServerDomain;
                        CF.ServerPath = Properties.Resources.sDefaultIATServerPath;
                        CF.ServerPort = Convert.ToInt32(Properties.Resources.sDefaultIATServerPort);
                        CF.UploadTimeMillis = uploadTimeMillis;
                        ClientID = trans.ClientID;
                        CF.ClientID = trans.ClientID;
                        ConfigFileXML = new MemoryStream();
                        XmlTextWriter xWriter = new XmlTextWriter(ConfigFileXML, Encoding.UTF8);
                        xWriter.WriteStartDocument();
                        CF.WriteXml(xWriter);
                        xWriter.WriteEndDocument();
                        xWriter.Flush();
                        if (CF.HasUniqiueResponses)
                        {
                            UniqueRespXML = new MemoryStream();
                            xWriter = new XmlTextWriter(UniqueRespXML, Encoding.UTF8);
                            IATConfigFileNamespace.UniqueResponseItem uri = new IATConfigFileNamespace.UniqueResponseItem(IAT.UniqueResponse);
                            uri.WriteXmlDocument(xWriter);
                            xWriter.Flush();
                        }
                        ProcessSurveys(Properties.Resources.sDefaultIATServerDomain, Convert.ToInt32(Properties.Resources.sDefaultIATServerPort));
                        BuildFileManifest();
                        if (_TransactionResult != ETransactionResult.unset)
                            return;
                        CEnvelope.SendMessage(fileManifest, webSocket);
                        Manifest itemSlideManifest = new Manifest();
                        itemSlideManifest.AddFiles(CF.ItemSlides.ConstructFileManifest());
                        itemSlideManifest.Type = Manifest.EType.ItemSlides;
                        CEnvelope.SendMessage(itemSlideManifest, webSocket);
                        outTrans = new TransactionRequest();
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestIATUpload;
                        CEnvelope.SendMessage(outTrans, webSocket);
                        break;

                    case TransactionRequest.ETransaction.TransactionSuccess:
                        CIATSummary IATSummary;
                        IATSummary = new CIATSummary(CF);
                        IATSummary.IATLink = "http://" + Properties.Resources.sDefaultIATServerDomain + Properties.Resources.sDefaultIATServerPath + String.Format(Properties.Resources.sIATServletURLPart, IATName, ClientID);
                        IATSummary.DataRetrievalPassword = DataPassword;
                        IATSummary.AdminPassword = AdminPassword;
                        MainForm.BeginInvoke(OperationComplete, IATSummary);
                        _TransactionResult = ETransactionResult.success;
                        TransactionComplete.Set();
                        break;

                    case TransactionRequest.ETransaction.TransactionFail:
                        MainForm.Invoke(OperationFailed, "Deployment failed", "Deployment failed");
                        _TransactionResult = ETransactionResult.fail;
                        TransactionComplete.Set();
                        break;

                    case TransactionRequest.ETransaction.BackupRestored:
                        MainForm.Invoke(OperationFailed, "The redeployment of your test atop the test on the server failed but the existing test was preserved.", "Deployment Failed");
                        _TransactionResult = ETransactionResult.fail;
                        TransactionComplete.Set();
                        break;

                    case TransactionRequest.ETransaction.CannotRestoreBackup:
                        MainForm.Invoke(OperationFailed, "The redeployment of your test atop the test on the server failed. Moreover, the backup of the test already on the server could not be restored.", "Deployment Failed");
                        webSocket.Close();
                        _TransactionResult = ETransactionResult.fail;
                        TransactionComplete.Set();
                        break;

                    case TransactionRequest.ETransaction.MismatchedDataFormat:
                        MainForm.Invoke(OperationFailed, "The redeployment of your test atop the test on the server failed because the result set format differs from the result set of the test currently on the server.", "Deployment Failed");
                        _TransactionResult = ETransactionResult.fail;
                        TransactionComplete.Set();
                        break;

                    case TransactionRequest.ETransaction.InsufficientDiskSpace:
                        MainForm.Invoke(OperationFailed, "You do not have sufficient remaining disk space on the server to upload your IAT", "Insufficient Disk Space");
                        _TransactionResult = ETransactionResult.fail;
                        TransactionComplete.Set();
                        break;

                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message);
                if (PacketTransmission != null)
                {
                    PacketTransmission.Halt();
                    PacketTransmission = null;
                }
                webSocket.Close();
            }
        }

        private void OnDeploymentException(INamedXmlSerializable ex)
        {
            if (PacketTransmission != null)
                PacketTransmission.Halt();
            _TransactionResult = ETransactionResult.fail;
            webSocket.Close();
            CDeploymentException deployEX = (CDeploymentException)ex;
            ErrorReportDisplay f = new ErrorReportDisplay(deployEX);
            MainForm.BeginInvoke(new Action<Form>(MainForm.ShowForm), f);
            TransactionComplete.Set();
        }

        public ETransactionResult Upload(String iatName, String password)
        {
            TransactionComplete.Reset();
            DataPassword = password;
            AdminPassword = password;
            _IATName = iatName;
     //       MainForm.AddToolStripCancelButton(new EventHandler(OnCancel));
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(TransactionReceived);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.DeploymentException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.DeploymentProgress] = new Action<INamedXmlSerializable>(OnDeploymentProgressUpdate);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(OnRSAKeyPairReceipt);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.UploadRequest] = new Action<INamedXmlSerializable>(OnUploadRequest);
            webSocket = new WebSocket(Properties.Resources.sDataTransactionWebsocketURI);
            webSocket.Opened += new EventHandler(WS_NegotiationOpened);
            webSocket.Closed += new EventHandler(WS_Closed);
            webSocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(WS_Error);
            webSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(CEnvelope.MessageReceived);
            webSocket.Open();
            MainForm.Invoke(SetStatusMessage, "Negotiating deployment");
            TransactionComplete.WaitOne();
            TransactionComplete.Reset();
            webSocket.Dispose();
            if (_TransactionResult != ETransactionResult.negotiationComplete)
            {
                MainForm.Invoke(new Action(MainForm.EndProgressBarUse));
                return _TransactionResult; 
            }
            MainForm.BeginInvoke(SetStatusMessage, "Uploading test");
            TransactionComplete.WaitOne();
            TransactionComplete.Reset();
            if (_TransactionResult != ETransactionResult.uploadComplete)
            {
                MainForm.Invoke(new Action(MainForm.EndProgressBarUse));
                return ETransactionResult.fail;
            }
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(TransactionReceived);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.DeploymentException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.DeploymentProgress] = new Action<INamedXmlSerializable>(OnDeploymentProgressUpdate);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(OnRSAKeyPairReceipt);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.UploadRequest] = new Action<INamedXmlSerializable>(OnUploadRequest);
            webSocket = new WebSocket(Properties.Resources.sDataTransactionWebsocketURI);
            webSocket.Opened += new EventHandler(WS_ConfirmationOpened);
            webSocket.Closed += new EventHandler(WS_Closed);
            webSocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(WS_Error);
            webSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(CEnvelope.MessageReceived);
            webSocket.Open();
            TransactionComplete.WaitOne();
            MainForm.Invoke(new Action(MainForm.EndProgressBarUse));
            webSocket.Dispose();
            return _TransactionResult;
        }
        /*
                public void Upload()
                {
                    UploadComplete.Reset();
                    DataPassword = MainForm.DataRetrievalPassword;
                    AdminPassword = MainForm.AdminPassword;
                    _IATName = MainForm.IATName;
                    MainForm.BeginProgressBarUse(new Func<int, bool, bool>(OnCancel), IATConfigMainForm.EProgressBarUses.Upload);
                    MainForm.AddToolStripCancelButton(OnCancel);
                    CDynamicSpecifier.CompactSpecifierDictionary(IAT);
                    webSocket = new WebSocket(Properties.Resources.sDeploymentWebSocketUri);
                    webSocket.Opened += new EventHandler(WS_Opened);
                    webSocket.Closed += new EventHandler(WS_Closed);
                    webSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(WebMessageReceived);
                    webSocket.Open();
                }
                */

        private void WS_NegotiationOpened(object sender, EventArgs e)
        {
            TransactionRequest trans = new TransactionRequest(TransactionRequest.ETransaction.RequestConnection, Properties.Resources.sServerPassword, IAT.Name);
            MainForm.BeginProgressBarUse(new EventHandler(OnCancel), IATConfigMainForm.EProgressBarUses.Upload);
            CEnvelope.SendMessage(trans, webSocket);
            webSocketClosed = false;
        }

        private void WS_ConfirmationOpened(object sender, EventArgs e)
        {
            TransactionRequest trans = new TransactionRequest(TransactionRequest.ETransaction.RequestReconnection, Properties.Resources.sServerPassword, IAT.Name);
            trans.StringValue = reconnectionKey;
            trans.LongValue = deploymentID;
            CEnvelope.SendMessage(trans, webSocket);
            webSocketClosed = false;
        }

        private void WS_Closed(object sender, EventArgs e)
        {
            CEnvelope.Shutdown();
            TransactionComplete.Set();
            webSocketClosed = true;
        }

        private void WS_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            _TransactionResult = ETransactionResult.exception;
            _TransactionException = e.Exception;
            if (PacketTransmission != null)
            {
                PacketTransmission.Halt();
                PacketTransmission = null;
            }
            if (!webSocketClosed)
            {
                webSocketClosed = true;
                webSocket.Close();
            }
        }
    }
}
