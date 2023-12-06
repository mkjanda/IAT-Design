using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    public partial class InstructionScreenPanel : UserControl
    {
        private int CurrentScreenNdx = -1;
        private ImageDisplay ScreenPreview;
        private Size ChildPanelSize { get; set; } = Size.Empty;
        private Point ChildPanelLoc { get; set; }
        private ScrollingPreviewPanel ScrollingPreview;
        private static Point ScrollingPreviewPos = new Point(0, 525);
        private CInstructionScreen ClipboardItem;
        private bool UpdatingInternally { get; set; }
        // the current block of instructions
        public int InstructionBlockNdx = -1;
        public Uri InstructionBlockUri { get; private set; }

        private Point ButtonPos { get { return new Point(this.Width - 400, ScrollingPreviewPos.Y + 50 - 4 * (ButtonDistance.Height + 4)); } }
        private Size ButtonSize { get { return new Size(120, 24); } }
        private Size ButtonDistance { get { return new Size(150, 5 * 24 / 4 + 4); } }


        private void InitializeComponent()
        {
            this.InstructionTypeLabel = new System.Windows.Forms.Label();
            this.TextRadio = new System.Windows.Forms.RadioButton();
            this.MockItemRadio = new System.Windows.Forms.RadioButton();
            this.KeyRadio = new System.Windows.Forms.RadioButton();
            this.InsertScreen = new System.Windows.Forms.Button();
            this.AddScreen = new System.Windows.Forms.Button();
            this.DeleteScreen = new System.Windows.Forms.Button();
            this.Done = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ContinueKeyLabel
            // 
            //            this.ContinueKeyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            //                      | System.Windows.Forms.AnchorStyles.Left)
            //                    | System.Windows.Forms.AnchorStyles.Right)));
            //      this.ContinueKeyLabel.AutoSize = true;
            //    this.ContinueKeyLabel.Location = new System.Drawing.Point(786, 530);
            //  this.ContinueKeyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            //            this.ContinueKeyLabel.Name = "ContinueKeyLabel";
            //          this.ContinueKeyLabel.Size = new System.Drawing.Size(304, 17);
            //        this.ContinueKeyLabel.TabIndex = 1;
            //      this.ContinueKeyLabel.Text = "Select the key the user must press to continue:";
            //    this.ContinueKeyLabel.Click += new System.EventHandler(this.ContinueKeyLabel_Click);
            // 
            // ContinueKeyDrop
            // 
            //            this.ContinueKeyDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            //           this.ContinueKeyDrop.FormattingEnabled = true;
            //          this.ContinueKeyDrop.Location = new System.Drawing.Point(1098, 530);
            //         this.ContinueKeyDrop.Margin = new System.Windows.Forms.Padding(4);
            //        this.ContinueKeyDrop.Name = "ContinueKeyDrop";
            //       this.ContinueKeyDrop.Size = new System.Drawing.Size(87, 24);
            //      this.ContinueKeyDrop.TabIndex = 2;
            //     this.ContinueKeyDrop.SelectedIndexChanged += new System.EventHandler(this.ContinueKeyDrop_SelectedIndexChanged);
            // 
            // InstructionTypeLabel
            // 
            //            this.InstructionTypeLabel.AutoSize = true;
            //          this.InstructionTypeLabel.Location = new System.Drawing.Point(755, 15);
            //        this.InstructionTypeLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            //      this.InstructionTypeLabel.Name = "InstructionTypeLabel";
            //    this.InstructionTypeLabel.Size = new System.Drawing.Size(155, 17);
            //  this.InstructionTypeLabel.TabIndex = 5;
            //this.InstructionTypeLabel.Text = "Instruction screen type:";
            // 
            // TextRadio
            // 
            this.TextRadio.AutoSize = true;
            this.TextRadio.Location = new System.Drawing.Point(570, 12);
            this.TextRadio.Margin = new System.Windows.Forms.Padding(4);
            this.TextRadio.Name = "TextRadio";
            this.TextRadio.Size = new System.Drawing.Size(56, 21);
            this.TextRadio.TabIndex = 6;
            this.TextRadio.TabStop = true;
            this.TextRadio.Text = "Text";
            this.TextRadio.UseVisualStyleBackColor = true;
            this.TextRadio.CheckedChanged += new System.EventHandler(this.TextRadio_CheckedChanged);
            // 
            // MockItemRadio
            // 
            this.MockItemRadio.AutoSize = true;
            this.MockItemRadio.Location = new System.Drawing.Point(680, 12);
            this.MockItemRadio.Margin = new System.Windows.Forms.Padding(4);
            this.MockItemRadio.Name = "MockItemRadio";
            this.MockItemRadio.Size = new System.Drawing.Size(92, 21);
            this.MockItemRadio.TabIndex = 8;
            this.MockItemRadio.TabStop = true;
            this.MockItemRadio.Text = "Mock Item";
            this.MockItemRadio.UseVisualStyleBackColor = true;
            this.MockItemRadio.CheckedChanged += new System.EventHandler(this.MockItemRadio_CheckedChanged);
            // 
            // KeyRadio
            // 
            this.KeyRadio.AutoSize = true;
            this.KeyRadio.Location = new System.Drawing.Point(610, 12);
            this.KeyRadio.Margin = new System.Windows.Forms.Padding(4);
            this.KeyRadio.Name = "KeyRadio";
            this.KeyRadio.Size = new System.Drawing.Size(165, 21);
            this.KeyRadio.TabIndex = 14;
            this.KeyRadio.TabStop = true;
            this.KeyRadio.Text = "Response Key";
            this.KeyRadio.UseVisualStyleBackColor = true;
            this.KeyRadio.CheckedChanged += new System.EventHandler(this.KeyRadio_CheckedChanged);
            // 
            // InsertScreen
            // 
            InsertScreen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            InsertScreen.Location = ButtonPos;
            InsertScreen.Size = ButtonSize;
            this.InsertScreen.Margin = new System.Windows.Forms.Padding(4);
            this.InsertScreen.Name = "InsertScreen";
            this.InsertScreen.TabIndex = 15;
            this.InsertScreen.Text = "Insert Screen";
            this.InsertScreen.UseVisualStyleBackColor = true;
            this.InsertScreen.Click += new System.EventHandler(this.InsertScreen_Click);
            // 
            // AddScreen
            // 
            AddScreen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            AddScreen.Location = new Point(ButtonPos.X + ButtonDistance.Width, ButtonPos.Y);
            AddScreen.Size = ButtonSize;
            this.AddScreen.Margin = new System.Windows.Forms.Padding(4);
            this.AddScreen.Name = "AddScreen";
            this.AddScreen.TabIndex = 16;
            this.AddScreen.Text = "Add Screen";
            this.AddScreen.UseVisualStyleBackColor = true;
            this.AddScreen.Click += new System.EventHandler(this.AddScreen_Click);
            // 
            // DeleteScreen
            // 
            DeleteScreen.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            DeleteScreen.Location = new Point(ButtonPos.X, ButtonPos.Y + ButtonDistance.Height);
            DeleteScreen.Size = ButtonSize;
            this.DeleteScreen.Margin = new System.Windows.Forms.Padding(4);
            this.DeleteScreen.Name = "DeleteScreen";
            this.DeleteScreen.TabIndex = 17;
            this.DeleteScreen.Text = "Delete Screen";
            this.DeleteScreen.UseVisualStyleBackColor = true;
            this.DeleteScreen.Click += new System.EventHandler(this.DeleteScreen_Click);
            // 
            // Done
            // 
            Done.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            Done.Location = ButtonPos + ButtonDistance;
            Done.Size = ButtonSize;
            this.Done.Margin = new System.Windows.Forms.Padding(4);
            this.Done.Name = "Done";
            this.Done.TabIndex = 18;
            this.Done.Text = "Done";
            this.Done.UseVisualStyleBackColor = true;
            this.Done.Click += new System.EventHandler(this.Done_Click);
            // 
            // InstructionScreenPanel
            // 
            this.Controls.Add(this.Done);
            this.Controls.Add(this.DeleteScreen);
            this.Controls.Add(this.AddScreen);
            this.Controls.Add(this.InsertScreen);
            this.Controls.Add(this.KeyRadio);
            this.Controls.Add(this.MockItemRadio);
            this.Controls.Add(this.TextRadio);
            this.Controls.Add(this.InstructionTypeLabel);
            //            this.Controls.Add(this.ContinueKeyDrop);
            //          this.Controls.Add(this.ContinueKeyLabel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.Name = "InstructionScreenPanel";
            this.Load += new System.EventHandler(this.InstructionScreenPanel_Load);
            this.ResumeLayout(false);


        }


        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }
        private System.ComponentModel.IContainer components = null;

        //      private System.Windows.Forms.Label ContinueKeyLabel;
        //        private System.Windows.Forms.ComboBox ContinueKeyDrop;
        private System.Windows.Forms.Label InstructionTypeLabel;
        private System.Windows.Forms.RadioButton TextRadio;
        private System.Windows.Forms.RadioButton MockItemRadio;
        private System.Windows.Forms.RadioButton KeyRadio;
        private System.Windows.Forms.Button InsertScreen;
        private System.Windows.Forms.Button AddScreen;
        private System.Windows.Forms.Button DeleteScreen;
        private System.Windows.Forms.Button Done;


        public CInstructionBlock InstructionBlock
        {
            get
            {
                return CIAT.SaveFile.GetInstructionBlock(InstructionBlockUri);
            }
        }

        public CInstructionScreen InstructionScreen
        {
            get
            {
                return InstructionBlock[CurrentInstructionScreenNdx];
            }
        }

        private bool screenIndexChanging = false;
        public int CurrentInstructionScreenNdx
        {
            get
            {
                return CurrentScreenNdx;
            }
            set
            {
                if (screenIndexChanging)
                    return;
                screenIndexChanging = true;
                try
                {
                    if ((CurrentScreenNdx != -1) && (InstructionBlock.NumScreens > 0) && (CurrentScreenNdx < InstructionBlock.NumScreens))
                        InstructionBlock[CurrentScreenNdx].PreviewPane = null;
                    CurrentScreenNdx = value;
                    TextRadio.Checked = false;
                    KeyRadio.Checked = false;
                    MockItemRadio.Checked = false;
                    TextInstructions.Visible = false;
                    KeyedInstructions.Visible = false;
                    MockItem.Visible = false;
                    if (CurrentScreenNdx == -1)
                    {
                        ScreenPreview.ClearImage();
                        return;
                    }
                    InstructionBlock[CurrentScreenNdx].PreviewPane = ScreenPreview;
                    CInstructionScreen scrn = InstructionBlock[CurrentScreenNdx];
                    //                    ContinueInstructions.TextDisplayItemUri = scrn.ContinueInstructionsUri;
                    //                  ContinueKeyDrop.SelectedIndex = ContinueKeyDrop.Items.IndexOf(scrn.ContinueKey);
                    if (scrn.Type == InstructionScreenType.Text)
                    {
                        TextInstructions.TextInstructionScreen = scrn as CTextInstructionScreen;
                        TextInstructions.Visible = true;
                        TextRadio.Checked = true;
                    }
                    else if (scrn.Type == InstructionScreenType.ResponseKey)
                    {
                        KeyedInstructions.KeyedInstructionScreen = scrn as CKeyInstructionScreen;
                        KeyedInstructions.Visible = true;
                        KeyRadio.Checked = true;
                    }
                    else if (scrn.Type == InstructionScreenType.MockItem)
                    {
                        MockItem.MockItemScreen = scrn as CMockItemScreen;
                        MockItem.Visible = true;
                        MockItemRadio.Checked = true;
                    }
                    ValidateInput();
                }
                finally
                {
                    this.Enabled = true;
                    screenIndexChanging = false;
                }
            }
        }


        public void UpdateScreenDefPanel(CInstructionScreen scrn)
        {
            //        ContinueInstructions.TextDisplayItemUri = scrn.ContinueInstructionsUri;
            if (scrn.Type == InstructionScreenType.Text)
            {
                TextInstructions.TextInstructionScreen = scrn as CTextInstructionScreen;
                TextInstructions.Visible = true;
                KeyedInstructions.Visible = false;
                MockItem.Visible = false;
                TextRadio.Checked = true;
            }
            else if (scrn.Type == InstructionScreenType.ResponseKey)
            {
                TextInstructions.Visible = false;
                KeyedInstructions.Visible = true;
                MockItem.Visible = false;
                KeyedInstructions.KeyedInstructionScreen = scrn as CKeyInstructionScreen;
                KeyRadio.Checked = true;
            }
            else if (scrn.Type == InstructionScreenType.MockItem)
            {
                TextInstructions.Visible = false;
                KeyedInstructions.Visible = false;
                MockItem.Visible = true;
                MockItem.MockItemScreen = scrn as CMockItemScreen;
                MockItemRadio.Checked = true;
            }
            else
            {
                TextInstructions.Visible = false;
                KeyedInstructions.Visible = false;
                MockItem.Visible = false;
            }
            ScrollingPreview.SelectedPreview = InstructionBlock.GetIndexOf(scrn.URI);
        }

        public KeyedDirection GetVisibleKeyedDir()
        {
            if (InstructionBlock[ScrollingPreview.SelectedPreview].Type == InstructionScreenType.MockItem)
            {
                if (((CMockItemScreen)InstructionBlock[ScrollingPreview.SelectedPreview]).InvalidResponseFlag)
                    return ((CMockItemScreen)InstructionBlock[ScrollingPreview.SelectedPreview]).KeyedDirection;
                return KeyedDirection.None;
            }
            else
                return KeyedDirection.None;
        }

        // the text instructions panel
        private TextInstructionsPanel TextInstructions;
        private static Point TextInstructionsLocation = new Point(574, 26);
        private static Size TextInstructionsSize = new Size(371, 306);

        // the continue instructions control variables
        //        private TextEditControl ContinueInstructions;
        //      private static int ContinueInstructionsWidth = 370;

        // the mock item control
        private MockItemPanel MockItem;
        private static Point MockItemLocation = new Point(574, 26);
        private static Size MockItemSize = new Size(371, 375);

        // the text instructions & response key control
        private KeyInstructionsPanel KeyedInstructions;
        private static Point KeyedInstructionsLocation = new Point(574, 26);
        private static Size KeyedInstructionsSize = new Size(371, 340);

        // define available continue keys
        private static String[] AvailableContinueKeys = {"Space", "Enter", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O",
                                                           "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };

        public IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Parent;
            }
        }

        public bool ValidateInput()
        {
            if (MainForm == null)
                return true;/*
            try
            {
                if (!TextRadio.Checked && !MockItemRadio.Checked && !KeyRadio.Checked)
                    throw new CValidationException(EValidationException.InstructionScreenWithoutType,
                        new CInstructionLocationDescriptor(InstructionBlock, InstructionScreen));
                if (ContinueInstructions.TextValue == String.Empty)
                    throw new CValidationException(EValidationException.ContinueInstructionsBlank,
                        new CInstructionLocationDescriptor(InstructionBlock, InstructionScreen));
                if (TextRadio.Checked)
                    TextInstructions?.ValidateInput();
                if (MockItemRadio.Checked)
                    MockItem?.ValidateInput();
                if (KeyRadio.Checked)
                    KeyedInstructions?.ValidateInput();
            }
            catch (CValidationException ex)
            {
                MainForm.ErrorMsg = ex.ErrorText;
                return false;
            }
            MainForm.ErrorMsg = String.Empty;*/
            return true;
        }

        public InstructionScreenPanel(CInstructionBlock instrBlock)
        {
            SuspendLayout();
            InstructionBlockUri = instrBlock.URI;
            InitializeComponent();

            ScreenPreview = new ImageDisplay();
            ScreenPreview.BackColor = CIAT.SaveFile.Layout.BackColor;
            ScreenPreview.SetImage(null);
            //            ScreenPreview.Dock = DockStyle.Fill;
            //            ScreenPreview.Size = Images.ImageMediaType.FullPreview.ImageSize;
            ScreenPreview.Location = new Point(3, 12);
            double arPreview = (double)Images.ImageMediaType.FullPreview.ImageSize.Width / (double)Images.ImageMediaType.FullPreview.ImageSize.Height;
            Size szPreview;
            if (arPreview < 1)
                szPreview = new Size(Images.ImageMediaType.FullPreview.ImageSize.Width * Images.ImageMediaType.FullPreview.ImageSize.Height / 500, 500);
            else
                szPreview = new Size(500, Images.ImageMediaType.FullPreview.ImageSize.Height * Images.ImageMediaType.FullPreview.ImageSize.Width / 500);
            ScreenPreview.Size = szPreview;
            ScreenPreview.Location += new Size((500 - szPreview.Width >> 1), 500 - szPreview.Height >> 1);
            Controls.Add(ScreenPreview);



            ScrollingPreview = new ScrollingPreviewPanel();
            ScrollingPreview.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;
            ScrollingPreview.Orientation = ScrollingPreviewPanel.EOrientation.horizontal;
            ScrollingPreview.Location = new Point(ScrollingPreviewPos.X, ScrollingPreviewPos.Y);
            ScrollingPreview.Width = this.Width;
            Controls.Add(ScrollingPreview);
            ScrollingPreview.PreviewClickCallback = new Action<int>((newNdx) => { CurrentInstructionScreenNdx = newNdx; });
            ScrollingPreview.OnMoveContainerItem = new Action<int, int>((startNdx, endNdx) =>
            {
                instrBlock.MoveScreen(startNdx, endNdx);
                CurrentInstructionScreenNdx = endNdx;
                ScrollingPreview.SelectedPreview = endNdx;
            });
            ScrollingPreview.ResetScroll();
            for (int ctr = 0; ctr < InstructionBlock.NumScreens; ctr++)
            {
                ScrollingPreview.InsertPreview(ctr, InstructionBlock[ctr]);
            }
            if (InstructionBlock.NumScreens == 0)
            {
                var openingScreen = new CInstructionScreen(InstructionBlock);
                InstructionBlock.AddScreen(openingScreen);
                ScrollingPreview.InsertPreview(0, openingScreen);
            }
            this.HandleCreated += (sender, args) => this.BeginInvoke(new Action(() =>
            {
                ScrollingPreview.ResumeRecalcLayout();
            }));

            // initialize continue instructions
            //           ContinueInstructions = new TextEditControl(ContinueInstructionsWidth, DIText.UsedAs.ContinueInstructions, false);
            //         ContinueInstructions.Size = ContinueInstructions.CalculatedSize;
            //       ContinueInstructions.Location = new Point((((Done.Left + PreviewGroup.Right) >> 1) - (ContinueInstructionsWidth >> 1)), ContinueKeyDrop.Bottom + (ContinueKeyDrop.Height >> 1));
            //     Controls.Add(ContinueInstructions);


            //            ContinueKeyDrop.Left = 8 + ContinueKeyLabel.Right;
            //          ContinueKeyDrop.Items.AddRange(AvailableContinueKeys);
            //        ContinueKeyDrop.SelectedIndex = 0;
            ChildPanelLoc = new Point(this.ScreenPreview.Right + 50, 50);
            ChildPanelSize = new Size(this.ScreenPreview.Width, (AddScreen.Top - AddScreen.Height) - (MockItemRadio.Bottom + MockItemRadio.Height));

            TextInstructions = new TextInstructionsPanel(ChildPanelSize, InstructionBlock);
            TextInstructions.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Bottom;
            TextInstructions.Size = ChildPanelSize;
            TextInstructions.Location = ChildPanelLoc;
            TextInstructions.Visible = false;
            Controls.Add(TextInstructions);

            MockItem = new MockItemPanel(ChildPanelSize, InstructionBlock);
            MockItem.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            MockItem.Size = ChildPanelSize;
            MockItem.Location = ChildPanelLoc;
            MockItem.Visible = false;
            Controls.Add(MockItem);

            KeyedInstructions = new KeyInstructionsPanel(ChildPanelSize, InstructionBlock);
            KeyedInstructions.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            KeyedInstructions.Size = ChildPanelSize;
            KeyedInstructions.Location = ChildPanelLoc;
            KeyedInstructions.Visible = false;
            Controls.Add(KeyedInstructions);


            TextRadio.Checked = false;
            MockItemRadio.Checked = false;
            KeyRadio.Checked = false;
            CurrentInstructionScreenNdx = 0;
            ResumeLayout(false);
        }


        public void SetActiveScreen(CInstructionScreen scrn)
        {
            scrn.PreviewPane = ScreenPreview;
            ScrollingPreview.SelectedPreview = InstructionBlock.GetIndexOf(scrn.URI);
            UpdateScreenDefPanel(InstructionBlock[ScrollingPreview.SelectedPreview]);
        }


        protected void HideMockItemPanel()
        {
            if (MockItem == null)
                return;
            if (Controls.Contains(MockItem))
            {
                MockItem.MockItemScreen = null;
                Controls.Remove(MockItem);
                //                MockItem.Dispose();
                //        MockItem = null;
            }
        }


        private void InstructionScreenPanel_Load(object sender, EventArgs e)
        {
            ValidateInput();
        }

        private void ContinueInstructions_Changed(TextEditControl sender)
        {
            ValidateInput();
        }

        private void TextRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (TextRadio.Checked)
            {
                if (InstructionBlock[CurrentScreenNdx].Type == InstructionScreenType.Text)
                    return;
                CTextInstructionScreen newScrn = new CTextInstructionScreen(InstructionBlock);
                var continueUri = (InstructionBlock[CurrentScreenNdx].ContinueInstructions.Clone() as DIContinueInstructions).URI;
                //                ContinueInstructions.TextDisplayItemUri = continueUri;
                ScrollingPreview.Replace(CurrentInstructionScreenNdx, newScrn);
                InstructionBlock[CurrentInstructionScreenNdx].Dispose();
                newScrn.DIPreview.PreviewPanel = ScreenPreview;
                InstructionBlock.InsertScreen(newScrn, CurrentInstructionScreenNdx);
                ValidateInput();
                UpdateScreenDefPanel(newScrn);
            }
            else
            {
                TextInstructions.Visible = false;
            }
        }

        private void MockItemRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (MockItemRadio.Checked)
            {
                if (InstructionBlock[CurrentScreenNdx].Type == InstructionScreenType.MockItem)
                    return;
                var continueInstructions = InstructionBlock[CurrentScreenNdx].ContinueInstructions.Clone() as DIContinueInstructions;
                InstructionBlock[CurrentInstructionScreenNdx].Dispose();
                var newScrn = new CMockItemScreen(InstructionBlock);
                newScrn.DIPreview.PreviewPanel = ScreenPreview;
                //              ContinueInstructions.TextDisplayItemUri = continueInstructions.URI;
                ScrollingPreview.Replace(CurrentInstructionScreenNdx, newScrn);
                InstructionBlock.InsertScreen(newScrn, CurrentInstructionScreenNdx);
                ValidateInput();
                UpdateScreenDefPanel(newScrn);
            }
            else
            {
                MockItem.Visible = false;
            }
        }


        private void ContinueKeyDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            //            if (ContinueKeyDrop.SelectedIndex != -1)
            //              ContinueInstructions.TextValue = String.Format(Properties.Resources.sDefaultContinueInstructions, ContinueKeyDrop.SelectedItem.ToString());
            ValidateInput();
        }

        private void DeleteScreen_Click(object sender, EventArgs e)
        {
            MainForm.Modified = true;
            CInstructionScreen deletedScrn = InstructionBlock[ScrollingPreview.SelectedPreview];
            SuspendLayout();
            TextInstructions.Visible = false;
            KeyedInstructions.Visible = false;
            MockItem.Visible = false;
            deletedScrn.Dispose();
            if (InstructionBlock.NumScreens == 0)
            {
                var scrn = new CInstructionScreen(InstructionBlock);
                InstructionBlock.InsertScreen(scrn, 0);
                ScrollingPreview.InsertPreview(1, scrn);
            }
            if (ScrollingPreview.SelectedPreview == 0)
            {
                ScrollingPreview.DeletePreview(0);
                ScrollingPreview.SelectedPreview = 0;
            }
            else
            {
                ScrollingPreview.DeletePreview(ScrollingPreview.SelectedPreview, false);
                ScrollingPreview.SelectedPreview = ScrollingPreview.SelectedPreview - 1;
            }
            ResumeLayout(false);
        }

        private void InsertScreen_Click(object sender, EventArgs e)
        {
            if (InstructionBlock[CurrentInstructionScreenNdx].Type == InstructionScreenType.Text)
                TextInstructions.TextInstructionScreen = null;
            if (InstructionBlock[CurrentInstructionScreenNdx].Type == InstructionScreenType.ResponseKey)
                KeyedInstructions.KeyedInstructionScreen = null;
            if (InstructionBlock[CurrentInstructionScreenNdx].Type == InstructionScreenType.MockItem)
                MockItem.MockItemScreen = null;
            InstructionBlock[CurrentInstructionScreenNdx].PreviewPane = null;
            CInstructionScreen scrn = new CInstructionScreen(InstructionBlock);
            scrn.PreviewPane = ScreenPreview;
            ScrollingPreview.InsertPreview(CurrentInstructionScreenNdx, scrn);
            InstructionBlock.InsertScreen(scrn, CurrentInstructionScreenNdx);
            ScrollingPreview.SelectedPreview = ScrollingPreview.SelectedPreview;
            UpdateScreenDefPanel(InstructionBlock[ScrollingPreview.SelectedPreview]);
        }

        private void ManageResponseKeys_Click(object sender, EventArgs e)
        {
            MainForm.ShowResponseKeyPanel();
        }

        private void KeyRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (KeyRadio.Checked)
            {
                if (InstructionBlock[CurrentScreenNdx].Type == InstructionScreenType.ResponseKey)
                    return;
                var continueInstructions = InstructionBlock[CurrentScreenNdx].ContinueInstructions.Clone() as DIContinueInstructions;
                InstructionBlock[CurrentInstructionScreenNdx].Dispose();
                var newScrn = new CKeyInstructionScreen(InstructionBlock);
                newScrn.DIPreview.PreviewPanel = ScreenPreview;
                MainForm.SuspendLayout();
                newScrn.DIPreview.PreviewPanel = ScreenPreview;
                //            ContinueInstructions.TextDisplayItemUri = continueInstructions.URI;
                ScrollingPreview.Replace(CurrentInstructionScreenNdx, newScrn);
                InstructionBlock.InsertScreen(newScrn, CurrentInstructionScreenNdx);
                ValidateInput();
                UpdateScreenDefPanel(newScrn);
                MainForm.ResumeLayout(false);
            }
            else
            {
                KeyedInstructions.Visible = false;
            }
        }

        private void AddScreen_Click(object sender, EventArgs e)
        {
            InstructionBlock.AddScreen(new CInstructionScreen(InstructionBlock));
            ScrollingPreview.InsertPreview(InstructionBlock.NumScreens - 1, InstructionBlock[InstructionBlock.NumScreens - 1]);
            ScrollingPreview.SelectedPreview = InstructionBlock.NumScreens - 1;
            TextInstructions.Visible = false;
            KeyedInstructions.Visible = false;
            MockItem.Visible = false;
        }

        private void Done_Click(object sender, EventArgs e)
        {
            if (InstructionBlock[CurrentInstructionScreenNdx].Type == InstructionScreenType.Text)
                TextInstructions.TextInstructionScreen = null;
            if (InstructionBlock[CurrentInstructionScreenNdx].Type == InstructionScreenType.ResponseKey)
                KeyedInstructions.KeyedInstructionScreen = null;
            if (InstructionBlock[CurrentInstructionScreenNdx].Type == InstructionScreenType.MockItem)
                MockItem.MockItemScreen = null;
            MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
        }


        public new bool Validate()
        {
            try
            {
                InstructionBlock.Validate();
            }
            catch (Exception ex)
            {
                if (MessageBox.Show(this, ex.Message + "\n" + Properties.Resources.sErrorsExistProceed, Properties.Resources.sErrorsExistCaption,
                    MessageBoxButtons.YesNo) == DialogResult.Yes)
                    return true;
                else return false;
            }
            return true;
        }

        private void ContinueKeyLabel_Click(object sender, EventArgs e)
        {

        }

        public new void Dispose()
        {
            if (CurrentInstructionScreenNdx != -1)
            {
                InstructionBlock[CurrentInstructionScreenNdx].PreviewPane = null;
                InstructionBlock[CurrentInstructionScreenNdx].DIPreview.SuspendLayout();
            }
            if (KeyedInstructions != null)
                KeyedInstructions.Dispose();
            if (MockItem != null)
                MockItem.Dispose();
            if (TextInstructions != null)
                TextInstructions.Dispose();
            ScrollingPreview.Dispose();
        }
    }
}
