namespace IATClient
{
    partial class IATUploadForm
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
            this.UploadIATInstructions = new System.Windows.Forms.TextBox();
            this.ServerLabel = new System.Windows.Forms.Label();
            this.ServerURLEdit = new System.Windows.Forms.TextBox();
            this.Upload = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // UploadIATInstructions
            // 
            this.UploadIATInstructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.UploadIATInstructions.Location = new System.Drawing.Point(12, 12);
            this.UploadIATInstructions.Multiline = true;
            this.UploadIATInstructions.Name = "UploadIATInstructions";
            this.UploadIATInstructions.ReadOnly = true;
            this.UploadIATInstructions.Size = new System.Drawing.Size(362, 35);
            this.UploadIATInstructions.TabIndex = 0;
            this.UploadIATInstructions.TabStop = false;
            this.UploadIATInstructions.Text = "Use the text edit box below to enter the URL of the IAT server which is to admini" +
                "ster the IAT and surveys and click the upload button to proceed.";
            // 
            // ServerLabel
            // 
            this.ServerLabel.AutoSize = true;
            this.ServerLabel.Location = new System.Drawing.Point(12, 50);
            this.ServerLabel.Name = "ServerLabel";
            this.ServerLabel.Size = new System.Drawing.Size(66, 13);
            this.ServerLabel.TabIndex = 2;
            this.ServerLabel.Text = "Server URL:";
            // 
            // ServerURLEdit
            // 
            this.ServerURLEdit.Location = new System.Drawing.Point(84, 47);
            this.ServerURLEdit.Name = "ServerURLEdit";
            this.ServerURLEdit.Size = new System.Drawing.Size(290, 20);
            this.ServerURLEdit.TabIndex = 1;
            // 
            // Upload
            // 
            this.Upload.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Upload.Location = new System.Drawing.Point(91, 85);
            this.Upload.Name = "Upload";
            this.Upload.Size = new System.Drawing.Size(75, 23);
            this.Upload.TabIndex = 3;
            this.Upload.Text = "Upload";
            this.Upload.UseVisualStyleBackColor = true;
            this.Upload.Click += new System.EventHandler(this.Upload_Click);
            // 
            // Cancel
            // 
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(221, 85);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 4;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // IATUploadForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(386, 119);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Upload);
            this.Controls.Add(this.ServerURLEdit);
            this.Controls.Add(this.ServerLabel);
            this.Controls.Add(this.UploadIATInstructions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "IATUploadForm";
            this.ShowInTaskbar = false;
            this.Text = "Upload IAT";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox UploadIATInstructions;
        private System.Windows.Forms.Label ServerLabel;
        private System.Windows.Forms.TextBox ServerURLEdit;
        private System.Windows.Forms.Button Upload;
        private System.Windows.Forms.Button Cancel;
    }
}