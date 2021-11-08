using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    public partial class DataPasswordForm : Form
    {
        public delegate void SetProgressRangeHandler(int nMin, int nMax);
        public delegate void ProgressIncrementHandler(int nInc);
        public delegate void SetStatusMessageHandler(String StatusMsg);
        public delegate void OperationCompleteHandler();
        public delegate void ResetProgressHandler();
        public delegate bool IsLoadedHandler(bool bFlagToTrue);
        public delegate DialogResult DisplayYesNoMessageBoxHandler(String msg, String caption);
        public delegate void OperationFailedHandler(String title, String message);
        public delegate void FormClosingHandler();
        public delegate void CloseDelegate();

        public enum EDataPassword { match, noMatch, cancel };
        private object lockObject = new object();
        private bool _CloseFlag = false;
        public new FormClosingHandler OnClosing = null;
        private bool ExternallyClosing = false;
        private bool _IsLoaded;

        public DataPasswordForm()
        {
            InitializeComponent();
        }

        public String Password
        {
            get
            {
                return PasswordBox.Text;
            }
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

    }
}
