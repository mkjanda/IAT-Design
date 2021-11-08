using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    partial class KeyInstructionsPanel : UserControl
    {
        // the text instructions control variables
        public TextEditControl TextInstructions { get; private set; } = null;
        private static Point TextInstructionsControlLocation = new Point(3, 50);
        private CInstructionBlock InstructionBlock = null;
        private new bool IsDisposed { get; set; } = false;
        // an array of line spacing values
        private static float[] AvailableLineSpacing = { 1, 1.25F, 1.5F, 1.75F, 2, 2.25F, 2.5F, 2.75F, 3 };

        private CKeyInstructionScreen _KeyedInstructionScreen = null;
        
        public InstructionScreenPanel ParentControl
        {
            get 
            {
                return (InstructionScreenPanel)Parent;
            }
        }

        public CKeyInstructionScreen KeyedInstructionScreen
        {
            get
            {
                return _KeyedInstructionScreen;
            }
            set
            {
                if (value == _KeyedInstructionScreen)
                    return;
                if (_KeyedInstructionScreen != null)
                    _KeyedInstructionScreen.PreviewPane = null;
                if (value == null)
                {
                    _KeyedInstructionScreen = null;
                    ResponseKeyDrop.SelectedIndex = -1;
                    TextInstructions.TextDisplayItemUri = null;
                    LineSpacingDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.KeyedInstructionsScreen].LineSpacing.ToString();
                    JustificationDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.KeyedInstructionsScreen].Justification.ToString();
                    return;
                }
                else
                {
                    _KeyedInstructionScreen = value;
                    if (value.ResponseKeyUri != null)
                        ResponseKeyDrop.SelectedItem = CIAT.SaveFile.GetIATKey(value.ResponseKeyUri);
                    else
                        ResponseKeyDrop.SelectedIndex = -1;
                    TextInstructions.TextDisplayItemUri = value.InstructionsUri;
                    LineSpacingDrop.SelectedItem = (CIAT.SaveFile.GetDI(value.InstructionsUri) as DIKeyedInstructionsScreen).LineSpacing.ToString();
                    JustificationDrop.SelectedItem = (CIAT.SaveFile.GetDI(value.InstructionsUri) as DIKeyedInstructionsScreen).Justification.ToString();
                }
            }
        }

        private Uri ResponseKeyUri
        {
            get
            {
                if (ResponseKeyDrop.SelectedIndex == -1)
                    return null;
                return (ResponseKeyDrop.SelectedItem as CIATKey).URI;
            }
            set
            {
                if (value == null)
                    ResponseKeyDrop.SelectedIndex = -1;
                else
                    ResponseKeyDrop.SelectedItem = CIAT.SaveFile.GetIATKey(value);
            }
        }

        public bool ValidateInput()
        {
            if (TextInstructions.TextValue == String.Empty)
                throw new CValidationException(EValidationException.TextInstructionsBlank,
                    new CInstructionLocationDescriptor(InstructionBlock, KeyedInstructionScreen));
            if (ResponseKeyDrop.SelectedIndex == -1)
                throw new CValidationException(EValidationException.KeyInstructionScreenWithoutResponseKey,
                    new CInstructionLocationDescriptor(InstructionBlock, KeyedInstructionScreen));
            return true;
        }

        public KeyInstructionsPanel(Size sz, CInstructionBlock b)
        {
            InitializeComponent();
            InstructionBlock = b;
            TextInstructions = new TextEditControl(KeyInstructionsBox.Height - (KeyInstructionsBox.Height - LineSpacingDrop.Top) - (ResponseKeyDrop.Bottom + ResponseKeyDrop.Height), KeyInstructionsBox.ClientRectangle.Width,
                DIText.UsedAs.KeyedInstructionsScreen, false);
            TextInstructions.Location = TextInstructionsControlLocation;
            TextInstructions.Size = TextInstructions.CalculatedSize;
            KeyInstructionsBox.Controls.Add(TextInstructions);

            for (int ctr = 0; ctr < AvailableLineSpacing.Length; ctr++)
                LineSpacingDrop.Items.Add(AvailableLineSpacing[ctr].ToString());
            LineSpacingDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.KeyedInstructionsScreen].LineSpacing;
            LineSpacingDrop.SelectedIndexChanged += new EventHandler(LineSpacingDrop_SelectedIndexChanged);

            JustificationDrop.Items.Add("Left");
            JustificationDrop.Items.Add("Center");
            JustificationDrop.Items.Add("Right");
            JustificationDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.KeyedInstructionsScreen].Justification.ToString();
            JustificationDrop.SelectedIndexChanged += new EventHandler(JustificationDrop_SelectedIndexChanged);

            PopulateResponseKeyDrop();
            ResponseKeyDrop.SelectedIndexChanged += new EventHandler(ResponseKeyDrop_SelectedIndexChanged);
            this.ParentChanged += new EventHandler(KeyInstructionsPanel_ParentChanged);
        }

        private void JustificationDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (JustificationDrop.SelectedIndex == -1)
                return;
            if (_KeyedInstructionScreen != null)
            {
                (CIAT.SaveFile.GetDI(_KeyedInstructionScreen.InstructionsUri) as DIKeyedInstructionsScreen).Justification = TextJustification.FromString(JustificationDrop.SelectedItem.ToString());
            }
        }

        private void LineSpacingDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (LineSpacingDrop.SelectedIndex == -1)
                return;
            if (_KeyedInstructionScreen != null)
                (CIAT.SaveFile.GetDI(_KeyedInstructionScreen.InstructionsUri) as DIKeyedInstructionsScreen).LineSpacing = Convert.ToSingle(LineSpacingDrop.SelectedItem);
        }

        private void KeyInstructionsPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent == null)
            {
                if (!IsDisposed)
                    KeyedInstructionScreen = null;
            }
            else
            {
            }
        }

        private void ResponseKeyDrop_DrawItem(object sender, DrawItemEventArgs e)
        {
            e.Graphics.FillRectangle(System.Drawing.SystemBrushes.ControlLightLight, e.Bounds);
            if (e.Index == -1)
                return;
            e.Graphics.DrawString(ResponseKeyDrop.Items[e.Index].ToString(), System.Drawing.SystemFonts.DialogFont, System.Drawing.SystemBrushes.ControlText, new PointF(3, 3));
        }

        private void PopulateResponseKeyDrop()
        {
            ResponseKeyDrop.Items.Clear();
            foreach (CIATKey key in CIAT.SaveFile.GetAllIATKeyUris().Select(u => CIAT.SaveFile.GetIATKey(u)))
                ResponseKeyDrop.Items.Add(key);
            ResponseKeyDrop.SelectedIndex = -1;
        }

        private void ResponseKeyDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (KeyedInstructionScreen == null)
                return;
            if (ResponseKeyDrop.SelectedItem == null)
                KeyedInstructionScreen.ResponseKeyUri = null;
            else 
                KeyedInstructionScreen.ResponseKeyUri = (ResponseKeyDrop.SelectedItem as CIATKey).URI;
        }

        private void ManageKeysButton_Click(object sender, EventArgs e)
        {
            ParentControl.MainForm.ShowResponseKeyPanel();
            PopulateResponseKeyDrop();
        }

        public new void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
       //     if (KeyedInstructionScreen != null)
         //       KeyedInstructionScreen.Dispose();
            base.Dispose();
        }
    }
}
