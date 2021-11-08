using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public partial class ConfirmV11Form : Form
    {
        public bool StopShowingForm { get { return StopShowing.Checked; } }

        public ConfirmV11Form()
        {
            InitializeComponent();
        }


        private void OK_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void V10Link_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            V10Link.LinkVisited = true;
            System.Diagnostics.Process.Start("https://www.iatsoftware.net/DownloadSoftware/V10");
        }
    }
}
