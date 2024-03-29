﻿using IATClient.Messages;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.WebSockets;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    class CIATManager
    {
        private enum ETransactionStage { unset, verifyingIATExistence, verifyingPassword };
        private object lockObject = new object();
        private IATConfigMainForm MainForm;
        private Dictionary<String, ResultData.ResultData> ResultDataMap = new Dictionary<String, ResultData.ResultData>();
        private Dictionary<String, CItemSlideContainer> ItemSlideMap = new Dictionary<String, CItemSlideContainer>();
        private ManualResetEvent TransactionCompleteEvent = new ManualResetEvent(false), TransactionFailedEvent = new ManualResetEvent(false), SlideDownloadWaiting = new ManualResetEvent(true);
        private CancellationToken AbortToken = new CancellationToken();
        private String CurrIATName, CurrIATPassword;
        private PartiallyEncryptedRSAData EncryptionKey;
        private ClientWebSocket IATManagerWebSocket;
        private object transmissionLock = new object();
        private Messages.Envelope IncomingMessage = null;
        private ArraySegment<byte> ReceiveBuffer = new ArraySegment<byte>(new byte[8192]);

        public ResultData.ResultData GetResultData(String iatname)
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

        public CServerReport ServerReport { get; private set; }
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
            Envelope env = new Envelope(outHand);
            env.SendMessage(IATManagerWebSocket, AbortToken);
        }

        private void OnDeploymentException(INamedXmlSerializable deploymentException)
        {
            CReportableException ex = (CServerException)deploymentException;
            ErrorReporter.ReportError(ex);
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
                Envelope env = new Envelope(request);
                env.SendMessage(IATManagerWebSocket, AbortToken);
            }
        }

        private void ReceiveServerReport(INamedXmlSerializable serverReport)
        {
            ServerReport = (CServerReport)serverReport;
            TransactionCompleteEvent.Set();
        }

        private void ReceiveUpdatedServerReport(INamedXmlSerializable updatedServerReport)
        {
            CServerReport sReport = (CServerReport)updatedServerReport;
            foreach (String iatName in sReport.IATReports.Keys)
                if (!ServerReport.IATReports.Keys.Contains(iatName))
                    ClearData(iatName);
            ServerReport = sReport;
            TransactionCompleteEvent.Set();
        }


        public bool RetrieveServerReport()
        {
            try
            {
                TransactionCompleteEvent.Reset();
                TransactionFailedEvent.Reset();
                Messages.Envelope.ClearMessageMap();
                Messages.Envelope.OnReceipt[Messages.Envelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
                Messages.Envelope.OnReceipt[Messages.Envelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(HandshakeConfirmation);
                Messages.Envelope.OnReceipt[Messages.Envelope.EMessageType.ServerReport] = new Action<INamedXmlSerializable>(ReceiveServerReport);
                Messages.Envelope.OnReceipt[Messages.Envelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
                Messages.Envelope env = new Messages.Envelope(new TransactionRequest(TransactionRequest.ETransaction.RequestConnection));
                if (!Connect())
                    return false;
                env.SendMessage(IATManagerWebSocket, AbortToken);
                int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionCompleteEvent, TransactionFailedEvent });
                Messages.Envelope.Shutdown();
                return (nTrigger == 0);
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("An error occurred retrieving your list of IATs from the server", ex));
                Envelope.Shutdown();
                return false;
            }
        }

        private void StartMessageReceiver()
        {
            Task<WebSocketReceiveResult> receiveTask = IATManagerWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
            receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
        }

        private void ReceiveMessage(Task<WebSocketReceiveResult> t)
        {
            try
            {
                if (t.IsCanceled)
                    return;
                if (t.IsFaulted)
                    return;
                if (t.Result.Count > 0)
                {
                    lock (transmissionLock)
                    {
                        WebSocketReceiveResult receipt = t.Result;
                        if (receipt.MessageType == WebSocketMessageType.Close)
                            IATManagerWebSocket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closing", new CancellationToken()).ContinueWith((t) =>
                            {
                                if (t.IsCompleted)
                                    IATManagerWebSocket.Dispose();
                            });
                        try
                        {
                            if (receipt.EndOfMessage)
                            {
                                if (IncomingMessage == null)
                                    IncomingMessage = new Envelope();
                                if (IncomingMessage.QueueByteData(ReceiveBuffer.Array.Take(receipt.Count).ToArray(), true))
                                    IncomingMessage = null;
                            }
                            else
                            {
                                if (IncomingMessage == null)
                                    IncomingMessage = new Envelope();
                                IncomingMessage.QueueByteData(ReceiveBuffer.Array.Take(receipt.Count).ToArray(), false);
                            }
                        }
                        catch (Exception ex)
                        {
                            ErrorReporter.ReportError(new CReportableException("Error receiving transmission from the server", ex));
                            TransactionFailedEvent.Set();
                        }
                    }
                }
                if ((IATManagerWebSocket.State == WebSocketState.Open) || (IATManagerWebSocket.State == WebSocketState.CloseSent))
                {
                    Task<WebSocketReceiveResult> receiveTask = IATManagerWebSocket.ReceiveAsync(ReceiveBuffer, AbortToken);
                    receiveTask.ContinueWith(new Action<Task<WebSocketReceiveResult>>(ReceiveMessage), AbortToken);
                }
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
            ResultData.ResultData rd = new ResultData.ResultData(Properties.Resources.sDataTransactionWebsocketURI, IATName, Password);
            if (rd.DoRetrieveData())
            {
                ResultDataMap[IATName] = rd;
                return true;
            }
            return false;
        }

        public bool RetrieveItemSlides(String iatName, String password)
        {
            if (ResultDataMap[iatName] == null)
                return false;
            CurrIATName = iatName;
            CurrIATPassword = password;
            TransactionCompleteEvent.Reset();
            TransactionFailedEvent.Reset();
            Envelope.ClearMessageMap();
            Envelope.OnReceipt[Envelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            Envelope.OnReceipt[Envelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnItemSlidesTransaction);
            Envelope.OnReceipt[Envelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            Envelope.OnReceipt[Envelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(OnAdminKeyReceived);
            Envelope.OnReceipt[Envelope.EMessageType.Manifest] = new Action<INamedXmlSerializable>(OnManifest);
            if (!Connect())
                return false;
            var requestConnection = new TransactionRequest(TransactionRequest.ETransaction.RequestConnection);
            requestConnection.IATName = iatName;
            var env = new Envelope(requestConnection);
            env.SendMessage(IATManagerWebSocket, AbortToken);
            if (ItemSlideMap.Keys.Contains(iatName))
                if (ItemSlideMap[iatName] != null)
                    ItemSlideMap[iatName].Dispose();
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionFailedEvent, TransactionCompleteEvent }, 60000, true);
            return (nTrigger == 1);
        }


        private void OnItemSlidesTransaction(INamedXmlSerializable obj)
        {
            TransactionRequest trans = (TransactionRequest)obj;
            TransactionRequest outTrans = null;
            Envelope env = null;
            switch (trans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    outTrans = new TransactionRequest(TransactionRequest.ETransaction.IATExists, CurrIATName);
                    env = new Envelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.IATExists:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                    outTrans.IATName = CurrIATName;
                    env = new Envelope(outTrans);
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
                    env = new Envelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.PasswordValid:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestItemSlideManifest;
                    outTrans.IATName = CurrIATName;
                    env = new Envelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.PasswordInvalid:
                    MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), "The password you entered for this IAT is incorrect.", "Incorrect Password");
                    TransactionFailedEvent.Set();
                    break;


                case TransactionRequest.ETransaction.TransactionFail:
                    MainForm.BeginInvoke(new Action<String, String>(MainForm.OperationFailed), "The deletion of your IAT data failed for unknown reasons.", "Deletion failed");
                    TransactionFailedEvent.Set();
                    break;


                case TransactionRequest.ETransaction.ItemSlideDownloadReady:
                    
                    WebClient web = new WebClient();
                    var query = new NameValueCollection(3);
                    query.Add("IATName", CurrIATName);
                    query.Add("ClientID", ClientId.ToString());
                    query.Add("DownloadKey", trans.StringValues["DownloadKey"]);
                    web.QueryString = query;
                    byte[] itemSlideData = web.DownloadData(Properties.Resources.sItemSlideDownloadURL);
                    var receipt = new MemoryStream(itemSlideData);
                    var slideData = new List<byte[]>();
                    var fileList = ItemSlideManifest.Contents.Where(fe => fe.FileEntityType == FileEntity.EFileEntityType.File).Cast<ManifestFile>().Where(mf => mf.ResourceType == ManifestFile.EResourceType.itemSlide).ToList();
                    foreach (var file in fileList)
                    {
                        byte[] sd = new byte[file.Size];
                        receipt.Read(sd, 0, (int)file.Size);
                        slideData.Add(sd);
                    }
                    ItemSlideMap[CurrIATName] = new CItemSlideContainer(ResultDataMap[CurrIATName], slideData, fileList);
                    ItemSlideMap[CurrIATName].ProcessSlides();
                    TransactionCompleteEvent.Set();
                    break;


            }
        }

        public long ClientId { get; private set; }
        public Manifest ItemSlideManifest { get; private set; }

        private void OnManifest(INamedXmlSerializable message)
        {
            var manifest = message as Manifest;
            ItemSlideManifest = manifest;
            var trans = new TransactionRequest(TransactionRequest.ETransaction.RequestItemSlides);
            ClientId = manifest.ClientId;
            trans.IATName = CurrIATName;
            new Envelope(trans).SendMessage(IATManagerWebSocket, AbortToken);
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
                Envelope env = new Envelope(trans);
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
            Envelope.ClearMessageMap();
            Envelope.OnReceipt[Envelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            Envelope.OnReceipt[Envelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnDeleteIATDataTransaction);
            Envelope.OnReceipt[Envelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            Envelope.OnReceipt[Envelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(OnAdminKeyReceived);
            if (!Connect())
                return false;
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            Envelope env = new Envelope(trans);
            env.SendMessage(IATManagerWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionCompleteEvent, TransactionFailedEvent });
            Envelope.Shutdown();
            return (nTrigger == 0);
        }

        public void OnDeleteIATDataTransaction(INamedXmlSerializable obj)
        {
            TransactionRequest trans = (TransactionRequest)obj;
            TransactionRequest outTrans = null;
            Envelope env = null;
            switch (trans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    outTrans = new TransactionRequest(TransactionRequest.ETransaction.IATExists, CurrIATName);
                    env = new Envelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.IATExists:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                    outTrans.IATName = CurrIATName;
                    env = new Envelope(outTrans);
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
                    env = new Envelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.PasswordValid:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.DeleteIATData;
                    outTrans.IATName = CurrIATName;
                    env = new Envelope(outTrans);
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
            Envelope.ClearMessageMap();
            Envelope.OnReceipt[Envelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            Envelope.OnReceipt[Envelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnDeleteIATTransaction);
            Envelope.OnReceipt[Envelope.EMessageType.ServerException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            Envelope.OnReceipt[Envelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(OnAdminKeyReceived);
            if (!Connect())
                return false;
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            Envelope env = new Envelope(trans);
            env.SendMessage(IATManagerWebSocket, AbortToken);
            int nTrigger = WaitHandle.WaitAny(new WaitHandle[] { TransactionCompleteEvent, TransactionFailedEvent });
            Envelope.Shutdown();
            return (nTrigger == 0);
        }

        public void OnDeleteIATTransaction(INamedXmlSerializable obj)
        {
            TransactionRequest trans = (TransactionRequest)obj;
            TransactionRequest outTrans = null;
            Envelope env = null;
            switch (trans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    outTrans = new TransactionRequest(TransactionRequest.ETransaction.IATExists, CurrIATName);
                    env = new Envelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.IATExists:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                    outTrans.IATName = CurrIATName;
                    env = new Envelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.TestBeingDeployed:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.AbortDeployment;
                    outTrans.IATName = CurrIATName;
                    outTrans.LongValues["DeploymentId"] = trans.LongValues["DeploymentId"];
                    env = new Envelope(outTrans);
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
                    env = new Envelope(outTrans);
                    env.SendMessage(IATManagerWebSocket, AbortToken);
                    break;

                case TransactionRequest.ETransaction.PasswordValid:
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.DeleteIAT;
                    outTrans.IATName = CurrIATName;
                    env = new Envelope(outTrans);
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
