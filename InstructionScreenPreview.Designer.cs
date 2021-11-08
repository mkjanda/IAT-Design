namespace IATClient
{
    partial class InstructionScreenPreview
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
            this.InstructionPreviewGroup = new System.Windows.Forms.GroupBox();
            this.PreviewPane = new System.Windows.Forms.PictureBox();
            this.InstructionPreviewGroup.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPane)).BeginInit();
            this.SuspendLayout();
            // 
            // InstructionPreviewGroup
            // 
            this.InstructionPreviewGroup.Controls.Add(this.PreviewPane);
            this.InstructionPreviewGroup.Location = new System.Drawing.Point(3, 3);
            this.InstructionPreviewGroup.Name = "InstructionPreviewGroup";
            this.InstructionPreviewGroup.Size = new System.Drawing.Size(506, 519);
            this.InstructionPreviewGroup.TabIndex = 0;
            this.InstructionPreviewGroup.TabStop = false;
            this.InstructionPreviewGroup.Text = "Instruction Screen Preview ";
            // 
            // PreviewPane
            // 
            this.PreviewPane.Dock = System.Windows.Forms.DockStyle.Fill;
            this.PreviewPane.Location = new System.Drawing.Point(3, 16);
            this.PreviewPane.Name = "PreviewPane";
            this.PreviewPane.Size = new System.Drawing.Size(500, 500);
            this.PreviewPane.TabIndex = 0;
            this.PreviewPane.TabStop = false;
            // 
            // InstructionScreenPreview
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.InstructionPreviewGroup);
            this.Name = "InstructionScreenPreview";
            this.Size = new System.Drawing.Size(511, 525);
            this.InstructionPreviewGroup.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.PreviewPane)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox InstructionPreviewGroup;
        private System.Windows.Forms.PictureBox PreviewPane;
    }
}
