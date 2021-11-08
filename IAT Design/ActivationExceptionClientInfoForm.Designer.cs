namespace IATClient
{
    partial class ActivationExceptionClientInfoForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ActivationExceptionClientInfoForm));
            this.MessageBox = new System.Windows.Forms.TextBox();
            this.EmailBox = new System.Windows.Forms.TextBox();
            this.SubmitButton = new System.Windows.Forms.Button();
            this.DeclineButton = new System.Windows.Forms.Button();
            this.ExceptionBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ProductKeyBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // MessageBox
            // 
            this.MessageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.MessageBox.Location = new System.Drawing.Point(12, 12);
            this.MessageBox.Multiline = true;
            this.MessageBox.Name = "MessageBox";
            this.MessageBox.ReadOnly = true;
            this.MessageBox.Size = new System.Drawing.Size(456, 63);
            this.MessageBox.TabIndex = 0;
            this.MessageBox.TabStop = false;
            this.MessageBox.Text = resources.GetString("MessageBox.Text");
            // 
            // EmailBox
            // 
            this.EmailBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.EmailBox.Location = new System.Drawing.Point(70, 262);
            this.EmailBox.Name = "EmailBox";
            this.EmailBox.Size = new System.Drawing.Size(135, 20);
            this.EmailBox.TabIndex = 1;
            // 
            // SubmitButton
            // 
            this.SubmitButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.SubmitButton.Location = new System.Drawing.Point(157, 288);
            this.SubmitButton.Name = "SubmitButton";
            this.SubmitButton.Size = new System.Drawing.Size(75, 23);
            this.SubmitButton.TabIndex = 3;
            this.SubmitButton.Text = "Submit";
            this.SubmitButton.UseVisualStyleBackColor = true;
            // 
            // DeclineButton
            // 
            this.DeclineButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.DeclineButton.Location = new System.Drawing.Point(252, 288);
            this.DeclineButton.Name = "DeclineButton";
            this.DeclineButton.Size = new System.Drawing.Size(75, 23);
            this.DeclineButton.TabIndex = 4;
            this.DeclineButton.Text = "No Thanks";
            this.DeclineButton.UseVisualStyleBackColor = true;
            // 
            // ExceptionBox
            // 
            this.ExceptionBox.Location = new System.Drawing.Point(12, 92);
            this.ExceptionBox.Multiline = true;
            this.ExceptionBox.Name = "ExceptionBox";
            this.ExceptionBox.ReadOnly = true;
            this.ExceptionBox.Size = new System.Drawing.Size(456, 164);
            this.ExceptionBox.TabIndex = 5;
            this.ExceptionBox.TabStop = false;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(29, 265);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 13);
            this.label1.TabIndex = 6;
            this.label1.Text = "Email:";
            // 
            // ProductKeyBox
            // 
            this.ProductKeyBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ProductKeyBox.Location = new System.Drawing.Point(313, 262);
            this.ProductKeyBox.Name = "ProductKeyBox";
            this.ProductKeyBox.Size = new System.Drawing.Size(135, 20);
            this.ProductKeyBox.TabIndex = 7;
            // 
            // label2
            // 
            this.label2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(239, 265);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(68, 13);
            this.label2.TabIndex = 8;
            this.label2.Text = "Product Key:";
            // 
            // ActivationExceptionClientInfoForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(480, 321);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ProductKeyBox);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ExceptionBox);
            this.Controls.Add(this.DeclineButton);
            this.Controls.Add(this.SubmitButton);
            this.Controls.Add(this.EmailBox);
            this.Controls.Add(this.MessageBox);
            this.Name = "ActivationExceptionClientInfoForm";
            this.Text = "Additional Information";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox MessageBox;
        private System.Windows.Forms.TextBox EmailBox;
        private System.Windows.Forms.Button SubmitButton;
        private System.Windows.Forms.Button DeclineButton;
        private System.Windows.Forms.TextBox ExceptionBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox ProductKeyBox;
        private System.Windows.Forms.Label label2;
    }
}