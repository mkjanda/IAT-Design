namespace IATClient
{
    partial class IATBlockItemsPanel
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
            this.KeyGroup = new System.Windows.Forms.GroupBox();
            this.MangeKeysButton = new System.Windows.Forms.Button();
            this.ResponseKeyDrop = new System.Windows.Forms.ComboBox();
            this.KeyLabel = new System.Windows.Forms.Label();
            this.IATBlockItemsGroup = new System.Windows.Forms.GroupBox();
            this.AddStimulusButton = new System.Windows.Forms.Button();
            this.DeleteStimulusButton = new System.Windows.Forms.Button();
            this.ReorderButton = new System.Windows.Forms.Button();
            this.RandomizeButton = new System.Windows.Forms.Button();
            this.KeyGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // KeyGroup
            // 
            this.KeyGroup.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.KeyGroup.Controls.Add(this.MangeKeysButton);
            this.KeyGroup.Controls.Add(this.ResponseKeyDrop);
            this.KeyGroup.Controls.Add(this.KeyLabel);
            this.KeyGroup.Location = new System.Drawing.Point(3, 3);
            this.KeyGroup.Name = "KeyGroup";
            this.KeyGroup.Size = new System.Drawing.Size(399, 46);
            this.KeyGroup.TabIndex = 0;
            this.KeyGroup.TabStop = false;
            this.KeyGroup.Text = "Response Key";
            // 
            // MangeKeysButton
            // 
            this.MangeKeysButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.MangeKeysButton.Location = new System.Drawing.Point(201, 17);
            this.MangeKeysButton.Name = "MangeKeysButton";
            this.MangeKeysButton.Size = new System.Drawing.Size(125, 23);
            this.MangeKeysButton.TabIndex = 2;
            this.MangeKeysButton.Text = "Create / Manage Keys";
            this.MangeKeysButton.UseVisualStyleBackColor = true;
            this.MangeKeysButton.Click += new System.EventHandler(this.MangeKeysButton_Click);
            // 
            // ResponseKeyDrop
            // 
            this.ResponseKeyDrop.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.ResponseKeyDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ResponseKeyDrop.FormattingEnabled = true;
            this.ResponseKeyDrop.Location = new System.Drawing.Point(76, 19);
            this.ResponseKeyDrop.Name = "ResponseKeyDrop";
            this.ResponseKeyDrop.Size = new System.Drawing.Size(121, 21);
            this.ResponseKeyDrop.TabIndex = 1;
            this.ResponseKeyDrop.SelectedIndexChanged += new System.EventHandler(this.ResponseKeyDrop_SelectedIndexChanged);
            // 
            // KeyLabel
            // 
            this.KeyLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)));
            this.KeyLabel.AutoSize = true;
            this.KeyLabel.Location = new System.Drawing.Point(51, 22);
            this.KeyLabel.Name = "KeyLabel";
            this.KeyLabel.Size = new System.Drawing.Size(28, 13);
            this.KeyLabel.TabIndex = 0;
            this.KeyLabel.Text = "Key:";
            // 
            // IATBlockItemsGroup
            // 
            this.IATBlockItemsGroup.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.IATBlockItemsGroup.Location = new System.Drawing.Point(3, 56);
            this.IATBlockItemsGroup.Name = "IATBlockItemsGroup";
            this.IATBlockItemsGroup.Size = new System.Drawing.Size(399, 264);
            this.IATBlockItemsGroup.TabIndex = 1;
            this.IATBlockItemsGroup.TabStop = false;
            this.IATBlockItemsGroup.Text = "Items in Block";
            // 
            // AddStimulusButton
            // 
            this.AddStimulusButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.AddStimulusButton.Location = new System.Drawing.Point(18, 326);
            this.AddStimulusButton.Name = "AddStimulusButton";
            this.AddStimulusButton.Size = new System.Drawing.Size(92, 23);
            this.AddStimulusButton.TabIndex = 0;
            this.AddStimulusButton.Text = "Add Stimulus";
            this.AddStimulusButton.UseVisualStyleBackColor = true;
            this.AddStimulusButton.Click += new System.EventHandler(this.AddStimulusButton_Click);
            // 
            // DeleteStimulusButton
            // 
            this.DeleteStimulusButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.DeleteStimulusButton.Location = new System.Drawing.Point(120, 326);
            this.DeleteStimulusButton.Name = "DeleteStimulusButton";
            this.DeleteStimulusButton.Size = new System.Drawing.Size(92, 23);
            this.DeleteStimulusButton.TabIndex = 1;
            this.DeleteStimulusButton.Text = "DeleteStimulus";
            this.DeleteStimulusButton.UseVisualStyleBackColor = true;
            this.DeleteStimulusButton.Click += new System.EventHandler(this.DeleteStimulusButton_Click);
            // 
            // ReorderButton
            // 
            this.ReorderButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.ReorderButton.Location = new System.Drawing.Point(222, 326);
            this.ReorderButton.Name = "ReorderButton";
            this.ReorderButton.Size = new System.Drawing.Size(75, 23);
            this.ReorderButton.TabIndex = 2;
            this.ReorderButton.Text = "Reorder";
            this.ReorderButton.UseVisualStyleBackColor = true;
//            this.ReorderButton.Click += new System.EventHandler(this.ReorderButton_Click);
            // 
            // RandomizeButton
            // 
            this.RandomizeButton.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.RandomizeButton.Location = new System.Drawing.Point(307, 326);
            this.RandomizeButton.Name = "RandomizeButton";
            this.RandomizeButton.Size = new System.Drawing.Size(80, 23);
            this.RandomizeButton.TabIndex = 3;
            this.RandomizeButton.Text = "Randomize";
            this.RandomizeButton.UseVisualStyleBackColor = true;
            this.RandomizeButton.Click += new System.EventHandler(this.RandomizeButton_Click);
            // 
            // IATBlockItemsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RandomizeButton);
            this.Controls.Add(this.IATBlockItemsGroup);
            this.Controls.Add(this.ReorderButton);
            this.Controls.Add(this.KeyGroup);
            this.Controls.Add(this.DeleteStimulusButton);
            this.Controls.Add(this.AddStimulusButton);
            this.Name = "IATBlockItemsPanel";
            this.Size = new System.Drawing.Size(405, 352);
            this.ParentChanged += new System.EventHandler(this.IATBlockItemsPanel_ParentChanged);
            this.KeyGroup.ResumeLayout(false);
            this.KeyGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox KeyGroup;
        private System.Windows.Forms.ComboBox ResponseKeyDrop;
        private System.Windows.Forms.Label KeyLabel;
        private System.Windows.Forms.Button MangeKeysButton;
        private System.Windows.Forms.GroupBox IATBlockItemsGroup;
        private System.Windows.Forms.Button DeleteStimulusButton;
        private System.Windows.Forms.Button AddStimulusButton;
        private System.Windows.Forms.Button ReorderButton;
        private System.Windows.Forms.Button RandomizeButton;
    }
}
