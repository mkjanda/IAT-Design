using IATClient.Messages;
using System;
using System.Drawing;
using System.Linq.Expressions;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    partial class ActivationDialog : Form
    {
        public String ActivationKey = String.Empty;
        public String ProductKeyText = String.Empty;
        public int UserID = -1;

        public enum EActivationStage
        {
            initializing = 0, establishingEncryption, handshaking, activating, activationSuccess, invalidProductCode, serverFailure, noActivationsRemaining, awaitingEMailConfirmation,
            accountFrozen, clientDeleted, communicationTimeout, webException, clear
        };

        public String[] ActivationCaptions = {
            "Activating Product . . .",
            "Establishing Encryption",
            "Handshaking",
            "Activating",
            "Activation Success",
            "Invalid Product Code",
            "Generic Server Failure",
            "No Activations Remaining",
            "Awaiting EMail Confirmation",
            "Account Frozen",
            "Account Deleted",
            "Communications Timeout",
            "Communications Error",
            ""  };

        public String[] ActivationMessages = {
            String.Empty,
            String.Empty,
            String.Empty,
            String.Empty,
            "Your product has been successfully unlocked and is now ready to use.",
            String.Empty,
            "A generic server error occurred during product activation.  If you are certain you have a valid product code and entered it correctly, please contact us at admin@iatsoftware.net",
            String.Empty,
            String.Empty,
            "Your account appears to have been frozen.  Please contact us at admin@iatsoftware.net for details.",
            "You no longer have an account with IAT Software",
            "This program timed out while attempting to communicate with the server.  Please make sure you are connected to the internet try again.  If problem persists, please contact us at admin@iatsoftware.net for details.",
            "An internet communication error occurred while communicating with the server.  Further details are unavialable.  If problem persists, please contact us at admin@iatsoftware.net for details.",
            String.Empty
        };

        private EActivationStage ActStage = EActivationStage.clear;
        public delegate void ActivationUpdateEventHandler(EActivationStage status);
        public delegate void ActivationExceptionHandler(Exception ex);
        public delegate void OperationCompleteHandler(TransactionRequest.ETransaction transResult);
        public delegate void EMailConfirmedHandler(bool bResult);
        public LocalStorage.EActivationStatus ProductActivated = LocalStorage.EActivationStatus.NotActivated;
        public ActivationConfirmation Confirmation = new ActivationConfirmation();

        public ActivationDialog()
        {
            this.Name = Properties.Resources.sActivationDialogName;
            InitializeComponent();
            TitleBox.Items.Add("Dr");
            TitleBox.Items.Add("Mr");
            TitleBox.Items.Add("Mrs");
            TitleBox.Items.Add("Ms");
            HaveVerifiedButton.Enabled = false;
            ResendVerificationButton.Enabled = false;
            StatusDetailsButton.Enabled = false;
            StatusDetailsButton.Click += new EventHandler(StatusDetailsButton_Click);
            ActivateMessage.Text = String.Empty;
            this.Shown += new EventHandler(ActivationDialog_Shown);
        }

        private bool OfflineErrorReport = false;
        public void ShowErrorReport(String caption, CReportableException ex)
        {
            this.BeginInvoke(new Action<String, CReportableException>((c, rex) =>
            {
                try
                {
                    WebClient uploader = new WebClient();
                    CClientException clientEx = new CClientException(caption, ex);
                    uploader.Headers.Add("Content-type: text/xml");
                    if (!OfflineErrorReport)
                    {
                        if (Encoding.UTF8.GetString(uploader.UploadData(Properties.Resources.sErrorReportURL, clientEx.GetXmlBytes())) == "success")
                        {
                            MessageBox.Show(String.Format(Properties.Resources.sErrorReportedMessage, LocalStorage.Activation[LocalStorage.Field.ProductKey]), Properties.Resources.sErrorReportedCaption);
                            return;
                        }
                    }
                }
                catch (Exception e) { }
                OfflineErrorReport = true;
                ErrorReportDisplay f = new ErrorReportDisplay(ex);
                f.ShowDialog();
            }), caption, ex);
        }

        void StatusDetailsButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show(ActivationMessages[(int)ActStage], ActivationCaptions[(int)ActStage]);
        }

        private void TransactionAborted(String errorMsg)
        {
            ActivateMessage.Text = errorMsg;
        }

        void ActivationDialog_Shown(object sender, EventArgs e)
        {
            ActivateMessage.Text = String.Empty;
            ActivateMessage.ForeColor = Color.FromArgb(192, 19, 82);
        }

        private void Activate_Click(object sender, EventArgs e)
        {
            try { 
            if (ProductKey.Text.Length != 20)
            {
                ActivateMessage.Text = "Product key should be 20 characters in length";
                return;
            }
            if (FName.Text.Length == 0)
            {
                ActivateMessage.Text = "Please enter your first name";
                return;
            }
            if (LName.Text.Length == 0)
            {
                ActivateMessage.Text = "Please enter your last name";
                return;
            }
            if (EMail.Text.Length > 0)
            {
                if (!Regex.IsMatch(EMail.Text, ".+@.+\\..+"))
                {
                    ActivateMessage.Text = "Please enter a valid email address";
                    return;
                }
            }
            else
            {
                ActivateMessage.Text = "Please enter a valid email address";
                return;
            }
            if (TitleBox.Text == String.Empty)
            {
                ActivateMessage.Text = "Please select a title.";
                return;
            }
            ProductKey.Enabled = false;
            FName.Enabled = false;
            LName.Enabled = false;
            TitleBox.Enabled = false;
            EMail.Enabled = false;
            ResendVerificationButton.Enabled = false;
            HaveVerifiedButton.Enabled = false;
            ActivateButton.Enabled = false;
            ActivationObject actObj = new ActivationObject(this);
            String fName = FName.Text, lName = LName.Text, email = EMail.Text, title = TitleBox.Text, productKey = ProductKey.Text;
            Task.Run(() =>
            {
                actObj.Activate(fName, lName, email, title, productKey);
                if (actObj.ActivationResult == ActivationResponse.EResult.Success)
                {
                    this.Invoke((Action)(() =>
                    {
                        ResendVerificationButton.Enabled = true;
                        HaveVerifiedButton.Enabled = true;
                        EMail.Enabled = true;
                        MessageBox.Show(String.Format("Your program has been successfully activated.  Before you may use it, you must confirm your email address. An automated email has been sent to {0}. " +
                            "Please check this email address and click on the link provided to confirm your email address.  If you do not receive this email in the next five minutes, " +
                            "check your spambox and, if it isn't found there, click the Resend Verification EMail button.", EMail.Text), "Product Activated", MessageBoxButtons.OK);
                    }));
                }
                else
                {
                    this.Invoke((Action)(() =>
                    {
                        ProductKey.Enabled = true;
                        FName.Enabled = true;
                        LName.Enabled = true;
                        TitleBox.Enabled = true;
                        EMail.Enabled = true;
                        ResendVerificationButton.Enabled = false;
                        HaveVerifiedButton.Enabled = false;
                        ActivateButton.Enabled = true;
                    }));
                }
                if (actObj.ActivationResult == ActivationResponse.EResult.InvalidProductCode)
                {
                    this.Invoke((Action)(() =>
                    {
                        ActivateMessage.Text = "Invalid Product Code";
                    }));
                }
                else if (actObj.ActivationResult == ActivationResponse.EResult.NoActivationsRemaining)
                {
                    MessageBox.Show("No Activations Remain", "You have met your alotted activations for the software. If you require more, write me and tell me why at michael@iatsoftware.net");
                }
                else if (actObj.ActivationResult == ActivationResponse.EResult.EmailAlreadyVerified)
                {
                    ProductActivated = LocalStorage.EActivationStatus.Activated;
                    this.Invoke((Action)Close);
                }
            }).ContinueWith((t) =>
            {
                if (t.IsFaulted)
                {
                    ErrorReporter.ReportActivationError(t.Exception.InnerException);
                    this.Invoke(new Action(() => Close()));
                }
            });
        }
            catch (Exception ex)
            {
                ErrorReporter.ReportActivationError(ex);
            }
        }

        private void ActivationComplete(IAsyncResult aResult)
        {
        }

        private void ActivationFailed(ActivationResponse.EResult result)
        {
            if (result == ActivationResponse.EResult.InvalidProductCode)
            {
                MessageBox.Show("The product key you entered is invalid.", "Activation Failed");
            }
            else if (result == ActivationResponse.EResult.NoActivationsRemaining)
            {
                MessageBox.Show("You have consumed all your allotted activations of the software. Please contact us at admin@iatsoftware.net to acquire more.", "Activation Failed");
            }
            else if (result == ActivationResponse.EResult.ClientFrozen)
            {
                MessageBox.Show("Your account has been frozen. Any data you have collected remains on the server. Contact us at admin@iatsoftware.net for details.", "Account Frozen");
            }
            else if (result == ActivationResponse.EResult.ClientDeleted)
            {
                MessageBox.Show("You no longer have an account with IAT Software.", "Account Deleted");
            }
            else if (result == ActivationResponse.EResult.ServerFailure)
            {
                MessageBox.Show("The server failed to complete the activation process. Please contact us at admin@iatsoftware.net", "Activation Failed");
            }
            else if (result == ActivationResponse.EResult.CannotConnect)
            {
                MessageBox.Show("The program cannot communicate with the server. Please check your internet connection and try again. If this problem persists, contact us at admin@iatsoftware.net", "Activation Failed");
            }
        }

        private void HaveVerifiedButton_Click(object sender, EventArgs e)
        {
            HaveVerifiedButton.Enabled = false;
            ResendVerificationButton.Enabled = false;
            EMail.Enabled = false;
            FName.Enabled = false;
            LName.Enabled = false;
            TitleBox.Enabled = false;
            LocalStorage.Activation[LocalStorage.Field.UserEmail] = EMail.Text;
            EMailConfirmation confirm = new EMailConfirmation();
            Task.Run(() =>
            {
                EMailConfirmation.EConfirmResult confirmResult = confirm.ConfirmEMailVerification();
                this.Invoke((Action<EMailConfirmation.EConfirmResult>)OnEMailConfirmed, confirmResult);
            });
        }

        private void OnEMailConfirmed(EMailConfirmation.EConfirmResult result)
        {
            ResendVerificationButton.Enabled = true;
            HaveVerifiedButton.Enabled = true;
            if (result == EMailConfirmation.EConfirmResult.success)
            {
                ProductActivated = LocalStorage.EActivationStatus.Activated;
                DialogResult = DialogResult.OK;
                this.Invoke((Action)Close);
            }
            else
            {
                TitleBox.Enabled = true;
                FName.Enabled = true;
                LName.Enabled = true;
                EMail.Enabled = true;
                HaveVerifiedButton.Enabled = true;
                ResendVerificationButton.Enabled = true;
                switch (result)
                {
                    case EMailConfirmation.EConfirmResult.failed:
                        MessageBox.Show(Properties.Resources.sEmailConfirmationFailed, Properties.Resources.sEmailConfirmationFailedCaption, MessageBoxButtons.OK);
                        break;

                    case EMailConfirmation.EConfirmResult.emailMismatch:
                        MessageBox.Show(Properties.Resources.sEmailConfirmationFailedEmailMismatch, Properties.Resources.sEmailConfirmationFailedCaption, MessageBoxButtons.OK);
                        break;

                    case EMailConfirmation.EConfirmResult.noSuchClient:
                        MessageBox.Show(Properties.Resources.sEmailConfirmationFailedNoSuchClient, Properties.Resources.sEmailConfirmationFailedCaption, MessageBoxButtons.OK);
                        break;
                }
            }
        }

        private void ResentEMailOpComplete(TransactionRequest.ETransaction resultTrans)
        {
        }

        private void ResendVerificationButton_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("Another verification email will be sent to the email address provided in the Activation Form.  Click OK to send this request or click cancel if you wish to return to the Activation Form to edit your email address.", "Resend Verification EMail", MessageBoxButtons.OKCancel) == DialogResult.OK)
            {
                EMailConfirmation resendConfirm = new EMailConfirmation();
                EMail.Enabled = false;
                LocalStorage.Activation[LocalStorage.Field.UserEmail] = EMail.Text;
                Task.Run(() => { resendConfirm.ResendEMailVerification(); }).Wait();
                EMail.Enabled = true;
                if (resendConfirm.FinalTransaction == null)
                {
                    LocalStorage.Activation[LocalStorage.Field.UserEmail] = String.Empty;
                    return;
                }
                if (resendConfirm.FinalTransaction.Transaction == TransactionRequest.ETransaction.TransactionFail)
                {
                    LocalStorage.Activation[LocalStorage.Field.UserEmail] = String.Empty;
                    MessageBox.Show("An error occurred while requesting that your email verification message be resent.  It is likely that this is the result of data being corrupted either on your machine's registry " +
                        "or on the server.  If this problem persists, please contact me at michael@iatsoftware.net.", "Verification Message Not Sent", MessageBoxButtons.OK);
                }
                else if (resendConfirm.FinalTransaction.Transaction == TransactionRequest.ETransaction.TransactionSuccess)
                {
                    MessageBox.Show(String.Format("A new automated email has been sent to {0}. Please check this email address and click on the link provided to confirm your email address.  If you do not receive this email in the next five minutes, " +
                        "check your spambox and, if it isn't found there, click the Resend Verification EMail button.", EMail.Text), "Product Activated", MessageBoxButtons.OK);
                    LocalStorage.Activation[LocalStorage.Field.UserEmail] = EMail.Text;
                }
                else if (resendConfirm.FinalTransaction.Transaction == TransactionRequest.ETransaction.EMailAlreadyVerified)
                {
                    LocalStorage.Activation[LocalStorage.Field.ActivationKey] = resendConfirm.FinalTransaction.StringValues["ActivationKey"];
                    MessageBox.Show("Your email address has already been verified. Your program is ready for use.", "EMail Already Verified", MessageBoxButtons.OK);
                    ProductActivated = LocalStorage.EActivationStatus.Activated;
                    this.Close();
                }
            }
        }
    }
}
