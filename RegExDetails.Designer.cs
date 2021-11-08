namespace IATClient
{
    partial class RegExDetails
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
            this.RegExGroup = new System.Windows.Forms.GroupBox();
            this.RegExEdit = new System.Windows.Forms.TextBox();
            this.ExpressionLabel = new System.Windows.Forms.Label();
            this.TestInputLabel = new System.Windows.Forms.Label();
            this.TestInput = new System.Windows.Forms.TextBox();
            this.TestResultLabel = new System.Windows.Forms.Label();
            this.RegExGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // RegExGroup
            // 
            this.RegExGroup.Controls.Add(this.TestResultLabel);
            this.RegExGroup.Controls.Add(this.TestInput);
            this.RegExGroup.Controls.Add(this.TestInputLabel);
            this.RegExGroup.Controls.Add(this.RegExEdit);
            this.RegExGroup.Controls.Add(this.ExpressionLabel);
            this.RegExGroup.Location = new System.Drawing.Point(3, 3);
            this.RegExGroup.Name = "RegExGroup";
            this.RegExGroup.Size = new System.Drawing.Size(220, 88);
            this.RegExGroup.TabIndex = 0;
            this.RegExGroup.TabStop = false;
            this.RegExGroup.Text = "Regular Expression";
            // 
            // RegExEdit
            // 
            this.RegExEdit.Location = new System.Drawing.Point(73, 19);
            this.RegExEdit.Name = "RegExEdit";
            this.RegExEdit.Size = new System.Drawing.Size(141, 20);
            this.RegExEdit.TabIndex = 1;
            this.RegExEdit.TextChanged += new System.EventHandler(this.RegExEdit_TextChanged);
            // 
            // ExpressionLabel
            // 
            this.ExpressionLabel.AutoSize = true;
            this.ExpressionLabel.Location = new System.Drawing.Point(6, 22);
            this.ExpressionLabel.Name = "ExpressionLabel";
            this.ExpressionLabel.Size = new System.Drawing.Size(61, 13);
            this.ExpressionLabel.TabIndex = 0;
            this.ExpressionLabel.Text = "Expression:";
            // 
            // TestInputLabel
            // 
            this.TestInputLabel.AutoSize = true;
            this.TestInputLabel.Location = new System.Drawing.Point(9, 48);
            this.TestInputLabel.Name = "TestInputLabel";
            this.TestInputLabel.Size = new System.Drawing.Size(58, 13);
            this.TestInputLabel.TabIndex = 2;
            this.TestInputLabel.Text = "Test Input:";
            // 
            // TestInput
            // 
            this.TestInput.Location = new System.Drawing.Point(73, 45);
            this.TestInput.Name = "TestInput";
            this.TestInput.Size = new System.Drawing.Size(141, 20);
            this.TestInput.TabIndex = 3;
            this.TestInput.TextChanged += new System.EventHandler(this.TestInput_TextChanged);
            // 
            // TestResultLabel
            // 
            this.TestResultLabel.AutoSize = true;
            this.TestResultLabel.Location = new System.Drawing.Point(32, 70);
            this.TestResultLabel.Name = "TestResultLabel";
            this.TestResultLabel.Size = new System.Drawing.Size(35, 13);
            this.TestResultLabel.TabIndex = 4;
            this.TestResultLabel.Text = "label1";
            // 
            // RegExDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RegExGroup);
            this.Name = "RegExDetails";
            this.Size = new System.Drawing.Size(226, 95);
            this.Load += new System.EventHandler(this.RegExDetails_Load);
            this.ParentChanged += new System.EventHandler(this.RegExDetails_ParentChanged);
            this.RegExGroup.ResumeLayout(false);
            this.RegExGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox RegExGroup;
        private System.Windows.Forms.TextBox RegExEdit;
        private System.Windows.Forms.Label ExpressionLabel;
        private System.Windows.Forms.Label TestResultLabel;
        private System.Windows.Forms.TextBox TestInput;
        private System.Windows.Forms.Label TestInputLabel;
    }
}
