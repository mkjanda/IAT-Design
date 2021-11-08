using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    partial class ErrorForm : Form
    {
        private Dictionary<IValidatedItem, CValidationException> Errors;
        private Dictionary<LinkLabel, CLocationDescriptor> ErrorBoxTable = new Dictionary<LinkLabel, CLocationDescriptor>();
        private Panel ErrorPanel = new Panel();
        private TextBox Header = new TextBox();
        private IATConfigMainForm MainForm;

        public ErrorForm(Dictionary<IValidatedItem, CValidationException> errors, IATConfigMainForm mainForm)
        {
            InitializeComponent();
            Header.Multiline = true;
            Header.ReadOnly = true;
            Header.BorderStyle = BorderStyle.None;
            Header.Size = new Size(360, 40);
            Header.Location = new Point((ClientRectangle.Width - Header.Width) >> 1, ClientRectangle.Top + 10);
            Header.Text = Properties.Resources.sErrorFormHeader;
            ErrorPanel.Controls.Add(Header);
            this.Load += new EventHandler(ErrorForm_Load);
            Errors = errors;
            MainForm = mainForm;
        }

        void ErrorForm_Load(object sender, EventArgs e)
        {
            List<Size> tbSizes = new List<Size>();
            List<LinkLabel> ErrorBoxes = new List<LinkLabel>();
            Point loc = new Point((int)(Header.Left + (Header.Width * .05)), Header.Bottom + 20);
            Size szBounds = new Size((int)(Header.Width * .9), 1);
            int vertMargin = 8;
            foreach (IValidatedItem i in Errors.Keys)
            {
                LinkLabel tb = new LinkLabel();
                tb.Location = loc;
                tb.Size = TextRenderer.MeasureText(Errors[i].ErrorText, tb.Font, szBounds);
                tb.Text = Errors[i].ErrorText;
                ErrorBoxTable[tb] = Errors[i].LocationDescriptor;
                ErrorBoxes.Add(tb);
                loc += new Size(0, tb.Height + vertMargin);
            }
            int ctr = Errors.Keys.Count - 1;
            int Height = loc.Y - vertMargin;
            while (Height > 500)
                Height = ErrorBoxes[--ctr].Bottom;
            ErrorPanel.Size = new Size(ClientRectangle.Width, Height);
            ErrorPanel.Location = new Point(0, 0);
            for (int ctr2 = 0; ctr2 <= ctr; ctr2++)
            {
                ErrorPanel.Controls.Add(ErrorBoxes[ctr2]);
                ErrorBoxes[ctr2].Click += new EventHandler(ErrorBox_Click);
            }
            ErrorPanel.Height = Height;
            this.Height = ErrorPanel.Height + 50;
            Controls.Add(ErrorPanel);
            Header.SelectionLength = 0;
        }

        private void ErrorBox_Click(object sender, EventArgs e)
        {
            this.Close();
            if (MainForm != null)
                ErrorBoxTable[(LinkLabel)sender].OpenItem(MainForm);
        }
    }
}
