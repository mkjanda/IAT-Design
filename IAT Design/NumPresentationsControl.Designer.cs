namespace IATClient
{
    partial class NumPresentationsControl
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
            this.NumPresentationsGroup = new System.Windows.Forms.GroupBox();
            this.NumPresentationsGrid = new System.Windows.Forms.DataGridView();
            this.Cancel = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.NumPresentationsGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumPresentationsGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // NumPresentationsGroup
            // 
            this.NumPresentationsGroup.Controls.Add(this.NumPresentationsGrid);
            this.NumPresentationsGroup.Location = new System.Drawing.Point(3, 3);
            this.NumPresentationsGroup.Name = "NumPresentationsGroup";
            this.NumPresentationsGroup.Size = new System.Drawing.Size(319, 310);
            this.NumPresentationsGroup.TabIndex = 0;
            this.NumPresentationsGroup.TabStop = false;
            this.NumPresentationsGroup.Text = "Number of Presentations";
            // 
            // NumPresentationsGrid
            // 
            this.NumPresentationsGrid.AllowUserToAddRows = false;
            this.NumPresentationsGrid.AllowUserToDeleteRows = false;
            this.NumPresentationsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.NumPresentationsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.NumPresentationsGrid.Location = new System.Drawing.Point(3, 16);
            this.NumPresentationsGrid.Name = "NumPresentationsGrid";
            this.NumPresentationsGrid.RowHeadersVisible = false;
            this.NumPresentationsGrid.Size = new System.Drawing.Size(313, 291);
            this.NumPresentationsGrid.TabIndex = 0;
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(84, 319);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 1;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(165, 319);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 2;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // NumPresentationsControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.OK);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.NumPresentationsGroup);
            this.Name = "NumPresentationsControl";
            this.Size = new System.Drawing.Size(325, 345);
            this.Load += new System.EventHandler(this.NumPresentationsControl_Load);
            this.NumPresentationsGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.NumPresentationsGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox NumPresentationsGroup;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.DataGridView NumPresentationsGrid;
    }
}
