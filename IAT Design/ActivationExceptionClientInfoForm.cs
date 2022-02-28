using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Forms;

namespace IATClient
{
    public partial class ActivationExceptionClientInfoForm : Form
    {

        public ActivationExceptionClientInfoForm(Exception ex)
        {
            InitializeComponent();
            while (ex != null)
            {
                ExceptionBox.Text += ex.Message + "\n";
                ExceptionBox.Text += "\t" + ex.StackTrace.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries).ToList().Aggregate((s1, s2) => s1 + "\t\r\n" + s2) + "\n";
                ex = ex.InnerException;
            }
            SubmitButton.Click += (sender, args) =>
            {
                Regex emailRegex = new Regex(@".+@.+\..+"), productKeyRegex = new Regex(@"[A-Z0-9]{20}");
                if (!emailRegex.IsMatch(EmailBox.Text.Trim()) && (EmailBox.Text.Trim() != String.Empty))
                {
                    System.Windows.Forms.MessageBox.Show("Invalid Email", "");
                    return;
                }
                if (!productKeyRegex.IsMatch(ProductKeyBox.Text.Trim()) && (ProductKeyBox.Text.Trim() != String.Empty))
                {
                    System.Windows.Forms.MessageBox.Show("Invalid Product Key");
                    return;
                }
                if ((EmailBox.Text.Trim() == String.Empty) && (ProductKeyBox.Text.Trim() == String.Empty))
                {
                    System.Windows.Forms.MessageBox.Show("Please enter a product key or email address.");
                    return;
                }
                DialogResult = DialogResult.OK;
                Close();
            };
            DeclineButton.Click += (sender, args) =>
            {
                DialogResult = DialogResult.Cancel;
                Close();
            };
        }

        public String Email
        {
            get
            {
                return EmailBox.Text.Trim();
            }
        }

        public String ProductKey
        {
            get
            {
                return ProductKeyBox.Text.Trim();
            }
        }
    }
}
