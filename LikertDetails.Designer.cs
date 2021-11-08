namespace IATClient
{
    partial class LikertDetails
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
            this.LikertGroup = new System.Windows.Forms.GroupBox();
            this.LikertView = new System.Windows.Forms.DataGridView();
            this.ValueColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.StatementColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReverseScoredCheck = new System.Windows.Forms.CheckBox();
            this.DeleteItemButton = new System.Windows.Forms.Button();
            this.AddItemButton = new System.Windows.Forms.Button();
            this.LikertGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LikertView)).BeginInit();
            this.SuspendLayout();
            // 
            // LikertGroup
            // 
            this.LikertGroup.Controls.Add(this.LikertView);
            this.LikertGroup.Controls.Add(this.ReverseScoredCheck);
            this.LikertGroup.Controls.Add(this.DeleteItemButton);
            this.LikertGroup.Controls.Add(this.AddItemButton);
            this.LikertGroup.Location = new System.Drawing.Point(3, 3);
            this.LikertGroup.Name = "LikertGroup";
            this.LikertGroup.Size = new System.Drawing.Size(220, 263);
            this.LikertGroup.TabIndex = 0;
            this.LikertGroup.TabStop = false;
            this.LikertGroup.Text = "Likert";
            // 
            // LikertView
            // 
            this.LikertView.AllowUserToAddRows = false;
            this.LikertView.AllowUserToDeleteRows = false;
            this.LikertView.AllowUserToResizeColumns = false;
            this.LikertView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.LikertView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ValueColumn,
            this.StatementColumn});
            this.LikertView.Location = new System.Drawing.Point(6, 20);
            this.LikertView.MultiSelect = false;
            this.LikertView.Name = "LikertView";
            this.LikertView.RowHeadersVisible = false;
            this.LikertView.Size = new System.Drawing.Size(208, 177);
            this.LikertView.TabIndex = 4;
            this.LikertView.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.LikertView_CellEndEdit);
            // 
            // ValueColumn
            // 
            this.ValueColumn.HeaderText = "Value";
            this.ValueColumn.Name = "ValueColumn";
            this.ValueColumn.ReadOnly = true;
            this.ValueColumn.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.ValueColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.ValueColumn.Width = 40;
            // 
            // StatementColumn
            // 
            this.StatementColumn.HeaderText = "Statement";
            this.StatementColumn.Name = "StatementColumn";
            this.StatementColumn.SortMode = System.Windows.Forms.DataGridViewColumnSortMode.NotSortable;
            this.StatementColumn.Width = 165;
            // 
            // ReverseScoredCheck
            // 
            this.ReverseScoredCheck.AutoSize = true;
            this.ReverseScoredCheck.Location = new System.Drawing.Point(59, 232);
            this.ReverseScoredCheck.Name = "ReverseScoredCheck";
            this.ReverseScoredCheck.Size = new System.Drawing.Size(103, 17);
            this.ReverseScoredCheck.TabIndex = 3;
            this.ReverseScoredCheck.Text = "Reverse Scored";
            this.ReverseScoredCheck.UseVisualStyleBackColor = true;
            this.ReverseScoredCheck.CheckedChanged += new System.EventHandler(this.ReverseScoredCheck_CheckedChanged);
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
            // LikertDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.LikertGroup);
            this.Name = "LikertDetails";
            this.Size = new System.Drawing.Size(226, 269);
            this.Load += new System.EventHandler(this.LikertDetails_Load);
            this.ParentChanged += new System.EventHandler(this.LikertDetails_ParentChanged);
            this.LikertGroup.ResumeLayout(false);
            this.LikertGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.LikertView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox LikertGroup;
        private System.Windows.Forms.CheckBox ReverseScoredCheck;
        private System.Windows.Forms.Button DeleteItemButton;
        private System.Windows.Forms.Button AddItemButton;
        private System.Windows.Forms.DataGridView LikertView;
        private System.Windows.Forms.DataGridViewTextBoxColumn ValueColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn StatementColumn;
    }
}
