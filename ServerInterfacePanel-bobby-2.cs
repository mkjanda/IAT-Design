using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Collections;
using System.Threading;
using System.IO;

namespace IATClient
{
    public class ResultsPanel : Control
    {

        class ServerInterfacePanel : UserControl
        {
            private enum ECurrentOperation { retrieveData, deleteIAT, deleteIATData, none };
            private ECurrentOperation CurrentOperation = ECurrentOperation.none;
            public Panel IATPanel = new Panel();
            public Button UploadIATButton, DeleteIATButton, RetrieveResultsButton, DeleteIATDataButton, ExportDataButton, ClearButton, CloseButton;
            public TextBox PasswordBox;
            private Label PasswordBoxLabel;
            private Dictionary<String, Label> ClientLabels = new Dictionary<String, Label>();
            private Dictionary<String, Label> TestLabels = new Dictionary<String, Label>();
            private List<Label> IATNameLabels = new List<Label>();
            public Label SelectedIATLabel = null;
            private String[] ClientLabelNames = { "Registered to", "Organization", "# of IATs alotted", "Total administrations", "Administrations remaining", "Disk alottment(MB)", 
                                                "Disk space remaining(MB)" };
            private String[] TestLabelNames = { "IAT Name", "Author", "Author eMail", "Last data retrieval", "Test size(KB)", "Administrations", "# of result sets" };
            private Padding PanelPadding = new Padding(20, 30, 20, 10);
            public CIATManager IATManager;
            private bool _ItemSlidesRetrieved = false;
            private static int ControlWidth = (int)(250 * CIATLayout.XScale);
            private object lockObject = new object();
            public enum EControls { dataButtons = 1, testLabels = 2, noDataButtons = 4, closeButton = 8 };
            private ResultsPanel resultsPanel;


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

            private IATConfigMainForm MainForm
            {
                get
                {
                    return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
                }
            }

            public ServerInterfacePanel(CIATManager iatManager, ResultsPanel resultsPanel)
            {
                this.resultsPanel = resultsPanel;
                IATManager = iatManager;
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
                UploadIATButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, UploadIATButton.Height);
                UploadIATButton.Text = "Upload IAT";
                UploadIATButton.Location = new Point(PanelPadding.Left, PasswordBox.Bottom + fontHeight);
                UploadIATButton.Enabled = false;
                Controls.Add(UploadIATButton);
                RetrieveResultsButton = new Button();
                RetrieveResultsButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, RetrieveResultsButton.Height);
                RetrieveResultsButton.Text = "Retrieve IAT Results";
                RetrieveResultsButton.Location = new Point(PanelPadding.Left, UploadIATButton.Bottom + fontHeight);
                RetrieveResultsButton.Enabled = false;
                Controls.Add(RetrieveResultsButton);
                DeleteIATButton = new Button();
                DeleteIATButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, DeleteIATButton.Height);
                DeleteIATButton.Text = "Delete IAT";
                DeleteIATButton.Location = new Point(PanelPadding.Left, RetrieveResultsButton.Bottom + fontHeight);
                DeleteIATButton.Enabled = false;
                Controls.Add(DeleteIATButton);
                DeleteIATDataButton = new Button();
                DeleteIATDataButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, DeleteIATDataButton.Height);
                DeleteIATDataButton.Text = "Delete IAT Data";
                DeleteIATDataButton.Location = new Point(PanelPadding.Left, DeleteIATButton.Bottom + fontHeight);
                DeleteIATDataButton.Enabled = false;
                Controls.Add(DeleteIATDataButton);
                ExportDataButton = new Button();
                ExportDataButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, ExportDataButton.Height);
                ExportDataButton.Text = "Export Data";
                ExportDataButton.Location = new Point(PanelPadding.Left, DeleteIATDataButton.Bottom + fontHeight);
                ExportDataButton.Enabled = false;
                Controls.Add(ExportDataButton);
/*                ClearButton = new Button();
                ClearButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, ClearButton.Height);
                ClearButton.Text = "Clear";
                ClearButton.Location = new Point(PanelPadding.Left, ExportDataButton.Bottom + fontHeight);
                ClearButton.Enabled = false;
                Controls.Add(ClearButton); */
                CloseButton = new Button();
                CloseButton.Size = new Size(ControlWidth - PanelPadding.Horizontal, CloseButton.Height);
                CloseButton.Text = "Close";
                CloseButton.Enabled = true;
                CloseButton.Location = new Point(PanelPadding.Left, ExportDataButton.Bottom + fontHeight);
                Controls.Add(CloseButton);
                this.Size = new Size(ControlWidth, CloseButton.Bottom + PanelPadding.Bottom);
            }

            public void Initialize(AsyncCallback callback)
            {
                DisableControlsForServerTransaction();
                Func<CIATManager.ETransactionResult> retrieveServerReport = IATManager.RetrieveServerReport;
                IAsyncResult async = retrieveServerReport.BeginInvoke(callback, retrieveServerReport);
            }

            public void PopulateIATPanel(CServerReport sr, EventHandler labelClickHandler)
            {
                IATManager.ServerReport = sr;
                PopulateIATPanel(labelClickHandler);
            }

            public void PopulateIATPanel(EventHandler labelClickHandler)
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
                ConstructIATPanel(labelClickHandler);
                PopulateTestLabels((CIATReport)null);
            }

            private void ConstructIATPanel(EventHandler labelClickHandler)
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
                    l.Click += labelClickHandler;
                    labelList.Add(l);
                    IATPanel.Controls.Add(l);
                    IATNameLabels.Add(l);
                    ctr++;
                }
                foreach (Label l in labelList)
                    l.Size = new Size(IATPanel.ClientRectangle.Width, (int)(fontHeight * 1.25));
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
                if ((controls & EControls.dataButtons) != 0)
                {
                    UploadIATButton.Enabled = true;
                    CloseButton.Enabled = true;
        //            ClearButton.Enabled = true;
                    if (SelectedIATLabel != null)
                    {
                        if (IATManager.ServerReport.IATReports[SelectedIATLabel.Text].NumResultSets > 0)
                            DeleteIATDataButton.Enabled = true;
                        DeleteIATButton.Enabled = true;
                        RetrieveResultsButton.Enabled = false;
                    }
                    ExportDataButton.Enabled = true;
                }
                if ((controls & EControls.noDataButtons) != 0)
                {
                    UploadIATButton.Enabled = true;
                    CloseButton.Enabled = true;
                    ExportDataButton.Enabled = false;
      //              ClearButton.Enabled = false;
                    if (SelectedIATLabel != null)
                    {
                        if (IATManager.ServerReport.IATReports[SelectedIATLabel.Text].NumResultSets > 0)
                            DeleteIATDataButton.Enabled = true;
                        DeleteIATButton.Enabled = true;
                        RetrieveResultsButton.Enabled = true;
                    }
                }
                if ((controls & EControls.closeButton) != 0)
                {
                    CloseButton.Enabled = true;
                    UploadIATButton.Enabled = true;
                }
            }


            public void EnableControlsAsync(EControls controls)
            {
                this.Invoke((Action<EControls>)EnableControls, controls);
            }

        }

        private static Size ResultsPanelSize = new Size(1010, 645);
        private ServerInterfacePanel m_ServerInterfacePanel;
        private ResultsGridPanel m_ResultsPanel = null;
        private ResultDetailsPanelContainer m_ResultDetailsContainer = null;
        private Dictionary<String, CResultData> ResultsDictionary = new Dictionary<String, CResultData>();
        private Dictionary<String, CItemSlideContainer> ItemSlideDictionary = new Dictionary<String, CItemSlideContainer>();
        private CResultDocument ResultDocument = null;
        private CIATManager IATManager = new CIATManager();
        private CIAT UploadedIAT;
        private MemoryStream CurrIATStream;
        private CWebSocketUploader IATUploader;
        private bool StorePasswordToRegistry = false;
        private String CurrentUploadingIAT, CurrentUploadingIATPassword;

        private String SelectedIAT
        {
            get
            {
                if (m_ServerInterfacePanel.SelectedIATLabel == null)
                    return String.Empty;
                return m_ServerInterfacePanel.SelectedIATLabel.Text;
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
            this.Size = new Size((int)(ResultsPanelSize.Width * CIATLayout.XScale), (int)(ResultsPanelSize.Height * CIATLayout.YScale));
        }

        public void ShowResultsPanel()
        {
            if (m_ResultsPanel != null)
                m_ResultsPanel.Dispose();
            m_ResultsPanel = new ResultsGridPanel(ResultsPanel_LabelClick);
            m_ResultsPanel.Dock = DockStyle.Fill;
            m_ResultDetailsContainer = new ResultDetailsPanelContainer(m_ResultsPanel);
            m_ResultDetailsContainer.Location = new Point(m_ServerInterfacePanel.Right, 0);
            m_ResultDetailsContainer.Size = new Size(this.ClientSize.Width - m_ServerInterfacePanel.Width, this.ClientSize.Height);
            m_ResultsPanel.Initialize(ResultsDictionary[SelectedIAT]);
            m_ResultsPanel.Width = this.Width - m_ServerInterfacePanel.Width;
            Controls.Add(m_ResultDetailsContainer);
            m_ResultsPanel.Invalidate();
            m_ResultDetailsContainer.Invalidate();
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
            Func<String, String, CIATManager.ETransactionResult> deleteIATData = m_ServerInterfacePanel.IATManager.DeleteIATData;
            IAsyncResult async = deleteIATData.BeginInvoke(SelectedIAT, password, iatDataDeleted, deleteIATData);
            m_ServerInterfacePanel.DisableControlsForServerTransaction();
        }

        private void IATDataDeletionComplete(IAsyncResult aResult)
        {
            m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.noDataButtons | ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.closeButton);
            Func<String, String, CIATManager.ETransactionResult> deleteIATData = (Func<String, String, CIATManager.ETransactionResult>)aResult.AsyncState;
            CIATManager.ETransactionResult result = deleteIATData.EndInvoke(aResult);
            if (result == CIATManager.ETransactionResult.exception)
            {
                MessageBox.Show("Communication Error", IATManager.TransmissionException.Message);
                return;
            }
            if (result == CIATManager.ETransactionResult.failed)
                return;
            if (ResultsDictionary[SelectedIAT] != null)
            {
                this.Invoke((Action<Control>)Controls.Remove, m_ResultDetailsContainer);
                if (m_ResultDetailsContainer != null)
                    m_ResultDetailsContainer.Invoke((Action)Dispose);
                m_ResultDetailsContainer = null;
            }
            if (ItemSlideDictionary[SelectedIAT] != null)
                ItemSlideDictionary[SelectedIAT].Dispose();
            ResultsDictionary[SelectedIAT] = null;
            ItemSlideDictionary[SelectedIAT] = null;
            m_ServerInterfacePanel.IATManager.ServerReport.RegisterIATDataDeletion(SelectedIAT);
            m_ServerInterfacePanel.Invoke((Action<String>)m_ServerInterfacePanel.PopulateTestLabels, SelectedIAT);
            MessageBox.Show("The deletion of your IAT data is complete", "Deletion Successful");
        }

        private void ResultsPanel_LabelClick(int nLabel, String iatName, int resultSet, Control callingPanel)
        {
            ResultDetailsPanel resultPanel = new ResultDetailsPanel(m_ResultDetailsContainer.Width, ResultsDictionary[iatName], ItemSlideDictionary[iatName], nLabel, m_ResultDetailsContainer.PanelClose, m_ResultDetailsContainer.PanelSplit);
            resultPanel.Size = new Size(resultPanel.Width, this.ClientSize.Height);
            m_ResultDetailsContainer.SetPanel(resultPanel, callingPanel);
            resultPanel.GeneratePreview();
        }

        private void UploadIATButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.DefaultExt = Properties.Resources.sFileExt;
            dlg.Title = Properties.Resources.sOpenFileDialogTitle;
            dlg.Filter = String.Format(Properties.Resources.sFileDialogFilter);
            dlg.FilterIndex = 0;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            if ((MainForm.CurrentIATFilename != String.Empty) || (MainForm.Modified == true))
                CurrIATStream = MainForm.IAT.Save(MainForm.CurrentIATFilename);
            else
                CurrIATStream = null;
            UploadedIAT = new CIAT();
            UploadedIAT.Load(dlg.FileName, MainForm.ProgressBar);
            CItemValidator.StartValidation();
            foreach (CIATBlock block in UploadedIAT.Blocks)
                CItemValidator.ValidateItem(block);
            foreach (CInstructionBlock iBlock in UploadedIAT.InstructionBlocks)
                CItemValidator.ValidateItem(iBlock);
            if (CItemValidator.HasErrors)
            {
                CItemValidator.DisplayErrors(null);
                UploadedIAT.Dispose();
                MainForm.IAT.Load(CurrIATStream, MainForm.CurrentIATFilename);
                return;
            }
            UploadForm upForm = new UploadForm(UploadedIAT, true);
            if (upForm.ShowDialog() != DialogResult.OK)
            {
                UploadedIAT.Dispose();
                if (CurrIATStream != null)
                    MainForm.IAT.Load(CurrIATStream, MainForm.CurrentIATFilename);
                return;
            }
            if (MessageBox.Show("Would you like to save this password to your computer's registry? Keep in mind that your password is not saved on the IATSoftware.net server " +
                "in order to ensure the privacy of your data. If you lose or forget your password, your data will be irretrievable. Storing your password to your registry will " +
                "allow you to retrieve your data form this computer even if you lose your password.", "Save password to registry?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                StorePasswordToRegistry = true;
            else
                StorePasswordToRegistry = false;
            UploadedIAT.Name = upForm.IATName;
            IATUploader = new CWebSocketUploader(UploadedIAT, MainForm);
            Func<String, String, CWebSocketUploader.ETransactionResult> del = new Func<String, String, CWebSocketUploader.ETransactionResult>(IATUploader.Upload);
            CurrentUploadingIAT = upForm.IATName;
            CurrentUploadingIATPassword = upForm.Password;
            del.BeginInvoke(upForm.IATName, upForm.Password, new AsyncCallback(IATUploadComplete), del);
            m_ServerInterfacePanel.DisableControlsForServerTransaction();
        }

        private void IATUploadComplete(IAsyncResult async)
        {
            Func<String, String, CWebSocketUploader.ETransactionResult> del = (Func<String, String, CWebSocketUploader.ETransactionResult>)async.AsyncState;
            CWebSocketUploader.ETransactionResult result = del.EndInvoke(async);
            if ((result == CWebSocketUploader.ETransactionResult.success) && StorePasswordToRegistry)
                CRegistry.AddIATPassword(CurrentUploadingIAT, CurrentUploadingIATPassword);
            UploadedIAT.Dispose();
            if (CurrIATStream != null)
            {
                MainForm.Invoke((Action<MemoryStream>)MainForm.LoadIAT, CurrIATStream);
                CurrIATStream.Dispose();
                CurrIATStream = null;
            }
            if (result == CWebSocketUploader.ETransactionResult.success)
            {
                m_ServerInterfacePanel.IATManager.ServerReport.RegisterNewIAT(IATUploader.IATName);
                ResultsDictionary[IATUploader.IATName] = null;
                ItemSlideDictionary[IATUploader.IATName] = null;
                m_ServerInterfacePanel.Invoke((Action<EventHandler>)m_ServerInterfacePanel.PopulateIATPanel, new EventHandler(IATLabel_Click));
            }
            else if (result == CWebSocketUploader.ETransactionResult.exception)
                MessageBox.Show(IATUploader.TransactionException.Message, "Deployment Failed");
            if (SelectedIAT == String.Empty)
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.closeButton | ServerInterfacePanel.EControls.testLabels);
            else if (ResultsDictionary[SelectedIAT] == null)
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.closeButton | ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.noDataButtons);
            else
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.closeButton | ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.dataButtons);
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
            Func<String, String, CResultData> retrieveResults = new Func<String, String, CResultData>(m_ServerInterfacePanel.IATManager.RetrieveResults);
            retrieveResults.BeginInvoke(SelectedIAT, password, new AsyncCallback(resultDataRetrieved), retrieveResults);
            m_ServerInterfacePanel.DisableControlsForServerTransaction();
            ResultDocument = new CResultDocument();
        }

        private void RetrieveResultsComplete(IAsyncResult async)
        {
            Func<String, String, CResultData> retrieveResultsDelegate = (Func<String, String, CResultData>)async.AsyncState;
            CResultData results = retrieveResultsDelegate.EndInvoke(async);
            if (results == null)
            {
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.noDataButtons | ServerInterfacePanel.EControls.closeButton);
                return;
            }
            ResultsDictionary[SelectedIAT] = results;
            this.Invoke((Action)ShowResultsPanel);
            String password;
            if ((password = m_ServerInterfacePanel.IATManager.GetIATPasswordFromRegistry(m_ServerInterfacePanel.SelectedIATLabel.Text)) == null)
                password = m_ServerInterfacePanel.PasswordBox.Text;
            if (ItemSlideDictionary[SelectedIAT] != null)
                ItemSlideDictionary[SelectedIAT].Dispose();
            ItemSlideDictionary[SelectedIAT] = new CItemSlideContainer(SelectedIAT, password, results.ResultDescriptor.ConfigFile);
            Func<Action<String, String, bool>, bool> startRetrieveItemSlides = new Func<Action<String, String,bool>, bool>(ItemSlideDictionary[SelectedIAT].StartRetrieval);
            startRetrieveItemSlides.BeginInvoke(new Action<String, String, bool>(SlideRetrieveFail), new AsyncCallback(SlideRetrievalComplete), startRetrieveItemSlides);
        }

        private void SlideRetrieveFail(String msg, String caption, bool retryOption)
        {
            if (!retryOption)
            {
                ItemSlideDictionary[SelectedIAT].Dispose();
                ItemSlideDictionary[SelectedIAT] = null;
                ResultsDictionary[SelectedIAT] = null;
                ClearResultData();
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.dataButtons | ServerInterfacePanel.EControls.closeButton);
            }
            else if (MessageBox.Show(this, msg, caption, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                Func<Action<String, String, bool>, bool> startRetrieveItemSlides = new Func<Action<String, String, bool>, bool>(ItemSlideDictionary[SelectedIAT].StartRetrieval);
                startRetrieveItemSlides.BeginInvoke(new Action<String, String, bool>(SlideRetrieveFail), new AsyncCallback(SlideRetrievalComplete), startRetrieveItemSlides);
            }
            else
            {
                ItemSlideDictionary[SelectedIAT].Dispose();
                ItemSlideDictionary[SelectedIAT] = null;
                ResultsDictionary[SelectedIAT] = null;
                ClearResultData();
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.dataButtons | ServerInterfacePanel.EControls.closeButton);
            }
        }

        private void SlideRetrievalComplete(IAsyncResult async)
        {
            Func<Action<String, String, bool>, bool> startRetrieve = (Func<Action<String, String, bool>, bool>)async.AsyncState;
            bool success = (bool)startRetrieve.EndInvoke(async);
            if (success)
            {
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.dataButtons | ServerInterfacePanel.EControls.closeButton);
                ResultDocument.SetResultData(ResultsDictionary[SelectedIAT], ItemSlideDictionary[SelectedIAT]);
            }
            else
            {
                this.BeginInvoke((Action)ClearResultData);
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.noDataButtons | ServerInterfacePanel.EControls.closeButton);
            }
        }

        private void IATPanel_Click(object sender, EventArgs e)
        {
            if (SelectedIAT != String.Empty)
                ClearResultData();
            m_ServerInterfacePanel.Clear();
        }

        public void Initialize()
        {
            
            m_ServerInterfacePanel.Initialize(new AsyncCallback(RetrieveServerReport_Done));
        }

        private void CloseButton_Click(object sender, EventArgs e)
        {
            MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
        }

        private void RetrieveServerReport_Done(IAsyncResult async)
        {
            Func<CIATManager.ETransactionResult> del = (Func<CIATManager.ETransactionResult>)async.AsyncState;
            CIATManager.ETransactionResult result = del.EndInvoke(async);
            if (result == CIATManager.ETransactionResult.success)
            {
                foreach (String iatName in IATManager.ServerReport.IATReports.Keys)
                {
                    ResultsDictionary[iatName] = null;
                    ItemSlideDictionary[iatName] = null;
                }
                Action<CServerReport, EventHandler> popIATPanel = (Action<CServerReport, EventHandler>)m_ServerInterfacePanel.PopulateIATPanel;
                m_ServerInterfacePanel.BeginInvoke(popIATPanel, IATManager.ServerReport, new EventHandler(IATLabel_Click));
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.noDataButtons | ServerInterfacePanel.EControls.testLabels | ServerInterfacePanel.EControls.closeButton);
            }
            else if (result == CIATManager.ETransactionResult.exception)
            {
                MessageBox.Show(IATManager.TransmissionException.Message, "Communication Error");
                MainForm.BeginInvoke((Action<IATConfigMainForm.EFormContents>)MainForm.SetFormContents, IATConfigMainForm.EFormContents.Main);
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
            Func<String, String, CIATManager.ETransactionResult> deleteIAT = m_ServerInterfacePanel.IATManager.DeleteIAT;
            IAsyncResult async = deleteIAT.BeginInvoke(iatName, password, iatDeleted, deleteIAT);
            m_ServerInterfacePanel.DisableControlsForServerTransaction();
        }

        private void IATDeletionComplete(IAsyncResult aResult)
        {
            m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.closeButton | ServerInterfacePanel.EControls.testLabels);
            Func<String, String, CIATManager.ETransactionResult> deleteIAT = (Func<String, String, CIATManager.ETransactionResult>)aResult.AsyncState;
            CIATManager.ETransactionResult result = deleteIAT.EndInvoke(aResult);
            if (result == CIATManager.ETransactionResult.exception)
            {
                MessageBox.Show(IATManager.TransmissionException.Message, "Communication Error");

            }
            else if (result == CIATManager.ETransactionResult.success)
            {
                if (ResultsDictionary[SelectedIAT] != null)
                    this.BeginInvoke((Action)ClearResultData);
                ResultsDictionary.Remove(SelectedIAT);
                if (ItemSlideDictionary[SelectedIAT] != null)
                    ItemSlideDictionary[SelectedIAT].Dispose();
                ItemSlideDictionary.Remove(SelectedIAT);
                m_ServerInterfacePanel.IATManager.ServerReport.RegisterIATDeletion(SelectedIAT);
                m_ServerInterfacePanel.BeginInvoke((Action<EventHandler>)m_ServerInterfacePanel.PopulateIATPanel, new EventHandler(IATLabel_Click));
                MessageBox.Show("The deletion of your IAT was successful", "Deletion Successful");
            }
        }

        private void IATLabel_Click(object sender, EventArgs e)
        {
            String currIAT = SelectedIAT;
            m_ServerInterfacePanel.IATName_Click((Label)sender);
            if (SelectedIAT == currIAT)
                return;
            if (currIAT != String.Empty)
                if (ResultsDictionary[currIAT] != null)
                    ClearResultData();
            if (ResultsDictionary[SelectedIAT] != null)
            {
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.closeButton | ServerInterfacePanel.EControls.dataButtons | ServerInterfacePanel.EControls.testLabels);
                ShowResultsPanel();
                ResultDocument.SetResultData(ResultsDictionary[SelectedIAT], ItemSlideDictionary[SelectedIAT]);
            }
            else
                m_ServerInterfacePanel.EnableControlsAsync(ServerInterfacePanel.EControls.closeButton | ServerInterfacePanel.EControls.noDataButtons | ServerInterfacePanel.EControls.testLabels);
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
        }

        private void EndExport(IAsyncResult async)
        {
            Func<String, bool> method = (Func<String, bool>)async.AsyncState;
            bool completed = method.EndInvoke(async);
            if (completed)
                this.Invoke((Func<IWin32Window, String, DialogResult>)MessageBox.Show, this, "Export to Excel successful.");
            else
                this.Invoke((Func<IWin32Window, String, DialogResult>)MessageBox.Show, this, "Export to Excel canceled.");
        }

        public void ClearResultData()
        {
            Controls.Remove(m_ResultDetailsContainer);
            if (m_ResultDetailsContainer != null)
                m_ResultDetailsContainer.Dispose();
            m_ResultDetailsContainer = null;
            if (SelectedIAT != String.Empty)
            {
                ResultsDictionary[SelectedIAT] = null;
                if (ItemSlideDictionary[SelectedIAT] != null)
                {
                    Action del = new Action(ItemSlideDictionary[SelectedIAT].Dispose);
                    del.BeginInvoke(null, null);
                }
                ItemSlideDictionary[SelectedIAT] = null;
            }
        }

        public void Clear()
        {
        }
    }
}
