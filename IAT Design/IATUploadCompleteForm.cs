using System;
using System.Drawing.Printing;
using System.Security.Permissions;
using System.Windows.Forms;

[assembly: PermissionSetAttribute(SecurityAction.RequestMinimum, Name = "FullTrust")]

namespace IATClient
{
    partial class IATUploadCompleteForm : Form
    {
        private CIATSummary _Summary = null;

        public CIATSummary Summary
        {
            set
            {
                _Summary = value;
            }
        }

        public IATUploadCompleteForm()
        {
            InitializeComponent();
        }

        private void Print_Click(object sender, EventArgs e)
        {
            PrintDialog dlg = new PrintDialog();
            dlg.UseEXDialog = true;
            PrintDocument doc = new PrintDocument();
            doc.PrintPage += new PrintPageEventHandler(_Summary.PrintPage);
            dlg.AllowSomePages = false;
            dlg.ShowHelp = true;
            dlg.Document = doc;
            dlg.UseEXDialog = true;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            doc.Print();
            this.Close();
        }

        private void IATUploadCompleteForm_Load(object sender, EventArgs e)
        {
            IATLinkBox.Text = _Summary.IATLink;
        }

        private void No_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
