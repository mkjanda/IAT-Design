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
using System.Threading;
using WebSocket4Net;

namespace IATClient
{
    class CIATManager
    {
        public enum ETransactionResult { failed, success, exception };
        private enum ETransactionStage { unset, verifyingIATExistence, verifyingPassword };
        private enum ETransaction { deleteIAT, retrieveServerReport, deleteIATData };
        private ETransaction TransactionType;
        private ETransactionStage TransactionStage = ETransactionStage.unset;
        private object lockObject = new object(), soapLockObject = new object();
        private bool _Aborted = false;
        private IATConfigMainForm MainForm;
        private CDeploymentException serverError;
        private Dictionary<String, CResultData> ResultDataMap = new Dictionary<String, CResultData>();
        private WebSocket websocket;
        private ManualResetEvent transactionCompleteEvent = new ManualResetEvent(false);
        private ETransactionResult currentTransResult;
        private String CurrIATName, CurrIATPassword;
        private String _ErrorMessage;
        private Exception _TransmissionException;

        class PasswordVerifier
        {
            private WebSocket websocket;
            private String password, iatName;
            private ManualResetEvent transCompleteEvent = new ManualResetEvent(false);
            private CPartiallyEncryptedRSAKey key;
            private CPartiallyEncryptedRSAKey.EKeyType keyType;
            private bool PasswordVerified = false;

            public PasswordVerifier(WebSocket websocket, String IATName, String Password, CPartiallyEncryptedRSAKey.EKeyType keyType)
            {
                this.websocket = websocket;
                this.iatName = IATName;
                this.password = Password;
                this.keyType = keyType;
            }

            public bool Verify()
            {
                transCompleteEvent.Reset();
                Dictionary<CEnvelope.EMessageType, Action<INamedXmlSerializable>> messageMap = CEnvelope.GetMessageMap();
                CEnvelope.ClearMessageMap();
                CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnTransaction);
                CEnvelope.OnReceipt[CEnvelope.EMessageType.RSAKeyPair] = new Action<INamedXmlSerializable>(OnKeyReceived);
                TransactionRequest trans = new TransactionRequest();
                trans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                trans.IATName = iatName;
                CEnvelope.SendMessage(trans, websocket);
                transCompleteEvent.WaitOne();
                CEnvelope.LoadMessageMap(messageMap);
                return PasswordVerified;
            }

            private void OnKeyReceived(INamedXmlSerializable obj)
            {
                CRSAKeyPair keyPair = (CRSAKeyPair)obj;
                if (keyType == CPartiallyEncryptedRSAKey.EKeyType.Data)
                    key = keyPair.DataKey;
                else
                    key = keyPair.AdminKey;
                key.DecryptKey(password);
                TransactionRequest trans = new TransactionRequest();
                if (keyType == CPartiallyEncryptedRSAKey.EKeyType.Admin)
                    trans.Transaction = TransactionRequest.ETransaction.RequestAdminPasswordVerification;
                else
                    trans.Transaction = TransactionRequest.ETransaction.RequestDataPasswordVerification;
                trans.StringValue = keyType.ToString();
                CEnvelope.SendMessage(trans, websocket);
            }

            private void OnTransaction(INamedXmlSerializable obj)
            {
                TransactionRequest trans = (TransactionRequest)obj;
                TransactionRequest outTrans = null;
                try
                {
                    switch (trans.Transaction)
                    {
                        case TransactionRequest.ETransaction.VerifyPassword:
                            String resultStr = String.Empty;
                            try
                            {
                                RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
                                rsaCrypt.ImportParameters(key.GetRSAParameters());
                                resultStr = Convert.ToBase64String(rsaCrypt.Decrypt(Convert.FromBase64String(trans.StringValue), false));
                            }
                            catch (Exception ex) {
                                PasswordVerified = false;
                                transCompleteEvent.Set();
                                return;
                            }
                            outTrans = new TransactionRequest();
                            outTrans.Transaction = TransactionRequest.ETransaction.VerifyPassword;
                            outTrans.StringValue = resultStr;
                            CEnvelope.SendMessage(outTrans, websocket);
                            break;

                        case TransactionRequest.ETransaction.PasswordValid:
                            PasswordVerified = true;
                            transCompleteEvent.Set();
                            break;

                        case TransactionRequest.ETransaction.PasswordInvalid:
                            PasswordVerified = false;
                            transCompleteEvent.Set();
                            break;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.StackTrace, ex.Message);
                }
            }
        }

        public CDeploymentException ServerError
        {
            get
            {
                return serverError;
            }
        }


        public Exception TransmissionException
        {
            get
            {
                return _TransmissionException;
            }
        }

        public CResultData GetResultData(String iatname)
        {
            return ResultDataMap[iatname];
        }

        private bool Aborted
        {
            get
            {
                lock (lockObject)
                {
                    return _Aborted;
                }
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
            set
            {
                lock (lockObject)
                {
                    _ServerReport = value;
                    if (value == null)
                        ResultDataMap.Clear();
                    else foreach (String iatname in value.IATReports.Keys)
                            ResultDataMap[iatname] = null;
                }
            }
        }


        public CIATManager()
        {
            MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];

        }

        private void WebSocket_Closed(object sender, EventArgs e)
        {
            transactionCompleteEvent.Set();
            websocket.Dispose();
        }

        private void ShakeHands(INamedXmlSerializable handshake)
        {
            HandShake hs = (HandShake)handshake;
            HandShake outHand = HandShake.CreateResponse(hs);
            CEnvelope.SendMessage(outHand, websocket);
        }

        private void OnDeploymentException(INamedXmlSerializable deploymentException)
        {
            ErrorReportDisplay f = new ErrorReportDisplay((CDeploymentException)deploymentException);
            MainForm.Invoke((Action<Form>)MainForm.ShowForm, f);
            currentTransResult = ETransactionResult.failed;
            websocket.Close();
        }

        private void HandshakeConfirmation(INamedXmlSerializable handshakeConfirmation)
        {
            TransactionRequest trans = (TransactionRequest)handshakeConfirmation;
            if (trans.Transaction == TransactionRequest.ETransaction.ClientFrozen)
            {
                Action<String, String> failed = new Action<String, String>(MainForm.OperationFailed);
                MainForm.BeginInvoke(failed, Properties.Resources.sClientFrozen, "Client Frozen");
                currentTransResult = ETransactionResult.failed;
                websocket.Close();
            }
            else if (trans.Transaction == TransactionRequest.ETransaction.ClientDeleted)
            {
                Action<String, String> failed = new Action<String, String>(MainForm.OperationFailed);
                MainForm.BeginInvoke(failed, Properties.Resources.sClientDeleted, "Client Deleted");
                currentTransResult = ETransactionResult.failed;
                websocket.Close();
            }
            else if (trans.Transaction == TransactionRequest.ETransaction.RequestTransmission)
            {
                TransactionRequest request = new TransactionRequest();
                request.Transaction = TransactionRequest.ETransaction.RequestServerReport;
                CEnvelope.SendMessage(request, websocket);
            }
        }

        private void ReceiveServerReport(INamedXmlSerializable serverReport)
        {
            _ServerReport= (CServerReport)serverReport;
            currentTransResult = ETransactionResult.success;
            transactionCompleteEvent.Set();
            websocket.Close();
        }

        private void WebSocket_Error(object sender, SuperSocket.ClientEngine.ErrorEventArgs e)
        {
            currentTransResult = ETransactionResult.exception;
            _TransmissionException = e.Exception;
            websocket.Close();
            transactionCompleteEvent.Set();
        }

        public ETransactionResult RetrieveServerReport()
        {
            TransactionType = ETransaction.retrieveServerReport;
            _ErrorMessage = String.Empty;
            _TransmissionException = null;
            _ServerReport = null;
            transactionCompleteEvent.Reset();
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(HandshakeConfirmation);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.ServerReport] = new Action<INamedXmlSerializable>(ReceiveServerReport);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.DeploymentException] = new Action<INamedXmlSerializable>(OnDeploymentException);
            websocket = new WebSocket(Properties.Resources.sDataTransactionWebsocketURI);
            websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(CEnvelope.MessageReceived);
            websocket.Opened += new EventHandler(WebSocket_Opened);
            websocket.Error += new EventHandler<SuperSocket.ClientEngine.ErrorEventArgs>(WebSocket_Error);
            websocket.Closed += new EventHandler(WebSocket_Closed);
            websocket.Open();
            transactionCompleteEvent.WaitOne();
            return currentTransResult;
        }

        public void WebSocket_Opened(object sender, EventArgs e)
        {
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestConnection;
            CEnvelope.SendMessage(trans, websocket);
        }

        public void SaveIATPasswordToRegistry(String IATName, String Password)
        {
            RegistryKey key = Registry.CurrentUser.OpenSubKey("Software");
            key = key.OpenSubKey("IATSoftware");
            key = key.OpenSubKey("IATClient");
            byte[] DESBytes, IVBytes;
            if (key.GetValue("Passkey") == null)
            {
                Random rand = new Random();
                DESBytes = new byte[8];
                rand.NextBytes(DESBytes);
                IVBytes = new byte[8];
                rand.NextBytes(IVBytes);
                MemoryStream memStream = new MemoryStream();
                memStream.Write(DESBytes, 0, 8);
                memStream.Write(IVBytes, 0, 8);
                String passKey = Convert.ToBase64String(memStream.ToArray());
                key.SetValue("Passkey", passKey);
            }
            else
            {
                MemoryStream memStream = new MemoryStream(Convert.FromBase64String((String)key.GetValue("Passkey")));
                DESBytes = new byte[8];
                memStream.Read(DESBytes, 0, 8);
                IVBytes = new byte[8];
                memStream.Read(IVBytes, 0, 8);
            }
            DESCryptoServiceProvider DESCrypt = new DESCryptoServiceProvider();
            ICryptoTransform desTrans = DESCrypt.CreateEncryptor(DESBytes, IVBytes);
            MemoryStream passStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(passStream, desTrans, CryptoStreamMode.Write);
            cStream.Write(System.Text.Encoding.UTF8.GetBytes(Password), 0, System.Text.Encoding.UTF8.GetByteCount(Password));
            cStream.FlushFinalBlock();
            key.SetValue("IAT-" + IATName, Convert.ToBase64String(passStream.ToArray()));
        }

        public String GetIATPasswordFromRegistry(String iatName)
        {
            return CRegistry.GetIATPassword(iatName);
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

        public CResultData RetrieveResults(String IATName, String Password)
        {
            CResultData rd = new CResultData(Properties.Resources.sDataTransactionWebsocketURI, IATName, Password);
            CResultData.ETransactionResult result = rd.DoRetrieveData();
            if (result == CResultData.ETransactionResult.exception)
                MessageBox.Show(rd.TransactionException.Message, "Data Retrieval Failed");
            if (result == CResultData.ETransactionResult.success)
            {
                ResultDataMap[IATName] = rd;
                return rd;
            }
            return null;
        }

        /*
        private ResultSetDescriptor RetrieveResultSetDescriptor(String IATName, String Password)
        {
            try
            {
                MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.DataRetrieval);
                if (!NegotiateConnection(IATName, Password))
                    return null;
                TransactionRequest outTrans = new TransactionRequest();
                MySOAP.BeginNewTransactionEvent("Retrieving result set descriptor");
                SetStatusBarMessage("Retrieving result set descriptor");
                outTrans.Transaction = TransactionRequest.ETransaction.RequestResultDescriptor;
                outTrans.IATName = IATName;
                TransactionRequest inTrans = new TransactionRequest();
                CallSOAP(MySOAP.ESoapAction.RequestResultDescriptor, outTrans, inTrans);
                if (inTrans.Transaction != TransactionRequest.ETransaction.RequestRetrievalReady)
                {
                    OperationFailed("An error was encountered while negotiating communication with the server.  If this problem persists, please contact us at admin@iatsoftware.net.", "Server Error");
                    return null;
                }
                int nVersion = inTrans.IntValue;
                CPacket packet;
                outTrans = new TransactionRequest();
                outTrans.Transaction = TransactionRequest.ETransaction.RequestPacket;
                outTrans.IATName = IATName;
                MemoryStream resultDescriptorStream = new MemoryStream();
                BinaryWriter bWriter = new BinaryWriter(resultDescriptorStream);
                do
                {
                    packet = new CPacket();
                    CallSOAP(MySOAP.ESoapAction.RequestPacket, outTrans, packet);
                    if (!packet.IsNullPacket)
                        bWriter.Write(packet.ByteData);
                } while (!packet.IsNullPacket);
                resultDescriptorStream.Seek(0, SeekOrigin.Begin);
                TextReader txtReader = new StreamReader(resultDescriptorStream, System.Text.Encoding.UTF8);
                XmlTextReader reader = new XmlTextReader(txtReader);
                ResultSetDescriptor Descriptor = new ResultSetDescriptor();
                Descriptor.ReadXml(reader);
                OperationSuccess();
                return Descriptor;
            }
            catch (TransactionAbortedException)
            {
                return null;
            }
            catch (CXmlSerializationException ex)
            {
                ErrorReportDisplay errorForm = new ErrorReportDisplay(ex.Message, ex);
                errorForm.ShowDialog();
                return null;
            }
            catch (Exception ex)
            {
                CXmlSerializationException ex2 = new CXmlSerializationException("Error Retrieving Test Data", ex.Message, ex);
                ErrorReportDisplay errorForm = new ErrorReportDisplay(ex2.Message, ex2);
                errorForm.ShowDialog();
                return null;
            }
            finally
            {
                MySOAP.EndTransaction();
            }
        }

        private List<CResultPacket> RetrieveResultData(String IATName, String Password, ResultSetDescriptor Descriptor)
        {
            try
            {
                MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.DataRetrieval);
                if (!NegotiateConnection(IATName, Password))
                    return null;
                SetStatusBarMessage("Retrieving Result Data");
                MySOAP.BeginNewTransactionEvent("Retrieving decryption key");
                TransactionRequest outTrans = new TransactionRequest();
                outTrans.Transaction = TransactionRequest.ETransaction.RequestEncryptionKey;
                outTrans.IATName = IATName;
                outTrans.IsLastTransaction = false;
                outTrans.StringValue = "Data";
                CPartiallyEncryptedRSAKey rsaKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
                CallSOAP(MySOAP.ESoapAction.RequestRSAKey, outTrans, rsaKey);
                if (rsaKey.IsNullKey())
                {
                    OperationFailed("An error while retrieving your encryption information from the server.  This is likely due to database problems.  If this problem persists, please contact us at admin@iatsoftware.net.", "Server Error");
                    return null;
                }
                outTrans.Transaction = TransactionRequest.ETransaction.RetrieveResults;
                TransactionRequest inTrans = new TransactionRequest();
                CallSOAP(MySOAP.ESoapAction.RetrieveResults, outTrans, inTrans);
                MySOAP.BeginNewTransactionEvent("Retrieving test result data");
                if (inTrans.Transaction != TransactionRequest.ETransaction.RequestRetrievalReady)
                {
                    OperationFailed("An error was encountered while negotiating communication with the server.  If this problem persists, please contact us at admin@iatsoftware.net.", "Server Error");
                    return null;
                }
                CResultPacket packet;
                List<CResultPacket> packetList = new List<CResultPacket>();
                outTrans = new TransactionRequest();
                outTrans.Transaction = TransactionRequest.ETransaction.RequestPacket;
                SetProgressBarRange(0, Descriptor.NumResults);
                if (Descriptor.NumResults == 0)
                {
                    OperationFailed("No results exist on the server for this IAT.", "No Test Results");
                    return null;
                }
                MySOAP.CurrentTransactionEvent.MaxProgressValue = Descriptor.NumResults;
                MySOAP.CurrentTransactionEvent.ProgressValue = 0;
                do
                {
                    packet = new CResultPacket(Descriptor.BeforeSurveys.Count, Descriptor.AfterSurveys.Count, Descriptor);
                    MySOAP.CurrentTransactionEvent.ProgressValue = MySOAP.CurrentTransactionEvent.ProgressValue + 1;
                    CallSOAP(MySOAP.ESoapAction.RequestPacket, outTrans, packet);
                    if (!packet.IsNullPacket)
                    {
                        packetList.Add(packet);
                        IncrementProgressBar(1);
                    }
                } while (!packet.IsNullPacket);
                OperationSuccess();
                return packetList;
            }
            catch (TransactionAbortedException)
            {
                return null;
            }
            catch (CXmlSerializationException ex)
            {
                try
                {
                    ErrorReportDisplay errorForm = new ErrorReportDisplay(ex.Message, ex);
                    errorForm.ShowDialog();
                }
                catch (Exception)
                {
                    return null;
                }
                return null;
            }
            catch (Exception ex)
            {
                CXmlSerializationException ex2 = new CXmlSerializationException("Error Retrieving Test Results", ex.Message, ex);
                ErrorReportDisplay errorForm = new ErrorReportDisplay(ex2.Message, ex2);
                errorForm.ShowDialog();
                return null;
            }
            finally
            {
                MySOAP.EndTransaction();
            }
        }
        */
        public ETransactionResult DeleteIATData(String IATName, String password)
        {
            TransactionType = ETransaction.deleteIATData;
            _ErrorMessage = String.Empty;
            _TransmissionException = null;
            currentTransResult = ETransactionResult.failed;
            CurrIATName = IATName;
            CurrIATPassword = password;
            transactionCompleteEvent.Reset();
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnDeleteIATDataTransaction);
            websocket = new WebSocket(Properties.Resources.sDataTransactionWebsocketURI);
            websocket.Opened += new EventHandler(WebSocket_Opened);
            websocket.Closed += new EventHandler(WebSocket_Closed);
            websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(CEnvelope.MessageReceived);
            websocket.Open();
            transactionCompleteEvent.WaitOne();
            return currentTransResult;
        }

        public void OnDeleteIATDataTransaction(INamedXmlSerializable obj)
        {
            TransactionRequest trans = (TransactionRequest)obj;
            TransactionRequest outTrans = null;
            switch (trans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    outTrans = new TransactionRequest(TransactionRequest.ETransaction.IATExists, IATConfigMainForm.ServerPassword, CurrIATName);
                    TransactionStage = ETransactionStage.verifyingIATExistence;
                    CEnvelope.SendMessage(outTrans, websocket);
                    break;

                case TransactionRequest.ETransaction.IATExists:
                    TransactionStage = ETransactionStage.verifyingPassword;
                    PasswordVerifier passVerify = new PasswordVerifier(websocket, CurrIATName, CurrIATPassword, CPartiallyEncryptedRSAKey.EKeyType.Admin);
                    if (!passVerify.Verify())
                    {
                        MessageBox.Show("The password you supplied for this IAT is incorrect.", "Incorrect Password");
                        currentTransResult = ETransactionResult.failed;
                        websocket.Close();
                    }
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.DeleteIATData;
                    CEnvelope.SendMessage(outTrans, websocket);
                    break;

                case TransactionRequest.ETransaction.TransactionSuccess:
                    currentTransResult = ETransactionResult.success;
                    websocket.Close();
                    break;

            }
        }
    
        public ETransactionResult DeleteIAT(String IATName, String password)
        {
            TransactionType = ETransaction.deleteIAT;
            _ErrorMessage = String.Empty;
            _TransmissionException = null;
            currentTransResult = ETransactionResult.failed;
            CurrIATName = IATName;
            CurrIATPassword = password;
            transactionCompleteEvent.Reset();
            CEnvelope.ClearMessageMap();
            CEnvelope.OnReceipt[CEnvelope.EMessageType.Handshake] = new Action<INamedXmlSerializable>(ShakeHands);
            CEnvelope.OnReceipt[CEnvelope.EMessageType.TransactionRequest] = new Action<INamedXmlSerializable>(OnDeleteIATTransaction);
            websocket = new WebSocket(Properties.Resources.sDataTransactionWebsocketURI);
            websocket.Opened += new EventHandler(WebSocket_Opened);
            websocket.Closed += new EventHandler(WebSocket_Closed);
            websocket.MessageReceived += new EventHandler<MessageReceivedEventArgs>(CEnvelope.MessageReceived);
            websocket.Open();
            transactionCompleteEvent.WaitOne();
            return currentTransResult;
        }

        public void OnDeleteIATTransaction(INamedXmlSerializable obj)
        {
            TransactionRequest trans = (TransactionRequest)obj;
            TransactionRequest outTrans = null;
            switch (trans.Transaction)
            {
                case TransactionRequest.ETransaction.RequestTransmission:
                    outTrans = new TransactionRequest(TransactionRequest.ETransaction.IATExists, IATConfigMainForm.ServerPassword, CurrIATName);
                    TransactionStage = ETransactionStage.verifyingIATExistence;
                    CEnvelope.SendMessage(outTrans, websocket);
                    break;

                case TransactionRequest.ETransaction.IATExists:
                    TransactionStage = ETransactionStage.verifyingPassword;
                    PasswordVerifier passVerify = new PasswordVerifier(websocket, CurrIATName, CurrIATPassword, CPartiallyEncryptedRSAKey.EKeyType.Admin);
                    if (!passVerify.Verify())
                    {
                        MessageBox.Show("The password you supplied for this IAT is incorrect.", "Incorrect Password");
                        currentTransResult = ETransactionResult.failed;
                        websocket.Close();
                    }
                    outTrans = new TransactionRequest();
                    outTrans.Transaction = TransactionRequest.ETransaction.DeleteIAT;
                    CEnvelope.SendMessage(outTrans, websocket);
                    break;

                case TransactionRequest.ETransaction.TransactionSuccess:
                    currentTransResult = ETransactionResult.success;
                    websocket.Close();
                    break;
            }
        }
        /*
                MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.Deletion);
                if (!NegotiateConnection(IATName))
                    return false;
                SetStatusBarMessage("Verifying password");
                if (!VerifyAdminPassword(IATName, password))
                {
                    OperationFailed("An incorrect password was supplied", "Incorrect Password");
                    return false;
                }
                MySOAP.BeginNewTransactionEvent("Verifying IAT Existance");
                TransactionRequest outTrans = new TransactionRequest(TransactionRequest.ETransaction.IATExists, IATConfigMainForm.ServerPassword, IATName, Properties.Resources.sDataProviderServlet);
                TransactionRequest inTrans = new TransactionRequest();
                outTrans.IsLastTransaction = false;
                CallSOAP(MySOAP.ESoapAction.IATExists, outTrans, inTrans);
                if (inTrans.Transaction != TransactionRequest.ETransaction.IATExists)
                {
                    OperationFailed("No IAT with that name exists on the server, registered to your account", "No Such IAT");
                    return false;
                }
                MySOAP.BeginNewTransactionEvent("Deleting IAT");
                SetStatusBarMessage("Deleting IAT");
                outTrans = new TransactionRequest(TransactionRequest.ETransaction.VerifyPassword, IATConfigMainForm.ServerPassword, IATName, Properties.Resources.sDataProviderServlet);
                outTrans.IsLastTransaction = true;
                inTrans = new TransactionRequest();
                CallSOAP(MySOAP.ESoapAction.DeleteIAT, outTrans, inTrans);
                if (inTrans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
                {
                    OperationFailed(Properties.Resources.sDeleteIATFail, Properties.Resources.sDeleteIATFailCaption);
                    return false;
                }
                OperationSuccess();
                return true;
            }
            catch (TransactionAbortedException) {
                return false;
            }
            catch (CXmlSerializationException ex)
            {
                try
                {
                    ErrorReportDisplay errorForm = new ErrorReportDisplay(ex.Message, ex);
                    errorForm.Show();
                }
                catch (Exception)
                {
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                CXmlSerializationException ex2 = new CXmlSerializationException("Error Retrieving Test Results", ex.Message, ex);
                ErrorReportDisplay errorForm = new ErrorReportDisplay(ex2.Message, ex2);
                return false;
            }
            finally
            {
                MySOAP.EndTransaction();
            }
        }
        
        public bool DeleteIATData(String iatName, String password)
        {
            try
            {
                TransactionEvent tEvent = MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.Deletion);
                if (!NegotiateConnection(iatName))
                    return false;
                SetStatusBarMessage("Verifying password");
                if (!VerifyAdminPassword(iatName, password))
                {
                    OperationFailed("An incorrect password was supplied.", "Incorrect Password");
                    return false;
                }
                MySOAP.BeginNewTransactionEvent("Deleting IAT Data");
                SetStatusBarMessage("Deleting IAT Data");
                TransactionRequest outTrans = new TransactionRequest(TransactionRequest.ETransaction.VerifyPassword, IATConfigMainForm.ServerPassword, iatName, Properties.Resources.sDataProviderServlet);
                outTrans.IsLastTransaction = true;
                TransactionRequest inTrans = new TransactionRequest();
                CallSOAP(MySOAP.ESoapAction.DeleteIATData, outTrans, inTrans);
                if (inTrans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
                {
                    OperationFailed(Properties.Resources.sDeleteIATDataFail, Properties.Resources.sDeleteIATDataFailCaption);
                    return false;
                }
                OperationSuccess();
                return true;
            }
            catch (TransactionAbortedException)
            {
                return false;
            }
            catch (CXmlSerializationException ex)
            {
                try
                {
                    ErrorReportDisplay errorForm = new ErrorReportDisplay(ex.Message, ex);
                    errorForm.Show();
                }
                catch (Exception)
                {
                    return false;
                }
                return false;
            }
            catch (Exception ex)
            {
                CXmlSerializationException ex2 = new CXmlSerializationException("Error Retrieving Test Results", ex.Message, ex);
                ErrorReportDisplay errorForm = new ErrorReportDisplay(ex2.Message, ex2);
                return false;
            }
            finally
            {
                MySOAP.EndTransaction();
            }
        }*/
    }
}
