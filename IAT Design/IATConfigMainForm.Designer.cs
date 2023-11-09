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

