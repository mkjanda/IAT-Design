namespace IATClient
{
    partial class ImageStimulusPanel
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
            this.FileLabel = new System.Windows.Forms.Label();
            this.FileName = new System.Windows.Forms.TextBox();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.StretchToFitCheck = new System.Windows.Forms.CheckBox();
            this.CopyToOutputDirectoryCheck = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // FileLabel
            // 
            this.FileLabel.AutoSize = true;
            this.FileLabel.Location = new System.Drawing.Point(-3, 4);
            this.FileLabel.Name = "FileLabel";
            this.FileLabel.Size = new System.Drawing.Size(58, 13);
            this.FileLabel.TabIndex = 0;
            this.FileLabel.Text = "Image File:";
            // 
            // FileName
            // 
            this.FileName.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.FileName.Location = new System.Drawing.Point(61, 1);
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            this.FileName.Size = new System.Drawing.Size(220, 20);
            this.FileName.TabIndex = 1;
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(287, -1);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BrowseButton.TabIndex = 2;
            this.BrowseButton.Text = "Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // StretchToFitCheck
            // 
            this.StretchToFitCheck.AutoSize = true;
            this.StretchToFitCheck.Location = new System.Drawing.Point(0, 27);
            this.StretchToFitCheck.Name = "StretchToFitCheck";
            this.StretchToFitCheck.Size = new System.Drawing.Size(170, 17);
            this.StretchToFitCheck.TabIndex = 4;
            this.StretchToFitCheck.Text = "Stretch to fit stimulus rectangle";
            this.StretchToFitCheck.UseVisualStyleBackColor = true;
            this.StretchToFitCheck.CheckedChanged += new System.EventHandler(this.StretchToFitCheck_CheckedChanged);
            // 
            // CopyToOutputDirectoryCheck
            // 
            this.CopyToOutputDirectoryCheck.AutoSize = true;
            this.CopyToOutputDirectoryCheck.Location = new System.Drawing.Point(186, 27);
            this.CopyToOutputDirectoryCheck.Name = "CopyToOutputDirectoryCheck";
            this.CopyToOutputDirectoryCheck.Size = new System.Drawing.Size(181, 17);
            this.CopyToOutputDirectoryCheck.TabIndex = 5;
            this.CopyToOutputDirectoryCheck.Text = "Copy to output directory on Save";
            this.CopyToOutputDirectoryCheck.UseVisualStyleBackColor = true;
            this.CopyToOutputDirectoryCheck.CheckedChanged += new System.EventHandler(this.CopyToOutputDirectoryCheck_CheckedChanged);
            // 
            // ImageStimulusPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.CopyToOutputDirectoryCheck);
            this.Controls.Add(this.StretchToFitCheck);
            this.Controls.Add(this.BrowseButton);
            this.Controls.Add(this.FileName);
            this.Controls.Add(this.FileLabel);
            this.Name = "ImageStimulusPanel";
            this.Size = new System.Drawing.Size(365, 45);
            this.Load += new System.EventHandler(this.ImageStimulusPanel_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label FileLabel;
        private System.Windows.Forms.TextBox FileName;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.CheckBox StretchToFitCheck;
        private System.Windows.Forms.CheckBox CopyToOutputDirectoryCheck;

    }
}
