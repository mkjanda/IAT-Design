namespace IATClient
{
    partial class WeightedMultipleDetails
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
            this.WeightedMultipleGroup = new System.Windows.Forms.GroupBox();
            this.DeleteItemButton = new System.Windows.Forms.Button();
            this.AddItemButton = new System.Windows.Forms.Button();
            this.WeightedMultiChoiceView = new System.Windows.Forms.DataGridView();
            this.WeightColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ChoiceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.WeightedMultipleGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.WeightedMultiChoiceView)).BeginInit();
            this.SuspendLayout();
            // 
            // WeightedMultipleGroup
            // 
            this.WeightedMultipleGroup.Controls.Add(this.DeleteItemButton);
            this.WeightedMultipleGroup.Controls.Add(this.AddItemButton);
            this.WeightedMultipleGroup.Controls.Add(this.WeightedMultiChoiceView);
            this.WeightedMultipleGroup.Location = new System.Drawing.Point(3, 3);
            this.WeightedMultipleGroup.Name = "WeightedMultipleGroup";
            this.WeightedMultipleGroup.Size = new System.Drawing.Size(220, 263);
            this.WeightedMultipleGroup.TabIndex = 0;
            this.WeightedMultipleGroup.TabStop = false;
            this.WeightedMultipleGroup.Text = "Weighted Multiple Choice";
            // 
            // DeleteItemButton
            // 
            this.DeleteItemButton.Location = new System.Drawing.Point(125, 234);
            this.DeleteItemButton.Name = "DeleteItemButton";
            this.DeleteItemButton.Size = new System.Drawing.Size(75, 23);
            this.DeleteItemButton.TabIndex = 2;
            this.DeleteItemButton.Text = "Delete Item";
            this.DeleteItemButton.UseVisualStyleBackColor = true;
            this.DeleteItemButton.Click += new System.EventHandler(this.DeleteItemButton_Click);
            // 
            // AddItemButton
            // 
            this.AddItemButton.Location = new System.Drawing.Point(20, 234);
            this.AddItemButton.Name = "AddItemButton";
            this.AddItemButton.Size = new System.Drawing.Size(75, 23);
            this.AddItemButton.TabIndex = 1;
            this.AddItemButton.Text = "Add Item";
            this.AddItemButton.UseVisualStyleBackColor = true;
            this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
            // 
            // WeightedMultiChoiceView
            // 
            this.WeightedMultiChoiceView.AllowUserToAddRows = false;
            this.WeightedMultiChoiceView.AllowUserToDeleteRows = false;
            this.WeightedMultiChoiceView.AllowUserToResizeColumns = false;
            this.WeightedMultiChoiceView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.WeightedMultiChoiceView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.WeightColumn,
            this.ChoiceColumn});
            this.WeightedMultiChoiceView.Location = new System.Drawing.Point(6, 19);
            this.WeightedMultiChoiceView.MultiSelect = false;
            this.WeightedMultiChoiceView.Name = "WeightedMultiChoiceView";
            this.WeightedMultiChoiceView.RowHeadersVisible = false;
            this.WeightedMultiChoiceView.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.WeightedMultiChoiceView.Size = new System.Drawing.Size(208, 209);
            this.WeightedMultiChoiceView.TabIndex = 0;
            this.WeightedMultiChoiceView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.WeightedMultiChoiceView_CellEndEdit);
            // 
            // WeightColumn
            // 
            this.WeightColumn.HeaderText = "Weight";
            this.WeightColumn.Name = "WeightColumn";
            this.WeightColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.WeightColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.WeightColumn.Width = 50;
            // 
            // ChoiceColumn
            // 
            this.ChoiceColumn.HeaderText = "Choice";
            this.ChoiceColumn.Name = "ChoiceColumn";
            this.ChoiceColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ChoiceColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ChoiceColumn.Width = 155;
            // 
            // WeightedMultipleDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.WeightedMultipleGroup);
            this.Name = "WeightedMultipleDetails";
            this.Size = new System.Drawing.Size(226, 269);
            this.ParentChanged += new System.EventHandler(this.WeightedMultipleDetails_ParentChanged);
            this.WeightedMultipleGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.WeightedMultiChoiceView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox WeightedMultipleGroup;
        private System.Windows.Forms.DataGridView WeightedMultiChoiceView;
        private System.Windows.Forms.Button DeleteItemButton;
        private System.Windows.Forms.Button AddItemButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn WeightColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChoiceColumn;
    }
}
