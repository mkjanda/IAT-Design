namespace IATClient
{
    public partial class IATConfigMainForm
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
            this.HeaderMenu = new System.Windows.Forms.MenuStrip();
            this.MessageBar = new System.Windows.Forms.StatusStrip();
            this.StatusImage = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.Progress = new System.Windows.Forms.ToolStripProgressBar();
            this.StatusLabel = new System.Windows.Forms.ToolStripStatusLabel();
            this.QuickPanel = new System.Windows.Forms.Panel();
            this.UploadButton = new System.Windows.Forms.Button();
            this.IATPasswordBox = new System.Windows.Forms.TextBox();
            this.DataRetrievalPasswordLabel = new System.Windows.Forms.Label();
            this.IATNameBox = new System.Windows.Forms.TextBox();
            this.IATNameLabel = new System.Windows.Forms.Label();
            this.MessageBar.SuspendLayout();
            this.QuickPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // HeaderMenu
            // 
            this.HeaderMenu.Dock = System.Windows.Forms.DockStyle.None;
            this.HeaderMenu.Location = new System.Drawing.Point(0, 0);
            this.HeaderMenu.Name = "HeaderMenu";
            this.HeaderMenu.Size = new System.Drawing.Size(202, 24);
            this.HeaderMenu.TabIndex = 2;
            // 
            // MessageBar
            // 
            this.MessageBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusImage,
            this.StatusText,
            this.Progress,
            this.StatusLabel});
            this.MessageBar.Location = new System.Drawing.Point(0, 672);
            this.MessageBar.Name = "MessageBar";
            this.MessageBar.Size = new System.Drawing.Size(1008, 22);
            this.MessageBar.TabIndex = 1;
            this.MessageBar.Text = "statusStrip1";
            // 
            // StatusImage
            // 
            this.StatusImage.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
            this.StatusImage.Image = global::IATClient.Properties.Resources.go;
            this.StatusImage.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.StatusImage.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.StatusImage.Name = "StatusImage";
            this.StatusImage.Size = new System.Drawing.Size(17, 17);
            this.StatusImage.Text = "toolStripStatusLabel1";
            // 
            // StatusText
            // 
            this.StatusText.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.StatusText.Name = "StatusText";
            this.StatusText.Size = new System.Drawing.Size(34, 17);
            this.StatusText.Text = "Okay";
            // 
            // Progress
            // 
            this.Progress.Name = "Progress";
            this.Progress.Size = new System.Drawing.Size(500, 16);
            // 
            // StatusLabel
            // 
            this.StatusLabel.Margin = new System.Windows.Forms.Padding(10, 3, 10, 2);
            this.StatusLabel.Name = "StatusLabel";
            this.StatusLabel.Size = new System.Drawing.Size(0, 17);
            // 
            // QuickPanel
            // 
            this.QuickPanel.Controls.Add(this.UploadButton);
            this.QuickPanel.Controls.Add(this.IATPasswordBox);
            this.QuickPanel.Controls.Add(this.DataRetrievalPasswordLabel);
            this.QuickPanel.Controls.Add(this.IATNameBox);
            this.QuickPanel.Controls.Add(this.IATNameLabel);
            this.QuickPanel.Location = new System.Drawing.Point(533, 0);
            this.QuickPanel.Name = "QuickPanel";
            this.QuickPanel.Size = new System.Drawing.Size(475, 26);
            this.QuickPanel.TabIndex = 3;
            // 
            // UploadButton
            // 
            this.UploadButton.Location = new System.Drawing.Point(356, 1);
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.Size = new System.Drawing.Size(112, 23);
            this.UploadButton.TabIndex = 6;
            this.UploadButton.Text = "Upload";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // IATPasswordBox
            // 
            this.IATPasswordBox.Location = new System.Drawing.Point(237, 3);
            this.IATPasswordBox.Name = "IATPasswordBox";
            this.IATPasswordBox.Size = new System.Drawing.Size(100, 20);
            this.IATPasswordBox.TabIndex = 5;
            // 
            // DataRetrievalPasswordLabel
            // 
            this.DataRetrievalPasswordLabel.AutoSize = true;
            this.DataRetrievalPasswordLabel.Location = new System.Drawing.Point(178, 6);
            this.DataRetrievalPasswordLabel.Name = "DataRetrievalPasswordLabel";
            this.DataRetrievalPasswordLabel.Size = new System.Drawing.Size(56, 13);
            this.DataRetrievalPasswordLabel.TabIndex = 4;
            this.DataRetrievalPasswordLabel.Text = "Password:";
            // 
            // IATNameBox
            // 
            this.IATNameBox.Location = new System.Drawing.Point(70, 3);
            this.IATNameBox.Name = "IATNameBox";
            this.IATNameBox.Size = new System.Drawing.Size(100, 20);
            this.IATNameBox.TabIndex = 1;
            // 
            // IATNameLabel
            // 
            this.IATNameLabel.AutoSize = true;
            this.IATNameLabel.Location = new System.Drawing.Point(6, 6);
            this.IATNameLabel.Name = "IATNameLabel";
            this.IATNameLabel.Size = new System.Drawing.Size(58, 13);
            this.IATNameLabel.TabIndex = 0;
            this.IATNameLabel.Text = "IAT Name:";
            // 
            // IATConfigMainForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1008, 694);
            this.Controls.Add(this.QuickPanel);
            this.Controls.Add(this.MessageBar);
            this.Controls.Add(this.HeaderMenu);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimumSize = new System.Drawing.Size(1024, 733);
            this.Name = "IATConfigMainForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Form1";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.IATConfigMainForm_FormClosing);
            this.Load += new System.EventHandler(this.IATConfigMainForm_Load);
            this.MessageBar.ResumeLayout(false);
            this.MessageBar.PerformLayout();
            this.QuickPanel.ResumeLayout(false);
            this.QuickPanel.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion
    

        private System.Windows.Forms.MenuStrip HeaderMenu;
        private System.Windows.Forms.StatusStrip MessageBar;
        private System.Windows.Forms.ToolStripStatusLabel StatusImage;
        private System.Windows.Forms.ToolStripStatusLabel StatusText;
        private System.Windows.Forms.ToolStripProgressBar Progress;
        private System.Windows.Forms.Panel QuickPanel;
        private System.Windows.Forms.Label IATNameLabel;
        private System.Windows.Forms.TextBox IATNameBox;
        private System.Windows.Forms.TextBox IATPasswordBox;
        private System.Windows.Forms.Label DataRetrievalPasswordLabel;
        private System.Windows.Forms.Button UploadButton;
        private System.Windows.Forms.ToolStripStatusLabel StatusLabel;
    }
}

