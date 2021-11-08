using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Threading;


namespace IATClient
{
    public class ServerInterfacePanel : UserControl
    {
        private enum ECurrentOperation { retrieveData, deleteIAT, deleteIATData, none };
        private ECurrentOperation CurrentOperation = ECurrentOperation.none;
        private Panel IATPanel = new Panel();
        private Button DeleteIATButton, RetrieveResultsButton, DeleteIATDataButton, ExportDataButton, ExportItemSlidesButton, ClearButton, CloseButton;
        private TextBox PasswordBox;
        private Label PasswordBoxLabel;
        private Dictionary<String, Label> ClientLabels = new Dictionary<String, Label>();
        private Dictionary<String, Label> TestLabels = new Dictionary<String, Label>();
        private String[] ClientLabelNames = { "Registered to", "Organization", "# of IATs alotted", "Total administrations", "Administrations remaining", "Disk alottment(MB)", 
                                                "Disk space remaining(MB)" };
        private String[] TestLabelNames = { "IAT name", "Author", "Author eMail", "Last data retrieval", "Test size(KB)", "Administrations", "# of result sets" };
        private Padding PanelPadding = new Padding(20, 30, 20, 10);
        private CIATManager IATManager = new CIATManager();
        private Label SelectedIATLabel = null;
        private bool _ItemSlidesRetrieved = false;
        public CItemSlideContainer ItemSlideContainer = null;
        private static int ControlWidth = (int)(250 * CIATLayout.XScale);
        private object lockObject = new object();

        public bool ItemSlidesRetrieved
        {
            get
            {
                return _ItemSlidesRetrieved;
            }
            set
            {
                _ItemSlidesRetrieved = value;
            }
        }

        public ServerInterfacePanel()
        {
            ControlWidth = (int)(ControlWidth * CIATLayout.XScale);
            int fontHeight = (int)System.Drawing.SystemFonts.DialogFont.GetHeight(ImageManager.CImageManager.ScreenDPI.Height);
            for (int ctr = 0; ctr < ClientLabelNames.Length; ctr++)
            {
                ClientLabels[ClientLabelNames[ctr]] = new Label();
                ClientLabels[ClientLabelNames[ctr]].Size = new Size(ControlWidth - PanelPadding.Horizontal, (int)(fontHeight * 1.5));
                ClientLabels[ClientLabelNames[ctr]].TextAlign = ContentAlignment.MiddleLeft;
                ClientLabels[ClientLabelNames[ctr]].Location = new Point(PanelPadding.Left, PanelPadding.Top + (ctr * (int)(fontHeight * 1.5)));
                Controls.Add(ClientLabels[ClientLabelNames[ctr]]);
            }
            IATPanel.BackColor = Color.White;
            IATPanel.Size = new Size(ControlWidth - PanelPadding.Horizontal, 5 * (int)(fontHeight * 1.5));
            IATPanel.Location = new Point(PanelPadding.Left, ClientLabels[ClientLabelNames[ClientLabelNames.Length - 1]].Bottom + (int)fontHeight);
            IATPanel.AutoScroll = true;
            IATPanel.HorizontalScroll.Enabled = false;
            IATPanel.Click += new EventHandler(IATPanel_Click);
            IATPanel.BorderStyle = BorderStyle.Fixed3D;
            Controls.Add(IATPanel);
            for (int ctr = 0; ctr < TestLabelNames.Length; ctr++)
            {
                TestLabels[TestLabelNames[ctr]] = new Label();
                TestLabels[TestLabelNames[ctr]].Size = new Size(ControlWidth - PanelPadding.Horizontal, (int)(1.5 * fontHeight));
                TestLabels[TestLabelNames[ctr]].TextAlign = ContentAlignment.MiddleLeft;
                TestLabels[TestLabelNames[ctr]].Location = new Point(PanelPadding.Left, IATPanel.Bottom + (ctr * (int)(fontHeight * 1.5)) + (int)fontHeight);
                Controls.Add(TestLabels[TestLabelNames[ctr]]);
            }
            PasswordBoxLabel = new Label();
            PasswordBoxLabel.Text = "Password:  ";
            PasswordBox = new TextBox();
            Size szLabel = TextRenderer.MeasureText(PasswordBoxLabel.Text, PasswordBoxLabel.Font);
            PasswordBox.Size = new Size(ControlWidth - szLabel.Width - PanelPadding.Horizontal, PasswordBox.Height);
            PasswordBox.Location = new Point(szLabel.Width + (int)fontHeight, TestLabels[TestLabelNames[TestLabelNames.Length - 1]].Bottom + (int)(fontHeight * 1.5));
            PasswordBox.Enabled = false;
            Controls.Add(PasswordBox);
            PasswordBoxLabel.Size = new Size(szLabel.Width, PasswordBox.Height);
            PasswordBoxLabel.TextAlign = ContentAlignment.MiddleLeft;
            PasswordBoxLabel.Location = new Point(PanelPadding.Left, PasswordBox.Bottom - PasswordBoxLabel.Height);
            Controls.Add(PasswordBoxLabel);
            RetrieveResultsButton = new Button();
            RetrieveResultsButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, RetrieveResultsButton.Height);
            RetrieveResultsButton.Text = "Retrieve IAT Results";
            RetrieveResultsButton.Location = new Point(PanelPadding.Left, PasswordBox.Bottom + fontHeight);
            RetrieveResultsButton.Enabled = false;
            RetrieveResultsButton.Click += new EventHandler(RetrieveResultsButton_Click);
            Controls.Add(RetrieveResultsButton);
            DeleteIATButton = new Button();
            DeleteIATButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, DeleteIATButton.Height);
            DeleteIATButton.Text = "Delete IAT";
            DeleteIATButton.Location = new Point(PanelPadding.Left, RetrieveResultsButton.Bottom + fontHeight);
            DeleteIATButton.Enabled = false;
            DeleteIATButton.Click += new EventHandler(DeleteIATButton_Click);
            Controls.Add(DeleteIATButton);
            DeleteIATDataButton = new Button();
            DeleteIATDataButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, DeleteIATDataButton.Height);
            DeleteIATDataButton.Text = "Delete IAT Data";
            DeleteIATDataButton.Location = new Point(PanelPadding.Left, DeleteIATButton.Bottom + fontHeight);
            DeleteIATDataButton.Enabled = false;
            DeleteIATDataButton.Click += new EventHandler(DeleteIATDataButton_Click);
            Controls.Add(DeleteIATDataButton);
            ExportDataButton = new Button();
            ExportDataButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, ExportDataButton.Height);
            ExportDataButton.Text = "Export Data";
            ExportDataButton.Location = new Point(PanelPadding.Left, DeleteIATDataButton.Bottom + fontHeight);
            ExportDataButton.Enabled = false;
            ExportDataButton.Click += new EventHandler(ExportDataButton_Click);
            Controls.Add(ExportDataButton);
            ExportItemSlidesButton = new Button();
            ExportItemSlidesButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, ExportItemSlidesButton.Height);
            ExportItemSlidesButton.Text = "Export Item Slides";
            ExportItemSlidesButton.Location = new Point(PanelPadding.Left, ExportDataButton.Bottom + fontHeight);
            ExportItemSlidesButton.Enabled = false;
            ExportItemSlidesButton.Click += new EventHandler(ExportItemSlidesButton_Click);
            Controls.Add(ExportItemSlidesButton);
            ClearButton = new Button();
            ClearButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, ClearButton.Height);
            ClearButton.Text = "Clear";
            ClearButton.Location = new Point(PanelPadding.Left, ExportItemSlidesButton.Bottom + fontHeight);
            ClearButton.Enabled = false;
            ClearButton.Click += new EventHandler(ClearButton_Click);
            Controls.Add(ClearButton);
            CloseButton = new Button();
            CloseButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, CloseButton.Height);
            CloseButton.Text = "Close";
            CloseButton.Enabled = true;
            CloseButton.Location = new Point(PanelPadding.Left, ClearButton.Bottom + fontHeight);
            CloseButton.Click += new EventHandler(CloseButton_Click);
            Controls.Add(CloseButton);
            this.Size = new Size(ControlWidth, CloseButton.Bottom + PanelPadding.Bottom);
        }

        private void ExportDataButton_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Excel File|*.xlsx";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                List<Image> ItemSlides = ItemSlideContainer.GetSlideImages();
                CResultDocument rDoc = new CResultDocument(IATManager.GetResultData(SelectedIATLabel.Text), ItemSlides);
                rDoc.WriteToFile(dlg.FileName);
            }
        }

        private void ExportItemSlidesButton_Click(object sender, EventArgs e)
        {
            if (ItemSlideContainer == null)
                return;
            FolderBrowserDialog fbr = new FolderBrowserDialog();
            fbr.ShowNewFolderButton = true;
            if (fbr.ShowDialog() == DialogResult.OK)
            {
                ItemSlideContainer.SaveItemSlides(fbr.SelectedPath, SelectedIATLabel.Text);
            }
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {

        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            mainForm.HideServerInterfacePanel();
        }

        public void Initialize()
        {
            Func<CServerReport> retrieveServerReport = IATManager.RetrieveServerReport;
            IAsyncResult async = retrieveServerReport.BeginInvoke(new AsyncCallback(RetrieveServerReport_Done), retrieveServerReport);
        }

        private void RetrieveServerReport_Done(IAsyncResult async)
        {
            Func<CServerReport> endDel = (Func<CServerReport>)(async.AsyncState);
            IATManager.ServerReport = endDel.EndInvoke(async);
            int fontHeight = (int)System.Drawing.SystemFonts.DialogFont.GetHeight(ImageManager.CImageManager.ScreenDPI.Height);
            if (IATManager.ServerReport != null)
            {
                Action popIATPanel = this.PopulateIATPanel;
                this.Invoke(popIATPanel);
            }
        }

        private void PopulateIATPanel()
        {
            ClientLabels["Registered to"].Text = String.Format("Registered to: {0} {1}", IATManager.ServerReport.ContactFName, IATManager.ServerReport.ContactLName);
            ClientLabels["Organization"].Text = String.Format("Organization: {0}", IATManager.ServerReport.Organization);
            if (IATManager.ServerReport.NumIATsAlotted == -1)
                ClientLabels["# of IATs alotted"].Text = "# of IATs alotted: Unlimited";
            else
                ClientLabels["# of IATs alotted"].Text = String.Format("# of IATs alotted: {0}", IATManager.ServerReport.NumIATsAlotted);
            ClientLabels["Total administrations"].Text = String.Format("Total administrations: {0}", IATManager.ServerReport.NumAdministrations);
            if (IATManager.ServerReport.NumAdministrationsRemaining == -1)
                ClientLabels["Administrations remaining"].Text = String.Format("Administrations remaining: Unlimited");
            else 
                ClientLabels["Administrations remaining"].Text = String.Format("Administrations remaining: {0}", IATManager.ServerReport.NumAdministrationsRemaining);
            bool bUnlimitedDiskAlottments = (Convert.ToInt32(IATManager.ServerReport.DiskAlottmentMB) == -1) ? true : false;
            if (bUnlimitedDiskAlottments)
            {
                ClientLabels["Disk alottment(MB)"].Text = "Disk Alottment(MB): Unlimited";
                ClientLabels["Disk space remaining(MB)"].Text = "Disk space remaining(MB): Unlimited";
            }
            else
            {
                ClientLabels["Disk alottment(MB)"].Text = String.Format("Disk alottment(MB): {0}", IATManager.ServerReport.DiskAlottmentMB);
                ClientLabels["Disk space remaining(MB)"].Text = String.Format("Disk space remaining(MB): {0}", IATManager.ServerReport.DiskAlottmentRemainingMB);
            }
            ConstructIATPanel();
            PopulateTestLabels(null);
        }

        private void ConstructIATPanel()
        {
            int fontHeight = (int)System.Drawing.SystemFonts.DialogFont.GetHeight(ImageManager.CImageManager.ScreenDPI.Height);
            SelectedIATLabel = null;
            IATPanel.Controls.Clear();
            List<Label> labelList = new List<Label>();
            int ctr = 0;
            foreach (String iatName in IATManager.ServerReport.IATReports.Keys)
            {
                Label l = new Label();
                l.Location = new Point(0, ctr * (int)(fontHeight * 1.25));
                l.Padding = new Padding(3, 0, 0, 0);
                l.TextAlign = ContentAlignment.MiddleLeft;
                l.Text = iatName;
                l.BackColor = Color.White;
                l.Size = new Size(IATPanel.Width, (int)(fontHeight * 1.25));
                if (IATManager.GetIATPasswordFromRegistry(iatName) == null)
                    l.ForeColor = Color.Black;
                else
                    l.ForeColor = Color.DarkGreen;
                l.Click += new EventHandler(IATName_Click);
                labelList.Add(l);
                IATPanel.Controls.Add(l);
                ctr++;
            }
            foreach (Label l in labelList)
                l.Size = new Size(IATPanel.ClientRectangle.Width, (int)(fontHeight * 1.25));
        }            

        private void PopulateTestLabels(CIATReport rep) 
        {
            TestLabels["Author"].Text = (rep == null) ? "Author: " : String.Format("Author: {0} {1} {2}", rep.AuthorTitle, rep.AuthorFName, rep.AuthorLName);
            TestLabels["Author eMail"].Text = (rep == null) ? "Author eMail: " : String.Format("Author eMail: {0}", rep.AuthorEMail);
            TestLabels["Last data retrieval"].Text = (rep == null) ? "Last data retrieval: " : String.Format("Last data retrieval: {0}", rep.LastDataRetrieval);
            TestLabels["IAT name"].Text = (rep == null) ? "IAT name: " : String.Format("IAT name: {0}", rep.IATName);
            TestLabels["Test size(KB)"].Text = (rep == null) ? "Test size(KB): " : String.Format("Test size(KB): {0}", rep.TestSizeKB);
            TestLabels["Administrations"].Text = (rep == null) ? "Administrations: " : String.Format("Administrations: {0}", rep.NumAdministrations);
            TestLabels["# of result sets"].Text = (rep == null) ? "# of result sets: " : String.Format("# of result sets: {0}", rep.NumResultSets);
        }

        private void IATPanel_Click(object sender, EventArgs e)
        {
            if (SelectedIATLabel != null)
                SelectedIATLabel.BackColor = Color.White;
            SelectedIATLabel = null;
            RetrieveResultsButton.Enabled = false;
            DeleteIATDataButton.Enabled = false;
            DeleteIATButton.Enabled = false;
            PasswordBox.Text = String.Empty;
            PasswordBox.Enabled = false;
        }

        private void IATName_Click(object sender, EventArgs e)
        {
            if (SelectedIATLabel == (Label)sender)
                return;
            if (SelectedIATLabel != null)
                SelectedIATLabel.BackColor = Color.White;
            SelectedIATLabel = (Label)sender;
            PopulateTestLabels(IATManager.ServerReport.IATReports[SelectedIATLabel.Text]);
            SelectedIATLabel.BackColor = Color.CornflowerBlue;
            RetrieveResultsButton.Enabled = true;
            DeleteIATDataButton.Enabled = true;
            DeleteIATButton.Enabled = true;
            PasswordBox.Text = String.Empty;
            if (IATManager.GetIATPasswordFromRegistry(SelectedIATLabel.Text) == null)
                PasswordBox.Enabled = true;
            else
            {
                PasswordBox.Text = "Stored in registry";
                PasswordBox.Enabled = false;
            }
        }

        private void DisableControlsForServerTransaction()
        {
            IEnumerator e = Controls.GetEnumerator();
            while (e.MoveNext())
                ((Control)e.Current).Enabled = false;
        }

        private void EnableControls()
        {
            foreach (Label l in ClientLabels.Values)
                l.Enabled = true;
            foreach (Label l in TestLabels.Values)
                l.Enabled = true;
            if (IATManager.GetIATPasswordFromRegistry(SelectedIATLabel.Text) == null)
                PasswordBox.Enabled = true;
            else
            {
                PasswordBox.Text = "Stored in registry";
                PasswordBox.Enabled = false;
            }
            PasswordBoxLabel.Enabled = true;
            IATPanel.Enabled = true;
            if (SelectedIATLabel != null)
            {
                DeleteIATButton.Enabled = true;
                RetrieveResultsButton.Enabled = true;
                DeleteIATDataButton.Enabled = true;
            }
            ExportDataButton.Enabled = true;
        }

        private void RetrieveResultsButton_Click(object sender, EventArgs e)
        {
            String password;
            if ((password = IATManager.GetIATPasswordFromRegistry(SelectedIATLabel.Text)) == null) {
            if (PasswordBox.Text == String.Empty)
            {
                MessageBox.Show("Please enter the data retrieval password for this IAT in the Password box.", "No Password Supplied");
                return;
            } else
                password = PasswordBox.Text;
            }  
        
            String iatName = SelectedIATLabel.Text;
            AsyncCallback resultDataRetrieved = new AsyncCallback(RetrieveResultsComplete);
            Func<String, String, CResultData> retrieveResults = IATManager.RetrieveResults;
            retrieveResults.BeginInvoke(iatName, password, resultDataRetrieved, retrieveResults);
            DisableControlsForServerTransaction();
        }

        private void RetrieveResultsComplete(IAsyncResult async)
        {
            Func<String, String, CResultData> retrieveResultsDelegate = (Func<String, String, CResultData>)async.AsyncState;
            CResultData results = retrieveResultsDelegate.EndInvoke(async);
            if (results == null)
                return;
//            DialogResult result = MessageBox.Show(Properties.Resources.sResultDataFileHeaderYesNo, Properties.Resources.sResultDataFileHeaderYesNoCaption, MessageBoxButtons.YesNoCancel);
  //          if (result == DialogResult.Cancel)
    //            return;
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            Delegate del = (Action<CResultData>)mainForm.ShowResultsPanel;
            mainForm.Invoke(del, results);
            String password;
            if ((password = IATManager.GetIATPasswordFromRegistry(SelectedIATLabel.Text)) == null) 
                password = PasswordBox.Text;
            mainForm.BeginRetrieveItemSlides(SelectedIATLabel.Text, password);
        }

        private void ShowResultData()
        {/*
            DialogResult result;
            SaveFileDialog fileSave = new SaveFileDialog();
            fileSave.AddExtension = true;
            fileSave.DefaultExt = "txt";
            fileSave.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (fileSave.ShowDialog() != DialogResult.Cancel)
                results.CreateResultFile(fileSave.FileName, (result == DialogResult.Yes) ? true : false, CResultData.EDelimitation.comma);
            if (MessageBox.Show(Properties.Resources.sResultsGroupedByItemYesNo, Properties.Resources.sResultsGroupedByItemYesNoCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                fileSave = new SaveFileDialog();
                fileSave.AddExtension = true;
                fileSave.DefaultExt = "txt";
                fileSave.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                if (fileSave.ShowDialog() != DialogResult.Cancel)
                    results.ExportToFile(fileSave.FileName, CResultData.EOutputGrouping.groupedByItem, CResultData.EDelimitation.comma);
            }
            if (MessageBox.Show(Properties.Resources.sResultsGroupedByTesteeYesNo, Properties.Resources.sResultsGroupedByTesteeYesNoCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                fileSave = new SaveFileDialog();
                fileSave.AddExtension = true;
                fileSave.DefaultExt = "txt";
                fileSave.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                if (fileSave.ShowDialog() != DialogResult.Cancel)
                    results.ExportToFile(fileSave.FileName, CResultData.EOutputGrouping.groupedByTestee, CResultData.EDelimitation.comma);
            }*/
        }

        private void DeleteIATButton_Click(object sender, EventArgs e)
        {
            String iatName = SelectedIATLabel.Text;
            String password;
            if ((password = IATManager.GetIATPasswordFromRegistry(SelectedIATLabel.Text)) == null)
            {
                if (PasswordBox.Text == String.Empty)
                {
                    MessageBox.Show("Please enter the data retrieval password for this IAT in the Password box.", "No Password Supplied");
                    return;
                }
                password = PasswordBox.Text;
            }
            if (MessageBox.Show("This will permanently remove your IAT and all data collected with it from the server. Are you sure you wish to proceed?", "Confirm IAT Deletion", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            AsyncCallback iatDeleted = new AsyncCallback(IATDeletionComplete);
            Func<String, String, bool> deleteIAT = IATManager.DeleteIAT;
            IAsyncResult async = deleteIAT.BeginInvoke(iatName, password, iatDeleted, deleteIAT);
            DisableControlsForServerTransaction();
        }

        private void IATDeletionComplete(IAsyncResult aResult)
        {
            EnableControlsAsync();
            Func<String, String, bool> deleteIAT = (Func<String, String, bool>)aResult.AsyncState;
            bool bResult = deleteIAT.EndInvoke(aResult);
            if (!bResult)
                return;
            MessageBox.Show("The deletion of your IAT was successful", "Deletion Successful");
            this.BeginInvoke((Action)Initialize);
        }

        public void EnableControlsAsync()
        {
            this.Invoke((Action)EnableControls);
        }

        private void DeleteIATDataButton_Click(object sender, EventArgs e)
        {
            if (PasswordBox.Text == String.Empty)
            {
                MessageBox.Show("Please enter the data retrieval password for this IAT in the Password box.", "No Password Supplied");
                return;
            }
            if (MessageBox.Show("This will permanently remove all data collected with this IAT from the server. Are you sure you wish to proceed?", "Confirm IAT Data Deletion", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            String iatName = SelectedIATLabel.Text;
            String password = PasswordBox.Text;
            AsyncCallback iatDataDeleted = new AsyncCallback(IATDataDeletionComplete);
            Func<String, String, bool> deleteIATData = IATManager.DeleteIATData;
            IAsyncResult async = deleteIATData.BeginInvoke(iatName, password, iatDataDeleted, this);
            DisableControlsForServerTransaction();
        }

        private void IATDataDeletionComplete(IAsyncResult aResult)
        {
            EnableControls();
            Func<String, String, bool> deleteIATData = IATManager.DeleteIATData;
            bool bResult = deleteIATData.EndInvoke(aResult);
            if (!bResult)
                return;
            MessageBox.Show("The delete of your IAT data is complete", "Deletion Successful");
            ConstructIATPanel();
        }

        private void RetrieveItemSlidesButton_Click(object sender, EventArgs e)
        {
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            mainForm.StartRetrieveItemSlides();
        }
    }
}
