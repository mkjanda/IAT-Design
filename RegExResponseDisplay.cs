using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;
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
        private const int InvalidLabelLeftMargin = 600;
        private const int TestResultLeftLabelMargin = 675;
        

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
            Padding = new Padding(10, 5, 10, 10);
            RegEx.ForeColor = Color.Gray;
            RegEx.BackColor = Color.White;
            RegEx.Font = ControlFont;
            RegEx.Padding = new Padding(3);
            RegEx.Location = new Point(RegExEditLeftMargin, Padding.Top + ControlFont.Height + LinePadding);
            RegEx.Size = new Size(550, RegEx.Padding.Vertical + RegEx.Margin.Vertical + (int)ControlFont.GetHeight());
            RegEx.BorderStyle = BorderStyle.None;
            RegEx.Text = RegExDefaultText;
            RegEx.MouseEnter += new EventHandler(TextBox_MouseEnter);
            RegEx.MouseLeave += new EventHandler(TextBox_MouseLeave);
            RegEx.Enter += new EventHandler(TextBox_Enter);
            RegEx.Leave += new EventHandler(TextBox_Leave);
            RegEx.TextChanged += new EventHandler(TextBox_TextChanged);
            Controls.Add(RegEx);
            TestBox.ForeColor = System.Drawing.Color.Gray;
            TestBox.BackColor = Color.White;
            TestBox.Font = ControlFont;
            TestBox.Padding = new Padding(3);
            TestBox.Location = new Point(TestBoxLeftMargin, RegEx.Location.Y + RegEx.Size.Height + LinePadding);
            TestBox.Size = new Size(550, TestBox.Padding.Vertical + TestBox.Margin.Vertical + (int)ControlFont.GetHeight());
            TestBox.BorderStyle = BorderStyle.None;
            TestBox.MouseEnter += new EventHandler(TextBox_MouseEnter);
            TestBox.MouseLeave += new EventHandler(TextBox_MouseLeave);
            TestBox.Enter += new EventHandler(TextBox_Enter);
            TestBox.Leave += new EventHandler(TextBox_Leave);
            TestBox.TextChanged += new EventHandler(TextBox_TextChanged);
            TestBox.Text = TestDefaultText;
            Controls.Add(TestBox);
        }

        protected override void LayoutControl()
        {
            if (UpdatingFromCode)
                return;
            UpdatingFromCode = true;
            Size sz = this.Size;
            using (Graphics g = Graphics.FromHwnd(this.Handle))
                LabelRect = new RectangleF(new PointF(Padding.Left, Padding.Top), g.MeasureString(Label, DisplayFont, sz.Width - Padding.Horizontal));
            RegEx.Location = new Point(RegEx.Location.X, (int)LabelRect.Bottom + LinePadding - ((RegEx.BorderStyle == BorderStyle.FixedSingle) ? RegEx.Margin.Top : 0));
            TestBox.Location = new Point(TestBox.Location.X, RegEx.Bottom + LinePadding - ((RegEx.BorderStyle == BorderStyle.FixedSingle) ? RegEx.Margin.Top : 0) + 
                ((TestBox.BorderStyle != BorderStyle.FixedSingle) ? TestBox.Margin.Top : 0));
            this.Size = new Size(this.Size.Width, TestBox.Bottom + Padding.Bottom);
            Invalidate();
            (Parent as QuestionDisplay).RecalcSize();
            UpdatingFromCode = false;
        }

        private void TextBox_MouseEnter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (tb.Focused)
                return;
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.ForeColor = Color.Black;
            tb.BackColor = Color.LightBlue;
            if ((tb == RegEx) && (tb.Text == RegExDefaultText))
                tb.Text = String.Empty;
            if ((tb == TestBox) && (tb.Text == TestDefaultText))
                tb.Text = String.Empty;
            LayoutControl();
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
            LayoutControl();
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
            LayoutControl();
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
            LayoutControl();
        }

        private void TextBox_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = sender as TextBox;
            if (tb == RegEx)
                (SurveyItem.Response as CRegExResponse).RegEx = tb.Text;
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
            ControlFont = new Font(ControlFont.FontFamily, (float)Math.Sqrt(ControlFont.SizeInPoints) * 3);
            base.ChangeResponseFont();
        }

        protected override void ResponseDisplay_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.FillRectangle(Brushes.White, new Rectangle(new Point(0, 0), this.Size));
            e.Graphics.DrawString(Label, DisplayFont, Brushes.Gray, LabelRect);
//            e.Graphics.DrawString(TestLabel, DisplayFont, Brushes.Gray,
 //               new Point(Padding.Left + TestBoxLeftMargin - TextRenderer.MeasureText(TestLabel + " ", DisplayFont).Width, TestBox.Location.Y + ((TestBox.BorderStyle == BorderStyle.FixedSingle) ? TestBox.Margin.Top : 0)));
            if ((RegEx.Text != RegExDefaultText) || ((RegEx.Text != String.Empty) && (TestBox.Text != String.Empty)))
            {
                if (!RegExIsValid(RegEx.Text))
                    e.Graphics.DrawString(InvalidLabel, ControlFont, Brushes.Red, new Point(Padding.Left + InvalidLabelLeftMargin, RegEx.Location.Y));
                else
                {
                    if (TestBox.Text != TestDefaultText)
                    {
                        if (Regex.IsMatch(TestBox.Text, RegEx.Text))
                            e.Graphics.DrawString(MatchLabel, ControlFont, Brushes.Green, new Point(Padding.Left + TestResultLeftLabelMargin, TestBox.Location.Y));
                        else
                            e.Graphics.DrawString(NoMatchLabel, ControlFont, Brushes.Red, new Point(Padding.Left + TestResultLeftLabelMargin, TestBox.Location.Y));
                    }
                }
            }
        }
    }
}
