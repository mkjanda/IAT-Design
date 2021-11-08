using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using WebSocket4Net;

namespace IATClient
{
    class CWebSocketUploader
    {
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
        private MemoryStream ConfigFileXML;
        private WebSocket webSocket;
        private enum EDeploymentStage { requestingConnection = 0, shakingHands = 1, queryingDuplicateIAT = 2, requestingDeployment = 3, performingDeployment = 4 };
        private EDeploymentStage deploymentStage;
        private Manifest fileManifest;
        private int ClientID;
        private IATConfigFileNamespace.ConfigFile CF;
        private ManualResetEvent TransactionComplete = new ManualResetEvent(false);
        public enum ETransactionResult { success, fail, exception, unset };
        private ETransactionResult _TransactionResult = ETransactionResult.unset;
        private Exception _TransactionException = null;
        private CPacketTransmission PacketTransmission = null;
        private Boolean webSocketClosed = true;
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
            webSocket.Close();
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
                for (int ctr2 = 0; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++)
                    IAT.BeforeSurvey[ctr1].Items[ctr2].WriteToXml(xmlWriter);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                Surveys.Add(beforeSurveyStream);

                // store the schema-ed XML used for result file processing
                beforeSurveyStream = new MemoryStream();
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
                ser.Serialize(beforeSurveyStream, s);
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
                for (int ctr2 = 0; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
                    IAT.AfterSurvey[ctr1].Items[ctr2].WriteToXml(xmlWriter);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                Surveys.Add(afterSurveyStream);

                // store the schema-ed XML used for result file processing
                afterSurveyStream = new MemoryStream();
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
                ser.Serialize(afterSurveyStream, s);
                SASurveys.Add(afterSurveyStream);
            }
        }

        private void BuildFileManifest()
        {
            fileManifest = new Manifest();
            fileManifest.Type = Manifest.EType.DeploymentFiles;
            ManifestFile configFile = new ManifestFile(IATName + ".cf", ConfigFileXML.Length);
            int nFiles = 2 + CF.DisplayItems.Count + (Surveys.Count * 2);
            fileManifest.AddFile(configFile);
            String surveyFNameBase;
            for (int ctr2 = 0; ctr2 < Surveys.Count; ctr2++)
            {
                if (ctr2 < IAT.BeforeSurvey.Count)
                    surveyFNameBase = IAT.BeforeSurvey[ctr2].FileNameBase;
                else
                    surveyFNameBase = IAT.AfterSurvey[ctr2 - IAT.BeforeSurvey.Count].FileNameBase;
                fileManifest.AddFile(new ManifestFile(String.Format("{0}.xml.survey", surveyFNameBase), Surveys[ctr2].Length));
                ManifestFile SurveyFileWithSchema = new ManifestFile();
                fileManifest.AddFile(new ManifestFile(String.Format("{0}.xml", surveyFNameBase), SASurveys[ctr2].Length));
            }
            fileManifest.AddFiles(CF.DisplayItemImages.ConstructFileManifest());
        }

        private void SendDeploymentFiles()
        {
            PacketTransmission = new CPacketTransmission(webSocket, CPacket.EType.DeploymentData);
            PacketTransmission.QueueFile(ConfigFileXML);
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
            PacketTransmission.Transmit(MainForm, ProgressIncrement, Properties.Resources.sDataTransactionWebsocketURI);
            PacketTransmission = null;
            MainForm.Invoke(ResetProgress);
        }

        private void SendItemSlides()
        {
            byte[][] itemSlideData = CF.ItemSlides.GetImageData();
            for (int ctr = 0; ctr < itemSlideData.Length; ctr++)
                PacketTransmission.QueueFile(itemSlideData[ctr]);
            PacketTransmission.BuildPacketList();
            MainForm.Invoke(SetStatusMessage, "Uploading Item Slides");
            MainForm.Invoke(SetProgressRange, 0, PacketTransmission.NumPackets);
            PacketTransmission.Transmit(MainForm, ProgressIncrement, Properties.Resources.sDataTransactionWebsocketURI);
            PacketTransmission = null;
        }

        private void ShakeHands(INamedXmlSerializable handshake)
        {
            HandShake hs = (HandShake)handshake;
            CEnvelope.SendMessage(HandShake.CreateResponse(hs), webSocket);
            deploymentStage++;
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
                        webSocket.Close();
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
                        webSocket.Close();
                        break;

                    case TransactionRequest.ETransaction.IATExists:
                        MainForm.BeginInvoke(OperationFailed, "An IAT with this name and associated with your account already exists on the server.", "IAT Exists");
                        webSocket.Close();
                        break;

                    case TransactionRequest.ETransaction.InsufficientIATS:
                        MainForm.BeginInvoke(OperationFailed, "You have met your maximum quota for the number of IATs you may have deployed on the server.", "Insufficient IATs Remaining");
                        webSocket.Close();
                        break;

                    case TransactionRequest.ETransaction.NoSuchIAT:
                        outTrans = new TransactionRequest();
                        outTrans.IATName = IAT.Name;
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestIATUpload;
                        deploymentStage++;
                        CEnvelope.SendMessage(outTrans, webSocket);
                        break;

                    case TransactionRequest.ETransaction.RequestIATUpload:
                        CPartiallyEncryptedRSAKey dataKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
                        dataKey.Generate(DataPassword);
                        CPartiallyEncryptedRSAKey adminKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Admin);
                        adminKey.Generate(AdminPassword);
                        CRSAKeyPair keyPair = new CRSAKeyPair(dataKey, adminKey);
                        CEnvelope.SendMessage(keyPair, webSocket);
                        CF = new IATConfigFileNamespace.ConfigFile(IAT);
                        CF.ServerURL = Properties.Resources.sDefaultIATServer;
                        CF.ServerPort = Convert.ToInt32(Properties.Resources.sDataProviderPort);
                        ClientID = trans.ClientID;
                        CF.ClientID = trans.ClientID;
                        ConfigFileXML = new MemoryStream();
                        XmlWriter xWriter = new XmlTextWriter(ConfigFileXML, Encoding.UTF8);
                        CF.WriteXml(xWriter);
                        xWriter.Flush();
                        ProcessSurveys(Properties.Resources.sDefaultIATServer, Convert.ToInt32(Properties.Resources.sDataProviderPort));
                        BuildFileManifest();
                        if (_TransactionResult != ETransactionResult.unset)
                            return;
                        CEnvelope.SendMessage(fileManifest, webSocket);
                        SendDeploymentFiles();
                        Manifest itemSlideManifest = new Manifest();
                        itemSlideManifest.AddFiles(CF.ItemSlides.ConstructFileManifest());
                        itemSlideManifest.Type = Manifest.EType.ItemSlides;
                        if (_TransactionResult != ETransactionResult.unset)
                            return;
                        CEnvelope.SendMessage(itemSlideManifest, webSocket);
                        SendItemSlides();
                        break;

                    case TransactionRequest.ETransaction.TransactionSuccess:
                        CIATSummary IATSummary;
                        IATSummary = new CIATSummary(CF);
                        IATSummary.IATLink = Properties.Resources.sDataProviderServlet.Substring(0, Properties.Resources.sDataProviderServlet.LastIndexOf("/")) + String.Format(Properties.Resources.sIATServletURLPart, IATName, ClientID);
                        IATSummary.DataRetrievalPassword = DataPassword;
                        IATSummary.AdminPassword = AdminPassword;
                        MainForm.BeginInvoke(OperationComplete, IATSummary);
                        _TransactionResult = ETransactionResult.success;
                        TransactionComplete.Set();
                        webSocket.Close();
                        break;

                    case TransactionRequest.ETransaction.TransactionFail:
                        MessageBox.Show("Deployment failed");
                        webSocket.Close();
                        _TransactionResult = ETransactionResult.fail;
                        break;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.StackTrace, ex.Message);
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
            MainForm.BeginProgressBarUse(new EventHandler(OnCancel), IATConfigMainForm.EProgressBarUses.Upload);
     //       MainForm.AddToolStripCancelButton(new EventHandler(OnCancel));
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(TransactionReceived);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.DeploymentException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            webSocket = new WebSocket(Properties.Resources.sDataTransactionWebsocketURI);
            webSocket.Opened += new EventHandler(WS_Opened);
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
        private void WS_Opened(object sender, EventArgs e)
        {
            TransactionRequest trans = new TransactionRequest(TransactionRequest.ETransaction.RequestConnection, Properties.Resources.sServerPassword, IAT.Name, "http://www.iatsoftware.net");
            CEnvelope.SendMessage(trans, webSocket);
            webSocketClosed = false;
        }

        private void WS_Closed(object sender, EventArgs e)
        {
            CEnvelope.Shutdown();
            if (PacketTransmission != null)
            {
                PacketTransmission.Halt();
                PacketTransmission = null;
            }
            MainForm.Invoke(EndProgressBarUse);
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
