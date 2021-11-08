namespace IATClient
{
    partial class ImageKeyPanel
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
            this.FileNameLabel = new System.Windows.Forms.Label();
            this.FileName = new System.Windows.Forms.TextBox();
            this.BrowseButton = new System.Windows.Forms.Button();
            this.SelectButton = new System.Windows.Forms.Button();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // FileNameLabel
            // 
            this.FileNameLabel.AutoSize = true;
            this.FileNameLabel.Location = new System.Drawing.Point(3, 6);
            this.FileNameLabel.Name = "FileNameLabel";
            this.FileNameLabel.Size = new System.Drawing.Size(89, 13);
            this.FileNameLabel.TabIndex = 0;
            this.FileNameLabel.Text = "Image File Name:";
            // 
            // FileName
            // 
            this.FileName.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.FileName.Location = new System.Drawing.Point(98, 3);
            this.FileName.Name = "FileName";
            this.FileName.ReadOnly = true;
            this.FileName.Size = new System.Drawing.Size(274, 20);
            this.FileName.TabIndex = 1;
            // 
            // BrowseButton
            // 
            this.BrowseButton.Location = new System.Drawing.Point(216, 29);
            this.BrowseButton.Name = "BrowseButton";
            this.BrowseButton.Size = new System.Drawing.Size(75, 23);
            this.BrowseButton.TabIndex = 2;
            this.BrowseButton.Text = "Browse";
            this.BrowseButton.UseVisualStyleBackColor = true;
            this.BrowseButton.Click += new System.EventHandler(this.BrowseButton_Click);
            /*
            // 
            // AutoScaleCheck
            // 
            this.AutoScaleCheck.AutoSize = true;
            this.AutoScaleCheck.Checked = true;
            this.AutoScaleCheck.CheckState = System.Windows.Forms.CheckState.Checked;
            this.AutoScaleCheck.Location = new System.Drawing.Point(216, 58);
            this.AutoScaleCheck.Name = "AutoScaleCheck";
            this.AutoScaleCheck.Size = new System.Drawing.Size(147, 17);
            this.AutoScaleCheck.TabIndex = 4;
            this.AutoScaleCheck.Text = "Automatically scale image";
            this.AutoScaleCheck.UseVisualStyleBackColor = true;
            this.AutoScaleCheck.CheckedChanged += new System.EventHandler(this.AutoScaleCheck_CheckedChanged);
            */
            // 
            // SelectButton
            // 
            this.SelectButton.Location = new System.Drawing.Point(297, 29);
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.Size = new System.Drawing.Size(75, 23);
            this.SelectButton.TabIndex = 5;
            this.SelectButton.Text = "Select";
            this.SelectButton.UseVisualStyleBackColor = true;
            this.SelectButton.Click += new System.EventHandler(this.SelectButton_Click);
            // 
            // textBox1
            // 
            this.textBox1.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.textBox1.Location = new System.Drawing.Point(3, 29);
            this.textBox1.Multiline = true;
            this.textBox1.Name = "textBox1";
            this.textBox1.ReadOnly = true;
            this.textBox1.Size = new System.Drawing.Size(207, 45);
            this.textBox1.TabIndex = 6;
            this.textBox1.Text = "Click Browse to browse for an image on your computer or Select to select an image" +
                " already included in the IAT.";
            // 
            // ImageKeyPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.textBox1);
            this.Controls.Add(this.SelectButton);
            this.Controls.Add(this.BrowseButton);
            this.Controls.Add(this.FileName);
            this.Controls.Add(this.FileNameLabel);
            this.Name = "ImageKeyPanel";
            this.Size = new System.Drawing.Size(375, 77);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label FileNameLabel;
        private System.Windows.Forms.TextBox FileName;
        private System.Windows.Forms.Button BrowseButton;
        private System.Windows.Forms.CheckBox AutoScaleCheck;
        private System.Windows.Forms.Button SelectButton;
        private System.Windows.Forms.TextBox textBox1;
    }
}
