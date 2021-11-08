using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    abstract class OneNumberResponseDisplay : ResponseDisplay
    {
        protected NumericUpDown Value1;
        protected Rectangle Value1Rect;
        protected String ResponseLabel;
        protected String Value1Label;
        private RectangleF ResponseLabelRect = Rectangle.Empty;
        private int Value1LabelWidth;
        protected static Padding LengthEditMargin = new Padding(5, 5, 5, 5);
        protected const int LinePadding = 5;
        protected const int ValueLineIndent = 25;

        protected virtual int ValueEntryWidth
        {
            get
            {
                return 40;
            }
        }


        public OneNumberResponseDisplay(String ResponseLabel, String Value1Label)
        {
            Value1 = new NumericUpDown();
            Value1.Minimum = decimal.MinValue;
            Value1.Maximum = decimal.MaxValue;
            Value1.ValueChanged += new EventHandler(Value1Changed);
            Value1.LostFocus += (sender, args) =>
            {
                Value1.Visible = false;
                Invalidate();
            };
            this.Controls.Add(Value1);
            this.ResponseLabel = ResponseLabel;
            this.Value1Label = Value1Label;
            this.MouseMove += new MouseEventHandler(OneNumberResponseDisplay_MouseMove);
            this.Value1Rect = Rectangle.Empty;
        }

        protected abstract void Value1Changed(object sender, EventArgs e);

        void OneNumberResponseDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            bool Visible = Value1.Visible;
            if (Value1Rect.Contains(e.Location))
                Value1.Visible = true;
            else if (!Value1.Focused)
                Value1.Visible = false;
            if (Visible != Value1.Visible)
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
                    using (Graphics g = Graphics.FromHwnd(this.Handle))
                    {
                        ResponseLabelRect = new RectangleF(new Point(Padding.Left, Padding.Top),
                            g.MeasureString(ResponseLabel, DisplayFont, ClientRectangle.Width - Padding.Horizontal));
                    }
                    Value1LabelWidth = TextRenderer.MeasureText(Value1Label, DisplayFont).Width;
                    Value1.BackColor = this.BackColor;
                    Value1.DecimalPlaces = 0;
                    Value1.BorderStyle = BorderStyle.FixedSingle;
                    Value1.Location = new Point(Padding.Left + Value1LabelWidth + LengthEditMargin.Left + ValueLineIndent, (int)ResponseLabelRect.Bottom + LinePadding);
                    Value1.Size = new Size(Value1.Padding.Horizontal + Value1.Margin.Horizontal + ValueEntryWidth, Value1.Margin.Vertical + Value1.Padding.Vertical + DisplayFont.Height);
                    Value1Rect = new Rectangle(Value1.Location, Value1.Size);
                    Value1.Visible = false;
                    Invalidate();
                    this.Size = new Size(ClientRectangle.Width, Value1.Bottom + Padding.Bottom);
                }));
                LayoutEvent.Set();
                (Parent as QuestionDisplay).RecalcSize(false);
            };
            await Task.Run(a);
        }

        protected override void ChangeResponseFont()
        {
            Value1.Font = DisplayFont;
            Value1.ForeColor = Format.Color;
            base.ChangeResponseFont();
        }

        protected override void ResponseDisplay_Paint(object sender, PaintEventArgs e)
        {
            Brush br = new SolidBrush(System.Drawing.Color.Gray);
            Brush dispBr = new SolidBrush(Format.Color);
            e.Graphics.DrawString(ResponseLabel, DisplayFont, br, ResponseLabelRect);
            e.Graphics.DrawString(Value1Label, DisplayFont, br, new PointF(Padding.Left + ValueLineIndent, ResponseLabelRect.Height + LinePadding + Value1.Margin.Top));
            if (!Value1.Visible)
                e.Graphics.DrawString(Value1.Value.ToString(), DisplayFont, dispBr, Value1.Location + new Size(0, Value1.Margin.Top));
            br.Dispose();
            dispBr.Dispose();
        }

    }
}
