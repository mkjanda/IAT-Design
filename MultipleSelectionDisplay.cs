using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{

    class MultipleSelectionDisplay : ChoiceResponseDisplay
    {
        private static Padding ChoiceEditPadding = new Padding(23, 2, 23, 3);
        private static Padding SelectionLimitsPadding = new Padding(10, 10, 10, 10);
        private const int ValueLabelMargin = 5;
        private const int SelectionSpacing = 40;
        protected NumericUpDown MinSelections, MaxSelections;
        private const int ValueEntryWidth = 40;
        private Point ptMinSelectionsLabel = new Point(0, 0), ptMaxSelectionsLabel = new Point(0, 0);

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
            MinSelections.TextAlign = HorizontalAlignment.Right;
            MinSelections.ForeColor = System.Drawing.Color.Black;
            MinSelections.BackColor = Color.White;
            MinSelections.BorderStyle = BorderStyle.FixedSingle;
            MinSelections.Value = 0;
            MinSelections.DecimalPlaces = 0;
            MinSelections.Size = new Size(MinSelections.Padding.Horizontal + MinSelections.Margin.Vertical + ValueEntryWidth,
                MinSelections.Padding.Vertical + MinSelections.Margin.Vertical + ControlFont.Height);
            MinSelections.Visible = false;
            MaxSelections = new NumericUpDown();
            MaxSelections.Maximum = 0;
            MaxSelections.Maximum = 0;
            MaxSelections.TextAlign = HorizontalAlignment.Right;
            MaxSelections.ForeColor = System.Drawing.Color.Black;
            MaxSelections.BackColor = Color.White;
            MaxSelections.BorderStyle = BorderStyle.FixedSingle;
            MaxSelections.DecimalPlaces = 0;
            MaxSelections.Value = 0;
            MaxSelections.Size = new Size(MaxSelections.Padding.Horizontal + MaxSelections.Margin.Vertical + ValueEntryWidth,
                MaxSelections.Padding.Vertical + MaxSelections.Margin.Vertical + ControlFont.Height);
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
            MinSelections.ValueChanged += new EventHandler(MinSelections_ValueChanged);
            MaxSelections.ValueChanged += new EventHandler(MaxSelections_ValueChanged);
            this.MouseMove += new MouseEventHandler(MultipleSelectionDisplay_MouseMove);
            Controls.Add(MinSelections);
            Controls.Add(MaxSelections);
            MinSelections.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
            MaxSelections.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
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

        protected override void LayoutControl()
        {
            if (UpdatingFromCode)
                return;
            UpdatingFromCode = true;
            SuspendLayout();
            Size ControlSize = LayoutChoices();
        //    AddChoiceButton.Location += new Size(0, MinSelections.Height + SelectionLimitsPadding.Vertical);
            int minSelectionsWidth = TextRenderer.MeasureText(Properties.Resources.sMinNumSelectionsLabel, ControlFont).Width + ValueLabelMargin + MinSelections.Width;
            int maxSelectionsWidth = TextRenderer.MeasureText(Properties.Resources.sMaxNumSelectionsLabel, ControlFont).Width + ValueLabelMargin + MaxSelections.Width;
            if (minSelectionsWidth + maxSelectionsWidth + InteriorPadding.Horizontal + SelectionSpacing > ControlSize.Width)
            {
                ptMinSelectionsLabel = new Point(InteriorPadding.Left, ControlSize.Height + SelectionLimitsPadding.Top + MinSelections.Margin.Top);
                MinSelections.Location = new Point(TextRenderer.MeasureText(Properties.Resources.sMinNumSelectionsLabel, ControlFont).Width + ValueLabelMargin, 
                    ControlSize.Height + SelectionLimitsPadding.Top);
                ptMaxSelectionsLabel = new Point(InteriorPadding.Left, ControlSize.Height + MinSelections.Height + SelectionLimitsPadding.Vertical + MaxSelections.Margin.Top);
                MaxSelections.Location = new Point(TextRenderer.MeasureText(Properties.Resources.sMaxNumSelectionsLabel, ControlFont).Width + ValueLabelMargin,
                    ControlSize.Height + SelectionLimitsPadding.Vertical + MinSelections.Height);
            } else
            {
                ptMinSelectionsLabel = new Point((ControlSize.Width - minSelectionsWidth - maxSelectionsWidth - SelectionSpacing) >> 1, ControlSize.Height + SelectionLimitsPadding.Top + MinSelections.Margin.Top);
                MinSelections.Location = new Point(((ControlSize.Width - minSelectionsWidth - maxSelectionsWidth) >> 1) + 
                    TextRenderer.MeasureText(Properties.Resources.sMinNumSelectionsLabel, ControlFont).Width + ValueLabelMargin,
                    ControlSize.Height + SelectionLimitsPadding.Top);
                ptMaxSelectionsLabel = new Point(MinSelections.Right + SelectionSpacing, ControlSize.Height + SelectionLimitsPadding.Top + MinSelections.Margin.Top); 
                MaxSelections.Location = new Point(MinSelections.Right + SelectionSpacing + TextRenderer.MeasureText(Properties.Resources.sMaxNumSelectionsLabel, ControlFont).Width + 
                    ValueLabelMargin, ControlSize.Height + SelectionLimitsPadding.Top);
            }
            ControlSize.Height = MaxSelections.Bottom + SelectionLimitsPadding.Bottom + InteriorPadding.Bottom;
            this.Size = ControlSize;
            ResumeLayout();
            UpdatingFromCode = false;
        }

        protected override void ChangeResponseFont()
        {
            MinSelections.Font = ControlFont;
            MaxSelections.Font = ControlFont;
            base.ChangeResponseFont();
        }

        protected override void ResponseDisplay_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Brush br = new SolidBrush(SurveyItem.Response.Format.Color);
            Brush controlBrush = new SolidBrush(Color.Gray);
            Size szMinLabel = TextRenderer.MeasureText(Properties.Resources.sMinNumSelectionsLabel, ControlFont);
            Size szMaxLabel = TextRenderer.MeasureText(Properties.Resources.sMaxNumSelectionsLabel, ControlFont);
            e.Graphics.DrawString(Properties.Resources.sMinNumSelectionsLabel, ControlFont, controlBrush, ptMinSelectionsLabel);
            if (!MinSelections.Visible)
                e.Graphics.DrawString(MinSelections.Value.ToString(), ControlFont, br, MinSelections.Location + new Size(0, MinSelections.Margin.Top));
            e.Graphics.DrawString(Properties.Resources.sMaxNumSelectionsLabel, ControlFont, controlBrush, ptMaxSelectionsLabel);
            if (!MaxSelections.Visible)
                e.Graphics.DrawString(MaxSelections.Value.ToString(), ControlFont, br, MaxSelections.Location + new Size(0, MaxSelections.Margin.Top));
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
