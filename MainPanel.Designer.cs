namespace IATClient
{
    partial class MainPanel
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
            this.AddIATBlockButton = new System.Windows.Forms.Button();
            this.AddInstructionBlockButton = new System.Windows.Forms.Button();
            this.AddPreSurveyButton = new System.Windows.Forms.Button();
            this.AddPostSurveyButton = new System.Windows.Forms.Button();
            this.Generate7BlockButton = new System.Windows.Forms.Button();
            this.SpecifyAlternatingItemsButton = new System.Windows.Forms.Button();
            this.PurchaseAdministrationsButton = new System.Windows.Forms.Button();
            this.ServerInterfaceButton = new System.Windows.Forms.Button();
            this.KeyDynamicButton = new System.Windows.Forms.Button();
            this.PreviewGroup = new System.Windows.Forms.GroupBox();
            this.MessageBoxGroup = new System.Windows.Forms.GroupBox();
            this.TestContentsGroup = new System.Windows.Forms.GroupBox();
            this.SuspendLayout();
            // 
            // AddIATBlockButton
            // 
            this.AddIATBlockButton.Location = new System.Drawing.Point(3, 3);
            this.AddIATBlockButton.Name = "AddIATBlockButton";
            this.AddIATBlockButton.Size = new System.Drawing.Size(65, 65);
            this.AddIATBlockButton.TabIndex = 2;
            this.AddIATBlockButton.Text = "Add IAT Block";
            this.AddIATBlockButton.UseVisualStyleBackColor = true;
            this.AddIATBlockButton.Click += new System.EventHandler(this.AddIATBlockButton_Click);
            // 
            // AddInstructionBlockButton
            // 
            this.AddInstructionBlockButton.Location = new System.Drawing.Point(3, 74);
            this.AddInstructionBlockButton.Name = "AddInstructionBlockButton";
            this.AddInstructionBlockButton.Size = new System.Drawing.Size(65, 65);
            this.AddInstructionBlockButton.TabIndex = 3;
            this.AddInstructionBlockButton.Text = "Add Instruction Block";
            this.AddInstructionBlockButton.UseVisualStyleBackColor = true;
            this.AddInstructionBlockButton.Click += new System.EventHandler(this.AddInstructionBlockButton_Click);
            // 
            // AddPreSurveyButton
            // 
            this.AddPreSurveyButton.Location = new System.Drawing.Point(3, 145);
            this.AddPreSurveyButton.Name = "AddPreSurveyButton";
            this.AddPreSurveyButton.Size = new System.Drawing.Size(65, 65);
            this.AddPreSurveyButton.TabIndex = 4;
            this.AddPreSurveyButton.Text = "Add Pre-IAT Survey";
            this.AddPreSurveyButton.UseVisualStyleBackColor = true;
            this.AddPreSurveyButton.Click += new System.EventHandler(this.AddPreSurveyButton_Click);
            // 
            // AddPostSurveyButton
            // 
            this.AddPostSurveyButton.Location = new System.Drawing.Point(3, 216);
            this.AddPostSurveyButton.Name = "AddPostSurveyButton";
            this.AddPostSurveyButton.Size = new System.Drawing.Size(65, 65);
            this.AddPostSurveyButton.TabIndex = 5;
            this.AddPostSurveyButton.Text = "Add Post-IAT Survey";
            this.AddPostSurveyButton.UseVisualStyleBackColor = true;
            this.AddPostSurveyButton.Click += new System.EventHandler(this.AddPostSurveyButton_Click);
            // 
            // Generate7BlockButton
            // 
            this.Generate7BlockButton.Location = new System.Drawing.Point(3, 287);
            this.Generate7BlockButton.Name = "Generate7BlockButton";
            this.Generate7BlockButton.Size = new System.Drawing.Size(65, 65);
            this.Generate7BlockButton.TabIndex = 6;
            this.Generate7BlockButton.Text = "Generate 7-Block IAT";
            this.Generate7BlockButton.UseVisualStyleBackColor = true;
            this.Generate7BlockButton.Click += new System.EventHandler(this.Generate7BlockButton_Click);
            // 
            // SpecifyAlternatingItemsButton
            // 
            this.SpecifyAlternatingItemsButton.Location = new System.Drawing.Point(3, 358);
            this.SpecifyAlternatingItemsButton.Name = "SpecifyAlternatingItemsButton";
            this.SpecifyAlternatingItemsButton.Size = new System.Drawing.Size(65, 65);
            this.SpecifyAlternatingItemsButton.TabIndex = 7;
            this.SpecifyAlternatingItemsButton.Text = "Specify Alternating Items";
            this.SpecifyAlternatingItemsButton.UseVisualStyleBackColor = true;
            this.SpecifyAlternatingItemsButton.Click += new System.EventHandler(this.SpecifyAlternatingItemsButton_Click);
            // 
            // PurchaseAdministrationsButton
            // 
            this.PurchaseAdministrationsButton.Location = new System.Drawing.Point(3, 429);
            this.PurchaseAdministrationsButton.Name = "PurchaseAdministrationsButton";
            this.PurchaseAdministrationsButton.Size = new System.Drawing.Size(65, 65);
            this.PurchaseAdministrationsButton.TabIndex = 8;
            this.PurchaseAdministrationsButton.Text = "Purchase";
            this.PurchaseAdministrationsButton.UseVisualStyleBackColor = true;
            this.PurchaseAdministrationsButton.Click += new System.EventHandler(this.PackageIATButton_Click);
            this.PurchaseAdministrationsButton.MouseClick += new System.Windows.Forms.MouseEventHandler(this.PurchaseAdministrationsButton_MouseClick);
            // 
            // ServerInterfaceButton
            // 
            this.ServerInterfaceButton.Location = new System.Drawing.Point(3, 500);
            this.ServerInterfaceButton.Name = "ServerInterfaceButton";
            this.ServerInterfaceButton.Size = new System.Drawing.Size(65, 65);
            this.ServerInterfaceButton.TabIndex = 9;
            this.ServerInterfaceButton.Text = "Server Interface";
            this.ServerInterfaceButton.UseVisualStyleBackColor = true;
            this.ServerInterfaceButton.Click += new System.EventHandler(this.ServerInterfaceButton_Click);
            // 
            // KeyDynamicButton
            // 
            this.KeyDynamicButton.Enabled = true;
            this.KeyDynamicButton.Location = new System.Drawing.Point(3, 571);
            this.KeyDynamicButton.Name = "KeyDynamicButton";
            this.KeyDynamicButton.Size = new System.Drawing.Size(65, 65);
            this.KeyDynamicButton.TabIndex = 10;
            this.KeyDynamicButton.Text = "Key Dynamic IAT Block";
            this.KeyDynamicButton.UseVisualStyleBackColor = true;
            this.KeyDynamicButton.Click += new System.EventHandler(this.KeyDynamicButton_Click);
            // 
            // PreviewGroup
            // 
            this.PreviewGroup.Location = new System.Drawing.Point(501, 3);
            this.PreviewGroup.Name = "PreviewGroup";
            this.PreviewGroup.Size = new System.Drawing.Size(506, 518);
            this.PreviewGroup.TabIndex = 11;
            this.PreviewGroup.TabStop = false;
            this.PreviewGroup.Text = "Preview";
            // 
            // MessageBoxGroup
            // 
            this.MessageBoxGroup.Location = new System.Drawing.Point(74, 527);
            this.MessageBoxGroup.Name = "MessageBoxGroup";
            this.MessageBoxGroup.Size = new System.Drawing.Size(933, 109);
            this.MessageBoxGroup.TabIndex = 12;
            this.MessageBoxGroup.TabStop = false;
            this.MessageBoxGroup.Text = "Messages";
            // 
            // TestContentsGroup
            // 
            this.TestContentsGroup.Location = new System.Drawing.Point(74, 3);
            this.TestContentsGroup.Name = "TestContentsGroup";
            this.TestContentsGroup.Size = new System.Drawing.Size(421, 518);
            this.TestContentsGroup.TabIndex = 13;
            this.TestContentsGroup.TabStop = false;
            this.TestContentsGroup.Text = "Test Contents";
            // 
            // MainPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.TestContentsGroup);
            this.Controls.Add(this.MessageBoxGroup);
            this.Controls.Add(this.PreviewGroup);
            this.Controls.Add(this.ServerInterfaceButton);
            this.Controls.Add(this.PurchaseAdministrationsButton);
            this.Controls.Add(this.SpecifyAlternatingItemsButton);
            this.Controls.Add(this.Generate7BlockButton);
            this.Controls.Add(this.AddInstructionBlockButton);
            this.Controls.Add(this.AddIATBlockButton);
            this.Controls.Add(this.AddPostSurveyButton);
            this.Controls.Add(this.AddPreSurveyButton);
            this.Controls.Add(this.KeyDynamicButton);
            this.Name = "MainPanel";
            this.Size = new System.Drawing.Size(1010, 645);
            this.ParentChanged += new System.EventHandler(this.MainPanel_ParentChanged);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button AddIATBlockButton;
        private System.Windows.Forms.Button AddInstructionBlockButton;
        private System.Windows.Forms.Button AddPreSurveyButton;
        private System.Windows.Forms.Button AddPostSurveyButton;
        private System.Windows.Forms.Button Generate7BlockButton;
        private System.Windows.Forms.Button SpecifyAlternatingItemsButton;
        private System.Windows.Forms.Button PurchaseAdministrationsButton;
        private System.Windows.Forms.Button ServerInterfaceButton;
        private System.Windows.Forms.Button KeyDynamicButton;
        private System.Windows.Forms.GroupBox PreviewGroup;
        private System.Windows.Forms.GroupBox MessageBoxGroup;
        private System.Windows.Forms.GroupBox TestContentsGroup;


    }
}
