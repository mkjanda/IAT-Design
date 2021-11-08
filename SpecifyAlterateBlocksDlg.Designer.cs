namespace IATClient
{
    partial class SpecifyAlterateBlocksDlg
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.BlockTypeTab = new System.Windows.Forms.TabControl();
            this.InstructionsTab = new System.Windows.Forms.TabPage();
            this.InstructionsList = new System.Windows.Forms.ListView();
            this.Instructions = new System.Windows.Forms.ColumnHeader();
            this.IATBlockTab = new System.Windows.Forms.TabPage();
            this.BlockList = new System.Windows.Forms.ListView();
            this.IATBlocks = new System.Windows.Forms.ColumnHeader();
            this.PracticeBlocksTab = new System.Windows.Forms.TabPage();
            this.PracticeBlocksList = new System.Windows.Forms.ListView();
            this.PracticeBlocks = new System.Windows.Forms.ColumnHeader();
            this.SurveyTab = new System.Windows.Forms.TabPage();
            this.PostpendedRadio = new System.Windows.Forms.RadioButton();
            this.PrependedRadio = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SurveyList = new System.Windows.Forms.ListView();
            this.AlternateButton = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.UndoAlternationButton = new System.Windows.Forms.Button();
            this.BlockTypeTab.SuspendLayout();
            this.InstructionsTab.SuspendLayout();
            this.IATBlockTab.SuspendLayout();
            this.PracticeBlocksTab.SuspendLayout();
            this.SurveyTab.SuspendLayout();
            this.SuspendLayout();
            // 
            // BlockTypeTab
            // 
            this.BlockTypeTab.Controls.Add(this.InstructionsTab);
            this.BlockTypeTab.Controls.Add(this.IATBlockTab);
            this.BlockTypeTab.Controls.Add(this.PracticeBlocksTab);
            this.BlockTypeTab.Controls.Add(this.SurveyTab);
            this.BlockTypeTab.Location = new System.Drawing.Point(12, 12);
            this.BlockTypeTab.Name = "BlockTypeTab";
            this.BlockTypeTab.SelectedIndex = 0;
            this.BlockTypeTab.Size = new System.Drawing.Size(298, 240);
            this.BlockTypeTab.TabIndex = 0;
            this.BlockTypeTab.Selected += new System.Windows.Forms.TabControlEventHandler(this.BlockTypeTab_Selected);
            // 
            // InstructionsTab
            // 
            this.InstructionsTab.Controls.Add(this.InstructionsList);
            this.InstructionsTab.Location = new System.Drawing.Point(4, 22);
            this.InstructionsTab.Name = "InstructionsTab";
            this.InstructionsTab.Padding = new System.Windows.Forms.Padding(3);
            this.InstructionsTab.Size = new System.Drawing.Size(290, 214);
            this.InstructionsTab.TabIndex = 0;
            this.InstructionsTab.Text = "Instructions";
            this.InstructionsTab.UseVisualStyleBackColor = true;
            // 
            // InstructionsList
            // 
            this.InstructionsList.CheckBoxes = true;
            this.InstructionsList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Instructions});
            this.InstructionsList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InstructionsList.Location = new System.Drawing.Point(3, 3);
            this.InstructionsList.Name = "InstructionsList";
            this.InstructionsList.Size = new System.Drawing.Size(284, 208);
            this.InstructionsList.TabIndex = 0;
            this.InstructionsList.UseCompatibleStateImageBehavior = false;
            this.InstructionsList.View = System.Windows.Forms.View.List;
            this.InstructionsList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.InstructionsList_ItemChecked);
            this.InstructionsList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.InstructionsList_ItemCheck);
            // 
            // Instructions
            // 
            this.Instructions.Text = "Instructions";
            this.Instructions.Width = 245;
            // 
            // IATBlockTab
            // 
            this.IATBlockTab.Controls.Add(this.BlockList);
            this.IATBlockTab.Location = new System.Drawing.Point(4, 22);
            this.IATBlockTab.Name = "IATBlockTab";
            this.IATBlockTab.Padding = new System.Windows.Forms.Padding(3);
            this.IATBlockTab.Size = new System.Drawing.Size(290, 214);
            this.IATBlockTab.TabIndex = 1;
            this.IATBlockTab.Text = "IAT Blocks";
            this.IATBlockTab.UseVisualStyleBackColor = true;
            // 
            // BlockList
            // 
            this.BlockList.CheckBoxes = true;
            this.BlockList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.IATBlocks});
            this.BlockList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BlockList.Location = new System.Drawing.Point(3, 3);
            this.BlockList.Name = "BlockList";
            this.BlockList.Size = new System.Drawing.Size(284, 208);
            this.BlockList.TabIndex = 0;
            this.BlockList.UseCompatibleStateImageBehavior = false;
            this.BlockList.View = System.Windows.Forms.View.List;
            this.BlockList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.BlockList_ItemChecked);
            this.BlockList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.BlockList_ItemCheck);
            // 
            // IATBlocks
            // 
            this.IATBlocks.Text = "IAT Blocks";
            this.IATBlocks.Width = 242;
            // 
            // PracticeBlocksTab
            // 
            this.PracticeBlocksTab.Controls.Add(this.PracticeBlocksList);
            this.PracticeBlocksTab.Location = new System.Drawing.Point(4, 22);
            this.PracticeBlocksTab.Name = "PracticeBlocksTab";
            this.PracticeBlocksTab.Padding = new System.Windows.Forms.Padding(3);
            this.PracticeBlocksTab.Size = new System.Drawing.Size(290, 214);
            this.PracticeBlocksTab.TabIndex = 2;
            this.PracticeBlocksTab.Text = "Practice Blocks";
            this.PracticeBlocksTab.UseVisualStyleBackColor = true;
            // 
            // PracticeBlocksList
            // 
            this.PracticeBlocksList.CheckBoxes = true;
            this.PracticeBlocksList.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.PracticeBlocks});
            this.PracticeBlocksList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PracticeBlocksList.Location = new System.Drawing.Point(3, 3);
            this.PracticeBlocksList.Name = "PracticeBlocksList";
            this.PracticeBlocksList.Size = new System.Drawing.Size(284, 208);
            this.PracticeBlocksList.TabIndex = 0;
            this.PracticeBlocksList.UseCompatibleStateImageBehavior = false;
            this.PracticeBlocksList.View = System.Windows.Forms.View.List;
            this.PracticeBlocksList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.PracticeBlocksList_ItemChecked);
            this.PracticeBlocksList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.PracticeBlocksList_ItemCheck);
            // 
            // PracticeBlocks
            // 
            this.PracticeBlocks.Text = "Practice Blocks";
            this.PracticeBlocks.Width = 242;
            // 
            // SurveyTab
            // 
            this.SurveyTab.Controls.Add(this.PostpendedRadio);
            this.SurveyTab.Controls.Add(this.PrependedRadio);
            this.SurveyTab.Controls.Add(this.label1);
            this.SurveyTab.Controls.Add(this.SurveyList);
            this.SurveyTab.Location = new System.Drawing.Point(4, 22);
            this.SurveyTab.Name = "SurveyTab";
            this.SurveyTab.Padding = new System.Windows.Forms.Padding(3);
            this.SurveyTab.Size = new System.Drawing.Size(290, 214);
            this.SurveyTab.TabIndex = 3;
            this.SurveyTab.Text = "Surveys";
            this.SurveyTab.UseVisualStyleBackColor = true;
            // 
            // PostpendedRadio
            // 
            this.PostpendedRadio.AutoSize = true;
            this.PostpendedRadio.Location = new System.Drawing.Point(162, 191);
            this.PostpendedRadio.Name = "PostpendedRadio";
            this.PostpendedRadio.Size = new System.Drawing.Size(82, 17);
            this.PostpendedRadio.TabIndex = 3;
            this.PostpendedRadio.TabStop = true;
            this.PostpendedRadio.Text = "Postpended";
            this.PostpendedRadio.UseVisualStyleBackColor = true;
            // 
            // PrependedRadio
            // 
            this.PrependedRadio.AutoSize = true;
            this.PrependedRadio.Location = new System.Drawing.Point(46, 191);
            this.PrependedRadio.Name = "PrependedRadio";
            this.PrependedRadio.Size = new System.Drawing.Size(77, 17);
            this.PrependedRadio.TabIndex = 2;
            this.PrependedRadio.TabStop = true;
            this.PrependedRadio.Text = "Prepended";
            this.PrependedRadio.UseVisualStyleBackColor = true;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 175);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(242, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Destination position of singular alternated surveys:";
            // 
            // SurveyList
            // 
            this.SurveyList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.SurveyList.CheckBoxes = true;
            this.SurveyList.Location = new System.Drawing.Point(0, 0);
            this.SurveyList.MultiSelect = false;
            this.SurveyList.Name = "SurveyList";
            this.SurveyList.Size = new System.Drawing.Size(287, 172);
            this.SurveyList.TabIndex = 0;
            this.SurveyList.UseCompatibleStateImageBehavior = false;
            this.SurveyList.View = System.Windows.Forms.View.List;
            this.SurveyList.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.SurveyList_ItemChecked);
            this.SurveyList.ItemCheck += new System.Windows.Forms.ItemCheckEventHandler(this.SurveyList_ItemCheck);
            // 
            // AlternateButton
            // 
            this.AlternateButton.Location = new System.Drawing.Point(3, 263);
            this.AlternateButton.Name = "AlternateButton";
            this.AlternateButton.Size = new System.Drawing.Size(95, 23);
            this.AlternateButton.TabIndex = 1;
            this.AlternateButton.Text = "Alternate Items";
            this.AlternateButton.UseVisualStyleBackColor = true;
            this.AlternateButton.Click += new System.EventHandler(this.AlternateButton_Click);
            // 
            // OK
            // 
            this.OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.OK.Location = new System.Drawing.Point(215, 263);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(95, 23);
            this.OK.TabIndex = 2;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // UndoAlternationButton
            // 
            this.UndoAlternationButton.Location = new System.Drawing.Point(109, 263);
            this.UndoAlternationButton.Name = "UndoAlternationButton";
            this.UndoAlternationButton.Size = new System.Drawing.Size(95, 23);
            this.UndoAlternationButton.TabIndex = 3;
            this.UndoAlternationButton.Text = "Undo Alternation";
            this.UndoAlternationButton.UseVisualStyleBackColor = true;
            this.UndoAlternationButton.Click += new System.EventHandler(this.UndoAlternationButton_Click);
            // 
            // SpecifyAlterateBlocksDlg
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(322, 298);
            this.Controls.Add(this.UndoAlternationButton);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.AlternateButton);
            this.Controls.Add(this.BlockTypeTab);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.Name = "SpecifyAlterateBlocksDlg";
            this.ShowInTaskbar = false;
            this.Text = "Specify Alternating Blocks";
            this.Load += new System.EventHandler(this.SpecifyAlterateBlocksDlg_Load);
            this.BlockTypeTab.ResumeLayout(false);
            this.InstructionsTab.ResumeLayout(false);
            this.IATBlockTab.ResumeLayout(false);
            this.PracticeBlocksTab.ResumeLayout(false);
            this.SurveyTab.ResumeLayout(false);
            this.SurveyTab.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TabControl BlockTypeTab;
        private System.Windows.Forms.TabPage InstructionsTab;
        private System.Windows.Forms.ListView InstructionsList;
        private System.Windows.Forms.ColumnHeader Instructions;
        private System.Windows.Forms.TabPage IATBlockTab;
        private System.Windows.Forms.ListView BlockList;
        private System.Windows.Forms.ColumnHeader IATBlocks;
        private System.Windows.Forms.TabPage PracticeBlocksTab;
        private System.Windows.Forms.ListView PracticeBlocksList;
        private System.Windows.Forms.ColumnHeader PracticeBlocks;
        private System.Windows.Forms.Button AlternateButton;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Button UndoAlternationButton;
        private System.Windows.Forms.TabPage SurveyTab;
        private System.Windows.Forms.ListView SurveyList;
        private System.Windows.Forms.RadioButton PostpendedRadio;
        private System.Windows.Forms.RadioButton PrependedRadio;
        private System.Windows.Forms.Label label1;
    }
}