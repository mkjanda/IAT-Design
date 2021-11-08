namespace IATClient
{
    partial class AboutBox
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AboutBox));
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.logoPictureBox = new System.Windows.Forms.PictureBox();
            this.labelProductName = new System.Windows.Forms.Label();
            this.labelVersion = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.labelCompanyName = new System.Windows.Forms.Label();
            this.ProductKeyLabel = new System.Windows.Forms.Label();
            this.IATDesignLabel = new System.Windows.Forms.Label();
            this.VersionLabel = new System.Windows.Forms.Label();
            this.CopyrightLabel = new System.Windows.Forms.Label();
            this.CompanyNameLabel = new System.Windows.Forms.Label();
            this.ProductKeyEdit = new System.Windows.Forms.TextBox();
            this.ProgressPanel = new System.Windows.Forms.Panel();
            this.ProductDescription = new System.Windows.Forms.RichTextBox();
            this.okButton = new System.Windows.Forms.Button();
            this.UnregisterButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.ColumnCount = 4;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 120F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.tableLayoutPanel.Controls.Add(this.logoPictureBox, 0, 0);
            this.tableLayoutPanel.Controls.Add(this.labelProductName, 1, 0);
            this.tableLayoutPanel.Controls.Add(this.labelVersion, 1, 1);
            this.tableLayoutPanel.Controls.Add(this.labelCopyright, 1, 2);
            this.tableLayoutPanel.Controls.Add(this.labelCompanyName, 1, 3);
            this.tableLayoutPanel.Controls.Add(this.ProductKeyLabel, 1, 4);
            this.tableLayoutPanel.Controls.Add(this.IATDesignLabel, 2, 0);
            this.tableLayoutPanel.Controls.Add(this.VersionLabel, 2, 1);
            this.tableLayoutPanel.Controls.Add(this.CopyrightLabel, 2, 2);
            this.tableLayoutPanel.Controls.Add(this.CompanyNameLabel, 2, 3);
            this.tableLayoutPanel.Controls.Add(this.ProductKeyEdit, 2, 4);
            this.tableLayoutPanel.Controls.Add(this.ProgressPanel, 1, 5);
            this.tableLayoutPanel.Controls.Add(this.ProductDescription, 1, 6);
            this.tableLayoutPanel.Controls.Add(this.okButton, 2, 7);
            this.tableLayoutPanel.Controls.Add(this.UnregisterButton, 1, 7);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(9, 9);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 8;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 10F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(417, 265);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // logoPictureBox
            // 
            this.logoPictureBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.logoPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("logoPictureBox.Image")));
            this.logoPictureBox.Location = new System.Drawing.Point(3, 3);
            this.logoPictureBox.Name = "logoPictureBox";
            this.tableLayoutPanel.SetRowSpan(this.logoPictureBox, 8);
            this.logoPictureBox.Size = new System.Drawing.Size(114, 259);
            this.logoPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.logoPictureBox.TabIndex = 12;
            this.logoPictureBox.TabStop = false;
            // 
            // labelProductName
            // 
            this.labelProductName.AutoSize = true;
            this.labelProductName.Dock = System.Windows.Forms.DockStyle.Right;
            this.labelProductName.Location = new System.Drawing.Point(133, 0);
            this.labelProductName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelProductName.MaximumSize = new System.Drawing.Size(0, 17);
            this.labelProductName.Name = "labelProductName";
            this.labelProductName.Size = new System.Drawing.Size(75, 17);
            this.labelProductName.TabIndex = 19;
            this.labelProductName.Text = "Product Name";
            this.labelProductName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelVersion
            // 
            this.labelVersion.AutoSize = true;
            this.labelVersion.Dock = System.Windows.Forms.DockStyle.Right;
            this.labelVersion.Location = new System.Drawing.Point(166, 26);
            this.labelVersion.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelVersion.MaximumSize = new System.Drawing.Size(0, 17);
            this.labelVersion.Name = "labelVersion";
            this.labelVersion.Size = new System.Drawing.Size(42, 17);
            this.labelVersion.TabIndex = 0;
            this.labelVersion.Text = "Version";
            this.labelVersion.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCopyright
            // 
            this.labelCopyright.AutoSize = true;
            this.labelCopyright.Dock = System.Windows.Forms.DockStyle.Right;
            this.labelCopyright.Location = new System.Drawing.Point(157, 52);
            this.labelCopyright.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelCopyright.MaximumSize = new System.Drawing.Size(0, 17);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(51, 17);
            this.labelCopyright.TabIndex = 21;
            this.labelCopyright.Text = "Copyright";
            this.labelCopyright.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // labelCompanyName
            // 
            this.labelCompanyName.AutoSize = true;
            this.labelCompanyName.Cursor = System.Windows.Forms.Cursors.Default;
            this.labelCompanyName.Dock = System.Windows.Forms.DockStyle.Right;
            this.labelCompanyName.Location = new System.Drawing.Point(126, 78);
            this.labelCompanyName.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.labelCompanyName.MaximumSize = new System.Drawing.Size(0, 17);
            this.labelCompanyName.Name = "labelCompanyName";
            this.labelCompanyName.Size = new System.Drawing.Size(82, 17);
            this.labelCompanyName.TabIndex = 22;
            this.labelCompanyName.Text = "Company Name";
            this.labelCompanyName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProductKeyLabel
            // 
            this.ProductKeyLabel.AutoSize = true;
            this.ProductKeyLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.ProductKeyLabel.Location = new System.Drawing.Point(143, 104);
            this.ProductKeyLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.ProductKeyLabel.MaximumSize = new System.Drawing.Size(0, 17);
            this.ProductKeyLabel.Name = "ProductKeyLabel";
            this.ProductKeyLabel.Size = new System.Drawing.Size(65, 17);
            this.ProductKeyLabel.TabIndex = 25;
            this.ProductKeyLabel.Text = "Product Key";
            // 
            // IATDesignLabel
            // 
            this.IATDesignLabel.AutoSize = true;
            this.IATDesignLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.IATDesignLabel.Location = new System.Drawing.Point(217, 0);
            this.IATDesignLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.IATDesignLabel.MaximumSize = new System.Drawing.Size(0, 17);
            this.IATDesignLabel.Name = "IATDesignLabel";
            this.IATDesignLabel.Size = new System.Drawing.Size(60, 17);
            this.IATDesignLabel.TabIndex = 26;
            this.IATDesignLabel.Text = "IAT Design";
            this.IATDesignLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // VersionLabel
            // 
            this.VersionLabel.AutoSize = true;
            this.VersionLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.VersionLabel.Location = new System.Drawing.Point(217, 26);
            this.VersionLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.VersionLabel.MaximumSize = new System.Drawing.Size(0, 17);
            this.VersionLabel.Name = "VersionLabel";
            this.VersionLabel.Size = new System.Drawing.Size(35, 17);
            this.VersionLabel.TabIndex = 27;
            this.VersionLabel.Text = "label2";
            this.VersionLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CopyrightLabel
            // 
            this.CopyrightLabel.AutoSize = true;
            this.CopyrightLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.CopyrightLabel.Location = new System.Drawing.Point(217, 52);
            this.CopyrightLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.CopyrightLabel.MaximumSize = new System.Drawing.Size(0, 17);
            this.CopyrightLabel.Name = "CopyrightLabel";
            this.CopyrightLabel.Size = new System.Drawing.Size(46, 17);
            this.CopyrightLabel.TabIndex = 28;
            this.CopyrightLabel.Text = "Pending";
            this.CopyrightLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // CompanyNameLabel
            // 
            this.CompanyNameLabel.AutoSize = true;
            this.CompanyNameLabel.Dock = System.Windows.Forms.DockStyle.Left;
            this.CompanyNameLabel.Location = new System.Drawing.Point(217, 78);
            this.CompanyNameLabel.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.CompanyNameLabel.MaximumSize = new System.Drawing.Size(0, 17);
            this.CompanyNameLabel.Name = "CompanyNameLabel";
            this.CompanyNameLabel.Size = new System.Drawing.Size(69, 17);
            this.CompanyNameLabel.TabIndex = 29;
            this.CompanyNameLabel.Text = "IAT Software";
            this.CompanyNameLabel.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // ProductKeyEdit
            // 
            this.ProductKeyEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ProductKeyEdit.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProductKeyEdit.Location = new System.Drawing.Point(217, 104);
            this.ProductKeyEdit.Margin = new System.Windows.Forms.Padding(6, 0, 3, 0);
            this.ProductKeyEdit.MaximumSize = new System.Drawing.Size(0, 17);
            this.ProductKeyEdit.Name = "ProductKeyEdit";
            this.ProductKeyEdit.ReadOnly = true;
            this.ProductKeyEdit.Size = new System.Drawing.Size(197, 13);
            this.ProductKeyEdit.TabIndex = 30;
            // 
            // ProgressPanel
            // 
            this.ProgressPanel.AutoSize = true;
            this.tableLayoutPanel.SetColumnSpan(this.ProgressPanel, 2);
            this.ProgressPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProgressPanel.Location = new System.Drawing.Point(145, 130);
            this.ProgressPanel.Margin = new System.Windows.Forms.Padding(25, 0, 25, 0);
            this.ProgressPanel.Name = "ProgressPanel";
            this.ProgressPanel.Size = new System.Drawing.Size(247, 26);
            this.ProgressPanel.TabIndex = 31;
            // 
            // ProductDescription
            // 
            this.ProductDescription.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tableLayoutPanel.SetColumnSpan(this.ProductDescription, 2);
            this.ProductDescription.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ProductDescription.Location = new System.Drawing.Point(123, 159);
            this.ProductDescription.Name = "ProductDescription";
            this.ProductDescription.ReadOnly = true;
            this.ProductDescription.Size = new System.Drawing.Size(291, 73);
            this.ProductDescription.TabIndex = 32;
            this.ProductDescription.Text = "";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel.SetColumnSpan(this.okButton, 2);
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.okButton.Location = new System.Drawing.Point(339, 239);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(75, 23);
            this.okButton.TabIndex = 24;
            this.okButton.Text = "&OK";
            // 
            // UnregisterButton
            // 
            this.UnregisterButton.Location = new System.Drawing.Point(123, 238);
            this.UnregisterButton.Name = "UnregisterButton";
            this.UnregisterButton.Size = new System.Drawing.Size(75, 23);
            this.UnregisterButton.TabIndex = 33;
            this.UnregisterButton.Text = "Deactivate";
            this.UnregisterButton.UseVisualStyleBackColor = true;
            this.UnregisterButton.Click += new System.EventHandler(this.UnregisterButton_Click);
            // 
            // AboutBox
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(435, 283);
            this.Controls.Add(this.tableLayoutPanel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AboutBox";
            this.Padding = new System.Windows.Forms.Padding(9);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "AboutBox";
            this.tableLayoutPanel.ResumeLayout(false);
            this.tableLayoutPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.logoPictureBox)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.PictureBox logoPictureBox;
        private System.Windows.Forms.Label labelProductName;
        private System.Windows.Forms.Label labelVersion;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Label labelCompanyName;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label ProductKeyLabel;
        private System.Windows.Forms.Label IATDesignLabel;
        private System.Windows.Forms.Label VersionLabel;
        private System.Windows.Forms.Label CopyrightLabel;
        private System.Windows.Forms.Label CompanyNameLabel;
        private System.Windows.Forms.TextBox ProductKeyEdit;
        private System.Windows.Forms.Panel ProgressPanel;
        private System.Windows.Forms.RichTextBox ProductDescription;
        private System.Windows.Forms.Button UnregisterButton;
    }
}
