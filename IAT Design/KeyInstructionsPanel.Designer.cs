namespace IATClient
{
    partial class KeyInstructionsPanel
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
            this.KeyInstructionsBox = new System.Windows.Forms.GroupBox();
            this.JustificationDrop = new System.Windows.Forms.ComboBox();
            this.JustificationLabel = new System.Windows.Forms.Label();
            this.LineSpacingLabel = new System.Windows.Forms.Label();
            this.LineSpacingDrop = new System.Windows.Forms.ComboBox();
            this.ManageKeysButton = new System.Windows.Forms.Button();
            this.ResponseKeyDrop = new System.Windows.Forms.ComboBox();
            this.ResponseKeyLabel = new System.Windows.Forms.Label();
            this.KeyInstructionsBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // KeyInstructionsBox
            // 
            this.KeyInstructionsBox.Controls.Add(this.JustificationDrop);
            this.KeyInstructionsBox.Controls.Add(this.JustificationLabel);
            this.KeyInstructionsBox.Controls.Add(this.LineSpacingLabel);
            this.KeyInstructionsBox.Controls.Add(this.LineSpacingDrop);
            this.KeyInstructionsBox.Controls.Add(this.ManageKeysButton);
            this.KeyInstructionsBox.Controls.Add(this.ResponseKeyDrop);
            this.KeyInstructionsBox.Controls.Add(this.ResponseKeyLabel);
            this.KeyInstructionsBox.Location = new System.Drawing.Point(3, 3);
            this.KeyInstructionsBox.Name = "KeyInstructionsBox";
            this.KeyInstructionsBox.Size = new System.Drawing.Size(365, 334);
            this.KeyInstructionsBox.TabIndex = 0;
            this.KeyInstructionsBox.TabStop = false;
            this.KeyInstructionsBox.Text = "Text Instructions with Response Key";
            // 
            // JustificationDrop
            // 
            this.JustificationDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.JustificationDrop.FormattingEnabled = true;
            this.JustificationDrop.Location = new System.Drawing.Point(273, 307);
            this.JustificationDrop.Name = "JustificationDrop";
            this.JustificationDrop.Size = new System.Drawing.Size(63, 21);
            this.JustificationDrop.TabIndex = 6;
            // 
            // JustificationLabel
            // 
            this.JustificationLabel.AutoSize = true;
            this.JustificationLabel.Location = new System.Drawing.Point(178, 310);
            this.JustificationLabel.Name = "JustificationLabel";
            this.JustificationLabel.Size = new System.Drawing.Size(89, 13);
            this.JustificationLabel.TabIndex = 5;
            this.JustificationLabel.Text = "Text Justification:";
            // 
            // LineSpacingLabel
            // 
            this.LineSpacingLabel.AutoSize = true;
            this.LineSpacingLabel.Location = new System.Drawing.Point(31, 310);
            this.LineSpacingLabel.Name = "LineSpacingLabel";
            this.LineSpacingLabel.Size = new System.Drawing.Size(72, 13);
            this.LineSpacingLabel.TabIndex = 4;
            this.LineSpacingLabel.Text = "Line Spacing:";
            // 
            // LineSpacingDrop
            // 
            this.LineSpacingDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LineSpacingDrop.FormattingEnabled = true;
            this.LineSpacingDrop.Location = new System.Drawing.Point(109, 307);
            this.LineSpacingDrop.Name = "LineSpacingDrop";
            this.LineSpacingDrop.Size = new System.Drawing.Size(63, 21);
            this.LineSpacingDrop.TabIndex = 3;
            // 
            // ManageKeysButton
            // 
            this.ManageKeysButton.Location = new System.Drawing.Point(233, 17);
            this.ManageKeysButton.Name = "ManageKeysButton";
            this.ManageKeysButton.Size = new System.Drawing.Size(126, 23);
            this.ManageKeysButton.TabIndex = 2;
            this.ManageKeysButton.Text = "Create / Manage Keys";
            this.ManageKeysButton.UseVisualStyleBackColor = true;
            this.ManageKeysButton.Click += new System.EventHandler(this.ManageKeysButton_Click);
            // 
            // ResponseKeyDrop
            // 
            this.ResponseKeyDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ResponseKeyDrop.FormattingEnabled = true;
            this.ResponseKeyDrop.Location = new System.Drawing.Point(91, 19);
            this.ResponseKeyDrop.Name = "ResponseKeyDrop";
            this.ResponseKeyDrop.Size = new System.Drawing.Size(113, 21);
            this.ResponseKeyDrop.TabIndex = 1;
            // 
            // ResponseKeyLabel
            // 
            this.ResponseKeyLabel.AutoSize = true;
            this.ResponseKeyLabel.Location = new System.Drawing.Point(6, 22);
            this.ResponseKeyLabel.Name = "ResponseKeyLabel";
            this.ResponseKeyLabel.Size = new System.Drawing.Size(79, 13);
            this.ResponseKeyLabel.TabIndex = 0;
            this.ResponseKeyLabel.Text = "Response Key:";
            // 
            // KeyInstructionsPanel
            // 
            this.Controls.Add(this.KeyInstructionsBox);
            this.Name = "KeyInstructionsPanel";
            this.KeyInstructionsBox.ResumeLayout(false);
            this.KeyInstructionsBox.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion



        private System.Windows.Forms.GroupBox KeyInstructionsBox;
        private System.Windows.Forms.ComboBox ResponseKeyDrop;
        private System.Windows.Forms.Label ResponseKeyLabel;
        private System.Windows.Forms.Button ManageKeysButton;
        private System.Windows.Forms.ComboBox LineSpacingDrop;
        private System.Windows.Forms.Label LineSpacingLabel;
        private System.Windows.Forms.Label JustificationLabel;
        private System.Windows.Forms.ComboBox JustificationDrop;
    }
}
