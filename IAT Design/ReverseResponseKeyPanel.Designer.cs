namespace IATClient
{
    partial class ReverseResponseKeyPanel
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
            this.ReverseResponseGroup = new System.Windows.Forms.GroupBox();
            this.ReversibleKeyDropList = new System.Windows.Forms.ComboBox();
            this.ReverseResponseKeyLabel = new System.Windows.Forms.Label();
            this.ReverseResponseGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // ReverseResponseGroup
            // 
            this.ReverseResponseGroup.Controls.Add(this.ReversibleKeyDropList);
            this.ReverseResponseGroup.Controls.Add(this.ReverseResponseKeyLabel);
            this.ReverseResponseGroup.Location = new System.Drawing.Point(3, 3);
            this.ReverseResponseGroup.Name = "ReverseResponseGroup";
            this.ReverseResponseGroup.Size = new System.Drawing.Size(346, 51);
            this.ReverseResponseGroup.TabIndex = 0;
            this.ReverseResponseGroup.TabStop = false;
            this.ReverseResponseGroup.Text = "Reversed Response Key";
            // 
            // ReversibleKeyDropList
            // 
            this.ReversibleKeyDropList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ReversibleKeyDropList.FormattingEnabled = true;
            this.ReversibleKeyDropList.Location = new System.Drawing.Point(188, 19);
            this.ReversibleKeyDropList.Name = "ReversibleKeyDropList";
            this.ReversibleKeyDropList.Size = new System.Drawing.Size(151, 21);
            this.ReversibleKeyDropList.TabIndex = 1;
            // 
            // ReverseResponseKeyLabel
            // 
            this.ReverseResponseKeyLabel.AutoSize = true;
            this.ReverseResponseKeyLabel.Location = new System.Drawing.Point(6, 22);
            this.ReverseResponseKeyLabel.Name = "ReverseResponseKeyLabel";
            this.ReverseResponseKeyLabel.Size = new System.Drawing.Size(176, 13);
            this.ReverseResponseKeyLabel.TabIndex = 0;
            this.ReverseResponseKeyLabel.Text = "Select a Response Key to Reverse:";
            // 
            // ReverseResponseKeyPanel
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.ReverseResponseGroup);
            this.Name = "ReverseResponseKeyPanel";
            this.Size = new System.Drawing.Size(353, 61);
            this.ParentChanged += new System.EventHandler(this.ReverseResponseKeyPanel_ParentChanged);
            this.ReverseResponseGroup.ResumeLayout(false);
            this.ReverseResponseGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox ReverseResponseGroup;
        private System.Windows.Forms.Label ReverseResponseKeyLabel;
        private System.Windows.Forms.ComboBox ReversibleKeyDropList;
    }
}
