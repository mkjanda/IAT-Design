using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    partial class TextInstructionsPanel : UserControl
    {
        // a flag to indicate the object is updating internally
        private bool IsUpdating;
        private CInstructionBlock InstructionsBlock = null;
        private CTextInstructionScreen _TextInstructionScreen = null;
        private bool IsDisposed { get; set; } = false;
        public CTextInstructionScreen TextInstructionScreen
        {
            get
            {
                return _TextInstructionScreen;
            }
            set
            {
                if (value == _TextInstructionScreen)
                    return;
                if (_TextInstructionScreen != null)
                    _TextInstructionScreen.PreviewPane = null;
                if (value == null)
                {
                    _TextInstructionScreen.PreviewPane = null;
                    _TextInstructionScreen = null;
                    TextInstructions.TextDisplayItemUri = null;
                    JustificationDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.TextInstructionsScreen].Justification.ToString();
                    LineSpacingDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.TextInstructionsScreen].LineSpacing.ToString();
                }
                else
                {
                    _TextInstructionScreen = value;
                    TextInstructions.TextDisplayItemUri = value.InstructionsUri;
                    JustificationDrop.SelectedItem = (CIAT.SaveFile.GetDI(value.InstructionsUri) as DITextInstructionsScreen).Justification.ToString();
                    LineSpacingDrop.SelectedItem = (CIAT.SaveFile.GetDI(value.InstructionsUri) as DITextInstructionsScreen).LineSpacing.ToString();
                }
            }
        }

        public void StartExternalUpdate()
        {
        }

        public void EndExternalUpdate()
        {
        }

        // the text instructions control variables
        private TextEditControl TextInstructions;
        private static Point TextInstructionsControlLocation = new Point(3, 16);
        private static int TextInstructionsControlWidth = 360;
        private static int TextInstructionsControlNumLines = 17;

        // an array of line spacing values
        private static float[] AvailableLineSpacing = { 1, 1.25F, 1.5F, 1.75F, 2, 2.25F, 2.5F, 2.75F, 3 };

        public InstructionScreenPanel ParentControl
        {
            get 
            {
                return (InstructionScreenPanel)Parent;
            }
        }

        public bool ValidateInput()
        {
            if (TextInstructions.TextValue == String.Empty)
            {
                ParentControl.MainForm.ErrorMsg = Properties.Resources.sTextInstructionsBlankException;
                return false;
            }
            ParentControl.MainForm.ErrorMsg = String.Empty;
            return true;
        }

        public TextInstructionsPanel(Size sz, CInstructionBlock b)
        {
            InstructionsBlock = b;
            InitializeComponent();
            IsUpdating = true;
            TextGroupBox.Dock = DockStyle.Fill;
            TextInstructions = new TextEditControl((TextGroupBox.ClientSize.Height - (TextGroupBox.ClientSize.Height - LineSpacingDrop.Top) - LineSpacingDrop.Height), TextGroupBox.ClientRectangle.Width,
                DIText.UsedAs.TextInstructionsScreen, false);
            TextInstructions.Location = TextInstructionsControlLocation;
            TextInstructions.Size = TextInstructions.CalculatedSize;
            TextGroupBox.Controls.Add(TextInstructions);

            for (int ctr = 0; ctr < AvailableLineSpacing.Length; ctr++)
                LineSpacingDrop.Items.Add(AvailableLineSpacing[ctr].ToString());
            LineSpacingDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.TextInstructionsScreen].LineSpacing;
            JustificationDrop.Items.Add("Left");
            JustificationDrop.Items.Add("Center");
            JustificationDrop.Items.Add("Right");
            JustificationDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.TextInstructionsScreen].Justification.ToString();

            this.ParentChanged += new EventHandler(TextInstructionsPanel_ParentChanged);
        }

        private void TextInstructionsPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
            }
            else
            {
                if (!IsDisposed)
                    TextInstructionScreen = null;
            }
        }
        /*
        private void TextInstructions_Changed(TextEditControl sender)
        {
            if (!IsUpdating)
            {
                if (_Instructions != null)
                {
                    _Instructions.BeginMultipleUpdate();
                    _Instructions.Phrase = sender.TextValue;
                    _Instructions.PhraseColor = sender.FontColor;
                    _Instructions.PhraseFontFamily = sender.FontFamily;
                    _Instructions.PhraseFontSize = sender.FontSize;
                    _Instructions.EndMultipleUpdate();
                }
                ParentControl.PreviewPane.InvalidateTextInstructions(Instructions);
                ParentControl.ValidateInput();
            }
        }
        */

        private void LineSpacingDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TextInstructionScreen != null)
                (CIAT.SaveFile.GetDI(TextInstructions.TextDisplayItemUri) as DITextInstructionsScreen).LineSpacing = Convert.ToSingle(LineSpacingDrop.SelectedItem);
        }

        private void JustificationDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (TextInstructionScreen != null)
                (CIAT.SaveFile.GetDI(TextInstructions.TextDisplayItemUri) as DITextInstructionsScreen).Justification = TextJustification.FromString(JustificationDrop.SelectedItem as String);
        }

        public new void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            if (TextInstructionScreen != null)
                TextInstructionScreen.Dispose();
            base.Dispose();

        }
    }
}
