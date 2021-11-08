using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
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
            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            responseKeyPanel = new ResponseKeyPanel();
            responseKeyPanel.Location = new Point(0, 0);
            Controls.Add(responseKeyPanel);
            this.ClientSize = responseKeyPanel.Size + new Size(0, MessageBar.Height);
        }
    }
}
