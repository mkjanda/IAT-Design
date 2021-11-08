using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public partial class InstructionScreenPanel : UserControl
    {
        // the size of the control
        public static Size InstructionScreenPanelSize = new Size(1010, 645);
        private int CurrentScreenNdx = 0;
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

        public int CurrentInstructionScreen
        {
            get
            {
                return CurrentScreenNdx;
            }
            set
            {
                try
                {
                    if (CurrentScreenNdx == value)
                        return;
                    if (CurrentScreenNdx != -1)
                    {
                        InstructionBlock[CurrentScreenNdx].PreviewPane = null;
                        InstructionBlock[CurrentScreenNdx].DIPreview.SuspendLayout();
                    }
                    CurrentScreenNdx = value;
                    TextRadio.Checked = false;
                    KeyRadio.Checked = false;
                    MockItemRadio.Checked = false;
                    HideTextInstructionsPanel();
                    HideKeyInstructionsPanel();
                    HideMockItemPanel();
                    if (CurrentScreenNdx == -1)
                    {
                        ScreenPreview.SetImage((Bitmap)null);
                        return;
                    }
                    InstructionBlock[CurrentScreenNdx].PreviewPane = ScreenPreview;
                    InstructionBlock[CurrentScreenNdx].DIPreview.ResumeLayout(true);
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
                        MockItem.MockItemScreen = scrn as CMockItemScreen;
                        MockItemRadio.Checked = true;
                    }
                    ValidateInput();
                }
                finally
                {
                    this.Enabled = true;
                }
            }
        }


        public void UpdateScreenDefPanel(CInstructionScreen scrn)
        {
            if (scrn.Type == InstructionScreenType.Text)
            {
                HideKeyInstructionsPanel();
                HideMockItemPanel();
                ShowTextInstructionsPanel(scrn);
                TextRadio.Checked = true;
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
                return true;
            try
            {
                if (!TextRadio.Checked && !MockItemRadio.Checked && !KeyRadio.Checked)
                {
                    MainForm.ErrorMsg = Properties.Resources.sNoInstructionScreenTypeSelectedException;
                    throw new Exception();
                }
                if (ContinueInstructions.TextValue == String.Empty)
                {
                    MainForm.ErrorMsg = Properties.Resources.sContinueInstructionsBlankException;
                    throw new Exception();
                }
                if (TextRadio.Checked)
                {
                    if (!TextInstructions.ValidateInput())
                        throw new Exception();
                }
                if (MockItemRadio.Checked)
                {
                    if (!MockItem.ValidateInput())
                        throw new Exception();
                }
            }
            catch (Exception)
            {
                return false;
            }
            InsertScreen.Enabled = true;
            MainForm.ErrorMsg = String.Empty;
            return true;
        }

        public InstructionScreenPanel(CInstructionBlock instrBlock)
        {
            this.Enabled = false;
            InstructionBlockUri = instrBlock.URI;
            InitializeComponent();

            PreviewGroup = new GroupBox();
            PreviewGroup.Location = new Point(0, 0);
            PreviewGroup.ClientSize = new Size(500, 500) + new Size(12, 18);
            PreviewGroup.Text = "Instruction Screen Preview";
            ScreenPreview = new ImageDisplay();
            ScreenPreview.BackColor = CIAT.SaveFile.Layout.BackColor;
            ScreenPreview.BackgroundImage = null;
            ScreenPreview.BackgroundImageLayout = ImageLayout.Stretch;
            double arPreview = (double)CIAT.SaveFile.Layout.InteriorSize.Width / (double)CIAT.SaveFile.Layout.InteriorSize.Height;
            ScreenPreview.Size = Images.ImageMediaType.FullPreview.ImageSize;
            ScreenPreview.Location = new Point(3, 12);
            PreviewGroup.Controls.Add(ScreenPreview);
            Controls.Add(PreviewGroup);

            ScrollingPreview = new ScrollingPreviewPanel();
            ScrollingPreview.Orientation = ScrollingPreviewPanel.EOrientation.horizontal;
            ScrollingPreview.Location = new Point(0, 525);
            ScrollingPreview.Size = new Size(this.ClientRectangle.Width, Images.ImageManager.ThumbnailSize.Height + 10);
            ScrollingPreview.PreviewSize = Images.ImageManager.ThumbnailSize;
            ScrollingPreview.AutoScaleMode = AutoScaleMode.Font;
            ScrollingPreview.PreviewClickCallback = new Action<int>((ndx) => { CurrentInstructionScreen = ndx; });
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
                (CIAT.SaveFile.GetDI(InstructionBlock[ctr].PreviewUri) as DIPreview).ResumeLayout(true);
            }
            if (InstructionBlock.NumScreens == 0)
            {
                InstructionBlock.AddScreen(new CInstructionScreen(InstructionBlock));
                ScrollingPreview.InsertPreview(0, InstructionBlock[0]);
            }
            this.HandleCreated += (sender, args) => this.BeginInvoke(new Action(() => { ScrollingPreview.ResumeRecalcLayout(); }));
            if (IsHandleCreated)
                this.BeginInvoke(new Action(() => { ScrollingPreview.ResumeRecalcLayout(); }));
            ScrollingPreview.ResumeRecalcLayout();
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
                TextInstructions.TextInstructionScreen = null;
                Controls.Remove(TextInstructions);
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
                CInstructionScreen currScreen = InstructionBlock[CurrentInstructionScreen];
                if (currScreen.Type == InstructionScreenType.Text)
                    return;
                MainForm.SuspendLayout();
                CTextInstructionScreen newScrn = new CTextInstructionScreen(InstructionBlock);
                newScrn.DIPreview.PreviewPanel = ScreenPreview;
                ScrollingPreview.Replace(CurrentInstructionScreen, newScrn);
                InstructionBlock.InsertScreen(newScrn, CurrentInstructionScreen);
                currScreen.Dispose();
                ValidateInput();
                UpdateScreenDefPanel(newScrn);
                ShowTextInstructionsPanel(newScrn);
                MainForm.ResumeLayout(false);
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
                CInstructionScreen currScreen = InstructionBlock[CurrentInstructionScreen];
                if (currScreen.Type == InstructionScreenType.MockItem)
                    return;
                MainForm.SuspendLayout();
                CMockItemScreen newScrn = new CMockItemScreen(InstructionBlock);
                newScrn.ContinueInstructions = InstructionBlock[CurrentInstructionScreen].ContinueInstructions.Clone() as DIContinueInstructions;
                newScrn.DIPreview.PreviewPanel = ScreenPreview;
                ScrollingPreview.Replace(CurrentInstructionScreen, newScrn);
                InstructionBlock.InsertScreen(newScrn, CurrentInstructionScreen);
                currScreen.Dispose();
                ValidateInput();
                ShowMockItemPanel(newScrn);
                UpdateScreenDefPanel(newScrn);
                MainForm.ResumeLayout(false);
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
            if (InstructionBlock.NumScreens == 1)
            {
                CInstructionScreen scrn = new CInstructionScreen(InstructionBlock);
                scrn.PreviewPane = ScreenPreview;
                InstructionBlock.InsertScreen(scrn, 0);
                ScrollingPreview.InsertPreview(0, scrn);
                ScrollingPreview.DeletePreview(1);
                ScrollingPreview.SelectedPreview = 0;
                UpdateScreenDefPanel(scrn);
            }
            else
            {
                ScrollingPreview.DeletePreview(CurrentInstructionScreen);
                if (CurrentInstructionScreen < InstructionBlock.NumScreens - 1)
                    ScrollingPreview.SelectedPreview = CurrentInstructionScreen;
                else
                    ScrollingPreview.SelectedPreview = CurrentInstructionScreen - 1;
            }
            deletedScrn.Dispose();
            ResumeLayout(false);
        }

        private void InsertScreen_Click(object sender, EventArgs e)
        {
            if (InstructionBlock[CurrentInstructionScreen].Type == InstructionScreenType.Text)
                TextInstructions.TextInstructionScreen = null;
            if (InstructionBlock[CurrentInstructionScreen].Type == InstructionScreenType.ResponseKey)
                KeyedInstructions.KeyedInstructionScreen = null;
            if (InstructionBlock[CurrentInstructionScreen].Type == InstructionScreenType.MockItem)
                MockItem.MockItemScreen = null;
            InstructionBlock[CurrentInstructionScreen].PreviewPane = null;
            CInstructionScreen scrn = new CInstructionScreen(InstructionBlock);
            scrn.PreviewPane = ScreenPreview;
            ScrollingPreview.InsertPreview(CurrentInstructionScreen, scrn);
            InstructionBlock.InsertScreen(scrn, CurrentInstructionScreen);
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
                CInstructionScreen currScreen = InstructionBlock[CurrentInstructionScreen];
                if (currScreen.Type == InstructionScreenType.ResponseKey)
                    return;
                MainForm.SuspendLayout();
                CKeyInstructionScreen newScrn = new CKeyInstructionScreen(InstructionBlock);
                newScrn.ContinueInstructions = InstructionBlock[CurrentInstructionScreen].ContinueInstructions.Clone() as DIContinueInstructions;
                newScrn.DIPreview.PreviewPanel = ScreenPreview;
                ScrollingPreview.Replace(CurrentInstructionScreen, newScrn);
                InstructionBlock.InsertScreen(newScrn, CurrentInstructionScreen);
                currScreen.Dispose();
                ValidateInput();
                ShowKeyInstructionsPanel(newScrn);
                UpdateScreenDefPanel(newScrn);
                MainForm.ResumeLayout(false);
            }
            else
            {
                HideKeyInstructionsPanel();
            }
        }

        private void AddScreen_Click(object sender, EventArgs e)
        {
            HideTextInstructionsPanel();
            HideMockItemPanel();
            HideKeyInstructionsPanel();
            InstructionBlock.AddScreen(new CInstructionScreen(InstructionBlock));
            ScrollingPreview.InsertPreview(InstructionBlock.NumScreens - 1, InstructionBlock[InstructionBlock.NumScreens - 1]);
            ScrollingPreview.SelectedPreview = InstructionBlock.NumScreens - 1;
        }

        private void Done_Click(object sender, EventArgs e)
        {
            if (InstructionBlock[CurrentInstructionScreen].Type == InstructionScreenType.Text)
                TextInstructions.TextInstructionScreen = null;
            if (InstructionBlock[CurrentInstructionScreen].Type == InstructionScreenType.ResponseKey)
                KeyedInstructions.KeyedInstructionScreen = null;
            if (InstructionBlock[CurrentInstructionScreen].Type == InstructionScreenType.MockItem)
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
            if (CurrentInstructionScreen != -1)
            {
                InstructionBlock[CurrentInstructionScreen].PreviewPane = null;
                InstructionBlock[CurrentInstructionScreen].DIPreview.SuspendLayout();
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
