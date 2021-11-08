using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Text.RegularExpressions;

namespace IATClient
{
    partial class ServerInterface : Form
    {
        private CIAT theIAT;
        private CResultData resultData;

        public ServerInterface(CIAT iat)
        {
            InitializeComponent();
            theIAT = iat;
            resultData = null;
        }

        private bool TestPassword(String pass)
        {
            if ((pass.Length < 7) || (pass.Length > 20))
                return false;
            if (!Regex.IsMatch(pass, "^[A-Za-z0-9\\-_]+$"))
                return false;
            return true;
        }

        private void RetrieveData_Click(object sender, EventArgs e)
        {
            if ((ServerURL.Text == String.Empty) || (IATPassword.Text == String.Empty) || (IATName.Text == String.Empty))
            {
                MessageBox.Show(this, Properties.Resources.sDataRetrievalPanelFieldsIncomplete, Properties.Resources.sDataRetrievalPanelFieldsIncompleteCaption, MessageBoxButtons.OK);
                return;
            }
            if (!TestPassword(IATPassword.Text))
            {
                MessageBox.Show(this, Properties.Resources.sInvalidIATPassword, Properties.Resources.sInvalidIATPasswordCaption, MessageBoxButtons.OK);
                return;
            }
            CServerURL url = new CServerURL(ServerURL.Text, Properties.Resources.sDataProviderServlet);
            if (!url.IsValidServerURL)
            {
                MessageBox.Show(this, Properties.Resources.sInvalidServerURLCaption, Properties.Resources.sInvalidServerURL, MessageBoxButtons.OK);
                return;
            }
            resultData = new CResultData(url.FullURL, IATName.Text, IATPassword.Text);
            try
            {
                ProgressWindow progress = new ProgressWindow();
                resultData.RetrieveData(progress);
                progress.ShowDialog(this);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Error Retrieving Data", MessageBoxButtons.OK);
                resultData = null;
                ExportData.Enabled = false;
                DataFileFormatGroup.Enabled = false;
                return;
            }
            DataFileFormatGroup.Enabled = true;
            if ((ResultFileRadio.Checked || ResultFileWithoutHeaderRadio.Checked || IATRawItemGroupedRadio.Checked || IATRawTesteeGroupedRadio.Checked) 
                && (DelimitationList.SelectedIndex != -1))
                ExportData.Enabled = true;
        }

        private void Okay_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ResultFileRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (ResultFileRadio.Checked)
                if ((resultData != null) && (DelimitationList.SelectedIndex != -1))
                    ExportData.Enabled = true;
        }

        private void ResultFileWithoutHeaderRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (ResultFileWithoutHeaderRadio.Checked)
                if ((resultData != null) && (DelimitationList.SelectedIndex != -1))
                    ExportData.Enabled = true;
        }

        private void IATRawTesteeGroupedRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (IATRawTesteeGroupedRadio.Checked)
                if ((resultData != null) && (DelimitationList.SelectedIndex != -1))
                    ExportData.Enabled = true;
        }

        private void IATRawItemGroupedRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (IATRawItemGroupedRadio.Checked)
                if ((resultData != null) && (DelimitationList.SelectedIndex != -1))
                    ExportData.Enabled = true;
        }

        private void ExportData_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = Properties.Resources.sTextFileFilter;
            dlg.AddExtension = true;
            CResultData.EDelimitation delim;
            switch (DelimitationList.SelectedItem.ToString())
            {
                case "Comma":
                    delim = CResultData.EDelimitation.comma;
                    break;

                case "Space":
                    delim = CResultData.EDelimitation.space;
                    break;

                case "Tab":
                    delim = CResultData.EDelimitation.tab;
                    break;

                default:
                    return;
            }
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                if (IATRawTesteeGroupedRadio.Checked)
                    resultData.ExportToFile(dlg.FileName, CResultData.EOutputGrouping.groupedByTestee, delim);
                else if (IATRawItemGroupedRadio.Checked)
                    resultData.ExportToFile(dlg.FileName, CResultData.EOutputGrouping.groupedByItem, delim);
                else if (ResultFileRadio.Checked)
                    resultData.CreateResultFile(dlg.FileName, true, delim);
                else if (ResultFileWithoutHeaderRadio.Checked)
                    resultData.CreateResultFile(dlg.FileName, false, delim);
            }
        }

        private void DataRetrievalDialog_Load(object sender, EventArgs e)
        {
            ExportData.Enabled = false;
            UploadButton.Enabled = false;
            DataFileFormatGroup.Enabled = false;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = Properties.Resources.sPackageFileFilter;
            dlg.Title = Properties.Resources.sOpenPackageFileDlgTitle;
            if (dlg.ShowDialog(this) != DialogResult.OK)
                return;
            IATFile.Text = dlg.FileName;
            if (ServerURL.Text != String.Empty)
                UploadButton.Enabled = true;
        }

        private void UploadServerURL_TextChanged(object sender, EventArgs e)
        {
            if (IATFile.Text != String.Empty)
                UploadButton.Enabled = true;
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            ProgressWindow progress = new ProgressWindow();
            if ((ServerURL.Text == String.Empty) || (IATPassword.Text == String.Empty) || (IATFile.Text == String.Empty))
            {
                MessageBox.Show(this, Properties.Resources.sDataRetrievalPanelFieldsIncomplete, Properties.Resources.sDataRetrievalPanelFieldsIncompleteCaption, MessageBoxButtons.OK);
                return;
            }
            CServerURL url = new CServerURL(ServerURL.Text, Properties.Resources.sDataProviderServlet);
            if (!TestPassword(IATPassword.Text))
            {
                MessageBox.Show(this, Properties.Resources.sInvalidIATPassword, Properties.Resources.sInvalidIATPasswordCaption, MessageBoxButtons.OK);
                return;
            }
            if (!url.IsValidServerURL)
            {
                MessageBox.Show(this, Properties.Resources.sInvalidServerURL, Properties.Resources.sInvalidServerURLCaption, MessageBoxButtons.OK);
                return;
            }
            CIATUploader upload = new CIATUploader(IATFile.Text, url.FullURL, progress, IATPassword.Text);
            upload.DeployIAT();
            if (progress.ShowDialog(this) == DialogResult.Cancel)
                return;
            if (upload.Summary == null)
            {
                MessageBox.Show(this, Properties.Resources.sUploadDenied, Properties.Resources.sUploadDeniedCaption);
                return;
            }
            IATUploadCompleteForm uploadComplete = new IATUploadCompleteForm();
            uploadComplete.Summary = upload.Summary;
            uploadComplete.ShowDialog(this);
        }

        private void Delete_Click(object sender, EventArgs e)
        {
            CServerURL url = new CServerURL(ServerURL.Text, Properties.Resources.sDataProviderServlet);
            if (!url.IsValidServerURL)
            {
                MessageBox.Show(this, Properties.Resources.sInvalidServerURL, Properties.Resources.sInvalidServerURLCaption, MessageBoxButtons.OK);
                return;
            }
            if ((IATPassword.Text == String.Empty) || (IATDeleteName.Text == String.Empty))
            {
                MessageBox.Show(this, Properties.Resources.sDeleteIATFormIncomplete, Properties.Resources.sDeleteIATFormIncompleteCaption, MessageBoxButtons.OK);
                return;
            }
            if (!TestPassword(IATPassword.Text))
            {
                MessageBox.Show(this, Properties.Resources.sInvalidIATPassword, Properties.Resources.sInvalidIATPasswordCaption, MessageBoxButtons.OK);
                return;
            }
            if (MessageBox.Show(this, Properties.Resources.sDeleteIATConfirmation, Properties.Resources.sDeleteIATConfirmationCaption, MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            long sessID = MySOAP.EstablishEncryption(url.FullURL);
            TransactionRequest trans = new TransactionRequest();
            String msg = MySOAP.CreateSOAPEnvelope(trans);
            HandShake inHand = new HandShake();
            MySOAP.CallSOAP(url.FullURL, "RequestSOAPExchange", msg, inHand, sessID);
            HandShake outHand = HandShake.CreateResponse(inHand);
            msg = MySOAP.CreateSOAPEnvelope(outHand);
            MySOAP.CallSOAP(url.FullURL, "Handshake", msg, trans, sessID);
            MySOAP.VerifyIATPassword(url.FullURL, IATDeleteName.Text, IATPassword.Text, sessID);
            trans = new TransactionRequest(TransactionRequest.ETransaction.DeleteIAT, "alotofalliteration", IATDeleteName.Text, url.FullURL);
            msg = MySOAP.CreateSOAPEnvelope(trans);
            MySOAP.CallSOAP(url.FullURL, "TransactionRequest", msg, trans, sessID);
            if (trans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
            {
                MessageBox.Show(this, Properties.Resources.sDeleteIATFail, Properties.Resources.sDeleteIATFailCaption);
                return;
            }
            MessageBox.Show(this, Properties.Resources.sDeleteIATSuccess, Properties.Resources.sDeleteIATSuccessCaption);
        }

        private void DeleteData_Click(object sender, EventArgs e)
        {
            if ((ServerURL.Text == String.Empty) || (IATPassword.Text == String.Empty) || (IATName.Text == String.Empty))
            {
                MessageBox.Show(this, Properties.Resources.sDataRetrievalPanelFieldsIncomplete, Properties.Resources.sDataRetrievalPanelFieldsIncompleteCaption, MessageBoxButtons.OK);
                return;
            }

            CServerURL url = new CServerURL(ServerURL.Text, Properties.Resources.sDataProviderServlet);
            if (!url.IsValidServerURL)
            {
                MessageBox.Show(this, Properties.Resources.sInvalidServerURLCaption, Properties.Resources.sInvalidServerURL, MessageBoxButtons.OK);
                 return;
            }
            if (MessageBox.Show(this, Properties.Resources.sDeleteIATDataConfirmation, Properties.Resources.sDeleteIATConfirmationCaption, MessageBoxButtons.YesNo) == DialogResult.No)
                return;
            long sessID = MySOAP.EstablishEncryption(url.FullURL);
            TransactionRequest trans = new TransactionRequest();
            String msg = MySOAP.CreateSOAPEnvelope(trans);
            HandShake inHand = new HandShake();
            MySOAP.CallSOAP(url.FullURL, "RequestSOAPExchange", msg, inHand, sessID);
            HandShake outHand = HandShake.CreateResponse(inHand);
            msg = MySOAP.CreateSOAPEnvelope(outHand);
            MySOAP.CallSOAP(url.FullURL, "Handshake", msg, trans, sessID);
            MySOAP.VerifyIATPassword(url.FullURL, IATName.Text, IATPassword.Text, sessID);
            trans = new TransactionRequest(TransactionRequest.ETransaction.DeleteIATData, "alotofalliteration", IATName.Text, url.FullURL);
            msg = MySOAP.CreateSOAPEnvelope(trans);
            MySOAP.CallSOAP(url.FullURL, "TransactionRequest", msg, trans, sessID);
            if (trans.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
            {
                MessageBox.Show(this, Properties.Resources.sDeleteIATFail, Properties.Resources.sDeleteIATFailCaption);
                return;
            }
            MessageBox.Show(this, Properties.Resources.sDeleteIATSuccess, Properties.Resources.sDeleteIATSuccessCaption);
        }

        private void DelimitationList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if ((DelimitationList.SelectedIndex != -1) && ((ResultFileRadio.Checked) || (ResultFileWithoutHeaderRadio.Checked)
                || (IATRawItemGroupedRadio.Checked) || (IATRawTesteeGroupedRadio.Checked)))
                ExportData.Enabled = true;
        }

        private void RetrieveItemSlides_Click(object sender, EventArgs e)
        {
            FolderBrowserDialog dlg = new FolderBrowserDialog();
            dlg.Description = "Please select a location to store the item slides.";
            if ((ServerURL.Text == String.Empty) || (IATPassword.Text == String.Empty) || (IATName.Text == String.Empty))
            {
                MessageBox.Show(this, Properties.Resources.sDataRetrievalPanelFieldsIncomplete, Properties.Resources.sDataRetrievalPanelFieldsIncompleteCaption, MessageBoxButtons.OK);
                return;
            }
            if (!TestPassword(IATPassword.Text))
            {
                MessageBox.Show(this, Properties.Resources.sInvalidIATPassword, Properties.Resources.sInvalidIATPasswordCaption, MessageBoxButtons.OK);
                return;
            }
            CServerURL url = new CServerURL(ServerURL.Text, Properties.Resources.sDataProviderServlet);
            if (!url.IsValidServerURL)
            {
                MessageBox.Show(this, Properties.Resources.sInvalidServerURLCaption, Properties.Resources.sInvalidServerURL, MessageBoxButtons.OK);
                return;
            }
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                ProgressWindow progress = new ProgressWindow();
                CItemSlideRetriever slideRetriever = new CItemSlideRetriever(url,
                    IATName.Text, IATPassword.Text);
                slideRetriever.RetrieveItemSlides(dlg.SelectedPath, progress);
                progress.ShowDialog(this);
            }
        }
    }
}
