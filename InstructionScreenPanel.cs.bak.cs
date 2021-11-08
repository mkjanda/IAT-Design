using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    partial class InstructionScreenPanel : UserControl
    {
        // the size of the control
        public static Size InstructionScreenPanelSize = new Size(1010, 645);
        private InstructionScreenPreview InstructionsPreview;

        private ScrollingPreviewPanel ScrollingPreview;
        private CInstructionScreen CurrentScreen = null;

        private CInstructionScreen ClipboardItem;

        // the current block of instructions
        public int InstructionBlockNdx = -1;
        private CInstructionBlock _InstructionBlock;
        protected int BlockNdx;


        public CInstructionBlock InstructionBlock
        {
            get
            {
                return _InstructionBlock;
            }
            set
            {
                _InstructionBlock = value;
            }
        }

        // a flag to indicate the control is updating internally
        private bool IsUpdating;

        public InstructionScreenPreview PreviewPane
        {
            get
            {
                return (InstructionScreenPreview)InstructionsPreview;
            }
        }

        /// <summary>
        /// gets the CInstructionScreen object described by the state of the control
        /// </summary>
        public CInstructionScreen DefinedScreen
        {
            get
            {
                if (CurrentScreen != null)
                    return CurrentScreen;
                if (TextRadio.Checked)
                {
                    return TextInstructions.TextInstructionScreen;
                }
                else if (MockItemRadio.Checked)
                {
                    return MockItem.MockItemScreen;
                }
                else if (KeyRadio.Checked)
                {
                    return KeyedInstructions.KeyedInstructionScreen;
                }
                else
                {
                    return CurrentScreen;
                }

            }
            set
            {
                if (CurrentScreen == value)
                    return;
                bool bCalledFromUpdate = false;
                if (IsUpdating == true)
                    bCalledFromUpdate = true;
                IsUpdating = true;
                if (value == null)
                {
                    throw new Exception();
                    /*
                    ContinueInstructions.DisplayItem = null;
                    ContinueKeyDrop.SelectedIndex = 0;
                    ContinueInstructions.TextValue = String.Format(Properties.Resources.sDefaultContinueInstructions, ContinueKeyDrop.SelectedItem.ToString());
                    TextRadio.Checked = false;
                    MockItemRadio.Checked = false;
                    KeyRadio.Checked = false;
                    TextInstructions.Instructions = null;
                    HideTextInstructionsPanel();
                    MockItem.MockItem = null;
                    MockItem.Instructions = null;
                    MockItem.OutlineKeyedDir = false;
                    MockItem.DisplayInvalidResponseMark = false;
                    MockItem.ResponseKeyName = String.Empty;
                    HideMockItemPanel();
                    KeyedInstructions.ResponseKeyName = String.Empty;
                    KeyedInstructions.Instructions = null;
                    HideKeyInstructionsPanel();
                    InstructionsPreview.Mode = InstructionScreenPreview.EMode.None;
                     */
                }
                else if (value.Type == CInstructionScreen.EType.Text)
                {
                    HideMockItemPanel();
                    HideKeyInstructionsPanel();
                    TextRadio.Checked = true; 
                    ContinueKeyDrop.SelectedIndex = ContinueKeyDrop.Items.IndexOf(value.ContinueKey);
                    ContinueInstructions.DisplayItem = value.ContinueInstructions;
                    CTextInstructionScreen Screen = (CTextInstructionScreen)value;
                    TextInstructions.StartExternalUpdate();
                    TextInstructions.TextInstructionScreen = Screen;
                    ShowTextInstructionsPanel();
                    TextInstructions.EndExternalUpdate();
                }
                else if (value.Type == CInstructionScreen.EType.MockItem)
                {
                    HideTextInstructionsPanel();
                    HideKeyInstructionsPanel();
                    MockItemRadio.Checked = true; 
                    ContinueKeyDrop.SelectedIndex = ContinueKeyDrop.Items.IndexOf(value.ContinueKey);
                    ContinueInstructions.DisplayItem = value.ContinueInstructions;
                    CMockItemScreen Screen = (CMockItemScreen)value;
                    MockItem.StartExternalUpdate();
                    MockItem.MockItemScreen = Screen;
                    ShowMockItemPanel();
                    MockItem.EndExternalUpdate();
                }
                else if (value.Type == CInstructionScreen.EType.Key)
                {
                    HideTextInstructionsPanel();
                    HideMockItemPanel();
                    KeyRadio.Checked = true;
                    ContinueKeyDrop.SelectedIndex = ContinueKeyDrop.Items.IndexOf(value.ContinueKey);
                    ContinueInstructions.DisplayItem = value.ContinueInstructions;
                    CKeyInstructionScreen Screen = (CKeyInstructionScreen)value;
                    KeyedInstructions.StartExternalUpdate();
                    KeyedInstructions.KeyedInstructionScreen = Screen;
                    ShowKeyInstructionsPanel();
                    KeyedInstructions.EndExternalUpdate();
                }
                else if (value.Type == CInstructionScreen.EType.None)
                {
                    ContinueKeyDrop.SelectedIndex = ContinueKeyDrop.Items.IndexOf(value.ContinueKey);
                    ContinueInstructions.DisplayItem = value.ContinueInstructions;
                    TextRadio.Checked = false;
                    MockItemRadio.Checked = false;
                    KeyRadio.Checked = false;
                    HideTextInstructionsPanel();
                    HideMockItemPanel();
                    HideKeyInstructionsPanel();
                    CurrentScreen = value;
                    InstructionBlock.OpenScreenForEditing(CurrentScreen, InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview));
                }
                CurrentScreen = value;
                if (!bCalledFromUpdate)
                {
                    IsUpdating = false;
                }
                ValidateInput();
            }
        }

        // the text instructions panel
        private TextInstructionsPanel TextInstructions;
        private static Point TextInstructionsLocation = new Point(574, 26);
        private static Size TextInstructionsSize = new Size(371, 306);

        // the continue instructions control variables
        private TextEditControl ContinueInstructions;
        private static int ContinueInstructionsLocationX = 574;
        private static int ContinueInstructionsWidth = 370;
        private static int ContinueInstructionsNumLines = 1;

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

        public InstructionScreenPanel()
        {
            InitializeComponent();
            IsUpdating = true;

            InstructionsPreview = new InstructionScreenPreview();
            InstructionsPreview.Size = new Size(512, 525);
            InstructionsPreview.Location = new Point(0, 0);
            Controls.Add(InstructionsPreview);

            ScrollingPreview = new ScrollingPreviewPanel();
            ScrollingPreview.Orientation = ScrollingPreviewPanel.EOrientation.horizontal;
            ScrollingPreview.PreviewSize = new Size(112, 112);
            ScrollingPreview.Size = new Size(910, 122);
            ScrollingPreview.AutoScaleMode = AutoScaleMode.Font;
            ScrollingPreview.Dock = DockStyle.Bottom;
            ScrollingPreview.Location = new Point(0, (int)(525F * CIATLayout.YScale));
            ScrollingPreview.PreviewClickCallback += new ScrollingPreviewPanel.PreviewClickHandler(InstructionScreen_Changed);
            ScrollingPreview.OnMoveContainerItem += new ScrollingPreviewPanel.MoveContainerItemHandler(MoveContainerItem);
            Controls.Add(ScrollingPreview);



            // initialize continue instructions
            ContinueInstructions = new TextEditControl(ContinueInstructionsNumLines, ContinueInstructionsWidth, 
                CIATPreferences.CFontPreferences.EUsedFor.ContinueInstructions, false);
            ContinueInstructions.Size = ContinueInstructions.CalculatedSize;
            ContinueInstructions.Location = new Point(ContinueInstructionsLocationX, 520 - ContinueInstructions.Size.Height);
            ContinueInstructions.DataChanged = new TextEditControl.DataChangedCallback(ContinueInstructions_Changed);
            Controls.Add(ContinueInstructions);
            ContinueKeyLabel.Location = new Point(ContinueKeyLabel.Location.X, ContinueInstructions.Location.Y - 25);
            ContinueKeyDrop.Location = new Point(ContinueKeyDrop.Location.X, ContinueInstructions.Location.Y - 29);

            ContinueKeyDrop.Items.AddRange(AvailableContinueKeys);
            ContinueKeyDrop.SelectedIndex = 0;


            // initialize text instructions control
            TextInstructions = new TextInstructionsPanel();
            TextInstructions.Location = TextInstructionsLocation;
            TextInstructions.Size = TextInstructionsSize;

            TextRadio.Checked = false;
            MockItemRadio.Checked = false;
            KeyRadio.Checked = false;

            // initialize mock item control
            MockItem = new MockItemPanel();
            MockItem.Location = MockItemLocation;
            MockItem.Size = MockItemSize;

            // initialize keyed instructions control
            KeyedInstructions = new KeyInstructionsPanel();
            KeyedInstructions.Location = KeyedInstructionsLocation;
            KeyedInstructions.Size = KeyedInstructionsSize;
            IsUpdating = false;
//            this.Disposed += new EventHandler(InstructionScreenPanel_Disposed);
        }
        /*
        void InstructionScreenPanel_Disposed(object sender, EventArgs e)
        {
            InstructionBlock.DestroyCompositeImageDictionary();
        }
        */
        private void MoveContainerItem(int StartNdx, int EndNdx)
        {
            InstructionBlock.MoveScreen(StartNdx, EndNdx);
            DefinedScreen = InstructionBlock[ScrollingPreview.SelectedPreview];
        }

        private void InstructionScreen_Changed(int NewNdx)
        {
            InstructionBlock.CloseScreenForEditing();
            DefinedScreen = InstructionBlock[NewNdx];
            InstructionBlock.OpenScreenForEditing(CurrentScreen, InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview));
            BlockNdx = NewNdx;
        }

        public void SetActiveScreen(CInstructionScreen scrn)
        {
            InstructionBlock.CloseScreenForEditing();
            DefinedScreen = scrn;
            InstructionBlock.OpenScreenForEditing(scrn, InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview));
            BlockNdx = InstructionBlock.GetIndexOf(scrn);
            ScrollingPreview.SelectedPreview = BlockNdx;
        }

        protected void HideTextInstructionsPanel()
        {
            if (Controls.Contains(TextInstructions))
            {
                Controls.Remove(TextInstructions);
            }
        }

        protected void ShowTextInstructionsPanel()
        {
            if (!Controls.Contains(TextInstructions))
            {
                Controls.Add(TextInstructions);
//                InstructionBlock.OpenScreenForEditing(DefinedScreen);
            }
        }       
    
        protected void HideMockItemPanel()
        {
            if (Controls.Contains(MockItem))
            {
                Controls.Remove(MockItem);
            }
        }

        protected void ShowMockItemPanel()
        {
            if (!Controls.Contains(MockItem))
            {
                Controls.Add(MockItem);
            }

        }

        protected void HideKeyInstructionsPanel()
        {
            if (Controls.Contains(KeyedInstructions))
            {
                Controls.Remove(KeyedInstructions);
            }
        }

        protected void ShowKeyInstructionsPanel()
        {
            if (!Controls.Contains(KeyedInstructions))
            {
                Controls.Add(KeyedInstructions);
            }
        }

        private void InstructionScreenPanel_Load(object sender, EventArgs e)
        {
            // initialize continue key drop
            IsUpdating = true;
            ValidateInput();
            IsUpdating = false;
        }

        private void ContinueInstructions_Changed(TextEditControl sender)
        {
            if (!IsUpdating)
            {
          //      InstructionsPreview.InvalidateContinueInstructions(sender.TextDisplayItem);
                ValidateInput();
            }
        }

        private void TextRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsUpdating)
            {
                if (TextRadio.Checked)
                {
                    CInstructionScreen currScrn = CurrentScreen;
                    DefinedScreen = new CTextInstructionScreen();
                    InstructionBlock.ReplaceCurrentScreen(CurrentScreen, InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview),
                        ScrollingPreview[ScrollingPreview.SelectedPreview], new ThumbnailNotification(ScrollingPreview[ScrollingPreview.SelectedPreview].SetImage));
                    ShowTextInstructionsPanel();
                    if (currScrn != null)
                    {
                        currScrn.Dispose();
                    }
                    ValidateInput();
                }
                else
                {
                    HideTextInstructionsPanel();
                    TextInstructions.TextInstructionScreen.Dispose();
                    CurrentScreen = null;
                }
            }
        }

        private void MockItemRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsUpdating)
            {
                if (MockItemRadio.Checked)
                {
                    CInstructionScreen currScrn = CurrentScreen;
                    DefinedScreen = new CMockItemScreen();
                    InstructionBlock.ReplaceCurrentScreen(CurrentScreen, InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview),
                        ScrollingPreview[ScrollingPreview.SelectedPreview], new ThumbnailNotification(ScrollingPreview[ScrollingPreview.SelectedPreview].SetImage));
                    ShowMockItemPanel();
                    if (currScrn != null)
                    {
                        currScrn.Dispose();
                    }
                    ValidateInput();
                }
                else
                {
                    HideMockItemPanel();
                    MockItem.MockItemScreen.Dispose();
                    CurrentScreen = null;
                }
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
            InstructionBlock.CloseScreenForEditing();
            DefinedScreen.Dispose();
            if (InstructionBlock.NumScreens == 1)
            {
                InstructionBlock.RemoveScreenAt(0);
                InstructionBlock.AddScreen(new CInstructionScreen(), ScrollingPreview[ScrollingPreview.SelectedPreview], 
                    new ThumbnailNotification(ScrollingPreview[ScrollingPreview.SelectedPreview].SetImage));
                DefinedScreen = InstructionBlock[0];
                ScrollingPreview.SelectedPreview = 0;
            }
            else
            {
                InstructionBlock.RemoveScreenAt(BlockNdx);
                ScrollingPreview.DeletePreview(BlockNdx);
                if (BlockNdx >= InstructionBlock.NumScreens)
                    BlockNdx--;
                DefinedScreen = InstructionBlock[BlockNdx];
                ScrollingPreview.SelectedPreview = BlockNdx;
            }
            InstructionBlock.OpenScreenForEditing(DefinedScreen, InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview));
        }

        private void InsertScreen_Click(object sender, EventArgs e)
        {
            MainForm.Modified = true;
            InstructionBlock.CloseScreenForEditing();
            IsUpdating = true;
            CInstructionScreen scrn = new CInstructionScreen();
            ScrollingPreview.InsertInstructionScreenPreview(BlockNdx, scrn);
            InstructionBlock.InsertScreen(BlockNdx, scrn, ScrollingPreview[BlockNdx], new ThumbnailNotification(ScrollingPreview[BlockNdx].SetImage));
            DefinedScreen = scrn;
            ScrollingPreview.SelectedPreview = BlockNdx;
            IsUpdating = false;
        }

        private void ManageResponseKeys_Click(object sender, EventArgs e)
        {
            MainForm.ShowResponseKeyPanel();
        }

        private void InstructionScreenPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                IsUpdating = true;
                PreviewPane.CreateGraphicsObjects();
                MockItem.StartExternalUpdate();
                MockItem.EndExternalUpdate();
                KeyedInstructions.StartExternalUpdate();
                KeyedInstructions.EndExternalUpdate();
                BlockNdx = 0;
                ScrollingPreview.ResetScroll();

                if (InstructionBlock.NumScreens != 0)
                {
                    for (int ctr = 0; ctr < InstructionBlock.NumScreens; ctr++)
                    {
                        ScrollingPreview.InsertInstructionScreenPreview(ctr, InstructionBlock[ctr]);
                        InstructionBlock.WireThumbnailCallback(InstructionBlock[ctr], ScrollingPreview[ctr], new ThumbnailNotification(ScrollingPreview[ctr].SetImage));
                        //                        InstructionBlock.InsertScreen(ctr, InstructionBlock[ctr], ScrollingPreview[ctr], new ThumbnailNotification(ScrollingPreview[ctr].SetImage));
                    }
                    DefinedScreen = InstructionBlock[BlockNdx];
                    InstructionBlock.OpenScreenForEditing(InstructionBlock[0], InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview));
                }
                else
                {
                    CInstructionScreen scrn = new CInstructionScreen();
                    ScrollingPreview.InsertInstructionScreenPreview(0, DefinedScreen);
                    InstructionBlock.AddScreen(scrn, ScrollingPreview[0], new ThumbnailNotification(ScrollingPreview[0].SetImage));
                    DefinedScreen = scrn;
                }
                ScrollingPreview.SelectedPreview = 0;
                IsUpdating = false;
                ScrollingPreview.StartTimer();
            }
            else
            {
                InstructionBlock.CloseScreenForEditing();
                InstructionBlock.FinalizeCompositeImageDictionary();
                ScrollingPreview.Clear();
                ScrollingPreview.StopTimer();
                PreviewPane.DisposeofGraphicsObjects();
                HideKeyInstructionsPanel();
                HideMockItemPanel();
                HideTextInstructionsPanel();
            }
        }

        private void KeyRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (!IsUpdating)
            {
                if (KeyRadio.Checked)
                {
                    CInstructionScreen currScrn = CurrentScreen;
                    DefinedScreen = new CKeyInstructionScreen();
                    InstructionBlock.ReplaceCurrentScreen(CurrentScreen, InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview),
                        ScrollingPreview[ScrollingPreview.SelectedPreview], new ThumbnailNotification(ScrollingPreview[ScrollingPreview.SelectedPreview].SetImage));
                    ShowKeyInstructionsPanel();
                    if (currScrn != null)
                    {
                        currScrn.Dispose();
                    }
                    ValidateInput();
                }
                else
                {
                    HideKeyInstructionsPanel();
                    KeyedInstructions.KeyedInstructionScreen.Dispose();
                    CurrentScreen = null;
                }
            }
        }

        public void Preview_Changed()
        {
            /*
             if ((BlockNdx >= 0) && (BlockNdx < InstructionBlock.NumScreens))
                ScrollingPreview.SetPreview(BlockNdx, DefinedScreen);
             * */
        }

        private void AddScreen_Click(object sender, EventArgs e)
        {
            MainForm.Modified = true;
            InstructionBlock.CloseScreenForEditing();
            BlockNdx = InstructionBlock.NumScreens;
            CInstructionScreen scrn = new CInstructionScreen();
            ScrollingPreview.InsertInstructionScreenPreview(BlockNdx, scrn);
            InstructionBlock.InsertScreen(BlockNdx, scrn, ScrollingPreview[BlockNdx], new ThumbnailNotification(ScrollingPreview[BlockNdx].SetImage));
            DefinedScreen = scrn;
//            InstructionBlock.OpenScreenForEditing(DefinedScreen);
            ScrollingPreview.SelectedPreview = BlockNdx;
        }

        private void Done_Click(object sender, EventArgs e)
        {
            MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
        }

        public void DoCut()
        {
            int CutNdx = ScrollingPreview.SelectedPreview;
            ClipboardItem = CInstructionScreen.Clone(DefinedScreen);
            ScrollingPreview.DeletePreview(CutNdx);
            InstructionBlock.CloseScreenForEditing();
            InstructionBlock.RemoveScreenAt(CutNdx);
            if (CutNdx > InstructionBlock.NumScreens)
                CutNdx = InstructionBlock.NumScreens - 1;
            ScrollingPreview.SelectedPreview = CutNdx;
            DefinedScreen = InstructionBlock[CutNdx];
            InstructionBlock.OpenScreenForEditing(DefinedScreen, InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview));
            if (InstructionBlock.NumScreens < 2)
            {
                ((IATConfigMainForm)Parent).EditCutEnabled = false;
                ((IATConfigMainForm)Parent).EditDeleteEnabled = false;
            }
            ((IATConfigMainForm)Parent).EditPasteEnabled = true;
        }

        public void DoCopy()
        {
            ClipboardItem = InstructionBlock[ScrollingPreview.SelectedPreview];
            ((IATConfigMainForm)Parent).EditPasteEnabled = true;
        }

        public void DoDelete()
        {
            int DelNdx = ScrollingPreview.SelectedPreview;
            ScrollingPreview.DeletePreview(DelNdx);
            InstructionBlock.RemoveScreenAt(DelNdx);
            if (DelNdx > InstructionBlock.NumScreens)
                DelNdx = InstructionBlock.NumScreens - 1;
            DefinedScreen = InstructionBlock[DelNdx];
            InstructionBlock.OpenScreenForEditing(DefinedScreen, InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview));
            ScrollingPreview.SelectedPreview = DelNdx;
            if (InstructionBlock.NumScreens < 2)
            {
                ((IATConfigMainForm)Parent).EditCutEnabled = false;
                ((IATConfigMainForm)Parent).EditDeleteEnabled = false;
            }
        }

        public void DoPaste()
        {
            int PasteNdx = ScrollingPreview.SelectedPreview;
            CInstructionScreen scrn = CInstructionScreen.Clone(ClipboardItem);
            InstructionBlock.InsertScreen(PasteNdx, scrn, ScrollingPreview[PasteNdx], new ThumbnailNotification(ScrollingPreview[PasteNdx].SetImage));
            DefinedScreen = scrn;
            ScrollingPreview.InsertInstructionScreenPreview(PasteNdx, scrn);
            ScrollingPreview.SelectedPreview = PasteNdx;
            InstructionBlock.CloseScreenForEditing();
            InstructionBlock.OpenScreenForEditing(DefinedScreen, InstructionsPreview, new CCompositeImage.ImageGeneratedHandler(InstructionsPreview.InvalidatePreview));
            if (InstructionBlock.NumScreens > 1)
            {
                ((IATConfigMainForm)Parent).EditCutEnabled = true;
                ((IATConfigMainForm)Parent).EditDeleteEnabled = true;
            }
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
    }
}
