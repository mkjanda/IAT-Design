namespace IATClient
{
    partial class ResponseTypeRadios
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
            this.AttachFileRadio = new System.Windows.Forms.RadioButton();
            this.DateRadio = new System.Windows.Forms.RadioButton();
            this.InstructionRadio = new System.Windows.Forms.RadioButton();
            this.LikertRadio = new System.Windows.Forms.RadioButton();
            this.MultiChoiceRadio = new System.Windows.Forms.RadioButton();
            this.MultiSelectRadio = new System.Windows.Forms.RadioButton();
            this.TextRadio = new System.Windows.Forms.RadioButton();
            this.WeightedMultiChoiceRadio = new System.Windows.Forms.RadioButton();
            this.ResponseRadiosGroup = new System.Windows.Forms.GroupBox();
            this.BoolRadio = new System.Windows.Forms.RadioButton();
            this.ResponseRadiosGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // AttachFileRadio
            // 
            this.AttachFileRadio.AutoSize = true;
            this.AttachFileRadio.Location = new System.Drawing.Point(6, 203);
            this.AttachFileRadio.Name = "AttachFileRadio";
            this.AttachFileRadio.Size = new System.Drawing.Size(179, 17);
            this.AttachFileRadio.TabIndex = 9;
            this.AttachFileRadio.TabStop = true;
            this.AttachFileRadio.Text = "Attach File with Valid Responses";
            this.AttachFileRadio.UseVisualStyleBackColor = true;
            this.AttachFileRadio.CheckedChanged += new System.EventHandler(this.AttachFileRadio_CheckedChanged);
            // 
            // DateRadio
            // 
            this.DateRadio.AutoSize = true;
            this.DateRadio.Location = new System.Drawing.Point(6, 19);
            this.DateRadio.Name = "DateRadio";
            this.DateRadio.Size = new System.Drawing.Size(48, 17);
            this.DateRadio.TabIndex = 1;
            this.DateRadio.TabStop = true;
            this.DateRadio.Text = "Date";
            this.DateRadio.UseVisualStyleBackColor = true;
            this.DateRadio.CheckedChanged += new System.EventHandler(this.DateRadio_CheckedChanged);
            // 
            // InstructionRadio
            // 
            this.InstructionRadio.AutoSize = true;
            this.InstructionRadio.Location = new System.Drawing.Point(6, 42);
            this.InstructionRadio.Name = "InstructionRadio";
            this.InstructionRadio.Size = new System.Drawing.Size(141, 17);
            this.InstructionRadio.TabIndex = 2;
            this.InstructionRadio.TabStop = true;
            this.InstructionRadio.Text = "Instruction (no response)";
            this.InstructionRadio.UseVisualStyleBackColor = true;
            this.InstructionRadio.CheckedChanged += new System.EventHandler(this.InstructionRadio_CheckedChanged);
            // 
            // LikertRadio
            // 
            this.LikertRadio.AutoSize = true;
            this.LikertRadio.Location = new System.Drawing.Point(6, 65);
            this.LikertRadio.Name = "LikertRadio";
            this.LikertRadio.Size = new System.Drawing.Size(51, 17);
            this.LikertRadio.TabIndex = 3;
            this.LikertRadio.TabStop = true;
            this.LikertRadio.Text = "Likert";
            this.LikertRadio.UseVisualStyleBackColor = true;
            this.LikertRadio.CheckedChanged += new System.EventHandler(this.LikertRadio_CheckedChanged);
            // 
            // MultiChoiceRadio
            // 
            this.MultiChoiceRadio.AutoSize = true;
            this.MultiChoiceRadio.Location = new System.Drawing.Point(6, 88);
            this.MultiChoiceRadio.Name = "MultiChoiceRadio";
            this.MultiChoiceRadio.Size = new System.Drawing.Size(97, 17);
            this.MultiChoiceRadio.TabIndex = 4;
            this.MultiChoiceRadio.TabStop = true;
            this.MultiChoiceRadio.Text = "Multiple Choice";
            this.MultiChoiceRadio.UseVisualStyleBackColor = true;
            this.MultiChoiceRadio.CheckedChanged += new System.EventHandler(this.MultiChoiceRadio_CheckedChanged);
            // 
            // MultiSelectRadio
            // 
            this.MultiSelectRadio.AutoSize = true;
            this.MultiSelectRadio.Location = new System.Drawing.Point(6, 111);
            this.MultiSelectRadio.Name = "MultiSelectRadio";
            this.MultiSelectRadio.Size = new System.Drawing.Size(108, 17);
            this.MultiSelectRadio.TabIndex = 5;
            this.MultiSelectRadio.TabStop = true;
            this.MultiSelectRadio.Text = "Multiple Selection";
            this.MultiSelectRadio.UseVisualStyleBackColor = true;
            this.MultiSelectRadio.CheckedChanged += new System.EventHandler(this.MultiSelectRadio_CheckedChanged);
            // 
            // TextRadio
            // 
            this.TextRadio.AutoSize = true;
            this.TextRadio.Location = new System.Drawing.Point(6, 134);
            this.TextRadio.Name = "TextRadio";
            this.TextRadio.Size = new System.Drawing.Size(98, 17);
            this.TextRadio.TabIndex = 7;
            this.TextRadio.TabStop = true;
            this.TextRadio.Text = "Text or Number";
            this.TextRadio.UseVisualStyleBackColor = true;
            this.TextRadio.CheckedChanged += new System.EventHandler(this.TextRadio_CheckedChanged);
            // 
            // WeightedMultiChoiceRadio
            // 
            this.WeightedMultiChoiceRadio.AutoSize = true;
            this.WeightedMultiChoiceRadio.Location = new System.Drawing.Point(6, 180);
            this.WeightedMultiChoiceRadio.Name = "WeightedMultiChoiceRadio";
            this.WeightedMultiChoiceRadio.Size = new System.Drawing.Size(146, 17);
            this.WeightedMultiChoiceRadio.TabIndex = 8;
            this.WeightedMultiChoiceRadio.TabStop = true;
            this.WeightedMultiChoiceRadio.Text = "Weighted Multiple Choice";
            this.WeightedMultiChoiceRadio.UseVisualStyleBackColor = true;
            this.WeightedMultiChoiceRadio.CheckedChanged += new System.EventHandler(this.WeightedMultiChoiceRadio_CheckedChanged);
            // 
            // ResponseRadiosGroup
            // 
            this.ResponseRadiosGroup.Controls.Add(this.BoolRadio);
            this.ResponseRadiosGroup.Controls.Add(this.WeightedMultiChoiceRadio);
            this.ResponseRadiosGroup.Controls.Add(this.TextRadio);
            this.ResponseRadiosGroup.Controls.Add(this.MultiSelectRadio);
            this.ResponseRadiosGroup.Controls.Add(this.MultiChoiceRadio);
            this.ResponseRadiosGroup.Controls.Add(this.LikertRadio);
            this.ResponseRadiosGroup.Controls.Add(this.InstructionRadio);
            this.ResponseRadiosGroup.Controls.Add(this.DateRadio);
            this.ResponseRadiosGroup.Controls.Add(this.AttachFileRadio);
            this.ResponseRadiosGroup.Location = new System.Drawing.Point(3, 3);
            this.ResponseRadiosGroup.Name = "ResponseRadiosGroup";
            this.ResponseRadiosGroup.Size = new System.Drawing.Size(220, 228);
            this.ResponseRadiosGroup.TabIndex = 9;
            this.ResponseRadiosGroup.TabStop = false;
            this.ResponseRadiosGroup.Text = "Response Type";
            // 
            // BoolRadio
            // 
            this.BoolRadio.AutoSize = true;
            this.BoolRadio.Location = new System.Drawing.Point(6, 157);
            this.BoolRadio.Name = "BoolRadio";
            this.BoolRadio.Size = new System.Drawing.Size(83, 17);
            this.BoolRadio.TabIndex = 9;
            this.BoolRadio.TabStop = true;
            this.BoolRadio.Text = "True / False";
            this.BoolRadio.UseVisualStyleBackColor = true;
            this.BoolRadio.CheckedChanged += new System.EventHandler(this.BoolRadio_CheckedChanged);
            // 
            // ResponseTypeRadios
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ResponseRadiosGroup);
            this.Name = "ResponseTypeRadios";
            this.Size = new System.Drawing.Size(226, 234);
            this.ResponseRadiosGroup.ResumeLayout(false);
            this.ResponseRadiosGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RadioButton AttachFileRadio;
        private System.Windows.Forms.RadioButton DateRadio;
        private System.Windows.Forms.RadioButton InstructionRadio;
        private System.Windows.Forms.RadioButton LikertRadio;
        private System.Windows.Forms.RadioButton MultiChoiceRadio;
        private System.Windows.Forms.RadioButton MultiSelectRadio;
        private System.Windows.Forms.RadioButton TextRadio;
        private System.Windows.Forms.RadioButton WeightedMultiChoiceRadio;
        private System.Windows.Forms.GroupBox ResponseRadiosGroup;
        private System.Windows.Forms.RadioButton BoolRadio;
    }
}
