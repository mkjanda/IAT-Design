namespace IATClient
{
    partial class SurveyPanel
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
            this.SurveyDisplayPanel = new System.Windows.Forms.Panel();
            this.ItemsPanel = new System.Windows.Forms.Panel();
            this.AddInstructions = new System.Windows.Forms.Button();
            this.AddRegExItem = new System.Windows.Forms.Button();
            this.AddDateItem = new System.Windows.Forms.Button();
            this.AddFixedDigitItem = new System.Windows.Forms.Button();
            this.AddBoundedNumberItem = new System.Windows.Forms.Button();
            this.AddBoundedLengthItem = new System.Windows.Forms.Button();
            this.AddMutliSelectionItem = new System.Windows.Forms.Button();
            this.AddWeightedMultiChoiceItem = new System.Windows.Forms.Button();
            this.AddMultiChoiceItem = new System.Windows.Forms.Button();
            this.AddLikertItem = new System.Windows.Forms.Button();
            this.AddTrueFalseItem = new System.Windows.Forms.Button();
            this.InsertRadio = new System.Windows.Forms.RadioButton();
            this.AppendRadio = new System.Windows.Forms.RadioButton();
            this.AddInsertPanel = new System.Windows.Forms.Panel();
            this.EditPanel = new System.Windows.Forms.Panel();
            this.Delete = new System.Windows.Forms.Button();
            this.Paste = new System.Windows.Forms.Button();
            this.Copy = new System.Windows.Forms.Button();
            this.Cut = new System.Windows.Forms.Button();
            this.CaptionCheck = new System.Windows.Forms.CheckBox();
            this.ReturnButton = new System.Windows.Forms.Button();
            this.TimeoutLabel = new System.Windows.Forms.Label();
            this.TimeoutText = new System.Windows.Forms.TextBox();
            this.MinLabel = new System.Windows.Forms.Label();
            this.textBox1 = new System.Windows.Forms.TextBox();
            this.AddImage = new System.Windows.Forms.Button();
            this.ItemsPanel.SuspendLayout();
            this.AddInsertPanel.SuspendLayout();
            this.EditPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // SurveyDisplayPanel
            // 
            this.SurveyDisplayPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SurveyDisplayPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.SurveyDisplayPanel.Location = new System.Drawing.Point(0, 0);
            this.SurveyDisplayPanel.Name = "SurveyDisplayPanel";
            this.SurveyDisplayPanel.Size = new System.Drawing.Size(735, 583);
            this.SurveyDisplayPanel.TabIndex = 0;
            // 
            // ItemsPanel
            // 
            this.ItemsPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ItemsPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.ItemsPanel.Controls.Add(this.AddImage);
            this.ItemsPanel.Controls.Add(this.AddInstructions);
            this.ItemsPanel.Controls.Add(this.AddRegExItem);
            this.ItemsPanel.Controls.Add(this.AddDateItem);
            this.ItemsPanel.Controls.Add(this.AddFixedDigitItem);
            this.ItemsPanel.Controls.Add(this.AddBoundedNumberItem);
            this.ItemsPanel.Controls.Add(this.AddBoundedLengthItem);
            this.ItemsPanel.Controls.Add(this.AddMutliSelectionItem);
            this.ItemsPanel.Controls.Add(this.AddWeightedMultiChoiceItem);
            this.ItemsPanel.Controls.Add(this.AddMultiChoiceItem);
            this.ItemsPanel.Controls.Add(this.AddLikertItem);
            this.ItemsPanel.Controls.Add(this.AddTrueFalseItem);
            this.ItemsPanel.Location = new System.Drawing.Point(736, 36);
            this.ItemsPanel.Name = "ItemsPanel";
            this.ItemsPanel.Size = new System.Drawing.Size(128, 279);
            this.ItemsPanel.TabIndex = 1;
            // 
            // AddInstructions
            // 
            this.AddInstructions.Location = new System.Drawing.Point(0, 230);
            this.AddInstructions.Margin = new System.Windows.Forms.Padding(0);
            this.AddInstructions.Name = "AddInstructions";
            this.AddInstructions.Size = new System.Drawing.Size(125, 23);
            this.AddInstructions.TabIndex = 12;
            this.AddInstructions.Text = "Instructions";
            this.AddInstructions.UseVisualStyleBackColor = true;
            this.AddInstructions.Click += new System.EventHandler(this.AddInstructions_Click);
            // 
            // AddRegExItem
            // 
            this.AddRegExItem.Location = new System.Drawing.Point(0, 207);
            this.AddRegExItem.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.AddRegExItem.Name = "AddRegExItem";
            this.AddRegExItem.Size = new System.Drawing.Size(125, 23);
            this.AddRegExItem.TabIndex = 11;
            this.AddRegExItem.Text = "RegEx Item";
            this.AddRegExItem.UseVisualStyleBackColor = true;
            this.AddRegExItem.Click += new System.EventHandler(this.AddRegExItem_Click);
            // 
            // AddDateItem
            // 
            this.AddDateItem.Location = new System.Drawing.Point(-1, 184);
            this.AddDateItem.Margin = new System.Windows.Forms.Padding(0);
            this.AddDateItem.Name = "AddDateItem";
            this.AddDateItem.Size = new System.Drawing.Size(125, 23);
            this.AddDateItem.TabIndex = 10;
            this.AddDateItem.Text = "Date Item";
            this.AddDateItem.UseVisualStyleBackColor = true;
            this.AddDateItem.Click += new System.EventHandler(this.AddDateItem_Click);
            // 
            // AddFixedDigitItem
            // 
            this.AddFixedDigitItem.Location = new System.Drawing.Point(-1, 161);
            this.AddFixedDigitItem.Margin = new System.Windows.Forms.Padding(0);
            this.AddFixedDigitItem.Name = "AddFixedDigitItem";
            this.AddFixedDigitItem.Size = new System.Drawing.Size(125, 23);
            this.AddFixedDigitItem.TabIndex = 9;
            this.AddFixedDigitItem.Text = "Fixed # of Digits Item";
            this.AddFixedDigitItem.UseVisualStyleBackColor = true;
            this.AddFixedDigitItem.Click += new System.EventHandler(this.AddFixedDigitItem_Click);
            // 
            // AddBoundedNumberItem
            // 
            this.AddBoundedNumberItem.Location = new System.Drawing.Point(-1, 138);
            this.AddBoundedNumberItem.Margin = new System.Windows.Forms.Padding(0);
            this.AddBoundedNumberItem.Name = "AddBoundedNumberItem";
            this.AddBoundedNumberItem.Size = new System.Drawing.Size(125, 23);
            this.AddBoundedNumberItem.TabIndex = 8;
            this.AddBoundedNumberItem.Text = "Bounded Number Item";
            this.AddBoundedNumberItem.UseVisualStyleBackColor = true;
            this.AddBoundedNumberItem.Click += new System.EventHandler(this.AddBoundedNumberItem_Click);
            // 
            // AddBoundedLengthItem
            // 
            this.AddBoundedLengthItem.Location = new System.Drawing.Point(-1, 115);
            this.AddBoundedLengthItem.Margin = new System.Windows.Forms.Padding(0);
            this.AddBoundedLengthItem.Name = "AddBoundedLengthItem";
            this.AddBoundedLengthItem.Size = new System.Drawing.Size(125, 23);
            this.AddBoundedLengthItem.TabIndex = 7;
            this.AddBoundedLengthItem.Text = "Bounded Length Item";
            this.AddBoundedLengthItem.UseVisualStyleBackColor = true;
            this.AddBoundedLengthItem.Click += new System.EventHandler(this.AddBoundedLengthItem_Click);
            // 
            // AddMutliSelectionItem
            // 
            this.AddMutliSelectionItem.Location = new System.Drawing.Point(0, 92);
            this.AddMutliSelectionItem.Margin = new System.Windows.Forms.Padding(0);
            this.AddMutliSelectionItem.Name = "AddMutliSelectionItem";
            this.AddMutliSelectionItem.Size = new System.Drawing.Size(125, 23);
            this.AddMutliSelectionItem.TabIndex = 6;
            this.AddMutliSelectionItem.Text = "Multiple Selection Item";
            this.AddMutliSelectionItem.UseVisualStyleBackColor = true;
            this.AddMutliSelectionItem.Click += new System.EventHandler(this.AddMutliSelectionItem_Click);
            // 
            // AddWeightedMultiChoiceItem
            // 
            this.AddWeightedMultiChoiceItem.Location = new System.Drawing.Point(0, 69);
            this.AddWeightedMultiChoiceItem.Margin = new System.Windows.Forms.Padding(0);
            this.AddWeightedMultiChoiceItem.Name = "AddWeightedMultiChoiceItem";
            this.AddWeightedMultiChoiceItem.Size = new System.Drawing.Size(125, 23);
            this.AddWeightedMultiChoiceItem.TabIndex = 5;
            this.AddWeightedMultiChoiceItem.Text = "Weighted Multi-Choice";
            this.AddWeightedMultiChoiceItem.UseVisualStyleBackColor = true;
            this.AddWeightedMultiChoiceItem.Click += new System.EventHandler(this.AddWeightedMultiChoiceItem_Click);
            // 
            // AddMultiChoiceItem
            // 
            this.AddMultiChoiceItem.Location = new System.Drawing.Point(0, 46);
            this.AddMultiChoiceItem.Margin = new System.Windows.Forms.Padding(0);
            this.AddMultiChoiceItem.Name = "AddMultiChoiceItem";
            this.AddMultiChoiceItem.Size = new System.Drawing.Size(125, 23);
            this.AddMultiChoiceItem.TabIndex = 2;
            this.AddMultiChoiceItem.Text = "Multiple Choice Item";
            this.AddMultiChoiceItem.UseVisualStyleBackColor = true;
            this.AddMultiChoiceItem.Click += new System.EventHandler(this.AddMultiChoiceItem_Click);
            // 
            // AddLikertItem
            // 
            this.AddLikertItem.Location = new System.Drawing.Point(0, 23);
            this.AddLikertItem.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.AddLikertItem.Name = "AddLikertItem";
            this.AddLikertItem.Size = new System.Drawing.Size(125, 23);
            this.AddLikertItem.TabIndex = 1;
            this.AddLikertItem.Text = "Likert Item";
            this.AddLikertItem.UseVisualStyleBackColor = true;
            this.AddLikertItem.Click += new System.EventHandler(this.AddLikertItem_Click);
            // 
            // AddTrueFalseItem
            // 
            this.AddTrueFalseItem.Location = new System.Drawing.Point(0, 0);
            this.AddTrueFalseItem.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.AddTrueFalseItem.Name = "AddTrueFalseItem";
            this.AddTrueFalseItem.Size = new System.Drawing.Size(125, 23);
            this.AddTrueFalseItem.TabIndex = 0;
            this.AddTrueFalseItem.Text = "True/False Item";
            this.AddTrueFalseItem.UseVisualStyleBackColor = true;
            this.AddTrueFalseItem.Click += new System.EventHandler(this.AddTrueFalseItem_Click);
            // 
            // InsertRadio
            // 
            this.InsertRadio.Appearance = System.Windows.Forms.Appearance.Button;
            this.InsertRadio.Location = new System.Drawing.Point(60, 0);
            this.InsertRadio.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.InsertRadio.Name = "InsertRadio";
            this.InsertRadio.Size = new System.Drawing.Size(63, 23);
            this.InsertRadio.TabIndex = 4;
            this.InsertRadio.TabStop = true;
            this.InsertRadio.Text = "Insert";
            this.InsertRadio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.InsertRadio.UseVisualStyleBackColor = true;
            // 
            // AppendRadio
            // 
            this.AppendRadio.Appearance = System.Windows.Forms.Appearance.Button;
            this.AppendRadio.Location = new System.Drawing.Point(-2, 0);
            this.AppendRadio.Margin = new System.Windows.Forms.Padding(3, 0, 3, 0);
            this.AppendRadio.Name = "AppendRadio";
            this.AppendRadio.Size = new System.Drawing.Size(63, 23);
            this.AppendRadio.TabIndex = 3;
            this.AppendRadio.TabStop = true;
            this.AppendRadio.Text = "Append";
            this.AppendRadio.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.AppendRadio.UseVisualStyleBackColor = true;
            // 
            // AddInsertPanel
            // 
            this.AddInsertPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddInsertPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.AddInsertPanel.Controls.Add(this.AppendRadio);
            this.AddInsertPanel.Controls.Add(this.InsertRadio);
            this.AddInsertPanel.Location = new System.Drawing.Point(735, 3);
            this.AddInsertPanel.Name = "AddInsertPanel";
            this.AddInsertPanel.Size = new System.Drawing.Size(126, 27);
            this.AddInsertPanel.TabIndex = 2;
            // 
            // EditPanel
            // 
            this.EditPanel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.EditPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.EditPanel.Controls.Add(this.Delete);
            this.EditPanel.Controls.Add(this.Paste);
            this.EditPanel.Controls.Add(this.Copy);
            this.EditPanel.Controls.Add(this.Cut);
            this.EditPanel.Location = new System.Drawing.Point(734, 325);
            this.EditPanel.Name = "EditPanel";
            this.EditPanel.Size = new System.Drawing.Size(128, 130);
            this.EditPanel.TabIndex = 3;
            // 
            // Delete
            // 
            this.Delete.Location = new System.Drawing.Point(62, 63);
            this.Delete.Margin = new System.Windows.Forms.Padding(0);
            this.Delete.Name = "Delete";
            this.Delete.Size = new System.Drawing.Size(63, 63);
            this.Delete.TabIndex = 3;
            this.Delete.Text = "Delete";
            this.Delete.UseVisualStyleBackColor = true;
            this.Delete.Click += new System.EventHandler(this.Delete_Click);
            // 
            // Paste
            // 
            this.Paste.Location = new System.Drawing.Point(0, 63);
            this.Paste.Margin = new System.Windows.Forms.Padding(0);
            this.Paste.Name = "Paste";
            this.Paste.Size = new System.Drawing.Size(63, 63);
            this.Paste.TabIndex = 2;
            this.Paste.Text = "Paste";
            this.Paste.UseVisualStyleBackColor = true;
            this.Paste.Click += new System.EventHandler(this.Paste_Click);
            // 
            // Copy
            // 
            this.Copy.Location = new System.Drawing.Point(62, 0);
            this.Copy.Margin = new System.Windows.Forms.Padding(0);
            this.Copy.Name = "Copy";
            this.Copy.Size = new System.Drawing.Size(63, 63);
            this.Copy.TabIndex = 1;
            this.Copy.Text = "Copy";
            this.Copy.UseVisualStyleBackColor = true;
            this.Copy.Click += new System.EventHandler(this.Copy_Click);
            // 
            // Cut
            // 
            this.Cut.Location = new System.Drawing.Point(0, 0);
            this.Cut.Margin = new System.Windows.Forms.Padding(0);
            this.Cut.Name = "Cut";
            this.Cut.Size = new System.Drawing.Size(63, 63);
            this.Cut.TabIndex = 0;
            this.Cut.Text = "Cut";
            this.Cut.UseVisualStyleBackColor = true;
            this.Cut.Click += new System.EventHandler(this.Cut_Click);
            // 
            // CaptionCheck
            // 
            this.CaptionCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.CaptionCheck.AutoSize = true;
            this.CaptionCheck.Location = new System.Drawing.Point(734, 461);
            this.CaptionCheck.Name = "CaptionCheck";
            this.CaptionCheck.Size = new System.Drawing.Size(136, 17);
            this.CaptionCheck.TabIndex = 4;
            this.CaptionCheck.Text = "Include Survey Caption";
            this.CaptionCheck.UseVisualStyleBackColor = true;
            this.CaptionCheck.CheckedChanged += new System.EventHandler(this.CaptionCheck_CheckedChanged);
            // 
            // ReturnButton
            // 
            this.ReturnButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ReturnButton.Location = new System.Drawing.Point(736, 558);
            this.ReturnButton.Name = "ReturnButton";
            this.ReturnButton.Size = new System.Drawing.Size(126, 23);
            this.ReturnButton.TabIndex = 5;
            this.ReturnButton.Text = "Return to Contents";
            this.ReturnButton.UseVisualStyleBackColor = true;
            // 
            // TimeoutLabel
            // 
            this.TimeoutLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TimeoutLabel.AutoSize = true;
            this.TimeoutLabel.Location = new System.Drawing.Point(729, 491);
            this.TimeoutLabel.Name = "TimeoutLabel";
            this.TimeoutLabel.Size = new System.Drawing.Size(57, 13);
            this.TimeoutLabel.TabIndex = 6;
            this.TimeoutLabel.Text = "Time Limit:";
            // 
            // TimeoutText
            // 
            this.TimeoutText.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.TimeoutText.Location = new System.Drawing.Point(792, 488);
            this.TimeoutText.Name = "TimeoutText";
            this.TimeoutText.Size = new System.Drawing.Size(42, 20);
            this.TimeoutText.TabIndex = 7;
            // 
            // MinLabel
            // 
            this.MinLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MinLabel.AutoSize = true;
            this.MinLabel.Location = new System.Drawing.Point(837, 491);
            this.MinLabel.Name = "MinLabel";
            this.MinLabel.Size = new System.Drawing.Size(23, 13);
            this.MinLabel.TabIndex = 8;
            this.MinLabel.Text = "min";
            // 
            // textBox1
            // 
            this.textBox1.Location = new System.Drawing.Point(0, 0);
            this.textBox1.Name = "textBox1";
            this.textBox1.Size = new System.Drawing.Size(100, 20);
            this.textBox1.TabIndex = 0;
            // 
            // AddImage
            // 
            this.AddImage.Location = new System.Drawing.Point(0, 253);
            this.AddImage.Margin = new System.Windows.Forms.Padding(0);
            this.AddImage.Name = "AddImage";
            this.AddImage.Size = new System.Drawing.Size(125, 23);
            this.AddImage.TabIndex = 13;
            this.AddImage.Text = "Image";
            this.AddImage.UseVisualStyleBackColor = true;
            this.AddImage.Click += new System.EventHandler(this.AddImage_Click);
            // 
            // SurveyPanel
            // 
            this.AutoSize = true;
            this.Controls.Add(this.MinLabel);
            this.Controls.Add(this.TimeoutText);
            this.Controls.Add(this.TimeoutLabel);
            this.Controls.Add(this.ReturnButton);
            this.Controls.Add(this.CaptionCheck);
            this.Controls.Add(this.EditPanel);
            this.Controls.Add(this.AddInsertPanel);
            this.Controls.Add(this.ItemsPanel);
            this.Controls.Add(this.SurveyDisplayPanel);
            this.Name = "SurveyPanel";
            this.Size = new System.Drawing.Size(873, 586);
            this.ItemsPanel.ResumeLayout(false);
            this.AddInsertPanel.ResumeLayout(false);
            this.EditPanel.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel SurveyDisplayPanel;
        private IATClient.SurveyDisplay SurveyView;
        private System.Windows.Forms.Panel ItemsPanel;
        private System.Windows.Forms.Button AddMultiChoiceItem;
        private System.Windows.Forms.Button AddLikertItem;
        private System.Windows.Forms.Button AddTrueFalseItem;
        private System.Windows.Forms.RadioButton InsertRadio;
        private System.Windows.Forms.RadioButton AppendRadio;
        private System.Windows.Forms.Button AddWeightedMultiChoiceItem;
        private System.Windows.Forms.Button AddMutliSelectionItem;
        private System.Windows.Forms.Button AddBoundedLengthItem;
        private System.Windows.Forms.Button AddFixedDigitItem;
        private System.Windows.Forms.Button AddBoundedNumberItem;
        private System.Windows.Forms.Button AddDateItem;
        private System.Windows.Forms.Button AddRegExItem;
        private System.Windows.Forms.Panel AddInsertPanel;
        private System.Windows.Forms.Panel EditPanel;
        private System.Windows.Forms.Button Delete;
        private System.Windows.Forms.Button Paste;
        private System.Windows.Forms.Button Copy;
        private System.Windows.Forms.Button Cut;
        private System.Windows.Forms.Button AddInstructions;
        private System.Windows.Forms.CheckBox CaptionCheck;
        private System.Windows.Forms.Button ReturnButton;
        private System.Windows.Forms.Label TimeoutLabel;
        private System.Windows.Forms.TextBox TimeoutText;
        private System.Windows.Forms.Label MinLabel;
        private System.Windows.Forms.TextBox textBox1;
        private System.Windows.Forms.Button AddImage;
    }
}
