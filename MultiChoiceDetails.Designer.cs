namespace IATClient
{
    partial class MultiChoiceDetails
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
            this.MultiChoiceGroup = new System.Windows.Forms.GroupBox();
            this.DeleteItemButton = new System.Windows.Forms.Button();
            this.AddItemButton = new System.Windows.Forms.Button();
            this.ChoiceView = new System.Windows.Forms.DataGridView();
            this.ChoiceColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.MultiChoiceGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.ChoiceView)).BeginInit();
            this.SuspendLayout();
            // 
            // MultiChoiceGroup
            // 
            this.MultiChoiceGroup.Controls.Add(this.DeleteItemButton);
            this.MultiChoiceGroup.Controls.Add(this.AddItemButton);
            this.MultiChoiceGroup.Controls.Add(this.ChoiceView);
            this.MultiChoiceGroup.Location = new System.Drawing.Point(3, 3);
            this.MultiChoiceGroup.Name = "MultiChoiceGroup";
            this.MultiChoiceGroup.Size = new System.Drawing.Size(220, 233);
            this.MultiChoiceGroup.TabIndex = 0;
            this.MultiChoiceGroup.TabStop = false;
            this.MultiChoiceGroup.Text = "Multiple Choice";
            // 
            // DeleteItemButton
            // 
            this.DeleteItemButton.Location = new System.Drawing.Point(125, 202);
            this.DeleteItemButton.Name = "DeleteItemButton";
            this.DeleteItemButton.Size = new System.Drawing.Size(75, 23);
            this.DeleteItemButton.TabIndex = 2;
            this.DeleteItemButton.Text = "Delete Item";
            this.DeleteItemButton.UseVisualStyleBackColor = true;
            this.DeleteItemButton.Click += new System.EventHandler(this.DeleteItemButton_Click);
            // 
            // AddItemButton
            // 
            this.AddItemButton.Location = new System.Drawing.Point(20, 202);
            this.AddItemButton.Name = "AddItemButton";
            this.AddItemButton.Size = new System.Drawing.Size(75, 23);
            this.AddItemButton.TabIndex = 1;
            this.AddItemButton.Text = "Add Item";
            this.AddItemButton.UseVisualStyleBackColor = true;
            this.AddItemButton.Click += new System.EventHandler(this.AddItemButton_Click);
            // 
            // ChoiceView
            // 
            this.ChoiceView.AllowUserToAddRows = false;
            this.ChoiceView.AllowUserToDeleteRows = false;
            this.ChoiceView.AllowUserToResizeColumns = false;
            this.ChoiceView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.ChoiceView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ChoiceColumn});
            this.ChoiceView.Location = new System.Drawing.Point(6, 19);
            this.ChoiceView.MultiSelect = false;
            this.ChoiceView.Name = "ChoiceView";
            this.ChoiceView.RowHeadersVisible = false;
            this.ChoiceView.Size = new System.Drawing.Size(208, 177);
            this.ChoiceView.TabIndex = 0;
            this.ChoiceView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.ChoiceView_CellEndEdit);
            // 
            // ChoiceColumn
            // 
            this.ChoiceColumn.HeaderText = "Choices";
            this.ChoiceColumn.Name = "ChoiceColumn";
            this.ChoiceColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ChoiceColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ChoiceColumn.Width = 205;
            // 
            // MultiChoiceDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MultiChoiceGroup);
            this.Name = "MultiChoiceDetails";
            this.Size = new System.Drawing.Size(226, 241);
            this.Load += new System.EventHandler(this.MultiChoiceDetails_Load);
            this.ParentChanged += new System.EventHandler(this.MultiChoiceDetails_ParentChanged);
            this.MultiChoiceGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.ChoiceView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox MultiChoiceGroup;
        private System.Windows.Forms.DataGridView ChoiceView;
        private System.Windows.Forms.Button DeleteItemButton;
        private System.Windows.Forms.Button AddItemButton;
        private System.Windows.Forms.DataGridViewTextBoxColumn ChoiceColumn;
    }
}
