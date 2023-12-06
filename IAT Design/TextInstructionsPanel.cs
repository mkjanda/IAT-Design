using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    partial class TextInstructionsPanel : UserControl
    {
        // a flag to indicate the object is updating internally
        private bool IsUpdating;
        private System.Windows.Forms.Label LineSpacingLabel;
        private System.Windows.Forms.Label Justificationlabel;
        private System.Windows.Forms.ComboBox LineSpacingDrop;
        private System.Windows.Forms.ComboBox JustificationDrop;
        private GroupBox TextInstructionsBox = new GroupBox();
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
                    JustificationDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.TextInstructionsScreen].Justification.ToString();
                    LineSpacingDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.TextInstructionsScreen].LineSpacing.ToString();
                    TextInstructions.TextDisplayItemUri = null;
                    _TextInstructionScreen = value;
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

        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }


        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.JustificationDrop = new System.Windows.Forms.ComboBox();
            this.Justificationlabel = new System.Windows.Forms.Label();
            this.LineSpacingDrop = new System.Windows.Forms.ComboBox();
            this.LineSpacingLabel = new System.Windows.Forms.Label();
            TextInstructionsBox.Size = new Size(365, 334);
            TextInstructionsBox.Controls.Add(JustificationDrop);
            TextInstructionsBox.Controls.Add(Justificationlabel);
            TextInstructionsBox.Controls.Add(LineSpacingDrop);
            TextInstructionsBox.Controls.Add(LineSpacingLabel);
            TextInstructionsBox.TabStop = false;

            this.SuspendLayout();
            // 
            // JustificationDrop
            // 
            this.JustificationDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.JustificationDrop.FormattingEnabled = true;
            this.JustificationDrop.Location = new System.Drawing.Point(273, 273);
            this.JustificationDrop.Name = "JustificationDrop";
            this.JustificationDrop.Size = new System.Drawing.Size(63, 21);
            this.JustificationDrop.TabIndex = 3;
            this.JustificationDrop.SelectedIndexChanged += new System.EventHandler(this.JustificationDrop_SelectedIndexChanged);
            this.Controls.Add(JustificationDrop);
            // 
            // Justificationlabel
            // 
            this.Justificationlabel.AutoSize = true;
            this.Justificationlabel.Location = new System.Drawing.Point(178, 276);
            this.Justificationlabel.Name = "Justificationlabel";
            this.Justificationlabel.Size = new System.Drawing.Size(89, 13);
            this.Justificationlabel.TabIndex = 2;
            this.Justificationlabel.Text = "Text Justification:";
            this.Controls.Add(Justificationlabel);
            // 
            // LineSpacingDrop
            // 
            this.LineSpacingDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LineSpacingDrop.FormattingEnabled = true;
            this.LineSpacingDrop.Location = new System.Drawing.Point(109, 273);
            this.LineSpacingDrop.Name = "LineSpacingDrop";
            this.LineSpacingDrop.Size = new System.Drawing.Size(63, 21);
            this.LineSpacingDrop.TabIndex = 1;
            this.LineSpacingDrop.SelectedIndexChanged += new System.EventHandler(this.LineSpacingDrop_SelectedIndexChanged);
            Controls.Add(this.LineSpacingDrop);
            // 
            // LineSpacingLabel
            // 
            this.LineSpacingLabel.AutoSize = true;
            this.LineSpacingLabel.Location = new System.Drawing.Point(31, 276);
            this.LineSpacingLabel.Name = "LineSpacingLabel";
            this.LineSpacingLabel.Size = new System.Drawing.Size(20, 13);
            this.LineSpacingLabel.TabIndex = 0;
            this.LineSpacingLabel.Text = "Line Spacing:";
            Controls.Add(LineSpacingLabel);
            // 
            // TextInstructionsPanel
            // 
            this.ResumeLayout(false);
        }



        public TextInstructionsPanel(Size sz, CInstructionBlock b)
        {
            InstructionsBlock = b;
            InitializeComponent();
            IsUpdating = true;
            this.Width = sz.Width;
            TextInstructions = new TextEditControl(TextInstructionsBox.Height - (LineSpacingDrop.Height >> 1), TextInstructionsBox.Width,
                DIText.UsedAs.TextInstructionsScreen, false);
            TextInstructions.Location = TextInstructionsControlLocation;
            TextInstructions.Size = TextInstructions.CalculatedSize;
            Controls.Add(TextInstructions);

            for (int ctr = 0; ctr < AvailableLineSpacing.Length; ctr++)
                LineSpacingDrop.Items.Add(AvailableLineSpacing[ctr].ToString());
            LineSpacingDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.TextInstructionsScreen].LineSpacing;
            JustificationDrop.Items.Add("Left");
            JustificationDrop.Items.Add("Center");
            JustificationDrop.Items.Add("Right");
            JustificationDrop.SelectedItem = CIAT.SaveFile.FontPreferences[DIText.UsedAs.TextInstructionsScreen].Justification.ToString();
            LineSpacingLabel.Location += new Size(35, 50);
            LineSpacingDrop.Location += new Size(15, 50);
            Justificationlabel.Location += new Size(0, 50);
            JustificationDrop.Location += new Size(-30, 50);
            JustificationDrop.Width += 20;
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
            //            if (TextInstructionScreen != null)
            //              TextInstructionScreen.Dispose();
            base.Dispose();

        }
    }
}
