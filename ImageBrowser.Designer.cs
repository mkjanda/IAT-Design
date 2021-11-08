namespace IATClient
{
    partial class ImageBrowser
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
            this.ImageGroup = new System.Windows.Forms.GroupBox();
            this.ImageList = new System.Windows.Forms.ListView();
            this.Accept = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.ImageGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // ImageGroup
            // 
            this.ImageGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ImageGroup.Controls.Add(this.ImageList);
            this.ImageGroup.Location = new System.Drawing.Point(12, 12);
            this.ImageGroup.Name = "ImageGroup";
            this.ImageGroup.Size = new System.Drawing.Size(740, 450);
            this.ImageGroup.TabIndex = 0;
            this.ImageGroup.TabStop = false;
            this.ImageGroup.Text = "Images in IAT File";
            // 
            // ImageList
            // 
            this.ImageList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ImageList.Location = new System.Drawing.Point(3, 16);
            this.ImageList.Name = "ImageList";
            this.ImageList.Size = new System.Drawing.Size(695, 450);
            this.ImageList.TabIndex = 0;
            this.ImageList.UseCompatibleStateImageBehavior = false;
            this.ImageList.SelectedIndexChanged += new System.EventHandler(this.ImageList_SelectedIndexChanged);
            // 
            // Accept
            // 
            this.Accept.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.Accept.Location = new System.Drawing.Point(250, 470);
            this.Accept.Name = "Accept";
            this.Accept.Size = new System.Drawing.Size(98, 23);
            this.Accept.TabIndex = 1;
            this.Accept.Text = "Accept";
            this.Accept.UseVisualStyleBackColor = true;
            this.Accept.Click += new System.EventHandler(this.Accept_Click);
            // 
            // Cancel
            // 
            this.Cancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Cancel.Location = new System.Drawing.Point(352, 470);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(91, 23);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // ImageBrowser
            // 
            this.AcceptButton = this.Accept;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.Cancel;
            this.ClientSize = new System.Drawing.Size(760, 500);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.Accept);
            this.Controls.Add(this.ImageGroup);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Sizable;
            this.Name = "ImageBrowser";
            this.ShowInTaskbar = false;
            this.Text = "Image Browser";
            this.ImageGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox ImageGroup;
        private System.Windows.Forms.ListView ImageList;
        private System.Windows.Forms.Button Accept;
        private System.Windows.Forms.Button Cancel;

    }
}