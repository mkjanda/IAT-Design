namespace IATClient
{
    partial class ServerInterface
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
            this.DataRetrievalPasswordLabel = new System.Windows.Forms.Label();
            this.IATPassword = new System.Windows.Forms.TextBox();
            this.IATNameLabel = new System.Windows.Forms.Label();
            this.IATName = new System.Windows.Forms.TextBox();
            this.RetrieveData = new System.Windows.Forms.Button();
            this.DataFileFormatGroup = new System.Windows.Forms.GroupBox();
            this.DelimitationList = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.IATRawItemGroupedRadio = new System.Windows.Forms.RadioButton();
            this.IATRawTesteeGroupedRadio = new System.Windows.Forms.RadioButton();
            this.ResultFileWithoutHeaderRadio = new System.Windows.Forms.RadioButton();
            this.ResultFileRadio = new System.Windows.Forms.RadioButton();
            this.ExportData = new System.Windows.Forms.Button();
            this.DataRetrievalGroup = new System.Windows.Forms.GroupBox();
            this.RetrieveItemSlides = new System.Windows.Forms.Button();
            this.DeleteData = new System.Windows.Forms.Button();
            this.UploadGroup = new System.Windows.Forms.GroupBox();
            this.UploadButton = new System.Windows.Forms.Button();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.IATFile = new System.Windows.Forms.TextBox();
            this.ServerURL = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.Delete = new System.Windows.Forms.Button();
            this.IATDeleteName = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.DataFileFormatGroup.SuspendLayout();
            this.DataRetrievalGroup.SuspendLayout();
            this.UploadGroup.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // DataRetrievalPasswordLabel
            // 
            this.DataRetrievalPasswordLabel.AutoSize = true;
            this.DataRetrievalPasswordLabel.Location = new System.Drawing.Point(8, 42);
            this.DataRetrievalPasswordLabel.Name = "DataRetrievalPasswordLabel";
            this.DataRetrievalPasswordLabel.Size = new System.Drawing.Size(76, 13);
            this.DataRetrievalPasswordLabel.TabIndex = 2;
            this.DataRetrievalPasswordLabel.Text = "IAT Password:";
            // 
            // IATPassword
            // 
            this.IATPassword.Location = new System.Drawing.Point(90, 39);
            this.IATPassword.Name = "IATPassword";
            this.IATPassword.Size = new System.Drawing.Size(325, 20);
            this.IATPassword.TabIndex = 3;
            // 
            // IATNameLabel
            // 
            this.IATNameLabel.AutoSize = true;
            this.IATNameLabel.Location = new System.Drawing.Point(36, 22);
            this.IATNameLabel.Name = "IATNameLabel";
            this.IATNameLabel.Size = new System.Drawing.Size(58, 13);
            this.IATNameLabel.TabIndex = 6;
            this.IATNameLabel.Text = "IAT Name:";
            // 
            // IATName
            // 
            this.IATName.Location = new System.Drawing.Point(100, 19);
            this.IATName.Name = "IATName";
            this.IATName.Size = new System.Drawing.Size(292, 20);
            this.IATName.TabIndex = 7;
            // 
            // RetrieveData
            // 
            this.RetrieveData.Location = new System.Drawing.Point(265, 67);
            this.RetrieveData.Name = "RetrieveData";
            this.RetrieveData.Size = new System.Drawing.Size(126, 23);
            this.RetrieveData.TabIndex = 8;
            this.RetrieveData.Text = "Retrieve Data";
            this.RetrieveData.UseVisualStyleBackColor = true;
            this.RetrieveData.Click += new System.EventHandler(this.RetrieveData_Click);
            // 
            // DataFileFormatGroup
            // 
            this.DataFileFormatGroup.Controls.Add(this.DelimitationList);
            this.DataFileFormatGroup.Controls.Add(this.label1);
            this.DataFileFormatGroup.Controls.Add(this.IATRawItemGroupedRadio);
            this.DataFileFormatGroup.Controls.Add(this.IATRawTesteeGroupedRadio);
            this.DataFileFormatGroup.Controls.Add(this.ResultFileWithoutHeaderRadio);
            this.DataFileFormatGroup.Controls.Add(this.ResultFileRadio);
            this.DataFileFormatGroup.Location = new System.Drawing.Point(18, 48);
            this.DataFileFormatGroup.Name = "DataFileFormatGroup";
            this.DataFileFormatGroup.Size = new System.Drawing.Size(225, 147);
            this.DataFileFormatGroup.TabIndex = 9;
            this.DataFileFormatGroup.TabStop = false;
            this.DataFileFormatGroup.Text = "Data File Format";
            // 
            // DelimitationList
            // 
            this.DelimitationList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DelimitationList.FormattingEnabled = true;
            this.DelimitationList.Items.AddRange(new object[] {
            "Comma",
            "Space",
            "Tab"});
            this.DelimitationList.Location = new System.Drawing.Point(76, 111);
            this.DelimitationList.Name = "DelimitationList";
            this.DelimitationList.Size = new System.Drawing.Size(104, 21);
            this.DelimitationList.TabIndex = 5;
            this.DelimitationList.SelectedIndexChanged += new System.EventHandler(this.DelimitationList_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 114);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "Delimitation:";
            // 
            // IATRawItemGroupedRadio
            // 
            this.IATRawItemGroupedRadio.AutoSize = true;
            this.IATRawItemGroupedRadio.Location = new System.Drawing.Point(6, 88);
            this.IATRawItemGroupedRadio.Name = "IATRawItemGroupedRadio";
            this.IATRawItemGroupedRadio.Size = new System.Drawing.Size(174, 17);
            this.IATRawItemGroupedRadio.TabIndex = 3;
            this.IATRawItemGroupedRadio.TabStop = true;
            this.IATRawItemGroupedRadio.Text = "IAT Raw Data Grouped by Item";
            this.IATRawItemGroupedRadio.UseVisualStyleBackColor = true;
            this.IATRawItemGroupedRadio.CheckedChanged += new System.EventHandler(this.IATRawItemGroupedRadio_CheckedChanged);
            // 
            // IATRawTesteeGroupedRadio
            // 
            this.IATRawTesteeGroupedRadio.AutoSize = true;
            this.IATRawTesteeGroupedRadio.Location = new System.Drawing.Point(6, 65);
            this.IATRawTesteeGroupedRadio.Name = "IATRawTesteeGroupedRadio";
            this.IATRawTesteeGroupedRadio.Size = new System.Drawing.Size(187, 17);
            this.IATRawTesteeGroupedRadio.TabIndex = 2;
            this.IATRawTesteeGroupedRadio.TabStop = true;
            this.IATRawTesteeGroupedRadio.Text = "IAT Raw Data Grouped by Testee";
            this.IATRawTesteeGroupedRadio.UseVisualStyleBackColor = true;
            this.IATRawTesteeGroupedRadio.CheckedChanged += new System.EventHandler(this.IATRawTesteeGroupedRadio_CheckedChanged);
            // 
            // ResultFileWithoutHeaderRadio
            // 
            this.ResultFileWithoutHeaderRadio.AutoSize = true;
            this.ResultFileWithoutHeaderRadio.Location = new System.Drawing.Point(6, 42);
            this.ResultFileWithoutHeaderRadio.Name = "ResultFileWithoutHeaderRadio";
            this.ResultFileWithoutHeaderRadio.Size = new System.Drawing.Size(152, 17);
            this.ResultFileWithoutHeaderRadio.TabIndex = 1;
            this.ResultFileWithoutHeaderRadio.TabStop = true;
            this.ResultFileWithoutHeaderRadio.Text = "Result File Without Header";
            this.ResultFileWithoutHeaderRadio.UseVisualStyleBackColor = true;
            this.ResultFileWithoutHeaderRadio.CheckedChanged += new System.EventHandler(this.ResultFileWithoutHeaderRadio_CheckedChanged);
            // 
            // ResultFileRadio
            // 
            this.ResultFileRadio.AutoSize = true;
            this.ResultFileRadio.Location = new System.Drawing.Point(6, 19);
            this.ResultFileRadio.Name = "ResultFileRadio";
            this.ResultFileRadio.Size = new System.Drawing.Size(137, 17);
            this.ResultFileRadio.TabIndex = 0;
            this.ResultFileRadio.TabStop = true;
            this.ResultFileRadio.Text = "Result File With Header";
            this.ResultFileRadio.UseVisualStyleBackColor = true;
            this.ResultFileRadio.CheckedChanged += new System.EventHandler(this.ResultFileRadio_CheckedChanged);
            // 
            // ExportData
            // 
            this.ExportData.Location = new System.Drawing.Point(265, 127);
            this.ExportData.Name = "ExportData";
            this.ExportData.Size = new System.Drawing.Size(126, 23);
            this.ExportData.TabIndex = 10;
            this.ExportData.Text = "Export Data";
            this.ExportData.UseVisualStyleBackColor = true;
            this.ExportData.Click += new System.EventHandler(this.ExportData_Click);
            // 
            // DataRetrievalGroup
            // 
            this.DataRetrievalGroup.Controls.Add(this.RetrieveItemSlides);
            this.DataRetrievalGroup.Controls.Add(this.DeleteData);
            this.DataRetrievalGroup.Controls.Add(this.ExportData);
            this.DataRetrievalGroup.Controls.Add(this.DataFileFormatGroup);
            this.DataRetrievalGroup.Controls.Add(this.RetrieveData);
            this.DataRetrievalGroup.Controls.Add(this.IATName);
            this.DataRetrievalGroup.Controls.Add(this.IATNameLabel);
            this.DataRetrievalGroup.Location = new System.Drawing.Point(12, 125);
            this.DataRetrievalGroup.Name = "DataRetrievalGroup";
            this.DataRetrievalGroup.Size = new System.Drawing.Size(409, 212);
            this.DataRetrievalGroup.TabIndex = 11;
            this.DataRetrievalGroup.TabStop = false;
            this.DataRetrievalGroup.Text = "Data Retrieval";
            // 
            // RetrieveItemSlides
            // 
            this.RetrieveItemSlides.Location = new System.Drawing.Point(266, 97);
            this.RetrieveItemSlides.Name = "RetrieveItemSlides";
            this.RetrieveItemSlides.Size = new System.Drawing.Size(126, 23);
            this.RetrieveItemSlides.TabIndex = 12;
            this.RetrieveItemSlides.Text = "Retrieve Item Slides";
            this.RetrieveItemSlides.UseVisualStyleBackColor = true;
            this.RetrieveItemSlides.Click += new System.EventHandler(this.RetrieveItemSlides_Click);
            // 
            // DeleteData
            // 
            this.DeleteData.Location = new System.Drawing.Point(265, 157);
            this.DeleteData.Name = "DeleteData";
            this.DeleteData.Size = new System.Drawing.Size(126, 23);
            this.DeleteData.TabIndex = 11;
            this.DeleteData.Text = "Delete Data";
            this.DeleteData.UseVisualStyleBackColor = true;
            this.DeleteData.Click += new System.EventHandler(this.DeleteData_Click);
            // 
            // UploadGroup
            // 
            this.UploadGroup.Controls.Add(this.UploadButton);
            this.UploadGroup.Controls.Add(this.BrowseButton);
            this.UploadGroup.Controls.Add(this.label3);
            this.UploadGroup.Controls.Add(this.IATFile);
            this.UploadGroup.Location = new System.Drawing.Point(15, 65);
            this.UploadGroup.Name = "UploadGroup";
            this.UploadGroup.Size = new System.Drawing.Size(400, 50);
            this.UploadGroup.TabIndex = 12;
            this.UploadGroup.TabStop = false;
            this.UploadGroup.Text = "IAT Upload";
            // 
            // UploadButton
            // 
            this.UploadButton.Location = new System.Drawing.Point(324, 17);
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.Size = new System.Drawing.Size(75, 23);
            this.UploadButton.TabIndex = 5;
            this.UploadButton.Text = "Upload";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(243, 17);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BrowseButton.TabIndex = 4;
            this.BrowseButton.Text = "Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(2, 22);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(46, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "IAT File:";
            // 
            // IATFile
            // 
            this.IATFile.BackColor = System.Drawing.SystemColors.Window;
            this.IATFile.Location = new System.Drawing.Point(54, 19);
            this.IATFile.Name = "IATFile";
            this.IATFile.ReadOnly = true;
            this.IATFile.Size = new System.Drawing.Size(183, 20);
            this.IATFile.TabIndex = 1;
            // 
            // ServerURL
            // 
            this.ServerURL.Location = new System.Drawing.Point(90, 12);
            this.ServerURL.Name = "ServerURL";
            this.ServerURL.ReadOnly = true;
            this.ServerURL.Size = new System.Drawing.Size(325, 20);
            this.ServerURL.TabIndex = 3;
            this.ServerURL.Text = "http://www.iatsoftware.net/IATServer/";
            this.ServerURL.TextChanged += new System.EventHandler(this.UploadServerURL_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(18, 15);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(66, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Server URL:";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.Delete);
            this.groupBox1.Controls.Add(this.IATDeleteName);
            this.groupBox1.Controls.Add(this.label4);
            this.groupBox1.Location = new System.Drawing.Point(12, 347);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(409, 60);
            this.groupBox1.TabIndex = 13;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Delete IAT";
            // 
            // Delete
            // 
            this.Delete.Location = new System.Drawing.Point(276, 17);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(75, 23);
            this.Delete.TabIndex = 4;
            this.Delete.Text = "Delete";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // IATDeleteName
            // 
            this.IATDeleteName.Location = new System.Drawing.Point(88, 19);
            this.IATDeleteName.Name = "IATDeleteName";
            this.IATDeleteName.Size = new System.Drawing.Size(155, 20);
            this.IATDeleteName.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(24, 22);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(58, 13);
            this.label4.TabIndex = 0;
            this.label4.Text = "IAT Name:";
            // 
            // ServerInterface
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(433, 415);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.UploadGroup);
            this.Controls.Add(this.DataRetrievalGroup);
            this.Controls.Add(this.ServerURL);
            this.Controls.Add(this.DataRetrievalPasswordLabel);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.IATPassword);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ServerInterface";
            this.ShowInTaskbar = false;
            this.Text = "Server Interface";
            this.Load += new System.EventHandler(this.DataRetrievalDialog_Load);
            this.DataFileFormatGroup.ResumeLayout(false);
            this.DataFileFormatGroup.PerformLayout();
            this.DataRetrievalGroup.ResumeLayout(false);
            this.DataRetrievalGroup.PerformLayout();
            this.UploadGroup.ResumeLayout(false);
            this.UploadGroup.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label DataRetrievalPasswordLabel;
        private System.Windows.Forms.TextBox IATPassword;
        private System.Windows.Forms.Label IATNameLabel;
        private System.Windows.Forms.TextBox IATName;
        private System.Windows.Forms.Button RetrieveData;
        private System.Windows.Forms.GroupBox DataFileFormatGroup;
        private System.Windows.Forms.RadioButton ResultFileRadio;
        private System.Windows.Forms.ComboBox DelimitationList;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton IATRawItemGroupedRadio;
        private System.Windows.Forms.RadioButton IATRawTesteeGroupedRadio;
        private System.Windows.Forms.RadioButton ResultFileWithoutHeaderRadio;
        private System.Windows.Forms.Button ExportData;
        private System.Windows.Forms.GroupBox DataRetrievalGroup;
        private System.Windows.Forms.GroupBox UploadGroup;
        private System.Windows.Forms.TextBox ServerURL;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox IATFile;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button UploadButton;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox IATDeleteName;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.Button DeleteData;
        private System.Windows.Forms.Button RetrieveItemSlides;
    }
}