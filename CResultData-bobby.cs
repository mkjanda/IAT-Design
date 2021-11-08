using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Threading;
using System.Security.Cryptography;
using System.Windows.Forms;
using IATClient.IATResultSetNamespaceV2;
using WebSocket4Net;

namespace IATClient
{
    public class CResultData
    {
        public delegate void DataRetrievalCompleteHandler(bool bSuccess, CResultData results);
        private CPartiallyEncryptedRSAKey DataKey;
        private DataRetrievalCompleteHandler OnDataRetrievalComplete = null;
        private ManualResetEvent TransactionComplete = new ManualResetEvent(false);
        private Control DataRetrievalCompleteInvokeTarget = null;
        private IATConfigMainForm MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
        private IResultData _IATResult;
        private ResultSetDescriptor Descriptor = null;
        public enum EOutputGrouping { groupedByItem, groupedByTestee, none };
        public enum EDelimitation { comma, space, tab };
        private bool bScored;
        private String ServerURL, IATName, DataRetrievalPassword;
        private List<MemoryStream> SurveySource = new List<MemoryStream>();
        private RSACryptoServiceProvider Encryption = null;
        private object lockObject = new object();
        private bool _Aborted = false, _DataRetrievalComplete = false;
        private bool _RetrievalRunning = false;
        private double _SD = double.NaN, _Mean = double.NaN;
        private WebSocket webSocket;
        private List<CPacket> RSDPacketQueue = new List<CPacket>(), ResultPacketQueue = new List<CPacket>();
        public enum ETransactionResult { success, failed, exception };
        private ETransactionResult TransactionResult = ETransactionResult.failed;
        private Exception _TransactionException;
        private enum ERetrievalStage
        {
            shakingHands = 0, loggingIn = 1, requestingResultSetDescriptor = 2, downloadingResultSetDescriptor = 3, requestingResults = 4,
            downloadingResults = 5, requestingItemSize = 6, requestingSlideManifest = 7, downloadingSlides = 8
        };
        private ERetrievalStage retrievalStage;

        public double Mean
        {
            get
            {
                return _IATResult.Mean;
            }
        }

        public double SD
        {
            get
            {
                return _IATResult.SD;
            }
        }

        public bool RetrievalRunning
        {
            get
            {
                lock (lockObject)
                {
                    return _RetrievalRunning;
                }
            }
        }

        public bool DataRetrievalComplete
        {
            get
            {
                lock (lockObject)
                {
                    return _DataRetrievalComplete;
                }
            }
        }


        private void Abort(object sender, EventArgs e)
        {
            webSocket.Close();
        }

        private void IncrementProgress(int n)
        {
            MainForm.Invoke(new Action<int>(MainForm.ProgressIncrement), n);
        }

        private void SetStatusMessage(String s)
        {
            MainForm.Invoke(new Action<String>(MainForm.SetStatusMessage), s);
        }

        private void ResetProgressBar()
        {
            MainForm.Invoke(new Action(MainForm.ResetProgress));
        }

        private void SetProgressBarRange(int min, int max)
        {
            MainForm.Invoke(new Action<int, int>(MainForm.SetProgressRange), min, max);
        }

        public IResultData ResultsInterface
        {
            get
            {
                return _IATResult;
            }
        }

        public IATResultSetList IATResults
        {
            get
            {
                return (IATResultSetList)_IATResult;
            }
        }

        public ResultSetDescriptor ResultDescriptor
        {
            get
            {
                return Descriptor;
            }
        }

        public IATConfigFileNamespace.ConfigFile IATConfiguration
        {
            get
            {
                return Descriptor.ConfigFile;
            }
        }

        public Exception TransactionException
        {
            get
            {
                return _TransactionException;
            }
        }

        public CResultData(String ServerURL, String IATName, String DataRetrievalPassword)
        {
            MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            _IATResult = null;
            bScored = false;
            this.ServerURL = ServerURL;
            this.IATName = IATName;
            this.DataRetrievalPassword = DataRetrievalPassword;
            Encryption = new RSACryptoServiceProvider();
        }
        /*
                public CResultData(BinaryReader bReader, IResultElemFactory factory)
                {
                    MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
        //            _IATResult = new 
                    _IATResult.Load(bReader);
                    int XmlLength = bReader.ReadInt32();
                    MemoryStream memStream = new MemoryStream(bReader.ReadBytes(XmlLength), 0, XmlLength);
                    StreamReader strmReader = new StreamReader(memStream, Encoding.Unicode);
                    XmlSerializer serializer = new XmlSerializer(typeof(IATConfigFileNamespace.ConfigFile));
                    memStream.Dispose();
                    bScored = false;
                    Encryption = new RSACryptoServiceProvider();
                }
                public void Save(BinaryWriter bWriter)
                {
                    MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
                    IATResults.Save(bWriter);
                    MemoryStream memStream = new MemoryStream();
                    XmlWriter xmlWriter = new XmlTextWriter(memStream, Encoding.Unicode);
                    XmlSerializer serializer = new XmlSerializer(typeof(IATConfigFileNamespace.ConfigFile));
                    serializer.Serialize(xmlWriter, IATConfiguration);
                    xmlWriter.Flush();
                    bWriter.Write(Convert.ToInt32(memStream.Length));
                    bWriter.Write(memStream.GetBuffer(), 0, (int)memStream.Length);
                    memStream.Dispose();
                }
                */ /*
        private bool ProcessPacketQueue(List<CPacket> packetQueue)
        {
            ResetProgressBar();
            SetStatusMessage("Processing Results");
            SetProgressBarRange(0, packetQueue.Count);
            List<CResultPacket> resultPackets = new List<CResultPacket>();
            MemoryStream memStream = new MemoryStream();
            foreach (CPacket p in packetQueue)
                memStream.Write(p.ByteData, 0, p.ByteData.Length);
            TextReader txtReader = new StreamReader(memStream, Encoding.UTF8);
            XmlReader xReader = new XmlTextReader(txtReader);
            xReader.ReadToDescendant("Packet");
            while (xReader.Name == "Packet")
            {
                CResultPacket rPacket = new CResultPacket(Descriptor.BeforeSurveys.Count, Descriptor.AfterSurveys.Count, Descriptor);
                rPacket.ReadXml(xReader);
                resultPackets.Add(rPacket);
            }
            _IATResult = Descriptor.CreateResultData();
            _IATResult.NumResultSets = resultPackets.Count;
            for (int ctr = 0; ctr < resultPackets.Count; ctr++)
            {
                if (!IncrementProgress(1))
                    return false;
                _IATResult.AppendResultSet(resultPackets[ctr].GenerateResultSet(Descriptor.GetRSADataKey(DataRetrievalPassword)));
            }
            if (Descriptor.Norms != null)
            {
                //     _IATResult.SD = Descriptor.Norms.SD;
                //   _IATResult.Mean = Descriptor.Norms.Mean;
                // _IATResult.CalcPercentileScores();
            }
            return true;
        }
        
        private bool NegotiateConnection()
        {
            SetStatusMessage("Connecting to server");
            UriBuilder uBuilder = new UriBuilder();
            String hostAndPath = Properties.Resources.sDataProviderServlet.Substring("http://".Length);
            uBuilder.Host = hostAndPath.Substring(0, hostAndPath.IndexOf("/"));
            uBuilder.Path = hostAndPath.Substring(hostAndPath.IndexOf("/"));
            uBuilder.Port = Convert.ToInt32(Properties.Resources.sDataProviderPort);
            MySOAP.EstablishEncryption(Properties.Resources.sDataProviderServlet);
            if (MySOAP.ShakeHands(Properties.Resources.sDataProviderServlet, IATName).Transaction != TransactionRequest.ETransaction.RequestTransmission)
            {
                OnOperationFailed("Server Error", "An error was encountered while negotiating communication with the server.  If this problem persists, please contact us at admin@iatsoftware.net.");
                return false;
            }
            if (MySOAP.VerifyPassword(IATName, CPartiallyEncryptedRSAKey.EKeyType.Data, DataRetrievalPassword) != TransactionRequest.ETransaction.TransactionSuccess)
            {
                OnOperationFailed("Incorrect Password", "The password you supplied for data retrieval is incorrect.");
                return false;
            }
            return true;
        }

        private void OnSurveyReceived(String filename, byte[] fileData)
        {
            if (IncrementProgress(1))
            {
                OnEndProgressBarUse();
                Abort();
                return;
            }
            SurveySource.Add(new MemoryStream(fileData));
        }

        private bool RetrieveResultSetDescriptor()
        {
            try
            {
                SetStatusMessage("Retrieving Result Set Descriptor");
                TransactionEvent tEvent = MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.DataRetrieval);
                if (!NegotiateConnection())
                    return false;
                TransactionRequest outTrans = new TransactionRequest();
                outTrans.Transaction = TransactionRequest.ETransaction.RequestResultDescriptor;
                outTrans.IATName = IATName;
                TransactionRequest inTrans = new TransactionRequest();
                MySOAP.CallSOAP(ServerURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestResultDescriptor, outTrans, inTrans);
                if (inTrans.Transaction != TransactionRequest.ETransaction.RequestRetrievalReady)
                {
                    OperationFailed("Server Error", "An error was encountered while negotiating communication with the server.  If this problem persists, please contact us at admin@iatsoftware.net.");
                    return false;
                }
                CPacket packet;
                outTrans = new TransactionRequest();
                outTrans.Transaction = TransactionRequest.ETransaction.RequestPacket;
                outTrans.IATName = IATName;
                MemoryStream resultDescriptorStream = new MemoryStream();
                BinaryWriter bWriter = new BinaryWriter(resultDescriptorStream);
                do
                {
                    packet = new CPacket();
                    MySOAP.CallSOAP(ServerURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestPacket, outTrans, packet);
                    if (!packet.IsNullPacket)
                        bWriter.Write(packet.ByteData);
                } while (!packet.IsNullPacket);
                resultDescriptorStream.Seek(0, SeekOrigin.Begin);
                TextReader txtReader = new StreamReader(resultDescriptorStream, System.Text.Encoding.UTF8);
                XmlTextReader reader = new XmlTextReader(txtReader);
                Descriptor = new ResultSetDescriptor(DataRetrievalPassword, dataVersion);
                Descriptor.ReadXml(reader);
                return true;
            }
            catch (CXmlSerializationException ex)
            {
                if (ex.ErrorType == CXmlSerializationException.EErrorType.fatal)
                {
                    ErrorReportDisplay errorDisplay = null;
                    errorDisplay = new ErrorReportDisplay(ex.Message, ex);
                    lock (lockObject)
                    {
                        if (!Aborted)
                        {
                            MainForm.ShowForm(errorDisplay);
                        }
                    }
                }
                else
                {
                    ErrorReportDisplay errorDisplay = null;
                    errorDisplay = new ErrorReportDisplay(ex.Message, ex);
                    lock (lockObject)
                    {
                        if (!Aborted)
                        {
                            MainForm.ShowForm(errorDisplay);
                        }
                    }
                }
                return false;
            }
            finally
            {
                MySOAP.EndTransaction();
            }
        }
         */
        /*
        private bool RetrieveSurveys()
        {
            SetStatusMessage("Retrieving Surveys");
            long sessID = MySOAP.EstablishEncryption(ServerURL);
            if (sessID == 0)
                return false;
            Manifest manifest = new Manifest();
            for (int ctr = 0; ctr < Descriptor.BeforeSurveys.Count; ctr++)
                manifest.Contents.Add(new ManifestFile(Descriptor.BeforeSurveys[ctr].Name + ".xml.survey"));
            for (int ctr = 0; ctr < Descriptor.AfterSurveys.Count; ctr++)
                manifest.Contents.Add(new ManifestFile(Descriptor.AfterSurveys.Count[ctr] + ".xml.survey"));
            TransactionRequest trans = new TransactionRequest();
            String soapMsg = MySOAP.CreateSOAPEnvelope(manifest);
            MySOAP.CallSOAP(ServerURL, "SendManifest", soapMsg, trans, sessID);
            trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RetrieveSurveys;
            trans.IATName = IATName;
            soapMsg = MySOAP.CreateSOAPEnvelope(trans);
            MySOAP.CallSOAP(ServerURL, "TransactionRequest", soapMsg, trans, sessID);
            if (trans.Transaction != TransactionRequest.ETransaction.RequestTransmission)
            {
                OnOperationFailed("Server Error", "An error was encountered while negotiating communication with the server.  If this problem persists, please contact us at admin@iatsoftware.net.");
                return false;
            }
            CPacketReceiver packetReceiver = new CPacketReceiver(manifest, new FileReceivedHandler(OnSurveyReceived));
            packetReceiver.Start();
            CPacket packet = new CPacket();
            trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestPacket;
            soapMsg = MySOAP.CreateSOAPEnvelope(trans);
            ResetProgressBar();
            SetProgressBarRange(0, manifest.Contents.Count);
            do
            {
                packet = new CPacket();
                MySOAP.CallSOAP(ServerURL, "TransactionRequest", soapMsg, packet, sessID);
                if (Aborted)
                {
                    packetReceiver.Halt();
                    return false;
                }
                if (!packet.IsNullPacket)
                    packetReceiver.QueuePacket(packet);
            } while (!packet.IsNullPacket);
            return true;
        }
        */
        /*
        private bool RetrieveResultData()
        {
            try
            {
                ResetProgressBar();

                TransactionEvent tEvent = MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.DataRetrieval);
                SetStatusMessage("Retrieving Result Data");
                if (!NegotiateConnection())
                    return false;
                TransactionRequest outTrans = new TransactionRequest();
                outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                outTrans.IATName = IATName;
                outTrans.IsLastTransaction = false;
                outTrans.StringValue = "Data";
                CPartiallyEncryptedRSAKey rsaKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
                MySOAP.CallSOAP(ServerURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestRSAKey, outTrans, rsaKey);
                if (rsaKey.IsNullKey())
                {
                    OnOperationFailed("Server Error", "An error while retrieving your encryption information from the server.  This is likely due to database problems.  If this problem persists, please contact us at admin@iatsoftware.net.");
                    return false;
                }
                outTrans.Transaction = TransactionRequest.ETransaction.RetrieveResults;
                TransactionRequest inTrans = new TransactionRequest();
                MySOAP.CallSOAP(ServerURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RetrieveResults, outTrans, inTrans);
                if (inTrans.Transaction != TransactionRequest.ETransaction.RequestRetrievalReady)
                {
                    OnOperationFailed("Server Error", "An error was encountered while negotiating communication with the server.  If this problem persists, please contact us at admin@iatsoftware.net.");
                    return false;
                }
                CResultPacket packet;
                List<CResultPacket> packetList = new List<CResultPacket>();
                outTrans = new TransactionRequest();
                outTrans.Transaction = TransactionRequest.ETransaction.RequestPacket;
                SetProgressBarRange(0, Descriptor.NumResults);
                if (Descriptor.NumResults == 0)
                {
                    OnOperationFailed("No results", "No results exist on the server for this IAT.");
                    return false;
                }
                do
                {
                    packet = new CResultPacket(Descriptor.BeforeSurveys.Count, Descriptor.AfterSurveys.Count, Descriptor);
                    MySOAP.CallSOAP(ServerURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestPacket, outTrans, packet);
                    if (!packet.IsNullPacket)
                    {
                        packetList.Add(packet);
                        if (!IncrementProgress(1))
                            return false;
                    }
                } while (!packet.IsNullPacket);
                if (!ProcessPacketQueue(packetList))
                    return false;
                return true;
            }
            catch (CXmlSerializationException ex)
            {
                IAsyncResult aResult = null;
                if (ex.ErrorType == CXmlSerializationException.EErrorType.fatal)
                {
                    ErrorReportDisplay errorDisplay = null;
                    errorDisplay = new ErrorReportDisplay(ex.Message, ex);
                    lock (lockObject)
                    {
                        if (!Aborted)
                        {
                            aResult = MainForm.BeginInvoke(ShowForm, errorDisplay);
                        }
                    }
                }
                else
                {
                    ErrorReportDisplay errorDisplay = null;
                    errorDisplay = new ErrorReportDisplay(ex.Message, ex);
                    lock (lockObject)
                    {
                        if (!Aborted)
                        {
                            aResult = MainForm.BeginInvoke(ShowForm, errorDisplay);
                        }
                    }
                }
                if (aResult != null)
                    MainForm.EndInvoke(aResult);
                return false;
            }
            finally
            {
                MySOAP.EndTransaction();
            }
        }
        */

        private void OperationFailed(String reason, String caption)
        {
            Action<String, String> operationFailed = new Action<String, String>(MainForm.OperationFailed);
            MainForm.BeginInvoke(operationFailed, reason, caption);
        }

        private void ShakeHands(INamedXmlSerializable obj)
        {
            HandShake hs = (HandShake)obj;
            CEnvelope.SendMessage(HandShake.CreateResponse(hs), webSocket);
        }

        private void OnTransaction(INamedXmlSerializable obj)
        {
            TransactionRequest outTrans, trans = (TransactionRequest)obj;
            switch (trans.Transaction)
            {
                case TransactionRequest.ETransaction.ClientFrozen:
                    OperationFailed(Properties.Resources.sClientFrozen, "Account Frozen");
                    webSocket.Close();
                    break;

                case TransactionRequest.ETransaction.ClientDeleted:
                    OperationFailed(Properties.Resources.sClientDeleted, "Account Deleted");
                    webSocket.Close();
                    break;

                case TransactionRequest.ETransaction.RequestTransmission:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.IATExists;
                    outTrans.IATName = IATName;
                    CEnvelope.SendMessage(outTrans, webSocket);
                    break;

                case TransactionRequest.ETransaction.NoSuchIAT:
                    OperationFailed(Properties.Resources.sNoSuchIAT, Properties.Resources.sNoSuchIATCaption);
                    webSocket.Close();
                    break;

                case TransactionRequest.ETransaction.IATExists:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                    outTrans.IATName = IATName;
                    CEnvelope.SendMessage(outTrans, webSocket);
                    break;

                case TransactionRequest.ETransaction.VerifyPassword:
                    try
                    {
                        byte[] encData = Convert.FromBase64String(trans.StringValue);
                        Encryption = new RSACryptoServiceProvider();
                        Encryption.ImportParameters(DataKey.GetRSAParameters());
                        byte[] data = Encryption.Decrypt(encData, false);
                        outTrans = new TransactionRequest();
                        outTrans.Transaction = TransactionRequest.ETransaction.VerifyPassword;
                        outTrans.StringValue = Convert.ToBase64String(data);
                        CEnvelope.SendMessage(outTrans, webSocket);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("The password you entered for this IAT is incorrect.", Properties.Resources.sInvalidIATPasswordCaption);
                        webSocket.Close();
                    }
                    break;

                case TransactionRequest.ETransaction.PasswordInvalid:
                    MessageBox.Show("The password you entered for this IAT is incorrect.", Properties.Resources.sInvalidIATPasswordCaption);
                    webSocket.Close();
                    break;

                case TransactionRequest.ETransaction.PasswordValid:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestResultDescriptor;
                    RSDPacketQueue.Clear();
                    SetStatusMessage("Retrieving result set descriptor");
                    CEnvelope.SendMessage(outTrans, webSocket);
                    break;
            }
        }

        private void KeyPairReceived(INamedXmlSerializable obj)
        {
            CRSAKeyPair keyPair = (CRSAKeyPair)obj;
            DataKey = keyPair.DataKey;
            try
            {
                DataKey.DecryptKey(DataRetrievalPassword);
            }
            catch (Exception ex)
            {
                MessageBox.Show(Properties.Resources.sInvalidIATPassword, Properties.Resources.sInvalidIATPasswordCaption);
                webSocket.Close();
            }
            TransactionRequest outTrans = new TransactionRequest();
            outTrans.Transaction = TransactionRequest.ETransaction.RequestDataPasswordVerification;
            outTrans.IATName = IATName;
            CEnvelope.SendMessage(outTrans, webSocket);
        }

        private void PacketReceived(INamedXmlSerializable obj)
        {
            lock (this)
            {
                CPacket p = (CPacket)obj;
                switch (p.PacketType)
                {
                    case CPacket.EType.ResultSetDescriptor:
                        if (p.Length > 0)
                            RSDPacketQueue.Add(p);
                        if (p.IsLastPacket)
                        {
                            BuildResultSetDescriptor();
                            TransactionRequest trans = new TransactionRequest();
                            trans.Transaction = TransactionRequest.ETransaction.RequestResults;
                            SetStatusMessage("Retrieving Results");
                            SetProgressBarRange(0, Descriptor.NumResults);
                            CEnvelope.SendMessage(trans, webSocket);
                        }
                        break;

                    case CPacket.EType.ResultData:
                        if (!p.IsNullPacket)
                        {
                            IncrementProgress(1);
                            ResultPacketQueue.Add(p);
                        }
                        if (p.IsLastPacket)
                        {
                            TransactionResult = ETransactionResult.success;
                            webSocket.Close();
                        }
                        break;
                }
            }
        }

        private void BuildResultSetDescriptor()
        {
            MemoryStream memStream = new MemoryStream();
            foreach (CPacket p in RSDPacketQueue)
                memStream.Write(p.ByteData, 0, p.Length);
            memStream.Seek(0, SeekOrigin.Begin);
            TextReader txtReader = new StreamReader(memStream, Encoding.UTF8);
            XmlReader xReader = new XmlTextReader(txtReader);
            xReader.MoveToContent();
            Descriptor = new ResultSetDescriptor();
            Descriptor.ReadXml(xReader);
        }

        private void ProcessResultData()
        {
            ResetProgressBar();
            SetStatusMessage("Processing Results");
            SetProgressBarRange(0, ResultPacketQueue.Count);
            _IATResult = Descriptor.CreateResultData();
            _IATResult.NumResultSets = ResultPacketQueue.Count;
            for (int ctr = 0; ctr < ResultPacketQueue.Count; ctr++)
            {
                IncrementProgress(1);
                ((CResultPacket)ResultPacketQueue[ctr]).SetResultElemFactory(Descriptor);
                IResultSet set = ((CResultPacket)ResultPacketQueue[ctr]).GenerateResultSet(DataKey);
                if (set != null)
                    _IATResult.AppendResultSet(set);
                else
                    _IATResult.NumResultSets--;
            }
            if (Descriptor.Norms != null)
            {
                //     _IATResult.SD = Descriptor.Norms.SD;
                //   _IATResult.Mean = Descriptor.Norms.Mean;
                // _IATResult.CalcPercentileScores();
            }
            ResetProgressBar();
        }

        private void OnDeploymentException(INamedXmlSerializable obj)
        {
            CDeploymentException ex = (CDeploymentException)obj;
            ErrorReportDisplay f = new ErrorReportDisplay(ex);
            MainForm.BeginInvoke(new Action<Form>(MainForm.ShowForm), f);
            webSocket.Close();
        }

        public ETransactionResult DoRetrieveData()
        {
            Descriptor = null;
            Action<EventHandler, IATConfigMainForm.EProgressBarUses> beginProgressBarUse = new Action<EventHandler, IATConfigMainForm.EProgressBarUses>(MainForm.BeginProgressBarUse);
            MainForm.Invoke(beginProgressBarUse, new EventHandler(Abort), IATConfigMainForm.EProgressBarUses.DataRetrieval);
            TransactionResult = ETransactionResult.failed;
            webSocket = new WebSocket(Properties.Resources.sDataTransactionWebsocketURI);
            webSocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(CEnvelope.MessageReceived);
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnTransaction);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.DeploymentException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Packet] = new Action<INamedXmlSerializable>(PacketReceived);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.ResultPacket] = new Action<INamedXmlSerializable>(PacketReceived);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(KeyPairReceived);
            webSocket.Opened += new EventHandler(WebSocket_Opened);
            webSocket.Closed += new EventHandler(WebSocket_Closed);
            webSocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(WebSocket_Error);
            SetStatusMessage("Connecting");
            webSocket.Open();
            TransactionComplete.WaitOne();
            if (TransactionResult != ETransactionResult.success)
                return TransactionResult;
            ProcessResultData();
            return TransactionResult;
        }

        private void WebSocket_Opened(object sender, EventArgs e)
        {
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            trans.IATName = IATName;
            CEnvelope.SendMessage(trans, webSocket);
        }

        private void WebSocket_Closed(object sender, EventArgs e)
        {
            webSocket.Dispose();
            TransactionComplete.Set();
            Action endProgressBarUse = new Action(MainForm.EndProgressBarUse);
            MainForm.Invoke(endProgressBarUse);
        }

        private void WebSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            _TransactionException = e.Exception;
            TransactionResult = ETransactionResult.exception;
        }
        /*
        private IATConfigMainForm.EndProgressBarUseHandler EndProgressBarUse = null;
        private IATConfigMainForm.ResetProgressHandler ResetProgress = null;
        private IATConfigMainForm.SetProgressRangeHandler SetProgressRange = null;
        private IATConfigMainForm.ProgressIncrementHandler ProgressIncrement = null;
        private IATConfigMainForm.OperationFailedHandler OperationFailed = null;
        private IATConfigMainForm.SetStatusMessageHandler SetProgressMessage = null;
        private IATConfigMainForm.ShowFormHandler ShowForm;
        */
        public bool ExportToFile(String FileName, EOutputGrouping grouping, EDelimitation delimitation)
        {
            if (_IATResult == null)
                return false;
            FileStream fOutStream = new FileStream(FileName, FileMode.Create);
            TextWriter outWriter = new StreamWriter(fOutStream);

            if (!bScored)
                Score();
            switch (grouping)
            {
                case EOutputGrouping.groupedByItem:
                    _IATResult.ExportIATLatenciesByItem(FileName);
                    break;

                case EOutputGrouping.groupedByTestee:
                    _IATResult.ExportIATLatenciesByTestee(FileName);
                    break;
            }

            outWriter.Close();
            fOutStream.Close();
            return true;
        }
        /*
        public bool ExportBeforeSurvey(String Filename)
        {
            FileStream fOutStream = new FileStream(Filename, FileMode.Create);
            TextWriter outWriter = new StreamWriter(fOutStream);

            for (int ctr1 = 0; ctr1 < IATResults.NumResultSets; ctr1++)
            {
                int nItems = 0;
                for (int ctr2 = 0; ctr2 < IATResults.IATResultSets[ctr1].BeforeSurvey.Length; ctr2++)
                    for (int ctr3 = 0; ctr3 < IATResults.IATResultSets[ctr1].BeforeSurvey[ctr2].SurveyResults.Length; ctr3++)
                        if (IATResults.IATResultSets[ctr1].BeforeSurvey[ctr2].SurveyResults.Length != 0)
                            nItems++;

                int itemCtr = 0;
                for (int ctr2 = 0; ctr2 < IATResults.IATResultSets[ctr1].BeforeSurvey.Length; ctr2++)
                {
                    for (int ctr3 = 0; ctr3 < IATResults.IATResultSets[ctr1].BeforeSurvey[ctr2].SurveyResults.Length; ctr3++)
                    {
                        if (itemCtr < nItems)
                            outWriter.Write(String.Format("{0}, ", IATResults.IATResultSets[ctr1].BeforeSurvey[ctr2].SurveyResults[ctr3]));
                        else
                            outWriter.WriteLine(IATResults.IATResultSets[ctr1].BeforeSurvey[ctr2].SurveyResults[ctr3]);
                        itemCtr++;
                    }
                }
            }
            outWriter.Close();
            fOutStream.Close();
            return true;
        }

        public bool ExportAfterSurvey(String Filename)
        {
            FileStream fOutStream = new FileStream(Filename, FileMode.Create);
            TextWriter outWriter = new StreamWriter(fOutStream);

            for (int ctr1 = 0; ctr1 < IATResults.NumResultSets; ctr1++)
            {
                int nItems = 0;
                for (int ctr2 = 0; ctr2 < IATResults.IATResultSets[ctr1].AfterSurvey.Length; ctr2++)
                    for (int ctr3 = 0; ctr3 < IATResults.IATResultSets[ctr1].AfterSurvey[ctr2].SurveyResults.Length; ctr3++)
                        if (IATResults.IATResultSets[ctr1].AfterSurvey[ctr2].SurveyResults.Length != 0)
                            nItems++;

                int itemCtr = 0;
                for (int ctr2 = 0; ctr2 < IATResults.IATResultSets[ctr1].AfterSurvey.Length; ctr2++)
                {
                    for (int ctr3 = 0; ctr3 < IATResults.IATResultSets[ctr1].AfterSurvey[ctr2].SurveyResults.Length; ctr3++)
                    {
                        if (itemCtr < nItems)
                            outWriter.Write(String.Format("{0}, ", IATResults.IATResultSets[ctr1].AfterSurvey[ctr2].SurveyResults[ctr3]));
                        else
                            outWriter.WriteLine(IATResults.IATResultSets[ctr1].AfterSurvey[ctr2].SurveyResults[ctr3]);
                        itemCtr++;
                    }
                }
            }
            outWriter.Close();
            fOutStream.Close();
            return true;
        }
        */
        protected void Score()
        {
            for (int ctr = 0; ctr < IATResults.NumResultSets; ctr++)
                IATResults[ctr].Score();
        }
    }


    class ResultDataRetrievalException : Exception
    {
        public enum EType { hostError, passwordError, iatNameError, transferError }
        private EType _Type;
        public EType Type
        {
            get
            {
                return _Type;
            }
        }

        public ResultDataRetrievalException(String msg, EType type)
            : base(msg)
        {
            _Type = type;
        }
    }
}
