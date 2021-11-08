namespace IATClient
{
    partial class TestPackagerPanel
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
            this.TestPackagerGroup = new System.Windows.Forms.GroupBox();
            this.ServerURL = new System.Windows.Forms.TextBox();
            this.ServerURLLabel = new System.Windows.Forms.Label();
            this.RedirectEdit = new System.Windows.Forms.TextBox();
            this.RedirectLabel = new System.Windows.Forms.Label();
            this.PasswordEdit = new System.Windows.Forms.TextBox();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.NumPresentationsGrid = new System.Windows.Forms.DataGridView();
            this.BlockName = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.NumPresentations = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.SetPresentationsRadio = new System.Windows.Forms.RadioButton();
            this.RandomRadio = new System.Windows.Forms.RadioButton();
            this.OrderedRadio = new System.Windows.Forms.RadioButton();
            this.RandomizationOptionsLabel = new System.Windows.Forms.Label();
            this.RightResponseKey = new System.Windows.Forms.ComboBox();
            this.RightResponseKeyLabel = new System.Windows.Forms.Label();
            this.LeftResponseKey = new System.Windows.Forms.ComboBox();
            this.LeftResponseKeyLabel = new System.Windows.Forms.Label();
            this.PackageButton = new System.Windows.Forms.Button();
            this.IATNameEdit = new System.Windows.Forms.TextBox();
            this.IATNameLabel = new System.Windows.Forms.Label();
            this.UploadButton = new System.Windows.Forms.Button();
            this.TestPackagerGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumPresentationsGrid)).BeginInit();
            this.SuspendLayout();
            // 
            // TestPackagerGroup
            // 
            this.TestPackagerGroup.Controls.Add(this.UploadButton);
            this.TestPackagerGroup.Controls.Add(this.ServerURL);
            this.TestPackagerGroup.Controls.Add(this.ServerURLLabel);
            this.TestPackagerGroup.Controls.Add(this.RedirectEdit);
            this.TestPackagerGroup.Controls.Add(this.RedirectLabel);
            this.TestPackagerGroup.Controls.Add(this.PasswordEdit);
            this.TestPackagerGroup.Controls.Add(this.PasswordLabel);
            this.TestPackagerGroup.Controls.Add(this.NumPresentationsGrid);
            this.TestPackagerGroup.Controls.Add(this.SetPresentationsRadio);
            this.TestPackagerGroup.Controls.Add(this.RandomRadio);
            this.TestPackagerGroup.Controls.Add(this.OrderedRadio);
            this.TestPackagerGroup.Controls.Add(this.RandomizationOptionsLabel);
            this.TestPackagerGroup.Controls.Add(this.RightResponseKey);
            this.TestPackagerGroup.Controls.Add(this.RightResponseKeyLabel);
            this.TestPackagerGroup.Controls.Add(this.LeftResponseKey);
            this.TestPackagerGroup.Controls.Add(this.LeftResponseKeyLabel);
            this.TestPackagerGroup.Controls.Add(this.PackageButton);
            this.TestPackagerGroup.Controls.Add(this.IATNameEdit);
            this.TestPackagerGroup.Controls.Add(this.IATNameLabel);
            this.TestPackagerGroup.Dock = System.Windows.Forms.DockStyle.Fill;
            this.TestPackagerGroup.Location = new System.Drawing.Point(0, 0);
            this.TestPackagerGroup.Name = "TestPackagerGroup";
            this.TestPackagerGroup.Size = new System.Drawing.Size(401, 452);
            this.TestPackagerGroup.TabIndex = 0;
            this.TestPackagerGroup.TabStop = false;
            this.TestPackagerGroup.Text = "Package Test";
            // 
            // ServerURL
            // 
            this.ServerURL.Location = new System.Drawing.Point(80, 182);
            this.ServerURL.Name = "ServerURL";
            this.ServerURL.Size = new System.Drawing.Size(281, 20);
            this.ServerURL.TabIndex = 21;
            // 
            // ServerURLLabel
            // 
            this.ServerURLLabel.AutoSize = true;
            this.ServerURLLabel.Location = new System.Drawing.Point(8, 185);
            this.ServerURLLabel.Name = "ServerURLLabel";
            this.ServerURLLabel.Size = new System.Drawing.Size(66, 13);
            this.ServerURLLabel.TabIndex = 20;
            this.ServerURLLabel.Text = "Server URL:";
            // 
            // RedirectEdit
            // 
            this.RedirectEdit.Location = new System.Drawing.Point(63, 152);
            this.RedirectEdit.Name = "RedirectEdit";
            this.RedirectEdit.Size = new System.Drawing.Size(298, 20);
            this.RedirectEdit.TabIndex = 19;
            this.RedirectEdit.TextChanged += new System.EventHandler(this.RedirectEdit_TextChanged);
            // 
            // RedirectLabel
            // 
            this.RedirectLabel.AutoSize = true;
            this.RedirectLabel.Location = new System.Drawing.Point(8, 136);
            this.RedirectLabel.Name = "RedirectLabel";
            this.RedirectLabel.Size = new System.Drawing.Size(294, 13);
            this.RedirectLabel.TabIndex = 18;
            this.RedirectLabel.Text = "URL to redirect testee to upon completion of IAT and survey:";
            // 
            // PasswordEdit
            // 
            this.PasswordEdit.Location = new System.Drawing.Point(223, 113);
            this.PasswordEdit.Name = "PasswordEdit";
            this.PasswordEdit.Size = new System.Drawing.Size(100, 20);
            this.PasswordEdit.TabIndex = 17;
            this.PasswordEdit.TextChanged += new System.EventHandler(this.PasswordEdit_TextChanged);
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Location = new System.Drawing.Point(82, 116);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(127, 13);
            this.PasswordLabel.TabIndex = 16;
            this.PasswordLabel.Text = "Data Retrieval Password:";
            // 
            // NumPresentationsGrid
            // 
            this.NumPresentationsGrid.AllowUserToAddRows = false;
            this.NumPresentationsGrid.AllowUserToDeleteRows = false;
            this.NumPresentationsGrid.AllowUserToResizeColumns = false;
            this.NumPresentationsGrid.AllowUserToResizeRows = false;
            this.NumPresentationsGrid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.NumPresentationsGrid.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.BlockName,
            this.NumPresentations});
            this.NumPresentationsGrid.Location = new System.Drawing.Point(81, 254);
            this.NumPresentationsGrid.Name = "NumPresentationsGrid";
            this.NumPresentationsGrid.RowHeadersVisible = false;
            this.NumPresentationsGrid.Size = new System.Drawing.Size(238, 150);
            this.NumPresentationsGrid.TabIndex = 15;
            this.NumPresentationsGrid.CellEndEdit += new System.Windows.Forms.DataGridViewCellEventHandler(this.NumPresentationsGrid_CellEndEdit);
            // 
            // BlockName
            // 
            this.BlockName.Frozen = true;
            this.BlockName.HeaderText = "IAT Block Name";
            this.BlockName.Name = "BlockName";
            this.BlockName.ReadOnly = true;
            this.BlockName.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.BlockName.Width = 115;
            // 
            // NumPresentations
            // 
            this.NumPresentations.Frozen = true;
            this.NumPresentations.HeaderText = "# of Presentations";
            this.NumPresentations.Name = "NumPresentations";
            this.NumPresentations.Resizable = System.Windows.Forms.DataGridViewTriState.False;
            this.NumPresentations.Width = 120;
            // 
            // SetPresentationsRadio
            // 
            this.SetPresentationsRadio.AutoSize = true;
            this.SetPresentationsRadio.Location = new System.Drawing.Point(205, 231);
            this.SetPresentationsRadio.Name = "SetPresentationsRadio";
            this.SetPresentationsRadio.Size = new System.Drawing.Size(130, 17);
            this.SetPresentationsRadio.TabIndex = 14;
            this.SetPresentationsRadio.TabStop = true;
            this.SetPresentationsRadio.Text = "Set # of Presentations";
            this.SetPresentationsRadio.UseVisualStyleBackColor = true;
            this.SetPresentationsRadio.CheckedChanged += new System.EventHandler(this.SetPresentationsRadio_CheckedChanged);
            // 
            // RandomRadio
            // 
            this.RandomRadio.AutoSize = true;
            this.RandomRadio.Location = new System.Drawing.Point(134, 231);
            this.RandomRadio.Name = "RandomRadio";
            this.RandomRadio.Size = new System.Drawing.Size(65, 17);
            this.RandomRadio.TabIndex = 13;
            this.RandomRadio.TabStop = true;
            this.RandomRadio.Text = "Random";
            this.RandomRadio.UseVisualStyleBackColor = true;
            this.RandomRadio.CheckedChanged += new System.EventHandler(this.RandomRadio_CheckedChanged);
            // 
            // OrderedRadio
            // 
            this.OrderedRadio.AutoSize = true;
            this.OrderedRadio.Location = new System.Drawing.Point(65, 231);
            this.OrderedRadio.Name = "OrderedRadio";
            this.OrderedRadio.Size = new System.Drawing.Size(63, 17);
            this.OrderedRadio.TabIndex = 12;
            this.OrderedRadio.TabStop = true;
            this.OrderedRadio.Text = "Ordered";
            this.OrderedRadio.UseVisualStyleBackColor = true;
            this.OrderedRadio.CheckedChanged += new System.EventHandler(this.OrderedRadio_CheckedChanged);
            // 
            // RandomizationOptionsLabel
            // 
            this.RandomizationOptionsLabel.AutoSize = true;
            this.RandomizationOptionsLabel.Location = new System.Drawing.Point(6, 215);
            this.RandomizationOptionsLabel.Name = "RandomizationOptionsLabel";
            this.RandomizationOptionsLabel.Size = new System.Drawing.Size(119, 13);
            this.RandomizationOptionsLabel.TabIndex = 11;
            this.RandomizationOptionsLabel.Text = "Randomization Options:";
            // 
            // RightResponseKey
            // 
            this.RightResponseKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RightResponseKey.FormattingEnabled = true;
            this.RightResponseKey.Location = new System.Drawing.Point(323, 79);
            this.RightResponseKey.Name = "RightResponseKey";
            this.RightResponseKey.Size = new System.Drawing.Size(39, 21);
            this.RightResponseKey.TabIndex = 10;
            this.RightResponseKey.SelectedIndexChanged += new System.EventHandler(this.RightResponseKey_SelectedIndexChanged);
            // 
            // RightResponseKeyLabel
            // 
            this.RightResponseKeyLabel.AutoSize = true;
            this.RightResponseKeyLabel.Location = new System.Drawing.Point(213, 82);
            this.RightResponseKeyLabel.Name = "RightResponseKeyLabel";
            this.RightResponseKeyLabel.Size = new System.Drawing.Size(107, 13);
            this.RightResponseKeyLabel.TabIndex = 9;
            this.RightResponseKeyLabel.Text = "Right Response Key:";
            // 
            // LeftResponseKey
            // 
            this.LeftResponseKey.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LeftResponseKey.FormattingEnabled = true;
            this.LeftResponseKey.Location = new System.Drawing.Point(148, 79);
            this.LeftResponseKey.Name = "LeftResponseKey";
            this.LeftResponseKey.Size = new System.Drawing.Size(39, 21);
            this.LeftResponseKey.TabIndex = 8;
            this.LeftResponseKey.SelectedIndexChanged += new System.EventHandler(this.LeftResponseKey_SelectedIndexChanged);
            // 
            // LeftResponseKeyLabel
            // 
            this.LeftResponseKeyLabel.AutoSize = true;
            this.LeftResponseKeyLabel.Location = new System.Drawing.Point(42, 82);
            this.LeftResponseKeyLabel.Name = "LeftResponseKeyLabel";
            this.LeftResponseKeyLabel.Size = new System.Drawing.Size(100, 13);
            this.LeftResponseKeyLabel.TabIndex = 7;
            this.LeftResponseKeyLabel.Text = "Left Response Key:";
            // 
            // PackageButton
            // 
            this.PackageButton.Location = new System.Drawing.Point(81, 423);
            this.PackageButton.Name = "PackageButton";
            this.PackageButton.Size = new System.Drawing.Size(110, 23);
            this.PackageButton.TabIndex = 5;
            this.PackageButton.Text = "Package IAT";
            this.PackageButton.UseVisualStyleBackColor = true;
            this.PackageButton.Click += new System.EventHandler(this.PackageButton_Click);
            // 
            // IATNameEdit
            // 
            this.IATNameEdit.Location = new System.Drawing.Point(100, 19);
            this.IATNameEdit.Name = "IATNameEdit";
            this.IATNameEdit.Size = new System.Drawing.Size(261, 20);
            this.IATNameEdit.TabIndex = 1;
            this.IATNameEdit.TextChanged += new System.EventHandler(this.IATNameEdit_TextChanged);
            // 
            // IATNameLabel
            // 
            this.IATNameLabel.AutoSize = true;
            this.IATNameLabel.Location = new System.Drawing.Point(36, 22);
            this.IATNameLabel.Name = "IATNameLabel";
            this.IATNameLabel.Size = new System.Drawing.Size(58, 13);
            this.IATNameLabel.TabIndex = 0;
            this.IATNameLabel.Text = "IAT Name:";
            // 
            // UploadButton
            // 
            this.UploadButton.Location = new System.Drawing.Point(209, 423);
            this.UploadButton.Name = "UploadButton";
            this.UploadButton.Size = new System.Drawing.Size(110, 23);
            this.UploadButton.TabIndex = 22;
            this.UploadButton.Text = "Upload IAT";
            this.UploadButton.UseVisualStyleBackColor = true;
            this.UploadButton.Click += new System.EventHandler(this.UploadButton_Click);
            // 
            // TestPackagerPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TestPackagerGroup);
            this.Name = "TestPackagerPanel";
            this.Size = new System.Drawing.Size(401, 452);
            this.TestPackagerGroup.ResumeLayout(false);
            this.TestPackagerGroup.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.NumPresentationsGrid)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox TestPackagerGroup;
        private System.Windows.Forms.TextBox IATNameEdit;
        private System.Windows.Forms.Label IATNameLabel;
        private System.Windows.Forms.Button PackageButton;
        private System.Windows.Forms.Label LeftResponseKeyLabel;
        private System.Windows.Forms.RadioButton SetPresentationsRadio;
        private System.Windows.Forms.RadioButton RandomRadio;
        private System.Windows.Forms.RadioButton OrderedRadio;
        private System.Windows.Forms.Label RandomizationOptionsLabel;
        private System.Windows.Forms.ComboBox RightResponseKey;
        private System.Windows.Forms.Label RightResponseKeyLabel;
        private System.Windows.Forms.ComboBox LeftResponseKey;
        private System.Windows.Forms.DataGridView NumPresentationsGrid;
        private System.Windows.Forms.DataGridViewTextBoxColumn BlockName;
        private System.Windows.Forms.DataGridViewTextBoxColumn NumPresentations;
        private System.Windows.Forms.TextBox PasswordEdit;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.TextBox RedirectEdit;
        private System.Windows.Forms.Label RedirectLabel;
        private System.Windows.Forms.TextBox ServerURL;
        private System.Windows.Forms.Label ServerURLLabel;
        private System.Windows.Forms.Button UploadButton;
    }
}
