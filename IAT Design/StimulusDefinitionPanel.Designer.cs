using System.Drawing;

namespace IATClient
{
    partial class StimulusDefinitionPanel
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
            this.StimulusGroup = new System.Windows.Forms.GroupBox();
            this.SelectButton = new System.Windows.Forms.Button();
            this.Browse = new System.Windows.Forms.Button();
            this.StimulusImage = new System.Windows.Forms.TextBox();
            this.StimulusImageLabel = new System.Windows.Forms.Label();
            this.KeyedGroup = new System.Windows.Forms.GroupBox();
            this.KeyedRight = new System.Windows.Forms.RadioButton();
            this.KeyedLeft = new System.Windows.Forms.RadioButton();
            this.StimulusTypeGroup = new System.Windows.Forms.GroupBox();
            this.TextRadio = new System.Windows.Forms.RadioButton();
            this.ImageRadio = new System.Windows.Forms.RadioButton();
            this.StimulusGroup.SuspendLayout();
            this.KeyedGroup.SuspendLayout();
            this.StimulusTypeGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // StimulusGroup
            // 
            this.StimulusGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.StimulusGroup.Controls.Add(this.SelectButton);
            this.StimulusGroup.Controls.Add(this.Browse);
            this.StimulusGroup.Controls.Add(this.StimulusImage);
            this.StimulusGroup.Controls.Add(this.StimulusImageLabel);
            this.StimulusGroup.Controls.Add(this.KeyedGroup);
            this.StimulusGroup.Controls.Add(this.StimulusTypeGroup);
            this.StimulusGroup.Location = new System.Drawing.Point(4, 9);
            this.StimulusGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.StimulusGroup.Name = "StimulusGroup";
            this.StimulusGroup.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.StimulusGroup.Size = new System.Drawing.Size(483, 251);
            this.StimulusGroup.TabIndex = 0;
            this.StimulusGroup.TabStop = false;
            this.StimulusGroup.Text = "Stimulus";
            // 
            // SelectButton
            // 
            this.SelectButton.Location = new System.Drawing.Point(300, 207);
            this.SelectButton.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.SelectButton.Name = "SelectButton";
            this.SelectButton.Size = new System.Drawing.Size(100, 28);
            this.SelectButton.TabIndex = 12;
            this.SelectButton.Text = "Select";
            this.SelectButton.UseVisualStyleBackColor = true;
            this.SelectButton.Click += new System.EventHandler(this.Select_Click);
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(192, 207);
            this.Browse.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(100, 28);
            this.Browse.TabIndex = 11;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // StimulusImage
            // 
            this.StimulusImage.Location = new System.Drawing.Point(192, 175);
            this.StimulusImage.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.StimulusImage.Name = "StimulusImage";
            this.StimulusImage.ReadOnly = true;
            this.StimulusImage.Size = new System.Drawing.Size(207, 22);
            this.StimulusImage.TabIndex = 10;
            // 
            // StimulusImageLabel
            // 
            this.StimulusImageLabel.AutoSize = true;
            this.StimulusImageLabel.Location = new System.Drawing.Point(76, 178);
            this.StimulusImageLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.StimulusImageLabel.Name = "StimulusImageLabel";
            this.StimulusImageLabel.Size = new System.Drawing.Size(107, 17);
            this.StimulusImageLabel.TabIndex = 9;
            this.StimulusImageLabel.Text = "Stimulus Image:";
            // 
            // KeyedGroup
            // 
            this.KeyedGroup.Controls.Add(this.KeyedRight);
            this.KeyedGroup.Controls.Add(this.KeyedLeft);
            this.KeyedGroup.Location = new System.Drawing.Point(300, 23);
            this.KeyedGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.KeyedGroup.Name = "KeyedGroup";
            this.KeyedGroup.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.KeyedGroup.Size = new System.Drawing.Size(151, 82);
            this.KeyedGroup.TabIndex = 8;
            this.KeyedGroup.TabStop = false;
            this.KeyedGroup.Text = "Keyed Direction";
            // 
            // KeyedRight
            // 
            this.KeyedRight.AutoSize = true;
            this.KeyedRight.Location = new System.Drawing.Point(8, 52);
            this.KeyedRight.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.KeyedRight.Name = "KeyedRight";
            this.KeyedRight.Size = new System.Drawing.Size(62, 21);
            this.KeyedRight.TabIndex = 1;
            this.KeyedRight.TabStop = true;
            this.KeyedRight.Text = "Right";
            this.KeyedRight.UseVisualStyleBackColor = true;
            this.KeyedRight.CheckedChanged += new System.EventHandler(this.KeyedRight_CheckedChanged);
            // 
            // KeyedLeft
            // 
            this.KeyedLeft.AutoSize = true;
            this.KeyedLeft.Location = new System.Drawing.Point(8, 23);
            this.KeyedLeft.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.KeyedLeft.Name = "KeyedLeft";
            this.KeyedLeft.Size = new System.Drawing.Size(53, 21);
            this.KeyedLeft.TabIndex = 0;
            this.KeyedLeft.TabStop = true;
            this.KeyedLeft.Text = "Left";
            this.KeyedLeft.UseVisualStyleBackColor = true;
            this.KeyedLeft.CheckedChanged += new System.EventHandler(this.KeyedLeft_CheckedChanged);
            // 
            // StimulusTypeGroup
            // 
            this.StimulusTypeGroup.Controls.Add(this.TextRadio);
            this.StimulusTypeGroup.Controls.Add(this.ImageRadio);
            this.StimulusTypeGroup.Location = new System.Drawing.Point(28, 23);
            this.StimulusTypeGroup.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.StimulusTypeGroup.Name = "StimulusTypeGroup";
            this.StimulusTypeGroup.Padding = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.StimulusTypeGroup.Size = new System.Drawing.Size(148, 82);
            this.StimulusTypeGroup.TabIndex = 7;
            this.StimulusTypeGroup.TabStop = false;
            this.StimulusTypeGroup.Text = "Stimulus Type";
            // 
            // TextRadio
            // 
            this.TextRadio.AutoSize = true;
            this.TextRadio.Location = new System.Drawing.Point(33, 23);
            this.TextRadio.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.TextRadio.Name = "TextRadio";
            this.TextRadio.Size = new System.Drawing.Size(56, 21);
            this.TextRadio.TabIndex = 1;
            this.TextRadio.TabStop = true;
            this.TextRadio.Text = "Text";
            this.TextRadio.UseVisualStyleBackColor = true;
            this.TextRadio.CheckedChanged += new System.EventHandler(this.TextRadio_CheckedChanged);
            // 
            // ImageRadio
            // 
            this.ImageRadio.AutoSize = true;
            this.ImageRadio.Location = new System.Drawing.Point(33, 52);
            this.ImageRadio.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ImageRadio.Name = "ImageRadio";
            this.ImageRadio.Size = new System.Drawing.Size(67, 21);
            this.ImageRadio.TabIndex = 0;
            this.ImageRadio.TabStop = true;
            this.ImageRadio.Text = "Image";
            this.ImageRadio.UseVisualStyleBackColor = true;
            this.ImageRadio.CheckedChanged += new System.EventHandler(this.ImageRadio_CheckedChanged);
            // 
            // StimulusDefinitionPanel
            // 
            this.Controls.Add(this.StimulusGroup);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Name = "StimulusDefinitionPanel";
            this.Size = new System.Drawing.Size(491, 263);
            this.Load += new System.EventHandler(this.StimulusDefinitionPanel_Load); 

            this.StimulusGroup.ResumeLayout(false);
            this.StimulusGroup.PerformLayout();
            this.KeyedGroup.PerformLayout();
            this.StimulusTypeGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox StimulusGroup;
        private System.Windows.Forms.GroupBox StimulusTypeGroup;
        private System.Windows.Forms.GroupBox KeyedGroup;
        private System.Windows.Forms.RadioButton KeyedRight;
        private System.Windows.Forms.RadioButton KeyedLeft;
        private System.Windows.Forms.RadioButton TextRadio;
        private System.Windows.Forms.RadioButton ImageRadio;
        private System.Windows.Forms.Label StimulusImageLabel;
        private System.Windows.Forms.Button SelectButton;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.TextBox StimulusImage;
    }
}
