namespace IATClient
{
    partial class ServerPanel
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
            this.DataPanel = new IATClient.DataRetrievalPanel();
            this.PackagePanel = new IATClient.TestPackagerPanel();
            this.SuspendLayout();
            // 
            // DataPanel
            // 
            this.DataPanel.Location = new System.Drawing.Point(401, 15);
            this.DataPanel.Name = "DataPanel";
            this.DataPanel.Size = new System.Drawing.Size(386, 468);
            this.DataPanel.TabIndex = 1;
            this.DataPanel.theIAT = null;
            // 
            // PackagePanel
            // 
            this.PackagePanel.IATName = "";
            this.PackagePanel.Location = new System.Drawing.Point(0, 26);
            this.PackagePanel.Name = "PackagePanel";
            this.PackagePanel.Size = new System.Drawing.Size(401, 452);
            this.PackagePanel.TabIndex = 0;
            // 
            // ServerPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DataPanel);
            this.Controls.Add(this.PackagePanel);
            this.Name = "ServerPanel";
            this.Size = new System.Drawing.Size(787, 505);
            this.ParentChanged += new System.EventHandler(this.ServerPanel_ParentChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private TestPackagerPanel PackagePanel;
        private DataRetrievalPanel DataPanel;
    }
}
