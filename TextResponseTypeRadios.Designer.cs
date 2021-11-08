namespace IATClient
{
    partial class TextResponseTypeRadios
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
            this.TextResponseTypeRadiosGroup = new System.Windows.Forms.GroupBox();
            this.MaxLengthRadio = new System.Windows.Forms.RadioButton();
            this.FixedLengthRadio = new System.Windows.Forms.RadioButton();
            this.FixedDigitRadio = new System.Windows.Forms.RadioButton();
            this.BoundedNumberRadio = new System.Windows.Forms.RadioButton();
            this.BoundedLengthRadio = new System.Windows.Forms.RadioButton();
            this.RegExRadio = new System.Windows.Forms.RadioButton();
            this.TextResponseTypeRadiosGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // TextResponseTypeRadiosGroup
            // 
            this.TextResponseTypeRadiosGroup.Controls.Add(this.RegExRadio);
            this.TextResponseTypeRadiosGroup.Controls.Add(this.MaxLengthRadio);
            this.TextResponseTypeRadiosGroup.Controls.Add(this.FixedLengthRadio);
            this.TextResponseTypeRadiosGroup.Controls.Add(this.FixedDigitRadio);
            this.TextResponseTypeRadiosGroup.Controls.Add(this.BoundedNumberRadio);
            this.TextResponseTypeRadiosGroup.Controls.Add(this.BoundedLengthRadio);
            this.TextResponseTypeRadiosGroup.Location = new System.Drawing.Point(3, 3);
            this.TextResponseTypeRadiosGroup.Name = "TextResponseTypeRadiosGroup";
            this.TextResponseTypeRadiosGroup.Size = new System.Drawing.Size(220, 160);
            this.TextResponseTypeRadiosGroup.TabIndex = 0;
            this.TextResponseTypeRadiosGroup.TabStop = false;
            this.TextResponseTypeRadiosGroup.Text = "Text Response Type";
            // 
            // MaxLengthRadio
            // 
            this.MaxLengthRadio.AutoSize = true;
            this.MaxLengthRadio.Location = new System.Drawing.Point(6, 134);
            this.MaxLengthRadio.Name = "MaxLengthRadio";
            this.MaxLengthRadio.Size = new System.Drawing.Size(151, 17);
            this.MaxLengthRadio.TabIndex = 4;
            this.MaxLengthRadio.TabStop = true;
            this.MaxLengthRadio.Text = "Text with Maximum Length";
            this.MaxLengthRadio.UseVisualStyleBackColor = true;
            this.MaxLengthRadio.CheckedChanged += new System.EventHandler(this.MaxLengthRadio_CheckedChanged);
            // 
            // FixedLengthRadio
            // 
            this.FixedLengthRadio.AutoSize = true;
            this.FixedLengthRadio.Location = new System.Drawing.Point(6, 88);
            this.FixedLengthRadio.Name = "FixedLengthRadio";
            this.FixedLengthRadio.Size = new System.Drawing.Size(110, 17);
            this.FixedLengthRadio.TabIndex = 3;
            this.FixedLengthRadio.TabStop = true;
            this.FixedLengthRadio.Text = "Fixed Length Text";
            this.FixedLengthRadio.UseVisualStyleBackColor = true;
            this.FixedLengthRadio.CheckedChanged += new System.EventHandler(this.FixedLengthRadio_CheckedChanged);
            // 
            // FixedDigitRadio
            // 
            this.FixedDigitRadio.AutoSize = true;
            this.FixedDigitRadio.Location = new System.Drawing.Point(6, 65);
            this.FixedDigitRadio.Name = "FixedDigitRadio";
            this.FixedDigitRadio.Size = new System.Drawing.Size(131, 17);
            this.FixedDigitRadio.TabIndex = 2;
            this.FixedDigitRadio.TabStop = true;
            this.FixedDigitRadio.Text = "Fixed Number of Digits";
            this.FixedDigitRadio.UseVisualStyleBackColor = true;
            this.FixedDigitRadio.CheckedChanged += new System.EventHandler(this.FixedDigitRadio_CheckedChanged);
            // 
            // BoundedNumberRadio
            // 
            this.BoundedNumberRadio.AutoSize = true;
            this.BoundedNumberRadio.Location = new System.Drawing.Point(6, 42);
            this.BoundedNumberRadio.Name = "BoundedNumberRadio";
            this.BoundedNumberRadio.Size = new System.Drawing.Size(108, 17);
            this.BoundedNumberRadio.TabIndex = 1;
            this.BoundedNumberRadio.TabStop = true;
            this.BoundedNumberRadio.Text = "Bounded Number";
            this.BoundedNumberRadio.UseVisualStyleBackColor = true;
            this.BoundedNumberRadio.CheckedChanged += new System.EventHandler(this.BoundedNumberRadio_CheckedChanged);
            // 
            // BoundedLengthRadio
            // 
            this.BoundedLengthRadio.AutoSize = true;
            this.BoundedLengthRadio.Location = new System.Drawing.Point(6, 19);
            this.BoundedLengthRadio.Name = "BoundedLengthRadio";
            this.BoundedLengthRadio.Size = new System.Drawing.Size(128, 17);
            this.BoundedLengthRadio.TabIndex = 0;
            this.BoundedLengthRadio.TabStop = true;
            this.BoundedLengthRadio.Text = "Bounded Length Text";
            this.BoundedLengthRadio.UseVisualStyleBackColor = true;
            this.BoundedLengthRadio.CheckedChanged += new System.EventHandler(this.BoundedLengthRadio_CheckedChanged);
            // 
            // RegExRadio
            // 
            this.RegExRadio.AutoSize = true;
            this.RegExRadio.Location = new System.Drawing.Point(6, 111);
            this.RegExRadio.Name = "RegExRadio";
            this.RegExRadio.Size = new System.Drawing.Size(187, 17);
            this.RegExRadio.TabIndex = 5;
            this.RegExRadio.TabStop = true;
            this.RegExRadio.Text = "Regular Expression Validated Text";
            this.RegExRadio.UseVisualStyleBackColor = true;
            this.RegExRadio.CheckedChanged += new System.EventHandler(this.RegExRadio_CheckedChanged);
            // 
            // TextResponseTypeRadios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TextResponseTypeRadiosGroup);
            this.Name = "TextResponseTypeRadios";
            this.Size = new System.Drawing.Size(226, 166);
            this.TextResponseTypeRadiosGroup.ResumeLayout(false);
            this.TextResponseTypeRadiosGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox TextResponseTypeRadiosGroup;
        private System.Windows.Forms.RadioButton MaxLengthRadio;
        private System.Windows.Forms.RadioButton FixedLengthRadio;
        private System.Windows.Forms.RadioButton FixedDigitRadio;
        private System.Windows.Forms.RadioButton BoundedNumberRadio;
        private System.Windows.Forms.RadioButton BoundedLengthRadio;
        private System.Windows.Forms.RadioButton RegExRadio;
    }
}
