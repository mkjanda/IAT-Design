using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class LikertDisplay : ChoiceResponseDisplay
    {
        private static Padding ChoiceEditPadding = new Padding(5, 2, 23, 3);
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
            SuspendLayout();
            CLikertResponse lr = (CLikertResponse)r;
            ClearChoices();
            for (int ctr = 0; ctr < lr.ChoiceDescriptions.Count; ctr++)
                CreateChoiceEdit(lr.ChoiceDescriptions[ctr]);
            ReverseScoredCheck.Checked = lr.ReverseScored;
            LayoutControl();
            Invalidate();
            ResumeLayout();
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
            ReverseScoredCheck.Font = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, ReverseScoredCheck.Font.Height);
            ReverseScoredCheck.Font = DisplayFont;
            ReverseScoredCheck.Location = new Point(Padding.Left, AddChoiceButton.Location.Y);
            ReverseScoredCheck.BackColor = Color.Transparent;
            ReverseScoredCheck.ForeColor = System.Drawing.Color.Black;
            ReverseScoredCheck.Text = "Reverse Scored";
            ReverseScoredCheck.TextAlign = ContentAlignment.MiddleLeft;
            Controls.Add(ReverseScoredCheck);
            ReverseScoredCheck.CheckedChanged += new EventHandler(ReverseScoredCheck_CheckedChanged);
            ReverseScoredCheck.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
            HandleCreated += (sender, args) =>
            {
                if (ChoiceEdits.Count == 0)
                    for (int ctr = 0; ctr < CLikertResponse.DefaultResponses.Length; ctr++)
                        CreateChoiceEdit(CLikertResponse.DefaultResponses[ctr]);
                LayoutControl();
            };
        }

        protected override void ChangeResponseFont()
        {
            ReverseScoredCheck.Font = ControlFont;
            base.ChangeResponseFont();
        }

        protected override void LayoutControl()
        {
            if (UpdatingFromCode)
                return;
            UpdatingFromCode = true;
            Size ControlSize = LayoutChoices();
            if (ControlSize == Size.Empty)
            {
                UpdatingFromCode = false;
                return;
            }
            SuspendLayout();
            ChoiceEditPadding.Left = 10 + ((ChoiceEdits.Count < 10) ? (int)(TextRenderer.MeasureText("W)", DisplayFont).Height * 8F / 9F) : (int)(TextRenderer.MeasureText("W)", DisplayFont).Width * 16F / 9F));
            Point ptRevScored = new Point(this.Width - Padding.Right - ReverseScoredCheck.Width, AddChoiceButton.Bottom + ChoiceEditPadding.Top);
            ReverseScoredCheck.Size = TextRenderer.MeasureText("Reverse Scored", ControlFont) + new Size(20, 6);
            ReverseScoredCheck.Location = ptRevScored;
            ControlSize.Height += ReverseScoredCheck.Height;
            this.Size = ControlSize;
            Invalidate();
            ResumeLayout(false);
            (Parent as QuestionDisplay).RecalcSize();
            UpdatingFromCode = false;
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
            if (!ReverseScored)
                for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
                    e.Graphics.DrawString(String.Format("{0})", ctr + 1), ControlFont, Brushes.Gray, ChoiceEdits[ctr].Location.X - (int)(TextRenderer.MeasureText("W)", ControlFont).Height), ChoiceEdits[ctr].Location.Y);
            else
                for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
                    e.Graphics.DrawString(String.Format("{0})", ChoiceEdits.Count - ctr), ControlFont, Brushes.Gray, ChoiceEdits[ctr].Location.X - (int)(TextRenderer.MeasureText("W)", ControlFont).Height), ChoiceEdits[ctr].Location.Y);
        }
    }
}
