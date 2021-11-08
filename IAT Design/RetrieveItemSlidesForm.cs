using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    public partial class RetrieveItemSlidesForm : Form
    {

        public String IATName
        {
            get
            {
                return IATNameBox.Text;
            }
        }

        public String Password
        {
            get
            {
                return PasswordBox.Text;
            }
        }

        public RetrieveItemSlidesForm()
        {
            InitializeComponent();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void RetrieveButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            this.Close();
        }
    }
}
