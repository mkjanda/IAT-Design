using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;
using System.Threading;
using System.Reflection;

namespace IATClient
{

    /// <summary>
    /// The class of the main window of the application
    /// </summary>
    public partial class IATConfigMainForm : Form
    {
        public const String ServerPassword = "sONGSoNtHEdEATHSoFcHILDREN";
        public static SizeF ScreenDPI = new SizeF();
        private CWebSocketUploader IATUploader;
        // the file that contains the fonts
        static private CFontFile FontFile;
        private bool StorePasswordToRegistry;

        /// <summary>
        /// gets the list of available fonts
        /// </summary>
        /// 
        static public CFontFile.FontData[] AvailableFonts
        {
            get
            {
                return FontFile.AvailableFonts;
            }
        }

        // the IAT document class
        private CIAT _IAT;

        // the current active item in the IAT
        private IContentsItem _ActiveItem;

        private static List<CCompositeImage> _OpenCompositeImages = new List<CCompositeImage>();

        public static List<CCompositeImage> OpenCompositeImages
        {
            get
            {
                return _OpenCompositeImages;
            }
        }

        public IContentsItem ActiveItem
        {
            get
            {
                return _ActiveItem;
            }
            set
            {
                _ActiveItem = value;
            }
        }


        /// <summary>
        /// gets the IAT document class
        /// </summary>
        public CIAT IAT
        {
            get
            {
                return _IAT;
            }
        }

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
                switch (_FormContents)
                {
                    case EFormContents.Main:
                        Controls.Remove(m_MainPanel);
                        QuickPanel.Enabled = false;
                        Controls.Remove(QuickPanel);
                        break;

                    case EFormContents.Survey:
                        Controls.Remove(m_SurveyPanel);
                        if (m_SurveyPanel.Ordinality == NewSurveyPanel.EOrdinality.before)
                        {
                            int SurveyNdx = ActiveItem.IndexInContainer;
                            IAT.BeforeSurvey[SurveyNdx].Items.Clear();
                            IAT.BeforeSurvey[SurveyNdx].Items.AddRange(m_SurveyPanel.SurveyItems);
                            foreach (CSurveyItem si in IAT.BeforeSurvey[SurveyNdx].Items)
                                si.ParentSurvey = IAT.BeforeSurvey[SurveyNdx];
                            IAT.BeforeSurvey[SurveyNdx].Timeout = m_SurveyPanel.SurveyTimeout;
                            if (m_SurveyPanel.UniqueResponseItemNum != -1)
                            {
                                IAT.UniqueResponse.SurveyName = ActiveItem.Name;
                                IAT.UniqueResponse.ItemNum = IAT.BeforeSurvey[SurveyNdx].Items[m_SurveyPanel.UniqueResponseItemNum].GetItemNum();
                                IAT.UniqueResponse.IndexInSurvey = m_SurveyPanel.UniqueResponseItemNum;
                                if (IAT.BeforeSurvey[SurveyNdx].Items[0].IsCaption)
                                    IAT.UniqueResponse.IndexInSurvey--;
                                IAT.UniqueResponse.Values.Clear();
                                IAT.UniqueResponse.SetValues(m_SurveyPanel.UniqueResponseValues);
                            }
                            else if ((IAT.UniqueResponse.SurveyName == ActiveItem.Name) && (m_SurveyPanel.UniqueResponseItemNum == -1))
                            {
                                IAT.UniqueResponse.Clear();
                            }
                        }
                        else
                        {
                            int SurveyNdx = ActiveItem.IndexInContainer;
                            IAT.AfterSurvey[SurveyNdx].Items.Clear();
                            IAT.AfterSurvey[SurveyNdx].Items.AddRange(m_SurveyPanel.SurveyItems);
                            foreach (CSurveyItem si in IAT.AfterSurvey[SurveyNdx].Items)
                                si.ParentSurvey = IAT.AfterSurvey[SurveyNdx];
                            IAT.AfterSurvey[SurveyNdx].Timeout = m_SurveyPanel.SurveyTimeout;
                            if (m_SurveyPanel.UniqueResponseItemNum != -1)
                            {
                                IAT.UniqueResponse.SurveyName = ActiveItem.Name;
                                IAT.UniqueResponse.ItemNum = IAT.AfterSurvey[SurveyNdx].Items[m_SurveyPanel.UniqueResponseItemNum].GetItemNum();
                                IAT.UniqueResponse.IndexInSurvey = m_SurveyPanel.UniqueResponseItemNum;
                                if (IAT.AfterSurvey[SurveyNdx].Items[0].IsCaption)
                                    IAT.UniqueResponse.IndexInSurvey--;
                                IAT.UniqueResponse.Values.Clear();
                                IAT.UniqueResponse.SetValues(m_SurveyPanel.UniqueResponseValues);
                            }
                            else if ((IAT.UniqueResponse.SurveyName == ActiveItem.Name) && (m_SurveyPanel.UniqueResponseItemNum == -1))
                            {
                                IAT.UniqueResponse.Clear();
                            }
                        }
                        break;

                    case EFormContents.IATBlock:
                        if (!m_IATBlockPanel.Validate())
                            return;
                        if (m_IATBlockPanel.Block != null)
                        {
                            int ndx = IAT.Blocks.IndexOf((CIATBlock)ActiveItem);
                            IAT.Blocks.RemoveAt(ndx);
                            IAT.Blocks.Insert(ndx, m_IATBlockPanel.Block);
                            ndx = IAT.Contents.IndexOf(ActiveItem);
                            IAT.Contents.RemoveAt(ndx);
                            IAT.Contents.Insert(ndx, m_IATBlockPanel.Block);
                        }
                        Controls.Remove(m_IATBlockPanel);
                        break;

                    case EFormContents.Instructions:
                        if (!m_InstructionPanel.Validate())
                            return;
                        if (m_InstructionPanel.InstructionBlock != null)
                        {
                            int ndx = IAT.InstructionBlocks.IndexOf((CInstructionBlock)ActiveItem);
                            IAT.InstructionBlocks.RemoveAt(ndx);
                            IAT.InstructionBlocks.Insert(ndx, m_InstructionPanel.InstructionBlock);
                            ndx = IAT.Contents.IndexOf(ActiveItem);
                            IAT.Contents.RemoveAt(ndx);
                            IAT.Contents.Insert(ndx, m_InstructionPanel.InstructionBlock);
                        }
                        Controls.Remove(m_InstructionPanel);
                        break;

                    case EFormContents.Layout:
                        Controls.Remove(m_LayoutPanel);
                        CIAT.SetLayout(m_LayoutPanel.FinalLayout);
                        IAT.ResizeImagesToLayout();
                        break;

                    case EFormContents.DynamicallyKey:
                        Controls.Remove(m_DynamicIATPanel);
                        break;

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
                        break;

                    case EFormContents.PurchasePage:
                        Controls.Remove(m_PurchasePanel);
                        m_PurchasePanel.Dispose();
                        break;
                }
                switch (value)
                {
                    case EFormContents.Main:
                        if (ActiveItem != null)
                            m_MainPanel.UpdateContentsItem(ActiveItem);
                        ActiveItem = null;
                        EditMenu.Enabled = false;
                        QuickPanel.Enabled = true;
                        Controls.Add(QuickPanel);
                        ShowMainPanel();
                        break;

                    case EFormContents.Survey:
                        EditMenu.Enabled = false;
                        if (ActiveItem.Type == ContentsItemType.AfterSurvey)
                            ShowSurveyPanel(NewSurveyPanel.EOrdinality.after, ActiveItem);
                        else if (ActiveItem.Type == ContentsItemType.BeforeSurvey)
                            ShowSurveyPanel(NewSurveyPanel.EOrdinality.before, ActiveItem);
                        else
                            throw new Exception();
                        break;

                    case EFormContents.IATBlock:
                        EditMenu.Enabled = true;
                        EditCutEnabled = false;
                        EditDeleteEnabled = false;
                        EditPasteEnabled = false;
                        EditCopyEnabled = true;
                        ShowIATBlockPanel(ActiveItem);
                        break;

                    case EFormContents.Instructions:
                        EditMenu.Enabled = true;
                        EditCutEnabled = false;
                        EditDeleteEnabled = false;
                        EditPasteEnabled = false;
                        EditCopyEnabled = true;
                        ShowInstructionsPanel(ActiveItem);
                        break;

                    case EFormContents.Layout:
                        EditMenu.Enabled = false;
                        ShowLayoutPanel();
                        break;

                    case EFormContents.DynamicallyKey:
                        EditMenu.Enabled = false;
                        ShowDynamicKeyPanel(ActiveItem);
                        break;

                    case EFormContents.ServerInterface:
                        ShowResultsPanel();
                        EditMenu.Enabled = false;
                        break;

                    case EFormContents.PurchasePage:
                        ShowPurchasePage();
                        EditMenu.Enabled = false;
                        break;
                }
                _PreviousFormContents = _FormContents;
                _FormContents = value;
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
                // display the appropriate image and message at the bottom of the window and enable or disable the 
                // appropriate controls
                if (_ErrorMsg != String.Empty)
                {
                    _HasErrors = true;
                    MessageBar.Items["StatusText"].Text = _ErrorMsg;
                    MessageBar.Items["StatusImage"].ImageIndex = 1;
                    //           FileSaveAsMenuItem.Enabled = false;
                    //          FileSaveMenuItem.Enabled = false;
                    /*
                                        if (m_SurveyPanel != null)
                                        {
                                            m_SurveyPanel.ChangeContextMenuInsertState(false);
                                            if (m_SurveyPanel.HasFocusedItem)
                                            {
                                                InsertMenu.Enabled = false;
                                                AppendMenu.Enabled = false;
                                            }
                                            else
                                                InsertMenu.Enabled = false;
                                        }
                     */
                    //                    ViewMenu.Enabled = false;
                }
                else
                {
                    _HasErrors = false;
                    MessageBar.Items["StatusText"].Text = "Okay";
                    MessageBar.Items["StatusImage"].ImageIndex = 0;
                    //              FileSaveAsMenuItem.Enabled = true;
                    //                    if (m_SurveyPanel != null)
                    //                        m_SurveyPanel.ChangeContextMenuInsertState(true);
                    //            if (Modified)
                    //           {
                    //             FileSaveMenuItem.Enabled = true;
                    //           if (m_SurveyPanel != null)
                    //         {
                    //                            InsertMenu.Enabled = true;
                    //                            AppendMenu.Enabled = true;
                    //       }
                    //    }
                    //                    ViewMenu.Enabled = true;
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
        public NewSurveyPanel m_SurveyPanel;

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
        public DynamicIATPanel m_DynamicIATPanel;
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
            InitializeHeaderMenu();
            InitializeComponent();
            IATNameBox.TextChanged += new EventHandler(IATNameBox_TextChanged);
            Name = Properties.Resources.sMainFormName;
            // perform one-time initialization
            FontFile = new CFontFile();
            this.Shown += new EventHandler(IATConfigMainForm_Shown);
            _ActiveItem = null;
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
            _IAT.Name = IATNameBox.Text;
        }
        /*
                private Label MainWinStatusLabel = null, MainWinStatusSubLabel = null;
                private bool bFontFileDone = false;
        */

        private void IATConfigMainForm_Shown(object sender, EventArgs e)
        {
            Graphics G = Graphics.FromHwnd(this.Handle);
            ImageManager.CImageManager.ScreenDPI = new SizeF(G.DpiX, G.DpiY);
            _IAT = new CIAT();
            FormContents = EFormContents.Main;
            if (!FontFile.TryLoad())
            {
                ThreadStart proc = new ThreadStart(LoadFontFile);
                Thread th = new Thread(proc);
                th.Start();
            }
            // set view state
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
            Invoke(enableControl, HeaderMenu, false);
            Invoke(enableControl, m_MainPanel, false);
            FontFile.Generate();
            FontFile.Load();
            Invoke(enableControl, HeaderMenu, true);
            Invoke(enableControl, m_MainPanel, true);
        }

        public void OperationFailed(String msg, String caption)
        {
            MessageBox.Show(this, msg, caption);
            EndProgressBarUse();
        }

        private void OperationComplete(String msg, String caption)
        {
            lock (lockObject)
            {
                MessageBox.Show(msg, caption, MessageBoxButtons.OK);
            }
        }

        private void IATConfigMainForm_Load(object sender, EventArgs e)
        {

            BuildMenu();
            CActivation activation = new CActivation();
            if (activation.Activated == CActivation.EActivationStatus.NotActivated)
            {
                ActivationDialog dlg = new ActivationDialog();
                dlg.ShowDialog();
                if (dlg.ProductActivated != CActivation.EActivationStatus.Activated)
                    this.Close();
            }
            else if (activation.Activated == CActivation.EActivationStatus.EMailNotVerified)
            {
                SendConfirmationEMailForm sendEMailForm = new SendConfirmationEMailForm();
                if (sendEMailForm.ShowDialog() != DialogResult.OK)
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
                //                m_MainPanel.Size = MainPanel.MainPanelSize;
            }
            Controls.Add(m_MainPanel);
        }

        /// <summary>
        /// Displays the SurveyPanel, creating it if necessary
        /// </summary>
        private void ShowSurveyPanel(NewSurveyPanel.EOrdinality Ordinality, IContentsItem SurveyItem)
        {
            int surveyNdx = -1;
            if (Ordinality == NewSurveyPanel.EOrdinality.before)
                surveyNdx = IAT.BeforeSurvey.IndexOf((CSurvey)SurveyItem);
            else
                surveyNdx = IAT.AfterSurvey.IndexOf((CSurvey)SurveyItem);
            if (m_SurveyPanel == null)
            {
                m_SurveyPanel = new NewSurveyPanel(Ordinality, surveyNdx);
                m_SurveyPanel.Location = new Point(0, HeaderMenu.Height);
                m_SurveyPanel.Size = new Size(this.ClientSize.Width, this.ClientSize.Height - HeaderMenu.Height - MessageBar.Height);
            }
            m_SurveyPanel.Ordinality = Ordinality;
            m_SurveyPanel.SurveyNdx = surveyNdx;
            Controls.Add(m_SurveyPanel);
            if (Ordinality == NewSurveyPanel.EOrdinality.before)
            {
                m_SurveyPanel.SurveyItems = IAT.BeforeSurvey[surveyNdx].Items;
                m_SurveyPanel.SurveyTimeout = IAT.BeforeSurvey[surveyNdx].Timeout;
                if (IAT.UniqueResponse.SurveyName == ActiveItem.Name)
                {
                    m_SurveyPanel.UniqueResponseValues.Clear();
                    m_SurveyPanel.UniqueResponseValues.AddRange(IAT.UniqueResponse.Values);
                }
            }
            else
            {
                m_SurveyPanel.SurveyItems = IAT.AfterSurvey[surveyNdx].Items;
                m_SurveyPanel.SurveyTimeout = IAT.AfterSurvey[surveyNdx].Timeout;
                if (IAT.UniqueResponse.SurveyName == ActiveItem.Name)
                {
                    m_SurveyPanel.UniqueResponseValues.Clear();
                    m_SurveyPanel.UniqueResponseValues.AddRange(IAT.UniqueResponse.Values);
                }
            }
        }

        /// <summary>
        /// Displays the IATBlockPanel, creating it if necessary
        /// </summary>
        /// <param name="Block">The CIATBlock object to be edited by the control</param>
        private void ShowIATBlockPanel(IContentsItem Block)
        {
            if (m_IATBlockPanel == null)
            {
                m_IATBlockPanel = new IATBlockPanel();
                m_IATBlockPanel.Location = new Point(0, HeaderMenu.Height);
            }
            m_IATBlockPanel.Block = (CIATBlock)Block;
            Controls.Add(m_IATBlockPanel);
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
        }

        /// <summary>
        /// Displays the ResponseKeyPanel, creating if necessary
        /// </summary>
        public void ShowResponseKeyPanel()
        {
            ResponseKeyDialog dlg = new ResponseKeyDialog(IAT);
            dlg.ShowDialog(this);
        }
        
        /// <summary>
        /// Displays the InstructionPanel, creating if necessary
        /// </summary>
        /// <param name="InstructionBlock">The block of instructions to be edited by the control</param>
        private void ShowInstructionsPanel(IContentsItem InstructionBlock)
        {
            if (m_InstructionPanel == null)
            {
                m_InstructionPanel = new InstructionScreenPanel();
                m_InstructionPanel.Location = new Point(0, HeaderMenu.Height);
                m_InstructionPanel.Size = InstructionScreenPanel.InstructionScreenPanelSize;
            }
            m_InstructionPanel.InstructionBlock = (CInstructionBlock)InstructionBlock;
            ActiveItem = InstructionBlock;
            Controls.Add(m_InstructionPanel);
        }

        private void ShowResultsPanel()
        {
            if (m_ResultsPanel == null)
            {
                m_ResultsPanel = new ResultsPanel();
                m_ResultsPanel.Location = new Point(0, 0);
            }
            Controls.Add(m_ResultsPanel);
            m_ResultsPanel.Initialize();
        }

        public void HideResultsPanel()
        {
            m_ResultsPanel.Clear();
            Controls.Remove(m_ResultsPanel);
        }

        public void SetActiveInstructionScreen(CInstructionScreen screen)
        {
            if (!Controls.Contains(m_InstructionPanel))
                return;
            m_InstructionPanel.SetActiveScreen(screen);
        }

        /// <summary>
        /// Displays the Layout Panel, creating it if necessary
        /// </summary>
        private void ShowLayoutPanel()
        {
            if (m_LayoutPanel == null)
            {
                m_LayoutPanel = new LayoutPanel();
                m_LayoutPanel.Location = new Point(0, HeaderMenu.Height);
                m_LayoutPanel.Size = this.ClientSize - new Size(0, HeaderMenu.Height + MessageBar.Height);
            }
            Controls.Add(m_LayoutPanel);
        }

        private void ShowDynamicKeyPanel(IContentsItem activeItem)
        {
            m_DynamicIATPanel = new DynamicIATPanel();
            m_DynamicIATPanel.Location = new Point(0, HeaderMenu.Height);
            m_DynamicIATPanel.Size = this.ClientSize - new Size(0, HeaderMenu.Height + MessageBar.Height);
            m_DynamicIATPanel.Block = (CIATBlock)activeItem;
            m_DynamicIATPanel.Surveys = IAT.BeforeSurvey;
            Controls.Add(m_DynamicIATPanel);
        }
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

        // the server menu 
        /*
        private System.Windows.Forms.ToolStripMenuItem ServerMenu;
        private System.Windows.Forms.ToolStripSeparator ServerSeparator;
        private System.Windows.Forms.ToolStripMenuItem ServerRetrieveItemSlidesItem;
        private System.Windows.Forms.ToolStripMenuItem ServerDeleteIATMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ServerDeleteIATDataMenuItem;
        private System.Windows.Forms.ToolStripMenuItem ServerServerInterfaceMenuItem;
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
            this.FileCloseMenuItem.Click += new System.EventHandler(this.FileCloseMenuItem_Click);
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
            // EditMenu
            //
            this.EditMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.EditMenu.Name = sEditMenuName;
            this.EditMenu.Size = new System.Drawing.Size(39, 20);
            this.EditMenu.Text = "&Edit";
            this.EditMenu.Enabled = false;
            //
            // EditCutMenuItem
            //
            this.EditCutMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditCutMenuItem.Name = sEditCutMenuItemName;
            this.EditCutMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.X)));
            this.EditCutMenuItem.Size = new System.Drawing.Size(158, 22);
            this.EditCutMenuItem.Text = "Cu&t";
            this.EditCutMenuItem.Click += new System.EventHandler(this.EditCutMenuItem_Click);
            this.EditCutMenuItem.Enabled = false;
            //
            // EditCopyMenuItem
            //
            this.EditCopyMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditCopyMenuItem.Name = sEditCopyMenuItemName;
            this.EditCopyMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C)));
            this.EditCopyMenuItem.Size = new System.Drawing.Size(158, 22);
            this.EditCopyMenuItem.Text = "&Copy";
            this.EditCopyMenuItem.Click += new System.EventHandler(this.EditCopyMenuItem_Click);
            this.EditCopyMenuItem.Enabled = false;
            //
            // EditPasteMenuItem
            //
            this.EditPasteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditPasteMenuItem.Name = sEditPasteMenuItemName;
            this.EditPasteMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.V)));
            this.EditPasteMenuItem.Size = new System.Drawing.Size(158, 22);
            this.EditPasteMenuItem.Text = "&Paste";
            this.EditPasteMenuItem.Click += new System.EventHandler(this.EditPasteMenuItem_Click);
            this.EditPasteMenuItem.Enabled = false;
            //
            // EditDeleteMenuItem
            //
            this.EditDeleteMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.EditDeleteMenuItem.Name = sEditDeleteMenuItemName;
            this.EditDeleteMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.Delete)));
            this.EditDeleteMenuItem.Size = new System.Drawing.Size(158, 22);
            this.EditDeleteMenuItem.Text = "D&elete";
            this.EditDeleteMenuItem.Click += new System.EventHandler(this.EditDeleteMenuItem_Click);
            this.EditDeleteMenuItem.Enabled = false;

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
            // ViewStripSeparator
            //
            this.ViewStripSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.ViewStripSeparator.Name = sViewStripSeparator;
            /*
                        //
                        // AppendMenu
                        //
                        this.AppendMenu = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendMenu.Name = sAppendMenuName;
                        this.AppendMenu.Size = new System.Drawing.Size(61, 20);
                        this.AppendMenu.Text = "&Append";
                        this.AppendMenu.Enabled = false;

                        //
                        // AppendQuestionWithCurrentResponseMenuItem
                        //
                        this.AppendQuestionWithCurrentResponseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendQuestionWithCurrentResponseMenuItem.Name = sAppendQuestionWithCurrentResponseMenuItemName;
                        this.AppendQuestionWithCurrentResponseMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.N)));
                        this.AppendQuestionWithCurrentResponseMenuItem.Size = new System.Drawing.Size(316, 22);
                        this.AppendQuestionWithCurrentResponseMenuItem.Text = "&Question with Current Response Type";
                        this.AppendQuestionWithCurrentResponseMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendQuestionWithCurrentResponseMenuItem.Enabled = false;
                        //
                        // AppendStripSeparator
                        //
                        this.AppendStripSeparator = new System.Windows.Forms.ToolStripSeparator();
                        this.AppendStripSeparator.Name = sAppendStripSeparatorName;
                        this.AppendStripSeparator.Size = new System.Drawing.Size(313, 6);
                        //
                        // AppendDateQuestionMenuItem
                        //
                        this.AppendDateQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendDateQuestionMenuItem.Name = sAppendDateQuestionMenuItemName;
                        this.AppendDateQuestionMenuItem.Size = new System.Drawing.Size(316, 22);
                        this.AppendDateQuestionMenuItem.Text = "Date Response Question";
                        this.AppendDateQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendDateQuestionMenuItem.Enabled = false;
                        //
                        // AppendInstructionMenuItem
                        //
                        this.AppendInstructionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendInstructionMenuItem.Name = sAppendInstructionMenuItemName;
                        this.AppendInstructionMenuItem.Size = new System.Drawing.Size(316, 22);
                        this.AppendInstructionMenuItem.Text = "Instruction";
                        this.AppendInstructionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendInstructionMenuItem.Enabled = false;
                        //
                        // AppendLikertQuestionMenuItem
                        //
                        this.AppendLikertQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendLikertQuestionMenuItem.Name = sAppendLikertQuestionMenuItemName;
                        this.AppendLikertQuestionMenuItem.Size = new System.Drawing.Size(316, 22);
                        this.AppendLikertQuestionMenuItem.Text = "Likert Response Question";
                        this.AppendLikertQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendLikertQuestionMenuItem.Enabled = false;
                        //
                        // AppendMultipleChoiceQuestionMenuItem
                        //
                        this.AppendMultipleChoiceQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendMultipleChoiceQuestionMenuItem.Name = sAppendMultipleChoiceQuestionMenuItemName;
                        this.AppendMultipleChoiceQuestionMenuItem.Size = new System.Drawing.Size(316, 22);
                        this.AppendMultipleChoiceQuestionMenuItem.Text = "Multiple Choice Response Question";
                        this.AppendMultipleChoiceQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendMultipleChoiceQuestionMenuItem.Enabled = false;
                        //
                        // AppendMultipleSelectionQuestionMenuItem
                        //
                        this.AppendMultipleSelectionQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendMultipleSelectionQuestionMenuItem.Name = sAppendMultipleSelectionQuestionMenuItemName;
                        this.AppendMultipleSelectionQuestionMenuItem.Size = new System.Drawing.Size(316, 22);
                        this.AppendMultipleSelectionQuestionMenuItem.Text = "Multiple Selection Question";
                        this.AppendMultipleSelectionQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendMultipleSelectionQuestionMenuItem.Enabled = false;
                        //
                        // AppendBoundedLengthQuestionMenuItem
                        //
                        this.AppendBoundedLengthQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendBoundedLengthQuestionMenuItem.Name = sAppendBoundedLengthQuestionMenuItemName;
                        this.AppendBoundedLengthQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.AppendBoundedLengthQuestionMenuItem.Text = "Bounded Length Response";
                        this.AppendBoundedLengthQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendBoundedLengthQuestionMenuItem.Enabled = false;
                        //
                        // AppendBoundedNumberQuestionMenuItem
                        //
                        this.AppendBoundedNumberQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendBoundedNumberQuestionMenuItem.Name = sAppendBoundedNumberQuestionMenuItemName;
                        this.AppendBoundedNumberQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.AppendBoundedNumberQuestionMenuItem.Text = "Bounded Number Response";
                        this.AppendBoundedNumberQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendBoundedNumberQuestionMenuItem.Enabled = false;
                        //
                        // AppendFixedDigQuestionMenuItem
                        //
                        this.AppendFixedDigQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendFixedDigQuestionMenuItem.Name = sAppendFixedDigQuestionMenuItemName;
                        this.AppendFixedDigQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.AppendFixedDigQuestionMenuItem.Text = "Fixed Number of Digits";
                        this.AppendFixedDigQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendFixedDigQuestionMenuItem.Enabled = false;
                        //
                        // AppendFixedLengthResponseMenuItem
                        //
                        this.AppendFixedLengthQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendFixedLengthQuestionMenuItem.Name = sAppendFixedLengthQuestionMenuItemName;
                        this.AppendFixedLengthQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.AppendFixedLengthQuestionMenuItem.Text = "Fixed Length Text";
                        this.AppendFixedLengthQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendFixedLengthQuestionMenuItem.Enabled = false;
                        //
                        // AppendMaxLengthQuestionMenuItem
                        //
                        this.AppendMaxLengthQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendMaxLengthQuestionMenuItem.Name = sAppendMaxLengthQuestionMenuItemName;
                        this.AppendMaxLengthQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.AppendMaxLengthQuestionMenuItem.Text = "Text with Maximum Length";
                        this.AppendMaxLengthQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendMaxLengthQuestionMenuItem.Enabled = false;
                        //
                        // AppendRegExQuestionMenuItem
                        //
                        this.AppendRegExQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendRegExQuestionMenuItem.Name = sAppendRegExQuestionMenuItemName;
                        this.AppendRegExQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.AppendRegExQuestionMenuItem.Text = "Valided by Regular Expression";
                        this.AppendRegExQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendRegExQuestionMenuItem.Enabled = false;
                        //
                        // AppendTextOrNumberQuestionMenuItem
                        //
                        this.AppendTextOrNumberQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendTextOrNumberQuestionMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                        this.AppendBoundedLengthQuestionMenuItem,
                        this.AppendBoundedNumberQuestionMenuItem,
                        this.AppendFixedDigQuestionMenuItem,
                        this.AppendFixedLengthQuestionMenuItem,
                        this.AppendMaxLengthQuestionMenuItem,
                        this.AppendRegExQuestionMenuItem});
                        this.AppendTextOrNumberQuestionMenuItem.Name = sAppendTextOrNumberQuestionMenuItemName;
                        this.AppendTextOrNumberQuestionMenuItem.Size = new System.Drawing.Size(316, 22);
                        this.AppendTextOrNumberQuestionMenuItem.Text = "Text or Number Response Question";
                        this.AppendTextOrNumberQuestionMenuItem.Enabled = false;
                        //
                        // AppendTrueFalseQuestionMenuItem
                        //
                        this.AppendTrueFalseQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendTrueFalseQuestionMenuItem.Name = sAppendTrueFalseQuestionMenuItemName;
                        this.AppendTrueFalseQuestionMenuItem.Size = new System.Drawing.Size(316, 22);
                        this.AppendTrueFalseQuestionMenuItem.Text = "True / False Response Question";
                        this.AppendTrueFalseQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendTrueFalseQuestionMenuItem.Enabled = false;
                        //
                        // AppendWeightedMultipleChoiceQuestionMenuItem
                        //
                        this.AppendWeightedMuitipleChoiceQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendWeightedMuitipleChoiceQuestionMenuItem.Name = sAppendWeightedMultipleChoiceQuestionMenuItemName;
                        this.AppendWeightedMuitipleChoiceQuestionMenuItem.Size = new System.Drawing.Size(316, 22);
                        this.AppendWeightedMuitipleChoiceQuestionMenuItem.Text = "Weighted Multiple Choice Question";
                        this.AppendWeightedMuitipleChoiceQuestionMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendWeightedMuitipleChoiceQuestionMenuItem.Enabled = false;
                        // 
                        // AppendQuestionValidatedByValuesInAttachedFileMenuItem
                        //
                        this.AppendQuestionValidatedByValuesInAttachedFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.AppendQuestionValidatedByValuesInAttachedFileMenuItem.Name = sAppendQuestionValidatedByValuesInAttachedFileMenuItemName;
                        this.AppendQuestionValidatedByValuesInAttachedFileMenuItem.Size = new System.Drawing.Size(316, 22);
                        this.AppendQuestionValidatedByValuesInAttachedFileMenuItem.Text = "Question Validated by Values in Attached File";
                        this.AppendQuestionValidatedByValuesInAttachedFileMenuItem.Click += new System.EventHandler(this.AppendMenuItem_Click);
                        this.AppendQuestionValidatedByValuesInAttachedFileMenuItem.Enabled = false;
                        // 
                        // InsertMenu
                        //
                        this.InsertMenu = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertMenu.Name = sInsertMenuName;
                        this.InsertMenu.Size = new System.Drawing.Size(48, 20);
                        this.InsertMenu.Text = "&Insert";
                        this.InsertMenu.Enabled = false;
                        //
                        // InsertQuestionWithCurrentResponseMenuItem
                        //
                        this.InsertQuestionWithCurrentResponseMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertQuestionWithCurrentResponseMenuItem.Name = sInsertQuestionWithCurrentResponseMenuItemName;
                        this.InsertQuestionWithCurrentResponseMenuItem.ShortcutKeys = ((System.Windows.Forms.Keys)(((System.Windows.Forms.Keys.Control | 
                            System.Windows.Forms.Keys.Shift) | System.Windows.Forms.Keys.N)));
                        this.InsertQuestionWithCurrentResponseMenuItem.Size = new System.Drawing.Size(340, 22);
                        this.InsertQuestionWithCurrentResponseMenuItem.Text = "&Question with current response type";
                        this.InsertQuestionWithCurrentResponseMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertQuestionWithCurrentResponseMenuItem.Enabled = false;
                        //
                        // InsertStripSeparator
                        //
                        this.InsertStripSeparator = new System.Windows.Forms.ToolStripSeparator();
                        this.InsertStripSeparator.Name = sInsertStripSeparatorName;
                        this.InsertStripSeparator.Size = new System.Drawing.Size(337, 6);
                        //
                        // InsertDateQuestionMenuItem
                        //
                        this.InsertDateQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertDateQuestionMenuItem.Name = sInsertDateQuestionMenuItemName;
                        this.InsertDateQuestionMenuItem.Size = new System.Drawing.Size(340, 22);
                        this.InsertDateQuestionMenuItem.Text = "Date Response Quesstion";
                        this.InsertDateQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertDateQuestionMenuItem.Enabled = false;
                        //
                        // InsertInstructionMenuItem
                        //
                        this.InsertInstructionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertInstructionMenuItem.Name = sInsertInstructionMenuItemName;
                        this.InsertInstructionMenuItem.Size = new System.Drawing.Size(340, 22);
                        this.InsertInstructionMenuItem.Text = "Instruction";
                        this.InsertInstructionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertInstructionMenuItem.Enabled = false;
                        // 
                        // InsertLikertQuestionMenuItem
                        //
                        this.InsertLikertQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertLikertQuestionMenuItem.Name = sInsertLikertQuestionMenuItemName;
                        this.InsertLikertQuestionMenuItem.Size = new System.Drawing.Size(340, 22);
                        this.InsertLikertQuestionMenuItem.Text = "Likert Response Question";
                        this.InsertLikertQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertLikertQuestionMenuItem.Enabled = false;
                        //
                        // InsertMultipleChoiceQuestionMenuItem
                        //
                        this.InsertMultipleChoiceQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertMultipleChoiceQuestionMenuItem.Name = sInsertMultipleChoiceQuestionMenuItemName;
                        this.InsertMultipleChoiceQuestionMenuItem.Size = new System.Drawing.Size(340, 22);
                        this.InsertMultipleChoiceQuestionMenuItem.Text = "Multiple Choice Question";
                        this.InsertMultipleChoiceQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertLikertQuestionMenuItem.Enabled = false;
                        //
                        // InsertMultipleSelectionQuestionMenuItem
                        //
                        this.InsertMultipleSelectionQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertMultipleSelectionQuestionMenuItem.Name = sInsertMultipleSelectionQuestionMenuItemName;
                        this.InsertMultipleSelectionQuestionMenuItem.Size = new System.Drawing.Size(340, 22);
                        this.InsertMultipleSelectionQuestionMenuItem.Text = "Multiple Selection Question";
                        this.InsertMultipleSelectionQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertMultipleSelectionQuestionMenuItem.Enabled = false;
                        //
                        // InsertBoundedLengthQuestionMenuItem
                        //
                        this.InsertBoundedLengthQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertBoundedLengthQuestionMenuItem.Name = sInsertBoundedLengthQuestionMenuItemName;
                        this.InsertBoundedLengthQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.InsertBoundedLengthQuestionMenuItem.Text = "Bounded Length Response";
                        this.InsertBoundedLengthQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertBoundedLengthQuestionMenuItem.Enabled = false;
                        //
                        // InsertBoundedNumberQuestionMenuItem
                        //
                        this.InsertBoundedNumberQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertBoundedNumberQuestionMenuItem.Name = sInsertBoundedNumberQuestionMenuItemName;
                        this.InsertBoundedNumberQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.InsertBoundedNumberQuestionMenuItem.Text = "Bounded Number Response";
                        this.InsertBoundedNumberQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertBoundedNumberQuestionMenuItem.Enabled = false;
                        //
                        // InsertFixedDigQuestionMenuItem
                        //
                        this.InsertFixedDigQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertFixedDigQuestionMenuItem.Name = sInsertFixedDigQuestionMenuItemName;
                        this.InsertFixedDigQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.InsertFixedDigQuestionMenuItem.Text = "Fixed Number of Digits";
                        this.InsertFixedDigQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertFixedDigQuestionMenuItem.Enabled = false;
                        //
                        // InsertFixedLengthQuestionMenuItem
                        //
                        this.InsertFixedLengthQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertFixedLengthQuestionMenuItem.Name = sInsertFixedLengthQuestionMenuItemName;
                        this.InsertFixedLengthQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.InsertFixedLengthQuestionMenuItem.Text = "Fixed Length Text";
                        this.InsertFixedLengthQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertFixedLengthQuestionMenuItem.Enabled = false;
                        //
                        // InsertMaxLengthQuestionMenuItem
                        //
                        this.InsertMaxLengthQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertMaxLengthQuestionMenuItem.Name = sInsertMaxLengthQuestionMenuItemName;
                        this.InsertMaxLengthQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.InsertMaxLengthQuestionMenuItem.Text = "Text with Maximum Length";
                        this.InsertMaxLengthQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertMaxLengthQuestionMenuItem.Enabled = false;
                        //
                        // InsertRegExQuestionMenuItem
                        //
                        this.InsertRegExQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertRegExQuestionMenuItem.Name = sInsertRegExQuestionMenuItemName;
                        this.InsertRegExQuestionMenuItem.Size = new System.Drawing.Size(230, 22);
                        this.InsertRegExQuestionMenuItem.Text = "Validated by Regular Expression";
                        this.InsertRegExQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertRegExQuestionMenuItem.Enabled = false;
                        //
                        // InsertTextOrNumberQuestionMenuItem
                        //
                        this.InsertTextOrNumberQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertTextOrNumberQuestionMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                        this.InsertBoundedLengthQuestionMenuItem,
                        this.InsertBoundedNumberQuestionMenuItem,
                        this.InsertFixedDigQuestionMenuItem,
                        this.InsertFixedLengthQuestionMenuItem,
                        this.InsertMaxLengthQuestionMenuItem,
                        this.InsertRegExQuestionMenuItem});
                        this.InsertTextOrNumberQuestionMenuItem.Name = sInsertTextOrNumberQuestionMenuItemName;
                        this.InsertTextOrNumberQuestionMenuItem.Size = new System.Drawing.Size(340, 22);
                        this.InsertTextOrNumberQuestionMenuItem.Text = "Text or Number Response Question";
                        this.InsertTextOrNumberQuestionMenuItem.Enabled = false;
                        //
                        // InsertTrueFalseQuestionMenuItem
                        //
                        this.InsertTrueFalseQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertTrueFalseQuestionMenuItem.Name = sInsertTrueFalseQuestionMenuItemName;
                        this.InsertTrueFalseQuestionMenuItem.Size = new System.Drawing.Size(340, 22);
                        this.InsertTrueFalseQuestionMenuItem.Text = "True / False Question";
                        this.InsertTrueFalseQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertTrueFalseQuestionMenuItem.Enabled = false;
                        //
                        // InsertWeightedMultipleChoiceQuestionMenuItem
                        //
                        this.InsertWeightedMultipleChoiceQuestionMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertWeightedMultipleChoiceQuestionMenuItem.Name = sInsertWeightedMultipleChoiceQuestionMenuItemName;
                        this.InsertWeightedMultipleChoiceQuestionMenuItem.Size = new System.Drawing.Size(340, 22);
                        this.InsertWeightedMultipleChoiceQuestionMenuItem.Text = "Weighted Multiple Choice Question";
                        this.InsertWeightedMultipleChoiceQuestionMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertWeightedMultipleChoiceQuestionMenuItem.Enabled = false;
                        //
                        // InsertQuestionValidatedByValuesInAttachedFileMenuItem
                        //
                        this.InsertQuestionValidatedByValuesInAttachedFileMenuItem = new System.Windows.Forms.ToolStripMenuItem();
                        this.InsertQuestionValidatedByValuesInAttachedFileMenuItem.Name = sInsertQuestionValidatedByValuesInAttachedFileMenuItemName;
                        this.InsertQuestionValidatedByValuesInAttachedFileMenuItem.Size = new System.Drawing.Size(340, 22);
                        this.InsertQuestionValidatedByValuesInAttachedFileMenuItem.Text = "Question Validated by Values in Attached File";
                        this.InsertQuestionValidatedByValuesInAttachedFileMenuItem.Click += new System.EventHandler(this.InsertMenuItem_Click);
                        this.InsertQuestionValidatedByValuesInAttachedFileMenuItem.Enabled = false;
                        */


            //
            // ServerMenu
            //
            /*
            this.ServerMenu = new System.Windows.Forms.ToolStripMenuItem();
            this.ServerMenu.Name = sServerMenuName;
            this.ServerMenu.Size = new System.Drawing.Size(80, 20);
            this.ServerMenu.Text = "&Server";
            this.ServerMenu.Enabled = true;
            //
            // ServerRetrieveItemSlidesItem
            //
            // 
            // ServerStripSeparator
            //
            this.ServerSeparator = new System.Windows.Forms.ToolStripSeparator();
            this.ServerSeparator.Name = sServerStripSeparator;
            //
            // ServerDeleteIATMenuItem
            //
            this.ServerDeleteIATMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ServerDeleteIATMenuItem.Name = sServerDeleteIATMenuItemName;
            this.ServerDeleteIATMenuItem.Size = new System.Drawing.Size(122, 22);
            this.ServerDeleteIATMenuItem.Text = "Delete &IAT";
            this.ServerDeleteIATMenuItem.Enabled = true;
            this.ServerDeleteIATMenuItem.Click += new EventHandler(ServerDeleteIATMenuItem_Click);
            //
            // ServerDeleteIATDataMenuItem
            //
            this.ServerDeleteIATDataMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ServerDeleteIATDataMenuItem.Name = sServerDeleteIATDataMenuItemName;
            this.ServerDeleteIATDataMenuItem.Size = new System.Drawing.Size(122, 22);
            this.ServerDeleteIATDataMenuItem.Text = "Delete IAT &Data";
            this.ServerDeleteIATDataMenuItem.Enabled = true;
            this.ServerDeleteIATDataMenuItem.Click += new EventHandler(ServerDeleteIATDataMenuItem_Click);
            //
            // ServerServerInterfaceMenuItem
            //
            this.ServerServerInterfaceMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.ServerServerInterfaceMenuItem.Name = sServerServerInterfaceMenuItemName;
            this.ServerServerInterfaceMenuItem.Size = new System.Drawing.Size(122, 22);
            this.ServerServerInterfaceMenuItem.Text = "&Server Interface";
            this.ServerServerInterfaceMenuItem.Enabled = true;
            this.ServerServerInterfaceMenuItem.Click += new EventHandler(ServerServerInterfaceMenuItem_Click);
             */
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

            // 
            // EditMenu
            // 
            this.EditMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.EditCutMenuItem,
            this.EditCopyMenuItem,
            this.EditPasteMenuItem,
            this.EditDeleteMenuItem});
            this.EditCutMenuItem.Enabled = false;
            this.EditCopyMenuItem.Enabled = false;
            this.EditPasteMenuItem.Enabled = false;
            this.EditDeleteMenuItem.Enabled = false;
            this.EditMenu.Enabled = false;

            // 
            // ViewMenu
            //
            ConstructViewMenu(false);

            //
            // ServerMenu
            //
            /*
            this.ServerMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            //    this.ServerRetrieveItemSlidesItem,
                this.ServerSeparator,
                this.ServerDeleteIATMenuItem,
                this.ServerDeleteIATDataMenuItem,
               this.ServerServerInterfaceMenuItem 
            });
            this.ServerMenu.Enabled = true;
//            this.ServerRetrieveItemSlidesItem.Enabled = true;
            this.ServerSeparator.Enabled = true;
            this.ServerDeleteIATMenuItem.Enabled = true;
            this.ServerDeleteIATDataMenuItem.Enabled = true;
            */
            /*            
                        //
                        // AppendMenu
                        //
                        this.AppendMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                        this.AppendQuestionWithCurrentResponseMenuItem,
                        this.AppendStripSeparator,
                        this.AppendDateQuestionMenuItem,
                        this.AppendInstructionMenuItem,
                        this.AppendLikertQuestionMenuItem,
                        this.AppendMultipleChoiceQuestionMenuItem,
                        this.AppendMultipleSelectionQuestionMenuItem,
                        this.AppendTextOrNumberQuestionMenuItem,
                        this.AppendTrueFalseQuestionMenuItem,
                        this.AppendWeightedMuitipleChoiceQuestionMenuItem,
                        this.AppendQuestionValidatedByValuesInAttachedFileMenuItem});
                        this.AppendQuestionWithCurrentResponseMenuItem.Enabled = true;
                        this.AppendDateQuestionMenuItem.Enabled = true;
                        this.AppendInstructionMenuItem.Enabled = true;
                        this.AppendLikertQuestionMenuItem.Enabled = true;
                        this.AppendMultipleChoiceQuestionMenuItem.Enabled = true;
                        this.AppendMultipleSelectionQuestionMenuItem.Enabled = true;
                        this.AppendTextOrNumberQuestionMenuItem.Enabled = true;
                        this.AppendTrueFalseQuestionMenuItem.Enabled = true;
                        this.AppendWeightedMuitipleChoiceQuestionMenuItem.Enabled = true;
                        this.AppendBoundedLengthQuestionMenuItem.Enabled = true;
                        this.AppendBoundedNumberQuestionMenuItem.Enabled = true;
                        this.AppendFixedDigQuestionMenuItem.Enabled = true;
                        this.AppendFixedLengthQuestionMenuItem.Enabled = true;
                        this.AppendMaxLengthQuestionMenuItem.Enabled = true;
                        this.AppendRegExQuestionMenuItem.Enabled = true;
                        this.AppendQuestionValidatedByValuesInAttachedFileMenuItem.Enabled = true;
                        this.AppendMenu.Enabled = false;
                        // 
                        // InsertMenu
                        // 
                        this.InsertMenu.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
                        this.InsertQuestionWithCurrentResponseMenuItem,
                        this.InsertStripSeparator,
                        this.InsertDateQuestionMenuItem,
                        this.InsertInstructionMenuItem,
                        this.InsertLikertQuestionMenuItem,
                        this.InsertMultipleChoiceQuestionMenuItem,
                        this.InsertMultipleSelectionQuestionMenuItem,
                        this.InsertTextOrNumberQuestionMenuItem,
                        this.InsertTrueFalseQuestionMenuItem,
                        this.InsertWeightedMultipleChoiceQuestionMenuItem});
                        this.InsertQuestionWithCurrentResponseMenuItem.Enabled = true;
                        this.InsertDateQuestionMenuItem.Enabled = true;
                        this.InsertInstructionMenuItem.Enabled = true;
                        this.InsertLikertQuestionMenuItem.Enabled = true;
                        this.InsertMultipleChoiceQuestionMenuItem.Enabled = true;
                        this.InsertMultipleSelectionQuestionMenuItem.Enabled = true;
                        this.InsertTextOrNumberQuestionMenuItem.Enabled = true;
                        this.InsertTrueFalseQuestionMenuItem.Enabled = true;
                        this.InsertWeightedMultipleChoiceQuestionMenuItem.Enabled = true;
                        this.InsertBoundedLengthQuestionMenuItem.Enabled = true;
                        this.InsertRegExQuestionMenuItem.Enabled = true;
                        this.InsertBoundedNumberQuestionMenuItem.Enabled = true;
                        this.InsertFixedDigQuestionMenuItem.Enabled = true;
                        this.InsertFixedLengthQuestionMenuItem.Enabled = true;
                        this.InsertMaxLengthQuestionMenuItem.Enabled = true;
                        this.InsertMenu.Enabled = false;

             */
            // 
            // HelpMenu
            // 

            this.HelpMenu.DropDownItems.Add(this.HelpAboutMenuItem);
            this.HelpIndexMenuItem.Enabled = true;
            this.HelpContentsMenuItem.Enabled = true;
            this.HelpAboutMenuItem.Enabled = true;
            this.HelpMenu.Enabled = true;
            // 
            // HeaderMenu
            // 
            this.HeaderMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FileMenu,
            this.EditMenu,
            this.ViewMenu,
//            this.ServerMenu,
//            this.AppendMenu,
//            this.InsertMenu,
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
            if (IAT != null)
            {
                for (int ctr = 0; ctr < IAT.Contents.Count; ctr++)
                {
                    ViewIATItemList.Add(new ToolStripMenuItem());
                    ViewIATItemList[ctr].Name = String.Format("ViewIATContentsItem{0}", ctr + 1);
                    ViewIATItemList[ctr].Text = IAT.Contents[ctr].Name;
                    ViewIATItemList[ctr].Click += new EventHandler(ViewIATItemList_Click);
                    ViewIATItemList[ctr].Enabled = true;
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
            if (IAT != null)
            {
                for (int ctr = 0; ctr < IAT.Contents.Count; ctr++)
                    ViewIATItemList[ctr].Size = new Size(ViewMenuWidth, 22);
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
        private void EditCutMenuItem_Click(object sender, EventArgs e)
        {
            switch (FormContents)
            {
                case EFormContents.Instructions:
                    m_InstructionPanel.DoCut();
                    break;

                case EFormContents.IATBlock:
                    m_IATBlockPanel.DoCut();
                    break;
            }
        }

        private void EditCopyMenuItem_Click(object sender, EventArgs e)
        {
            switch (FormContents)
            {
                case EFormContents.Instructions:
                    m_InstructionPanel.DoCopy();
                    break;

                case EFormContents.IATBlock:
                    m_IATBlockPanel.DoCopy();
                    break;
            }
        }

        private void EditPasteMenuItem_Click(object sender, EventArgs e)
        {
            switch (FormContents)
            {
                case EFormContents.Instructions:
                    m_InstructionPanel.DoPaste();
                    break;

                case EFormContents.IATBlock:
                    m_IATBlockPanel.DoPaste();
                    break;
            }
        }

        private void EditDeleteMenuItem_Click(object sender, EventArgs e)
        {
            switch (FormContents)
            {
                case EFormContents.Instructions:
                    m_InstructionPanel.DoDelete();
                    break;

                case EFormContents.IATBlock:
                    m_IATBlockPanel.DoDelete();
                    break;
            }
        }

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
            if (CurrentFilename != String.Empty)
                CIAT.ImageManager.CreatePreSaveBackup(CurrentFilename);
            CurrentFilename = dialog.FileName;
            Save(CurrentFilename);
        }


        private void FileCloseMenuItem_Click(object sender, EventArgs e)
        {
            // the File->Close menu item has been clicked so close the file
            FormContents = EFormContents.Main;
            CloseFile();
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
            IContentsItem cItem = IAT.Contents[ndx];
            ActiveItem = cItem;
            switch (cItem.Type)
            {
                case ContentsItemType.AfterSurvey:
                    FormContents = IATConfigMainForm.EFormContents.Survey;
                    break;

                case ContentsItemType.BeforeSurvey:
                    FormContents = IATConfigMainForm.EFormContents.Survey;
                    break;

                case ContentsItemType.IATBlock:
                    FormContents = IATConfigMainForm.EFormContents.IATBlock;
                    break;

                case ContentsItemType.IATPracticeBlock:
                    FormContents = IATConfigMainForm.EFormContents.IATBlock;
                    break;

                case ContentsItemType.InstructionBlock:
                    FormContents = IATConfigMainForm.EFormContents.Instructions;
                    break;
            }
        }


        #endregion

        public void LoadIAT(MemoryStream memStream)
        {
            IAT.Load(memStream, CurrentIATFilename);
            m_MainPanel.Dispose();
            ConstructViewMenu(true);
            m_MainPanel = new MainPanel(this);
            m_MainPanel.Location = new Point(0, HeaderMenu.Height);
            m_MainPanel.PopulateContents(IAT.Contents);
            if (FormContents == EFormContents.Main)
                FormContents = EFormContents.Main;
        }

        /// <summary>
        /// Displays an OpenFileDialog and loads the file the user specifies, updating the SurveyView
        /// </summary>
        public void Open()
        {
            // clear the error message
            ErrorMsg = String.Empty;

            // instantiate and initialize an OpenFileDialog
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.AddExtension = true;
            dialog.DefaultExt = Properties.Resources.sFileExt;
            dialog.Title = Properties.Resources.sOpenFileDialogTitle;
            dialog.Filter = String.Format(Properties.Resources.sFileDialogFilter);
            dialog.FilterIndex = 0;
            bool bBackupMade = false;
            // if the user doesn't click "Open", return
            if (dialog.ShowDialog(this) != DialogResult.OK)
                return;


            // try to load the file
            try
            {
                if (Modified)
                {
                    CIAT.ImageManager.Halt();
                    if (FormContents == EFormContents.Survey)
                    {
                        CSurvey survey = (CSurvey)ActiveItem;
                        survey.Items.Clear();
                        survey.Items.AddRange(m_SurveyPanel.SurveyItems);
                        foreach (CSurveyItem si in survey.Items)
                            si.ParentSurvey = survey;
                        survey.Timeout = m_SurveyPanel.SurveyTimeout;
                    }
                    FormContents = EFormContents.Main;
                    File.Delete(Properties.Resources.sTempIATSaveFile);
                    FileStream fStream = new FileStream(Properties.Resources.sTempIATSaveFile, FileMode.Create);
                    FileInfo fi = new FileInfo(Properties.Resources.sTempIATSaveFile);
                    fi.Attributes |= FileAttributes.Hidden;
                    BinaryWriter bWriter = new BinaryWriter(fStream);
                    CIAT.ImageManager.CreatePreSaveBackup(CurrentFilename);
                    IAT.Save(bWriter, Properties.Resources.sTempIATSaveFile);

                    // set the modified flag to false
                    Modified = false;

                    bWriter.Close();
                    bBackupMade = true;
                    IAT.Clear();
                    CIAT.ImageManager.Start();
                    m_MainPanel.Dispose();
                    m_MainPanel = null;
                    FormContents = EFormContents.Main;
                    // set the name of the currently open file
                }
                // try to load the file
                CurrentFilename = dialog.FileName;
                IAT.Load(dialog.FileName, Progress);
                IATNameBox.Text = IAT.Name;
                Progress.Value = 0;
                Modified = true;
            }
            catch (Exception ex)
            {
                CIAT.ImageManager.Clear();
                MessageBox.Show(ex.Message);
                // display an error message box
                MessageBox.Show(this, String.Format(String.Format(Properties.Resources.sFileOpenError, dialog.SafeFileName), dialog.SafeFileName),
                    Properties.Resources.sFileOpenErrorCaption, MessageBoxButtons.OK);
                CurrentFilename = String.Empty;
                if (bBackupMade)
                {
                    IAT.Load(Properties.Resources.sTempIATSaveFile, Progress);
                    File.Delete(Properties.Resources.sTempIATSaveFile);
                }
            }
            ConstructViewMenu(true);
            m_MainPanel.PopulateContents(IAT.Contents);
            FormContents = EFormContents.Main;
        }

        /// <summary>
        /// Saves the IAT config file to the specified filename
        /// </summary>
        /// <param name="filename">The name of the output file</param>
        private void Save(String filename)
        {
            CIAT.ImageManager.Halt();
            if (FormContents == EFormContents.Survey)
            {
                CSurvey survey = (CSurvey)ActiveItem;
                survey.Items.Clear();
                survey.Items.AddRange(m_SurveyPanel.SurveyItems);
                foreach (CSurveyItem si in survey.Items)
                    si.ParentSurvey = survey;
                survey.Timeout = m_SurveyPanel.SurveyTimeout;
            }
            BinaryWriter bWriter = new BinaryWriter(new FileStream(filename, FileMode.Create));
            IAT.Name = IATName;
            IAT.Save(bWriter, filename);

            // set the modified flag to false
            Modified = false;

            bWriter.Close();
            CIAT.ImageManager.Start();
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
            else
                CIAT.ImageManager.CreatePreSaveBackup(CurrentFilename);

            // save the test
            Save(CurrentFilename);
        }

        public String CurrentIATFilename
        {
            get
            {
                return CurrentFilename;
            }
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
        public void CloseFile()
        {
            if (Modified) {
                DialogResult saveResult = PromptForSave();
                if (saveResult == DialogResult.Yes)
                    Save();
                else if (saveResult == DialogResult.Cancel)
                    return;
            }
            FormContents = EFormContents.Main;
            IAT.Clear();
            ConstructViewMenu(true);
            Modified = false;
            CurrentFilename = String.Empty;
            m_MainPanel.Dispose();
            m_MainPanel = null;
            FormContents = EFormContents.Main;
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
                    CancelMessageBarProcess.Invoke(this, new EventArgs());
                    BackgroundWorker yielder = new BackgroundWorker();
                    yielder.DoWork += (o, args) => { Thread.Sleep(100); };
                    while (ProgressBarUse != EProgressBarUses.None)
                    {
                        yielder.RunWorkerAsync();
                        Thread.Yield();
                    }
                }
                _IsInShutdown = true;
                foreach (CCompositeImage ci in OpenCompositeImages)
                    ci.CloseForEditing();
                while (OpenCompositeImages.Count > 0)
                    Thread.Sleep(10);
                CIAT.ImageManager.Dispose();
            }
        }

        private void FinishFormClose(object sender, EventArgs e)
        {
            _IsInShutdown = true;
            foreach (CCompositeImage ci in OpenCompositeImages)
                ci.CloseForEditing();
            while (OpenCompositeImages.Count > 0)
                Thread.Sleep(10);
            CIAT.ImageManager.Dispose();
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

        public enum EProgressBarUses { None, Upload, DataRetrieval, ItemSlideRetrieval, Reencryption, Packaging, ExportData, GeneratingFontFile };
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
            if ((use != EProgressBarUses.Packaging) && (use != EProgressBarUses.GeneratingFontFile))
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
            Progress.Minimum = nMin;
            Progress.Maximum = nMax;
        }

        public void ProgressIncrement(int nInc)
        {
            try
            {
                Progress.Value += nInc;
            }
            catch (Exception ex) { }
        }

        public void SetStatusMessage(String statusMessage)
        {
            MessageBar.Items["StatusMessage"].Text = statusMessage;
        }

        public String GetStatusMessage()
        {
            return MessageBar.Items["StatusMessage"].Text;
        }

        public void OperationComplete(CIATSummary summary)
        {
            IATUploadCompleteForm uploadComplete = new IATUploadCompleteForm();
            uploadComplete.Summary = summary;
            uploadComplete.ShowDialog();
        }

        public void ResetProgress()
        {
            Progress.Value = 0;
        }

        public void SetProgressValue(int val)
        {
            Progress.Value = val;
        }

        public DialogResult OnDisplayYesNoMessageBox(String msg, String caption)
        {
            return MessageBox.Show(this, msg, caption, MessageBoxButtons.YesNo);
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
        public void ShowForm(Form f)
        {
            f.ShowDialog();
        }

        private void UploadButton_Click(object sender, EventArgs e)
        {
            CItemValidator.StartValidation();
            foreach (CIATBlock b in IAT.Blocks)
                CItemValidator.ValidateItem(b);
            foreach (CInstructionBlock ib in IAT.InstructionBlocks)
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
                if (!Char.IsLetterOrDigit(IATName[ctr]) && (IATName[ctr] != '_'))
                {
                    MessageBox.Show("Your IAT Name may contain only letter, numerical digits, and the underscore character.", "Invalid IAT Name");
                    return;
                }
            if (IATPasswordBox.Text.Length < 4)
            {
                MessageBox.Show("Please enter a password of at least four characters.", "Invalid Password");
                return;
            }
           
             
            UploadForm upForm = new UploadForm(IAT, false);
            if (upForm.ShowDialog(this) == DialogResult.Cancel)
                return;
            if (MessageBox.Show("Would you like to save this password to your computer's registry? Keep in mind that your password is not saved on the IATSoftware.net server " +
                "in order to ensure the privacy of your data. If you lose or forget your password, your data will be irretrievable. Storing your password to your registry will " +
                "allow you to retrieve your data form this computer even if you lose your password.", "Save password to registry?", MessageBoxButtons.YesNo) == DialogResult.Yes)
                StorePasswordToRegistry = true;
            else
                StorePasswordToRegistry = false;
            IATUploader = new CWebSocketUploader(IAT, this);
            Func<String, String, CWebSocketUploader.ETransactionResult> del = new Func<String, String, CWebSocketUploader.ETransactionResult>(IATUploader.Upload);
            del.BeginInvoke(IAT.Name, IATPasswordBox.Text, new AsyncCallback(UploadComplete), del);
        }

        private void UploadComplete(IAsyncResult aResult)
        {
            Func<String, String, CWebSocketUploader.ETransactionResult> del = (Func<String, String, CWebSocketUploader.ETransactionResult>)aResult.AsyncState;
            CWebSocketUploader.ETransactionResult result = del.EndInvoke(aResult);
            if (StorePasswordToRegistry && (result == CWebSocketUploader.ETransactionResult.success))
                CRegistry.AddIATPassword(IATName, IATPasswordBox.Text);
            if (result == CWebSocketUploader.ETransactionResult.exception)
                MessageBox.Show(IATUploader.TransactionException.Message, "Upload Failed");
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
        /*
        public void StartRetrieveItemSlides()
        {
            if (DataRetrievalPassword == String.Empty)
                MessageBox.Show(this, "Please Provide Password", "Please provide your data retrieval password to download the itemslides.");
            if (ItemSlideContainer != null)
                ItemSlideContainer.Dispose();
            _ItemSlides = new CItemSlideContainer(IATName, DataRetrievalPassword);
            Func<Action<String, String, bool>, bool> startRetrieveItemSlides = ItemSlideContainer.StartRetrieval;
            startRetrieveItemSlides.BeginInvoke((Action<String, String, bool>)(SlideRetrieveFail), new AsyncCallback(SlideRetrievalComplete), startRetrieveItemSlides);
            bSlideRetrievalInProcess = true;
        }
        
        public void PauseItemSlideRetrieval()
        {
            if (SlideRetrievalInProcess)
                ItemSlideContainer.HaltRetrieval();
        }

        public void ResumeItemSlideRetrieval()
        {
            if (!SlideRetrievalInProcess)
                StartRetrieveItemSlides();
            else
            {
                Func<Action<String, String, bool>, bool> startRetrieveItemSlides = ItemSlideContainer.StartRetrieval;
                startRetrieveItemSlides.BeginInvoke((Action<String, String, bool>)(SlideRetrieveFail), new AsyncCallback(SlideRetrievalComplete), startRetrieveItemSlides);
                bSlideRetrievalInProcess = true;
            } 
        }
        */
        public void StopItemSlideRetrieval()
        {
            ItemSlideContainer.Dispose();
            _ItemSlides = null;
            bSlideRetrievalInProcess = false;
        }

        private void DataRetrievalComplete(bool bSuccess, CResultData results)
        {
            if (!bSuccess)
                return;
            DialogResult result = MessageBox.Show(Properties.Resources.sResultDataFileHeaderYesNo, Properties.Resources.sResultDataFileHeaderYesNoCaption, MessageBoxButtons.YesNoCancel);
            if (result == DialogResult.Cancel)
                return;
            SaveFileDialog fileSave = new SaveFileDialog();
            fileSave.AddExtension = true;
            fileSave.DefaultExt = "txt";
            fileSave.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (fileSave.ShowDialog() != DialogResult.Cancel)
                results.ResultsInterface.ExportSummaryFile(fileSave.FileName);
            if (MessageBox.Show(Properties.Resources.sResultsGroupedByItemYesNo, Properties.Resources.sResultsGroupedByItemYesNoCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                fileSave = new SaveFileDialog();
                fileSave.AddExtension = true;
                fileSave.DefaultExt = "txt";
                fileSave.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                if (fileSave.ShowDialog() != DialogResult.Cancel)
                    results.ResultsInterface.ExportIATLatenciesByItem(fileSave.FileName);
            }
            if (MessageBox.Show(Properties.Resources.sResultsGroupedByTesteeYesNo, Properties.Resources.sResultsGroupedByTesteeYesNoCaption, MessageBoxButtons.YesNo) == DialogResult.Yes)
            {
                fileSave = new SaveFileDialog();
                fileSave.AddExtension = true;
                fileSave.DefaultExt = "txt";
                fileSave.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
                if (fileSave.ShowDialog() != DialogResult.Cancel)
                    results.ResultsInterface.ExportIATLatenciesByTestee(fileSave.FileName);
            }
        }
    }
}
