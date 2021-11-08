namespace IATClient
{
    partial class UniqueResponseForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UniqueResponseForm));
            this.QuestionLabel = new System.Windows.Forms.Label();
            this.SurveyItemDrop = new System.Windows.Forms.ComboBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.ResponseBox = new System.Windows.Forms.TextBox();
            this.InstructionsText = new System.Windows.Forms.TextBox();
            this.OKButton = new System.Windows.Forms.Button();
            this.CancelButton = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // QuestionLabel
            // 
            this.QuestionLabel.AutoSize = true;
            this.QuestionLabel.Location = new System.Drawing.Point(12, 24);
            this.QuestionLabel.Name = "QuestionLabel";
            this.QuestionLabel.Size = new System.Drawing.Size(66, 13);
            this.QuestionLabel.TabIndex = 0;
            this.QuestionLabel.Text = "Survey Item:";
            // 
            // SurveyItemDrop
            // 
            this.SurveyItemDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.SurveyItemDrop.FormattingEnabled = true;
            this.SurveyItemDrop.Location = new System.Drawing.Point(84, 21);
            this.SurveyItemDrop.Name = "SurveyItemDrop";
            this.SurveyItemDrop.Size = new System.Drawing.Size(442, 21);
            this.SurveyItemDrop.TabIndex = 1;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.ResponseBox);
            this.groupBox1.Controls.Add(this.InstructionsText);
            this.groupBox1.Location = new System.Drawing.Point(12, 48);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(514, 383);
            this.groupBox1.TabIndex = 2;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Acceptable Responses";
            // 
            // ResponseBox
            // 
            this.ResponseBox.Location = new System.Drawing.Point(6, 84);
            this.ResponseBox.Multiline = true;
            this.ResponseBox.Name = "ResponseBox";
            this.ResponseBox.Size = new System.Drawing.Size(502, 293);
            this.ResponseBox.TabIndex = 1;
            // 
            // InstructionsText
            // 
            this.InstructionsText.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.InstructionsText.Enabled = false;
            this.InstructionsText.Location = new System.Drawing.Point(14, 19);
            this.InstructionsText.Multiline = true;
            this.InstructionsText.Name = "InstructionsText";
            this.InstructionsText.ReadOnly = true;
            this.InstructionsText.Size = new System.Drawing.Size(494, 59);
            this.InstructionsText.TabIndex = 0;
            this.InstructionsText.Text = resources.GetString("InstructionsText.Text");
            // 
            // OKButton
            // 
            this.OKButton.Location = new System.Drawing.Point(134, 437);
            this.OKButton.Name = "OKButton";
            this.OKButton.Size = new System.Drawing.Size(75, 23);
            this.OKButton.TabIndex = 3;
            this.OKButton.Text = "OK";
            this.OKButton.UseVisualStyleBackColor = true;
            // 
            // CancelButton
            // 
            this.CancelButton.Location = new System.Drawing.Point(329, 437);
            this.CancelButton.Name = "CancelButton";
            this.CancelButton.Size = new System.Drawing.Size(75, 23);
            this.CancelButton.TabIndex = 4;
            this.CancelButton.Text = "Cancel";
            this.CancelButton.UseVisualStyleBackColor = true;
            // 
            // UniqueResponseForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(538, 476);
            this.Controls.Add(this.CancelButton);
            this.Controls.Add(this.OKButton);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.SurveyItemDrop);
            this.Controls.Add(this.QuestionLabel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "UniqueResponseForm";
            this.Text = "Unique Response";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label QuestionLabel;
        private System.Windows.Forms.ComboBox SurveyItemDrop;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TextBox InstructionsText;
        private System.Windows.Forms.TextBox ResponseBox;
        private System.Windows.Forms.Button OKButton;
        private System.Windows.Forms.Button CancelButton;
    }
}