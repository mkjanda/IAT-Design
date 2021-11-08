/*using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class InertSpecifierControl : SpecifierPanel
    {
        private TextBox QuestionEdit;
        private CSurvey Survey;
        private CSurveyItem Question;
        private Label NoSpecifierLabel;

        public override List<CDynamicSpecifier> GetDefinedSpecifiers()
        {
            return new List<CDynamicSpecifier>();
        }

        public InertSpecifierControl(CSurvey survey, CSurveyItem question, int width)
        {
            Survey = survey;
            Question = question;


            QuestionEdit = new TextBox();
            QuestionEdit.Location = new Point(1, 1);
            Size szText = TextRenderer.MeasureText(question.Text, DisplayFont, new Size(width - QuestionEdit.Margin.Horizontal - 2, 0), 
                TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak);
            QuestionEdit.Size = szText + new Size(0, QuestionEdit.Margin.Vertical + 2);
            QuestionEdit.BorderStyle = BorderStyle.None;
            QuestionEdit.TextChanged += new EventHandler(QuestionEdit_TextChanged);
            QuestionEdit.MouseEnter += new EventHandler(QuestionEdit_MouseEnter);
            QuestionEdit.MouseLeave += new EventHandler(QuestionEdit_MouseLeave);
            QuestionEdit.GotFocus += new EventHandler(QuestionEdit_GotFocus);
            QuestionEdit.LostFocus += new EventHandler(QuestionEdit_LostFocus);
            QuestionEdit.Text = question.Text;
            QuestionEdit.ForeColor = System.Drawing.Color.Black;
            QuestionEdit.BackColor = System.Drawing.Color.White;
            QuestionEdit.Font = DisplayFont;
            Controls.Add(QuestionEdit);

            NoSpecifierLabel = new Label();
            NoSpecifierLabel.Font = DisplayFont;
            NoSpecifierLabel.ForeColor = System.Drawing.Color.DarkGray;
            NoSpecifierLabel.BackColor = System.Drawing.Color.White;
            NoSpecifierLabel.Text = "Cannot create IAT key specifier utilizing this question";
            szText = TextRenderer.MeasureText(NoSpecifierLabel.Text, DisplayFont, new Size((width << 1) / 3, 0),
                TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak);
            NoSpecifierLabel.Size = szText;
            NoSpecifierLabel.Location = new Point((width - szText.Width) >> 1, QuestionEdit.Bottom + 5);
            Controls.Add(NoSpecifierLabel);

            this.Size = new Size(width, NoSpecifierLabel.Bottom + 5);
        }

        private void QuestionEdit_TextChanged(object sender, EventArgs e)
        {
            Question.Text = QuestionEdit.Text;
            Size szText = TextRenderer.MeasureText(QuestionEdit.Text, DisplayFont, new Size(Width - QuestionEdit.Margin.Horizontal - 2, 0));
            if (szText.Height != QuestionEdit.Height + QuestionEdit.Margin.Vertical + 2)
            {
                SuspendLayout();
                QuestionEdit.Size = szText + new Size(0, QuestionEdit.Margin.Vertical + 2);
                if (NoSpecifierLabel != null)
                    NoSpecifierLabel.Location = new Point(NoSpecifierLabel.Left, QuestionEdit.Bottom + 5);
                ResumeLayout(true);
            }
        }

        private void QuestionEdit_MouseEnter(object sender, EventArgs e)
        {
            if (!QuestionEdit.Focused)
                QuestionEdit.BackColor = System.Drawing.Color.LightGray;
        }

        private void QuestionEdit_MouseLeave(object sender, EventArgs e)
        {
            if (!QuestionEdit.Focused)
                QuestionEdit.BackColor = System.Drawing.Color.White;
        }

        private void QuestionEdit_GotFocus(object sender, EventArgs e)
        {
            SuspendLayout();
            QuestionEdit.BorderStyle = BorderStyle.FixedSingle;
            QuestionEdit.ForeColor = System.Drawing.Color.Black;
            QuestionEdit.BackColor = System.Drawing.Color.LightBlue;
            QuestionEdit.Location -= new Size(1, 1);
            ResumeLayout(true);
        }

        private void QuestionEdit_LostFocus(object sender, EventArgs e)
        {
            SuspendLayout();
            QuestionEdit.BorderStyle = BorderStyle.None;
            QuestionEdit.ForeColor = System.Drawing.Color.Black;
            QuestionEdit.BackColor = System.Drawing.Color.White;
            QuestionEdit.Location += new Size(1, 1);
            ResumeLayout(true);
        }


    }
}
*/