namespace IATClient
{
    partial class SendConfirmationEMailForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SendConfirmationEMailForm));
            this.Instructions = new System.Windows.Forms.TextBox();
            this.EMialLabel = new System.Windows.Forms.Label();
            this.EMailBox = new System.Windows.Forms.TextBox();
            this.ResendVerification = new System.Windows.Forms.Button();
            this.HaveVerifiedButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Instructions
            // 
            this.Instructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Instructions.Location = new System.Drawing.Point(12, 12);
            this.Instructions.Multiline = true;
            this.Instructions.Name = "Instructions";
            this.Instructions.ReadOnly = true;
            this.Instructions.Size = new System.Drawing.Size(311, 67);
            this.Instructions.TabIndex = 0;
            this.Instructions.TabStop = false;
            this.Instructions.Text = resources.GetString("Instructions.Text");
            // 
            // EMialLabel
            // 
            this.EMialLabel.AutoSize = true;
            this.EMialLabel.Location = new System.Drawing.Point(83, 93);
            this.EMialLabel.Name = "EMialLabel";
            this.EMialLabel.Size = new System.Drawing.Size(36, 13);
            this.EMialLabel.TabIndex = 1;
            this.EMialLabel.Text = "EMail:";
            // 
            // EMailBox
            // 
            this.EMailBox.Location = new System.Drawing.Point(125, 90);
            this.EMailBox.Name = "EMailBox";
            this.EMailBox.Size = new System.Drawing.Size(126, 20);
            this.EMailBox.TabIndex = 2;
            // 
            // ResendVerification
            // 
            this.ResendVerification.Location = new System.Drawing.Point(94, 119);
            this.ResendVerification.Name = "ResendVerification";
            this.ResendVerification.Size = new System.Drawing.Size(147, 23);
            this.ResendVerification.TabIndex = 3;
            this.ResendVerification.Text = "Resend Verification Email";
            this.ResendVerification.UseVisualStyleBackColor = true;
            this.ResendVerification.Click += new System.EventHandler(this.ResendVerification_Click);
            // 
            // HaveVerifiedButton
            // 
            this.HaveVerifiedButton.Location = new System.Drawing.Point(94, 148);
            this.HaveVerifiedButton.Name = "HaveVerifiedButton";
            this.HaveVerifiedButton.Size = new System.Drawing.Size(147, 23);
            this.HaveVerifiedButton.TabIndex = 4;
            this.HaveVerifiedButton.Text = "I Have Verified My Email";
            this.HaveVerifiedButton.UseVisualStyleBackColor = true;
            this.HaveVerifiedButton.Click += new System.EventHandler(this.HaveVerifiedButton_Click);
            // 
            // SendConfirmationEMailForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(335, 178);
            this.Controls.Add(this.HaveVerifiedButton);
            this.Controls.Add(this.ResendVerification);
            this.Controls.Add(this.EMailBox);
            this.Controls.Add(this.EMialLabel);
            this.Controls.Add(this.Instructions);
            this.Name = "SendConfirmationEMailForm";
            this.Text = "Resend Confirmation EMail";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Instructions;
        private System.Windows.Forms.Label EMialLabel;
        private System.Windows.Forms.TextBox EMailBox;
        private System.Windows.Forms.Button ResendVerification;
        private System.Windows.Forms.Button HaveVerifiedButton;
    }
}