namespace IATClient
{
    partial class DateDetails
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
            this.DateGroup = new System.Windows.Forms.GroupBox();
            this.EnableStartDateCheck = new System.Windows.Forms.CheckBox();
            this.EnableEndDateCheck = new System.Windows.Forms.CheckBox();
            this.EndDate = new System.Windows.Forms.DateTimePicker();
            this.StartDate = new System.Windows.Forms.DateTimePicker();
            this.DateGroup.SuspendLayout();
            this.SuspendLayout();
            // 
            // DateGroup
            // 
            this.DateGroup.Controls.Add(this.EnableStartDateCheck);
            this.DateGroup.Controls.Add(this.EnableEndDateCheck);
            this.DateGroup.Controls.Add(this.EndDate);
            this.DateGroup.Controls.Add(this.StartDate);
            this.DateGroup.Location = new System.Drawing.Point(3, 3);
            this.DateGroup.Name = "DateGroup";
            this.DateGroup.Size = new System.Drawing.Size(220, 119);
            this.DateGroup.TabIndex = 0;
            this.DateGroup.TabStop = false;
            this.DateGroup.Text = "Date";
            // 
            // EnableStartDateCheck
            // 
            this.EnableStartDateCheck.AutoSize = true;
            this.EnableStartDateCheck.Location = new System.Drawing.Point(6, 19);
            this.EnableStartDateCheck.Name = "EnableStartDateCheck";
            this.EnableStartDateCheck.Size = new System.Drawing.Size(171, 17);
            this.EnableStartDateCheck.TabIndex = 1;
            this.EnableStartDateCheck.Text = "Enable Earliest Accepted Date";
            this.EnableStartDateCheck.UseVisualStyleBackColor = true;
            this.EnableStartDateCheck.CheckedChanged += new System.EventHandler(this.StartDateEnableCheck_CheckedChanged);
            // 
            // EnableEndDateCheck
            // 
            this.EnableEndDateCheck.AutoSize = true;
            this.EnableEndDateCheck.Location = new System.Drawing.Point(6, 68);
            this.EnableEndDateCheck.Name = "EnableEndDateCheck";
            this.EnableEndDateCheck.Size = new System.Drawing.Size(166, 17);
            this.EnableEndDateCheck.TabIndex = 3;
            this.EnableEndDateCheck.Text = "Enable Latest Accepted Date";
            this.EnableEndDateCheck.UseVisualStyleBackColor = true;
            this.EnableEndDateCheck.CheckedChanged += new System.EventHandler(this.EnableEndDateCheck_CheckedChanged);
            // 
            // EndDate
            // 
            this.EndDate.CustomFormat = "";
            this.EndDate.Location = new System.Drawing.Point(6, 91);
            this.EndDate.Name = "EndDate";
            this.EndDate.Size = new System.Drawing.Size(208, 20);
            this.EndDate.TabIndex = 4;
            this.EndDate.ValueChanged += new System.EventHandler(this.EndDate_ValueChanged);
            // 
            // StartDate
            // 
            this.StartDate.CustomFormat = "";
            this.StartDate.Location = new System.Drawing.Point(6, 42);
            this.StartDate.Name = "StartDate";
            this.StartDate.Size = new System.Drawing.Size(208, 20);
            this.StartDate.TabIndex = 2;
            this.StartDate.ValueChanged += new System.EventHandler(this.StartDate_ValueChanged);
            // 
            // DateDetails
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.DateGroup);
            this.ForeColor = System.Drawing.SystemColors.ControlText;
            this.Name = "DateDetails";
            this.Size = new System.Drawing.Size(226, 128);
            this.Load += new System.EventHandler(this.DateDetails_Load);
            this.ParentChanged += new System.EventHandler(this.DateDetails_ParentChanged);
            this.DateGroup.ResumeLayout(false);
            this.DateGroup.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox DateGroup;
        private System.Windows.Forms.DateTimePicker StartDate;
        private System.Windows.Forms.DateTimePicker EndDate;
        private System.Windows.Forms.CheckBox EnableEndDateCheck;
        private System.Windows.Forms.CheckBox EnableStartDateCheck;
    }
}
