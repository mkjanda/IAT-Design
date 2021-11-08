namespace IATClient
{
    partial class OrderPanel
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
            this.PurchaseLabel = new System.Windows.Forms.Label();
            this.AdministrationsLabel = new System.Windows.Forms.Label();
            this.DiskSpaceLabel = new System.Windows.Forms.Label();
            this.IATsLabel = new System.Windows.Forms.Label();
            this.AdministrationsDrop = new System.Windows.Forms.ComboBox();
            this.DiskSpaceDrop = new System.Windows.Forms.ComboBox();
            this.IATsDrop = new System.Windows.Forms.ComboBox();
            this.TotalLabel = new System.Windows.Forms.Label();
            this.TotalBox = new System.Windows.Forms.TextBox();
            this.PurchaseButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.PurchaseDisclaimer = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // PurchaseLabel
            // 
            this.PurchaseLabel.AutoSize = true;
            this.PurchaseLabel.Location = new System.Drawing.Point(3, 5);
            this.PurchaseLabel.Name = "PurchaseLabel";
            this.PurchaseLabel.Size = new System.Drawing.Size(109, 13);
            this.PurchaseLabel.TabIndex = 0;
            this.PurchaseLabel.Text = "Purchase Resources:";
            // 
            // AdministrationsLabel
            // 
            this.AdministrationsLabel.AutoSize = true;
            this.AdministrationsLabel.Location = new System.Drawing.Point(28, 27);
            this.AdministrationsLabel.Name = "AdministrationsLabel";
            this.AdministrationsLabel.Size = new System.Drawing.Size(85, 13);
            this.AdministrationsLabel.TabIndex = 1;
            this.AdministrationsLabel.Text = "Administrations x";
            // 
            // DiskSpaceLabel
            // 
            this.DiskSpaceLabel.AutoSize = true;
            this.DiskSpaceLabel.Location = new System.Drawing.Point(43, 57);
            this.DiskSpaceLabel.Name = "DiskSpaceLabel";
            this.DiskSpaceLabel.Size = new System.Drawing.Size(70, 13);
            this.DiskSpaceLabel.TabIndex = 2;
            this.DiskSpaceLabel.Text = "Disk Space x";
            // 
            // IATsLabel
            // 
            this.IATsLabel.AutoSize = true;
            this.IATsLabel.Location = new System.Drawing.Point(76, 87);
            this.IATsLabel.Name = "IATsLabel";
            this.IATsLabel.Size = new System.Drawing.Size(37, 13);
            this.IATsLabel.TabIndex = 3;
            this.IATsLabel.Text = "IATs x";
            // 
            // AdministrationsDrop
            // 
            this.AdministrationsDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.AdministrationsDrop.FormattingEnabled = true;
            this.AdministrationsDrop.Location = new System.Drawing.Point(119, 24);
            this.AdministrationsDrop.Name = "AdministrationsDrop";
            this.AdministrationsDrop.Size = new System.Drawing.Size(67, 21);
            this.AdministrationsDrop.TabIndex = 4;
            // 
            // DiskSpaceDrop
            // 
            this.DiskSpaceDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.DiskSpaceDrop.FormattingEnabled = true;
            this.DiskSpaceDrop.Location = new System.Drawing.Point(119, 54);
            this.DiskSpaceDrop.Name = "DiskSpaceDrop";
            this.DiskSpaceDrop.Size = new System.Drawing.Size(67, 21);
            this.DiskSpaceDrop.TabIndex = 5;
            // 
            // IATsDrop
            // 
            this.IATsDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.IATsDrop.FormattingEnabled = true;
            this.IATsDrop.Location = new System.Drawing.Point(119, 84);
            this.IATsDrop.Name = "IATsDrop";
            this.IATsDrop.Size = new System.Drawing.Size(67, 21);
            this.IATsDrop.TabIndex = 6;
            // 
            // TotalLabel
            // 
            this.TotalLabel.AutoSize = true;
            this.TotalLabel.Location = new System.Drawing.Point(67, 164);
            this.TotalLabel.Name = "TotalLabel";
            this.TotalLabel.Size = new System.Drawing.Size(34, 13);
            this.TotalLabel.TabIndex = 7;
            this.TotalLabel.Text = "Total:";
            // 
            // TotalBox
            // 
            this.TotalBox.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.TotalBox.Location = new System.Drawing.Point(107, 161);
            this.TotalBox.Name = "TotalBox";
            this.TotalBox.ReadOnly = true;
            this.TotalBox.Size = new System.Drawing.Size(79, 20);
            this.TotalBox.TabIndex = 8;
            // 
            // PurchaseButton
            // 
            this.PurchaseButton.Location = new System.Drawing.Point(16, 211);
            this.PurchaseButton.Name = "PurchaseButton";
            this.PurchaseButton.Size = new System.Drawing.Size(75, 23);
            this.PurchaseButton.TabIndex = 9;
            this.PurchaseButton.Text = "Purchase";
            this.PurchaseButton.UseVisualStyleBackColor = true;
            this.PurchaseButton.Click += new System.EventHandler(this.PurchaseButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(107, 211);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 10;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // PurchaseDisclaimer
            // 
            this.PurchaseDisclaimer.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.PurchaseDisclaimer.Location = new System.Drawing.Point(16, 250);
            this.PurchaseDisclaimer.Multiline = true;
            this.PurchaseDisclaimer.Name = "PurchaseDisclaimer";
            this.PurchaseDisclaimer.ReadOnly = true;
            this.PurchaseDisclaimer.Size = new System.Drawing.Size(166, 119);
            this.PurchaseDisclaimer.TabIndex = 11;
            this.PurchaseDisclaimer.Text = "Your credit card information is not kept on file and clicking the Purchase button" +
    " above will not finalize your order.";
            // 
            // OrderPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.PurchaseDisclaimer);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.PurchaseButton);
            this.Controls.Add(this.TotalBox);
            this.Controls.Add(this.TotalLabel);
            this.Controls.Add(this.IATsDrop);
            this.Controls.Add(this.DiskSpaceDrop);
            this.Controls.Add(this.AdministrationsDrop);
            this.Controls.Add(this.IATsLabel);
            this.Controls.Add(this.DiskSpaceLabel);
            this.Controls.Add(this.AdministrationsLabel);
            this.Controls.Add(this.PurchaseLabel);
            this.Name = "OrderPanel";
            this.Size = new System.Drawing.Size(200, 500);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label PurchaseLabel;
        private System.Windows.Forms.Label AdministrationsLabel;
        private System.Windows.Forms.Label DiskSpaceLabel;
        private System.Windows.Forms.Label IATsLabel;
        private System.Windows.Forms.ComboBox AdministrationsDrop;
        private System.Windows.Forms.ComboBox DiskSpaceDrop;
        private System.Windows.Forms.ComboBox IATsDrop;
        private System.Windows.Forms.Label TotalLabel;
        private System.Windows.Forms.TextBox TotalBox;
        private System.Windows.Forms.Button PurchaseButton;
        private System.Windows.Forms.Button CancelButton;
        private System.Windows.Forms.TextBox PurchaseDisclaimer;
    }
}
