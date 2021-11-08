using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Xml;
using IATClient.IATResultSetNamespaceV2;

namespace IATClient
{
    class CResultRencryption
    {
        private RSACryptoServiceProvider OldCrypt, NewCrypt;
        private CPartiallyEncryptedRSAKey OldKey, _NewKey;
        private String OldPassword, NewPassword;
        private IATResultSet Results;
        private IATConfigMainForm MainForm;
        private String IATName;
        private bool IsMultithreaded;
        private bool AbortFlag = false;
        private object lockObject = new object();

        private IATConfigMainForm.DisplayMessageBoxHandler OnDisplayMessageBox;
        private IATConfigMainForm.DisplayYesNoMessageBoxHandler OnDisplayYesNoMessageBox;
        private IATConfigMainForm.EndProgressBarUseHandler OnEndProgressBarUse;
        private IATConfigMainForm.OperationFailedHandler OnOperationFailed;
        private IATConfigMainForm.ProgressIncrementHandler OnProgressIncrement;
        private IATConfigMainForm.ResetProgressHandler OnResetProgress;
        private IATConfigMainForm.SetProgressRangeHandler OnSetProgressRange;
        private IATConfigMainForm.SetStatusMessageHandler OnSetStatusMessage;


        class ReencryptionException : Exception {}

        private bool Aborted
        {
            get
            {
                lock (lockObject)
                {
                    return AbortFlag;
                }
            }
            set
            {
                lock (lockObject)
                {
                    AbortFlag = value;
                }
            }
        }

        private bool OnCancel(int waitTimeout, bool forceAbort)
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
            Aborted = true;
            EndProgressBarUse();
            Monitor.Exit(lockObject);
            return true;
        }

        private void EndProgressBarUse()
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnEndProgressBarUse);
                else
                    throw new ReencryptionException();
            }
        }

        private DialogResult DisplayYesNoMessageBox(String msg, String caption)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    return (DialogResult)MainForm.Invoke(OnDisplayYesNoMessageBox, msg, caption);
                else
                    throw new ReencryptionException();
            }
        }

        private void DisplayMessageBox(String msg, String caption)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnDisplayMessageBox, msg, caption);
                else
                    throw new ReencryptionException();
            }
        }

        private void OperationFailed(String msg, String caption)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnOperationFailed, msg, caption);
                else
                    throw new ReencryptionException();
            }
        }

        private void SetProgressBarRange(int min, int max)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnSetProgressRange, min, max);
                else
                    throw new ReencryptionException();
            }
        }

        private void ProgressIncrement(int nInc)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnProgressIncrement, nInc);
                else
                    throw new ReencryptionException();
            }
        }

        private void ResetProgress()
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnResetProgress);
                else
                    throw new ReencryptionException();
            }
        }

        public void SetStatusMessage(String text)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OnSetStatusMessage, text);
                else
                    throw new ReencryptionException();
            }
        }

        public CPartiallyEncryptedRSAKey NewKey
        {
            get
            {
                lock (lockObject)
                {
                    return NewKey;
                }
            }
        }

        public CResultRencryption(String iatName, bool IsMultiThreaded)
        {
            MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            IATName = iatName;
            OnDisplayYesNoMessageBox += new IATConfigMainForm.DisplayYesNoMessageBoxHandler(MainForm.OnDisplayYesNoMessageBox);
            OnDisplayMessageBox += new IATConfigMainForm.DisplayMessageBoxHandler(MainForm.OnDisplayMessageBox);
            OnOperationFailed += new IATConfigMainForm.OperationFailedHandler(MainForm.OperationFailed);
            OnProgressIncrement += new IATConfigMainForm.ProgressIncrementHandler(MainForm.ProgressIncrement);
            OnResetProgress += new IATConfigMainForm.ResetProgressHandler(MainForm.ResetProgress);
            OnSetProgressRange += new IATConfigMainForm.SetProgressRangeHandler(MainForm.SetProgressRange);
            OnSetStatusMessage += new IATConfigMainForm.SetStatusMessageHandler(MainForm.SetStatusMessage);
            OnEndProgressBarUse += new IATConfigMainForm.EndProgressBarUseHandler(MainForm.EndProgressBarUse);
        }

        public bool ChangeDataPassword(String oldPass, String newPass)
        {
            OldPassword = oldPass;
            NewPassword = newPass;
            if (IsMultithreaded)
            {
                MainForm.BeginProgressBarUse(OnCancel, IATConfigMainForm.EProgressBarUses.Reencryption);
                ThreadStart proc = new ThreadStart(MultithreadedChangeDataPasswordProc);
                Thread th = new Thread(proc);
                th.Start();
                return true;
            }
            else
                return ChangeDataPasswordProc();
        }

        private void MultithreadedChangeDataPasswordProc()
        {
            ChangeDataPasswordProc();
        }

        private bool ChangeDataPasswordProc()
        {
            SetStatusMessage("Retrieving Result Descriptor");
            ResetProgress();
            ResultSetDescriptor descriptor = ResultSetDescriptor.Download(Properties.Resources.sDataProviderServlet, IATName, OldPassword);
            MySOAP.EstablishEncryption(Properties.Resources.sDataProviderServlet);
            if (MySOAP.ShakeHands(Properties.Resources.sDataProviderServlet, IATName).Transaction != TransactionRequest.ETransaction.TransactionSuccess)
            {
                    OperationFailed("The server failed to negotiate a connection with your computer.", "Server Error");
                return false;
            }
            if (MySOAP.VerifyPassword(IATName, CPartiallyEncryptedRSAKey.EKeyType.Data, OldPassword) != TransactionRequest.ETransaction.TransactionSuccess)
            {
                OperationFailed("The existing data retrieval password you supplied was incorrect.", "Password Incorrect");
                return false;
            }

            TransactionRequest Transaction = new TransactionRequest();
            Transaction.IATName = IATName;
            Transaction.StringValue = OldPassword;
            Transaction.Transaction = TransactionRequest.ETransaction.RequestResultDescriptor;
            Transaction.IsLastTransaction = false;
            TransactionRequest inTrans = new TransactionRequest();
            MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestResultDescriptor, Transaction, inTrans);
            int nPackets = inTrans.IntValue;
            SetProgressBarRange(0, nPackets);
            List<byte[]> DescriptorQueue = new List<byte[]>();
            Transaction.Transaction = TransactionRequest.ETransaction.RequestPacket;
            CPacket p;
            do
            {
                p = new CPacket();
                MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestPacket, Transaction, p);
                ProgressIncrement(1);
                if (!p.IsNullPacket)
                    DescriptorQueue.Add(p.ByteData);
            } while (!p.IsNullPacket);
            MemoryStream descriptorStream = new MemoryStream();
            for (int ctr = 0; ctr < p.Length; ctr++)
                descriptorStream.Write(DescriptorQueue[ctr], 0, DescriptorQueue[ctr].Length);
            descriptorStream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(descriptorStream, System.Text.Encoding.UTF8);
            XmlReader xReader = new XmlTextReader(reader);
            ResultSetDescriptor rsd = new ResultSetDescriptor();
            rsd.ReadXml(xReader);

            // reestablish connection
            MySOAP.EstablishEncryption(Properties.Resources.sDataProviderServlet);
            if (MySOAP.ShakeHands(Properties.Resources.sDataProviderServlet, IATName).Transaction != TransactionRequest.ETransaction.TransactionSuccess)
            {
                OperationFailed("The server failed to negotiate a connection with your computer.", "Server Error");
                return false;
            }
            MySOAP.VerifyPassword(IATName, CPartiallyEncryptedRSAKey.EKeyType.Data, OldPassword);
            Transaction = new TransactionRequest();
            Transaction.IATName = IATName;
            Transaction.StringValue = OldPassword;
            Transaction.Transaction = TransactionRequest.ETransaction.RetrieveResults;
            Transaction.IsLastTransaction = false;
            inTrans = new TransactionRequest();
            MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RetrieveResults, Transaction, inTrans);
            CResultPacket rp;
            Transaction.IATName = IATName;
            Transaction.Transaction = TransactionRequest.ETransaction.RequestPacket;
            List<CResultPacket> PacketQueue = new List<CResultPacket>();
            do
            {
                rp = new CResultPacket(descriptor.BeforeSurveys.Count, descriptor.AfterSurveys.Count, descriptor);
                MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestPacket, Transaction, rp);
                if (!rp.IsNullPacket)
                    PacketQueue.Add(rp);
            } while (!rp.IsNullPacket);
            for (int ctr = 0; ctr < PacketQueue.Count; ctr++)
                PacketQueue[ctr].Reencrypt(OldPassword, NewPassword);

            CPartiallyEncryptedRSAKey newKey = new CPartiallyEncryptedRSAKey(CPartiallyEncryptedRSAKey.EKeyType.Data);
            MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.UpdateDataRSAKey, newKey, inTrans);
            if (inTrans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
            {
                OperationFailed("The server denied the upload of your new encryption key", "Server Error");
                return false;
            }
            for (int ctr = 0; ctr < PacketQueue.Count; ctr++)
            {
                MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.SendReencryptionResultPacket, PacketQueue[ctr], inTrans);
                if (inTrans.Transaction == TransactionRequest.ETransaction.TransactionSuccess)
                {
                    OperationFailed("The upload of a packet encrypted with your new encryption key failed.", "Server Error");
                    return false; 
                }
            }
            MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.SendReencryptionResultPacket, CResultPacket.CreateNullPacket(), inTrans);
            if (inTrans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
            {
                OperationFailed("The upload of a packet encrypted with your new encryption key failed.", "Server Error");
                return false;
            }
            return true;
        }
    }
}
            


 

          