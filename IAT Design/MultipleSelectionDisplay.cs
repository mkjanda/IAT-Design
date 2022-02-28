using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{

    class MultipleSelectionDisplay : ChoiceResponseDisplay
    {
        private static Padding ChoiceEditPadding = new Padding(45, 2, 35, 3);
        private static Padding SelectionLimitsPadding = new Padding(10, 10, 10, 10);
        private const int ValueLabelMargin = 5;
        private const int SelectionSpacing = 40;
        protected NumericUpDown MinSelections, MaxSelections;
        private const int ValueEntryWidth = 40;
        private Point ptMinSelectionsLabel = new Point(0, 0), ptMaxSelectionsLabel = new Point(0, 0);
        private int arrowThingGuessWidth = 20;
        protected override Padding GetChoiceEditPadding()
        {
            return ChoiceEditPadding;
        }

        public MultipleSelectionDisplay()
            : base(true)
        {
            MinSelections = new NumericUpDown();
            MinSelections.Minimum = 0;
            MinSelections.Maximum = 0;
            MinSelections.TextAlign = HorizontalAlignment.Left;
            MinSelections.ForeColor = System.Drawing.Color.Black;
            MinSelections.BackColor = Color.White;
            MinSelections.BorderStyle = BorderStyle.FixedSingle;
            MinSelections.Value = 0;
            MinSelections.DecimalPlaces = 0;
            var szNumSelections = TextRenderer.MeasureText("0", DisplayFont, new Size(100, 1), TextFormatFlags.TextBoxControl);
            //            Size szNumSelections = new Size(MinSelections.Padding.Horizontal + MinSelections.Margin.Vertical + ValueEntryWidth,
            //              MinSelections.Padding.Vertical + MinSelections.Margin.Vertical + DisplayFont.Height);
            szNumSelections = new Size(szNumSelections.Width + arrowThingGuessWidth, szNumSelections.Height + MinSelections.Margin.Vertical);
            MinSelections.Size = szNumSelections;
            MinSelections.Visible = false;
            MaxSelections = new NumericUpDown();
            MaxSelections.Maximum = 0;
            MaxSelections.Maximum = 0;
            MaxSelections.TextAlign = HorizontalAlignment.Left;
            MaxSelections.ForeColor = System.Drawing.Color.Black;
            MaxSelections.BackColor = Color.White;
            MaxSelections.BorderStyle = BorderStyle.FixedSingle;
            MaxSelections.DecimalPlaces = 0;
            MaxSelections.Value = 0;
            MaxSelections.Size = szNumSelections;
            MaxSelections.Visible = false;
            MinSelections.LostFocus += (sender, args) =>
            {
                MinSelections.Visible = false;
                Invalidate();
            };
            MaxSelections.LostFocus += (sender, args) =>
            {
                MaxSelections.Visible = false;
                Invalidate();
            };
            this.MouseMove += (sender, args) =>
            {
                if (MaxSelections.Bounds.Contains(args.Location) && !MaxSelections.Visible)
                {
                    MaxSelections.Visible = true;
                    Invalidate();
                }
                else if (!MaxSelections.Bounds.Contains(args.Location) && MaxSelections.Visible)
                {
                    MaxSelections.Visible = false;
                    Invalidate();
                }
                if (MinSelections.Bounds.Contains(args.Location) && !MinSelections.Visible)
                {
                    MinSelections.Visible = true;
                    Invalidate();
                }
                else if (!MinSelections.Bounds.Contains(args.Location) && MinSelections.Visible)
                {
                    MinSelections.Visible = false;
                    Invalidate();
                }
            };
            MinSelections.ValueChanged += (sender, args) =>
            {
                MinSelections.Width = TextRenderer.MeasureText(Convert.ToString(MinSelections.Value), DisplayFont).Width + arrowThingGuessWidth;
            };
            MinSelections.ValueChanged += (sender, args) =>
            {
                MaxSelections.Width = TextRenderer.MeasureText(Convert.ToString(MaxSelections.Value), DisplayFont).Width + arrowThingGuessWidth;
            };
            MinSelections.ValueChanged += new EventHandler(MinSelections_ValueChanged);
            MaxSelections.ValueChanged += new EventHandler(MaxSelections_ValueChanged);
            //   this.MouseMove += new MouseEventHandler(MultipleSelectionDisplay_MouseMove);
            Controls.Add(MinSelections);
            Controls.Add(MaxSelections);
            // MinSelections.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
            /// MaxSelections.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
        }

        void MaxSelections_ValueChanged(object sender, EventArgs e)
        {
            if (MaxSelections.Value < MinSelections.Value)
            {
                MessageBox.Show("The minimum number of selections that must be made cannot exceed the maximum number of allowable selections. The minimum number of allowable selections " +
                    "has been adjusted.");
                MinSelections.Value = MaxSelections.Value;
            }
            (SurveyItem.Response as CMultiBooleanResponse).MaxSelections = Convert.ToInt32(MaxSelections.Value);
        }

        void MinSelections_ValueChanged(object sender, EventArgs e)
        {
            if (MaxSelections.Value < MinSelections.Value)
            {
                MessageBox.Show("The minimum number of selections that must be made cannot exceed the maximum number of allowable selections. The maximum number of allowable selections " +
                    "has been adjusted.");
                MaxSelections.Value = MinSelections.Value;
            }
            (SurveyItem.Response as CMultiBooleanResponse).MinSelections = Convert.ToInt32(MinSelections.Value);
        }

        void MultipleSelectionDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            bool MinVisible = MinSelections.Visible;
            bool MaxVisible = MaxSelections.Visible;
            if ((new Rectangle(MinSelections.Location, MinSelections.Size)).Contains(e.Location))
                MinSelections.Visible = true;
            else if (!MinSelections.Focused)
                MinSelections.Visible = false;
            if ((new Rectangle(MaxSelections.Location, MaxSelections.Size)).Contains(e.Location))
                MaxSelections.Visible = true;
            else if (!MaxSelections.Focused)
                MaxSelections.Visible = false;
            if ((MinSelections.Visible != MinVisible) || (MaxSelections.Visible != MaxVisible))
                Invalidate();
        }

        protected override CResponse GetResponse()
        {
            CMultiBooleanResponse mbr = new CMultiBooleanResponse(ChoiceEdits.Count);
            for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
                mbr.SetLabel(ctr, ChoiceEdits[ctr].Text);
            mbr.MinSelections = (int)MinSelections.Value;
            mbr.MaxSelections = (int)MaxSelections.Value;

            return mbr;
        }

        public override void SetResponse(CResponse response)
        {
            CMultiBooleanResponse mbr = (CMultiBooleanResponse)response;
            SuspendLayout();
            ClearChoices();
            for (int ctr = 0; ctr < mbr.LabelList.Length; ctr++)
                CreateChoiceEdit(mbr.LabelList[ctr]);
            MinSelections.Maximum = mbr.NumValues;
            MaxSelections.Maximum = mbr.NumValues;
            MinSelections.Value = mbr.MinSelections;
            MaxSelections.Value = mbr.MaxSelections;
            ResumeLayout();
        }

        protected override string GetChoiceDefaultText(TextBox sender)
        {
            return Properties.Resources.sMultiSelectDefaultChoiceText;
        }

        protected override void AddChoiceButton_Click(object sender, EventArgs e)
        {
            (SurveyItem.Response as CMultiBooleanResponse).AddLabel(Properties.Resources.sMultiSelectDefaultChoiceText);
            CreateChoiceEdit(Properties.Resources.sMultiSelectDefaultChoiceText);
            MinSelections.Maximum++;
            MaxSelections.Maximum++;
            MaxSelections.Value++;
            LayoutControl();
        }

        protected override void UpdateChoiceText(int ndx, string text)
        {
            (SurveyItem.Response as CMultiBooleanResponse).SetLabel(ndx, text);
        }

        protected override void RemoveChoiceFromResponse(int ndx)
        {
            (SurveyItem.Response as CMultiBooleanResponse).RemoveLabel(ndx);
        }

        public override async void LayoutControl()
        {
            Action a = () =>
            {
                if (!Monitor.TryEnter(lockObj))
                    return;
                LayoutEvent.WaitOne();
                LayoutEvent.Reset();
                LayoutChoices();
                Monitor.Exit(lockObj);
                this.Invoke(new Action(() =>
                {
                    Size ControlSize = new Size(ClientSize.Width, ChoicesSize.Height);

                    int minSelectionsWidth = TextRenderer.MeasureText(Properties.Resources.sMinNumSelectionsLabel, DisplayFont).Width;
                    int maxSelectionsWidth = TextRenderer.MeasureText(Properties.Resources.sMaxNumSelectionsLabel, DisplayFont).Width;
                    int lineWidth = minSelectionsWidth + maxSelectionsWidth + MinSelections.Width + MaxSelections.Width + InteriorPadding.Horizontal + (SelectionSpacing << 1);
                    if (lineWidth < this.Width)
                    {
                        ptMinSelectionsLabel = new Point(0, ControlSize.Height + SelectionLimitsPadding.Top);
                        Point minSelectionsLocation = new Point((SelectionSpacing >> 2) + ptMinSelectionsLabel.X + minSelectionsWidth,
                            ptMinSelectionsLabel.Y);
                        ptMaxSelectionsLabel = new Point(minSelectionsLocation.X + MinSelections.Width + SelectionSpacing,
                            ControlSize.Height + SelectionLimitsPadding.Top);
                        var maxSelectionsLocation = new Point(ptMaxSelectionsLabel.X + maxSelectionsWidth + (SelectionSpacing / 4), ptMaxSelectionsLabel.Y);
                        int diff = (this.Width - lineWidth) >> 1;
                        ptMinSelectionsLabel.X += diff;
                        MinSelections.Location = minSelectionsLocation + new Size(diff, 0);
                        ptMaxSelectionsLabel.X += diff;
                        MaxSelections.Location = maxSelectionsLocation + new Size(diff, 0);
                    }

                    else
                    {
                        ptMinSelectionsLabel = new Point(InteriorPadding.Left, ControlSize.Height + SelectionLimitsPadding.Top);
                        MinSelections.Location = new Point(InteriorPadding.Left + ValueLabelMargin + minSelectionsWidth - MinSelections.Margin.Left,
                            ptMinSelectionsLabel.Y);
                        ptMaxSelectionsLabel = new Point(InteriorPadding.Left, ControlSize.Height
                            + MinSelections.Height + SelectionLimitsPadding.Vertical);
                        MaxSelections.Location = new Point(maxSelectionsWidth + ValueLabelMargin, ControlSize.Height + SelectionLimitsPadding.Vertical + MaxSelections.Height);
                        this.Size = new Size(ClientSize.Width, ControlSize.Height + InteriorPadding.Vertical + SelectionLimitsPadding.Vertical * 2 +
                            MinSelections.Height + MaxSelections.Height);
                    }
                    this.Size = new Size(ClientSize.Width, InteriorPadding.Bottom + Math.Max(MinSelections.Bottom, MaxSelections.Bottom));
                    LayoutEvent.Set();
                }));
                (Parent as QuestionDisplay).RecalcSize(false);
            };
            await Task.Run(a);
        }

        protected override void ChangeResponseFont()
        {
            MinSelections.Font = DisplayFont;
            MinSelections.Width = TextRenderer.MeasureText(Convert.ToString(MinSelections.Value), DisplayFont).Width + arrowThingGuessWidth;
            MaxSelections.Font = DisplayFont;
            MaxSelections.Width = TextRenderer.MeasureText(Convert.ToString(MaxSelections.Value), DisplayFont).Width + arrowThingGuessWidth;
            base.ChangeResponseFont();
        }

        protected override void ResponseDisplay_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Brush br = new SolidBrush(SurveyItem.Response.Format.Color);
            Brush controlBrush = new SolidBrush(Color.Gray);
            Size szMinLabel = TextRenderer.MeasureText(Properties.Resources.sMinNumSelectionsLabel, DisplayFont);
            Size szMaxLabel = TextRenderer.MeasureText(Properties.Resources.sMaxNumSelectionsLabel, DisplayFont);
            e.Graphics.DrawString(Properties.Resources.sMinNumSelectionsLabel, DisplayFont, controlBrush, ptMinSelectionsLabel);
            if (!MinSelections.Visible)
                e.Graphics.DrawString(MinSelections.Value.ToString(), DisplayFont, br, MinSelections.Location);
            e.Graphics.DrawString(Properties.Resources.sMaxNumSelectionsLabel, DisplayFont, controlBrush, ptMaxSelectionsLabel);
            if (!MaxSelections.Visible)
                e.Graphics.DrawString(MaxSelections.Value.ToString(), DisplayFont, br, MaxSelections.Location);
            controlBrush.Dispose();
            br.Dispose();
        }

        protected override void DeleteChoiceButton_Click(object sender, EventArgs e)
        {
            base.DeleteChoiceButton_Click(sender, e);
            if (ChoiceEdits.Count <= MinSelections.Maximum)
                MinSelections.Maximum = ChoiceEdits.Count;
            if (MinSelections.Value > ChoiceEdits.Count)
                MinSelections.Value = ChoiceEdits.Count;
            if (MinSelections.Value > MaxSelections.Minimum)
                MaxSelections.Minimum = MinSelections.Value;
            if (MaxSelections.Value > ChoiceEdits.Count)
                MaxSelections.Value = ChoiceEdits.Count;
            if (MaxSelections.Maximum > ChoiceEdits.Count)
                MaxSelections.Maximum = ChoiceEdits.Count;
        }
    }
}
