namespace IATClient
{
    partial class FixedDigDetails
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
            this.FixedDigGroup = new System.Windows.Forms.GroupBox();
            this.NumDigits = new System.Windows.Forms.TextBox();
            this.NumDigitsLabel = new System.Windows.Forms.Label();
            this.FixedDigGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // FixedDigGroup
            // 
            this.FixedDigGroup.Controls.Add(this.NumDigits);
            this.FixedDigGroup.Controls.Add(this.NumDigitsLabel);
            this.FixedDigGroup.Location = new System.Drawing.Point(3, 3);
            this.FixedDigGroup.Name = "FixedDigGroup";
            this.FixedDigGroup.Size = new System.Drawing.Size(220, 50);
            this.FixedDigGroup.TabIndex = 0;
            this.FixedDigGroup.TabStop = false;
            this.FixedDigGroup.Text = "Fixed Number of Digits";
            // 
            // NumDigits
            // 
            this.NumDigits.Location = new System.Drawing.Point(130, 22);
            this.NumDigits.Name = "NumDigits";
            this.NumDigits.Size = new System.Drawing.Size(54, 20);
            this.NumDigits.TabIndex = 1;
            this.NumDigits.TextChanged += new System.EventHandler(this.NumDigits_TextChanged);
            // 
            // NumDigitsLabel
            // 
            this.NumDigitsLabel.AutoSize = true;
            this.NumDigitsLabel.Location = new System.Drawing.Point(36, 25);
            this.NumDigitsLabel.Name = "NumDigitsLabel";
            this.NumDigitsLabel.Size = new System.Drawing.Size(88, 13);
            this.NumDigitsLabel.TabIndex = 0;
            this.NumDigitsLabel.Text = "Number of Digits:";
            // 
            // FixedDigDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.FixedDigGroup);
            this.Name = "FixedDigDetails";
            this.Size = new System.Drawing.Size(226, 55);
            this.ParentChanged += new System.EventHandler(this.FixedDigDetails_ParentChanged);
            this.FixedDigGroup.ResumeLayout(false);
            this.FixedDigGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox FixedDigGroup;
        private System.Windows.Forms.TextBox NumDigits;
        private System.Windows.Forms.Label NumDigitsLabel;
    }
}
