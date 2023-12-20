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
            InitializeComponent();
            this.AutoScaleDimensions = new SizeF(72F, 72F);
            this.AutoScaleMode = AutoScaleMode.Dpi;
            this.AutoSize = true;
            responseKeyPanel = new ResponseKeyPanel();
            responseKeyPanel.Location = new Point(0, 0);
            responseKeyPanel.Size = this.ClientSize - new Size(0, MessageBar.Height);
            responseKeyPanel.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            Controls.Add(responseKeyPanel);
        }
    }
}
