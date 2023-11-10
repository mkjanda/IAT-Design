namespace IATClient
{
    partial class TextEditControl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(TextEditControl));
            this.FontToolStrip = new System.Windows.Forms.ToolStrip();
            this.FontLabel = new System.Windows.Forms.ToolStripLabel();
            this.FontSizeDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.ColorDropDown = new System.Windows.Forms.ToolStripDropDownButton();
            this.FontToolStrip.SuspendLayout();
            this.SuspendLayout();
            // 
            // FontToolStrip
            // 
            this.FontToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.FontLabel,
            this.FontSizeDropDown,
            this.ColorDropDown});
            this.FontToolStrip.Location = new System.Drawing.Point(0, 0);
            this.FontToolStrip.Name = "FontToolStrip";
            this.FontToolStrip.Size = new System.Drawing.Size(375, 25);
            this.FontToolStrip.TabIndex = 0;
            this.FontToolStrip.Text = "toolStrip1";
            FontToolStrip.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            // 
            // FontLabel
            // 
            this.FontLabel.Name = "FontLabel";
            this.FontLabel.Size = new System.Drawing.Size(34, 22);
            this.FontLabel.Text = "Font:";
            // 
            // FontSizeDropDown
            // 
            this.FontSizeDropDown.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.FontSizeDropDown.Image = ((System.Drawing.Image)(resources.GetObject("FontSizeDropDown.Image")));
            this.FontSizeDropDown.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.FontSizeDropDown.Name = "FontSizeDropDown";
            this.FontSizeDropDown.Size = new System.Drawing.Size(67, 22);
            this.FontSizeDropDown.Text = "Font Size";
            this.FontSizeDropDown.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.FontSizeDropDown_DropDownItemClicked);
            // 
            // System.Drawing.ColorDropDown
            // 
            this.ColorDropDown.Image = ((System.Drawing.Image)(resources.GetObject("ColorDropDown.Image")));
            this.ColorDropDown.ImageTransparentColor = System.Drawing.Color.Transparent;
            this.ColorDropDown.Name = "ColorDropDown";
            this.ColorDropDown.Size = new System.Drawing.Size(65, 22);
            this.ColorDropDown.Text = "Color";
            this.ColorDropDown.DropDownItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(this.ColorDropDown_DropDownItemClicked);
            // 
            // TextEditControl
            // 
            this.Controls.Add(this.FontToolStrip);
            this.Name = "TextEditControl";
            this.Size = new System.Drawing.Size(375, 45);
            this.FontToolStrip.ResumeLayout(false);
            this.FontToolStrip.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ToolStrip FontToolStrip;
        private System.Windows.Forms.ToolStripLabel FontLabel;
        protected System.Windows.Forms.ToolStripDropDownButton FontSizeDropDown;
        protected System.Windows.Forms.ToolStripDropDownButton ColorDropDown;
    }
}
