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
    partial class BoundedLengthDeleteConfirmation : Form
    {
        private List<CBoundedLengthResponseObject.CSearch> Searches = new List<CBoundedLengthResponseObject.CSearch>();
        private Panel ParentPanel = new Panel();
        public BoundedLengthDeleteConfirmation(List<CBoundedLengthResponseObject.CSearch> searches)
        {

            InitializeComponent();
            Searches.AddRange(searches);
            ParentPanel.Location = new Point(Instructions.Left + 20, Instructions.Bottom + 10);
            ParentPanel.Size = new Size(Instructions.Right - 20 - ParentPanel.Location.X, ProceedButton.Top - 10);
            ParentPanel.AutoScroll = true;
            ParentPanel.VerticalScroll.Visible = true;
            Panel childPanel;
            TextBox tb;
            int yOffset = 0;
            foreach (CBoundedLengthResponseObject.CSearch s in Searches)
            {
                tb = new TextBox();
                tb.Text = s.Description;
                tb.Multiline = true;
                tb.Size = TextRenderer.MeasureText(tb.Text, System.Drawing.SystemFonts.DialogFont, new Size(ParentPanel.ClientRectangle.Width - 10, 0),
                    TextFormatFlags.WordBreak | TextFormatFlags.GlyphOverhangPadding);
                childPanel = new Panel();
                childPanel.Location = new Point(0, yOffset);
                childPanel.Size = new Size(ParentPanel.ClientRectangle.Width, tb.Size.Height + 10);
                childPanel.BorderStyle = BorderStyle.FixedSingle;
                tb.Location = childPanel.Location + new Size(5, 5);
                childPanel.Controls.Add(tb);
                yOffset += childPanel.Height;
                ParentPanel.Controls.Add(childPanel);
            }
            int nDiff = 0;
            if ((nDiff = ParentPanel.Height - yOffset) > 0)
                ParentPanel.Height -= nDiff;
            ProceedButton.Location += new Size(0, -nDiff);
            Cancel.Location += new Size(0, -nDiff);
            this.Height -= nDiff;
        }

        private void ProceedButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
 