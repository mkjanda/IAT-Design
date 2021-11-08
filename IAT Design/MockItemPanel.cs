using System;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Linq;

namespace IATClient
{
    partial class MockItemPanel : UserControl
    {
        private TextEditControl MockItemInstructions;
        private static Point MockItemInstructionsLocation = new Point(3, 16);
        private CInstructionBlock InstructionBlock = null;
        private TextEditControl MockItemTextStimulus;
        private new bool IsDisposed { get; set; } = false;
        private static Point MockItemTextStimulusLocation = new Point(3, 16);

        // holds the selected response key name
        private String _ResponseKeyName = String.Empty;

        private CMockItemScreen _MockItemScreen = null;

        public InstructionScreenPanel ParentControl
        {
            get
            {
                return (InstructionScreenPanel)Parent;
            }
        }

        public CMockItemScreen MockItemScreen
        {
            get
            {
                return _MockItemScreen;
            }
            set
            {
                if (value == _MockItemScreen)
                    return;
                if (MockItemScreen != null)
                    MockItemScreen.PreviewPane = null;
                if (value == null)
                {
                    _MockItemScreen = null;
                    return;
                }
                else
                {
                    _MockItemScreen = null;
                    DIBase stim = CIAT.SaveFile.GetDI(value.StimulusUri);
                    if (stim.Type == DIType.StimulusImage)
                    {
                        ImageFileName.Text = (stim as DIStimulusImage).Description;
                        ImageStimulusRadio.Checked = true;
                        ImageStimulusGroup.Enabled = true;
                        TextStimulusGroup.Enabled = false;
                        TextStimulusRadio.Checked = false;
                        MockItemTextStimulus.TextDisplayItemUri = null;
                    }
                    else if (stim.Type == DIType.StimulusText)
                    {
                        ImageFileName.Text = String.Empty;
                        ImageStimulusRadio.Checked = false;
                        ImageStimulusGroup.Enabled = false;
                        TextStimulusGroup.Enabled = true;
                        MockItemTextStimulus.TextDisplayItemUri = stim.URI;
                        TextStimulusRadio.Checked = true;
                    }
                    else if (stim.Type == DIType.Null)
                    {
                        ImageFileName.Text = String.Empty;
                        ImageStimulusRadio.Checked = false;
                        ImageStimulusGroup.Enabled = false;
                        TextStimulusGroup.Enabled = false;
                        TextStimulusRadio.Checked = false;
                        MockItemTextStimulus.TextDisplayItemUri = null;
                    }
                    if (value.ResponseKeyUri != null)
                        ResponseKeyDrop.SelectedItem = CIAT.SaveFile.GetIATKey(value.ResponseKeyUri);
                    MarkAsInvalid.Checked = value.InvalidResponseFlag;
                    OutlineResponse.Checked = value.KeyedDirOutlined;
                    MockItemInstructions.TextDisplayItemUri = value.InstructionsUri;
                    _MockItemScreen = value;
                }
            }
        }

        public bool ValidateInput()
        {
            if (KeyedDirDrop.SelectedIndex == -1)
                throw new CValidationException(EValidationException.MockItemScreenWithoutResponseKey,
                    new CInstructionLocationDescriptor(InstructionBlock, MockItemScreen));
            if (!TextStimulusRadio.Checked && !ImageStimulusRadio.Checked)
                throw new CValidationException(EValidationException.MockItemScreenWithoutStimulus,
                    new CInstructionLocationDescriptor(InstructionBlock, MockItemScreen));
            if (TextStimulusRadio.Checked && (MockItemTextStimulus?.TextValue == String.Empty))
                throw new CValidationException(EValidationException.MockItemScreenWithIncompletelyInitializedTextStimulus,
                    new CInstructionLocationDescriptor(InstructionBlock, MockItemScreen));
            if (ImageStimulusRadio.Checked && (ImageFileName.Text == String.Empty))
                throw new CValidationException(EValidationException.MockItemScreenWithIncompletelyInitializedImageStimulus,
                    new CInstructionLocationDescriptor(InstructionBlock, MockItemScreen));
            return true;
        }


        public MockItemPanel(Size sz, CInstructionBlock b)
        {
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            InstructionBlock = b;

            // initialize mock item instructions
            MockItemInstructions = new TextEditControl(InstructionsGroup.ClientRectangle.Height - 18, InstructionsGroup.ClientRectangle.Width - 6,
                DIText.UsedAs.IatBlockInstructions, false);
            MockItemInstructions.AutoScaleMode = AutoScaleMode.Dpi;
            MockItemInstructions.Location = MockItemInstructionsLocation;
            MockItemInstructions.Size = MockItemInstructions.CalculatedSize;
            InstructionsGroup.Controls.Add(MockItemInstructions);

            // initialize text stimulus edit
            MockItemTextStimulus = new TextEditControl(TextStimulusGroup.ClientRectangle.Width - 6, DIText.UsedAs.Stimulus, false);
            MockItemTextStimulus.AutoScaleMode = AutoScaleMode.Dpi;
            MockItemTextStimulus.Location = MockItemTextStimulusLocation;
            MockItemTextStimulus.Size = MockItemTextStimulus.CalculatedSize;

            KeyedDirDrop.Items.Add("Left");
            KeyedDirDrop.Items.Add("Right");
            TextStimulusGroup.Controls.Add(MockItemTextStimulus);
            TextStimulusRadio.Checked = false;
            TextStimulusGroup.Enabled = false;
            ImageStimulusRadio.Checked = false;
            ImageStimulusGroup.Enabled = false;
            ImageFileName.Text = String.Empty;
            MockItemTextStimulus.TextValue = String.Empty;

            ResponseKeyDrop.SelectedIndexChanged += new EventHandler(ResponseKeyDrop_SelectedIndexChanged);
            PopulateResponseKeyDrop();
            this.ParentChanged += new EventHandler(MockItemPanel_ParentChanged);
        }

        private void MockItemPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent == null)
            {
                if (!IsDisposed)
                    MockItemScreen = null;
            }
        }

        private void ResponseKeyDrop_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.Graphics.FillRectangle(System.Drawing.SystemBrushes.ControlLightLight, e.Bounds);
            if (e.Index == -1)
                return;
            e.Graphics.DrawString(ResponseKeyDrop.Items[e.Index].ToString(), System.Drawing.SystemFonts.DialogFont, System.Drawing.SystemBrushes.ControlText, new PointF(3, 3));
        }

        private void ResponseKeyDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MockItemScreen == null)
                return;
            if (ResponseKeyDrop.SelectedIndex < 0)
                MockItemScreen.ResponseKeyUri = null;
            else
                MockItemScreen.ResponseKeyUri = (ResponseKeyDrop.SelectedItem as CIATKey).URI;
        }

        private void KeyedDirDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (MockItemScreen == null)
                return;
            if (KeyedDirDrop.SelectedIndex != -1)
                MockItemScreen.KeyedDirection = KeyedDirection.FromString(KeyedDirDrop.SelectedItem.ToString());
            else 
                MockItemScreen.KeyedDirection = KeyedDirection.None;
            ParentControl.ValidateInput();
        }

        private void OutlineResponse_CheckedChanged(object sender, EventArgs e)
        {
            if (MockItemScreen == null)
                return;
            MockItemScreen.KeyedDirOutlined = OutlineResponse.Checked;
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = Properties.Resources.sImageFileFilter;
            dlg.Title = Properties.Resources.sOpenImageFileDialogTitle;
            if (dlg.ShowDialog() != DialogResult.OK)
                return;
            if (new FileInfo(dlg.FileName).Length > DIImage.MaxFileSize)
            {
                MessageBox.Show("Only pictures of 4MB or smaller are allowed.", "File Too Large");
                return;
            }
            ImageFileName.Text = System.IO.Path.GetFileNameWithoutExtension(dlg.FileName);
            DIStimulusImage diStim = CIAT.SaveFile.GetDI(MockItemScreen.StimulusUri) as DIStimulusImage;
            try
            {
                diStim.LoadImageFromFile(dlg.FileName);
                diStim.StretchToFit = StretchToFit.Checked;
                MockItemScreen.StimulusUri = diStim.URI;
                ParentControl.ValidateInput();
            }
            catch (Exception ex)
            {
                diStim.Dispose();
                MessageBox.Show("Please choose another image", "Invalid Image File");
            }
        }

        private void StretchToFit_CheckedChanged(object sender, EventArgs e)
        {
            DIStimulusImage stim = CIAT.SaveFile.GetDI(MockItemScreen.StimulusUri) as DIStimulusImage;
            stim.StretchToFit = StretchToFit.Checked;
        }

        private void MarkAsInvalid_CheckedChanged(object sender, EventArgs e)
        {
            if (MockItemScreen == null)
                return;
            MockItemScreen.InvalidResponseFlag = MarkAsInvalid.Checked;
        }

        public void PopulateResponseKeyDrop()
        {
            ResponseKeyDrop.Items.Clear();
            foreach (Uri u in CIAT.SaveFile.GetAllIATKeyUris())
                ResponseKeyDrop.Items.Add(CIAT.SaveFile.GetIATKey(u));
        }

        private void TextStimulusRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (MockItemScreen == null)
                return;
            if (TextStimulusRadio.Checked)
            {
                MockItemScreen.StimulusUri = MockItemTextStimulus.TextDisplayItemUri;
                TextStimulusGroup.Enabled = true;
            }
            else 
            {
                MockItemScreen.StimulusUri = null;
                TextStimulusGroup.Enabled = false;
            }
        }

        private void ImageStimulusRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (MockItemScreen == null)
                return;
            if (ImageStimulusRadio.Checked)
            {
                ImageStimulusGroup.Enabled = true;
                MockItemScreen.StimulusUri = new DIStimulusImage().URI;
            }
            else 
            {
                DIBase stim = (MockItemScreen.StimulusUri != null) ? CIAT.SaveFile.GetDI(MockItemScreen.StimulusUri) : null;
                MockItemScreen.StimulusUri = null;
                stim?.Dispose();
                ImageStimulusGroup.Enabled = false;
            }
        }

        private void ManageKeysButton_Click(object sender, EventArgs e)
        {
            CIATKey key = ResponseKeyDrop.SelectedItem as CIATKey;
            ParentControl.MainForm.ShowResponseKeyPanel();
            PopulateResponseKeyDrop();
            ResponseKeyDrop.SelectedItem = key;
        }

        public new void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
     //       if (MockItemScreen != null)
       //         MockItemScreen.Dispose();
            base.Dispose();
        }
    }
}
