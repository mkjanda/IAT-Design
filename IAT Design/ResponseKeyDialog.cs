using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    public partial class ResponseKeyDialog : Form
    {
        private ResponseKeyPanel responseKeyPanel;

        public bool Modified
        {
            set
            {
                ((IATConfigMainForm)Owner).Modified = value;
            }
        }

        public String ErrorMsg
        {
            set
            {
                if (value == String.Empty)
                {
                    StatusText.Text = "Okay";
                    StatusImage.Image = Properties.Resources.go;
                }
                else
                {
                    StatusText.Text = value;
                    StatusImage.Image = Properties.Resources.stop;
                }
            }
        }

        public ResponseKeyDialog()
        {
            this.AutoScaleDimensions = new System.Drawing.SizeF(72F, 72F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            InitializeComponent();
            this.AutoSize = true;
            this.Font = new Font(SystemFonts.DefaultFont.FontFamily, 10F);
            responseKeyPanel = new ResponseKeyPanel();
            this.responseKeyPanel.Size = new System.Drawing.Size(787, 505);
            responseKeyPanel.AutoScaleMode = AutoScaleMode.Dpi;
            responseKeyPanel.AutoScaleDimensions = new SizeF(72F, 72F);
            responseKeyPanel.Location = new Point(0, 0);

            Controls.Add(responseKeyPanel);
        }
    }
}
