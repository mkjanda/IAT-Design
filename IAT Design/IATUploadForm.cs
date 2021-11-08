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
    public partial class IATUploadForm : Form
    {
        protected String _ServerURL;

        public String ServerURL
        {
            get
            {
                return _ServerURL;
            }
            set
            {
                _ServerURL = value;
            }
        }

        public IATUploadForm()
        {
            InitializeComponent();
            _ServerURL = String.Empty;
        }

        private void Upload_Click(object sender, EventArgs e)
        {
            if (ServerURLEdit.Text.IndexOf("http://") == -1)
                _ServerURL = "http://" + ServerURLEdit.Text;
            else
                _ServerURL = ServerURLEdit.Text;
            if (_ServerURL[_ServerURL.Length - 1] != '/')
                _ServerURL += '/';
            DialogResult = DialogResult.OK;
            Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
