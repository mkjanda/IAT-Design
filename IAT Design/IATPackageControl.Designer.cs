namespace IATClient
{
    partial class IATPackageControl
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
            this.IATNameLabel = new System.Windows.Forms.Label();
            this.IATName = new System.Windows.Forms.TextBox();
            this.RedirectionURLLabel = new System.Windows.Forms.Label();
            this.RedirectionURL = new System.Windows.Forms.TextBox();
            this.FixedOrder = new System.Windows.Forms.RadioButton();
            this.RandomOrder = new System.Windows.Forms.RadioButton();
            this.SetNumPresentations = new System.Windows.Forms.RadioButton();
            this.PackFileNameLabel = new System.Windows.Forms.Label();
            this.PackageFileName = new System.Windows.Forms.TextBox();
            this.Browse = new System.Windows.Forms.Button();
            this.RandomizationBox = new System.Windows.Forms.GroupBox();
            this.Cancel = new System.Windows.Forms.Button();
            this.OK = new System.Windows.Forms.Button();
            this.LeftResponseKeyLabel = new System.Windows.Forms.Label();
            this.LeftResponseKeyDrop = new System.Windows.Forms.ComboBox();
            this.RightResponseKeyLabel = new System.Windows.Forms.Label();
            this.RightResponseKeyDrop = new System.Windows.Forms.ComboBox();
            this.RandomizationBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // IATNameLabel
            // 
            this.IATNameLabel.AutoSize = true;
            this.IATNameLabel.Location = new System.Drawing.Point(78, 32);
            this.IATNameLabel.Name = "IATNameLabel";
            this.IATNameLabel.Size = new System.Drawing.Size(58, 13);
            this.IATNameLabel.TabIndex = 0;
            this.IATNameLabel.Text = "IAT Name:";
            // 
            // IATName
            // 
            this.IATName.Location = new System.Drawing.Point(141, 29);
            this.IATName.Name = "IATName";
            this.IATName.Size = new System.Drawing.Size(156, 20);
            this.IATName.TabIndex = 1;
            this.IATName.TextChanged += new System.EventHandler(this.IATName_TextChanged);
            // 
            // RedirectionURLLabel
            // 
            this.RedirectionURLLabel.AutoSize = true;
            this.RedirectionURLLabel.Location = new System.Drawing.Point(3, 58);
            this.RedirectionURLLabel.Name = "RedirectionURLLabel";
            this.RedirectionURLLabel.Size = new System.Drawing.Size(133, 13);
            this.RedirectionURLLabel.TabIndex = 2;
            this.RedirectionURLLabel.Text = "URL upon test completion:";
            // 
            // RedirectionURL
            // 
            this.RedirectionURL.Location = new System.Drawing.Point(142, 55);
            this.RedirectionURL.Name = "RedirectionURL";
            this.RedirectionURL.Size = new System.Drawing.Size(155, 20);
            this.RedirectionURL.TabIndex = 3;
            this.RedirectionURL.TextChanged += new System.EventHandler(this.RedirectionURL_TextChanged);
            // 
            // FixedOrder
            // 
            this.FixedOrder.AutoSize = true;
            this.FixedOrder.Enabled = false;
            this.FixedOrder.Location = new System.Drawing.Point(48, 19);
            this.FixedOrder.Name = "FixedOrder";
            this.FixedOrder.Size = new System.Drawing.Size(79, 17);
            this.FixedOrder.TabIndex = 5;
            this.FixedOrder.TabStop = true;
            this.FixedOrder.Text = "Fixed Order";
            this.FixedOrder.UseVisualStyleBackColor = true;
            this.FixedOrder.CheckedChanged += new System.EventHandler(this.FixedOrder_CheckedChanged);
            // 
            // RandomOrder
            // 
            this.RandomOrder.AutoSize = true;
            this.RandomOrder.Enabled = false;
            this.RandomOrder.Location = new System.Drawing.Point(48, 42);
            this.RandomOrder.Name = "RandomOrder";
            this.RandomOrder.Size = new System.Drawing.Size(94, 17);
            this.RandomOrder.TabIndex = 6;
            this.RandomOrder.TabStop = true;
            this.RandomOrder.Text = "Random Order";
            this.RandomOrder.UseVisualStyleBackColor = true;
            this.RandomOrder.CheckedChanged += new System.EventHandler(this.RandomOrder_CheckedChanged);
            // 
            // SetNumPresentations
            // 
            this.SetNumPresentations.AutoSize = true;
            this.SetNumPresentations.Location = new System.Drawing.Point(48, 65);
            this.SetNumPresentations.Name = "SetNumPresentations";
            this.SetNumPresentations.Size = new System.Drawing.Size(130, 17);
            this.SetNumPresentations.TabIndex = 7;
            this.SetNumPresentations.TabStop = true;
            this.SetNumPresentations.Text = "Set # of Presentations";
            this.SetNumPresentations.UseVisualStyleBackColor = true;
            this.SetNumPresentations.CheckedChanged += new System.EventHandler(this.SetNumPresentations_CheckedChanged);
            // 
            // PackFileNameLabel
            // 
            this.PackFileNameLabel.AutoSize = true;
            this.PackFileNameLabel.Location = new System.Drawing.Point(33, 89);
            this.PackFileNameLabel.Name = "PackFileNameLabel";
            this.PackFileNameLabel.Size = new System.Drawing.Size(103, 13);
            this.PackFileNameLabel.TabIndex = 10;
            this.PackFileNameLabel.Text = "Package File Name:";
            // 
            // PackageFileName
            // 
            this.PackageFileName.Location = new System.Drawing.Point(142, 86);
            this.PackageFileName.Name = "PackageFileName";
            this.PackageFileName.ReadOnly = true;
            this.PackageFileName.Size = new System.Drawing.Size(155, 20);
            this.PackageFileName.TabIndex = 11;
            // 
            // Browse
            // 
            this.Browse.Location = new System.Drawing.Point(125, 112);
            this.Browse.Name = "Browse";
            this.Browse.Size = new System.Drawing.Size(75, 23);
            this.Browse.TabIndex = 12;
            this.Browse.Text = "Browse";
            this.Browse.UseVisualStyleBackColor = true;
            this.Browse.Click += new System.EventHandler(this.Browse_Click);
            // 
            // RandomizationBox
            // 
            this.RandomizationBox.Controls.Add(this.SetNumPresentations);
            this.RandomizationBox.Controls.Add(this.RandomOrder);
            this.RandomizationBox.Controls.Add(this.FixedOrder);
            this.RandomizationBox.Location = new System.Drawing.Point(49, 191);
            this.RandomizationBox.Name = "RandomizationBox";
            this.RandomizationBox.Size = new System.Drawing.Size(226, 95);
            this.RandomizationBox.TabIndex = 13;
            this.RandomizationBox.TabStop = false;
            this.RandomizationBox.Text = "Randomization Type";
            // 
            // Cancel
            // 
            this.Cancel.Location = new System.Drawing.Point(84, 292);
            this.Cancel.Name = "Cancel";
            this.Cancel.Size = new System.Drawing.Size(75, 23);
            this.Cancel.TabIndex = 14;
            this.Cancel.Text = "Cancel";
            this.Cancel.UseVisualStyleBackColor = true;
            this.Cancel.Click += new System.EventHandler(this.Cancel_Click);
            // 
            // OK
            // 
            this.OK.Location = new System.Drawing.Point(165, 292);
            this.OK.Name = "OK";
            this.OK.Size = new System.Drawing.Size(75, 23);
            this.OK.TabIndex = 15;
            this.OK.Text = "OK";
            this.OK.UseVisualStyleBackColor = true;
            this.OK.Click += new System.EventHandler(this.OK_Click);
            // 
            // LeftResponseKeyLabel
            // 
            this.LeftResponseKeyLabel.AutoSize = true;
            this.LeftResponseKeyLabel.Location = new System.Drawing.Point(9, 148);
            this.LeftResponseKeyLabel.Name = "LeftResponseKeyLabel";
            this.LeftResponseKeyLabel.Size = new System.Drawing.Size(100, 13);
            this.LeftResponseKeyLabel.TabIndex = 16;
            this.LeftResponseKeyLabel.Text = "Left Response Key:";
            // 
            // LeftResponseKeyDrop
            // 
            this.LeftResponseKeyDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.LeftResponseKeyDrop.FormattingEnabled = true;
            this.LeftResponseKeyDrop.Location = new System.Drawing.Point(115, 145);
            this.LeftResponseKeyDrop.Name = "LeftResponseKeyDrop";
            this.LeftResponseKeyDrop.Size = new System.Drawing.Size(39, 21);
            this.LeftResponseKeyDrop.TabIndex = 17;
            // 
            // RightResponseKeyLabel
            // 
            this.RightResponseKeyLabel.AutoSize = true;
            this.RightResponseKeyLabel.Location = new System.Drawing.Point(162, 148);
            this.RightResponseKeyLabel.Name = "RightResponseKeyLabel";
            this.RightResponseKeyLabel.Size = new System.Drawing.Size(107, 13);
            this.RightResponseKeyLabel.TabIndex = 18;
            this.RightResponseKeyLabel.Text = "Right Response Key:";
            // 
            // RightResponseKeyDrop
            // 
            this.RightResponseKeyDrop.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.RightResponseKeyDrop.FormattingEnabled = true;
            this.RightResponseKeyDrop.Location = new System.Drawing.Point(275, 145);
            this.RightResponseKeyDrop.Name = "RightResponseKeyDrop";
            this.RightResponseKeyDrop.Size = new System.Drawing.Size(39, 21);
            this.RightResponseKeyDrop.TabIndex = 19;
            // 
            // IATPackageControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.RightResponseKeyDrop);
            this.Controls.Add(this.RightResponseKeyLabel);
            this.Controls.Add(this.LeftResponseKeyDrop);
            this.Controls.Add(this.LeftResponseKeyLabel);
            this.Controls.Add(this.OK);
            this.Controls.Add(this.Cancel);
            this.Controls.Add(this.RandomizationBox);
            this.Controls.Add(this.Browse);
            this.Controls.Add(this.PackageFileName);
            this.Controls.Add(this.PackFileNameLabel);
            this.Controls.Add(this.RedirectionURL);
            this.Controls.Add(this.RedirectionURLLabel);
            this.Controls.Add(this.IATName);
            this.Controls.Add(this.IATNameLabel);
            this.Name = "IATPackageControl";
            this.Size = new System.Drawing.Size(325, 325);
            this.RandomizationBox.ResumeLayout(false);
            this.RandomizationBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label IATNameLabel;
        private System.Windows.Forms.TextBox IATName;
        private System.Windows.Forms.Label RedirectionURLLabel;
        private System.Windows.Forms.TextBox RedirectionURL;
        private System.Windows.Forms.RadioButton FixedOrder;
        private System.Windows.Forms.RadioButton RandomOrder;
        private System.Windows.Forms.RadioButton SetNumPresentations;
        private System.Windows.Forms.Label PackFileNameLabel;
        private System.Windows.Forms.TextBox PackageFileName;
        private System.Windows.Forms.Button Browse;
        private System.Windows.Forms.GroupBox RandomizationBox;
        private System.Windows.Forms.Button Cancel;
        private System.Windows.Forms.Button OK;
        private System.Windows.Forms.Label LeftResponseKeyLabel;
        private System.Windows.Forms.ComboBox LeftResponseKeyDrop;
        private System.Windows.Forms.Label RightResponseKeyLabel;
        private System.Windows.Forms.ComboBox RightResponseKeyDrop;
    }
}
