namespace IATClient
{
    partial class MissingFontDisplay
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
            this.MissingFontItemTable = new System.Windows.Forms.TableLayoutPanel();
            this.SuspendLayout();
            // 
            // MissingFontItemTable
            // 
            this.MissingFontItemTable.ColumnCount = 4;
            this.MissingFontItemTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MissingFontItemTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MissingFontItemTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MissingFontItemTable.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
            this.MissingFontItemTable.Location = new System.Drawing.Point(0, 0);
            this.MissingFontItemTable.Name = "MissingFontItemTable";
            this.MissingFontItemTable.RowCount = 1;
            this.MissingFontItemTable.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.MissingFontItemTable.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 101F));
            this.MissingFontItemTable.Size = new System.Drawing.Size(468, 101);
            this.MissingFontItemTable.TabIndex = 0;
            // 
            // MissingFontDisplay
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MissingFontItemTable);
            this.Name = "MissingFontDisplay";
            this.Size = new System.Drawing.Size(468, 101);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel MissingFontItemTable;
    }
}
