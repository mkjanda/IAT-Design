namespace IATClient
{
    partial class FixedLengthDetails
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
            this.FixedLengthGroup = new System.Windows.Forms.GroupBox();
            this.TextLength = new System.Windows.Forms.TextBox();
            this.TextLengthLabel = new System.Windows.Forms.Label();
            this.FixedLengthGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // FixedLengthGroup
            // 
            this.FixedLengthGroup.Controls.Add(this.TextLength);
            this.FixedLengthGroup.Controls.Add(this.TextLengthLabel);
            this.FixedLengthGroup.Location = new System.Drawing.Point(3, 3);
            this.FixedLengthGroup.Name = "FixedLengthGroup";
            this.FixedLengthGroup.Size = new System.Drawing.Size(220, 50);
            this.FixedLengthGroup.TabIndex = 0;
            this.FixedLengthGroup.TabStop = false;
            this.FixedLengthGroup.Text = "Fixed Length Text";
            // 
            // TextLength
            // 
            this.TextLength.Location = new System.Drawing.Point(109, 19);
            this.TextLength.Name = "TextLength";
            this.TextLength.Size = new System.Drawing.Size(75, 20);
            this.TextLength.TabIndex = 1;
            this.TextLength.TextChanged += new System.EventHandler(this.TextLength_TextChanged);
            // 
            // TextLengthLabel
            // 
            this.TextLengthLabel.AutoSize = true;
            this.TextLengthLabel.Location = new System.Drawing.Point(36, 22);
            this.TextLengthLabel.Name = "TextLengthLabel";
            this.TextLengthLabel.Size = new System.Drawing.Size(67, 13);
            this.TextLengthLabel.TabIndex = 0;
            this.TextLengthLabel.Text = "Text Length:";
            // 
            // FixedLengthDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FixedLengthGroup);
            this.Name = "FixedLengthDetails";
            this.Size = new System.Drawing.Size(226, 55);
            this.ParentChanged += new System.EventHandler(this.FixedLengthDetails_ParentChanged);
            this.FixedLengthGroup.ResumeLayout(false);
            this.FixedLengthGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox FixedLengthGroup;
        private System.Windows.Forms.TextBox TextLength;
        private System.Windows.Forms.Label TextLengthLabel;
    }
}
