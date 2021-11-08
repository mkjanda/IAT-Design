using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Linq;

namespace IATClient
{
    class CItemSlideRetriever
    {
        private String IATName, IATPassword;
        private ClientWebSocket ItemSlideWebSocket;
        private CancellationToken AbortToken = new CancellationToken();
        private Func<String, byte[], bool> OnFileReceived;
        private Manifest _SlideManifest = null;
        private ManualResetEvent TransactionComplete = new ManualResetEvent(false), TransactionFailed = new ManualResetEvent(false);
        private CPartiallyEncryptedRSAKey DataKey;
        private object PacketQueueSyncObject = new object();
        private Dictionary<int, CPacket> PacketMap = null;
        private String itemSlideDownloadKey;
        private long clientID;
        private object transmissionLock = new object();
        private ArraySegment<byte> ReceiveBuffer = new ArraySegment<byte>(new byte[8192]);
        private CEnvelope IncomingMessage = null;
        public ManualResetEvent AbortEvent = null;
        private bool bAbort = false;

        public Manifest SlideManifest
        {
            get
            {
                return _SlideManifest;
            }
        }

        private IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            }
        }

        private void IncrementProgress(int n)
        {
            Action<int> incProgress = new Action<int>(MainForm.ProgressIncrement);
            MainForm.Invoke(incProgress, n);
        }

        private void SetStatusMessage(String msg)
        {
            MainForm.Invoke(new Action<String>(MainForm.SetStatusMessage), msg);
        }

        private void ResetProgressBar()
        {
            MainForm.Invoke(new Action(MainForm.ResetProgress));
        }

        private void SetProgressBarRange(int min, int max)
        {
            MainForm.Invoke(new Action<int, int>(MainForm.SetProgressRange), min, max);
        }

        private void OperationFailed(String msg, String caption)
        {
            MainForm.Invoke(new Action<String, String>(MainForm.OperationFailed), msg, caption);
        }

        private void EndProgressBarUse()
        {
            MainForm.Invoke(new Action(MainForm.EndProgressBarUse));
        }


        public void Abort(object sender, EventArgs e)
        {
            try
            {
                bAbort = true;
                TransactionFailed.Set();
            }
            catch (Exception ex) { }
        }

        public CItemSlideRetriever(String iatName, String dataPassword, Func<String, byte[], bool> onSlideReceived)
        {
            IATName = iatName;
            IATPassword = dataPassword;
            OnFileReceived = new Func<String, byte[], bool>(onSlideReceived);
        }

        private void ShakeHands(INamedXmlSerializable obj)
        {
            HandShake hs = (HandShake)obj;
            CEnvelope env = new CEnvelope(HandShake.CreateResponse(hs));
            env.SendMessage(ItemSlideWebSocket, AbortToken);
        }

        private void OnTransaction(INamedXmlSerializable obj)
        {
            try
            {
                TransactionRequest trans = (TransactionRequest)obj, outTrans;
                CEnvelope env;
                switch (trans.Transaction)
                {
                    case TransactionRequest.ETransaction.RequestTransmission:
                        outTrans = new TransactionRequest();
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                        outTrans.IATName = IATName;
                        env = new CEnvelope(outTrans);
                        env.SendMessage(ItemSlideWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.VerifyPassword:
                        try
                        {
                            RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
                            rsaCrypt.ImportParameters(DataKey.GetRSAParameters());
                            outTrans = new TransactionRequest();
                            outTrans.Transaction = TransactionRequest.ETransaction.VerifyPassword;
                            outTrans.StringValues["DecryptedTestString"] = Convert.ToBase64String(rsaCrypt.Decrypt(Convert.FromBase64String(trans.StringValues["EncryptedTestString"]), false));
                            env = new CEnvelope(outTrans);
                            env.SendMessage(ItemSlideWebSocket, AbortToken);
                        }
                        catch (Exception ex)
                        {
                            OperationFailed("The password you entered is incorrect.", "Incorrect Password");
                        }
                        break;

                    case TransactionRequest.ETransaction.PasswordInvalid:
                        OperationFailed("The password you entered is incorrect.", "Incorrect Password");
                        break;

                    case TransactionRequest.ETransaction.PasswordValid:
                        ResetProgressBar();
                        SetStatusMessage("Retrieving item slides");
                        outTrans = new TransactionRequest();
                        outTrans.Transaction = TransactionRequest.ETransaction.RequestItemSlideManifest;
                        env = new CEnvelope(outTrans);
                        env.SendMessage(ItemSlideWebSocket, AbortToken);
                        break;

                    case TransactionRequest.ETransaction.ItemSlideDownloadReady:
                        ResetProgressBar();
                        SetStatusMessage("Downloading Item Slides");
                        clientID = trans.ClientID;
                        itemSlideDownloadKey = trans.StringValues["DownloadKey"];
                        TransactionComplete.Set();
                        break;
                }
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error processing server transmission", ex));
                TransactionFailed.Set();
            }
        }

        private void KeyPairReceived(INamedXmlSerializable obj)
        {
            try
            {
                CRSAKeyPair keyPair = (CRSAKeyPair)obj;
                DataKey = keyPair.DataKey;
                DataKey.DecryptKey(IATPassword);
                TransactionRequest outTrans = new TransactionRequest();
                outTrans.Transaction = TransactionRequest.ETransaction.RequestDataPasswordVerification;
                CEnvelope env = new CEnvelope(outTrans);
                env.SendMessage(ItemSlideWebSocket, AbortToken);
            }
            catch (Exception ex)
            {
                OperationFailed("The password you entered is incorrect.", "Incorrect Password");
            }
        }

        private void ManifestReceived(INamedXmlSerializable obj)
        {
            _SlideManifest = (Manifest)obj;
            SetProgressBarRange(0, (int)((_SlideManifest.TotalSize / CPacket.PacketLength) + 1));
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestItemSlides;
            PacketMap = new Dictionary<int, CPacket>();
            CEnvelope env = new CEnvelope(trans);
            env.SendMessage(ItemSlideWebSocket, AbortToken);
        }

        public void DownloadItemSlides()
        {
            try
            {
                WebClient wClient = new WebClient();
                SetProgressBarRange(0, 100);
                Stream s = wClient.OpenRead(String.Format("{0}?DownloadKey={1}&ClientID={2}&IATName={3}", Properties.Resources.sItemSlideURL, itemSlideDownloadKey, clientID, IATName));
                for (int ctr = 0; ctr < _SlideManifest.NumEntities; ctr++)
                {
                    FileEntity fe = _SlideManifest[ctr];
                    byte[] slideData = new byte[fe.Size];
                    int slideBytesRead = 0;
                    while (slideBytesRead < fe.Size)
                    {
                        int nBytesRead = s.Read(slideData, slideBytesRead, (int)(fe.Size - slideBytesRead));
                        if (nBytesRead == 0)
                            throw new EndOfStreamException("Unexpected end of stream while downloading item slides");
                        slideBytesRead += nBytesRead;
                        if (slideBytesRead == fe.Size)
                        {
                            if (!OnFileReceived.Invoke(fe.Name, slideData))
                                return;
                            IncrementProgress(1);
                        }
                    }
                }
                EndProgressBarUse();
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error retrieving item slides", ex));
                AbortEvent.Set();
            }
        }

        private void OnDeploymentException(INamedXmlSerializable obj)
        {
            ErrorReporter.ReportError((CServerException)obj);
            TransactionFailed.Set();
        }

        public void RetrieveItemSlides(ManualResetEvent abortEvent)
        {
            AbortEvent = abortEvent;
            PacketMap = null;
            bAbort = false;
            TransactionComplete.Reset();
            TransactionFailed.Reset();
            MainForm.Invoke(new Action<EventHandler, IATConfigMainForm.EProgressBarUses>(MainForm.BeginProgressBarUse), new EventHandler(Abort), IATConfigMainForm.EProgressBarUses.ItemSlideRetrieval);
            SetStatusMessage("Connecting");
            ItemSlideWebSocket = new ClientWebSocket();
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnTransaction);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Manifest] = new Action<INamedXmlSerializable>(ManifestReceived);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(KeyPairReceived);
            bool connectionMade = false;
            try
            {
                if (!ItemSlideWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), AbortToken).ContinueWith(t =>
                {
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
                            if (webException != null)
                            {
                                HttpStatusCode code = (webException.Response as HttpWebResponse).StatusCode;
                                ErrorReporter.ReportError(new CReportableException(String.Format("Status {0} while connecting for item slide retrieval", code.ToString()), webException));
                                if ((code == HttpStatusCode.BadGateway) || (code == HttpStatusCode.InternalServerError) || (code == HttpStatusCode.BadRequest))
                                    OperationFailed(Properties.Resources.sServerDown, Properties.Resources.sServerDownCaption);
                                else
                                    OperationFailed(Properties.Resources.sConnectionError, Properties.Resources.sConnectionErrorCaption);
                            }
                            else
                            {
                                ErrorReporter.ReportError(new CReportableException("Error connecting for item slide retrieval", t.Exception.InnerException));
                            }
                        }
                    }
                }).Wait(15000))
                {
                    OperationFailed(Properties.Resources.sConnectionTimeoutMessage, Properties.Resources.sConnectionTimeoutCaption);
                    ItemSlideWebSocket.Dispose();
                    return;
                }
            }
            catch (WebException webException)
            {
                HttpStatusCode code = (webException.Response as HttpWebResponse).StatusCode;
                if ((code == HttpStatusCode.BadGateway) || (code == HttpStatusCode.InternalServerError) || (code == HttpStatusCode.BadRequest))
                    OperationFailed(Properties.Resources.sServerDown, Properties.Resources.sServerDownCaption);
                else
                    OperationFailed(Properties.Resources.sConnectionError, Properties.Resources.sConnectionErrorCaption);
                ErrorReporter.ReportError(new CReportableException(String.Format("Status {0} while connecting for item slide retrieval", code.ToString()), webException));
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error connecting for item slide retrieval", ex));
            }
            if (!connectionMade)
                return;
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            CEnvelope env = new CEnvelope(trans);
            env.SendMessage(ItemSlideWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionComplete, TransactionFailed });
            if (nTrigger == 1)
                AbortEvent.Set();
            else 
                DownloadItemSlides();
        }

        private void StartMessageReceiver()
        {
            Task<WebSocketReceiveResult> receiveTask = ItemSlideWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
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
                lock (transmissionLock)
                {
                    try
                    {
                        WebSocketReceiveResult receipt = t.Result;
                        if (receipt.MessageType == WebSocketMessageType.Close)
                            ItemSlideWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", new CancellationToken()).ContinueWith((t) =>
                            {
                                if (t.IsCompleted)
                                    ItemSlideWebSocket.Dispose();
                            });
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
                    catch (CXmlSerializationException ex)
                    {
                        ErrorReporter.ReportError(new CReportableException("Error receiving messsage from server during item slide retrieval", ex));
                        TransactionFailed.Set();
                    }
                }
                if ((ItemSlideWebSocket.State == WebSocketState.Open) || (ItemSlideWebSocket.State == WebSocketState.CloseSent))
                {
                    Task<WebSocketReceiveResult> receiveTask = ItemSlideWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
                    receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
                }
            }
            catch (Exception ex) { }
        }
    }
}
