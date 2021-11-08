namespace IATClient
{
    partial class RetrieveItemSlidesForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RetrieveItemSlidesForm));
            this.Instructions = new System.Windows.Forms.TextBox();
            this.IATNameLabel = new System.Windows.Forms.Label();
            this.IATNameBox = new System.Windows.Forms.TextBox();
            this.PasswordLabel = new System.Windows.Forms.Label();
            this.PasswordBox = new System.Windows.Forms.TextBox();
            this.RetrieveButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Instructions
            // 
            this.Instructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Instructions.Location = new System.Drawing.Point(12, 12);
            this.Instructions.Multiline = true;
            this.Instructions.Name = "Instructions";
            this.Instructions.ReadOnly = true;
            this.Instructions.Size = new System.Drawing.Size(320, 57);
            this.Instructions.TabIndex = 0;
            this.Instructions.TabStop = false;
            this.Instructions.Text = resources.GetString("Instructions.Text");
            // 
            // IATNameLabel
            // 
            this.IATNameLabel.AutoSize = true;
            this.IATNameLabel.Location = new System.Drawing.Point(90, 85);
            this.IATNameLabel.Name = "IATNameLabel";
            this.IATNameLabel.Size = new System.Drawing.Size(58, 13);
            this.IATNameLabel.TabIndex = 0;
            this.IATNameLabel.Text = "IAT Name:";
            // 
            // IATNameBox
            // 
            this.IATNameBox.Location = new System.Drawing.Point(154, 82);
            this.IATNameBox.Name = "IATNameBox";
            this.IATNameBox.Size = new System.Drawing.Size(123, 20);
            this.IATNameBox.TabIndex = 1;
            // 
            // PasswordLabel
            // 
            this.PasswordLabel.AutoSize = true;
            this.PasswordLabel.Location = new System.Drawing.Point(92, 111);
            this.PasswordLabel.Name = "PasswordLabel";
            this.PasswordLabel.Size = new System.Drawing.Size(56, 13);
            this.PasswordLabel.TabIndex = 0;
            this.PasswordLabel.Text = "Password:";
            // 
            // PasswordBox
            // 
            this.PasswordBox.Location = new System.Drawing.Point(154, 108);
            this.PasswordBox.Name = "PasswordBox";
            this.PasswordBox.Size = new System.Drawing.Size(123, 20);
            this.PasswordBox.TabIndex = 2;
            // 
            // RetrieveButton
            // 
            this.RetrieveButton.Location = new System.Drawing.Point(70, 142);
            this.RetrieveButton.Name = "RetrieveButton";
            this.RetrieveButton.Size = new System.Drawing.Size(75, 23);
            this.RetrieveButton.TabIndex = 3;
            this.RetrieveButton.Text = "Retrieve";
            this.RetrieveButton.UseVisualStyleBackColor = true;
            this.RetrieveButton.Click += new System.EventHandler(this.RetrieveButton_Click);
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(200, 142);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 4;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // RetrieveItemSlidesForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(344, 177);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.RetrieveButton);
            this.Controls.Add(this.PasswordBox);
            this.Controls.Add(this.PasswordLabel);
            this.Controls.Add(this.IATNameBox);
            this.Controls.Add(this.IATNameLabel);
            this.Controls.Add(this.Instructions);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.Fixed3D;
            this.Name = "RetrieveItemSlidesForm";
            this.Text = "Retrieve Item Slides";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Instructions;
        private System.Windows.Forms.Label IATNameLabel;
        private System.Windows.Forms.TextBox IATNameBox;
        private System.Windows.Forms.Label PasswordLabel;
        private System.Windows.Forms.TextBox PasswordBox;
        private System.Windows.Forms.Button RetrieveButton;
        private System.Windows.Forms.Button CancelButton;
    }
}