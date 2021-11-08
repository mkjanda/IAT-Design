namespace IATClient
{
    partial class ConfirmV11Form
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfirmV11Form));
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.V10Link = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.StopShowing = new System.Windows.Forms.CheckBox();
            this.OK = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // textBox1
            // 
            this.textBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(12, 12);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(442, 110);
            this.textBox1.TabIndex = 0;
            this.textBox1.TabStop = false;
            this.textBox1.Text = resources.GetString("textBox1.Text");
            // 
            // V10Link
            // 
            this.V10Link.AutoSize = true;
            this.V10Link.Location = new System.Drawing.Point(130, 142);
            this.V10Link.Name = "V10Link";
            this.V10Link.Size = new System.Drawing.Size(258, 13);
            this.V10Link.TabIndex = 1;
            this.V10Link.TabStop = true;
            this.V10Link.Text = "https://www.iatsoftware.net/DownloadSoftware/V10";
            this.V10Link.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.V10Link_LinkClicked);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(9, 142);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(115, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Download Old Version:";
            // 
            // StopShowing
            // 
            this.StopShowing.AutoSize = true;
            this.StopShowing.Location = new System.Drawing.Point(97, 178);
            this.StopShowing.Name = "StopShowing";
            this.StopShowing.Size = new System.Drawing.Size(108, 17);
            this.StopShowing.TabIndex = 3;
            this.StopShowing.Text = "Don\'t show again";
            this.StopShowing.UseVisualStyleBackColor = true;
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(232, 174);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(92, 23);
            this.OK.TabIndex = 4;
            this.OK.Text = "I Understand";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // ConfirmV11Form
            // 
            this.AcceptButton = this.OK;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(466, 207);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.StopShowing);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.V10Link);
            this.Controls.Add(this.textBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ConfirmV11Form";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "Notification";
            this.TopMost = true;
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.LinkLabel V10Link;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.CheckBox StopShowing;
        private System.Windows.Forms.Button OK;
    }
}