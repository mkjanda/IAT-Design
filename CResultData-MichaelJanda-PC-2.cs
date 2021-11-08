using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Net;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Linq;


namespace IATClient
{
    public class CResultData
    {
        public delegate void DataRetrievalCompleteHandler(bool bSuccess, CResultData results);
        private CPartiallyEncryptedRSAKey DataKey;
        private ManualResetEvent TransactionComplete = new ManualResetEvent(false), TransactionFailed = new ManualResetEvent(false), TransactionAborted = new ManualResetEvent(false);
        private IATConfigMainForm MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
        private ResultSetDescriptor Descriptor = null;
        public enum EOutputGrouping { groupedByItem, groupedByTestee, none };
        public enum EDelimitation { comma, space, tab };
        private bool bScored, bAborted = false;
        private String ServerURL, IATName, DataRetrievalPassword;
        private List<MemoryStream> SurveySource = new List<MemoryStream>();
        private RSACryptoServiceProvider Encryption = null;
        private object lockObject = new object();
        private bool _DataRetrievalComplete = false;
        private bool _RetrievalRunning = false;
        private ClientWebSocket ResultWebSocket;
        private ArraySegment<byte> ReceiveBuffer = new ArraySegment<byte>(new byte[8192]);
        private CEnvelope IncomingMessage = null;
        private List<CPacket> RSDPacketQueue = new List<CPacket>(), ResultPacketQueue = new List<CPacket>();
        private object transmissionLock = new object();
        private CancellationToken AbortToken = new CancellationToken();

        public double Mean
        {
            get
            {
                return IATResults.Mean;
            }
        }

        public double SD
        {
            get
            {
                return IATResults.SD;
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


        private void Cancel(object sender, EventArgs e)
        {
            bAborted = true;
            TransactionAborted.Set();
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
                return IATResults;
            }
        }

        public IResultData IATResults { get; private set; } = null;

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

        public CResultData(String ServerURL, String IATName, String DataRetrievalPassword)
        {
            MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            bScored = false;
            this.ServerURL = ServerURL;
            this.IATName = IATName;
            this.DataRetrievalPassword = DataRetrievalPassword;
            Encryption = new RSACryptoServiceProvider();
        }

        private void OperationFailed(String reason, String caption)
        {
            Action<String, String> operationFailed = new Action<String, String>(MainForm.OperationFailed);
            MainForm.BeginInvoke(operationFailed, reason, caption);
            TransactionFailed.Set();
        }

        private void ShakeHands(INamedXmlSerializable obj)
        {
            HandShake hs = (HandShake)obj;
            CEnvelope env = new CEnvelope(HandShake.CreateResponse(hs));
            env.SendMessage(ResultWebSocket, AbortToken);
        }

        private void OnTransaction(INamedXmlSerializable obj)
        {
            try
            {
                TransactionRequest outTrans, trans = (TransactionRequest)obj;
                CEnvelope env;
                switch (trans.Transaction)
                {
                    case TransactionRequest.ETransaction.ClientFrozen:
                        OperationFailed(Properties.Resources.sClientFrozen, "Account Frozen");
                        break;

                    case TransactionRequest.ETransaction.ClientDeleted:
                        OperationFailed(Properties.Resources.sClientDeleted, "Account Deleted");
                        break;

                    case TransactionRequest.ETransaction.RequestTransmission:
                        outTrans = new TransactionRequest();
                        outTrans.Transaction = TransactionRequest.ETransaction.IATExists;
                        outTrans.IATName = IATName;
                        env = new CEnvelope(outTrans);
                        env.SendMessage(ResultWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.NoSuchIAT:
                        OperationFailed(Properties.Resources.sNoSuchIAT, Properties.Resources.sNoSuchIATCaption);
                        break;

                    case TransactionRequest.ETransaction.IATExists:
                        outTrans = new TransactionRequest();
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                        outTrans.IATName = IATName;
                        env = new CEnvelope(outTrans);
                        env.SendMessage(ResultWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.VerifyPassword:
                        try
                        {
                            byte[] encData = Convert.FromBase64String(trans.StringValues["EncryptedTestString"]);
                            Encryption = new RSACryptoServiceProvider();
                            Encryption.ImportParameters(DataKey.GetRSAParameters());
                            byte[] data = Encryption.Decrypt(encData, false);
                            outTrans = new TransactionRequest();
                            outTrans.Transaction = TransactionRequest.ETransaction.VerifyPassword;
                            outTrans.StringValues["DecryptedTestString"] = Convert.ToBase64String(data);
                            env = new CEnvelope(outTrans);
                            env.SendMessage(ResultWebSocket, AbortToken);
                        }
                        catch (Exception ex)
                        {
                            OperationFailed("The password you entered for this IAT is incorrect.", Properties.Resources.sInvalidIATPasswordCaption);
                        }
                        break;

                    case TransactionRequest.ETransaction.PasswordInvalid:
                        OperationFailed("The password you entered for this IAT is incorrect.", Properties.Resources.sInvalidIATPasswordCaption);
                        break;

                    case TransactionRequest.ETransaction.PasswordValid:
                        outTrans = new TransactionRequest();
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestResults;
                        SetStatusMessage("Requesting results");
                        env = new CEnvelope(outTrans);
                        env.SendMessage(ResultWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.ResultsReady:
                        SetStatusMessage("Retrieving results");
                        HttpWebRequest request = WebRequest.CreateHttp(Properties.Resources.sRetrieveResultsURL);
                        String requestBody = String.Format("{{ \"clientId\" : {0}, \"testName\" : \"{1}\", \"authToken\" : \"{2}\" }}", trans.LongValues["ClientId"], IATName, trans.StringValues["AuthToken"]);
                        request.ContentType = "text/json";
                        request.ContentLength = requestBody.Length;
                        request.Method = "POST";
                        Stream s = request.GetRequestStream();
                        s.Write(Encoding.UTF8.GetBytes(requestBody), 0, Encoding.UTF8.GetBytes(requestBody).Length);
                        HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                        XDocument xDoc = XDocument.Load(response.GetResponseStream());
                        Descriptor = new ResultSetDescriptor();
                        Descriptor.Load(xDoc.Root.Element("Descriptor"));
                        IATResults = Descriptor.CreateResultData();
                        foreach (var rsElem in xDoc.Root.Elements("ResultSet"))
                        {
                            CResultSet rs = new CResultSet(Descriptor);
                            rs.Load(rsElem);
                            IResultSet irs = rs.GenerateResultSet(DataKey);
                            if (irs != null)
                                IATResults.AppendResultSet(rs.GenerateResultSet(DataKey));
                        }
                        Score();
                        TransactionComplete.Set();
                        break;
                }
            }
            catch (Exception ex)
            {
                CReportableException reportable = new CReportableException(ex.Message, ex);
                IATConfigMainForm.ShowErrorReport("Error processing server transmission", reportable);
                TransactionFailed.Set();
            }
        }

        private void KeyPairReceived(INamedXmlSerializable obj)
        {
            try
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
                    TransactionFailed.Set();
                }
                TransactionRequest outTrans = new TransactionRequest();
                outTrans.Transaction = TransactionRequest.ETransaction.RequestDataPasswordVerification;
                outTrans.IATName = IATName;
                CEnvelope env = new CEnvelope(outTrans);
                env.SendMessage(ResultWebSocket, AbortToken);
            }
            catch (Exception ex)
            {
                CReportableException reportable = new CReportableException(ex.Message, ex);
                IATConfigMainForm.ShowErrorReport("Error processing server transmission", reportable);
                TransactionFailed.Set();
            }
        }

        private void OnDeploymentException(INamedXmlSerializable obj)
        {
            CServerException ex = (CServerException)obj;
            IATConfigMainForm.ShowErrorReport("Error retrieving result data", ex);
            TransactionFailed.Set();
        }

        public bool DoRetrieveData()
        {
            Descriptor = null;
            TransactionComplete.Reset();
            TransactionFailed.Reset();
            TransactionAborted.Reset();
            Action<EventHandler, IATConfigMainForm.EProgressBarUses> beginProgressBarUse = new Action<EventHandler, IATConfigMainForm.EProgressBarUses>(MainForm.BeginProgressBarUse);
            MainForm.Invoke(beginProgressBarUse, new EventHandler(Cancel), IATConfigMainForm.EProgressBarUses.DataRetrieval);
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnTransaction);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(KeyPairReceived);
            ResultWebSocket = new ClientWebSocket();
            SetStatusMessage("Connecting");
            Task connectTask = ResultWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), AbortToken);
            int nSecsWaited = 0;
            while ((nSecsWaited++ < 30) && !connectTask.IsCompleted && !bAborted)
                ((Func<Task>)(async () => await Task.Delay(1000)))().Wait();
            if (bAborted)
            {
                MainForm.BeginInvoke(new Action(MainForm.EndProgressBarUse));
                ResultWebSocket.Dispose();
                return false;
            }
            else if (nSecsWaited >= 30)
            {
                ResultWebSocket.Dispose();
                OperationFailed(Properties.Resources.sConnectionTimeoutMessage, Properties.Resources.sConnectionTimeoutCaption);
                return false;
            }
            else if (connectTask.IsFaulted)
            {
                ResultWebSocket.Dispose();
                OperationFailed(Properties.Resources.sConnectionFailedMessage, Properties.Resources.sConnectionFailedCaption);
                return false;
            }
            Task<WebSocketReceiveResult>.Run(() => ResultWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken)).ContinueWith((t) => ReceiveMessage(t));
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            CEnvelope env = new CEnvelope(trans);
            env.SendMessage(ResultWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionComplete, TransactionFailed, TransactionAborted });
            ResultWebSocket.Dispose();
            bool bResult = (nTrigger == 0);
            MainForm.BeginInvoke(new Action(() => MainForm.EndProgressBarUse()));
            return bResult;
        }

        private void StartMessageReceiver()
        {
            Task<WebSocketReceiveResult> receiveTask = ResultWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
            receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
        }

        private void ReceiveMessage(Task<WebSocketReceiveResult> t)
        {
            if (t.IsCanceled)
                return;
            if (t.IsFaulted)
                return;
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
                    Task<WebSocketReceiveResult> receiveTask = ResultWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
                    receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
                }
                catch (CXmlSerializationException ex)
                {
                    CReportableException reportable = new CReportableException(ex.Message, ex);
                    IATConfigMainForm.ShowErrorReport("Error retrieving result data", reportable);
                    TransactionFailed.Set();
                }
            }
        }

        private void WebSocket_Opened(object sender, EventArgs e)
        {
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            trans.IATName = IATName;
            CEnvelope env = new CEnvelope(trans);
            env.SendMessage(ResultWebSocket, AbortToken);
        }

        private void WebSocket_Closed(object sender, EventArgs e)
        {
            lock (transmissionLock)
            {
                TransactionFailed.Set();
            }
        }
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
