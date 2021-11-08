using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class WeightedMultiChoiceDisplay : ChoiceResponseDisplay
    {
        private static Padding ChoiceEditPadding = new Padding(43, 2, 33, 3);
        private List<NumericUpDown> Weights;
        private List<Rectangle> WeightRects;

        protected override Padding GetChoiceEditPadding()
        {
            return ChoiceEditPadding;
        }

        protected override CResponse GetResponse()
        {
            CWeightedMultipleResponse wmr = new CWeightedMultipleResponse(ChoiceEdits.Count);
            for (int ctr = 0; ctr < ChoiceEdits.Count; ctr++)
                wmr.SetChoice(ctr, ChoiceEdits[ctr].Text, (int)Weights[ctr].Value);

            return wmr;
        }

        public override void SetResponse(CResponse response)
        {
            CWeightedMultipleResponse wmr = (CWeightedMultipleResponse)response;
            SuspendLayout();
            UpdatingFromCode = true;
            ClearChoices();
            ClearWeights();
            for (int ctr = 0; ctr < wmr.Choices.Length; ctr++)
            {
                CreateChoiceEdit(wmr.Choices[ctr]);
                CreateWeightEdit(wmr.Weights[ctr]);
            }
            UpdatingFromCode = false;
            ResumeLayout();
            LayoutControl();
        }

        protected override string GetChoiceDefaultText(TextBox sender)
        {
            return Properties.Resources.sMultiChoiceDefaultChoiceText;
        }

        public WeightedMultiChoiceDisplay()
            : base(true)
        {
            Weights = new List<NumericUpDown>();
            WeightRects = new List<Rectangle>();
            this.MouseMove += new MouseEventHandler(WeightedMultiChoiceDisplay_MouseMove);
        }

        void WeightedMultiChoiceDisplay_MouseMove(object sender, MouseEventArgs e)
        {
            for (int ctr = 0; ctr < Weights.Count; ctr++)
            {
                bool Visible = Weights[ctr].Visible; 
                if (Weights[ctr].Bounds.Contains(e.Location))
                    Weights[ctr].Visible = true;
                else if (!Weights[ctr].Focused)
                    Weights[ctr].Visible = false;
            }
            Invalidate();
        }

        protected void ClearWeights()
        {
            for (int ctr = 0; ctr < Weights.Count; ctr++)
                Controls.Remove(Weights[ctr]);
            Weights.Clear();
        }

        protected void CreateWeightEdit(int value)
        {
            NumericUpDown weight = new NumericUpDown();
            weight.Location = new Point(Padding.Left, ChoiceEdits[ChoiceEdits.Count - 1].Location.Y);
            weight.Size = new Size(ChoiceEdits[ChoiceEdits.Count - 1].Location.X - (Padding.Left << 1), ChoiceEdits[ChoiceEdits.Count - 1].Height);
            weight.Maximum = 1000000000;
            weight.Minimum = -1000000000;
            weight.Value = value;
            weight.DecimalPlaces = 0;
            weight.TextAlign = HorizontalAlignment.Right;
            weight.BorderStyle = BorderStyle.FixedSingle;
            weight.Font = ControlFont;
            weight.ForeColor = System.Drawing.Color.Black;
            weight.BackColor = this.BackColor;
            weight.Hide();
            weight.Font = ControlFont;
            Controls.Add(weight);
            Weights.Add(weight);
            weight.MouseClick += new MouseEventHandler(ResponseDisplay_MouseClick);
            weight.ValueChanged += (sender, args) =>
            {
                (SurveyItem.Response as CWeightedMultipleResponse).SetWeight(Weights.IndexOf(sender as NumericUpDown), (int)weight.Value);
            };
            weight.LostFocus += (sender, args) =>
            {
                weight.Visible = false;
                Invalidate();
            };
            WeightRects.Add(new Rectangle(weight.Location, weight.Size));
        }

        protected override void LayoutControl()
        {
            UpdatingFromCode = true;
            SuspendLayout();
            Size ControlSize = this.Size;
            ControlSize.Height = InteriorPadding.Top;
            ControlSize = LayoutChoices();
            ResumeLayout(false);
            int maxWeightWidth = TextRenderer.MeasureText("00", ControlFont).Width;
            for (int ctr = 0; ctr < Weights.Count; ctr++)
            {
                String str = Weights[ctr].Value.ToString();
                int width = TextRenderer.MeasureText(str, ControlFont).Width;
                if (width > maxWeightWidth)
                    maxWeightWidth = width;
            }
            for (int ctr = 0; ctr < Weights.Count; ctr++)
            {
                Weights[ctr].Font = ControlFont;
                Weights[ctr].Location = new Point(Padding.Left, ChoiceEdits[ctr].Location.Y);
                Weights[ctr].Size = new Size(maxWeightWidth + 25, ChoiceEdits[ctr].Height);
                if (Weights[ctr].Right > ChoiceEdits[ctr].Left - Padding.Left)
                    ChoiceEdits[ctr].Location = new Point(Weights[ctr].Right + Padding.Left, ChoiceEdits[ctr].Top);
            }
            SuspendLayout();
            ControlSize.Height += InteriorPadding.Bottom;
            this.Size = ControlSize;
            ((QuestionDisplay)Parent).RecalcSize();
            ResumeLayout(true);
            UpdatingFromCode = false;
            Invalidate();
        }

        protected override void AddChoiceButton_Click(object sender, EventArgs e)
        {
            (SurveyItem.Response as CWeightedMultipleResponse).AddChoice(Properties.Resources.sMultiChoiceDefaultChoiceText, 0);
            CreateChoiceEdit(Properties.Resources.sMultiChoiceDefaultChoiceText);
            CreateWeightEdit(0);
            LayoutControl();
        }

        protected override void UpdateChoiceText(int ndx, string text)
        {
            (SurveyItem.Response as CWeightedMultipleResponse).SetChoice(ndx, text);
        }

        protected override void ChangeResponseFont()
        {
            foreach (var weight in Weights)
                weight.Font = ControlFont;
            base.ChangeResponseFont();
        }

        protected override void RemoveChoiceFromResponse(int ndx)
        {
            (SurveyItem.Response as CWeightedMultipleResponse).RemoveChoice(ndx);
        }

        protected override void ResponseDisplay_Paint(object sender, PaintEventArgs e)
        {
            Brush br = new SolidBrush(System.Drawing.Color.Gray);
            for (int ctr = 0; ctr < Weights.Count; ctr++)
            {
                if (!Weights[ctr].Visible)
                {
                    String str = Weights[ctr].Value.ToString();
                    e.Graphics.DrawString(str, ControlFont, br, 
                        new Point(Weights[ctr].Location.X + Weights[ctr].Size.Width - TextRenderer.MeasureText(str, ControlFont).Width, Weights[ctr].Location.Y));
                }
            }
            br.Dispose();
        }

        protected override void ResponseDisplay_Load(object sender, EventArgs e)
        {
            base.ResponseDisplay_Load(sender, e);
            UpdatingFromCode = true;
            LayoutControl();
            UpdatingFromCode = false;
        }

        protected override void DeleteChoiceButton_Click(object sender, EventArgs e)
        {
            int ndx = ChoiceDeleteButtons.IndexOf((Button)sender);
            NumericUpDown weight = Weights[ndx];
            Controls.Remove(weight);
            Weights.RemoveAt(ndx);
            WeightRects.RemoveAt(ndx);
            base.DeleteChoiceButton_Click(sender, e);
        }
    }
}
