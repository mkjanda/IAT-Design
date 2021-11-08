using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Windows.Forms;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Net.WebSockets;
using System.Linq;

namespace IATClient
{
    class EMailConfirmation
    {
        private String ProductKey, VerificationCode, ActivationKey = String.Empty;
        private ClientWebSocket EMailUtilityWebSocket;
        private CancellationToken AbortToken = new CancellationToken();
        private ManualResetEvent OpComplete = new ManualResetEvent(false), OpFailed = new ManualResetEvent(false);
        private String email;
        private object transmissionLock = new object();
        private ArraySegment<byte> ReceiveBuffer = new ArraySegment<byte>(new byte[8192]);
        private CEnvelope IncomingMessage = null;
        public TransactionRequest FinalTransaction { get; private set; }
        
        private void ReportError(String caption, CReportableException rex)
        {
            try
            {
                WebClient uploader = new WebClient();
                CClientException clientEx = new CClientException(caption, rex);
                uploader.Headers.Add("Content-type: text/xml");
                if (Encoding.UTF8.GetString(uploader.UploadData(Properties.Resources.sErrorReportURL, clientEx.GetXmlBytes())) == "success")
                {
                    MessageBox.Show(String.Format(Properties.Resources.sErrorReportedMessage, LocalStorage.Activation[LocalStorage.Field.ProductKey]), Properties.Resources.sErrorReportedCaption);
                    return;
                }
            }
            catch (Exception e) { }
            ErrorReportDisplay f = new ErrorReportDisplay(rex);
            f.ShowDialog();
        }

        public enum EConfirmResult { failed, success };

        public EMailConfirmation()
        {
            ProductKey = LocalStorage.Activation[LocalStorage.Field.ProductKey];
            VerificationCode = LocalStorage.Activation[LocalStorage.Field.VerificationCode];
        }

        private void AbortTransaction(String errorMsg)
        {
        }

        public EConfirmResult ConfirmEMailVerification()
        {
            if (LocalStorage.Activation[LocalStorage.Field.UserEmail] == null)
                return EConfirmResult.failed;
            OpComplete.Reset();
            OpFailed.Reset();
            EMailUtilityWebSocket = new ClientWebSocket();
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(Confirmation_OnTransaction);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(OnHandshake);
            Task connectTask = EMailUtilityWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), AbortToken);
            int nSecsWaited = 0;
            while ((nSecsWaited++ < 30) && !connectTask.IsCompleted)
                ((Func<Task>)(async () => await Task.Delay(1000)))().Wait();
            if ((nSecsWaited >= 30) || connectTask.IsFaulted)
            {
                MessageBox.Show(Properties.Resources.sConnectionTimeoutCaption, Properties.Resources.sConnectionTimeoutCaption);
                EMailUtilityWebSocket.Dispose();
                return EConfirmResult.failed;
            }
            StartMessageReceiver();
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            CEnvelope env = new CEnvelope(trans);
            env.SendMessage(EMailUtilityWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { OpComplete, OpFailed} );
            EMailUtilityWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing on email confirmation check complete", AbortToken);
            if (nTrigger == 0) {
                LocalStorage.Activation[LocalStorage.Field.ActivationKey] = ActivationKey;
                LocalStorage.Activation[LocalStorage.Field.UserEmail] = email;
                return EConfirmResult.success;
            }
            else
                return EConfirmResult.failed;
        }

        private void StartMessageReceiver()
        {
            Task<WebSocketReceiveResult> receiveTask = EMailUtilityWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
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
                    lock (transmissionLock)
                    {
                        WebSocketReceiveResult receipt = t.Result;
                        try
                        {
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
                            CReportableException rex = new CReportableException(ex.Message, ex);
                            ReportError("Error receiving server transmission", rex);
                            OpFailed.Set();
                        }
                    }
                }
                Task<WebSocketReceiveResult> receiveTask = EMailUtilityWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
                receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
            }
            catch (Exception ex) {
                OpFailed.Set();
            }
        }
        

        public void ResendEMailVerification(String email)
        {
            OpComplete.Reset();
            OpFailed.Reset();
            CEnvelope.ClearMessageMap();
            this.email = email;
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(ResendConfirmationEMail_OnTransaction);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(OnHandshake);
            EMailUtilityWebSocket = new ClientWebSocket();
            Task connectTask = EMailUtilityWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), AbortToken);
            int nSecsWaited = 0;
            while ((nSecsWaited++ < 30) && !connectTask.IsCompleted)
                ((Func<Task>)(async () => await Task.Delay(1000)))().Wait();
            if ((nSecsWaited >= 30) || connectTask.IsFaulted)
            {
                MessageBox.Show(Properties.Resources.sConnectionTimeoutMessage, Properties.Resources.sConnectionTimeoutMessage);
                EMailUtilityWebSocket.Dispose();
                FinalTransaction = null;
            }
            StartMessageReceiver();
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            trans.StringValues["email"] = email;
            CEnvelope env = new CEnvelope(trans);
            env.SendMessage(EMailUtilityWebSocket, AbortToken);
            WaitHandle.WaitAny(new WaitHandle[] { OpComplete, OpFailed });
            EMailUtilityWebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Resend email request complete", AbortToken);
        }


        private void Confirmation_OnTransaction(INamedXmlSerializable transaction)
        {
            TransactionRequest inTrans = (TransactionRequest)transaction;
            switch (inTrans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    TransactionRequest outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestEMailVerification;
                    outTrans.StringValues["VerificationCode"] = LocalStorage.Activation[LocalStorage.Field.VerificationCode];
                    CEnvelope env = new CEnvelope(outTrans);
                    env.SendMessage(EMailUtilityWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.TransactionSuccess:
                    ActivationKey = inTrans.StringValues["ActivationKey"];
                    OpComplete.Set();
                    break;

                case TransactionRequest.ETransaction.TransactionFail:
                    OpFailed.Set();
                    break;

                case TransactionRequest.ETransaction.NoSuchClient:
                    OpFailed.Set();
                    break;

                default:
                    throw new CUnexpectedServerMessage("Unexpected message from server while confirming email activation", transaction);
                    break;
            }
        }

        private void ResendConfirmationEMail_OnTransaction(INamedXmlSerializable transaction)
        {
            TransactionRequest inTrans = (TransactionRequest)transaction;
            switch (inTrans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    TransactionRequest outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestNewVerificationEMail;
                    outTrans.StringValues["UserEmail"] = email;
                    CEnvelope env = new CEnvelope(outTrans);
                    env.SendMessage(EMailUtilityWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.TransactionSuccess:
                    FinalTransaction = inTrans;
                    OpComplete.Set();
                    break;

                case TransactionRequest.ETransaction.TransactionFail:
                    FinalTransaction = inTrans;
                    OpComplete.Set();
                    break;

                case TransactionRequest.ETransaction.EMailAlreadyVerified:
                    FinalTransaction = inTrans;
                    OpComplete.Set();
                    break;
            }
        }

        private void OnHandshake(INamedXmlSerializable inHand)
        {
            HandShake outHand = HandShake.CreateResponse((HandShake)inHand);
            CEnvelope env = new CEnvelope(outHand);
            env.SendMessage(EMailUtilityWebSocket, AbortToken);
        }


        /*
        private void 
            try
            {
                MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.Activation);
                MySOAP.EstablishEncryption(Properties.Resources.sActivationServer);
                TransactionRequest trans = MySOAP.ShakeHands(Properties.Resources.sActivationServer, String.Empty);
                if (trans.Transaction != TransactionRequest.ETransaction.RequestTransmission)
                    return EConfirmResult.handshakeError;
                trans = new TransactionRequest(TransactionRequest.ETransaction.RequestEMailVerification, IATConfigMainForm.ServerPassword, String.Empty, Properties.Resources.sActivationServer);
                trans.UserKey = UserKey;
                TransactionRequest returnTransaction = new TransactionRequest();
                MySOAP.CallSOAP(Properties.Resources.sActivationServer, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestEMailVerification, trans, returnTransaction);
                if (returnTransaction.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
                    return EConfirmResult.verificationError;
                _ActivationKey = returnTransaction.StringValue;
                return EConfirmResult.success;
            }
            catch (CXmlSerializationException ex)
            {
                ErrorReportDisplay errorDisplay = new ErrorReportDisplay("Error Verifying EMail Confirmation", ex);
                errorDisplay.Show();
                return EConfirmResult.verificationError;
            }
            finally
            {
                MySOAP.EndTransaction();
            }
        }
         * */
    }
}
