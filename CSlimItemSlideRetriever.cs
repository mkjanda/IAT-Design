using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Drawing;
using System.IO;

namespace IATClient
{
    class CSlimItemSlideRetriever
    {
        private CItemSlideRetriever ItemSlideRetriever;
        private String IATName, Password;
        public delegate void OnSlideRetrievalComplete(CSlimItemSlideRetriever isr);
        private OnSlideRetrievalComplete OperationComplete;
        private Action<String, String, bool> OnFail;
        private IATConfigMainForm.ShowFormHandler ShowForm;
        private IATConfigMainForm MainForm;
        private Dictionary<String, Image> SlideDictionary = new Dictionary<String, Image>();
        private Manifest FileManifest = new Manifest();
        private ItemSlideManifest SlideManifest = null;
        private int SlidesLoaded = 0;

        public CSlimItemSlideRetriever(String iatName, String password)
        {
            IATName = iatName;
            Password = password;
        }

        public void Start(OnSlideRetrievalComplete onComplete, IATConfigMainForm mainForm, Action<String, String, bool> onFail)
        {
            MainForm = mainForm;
            OnFail = onFail;
            OperationComplete += new OnSlideRetrievalComplete(onComplete);
            ShowForm += new IATConfigMainForm.ShowFormHandler(MainForm.ShowForm);
            ThreadStart proc = new ThreadStart(DoItemSlideRetrieval);
            Thread th = new Thread(proc);
            th.Start();
        }

        private void OnAbort(String errorMsg)
        {
            ItemSlideRetriever.Abort();
            MainForm.Invoke(OnFail, "Error Retrieving Item Slides", errorMsg, false);
        }

        public bool RetrieveManifest()
        {
            try
            {
                MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.DataRetrieval);
                MySOAP.EstablishEncryption(Properties.Resources.sDataProviderServlet);
                TransactionRequest inTrans = MySOAP.ShakeHands(Properties.Resources.sDataProviderServlet, IATName);
                if (inTrans.Transaction == TransactionRequest.ETransaction.ClientFrozen)
                {
                    MainForm.Invoke(OnFail, "Your account appears to have been frozen.  Please contact us at admin@iatsoftware.net for details.", "Account Frozen", false);
                    return false;
                }
                else if (inTrans.Transaction == TransactionRequest.ETransaction.ClientDeleted)
                {
                    MainForm.Invoke(OnFail, "You no longer have an account with IAT Software.", "Account Deleted");
                    return false;
                }
                else if (inTrans.Transaction == TransactionRequest.ETransaction.NoSuchClient)
                {
                    MainForm.Invoke(OnFail, "Your product is not registered with the IATSoftware.net server.  If you believe you are seeing this message in error, " +
                        "please contact us at admin@iatsoftware.net", "Unknown Client", false);
                    return false;
                }
                else if (inTrans.Transaction != TransactionRequest.ETransaction.RequestTransmission)
                {
                    MainForm.Invoke(OnFail, "Could Not Connect with Server.", "Server Error", false);
                    return false;
                }
                TransactionRequest trans = new TransactionRequest();
                trans.IATName = IATName;
                trans.IsLastTransaction = false;
                MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.IATExists, trans, inTrans);
                if (inTrans.Transaction != TransactionRequest.ETransaction.IATExists)
                {
                    MainForm.Invoke(OnFail, "No IAT with this name that is registered to you exists on the server.", "No Such IAT", false);
                    return false;
                }
                TransactionRequest.ETransaction transactionResult = MySOAP.VerifyPassword(IATName, CPartiallyEncryptedRSAKey.EKeyType.Data, Password);
                if (transactionResult == TransactionRequest.ETransaction.Unset)
                {
                    MainForm.Invoke(OnFail, "Unable to verify data retrieval password.  This password is correct, however due to server errors or connectivity issues, it cannot be re-verified.  If this problem persists, contact us at admin@iatsoftware.net.", "Server Error", false);
                    return false;
                }
                else if (transactionResult != TransactionRequest.ETransaction.TransactionSuccess)
                {
                    MainForm.Invoke(OnFail, "An incorrect password for this IAT was supplied.", "Password Incorrect", false);
                    return false;
                }
                trans.IsLastTransaction = true;
                trans.Transaction = TransactionRequest.ETransaction.GetItemSlideManifest;
                MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.GetItemSlideManifest, trans, FileManifest);
                return true;
            }
            catch (TimeoutException)
            {
                MainForm.Invoke(OnFail, "The attempt to retrieve the item slide manifest from the server timed out.  Please try again later.", "Communication Timeout", false);
                return false;
            }
            catch (CXmlSerializationException ex)
            {
                ErrorReportDisplay dlg = new ErrorReportDisplay("Error Retrieving Item Slides", ex);
                MainForm.Invoke(ShowForm, dlg);
                return false;
            }
        }

        private bool SlideReceived(String name, byte[] data)
        {
            uint[] IDs = SlideManifest.GetItemIDs(name);
            MemoryStream memStream = new MemoryStream(data);
            Image img = Image.FromStream(memStream);
            for (int ctr = 0; ctr < IDs.Length; ctr++)
                SlideDictionary[String.Format("Item {0}", IDs[ctr])] = img;
            if (++SlidesLoaded == SlideManifest.ItemSlideEntries.Length)
                MainForm.Invoke(OperationComplete, this);
            return true;
        }

        private void OnManifestRecieved(ItemSlideManifest manifest)
        {
            SlideManifest = manifest;
        }

        private void DoItemSlideRetrieval()
        {
            if (!RetrieveManifest())
                return;
            ItemSlideRetriever = new CItemSlideRetriever(FileManifest, Password, MainForm, (Func<String, byte[], bool>)SlideReceived, (Action<ItemSlideManifest>)OnManifestRecieved);
            ItemSlideRetriever.RetrieveItemSlides(MainForm.IATName, OnFail);
        }

        public void Save(String path)
        {
            foreach (String filename in SlideDictionary.Keys)
                SlideDictionary[filename].Save(path + System.IO.Path.DirectorySeparatorChar + filename, System.Drawing.Imaging.ImageFormat.Jpeg);
        }
    }
}
