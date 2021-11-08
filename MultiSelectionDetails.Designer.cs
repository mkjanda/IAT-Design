namespace IATClient
{
    partial class MultiSelectionDetails
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
            this.MultiSelectionGroup = new System.Windows.Forms.GroupBox();
            this.DeleteItemButton = new System.Windows.Forms.Button();
            this.AddItemButton = new System.Windows.Forms.Button();
            this.MultiSelectView = new System.Windows.Forms.DataGridView();
            this.OptionColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MultiSelectionGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MultiSelectView)).BeginInit();
            this.SuspendLayout();
            // 
            // MultiSelectionGroup
            // 
            this.MultiSelectionGroup.Controls.Add(this.DeleteItemButton);
            this.MultiSelectionGroup.Controls.Add(this.AddItemButton);
            this.MultiSelectionGroup.Controls.Add(this.MultiSelectView);
            this.MultiSelectionGroup.Location = new System.Drawing.Point(3, 3);
            this.MultiSelectionGroup.Name = "MultiSelectionGroup";
            this.MultiSelectionGroup.Size = new System.Drawing.Size(220, 235);
            this.MultiSelectionGroup.TabIndex = 0;
            this.MultiSelectionGroup.TabStop = false;
            this.MultiSelectionGroup.Text = "Multiple Selection";
            // 
            // DeleteItemButton
            // 
            this.DeleteItemButton.Location = new System.Drawing.Point(125, 203);
            this.DeleteItemButton.Name = "DeleteItemButton";
            this.DeleteItemButton.Size = new System.Drawing.Size(75, 23);
            this.DeleteItemButton.TabIndex = 2;
            this.DeleteItemButton.Text = "Delete Item";
            this.DeleteItemButton.UseVisualStyleBackColor = true;
            this.DeleteItemButton.Click += new System.EventHandler(this.DeleteItemButton_Click);
            // 
            // AddItemButton
            // 
            this.AddItemButton.Location = new System.Drawing.Point(20, 203);
            this.AddItemButton.Name = "AddItemButton";
            this.AddItemButton.Size = new System.Drawing.Size(75, 23);
            this.AddItemButton.TabIndex = 1;
            this.AddItemButton.Text = "Add Item";
            this.AddItemButton.UseVisualStyleBackColor = true;
            this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
            // 
            // MultiSelectView
            // 
            this.MultiSelectView.AllowUserToAddRows = false;
            this.MultiSelectView.AllowUserToDeleteRows = false;
            this.MultiSelectView.AllowUserToResizeColumns = false;
            this.MultiSelectView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.MultiSelectView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.OptionColumn});
            this.MultiSelectView.Location = new System.Drawing.Point(6, 20);
            this.MultiSelectView.MultiSelect = false;
            this.MultiSelectView.Name = "MultiSelectView";
            this.MultiSelectView.RowHeadersVisible = false;
            this.MultiSelectView.Size = new System.Drawing.Size(208, 177);
            this.MultiSelectView.TabIndex = 0;
            this.MultiSelectView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.MultiSelectView_CellEndEdit);
            // 
            // OptionColumn
            // 
            this.OptionColumn.HeaderText = "Options";
            this.OptionColumn.Name = "OptionColumn";
            this.OptionColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.OptionColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.OptionColumn.Width = 205;
            // 
            // MultiSelectionDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MultiSelectionGroup);
            this.Name = "MultiSelectionDetails";
            this.Size = new System.Drawing.Size(226, 241);
            this.Load += new System.EventHandler(this.MultiSelectionDetails_Load);
            this.ParentChanged += new System.EventHandler(this.MultiSelectionDetails_ParentChanged);
            this.MultiSelectionGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.MultiSelectView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox MultiSelectionGroup;
        private System.Windows.Forms.DataGridView MultiSelectView;
        private System.Windows.Forms.DataGridViewTextBoxColumn OptionColumn;
        private System.Windows.Forms.Button AddItemButton;
        private System.Windows.Forms.Button DeleteItemButton;
    }
}
