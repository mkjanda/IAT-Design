using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;

namespace IATClient
{
    class InstructionDisplay : QuestionDisplay
    {
        private Font LabelFont;
        private Rectangle LabelRect;
        private String Label;
        private CSurveyItem _SurveyItem = null;
        private Rectangle _FormatRect;
        protected override Rectangle FormatRect { get { return _FormatRect; } }
        protected override String DefaultQuestionEditText
        {
            get
            {
                return Properties.Resources.sDefaultInstructionText;
            }
        }

        public override CSurveyItem SurveyItem
        {
            get
            {
                return _SurveyItem;
            }
            set
            {
                SuspendLayoutCalculations();
                _SurveyItem?.Dispose();
                _SurveyItem = value;
                if (value == null)
                    return;
                Format = value.Format;
                if (value.Text == String.Empty)
                    QuestionEdit.Text = DefaultQuestionEditText;
                else
                    QuestionEdit.Text = value.Text;
                QuestionEdit.ForeColor = value.Format.Color;
                SizeQuestionEdit(true);
                RecalcSize();
                ResumeLayoutCalculations();
            }
        }


        public InstructionDisplay() 
        {
            LabelFont = new Font(FontFamily.GenericSerif, 11.7F, FontStyle.Regular);
            Label = "Instructions";
            LabelRect = new Rectangle(new Point(QuestionEditMargin.Left << 2, 0), TextRenderer.MeasureText(Label, LabelFont));
            QuestionEdit.Text = Properties.Resources.sDefaultInstructionText;
            Size szFormat = TextRenderer.MeasureText("Format Text", System.Drawing.SystemFonts.DialogFont) + new Size(10, 4);
            _FormatRect = new Rectangle(this.Width, this.Top, szFormat.Width, szFormat.Height);
            Controls.Remove(CollapseButton);
            Controls.Remove(ExpandButton);
        }


        protected override void OnActivate(bool BecomingActive)
        {
            Invalidate();
        }

        public override void RecalcSize()
        {
            if (QuestionEdit == null)
                return;
            Action recalcAction = () =>
            {
                SizeQuestionEdit(false);
                this.Height = Padding.Vertical + QuestionEdit.Height + QuestionEditMargin.Vertical + ResponseEditMargin.Vertical;
                Size szFormat = TextRenderer.MeasureText("Format Text", System.Drawing.SystemFonts.DialogFont) + new Size(10, 4);
                _FormatRect = new Rectangle(this.Width - szFormat.Width * 3 / 2, Padding.Top, szFormat.Width, szFormat.Height);
                ((SurveyDisplay)Parent)?.RecalcSize();
                Invalidate();
            };
            if (Interlocked.CompareExchange(ref RecalculatingSize, 1, 0) == 0)
            {
                if (!IsHandleCreated)
                {
                    HandleCreated += (sender, args) =>
                    {
                        recalcAction();
                        RecalculatingSize = 0;
                    };
                }
                else
                {
                    Task.Run(() =>
                    {
                        this.Invoke(recalcAction);
                        if (Interlocked.Equals(RecalculatingSize, 2))
                            this.Invoke(recalcAction);
                        RecalculatingSize = 0;
                    });
                }
            }
            else
                Interlocked.CompareExchange(ref RecalculatingSize, 2, 1);
        }

        protected override void SurveyItemDisplay_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
        {
            Brush br;
            br = new SolidBrush(Color.Chartreuse);
            Brush backBr = new SolidBrush(this.BackColor);
            Pen pen = new Pen(br);
            e.Graphics.DrawRectangle(pen, new Rectangle(new Point(0, LabelRect.Top + (LabelRect.Height >> 1)),
                new Size(this.Size.Width - 1, this.Size.Height - Padding.Vertical - (LabelRect.Height >> 1) - 1)));
            e.Graphics.FillRectangle(backBr, LabelRect);
            if (bFormatHover)
                e.Graphics.FillRectangle(Brushes.CornflowerBlue, FormatRect);
            else
                e.Graphics.FillRectangle(Brushes.White, FormatRect);
            e.Graphics.DrawString("Format Text", System.Drawing.SystemFonts.DialogFont, Brushes.Black, new PointF(FormatRect.Left + 5, FormatRect.Top + 2));
            e.Graphics.DrawRectangle(pen, FormatRect);
            e.Graphics.DrawString(Label, LabelFont, br, LabelRect.Location);
            pen.Dispose();
            backBr.Dispose();
            br.Dispose();
        }
    }
}
