namespace IATClient
{
    public partial class IATBlockPanel
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
            this.InstructionsLabel = new System.Windows.Forms.Label();
            this.AddStimulus = new System.Windows.Forms.Button();
            this.InsertStimulus = new System.Windows.Forms.Button();
            this.DeleteStimulus = new System.Windows.Forms.Button();
            this.Done = new System.Windows.Forms.Button();
            this.ResponseKeyLabel = new System.Windows.Forms.Label();
            this.ResponseKeyDrop = new System.Windows.Forms.ComboBox();
            this.ManageKeys = new System.Windows.Forms.Button();
            this.DynamicallyKeyedCheck = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // InstructionsLabel
            // 
            this.InstructionsLabel.AutoSize = true;
            this.InstructionsLabel.Location = new System.Drawing.Point(755, 450);
            this.InstructionsLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.InstructionsLabel.Name = "InstructionsLabel";
            this.InstructionsLabel.Size = new System.Drawing.Size(250, 17);
            this.InstructionsLabel.TabIndex = 3;
            this.InstructionsLabel.Text = "Enter Instructions for item block below:";
            // 
            // AddStimulus
            // 
            this.AddStimulus.Location = new System.Drawing.Point(1215, 482);
            this.AddStimulus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.AddStimulus.Name = "AddStimulus";
            this.AddStimulus.Size = new System.Drawing.Size(128, 28);
            this.AddStimulus.TabIndex = 4;
            this.AddStimulus.Text = "Add Stimulus";
            this.AddStimulus.UseVisualStyleBackColor = true;
            this.AddStimulus.Click += new System.EventHandler(this.AddStimulus_Click);
            // 
            // InsertStimulus
            // 
            this.InsertStimulus.Location = new System.Drawing.Point(1215, 521);
            this.InsertStimulus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.InsertStimulus.Name = "InsertStimulus";
            this.InsertStimulus.Size = new System.Drawing.Size(128, 28);
            this.InsertStimulus.TabIndex = 5;
            this.InsertStimulus.Text = "Insert Stimulus";
            this.InsertStimulus.UseVisualStyleBackColor = true;
            this.InsertStimulus.Click += new System.EventHandler(this.InsertStimulus_Click);
            // 
            // DeleteStimulus
            // 
            this.DeleteStimulus.Location = new System.Drawing.Point(1215, 559);
            this.DeleteStimulus.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DeleteStimulus.Name = "DeleteStimulus";
            this.DeleteStimulus.Size = new System.Drawing.Size(128, 28);
            this.DeleteStimulus.TabIndex = 6;
            this.DeleteStimulus.Text = "Delete Stimulus";
            this.DeleteStimulus.UseVisualStyleBackColor = true;
            this.DeleteStimulus.Click += new System.EventHandler(this.DeleteStimulus_Click);
            // 
            // Done
            // 
            this.Done.Location = new System.Drawing.Point(1215, 597);
            this.Done.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.Done.Name = "Done";
            this.Done.Size = new System.Drawing.Size(128, 28);
            this.Done.TabIndex = 7;
            this.Done.Text = "Done";
            this.Done.UseVisualStyleBackColor = true;
            this.Done.Click += new System.EventHandler(this.Done_Click);
            // 
            // ResponseKeyLabel
            // 
            this.ResponseKeyLabel.AutoSize = true;
            this.ResponseKeyLabel.Location = new System.Drawing.Point(755, 36);
            this.ResponseKeyLabel.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.ResponseKeyLabel.Name = "ResponseKeyLabel";
            this.ResponseKeyLabel.Size = new System.Drawing.Size(104, 17);
            this.ResponseKeyLabel.TabIndex = 0;
            this.ResponseKeyLabel.Text = "Response Key:";
            // 
            // ResponseKeyDrop
            // 
            this.ResponseKeyDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ResponseKeyDrop.FormattingEnabled = true;
            this.ResponseKeyDrop.Location = new System.Drawing.Point(868, 32);
            this.ResponseKeyDrop.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ResponseKeyDrop.Name = "ResponseKeyDrop";
            this.ResponseKeyDrop.Size = new System.Drawing.Size(204, 24);
            this.ResponseKeyDrop.TabIndex = 9;
            this.ResponseKeyDrop.SelectedIndexChanged += new System.EventHandler(this.ResponseKeyDrop_SelectedIndexChanged);
            // 
            // ManageKeys
            // 
            this.ManageKeys.Location = new System.Drawing.Point(1081, 32);
            this.ManageKeys.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.ManageKeys.Name = "ManageKeys";
            this.ManageKeys.Size = new System.Drawing.Size(183, 28);
            this.ManageKeys.TabIndex = 10;
            this.ManageKeys.Text = "Create / Manage Keys";
            this.ManageKeys.UseVisualStyleBackColor = true;
            this.ManageKeys.Click += new System.EventHandler(this.ManageKeys_Click);
            // 
            // DynamicallyKeyedCheck
            // 
            this.DynamicallyKeyedCheck.AutoSize = true;
            this.DynamicallyKeyedCheck.Location = new System.Drawing.Point(868, 65);
            this.DynamicallyKeyedCheck.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.DynamicallyKeyedCheck.Name = "DynamicallyKeyedCheck";
            this.DynamicallyKeyedCheck.Size = new System.Drawing.Size(372, 21);
            this.DynamicallyKeyedCheck.TabIndex = 11;
            this.DynamicallyKeyedCheck.Text = "Dynamically key IAT block based on survey responses";
            this.DynamicallyKeyedCheck.UseVisualStyleBackColor = true;
            this.DynamicallyKeyedCheck.CheckedChanged += new System.EventHandler(this.DynamicallyKeyedCheck_CheckedChanged);
            // 
            // IATBlockPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.Controls.Add(this.DynamicallyKeyedCheck);
            this.Controls.Add(this.ManageKeys);
            this.Controls.Add(this.ResponseKeyDrop);
            this.Controls.Add(this.ResponseKeyLabel);
            this.Controls.Add(this.Done);
            this.Controls.Add(this.DeleteStimulus);
            this.Controls.Add(this.InsertStimulus);
            this.Controls.Add(this.AddStimulus);
            this.Controls.Add(this.InstructionsLabel);
            this.Margin = new System.Windows.Forms.Padding(4, 4, 4, 4);
            this.MinimumSize = new System.Drawing.Size(1347, 794);
            this.Name = "IATBlockPanel";
            this.Size = new System.Drawing.Size(1347, 794);
//            this.ParentChanged += new System.EventHandler(this.IATBlockPanel_ParentChanged);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label InstructionsLabel;
        private System.Windows.Forms.Button AddStimulus;
        private System.Windows.Forms.Button InsertStimulus;
        private System.Windows.Forms.Button DeleteStimulus;
        private System.Windows.Forms.Button Done;
        private System.Windows.Forms.Label ResponseKeyLabel;
        private System.Windows.Forms.ComboBox ResponseKeyDrop;
        private System.Windows.Forms.Button ManageKeys;
        private System.Windows.Forms.CheckBox DynamicallyKeyedCheck;
    }
}
