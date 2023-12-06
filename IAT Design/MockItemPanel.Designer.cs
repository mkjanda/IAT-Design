namespace IATClient
{
    partial class MockItemPanel
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
            this.MarkAsInvalid = new System.Windows.Forms.CheckBox();
            this.OutlineResponse = new System.Windows.Forms.CheckBox();
            this.InstructionsGroup = new System.Windows.Forms.GroupBox();
            this.ImageStimulusGroup = new System.Windows.Forms.GroupBox();
            this.StretchToFit = new System.Windows.Forms.CheckBox();
            this.ImageFileName = new System.Windows.Forms.TextBox();
            this.Browse = new System.Windows.Forms.Button();
            this.ImageFileNameLabel = new System.Windows.Forms.Label();
            this.TextStimulusGroup = new System.Windows.Forms.GroupBox();
            this.KeyedDirLabel = new System.Windows.Forms.Label();
            this.KeyedDirDrop = new System.Windows.Forms.ComboBox();
            this.ImageStimulusRadio = new System.Windows.Forms.RadioButton();
            this.TextStimulusRadio = new System.Windows.Forms.RadioButton();
            this.StimulusTypeLabel = new System.Windows.Forms.Label();
            this.ManageKeysButton = new System.Windows.Forms.Button();
            this.ResponseKeyDrop = new System.Windows.Forms.ComboBox();
            this.ResponseKeyLabel = new System.Windows.Forms.Label();
            this.ImageStimulusGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // MockItemGroup
            // 
            Controls.Add(this.MarkAsInvalid);
            this.Controls.Add(this.OutlineResponse);
            this.Controls.Add(this.InstructionsGroup);
            this.Controls.Add(this.ImageStimulusGroup);
            this.Controls.Add(this.TextStimulusGroup);
            this.Controls.Add(this.KeyedDirLabel);
            this.Controls.Add(this.KeyedDirDrop);
            this.Controls.Add(this.ImageStimulusRadio);
            this.Controls.Add(this.TextStimulusRadio);
            this.Controls.Add(this.StimulusTypeLabel);
            this.Controls.Add(this.ManageKeysButton);
            this.Controls.Add(this.ResponseKeyDrop);
            this.Controls.Add(this.ResponseKeyLabel);
            // 
            // MarkAsInvalid
            // 
            this.MarkAsInvalid.AutoSize = true;
            this.MarkAsInvalid.Location = new System.Drawing.Point(182, 259);
            this.MarkAsInvalid.Name = "MarkAsInvalid";
            this.MarkAsInvalid.Size = new System.Drawing.Size(143, 17);
            this.MarkAsInvalid.TabIndex = 12;
            this.MarkAsInvalid.Text = "Mark response as invalid";
            this.MarkAsInvalid.UseVisualStyleBackColor = true;
            this.MarkAsInvalid.CheckedChanged += new System.EventHandler(this.MarkAsInvalid_CheckedChanged);
            // 
            // OutlineResponse
            // 
            this.OutlineResponse.AutoSize = true;
            this.OutlineResponse.Location = new System.Drawing.Point(39, 259);
            this.OutlineResponse.Name = "OutlineResponse";
            this.OutlineResponse.Size = new System.Drawing.Size(137, 17);
            this.OutlineResponse.TabIndex = 11;
            this.OutlineResponse.Text = "Outline keyed response";
            this.OutlineResponse.UseVisualStyleBackColor = true;
            this.OutlineResponse.CheckedChanged += new System.EventHandler(this.OutlineResponse_CheckedChanged);
            // 
            // InstructionsGroup
            // 
            this.InstructionsGroup.Location = new System.Drawing.Point(6, 282);
            this.InstructionsGroup.Name = "InstructionsGroup";
            this.InstructionsGroup.Size = new System.Drawing.Size(353, 82);
            this.InstructionsGroup.TabIndex = 10;
            this.InstructionsGroup.TabStop = false;
            this.InstructionsGroup.Text = "Mock Item Instructions";
            // 
            // ImageStimulusGroup
            // 
            this.ImageStimulusGroup.Controls.Add(this.StretchToFit);
            this.ImageStimulusGroup.Controls.Add(this.ImageFileName);
            this.ImageStimulusGroup.Controls.Add(this.Browse);
            this.ImageStimulusGroup.Controls.Add(this.ImageFileNameLabel);
            this.ImageStimulusGroup.Location = new System.Drawing.Point(6, 161);
            this.ImageStimulusGroup.Name = "ImageStimulusGroup";
            this.ImageStimulusGroup.Size = new System.Drawing.Size(353, 92);
            this.ImageStimulusGroup.TabIndex = 9;
            this.ImageStimulusGroup.TabStop = false;
            this.ImageStimulusGroup.Text = "Image Stimulus";
            // 
            // StretchToFit
            // 
            this.StretchToFit.AutoSize = true;
            this.StretchToFit.Location = new System.Drawing.Point(96, 47);
            this.StretchToFit.Name = "StretchToFit";
            this.StretchToFit.Size = new System.Drawing.Size(170, 17);
            this.StretchToFit.TabIndex = 3;
            this.StretchToFit.Text = "Stretch to fit stimulus rectangle";
            this.StretchToFit.UseVisualStyleBackColor = true;
            this.StretchToFit.CheckedChanged += new System.EventHandler(this.StretchToFit_CheckedChanged);
            // 
            // ImageFileName
            // 
            this.ImageFileName.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ImageFileName.Location = new System.Drawing.Point(96, 21);
            this.ImageFileName.Name = "ImageFileName";
            this.ImageFileName.ReadOnly = true;
            this.ImageFileName.Size = new System.Drawing.Size(170, 20);
            this.ImageFileName.TabIndex = 2;
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(272, 19);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(75, 23);
            this.Browse.TabIndex = 1;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // ImageFileNameLabel
            // 
            this.ImageFileNameLabel.AutoSize = true;
            this.ImageFileNameLabel.Location = new System.Drawing.Point(6, 24);
            this.ImageFileNameLabel.Name = "ImageFileNameLabel";
            this.ImageFileNameLabel.Size = new System.Drawing.Size(84, 13);
            this.ImageFileNameLabel.TabIndex = 0;
            this.ImageFileNameLabel.Text = "Image Filename:";
            // 
            // TextStimulusGroup
            // 
            this.TextStimulusGroup.Location = new System.Drawing.Point(6, 92);
            this.TextStimulusGroup.Name = "TextStimulusGroup";
            this.TextStimulusGroup.Size = new System.Drawing.Size(353, 63);
            this.TextStimulusGroup.TabIndex = 8;
            this.TextStimulusGroup.TabStop = false;
            this.TextStimulusGroup.Text = "Text Stimulus";
            // 
            // KeyedDirLabel
            // 
            this.KeyedDirLabel.AutoSize = true;
            this.KeyedDirLabel.Location = new System.Drawing.Point(200, 48);
            this.KeyedDirLabel.Name = "KeyedDirLabel";
            this.KeyedDirLabel.Size = new System.Drawing.Size(85, 13);
            this.KeyedDirLabel.TabIndex = 7;
            this.KeyedDirLabel.Text = "Keyed Direction:";
            // 
            // KeyedDirDrop
            // 
            this.KeyedDirDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.KeyedDirDrop.FormattingEnabled = true;
            this.KeyedDirDrop.Location = new System.Drawing.Point(291, 45);
            this.KeyedDirDrop.Name = "KeyedDirDrop";
            this.KeyedDirDrop.Size = new System.Drawing.Size(68, 21);
            this.KeyedDirDrop.TabIndex = 6;
            this.KeyedDirDrop.SelectedIndexChanged += new System.EventHandler(this.KeyedDirDrop_SelectedIndexChanged);
            // 
            // ImageStimulusRadio
            // 
            this.ImageStimulusRadio.AutoSize = true;
            this.ImageStimulusRadio.Location = new System.Drawing.Point(88, 69);
            this.ImageStimulusRadio.Name = "ImageStimulusRadio";
            this.ImageStimulusRadio.Size = new System.Drawing.Size(54, 17);
            this.ImageStimulusRadio.TabIndex = 5;
            this.ImageStimulusRadio.TabStop = true;
            this.ImageStimulusRadio.Text = "Image";
            this.ImageStimulusRadio.UseVisualStyleBackColor = true;
            this.ImageStimulusRadio.CheckedChanged += new System.EventHandler(this.ImageStimulusRadio_CheckedChanged);
            // 
            // TextStimulusRadio
            // 
            this.TextStimulusRadio.AutoSize = true;
            this.TextStimulusRadio.Location = new System.Drawing.Point(88, 46);
            this.TextStimulusRadio.Name = "TextStimulusRadio";
            this.TextStimulusRadio.Size = new System.Drawing.Size(46, 17);
            this.TextStimulusRadio.TabIndex = 4;
            this.TextStimulusRadio.TabStop = true;
            this.TextStimulusRadio.Text = "Text";
            this.TextStimulusRadio.UseVisualStyleBackColor = true;
            this.TextStimulusRadio.CheckedChanged += new System.EventHandler(this.TextStimulusRadio_CheckedChanged);
            // 
            // StimulusTypeLabel
            // 
            this.StimulusTypeLabel.AutoSize = true;
            this.StimulusTypeLabel.Location = new System.Drawing.Point(9, 48);
            this.StimulusTypeLabel.Name = "StimulusTypeLabel";
            this.StimulusTypeLabel.Size = new System.Drawing.Size(76, 13);
            this.StimulusTypeLabel.TabIndex = 3;
            this.StimulusTypeLabel.Text = "Stimulus Type:";
            // 
            // ManageKeysButton
            // 
            this.ManageKeysButton.Location = new System.Drawing.Point(228, 17);
            this.ManageKeysButton.Name = "ManageKeysButton";
            this.ManageKeysButton.Size = new System.Drawing.Size(131, 23);
            this.ManageKeysButton.TabIndex = 2;
            this.ManageKeysButton.Text = "Create / Manage Keys";
            this.ManageKeysButton.UseVisualStyleBackColor = true;
            this.ManageKeysButton.Click += new System.EventHandler(this.ManageKeysButton_Click);
            // 
            // ResponseKeyDrop
            // 
            this.ResponseKeyDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ResponseKeyDrop.FormattingEnabled = true;
            this.ResponseKeyDrop.Location = new System.Drawing.Point(91, 19);
            this.ResponseKeyDrop.Name = "ResponseKeyDrop";
            this.ResponseKeyDrop.Size = new System.Drawing.Size(113, 21);
            this.ResponseKeyDrop.TabIndex = 1;
            this.ResponseKeyDrop.SelectedIndexChanged += new System.EventHandler(this.ResponseKeyDrop_SelectedIndexChanged);
            // 
            // ResponseKeyLabel
            // 
            this.ResponseKeyLabel.AutoSize = true;
            this.ResponseKeyLabel.Location = new System.Drawing.Point(6, 22);
            this.ResponseKeyLabel.Name = "ResponseKeyLabel";
            this.ResponseKeyLabel.Size = new System.Drawing.Size(79, 13);
            this.ResponseKeyLabel.TabIndex = 0;
            this.ResponseKeyLabel.Text = "Response Key:";
            // 
            // MockItemPanel
            // 
            this.Name = "MockItemPanel";
            this.ImageStimulusGroup.ResumeLayout(false);
            this.ImageStimulusGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton ImageStimulusRadio;
        private System.Windows.Forms.RadioButton TextStimulusRadio;
        private System.Windows.Forms.Label StimulusTypeLabel;
        private System.Windows.Forms.Button ManageKeysButton;
        private System.Windows.Forms.ComboBox ResponseKeyDrop;
        private System.Windows.Forms.Label ResponseKeyLabel;
        private System.Windows.Forms.GroupBox ImageStimulusGroup;
        private System.Windows.Forms.GroupBox TextStimulusGroup;
        private System.Windows.Forms.Label KeyedDirLabel;
        private System.Windows.Forms.ComboBox KeyedDirDrop;
        private System.Windows.Forms.TextBox ImageFileName;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.Label ImageFileNameLabel;
        private System.Windows.Forms.CheckBox StretchToFit;
        private System.Windows.Forms.GroupBox InstructionsGroup;
        private System.Windows.Forms.CheckBox OutlineResponse;
        private System.Windows.Forms.CheckBox MarkAsInvalid;
    }
}
