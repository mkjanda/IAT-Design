namespace IATClient
{
    partial class BoundedLengthDetails
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
            this.BoundedLengthGroup = new System.Windows.Forms.GroupBox();
            this.MaxLength = new System.Windows.Forms.TextBox();
            this.MinLength = new System.Windows.Forms.TextBox();
            this.MaxLengthLabel = new System.Windows.Forms.Label();
            this.MinLengthLabel = new System.Windows.Forms.Label();
            this.BoundedLengthGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // BoundedLengthGroup
            // 
            this.BoundedLengthGroup.Controls.Add(this.MaxLength);
            this.BoundedLengthGroup.Controls.Add(this.MinLength);
            this.BoundedLengthGroup.Controls.Add(this.MaxLengthLabel);
            this.BoundedLengthGroup.Controls.Add(this.MinLengthLabel);
            this.BoundedLengthGroup.Location = new System.Drawing.Point(3, 3);
            this.BoundedLengthGroup.Name = "BoundedLengthGroup";
            this.BoundedLengthGroup.Size = new System.Drawing.Size(220, 76);
            this.BoundedLengthGroup.TabIndex = 0;
            this.BoundedLengthGroup.TabStop = false;
            this.BoundedLengthGroup.Text = "Bounded Length Text";
            // 
            // MaxLength
            // 
            this.MaxLength.Location = new System.Drawing.Point(144, 45);
            this.MaxLength.Name = "MaxLength";
            this.MaxLength.Size = new System.Drawing.Size(52, 20);
            this.MaxLength.TabIndex = 3;
            this.MaxLength.TextChanged += new System.EventHandler(this.MaxLength_TextChanged);
            // 
            // MinLength
            // 
            this.MinLength.Location = new System.Drawing.Point(144, 19);
            this.MinLength.Name = "MinLength";
            this.MinLength.Size = new System.Drawing.Size(52, 20);
            this.MinLength.TabIndex = 2;
            this.MinLength.TextChanged += new System.EventHandler(this.MinLength_TextChanged);
            // 
            // MaxLengthLabel
            // 
            this.MaxLengthLabel.AutoSize = true;
            this.MaxLengthLabel.Location = new System.Drawing.Point(24, 48);
            this.MaxLengthLabel.Name = "MaxLengthLabel";
            this.MaxLengthLabel.Size = new System.Drawing.Size(114, 13);
            this.MaxLengthLabel.TabIndex = 1;
            this.MaxLengthLabel.Text = "Maximum Text Length:";
            // 
            // MinLengthLabel
            // 
            this.MinLengthLabel.AutoSize = true;
            this.MinLengthLabel.Location = new System.Drawing.Point(27, 22);
            this.MinLengthLabel.Name = "MinLengthLabel";
            this.MinLengthLabel.Size = new System.Drawing.Size(111, 13);
            this.MinLengthLabel.TabIndex = 0;
            this.MinLengthLabel.Text = "Minimum Text Length:";
            // 
            // BoundedLengthDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BoundedLengthGroup);
            this.Name = "BoundedLengthDetails";
            this.Size = new System.Drawing.Size(226, 83);
            this.ParentChanged += new System.EventHandler(this.BoundedLengthDetails_ParentChanged);
            this.BoundedLengthGroup.ResumeLayout(false);
            this.BoundedLengthGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox BoundedLengthGroup;
        private System.Windows.Forms.TextBox MaxLength;
        private System.Windows.Forms.TextBox MinLength;
        private System.Windows.Forms.Label MaxLengthLabel;
        private System.Windows.Forms.Label MinLengthLabel;
    }
}
