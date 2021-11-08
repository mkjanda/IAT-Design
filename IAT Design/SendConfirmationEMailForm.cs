using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    partial class SendConfirmationEMailForm : Form
    {
        public SendConfirmationEMailForm()
        {
            InitializeComponent();
            EMailBox.Text = LocalStorage.Activation[LocalStorage.Field.UserEmail];
        }

        private void ResendVerification_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Another verification email will be sent to the email address provided in the Activation Form.  Click OK to send this request or click cancel if you wish to return to the Activation Form to edit your email address.", "Resend Verification EMail", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                EMailConfirmation resendConfirm = new EMailConfirmation();
                LocalStorage.Activation[LocalStorage.Field.UserEmail] = EMailBox.Text;
                String email = EMailBox.Text;
                Task.Run(() => { resendConfirm.ResendEMailVerification(); }).Wait();
                if (resendConfirm.FinalTransaction == null)
                    return;
                if (resendConfirm.FinalTransaction.Transaction == TransactionRequest.ETransaction.TransactionFail)
                    MessageBox.Show("An error occurred while requesting that your email verification message be resent.  It is likely that this is the result of data being corrupted either on your machine's registry " +
                        "or on the server.  If this problem persists, please contact us at admin@iatsoftware.net.", "Verification Message Not Sent", MessageBoxButtons.OK);
                else if (resendConfirm.FinalTransaction.Transaction == TransactionRequest.ETransaction.TransactionSuccess)
                {
                    LocalStorage.Activation[LocalStorage.Field.UserEmail] = email;
                    MessageBox.Show(String.Format("A new automated email has been sent to {0}. Please check this email address and click on the link provided to confirm your email address.  If you do not receive this email in the next five minutes, " +
                        "check your spambox and, if it isn't found there, click the Resend Verification EMail button.", email), "Product Activated", MessageBoxButtons.OK);
                }
                else if (resendConfirm.FinalTransaction.Transaction == TransactionRequest.ETransaction.EMailAlreadyVerified)
                {
                    LocalStorage.Activation[LocalStorage.Field.ActivationKey] = resendConfirm.FinalTransaction.StringValues["ActivationKey"];
                    LocalStorage.Activation[LocalStorage.Field.UserEmail] = email;
                    MessageBox.Show("Your email address has already been verified. Your program is ready for use.", "EMail Already Verified", MessageBoxButtons.OK);
                    DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void ResendEMailVerification_Complete(IAsyncResult aResult)
        {
            Func<String, bool> del = (Func<String, bool>)aResult.AsyncState;
            bool bResult = del.EndInvoke(aResult);
            if (bResult)
                MessageBox.Show("A confirmation email was sent to the address provided. If it is not present, please check your spambox.", "EMail Sent");
            else
                MessageBox.Show("The server was unable to send a new verification email. Please try again momentarily and, if this error reoccurs, contact us at admin@iatsoftware.net.", "Server Error");
        }

        private void HaveVerifiedButton_Click(object sender, EventArgs e)
        {
            LocalStorage.Activation[LocalStorage.Field.UserEmail] = EMailBox.Text;
            EMailConfirmation confirmation = new EMailConfirmation();
            Task<EMailConfirmation.EConfirmResult> t = Task<EMailConfirmation.EConfirmResult>.Run(() => { return confirmation.ConfirmEMailVerification(); });
            t.Wait();
            if (t.IsFaulted)
                return;
            else if (t.Result == EMailConfirmation.EConfirmResult.success)
            {
                DialogResult = DialogResult.OK;
                this.Close();
            }
            else
                MessageBox.Show("We have yet to receive your email verification.  If you have still not received an email from IAT Software instructing you on how to complete activation of your product, please contact us at admin@iatsoftware.net", "EMail Not Verified", MessageBoxButtons.OK);
        }
    }
}
