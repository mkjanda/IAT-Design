namespace IATClient
{
    partial class ContentsPanel
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
            this.components = new System.ComponentModel.Container();
            this.ContentsGroup = new System.Windows.Forms.GroupBox();
            this.AlternateBlockButton = new System.Windows.Forms.Button();
            this.ContentsView = new System.Windows.Forms.ListView();
            this.ContentsContextMenu = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.ContextRename = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextModify = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.ContextCut = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextPaste = new System.Windows.Forms.ToolStripMenuItem();
            this.ContextDelete = new System.Windows.Forms.ToolStripMenuItem();
            this.GenerateIAT = new System.Windows.Forms.Button();
            this.DeleteSelectedItem = new System.Windows.Forms.Button();
            this.AddInstructionScreen = new System.Windows.Forms.Button();
            this.AddAfterSurvey = new System.Windows.Forms.Button();
            this.AddBeforeSurvey = new System.Windows.Forms.Button();
            this.AddPracticeBlock = new System.Windows.Forms.Button();
            this.AddBlock = new System.Windows.Forms.Button();
            this.DynamicallyKeyButton = new System.Windows.Forms.Button();
            this.ContentsGroup.SuspendLayout();
            this.ContentsContextMenu.SuspendLayout();
            this.SuspendLayout();
            // 
            // ContentsGroup
            // 
            this.ContentsGroup.Controls.Add(this.DynamicallyKeyButton);
            this.ContentsGroup.Controls.Add(this.AlternateBlockButton);
            this.ContentsGroup.Controls.Add(this.ContentsView);
            this.ContentsGroup.Controls.Add(this.GenerateIAT);
            this.ContentsGroup.Controls.Add(this.DeleteSelectedItem);
            this.ContentsGroup.Controls.Add(this.AddInstructionScreen);
            this.ContentsGroup.Controls.Add(this.AddAfterSurvey);
            this.ContentsGroup.Controls.Add(this.AddBeforeSurvey);
            this.ContentsGroup.Controls.Add(this.AddPracticeBlock);
            this.ContentsGroup.Controls.Add(this.AddBlock);
            this.ContentsGroup.Location = new System.Drawing.Point(3, 3);
            this.ContentsGroup.Name = "ContentsGroup";
            this.ContentsGroup.Size = new System.Drawing.Size(274, 412);
            this.ContentsGroup.TabIndex = 1;
            this.ContentsGroup.TabStop = false;
            this.ContentsGroup.Text = "Test Contents";
            // 
            // AlternateBlockButton
            // 
            this.AlternateBlockButton.Location = new System.Drawing.Point(56, 351);
            this.AlternateBlockButton.Name = "AlternateBlockButton";
            this.AlternateBlockButton.Size = new System.Drawing.Size(163, 23);
            this.AlternateBlockButton.TabIndex = 9;
            this.AlternateBlockButton.Text = "Specify Alternating Blocks";
            this.AlternateBlockButton.UseVisualStyleBackColor = true;
            this.AlternateBlockButton.Click += new System.EventHandler(this.AlternateBlockButton_Click);
            // 
            // ContentsView
            // 
            this.ContentsView.AllowDrop = true;
            this.ContentsView.ContextMenuStrip = this.ContentsContextMenu;
            this.ContentsView.LabelEdit = true;
            this.ContentsView.Location = new System.Drawing.Point(6, 19);
            this.ContentsView.MultiSelect = false;
            this.ContentsView.Name = "ContentsView";
            this.ContentsView.Size = new System.Drawing.Size(262, 210);
            this.ContentsView.TabIndex = 8;
            this.ContentsView.UseCompatibleStateImageBehavior = false;
            this.ContentsView.View = System.Windows.Forms.View.List;
            this.ContentsView.ItemActivate += new System.EventHandler(this.ContentsView_ItemActivate);
            this.ContentsView.AfterLabelEdit += new System.Windows.Forms.LabelEditEventHandler(this.ContentsView_AfterLabelEdit);
            this.ContentsView.DragDrop += new System.Windows.Forms.DragEventHandler(this.ContentsView_DragDrop);
            this.ContentsView.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ContentsView_MouseDown);
            this.ContentsView.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.ContentsView_ItemSelectionChanged);
            this.ContentsView.DragEnter += new System.Windows.Forms.DragEventHandler(this.ContentsView_DragEnter);
            this.ContentsView.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ContentsView_KeyUp);
            this.ContentsView.DragLeave += new System.EventHandler(this.ContentsView_DragLeave);
            this.ContentsView.ItemDrag += new System.Windows.Forms.ItemDragEventHandler(this.ContentsView_ItemDrag);
            this.ContentsView.DragOver += new System.Windows.Forms.DragEventHandler(this.ContentsView_DragOver);
            // 
            // ContentsContextMenu
            // 
            this.ContentsContextMenu.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.ContextRename,
            this.ContextModify,
            this.toolStripSeparator1,
            this.ContextCut,
            this.ContextPaste,
            this.ContextDelete});
            this.ContentsContextMenu.Name = "ContextCut";
            this.ContentsContextMenu.Size = new System.Drawing.Size(118, 120);
            // 
            // ContextRename
            // 
            this.ContextRename.Name = "ContextRename";
            this.ContextRename.Size = new System.Drawing.Size(117, 22);
            this.ContextRename.Text = "Rename";
            this.ContextRename.Click += new System.EventHandler(this.ContextRename_Click);
            // 
            // ContextModify
            // 
            this.ContextModify.Name = "ContextModify";
            this.ContextModify.Size = new System.Drawing.Size(117, 22);
            this.ContextModify.Text = "Modify";
            this.ContextModify.Click += new System.EventHandler(this.ContextModify_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(114, 6);
            // 
            // ContextCut
            // 
            this.ContextCut.Name = "ContextCut";
            this.ContextCut.Size = new System.Drawing.Size(117, 22);
            this.ContextCut.Text = "Cut";
            this.ContextCut.Click += new System.EventHandler(this.ContextCut_Click);
            // 
            // ContextPaste
            // 
            this.ContextPaste.Name = "ContextPaste";
            this.ContextPaste.Size = new System.Drawing.Size(117, 22);
            this.ContextPaste.Text = "Paste";
            this.ContextPaste.Click += new System.EventHandler(this.ContextPaste_Click);
            // 
            // ContextDelete
            // 
            this.ContextDelete.Name = "ContextDelete";
            this.ContextDelete.Size = new System.Drawing.Size(117, 22);
            this.ContextDelete.Text = "Delete";
            this.ContextDelete.Click += new System.EventHandler(this.ContextDelete_Click);
            // 
            // GenerateIAT
            // 
            this.GenerateIAT.Location = new System.Drawing.Point(56, 322);
            this.GenerateIAT.Name = "GenerateIAT";
            this.GenerateIAT.Size = new System.Drawing.Size(163, 23);
            this.GenerateIAT.TabIndex = 7;
            this.GenerateIAT.Text = "Generate 7-Block IAT";
            this.GenerateIAT.UseVisualStyleBackColor = true;
            this.GenerateIAT.Click += new System.EventHandler(this.GenerateIAT_Click);
            // 
            // DeleteSelectedItem
            // 
            this.DeleteSelectedItem.AllowDrop = true;
            this.DeleteSelectedItem.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.DeleteSelectedItem.Location = new System.Drawing.Point(143, 264);
            this.DeleteSelectedItem.Name = "DeleteSelectedItem";
            this.DeleteSelectedItem.Size = new System.Drawing.Size(125, 23);
            this.DeleteSelectedItem.TabIndex = 6;
            this.DeleteSelectedItem.Text = "Delete Selected Item";
            this.DeleteSelectedItem.UseVisualStyleBackColor = true;
            this.DeleteSelectedItem.Click += new System.EventHandler(this.DeleteSelectedItem_Click);
            // 
            // AddInstructionScreen
            // 
            this.AddInstructionScreen.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddInstructionScreen.Location = new System.Drawing.Point(143, 235);
            this.AddInstructionScreen.Name = "AddInstructionScreen";
            this.AddInstructionScreen.Size = new System.Drawing.Size(125, 23);
            this.AddInstructionScreen.TabIndex = 5;
            this.AddInstructionScreen.Text = "Add Instruction Screen";
            this.AddInstructionScreen.UseVisualStyleBackColor = true;
            this.AddInstructionScreen.Click += new System.EventHandler(this.AddInstructionScreen_Click);
            // 
            // AddAfterSurvey
            // 
            this.AddAfterSurvey.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.AddAfterSurvey.Location = new System.Drawing.Point(143, 293);
            this.AddAfterSurvey.Name = "AddAfterSurvey";
            this.AddAfterSurvey.Size = new System.Drawing.Size(125, 23);
            this.AddAfterSurvey.TabIndex = 4;
            this.AddAfterSurvey.Text = "Add Post-IAT Survey";
            this.AddAfterSurvey.UseVisualStyleBackColor = true;
            this.AddAfterSurvey.Click += new System.EventHandler(this.AddAfterSurvey_Click);
            // 
            // AddBeforeSurvey
            // 
            this.AddBeforeSurvey.Location = new System.Drawing.Point(6, 293);
            this.AddBeforeSurvey.Name = "AddBeforeSurvey";
            this.AddBeforeSurvey.Size = new System.Drawing.Size(125, 23);
            this.AddBeforeSurvey.TabIndex = 3;
            this.AddBeforeSurvey.Text = "Add Pre-IAT Survey";
            this.AddBeforeSurvey.UseVisualStyleBackColor = true;
            this.AddBeforeSurvey.Click += new System.EventHandler(this.AddBeforeSurvey_Click);
            // 
            // AddPracticeBlock
            // 
            this.AddPracticeBlock.Enabled = false;
            this.AddPracticeBlock.Location = new System.Drawing.Point(6, 264);
            this.AddPracticeBlock.Name = "AddPracticeBlock";
            this.AddPracticeBlock.Size = new System.Drawing.Size(125, 23);
            this.AddPracticeBlock.TabIndex = 2;
            this.AddPracticeBlock.Text = "Add Practice Block";
            this.AddPracticeBlock.UseVisualStyleBackColor = true;
            this.AddPracticeBlock.Click += new System.EventHandler(this.AddPracticeBlock_Click);
            // 
            // AddBlock
            // 
            this.AddBlock.Location = new System.Drawing.Point(6, 235);
            this.AddBlock.Name = "AddBlock";
            this.AddBlock.Size = new System.Drawing.Size(125, 23);
            this.AddBlock.TabIndex = 1;
            this.AddBlock.Text = "Add IAT Block";
            this.AddBlock.UseVisualStyleBackColor = true;
            this.AddBlock.Click += new System.EventHandler(this.AddBlock_Click);
            // 
            // DynamicallyKeyButton
            // 
            this.DynamicallyKeyButton.Location = new System.Drawing.Point(56, 380);
            this.DynamicallyKeyButton.Name = "DynamicallyKeyButton";
            this.DynamicallyKeyButton.Size = new System.Drawing.Size(163, 23);
            this.DynamicallyKeyButton.TabIndex = 10;
            this.DynamicallyKeyButton.Text = "Dynamically Key IAT Block";
            this.DynamicallyKeyButton.UseVisualStyleBackColor = true;
            this.DynamicallyKeyButton.Click += new System.EventHandler(this.DynamiclyKeyButton_Click);
            // 
            // ContentsPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ContentsGroup);
            this.Name = "ContentsPanel";
            this.Size = new System.Drawing.Size(280, 419);
            this.ParentChanged += new System.EventHandler(this.ContentsPanel_ParentChanged);
            this.ContentsGroup.ResumeLayout(false);
            this.ContentsContextMenu.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox ContentsGroup;
        private System.Windows.Forms.Button AddInstructionScreen;
        private System.Windows.Forms.Button AddAfterSurvey;
        private System.Windows.Forms.Button AddBeforeSurvey;
        private System.Windows.Forms.Button AddPracticeBlock;
        private System.Windows.Forms.Button AddBlock;
        private System.Windows.Forms.Button DeleteSelectedItem;
        private System.Windows.Forms.Button GenerateIAT;
        private System.Windows.Forms.ListView ContentsView;
        private System.Windows.Forms.ContextMenuStrip ContentsContextMenu;
        private System.Windows.Forms.ToolStripMenuItem ContextRename;
        private System.Windows.Forms.ToolStripMenuItem ContextModify;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem ContextCut;
        private System.Windows.Forms.ToolStripMenuItem ContextPaste;
        private System.Windows.Forms.ToolStripMenuItem ContextDelete;
        private System.Windows.Forms.Button AlternateBlockButton;
        private System.Windows.Forms.Button DynamicallyKeyButton;
    }
}
