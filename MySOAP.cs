using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Web;
using System.Threading;
using System.Text.RegularExpressions;

namespace IATClient
{
    class TransactionAbortedException : Exception
    {
        public TransactionAbortedException()
        {
            //   MySOAP.TerminateConnection("Transmission Error");
        }
    }

    class MySOAP
    {
        private static CookieContainer CookieJar;
        private static Encoding soapEncoding = new UTF8Encoding(false);
        private static TransactionProgress _TransProgress;
        private static DESCryptoServiceProvider EncryptionProvider = new DESCryptoServiceProvider();
        private static CSOAPTransState _TransactionState = null;
        private static object lockObject = new object();
        private static byte[] Cipher = null;
        private static bool bTransInitialized = false;

        public static CSOAPTransState TransactionState {
            get {
                lock (lockObject) {
                    return _TransactionState;
                }
            }
            set {
                lock (lockObject) {
                    _TransactionState = value;
                }
            }
        }
                

        private static byte[] IV
        {
            get
            {
                return new byte[] { (byte)0xFA, (byte)0x64, (byte)0x92, (byte)0x21, (byte)0x4A, (byte)0x74, (byte)0x41, (byte)0xE9 };
            }
        }

        private static CSOAPTransState.SOAPTransactionAbortHandler TransactionAbortHandler;


        public static TransactionEvent BeginNewTransaction(TransactionProgress.ETransactionType type)
        {
            _TransProgress = new TransactionProgress(type);
            TransactionEvent tEvent = new TransactionEvent("Negotiating Transmission with Server");
            _TransProgress.BeginNewEvent(tEvent);
            return tEvent;
        }

        public static TransactionEvent BeginNewTransaction(TransactionProgress.ETransactionType type, String iatName)
        {
            CookieJar = null;
            _TransProgress = new TransactionProgress(type, iatName);
            TransactionEvent tEvent = new TransactionEvent("Negotiating Transmission with Server");
            _TransProgress.BeginNewEvent(tEvent);
            return tEvent;
        }

        public static TransactionEvent BeginNewTransactionEvent(String name)
        {
            TransactionEvent tEvent = new TransactionEvent(name);
            bTransInitialized = true;
            _TransProgress.BeginNewEvent(tEvent);
            return tEvent;
        }

        public static TransactionEvent CurrentTransactionEvent
        {
            get
            {
                return _TransProgress.CurrentEvent;
            }
        }

        public static TransactionProgress TerminateTransaction(String reason)
        {
            if (_TransProgress != null)
                _TransProgress.Terminate(reason);
            TransactionProgress tp = _TransProgress;
            _TransProgress = null;
            bTransInitialized = false;
            CookieJar = null;
            return tp;
        }

        public static void EndTransaction()
        {
            _TransProgress = null;
            bTransInitialized = false;
            CookieJar = null;
        }


        public enum ESoapAction
        {
            AbortTransaction, DeleteIATData, DeleteIAT, DoIATDeploy, EstablishEncryption, GetItemSlideManifest, HaltIATDeployment, Handshake,
            IATExists, InitiateIATDeployment, QueryDeploymentProgress, QueryItemSize, RecordEncryptionKey, RequestActivation, RequestEMailVerification, RequestFiles, RequestIATList,
            RequestNewVerificationEMail, RequestPacket, RequestPasswordVerification, RequestRemainingResources, RequestResultDescriptor, RequestRSAKey, RequestSOAPExchange, RequestUsageReport, 
            RetrieveItemSlideManifest, RetrieveResults, SendManifest, SendPacket, SendReencryptionResultPacket, StorePublicKey, UpdateDataRSAKey, VerifyIATDeployment, VerifyPassword
        };

        static public INamedXmlSerializable PerformTransaction(String destURL, String productCode, ESoapAction soapAction, TransactionRequest sentObj, INamedXmlSerializable receivedObj)
        {
            try
            {
                EstablishEncryption(destURL);
                if (ShakeHands(destURL, String.Empty).Transaction != TransactionRequest.ETransaction.TransactionSuccess)
                    return null;
                sentObj.IsLastTransaction = true;
                MySOAP.CallSOAP(destURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), soapAction, sentObj, receivedObj);
            }
            catch (Exception)
            {
                return null;
            }
            return receivedObj;
        }

        static public TransactionRequest ShakeHands(String serverURL, String IATName)
        {
            TransactionRequest trans = new TransactionRequest(TransactionRequest.ETransaction.RequestTransmission, IATConfigMainForm.ServerPassword,
                IATName, serverURL);
            HandShake inHand = new HandShake();
            CurrentTransactionEvent.AddChildEvent("Requesting Hand Shake");
            CallSOAP(serverURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestSOAPExchange, trans, inHand);
            if (!inHand.Valid)
            {
                trans.Transaction = TransactionRequest.ETransaction.NoSuchClient;
                return trans;
            }
            CurrentTransactionEvent.AddChildEvent("Formulating Reply");
            HandShake outHand = HandShake.CreateResponse(inHand);
            CurrentTransactionEvent.AddChildEvent("Sending Response Hand Shake");
            MySOAP.CallSOAP(serverURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.Handshake, outHand, trans);
            return trans;
        }

        static public void SetCipher(byte[] cipher)
        {
            Cipher = cipher;
            EncryptionProvider.Mode = CipherMode.CBC;
            EncryptionProvider.Padding = PaddingMode.ISO10126;
            bTransInitialized = true;
        }

        static public void EstablishEncryption(String destURL)
        {
            CookieJar = null;
            CurrentTransactionEvent.AddChildEvent(new TransactionEvent("Generating Asymmetric Encryption Key"));
            RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
            RSAParameters rsaParams = rsaCrypt.ExportParameters(true);
            CPublicKey pk = new CPublicKey(rsaParams.Modulus, rsaParams.Exponent);
            TransactionRequest trans = new TransactionRequest();
            trans.ProductKey = String.Empty;
            trans.UserKey = String.Empty;
            CurrentTransactionEvent.AddChildEvent(new TransactionEvent("Sending Public Key to Server"));
            CallSOAP(destURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.EstablishEncryption, pk, trans);
            CurrentTransactionEvent.AddChildEvent(new TransactionEvent("Decrypting Shared Cipher"));
            Cipher = rsaCrypt.Decrypt(Convert.FromBase64String(trans.StringValue), false);
            EncryptionProvider.Mode = CipherMode.CBC;
            EncryptionProvider.Padding = PaddingMode.ISO10126;
            bTransInitialized = true;
        }

        static public TransactionRequest.ETransaction VerifyPassword(String IATName, CPartiallyEncryptedRSAKey.EKeyType passwordType, String password)
        {
            String destURL = Properties.Resources.sDataProviderServlet;
            int destPort = Convert.ToInt32(Properties.Resources.sDataProviderPort);
            CPartiallyEncryptedRSAKey rsa = new CPartiallyEncryptedRSAKey(passwordType);
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.RequestRSAKey;
            trans.IATName = IATName;
            trans.StringValue = passwordType.ToString();
            trans.IsLastTransaction = false;
            MySOAP.CallSOAP(destURL, destPort, ESoapAction.RequestRSAKey, trans, rsa);
            if (rsa.IsNullKey())
                return TransactionRequest.ETransaction.Unset;
            rsa.DecryptKey(password);
            if (passwordType == CPartiallyEncryptedRSAKey.EKeyType.Admin)
                trans.Transaction = TransactionRequest.ETransaction.RequestAdminPasswordVerification;
            else if (passwordType == CPartiallyEncryptedRSAKey.EKeyType.Data)
                trans.Transaction = TransactionRequest.ETransaction.RequestDataPasswordVerification;
            trans.StringValue = passwordType.ToString();
            trans.IsLastTransaction = false;
            TransactionRequest inTrans = new TransactionRequest();
            MySOAP.CallSOAP(destURL, destPort, MySOAP.ESoapAction.RequestPasswordVerification, trans, inTrans);
            if (inTrans.Transaction == TransactionRequest.ETransaction.TransactionFail)
                return TransactionRequest.ETransaction.Unset;
            RSACryptoServiceProvider crypt = new RSACryptoServiceProvider();
            crypt.ImportParameters(rsa.GetRSAParameters());
            byte[] decryptedData = crypt.Decrypt(Convert.FromBase64String(inTrans.StringValue), false);
            trans.Transaction = TransactionRequest.ETransaction.VerifyPassword;
            trans.StringValue = Convert.ToBase64String(decryptedData);
            trans.IsLastTransaction = false;
            MySOAP.CallSOAP(destURL, destPort, MySOAP.ESoapAction.VerifyPassword, trans, inTrans);
            return inTrans.Transaction;
        }

        static public CSOAPTransState.ETransResult CallSOAPAsync(ESoapAction soapAction, INamedXmlSerializable outBound, INamedXmlSerializable inBound)
        {
            int timeout = 30;
            UriBuilder uBuilder = new UriBuilder();
            uBuilder.Scheme = "http";
            Regex urlExp = new Regex("//([^/]+)(.+)");
            Match m = urlExp.Match(Properties.Resources.sDataProviderServlet);
            uBuilder.Host = m.Groups[1].Value;
            uBuilder.Path = m.Groups[2].Value;
            uBuilder.Port = Convert.ToInt32(Properties.Resources.sDataProviderPort);
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uBuilder.Uri);
            if (CookieJar == null)
                CookieJar = new CookieContainer();
            request.CookieContainer = CookieJar;
            request.ContentType = "text/html; charset=" + soapEncoding.HeaderName;
            request.Headers["SOAPAction"] = soapAction.ToString();
            request.Headers["Version"] = Properties.Resources.sVersion;
            request.Method = "POST";
            CSOAPTransmission transmission = new CSOAPTransmission(request, outBound, inBound);
            TransactionState = transmission.TransactionState;
            Action wait = (Action)TransactionState.WaitOnTransaction;
            IAsyncResult async = wait.BeginInvoke(null, null);
            lock (lockObject)
                TransactionState = transmission.Perform(CookieJar, timeout);
            wait.EndInvoke(async);
            lock (lockObject)
            {
                if (TransactionState.TransactionResult == CSOAPTransState.ETransResult.completed)
                {
                    try
                    {
                        inBound = transmission.GetResponse();
                        return CSOAPTransState.ETransResult.completed;
                    }
                    catch (CXmlSerializationException ex)
                    {
                        ErrorReportDisplay errDlg = new ErrorReportDisplay(ex.Message, ex);
                        errDlg.ShowDialog();
                        return CSOAPTransState.ETransResult.failed;
                    }

                }
                else if (TransactionState.TransactionResult == CSOAPTransState.ETransResult.aborted)
                {
                    TransactionState = null;
                    return CSOAPTransState.ETransResult.aborted;
                }
                else if (TransactionState.TransactionResult == CSOAPTransState.ETransResult.timeout)
                {
                    TransactionState = null;
                    return CSOAPTransState.ETransResult.timeout;
                }
                else if (TransactionState.TransactionResult == CSOAPTransState.ETransResult.failed)
                {
                    return CSOAPTransState.ETransResult.failed;
                }
                else return CSOAPTransState.ETransResult.inProgress;
            }
        }

        static public void CallSOAP(String destURL, int destPort, ESoapAction soapAction, INamedXmlSerializable outBound, INamedXmlSerializable inBound)
        {
            CallSOAP(destURL, destPort, soapAction, outBound, inBound, 30);
        }

        static public void CallSOAP(String destURL, int destPort, ESoapAction soapAction, INamedXmlSerializable outBound, INamedXmlSerializable inBound, int timeOut)
        {
            CallSOAP(destURL, destPort, soapAction, outBound, inBound, timeOut, 20);
        }

        static public void CallSOAP(String destURL, int destPort, ESoapAction action, INamedXmlSerializable outObj, INamedXmlSerializable inObj, int timeout, int retryCtr)
        {
            Uri destUri = new Uri(destURL);
            UriBuilder uBuilder = new UriBuilder();
            uBuilder.Scheme = "http";
            Regex urlExp = new Regex("//([^/]+)(.+)");
            Match m = urlExp.Match(destURL);
            uBuilder.Host = m.Groups[1].Value;
            uBuilder.Path = m.Groups[2].Value;
            uBuilder.Port = destPort;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uBuilder.Uri);

            if (CookieJar == null)
                CookieJar = new CookieContainer();
            request.CookieContainer = CookieJar;
            request.ContentType = "text/html; charset=" + soapEncoding.HeaderName;
            request.Headers["SOAPAction"] = action.ToString();
            request.Headers["Version"] = Properties.Resources.sVersion;
            request.Method = "POST";
            CSOAPTransmission transmission = new CSOAPTransmission(request, outObj, inObj);
            lock (lockObject)
                TransactionState = transmission.Perform(CookieJar, timeout);
            TransactionState.WaitOnTransaction();
            lock (lockObject)
            {
                if (TransactionState.TransactionResult == CSOAPTransState.ETransResult.completed)
                    outObj = transmission.GetResponse();
                else if (TransactionState.TransactionResult == CSOAPTransState.ETransResult.timeout)
                {
                    TransactionState = null;
                    throw new TimeoutException();
                }
                else if (TransactionState.TransactionResult == CSOAPTransState.ETransResult.aborted)
                {
                    TransactionState = null;
                    throw new TransactionAbortedException();
                }
                else if (TransactionState.TransactionResult == CSOAPTransState.ETransResult.failed)
                {
                    throw new CXmlSerializationException("Communication Error", TransactionState.Error.Message, TransactionState.Error);
                }
                TransactionState = null;
            }

        }

        public static bool AbortCurrentTransaction(String errorMsg)
        {
            lock (lockObject)
            {
                if (TransactionState != null)
                    TransactionState.Abort(errorMsg);
                return true;
            }
        }

        public static void AbortCurrentTransaction()
        {
            lock (lockObject)
            {
                if (TransactionState != null)
                    TransactionState.Abort();
            }
        }


        public static ICryptoTransform GetEncryptor()
        {
            if (EncryptionProvider == null)
                return null;
            return EncryptionProvider.CreateEncryptor(Cipher, IV);
        }

        public static ICryptoTransform GetDecryptor()
        {
            if (EncryptionProvider == null)
                return null;
            return EncryptionProvider.CreateDecryptor(Cipher, IV);
        }

        protected static void RetrieveSerializableObject(INamedXmlSerializable obj, XmlReader reader)
        {
            reader.ReadToDescendant(obj.GetName());
            obj.ReadXml(reader);
        }


        public static TransactionRequest TerminateConnection(String URL)
        {
            if (CookieJar == null)
                return null;
            if (!bTransInitialized)
                return null;
            TransactionRequest trans = new TransactionRequest();
            trans.Transaction = TransactionRequest.ETransaction.AbortTransaction;
            trans.IsLastTransaction = true;
            TransactionRequest outTrans = new TransactionRequest();
            MySOAP.CallSOAP(URL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.AbortTransaction, trans, outTrans);
            CookieJar = null;
            bTransInitialized = false;
            return outTrans;
        }
    }
}
