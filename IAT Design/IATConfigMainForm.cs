using javax.xml.transform;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Windows.Forms;
namespace IATClient
{

    /// <summary>
    /// The class of the main window of the application
    /// </summary>
    public partial class IATConfigMainForm : Form
    {
        public static IATConfigMainForm MainForm { get { return Application.OpenForms[Properties.Resources.sMainFormName] as IATConfigMainForm; } }
        public const String ServerPassword = "sONGSoNtHEdEATHSoFcHILDREN";
        public static SizeF ScreenDPI = new SizeF();
        private CWebSocketUploader IATUploader;
        // the file that contains the fonts
        private bool StorePasswordToRegistry;
        private SynchronizationContext _SyncCtx = WindowsFormsSynchronizationContext.Current;

        public SynchronizationContext SyncCtx
        {
            get
            {
                return _SyncCtx;
            }
        }

        /// <summary>
        /// gets the list of available fonts
        /// </summary>
        /// 
        static public CFontFile.FontData[] AvailableFonts
        {
            get
            {
                return CFontFile.AvailableFonts;
            }
        }

        public String StatusMessage
        {
            get
            {
                return MessageBar.Items["StatusLabel"].Text;
            }
            set
            {
                this.BeginInvoke(new Action(() =>
                {
                    if (ProgressBarUse == EProgressBarUses.None)
                        return;
                    MessageBar.Items["StatusLabel"].Text = value;
                }));
            }
        }

        public void SetProgressRange(int min, int max, int current)
        {
            if (min != max)
                this.BeginInvoke(new Action(() =>
                {
                    Progress.Minimum = min;
                    Progress.Maximum = max;
                    Progress.Value = current;
                }));
        }

        public DialogResult DisplayYesNoMessageBox(String message, String caption)
        {
            return (DialogResult)this.Invoke(new Func<DialogResult>(() => MessageBox.Show(message, caption, MessageBoxButtons.YesNo)));
        }


        public IContentsItem ActiveItem { get; set; }

        /// <summary>
        /// An enumeration of the various view states of the form
        /// </summary>
        public enum EFormContents { Main, Survey, IATBlock, Instructions, Layout, DynamicallyKey, ServerInterface, PurchasePage, None };

        // the current view state of the form
        private EFormContents _FormContents, _PreviousFormContents;
        public EFormContents PreviousFormContents
        {
            get
            {
                return _PreviousFormContents;
            }
        }

        /// <summary>
        /// gets or sets the current contents of the form
        /// </summary>
        public EFormContents FormContents
        {
            get
            {
                return _FormContents;
            }
            set
            {
                try
                {
                    CIAT IAT = CIAT.SaveFile.IAT;
                    switch (_FormContents)
                    {
                        case EFormContents.Main:
                            m_MainPanel.EndPreview();
                            Controls.Remove(m_MainPanel);
                            QuickPanel.Enabled = false;
                            Controls.Remove(QuickPanel);
                            m_MainPanel.Dispose();
                            //   QuickPanel.Dispose();
                            m_MainPanel = null;
                            break;

                        case EFormContents.Survey:
                            Controls.Remove(m_SurveyPanel);
                            m_SurveyPanel.Dispose();
                            m_SurveyPanel = null;
                            break;

                        case EFormContents.IATBlock:
                            if (!m_IATBlockPanel.Validate())
                                return;
                            Controls.Remove(m_IATBlockPanel);
                            m_IATBlockPanel.Dispose();
                            m_IATBlockPanel = null;
                            break;

                        case EFormContents.Instructions:
                            if (!m_InstructionPanel.Validate())
                                return;
                            /*                        if (m_InstructionPanel.InstructionBlock != null)
                                                    {
                                                        int ndx = IAT.InstructionBlocks.IndexOf((CInstructionBlock)ActiveItem);
                                                        IAT.DeleteInstructionBlock(IAT.InstructionBlocks[ndx]);
                                                        IAT.InsertInstructionBlock(m_InstructionPanel.InstructionBlock, ndx);
                                                        ndx = IAT.Contents.IndexOf(ActiveItem);
                                                        IAT.Contents.RemoveAt(ndx);
                                                        IAT.Contents.Insert(ndx, m_InstructionPanel.InstructionBlock);
                                                    }
                                                    */
                            Controls.Remove(m_InstructionPanel);
                            m_InstructionPanel.Dispose();
                            m_InstructionPanel = null;
                            break;

                        case EFormContents.Layout:
                            Controls.Remove(m_LayoutPanel);
                            m_LayoutPanel.Dispose();
                            break;
                        /*
                                            case EFormContents.DynamicallyKey:
                                                Controls.Remove(m_DynamicIATPanel);
                                                break;
                        */
                        case EFormContents.ServerInterface:
                            HideResultsPanel();
                            if (m_ResultDetailsContainer != null)
                            {
                                Controls.Remove(m_ResultDetailsContainer);
                                m_ResultDetailsContainer.Dispose();
                                m_ResultDetailsContainer = null;
                                ItemSlideContainer.Dispose();
                                _ItemSlides = null;
                            }
                            m_ResultsPanel.Dispose();
                            break;

                        case EFormContents.PurchasePage:
                            Controls.Remove(m_PurchasePanel);
                            m_PurchasePanel.Dispose();
                            break;
                    }
                    switch (value)
                    {
                        case EFormContents.Main:
                            ShowMainPanel();
                            m_MainPanel.PopulateContents(IAT.Contents);
                            ActiveItem = null;
                            QuickPanel.Enabled = true;
                            Controls.Add(QuickPanel);
                            break;

                        case EFormContents.Survey:
                            if (ActiveItem.Type == ContentsItemType.AfterSurvey)
                                ShowSurveyPanel(SurveyPanel.EOrdinality.after, ActiveItem);
                            else if (ActiveItem.Type == ContentsItemType.BeforeSurvey)
                                ShowSurveyPanel(SurveyPanel.EOrdinality.before, ActiveItem);
                            else
                                throw new Exception();
                            break;

                        case EFormContents.IATBlock:
                            ShowIATBlockPanel(ActiveItem);
                            break;

                        case EFormContents.Instructions:
                            ShowInstructionsPanel(ActiveItem);
                            break;

                        case EFormContents.Layout:
                            ShowLayoutPanel();
                            break;
                        /*
                                            case EFormContents.DynamicallyKey:
                                                EditMenu.Enabled = false;
                                                ShowDynamicKeyPanel(ActiveItem);
                                                break;
                        */
                        case EFormContents.ServerInterface:
                            ShowResultsPanel();
                            break;

                        case EFormContents.PurchasePage:
                            ShowPurchasePage();
                            break;
                    }
                    _PreviousFormContents = _FormContents;
                    _FormContents = value;
                }
                catch (Exception ex)
                {
                    ErrorReporter.ReportError(new CReportableException("User interface error", ex));
                }
            }
        }

        public void SetFormContents(EFormContents contents)
        {
            FormContents = contents;
        }

        // the error flag and current error message
        private bool _HasErrors;
        private String _ErrorMsg;

        // the current open file
        private static String _CurrentFilename; // there can be only one

        // a flag that indicates if the file has been modified
        private bool _Modified;

        /// <summary>
        /// gets or sets whether the currently open file has been modified
        /// </summary>
        public bool Modified
        {
            get
            {
                return _Modified;
            }
            set
            {
                // if flag is being set to "true" for the first time, enable appropriate controls and put an asterisk next
                // to the filename at the top of the window
                if ((value == true) && (_Modified == false))
                {
                    FileSaveMenuItem.Enabled = true;
                    if (CurrentFilename != String.Empty)
                        this.Text = String.Format(Properties.Resources.sMainWindowTitle, System.IO.Path.GetFileName(CurrentFilename) + "*");
                }
                else if ((value == false) && (_Modified == true))
                {
                    if (CurrentFilename != String.Empty)
                        this.Text = String.Format(Properties.Resources.sMainWindowTitle, System.IO.Path.GetFileName(CurrentFilename));
                }
                _Modified = value;
            }
        }

        /// <summary>
        /// gets or sets the name of the current filename.  set accepts a value with or without a path.
        /// </summary>
        private String CurrentFilename
        {
            get
            {
                return _CurrentFilename;
            }
            set
            {
                _CurrentFilename = value;
                if (_CurrentFilename == String.Empty)
                    Text = Properties.Resources.sMainWindowTitleNoFilename;
                else
                    Text = String.Format(Properties.Resources.sMainWindowTitle, System.IO.Path.GetFileName(value));
            }
        }

        /// <summary>
        /// A callback function that returns the directory of the file currently open, being loaded, or being saved
        /// </summary>
        /// <returns>The directory where the configuration file is located</returns>
        public delegate String IATDirectoryCallback();

        public static String GetIATDirectory()
        {
            if (_CurrentFilename == String.Empty)
                return String.Empty;
            return System.IO.Path.GetDirectoryName(_CurrentFilename);
        }

        /// <summary>
        /// gets the error flag
        /// </summary>
        public bool HasErrors
        {
            get
            {
                return _HasErrors;
            }
        }

        /// <summary>
        /// gets or sets the current error message.  setting the error message to String.Empty will clear the error flag 
        /// </summary>
        public String ErrorMsg
        {
            get
            {
                return _ErrorMsg;
            }
            set
            {
                _ErrorMsg = value;
                if (_ErrorMsg != String.Empty)
                {
                    _HasErrors = true;
                    MessageBar.Items["StatusText"].Text = _ErrorMsg;
                    MessageBar.Items["StatusImage"].ImageIndex = 1;
                }
                else
                {
                    _HasErrors = false;
                    MessageBar.Items["StatusText"].Text = "Okay";
                    MessageBar.Items["StatusImage"].ImageIndex = 0;
                }
            }
        }

        /// <summary>
        /// The MainPanel object that appears on program startup and from which the user can add components to the IAT,
        /// change the layout of the IAT window, publish the IAT to a server, etc . . .
        /// </summary>
        public MainPanel m_MainPanel;

        /// <summary>
        /// The SurveyPanel object that displays and allows for editing the survey
        /// </summary>
        public SurveyPanel m_SurveyPanel;

        /// <summary>
        /// The IATBlockPanel object that allows for the creation of and editing of IAT blocks
        /// </summary>
        public IATBlockPanel m_IATBlockPanel;

        /// <summary>
        /// The ResponseKeyPanel object that allows for the definition of IAT item response keys
        /// </summary>
        public ResponseKeyPanel m_ResponseKeyPanel;

        /// <summary>
        /// The InstructionScreenPanel object that allows the user to edit a block of instruction screens
        /// </summary>
        public InstructionScreenPanel m_InstructionPanel;

        public PurchasePanel m_PurchasePanel;
        //public DynamicIATPanel m_DynamicIATPanel;
        public ResultsPanel m_ResultsPanel;
        public ItemSlidePanel m_ItemSlidePanel;
        public ResultDetailsPanelContainer m_ResultDetailsContainer;
        /// <summary>
        /// The LayoutPanel object that allows for modifications to the IAT Layout
        /// </summary>
        public LayoutPanel m_LayoutPanel;
        private object lockObject = new object();
        /// <summary>
        /// the default constructor
        /// </summary>
        public IATConfigMainForm()
        {
            InitializeComponent();
            InitializeHeaderMenu();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(IATConfigMainForm));
            this.SuspendLayout();
            this.ClientSize = new System.Drawing.Size(284, 261);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = Properties.Resources.sMainFormName;
            this.ResumeLayout(false);
            IATNameBox.TextChanged += new EventHandler(IATNameBox_TextChanged);
            Name = Properties.Resources.sMainFormName;
            // perform one-time initialization
            this.Shown += new EventHandler(IATConfigMainForm_Shown);
            ActiveItem = null;
            _PreviousFormContents = EFormContents.None;
            _FormContents = EFormContents.None;
            m_SurveyPanel = null;
            m_IATBlockPanel = null;
            m_ResponseKeyPanel = null;
            m_MainPanel = null;
            m_InstructionPanel = null;
            m_LayoutPanel = null;
            MessageBar.ImageList = new ImageList();
            MessageBar.ImageList.TransparentColor = System.Drawing.Color.White;
            MessageBar.ImageList.Images.Add(Properties.Resources.go);
            MessageBar.ImageList.Images.Add(Properties.Resources.stop);

            // set error message and current file to none
            ErrorMsg = String.Empty;
            CurrentFilename = String.Empty;
            Modified = false;
        }

        void IATNameBox_TextChanged(object sender, EventArgs e)
        {
            CIAT.SaveFile.IAT.Name = IATNameBox.Text;
        }
        /*
                private Label MainWinStatusLabel = null, MainWinStatusSubLabel = null;
                private bool bFontFileDone = false;
        */

        private void IATConfigMainForm_Shown(object sender, EventArgs e)
        {
            if (!CFontFile.TryLoad())
                Task.Run(() =>
                {
                    this.Invoke(new Action(() =>
                    {
                        if (HeaderMenu != null)
                            HeaderMenu.Enabled = false;
                        if (m_MainPanel != null)
                            m_MainPanel.Enabled = false;
                    }));
                    CFontFile.Generate();
                    this.Invoke(new Action(() =>
                    {
                        if (HeaderMenu != null)
                            HeaderMenu.Enabled = true;
                        if (m_MainPanel != null)
                            m_MainPanel.Enabled = true;
                    }));
                });
            if (!File.Exists(SaveFile.RecoveryFilePath))
                CIAT.Create();
            else if (MessageBox.Show(Properties.Resources.sRecoveryFileDetected, Properties.Resources.sRecoveryFileDetectedCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                try
                {
                    CIAT.Recover();
                    m_MainPanel.PopulateContents(CIAT.SaveFile.IAT.Contents);
                    Modified = true;
                    MessageBox.Show(Properties.Resources.sRecoveryFileLoaded, Properties.Resources.sRecoveryFileLoadedCaption, MessageBoxButtons.OK);
                }
                catch (InvalidSaveFileException ex)
                {
                    throw ex;
                }
                catch (Exception ex)
                {
                    ErrorReporter.ReportError(new CReportableException("Error loading recovery file", ex));
                    MessageBox.Show(Properties.Resources.sRecoveryFileLoadFailure, Properties.Resources.sRecoveryFileLoadFailureCaption);
                    CIAT.Create();
                }
                finally
                {
                    File.Delete(SaveFile.RecoveryFilePath);
                }
            }
            else
            {
                File.Delete(SaveFile.RecoveryFilePath);
                CIAT.Create();
            }
            FormContents = EFormContents.Main;
        }

        private void OnEnableControl(Control c, bool bEnable)
        {
            c.Enabled = bEnable;
        }

        private void OnSuspendLayout(Control c, bool bSuspend)
        {
            if (bSuspend)
                c.SuspendLayout();
            else
                c.ResumeLayout(false);
        }

        private void LoadFontFile()
        {
            Action<Control, bool> enableControl = new Action<Control, bool>(OnEnableControl);
        }

        public void OperationFailed(String msg, String caption)
        {
            MessageBox.Show(this, msg, caption);
            EndProgressBarUse();
        }

        private void OperationComplete(String msg, String caption)
        {
            this.BeginInvoke(new Action(() => MessageBox.Show(this, msg, caption, MessageBoxButtons.OK)));
        }

        private void IATConfigMainForm_Load(object sender, EventArgs e)
        {

            BuildMenu();
            if (LocalStorage.Activated == LocalStorage.EActivationStatus.NotActivated)
            {
                if (MessageBox.Show("You will be uploading material to my server that I will not review. I want a verified " +
                    "email address should you upload something GROSSLY inappropriate.", "What's With Activation?", MessageBoxButtons.YesNo) != DialogResult.Yes)
                    this.Close();
                ActivationDialog dlg = new ActivationDialog();
                dlg.ShowDialog();
                if (dlg.ProductActivated != LocalStorage.EActivationStatus.Activated)
                    this.Close();
                return;
            }
            else if (LocalStorage.Activated == LocalStorage.EActivationStatus.EMailNotVerified)
            {
                SendConfirmationEMailForm sendEMailForm = new SendConfirmationEMailForm();
                if (sendEMailForm.ShowDialog() != DialogResult.OK)
                    this.Close();
            }
            else if (LocalStorage.Activated == LocalStorage.EActivationStatus.InconsistentVersion)
            {
                MessageBox.Show("The version of the product you are running is inconsistent with the version on this machine. The program will now terminate.", "Inconsistent Version", MessageBoxButtons.OK);
                this.Close();
            }
        }

        /// <summary>
        /// Displays the MainPanel, creating if necessary
        /// </summary>
        private void ShowMainPanel()
        {
            if (m_MainPanel == null)
            {
                m_MainPanel = new MainPanel(this);
                m_MainPanel.Location = new Point(0, HeaderMenu.Height);
                if (!CFontFile.Loaded)
                    m_MainPanel.Enabled = false;
            }
            Controls.Add(m_MainPanel);
        }

        /// <summary>
        /// Displays the SurveyPanel, creating it if necessary
        /// </summary>
        private void ShowSurveyPanel(SurveyPanel.EOrdinality Ordinality, IContentsItem SurveyItem)
        {
            int surveyNdx = -1;
            if (Ordinality == SurveyPanel.EOrdinality.before)
                surveyNdx = CIAT.SaveFile.IAT.BeforeSurvey.IndexOf((CSurvey)SurveyItem);
            else
                surveyNdx = CIAT.SaveFile.IAT.AfterSurvey.IndexOf((CSurvey)SurveyItem);
            if (m_SurveyPanel == null)
            {
                m_SurveyPanel = new SurveyPanel(Ordinality)
                {
                    Location = new Point(0, HeaderMenu.Height),
                    Size = new Size(this.ClientSize.Width, this.ClientSize.Height - HeaderMenu.Height - MessageBar.Height),
                    Survey = ActiveItem as CSurvey
                };
            }
            m_SurveyPanel.Ordinality = Ordinality;
            Controls.Add(m_SurveyPanel);
            m_SurveyPanel.SurveyTimeout = (Ordinality == SurveyPanel.EOrdinality.before) ? CIAT.SaveFile.IAT.BeforeSurvey[surveyNdx].Timeout :
                CIAT.SaveFile.IAT.AfterSurvey[surveyNdx].Timeout;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Display, SurveyItem.URI);
        }

        /// <summary>
        /// Displays the IATBlockPanel, creating it if necessary
        /// </summary>
        /// <param name="Block">The CIATBlock object to be edited by the control</param>
        private void ShowIATBlockPanel(IContentsItem Block)
        {
            m_IATBlockPanel = new IATBlockPanel(Block as CIATBlock);
            m_IATBlockPanel.Location = new Point(0, HeaderMenu.Height);
            m_IATBlockPanel.SuspendLayout();
            Controls.Add(m_IATBlockPanel);
            m_IATBlockPanel.ResumeLayout(false);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Display, Block.URI);
        }

        private void ShowPurchasePage()
        {
            m_PurchasePanel = new PurchasePanel();
            m_PurchasePanel.Location = new Point(0, HeaderMenu.Height);
            Controls.Add(m_PurchasePanel);
        }

        public void SetActiveIATItem(CIATItem item)
        {
            if (!Controls.Contains(m_IATBlockPanel))
                return;
            m_IATBlockPanel.SetActiveItem(item);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Display, item.URI);
        }

        /// <summary>
        /// Displays the ResponseKeyPanel, creating if necessary
        /// </summary>
        public void ShowResponseKeyPanel()
        {
            ResponseKeyDialog dlg = new ResponseKeyDialog();
            dlg.ShowDialog(this);
        }

        /// <summary>
        /// Displays the InstructionPanel, creating if necessary
        /// </summary>
        /// <param name="InstructionBlock">The block of instructions to be edited by the control</param>
        private void ShowInstructionsPanel(IContentsItem InstructionBlock)
        {
            if (m_InstructionPanel != null)
                m_InstructionPanel.Dispose();
            m_InstructionPanel = new InstructionScreenPanel(InstructionBlock as CInstructionBlock);
            m_InstructionPanel.Location = new Point(0, HeaderMenu.Height);
            m_InstructionPanel.Size = InstructionScreenPanel.InstructionScreenPanelSize;
            Controls.Add(m_InstructionPanel);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Display, InstructionBlock.URI);
        }

        private void ShowResultsPanel()
        {
            m_ResultsPanel = new ResultsPanel();
            Controls.Add(m_ResultsPanel);
            m_ResultsPanel.Location = new Point(0, 0);
            m_ResultsPanel.Initialize();
        }

        public void HideResultsPanel()
        {
            Controls.Remove(m_ResultsPanel);
        }

        public void SetActiveInstructionScreen(CInstructionScreen screen)
        {
            if (!Controls.Contains(m_InstructionPanel))
                return;
            m_InstructionPanel.SetActiveScreen(screen);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Display, screen.URI);
        }

        /// <summary>
        /// Displays the Layout Panel, creating it if necessary
        /// </summary>
        private void ShowLayoutPanel()
        {
            m_LayoutPanel = new LayoutPanel();
            m_LayoutPanel.Location = new Point(0, HeaderMenu.Height);
            m_LayoutPanel.Size = this.ClientSize - new Size(0, HeaderMenu.Height + MessageBar.Height);
            Controls.Add(m_LayoutPanel);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Open, CIAT.SaveFile.Layout.URI);
        }
        /*
                private void ShowDynamicKeyPanel(IContentsItem activeItem)
                {
                    m_DynamicIATPanel = new DynamicIATPanel();
                    m_DynamicIATPanel.Location = new Point(0, HeaderMenu.Height);
                    m_DynamicIATPanel.Size = this.ClientSize - new Size(0, HeaderMenu.Height + MessageBar.Height);
                    m_DynamicIATPanel.Block = (CIATBlock)activeItem;
                    m_DynamicIATPanel.Surveys = IAT.BeforeSurvey;
                    Controls.Add(m_DynamicIATPanel);
                }
                */
        /*
                /// <summary>
                /// Displays the server interface, creating it if necessary
                /// </summary>
                private void ShowServerInterface()
                {
                    if (m_ServerPanel == null)
                    {
                        m_ServerPanel = new ServerPanel();
                        m_ServerPanel.Location = new Point(0, HeaderMenu.Height);
                        m_ServerPanel.Size = ServerPanel.ServerPanelSize;
                    }
                    Controls.Add(m_ServerPanel);
                }
        */
        #region menu strip associated code

        public int ViewMenuWidth
        {
            get
            {
                Size sz = System.Windows.Forms.TextRenderer.MeasureText(ViewIATContentsMenuItem.Text, ViewIATContentsMenuItem.Font);
                int nWidth = sz.Width;
                sz = System.Windows.Forms.TextRenderer.MeasureText(ViewResponseKeysMenuItem.Text, ViewResponseKeysMenuItem.Font);
                if (nWidth < sz.Width)
                    nWidth = sz.Width;
                for (int ctr = 0; ctr < ViewIATItemList.Count; ctr++)
                {
                    sz = System.Windows.Forms.TextRenderer.MeasureText(ViewIATItemList[ctr].Text, ViewIATItemList[ctr].Font);
                    if (nWidth < sz.Width)
                        nWidth = sz.Width;
                }
                return nWidth;
            }
        }

        // the file menu
        private System.Windows.Forms.ToolStripMenuItem FileMenu;
        private System.Windows.Forms.ToolStripMenuItem FileNewMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileOpenMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileSaveMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileSaveAsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileCloseMenuItem;
        private System.Windows.Forms.ToolStripMenuItem FileExitMenuItem;

        // the edit menu
        private System.Windows.Forms.ToolStripMenuItem EditMenu;
        private System.Windows.Forms.ToolStripMenuItem EditCutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EditCopyMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EditPasteMenuItem;
        private System.Windows.Forms.ToolStripMenuItem EditDeleteMenuItem;

        // the view menu
        private System.Windows.Forms.ToolStripMenuItem ViewMenu;
        private System.Windows.Forms.ToolStripMenuItem ViewIATContentsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewDataRetrievalDialogMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewLayoutMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ViewResponseKeysMenuItem;
        private ToolStripMenuItem ViewFixationCrossMenuItem;
        private System.Windows.Forms.ToolStripSeparator ViewStripSeparator;
        private List<System.Windows.Forms.ToolStripMenuItem> ViewIATItemList;
        /*
                // the append menu
                private System.Windows.Forms.ToolStripMenuItem AppendMenu;
                private System.Windows.Forms.ToolStripMenuItem AppendQuestionWithCurrentResponseMenuItem;
                private System.Windows.Forms.ToolStripSeparator AppendStripSeparator;
                private System.Windows.Forms.ToolStripMenuItem AppendDateQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendInstructionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendLikertQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendMultipleChoiceQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendMultipleSelectionQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendTextOrNumberQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendBoundedLengthQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendBoundedNumberQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendFixedDigQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendFixedLengthQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendMaxLengthQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendTrueFalseQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendWeightedMuitipleChoiceQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendRegExQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem AppendQuestionValidatedByValuesInAttachedFileMenuItem;

                // the insert menu
                private System.Windows.Forms.ToolStripMenuItem InsertMenu;
                private System.Windows.Forms.ToolStripMenuItem InsertQuestionWithCurrentResponseMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertDateQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertInstructionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertLikertQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertMultipleChoiceQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertMultipleSelectionQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertRegExQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertTextOrNumberQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertBoundedLengthQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertBoundedNumberQuestionMenuItem;
                private System.Windows.Forms.ToolStripSeparator InsertStripSeparator;
                private System.Windows.Forms.ToolStripMenuItem InsertFixedDigQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertFixedLengthQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertMaxLengthQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertTrueFalseQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertWeightedMultipleChoiceQuestionMenuItem;
                private System.Windows.Forms.ToolStripMenuItem InsertQuestionValidatedByValuesInAttachedFileMenuItem;
        */


        // the help menu
        private System.Windows.Forms.ToolStripMenuItem HelpMenu;
        private System.Windows.Forms.ToolStripSeparator HelpSeparator;
        private System.Windows.Forms.ToolStripMenuItem HelpIndexMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HelpContentsMenuItem;
        private System.Windows.Forms.ToolStripMenuItem HelpAboutMenuItem;

        // menu item names
        private const String sFileMenuName = "FileMenu";
        private const String sFileNewMenuItemName = "FileNewMenuItem";
        private const String sFileOpenMenuItemName = "FileOpenMenuItem";
        private const String sFileSaveMenuItemName = "FileSaveMenuItem";
        private const String sFileSaveAsMenuItemName = "FileSaveAsMenuItem";
        private const String sFileCloseMenuItemName = "FileCloseMenuItem";
        private const String sFileExitMenuItemName = "FileExitMenuItemName";
        private const String sEditMenuName = "EditMenu";
        private const String sEditCutMenuItemName = "EditCutMenuItem";
        private const String sEditCopyMenuItemName = "EditCopyMenuItem";
        private const String sEditPasteMenuItemName = "EditPasteMenuItem";
        private const String sEditDeleteMenuItemName = "EditDeleteMenuItem";
        private const String sViewMenuName = "ViewMenu";
        private const String sViewIATContentsMenuItemName = "ViewIATContentsMenuItem";
        private const String sViewDataRetrievalDialogMenuItemName = "ViewDataRetrievalDialogMenuItem";
        private const String sViewLayoutMenuItem = "ViewLayoutMenuItem";
        private const String sViewResponseKeysMeuItemName = "ViewResponseKeysMenuItem";
        private const String sViewFixationCrossMenuItemName = "ViewFixationCrossMenuItemName";
        private const String sViewStripSeparator = "ViewStripSeparator";
        private const String sServerMenuName = "ServerMenu";
        private const String sServerRetrieveItemSlidesItemName = "ServerRetrieveItemSlidesMenuItem";
        private const String sServerStripSeparator = "ServerStripSeparator";
        private const String sServerDeleteIATMenuItemName = "ServerDeleteIATMenuItem";
        private const String sServerDeleteIATDataMenuItemName = "ServerDeleteIATDataMenuItem";
        private const String sServerServerInterfaceMenuItemName = "ServerInterfaceMenuItem";
        private const String sAppendMenuName = "AppendMenu";
        private const String sAppendStripSeparatorName = "AppendStripSeparator";
        private const String sAppendQuestionWithCurrentResponseMenuItemName = "AppendQuestionWithCurrentResponseMenuItem";
        private const String sAppendDateQuestionMenuItemName = "AppendDateQuestionMenuItem";
        private const String sAppendInstructionMenuItemName = "AppendInstructionMenuItem";
        private const String sAppendLikertQuestionMenuItemName = "AppendLikertQuestionMenuItem";
        private const String sAppendMultipleChoiceQuestionMenuItemName = "AppendMultipleChoiceQuestionMenuItem";
        private const String sAppendMultipleSelectionQuestionMenuItemName = "AppendMultipleSelectionQuesstionMenuItem";
        private const String sAppendTextOrNumberQuestionMenuItemName = "AppendTextOrNumberQuestionMenuItem";
        private const String sAppendBoundedLengthQuestionMenuItemName = "AppendBoundedLengthQuestionMenuItem";
        private const String sAppendBoundedNumberQuestionMenuItemName = "AppendBoundedNumberQuestionMenuItem";
        private const String sAppendFixedDigQuestionMenuItemName = "AppendFixedDigQuestionMenuItem";
        private const String sAppendFixedLengthQuestionMenuItemName = "AppendFixedLengthQuestionMenuItem";
        private const String sAppendMaxLengthQuestionMenuItemName = "AppendMaxLengthQuestionMenuItem";
        private const String sAppendRegExQuestionMenuItemName = "AppendRegExQuestionMenuItem";
        private const String sAppendTrueFalseQuestionMenuItemName = "AppendTrueFalseQuestionMenuItem";
        private const String sAppendWeightedMultipleChoiceQuestionMenuItemName = "AppendWeightedMultipleChoiceQuestionMenuItem";
        private const String sAppendQuestionValidatedByValuesInAttachedFileMenuItemName = "AppendQuestionValidatedByValuesInAttachedFileMenuItem";
        private const String sInsertMenuName = "InsertMenu";
        private const String sInsertStripSeparatorName = "InsertStripSeparator";
        private const String sInsertQuestionWithCurrentResponseMenuItemName = "InsertQuestionWithCurrentResponseMenuItem";
        private const String sInsertDateQuestionMenuItemName = "InsertDateQuestionMenuItem";
        private const String sInsertInstructionMenuItemName = "InsertInstructionMenuItem";
        private const String sInsertLikertQuestionMenuItemName = "InsertLikertQuestionMenuItem";
        private const String sInsertMultipleChoiceQuestionMenuItemName = "InsertMultipleChoiceQuestionMenuItem";
        private const String sInsertMultipleSelectionQuestionMenuItemName = "InsertMultipleSelectionQuesstionMenuItem";
        private const String sInsertTextOrNumberQuestionMenuItemName = "InsertTextOrNumberQuestionMenuItem";
        private const String sInsertBoundedLengthQuestionMenuItemName = "InsertBoundedLengthQuestionMenuItem";
        private const String sInsertBoundedNumberQuestionMenuItemName = "InsertBoundedNumberQuestionMenuItem";
        private const String sInsertFixedDigQuestionMenuItemName = "InsertFixedDigQuestionMenuItem";
        private const String sInsertFixedLengthQuestionMenuItemName = "InsertFixedLengthQuestionMenuItem";
        private const String sInsertMaxLengthQuestionMenuItemName = "InsertMaxLengthQuestionMenuItem";
        private const String sInsertRegExQuestionMenuItemName = "InsertRegExQuestionMenuItem";
        private const String sInsertTrueFalseQuestionMenuItemName = "InsertTrueFalseQuestionMenuItem";
        private const String sInsertWeightedMultipleChoiceQuestionMenuItemName = "InsertWeightedMultipleChoiceQuestionMenuItem";
        private const String sInsertQuestionValidatedByValuesInAttachedFileMenuItemName = "InsertQuestionValidatedByValuesInAttachedFileMenuItem";
        private const String sHelpMenuName = "HelpMenu";
        private const String sHelpSeparatorName = "HelpSeparator";
        private const String sHelpIndexMenuItemName = "HelpIndexMenuItem";
        private const String sHelpContentsMenuItemName = "HelpContentsMenuItem";
        private const String sHelpAboutMenuItemName = "HelpAboutMenuItem";
        private const String sHeaderMenuName = "HeaderMenu";

        /// <summary>
        /// Initializes the header menu and its child controls
        /// </summary>
        private void InitializeHeaderMenu()
        {
            //
            // FileMenu
            //
            this.FileMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.FileMenu.Name = sFileMenuName;
            this.FileMenu.Size = new System.Drawing.Size(37, 20);
            this.FileMenu.Text = "&File";
            this.FileMenu.Enabled = false;
            // 
            // FileNewMenuItem
            // 
            this.FileNewMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileNewMenuItem.Name = sFileNewMenuItemName;
            this.FileNewMenuItem.Size = new System.Drawing.Size(132, 22);
            this.FileNewMenuItem.Text = "&New";
            this.FileNewMenuItem.Enabled = false;
            this.FileNewMenuItem.Click += (sender, args) => CloseFile(true);
            //
            // FileOpenMenuItem
            //
            this.FileOpenMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileOpenMenuItem.Name = sFileOpenMenuItemName;
            this.FileOpenMenuItem.Size = new System.Drawing.Size(132, 22);
            this.FileOpenMenuItem.Text = "&Open";
            this.FileOpenMenuItem.Click += new System.EventHandler(this.FileOpenMenuItem_Click);
            this.FileOpenMenuItem.Enabled = false;
            //
            // FileSaveMenuItem
            //
            this.FileSaveMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileSaveMenuItem.Name = sFileSaveMenuItemName;
            this.FileSaveMenuItem.Size = new System.Drawing.Size(132, 22);
            this.FileSaveMenuItem.Text = "&Save";
            this.FileSaveMenuItem.Click += new System.EventHandler(this.FileSaveMenuItem_Click);
            this.FileSaveMenuItem.Enabled = false;
            //
            // FileSaveAsMenuItem
            //
            this.FileSaveAsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileSaveAsMenuItem.Name = sFileSaveAsMenuItemName;
            this.FileSaveAsMenuItem.Size = new System.Drawing.Size(132, 22);
            this.FileSaveAsMenuItem.Text = "Save &As . . .";
            this.FileSaveAsMenuItem.Click += new System.EventHandler(this.FileSaveAsMenuItem_Click);
            this.FileSaveAsMenuItem.Enabled = false;
            //
            // FileCloseMenuItem
            //
            this.FileCloseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileCloseMenuItem.Name = sFileCloseMenuItemName;
            this.FileCloseMenuItem.Size = new System.Drawing.Size(132, 22);
            this.FileCloseMenuItem.Text = "&Close";
            this.FileCloseMenuItem.Click += (s, a) => CloseFile(true);
            this.FileCloseMenuItem.Enabled = false;
            //
            // FileExitMenuItem
            //
            this.FileExitMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.FileExitMenuItem.Name = sFileExitMenuItemName;
            this.FileExitMenuItem.Size = new System.Drawing.Size(132, 22);
            this.FileExitMenuItem.Text = "E&xit";
            this.FileExitMenuItem.Click += new System.EventHandler(this.FileExitMenuItem_Click);
            this.FileExitMenuItem.Enabled = false;
            //
            // ViewMenu
            //
            this.ViewMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewMenu.Name = sViewMenuName;
            this.ViewMenu.Size = new System.Drawing.Size(44, 20);
            this.ViewMenu.Text = "&View";
            this.ViewMenu.Enabled = true;
            //
            // ViewIATItemList
            //
            this.ViewIATItemList = new List<ToolStripMenuItem>();
            //
            // ViewIATContentsMenuItem
            //
            this.ViewIATContentsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewIATContentsMenuItem.Name = sViewIATContentsMenuItemName;
            this.ViewIATContentsMenuItem.Text = "Main &Contents";
            this.ViewIATContentsMenuItem.Click += new EventHandler(ViewIATContentsMenuItem_Click);
            this.ViewIATContentsMenuItem.Enabled = true;
            //
            // ViewServerInterfaceMenuItem
            //
            this.ViewDataRetrievalDialogMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewDataRetrievalDialogMenuItem.Name = sViewDataRetrievalDialogMenuItemName;
            this.ViewDataRetrievalDialogMenuItem.Text = "&Server Interface";
            this.ViewDataRetrievalDialogMenuItem.Click += new EventHandler(ViewDataRetrievalDialogMenuItem_Click);
            this.ViewDataRetrievalDialogMenuItem.Enabled = true;
            //
            // ViewLayoutMenuItem
            //
            this.ViewLayoutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewLayoutMenuItem.Name = sViewLayoutMenuItem;
            this.ViewLayoutMenuItem.Text = "&Layout";
            this.ViewLayoutMenuItem.Click += new EventHandler(ViewLayoutMenuItem_Click);
            this.ViewLayoutMenuItem.Enabled = true;
            //
            // ViewResponseKeysMenuItem
            //
            this.ViewResponseKeysMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ViewResponseKeysMenuItem.Name = sViewResponseKeysMeuItemName;
            this.ViewResponseKeysMenuItem.Text = "&Response Keys";
            this.ViewResponseKeysMenuItem.Click += new EventHandler(ViewResponseKeysMenuItem_Click);
            this.ViewResponseKeysMenuItem.Enabled = true;
            //
            // ViewFixationCrossMenuItem
            //
            this.ViewFixationCrossMenuItem = new ToolStripMenuItem();
            this.ViewFixationCrossMenuItem.Name = sViewFixationCrossMenuItemName;
            this.ViewFixationCrossMenuItem.Text = "&Fixation Cross";
            //      this.ViewFixationCrossMenuItem.Click += new EventHandler(ViewFixactionCrossMenuItem_Click);
            this.ViewFixationCrossMenuItem.Enabled = true;

            //
            // ViewStripSeparator
            //
            this.ViewStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.ViewStripSeparator.Name = sViewStripSeparator;
            //
            // HelpMenu
            //
            this.HelpMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpMenu.Name = sHelpMenuName;
            this.HelpMenu.Size = new System.Drawing.Size(44, 20);
            this.HelpMenu.Text = "&Help";
            this.HelpMenu.Enabled = true;
            //
            // HelpIndexMenuItem
            //
            this.HelpIndexMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpIndexMenuItem.Name = sHelpIndexMenuItemName;
            this.HelpIndexMenuItem.Size = new System.Drawing.Size(122, 22);
            this.HelpIndexMenuItem.Text = "&Index";
            this.HelpIndexMenuItem.Enabled = true;
            this.HelpIndexMenuItem.Click += new EventHandler(HelpIndexMenuItem_Click);
            //
            // HelpContentsMenuItem
            // 
            this.HelpContentsMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpContentsMenuItem.Name = sHelpContentsMenuItemName;
            this.HelpContentsMenuItem.Size = new System.Drawing.Size(122, 22);
            this.HelpContentsMenuItem.Text = "&Contents";
            this.HelpContentsMenuItem.Enabled = true;
            this.HelpContentsMenuItem.Click += new EventHandler(HelpContentsMenuItem_Click);

            //
            // HelpSeparator
            //
            this.HelpSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.HelpSeparator.Name = sHelpSeparatorName;
            this.HelpSeparator.Size = new System.Drawing.Size(119, 6);
            //
            // HelpAboutMenuItem
            //
            this.HelpAboutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.HelpAboutMenuItem.Name = sHelpAboutMenuItemName;
            this.HelpAboutMenuItem.Size = new System.Drawing.Size(122, 22);
            this.HelpAboutMenuItem.Text = "&About";
            this.HelpAboutMenuItem.Enabled = false;
            this.HelpAboutMenuItem.Click += new EventHandler(HelpAboutMenuItem_Click);
        }



        void HelpAboutMenuItem_Click(object sender, EventArgs e)
        {
            AboutBox aboutBox = new AboutBox();
            aboutBox.Show();
        }

        /// <summary>
        /// Builds the header menu, enabling all options suited for non-survey work
        /// </summary>
        private void BuildMenu()
        {
            SuspendLayout();
            HeaderMenu.SuspendLayout();

            //
            // FileMenu
            //
            this.FileMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileNewMenuItem,
            this.FileOpenMenuItem,
            this.FileSaveMenuItem,
            this.FileSaveAsMenuItem,
            this.FileCloseMenuItem,
            this.FileExitMenuItem});
            this.FileNewMenuItem.Enabled = true;
            this.FileOpenMenuItem.Enabled = true;
            this.FileSaveMenuItem.Enabled = true;
            this.FileSaveAsMenuItem.Enabled = true;
            this.FileCloseMenuItem.Enabled = true;
            this.FileExitMenuItem.Enabled = true;
            this.FileMenu.Enabled = true;

            ConstructViewMenu(false);


            this.HelpMenu.DropDownItems.Add(this.HelpAboutMenuItem);
            this.HelpIndexMenuItem.Enabled = true;
            this.HelpContentsMenuItem.Enabled = true;
            this.HelpAboutMenuItem.Enabled = true;
            this.HelpMenu.Enabled = true;

            this.HeaderMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.ViewMenu,
            this.HelpMenu
            });
            this.HeaderMenu.Location = new System.Drawing.Point(0, 0);
            this.HeaderMenu.Name = sHeaderMenuName;
            this.HeaderMenu.Size = new System.Drawing.Size(764, 24);
            this.HeaderMenu.TabIndex = 0;
            this.HeaderMenu.Text = "menuStrip1";

            HeaderMenu.ResumeLayout(false);
            HeaderMenu.PerformLayout();
            ResumeLayout(false);
            PerformLayout();
        }

        /// <summary>
        /// Constructs the view menu in the header menu
        /// </summary>
        public void ConstructViewMenu(bool bSuspendLayout)
        {
            if (bSuspendLayout)
            {
                this.SuspendLayout();
                HeaderMenu.SuspendLayout();
            }
            ViewIATItemList.Clear();
            if (CIAT.SaveFile != null)
            {
                if (CIAT.SaveFile.IAT != null)
                {
                    for (int ctr = 0; ctr < CIAT.SaveFile.IAT.Contents.Count; ctr++)
                    {
                        ViewIATItemList.Add(new ToolStripMenuItem());
                        ViewIATItemList[ctr].Name = String.Format("ViewIATContentsItem{0}", ctr + 1);
                        ViewIATItemList[ctr].Text = CIAT.SaveFile.IAT.Contents[ctr].Name;
                        ViewIATItemList[ctr].Click += new EventHandler(ViewIATItemList_Click);
                        ViewIATItemList[ctr].Enabled = true;
                    }
                }
            }
            ViewMenu.DropDownItems.Clear();
            ViewMenu.DropDownItems.Add(ViewIATContentsMenuItem);
            ViewMenu.DropDownItems.Add(ViewDataRetrievalDialogMenuItem);
            ViewMenu.DropDownItems.Add(ViewLayoutMenuItem);
            ViewMenu.DropDownItems.Add(ViewResponseKeysMenuItem);
            if (ViewIATItemList.Count != 0)
                ViewMenu.DropDownItems.Add(ViewStripSeparator);
            ViewMenu.DropDownItems.AddRange(ViewIATItemList.ToArray());
            ViewIATContentsMenuItem.Size = new Size(ViewMenuWidth, 22);
            ViewLayoutMenuItem.Size = new Size(ViewMenuWidth, 22);
            ViewResponseKeysMenuItem.Size = new Size(ViewMenuWidth, 22);
            ViewStripSeparator.Size = new Size(ViewMenuWidth - 3, 6);
            if (CIAT.SaveFile != null)
            {
                if (CIAT.SaveFile.IAT != null)
                {
                    for (int ctr = 0; ctr < CIAT.SaveFile.IAT.Contents.Count; ctr++)
                        ViewIATItemList[ctr].Size = new Size(ViewMenuWidth, 22);
                }
            }
            if (bSuspendLayout)
            {
                this.ResumeLayout(false);
                HeaderMenu.ResumeLayout(false);
                HeaderMenu.PerformLayout();
                this.PerformLayout();
            }
        }

        /*
                private void AppendMenuItem_Click(object sender, EventArgs e)
                {
                    if (FormContents != EFormContents.Survey)
                        return;
                    if (sender.GetType() != typeof(System.Windows.Forms.ToolStripMenuItem))
                        return;
                    ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
                    switch (menuItem.Name)
                    {
                        case sAppendQuestionWithCurrentResponseMenuItemName:
                            m_SurveyPanel.AppendItemWithCurrentResponse();
                            break;

                        case sAppendBoundedLengthQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.BoundedLength);
                            break;

                        case sAppendBoundedNumberQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.BoundedNum);
                            break;

                        case sAppendDateQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.Date);
                            break;

                        case sAppendFixedDigQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.FixedDig);
                            break;

                        case sAppendFixedLengthQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.FixedLength);
                            break;

                        case sAppendInstructionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.Instruction);
                            break;

                        case sAppendLikertQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.Likert);
                            break;

                        case sAppendMaxLengthQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.MaxLength);
                            break;

                        case sAppendMultipleChoiceQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.Multiple);
                            break;

                        case sAppendMultipleSelectionQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.MultiBoolean);
                            break;

                        case sAppendRegExQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.RegEx);
                            break;

                        case sAppendTrueFalseQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.Boolean);
                            break;

                        case sAppendWeightedMultipleChoiceQuestionMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.WeightedMultiple);
                            break;

                        case sAppendQuestionValidatedByValuesInAttachedFileMenuItemName:
                            m_SurveyPanel.AppendItemWithResponseType(CResponse.EResponseType.ExistsInFile);
                            break;
                    }
                }

                private void InsertMenuItem_Click(object sender, EventArgs e)
                {
                    if (FormContents != EFormContents.Survey)
                        return;
                    if (sender.GetType() != typeof(System.Windows.Forms.ToolStripMenuItem))
                        return;
                    ToolStripMenuItem menuItem = (ToolStripMenuItem)sender;
                    switch (menuItem.Name)
                    {
                        case sInsertQuestionWithCurrentResponseMenuItemName:
                            m_SurveyPanel.InsertItemWithCurrentResponse();
                            break;

                        case sInsertBoundedLengthQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.BoundedLength);
                            break;

                        case sInsertBoundedNumberQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.BoundedNum);
                            break;

                        case sInsertDateQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.Date);
                            break;

                        case sInsertFixedDigQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.FixedDig);
                            break;

                        case sInsertFixedLengthQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.FixedLength);
                            break;

                        case sInsertInstructionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.Instruction);
                            break;

                        case sInsertLikertQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.Likert);
                            break;

                        case sInsertMaxLengthQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.MaxLength);
                            break;

                        case sInsertMultipleChoiceQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.Multiple);
                            break;

                        case sInsertMultipleSelectionQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.MultiBoolean);
                            break;

                        case sInsertRegExQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.RegEx);
                            break;

                        case sInsertTrueFalseQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.Boolean);
                            break;

                        case sInsertWeightedMultipleChoiceQuestionMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.WeightedMultiple);
                            break;

                        case sInsertQuestionValidatedByValuesInAttachedFileMenuItemName:
                            m_SurveyPanel.InsertItemWithResponseType(CResponse.EResponseType.ExistsInFile);
                            break;
                    }
                }
        */

        /// <summary>
        /// gets or sets the enable state of the Edit->Cut menu item
        /// </summary>
        public bool EditCutEnabled
        {
            get
            {
                return EditCutMenuItem.Enabled;
            }
            set
            {
                EditCutMenuItem.Enabled = value;
            }
        }

        /// <summary>
        /// gets or sets the enable state of the Edit->Copy menu item
        /// </summary>
        public bool EditCopyEnabled
        {
            get
            {
                return EditCopyMenuItem.Enabled;
            }
            set
            {
                EditCopyMenuItem.Enabled = value;
            }
        }

        /// <summary>
        /// gets or sets the enable state of the Edit->Paste menu item
        /// </summary>
        public bool EditPasteEnabled
        {
            get
            {
                return EditPasteMenuItem.Enabled;
            }
            set
            {
                EditPasteMenuItem.Enabled = value;
            }
        }

        /// <summary>
        /// gets or sets the enable state of the Edit->Delete menu item
        /// </summary>
        public bool EditDeleteEnabled
        {
            get
            {
                return EditDeleteMenuItem.Enabled;
            }
            set
            {
                EditDeleteMenuItem.Enabled = value;
            }
        }

        private void FileOpenMenuItem_Click(object sender, EventArgs e)
        {
            // File->Open has been clicked. if the currently open file has been modified, prompt for save
            if (Modified)
            {
                DialogResult r = PromptForSave();
                if (r == DialogResult.Cancel)
                    return;
                if (r == DialogResult.Yes)
                    Save();
            }


            // close the open file and open a new one
            Open();
        }


        private void FileSaveMenuItem_Click(object sender, EventArgs e)
        {
            // the File->Save menu item has been clicked so save the file
            Save();
        }

        private void FileSaveAsMenuItem_Click(object sender, EventArgs e)
        {
            // the File->SaveAs menu item has been clicked, prompt for a filename then save
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.DefaultExt = Properties.Resources.sFileExt;
            dialog.Title = Properties.Resources.sSaveFileDialogTitle;
            dialog.AddExtension = true;
            dialog.Filter = String.Format(Properties.Resources.sFileDialogFilter);
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;
            //          if (CurrentFilename != String.Empty)
            //           CIAT.ImageManager.CreatePreSaveBackup(CurrentFilename);
            CurrentFilename = dialog.FileName;
            Save(CurrentFilename);
        }


        private void FileCloseMenuItem_Click(object sender, EventArgs e)
        {
            FormContents = EFormContents.Main;
            CIAT.Create();

        }

        private void FileExitMenuItem_Click(object sender, EventArgs e)
        {
            // the File->Exit menu item has been clicked so close the window
            Close();
        }

        private void ViewIATContentsMenuItem_Click(object sender, EventArgs e)
        {
            FormContents = EFormContents.Main;
        }

        private void ViewLayoutMenuItem_Click(object sender, EventArgs e)
        {
            FormContents = EFormContents.Layout;
        }

        private void ViewDataRetrievalDialogMenuItem_Click(object sender, EventArgs e)
        {
            //            ServerInterface dlg = new ServerInterface(IAT);
            //           dlg.ShowDialog(this);
        }

        private void ViewResponseKeysMenuItem_Click(object sender, EventArgs e)
        {
            ShowResponseKeyPanel();
        }

        private void HelpIndexMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelpIndex(this, "IATClient.chm");
        }

        private void HelpContentsMenuItem_Click(object sender, EventArgs e)
        {
            Help.ShowHelp(this, "IATClient.chm");
        }

        private void ViewIATItemList_Click(object sender, EventArgs e)
        {
            int ndx = ViewIATItemList.IndexOf((ToolStripMenuItem)sender);
            FormContents = EFormContents.None;
            IContentsItem cItem = CIAT.SaveFile.IAT.Contents[ndx];
            ActiveItem = cItem;
            if ((cItem.Type == ContentsItemType.AfterSurvey) || (cItem.Type == ContentsItemType.BeforeSurvey))
                FormContents = IATConfigMainForm.EFormContents.Survey;
            if (cItem.Type == ContentsItemType.IATBlock)
                FormContents = IATConfigMainForm.EFormContents.IATBlock;
            if (cItem.Type == ContentsItemType.InstructionBlock)
                FormContents = IATConfigMainForm.EFormContents.Instructions;
        }


        #endregion

        public void LoadIAT(MemoryStream memStream)
        {

            m_MainPanel.Dispose();
            ConstructViewMenu(true);
            m_MainPanel = new MainPanel(this);
            m_MainPanel.Location = new Point(0, HeaderMenu.Height);
            m_MainPanel.PopulateContents(CIAT.SaveFile.IAT.Contents);
            if (FormContents == EFormContents.Main)
                FormContents = EFormContents.Main;
        }

        /// <summary>
        /// Displays an OpenFileDialog and loads the file the user specifies, updating the SurveyView
        /// </summary>
        public void Open()
        {
            // clear the error message

            // instantiate and initialize an OpenFileDialog
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = Properties.Resources.sFileExt;
            dialog.Title = Properties.Resources.sOpenFileDialogTitle;
            dialog.Filter = String.Format(Properties.Resources.sFileDialogFilter);
            dialog.FilterIndex = 0;
            // if the user doesn't click "Open", return
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;
            //        CloseFile(false);
            ErrorMsg = String.Empty;
            // try to load the file
            try
            {
                CIAT.SaveFile.Dispose();
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error disposing of test", ex));
            }
            try
            {
                Modified = false;
                CurrentFilename = dialog.FileName;
                if (!CIAT.Open(dialog.FileName, false, true))
                {
                    CIAT.Create();
                    return;
                }
                EndProgressBarUse();
                IATNameBox.Text = CIAT.SaveFile.IAT.Name;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error loading save file", ex));
            }
            //      ConstructViewMenu(true);
            if (m_MainPanel == null)
            {
                m_MainPanel = new MainPanel(this);
                m_MainPanel.Location = new Point(0, HeaderMenu.Height);
            }
            m_MainPanel.PopulateContents(CIAT.SaveFile.IAT.Contents);
            FirstValidation = true;
            FormContents = EFormContents.Main;
        }

        public static bool FirstValidation { get; set; } = false;

        /// <summary>
        /// Saves the IAT config file to the specified filename
        /// </summary>
        /// <param name="filename">The name of the output file</param>
        private void Save(String filename)
        {
            if (FormContents == EFormContents.Survey)
            {
                int surveyNdx = ActiveItem.IndexInContainer;
                if (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri != null)
                    if (CIAT.SaveFile.IAT.UniqueResponse.SurveyUri.Equals(ActiveItem.URI))
                        CIAT.SaveFile.IAT.UniqueResponse.SurveyUri = CIAT.SaveFile.IAT.Contents[surveyNdx].URI;
                ActiveItem.Dispose();
                ActiveItem = CIAT.SaveFile.IAT.Contents[surveyNdx];
            }
            CIAT.SaveFile.IAT.Name = IATName;
            Task.Run(() => { CIAT.SaveFile.Save(filename); });
            // set the modified flag to false
            Modified = false;
        }

        /// <summary>
        /// Saves the currently open file.  If the file is new, Save displays a SaveFileDialog, prompting the
        /// user for a filename
        /// </summary>
        public void Save()
        {
            // test to see if the test is new
            if (CurrentFilename == String.Empty)
            {
                SaveFileDialog dialog = new SaveFileDialog();
                dialog.DefaultExt = Properties.Resources.sFileExt;
                dialog.Title = Properties.Resources.sSaveFileDialogTitle;
                dialog.AddExtension = true;
                dialog.Filter = String.Format(Properties.Resources.sFileDialogFilter);
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;
                CurrentFilename = dialog.FileName;
            }
            //   else
            //        CIAT.ImageManager.CreatePreSaveBackup(CurrentFilename);

            // save the test
            Save(CurrentFilename);
        }

        /// <summary>
        /// Displays a prompt that notifies the user that the currently open test has been modified
        /// and changes will be lost if s/he proceeds.  The prompt as the user is s/he wishes to save
        /// the open test
        /// </summary>
        /// <returns>The DialogResult of the displayed MessageBox</returns>
        private DialogResult PromptForSave()
        {
            return MessageBox.Show(this, Properties.Resources.sSaveBeforeCloseQuery, Properties.Resources.sSaveBeforeCloseCaption,
                MessageBoxButtons.YesNoCancel);
        }

        /// <summary>
        /// Closes the currently open file, prompting for save if the file has been modified
        /// </summary>
        public void CloseFile(bool bCreateNew)
        {
            DialogResult saveResult = PromptForSave();
            if (saveResult == DialogResult.Yes)
                Save();
            else if (saveResult == DialogResult.Cancel)
                return;
            FormContents = EFormContents.Main;
            if (bCreateNew)
            {
                CIAT.SaveFile.Dispose();
                CIAT.Create();
            }
            m_IATBlockPanel?.Dispose();
            m_IATBlockPanel = null;
            m_InstructionPanel?.Dispose();
            m_InstructionPanel = null;
            m_ItemSlidePanel?.Dispose();
            m_ItemSlidePanel = null;
            m_LayoutPanel?.Dispose();
            m_LayoutPanel = null;
            m_SurveyPanel?.Dispose();
            m_SurveyPanel = null;

            m_MainPanel?.Dispose();
            m_MainPanel = new MainPanel(this);
            m_MainPanel.Location = new Point(0, HeaderMenu.Height);
            if (!CFontFile.Loaded)
                m_MainPanel.Enabled = false;
            Controls.Add(m_MainPanel);
        }

        static private bool _IsInShutdown = false;

        static public bool IsInShutdown
        {
            get
            {
                return _IsInShutdown;
            }
        }
        private void IATConfigMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // if the test has been modified, prompt for save, canceling the close operation if 
            // the user selects "Cancel"
            if (Modified)
            {
                DialogResult result = PromptForSave();
                if (result == DialogResult.Cancel)
                    e.Cancel = true;
                else if (result == DialogResult.Yes)
                    Save();
            }
            if (!e.Cancel)
            {
                if (ProgressBarUse != EProgressBarUses.None)
                {
                    String message = String.Empty, caption = String.Empty;
                    switch (ProgressBarUse)
                    {
                        case EProgressBarUses.Upload:
                            message = "An IAT Upload is currently in progress.  If you close the program now, that upload will be lost.  Do " +
                                "you still wish to exit?";
                            caption = "IAT Upload In Progress";
                            break;

                        case EProgressBarUses.DataRetrieval:
                            message = "Test result data is currently being retrieved.  Do you still wish to exit?";
                            caption = "Data Retrieval In Progress";
                            break;

                        case EProgressBarUses.Reencryption:
                            message = "Your data retrieval password is being changed.  This involves the decryption and reencryption of all your result data, which" +
                                " takes place on your computer to protect your password.  It is STRONGLY recommended that you not interrupt this process or all your"
                                + " data might be lost.  Do you still wish to exit?";
                            caption = "Data Retrieval Password Change In Progress";
                            break;

                        case EProgressBarUses.ItemSlideRetrieval:
                            message = "The program is currently engaged in an attempt to communicate with the server.  It should terminate.  If it does not, save your work and attempt to close the program.";
                            caption = "Waiting on Network Process";
                            break;
                    }
                    if ((ProgressBarUse != EProgressBarUses.Packaging) && (ProgressBarUse != EProgressBarUses.GeneratingFontFile))
                    {
                        if ((MessageBox.Show(this, message, caption, MessageBoxButtons.YesNo) == DialogResult.No))
                        {
                            e.Cancel = true;
                            return;
                        }
                    }
                }
                CIAT.CancelToken();
                _IsInShutdown = true;
            }
        }


        private void FinishFormClose(object sender, EventArgs e)
        {
            _IsInShutdown = true;
            this.Invoke(new Action(this.Close));
        }

        public ToolStripButton AddToolStripCancelButton(EventHandler OnClick)
        {
            ToolStripButton button = new ToolStripButton("Cancel");
            button.Click += new EventHandler(ToolstripCancelButton_Click);
            MessageBar.Items.Add(button);
            ProgressButtons.Add(button);
            return button;
        }

        private void ToolstripCancelButton_Click(object sender, EventArgs e)
        {
            CancelMessageBarProcess.Invoke(this, new EventArgs());
        }

        public enum EProgressBarUses { None, Upload, DataRetrieval, ItemSlideRetrieval, Reencryption, Packaging, ExportData, GeneratingFontFile, LoadSaveFile };
        private EProgressBarUses ProgressBarUse = EProgressBarUses.None;
        private List<ToolStripButton> ProgressButtons = new List<ToolStripButton>();
        private EventHandler CancelMessageBarProcess = null;
        public ToolStripProgressBar ProgressBar
        {
            get
            {
                return Progress;
            }
        }

        public void BeginProgressBarUse(EventHandler handler, EProgressBarUses use)
        {
            CancelMessageBarProcess = handler;
            ProgressBarUse = use;
            if ((use != EProgressBarUses.Packaging) && (use != EProgressBarUses.GeneratingFontFile) && (use != EProgressBarUses.LoadSaveFile))
                AddToolStripCancelButton(handler);
        }


        public void EndProgressBarUse()
        {
            foreach (ToolStripButton button in ProgressButtons)
                MessageBar.Items.Remove(button);
            ResetProgress();
            SetProgressRange(0, 1);
            SetStatusMessage(String.Empty);
            CancelMessageBarProcess = null;
            ProgressBarUse = EProgressBarUses.None;
        }

        public void SetProgressRange(int nMin, int nMax)
        {
            if (ProgressBarUse == EProgressBarUses.None)
                return;
            Progress.Minimum = nMin;
            Progress.Maximum = nMax;
        }

        public void ProgressIncrement(int nInc)
        {
            if (ProgressBarUse == EProgressBarUses.None)
                return;
            try
            {
                Progress.Value += nInc;
            }
            catch (Exception ex) { }
        }



        public void SetStatusMessage(String statusMessage)
        {
        }

        public String GetStatusMessage()
        {
            return MessageBar.Items["StatusMessage"].Text;
        }

        public void OperationComplete(CIATSummary summary)
        {
            this.BeginInvoke(new Action(() =>
            {
                IATUploadCompleteForm uploadComplete = new IATUploadCompleteForm();
                uploadComplete.Summary = summary;
                uploadComplete.ShowDialog();
            }));
        }

        public void ResetProgress()
        {
            if (ProgressBarUse == EProgressBarUses.None)
                return;
            Progress.Value = 0;
        }

        public void SetProgressValue(int val)
        {
            if (ProgressBarUse == EProgressBarUses.None)
                return;
            Progress.Value = val;
        }

        public void OnDisplayMessageBox(String msg, String caption)
        {
            MessageBox.Show(this, msg, caption);
        }
        /*
        public delegate void PackingOperationCompleteHandler();
        public delegate void SetProgressRangeHandler(int nMin, int nMax);
        public delegate void ProgressIncrementHandler(int nInc);
        public delegate void SetProgressValueHandler(int val);
        public delegate void SetStatusMessageHandler(String StatusMsg);
        public delegate void OperationCompleteHandler(CIATSummary Summary);
        public delegate void ResetProgressHandler();
        public delegate bool IsLoadedHandler(bool bFlagToTrue);
        public delegate DialogResult DisplayYesNoMessageBoxHandler(String msg, String caption);
        public delegate void OperationFailedHandler(String title, String message);
        public delegate void CloseDelegate();
        public delegate void EndProgressBarUseHandler();
        public delegate void BeginProgressBarUseHandler(AbortHandler CancelDelegate, EProgressBarUses use);
        public delegate void DisplayMessageBoxHandler(String msg, String caption);
        public delegate DataPasswordForm.EDataPassword DisplayDataPasswordHandler(DataPasswordForm dpf);
        public delegate void EnableControlHandler(Control c, bool enable);
        public delegate void SuspendLayoutHandler(Control c, bool bSuspend);
        public delegate void ShowFormHandler(Form f);
        public delegate void DisplayMessageBox(String msg, String caption);
        public delegate bool AbortHandler(int waitTimeout, bool forceAbort);
        */
        /*
        public static Thread ErrorReportThread { get; private set; } = null;
        private static ManualResetEvent ErrorReportReset = new ManualResetEvent(true);
        public static void ShowErrorReport(String caption, CReportableException reportedException)
        {
            Task.Run(() =>
            {
                ErrorReportReset.WaitOne();
                ErrorReportReset.Reset();
                Point ptSplash = (Application.OpenForms[Properties.Resources.sMainFormName] != null) ?
                    Application.OpenForms[Properties.Resources.sMainFormName].Location : new Point(200, 200);
                ErrorReportThread = new Thread(new ThreadStart(() =>
                {
                    ErrorReportSplash spl = new ErrorReportSplash()
                    {
                        Topmost = true,
                        Left = ptSplash.X + 150,
                        Top = ptSplash.Y + 50
                    };
                    try
                    {
                        spl.Show();
                        WebClient downloader = new WebClient();
                        downloader.Headers.Add("ProductKey", LocalStorage.Activation[LocalStorage.Field.ProductKey]);
                        Task<String> getChallenge = Task<String>.Run(() =>
                        {
                            try
                            {
                                return downloader.DownloadString(Properties.Resources.sErrorReportURL);
                            }
                            catch (Exception ex)
                            {
                                return "failure";
                            }
                        });
                        getChallenge.Wait(10000);
                        if (!getChallenge.IsCompleted || (getChallenge.Result == "failure"))
                        {
                            spl.Close();
                            ErrorReportDisplay f = new ErrorReportDisplay(reportedException);
                            f.ShowDialog();
                            return;
                        }
                        String challenge = getChallenge.Result;
                        WebClient uploader = new WebClient();
                        try
                        {
                            RSACryptoServiceProvider rsa = new RSACryptoServiceProvider();
                            rsa.ImportCspBlob(Convert.FromBase64String(Properties.Resources.ErrorReportCspBlob));
                            String response = Convert.ToBase64String(rsa.Decrypt(Convert.FromBase64String(challenge), RSAEncryptionPadding.Pkcs1));
                            CClientException clientEx = new CClientException(caption, reportedException);
                            uploader.Headers.Add(HttpRequestHeader.ContentType, "text/xml");
                            uploader.Headers.Add("ProductKey", LocalStorage.Activation[LocalStorage.Field.ProductKey]);
                            uploader.Headers.Add("response", response);
                        }
                        catch (CryptographicException cEx)
                        {
                            MessageBox.Show(Properties.Resources.sErrorReportHandshakeFailedMessage, Properties.Resources.sErrorReportHandshakeFailedCaption);
                        }
                        catch (Exception ex)
                        {
                            ErrorsReported--;
                            ErrorReportDisplay f = new ErrorReportDisplay(reportedException);
                            f.ShowDialog();
                            return;
                        }
                        Task<ErrorReportResponse> reportError = Task<String>.Run(() =>
                            {
                                try
                                {
                                    MemoryStream serverResponse = new MemoryStream(uploader.UploadData(Properties.Resources.sErrorReportURL, new CClientException(reportedException.ExceptionMessage, reportedException).GetXmlBytes()));
                                    XmlSerializer ser = new XmlSerializer(typeof(ErrorReportResponse));
                                    return ser.Deserialize(serverResponse) as ErrorReportResponse;
                                }
                                catch (Exception errorReportEx)
                                {
                                    return new ErrorReportResponse()
                                    {
                                        Response = ErrorReportResponse.EResponseCode.serverError
                                    };
                                }
                            });
                        reportError.Wait(30000);
                        spl.Close();
                        if (!reportError.IsCompleted)
                        {
                            ErrorsReported--;
                            ErrorReportDisplay f = new ErrorReportDisplay(reportedException);
                            f.ShowDialog();
                        }
                        else if (reportError.Result.Response == ErrorReportResponse.EResponseCode.success)
                            MessageBox.Show(String.Format(Properties.Resources.sErrorReportedMessage, LocalStorage.Activation[LocalStorage.Field.ProductKey]), Properties.Resources.sErrorReportedCaption);
                        else if (reportError.Result.Caption != String.Empty)
                            MessageBox.Show(reportError.Result.Message, reportError.Result.Caption);
                    }
                    catch (Exception e)
                    {
                        spl.Close();
                        ErrorsReported--;
                        ErrorReportDisplay f = new ErrorReportDisplay(reportedException);
                        f.ShowDialog();
                    }
                    finally
                    {
                        ErrorReportReset.Set();
                    }
                }));
                ErrorReportThread.SetApartmentState(ApartmentState.STA);
                ErrorReportThread.Start();
            });
        }
        */


        private void UploadButton_Click(object sender, EventArgs e)
        {
            if (CIAT.SaveFile.IAT.Blocks.Count != 7)
            {
                MessageBox.Show(this, "Your test contains fewer than 7 IAT blocks. Only 7-Block tests are supported by this software.", "Too few blocks");
                return;
            }
            CItemValidator.StartValidation();
            foreach (CIATBlock b in CIAT.SaveFile.IAT.Blocks)
                CItemValidator.ValidateItem(b);
            foreach (CInstructionBlock ib in CIAT.SaveFile.IAT.InstructionBlocks)
                CItemValidator.ValidateItem(ib);
            if (CItemValidator.HasErrors)
            {
                if (FormContents == EFormContents.Main)
                    MessageBox.Show(this, "Your IAT cannot be uploaded because it contains errors.  Please check the list of errors at the bottom of the screen " +
                        "and correct them.", "IAT Errors Exist");
                else
                    MessageBox.Show(this, "Your IAT cannot be uploaded because it contains errors.  You may return to the main interface to see the list of " +
                        "errors that are preventing your IAT from being uploaded.", "IAT Errors Exist");
                return;
            }
            if (IATName.Length == 0)
            {
                MessageBox.Show("Please enter a name for your IAT.", "Invalid IAT Name");
                return;
            }
            for (int ctr = 0; ctr < IATName.Length; ctr++)
                if (!Char.IsLetterOrDigit(IATName[ctr]) && (IATName[ctr] != '_') && (IATName[ctr] != '-'))
                {
                    MessageBox.Show("Your IAT Name may contain only letter, numerical digits, underscores, and hyphens.", "Invalid IAT Name");
                    return;
                }
            if (IATPasswordBox.Text.Length < 4)
            {
                MessageBox.Show("Please enter a password of at least four characters.", "Invalid Password");
                return;
            }
            else { 

                var bytes = MD5.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(IATPasswordBox.Text));
                byte[] des = new List<byte>(bytes).Where((b, ndx) => ndx < 8).ToArray();
                byte[] iv = new List<byte>(bytes).Where((b, ndx) => ndx >= 8).ToArray();
                try
                {
                    var crypt = new DESCryptoServiceProvider();
                    crypt.CreateEncryptor(des, iv);
                }
                catch (CryptographicException ex)
                {
                    MessageBox.Show("This password cannot be translated into a strong encryption key. Please chooose another.");
                    IATPasswordBox.Text = String.Empty;
                    return;
                }

            }
            UploadForm upForm = new UploadForm(CIAT.SaveFile.IAT, false);
            if (upForm.ShowDialog(this) == DialogResult.Cancel)
                return;
            if (MessageBox.Show("Would you like to save this password to your computer's registry? Keep in mind that your password is not saved on the IATSoftware.net server " +
                "in order to ensure the privacy of your data. If you lose or forget your password, your data will be irretrievable. Storing your password to your registry will " +
                "allow you to retrieve your data form this computer even if you lose your password.", "Save password to registry?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                StorePasswordToRegistry = true;
            else
                StorePasswordToRegistry = false;
            Task.Run(() =>
            {
                try
                {
                    var password = "secret:" + BitConverter.ToString(MD5.Create().ComputeHash(
                        System.Text.Encoding.UTF8.GetBytes(IATPasswordBox.Text)));
                    var IATUploader = new CWebSocketUploader(CIAT.SaveFile.IAT, this);
                    if (IATUploader.Upload(CIAT.SaveFile.IAT.Name, password))
                    {
                        if (StorePasswordToRegistry)
                        {
                            LocalStorage.SetIATPassword(IATName, password);
                        }
                    }
                    StatusMessage = String.Empty;
                }
                catch (Exception ex)
                {
                    ErrorReporter.ReportError(new CReportableException("Error uploading IAT", ex));
                }
            });

        }

        private void UploadComplete(bool result)
        {
            try
            {
            }
            catch (Exception ex)
            {
                ErrorReportDisplay f = new ErrorReportDisplay(new CReportableException(ex.Message, ex));
            }
        }

        public String IATName
        {
            get
            {
                return IATNameBox.Text;
            }
        }

        public String AdminPassword
        {
            get
            {
                return IATPasswordBox.Text;
            }
        }

        public String DataRetrievalPassword
        {
            get
            {
                return IATPasswordBox.Text;
            }
        }

        private CItemSlideContainer _ItemSlides = null;
        private bool bSlideRetrievalInProcess = false;

        public bool SlideRetrievalInProcess
        {
            get
            {
                {
                    return bSlideRetrievalInProcess;
                }
            }
        }

        public CItemSlideContainer ItemSlideContainer
        {
            get
            {
                return _ItemSlides;
            }
        }
    }
}
