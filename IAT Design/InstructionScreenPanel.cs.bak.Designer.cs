namespace IATClient
{
    partial class InstructionScreenPanel
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
            this.ContinueKeyLabel = new System.Windows.Forms.Label();
            this.ContinueKeyDrop = new System.Windows.Forms.ComboBox();
            this.InstructionTypeLabel = new System.Windows.Forms.Label();
            this.TextRadio = new System.Windows.Forms.RadioButton();
            this.MockItemRadio = new System.Windows.Forms.RadioButton();
            this.KeyRadio = new System.Windows.Forms.RadioButton();
            this.InsertScreen = new System.Windows.Forms.Button();
            this.AddScreen = new System.Windows.Forms.Button();
            this.DeleteScreen = new System.Windows.Forms.Button();
            this.Done = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // ContinueKeyLabel
            // 
            this.ContinueKeyLabel.AutoSize = true;
            this.ContinueKeyLabel.Location = new System.Drawing.Point(602, 413);
            this.ContinueKeyLabel.Name = "ContinueKeyLabel";
            this.ContinueKeyLabel.Size = new System.Drawing.Size(228, 13);
            this.ContinueKeyLabel.TabIndex = 1;
            this.ContinueKeyLabel.Text = "Select the key the user must press to continue:";
            // 
            // ContinueKeyDrop
            // 
            this.ContinueKeyDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ContinueKeyDrop.FormattingEnabled = true;
            this.ContinueKeyDrop.Location = new System.Drawing.Point(836, 410);
            this.ContinueKeyDrop.Name = "ContinueKeyDrop";
            this.ContinueKeyDrop.Size = new System.Drawing.Size(66, 21);
            this.ContinueKeyDrop.TabIndex = 2;
            this.ContinueKeyDrop.SelectedIndexChanged += new System.EventHandler(this.ContinueKeyDrop_SelectedIndexChanged);
            // 
            // InstructionTypeLabel
            // 
            this.InstructionTypeLabel.AutoSize = true;
            this.InstructionTypeLabel.Location = new System.Drawing.Point(574, 12);
            this.InstructionTypeLabel.Name = "InstructionTypeLabel";
            this.InstructionTypeLabel.Size = new System.Drawing.Size(117, 13);
            this.InstructionTypeLabel.TabIndex = 5;
            this.InstructionTypeLabel.Text = "Instruction screen type:";
            // 
            // TextRadio
            // 
            this.TextRadio.AutoSize = true;
            this.TextRadio.Location = new System.Drawing.Point(697, 10);
            this.TextRadio.Name = "TextRadio";
            this.TextRadio.Size = new System.Drawing.Size(46, 17);
            this.TextRadio.TabIndex = 6;
            this.TextRadio.TabStop = true;
            this.TextRadio.Text = "Text";
            this.TextRadio.UseVisualStyleBackColor = true;
            this.TextRadio.CheckedChanged += new System.EventHandler(this.TextRadio_CheckedChanged);
            // 
            // MockItemRadio
            // 
            this.MockItemRadio.AutoSize = true;
            this.MockItemRadio.Location = new System.Drawing.Point(870, 10);
            this.MockItemRadio.Name = "MockItemRadio";
            this.MockItemRadio.Size = new System.Drawing.Size(75, 17);
            this.MockItemRadio.TabIndex = 8;
            this.MockItemRadio.TabStop = true;
            this.MockItemRadio.Text = "Mock Item";
            this.MockItemRadio.UseVisualStyleBackColor = true;
            this.MockItemRadio.CheckedChanged += new System.EventHandler(this.MockItemRadio_CheckedChanged);
            // 
            // KeyRadio
            // 
            this.KeyRadio.AutoSize = true;
            this.KeyRadio.Location = new System.Drawing.Point(743, 10);
            this.KeyRadio.Name = "KeyRadio";
            this.KeyRadio.Size = new System.Drawing.Size(127, 17);
            this.KeyRadio.TabIndex = 14;
            this.KeyRadio.TabStop = true;
            this.KeyRadio.Text = "Text && Response Key";
            this.KeyRadio.UseVisualStyleBackColor = true;
            this.KeyRadio.CheckedChanged += new System.EventHandler(this.KeyRadio_CheckedChanged);
            // 
            // InsertScreen
            // 
            this.InsertScreen.Location = new System.Drawing.Point(914, 555);
            this.InsertScreen.Name = "InsertScreen";
            this.InsertScreen.Size = new System.Drawing.Size(96, 25);
            this.InsertScreen.TabIndex = 15;
            this.InsertScreen.Text = "Insert Screen";
            this.InsertScreen.UseVisualStyleBackColor = true;
            this.InsertScreen.Click += new System.EventHandler(this.InsertScreen_Click);
            // 
            // AddScreen
            // 
            this.AddScreen.Location = new System.Drawing.Point(914, 523);
            this.AddScreen.Name = "AddScreen";
            this.AddScreen.Size = new System.Drawing.Size(96, 25);
            this.AddScreen.TabIndex = 16;
            this.AddScreen.Text = "Add Screen";
            this.AddScreen.UseVisualStyleBackColor = true;
            this.AddScreen.Click += new System.EventHandler(this.AddScreen_Click);
            // 
            // DeleteScreen
            // 
            this.DeleteScreen.Location = new System.Drawing.Point(914, 587);
            this.DeleteScreen.Name = "DeleteScreen";
            this.DeleteScreen.Size = new System.Drawing.Size(96, 25);
            this.DeleteScreen.TabIndex = 17;
            this.DeleteScreen.Text = "Delete Screen";
            this.DeleteScreen.UseVisualStyleBackColor = true;
            this.DeleteScreen.Click += new System.EventHandler(this.DeleteScreen_Click);
            // 
            // Done
            // 
            this.Done.Location = new System.Drawing.Point(914, 619);
            this.Done.Name = "Done";
            this.Done.Size = new System.Drawing.Size(96, 25);
            this.Done.TabIndex = 18;
            this.Done.Text = "Done";
            this.Done.UseVisualStyleBackColor = true;
            this.Done.Click += new System.EventHandler(this.Done_Click);
            // 
            // InstructionScreenPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.Done);
            this.Controls.Add(this.DeleteScreen);
            this.Controls.Add(this.AddScreen);
            this.Controls.Add(this.InsertScreen);
            this.Controls.Add(this.KeyRadio);
            this.Controls.Add(this.MockItemRadio);
            this.Controls.Add(this.TextRadio);
            this.Controls.Add(this.InstructionTypeLabel);
            this.Controls.Add(this.ContinueKeyDrop);
            this.Controls.Add(this.ContinueKeyLabel);
            this.MinimumSize = new System.Drawing.Size(1010, 645);
            this.Name = "InstructionScreenPanel";
            this.Size = new System.Drawing.Size(1010, 645);
            this.Load += new System.EventHandler(this.InstructionScreenPanel_Load);
            this.ParentChanged += new System.EventHandler(this.InstructionScreenPanel_ParentChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label ContinueKeyLabel;
        private System.Windows.Forms.ComboBox ContinueKeyDrop;
        private System.Windows.Forms.Label InstructionTypeLabel;
        private System.Windows.Forms.RadioButton TextRadio;
        private System.Windows.Forms.RadioButton MockItemRadio;
        private System.Windows.Forms.RadioButton KeyRadio;
        private System.Windows.Forms.Button InsertScreen;
        private System.Windows.Forms.Button AddScreen;
        private System.Windows.Forms.Button DeleteScreen;
        private System.Windows.Forms.Button Done;
    }
}
