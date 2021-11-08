namespace IATClient
{
    partial class BoundedNumberDetails
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
            this.BoundedNumberGroup = new System.Windows.Forms.GroupBox();
            this.MaxValue = new System.Windows.Forms.TextBox();
            this.MinValue = new System.Windows.Forms.TextBox();
            this.MaxValueLabel = new System.Windows.Forms.Label();
            this.MinValueLabel = new System.Windows.Forms.Label();
            this.BoundedNumberGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // BoundedNumberGroup
            // 
            this.BoundedNumberGroup.Controls.Add(this.MaxValue);
            this.BoundedNumberGroup.Controls.Add(this.MinValue);
            this.BoundedNumberGroup.Controls.Add(this.MaxValueLabel);
            this.BoundedNumberGroup.Controls.Add(this.MinValueLabel);
            this.BoundedNumberGroup.Location = new System.Drawing.Point(3, 3);
            this.BoundedNumberGroup.Name = "BoundedNumberGroup";
            this.BoundedNumberGroup.Size = new System.Drawing.Size(220, 76);
            this.BoundedNumberGroup.TabIndex = 0;
            this.BoundedNumberGroup.TabStop = false;
            this.BoundedNumberGroup.Text = "Bounded Number";
            // 
            // MaxValue
            // 
            this.MaxValue.Location = new System.Drawing.Point(126, 45);
            this.MaxValue.Name = "MaxValue";
            this.MaxValue.Size = new System.Drawing.Size(58, 20);
            this.MaxValue.TabIndex = 3;
            this.MaxValue.TextChanged += new System.EventHandler(this.MaxValue_TextChanged);
            // 
            // MinValue
            // 
            this.MinValue.Location = new System.Drawing.Point(126, 19);
            this.MinValue.Name = "MinValue";
            this.MinValue.Size = new System.Drawing.Size(58, 20);
            this.MinValue.TabIndex = 2;
            this.MinValue.TextChanged += new System.EventHandler(this.MinValue_TextChanged);
            // 
            // MaxValueLabel
            // 
            this.MaxValueLabel.AutoSize = true;
            this.MaxValueLabel.Location = new System.Drawing.Point(36, 48);
            this.MaxValueLabel.Name = "MaxValueLabel";
            this.MaxValueLabel.Size = new System.Drawing.Size(84, 13);
            this.MaxValueLabel.TabIndex = 1;
            this.MaxValueLabel.Text = "Maximum Value:";
            // 
            // MinValueLabel
            // 
            this.MinValueLabel.AutoSize = true;
            this.MinValueLabel.Location = new System.Drawing.Point(39, 22);
            this.MinValueLabel.Name = "MinValueLabel";
            this.MinValueLabel.Size = new System.Drawing.Size(81, 13);
            this.MinValueLabel.TabIndex = 0;
            this.MinValueLabel.Text = "Minimum Value:";
            // 
            // BoundedNumberDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.BoundedNumberGroup);
            this.Name = "BoundedNumberDetails";
            this.Size = new System.Drawing.Size(226, 83);
            this.ParentChanged += new System.EventHandler(this.BoundedNumberDetails_ParentChanged);
            this.BoundedNumberGroup.ResumeLayout(false);
            this.BoundedNumberGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox BoundedNumberGroup;
        private System.Windows.Forms.Label MinValueLabel;
        private System.Windows.Forms.TextBox MaxValue;
        private System.Windows.Forms.TextBox MinValue;
        private System.Windows.Forms.Label MaxValueLabel;
    }
}
