using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class LikertDisplay : ChoiceResponseDisplay
    {
        private static Padding ChoiceEditPadding = new Padding(45, 2, 23, 3);
        private CheckBox ReverseScoredCheck = null;

        protected override Padding GetChoiceEditPadding()
        {
            return ChoiceEditPadding;
        }

        protected override CResponse GetResponse()
        {
            CLikertResponse response = new CLikertResponse(ChoiceEdits.Count, ReverseScored);
            for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
                response.SetChoiceDesc(ctr, ChoiceEdits[ctr].Text);
            return response;
        }

        public override void SetResponse(CResponse r)
        {
            CLikertResponse lr = (CLikertResponse)r;
            ClearChoices();
            for (int ctr = 0; ctr < lr.ChoiceDescriptions.Count; ctr++)
                CreateChoiceEdit(lr.ChoiceDescriptions[ctr]);
            ReverseScoredCheck.Checked = lr.ReverseScored;
        }

        protected override string GetChoiceDefaultText(TextBox sender)
        {
            return Properties.Resources.sLikertDefaultChoiceText;
        }

        public bool ReverseScored
        {
            get
            {
                return ReverseScoredCheck.Checked;
            }
            set
            {
                ReverseScoredCheck.Checked = value;
            }
        }

        public LikertDisplay()
            : base(true)
        {
            ReverseScoredCheck = new CheckBox();
            ReverseScoredCheck.Location = new Point(Padding.Left, AddChoiceButton.Location.Y);
            ReverseScoredCheck.BackColor = Color.Transparent;
            ReverseScoredCheck.ForeColor = System.Drawing.Color.Black;
            ReverseScoredCheck.Text = "Reverse Scored";
            ReverseScoredCheck.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(ReverseScoredCheck);
            ReverseScoredCheck.CheckedChanged += new EventHandler(ReverseScoredCheck_CheckedChanged);
            ReverseScoredCheck.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
        }

        protected override void ChangeResponseFont()
        {
            ReverseScoredCheck.Font = DisplayFont;
            base.ChangeResponseFont();
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
                LayoutChoices();
                this.Invoke(new Action(() =>
                {
                    ReverseScoredCheck.Size = TextRenderer.MeasureText("Reverse Scored", DisplayFont) + new Size(20, 0);
                    ReverseScoredCheck.Location = new Point(this.Width - Padding.Right - ReverseScoredCheck.Width, AddChoiceButton.Bottom + ChoiceEditPadding.Top);
                    this.Size = new Size(ClientSize.Width, ChoicesSize.Height + ChoiceEditPadding.Vertical + ReverseScoredCheck.Height + InteriorPadding.Vertical);
                    Invalidate();
                }));
                LayoutEvent.Set();
                (Parent as QuestionDisplay).RecalcSize(false);
            };
            await Task.Run(a);
        }

        void ReverseScoredCheck_CheckedChanged(object sender, EventArgs e)
        {
            Invalidate();
        }

        protected override void AddChoiceButton_Click(object sender, EventArgs e)
        {
            (SurveyItem.Response as CLikertResponse).ChoiceDescriptions.Add(Properties.Resources.sLikertDefaultChoiceText);
            CreateChoiceEdit(Properties.Resources.sLikertDefaultChoiceText);
            LayoutControl();
        }

        protected override void UpdateChoiceText(int ndx, string text)
        {
            (SurveyItem.Response as CLikertResponse).ChoiceDescriptions[ndx] = text;
        }

        protected override void RemoveChoiceFromResponse(int ndx)
        {
            (SurveyItem.Response as CLikertResponse).ChoiceDescriptions.RemoveAt(ndx);
        }

        protected override void ResponseDisplay_Paint(object sender, PaintEventArgs e)
        {
            int lineHeight = (int)((double)DisplayFont.Size * (double)(DisplayFont.FontFamily.GetCellAscent(FontStyle.Regular) + (double)DisplayFont.FontFamily.GetCellDescent(FontStyle.Regular)) / (double)DisplayFont.FontFamily.GetEmHeight(FontStyle.Regular));
            if (!ReverseScored)
                for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
                {
                    SizeF szText = e.Graphics.MeasureString(String.Format("{0})", ctr + 1), DisplayFont);
                    PointF ptText = new PointF(ChoiceEdits[ctr].Location.X - szText.Width - (float)Math.Sqrt(DisplayFont.Size), ChoiceEdits[ctr].Location.Y + (ChoiceEdits[ctr].Height - szText.Height));
                    e.Graphics.DrawString(String.Format("{0})", ctr + 1), DisplayFont, Brushes.Gray, ptText);
                }
            else
                for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++) {
                    SizeF szText = e.Graphics.MeasureString(String.Format("{0})", ChoiceEdits.Count - ctr), DisplayFont);
                    PointF ptText = new PointF(ChoiceEdits[ctr].Location.X - szText.Width - (float)Math.Sqrt(DisplayFont.Size), ChoiceEdits[ctr].Location.Y + ((ChoiceEdits[ctr].Height - szText.Height) / 2));
                    e.Graphics.DrawString(String.Format("{0})", ChoiceEdits.Count - ctr), DisplayFont, Brushes.Gray, ptText);
                }
        }
    }
}
