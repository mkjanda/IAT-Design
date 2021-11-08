namespace IATClient
{
    partial class CombinedResponseKeyPanel
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
            this.FirstCombinedKeyLabel = new System.Windows.Forms.Label();
            this.FirstCombinedKey = new System.Windows.Forms.ComboBox();
            this.SecondCombinedKeyLabel = new System.Windows.Forms.Label();
            this.SecondCombinedKey = new System.Windows.Forms.ComboBox();
            this.ConjunctionGroup = new System.Windows.Forms.GroupBox();
            this.PaddingLabel = new System.Windows.Forms.Label();
            this.PixelsLabel = new System.Windows.Forms.Label();
            this.PaddingEdit = new System.Windows.Forms.TextBox();
            this.CombinedKeyGroup = new System.Windows.Forms.GroupBox();
            this.ConjunctionGroup.SuspendLayout();
            this.CombinedKeyGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // FirstCombinedKeyLabel
            // 
            this.FirstCombinedKeyLabel.AutoSize = true;
            this.FirstCombinedKeyLabel.Location = new System.Drawing.Point(18, 22);
            this.FirstCombinedKeyLabel.Name = "FirstCombinedKeyLabel";
            this.FirstCombinedKeyLabel.Size = new System.Drawing.Size(51, 13);
            this.FirstCombinedKeyLabel.TabIndex = 0;
            this.FirstCombinedKeyLabel.Text = "Combine:";
            // 
            // FirstCombinedKey
            // 
            this.FirstCombinedKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FirstCombinedKey.FormattingEnabled = true;
            this.FirstCombinedKey.Location = new System.Drawing.Point(75, 19);
            this.FirstCombinedKey.Name = "FirstCombinedKey";
            this.FirstCombinedKey.Size = new System.Drawing.Size(151, 21);
            this.FirstCombinedKey.TabIndex = 1;
            this.FirstCombinedKey.SelectedIndexChanged += new System.EventHandler(this.FirstCombinedKey_SelectedIndexChanged);
            // 
            // SecondCombinedKeyLabel
            // 
            this.SecondCombinedKeyLabel.AutoSize = true;
            this.SecondCombinedKeyLabel.Location = new System.Drawing.Point(18, 49);
            this.SecondCombinedKeyLabel.Name = "SecondCombinedKeyLabel";
            this.SecondCombinedKeyLabel.Size = new System.Drawing.Size(32, 13);
            this.SecondCombinedKeyLabel.TabIndex = 2;
            this.SecondCombinedKeyLabel.Text = "With:";
            // 
            // SecondCombinedKey
            // 
            this.SecondCombinedKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SecondCombinedKey.FormattingEnabled = true;
            this.SecondCombinedKey.Location = new System.Drawing.Point(56, 46);
            this.SecondCombinedKey.Name = "SecondCombinedKey";
            this.SecondCombinedKey.Size = new System.Drawing.Size(151, 21);
            this.SecondCombinedKey.TabIndex = 3;
            this.SecondCombinedKey.SelectedIndexChanged += new System.EventHandler(this.SecondCombinedKey_SelectedIndexChanged);
            // 
            // ConjunctionGroup
            // 
            this.ConjunctionGroup.Location = new System.Drawing.Point(17, 73);
            this.ConjunctionGroup.Name = "ConjunctionGroup";
            this.ConjunctionGroup.Size = new System.Drawing.Size(381, 67);
            this.ConjunctionGroup.TabIndex = 4;
            this.ConjunctionGroup.TabStop = false;
            this.ConjunctionGroup.Text = "Conjunction";
            // 
            // PaddingLabel
            // 
            this.PaddingLabel.AutoSize = true;
            this.PaddingLabel.Location = new System.Drawing.Point(243, 30);
            this.PaddingLabel.Name = "PaddingLabel";
            this.PaddingLabel.Size = new System.Drawing.Size(152, 13);
            this.PaddingLabel.TabIndex = 6;
            this.PaddingLabel.Text = "Vertical space between stimuli:";
            // 
            // PixelsLabel
            // 
            this.PixelsLabel.AutoSize = true;
            this.PixelsLabel.Location = new System.Drawing.Point(329, 49);
            this.PixelsLabel.Name = "PixelsLabel";
            this.PixelsLabel.Size = new System.Drawing.Size(33, 13);
            this.PixelsLabel.TabIndex = 7;
            this.PixelsLabel.Text = "pixels";
            // 
            // PaddingEdit
            // 
            this.PaddingEdit.Location = new System.Drawing.Point(274, 46);
            this.PaddingEdit.Name = "PaddingEdit";
            this.PaddingEdit.Size = new System.Drawing.Size(49, 20);
            this.PaddingEdit.TabIndex = 8;
            // 
            // CombinedKeyGroup
            // 
//            this.CombinedKeyGroup.Controls.Add(this.PaddingEdit);
//            this.CombinedKeyGroup.Controls.Add(this.PixelsLabel);
//            this.CombinedKeyGroup.Controls.Add(this.PaddingLabel);
            this.CombinedKeyGroup.Controls.Add(this.ConjunctionGroup);
            this.CombinedKeyGroup.Controls.Add(this.SecondCombinedKey);
            this.CombinedKeyGroup.Controls.Add(this.SecondCombinedKeyLabel);
            this.CombinedKeyGroup.Controls.Add(this.FirstCombinedKey);
            this.CombinedKeyGroup.Controls.Add(this.FirstCombinedKeyLabel);
            this.CombinedKeyGroup.Location = new System.Drawing.Point(3, 3);
            this.CombinedKeyGroup.Name = "CombinedKeyGroup";
            this.CombinedKeyGroup.Size = new System.Drawing.Size(414, 147);
            this.CombinedKeyGroup.TabIndex = 9;
            this.CombinedKeyGroup.TabStop = false;
            this.CombinedKeyGroup.Text = "Combined Response Key";
            // 
            // CombinedResponseKeyPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CombinedKeyGroup);
            this.Name = "CombinedResponseKeyPanel";
            this.Size = new System.Drawing.Size(420, 158);
            this.ConjunctionGroup.ResumeLayout(false);
            this.CombinedKeyGroup.ResumeLayout(false);
            this.CombinedKeyGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label FirstCombinedKeyLabel;
        private System.Windows.Forms.ComboBox FirstCombinedKey;
        private System.Windows.Forms.Label SecondCombinedKeyLabel;
        private System.Windows.Forms.ComboBox SecondCombinedKey;
        private System.Windows.Forms.GroupBox ConjunctionGroup;
        private System.Windows.Forms.Label PaddingLabel;
        private System.Windows.Forms.Label PixelsLabel;
        private System.Windows.Forms.TextBox PaddingEdit;
        private System.Windows.Forms.GroupBox CombinedKeyGroup;
    }
}
