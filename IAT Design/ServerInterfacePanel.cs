using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Remoting.Messaging;

namespace IATClient
{
    public class ResultsPanel : Control
    {
        class ServerInterfacePanel : UserControl
        {
            private enum ECurrentOperation { retrieveData, deleteIAT, deleteIATData, none };
            private ECurrentOperation CurrentOperation = ECurrentOperation.none;
            public Panel IATPanel { get; private set; } = new Panel();
            public Button UploadIATButton, DeleteIATButton, RetrieveResultsButton, DeleteIATDataButton, ExportDataButton, ClearButton, CloseButton;
            public TextBox PasswordBox;
            private Label PasswordBoxLabel;
            private Dictionary<String, Label> ClientLabels = new Dictionary<String, Label>();
            private Dictionary<String, Label> TestLabels = new Dictionary<String, Label>();
            private List<Label> IATNameLabels = new List<Label>();
            private String[] ClientLabelNames = { "Registered to", "Organization", "# of IATs alotted", "Total administrations", "Administrations remaining", "Disk alottment(MB)", 
                                                "Disk space remaining(MB)" };
            private String[] TestLabelNames = { "IAT Name", "Author", "Author eMail", "Last data retrieval", "Test size(KB)", "Administrations", "# of result sets" };
            private Padding PanelPadding = new Padding(20, 30, 20, 10);
            public readonly CIATManager IATManager;
            private static int ControlWidth = 250;
            public enum EControls { exportButton = 1, testLabels = 2, retrieveButton = 4, deleteButtons = 8 };
            public SynchronizationContext SyncCtx { get; private set; } = WindowsFormsSynchronizationContext.Current; 

            public Label SelectedIATLabel { get; private set; }

            public String SelectedIATName
            {
                get
                {
                    if (SelectedIATLabel == null)
                        return null;
                    return SelectedIATLabel.Text;
                }
            }
            public bool ItemSlidesRetrieved { get; set; }

            private IATConfigMainForm MainForm
            {
                get
                {
                    return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
                }
            }

            public ServerInterfacePanel(CIATManager iatManager, ResultsPanel resultsPanel)
            {
                this.AutoScaleMode = AutoScaleMode.Dpi;
                IATManager = iatManager;
                int fontHeight = (int)System.Drawing.SystemFonts.DialogFont.Height;
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
                UploadIATButton = new Button();
                UploadIATButton.Size = new Size((ControlWidth >> 1) - (PanelPadding.Horizontal >> 1), UploadIATButton.Height);
                UploadIATButton.Text = "Upload IAT";
                UploadIATButton.Location = new Point(PanelPadding.Left, PasswordBox.Bottom + fontHeight);
                UploadIATButton.Enabled = false;
                Controls.Add(UploadIATButton);
                RetrieveResultsButton = new Button();
                RetrieveResultsButton.Size = new Size((ControlWidth >> 1) - (PanelPadding.Horizontal >> 1), RetrieveResultsButton.Height);
                RetrieveResultsButton.Text = "Retrieve Data";
                RetrieveResultsButton.Location = new Point(UploadIATButton.Right + (PanelPadding.Horizontal >> 2), PasswordBox.Bottom + fontHeight);
                RetrieveResultsButton.Enabled = false;
                Controls.Add(RetrieveResultsButton);
                DeleteIATButton = new Button();
                DeleteIATButton.Size = new Size((ControlWidth >> 1) - (PanelPadding.Horizontal >> 1), DeleteIATButton.Height);
                DeleteIATButton.Text = "Delete IAT";
                DeleteIATButton.Location = new Point(PanelPadding.Left, UploadIATButton.Bottom + fontHeight);
                DeleteIATButton.Enabled = false;
                Controls.Add(DeleteIATButton);
                DeleteIATDataButton = new Button();
                DeleteIATDataButton.Size = new Size((ControlWidth >> 1) - (PanelPadding.Horizontal >> 1), DeleteIATDataButton.Height);
                DeleteIATDataButton.Text = "Delete Data";
                DeleteIATDataButton.Location = new Point(DeleteIATButton.Right + (PanelPadding.Horizontal >> 2), RetrieveResultsButton.Bottom + fontHeight);
                DeleteIATDataButton.Enabled = false;
                Controls.Add(DeleteIATDataButton);
                ExportDataButton = new Button();
                ExportDataButton.Size = new Size((ControlWidth >> 1) - (PanelPadding.Horizontal >> 1), ExportDataButton.Height);
                ExportDataButton.Text = "Export Data";
                ExportDataButton.Location = new Point(PanelPadding.Left, DeleteIATButton.Bottom + fontHeight);
                ExportDataButton.Enabled = false;
                Controls.Add(ExportDataButton);
                CloseButton = new Button();
                CloseButton.Size = new Size((ControlWidth >> 1) - (PanelPadding.Horizontal >> 1), CloseButton.Height);
                CloseButton.Text = "Close";
                CloseButton.Enabled = true;
                CloseButton.Location = new Point(ExportDataButton.Right + (PanelPadding.Horizontal >> 2), DeleteIATButton.Bottom + fontHeight);
                Controls.Add(CloseButton);
                this.Size = new Size(ControlWidth, CloseButton.Bottom + PanelPadding.Bottom);
            }

            public void Initialize(AsyncCallback callback)
            {
                DisableControlsForServerTransaction();
                Func<bool> del = new Func<bool>(IATManager.RetrieveServerReport);
                IAsyncResult async = del.BeginInvoke(callback, del);
            }

            public void PopulateIATPanel(EventHandler labelClickHandler)
            {
                if (IATManager.ServerReport == null)
                    return;
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
                ConstructIATPanel(labelClickHandler);
                PopulateTestLabels((CIATReport)null);
            }

            public void SelectIAT(String iatName)
            {
                if (iatName == null)
                {
                    PopulateTestLabels((CIATReport)null);
                    SelectedIATLabel = null;
                }
                else
                    IATName_Click(IATNameLabels.Find(l => l.Text == iatName));
            }

            private void ConstructIATPanel(EventHandler labelClickHandler)
            {
                int fontHeight = (int)System.Drawing.SystemFonts.DialogFont.Height;
                SelectedIATLabel = null;
                IATPanel.Controls.Clear();
                IATNameLabels.Clear();
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
                    if (IATManager.ServerReport.IATReports[iatName].Deploying)
                        l.ForeColor = Color.Red;
                    l.Click += labelClickHandler;
                    labelList.Add(l);
                    ctr++;
                }
                labelList.Sort((l1, l2) => l1.Text.CompareTo(l2.Text));
                foreach (Label l in labelList)
                {
                    l.Size = new Size(IATPanel.ClientRectangle.Width - SystemInformation.VerticalScrollBarWidth, (int)(fontHeight * 1.25));
                    IATPanel.Controls.Add(l);
                    IATNameLabels.Add(l);
                }
            }

            public void PopulateTestLabels(String iatName)
            {
                PopulateTestLabels(IATManager.ServerReport.IATReports[iatName]);
            }

            private void PopulateTestLabels(CIATReport rep)
            {
                TestLabels["Author"].Text = (rep == null) ? "Author: " : String.Format("Author: {0} {1} {2}", rep.AuthorTitle, rep.AuthorFName, rep.AuthorLName);
                TestLabels["Author eMail"].Text = (rep == null) ? "Author eMail: " : String.Format("Author eMail: {0}", rep.AuthorEMail);
                TestLabels["Last data retrieval"].Text = (rep == null) ? "Last data retrieval: " : String.Format("Last data retrieval: {0}", rep.LastDataRetrieval);
                TestLabels["IAT Name"].Text = (rep == null) ? "IAT Name: " : String.Format("IAT Name: {0}", rep.IATName);
                //                TestLabels["Test URL"].Text = (rep == null) ? "URL: " : String.Format("URL: {0}", rep.URL);
                TestLabels["Test size(KB)"].Text = (rep == null) ? "Test size(KB): " : String.Format("Test size(KB): {0}", rep.TestSizeKB);
                TestLabels["Administrations"].Text = (rep == null) ? "Administrations: " : String.Format("Administrations: {0}", rep.NumAdministrations);
                TestLabels["# of result sets"].Text = (rep == null) ? "# of result sets: " : String.Format("# of result sets: {0}", rep.NumResultSets);
            }

            public void Clear()
            {
                if (SelectedIATLabel != null)
                    SelectedIATLabel.BackColor = Color.White;
                SelectedIATLabel = null;
                RetrieveResultsButton.Enabled = false;
                DeleteIATDataButton.Enabled = false;
                DeleteIATButton.Enabled = false;
                PasswordBox.Text = String.Empty;
                PasswordBox.Enabled = false;
                PopulateTestLabels((CIATReport)null);
            }

            public String IATName_Click(Label sender)
            {
                if ((sender == null) && (SelectedIATLabel == null))
                    return String.Empty;
                else if (sender == null)
                    return SelectedIATLabel.Text;
                if (SelectedIATLabel != null)
                    SelectedIATLabel.BackColor = Color.White;
                SelectedIATLabel = sender;
                PopulateTestLabels(IATManager.ServerReport.IATReports[SelectedIATLabel.Text]);
                SelectedIATLabel.BackColor = Color.CornflowerBlue;
                PasswordBox.Text = String.Empty;
                if (IATManager.GetIATPasswordFromRegistry(SelectedIATLabel.Text) == null)
                    PasswordBox.Enabled = true;
                else
                {
                    PasswordBox.Text = "Stored in registry";
                    PasswordBox.Enabled = false;
                }
                return SelectedIATLabel.Text;
            }

            public void DisableControlsForServerTransaction()
            {
                IEnumerator e = Controls.GetEnumerator();
                while (e.MoveNext())
                    ((Control)e.Current).Enabled = false;
            }

            private void EnableControls(EControls controls)
            {
                CloseButton.Enabled = true;
                if (CIAT.SaveFile.IAT.Is7Block)
                    UploadIATButton.Enabled = true;
                foreach (Label l in ClientLabels.Values)
                    l.Enabled = true;
                foreach (Label l in TestLabels.Values)
                    l.Enabled = true;
                if ((controls & EControls.testLabels) != 0)
                {
                    if (SelectedIATLabel == null)
                        PasswordBox.Enabled = false;
                    else if (IATManager.GetIATPasswordFromRegistry(SelectedIATLabel.Text) == null)
                        PasswordBox.Enabled = true;
                    else
                    {
                        PasswordBox.Text = "Stored in registry";
                        PasswordBox.Enabled = false;
                    }
                    PasswordBoxLabel.Enabled = true;
                    IATPanel.Enabled = true;
                }
                if ((controls & EControls.exportButton) != 0)
                {
                    if (SelectedIATLabel != null)
                        ExportDataButton.Enabled = true;
                    else
                        ExportDataButton.Enabled = false;
                }
                else
                    ExportDataButton.Enabled = false;
                if ((controls & EControls.retrieveButton) != 0)
                {
                    if (SelectedIATLabel != null)
                    {
                        if (IATManager.ServerReport.IATReports[SelectedIATLabel.Text].NumResultSets > 0)
                            RetrieveResultsButton.Enabled = true;
                        else
                            RetrieveResultsButton.Enabled = false;
                    }
                    else
                        RetrieveResultsButton.Enabled = false;
                }
                else
                    RetrieveResultsButton.Enabled = false;
                if ((controls & EControls.deleteButtons) != 0)
                {
                    if (SelectedIATLabel != null)
                        DeleteIATButton.Enabled = true;
                    if (IATManager.ServerReport.IATReports[SelectedIATLabel.Text].NumResultSets > 0)
                        DeleteIATDataButton.Enabled = true;
                }
                else
                {
                    DeleteIATButton.Enabled = false;
                    DeleteIATDataButton.Enabled = false;
                    RetrieveResultsButton.Enabled = false;
                }
            }


            public void EnableControlsAsync(EControls controls)
            {
                SyncCtx.Post(o => this.EnableControls(controls), null);
            }

        }

        private static Size ResultsPanelSize = new Size(1010, 645);
        private ServerInterfacePanel m_ServerInterfacePanel;
        private ResultsGridPanel m_ResultsPanel = null;
        private ResultDetailsPanelContainer m_ResultDetailsContainer = null;
        private CResultDocument ResultDocument = null;
        private CIATManager IATManager = new CIATManager();
        private CWebSocketUploader IATUploader;
        private bool StorePasswordToRegistry = false;
        private String CurrentUploadingIAT, CurrentUploadingIATPassword;
        private SynchronizationContext _SyncCtx = WindowsFormsSynchronizationContext.Current;
        private String ActiveTestFilename = String.Empty;

        public SynchronizationContext SyncCtx
        {
            get
            {
                return _SyncCtx;
            }
        }

        private String SelectedIAT
        {
            get
            {
                if (m_ServerInterfacePanel.SelectedIATLabel == null)
                    return String.Empty;
                return m_ServerInterfacePanel.SelectedIATLabel.Text;
            }
            set
            {
                if (value == null)
                    m_ServerInterfacePanel.Invoke(new Action<String>(m_ServerInterfacePanel.SelectIAT), null);
                m_ServerInterfacePanel.Invoke(new Action<String>(m_ServerInterfacePanel.SelectIAT), value);
            }
        }

        private IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            }
        }

        public ResultsPanel()
        {
            m_ServerInterfacePanel = new ServerInterfacePanel(IATManager, this);
            m_ServerInterfacePanel.UploadIATButton.Click += new EventHandler(UploadIATButton_Click);
            m_ServerInterfacePanel.RetrieveResultsButton.Click += new EventHandler(RetrieveResultsButton_Click);
            m_ServerInterfacePanel.DeleteIATButton.Click += new EventHandler(DeleteIATButton_Click);
            m_ServerInterfacePanel.DeleteIATDataButton.Click += new EventHandler(DeleteIATDataButton_Click);
            m_ServerInterfacePanel.ExportDataButton.Click += new EventHandler(ExportDataButton_Click);
            //         m_ServerInterfacePanel.ClearButton.Click += new EventHandler(ClearButton_Click);
            m_ServerInterfacePanel.CloseButton.Click += new EventHandler(CloseButton_Click);
            m_ServerInterfacePanel.IATPanel.Click += new EventHandler(IATPanel_Click);
            m_ServerInterfacePanel.Location = new Point(0, 0);
            Controls.Add(m_ServerInterfacePanel);
            this.Size = new Size(ResultsPanelSize.Width, ResultsPanelSize.Height);
        }

        public void ShowResultsPanel()
        {
            try
            {
                if (m_ResultsPanel != null)
                    m_ResultsPanel.Dispose();
                m_ResultsPanel = new ResultsGridPanel(ResultsPanel_LabelClick);
                m_ResultsPanel.Dock = DockStyle.Fill;
                m_ResultDetailsContainer = new ResultDetailsPanelContainer(m_ResultsPanel);
                m_ResultDetailsContainer.Location = new Point(m_ServerInterfacePanel.Right, 0);
                m_ResultDetailsContainer.Size = new Size(this.ClientSize.Width - m_ServerInterfacePanel.Width, this.ClientSize.Height);
                m_ResultsPanel.Initialize(IATManager.GetResultData(SelectedIAT));
                m_ResultsPanel.Width = this.Width - m_ServerInterfacePanel.Width;
                Controls.Add(m_ResultDetailsContainer);
                m_ResultsPanel.Invalidate();
                m_ResultDetailsContainer.Invalidate();
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error initializing results panel", ex));
            }
        }

        private void DeleteIATDataButton_Click(object sender, EventArgs e)
        {
            String password;
            if ((password = m_ServerInterfacePanel.IATManager.GetIATPasswordFromRegistry(SelectedIAT)) == null)
            {
                if (m_ServerInterfacePanel.PasswordBox.Text == String.Empty)
                {
                    MessageBox.Show("Please enter the data retrieval password for this IAT in the Password box.", "No Password Supplied");
                    return;
                }
                else
                    password = m_ServerInterfacePanel.PasswordBox.Text;
            }
            if (MessageBox.Show("This will permanently remove all data collected with this IAT from the server. Are you sure you wish to proceed?", "Confirm IAT Data Deletion", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            AsyncCallback iatDataDeleted = new AsyncCallback(IATDataDeletionComplete);
            Func<String, String, bool> deleteIATData = new Func<String, String, bool>(m_ServerInterfacePanel.IATManager.DeleteIATData);
            IAsyncResult async = deleteIATData.BeginInvoke(SelectedIAT, password, iatDataDeleted, deleteIATData);
            m_ServerInterfacePanel.DisableControlsForServerTransaction();
        }

        private void IATDataDeletionComplete(IAsyncResult aResult)
        {
            m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels);
            Func<String, String, bool> deleteIATData = (Func<String, String, bool>)aResult.AsyncState;
            bool result = deleteIATData.EndInvoke(aResult);
            if (!result)
            {
                MessageBox.Show("The deletion of your IAT data failed", "Deletion Failed");
                return;
            }
            if (IATManager.GetResultData(SelectedIAT) != null)
            {
                this.Invoke(new Action<Control>(Controls.Remove), m_ResultDetailsContainer);
                m_ResultDetailsContainer.Invoke(new Action(Dispose));
                IATManager.ClearData(SelectedIAT);
            }
            m_ServerInterfacePanel.IATManager.ServerReport.RegisterIATDataDeletion(SelectedIAT);
            m_ServerInterfacePanel.Invoke((Action<String>)m_ServerInterfacePanel.PopulateTestLabels, SelectedIAT);
            MessageBox.Show("The deletion of your IAT data is complete", "Deletion Successful");
        }

        private void ResultsPanel_LabelClick(int nLabel, String iatName, int resultSet, Control callingPanel)
        {
            ResultDetailsPanel resultPanel = new ResultDetailsPanel(m_ResultDetailsContainer.Width, IATManager.GetResultData(m_ServerInterfacePanel.SelectedIATName), IATManager.GetItemSlides(iatName), nLabel, m_ResultDetailsContainer.PanelClose, m_ResultDetailsContainer.PanelSplit);
            resultPanel.Size = new Size(resultPanel.Width, this.ClientSize.Height);
            m_ResultDetailsContainer.AutoScaleMode = AutoScaleMode.Dpi;
            m_ResultDetailsContainer.SetPanel(resultPanel, callingPanel);
            resultPanel.GeneratePreview();
        }

        private void UploadIATButton_Click(object sender, EventArgs e)
        {
            MainForm.SetStatusMessage(Properties.Resources.sCreatingPreUploadBackup);
            MainForm.Invalidate();
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = Properties.Resources.sFileExt;
            dlg.Title = Properties.Resources.sOpenFileDialogTitle;
            dlg.Filter = String.Format(Properties.Resources.sFileDialogFilter);
            dlg.FilterIndex = 0;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            CItemValidator.StartValidation();
            foreach (CIATBlock block in CIAT.SaveFile.IAT.Blocks)
                CItemValidator.ValidateItem(block);
            foreach (CInstructionBlock iBlock in CIAT.SaveFile.IAT.InstructionBlocks)
                CItemValidator.ValidateItem(iBlock);
            if (CItemValidator.HasErrors)
            {
                CItemValidator.DisplayErrors(null);
                return;
            }
            ActiveTestFilename = Properties.Resources.sTempIATSaveFile;
            CIAT.SaveFile.Save(ActiveTestFilename);
            if (SaveFile.SaveThread.IsAlive)
                SaveFile.SaveThread.Join();
            CIAT.SaveFile.Dispose();
            File.SetAttributes(ActiveTestFilename, File.GetAttributes(ActiveTestFilename) | FileAttributes.Hidden);
            if (!CIAT.Open(dlg.FileName, false, true))
            {
                CIAT.SaveFile.Dispose();
                CIAT.Open(ActiveTestFilename, false, true);
                return;
            }
            CIAT.ImageManager.StartWorkers();
            UploadForm upForm = new UploadForm(CIAT.SaveFile.IAT, true);
            if (upForm.ShowDialog() != DialogResult.OK)
            {
                CIAT.SaveFile.Dispose();
                CIAT.Open(ActiveTestFilename, false, true);
                File.Delete(ActiveTestFilename);
                return;
            }
            if (MessageBox.Show("Would you like to save this password to your computer's registry? Keep in mind that your password is not saved on the IATSoftware.net server " +
                "in order to ensure the privacy of your data. If you lose or forget your password, your data will be irretrievable. Storing your password to your registry will " +
                "allow you to retrieve your data form this computer even if you lose your password.", "Save password to registry?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                StorePasswordToRegistry = true;
            else
                StorePasswordToRegistry = false;
            CIAT.SaveFile.IAT.Name = upForm.IATName;
            IATUploader = new CWebSocketUploader(CIAT.SaveFile.IAT, MainForm);
            Func<String, String, bool> del = new Func<String, String, bool>(IATUploader.Upload);
            CurrentUploadingIAT = upForm.IATName;
            CurrentUploadingIATPassword = upForm.Password;
            del.BeginInvoke(upForm.IATName, upForm.Password, new AsyncCallback(IATUploadComplete), del);
            m_ServerInterfacePanel.DisableControlsForServerTransaction();
        }


        public void Reinitialize()
        {
            m_ServerInterfacePanel.DisableControlsForServerTransaction();
            Task.Run<bool>(() => IATManager.RetrieveServerReport()).ContinueWith((task) => m_ServerInterfacePanel.SyncCtx.Send(
                obj => m_ServerInterfacePanel.PopulateIATPanel(new EventHandler(IATLabel_Click)), null)).ContinueWith(
                task => m_ServerInterfacePanel.SyncCtx.Send(obj => m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels), null));
        }

        private void IATUploadComplete(IAsyncResult async)
        {
            try
            {
                bool bResult = ((async as AsyncResult).AsyncDelegate as Func<String, String, bool>).EndInvoke(async);
                if ((bResult) && StorePasswordToRegistry)
                    LocalStorage.SetIATPassword(CurrentUploadingIAT, CurrentUploadingIATPassword);
                String uploadedIatName = CIAT.SaveFile.IAT.Name;
                CIAT.SaveFile.Dispose();
                CIAT.Open(ActiveTestFilename, false, true);
                CIAT.ImageManager.StartWorkers();
                File.Delete(ActiveTestFilename);
                Task.Run<bool>(() => IATManager.RetrieveServerReport()).ContinueWith((task) =>
                {
                    try
                    {
                        if (task.Result)
                        {
                            m_ServerInterfacePanel.SyncCtx.Send(obj => m_ServerInterfacePanel.PopulateIATPanel(new EventHandler(IATLabel_Click)), null);
                            SelectedIAT = uploadedIatName;
                            SyncCtx.Post(obj => this.IATLabel_Click(obj, new EventArgs()), m_ServerInterfacePanel.SelectedIATLabel);
                        }
                        else throw new Exception("Error updating server report.");
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.ReportError(new CReportableException("Error retrieving server report", ex));
                    }
                });
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error refreshing IAT list", ex));
            }
            finally
            {
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels);
            }
        }

        private void RetrieveResultsButton_Click(object sender, EventArgs e)
        {
            String password;
            if ((password = m_ServerInterfacePanel.IATManager.GetIATPasswordFromRegistry(SelectedIAT)) == null)
            {
                if (m_ServerInterfacePanel.PasswordBox.Text == String.Empty)
                {
                    MessageBox.Show("Please enter the data retrieval password for this IAT in the Password box.", "No Password Supplied");
                    return;
                }
                else
                    password = m_ServerInterfacePanel.PasswordBox.Text;
            }
            AsyncCallback resultDataRetrieved = new AsyncCallback(RetrieveResultsComplete);
            Func<String, String, bool> retrieveResults = new Func<String, String, bool>(m_ServerInterfacePanel.IATManager.RetrieveResults);
            retrieveResults.BeginInvoke(SelectedIAT, password, new AsyncCallback(resultDataRetrieved), retrieveResults);
            m_ServerInterfacePanel.DisableControlsForServerTransaction();
            ResultDocument = new CResultDocument();
        }

        private void RetrieveResultsComplete(IAsyncResult async)
        {
            Func<String, String, bool> retrieveResultsDelegate = (Func<String, String, bool>)async.AsyncState;
            bool result = retrieveResultsDelegate.EndInvoke(async);
            if (!result)
            {
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels);
                return;
            }
            this.Invoke((Action)ShowResultsPanel);
            String password;
            if ((password = m_ServerInterfacePanel.IATManager.GetIATPasswordFromRegistry(m_ServerInterfacePanel.SelectedIATLabel.Text)) == null)
                password = m_ServerInterfacePanel.PasswordBox.Text;
            Task.Run(() =>
            {
                m_ServerInterfacePanel.IATPanel.Enabled = false;
                Task downloadSlides = new Task(() =>
                {
                    IATManager.RetrieveItemSlides(SelectedIAT, password);
                });
                downloadSlides.RunSynchronously();
                bool again = true;
                while (!IATManager.ItemSlidesDownloaded(SelectedIAT) && again)
                {
                    if (MessageBox.Show("Would you like to attempt to retrieve item slides again?", "Slide Retrieval Failed", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        downloadSlides.RunSynchronously();
                    else
                    {
                        ClearResultPanel();
                        IATManager.ClearData(SelectedIAT);
                        m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.retrieveButton);
                        again = false;
                    }
                }
                if (IATManager.ItemSlidesDownloaded(SelectedIAT))
                    m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.exportButton | ServerInterfacePanel.EControls.deleteButtons);
                else
                    m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.deleteButtons | ServerInterfacePanel.EControls.retrieveButton | ServerInterfacePanel.EControls.testLabels);
                ResultDocument.SetResultData(IATManager.GetResultData(SelectedIAT), IATManager.GetItemSlides(SelectedIAT));
            });
        }

        private void IATPanel_Click(object sender, EventArgs e)
        {
            if (SelectedIAT != String.Empty)
                ClearResultPanel();
            m_ServerInterfacePanel.Clear();
        }

        public void Initialize()
        {
            m_ServerInterfacePanel.Initialize(new AsyncCallback(RetrieveServerReport_Done));
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            ClearResultPanel();
            MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
        }

        private void UpdateServerReport_Done(IAsyncResult async)
        {
            Func<bool> del = (Func<bool>)async.AsyncState;
            bool result = del.EndInvoke(async);
            if (result)
            {
                Action<EventHandler> popIATPanel = new Action<EventHandler>(m_ServerInterfacePanel.PopulateIATPanel);
                m_ServerInterfacePanel.BeginInvoke(popIATPanel, new EventHandler(IATLabel_Click));
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels);
            }
            else
            {
                MainForm.BeginInvoke((Action<IATConfigMainForm.EFormContents>)MainForm.SetFormContents, IATConfigMainForm.EFormContents.Main);
            }
        }


        private void RetrieveServerReport_Done(IAsyncResult async)
        {
            Func<bool> del = (Func<bool>)async.AsyncState;
            bool result = del.EndInvoke(async);
            if (result)
            {
                Action<EventHandler> popIATPanel = new Action<EventHandler>(m_ServerInterfacePanel.PopulateIATPanel);
                m_ServerInterfacePanel.BeginInvoke(popIATPanel, new EventHandler(IATLabel_Click));
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels);
            }
            else
            {
                MainForm.BeginInvoke((Action<IATConfigMainForm.EFormContents>)MainForm.SetFormContents, IATConfigMainForm.EFormContents.Main);
            }
        }

        private void DeleteIATButton_Click(object sender, EventArgs e)
        {
            String iatName = SelectedIAT;
            String password;
            if ((password = m_ServerInterfacePanel.IATManager.GetIATPasswordFromRegistry(SelectedIAT)) == null)
            {
                if (m_ServerInterfacePanel.PasswordBox.Text == String.Empty)
                {
                    MessageBox.Show("Please enter the data retrieval password for this IAT in the Password box.", "No Password Supplied");
                    return;
                }
                password = m_ServerInterfacePanel.PasswordBox.Text;
            }
            if (MessageBox.Show("This will permanently remove your IAT and all data collected with it from the server. Are you sure you wish to proceed?", "Confirm IAT Deletion", MessageBoxButtons.YesNo) != DialogResult.Yes)
                return;
            AsyncCallback iatDeleted = new AsyncCallback(IATDeletionComplete);
            Func<String, String, bool> deleteIAT = new Func<String, String, bool>(m_ServerInterfacePanel.IATManager.DeleteIAT);
            IAsyncResult async = deleteIAT.BeginInvoke(iatName, password, iatDeleted, deleteIAT);
            m_ServerInterfacePanel.DisableControlsForServerTransaction();
        }

        private void IATDeletionComplete(IAsyncResult aResult)
        {
            m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels);
            Func<String, String, bool> deleteIAT = (Func<String, String, bool>)aResult.AsyncState;
            bool result = deleteIAT.EndInvoke(aResult);
            if (result)
            {
                if (IATManager.GetResultData(SelectedIAT) != null)
                {
                    this.Invoke(new Action<Control>(Controls.Remove), m_ResultDetailsContainer);
                    m_ResultDetailsContainer.Invoke(new Action(Dispose));
                    IATManager.ClearData(SelectedIAT);
                }
                m_ServerInterfacePanel.IATManager.ServerReport.RegisterIATDeletion(SelectedIAT);
                m_ServerInterfacePanel.SyncCtx.Send(o => m_ServerInterfacePanel.SelectIAT(null), null);
                m_ServerInterfacePanel.BeginInvoke((Action<EventHandler>)m_ServerInterfacePanel.PopulateIATPanel, new EventHandler(IATLabel_Click));
                MessageBox.Show("The deletion of your IAT was successful", "Deletion Successful");
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels);
            }
            else
            {
                MessageBox.Show("The deletion of your IAT failed", "Deletion Failed");
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.deleteButtons | ServerInterfacePanel.EControls.retrieveButton);
            }
        }

        private void IATLabel_Click(object sender, EventArgs e)
        {
            String currIAT = SelectedIAT;
            String newIATName = m_ServerInterfacePanel.IATName_Click((Label)sender);
            if (newIATName == currIAT)
                return;
            ResultData.ResultData rd = IATManager.GetResultData(SelectedIAT);
            this.Invoke(new Action(ClearResultPanel));
            if (rd != null)
            {
                this.Invoke(new Action(ShowResultsPanel));
                ResultDocument.SetResultData(IATManager.GetResultData(SelectedIAT), IATManager.GetItemSlides(SelectedIAT));
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.exportButton | ServerInterfacePanel.EControls.testLabels);
            }
            else
            {
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.retrieveButton | ServerInterfacePanel.EControls.deleteButtons | ServerInterfacePanel.EControls.testLabels);
            }
        }

        private void ExportDataButton_Click(object sender, EventArgs e)
        {
            ResultDocument.WaitOnTransforms();
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = "Excel File|*.xlsx";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                Func<String, bool> del = new Func<String, bool>(ResultDocument.BeginWriteToFile);
                del.BeginInvoke(dlg.FileName, new AsyncCallback(EndExport), del);
            }
            m_ServerInterfacePanel.DisableControlsForServerTransaction();
        }

        private void EndExport(IAsyncResult async)
        {
            Func<String, bool> method = (Func<String, bool>)async.AsyncState;
            bool completed = method.EndInvoke(async);
            if (completed)
                this.Invoke((Func<IWin32Window, String, DialogResult>)MessageBox.Show, this, "Export to Excel successful.");
            else
                this.Invoke((Func<IWin32Window, String, DialogResult>)MessageBox.Show, this, "Export to Excel canceled.");
            m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.exportButton | ServerInterfacePanel.EControls.deleteButtons | ServerInterfacePanel.EControls.testLabels);
        }

        public void ClearResultPanel()
        {
            this.Invoke(new Action<Control>(this.Controls.Remove), m_ResultDetailsContainer);
            if (m_ResultDetailsContainer != null)
                m_ResultDetailsContainer.Invoke(new Action(m_ResultDetailsContainer.Dispose));
            m_ResultDetailsContainer = null;
        }

        public void Clear()
        {
        }
    }
}
