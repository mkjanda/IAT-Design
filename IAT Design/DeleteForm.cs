using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.Security.Cryptography;
using System.Net;

namespace IATClient
{
    partial class DeleteForm : Form
    {
        public enum EDeletionType { IAT, Data };
        private EDeletionType DeletionType;
        private delegate void DeletionResultHandler(String msg);
        private delegate DialogResult MessagePrompt(String text, String caption);

        public DeleteForm(EDeletionType deletionType)
        {
            InitializeComponent();
            DeletionType = deletionType;
            if (DeletionType == EDeletionType.IAT)
            {
                this.Text = Properties.Resources.sDeleteIATFormTitle;
                Instructions.Text = Properties.Resources.sDeleteIATFormInstructions;
            }
            else
            {
                this.Text = Properties.Resources.sDeleteIATDataFormTitle;
                Instructions.Text = Properties.Resources.sDeleteIATDataFormInstructions;
            }
            Instructions.ReadOnly = true;
            Instructions.AcceptsTab = false;
            Instructions.TabStop = false;
            IATName.TabIndex = 0;
            IATName.TabStop = true;
            PasswordBox.TabIndex = 1;
            PasswordBox.TabStop = true;
            DeleteButton.TabIndex = 2;
            DeleteButton.TabStop = true;
            Cancel.TabIndex = 3;
            Cancel.TabStop = true;
        }

        private void TransactionAborted(String errorMsg)
        {
            if (DeletionType == EDeletionType.Data)
                MessageBox.Show("Result Data Deletion Failed", errorMsg);
            else
                MessageBox.Show("IAT Deletion Failed", errorMsg);
        }

        private void IATDeletionSuccess(String msg)
        {
            MessageBox.Show(msg, "Deletion Successful");
            DeleteButton.Enabled = true;
            Cancel.Enabled = true;
            this.Close();
        }

        private void DataDeletionSuccess(String msg)
        {
            MessageBox.Show(msg, "Deletion Successful");
            DeleteButton.Enabled = true;
            Cancel.Enabled = true;
            this.Close();
        }

        private void DeletionFailed(String errorMsg)
        {
            MessageBox.Show(errorMsg, "Deletion Failed");
            DeleteButton.Enabled = true;
            Cancel.Enabled = true;
            this.Close();
        }

        private DialogResult DisplayYesNoMsgBox(String text, String caption)
        {
            return MessageBox.Show(text, caption, MessageBoxButtons.YesNo);
        }

        private void DeletionCanceled(String msg)
        {
            this.Close();
        }

        private void DoDelete()
        {
            try
            {
                try
                {
                    MySOAP.BeginNewTransaction(TransactionProgress.ETransactionType.Deletion);
                    MySOAP.EstablishEncryption(Properties.Resources.sDataProviderServlet);
                    MySOAP.ShakeHands(Properties.Resources.sDataProviderServlet, IATName.Text);
                    TransactionRequest trans = new TransactionRequest(TransactionRequest.ETransaction.IATExists, IATConfigMainForm.ServerPassword, IATName.Text, Properties.Resources.sDataProviderServlet);
                    TransactionRequest inTrans = new TransactionRequest();
                    MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.IATExists, trans, inTrans);
                    if (inTrans.Transaction != TransactionRequest.ETransaction.IATExists)
                    {
                        this.BeginInvoke(new DeletionResultHandler(DeletionFailed), Properties.Resources.sNoSuchIAT);
                        MySOAP.TerminateConnection(Properties.Resources.sDataProviderServlet);
                        return;
                    }
                    if (MySOAP.VerifyPassword(IATName.Text, CPartiallyEncryptedRSAKey.EKeyType.Admin, PasswordBox.Text) != TransactionRequest.ETransaction.TransactionSuccess)
                    {
                        this.BeginInvoke(new DeletionResultHandler(DeletionFailed), "The password you supplied is incorrect");
                        MySOAP.TerminateConnection(Properties.Resources.sDataProviderServlet);
                        return;
                    }
                    MySOAP.ESoapAction action = MySOAP.ESoapAction.DeleteIAT;
                    if (DeletionType == EDeletionType.IAT)
                    {
                        if (((DialogResult)this.Invoke(new MessagePrompt(DisplayYesNoMsgBox), Properties.Resources.sDeleteIATConfirmation, Properties.Resources.sDeleteIATConfirmationCaption)) != DialogResult.Yes)
                        {
                            this.BeginInvoke(new DeletionResultHandler(DeletionCanceled));
                            MySOAP.TerminateConnection(Properties.Resources.sDataProviderServlet);
                            return;
                        }
                        trans = new TransactionRequest(TransactionRequest.ETransaction.DeleteIAT, IATConfigMainForm.ServerPassword, IATName.Text, Properties.Resources.sDataProviderServlet);
                        action = MySOAP.ESoapAction.DeleteIAT;
                    }
                    else
                    {
                        if (((DialogResult)this.Invoke(new MessagePrompt(DisplayYesNoMsgBox), Properties.Resources.sDeleteIATDataConfirmation, Properties.Resources.sDeleteIATDataConfirmationCaption)) != DialogResult.Yes)
                        {
                            this.BeginInvoke(new DeletionResultHandler(DeletionCanceled), String.Empty);
                            MySOAP.TerminateConnection(Properties.Resources.sDataProviderServlet);
                            return;
                        }
                        trans = new TransactionRequest(TransactionRequest.ETransaction.DeleteIATData, IATConfigMainForm.ServerPassword, IATName.Text, Properties.Resources.sDataProviderServlet);
                        action = MySOAP.ESoapAction.DeleteIATData;
                    }
                    trans.IsLastTransaction = true;
                    MySOAP.CallSOAP(Properties.Resources.sDataProviderServlet, Convert.ToInt32(Properties.Resources.sDataProviderPort), action, trans, inTrans);
                    if (inTrans.Transaction == TransactionRequest.ETransaction.TransactionSuccess)
                    {
                        if (CRegistry.ContainsIAT(IATName.Text))
                            CRegistry.DeleteIAT(IATName.Text);
                        if (DeletionType == EDeletionType.IAT)
                            this.BeginInvoke(new DeletionResultHandler(IATDeletionSuccess), "The deletion of IAT was successful.");
                        else
                            this.BeginInvoke(new DeletionResultHandler(DataDeletionSuccess), "The deletion of your data was successful.");
                    }
                    else
                    {
                        if (DeletionType == EDeletionType.IAT)
                            this.BeginInvoke(new DeletionResultHandler(DeletionFailed), Properties.Resources.sDeleteIATFail);
                        else
                            this.BeginInvoke(new DeletionResultHandler(DeletionFailed), Properties.Resources.sDeleteIATDataFail);
                    }
                    MySOAP.EndTransaction();
                }
                catch (TimeoutException ex)
                {
                    this.BeginInvoke(new DeletionResultHandler(DeletionFailed), "An attempt to communicate with the server timed out. Please try again.");
                    MySOAP.EndTransaction();
                }
                catch (TransactionAbortedException)
                {
                    return;
                }
                catch (IOException ex)
                {
                    throw new CXmlSerializationException("Error", ex.Message, ex);
                }
                catch (CryptographicException ex)
                {
                    throw new CXmlSerializationException("Error", ex.Message, ex);
                }
                catch (ArgumentException ex)
                {
                    throw new CXmlSerializationException("Error", ex.Message, ex);
                }
                catch (WebException ex)
                {
                    throw new CXmlSerializationException("Communication Error", ex.Message, ex);
                }
            }
            catch (CXmlSerializationException ex)
            {
                if (ex.ErrorType == CXmlSerializationException.EErrorType.exception)
                {
                    ErrorReportDisplay errorDisplay = new ErrorReportDisplay(ex.Message, ex);
                    errorDisplay.ShowDialog();
                    return;
                }
                else if (ex.ErrorType == CXmlSerializationException.EErrorType.message)
                {
                    MessageBox.Show(ex.Message);
                    return;
                }
            }
        }

        private void DeleteButton_Click(object sender, EventArgs e)
        {
            DeleteButton.Enabled = false;
            Cancel.Enabled = false;
            ThreadStart proc = new ThreadStart(DoDelete);
            Thread th = new Thread(proc);
            th.Start();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
