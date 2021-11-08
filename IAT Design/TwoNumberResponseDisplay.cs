using System;
using System.Collections.Generic;

using System.Threading;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    abstract class TwoNumberResponseDisplay : OneNumberResponseDisplay
    {
        protected NumericUpDown Value2;
        protected Rectangle Value2Rect;
        protected String Value2Label;
        
        protected override int ValueEntryWidth
        {
            get
            {
                return 100;
            }
        }

        public TwoNumberResponseDisplay(String ResponseLabel, String Value1Label, String Value2Label)
            : base(ResponseLabel, Value1Label)
        {
            Value2 = new NumericUpDown();
            Value2.Minimum = decimal.MinValue;
            Value2.Maximum = decimal.MaxValue;
            Value2.ValueChanged += new EventHandler(Value2Changed);
            Value2.LostFocus += (sender, args) =>
            {
                Value2.Visible = false;
                Invalidate();
            };
            Controls.Add(Value2);
            this.Value2Label = Value2Label;
            this.Value2Rect = Rectangle.Empty;
            this.MouseMove += new MouseEventHandler(TwoNumberResponseDisplay_MouseMove);
        }

        protected abstract void Value2Changed(object sender, EventArgs e);

        protected override void ChangeResponseFont()
        {
            Value2.Font = DisplayFont;
            Value2.ForeColor = Format.Color;
            base.ChangeResponseFont();
        }

        void TwoNumberResponseDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            bool Visible = Value2.Visible;
            if (Value2Rect.Contains(e.Location))
                Value2.Visible = true;
            else if (!Value2.Focused)
                Value2.Visible = false;
            if (Visible != Value2.Visible)
                Invalidate();
            Visible = Value1.Visible;
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
                    Value1.Size = new Size(TextRenderer.MeasureText("WWWW", DisplayFont).Width + Value1.Margin.Horizontal + 2, DisplayFont.Height + Value1.Margin.Vertical + Value1.Padding.Vertical + DisplayFont.Height);
                    Value2.Size = new Size(TextRenderer.MeasureText("WWWW", DisplayFont).Width + Value2.Margin.Horizontal + 2, DisplayFont.Height + Value2.Margin.Vertical + Value2.Padding.Vertical + DisplayFont.Height);
                    int val1Width = TextRenderer.MeasureText(Value1Label, DisplayFont).Width + LengthEditMargin.Horizontal + Value1.Width;
                    int val2Width = TextRenderer.MeasureText(Value2Label, DisplayFont).Width + LengthEditMargin.Horizontal + Value2.Width;
                    if (val1Width + val2Width + 2 * LengthEditMargin.Horizontal > ClientSize.Width)
                    {
                        Value1.Location = new Point(LengthEditMargin.Horizontal + TextRenderer.MeasureText(Value1Label, DisplayFont).Width, Padding.Top + LinePadding - Value1.Margin.Top);
                        Value2.Location = new Point(LengthEditMargin.Horizontal + TextRenderer.MeasureText(Value2Label, DisplayFont).Width, Value1.Bottom + Padding.Top + 2 * LinePadding - Value2.Margin.Top);
                        this.Height = Math.Min(Value1.Location.Y + Value1.Size.Height, Value2.Location.Y + Value2.Size.Height);
                    }
                    else
                    {
                        Value1.Location = new Point(LengthEditMargin.Horizontal + TextRenderer.MeasureText(Value1Label, DisplayFont).Width, Padding.Top + LinePadding - Value1.Margin.Top);
                        Value2.Location = new Point((ClientSize.Width >> 1) + LengthEditMargin.Horizontal + TextRenderer.MeasureText(Value2Label, DisplayFont).Width, Padding.Top + LinePadding - Value2.Margin.Top);
                        this.Height = Math.Min(Value1.Location.Y + Value1.Size.Height, Value2.Location.Y + Value2.Size.Height);
                    }
                    Value1.DecimalPlaces = 0;
                    Value2.DecimalPlaces = 0;
                    Value2Rect = new Rectangle(Value2.Location, Value2.Size);
                    Value1Rect = new Rectangle(Value1.Location, Value1.Size);
                    Value1.Visible = false;
                    Value2.Visible = false;
                    this.Height = Value2.Bottom + LengthEditMargin.Bottom;
                    Invalidate();
                }));
                LayoutEvent.Set();
                (Parent as QuestionDisplay).RecalcSize(false);
            };
            await Task.Run(a);
        }

        protected override void ResponseDisplay_Paint(object sender, PaintEventArgs e)
        {
            Brush br = new SolidBrush(System.Drawing.Color.Gray);
            Brush blackBr = new SolidBrush(Format.Color);
            e.Graphics.DrawString(Value1Label, DisplayFont, br, new Point(LengthEditMargin.Left, Padding.Top + LinePadding));
            if (!Value1.Visible)
                e.Graphics.DrawString(Value1.Value.ToString(), DisplayFont, blackBr, new Point(Value1.Left, Padding.Top + LinePadding));
            if (Value1.Location.Y == Value2.Location.Y) {
                e.Graphics.DrawString(Value2Label, DisplayFont, br, new Point((ClientSize.Width >> 1) + LengthEditMargin.Left, Padding.Top + LinePadding));
                if (!Value2.Visible)
                    e.Graphics.DrawString(Value2.Value.ToString(), DisplayFont, blackBr, new Point(Value2.Left, Padding.Top + LinePadding));
            }
            else {
                e.Graphics.DrawString(Value2Label, DisplayFont, br, new Point(LengthEditMargin.Left, Padding.Top + Value1.Height + (LinePadding << 1)));
                if (!Value2.Visible)
                    e.Graphics.DrawString(Value2.Value.ToString(), DisplayFont, blackBr, new Point(Value2.Left, Padding.Top + Value1.Height + (LinePadding << 1)));
            }
            br.Dispose();
            blackBr.Dispose();
        }
    }
}
