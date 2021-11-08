namespace IATClient
{
    partial class KeyPreviewPanel
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
            this.LeftResponsePreviewGroup = new System.Windows.Forms.GroupBox();
            this.RightResponsePreviewGroup = new System.Windows.Forms.GroupBox();
            this.LeftResponsePreviewGroup.SuspendLayout();
            this.RightResponsePreviewGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // LeftResponsePreviewGroup
            // 
            this.LeftResponsePreviewGroup.Controls.Add(this.LeftResponsePreview);
            this.LeftResponsePreviewGroup.Location = new System.Drawing.Point(3, 3);
            this.LeftResponsePreviewGroup.Name = "LeftResponsePreviewGroup";
            this.LeftResponsePreviewGroup.Size = new System.Drawing.Size(231, 169);
            this.LeftResponsePreviewGroup.TabIndex = 0;
            this.LeftResponsePreviewGroup.TabStop = false;
            this.LeftResponsePreviewGroup.Text = "Left Response Preview";
            // 
            // RightResponsePreviewGroup
            // 
            this.RightResponsePreviewGroup.Controls.Add(this.RightResponsePreview);
            this.RightResponsePreviewGroup.Location = new System.Drawing.Point(3, 178);
            this.RightResponsePreviewGroup.Name = "RightResponsePreviewGroup";
            this.RightResponsePreviewGroup.Size = new System.Drawing.Size(231, 169);
            this.RightResponsePreviewGroup.TabIndex = 1;
            this.RightResponsePreviewGroup.TabStop = false;
            this.RightResponsePreviewGroup.Text = "Right Response Preview";
            // 
            // KeyPreviewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Name = "KeyPreviewPanel";
            this.Size = new System.Drawing.Size(237, 350);
            this.LeftResponsePreviewGroup.ResumeLayout(false);
            this.RightResponsePreviewGroup.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox LeftResponsePreviewGroup;
        private System.Windows.Forms.GroupBox RightResponsePreviewGroup;
    }
}
