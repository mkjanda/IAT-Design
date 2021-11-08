namespace IATClient
{
    partial class IATItemPreviewPanel
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
            this.PreviewGroup = new System.Windows.Forms.GroupBox();
            this.ItemPreview = new System.Windows.Forms.PictureBox();
//            this.scrollingPreviewPanel1 = new IATClient.ScrollingPreviewPanel();
            this.PreviewGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ItemPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // PreviewGroup
            // 
            this.PreviewGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.PreviewGroup.Controls.Add(this.scrollingPreviewPanel1);
            this.PreviewGroup.Controls.Add(this.ItemPreview);
            this.PreviewGroup.Location = new System.Drawing.Point(4, 4);
            this.PreviewGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PreviewGroup.Name = "PreviewGroup";
            this.PreviewGroup.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.PreviewGroup.Size = new System.Drawing.Size(675, 639);
            this.PreviewGroup.TabIndex = 0;
            this.PreviewGroup.TabStop = false;
            this.PreviewGroup.Text = "Item Preview";
            // 
            // ItemPreview
            // 
            this.ItemPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ItemPreview.Location = new System.Drawing.Point(4, 19);
            this.ItemPreview.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ItemPreview.Name = "ItemPreview";
            this.ItemPreview.Size = new System.Drawing.Size(667, 616);
            this.ItemPreview.TabIndex = 0;
            this.ItemPreview.TabStop = false;
            // 
            // scrollingPreviewPanel1
            // 
            /*this.scrollingPreviewPanel1.AllowDrop = true;
            
            this.scrollingPreviewPanel1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.scrollingPreviewPanel1.Location = new System.Drawing.Point(161, 181);
            this.scrollingPreviewPanel1.Name = "scrollingPreviewPanel1";
            this.scrollingPreviewPanel1.Orientation = IATClient.ScrollingPreviewPanel.EOrientation.unset;
            this.scrollingPreviewPanel1.PreviewSize = new System.Drawing.Size(0, 0);
            this.scrollingPreviewPanel1.SelectedPreview = -1;
            this.scrollingPreviewPanel1.Size = new System.Drawing.Size(150, 150);
            this.scrollingPreviewPanel1.TabIndex = 1; */
            // 
            // IATItemPreviewPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PreviewGroup);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "IATItemPreviewPanel";
            this.Size = new System.Drawing.Size(683, 646);
            this.ParentChanged += new System.EventHandler(this.IATItemPreviewPanel_ParentChanged);
            this.PreviewGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ItemPreview)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox PreviewGroup;
        private System.Windows.Forms.PictureBox ItemPreview;
        private ScrollingPreviewPanel scrollingPreviewPanel1;

    }
}
