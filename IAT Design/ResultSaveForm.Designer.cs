namespace IATClient
{
    partial class ResultSaveForm
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
            this.SummaryDataGroup = new System.Windows.Forms.GroupBox();
            this.SummaryWithHeaderRadio = new System.Windows.Forms.RadioButton();
            this.SummaryDataGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // SummaryDataGroup
            // 
            this.SummaryDataGroup.Controls.Add(this.SummaryWithHeaderRadio);
            this.SummaryDataGroup.Location = new System.Drawing.Point(12, 12);
            this.SummaryDataGroup.Name = "SummaryDataGroup";
            this.SummaryDataGroup.Size = new System.Drawing.Size(337, 71);
            this.SummaryDataGroup.TabIndex = 0;
            this.SummaryDataGroup.TabStop = false;
            this.SummaryDataGroup.Text = "Summary Data";
            // 
            // SummaryWithHeaderRadio
            // 
            this.SummaryWithHeaderRadio.AutoSize = true;
            this.SummaryWithHeaderRadio.Location = new System.Drawing.Point(6, 19);
            this.SummaryWithHeaderRadio.Name = "SummaryWithHeaderRadio";
            this.SummaryWithHeaderRadio.Size = new System.Drawing.Size(14, 13);
            this.SummaryWithHeaderRadio.TabIndex = 0;
            this.SummaryWithHeaderRadio.TabStop = true;
            this.SummaryWithHeaderRadio.UseVisualStyleBackColor = true;
            // 
            // ResultSaveForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(361, 167);
            this.Controls.Add(this.SummaryDataGroup);
            this.Name = "ResultSaveForm";
            this.Text = "Save Result Data to File";
            this.SummaryDataGroup.ResumeLayout(false);
            this.SummaryDataGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox SummaryDataGroup;
        private System.Windows.Forms.RadioButton SummaryWithHeaderRadio;
    }
}