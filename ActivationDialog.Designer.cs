namespace IATClient
{
    partial class ActivationDialog
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.ProductKey = new System.Windows.Forms.TextBox();
            this.ProductKeyLabel = new System.Windows.Forms.Label();
            this.ActivateButton = new System.Windows.Forms.Button();
            this.ActivateMessage = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.FName = new System.Windows.Forms.TextBox();
            this.LName = new System.Windows.Forms.TextBox();
            this.EMail = new System.Windows.Forms.TextBox();
            this.TitleLabel = new System.Windows.Forms.Label();
            this.TitleBox = new System.Windows.Forms.ComboBox();
            this.HaveVerifiedButton = new System.Windows.Forms.Button();
            this.ResendVerificationButton = new System.Windows.Forms.Button();
            this.StatusDetailsButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ProductKey
            // 
            this.ProductKey.Location = new System.Drawing.Point(114, 15);
            this.ProductKey.Name = "ProductKey";
            this.ProductKey.Size = new System.Drawing.Size(198, 20);
            this.ProductKey.TabIndex = 0;
            // 
            // ProductKeyLabel
            // 
            this.ProductKeyLabel.AutoSize = true;
            this.ProductKeyLabel.Location = new System.Drawing.Point(40, 18);
            this.ProductKeyLabel.Name = "ProductKeyLabel";
            this.ProductKeyLabel.Size = new System.Drawing.Size(68, 13);
            this.ProductKeyLabel.TabIndex = 1;
            this.ProductKeyLabel.Text = "Product Key:";
            // 
            // ActivateButton
            // 
            this.ActivateButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.ActivateButton.Location = new System.Drawing.Point(124, 154);
            this.ActivateButton.Name = "ActivateButton";
            this.ActivateButton.Size = new System.Drawing.Size(101, 23);
            this.ActivateButton.TabIndex = 5;
            this.ActivateButton.Text = "Activate";
            this.ActivateButton.UseVisualStyleBackColor = true;
            this.ActivateButton.Click += new System.EventHandler(this.Activate_Click);
            // 
            // ActivateMessage
            // 
            this.ActivateMessage.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.ActivateMessage.AutoSize = true;
            this.ActivateMessage.Location = new System.Drawing.Point(73, 185);
            this.ActivateMessage.Name = "ActivateMessage";
            this.ActivateMessage.Size = new System.Drawing.Size(35, 13);
            this.ActivateMessage.TabIndex = 3;
            this.ActivateMessage.Text = "label1";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(49, 79);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(60, 13);
            this.label1.TabIndex = 4;
            this.label1.Text = "First Name:";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(48, 105);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(61, 13);
            this.label2.TabIndex = 5;
            this.label2.Text = "Last Name:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(31, 131);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(77, 13);
            this.label3.TabIndex = 6;
            this.label3.Text = "EMail Address:";
            // 
            // FName
            // 
            this.FName.Location = new System.Drawing.Point(114, 76);
            this.FName.Name = "FName";
            this.FName.Size = new System.Drawing.Size(198, 20);
            this.FName.TabIndex = 2;
            // 
            // LName
            // 
            this.LName.Location = new System.Drawing.Point(114, 102);
            this.LName.Name = "LName";
            this.LName.Size = new System.Drawing.Size(198, 20);
            this.LName.TabIndex = 3;
            // 
            // EMail
            // 
            this.EMail.Location = new System.Drawing.Point(114, 128);
            this.EMail.Name = "EMail";
            this.EMail.Size = new System.Drawing.Size(198, 20);
            this.EMail.TabIndex = 4;
            // 
            // TitleLabel
            // 
            this.TitleLabel.AutoSize = true;
            this.TitleLabel.Location = new System.Drawing.Point(78, 50);
            this.TitleLabel.Name = "TitleLabel";
            this.TitleLabel.Size = new System.Drawing.Size(30, 13);
            this.TitleLabel.TabIndex = 7;
            this.TitleLabel.Text = "Title:";
            // 
            // TitleBox
            // 
            this.TitleBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.TitleBox.FormattingEnabled = true;
            this.TitleBox.Location = new System.Drawing.Point(114, 47);
            this.TitleBox.Name = "TitleBox";
            this.TitleBox.Size = new System.Drawing.Size(59, 21);
            this.TitleBox.TabIndex = 1;
            // 
            // HaveVerifiedButton
            // 
            this.HaveVerifiedButton.Location = new System.Drawing.Point(81, 212);
            this.HaveVerifiedButton.Name = "HaveVerifiedButton";
            this.HaveVerifiedButton.Size = new System.Drawing.Size(180, 23);
            this.HaveVerifiedButton.TabIndex = 6;
            this.HaveVerifiedButton.Text = "I Have Verified My EMail";
            this.HaveVerifiedButton.UseVisualStyleBackColor = true;
            this.HaveVerifiedButton.Click += new System.EventHandler(this.HaveVerifiedButton_Click);
            // 
            // ResendVerificationButton
            // 
            this.ResendVerificationButton.Location = new System.Drawing.Point(81, 241);
            this.ResendVerificationButton.Name = "ResendVerificationButton";
            this.ResendVerificationButton.Size = new System.Drawing.Size(180, 23);
            this.ResendVerificationButton.TabIndex = 7;
            this.ResendVerificationButton.Text = "Resend Verification EMail";
            this.ResendVerificationButton.UseVisualStyleBackColor = true;
            this.ResendVerificationButton.Click += new System.EventHandler(this.ResendVerificationButton_Click);
            // 
            // StatusDetailsButton
            // 
            this.StatusDetailsButton.ForeColor = System.Drawing.Color.Blue;
            this.StatusDetailsButton.Location = new System.Drawing.Point(313, 180);
            this.StatusDetailsButton.Name = "StatusDetailsButton";
            this.StatusDetailsButton.Size = new System.Drawing.Size(23, 23);
            this.StatusDetailsButton.TabIndex = 8;
            this.StatusDetailsButton.Text = "?";
            this.StatusDetailsButton.UseVisualStyleBackColor = true;
            // 
            // ActivationDialog
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(348, 290);
            this.Controls.Add(this.StatusDetailsButton);
            this.Controls.Add(this.ResendVerificationButton);
            this.Controls.Add(this.HaveVerifiedButton);
            this.Controls.Add(this.TitleBox);
            this.Controls.Add(this.TitleLabel);
            this.Controls.Add(this.EMail);
            this.Controls.Add(this.LName);
            this.Controls.Add(this.FName);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ActivateMessage);
            this.Controls.Add(this.ActivateButton);
            this.Controls.Add(this.ProductKeyLabel);
            this.Controls.Add(this.ProductKey);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "ActivationDialog";
            this.Text = "IAT Client Activation";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox ProductKey;
        private System.Windows.Forms.Label ProductKeyLabel;
        private System.Windows.Forms.Button ActivateButton;
        private System.Windows.Forms.Label ActivateMessage;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox FName;
        private System.Windows.Forms.TextBox LName;
        private System.Windows.Forms.TextBox EMail;
        private System.Windows.Forms.Label TitleLabel;
        private System.Windows.Forms.ComboBox TitleBox;
        private System.Windows.Forms.Button HaveVerifiedButton;
        private System.Windows.Forms.Button ResendVerificationButton;
        private System.Windows.Forms.Button StatusDetailsButton;
    }
}