using System;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    class MultiChoiceDisplay : ChoiceResponseDisplay
    {
        private static Padding ChoiceEditPadding = new Padding(45, 2, 23, 3);

        protected override Padding GetChoiceEditPadding()
        {
            return ChoiceEditPadding;
        }
        protected override CResponse GetResponse()
        {
            CMultipleResponse r = new CMultipleResponse(ChoiceEdits.Count);
            for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
                r.SetChoice(ctr, ChoiceEdits[ctr].Text);
            return r;
        }

        public override void SetResponse(CResponse response)
        {
            CMultipleResponse mr = response as CMultipleResponse;
            SuspendLayout();
            ClearChoices();
            for (int ctr = 0; ctr < mr.Choices.Length; ctr++)
                CreateChoiceEdit(mr.Choices[ctr]);
            ResumeLayout(false);
        }

        protected override string GetChoiceDefaultText(TextBox sender)
        {
            return Properties.Resources.sMultiChoiceDefaultChoiceText;
        }

        public MultiChoiceDisplay()
            : base(true)
        {
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
                this.BeginInvoke(new Action(() =>
                {
                    SuspendLayout();
                    Size ControlSize = this.Size;
                    int height = ChoicesSize.Height + InteriorPadding.Bottom + InteriorPadding.Top;
                    this.Size = new Size(this.Width, height);
                    ResumeLayout();
                    Invalidate();
                }));
                LayoutEvent.Set();
                (Parent as QuestionDisplay).RecalcSize(false);
            };
            await Task.Run(a);
        }

        protected override void AddChoiceButton_Click(object sender, EventArgs e)
        {
            (SurveyItem.Response as CMultipleResponse).ChoiceList.Add(Properties.Resources.sMultiChoiceDefaultChoiceText);
            CreateChoiceEdit(Properties.Resources.sMultiChoiceDefaultChoiceText);
            LayoutControl();
        }

        protected override void UpdateChoiceText(int ndx, string text)
        {
            (SurveyItem.Response as CMultipleResponse).ChoiceList[ndx] = text;
        }

        protected override void RemoveChoiceFromResponse(int ndx)
        {
            (SurveyItem.Response as CMultipleResponse).ChoiceList.RemoveAt(ndx);
        }

        protected override void ResponseDisplay_Paint(object sender, PaintEventArgs e)
        {
            for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
            {
                SizeF szText = e.Graphics.MeasureString(String.Format("{0})", ctr + 1), DisplayFont);
                PointF ptText = new PointF(ChoiceEdits[ctr].Location.X - szText.Width - (float)Math.Sqrt(DisplayFont.Size), ChoiceEdits[ctr].Location.Y + ((ChoiceEdits[ctr].Height - szText.Height) / 2));
                e.Graphics.DrawString(String.Format("{0})", ctr + 1), DisplayFont, Brushes.Gray, ptText);
            }
        }

    }
}
