namespace IATClient
{
    partial class MaxLengthDetails
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
            this.MaxLengthGroup = new System.Windows.Forms.GroupBox();
            this.MaxLength = new System.Windows.Forms.TextBox();
            this.MaxLengthLabel = new System.Windows.Forms.Label();
            this.MaxLengthGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // MaxLengthGroup
            // 
            this.MaxLengthGroup.Controls.Add(this.MaxLength);
            this.MaxLengthGroup.Controls.Add(this.MaxLengthLabel);
            this.MaxLengthGroup.Location = new System.Drawing.Point(3, 3);
            this.MaxLengthGroup.Name = "MaxLengthGroup";
            this.MaxLengthGroup.Size = new System.Drawing.Size(220, 49);
            this.MaxLengthGroup.TabIndex = 0;
            this.MaxLengthGroup.TabStop = false;
            this.MaxLengthGroup.Text = "Text with Maximum Length";
            // 
            // MaxLength
            // 
            this.MaxLength.Location = new System.Drawing.Point(132, 19);
            this.MaxLength.Name = "MaxLength";
            this.MaxLength.Size = new System.Drawing.Size(52, 20);
            this.MaxLength.TabIndex = 1;
            this.MaxLength.TextChanged += new System.EventHandler(this.MaxLength_TextChanged);
            // 
            // MaxLengthLabel
            // 
            this.MaxLengthLabel.AutoSize = true;
            this.MaxLengthLabel.Location = new System.Drawing.Point(36, 22);
            this.MaxLengthLabel.Name = "MaxLengthLabel";
            this.MaxLengthLabel.Size = new System.Drawing.Size(90, 13);
            this.MaxLengthLabel.TabIndex = 0;
            this.MaxLengthLabel.Text = "Maximum Length:";
            // 
            // MaxLengthDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.MaxLengthGroup);
            this.Name = "MaxLengthDetails";
            this.Size = new System.Drawing.Size(226, 55);
            this.ParentChanged += new System.EventHandler(this.MaxLengthDetails_ParentChanged);
            this.MaxLengthGroup.ResumeLayout(false);
            this.MaxLengthGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox MaxLengthGroup;
        private System.Windows.Forms.Label MaxLengthLabel;
        private System.Windows.Forms.TextBox MaxLength;
    }
}
