using System;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;

namespace IATClient
{
    public partial class InstructionScreenPanel : UserControl
    {
        // the size of the control
        public static Size InstructionScreenPanelSize = new Size(1010, 645);
        private int CurrentScreenNdx = -1;
        private GroupBox PreviewGroup;
        private ImageDisplay ScreenPreview;
        private Size ChildPanelSize { get; set; } = Size.Empty;
        private Point ChildPanelLoc { get; set; }
        private ScrollingPreviewPanel ScrollingPreview;
        private CInstructionScreen ClipboardItem;
        private bool UpdatingInternally { get; set; }
        // the current block of instructions
        public int InstructionBlockNdx = -1;
        public Uri InstructionBlockUri { get; private set; }


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
                    HideTextInstructionsPanel();
                    HideKeyInstructionsPanel();
                    HideMockItemPanel();
                    if (CurrentScreenNdx == -1)
                    {
                        ScreenPreview.ClearImage();
                        return;
                    }
                    InstructionBlock[CurrentScreenNdx].PreviewPane = ScreenPreview;
                    CInstructionScreen scrn = InstructionBlock[CurrentScreenNdx];
                    ContinueInstructions.TextDisplayItemUri = scrn.ContinueInstructionsUri;
                    ContinueKeyDrop.SelectedIndex = ContinueKeyDrop.Items.IndexOf(scrn.ContinueKey);
                    if (scrn.Type == InstructionScreenType.Text)
                    {
                        ShowTextInstructionsPanel(scrn);
                        TextInstructions.TextInstructionScreen = scrn as CTextInstructionScreen;
                        TextRadio.Checked = true;
                    }
                    else if (scrn.Type == InstructionScreenType.ResponseKey)
                    {
                        ShowKeyInstructionsPanel(scrn);
                        KeyedInstructions.KeyedInstructionScreen = scrn as CKeyInstructionScreen;
                        KeyRadio.Checked = true;
                    }
                    else if (scrn.Type == InstructionScreenType.MockItem)
                    {
                        ShowMockItemPanel(scrn);
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
            ContinueInstructions.TextDisplayItemUri = scrn.ContinueInstructionsUri;
            if (scrn.Type == InstructionScreenType.Text)
            {
                HideKeyInstructionsPanel();
                HideMockItemPanel();
                ShowTextInstructionsPanel(scrn);
            }
            else if (scrn.Type == InstructionScreenType.ResponseKey)
            {
                HideTextInstructionsPanel();
                HideMockItemPanel();
                ShowKeyInstructionsPanel(scrn);
                KeyRadio.Checked = true;
            }
            else if (scrn.Type == InstructionScreenType.MockItem)
            {
                HideTextInstructionsPanel();
                HideKeyInstructionsPanel();
                ShowMockItemPanel(scrn);
                MockItemRadio.Checked = true;
            }
            else
            {
                HideTextInstructionsPanel();
                HideKeyInstructionsPanel();
                HideMockItemPanel();
            }
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
        private TextEditControl ContinueInstructions;
        private static int ContinueInstructionsWidth = 370;

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
            this.Enabled = false;
            InstructionBlockUri = instrBlock.URI;
            InitializeComponent();
            this.Size = InstructionScreenPanelSize;
            PreviewGroup = new GroupBox();
            PreviewGroup.Location = new Point(0, 0);
            PreviewGroup.Text = "Instruction Screen Preview";
            ScreenPreview = new ImageDisplay();
            ScreenPreview.BackColor = CIAT.SaveFile.Layout.BackColor;
            ScreenPreview.BackgroundImage = null;
            ScreenPreview.BackgroundImageLayout = ImageLayout.Stretch;
            double arPreview = (double)CIAT.SaveFile.Layout.InteriorSize.Width / (double)CIAT.SaveFile.Layout.InteriorSize.Height;
            ScreenPreview.Size = Images.ImageMediaType.FullPreview.ImageSize;
            ScreenPreview.Location = new Point(3, 12);
            Controls.Add(ScreenPreview);

            ScrollingPreview = new ScrollingPreviewPanel();//new Size(this.ClientRectangle.Width, Images.ImageManager.ThumbnailSize.Height));
            ScrollingPreview.Orientation = ScrollingPreviewPanel.EOrientation.horizontal;
            ScrollingPreview.Location = new Point(0, 525);
            ScrollingPreview.PreviewSize = Images.ImageManager.ThumbnailSize;
            ScrollingPreview.AutoScaleMode = AutoScaleMode.Font;
            ScrollingPreview.PreviewClickCallback = new Action<int>((ndx) => { CurrentInstructionScreenNdx = ndx; });
            ParentChanged += (sender, args) => { if (Parent != null) ValidateInput(); };
            ScrollingPreview.OnMoveContainerItem = new Action<int, int>((startNdx, endNdx) =>
            {
                InstructionBlock.MoveScreen(startNdx, endNdx);
                ScrollingPreview.SelectedPreview = endNdx;
            });
            ScrollingPreview.SuspendRecalcLayout();
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
            Controls.Add(ScrollingPreview);

            // initialize continue instructions
            ContinueInstructions = new TextEditControl(ContinueInstructionsWidth, DIText.UsedAs.ContinueInstructions, false);
            ContinueInstructions.Size = ContinueInstructions.CalculatedSize;
            ContinueInstructions.Location = new Point((((Done.Left + PreviewGroup.Right) >> 1) - (ContinueInstructionsWidth >> 1)), ContinueKeyDrop.Bottom + (ContinueKeyDrop.Height >> 1));
            Controls.Add(ContinueInstructions);


            ContinueKeyDrop.Left = 8 + ContinueKeyLabel.Right;
            ContinueKeyDrop.Items.AddRange(AvailableContinueKeys);
            ContinueKeyDrop.SelectedIndex = 0;
            ChildPanelSize = new Size(MockItemRadio.Right - InstructionTypeLabel.Left, (AddScreen.Top - AddScreen.Height) - (MockItemRadio.Bottom + MockItemRadio.Height));
            ChildPanelLoc = new Point(InstructionTypeLabel.Left - 40, MockItemRadio.Bottom + MockItemRadio.Height);

            TextRadio.Checked = false;
            MockItemRadio.Checked = false;
            KeyRadio.Checked = false;
            this.PerformLayout();
            this.PerformAutoScale();
            CurrentInstructionScreenNdx = 0;
        }

        private void MoveContainerItem(int StartNdx, int EndNdx)
        {
            InstructionBlock.MoveScreen(StartNdx, EndNdx);
            ScrollingPreview.SelectedPreview = EndNdx;
        }

        public void SetActiveScreen(CInstructionScreen scrn)
        {
            scrn.PreviewPane = ScreenPreview;
            ScrollingPreview.SelectedPreview = InstructionBlock.GetIndexOf(scrn.URI);
            UpdateScreenDefPanel(InstructionBlock[ScrollingPreview.SelectedPreview]);
        }

        protected void HideTextInstructionsPanel()
        {
            if (TextInstructions == null)
                return;
            if (Controls.Contains(TextInstructions))
            {
                Controls.Remove(TextInstructions);
                if (ScrollingPreview.SelectedPreview == InstructionBlock.GetIndexOf(TextInstructions.TextInstructionScreen.URI))
                    TextInstructions.Dispose();
                TextInstructions = null;
            }
        }

        protected void ShowTextInstructionsPanel(CInstructionScreen scrn)
        {
            if (TextInstructions == null)
            {
                TextInstructions = new TextInstructionsPanel(ChildPanelSize, InstructionBlock);
                TextInstructions.Location = ChildPanelLoc;
            }
            if (!Controls.Contains(TextInstructions))
            {
                TextInstructions.TextInstructionScreen = scrn as CTextInstructionScreen;
                scrn.PreviewPane = ScreenPreview;
                ScrollingPreview.SelectedPreview = InstructionBlock.GetIndexOf(scrn.URI);
                Controls.Add(TextInstructions);
            }
        }

        protected void HideMockItemPanel()
        {
            if (MockItem == null)
                return;
            if (Controls.Contains(MockItem))
            {
                MockItem.MockItemScreen = null;
                Controls.Remove(MockItem);
                MockItem.Dispose();
                MockItem = null;
            }
        }

        protected void ShowMockItemPanel(CInstructionScreen scrn)
        {
            if (MockItem == null)
            {
                MockItem = new MockItemPanel(ChildPanelSize, InstructionBlock);
                MockItem.Location = ChildPanelLoc;
            }
            if (!Controls.Contains(MockItem))
            {
                MockItem.MockItemScreen = scrn as CMockItemScreen;
                scrn.PreviewPane = ScreenPreview;
                Controls.Add(MockItem);
            }
        }

        protected void HideKeyInstructionsPanel()
        {
            if (KeyedInstructions == null)
                return;
            if (Controls.Contains(KeyedInstructions))
            {
                KeyedInstructions.KeyedInstructionScreen = null;
                Controls.Remove(KeyedInstructions);
                KeyedInstructions.Dispose();
                KeyedInstructions = null;
            }
        }

        protected void ShowKeyInstructionsPanel(CInstructionScreen scrn)
        {
            if (KeyedInstructions == null)
            {
                KeyedInstructions = new KeyInstructionsPanel(ChildPanelSize, InstructionBlock);
                KeyedInstructions.Location = ChildPanelLoc;
            }
            if (!Controls.Contains(KeyedInstructions))
            {
                KeyedInstructions.KeyedInstructionScreen = scrn as CKeyInstructionScreen;
                scrn.PreviewPane = ScreenPreview;
                Controls.Add(KeyedInstructions);
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
                ContinueInstructions.TextDisplayItemUri = continueUri;
                ScrollingPreview.Replace(CurrentInstructionScreenNdx, newScrn);
                InstructionBlock[CurrentInstructionScreenNdx].Dispose();
                newScrn.DIPreview.PreviewPanel = ScreenPreview;
                InstructionBlock.InsertScreen(newScrn, CurrentInstructionScreenNdx);
                ValidateInput();
                if (TextInstructions == null)
                {
                    TextInstructions = new TextInstructionsPanel(ChildPanelSize, InstructionBlock);
                    TextInstructions.Location = ChildPanelLoc;
                }
                TextInstructions.TextInstructionScreen = newScrn;
                ScrollingPreview.SelectedPreview = InstructionBlock.GetIndexOf(newScrn.URI);
                Controls.Add(TextInstructions);
            }
            else
            {
                HideTextInstructionsPanel();
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
                MainForm.SuspendLayout();
                ContinueInstructions.TextDisplayItemUri = continueInstructions.URI;
                ScrollingPreview.Replace(CurrentInstructionScreenNdx, newScrn);
                InstructionBlock.InsertScreen(newScrn, CurrentInstructionScreenNdx);
                ValidateInput();
                UpdateScreenDefPanel(newScrn);
                ShowMockItemPanel(newScrn);
                MainForm.ResumeLayout(false);
                newScrn.DIPreview.ScheduleInvalidation();
            }
            else
            {
                HideMockItemPanel();
            }
        }


        private void ContinueKeyDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ContinueKeyDrop.SelectedIndex != -1)
                ContinueInstructions.TextValue = String.Format(Properties.Resources.sDefaultContinueInstructions, ContinueKeyDrop.SelectedItem.ToString());
            ValidateInput();
        }

        private void DeleteScreen_Click(object sender, EventArgs e)
        {
            MainForm.Modified = true;
            CInstructionScreen deletedScrn = InstructionBlock[ScrollingPreview.SelectedPreview];
            SuspendLayout();
            HideTextInstructionsPanel();
            HideMockItemPanel();
            HideKeyInstructionsPanel();
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
            HideMockItemPanel();
            HideTextInstructionsPanel();
            HideKeyInstructionsPanel();
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
                ContinueInstructions.TextDisplayItemUri = continueInstructions.URI;
                ScrollingPreview.Replace(CurrentInstructionScreenNdx, newScrn);
                InstructionBlock.InsertScreen(newScrn, CurrentInstructionScreenNdx);
                ValidateInput();
                UpdateScreenDefPanel(newScrn);
                ShowKeyInstructionsPanel(newScrn);
                MainForm.ResumeLayout(false);
            }
            else
            {
                HideKeyInstructionsPanel();
            }
        }

        private void AddScreen_Click(object sender, EventArgs e)
        {
            InstructionBlock.AddScreen(new CInstructionScreen(InstructionBlock));
            ScrollingPreview.InsertPreview(InstructionBlock.NumScreens - 1, InstructionBlock[InstructionBlock.NumScreens - 1]);
            ScrollingPreview.SelectedPreview = InstructionBlock.NumScreens - 1;
            HideTextInstructionsPanel();
            HideMockItemPanel();
            HideKeyInstructionsPanel();
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
