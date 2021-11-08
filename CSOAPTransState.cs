using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;
using System.Threading;

namespace IATClient
{
    class CSOAPTransState
    {
        public enum ETransResult { inProgress, aborted, completed, timeout, failed };

        private ETransResult _TransactionResult = ETransResult.inProgress;
        public delegate void SOAPTransactionCompleteHandler();
        public delegate void SOAPTransactionAbortHandler(String error);

        private HttpWebRequest _Request = null;
        private HttpWebResponse _Response = null;
        private ManualResetEvent TransactionAborted;
        private ManualResetEvent TransactionCompleted;
        private ManualResetEvent TransactionTimeout;
        private ManualResetEvent TransactionFailed;
        private SOAPTransactionCompleteHandler TransactionCompleteEvent = null;
        private SOAPTransactionAbortHandler TransactionAbortEvent = null;
        private String ErrorMessage = String.Empty;
        private WaitHandle[] WaitHandlers = null;
        private Exception _Error;

        public Exception Error
        {
            get
            {
                return _Error;
            }
        }

        public ETransResult TransactionResult
        {
            get
            {
                return _TransactionResult;
            }
        }

        public HttpWebRequest Request
        {
            get
            {
                return _Request;
            }
        }

        public HttpWebResponse Response
        {
            get
            {
                return _Response;
            }
        }

        public CSOAPTransState(HttpWebRequest request)
        {
            _Request = request;
            TransactionAborted = new ManualResetEvent(false);
            TransactionCompleted = new ManualResetEvent(false);
            TransactionTimeout = new ManualResetEvent(false);
            TransactionFailed = new ManualResetEvent(false);
            WaitHandlers = new WaitHandle[] { TransactionAborted, TransactionCompleted, TransactionTimeout, TransactionFailed };
        }

        public void WaitOnTransaction()
        {
            int resultNdx = WaitHandle.WaitAny(WaitHandlers);
            if (resultNdx == 0)
            {
                _TransactionResult = ETransResult.aborted;
                if ((TransactionAbortEvent != null) && (ErrorMessage != String.Empty))
                    TransactionAbortEvent.Invoke(ErrorMessage);
            }
            else if (resultNdx == 1)
            {
                _TransactionResult = ETransResult.completed;
                if (TransactionCompleteEvent != null)
                    TransactionCompleteEvent.Invoke();
            }
            else if (resultNdx == 2)
            {
                _TransactionResult = ETransResult.timeout;
            }
            else if (resultNdx == 3)
            {
                _TransactionResult = ETransResult.failed;
            }
        }

        public void AddTransactionCompleteListener(SOAPTransactionCompleteHandler h)
        {
            TransactionCompleteEvent += new SOAPTransactionCompleteHandler(h);
        }

        public void AddTransactionAbortListener(SOAPTransactionAbortHandler h)
        {
            TransactionAbortEvent += new SOAPTransactionAbortHandler(h);
        }

        public void Abort(String errorMessage)
        {
            ErrorMessage = errorMessage;
            _TransactionResult = ETransResult.aborted;
            TransactionAborted.Set();
        }

        public void Timeout()
        {
            _TransactionResult = ETransResult.timeout;
            TransactionTimeout.Set();
        }

        public void Abort()
        {
            _TransactionResult = ETransResult.aborted;
            TransactionAborted.Set();
        }

        public void SetComplete()
        {
            _TransactionResult = ETransResult.completed;
            TransactionCompleted.Set();
        }

        public void SetFail(Exception ex)
        {
            _TransactionResult = ETransResult.failed;
            _Error = ex;
            TransactionFailed.Set();
        }

        public void EndGetResponse(IAsyncResult async)
        {
            _Response = (HttpWebResponse)_Request.EndGetResponse(async);
        }
    }
}
