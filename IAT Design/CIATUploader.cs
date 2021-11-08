/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Xml;
using System.IO;
using System.Windows.Forms;
using System.Threading;

namespace IATClient
{
    class CIATUploader : ITransmissionOwner 
    {
        private String ServerURL, PackageFilePath, ServerContext;
        private IATConfigFileNamespace.ConfigFile ConfigFile;
        private Manifest IATManifest;
        private String DataRetrievalPassword;
        private IATConfigMainForm.CloseDelegate OnClose;
        private IATConfigMainForm.DisplayYesNoMessageBoxHandler OnDisplayYesNoMessageBox;
        private IATConfigMainForm.ProgressIncrementHandler OnProgressIncrement;
        private IATConfigMainForm.ResetProgressHandler OnResetProgress;
        private IATConfigMainForm.SetProgressRangeHandler OnSetProgressRange;
        private IATConfigMainForm.SetStatusMessageHandler OnSetStatusMessage;
        private IATConfigMainForm.OperationFailedHandler OnOperationFailed;
        private IATConfigMainForm.EndProgressBarUseHandler OnEndProgressBarUse;
        private IATConfigMainForm.OperationCompleteHandler OnOperationComplete;
        private IATConfigMainForm.DisplayDataPasswordHandler OnDisplayDataPassword;
        private IATConfigMainForm.DisplayMessageBoxHandler OnDisplayMessageBox;
        private IATConfigMainForm.SetProgressValueHandler OnSetProgressValue;
        private IATConfigMainForm.ShowFormHandler OnShowForm;
        public delegate void IsAbortedCallback(object sender, EventArgs e);
        private IATConfigMainForm MainForm;
        private CIAT IAT;
        private List<IATImage> Images;
        private List<IATImage> ItemSlides;
        private MemoryStream SchemalessIATData, ConfigFileData;
        private List<MemoryStream> BeforeSurveys, AfterSurveys;
        private List<MemoryStream> SurveySAXmlList;
        private CIATSummary _IATSummary;
        private String DataPassword, AdminPassword;
        
        class UploadAbortedException : Exception
        { }

        class IATUploadException : Exception
        {
            public TransactionProgress _TransProgress;

            public TransactionProgress TransProgress
            {
                get
                {
                    return _TransProgress;
                }
            }

            public IATUploadException(String caption, TransactionProgress tProgress)
                : base(caption)
            {
                _TransProgress = tProgress;
            }

            public IATUploadException(String message, String caption, Exception innerException)
                : base(caption, innerException)
            {
                _TransProgress = MySOAP.TerminateTransaction(message);
            }
        }

        private enum ERegisterIATResult
        {
            Success, IATExists, InsufficientDiskSpace, InsufficientIATs, ClientNotRegistered, DatabaseError
        };

        public enum EDeployResult
        {
            incomplete, successful, cannotBackup, incompatibleResultDescriptors, genericError, deploymentTimerExpired, fileDeploymentError, transformError, databaseError, backupLost
        };

        private bool OnAbort(int timeout, bool forceAbort)
        {
            if (!Monitor.TryEnter(lockObject, timeout))
            {
                if (forceAbort)
                {
                    MySOAP.AbortCurrentTransaction();
                    Monitor.Enter(lockObject);
                }
                else
                    return false;
            }
            _AbortFlag = true;
            MainForm.Invoke(OnEndProgressBarUse);
            Monitor.Exit(lockObject);
            return true;
        }

        private void ResetProgress()
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnResetProgress);
                else
                    throw new UploadAbortedException();
            }
        }

        public bool TryLock(int millis)
        {
            if (!Monitor.TryEnter(lockObject, millis))
                return false;
            return true;
        }

        public void Unlock()
        {
            Monitor.Exit(lockObject);
        }


        private void SetProgressValue(int val)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnSetProgressValue, val);
                else
                    throw new UploadAbortedException();
            }
        }

        private void ShowForm(Form f)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnShowForm, f);
            }
        }

        private DataPasswordForm.EDataPassword DisplayDataPassword(DataPasswordForm dpf)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    return (DataPasswordForm.EDataPassword)MainForm.Invoke(OnDisplayDataPassword, dpf);
                else
                    throw new UploadAbortedException();
            }
        }

        private DialogResult DisplayYesNoMessageBox(String caption, String text)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    return (DialogResult)MainForm.Invoke(OnDisplayYesNoMessageBox, text, caption);
                else
                    throw new UploadAbortedException();
            }
        }

        private void OperationFailed(String caption, String msg)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnOperationFailed);
                else
                    throw new UploadAbortedException();
            }
        }

        private void DisplayMessageBox(String caption, String msg)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnDisplayMessageBox, caption, msg);
                else
                    throw new UploadAbortedException();
            }
        }

        private void ProgressIncrement(int nTicks)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnProgressIncrement, nTicks);
                else
                    throw new UploadAbortedException();
            }
        }

        private void SetProgressRange(int min, int max)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnSetProgressRange, min, max);
                else
                    throw new UploadAbortedException();
            }
        }

        private void SetStatusMessage(String msg)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnSetStatusMessage, msg);
                else
                    throw new UploadAbortedException();
            }
        }

        private void EndProgressBarUse()
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnEndProgressBarUse);
                else
                    throw new UploadAbortedException();
            }
        }

        private void OperationComplete(CIATSummary summary)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnOperationComplete, summary);
                else
                {
                    String serverURL = Properties.Resources.sDataProviderServlet;
                    MySOAP.EstablishEncryption(serverURL);
                    MySOAP.ShakeHands(serverURL, ConfigFile.Name);
                    MySOAP.VerifyPassword(MainForm.IATName, CPartiallyEncryptedRSAKey.EKeyType.Admin, AdminPassword);
                    TransactionRequest trans = new TransactionRequest(); 
                    trans.Transaction = TransactionRequest.ETransaction.DeleteIAT;
                    trans.IATName = ConfigFile.Name;
                    TransactionRequest outTrans = new TransactionRequest();
                    MySOAP.CallSOAP(serverURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.DeleteIAT, trans, trans);
                }
            }
        }

        class IATImage {
            public String FileName;
            public int FileSize;
            public byte[] data;
        }

        public CIATSummary Summary
        {
            get
            {
                return _IATSummary;
            }
        }

        public CIATUploader(String PackageFilePath, CIAT iat, IATConfigMainForm mainForm)
        {
            ServerURL = Properties.Resources.sDataProviderServlet;
            this.PackageFilePath = PackageFilePath;
            IAT = iat;
            MainForm = mainForm;
            ConfigFile = null;
            IATManifest = null;
            OnDisplayYesNoMessageBox += new IATConfigMainForm.DisplayYesNoMessageBoxHandler(MainForm.OnDisplayYesNoMessageBox);
            OnProgressIncrement += new IATConfigMainForm.ProgressIncrementHandler(MainForm.ProgressIncrement);
            OnResetProgress += new IATConfigMainForm.ResetProgressHandler(MainForm.ResetProgress);
            OnSetProgressRange += new IATConfigMainForm.SetProgressRangeHandler(MainForm.SetProgressRange);
            OnSetStatusMessage += new IATConfigMainForm.SetStatusMessageHandler(MainForm.SetStatusMessage);
            OnOperationFailed += new IATConfigMainForm.OperationFailedHandler(MainForm.OperationFailed);
            OnOperationComplete += new IATConfigMainForm.OperationCompleteHandler(MainForm.OperationComplete);
            OnEndProgressBarUse += new IATConfigMainForm.EndProgressBarUseHandler(MainForm.EndProgressBarUse);
            OnDisplayDataPassword += new IATConfigMainForm.DisplayDataPasswordHandler(MainForm.DisplayDataPassword);
            OnDisplayMessageBox += new IATConfigMainForm.DisplayMessageBoxHandler(MainForm.OnDisplayMessageBox);
            OnSetProgressValue += new IATConfigMainForm.SetProgressValueHandler(MainForm.SetProgressValue);
            OnShowForm += new IATConfigMainForm.ShowFormHandler(MainForm.ShowForm); 
            Images = new List<IATImage>();
            ItemSlides = new List<IATImage>();
            SurveySAXmlList = new List<MemoryStream>();
            BeforeSurveys = new List<MemoryStream>();
            AfterSurveys = new List<MemoryStream>();
            DataPassword = MainForm.DataRetrievalPassword;
            AdminPassword = MainForm.AdminPassword;
        }

        private bool _AbortFlag = false;
        private object lockObject = new object();

        public bool Aborted
        {
            get
            {
                lock (lockObject)
                {
                    return _AbortFlag;
                }
            }
        }

        protected MemoryStream GenerateSchemalessXML(IATConfigFileNamespace.ConfigFile configFile)
        {
            MemoryStream outMemStream = new MemoryStream();
            XmlTextWriter xmlWriter = new XmlTextWriter(outMemStream, Encoding.Unicode);
            xmlWriter.Formatting = Formatting.Indented;
            configFile.WriteXml(xmlWriter);
            xmlWriter.Flush();
            return outMemStream;
        }


        protected void ProcessPackageFile(int clientID)
        {
            FileStream fStream = new FileStream(PackageFilePath, FileMode.Open);
            BinaryReader bReader = new BinaryReader(fStream, Encoding.Unicode);
            int nImages = bReader.ReadInt32();
            for (int ctr = 0; ctr < nImages; ctr++)
            {
                IATImage img = new IATImage();
                img.FileName = bReader.ReadString();
                img.FileSize = bReader.ReadInt32();
                img.data = new byte[img.FileSize];
                bReader.Read(img.data, 0, img.FileSize);
                Images.Add(img);
            }
            int nLen = bReader.ReadInt32();
            MemoryStream configFileStream = new MemoryStream(bReader.ReadBytes(nLen), 0, nLen);
            XmlReader xReader = new XmlTextReader(configFileStream);
            xReader.MoveToContent();
            ConfigFile = new IATConfigFileNamespace.ConfigFile(IAT);
            ConfigFile.ReadXml(xReader);
            configFileStream.Dispose();
            ConfigFile.ServerURL = ServerContext;
            ConfigFile.ClientID = clientID;
            _IATSummary = new CIATSummary(ConfigFile);
            int nSurveys = bReader.ReadInt32();
            int unnamedSurveyCtr = 1;
            for (int ctr = 0; ctr < nSurveys; ctr++)
            {
                XmlDocument BeforeSurvey = new XmlDocument();
                nLen = bReader.ReadInt32();
                MemoryStream beforeSurveyStream = new MemoryStream(bReader.ReadBytes(nLen), 0, nLen);
                BeforeSurvey.Load(beforeSurveyStream);
                XmlAttribute serverAttr = BeforeSurvey.CreateAttribute("ServerURL");
                serverAttr.Value = ServerContext;
                XmlAttribute clientIDAttr = BeforeSurvey.CreateAttribute("ClientID");
                clientIDAttr.Value = clientID.ToString();
                BeforeSurvey.DocumentElement.Attributes.Append(serverAttr);
                BeforeSurvey.DocumentElement.Attributes.Append(clientIDAttr);

                beforeSurveyStream.Dispose();
                MemoryStream beforeSurveyData = new MemoryStream();
                XmlTextWriter xmlWriter = new XmlTextWriter(beforeSurveyData, Encoding.Unicode);
                BeforeSurvey.Save(xmlWriter);
                xmlWriter.Flush();
                BeforeSurveys.Add(beforeSurveyData);
                nLen = bReader.ReadInt32();
                SurveySAXmlList.Add(new MemoryStream(bReader.ReadBytes(nLen), 0, nLen, false, true));
                xReader = new XmlTextReader(SurveySAXmlList.Last());
                xReader.MoveToContent();
       //         IATSurveyFileNamespace.Survey s = IATSurveyFileNamespace.Survey.CreateFromXml(xReader);
       //       s.ReadXml(xReader);
//                if (s.HasCaption)
  //                  Summary.Surveys.Add(((IATSurveyFileNamespace.SurveyCaption)s.SurveyItems[0]).Text);
    //            else
      //              Summary.Surveys.Add(String.Format("Unnamed survey #{0}", unnamedSurveyCtr++));
            }
            nSurveys = bReader.ReadInt32();
            for (int ctr = 0; ctr < nSurveys; ctr++)
            {
                XmlDocument AfterSurvey = new XmlDocument();
                nLen = bReader.ReadInt32();
                MemoryStream afterSurveyStream = new MemoryStream(bReader.ReadBytes(nLen), 0, nLen);
                AfterSurvey.Load(afterSurveyStream);
                XmlAttribute serverAttr = AfterSurvey.CreateAttribute("ServerURL");
                serverAttr.Value = ServerContext;
                XmlAttribute clientIDAttr = AfterSurvey.CreateAttribute("ClientID");
                clientIDAttr.Value = clientID.ToString();
                AfterSurvey.DocumentElement.Attributes.Append(serverAttr);
                AfterSurvey.DocumentElement.Attributes.Append(clientIDAttr);
                afterSurveyStream.Dispose();
                MemoryStream afterSurveyData = new MemoryStream();
                XmlTextWriter xmlWriter = new XmlTextWriter(afterSurveyData, Encoding.Unicode);
                AfterSurvey.Save(xmlWriter);
                xmlWriter.Flush();
                AfterSurveys.Add(afterSurveyData);
                nLen = bReader.ReadInt32();
                SurveySAXmlList.Add(new MemoryStream(bReader.ReadBytes(nLen), 0, nLen, false, true));
                xReader = new XmlTextReader(SurveySAXmlList.Last());
                xReader.MoveToContent();
     //           IATSurveyFileNamespace.Survey s = IATSurveyFileNamespace.Survey.CreateFromXml(xReader);
   //             s.ReadXml(xReader);
   //             if (s.HasCaption)
     //               Summary.Surveys.Add(((IATSurveyFileNamespace.SurveyCaption)s.SurveyItems[0]).Text);
       //         else
         //           Summary.Surveys.Add(String.Format("Unnamed survey #{0}", unnamedSurveyCtr++));
            }
            nImages = bReader.ReadInt32();
            for (int ctr = 0; ctr < nImages; ctr++)
            {
                IATImage img = new IATImage();
                img.FileName = bReader.ReadString();
                img.FileSize = bReader.ReadInt32();
                img.data = new Byte[img.FileSize];
                bReader.Read(img.data, 0, img.FileSize);
                ItemSlides.Add(img);
            }
            fStream.Close();
        }

        protected Manifest GenerateFileManifest()
        {
            Manifest manifest = new Manifest();
            ManifestFile configFile = new ManifestFile();
            configFile.Name = ConfigFile.Name + ".xml";
            configFile.Size = ConfigFileData.Length;

            ManifestFile schemalessXML = new ManifestFile();
            schemalessXML.Name = "SchemalessIAT.xml";
            schemalessXML.Size = SchemalessIATData.Length;

            int nFiles = 3 + Images.Count + (BeforeSurveys.Count * 2) + (AfterSurveys.Count * 2);

            manifest.Contents.Add(configFile);
            manifest.Contents.Add(schemalessXML);
            for (int ctr2 = 0; ctr2 < BeforeSurveys.Count; ctr2++)
            {
                ManifestFile BeforeSurveyFile = new ManifestFile();
                BeforeSurveyFile.Name = String.Format("{0}.xml.survey", ConfigFile.BeforeSurveys[ctr2].SurveyName);
                BeforeSurveyFile.Size = BeforeSurveys[ctr2].Length;
                manifest.Contents.Add(BeforeSurveyFile);
                ManifestFile BeforeSurveyFileWithSchema = new ManifestFile();
                BeforeSurveyFileWithSchema.Name = String.Format("{0}WithSchema.xml", ConfigFile.BeforeSurveys[ctr2].SurveyName);
                BeforeSurveyFileWithSchema.Size = SurveySAXmlList[ctr2].Length;
                manifest.Contents.Add(BeforeSurveyFileWithSchema);
            }
            for (int ctr2 = 0; ctr2 < AfterSurveys.Count; ctr2++)
            {
                ManifestFile AfterSurveyFile = new ManifestFile();
                AfterSurveyFile.Name = String.Format("{0}.xml.survey", ConfigFile.AfterSurveys[ctr2].SurveyName);
                AfterSurveyFile.Size = AfterSurveys[ctr2].Length;
                manifest.Contents.Add(AfterSurveyFile);
                ManifestFile AfterSurveyFileWithSchema = new ManifestFile();
                AfterSurveyFileWithSchema.Name = String.Format("{0}WithSchema.xml", ConfigFile.AfterSurveys[ctr2].SurveyName);
                AfterSurveyFileWithSchema.Size = SurveySAXmlList[ConfigFile.BeforeSurveys.Count + ctr2].Length;
                manifest.Contents.Add(AfterSurveyFileWithSchema);
            }

            for (int ctr2 = 0; ctr2 < Images.Count; ctr2++)
            {
                ManifestFile file = new ManifestFile();
                file.Name = Images[ctr2].FileName;
                file.Size = Images[ctr2].FileSize;
                manifest.Contents.Add(file);
            }

            ManifestDirectory ItemSlideDir = new ManifestDirectory();
            ItemSlideDir.Name = "ItemSlides";

            for (int ctr2 = 0; ctr2 < ItemSlides.Count; ctr2++)
            {
                ManifestFile file = new ManifestFile();
                file.Name = ItemSlides[ctr2].FileName;
                file.Size = ItemSlides[ctr2].FileSize;
                ItemSlideDir.Contents.Add(file);
            }
            manifest.Contents.Add(ItemSlideDir);
            manifest.IATName = ConfigFile.Name;
            return manifest;
        }


        /*
        protected bool ExchangeFileManifests()
        {
            bool bContinue = true;
            XmlSerializer serializer = new XmlSerializer(typeof(IATClient.FileManifest.Manifest));
            MemoryStream memStream;
            XmlTextWriter xmlWriter;
                        
            try
            {
                if (socket == null)
                    throw new WebException("Could not establish connection with server.");
                memStream = new MemoryStream();
                xmlWriter = new XmlTextWriter(memStream, Encoding.Unicode);
                serializer.Serialize(xmlWriter, IATManifest);
                xmlWriter.Flush();
                CServerConnection.sendByteBuffer(memStream, socket);
                xmlWriter.Close();
                memStream.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                throw new WebException("Could not transmit file manifest to server.");
            }
            FileManifest.Manifest returnedManifest;
            try
            {
                memStream = CServerConnection.receiveByteBuffer(socket);
                returnedManifest = (FileManifest.Manifest)serializer.Deserialize(memStream);
                memStream.Dispose();
            }
            catch (Exception)
            {
                throw new WebException("Did not receive response to file manifest transmission from server.");
            }
            if (returnedManifest.Contents.Count != 0)
            {
                progressWindow.Invoke(DisplayYesNoMessageBox, Properties.Resources.sIATExistsMsg, Properties.Resources.sIATExistsCaption);
                DialogResult result = DialogResult.None;
                while (result == DialogResult.None)
                {
                    result = progressWindow.YesNoResult;
                    Thread.Sleep(100);
                }
                if (result == DialogResult.Yes)
                    bContinue = true;
                else
                    bContinue = false;
            }

            if (!bContinue)
                return false;

            try
            {
                memStream = new MemoryStream();
                xmlWriter = new XmlTextWriter(memStream, Encoding.Unicode);
                serializer.Serialize(xmlWriter, IATManifest);
                xmlWriter.Flush();
                CServerConnection.sendByteBuffer(memStream, socket);
                xmlWriter.Close();
                memStream.Dispose();
            }
            catch (Exception)
            {
                throw new WebException("Could not finialize file manifest with server.");
            }
            
                        
            return true;
        }
        *//*

        protected CPacketTransmission.ETransmissionResult SendManifest()
        {
            CPacketTransmission transmission = new CPacketTransmission(webSocket);
            SetStatusMessage("Preparing files for transfer");
            SetProgressRange(0, 2 * (1 + BeforeSurveys.Count + AfterSurveys.Count) + Images.Count + ItemSlides.Count);
            ResetProgress();
            transmission.QueueFile(ConfigFileData);
            ProgressIncrement(1);
            transmission.QueueFile(SchemalessIATData);
            ProgressIncrement(1);
            for (int ctr = 0; ctr < BeforeSurveys.Count; ctr++)
            {
                transmission.QueueFile(BeforeSurveys[ctr]);
                ProgressIncrement(1);
                transmission.QueueFile(SurveySAXmlList[ctr]);
                ProgressIncrement(1);
            }
            for (int ctr = 0; ctr < AfterSurveys.Count; ctr++)
            {
                transmission.QueueFile(AfterSurveys[ctr]);
                ProgressIncrement(1);
                transmission.QueueFile(SurveySAXmlList[ctr + BeforeSurveys.Count]);
                ProgressIncrement(1);
            }
            for (int ctr = 0; ctr < Images.Count; ctr++) {
                transmission.QueueFile(Images[ctr].data);
                ProgressIncrement(1);
            }
            for (int ctr = 0; ctr < ItemSlides.Count; ctr++) {
                transmission.QueueFile(ItemSlides[ctr].data);
                ProgressIncrement(1);
            }
            transmission.BuildPacketList();
            if (Aborted)
                return CPacketTransmission.ETransmissionResult.Cancel;
            SetStatusMessage("Uploading files");
            ResetProgress();
            TransactionEvent tEvent = MySOAP.CurrentTransactionEvent.AddChildEvent("Sending Packets");
            SetProgressRange(0, (int)(transmission.QueueLength  / 65536));
            return transmission.Transmit(MainForm, OnProgressIncrement, tEvent, ServerURL);
        }

        private void UploadAbort(String errorMsg)
        {
            OnOperationFailed("Upload Failed", errorMsg);
        }

        protected void run()
        {
            bool bIATExists = false;
            try
            {
                try
                {
                    SetStatusMessage(Properties.Resources.sConnectingToServerMsg);
                    ResetProgress();
                    MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.IAT);
                    MySOAP.BeginNewTransactionEvent("Negotiating Transmission with Server");
                    MySOAP.EstablishEncryption(Properties.Resources.sDataProviderServlet);
                    TransactionRequest inTrans = MySOAP.ShakeHands(Properties.Resources.sDataProviderServlet, MainForm.IATName);
                    int clientID = inTrans.ClientID;
                    if (inTrans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
                    {
                        if (inTrans.Transaction == TransactionRequest.ETransaction.ClientFrozen)
                            throw new IATUploadException("Account Frozen", MySOAP.TerminateTransaction("Your account appears to have been frozen.  Please contact us at admin@iatsoftware.net for details."));
                        else if (inTrans.Transaction == TransactionRequest.ETransaction.ClientDeleted)
                            throw new IATUploadException("Account Deleted", MySOAP.TerminateTransaction("You no longer have an account with IAT Software."));
                        else if (inTrans.Transaction != TransactionRequest.ETransaction.RequestTransmission)
                            throw new IATUploadException("Server Error", MySOAP.TerminateTransaction("Could not connect with server"));
                    }
                    TransactionRequest trans = new TransactionRequest();
                    trans.IATName = MainForm.IATName;
                    trans.Transaction = TransactionRequest.ETransaction.IATExists;
                    trans.IsLastTransaction = false;
                    MySOAP.BeginNewTransactionEvent("Initiating IAT Deployment");
                    MySOAP.CurrentTransactionEvent.AddChildEvent("Checking for an IAT on the server with the same name");
                    MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.IATExists, trans, inTrans);
                    CPartiallyEncryptedRSAKey dataKey, adminKey;
                    if (inTrans.Transaction == TransactionRequest.ETransaction.IATExists)
                    {
                        bIATExists = true;
                        if (DisplayYesNoMessageBox(Properties.Resources.sIATExistsCaption, Properties.Resources.sIATExistsMsg) != DialogResult.Yes)
                        {
                            OnEndProgressBarUse();
                            return;
                        }
                        if (MySOAP.VerifyPassword(MainForm.IATName, CPartiallyEncryptedRSAKey.EKeyType.Admin, AdminPassword) != TransactionRequest.ETransaction.TransactionSuccess)
                        {
                            OperationFailed("Invalid Password", "The administrative password you supplied does not match the administrative password previously supplied for this IAT.");
                            return;
                        }/*
                        if (DataPassword != String.Empty)
                        {
                            if (!MySOAP.VerifyPassword(Properties.Resources.sDataProviderServlet, MainForm.IATName, CPartiallyEncryptedRSAKey.EKeyType.Data, DataPassword))
                            {
                                if (DisplayYesNoMessageBox("Passwords Do Not Match", "The data retrieval password you entered does not match the password previously supplied.  Would you like to replace the old password" +
                                    " with the new one?") != DialogResult.Yes)
                                {
                                    DataPasswordForm dpf = new DataPasswordForm();
                                    DataPasswordForm.EDataPassword dataPassResult = DisplayDataPassword(dpf);
                                    if (dataPassResult == DataPasswordForm.EDataPassword.cancel)
                                    {
                                        OnEndProgressBarUse();
                                        return;
                                    }
                                    else if (dataPassResult == DataPasswordForm.EDataPassword.noMatch)
                                    {
                                        OperationFailed("Wrong Password", "The password you supplied does not match the previous data retrieval password.");
                                        return;
                                    }
                                    CResultRencryption rencryption = new CResultRencryption(MainForm.IATName, false);
                                    if (!rencryption.ChangeDataPassword(dpf.Password, MainForm.DataRetrievalPassword))
                                        return;
                                }
                            }
                        }
                        MySOAP.CurrentTransactionEvent.AddChildEvent("Retrieving IAT Encryption Data from Server");
                        trans.Transaction = TransactionRequest.ETransaction.RequestRSAKey;
                        trans.IATName = MainForm.IATName;
                        trans.StringValue = CPartiallyEncryptedRSAKey.EKeyType.Data.ToString();
                        trans.IsLastTransaction = false;
                        MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, MySOAP.ESoapAction.RequestRSAKey, trans, dataKey);
                        trans.StringValue = CPartiallyEncryptedRSAKey.EKeyType.Admin.ToString();
                        MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, MySOAP.ESoapAction.RequestRSAKey, trans, adminKey);   */
                    /*
                    }
                    else
                    {
                        MySOAP.CurrentTransactionEvent.AddChildEvent("Generating IAT Encryption Data");
                        dataKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
                        dataKey.Generate(DataPassword);
                        adminKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Admin);
                        adminKey.Generate(AdminPassword);
                    }
                    SetStatusMessage(Properties.Resources.sUnpackingPackageFileMsg);
                    ResetProgress();
                    ServerContext = ServerURL.Substring(0, ServerURL.IndexOf("/IATServer") + "/IATServer".Length);
                    ProcessPackageFile(clientID);
                    ConfigFileData = new MemoryStream();
                    XmlWriter xmlWriter = new XmlTextWriter(ConfigFileData, Encoding.Unicode);
                    ConfigFile.WriteXml(xmlWriter);
                    xmlWriter.Flush();
                    SchemalessIATData = GenerateSchemalessXML(ConfigFile);
                    IATManifest = GenerateFileManifest();
                    MySOAP.CurrentTransactionEvent.AddChildEvent("Sending IAT File Manifest");
                    MySOAP.CallSOAP(ServerURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.InitiateIATDeployment, IATManifest, inTrans);
                    if (inTrans.Transaction == TransactionRequest.ETransaction.TransactionFail)
                    {
                        ERegisterIATResult regIATResult = (ERegisterIATResult)Enum.Parse(typeof(ERegisterIATResult), inTrans.StringValue);
                        if (regIATResult == ERegisterIATResult.InsufficientDiskSpace)
                        {
                            OperationFailed("Insufficient Disk Space", "You do not have enough disk space alotted to your account to upload this IAT.  If you wish to more or have questions regarding this, please contact us at admin@iatsoftware.net");
                            return;
                        }
                        else if (regIATResult == ERegisterIATResult.InsufficientIATs)
                        {
                            OperationFailed("Insufficient IATs", "You have already uploaded as many IATs as permitted by your alotted quota.  If you wish to increase your quoata or have questions regarding this, please contact us at admin@iatsoftware.net");
                            return;
                        }
                        else if (regIATResult == ERegisterIATResult.DatabaseError)
                        {
                            throw new IATUploadException("Database Error", MySOAP.TerminateTransaction("The IAT server encountered an error while attempting to register your IAT.  If this problem persists, please contact us at admin@iatsoftware.net."));
                        }
                    }
                    if (!bIATExists)
                    {
                        MySOAP.CurrentTransactionEvent.AddChildEvent("Recording IAT Encryption Data");
                        dataKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
                        dataKey.Generate(MainForm.DataRetrievalPassword);
                        adminKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Admin);
                        adminKey.Generate(MainForm.AdminPassword);
                        CRSAKeyPair keyPair = new CRSAKeyPair(dataKey, adminKey);
                        MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RecordEncryptionKey, keyPair, inTrans);
                        if (inTrans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
                            throw new IATUploadException("Server Error", MySOAP.TerminateTransaction("An error occurred while attempting to register your IAT encryption key."));
                    }
                    /*
                else
                {
                    MySOAP.CurrentTransactionEvent.AddChildEvent("
                    trans.Transaction = TransactionRequest.ETransaction.RequestRSAKey;
                    trans.StringValue = CPartiallyEncryptedRSAKey.EKeyType.Data.ToString();
                    trans.IsLastTransaction = false;
                    dataKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
                    MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, MySOAP.ESoapAction.RequestRSAKey, trans, dataKey);
                    adminKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Admin);
                    trans.StringValue = CPartiallyEncryptedRSAKey.EKeyType.Admin.ToString();
                    trans.IsLastTransaction = false;
                    MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, MySOAP.ESoapAction.RequestRSAKey, trans, adminKey);
                }*//*
                    trans.Transaction = TransactionRequest.ETransaction.DoIATDeploy;
                    MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.DoIATDeploy, trans, inTrans);
                    if (inTrans.Transaction != TransactionRequest.ETransaction.RequestTransmission)
                        throw new IATUploadException("Server Error", MySOAP.TerminateTransaction("The IAT server encountered an error while preparing to receive your IAT.  If this problem persists, please contact us at admin@iatsoftware.net."));
                    CPacketTransmission.ETransmissionResult transResult = SendManifest();
                    if (transResult == CPacketTransmission.ETransmissionResult.Fail)
                        throw new IATUploadException("Server Error", MySOAP.TerminateTransaction("An error occurred while uploading your IAT.  This could be due to connectivity issues or might be an error on the server end."));
                    ResetProgress();
                    CDeploymentProgressUpdate.EStage deploymentStage = CDeploymentProgressUpdate.EStage.unset;
                    trans.Transaction = TransactionRequest.ETransaction.QueryDeploymentProgress;
                    CDeploymentProgressUpdate update = new CDeploymentProgressUpdate();
                    int nDeploymentStage = -1;
                    MySOAP.BeginNewTransactionEvent("Awaiting Deployment Confirmation");
                    TransactionEvent tEvent = null;
                    while ((deploymentStage != CDeploymentProgressUpdate.EStage.done) && (deploymentStage != CDeploymentProgressUpdate.EStage.timerExpired)
                        || (deploymentStage != CDeploymentProgressUpdate.EStage.failed))
                    {
                        MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.QueryDeploymentProgress, trans, update);
                        if (nDeploymentStage != update.StageNum)
                        {
                            if (tEvent != null)
                                if (tEvent.MaxProgressValue != -1)
                                    tEvent.ProgressValue = tEvent.MaxProgressValue;
                            tEvent = MySOAP.CurrentTransactionEvent.AddChildEvent(update.StatusMessage);
                            SetStatusMessage(update.StatusMessage);
                            if (update.ProgressMax > 0)
                            {
                                tEvent.MaxProgressValue = update.ProgressMax;
                                tEvent.ProgressValue = update.CurrentProgress;
                                SetProgressRange(0, update.ProgressMax);
                                SetProgressValue(update.CurrentProgress);
                            }
                            if (update.ActiveItem == String.Empty)
                                SetStatusMessage(update.StatusMessage);
                        }
                        else if (update.ProgressMax != -1)
                        {
                            tEvent.ProgressValue = update.CurrentProgress;
                            SetProgressValue(update.CurrentProgress);
                        }
                        deploymentStage = update.Stage;
                    }
                    trans.Transaction = TransactionRequest.ETransaction.VerifyIATDeployment;
                    trans.IATName = MainForm.IATName;
                    MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.VerifyIATDeployment, trans, inTrans);
                    if (inTrans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
                    {
                        EDeployResult deployResult = (EDeployResult)Enum.Parse(typeof(EDeployResult), inTrans.StringValue);
                        String deployErrorMessage;
                        switch (deployResult)
                        {
                            case EDeployResult.backupLost:
                                deployErrorMessage = "Prior to deploying your IAT, the server made a backup of the existing IAT. The result set generated by your new IAT was of a different format " +
                                    "from the result set of the existing IAT. Incongruous result set formats cannot coexist in the database for the same IAT so your attempt to overwrite the existing " +
                                    "failed. Furthermore, the server failed to restore the backup it created of your existing IAT. You may redeploy your original IAT without error. If you do not have " +
                                    "a copy of your original IAT, please contact us at admin@iatsoftware.net so we can attempt to restore your original IAT.";
                                break;

                            case EDeployResult.cannotBackup:
                                deployErrorMessage = "Prior to attempting the redeployment of your IAT, the server attempted to backup the existing IAT. This backup failed. It is unlikely that your " +
                                    "original IAT was lost. However, you should ensure that it still administers correctly. If you encounter an error, please contact us at admin@iatsoftware.net so can " +
                                    "attempt to restore your original IAT. It is unwise to attempt to redeploy your original IAT if you encounter an administration error.";
                                break;

                            case EDeployResult.databaseError:
                                deployErrorMessage = "A database error occurred on the server. If this problem persists, please contact us at admin@iatsoftware.net.";
                                break;

                            case EDeployResult.deploymentTimerExpired:
                                deployErrorMessage = "A long period of inactivity occured during IAT deployment and the server presummed deployment had failed. If this was due to internet connectivity issues " +
                                    "during deployment, please try again. If this problem persists, please contact us at admin@iatsoftware.net.";
                                break;

                            case EDeployResult.fileDeploymentError:
                                deployErrorMessage = "An error occurred either in the the transfer of your IAT package or the server was unable to process and store your IAT. This could be due " +
                                    "to a corrupt package file. Please repackage your IAT and reattempt deployment. If this does not resolve the issue, please contact us at admin@iatsoftware.net.";
                                break;

                            case EDeployResult.genericError:
                                deployErrorMessage = "The server failed to deploy your IAT. Please try again. If this problem persists, please contact us at admin@iatsoftware.net.";
                                break;

                            case EDeployResult.incompatibleResultDescriptors:
                                deployErrorMessage = "The result set format that would be generated by your new IAT is not compatible with the result set format of the existing IAT. The server " +
                                    "would be unable to continue recording result data atop the result data already collected and so redeployment was rejected. If you have deleted your result data or if no result data has been collected yet, please delete your IAT as well " +
                                    "and try again.";
                                break;

                            case EDeployResult.transformError:
                                deployErrorMessage = "The server encountered an error while processing your IAT. This could be due " +
                                    "to a corrupt package file. Please repackage your IAT and reattempt deployment. If this does not resolve the issue, please contact us at admin@iatsoftware.net.";
                                break;

                            default:
                                deployErrorMessage = "A corrupt transmission was recieved from the server while attempting to verify the deployment of your IAT. Please check to see if your IAT " +
                                    "administers correctly by copying the following URL into the address bar of your web browser.  Please either bookmark this URL or copy and paste it into a file " +
                                    "as it is the location of your IAT on the Internet:\r\n" + ServerURL.Substring(0, ServerURL.LastIndexOf("/") + 1) + String.Format(Properties.Resources.sIATServletURLPart, ConfigFile.Name, clientID) +
                                    "\r\nIf your IAT does not administer correctly, attempt redeployment. If this does not resolve the issue, please contact us at admin@iatsoftware.net.";
                                break;

                        }
                        throw new IATUploadException("IAT Deployment Error", MySOAP.TerminateTransaction(deployErrorMessage));
                    }
                    MySOAP.EndTransaction();
                }
                catch (UploadAbortedException)
                {
                    MySOAP.TerminateConnection(Properties.Resources.sDataProviderServlet);
                }
                catch (TimeoutException ex)
                {
                    throw new IATUploadException(ex.Message, "Server Not Responsive", ex);
                }
                catch (WebException ex)
                {
                    throw new IATUploadException(ex.Message, "Server Error", ex);
                }
                catch (CXmlSerializationException ex)
                {
                    throw new IATUploadException(ex.Message, "Server Error", ex);
                }
            }
            catch (CXmlSerializationException ex)
            {
                ErrorReportDisplay errorDisplay = new ErrorReportDisplay(ex.Message, ex);
                ShowForm(errorDisplay);
            }
        }

        public void DeployIAT()
        {
            _AbortFlag = false;
            ThreadStart threadStart = new ThreadStart(run);
            Thread thread = new Thread(threadStart);
            MainForm.BeginProgressBarUse(OnAbort, IATConfigMainForm.EProgressBarUses.Upload);
            thread.Start();
        }
    }

}
*/