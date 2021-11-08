namespace IATClient
{
    partial class IATItemsReorderPanel
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
            this.ReorderBox = new System.Windows.Forms.GroupBox();
            this.ReorderView = new System.Windows.Forms.ListView();
            this.ReorderInstructionsBox = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.ResetButton = new System.Windows.Forms.Button();
            this.ReorderBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // ReorderBox
            // 
            this.ReorderBox.Controls.Add(this.ReorderView);
            this.ReorderBox.Location = new System.Drawing.Point(3, 56);
            this.ReorderBox.Name = "ReorderBox";
            this.ReorderBox.Size = new System.Drawing.Size(369, 417);
            this.ReorderBox.TabIndex = 0;
            this.ReorderBox.TabStop = false;
            this.ReorderBox.Text = "Reorder Items";
            // 
            // ReorderView
            // 
            this.ReorderView.AllowDrop = true;
            this.ReorderView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ReorderView.Location = new System.Drawing.Point(3, 16);
            this.ReorderView.MultiSelect = false;
            this.ReorderView.Name = "ReorderView";
            this.ReorderView.Size = new System.Drawing.Size(363, 398);
            this.ReorderView.TabIndex = 0;
            this.ReorderView.UseCompatibleStateImageBehavior = false;
            this.ReorderView.View = System.Windows.Forms.View.List;
            this.ReorderView.SelectedIndexChanged += new System.EventHandler(this.ReorderView_SelectedIndexChanged);
            this.ReorderView.DragDrop += new System.Windows.Forms.DragEventHandler(this.ReorderView_DragDrop);
            this.ReorderView.DragEnter += new System.Windows.Forms.DragEventHandler(this.ReorderView_DragEnter);
            this.ReorderView.DragLeave += new System.EventHandler(this.ReorderView_DragLeave);
            this.ReorderView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.ReorderView_ItemDrag);
            this.ReorderView.DragOver += new System.Windows.Forms.DragEventHandler(this.ReorderView_DragOver);
            // 
            // ReorderInstructionsBox
            // 
            this.ReorderInstructionsBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.ReorderInstructionsBox.Location = new System.Drawing.Point(3, 3);
            this.ReorderInstructionsBox.Multiline = true;
            this.ReorderInstructionsBox.Name = "ReorderInstructionsBox";
            this.ReorderInstructionsBox.ReadOnly = true;
            this.ReorderInstructionsBox.Size = new System.Drawing.Size(369, 47);
            this.ReorderInstructionsBox.TabIndex = 1;
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(6, 479);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 2;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(150, 479);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 3;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // ResetButton
            // 
            this.ResetButton.Location = new System.Drawing.Point(294, 479);
            this.ResetButton.Name = "ResetButton";
            this.ResetButton.Size = new System.Drawing.Size(75, 23);
            this.ResetButton.TabIndex = 4;
            this.ResetButton.Text = "Reset";
            this.ResetButton.UseVisualStyleBackColor = true;
            this.ResetButton.Click += new System.EventHandler(this.ResetButton_Click);
            // 
            // IATItemsReorderPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ResetButton);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.ReorderInstructionsBox);
            this.Controls.Add(this.ReorderBox);
            this.Name = "IATItemsReorderPanel";
            this.Size = new System.Drawing.Size(375, 505);
            this.Load += new System.EventHandler(this.IATItemsReorderPanel_Load);
            this.ParentChanged += new System.EventHandler(this.IATItemsReorderPanel_ParentChanged);
            this.ReorderBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox ReorderBox;
        private System.Windows.Forms.ListView ReorderView;
        private System.Windows.Forms.TextBox ReorderInstructionsBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.Button ResetButton;
    }
}
