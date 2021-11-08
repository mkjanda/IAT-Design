using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;
/*
namespace IATClient
{
    partial class IATPackageControl : UserControl
    {
        private static String[] ResponseKeyValues = {"A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L",
                                                        "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W",
                                                        "X", "Y", "Z" };

        public PackagerForm.ChildControlCompleteEventHandler OnControlComplete;

        private void ValidateInput()
        {
            if (IATName.Text == String.Empty)
            {
                OK.Enabled = false;
                return;
            }
            else if (RedirectionURL.Text == String.Empty)
            {
                OK.Enabled = false;
                return;
            }
            else if (PackageFileName.Text == String.Empty)
            {
                OK.Enabled = false;
                return;
            }
            else if ((!FixedOrder.Checked) && (!RandomOrder.Checked) && (!SetNumPresentations.Checked))
            {
                OK.Enabled = false;
                return;
            }
            OK.Enabled = true;
        }

        public String LeftResponseChar
        {
            get
            {
                return LeftResponseKeyDrop.SelectedItem.ToString();
            }
        }

        public String RightResponseChar
        {
            get
            {
                return RightResponseKeyDrop.SelectedItem.ToString();
            }
        }

        public String theIATName
        {
            get
            {
                return IATName.Text;
            }
        }

        public String thePackageFileName
        {
            get
            {
                return PackageFileName.Text;
            }
        }

        public String theRedirectionURL
        {
            get
            {
                String val = RedirectionURL.Text;
                if (!val.StartsWith("http://") && !val.StartsWith("https://"))
                    val = "http://" + val;
                return val;
            }
        }

        public IATConfigFileNamespace.ConfigFile.ERandomizationType RandomizationType
        {
            get
            {
                if (FixedOrder.Checked)
                    return IATConfigFileNamespace.ConfigFile.ERandomizationType.None;
                else if (RandomOrder.Checked)
                    return IATConfigFileNamespace.ConfigFile.ERandomizationType.RandomOrder;
                else
                    return IATConfigFileNamespace.ConfigFile.ERandomizationType.SetNumberOfPresentations;
            }
        }

        public IATPackageControl()
        {
            InitializeComponent();
            LeftResponseKeyDrop.Items.AddRange(ResponseKeyValues);
            RightResponseKeyDrop.Items.AddRange(ResponseKeyValues);
            LeftResponseKeyDrop.SelectedIndex = 4;
            RightResponseKeyDrop.SelectedIndex = 8;
            OnControlComplete = null;
        }

        private void FixedOrder_CheckedChanged(object sender, EventArgs e)
        {
            if (FixedOrder.Checked)
                OK.Text = "OK";
            ValidateInput();
        }

        private void RandomOrder_CheckedChanged(object sender, EventArgs e)
        {
            if (RandomOrder.Checked)
                OK.Text = "OK";
            ValidateInput();
        }

        private void SetNumPresentations_CheckedChanged(object sender, EventArgs e)
        {
            if (SetNumPresentations.Checked)
                OK.Text = "Next";
            ValidateInput();
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            SaveFileDialog dlg = new SaveFileDialog();
            dlg.Filter = Properties.Resources.sPackageFileFilter;
            dlg.DefaultExt = Properties.Resources.sPackageFileExtension;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                PackageFileName.Text = dlg.FileName;
            }
            ValidateInput();
        }

        private void OK_Click(object sender, EventArgs e)
        {
            OnControlComplete(PackagerForm.EChildControlResult.ok);
        }

        private void IATName_TextChanged(object sender, EventArgs e)
        {
            ValidateInput();
        }

        private void RedirectionURL_TextChanged(object sender, EventArgs e)
        {
            ValidateInput();
        }

        private void DataRetrievalPassword_TextChanged(object sender, EventArgs e)
        {
            ValidateInput();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            OnControlComplete(PackagerForm.EChildControlResult.cancel);
        }
    }
}
*/