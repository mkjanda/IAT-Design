namespace IATClient
{
    partial class IATGenerateBlockSelect
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.FirstBlockLabel = new System.Windows.Forms.Label();
            this.FirstBlockCombo = new System.Windows.Forms.ComboBox();
            this.SecondBlockLabel = new System.Windows.Forms.Label();
            this.SecondBlockCombo = new System.Windows.Forms.ComboBox();
            this.SelectBlocksInstructionsLabel = new System.Windows.Forms.TextBox();
            this.OK = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.EnableAlternationCheck = new System.Windows.Forms.CheckBox();
            this.RandomizeGeneratedBlocksCheck = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // FirstBlockLabel
            // 
            this.FirstBlockLabel.AutoSize = true;
            this.FirstBlockLabel.Location = new System.Drawing.Point(30, 50);
            this.FirstBlockLabel.Name = "FirstBlockLabel";
            this.FirstBlockLabel.Size = new System.Drawing.Size(112, 13);
            this.FirstBlockLabel.TabIndex = 0;
            this.FirstBlockLabel.Text = "Select First IAT Block:";
            // 
            // FirstBlockCombo
            // 
            this.FirstBlockCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.FirstBlockCombo.FormattingEnabled = true;
            this.FirstBlockCombo.Location = new System.Drawing.Point(148, 47);
            this.FirstBlockCombo.Name = "FirstBlockCombo";
            this.FirstBlockCombo.Size = new System.Drawing.Size(206, 21);
            this.FirstBlockCombo.TabIndex = 1;
            this.FirstBlockCombo.SelectedIndexChanged += new System.EventHandler(this.FirstBlock_SelectedIndexChanged);
            // 
            // SecondBlockLabel
            // 
            this.SecondBlockLabel.AutoSize = true;
            this.SecondBlockLabel.Location = new System.Drawing.Point(12, 77);
            this.SecondBlockLabel.Name = "SecondBlockLabel";
            this.SecondBlockLabel.Size = new System.Drawing.Size(130, 13);
            this.SecondBlockLabel.TabIndex = 2;
            this.SecondBlockLabel.Text = "Select Second IAT Block:";
            // 
            // SecondBlockCombo
            // 
            this.SecondBlockCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SecondBlockCombo.FormattingEnabled = true;
            this.SecondBlockCombo.Location = new System.Drawing.Point(148, 74);
            this.SecondBlockCombo.Name = "SecondBlockCombo";
            this.SecondBlockCombo.Size = new System.Drawing.Size(206, 21);
            this.SecondBlockCombo.TabIndex = 3;
            this.SecondBlockCombo.SelectedIndexChanged += new System.EventHandler(this.SecondBlockCombo_SelectedIndexChanged);
            // 
            // SelectBlocksInstructionsLabel
            // 
            this.SelectBlocksInstructionsLabel.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.SelectBlocksInstructionsLabel.Location = new System.Drawing.Point(12, 12);
            this.SelectBlocksInstructionsLabel.Multiline = true;
            this.SelectBlocksInstructionsLabel.Name = "SelectBlocksInstructionsLabel";
            this.SelectBlocksInstructionsLabel.ReadOnly = true;
            this.SelectBlocksInstructionsLabel.Size = new System.Drawing.Size(342, 29);
            this.SelectBlocksInstructionsLabel.TabIndex = 4;
            this.SelectBlocksInstructionsLabel.Text = "Select the two IAT blocks you wish to use as the basis for the 7-block IAT from t" +
                "he drop lists below and click OK.";
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(77, 147);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 5;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(215, 147);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 6;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // EnableAlternationCheck
            // 
            this.EnableAlternationCheck.AutoSize = true;
            this.EnableAlternationCheck.Location = new System.Drawing.Point(50, 124);
            this.EnableAlternationCheck.Name = "EnableAlternationCheck";
            this.EnableAlternationCheck.Size = new System.Drawing.Size(267, 17);
            this.EnableAlternationCheck.TabIndex = 7;
            this.EnableAlternationCheck.Text = "Enable alternation of blocks 3 && 4 with blocks 6 && 7";
            this.EnableAlternationCheck.UseVisualStyleBackColor = true;
            this.EnableAlternationCheck.CheckedChanged += new System.EventHandler(this.EnableAlternationCheck_CheckedChanged);
            // 
            // RandomizeGeneratedBlocksCheck
            // 
            this.RandomizeGeneratedBlocksCheck.AutoSize = true;
            this.RandomizeGeneratedBlocksCheck.Location = new System.Drawing.Point(50, 101);
            this.RandomizeGeneratedBlocksCheck.Name = "RandomizeGeneratedBlocksCheck";
            this.RandomizeGeneratedBlocksCheck.Size = new System.Drawing.Size(202, 17);
            this.RandomizeGeneratedBlocksCheck.TabIndex = 8;
            this.RandomizeGeneratedBlocksCheck.Text = "Randomize items in generated blocks";
            this.RandomizeGeneratedBlocksCheck.UseVisualStyleBackColor = true;
            this.RandomizeGeneratedBlocksCheck.CheckedChanged += new System.EventHandler(this.RandomizeGeneratedBlocksCheck_CheckedChanged);
            // 
            // IATGenerateBlockSelect
            // 
            this.AcceptButton = this.OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(366, 178);
            this.Controls.Add(this.RandomizeGeneratedBlocksCheck);
            this.Controls.Add(this.EnableAlternationCheck);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.SelectBlocksInstructionsLabel);
            this.Controls.Add(this.SecondBlockCombo);
            this.Controls.Add(this.SecondBlockLabel);
            this.Controls.Add(this.FirstBlockCombo);
            this.Controls.Add(this.FirstBlockLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IATGenerateBlockSelect";
            this.ShowInTaskbar = false;
            this.Text = "Select Blocks for IAT Generation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label FirstBlockLabel;
        private System.Windows.Forms.ComboBox FirstBlockCombo;
        private System.Windows.Forms.Label SecondBlockLabel;
        private System.Windows.Forms.ComboBox SecondBlockCombo;
        private System.Windows.Forms.TextBox SelectBlocksInstructionsLabel;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.CheckBox EnableAlternationCheck;
        private System.Windows.Forms.CheckBox RandomizeGeneratedBlocksCheck;
    }
}