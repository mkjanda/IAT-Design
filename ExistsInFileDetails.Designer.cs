namespace IATClient
{
    partial class ExistsInFileDetails
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
            this.ExistsInFileGroup = new System.Windows.Forms.GroupBox();
            this.DelimitationGroup = new System.Windows.Forms.GroupBox();
            this.LinebreakRadio = new System.Windows.Forms.RadioButton();
            this.TabRadio = new System.Windows.Forms.RadioButton();
            this.CommaRadio = new System.Windows.Forms.RadioButton();
            this.CopyOnSaveCheck = new System.Windows.Forms.CheckBox();
            this.AllowEachResponseOnceCheck = new System.Windows.Forms.CheckBox();
            this.FileValuesGroup = new System.Windows.Forms.GroupBox();
            this.ValueView = new System.Windows.Forms.DataGridView();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.FilenameTextBox = new System.Windows.Forms.TextBox();
            this.FileLabel = new System.Windows.Forms.Label();
            this.ExistsInFileGroup.SuspendLayout();
            this.DelimitationGroup.SuspendLayout();
            this.FileValuesGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ValueView)).BeginInit();
            this.SuspendLayout();
            // 
            // ExistsInFileGroup
            // 
            this.ExistsInFileGroup.Controls.Add(this.DelimitationGroup);
            this.ExistsInFileGroup.Controls.Add(this.CopyOnSaveCheck);
            this.ExistsInFileGroup.Controls.Add(this.AllowEachResponseOnceCheck);
            this.ExistsInFileGroup.Controls.Add(this.FileValuesGroup);
            this.ExistsInFileGroup.Controls.Add(this.BrowseButton);
            this.ExistsInFileGroup.Controls.Add(this.FilenameTextBox);
            this.ExistsInFileGroup.Controls.Add(this.FileLabel);
            this.ExistsInFileGroup.Location = new System.Drawing.Point(3, 3);
            this.ExistsInFileGroup.Name = "ExistsInFileGroup";
            this.ExistsInFileGroup.Size = new System.Drawing.Size(220, 263);
            this.ExistsInFileGroup.TabIndex = 0;
            this.ExistsInFileGroup.TabStop = false;
            this.ExistsInFileGroup.Text = "Attach File with Valid Responses";
            // 
            // DelimitationGroup
            // 
            this.DelimitationGroup.Controls.Add(this.LinebreakRadio);
            this.DelimitationGroup.Controls.Add(this.TabRadio);
            this.DelimitationGroup.Controls.Add(this.CommaRadio);
            this.DelimitationGroup.Location = new System.Drawing.Point(6, 18);
            this.DelimitationGroup.Name = "DelimitationGroup";
            this.DelimitationGroup.Size = new System.Drawing.Size(208, 44);
            this.DelimitationGroup.TabIndex = 6;
            this.DelimitationGroup.TabStop = false;
            this.DelimitationGroup.Text = "Delimitation";
            // 
            // LinebreakRadio
            // 
            this.LinebreakRadio.AutoSize = true;
            this.LinebreakRadio.Location = new System.Drawing.Point(122, 19);
            this.LinebreakRadio.Name = "LinebreakRadio";
            this.LinebreakRadio.Size = new System.Drawing.Size(72, 17);
            this.LinebreakRadio.TabIndex = 2;
            this.LinebreakRadio.TabStop = true;
            this.LinebreakRadio.Text = "Linebreak";
            this.LinebreakRadio.UseVisualStyleBackColor = true;
            this.LinebreakRadio.CheckedChanged += new System.EventHandler(this.LinebreakRadio_CheckedChanged);
            // 
            // TabRadio
            // 
            this.TabRadio.AutoSize = true;
            this.TabRadio.Location = new System.Drawing.Point(72, 19);
            this.TabRadio.Name = "TabRadio";
            this.TabRadio.Size = new System.Drawing.Size(44, 17);
            this.TabRadio.TabIndex = 1;
            this.TabRadio.TabStop = true;
            this.TabRadio.Text = "Tab";
            this.TabRadio.UseVisualStyleBackColor = true;
            this.TabRadio.CheckedChanged += new System.EventHandler(this.TabRadio_CheckedChanged);
            // 
            // CommaRadio
            // 
            this.CommaRadio.AutoSize = true;
            this.CommaRadio.Location = new System.Drawing.Point(6, 19);
            this.CommaRadio.Name = "CommaRadio";
            this.CommaRadio.Size = new System.Drawing.Size(60, 17);
            this.CommaRadio.TabIndex = 0;
            this.CommaRadio.TabStop = true;
            this.CommaRadio.Text = "Comma";
            this.CommaRadio.UseVisualStyleBackColor = true;
            this.CommaRadio.CheckedChanged += new System.EventHandler(this.CommaRadio_CheckedChanged);
            // 
            // CopyOnSaveCheck
            // 
            this.CopyOnSaveCheck.AutoSize = true;
            this.CopyOnSaveCheck.Location = new System.Drawing.Point(6, 240);
            this.CopyOnSaveCheck.Name = "CopyOnSaveCheck";
            this.CopyOnSaveCheck.Size = new System.Drawing.Size(195, 17);
            this.CopyOnSaveCheck.TabIndex = 5;
            this.CopyOnSaveCheck.Text = "Copy file to output directory on save";
            this.CopyOnSaveCheck.UseVisualStyleBackColor = true;
            this.CopyOnSaveCheck.CheckedChanged += new System.EventHandler(this.CopyOnSaveCheck_CheckedChanged);
            // 
            // AllowEachResponseOnceCheck
            // 
            this.AllowEachResponseOnceCheck.AutoSize = true;
            this.AllowEachResponseOnceCheck.Location = new System.Drawing.Point(6, 217);
            this.AllowEachResponseOnceCheck.Name = "AllowEachResponseOnceCheck";
            this.AllowEachResponseOnceCheck.Size = new System.Drawing.Size(200, 17);
            this.AllowEachResponseOnceCheck.TabIndex = 4;
            this.AllowEachResponseOnceCheck.Text = "Allow each response in file only once";
            this.AllowEachResponseOnceCheck.UseVisualStyleBackColor = true;
            this.AllowEachResponseOnceCheck.CheckedChanged += new System.EventHandler(this.AllowEachResponseOnceCheck_CheckedChanged);
            // 
            // FileValuesGroup
            // 
            this.FileValuesGroup.Controls.Add(this.ValueView);
            this.FileValuesGroup.Location = new System.Drawing.Point(6, 95);
            this.FileValuesGroup.Name = "FileValuesGroup";
            this.FileValuesGroup.Size = new System.Drawing.Size(208, 116);
            this.FileValuesGroup.TabIndex = 3;
            this.FileValuesGroup.TabStop = false;
            this.FileValuesGroup.Text = "Values in File";
            // 
            // ValueView
            // 
            this.ValueView.AllowUserToAddRows = false;
            this.ValueView.AllowUserToDeleteRows = false;
            this.ValueView.AllowUserToResizeColumns = false;
            this.ValueView.AllowUserToResizeRows = false;
            this.ValueView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ValueView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ValueView.Location = new System.Drawing.Point(3, 16);
            this.ValueView.Name = "ValueView";
            this.ValueView.RowHeadersVisible = false;
            this.ValueView.Size = new System.Drawing.Size(202, 97);
            this.ValueView.TabIndex = 0;
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(144, 66);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(70, 23);
            this.BrowseButton.TabIndex = 2;
            this.BrowseButton.Text = "Browse...";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // FilenameTextBox
            // 
            this.FilenameTextBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.FilenameTextBox.Location = new System.Drawing.Point(38, 68);
            this.FilenameTextBox.Name = "FilenameTextBox";
            this.FilenameTextBox.ReadOnly = true;
            this.FilenameTextBox.Size = new System.Drawing.Size(100, 20);
            this.FilenameTextBox.TabIndex = 1;
            // 
            // FileLabel
            // 
            this.FileLabel.AutoSize = true;
            this.FileLabel.Location = new System.Drawing.Point(6, 71);
            this.FileLabel.Name = "FileLabel";
            this.FileLabel.Size = new System.Drawing.Size(26, 13);
            this.FileLabel.TabIndex = 0;
            this.FileLabel.Text = "File:";
            // 
            // ExistsInFileDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ExistsInFileGroup);
            this.Name = "ExistsInFileDetails";
            this.Size = new System.Drawing.Size(226, 269);
            this.ParentChanged += new System.EventHandler(this.ExistsInFileDetails_ParentChanged);
            this.ExistsInFileGroup.ResumeLayout(false);
            this.ExistsInFileGroup.PerformLayout();
            this.DelimitationGroup.ResumeLayout(false);
            this.DelimitationGroup.PerformLayout();
            this.FileValuesGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ValueView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox ExistsInFileGroup;
        private System.Windows.Forms.TextBox FilenameTextBox;
        private System.Windows.Forms.Label FileLabel;
        private System.Windows.Forms.CheckBox CopyOnSaveCheck;
        private System.Windows.Forms.CheckBox AllowEachResponseOnceCheck;
        private System.Windows.Forms.GroupBox FileValuesGroup;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.DataGridView ValueView;
        private System.Windows.Forms.GroupBox DelimitationGroup;
        private System.Windows.Forms.RadioButton CommaRadio;
        private System.Windows.Forms.RadioButton LinebreakRadio;
        private System.Windows.Forms.RadioButton TabRadio;
    }
}
