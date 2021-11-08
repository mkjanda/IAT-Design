using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    class MultiChoiceDisplay : ChoiceResponseDisplay
    {
        private static Padding ChoiceEditPadding = new Padding(23, 2, 23, 3);

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


        protected override void LayoutControl()
        {
            if (UpdatingFromCode)
                return;
            SuspendLayout();
            UpdatingFromCode = true;
            Size ControlSize = this.Size;
            ControlSize = LayoutChoices();
            ControlSize.Height += InteriorPadding.Bottom + InteriorPadding.Bottom;
            this.Size = ControlSize;
            ResumeLayout();
            UpdatingFromCode = false;
            Invalidate();
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
                SizeF szText = e.Graphics.MeasureString(String.Format("{0})", ctr + 1), ControlFont, ChoiceEdits[ctr].Location.X, StringFormat.GenericDefault);
                e.Graphics.DrawString(String.Format("{0})", ctr + 1), ControlFont, Brushes.Gray, new Point(ChoiceEdits[ctr].Location.X - (int)szText.Width - (int)Math.Sqrt(ControlFont.Size), ChoiceEdits[ctr].Location.Y));
            }
        }

    }
}
