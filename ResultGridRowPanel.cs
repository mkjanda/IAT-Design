using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class ResultGridRowPanel : Panel
    {
        private List<TextBox> ResultLabels = new List<TextBox>();
        private Action<ResultGridRowPanel> ClickDelegate;
        private Control InvokeTarget;
        private Font NormalFont, SelectedFont;
        private bool _Selected = false;

        public bool Selected
        {
            get
            {
                return _Selected;
            }
            set
            {
                if (value && !_Selected)
                {
                    this.SuspendLayout();
                    foreach (TextBox tb in ResultLabels)
                    {
                        tb.Font = SelectedFont;
                    }
                    this.ResumeLayout(false);
                }
                else if (_Selected && !value)
                {
                    SuspendLayout();
                    foreach (TextBox tb in ResultLabels) 
                    {
                        tb.Font = NormalFont;
                    }
                    ResumeLayout(false);
                }
                _Selected = value;
            }
        }


        public ResultGridRowPanel(List<Rectangle> cellRects, List<String> values, Font font, Action<ResultGridRowPanel> clickDelegate, Control invokeTarget)
        {
            for (int ctr = 0; ctr < values.Count; ctr++)
            {
                TextBox tb = new TextBox();
                tb.ReadOnly = true;
                tb.Multiline = true;
                tb.BackColor = Color.White;
                tb.ForeColor = Color.Black;
                tb.Font = font;
                tb.Text = values[ctr];
                tb.BorderStyle = BorderStyle.None;
                tb.TextAlign = HorizontalAlignment.Center;
                tb.Click += new EventHandler(Row_Click);
                tb.Location = new Point(ResultsGridPanel.RowPadding.Left + cellRects[ctr].Left - cellRects[0].Left, ResultsGridPanel.RowPadding.Top + cellRects[ctr].Top);
                tb.Size = cellRects[ctr].Size - new Size(1, 0);
                ResultLabels.Add(tb);
                Controls.Add(tb);
            }
            NormalFont = font;
            SelectedFont = new Font(font, FontStyle.Bold);
            this.Click += new EventHandler(Row_Click);
            this.BackColor = Color.White;
            ClickDelegate = clickDelegate;
            InvokeTarget = invokeTarget;
            this.Paint += new PaintEventHandler(ResultGridRowPanel_Paint);
        }

        void ResultGridRowPanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.DrawLine(Pens.Black, new Point(0, this.Height - 1), new Point(this.Width - 1, this.Height - 1));
            for (int ctr = 0; ctr < ResultLabels.Count - 1; ctr++)
                e.Graphics.DrawLine(Pens.Black, new Point(ResultLabels[ctr].Right, 0), new Point(ResultLabels[ctr].Right, this.Height - 1));
           
        }

        private void Row_Click(object sender, EventArgs e)
        {
            Selected = true;
            InvokeTarget.BeginInvoke(ClickDelegate, this);
        }
    }
}
