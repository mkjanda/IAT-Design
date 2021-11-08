namespace IATClient
{
    public partial class ResponseKeyPanel
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
            this.CreateResponseKeyButton = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ModifyResponseKeyCombo = new System.Windows.Forms.ComboBox();
            this.SimpleRadio = new System.Windows.Forms.RadioButton();
            this.ReversedRadio = new System.Windows.Forms.RadioButton();
            this.CombinedRadio = new System.Windows.Forms.RadioButton();
            this.ResponseKeyNameLabel = new System.Windows.Forms.Label();
            this.ResponseKeyName = new System.Windows.Forms.TextBox();
            this.CreateButton = new System.Windows.Forms.Button();
            this.BackButton = new System.Windows.Forms.Button();
            this.ResponseKeyGroup = new System.Windows.Forms.GroupBox();
            this.Instructions = new System.Windows.Forms.TextBox();
            this.InstructionsGroup = new System.Windows.Forms.GroupBox();
            this.LeftKeyGroup = new System.Windows.Forms.GroupBox();
            this.RightKeyBox = new System.Windows.Forms.GroupBox();
            this.ResponseKeyGroup.SuspendLayout();
            this.InstructionsGroup.SuspendLayout();
            this.LeftKeyGroup.SuspendLayout();
            this.RightKeyBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // CreateResponseKeyButton
            // 
            this.CreateResponseKeyButton.Location = new System.Drawing.Point(31, 103);
            this.CreateResponseKeyButton.Name = "CreateResponseKeyButton";
            this.CreateResponseKeyButton.Size = new System.Drawing.Size(147, 23);
            this.CreateResponseKeyButton.TabIndex = 1;
            this.CreateResponseKeyButton.Text = "Create New Resonse Key";
            this.CreateResponseKeyButton.UseVisualStyleBackColor = true;
            this.CreateResponseKeyButton.Click += new System.EventHandler(this.CreateResponseKeyButton_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(184, 108);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(181, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Or Select a Response Key to Modify:";
            // 
            // ModifyResponseKeyCombo
            // 
            this.ModifyResponseKeyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ModifyResponseKeyCombo.FormattingEnabled = true;
            this.ModifyResponseKeyCombo.Location = new System.Drawing.Point(371, 105);
            this.ModifyResponseKeyCombo.Name = "ModifyResponseKeyCombo";
            this.ModifyResponseKeyCombo.Size = new System.Drawing.Size(144, 21);
            this.ModifyResponseKeyCombo.TabIndex = 3;
            this.ModifyResponseKeyCombo.SelectedIndexChanged += new System.EventHandler(this.ModifyResponseKeyCombo_SelectedIndexChanged);
            // 
            // SimpleRadio
            // 
            this.SimpleRadio.AutoSize = true;
            this.SimpleRadio.Location = new System.Drawing.Point(10, 47);
            this.SimpleRadio.Name = "SimpleRadio";
            this.SimpleRadio.Size = new System.Drawing.Size(128, 17);
            this.SimpleRadio.TabIndex = 4;
            this.SimpleRadio.TabStop = true;
            this.SimpleRadio.Text = "Simple Response Key";
            this.SimpleRadio.UseVisualStyleBackColor = true;
            this.SimpleRadio.CheckedChanged += new System.EventHandler(this.SimpleRadio_CheckedChanged);
            // 
            // ReversedRadio
            // 
            this.ReversedRadio.AutoSize = true;
            this.ReversedRadio.Location = new System.Drawing.Point(144, 47);
            this.ReversedRadio.Name = "ReversedRadio";
            this.ReversedRadio.Size = new System.Drawing.Size(143, 17);
            this.ReversedRadio.TabIndex = 5;
            this.ReversedRadio.TabStop = true;
            this.ReversedRadio.Text = "Reversed Response Key";
            this.ReversedRadio.UseVisualStyleBackColor = true;
            this.ReversedRadio.CheckedChanged += new System.EventHandler(this.ReversedRadio_CheckedChanged);
            // 
            // CombinedRadio
            // 
            this.CombinedRadio.AutoSize = true;
            this.CombinedRadio.Location = new System.Drawing.Point(293, 47);
            this.CombinedRadio.Name = "CombinedRadio";
            this.CombinedRadio.Size = new System.Drawing.Size(144, 17);
            this.CombinedRadio.TabIndex = 6;
            this.CombinedRadio.TabStop = true;
            this.CombinedRadio.Text = "Combined Response Key";
            this.CombinedRadio.UseVisualStyleBackColor = true;
            this.CombinedRadio.CheckedChanged += new System.EventHandler(this.CombinedRadio_CheckedChanged);
            // 
            // ResponseKeyNameLabel
            // 
            this.ResponseKeyNameLabel.AutoSize = true;
            this.ResponseKeyNameLabel.Location = new System.Drawing.Point(54, 22);
            this.ResponseKeyNameLabel.Name = "ResponseKeyNameLabel";
            this.ResponseKeyNameLabel.Size = new System.Drawing.Size(110, 13);
            this.ResponseKeyNameLabel.TabIndex = 8;
            this.ResponseKeyNameLabel.Text = "Response Key Name:";
            // 
            // ResponseKeyName
            // 
            this.ResponseKeyName.Location = new System.Drawing.Point(168, 19);
            this.ResponseKeyName.Name = "ResponseKeyName";
            this.ResponseKeyName.Size = new System.Drawing.Size(124, 20);
            this.ResponseKeyName.TabIndex = 9;
            this.ResponseKeyName.TextChanged += new System.EventHandler(this.ResponseKeyName_TextChanged);
            // 
            // CreateButton
            // 
            this.CreateButton.Location = new System.Drawing.Point(541, 461);
            this.CreateButton.Name = "CreateButton";
            this.CreateButton.Size = new System.Drawing.Size(111, 35);
            this.CreateButton.TabIndex = 12;
            this.CreateButton.Text = "Create Response";
            this.CreateButton.UseVisualStyleBackColor = true;
            this.CreateButton.Click += new System.EventHandler(this.CreateButton_Click);
            // 
            // BackButton
            // 
            this.BackButton.Location = new System.Drawing.Point(667, 461);
            this.BackButton.Name = "BackButton";
            this.BackButton.Size = new System.Drawing.Size(111, 35);
            this.BackButton.TabIndex = 13;
            this.BackButton.Text = "Previous Window";
            this.BackButton.UseVisualStyleBackColor = true;
            this.BackButton.Click += new System.EventHandler(this.BackButton_Click);
            // 
            // ResponseKeyGroup
            // 
            this.ResponseKeyGroup.Controls.Add(this.ResponseKeyName);
            this.ResponseKeyGroup.Controls.Add(this.ResponseKeyNameLabel);
            this.ResponseKeyGroup.Controls.Add(this.CombinedRadio);
            this.ResponseKeyGroup.Controls.Add(this.ReversedRadio);
            this.ResponseKeyGroup.Controls.Add(this.SimpleRadio);
            this.ResponseKeyGroup.Location = new System.Drawing.Point(50, 132);
            this.ResponseKeyGroup.Name = "ResponseKeyGroup";
            this.ResponseKeyGroup.Size = new System.Drawing.Size(444, 82);
            this.ResponseKeyGroup.TabIndex = 15;
            this.ResponseKeyGroup.TabStop = false;
            this.ResponseKeyGroup.Text = "Response Key";
            // 
            // Instructions
            // 
            this.Instructions.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Instructions.Location = new System.Drawing.Point(6, 19);
            this.Instructions.Multiline = true;
            this.Instructions.Name = "Instructions";
            this.Instructions.ReadOnly = true;
            this.Instructions.Size = new System.Drawing.Size(769, 66);
            this.Instructions.TabIndex = 16;
            // 
            // InstructionsGroup
            // 
            this.InstructionsGroup.Controls.Add(this.Instructions);
            this.InstructionsGroup.Location = new System.Drawing.Point(3, 3);
            this.InstructionsGroup.Name = "InstructionsGroup";
            this.InstructionsGroup.Size = new System.Drawing.Size(781, 96);
            this.InstructionsGroup.TabIndex = 17;
            this.InstructionsGroup.TabStop = false;
            this.InstructionsGroup.Text = "Instructions";
            // 
            // LeftKeyGroup
            // 
            this.LeftKeyGroup.Controls.Add(this.LeftPreviewPane);
            this.LeftKeyGroup.Location = new System.Drawing.Point(520, 105);
            this.LeftKeyGroup.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.LeftKeyGroup.Name = "LeftKeyGroup";
            this.LeftKeyGroup.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.LeftKeyGroup.Size = new System.Drawing.Size(258, 173);
            this.LeftKeyGroup.TabIndex = 18;
            this.LeftKeyGroup.TabStop = false;
            this.LeftKeyGroup.Text = "Left Key Preview";
            // 
            // RightKeyBox
            // 
            this.RightKeyBox.Controls.Add(this.RightPreviewPane);
            this.RightKeyBox.Location = new System.Drawing.Point(520, 282);
            this.RightKeyBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RightKeyBox.Name = "RightKeyBox";
            this.RightKeyBox.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.RightKeyBox.Size = new System.Drawing.Size(258, 173);
            this.RightKeyBox.TabIndex = 19;
            this.RightKeyBox.TabStop = false;
            this.RightKeyBox.Text = "Right Key Preview";
            // 
            // RightPreviewPane
            // 
            // ResponseKeyPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RightKeyBox);
            this.Controls.Add(this.LeftKeyGroup);
            this.Controls.Add(this.InstructionsGroup);
            this.Controls.Add(this.ResponseKeyGroup);
            this.Controls.Add(this.BackButton);
            this.Controls.Add(this.CreateButton);
            this.Controls.Add(this.ModifyResponseKeyCombo);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.CreateResponseKeyButton);
            this.Name = "ResponseKeyPanel";
            this.Size = new System.Drawing.Size(787, 505);
            this.Load += new System.EventHandler(this.ResponseKeyPanel_Load);
            this.ParentChanged += new System.EventHandler(this.ResponseKeyPanel_ParentChanged);
            this.ResponseKeyGroup.ResumeLayout(false);
            this.ResponseKeyGroup.PerformLayout();
            this.InstructionsGroup.ResumeLayout(false);
            this.InstructionsGroup.PerformLayout();
            this.LeftKeyGroup.ResumeLayout(false);
            this.RightKeyBox.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button CreateResponseKeyButton;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ModifyResponseKeyCombo;
        private System.Windows.Forms.RadioButton SimpleRadio;
        private System.Windows.Forms.RadioButton ReversedRadio;
        private System.Windows.Forms.RadioButton CombinedRadio;
        private System.Windows.Forms.Label ResponseKeyNameLabel;
        private System.Windows.Forms.TextBox ResponseKeyName;
        private System.Windows.Forms.Button CreateButton;
        private System.Windows.Forms.Button BackButton;
        private System.Windows.Forms.GroupBox ResponseKeyGroup;
        private System.Windows.Forms.TextBox Instructions;
        private System.Windows.Forms.GroupBox InstructionsGroup;
        private System.Windows.Forms.GroupBox LeftKeyGroup;
        private System.Windows.Forms.GroupBox RightKeyBox;

    }
}
