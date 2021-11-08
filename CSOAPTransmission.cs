using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Web;
using System.Net;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Serialization;



namespace IATClient
{
    class CSOAPTransmission
    {
        private const String soapNS = "http://schemas.xmlsoap.org/soap/envelope/";
        private Exception _Error = null;
        private static Encoding soapEncoding = new UTF8Encoding(false);
        private INamedXmlSerializable _InputObject, _OutputObject;
        private DESCryptoServiceProvider EncryptionProvider = null;
        private CSOAPTransState transState = null;
        private bool bEncrypted = false;
        private byte[] PostData = null;
        private byte[] GetData = null;
        private CookieContainer CookieJar = null;
        private Stream ReadStream = null, WriteStream = null;
        private int BytesRead = 0;
        private int Timeout = -1;

        public Exception Error
        {
            get
            {
                return _Error;
            }
        }

        public CSOAPTransState TransactionState
        {
            get
            {
                return transState;
            }
        }
        
        
        public INamedXmlSerializable InputObject
        {
            get
            {
                return _InputObject;
            }
        }

        public INamedXmlSerializable OutputObject
        {
            get
            {
                return _OutputObject;
            }
        }

        public String CreateSOAPEnvelope(INamedXmlSerializable obj)
        {
            MemoryStream memStream = new MemoryStream();
            XmlWriter writer = new XmlTextWriter(memStream, soapEncoding);
            writer.WriteStartDocument();
            writer.WriteStartElement("soap", "Envelope", soapNS);
            writer.WriteStartElement("Body", soapNS);
            obj.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
            writer.WriteEndDocument();
            writer.Flush();
            memStream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(memStream, soapEncoding);
            String soapMsg = reader.ReadToEnd();
            memStream.Dispose();
            return soapMsg;
        }


        public CSOAPTransmission(HttpWebRequest request, INamedXmlSerializable output, INamedXmlSerializable input)
        {
            _InputObject = input;
            String OutputSOAP = CreateSOAPEnvelope(output);
            _OutputObject = output;
            MySOAP.ESoapAction action= (MySOAP.ESoapAction)Enum.Parse(typeof(MySOAP.ESoapAction), request.Headers["SOAPAction"]);
            if ((action == MySOAP.ESoapAction.EstablishEncryption) || (action == MySOAP.ESoapAction.SendPacket))
            {
                request.ContentType = "text/plain; charset=utf-8";
                request.ContentLength = soapEncoding.GetByteCount(OutputSOAP);
                PostData = soapEncoding.GetBytes(OutputSOAP);
            }
            else
            {
                bEncrypted = true;
                MemoryStream encryptedData = new MemoryStream();
                CryptoStream cStream = new CryptoStream(encryptedData, MySOAP.GetEncryptor(), CryptoStreamMode.Write);
                cStream.Write(soapEncoding.GetBytes(OutputSOAP), 0, soapEncoding.GetByteCount(OutputSOAP));
                cStream.FlushFinalBlock();
                String encryptedBase64 = Convert.ToBase64String(encryptedData.ToArray());
                request.ContentType = "text/plain; charset=utf-8";
                request.ContentLength = soapEncoding.GetByteCount(encryptedBase64);
                PostData = soapEncoding.GetBytes(encryptedBase64);
            }
            transState = new CSOAPTransState(request);
        }

        public void SetAbortHandler(CSOAPTransState.SOAPTransactionAbortHandler abortHandler)
        {
            transState.AddTransactionAbortListener(abortHandler);
        }

        public CSOAPTransState Perform(CookieContainer cookieJar, int timeout)
        {
            CookieJar = cookieJar;
            Timeout = timeout;
            IAsyncResult result = transState.Request.BeginGetRequestStream(new AsyncCallback(WriteRequestStream), transState);
            return transState;
        }

        public INamedXmlSerializable GetResponse()
        {            
            MemoryStream memStream = null;
            if (transState.Response.Headers["Encrypted"] == "Yes")
            {
                memStream = Decrypt(GetData);
            }
            else
            {
                memStream = new MemoryStream(GetData);
            }
            StreamReader sReader = new StreamReader(memStream, soapEncoding);
            XmlReader xmlReader = new XmlTextReader(sReader);
            xmlReader.ReadToDescendant(InputObject.GetName());
            InputObject.ReadXml(xmlReader);
            CookieJar.Add(transState.Response.Cookies);
            return InputObject;
        }

        private MemoryStream Decrypt(byte[] data)
        {
            MemoryStream memStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(memStream, MySOAP.GetDecryptor(), CryptoStreamMode.Write);
            CryptoStream b64Stream = new CryptoStream(cStream, new FromBase64Transform(FromBase64TransformMode.IgnoreWhiteSpaces), CryptoStreamMode.Write);
            b64Stream.Write(data, 0, data.Length);
            b64Stream.FlushFinalBlock();
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }

        private void WriteRequestStream(IAsyncResult async)
        {
            CSOAPTransState transState = (CSOAPTransState)async.AsyncState;
            try
            {
                if (transState.TransactionResult != CSOAPTransState.ETransResult.inProgress)
                    return;
                HttpWebRequest request = transState.Request;
                WriteStream = request.GetRequestStream();
                WriteStream.BeginWrite(PostData, 0, PostData.Length, new AsyncCallback(WriteComplete), async.AsyncState);
            }
            catch (Exception ex)
            {
                transState.SetFail(ex);
            }
        }

        public void WriteComplete(IAsyncResult async)
        {
            CSOAPTransState transState = (CSOAPTransState)async.AsyncState;
            try
            {
                WriteStream.EndWrite(async);
                transState = (CSOAPTransState)async.AsyncState;
                if (transState.TransactionResult != CSOAPTransState.ETransResult.inProgress)
                    return;
                HttpWebRequest request = transState.Request;
                IAsyncResult asyncResult = request.BeginGetResponse(new AsyncCallback(ReceiveResponse), async.AsyncState);
                ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), transState, 1000 * Timeout, true);
            }
            catch (Exception ex)
            {
                transState.SetFail(ex);
            }
        }

        public void ReceiveResponse(IAsyncResult async)
        {
            CSOAPTransState transState = (CSOAPTransState)async.AsyncState;
            try
            {
                if (transState.TransactionResult != CSOAPTransState.ETransResult.inProgress)
                    return;
                transState.EndGetResponse(async);
                if (transState.Response.StatusCode != HttpStatusCode.OK)
                {
                    transState.Abort(transState.Response.Headers["Error"]);
                    return;
                }
                ReadStream = transState.Response.GetResponseStream();
                GetData = new byte[transState.Response.ContentLength];
                BytesRead = 0;
                IAsyncResult asyncResult = ReadStream.BeginRead(GetData, 0, (GetData.Length < 4096) ? GetData.Length : 4096, new AsyncCallback(ReadCallback), async.AsyncState);
                ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), transState, Timeout * 1000, true);
            }
            catch (Exception ex)
            {
                transState.SetFail(ex);
            }
        }

        private void ReadCallback(IAsyncResult async)
        {
            CSOAPTransState transState = (CSOAPTransState)async.AsyncState;
            try
            {
                if (transState.TransactionResult != CSOAPTransState.ETransResult.inProgress)
                    return;
                BytesRead += ReadStream.EndRead(async);
                if ((BytesRead < GetData.Length) || (GetData.Length == 0))
                {
                    IAsyncResult asyncResult = ReadStream.BeginRead(GetData, BytesRead, (BytesRead + 4096 < GetData.Length) ? 4096 : (GetData.Length - BytesRead), new AsyncCallback(ReadCallback), transState);
                    ThreadPool.RegisterWaitForSingleObject(asyncResult.AsyncWaitHandle, new WaitOrTimerCallback(TimeoutCallback), transState, Timeout * 1000, true);
                }
                else
                {
                    ReadStream.Close();
                    transState.SetComplete();
                }
                return;
            }
            catch (Exception ex)
            {
                transState.SetFail(ex);
            }
        }

        private void TimeoutCallback(Object state, bool timeout)
        {
            if (timeout)
            {
                if (transState != null)
                    transState.Timeout();
            }
        }

        public void AbortTransaction()
        {
            if (transState != null)
            {
                transState.Abort();
                if (transState.Request != null)
                {
                    transState.Request.Abort();
                }
            }
        }
    }
}
