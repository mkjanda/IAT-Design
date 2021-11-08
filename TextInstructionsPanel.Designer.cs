namespace IATClient
{
    partial class TextInstructionsPanel
    {
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.TextGroupBox = new System.Windows.Forms.GroupBox();
            this.JustificationDrop = new System.Windows.Forms.ComboBox();
            this.Justificationlabel = new System.Windows.Forms.Label();
            this.LineSpacingDrop = new System.Windows.Forms.ComboBox();
            this.LineSpacingLabel = new System.Windows.Forms.Label();
            this.TextGroupBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // TextGroupBox
            // 
            this.TextGroupBox.Controls.Add(this.JustificationDrop);
            this.TextGroupBox.Controls.Add(this.Justificationlabel);
            this.TextGroupBox.Controls.Add(this.LineSpacingDrop);
            this.TextGroupBox.Controls.Add(this.LineSpacingLabel);
            this.TextGroupBox.Location = new System.Drawing.Point(3, 3);
            this.TextGroupBox.Name = "TextGroupBox";
            this.TextGroupBox.Size = new System.Drawing.Size(366, 300);
            this.TextGroupBox.TabIndex = 0;
            this.TextGroupBox.TabStop = false;
            this.TextGroupBox.Text = "Text Instructions";
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
            // 
            // Justificationlabel
            // 
            this.Justificationlabel.AutoSize = true;
            this.Justificationlabel.Location = new System.Drawing.Point(178, 276);
            this.Justificationlabel.Name = "Justificationlabel";
            this.Justificationlabel.Size = new System.Drawing.Size(89, 13);
            this.Justificationlabel.TabIndex = 2;
            this.Justificationlabel.Text = "Text Justification:";
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
            // 
            // LineSpacingLabel
            // 
            this.LineSpacingLabel.AutoSize = true;
            this.LineSpacingLabel.Location = new System.Drawing.Point(31, 276);
            this.LineSpacingLabel.Name = "LineSpacingLabel";
            this.LineSpacingLabel.Size = new System.Drawing.Size(72, 13);
            this.LineSpacingLabel.TabIndex = 0;
            this.LineSpacingLabel.Text = "Line Spacing:";
            // 
            // TextInstructionsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TextGroupBox);
            this.Name = "TextInstructionsPanel";
            this.Size = new System.Drawing.Size(371, 306);
            this.TextGroupBox.ResumeLayout(false);
            this.TextGroupBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox TextGroupBox;
        private System.Windows.Forms.Label LineSpacingLabel;
        private System.Windows.Forms.Label Justificationlabel;
        private System.Windows.Forms.ComboBox LineSpacingDrop;
        private System.Windows.Forms.ComboBox JustificationDrop;
    }
}
