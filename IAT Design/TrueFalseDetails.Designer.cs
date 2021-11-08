namespace IATClient
{
    partial class TrueFalseDetails
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
            this.TrueFalseGroup = new System.Windows.Forms.GroupBox();
            this.FalseEdit = new System.Windows.Forms.TextBox();
            this.FalseStatementLabel = new System.Windows.Forms.Label();
            this.TrueEdit = new System.Windows.Forms.TextBox();
            this.TrueStatementLabel = new System.Windows.Forms.Label();
            this.TrueFalseGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // TrueFalseGroup
            // 
            this.TrueFalseGroup.Controls.Add(this.FalseEdit);
            this.TrueFalseGroup.Controls.Add(this.FalseStatementLabel);
            this.TrueFalseGroup.Controls.Add(this.TrueEdit);
            this.TrueFalseGroup.Controls.Add(this.TrueStatementLabel);
            this.TrueFalseGroup.Location = new System.Drawing.Point(3, 3);
            this.TrueFalseGroup.Name = "TrueFalseGroup";
            this.TrueFalseGroup.Size = new System.Drawing.Size(220, 103);
            this.TrueFalseGroup.TabIndex = 0;
            this.TrueFalseGroup.TabStop = false;
            this.TrueFalseGroup.Text = "True/False";
            // 
            // FalseEdit
            // 
            this.FalseEdit.Location = new System.Drawing.Point(18, 71);
            this.FalseEdit.Name = "FalseEdit";
            this.FalseEdit.Size = new System.Drawing.Size(196, 20);
            this.FalseEdit.TabIndex = 3;
            this.FalseEdit.Text = "False";
            this.FalseEdit.TextChanged += new System.EventHandler(this.FalseEdit_TextChanged);
            // 
            // FalseStatementLabel
            // 
            this.FalseStatementLabel.AutoSize = true;
            this.FalseStatementLabel.Location = new System.Drawing.Point(6, 55);
            this.FalseStatementLabel.Name = "FalseStatementLabel";
            this.FalseStatementLabel.Size = new System.Drawing.Size(86, 13);
            this.FalseStatementLabel.TabIndex = 2;
            this.FalseStatementLabel.Text = "False Statement:";
            // 
            // TrueEdit
            // 
            this.TrueEdit.Location = new System.Drawing.Point(18, 32);
            this.TrueEdit.Name = "TrueEdit";
            this.TrueEdit.Size = new System.Drawing.Size(196, 20);
            this.TrueEdit.TabIndex = 1;
            this.TrueEdit.Text = "True";
            this.TrueEdit.TextChanged += new System.EventHandler(this.TrueEdit_TextChanged);
            // 
            // TrueStatementLabel
            // 
            this.TrueStatementLabel.AutoSize = true;
            this.TrueStatementLabel.Location = new System.Drawing.Point(6, 16);
            this.TrueStatementLabel.Name = "TrueStatementLabel";
            this.TrueStatementLabel.Size = new System.Drawing.Size(83, 13);
            this.TrueStatementLabel.TabIndex = 0;
            this.TrueStatementLabel.Text = "True Statement:";
            // 
            // TrueFalseDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TrueFalseGroup);
            this.Name = "TrueFalseDetails";
            this.Size = new System.Drawing.Size(226, 112);
            this.ParentChanged += new System.EventHandler(this.TrueFalseDetails_ParentChanged);
            this.TrueFalseGroup.ResumeLayout(false);
            this.TrueFalseGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox TrueFalseGroup;
        private System.Windows.Forms.TextBox FalseEdit;
        private System.Windows.Forms.Label FalseStatementLabel;
        private System.Windows.Forms.TextBox TrueEdit;
        private System.Windows.Forms.Label TrueStatementLabel;
    }
}
