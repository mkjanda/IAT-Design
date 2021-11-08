namespace IATClient
{
    partial class BoundedLengthDeleteConfirmation
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BoundedLengthDeleteConfirmation));
            this.Instructions = new System.Windows.Forms.TextBox();
            this.ProceedButton = new System.Windows.Forms.Button();
            this.Cancel = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // Instructions
            // 
            this.Instructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Instructions.Location = new System.Drawing.Point(12, 12);
            this.Instructions.Multiline = true;
            this.Instructions.Name = "Instructions";
            this.Instructions.ReadOnly = true;
            this.Instructions.Size = new System.Drawing.Size(397, 60);
            this.Instructions.TabIndex = 0;
            this.Instructions.Text = resources.GetString("Instructions.Text");
            // 
            // ProceedButton
            // 
            this.ProceedButton.Location = new System.Drawing.Point(104, 407);
            this.ProceedButton.Name = "ProceedButton";
            this.ProceedButton.Size = new System.Drawing.Size(91, 23);
            this.ProceedButton.TabIndex = 1;
            this.ProceedButton.Text = "Proceed";
            this.ProceedButton.UseVisualStyleBackColor = true;
            this.ProceedButton.Click += new System.EventHandler(this.ProceedButton_Click);
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(225, 407);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(91, 23);
            this.Cancel.TabIndex = 2;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.CancelButton_Click);
            // 
            // BoundedLengthDeleteConfirmation
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(421, 442);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.ProceedButton);
            this.Controls.Add(this.Instructions);
            this.Name = "BoundedLengthDeleteConfirmation";
            this.Text = "Confirm Deletion";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox Instructions;
        private System.Windows.Forms.Button ProceedButton;
        private System.Windows.Forms.Button Cancel;
    }
}