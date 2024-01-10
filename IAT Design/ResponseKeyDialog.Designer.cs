namespace IATClient
{
    public partial class ResponseKeyDialog
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
            this.MessageBar = new System.Windows.Forms.StatusStrip();
            this.StatusImage = new System.Windows.Forms.ToolStripStatusLabel();
            this.StatusText = new System.Windows.Forms.ToolStripStatusLabel();
            this.MessageBar.SuspendLayout();
            this.SuspendLayout();
            // 
            // MessageBar
            // 
            this.MessageBar.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.StatusImage,
            this.StatusText});
            this.MessageBar.Location = new System.Drawing.Point(0, 552);
            this.MessageBar.Name = "MessageBar";
            this.MessageBar.Size = new System.Drawing.Size(794, 22);
            this.MessageBar.TabIndex = 0;
            this.MessageBar.Text = "statusStrip1";
            // 
            // StatusImage
            // 
            this.StatusImage.Image = global::IATClient.Properties.Resources.go;
            this.StatusImage.Name = "StatusImage";
            this.StatusImage.Size = new System.Drawing.Size(16, 17);
            // 
            // StatusText
            // 
            this.StatusText.Name = "StatusText";
            this.StatusText.Size = new System.Drawing.Size(34, 17);
            this.StatusText.Text = "Okay";
            // 
            // ResponseKeyDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(72F, 72F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.ClientSize = new System.Drawing.Size(800, 600);
            this.Controls.Add(this.MessageBar);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "ResponseKeyDialog";
            this.ShowInTaskbar = false;
            this.Text = "Response Keys";
            this.MessageBar.ResumeLayout(false);
            this.MessageBar.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip MessageBar;
        private System.Windows.Forms.ToolStripStatusLabel StatusImage;
        private System.Windows.Forms.ToolStripStatusLabel StatusText;
    }
}