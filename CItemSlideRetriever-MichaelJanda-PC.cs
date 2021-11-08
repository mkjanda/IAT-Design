using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Net.Sockets;
using System.Threading;
using System.Windows.Forms;
using System.Xml.Serialization;

namespace IATClient
{
    class CItemSlideRetriever
    {
        private String FullURL;
        private Manifest FileManifest;
        private String IATName, IATPassword;
        private IATConfigMainForm MainForm;
        private bool AbortFlag = false;
        private IATConfigMainForm.SetProgressRangeHandler SetProgressRange;
        private IATConfigMainForm.SetStatusMessageHandler SetProgressMessage;
        private IATConfigMainForm.ResetProgressHandler ResetProgress;
        private Action<String, String, bool> OperationFail;
        private IATConfigMainForm.ProgressIncrementHandler ProgressIncrement;
        private IATConfigMainForm.EndProgressBarUseHandler EndProgressBarUse;
        private IATConfigMainForm.OperationFailedHandler OperationFailedHandler;
        private IATConfigMainForm.OperationCompleteHandler OperationComplete;
        private IATConfigMainForm.ShowFormHandler ShowForm;
        private CPacketReceiver PacketReceiver;
        private Func<String, byte[], bool> OnFileReceived;
        private Action<ItemSlideManifest> SetSlideManifest;
        private object lockObject = new object();
        private ItemSlideManifest _SlideManifest = null;

        public ItemSlideManifest SlideManifest
        {
            get
            {
                return _SlideManifest;
            }
        }
        
        private bool IncrementProgress(int n)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(ProgressIncrement, n);
                else
                    throw new TransactionAbortedException();
            }
            return true;
        }

        private bool SetStatusMessage(String msg)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(SetProgressMessage, msg);
                else
                    throw new TransactionAbortedException();
            }
            return true;
        }

        private bool ResetProgressBar()
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(ResetProgress);
                else
                    throw new TransactionAbortedException();
            }
            return true;
        }

        private bool SetProgressBarRange(int min, int max)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(SetProgressRange, min, max);
                else
                    throw new TransactionAbortedException();
            }
            return true;
        }

        private void OnOperationFailed(String msg, String caption, bool retryOption)
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(OperationFail, msg, caption, retryOption);
                else
                    throw new TransactionAbortedException();
            }
        }

        private void OnEndProgressBarUse()
        {
            lock (lockObject)
            {
                if (!Aborted)
                    MainForm.Invoke(EndProgressBarUse);
                else
                    throw new TransactionAbortedException();
            }
        }


        private void Abort(object sender, EventArgs e)
        {
            lock (lockObject)
            {
                AbortFlag = true;
            }
        }

        public void Abort()
        {
            lock (lockObject)
            {
                AbortFlag = true;
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
            AbortFlag = true;
            MainForm.Invoke(EndProgressBarUse);
            Monitor.Exit(lockObject);
            return true;
        }

        private bool Aborted
        {
            get
            {
                lock (lockObject)
                {
                    return AbortFlag;
                }
            }
        }


        public CItemSlideRetriever(Manifest fileManifest, String dataPassword, IATConfigMainForm mainForm, Func<String, byte[], bool> onSlideReceived, Action<ItemSlideManifest> setSlideManifest)
        {
            FullURL = Properties.Resources.sDataProviderServlet;
            if (Convert.ToInt32(Properties.Resources.sDataProviderPort) != 80)
                FullURL += ":" + Properties.Resources.sDataProviderPort;
            IATName = fileManifest.IATName;
            IATPassword = dataPassword;
            FileManifest = fileManifest;
            MainForm = mainForm;
            OnFileReceived = onSlideReceived;
            SetSlideManifest = setSlideManifest;
        }

        public bool RetrieveItemSlides(String iatName, Action<String, String, bool> onFail)
        {
            IATName = iatName;
            OperationFail = onFail;
            OperationComplete = null;
            ProgressIncrement += new IATConfigMainForm.ProgressIncrementHandler(MainForm.ProgressIncrement);
            SetProgressMessage += new IATConfigMainForm.SetStatusMessageHandler(MainForm.SetStatusMessage);
            SetProgressRange += new IATConfigMainForm.SetProgressRangeHandler(MainForm.SetProgressRange);
            EndProgressBarUse += new IATConfigMainForm.EndProgressBarUseHandler(MainForm.EndProgressBarUse);
            ResetProgress += new IATConfigMainForm.ResetProgressHandler(MainForm.ResetProgress);
            ShowForm += new IATConfigMainForm.ShowFormHandler(MainForm.ShowForm);
            MainForm.BeginProgressBarUse(new IATConfigMainForm.AbortHandler(OnCancel), IATConfigMainForm.EProgressBarUses.ItemSlideRetrieval);
            MainForm.AddToolStripCancelButton(OnCancel);
            return run();
        }


        private bool run()
        {
            return ReceiveItemSlides();
        }

        private bool ReceiveItemSlides()
        {
            try
            {
                try
                {
                    TransactionEvent tEvent = MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.RetrieveItemSlides);
                    SetStatusMessage(Properties.Resources.sConnectingToServerMsg);
                    MySOAP.EstablishEncryption(FullURL);
                    if (MySOAP.ShakeHands(FullURL, IATName).Transaction != TransactionRequest.ETransaction.RequestTransmission)
                    {
                        OnOperationFailed("Unable to negotiate communications with server.  Please try again later.  If this problem persists, contact us at admin@iatsoftware.net. Do you wish to reattempt?", "Server Error", true);
                        return false;
                    }
                    MySOAP.BeginNewTransactionEvent("Verifying Password");
                    TransactionRequest.ETransaction transactionResult = MySOAP.VerifyPassword(IATName, CPartiallyEncryptedRSAKey.EKeyType.Data, IATPassword);
                    if (transactionResult == TransactionRequest.ETransaction.Unset)
                    {
                        OnOperationFailed("Unable to verify data retrieval password.  This password is correct, however due to server errors or connectivity issues, it cannot be re-verified.  If this problem persists, contact us at admin@iatsoftware.net. Do you wish to reattempt?", "Server Error", false);
                        return false;
                    }
                    else if (transactionResult != TransactionRequest.ETransaction.TransactionSuccess)
                    {
                        OnOperationFailed("An incorrect password for this IAT was supplied.", "Password Incorrect", false);
                        return false;
                    }
                    TransactionRequest trans = new TransactionRequest();
                    trans.Transaction = TransactionRequest.ETransaction.GetItemSlideManifest;
                    trans.IATName = IATName;
                    MySOAP.CallSOAP(FullURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestFiles, FileManifest, trans);
                    if (trans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
                    {
                        OnOperationFailed("The server denied the request to initiate item slide retrieval for reasons that cannot now be determined. Please try again later. If this problem persists, contact us at admin@iatsoftware.net. Do you wish to reattempt", "Server Error", true);
                        return false;
                    }
                    SetStatusMessage("Downloading Item Slides");
                    MySOAP.BeginNewTransactionEvent("Downloading Item Slides");
                    ResetProgressBar();
                    SetProgressBarRange(0, FileManifest.FileCount + 1);
                    PacketReceiver = new CPacketReceiver(FileManifest, CompleteFileReceived);
                    PacketReceiver.Start();
                    trans.Transaction = TransactionRequest.ETransaction.RequestPacket;
                    trans.IsLastTransaction = true;
                    CPacket p;
                    do
                    {
                        p = new CPacket();
                        MySOAP.CallSOAP(FullURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.RequestPacket, trans, p);
                        if (!p.IsNullPacket)
                        {
                            PacketReceiver.QueuePacket(p);
                            IncrementProgress(1);
                        }
                    } while (!p.IsNullPacket);
                }
                catch (TransactionAbortedException)
                {
                    return false;
                }
                catch (TimeoutException)
                {
                    OnOperationFailed("The attempt to retrieve item slides from the server timed out. Do you wish to reattempt?", "Communication Timeout", true);
                    return false;
                }
                catch (WebException ex)
                {
                    OnOperationFailed(ex.Message, "Internet Error", false);
                    return false;
                }
            }
            catch (CXmlSerializationException ex)
            {
                if (ex.ErrorType == CXmlSerializationException.EErrorType.fatal)
                {
                    ErrorReportDisplay errorDisplay = null;
                    errorDisplay = new ErrorReportDisplay(ex.Message, ex);
                    lock (lockObject)
                    {
                        if (!Aborted)
                        {
                            MainForm.Invoke(ShowForm, errorDisplay);
                        }
                    }
                }
                else
                {
                    ErrorReportDisplay errorDisplay = null;
                    errorDisplay = new ErrorReportDisplay(ex.Message, ex);
                    lock (lockObject)
                    {
                        if (!Aborted)
                        {
                            MainForm.Invoke(ShowForm, errorDisplay);
                        }
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                CXmlSerializationException ex2 = new CXmlSerializationException("Error Retrieving Test Results", ex.Message, ex);
                ErrorReportDisplay errorForm = new ErrorReportDisplay(ex2.Message, ex2);
                errorForm.ShowDialog();
                return false;
            }
            finally
            {
                MySOAP.EndTransaction();
                MainForm.Invoke(EndProgressBarUse);
            }
            return true;
        }

        private bool CompleteFileReceived(String name, byte[] data)
        {
            if (name == "SlideManifest.xml")
            {
                XmlSerializer ser = new XmlSerializer(typeof(ItemSlideManifest));
                MemoryStream memStream = new MemoryStream(data);
                SetSlideManifest((ItemSlideManifest)ser.Deserialize(memStream));
                memStream.Dispose();
                return true;
            }
            MainForm.Invoke(OnFileReceived, name, data);
            return true;
        }
    }
}
