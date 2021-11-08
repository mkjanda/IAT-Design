using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;

namespace IATClient
{
    class RegExResponseDisplay : ResponseDisplay
    {
        protected TextBox RegEx, TestBox;
        private static String Label = Properties.Resources.sRegExResponseLabel;
        private static String RegExDefaultText = Properties.Resources.sRegExEditDefaullt;
        private static String InvalidLabel = Properties.Resources.sRegExInvalidLabel;
        private static String MatchLabel = Properties.Resources.sRegExMatchLabel;
        private static String NoMatchLabel = Properties.Resources.sRegExNoMatchLabel;
        private static String TestDefaultText = Properties.Resources.sTestRegExDefault;
        private static String TestLabel = Properties.Resources.sTestRegExLabel;
        private RectangleF LabelRect = Rectangle.Empty;
        private const int LinePadding = 10;
        private const int RegExEditLeftMargin = 25;
        private const int TestBoxLeftMargin = 25;
        private const int InvalidLabelLeftMargin = 550;
        private const int TestResultLeftLabelMargin = 550;
        

        protected override CResponse GetResponse()
        {
            return new CRegExResponse(RegEx.Text);
        }

        public override void SetResponse(CResponse response)
        {
            RegEx.Text = ((CRegExResponse)response).RegEx;
            if (RegEx.Text == String.Empty)
                RegEx.Text = RegExDefaultText;
        }


        public RegExResponseDisplay()
        {
            RegEx = new TextBox();
            TestBox = new TextBox();
            foreach (var box in new TextBox[]{ RegEx, TestBox})
            {
                box.FontChanged += (sender, args) =>
                {
                    box.Invalidate();
                };
                box.MouseHover += (sender, args) =>
                {
                    TextBox b = (TextBox)sender;
                    b.ForeColor = Format.Color;
                    b.BackColor = Color.LightBlue;
                    b.BorderStyle = BorderStyle.FixedSingle;
                };
                box.MouseLeave += (sender, args) =>
                {
                    TextBox b = (TextBox)sender;
                    if (!b.Focused)
                    {
                        b.ForeColor = Format.Color;
                        b.BackColor = this.BackColor;
                        b.BorderStyle = BorderStyle.None;
                    }
                };
                box.Leave += (sender, args) =>
                {
                    TextBox b = (TextBox)sender;
                    b.ForeColor = Format.Color;
                    b.BackColor = this.BackColor;
                    b.BorderStyle = BorderStyle.None;
                    if ((b == RegEx) && (b.Text == String.Empty))
                        b.Text = RegExDefaultText;
                    else if ((b == TestBox) && (b.Text == String.Empty))
                        b.Text = TestDefaultText;
                };
                box.Enter += (sender, args) =>
                {
                    TextBox b = sender as TextBox;
                    b.ForeColor = Format.Color;
                    b.BackColor = Color.LightBlue;
                    if ((b == RegEx) && (b.Text == RegExDefaultText))
                        b.Text = String.Empty;
                    else if ((b == TestBox) && (b.Text == TestDefaultText))
                        b.Text = String.Empty;
                    if (Parent != null)
                        ((QuestionDisplay)Parent).Active = true;
                };
                box.Multiline = true;
                box.TabStop = true;
                if (box == RegEx)
                    box.Text = RegExDefaultText;
                box.TextChanged += (sender, args) => LayoutBox(box);
                box.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
                box.ForeColor = Format.Color;
                Controls.Add(box);
                if (box == TestBox)
                {
                    box.Location = new Point(InteriorPadding.Left, box.Bottom + LinePadding + ((RegEx.BorderStyle == BorderStyle.FixedSingle) ? RegEx.Margin.Top : 0) +
                        ((TestBox.BorderStyle != BorderStyle.FixedSingle) ? TestBox.Margin.Top : 0));
                }
                else if (box == RegEx)
                {
                    new Point(RegEx.Location.X, (int)LabelRect.Bottom + LinePadding - ((RegEx.BorderStyle == BorderStyle.FixedSingle) ? RegEx.Margin.Top : 0));
                }
                box.Width = Size.Width - InteriorPadding.Horizontal;
            };
            this.Height = TestBox.Bottom + InteriorPadding.Top;
            this.Paint += (sender, args) => ResponseDisplay_Paint(sender, args);
            Invalidate();
        }

        public override async void LayoutControl()
        {
            Action a = () =>
            {
                if (!Monitor.TryEnter(lockObj))
                    return;
                LayoutEvent.WaitOne();
                LayoutEvent.Reset();
                Monitor.Exit(lockObj);
                this.Invoke(new Action(() =>
                {
                    this.Width = Parent.Width;
                    Size sz = this.Size;
                    using (Graphics g = Graphics.FromHwnd(this.Handle))
                        LabelRect = new RectangleF(new PointF(Padding.Left, Padding.Top), g.MeasureString(Label, DisplayFont, sz.Width - Padding.Horizontal));
                    RegEx.Location = new Point(RegEx.Location.X, (int)LabelRect.Bottom + LinePadding);
                    LayoutBox(RegEx);
                    TestBox.Location = new Point(TestBox.Location.X, RegEx.Bottom + LinePadding);
                    LayoutBox(TestBox);
                    this.Size = new Size(this.Size.Width, TestBox.Bottom + Padding.Bottom);
                    Invalidate();
                    LayoutEvent.Set();
                }));
                (Parent as QuestionDisplay).RecalcSize(false);
            };
            await Task.Run(a);
        }
        


        protected void LayoutBox(TextBox box)
        {
            Size szF = Size.Empty;
            int nLines = 1;
            box.Width = InvalidLabelLeftMargin;
            szF = (box.Text == String.Empty) ? new Size(box.Width, box.Height) : TextRenderer.MeasureText(box.Text, DisplayFont,
                new Size(box.Width - InteriorPadding.Horizontal, 0), TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            double lineHeight = ((double)(DisplayFont.FontFamily.GetCellAscent(DisplayFont.Style) + DisplayFont.FontFamily.GetCellDescent(DisplayFont.Style))
                / (double)DisplayFont.FontFamily.GetEmHeight(DisplayFont.Style)) * Format.FontSizeAsPixels;
            nLines = (int)Math.Floor((double)(szF.Height - box.Margin.Vertical) / (double)DisplayFont.Size);
            box.Height = szF.Height + box.Margin.Vertical;
            Invalidate();
        }
        

        private void TextBox_MouseEnter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Focused)
                return;
            if ((tb == RegEx) && (tb.Text == RegExDefaultText))
                tb.Text = String.Empty;
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.ForeColor = Color.Black;
            tb.BackColor = Color.LightBlue;
            if ((tb == TestBox) && (tb.Text == TestDefaultText))
                tb.Text = String.Empty;
        }

        private void TextBox_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.ForeColor = Color.Black;
            tb.BackColor = System.Drawing.Color.LightBlue;
            if (tb == RegEx)
            {
                if (RegEx.Text == RegExDefaultText)
                    RegEx.Text = String.Empty;
            }
            else
            {
                if (TestBox.Text == TestDefaultText)
                    TestBox.Text = String.Empty;
            }
        }

        private void TextBox_Leave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            tb.BorderStyle = BorderStyle.None;
            tb.ForeColor = Format.Color;
            tb.BackColor = this.BackColor;
            if ((tb == TestBox) && (tb.Text == String.Empty))
                tb.Text = TestDefaultText;
            if ((tb == RegEx) && (tb.Text == String.Empty))
                tb.Text = RegExDefaultText;
        }

        private void TextBox_MouseLeave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Focused)
                return;
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = this.BackColor;
            tb.ForeColor = Format.Color;
            if ((tb == TestBox) && (tb.Text == String.Empty))
                tb.Text = TestDefaultText;
            if ((tb == RegEx) && (tb.Text == String.Empty))
                tb.Text = RegExDefaultText;
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == RegEx)
                (SurveyItem.Response as CRegExResponse).RegEx = tb.Text;
            ResumeLayout(true);
            Invalidate();
        }

        protected bool RegExIsValid(String regEx)
        {
            try {
                new Regex(regEx);
            }
            catch (ArgumentException)
            {
                return false;
            }
            return true;
        }

        protected override void ChangeResponseFont()
        {
            RegEx.Font = DisplayFont;
            TestBox.Font = DisplayFont;
            TestBox.ForeColor = Format.Color;
            RegEx.ForeColor = Format.Color;
            int tbHeight = TextRenderer.MeasureText(TestBox.Text, DisplayFont, new Size(RegEx.Width, 1),
                TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl).Height;
            RegEx.Bounds = new Rectangle(RegEx.Location, new Size(RegEx.Width, tbHeight));
            TestBox.Bounds = new Rectangle(TestBox.Location, new Size(RegEx.Width, tbHeight));
            base.ChangeResponseFont();
        }

        protected override void ResponseDisplay_Paint(object sender, PaintEventArgs e)
        {
    //        e.Graphics.FillRectangle(Brushes.White, new Rectangle(new Point(RegEx.Right, RegEx.Top), new Size(this.Size.Width - RegEx.Right, this.Size.Height - TestBox.Bottom)));
            e.Graphics.DrawString(Label, DisplayFont, Brushes.Gray, LabelRect);
            if ((RegEx.Text != RegExDefaultText) || ((RegEx.Text != String.Empty) && (TestBox.Text != String.Empty)))
            {
                if (!RegExIsValid(RegEx.Text))
                    e.Graphics.DrawString(InvalidLabel, DisplayFont, Brushes.Red, new Point(Padding.Left + InvalidLabelLeftMargin, RegEx.Location.Y));
                else
                {
                    if (TestBox.Text != TestDefaultText)
                    {
                        if (Regex.IsMatch(TestBox.Text, RegEx.Text))
                            e.Graphics.DrawString(MatchLabel, DisplayFont, Brushes.Green, new Point(Padding.Left + TestResultLeftLabelMargin, TestBox.Location.Y));
                        else
                            e.Graphics.DrawString(NoMatchLabel, DisplayFont, Brushes.Red, new Point(Padding.Left + TestResultLeftLabelMargin, TestBox.Location.Y));
                    }
                }
            }
        }
    }
}
