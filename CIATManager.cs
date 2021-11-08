using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using Microsoft.Win32;
using System.Security.Cryptography;
using System.IO;
using System.Xml;
using System.Windows.Forms;
using System.Threading.Tasks;
using System.Net.WebSockets;

namespace IATClient
{
    class CIATManager
    {
        private enum ETransactionStage { unset, verifyingIATExistence, verifyingPassword };
        private object lockObject = new object();
        private IATConfigMainForm MainForm;
        private Dictionary<String, CResultData> ResultDataMap = new Dictionary<String, CResultData>();
        private Dictionary<String, CItemSlideContainer> ItemSlideMap = new Dictionary<String, CItemSlideContainer>();
        private ManualResetEvent TransactionCompleteEvent = new ManualResetEvent(false), TransactionFailedEvent = new ManualResetEvent(false);
        private CancellationToken AbortToken = new CancellationToken();
        private String CurrIATName, CurrIATPassword;
        private CPartiallyEncryptedRSAKey EncryptionKey;
        private ClientWebSocket IATManagerWebSocket;
        private object transmissionLock = new object();
        private CEnvelope IncomingMessage = null;
        private ArraySegment<byte> ReceiveBuffer = new ArraySegment<byte>(new byte[8192]);

        public CResultData GetResultData(String iatname)
        {
            if (!ResultDataMap.Keys.Contains(iatname))
                return null;
            return ResultDataMap[iatname];
        }

        public CItemSlideContainer GetItemSlides(String iatName)
        {
            if (!ItemSlideMap.Keys.Contains(iatName))
                return null;
            return ItemSlideMap[iatName];
        }

        public void ClearData(String selectedIAT)
        {
            if (ResultDataMap.Keys.Contains(selectedIAT))
                ResultDataMap.Remove(selectedIAT);
            if (ItemSlideMap.Keys.Contains(selectedIAT))
            {
                ItemSlideMap[selectedIAT].Dispose();
                ItemSlideMap.Remove(selectedIAT);
            }
        }
        private CServerReport _ServerReport;

        public CServerReport ServerReport
        {
            get
            {
                lock (lockObject)
                    return _ServerReport;
            }
        }


        public CIATManager()
        {
            MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
        }

        public bool Connect()
        {
            IATManagerWebSocket = new ClientWebSocket();
            CancellationToken connectCancellation = new CancellationToken();
            Task<bool> connection = IATManagerWebSocket.ConnectAsync(new Uri(Properties.Resources.sDataTransactionWebsocketURI), connectCancellation).ContinueWith((t) =>
            {
                if (!t.IsFaulted)
                {
                    StartMessageReceiver();
                    return true;
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
                    if (webException.Response == null)
                    {
                        MessageBox.Show(Properties.Resources.sServerDown, Properties.Resources.sServerDownCaption);
                        return false;
                    }
                    HttpStatusCode code = (webException.Response as HttpWebResponse).StatusCode;
                    if ((code == HttpStatusCode.BadGateway) || (code == HttpStatusCode.InternalServerError))
                        MessageBox.Show(Properties.Resources.sServerDown, Properties.Resources.sServerDownCaption);
                    else
                        MessageBox.Show(Properties.Resources.sConnectionError, Properties.Resources.sConnectionErrorCaption);
                    return false;
                }
            });
            if (!connection.Wait(15000))
            {
                MessageBox.Show(Properties.Resources.sConnectionTimeoutMessage, Properties.Resources.sConnectionTimeoutCaption);
                IATManagerWebSocket.Dispose();
                return false;
            }
            return connection.Result;
        }

        private void ShakeHands(INamedXmlSerializable handshake)
        {
            HandShake hs = (HandShake)handshake;
            HandShake outHand = HandShake.CreateResponse(hs);
            CEnvelope env = new CEnvelope(outHand);
            env.SendMessage(IATManagerWebSocket, AbortToken);
        }

        private void OnDeploymentException(INamedXmlSerializable deploymentException)
        {
            CReportableException ex = (CServerException)deploymentException;
            IATConfigMainForm.ShowErrorReport(ex.Caption, ex);
            TransactionFailedEvent.Set();
        }

        private void HandshakeConfirmation(INamedXmlSerializable handshakeConfirmation)
        {
            TransactionRequest trans = (TransactionRequest)handshakeConfirmation;
            if (trans.Transaction == TransactionRequest.ETransaction.ClientFrozen)
            {
                Action<String, String> failed = new Action<String, String>(MainForm.OperationFailed);
                MainForm.BeginInvoke(failed, Properties.Resources.sClientFrozen, "Client Frozen");
            }
            else if (trans.Transaction == TransactionRequest.ETransaction.ClientDeleted)
            {
                Action<String, String> failed = new Action<String, String>(MainForm.OperationFailed);
                MainForm.BeginInvoke(failed, Properties.Resources.sClientDeleted, "Client Deleted");
            }
            else if (trans.Transaction == TransactionRequest.ETransaction.RequestTransmission)
            {
                TransactionRequest request = new TransactionRequest();
                request.Transaction = TransactionRequest.ETransaction.RequestServerReport;
                CEnvelope env = new CEnvelope(request);
                env.SendMessage(IATManagerWebSocket, AbortToken);
            }
        }

        private void ReceiveServerReport(INamedXmlSerializable serverReport)
        {
            _ServerReport = (CServerReport)serverReport;
            TransactionCompleteEvent.Set();
        }

        private void ReceiveUpdatedServerReport(INamedXmlSerializable updatedServerReport)
        {
            CServerReport sReport = (CServerReport)updatedServerReport;
            foreach (String iatName in sReport.IATReports.Keys)
                if (!ServerReport.IATReports.Keys.Contains(iatName))
                    ClearData(iatName);
            _ServerReport = sReport;
            TransactionCompleteEvent.Set();
        }

        public bool ItemSlidesDownloaded(String iatName)
        {
            return ItemSlideMap[iatName].ItemSlideDownloadComplete;
        }


        public bool RetrieveServerReport()
        {
            try
            {
                TransactionCompleteEvent.Reset();
                TransactionFailedEvent.Reset();
                CEnvelope.ClearMessageMap();
                CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
                CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(HandshakeConfirmation);
                CEnvelope.OnReceipt[CEnvelope.EMessageType.ServerReport] = new Action<INamedXmlSerializable>(ReceiveServerReport);
                CEnvelope.OnReceipt[CEnvelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
                CEnvelope env = new CEnvelope(new TransactionRequest(TransactionRequest.ETransaction.RequestConnection, Properties.Resources.sServerPassword, String.Empty));
                if (!Connect())
                    return false;
                env.SendMessage(IATManagerWebSocket, AbortToken);
                int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionCompleteEvent, TransactionFailedEvent });
                IATManagerWebSocket.Dispose();
                CEnvelope.Shutdown();
                return (nTrigger == 0);
            }
            catch (Exception ex)
            {
                CReportableException reportable = new CReportableException(ex.Message, ex);
                IATConfigMainForm.ShowErrorReport("An error occurred retrieving your list of IATs from the server", reportable);
                CEnvelope.Shutdown();
                return false;
            }
        }

        public bool UpdateServerReport()
        {
            TransactionCompleteEvent.Reset();
            TransactionFailedEvent.Reset();
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(HandshakeConfirmation);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.ServerReport] = new Action<INamedXmlSerializable>(ReceiveUpdatedServerReport);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            if (!Connect())
                return false;
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            CEnvelope env = new CEnvelope(trans);
            env.SendMessage(IATManagerWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionCompleteEvent, TransactionFailedEvent });
            CEnvelope.Shutdown();
            return (nTrigger == 0);
        }

        private void StartMessageReceiver()
        {
            Task<WebSocketReceiveResult> receiveTask = IATManagerWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
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
                if (t.Result.Count > 0)
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
                        catch (Exception ex)
                        {
                            CReportableException reportable = new CReportableException(ex.Message, ex);
                            IATConfigMainForm.ShowErrorReport("Error receiving transmission from the server", reportable);
                            TransactionFailedEvent.Set();
                        }
                    }
                }
                Task<WebSocketReceiveResult> receiveTask = IATManagerWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
                receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
            }
            catch (Exception ex) { }
        }



        public String GetIATPasswordFromRegistry(String iatName)
        {
            return LocalStorage.GetIATPassword(iatName);
        }

        public void DeleteIATPasswordFromRegistry(String iatName)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software");
            key = key.OpenSubKey("IATSoftware");
            key = key.OpenSubKey("IATClient");
            if (key.GetValue("IAT-" + iatName) != null)
                key.DeleteValue("IAT-" + iatName);
            return;
        }

        public bool RetrieveResults(String IATName, String Password)
        {
            ClearData(IATName);
            CResultData rd = new CResultData(Properties.Resources.sDataTransactionWebsocketURI, IATName, Password);
            if (rd.DoRetrieveData())
            {
                ResultDataMap[IATName] = rd;
                return true;
            }
            return false;
        }

        public void RetrieveItemSlides(String iatName, String password)
        {
            if (ResultDataMap[iatName] == null)
                return;
            if (ItemSlideMap.Keys.Contains(iatName))
                if (ItemSlideMap[iatName] != null)
                    ItemSlideMap[iatName].Dispose();
            ItemSlideMap[iatName] = new CItemSlideContainer(iatName, password, ResultDataMap[iatName].ResultDescriptor.ConfigFile);
            ItemSlideMap[iatName].StartRetrieval();
        }

        private void OnAdminKeyReceived(INamedXmlSerializable obj)
        {
            try
            {
                CRSAKeyPair keyPair = (CRSAKeyPair)obj;
                EncryptionKey = keyPair.AdminKey;
                EncryptionKey.DecryptKey(CurrIATPassword);
                TransactionRequest trans = new TransactionRequest();
                trans.Transaction = TransactionRequest.ETransaction.RequestAdminPasswordVerification;
                trans.IATName = CurrIATName;
                CEnvelope env = new CEnvelope(trans);
                env.SendMessage(IATManagerWebSocket, AbortToken);
            }
            catch (Exception ex)
            {
                MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), "The password you entered for this IAT is incorrect.", "Incorrect Password");
                TransactionFailedEvent.Set();
            }
        }

        public bool DeleteIATData(String IATName, String password)
        {
            CurrIATName = IATName;
            CurrIATPassword = password;
            TransactionCompleteEvent.Reset();
            TransactionFailedEvent.Reset();
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnDeleteIATDataTransaction);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(OnAdminKeyReceived);
            if (!Connect())
                return false;
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            CEnvelope env = new CEnvelope(trans);
            env.SendMessage(IATManagerWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionCompleteEvent, TransactionFailedEvent });
            CEnvelope.Shutdown();
            return (nTrigger == 0);
        }

        public void OnDeleteIATDataTransaction(INamedXmlSerializable obj)
        {
            TransactionRequest trans = (TransactionRequest)obj;
            TransactionRequest outTrans = null;
            CEnvelope env = null;
            switch (trans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    outTrans = new TransactionRequest(TransactionRequest.ETransaction.IATExists, IATConfigMainForm.ServerPassword, CurrIATName);
                    env = new CEnvelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.IATExists:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                    outTrans.IATName = CurrIATName;
                    env = new CEnvelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.VerifyPassword:
                    String resultStr = String.Empty;
                    try
                    {
                        RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
                        rsaCrypt.ImportParameters(EncryptionKey.GetRSAParameters());
                        resultStr = Convert.ToBase64String(rsaCrypt.Decrypt(Convert.FromBase64String(trans.StringValues["EncryptedTestString"]), false));
                    }
                    catch (Exception ex)
                    {
                        MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), "The password you entered for this IAT is incorrect.", "Incorrect Password");
                        TransactionFailedEvent.Set();
                    }
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.VerifyPassword;
                    outTrans.StringValues["DecryptedTestString"] = resultStr;
                    env = new CEnvelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.PasswordValid:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.DeleteIATData;
                    outTrans.IATName = CurrIATName;
                    env = new CEnvelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.PasswordInvalid:
                    MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), "The password you entered for this IAT is incorrect.", "Incorrect Password");
                    TransactionFailedEvent.Set();
                    break;


                case TransactionRequest.ETransaction.TransactionSuccess:
                    TransactionCompleteEvent.Set();
                    break;

                case TransactionRequest.ETransaction.TransactionFail:
                    MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), "The deletion of your IAT data failed for unknown reasons.", "Deletion failed");
                    TransactionFailedEvent.Set();
                    break;

            }
        }

        public bool DeleteIAT(String IATName, String password)
        {
            CurrIATName = IATName;
            CurrIATPassword = password;
            TransactionCompleteEvent.Reset();
            TransactionFailedEvent.Reset();
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnDeleteIATTransaction);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(OnAdminKeyReceived);
            if (!Connect())
                return false;
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            CEnvelope env = new CEnvelope(trans);
            env.SendMessage(IATManagerWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionCompleteEvent, TransactionFailedEvent });
            CEnvelope.Shutdown();
            return (nTrigger == 0);
        }

        public void OnDeleteIATTransaction(INamedXmlSerializable obj)
        {
            TransactionRequest trans = (TransactionRequest)obj;
            TransactionRequest outTrans = null;
            CEnvelope env = null;
            switch (trans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    outTrans = new TransactionRequest(TransactionRequest.ETransaction.IATExists, IATConfigMainForm.ServerPassword, CurrIATName);
                    env = new CEnvelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.IATExists:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                    outTrans.IATName = CurrIATName;
                    env = new CEnvelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.TestBeingDeployed:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.AbortDeployment;
                    outTrans.IATName = CurrIATName;
                    outTrans.LongValues["DeploymentId"] = trans.LongValues["DeploymentId"];
                    env = new CEnvelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.VerifyPassword:
                    String resultStr = String.Empty;
                    try
                    {
                        RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
                        rsaCrypt.ImportParameters(EncryptionKey.GetRSAParameters());
                        resultStr = Convert.ToBase64String(rsaCrypt.Decrypt(Convert.FromBase64String(trans.StringValues["EncryptedTestString"]), false));
                    }
                    catch (Exception ex)
                    {
                        MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), "The password you entered for this IAT is incorrect.", "Incorrect Password");
                        TransactionFailedEvent.Set();
                    }
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.VerifyPassword;
                    outTrans.StringValues["DecryptedTestString"] = resultStr;
                    env = new CEnvelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.PasswordValid:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.DeleteIAT;
                    outTrans.IATName = CurrIATName;
                    env = new CEnvelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.PasswordInvalid:
                    MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), "The password you entered for this IAT is incorrect.", "Incorrect Password");
                    TransactionFailedEvent.Set();
                    break;


                case TransactionRequest.ETransaction.TransactionSuccess:
                    TransactionCompleteEvent.Set();
                    break;

                case TransactionRequest.ETransaction.TransactionFail:
                    MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), "The deletion of your IAT failed for unknown reasons.", "Deletion failed");
                    TransactionFailedEvent.Set();
                    break;

                case TransactionRequest.ETransaction.DeploymentAbortFailed:
                    TransactionFailedEvent.Set();
                    break;

                case TransactionRequest.ETransaction.NoSuchIAT:
                    TransactionCompleteEvent.Set();
                    break;
            }
        }
    }
}
