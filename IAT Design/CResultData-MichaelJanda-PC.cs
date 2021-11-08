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

namespace IATClient
{
    public class CResultData
    {
        public delegate void DataRetrievalCompleteHandler(bool bSuccess, CResultData results);

        private DataRetrievalCompleteHandler OnDataRetrievalComplete = null;
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

        public bool Aborted
        {
            get
            {
                lock (lockObject)
                {
                    return _Aborted;
                }
            }
        }

        public void Abort()
        {
            lock (lockObject)
            {
                _Aborted = true;
            }
        }


        private bool IncrementProgress(int n)
        {
            lock (lockObject)
            {
                if (Aborted)
                    return false;
                Action<int> progressIncrement = MainForm.ProgressIncrement;
                MainForm.Invoke(progressIncrement, n);
            }
            return true;
        }

        public bool SetStatusMessage(String s)
        {
            lock (lockObject)
            {
                if (Aborted)
                    return false;
                Action<String> setStatusMessage = MainForm.SetStatusMessage;
                MainForm.BeginInvoke(setStatusMessage, s);
            }
            return true;
        }

        public bool ResetProgressBar()
        {
            lock (lockObject)
            {
                if (Aborted)
                    return false;
                Action resetProgress = MainForm.ResetProgress;
                MainForm.BeginInvoke(resetProgress);
            }
            return true;
        }

        public bool SetProgressBarRange(int min, int max)
        {
            lock (lockObject)
            {
                if (Aborted)
                    return false;
                Action<int, int> setProgressRange = MainForm.SetProgressRange;
                MainForm.Invoke(setProgressRange, min, max);
            }
            return true;
        }

        public void OnOperationFailed(String caption, String msg)
        {
            lock (lockObject)
            {
                if (Aborted)
                    return;
                MainForm.Invoke(OperationFailed, msg, caption);
                DataRetrievalCompleteInvokeTarget.Invoke(OnDataRetrievalComplete, false, this);
            }
        }

        private void OnDataRetrievalFailed(String errorMsg)
        {
            OnOperationFailed("Error Retrieving Data", errorMsg);
        }

        public bool OnCancel(int waitTimeout, bool forceAbort)
        {
            if (!Monitor.TryEnter(lockObject, waitTimeout))
            {
                if (forceAbort)
                {
                    MySOAP.AbortCurrentTransaction();
                    Monitor.Enter(lockObject);
                }
                else
                    return false;
            }
            _Aborted = true;
            OnEndProgressBarUse();
            Monitor.Exit(lockObject);
            return true;
        }

        public void OnEndProgressBarUse()
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(EndProgressBarUse);
            }
        }

        public void Abort(object sender, EventArgs e)
        {
            lock (lockObject)
            {
                _Aborted = true;
            }
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

        public CResultData(ResultSetDescriptor descriptor, List<CResultPacket> packetList)
        {
            Descriptor = descriptor;
            ProcessPacketQueue(packetList);
            bScored = false;
            Encryption = new RSACryptoServiceProvider();
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
        */
        private bool ProcessPacketQueue(List<CResultPacket> packetQueue)
        {
            SetStatusMessage("Processing Results");
            SetProgressBarRange(0, packetQueue.Count);
            ResetProgressBar();
            _IATResult = Descriptor.CreateResultData();
            _IATResult.NumResultSets = packetQueue.Count;
            for (int ctr = 0; ctr < packetQueue.Count; ctr++)
            {
                if (!IncrementProgress(1))
                    return false;
                _IATResult.AppendResultSet(packetQueue[ctr].GenerateResultSet(Descriptor.RSADataKey));
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
                int dataVersion = inTrans.IntValue;
                CPacket packet;
                outTrans = new TransactionRequest();
                outTrans.Transaction = TransactionRequest.ETransaction.RequestPacket;
                outTrans.IATName = IATName;
                outTrans.IsLastTransaction = true;
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
                outTrans.IsLastTransaction = true;
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

        private void DoRetrieveData()
        {
            try
            {
                if (!RetrieveResultSetDescriptor())
                    return;
                if (!RetrieveResultData())
                    return;
                OnEndProgressBarUse();
                if ((DataRetrievalCompleteInvokeTarget != null) && (OnDataRetrievalComplete != null))
                    DataRetrievalCompleteInvokeTarget.Invoke(OnDataRetrievalComplete, true, this);
            }
            catch (XmlException ex)
            {

            }
            catch (Exception)
            {
                OnOperationFailed("Generic Error", "Failed to retrieve data");
            }
        }

        private IATConfigMainForm.EndProgressBarUseHandler EndProgressBarUse = null;
        private IATConfigMainForm.ResetProgressHandler ResetProgress = null;
        private IATConfigMainForm.SetProgressRangeHandler SetProgressRange = null;
        private IATConfigMainForm.ProgressIncrementHandler ProgressIncrement = null;
        private IATConfigMainForm.OperationFailedHandler OperationFailed = null;
        private IATConfigMainForm.SetStatusMessageHandler SetProgressMessage = null;
        private IATConfigMainForm.ShowFormHandler ShowForm;


        public void RetrieveData()
        {
            EndProgressBarUse = new IATConfigMainForm.EndProgressBarUseHandler(MainForm.EndProgressBarUse);
            ResetProgress = new IATConfigMainForm.ResetProgressHandler(MainForm.ResetProgress);
            SetProgressRange = new IATConfigMainForm.SetProgressRangeHandler(MainForm.SetProgressRange);
            ProgressIncrement = new IATConfigMainForm.ProgressIncrementHandler(MainForm.ProgressIncrement);
            OperationFailed = new IATConfigMainForm.OperationFailedHandler(MainForm.OperationFailed);
            SetProgressMessage = new IATConfigMainForm.SetStatusMessageHandler(MainForm.SetStatusMessage);
            ShowForm = new IATConfigMainForm.ShowFormHandler(MainForm.ShowForm);
            MainForm.BeginProgressBarUse(OnCancel, IATConfigMainForm.EProgressBarUses.DataRetrieval);
            MainForm.AddToolStripCancelButton(OnCancel);
            ThreadStart threadStart = new ThreadStart(DoRetrieveData);
            Thread retriever = new Thread(threadStart);
            retriever.Start();
        }

        public void RetrieveData(Control invokeTarget, DataRetrievalCompleteHandler completeHandler)
        {
            EndProgressBarUse = new IATConfigMainForm.EndProgressBarUseHandler(MainForm.EndProgressBarUse);
            ResetProgress = new IATConfigMainForm.ResetProgressHandler(MainForm.ResetProgress);
            SetProgressRange = new IATConfigMainForm.SetProgressRangeHandler(MainForm.SetProgressRange);
            ProgressIncrement = new IATConfigMainForm.ProgressIncrementHandler(MainForm.ProgressIncrement);
            OperationFailed = new IATConfigMainForm.OperationFailedHandler(MainForm.OperationFailed);
            SetProgressMessage = new IATConfigMainForm.SetStatusMessageHandler(MainForm.SetStatusMessage);
            ShowForm = new IATConfigMainForm.ShowFormHandler(MainForm.ShowForm);
            MainForm.BeginProgressBarUse(OnCancel, IATConfigMainForm.EProgressBarUses.DataRetrieval);
            MainForm.AddToolStripCancelButton(OnCancel);
            OnDataRetrievalComplete = completeHandler;
            DataRetrievalCompleteInvokeTarget = invokeTarget;
            ThreadStart threadStart = new ThreadStart(DoRetrieveData);
            Thread retriever = new Thread(threadStart);
            retriever.Start();
        }

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
