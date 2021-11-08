/*using System;
using System.Collections.Generic;

using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class TrueFalseSpecifierControl : SpecifierPanel
    {
        private List<CIATItem> IATItems = new List<CIATItem>();
        private CTrueFalseSpecifier _Specifier;
        private TextBox TrueStatement, FalseStatement, QuestionEdit;
        private Panel StimulusPane;
        private GroupBox StimulusGroup;
        private CSurvey Survey;
        private CSurveyItem Question;
        private Padding StimulusPadding = new Padding(10);
        private Padding StatementPadding = new Padding(20, 5, 5, 5);
        private Size StimulusSize = new Size(112, 112);

        public override List<CDynamicSpecifier> GetDefinedSpecifiers()
        {
            List<CDynamicSpecifier> list = new List<CDynamicSpecifier>();
            list.Add(_Specifier);
            return list;
        }

        public CTrueFalseSpecifier Specifier
        {
            get
            {
                return _Specifier;
            }
            set
            {
                CDynamicSpecifier.DeleteSpecifier(_Specifier.ID);
                _Specifier = value;
                IATItems.Clear();
                IATItems.AddRange(_Specifier.IATItems);
                IATItems[0].StimulusImage.CreateThumbnail();
                IATItems[0].ThumbnailPreviewPanel = StimulusPane;
            }
        }


        public TrueFalseSpecifierControl(CSurvey survey, CSurveyItem question, int width)
        {
            Survey = survey;
            Question = question;
            this.Width = width;
            this.DragOver += new DragEventHandler(TrueFalseSpecifierControl_DragOver);
            this.DragDrop += new DragEventHandler(TrueFalseSpecifierControl_DragDrop);
            QuestionEdit = new TextBox();
            QuestionEdit.Font = DisplayFont;
            QuestionEdit.BorderStyle = BorderStyle.None;
            QuestionEdit.ForeColor = System.Drawing.Color.Black;
            QuestionEdit.BackColor = System.Drawing.Color.White;
            QuestionEdit.Location = new Point(1, 1);
            QuestionEdit.Size = TextRenderer.MeasureText(Question.Text, DisplayFont, new Size(width - 2 - QuestionEdit.Margin.Horizontal, 0),
                TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.Left) + new Size(QuestionEdit.Margin.Horizontal + 2, QuestionEdit.Margin.Vertical + 2);
            QuestionEdit.Text = Question.Text;
            QuestionEdit.AllowDrop = true;
            QuestionEdit.DragOver += new DragEventHandler(TrueFalseSpecifierControl_DragOver);
            QuestionEdit.DragDrop += new DragEventHandler(TrueFalseSpecifierControl_DragDrop);
            Controls.Add(QuestionEdit);

            TrueStatement = new TextBox();
            TrueStatement.Font = DisplayFont;
            TrueStatement.ForeColor = System.Drawing.Color.Black;
            TrueStatement.BorderStyle = BorderStyle.None;
            TrueStatement.Location = new Point(StatementPadding.Left, QuestionEdit.Bottom + StatementPadding.Vertical);
            TrueStatement.Size = TextRenderer.MeasureText(((CBoolResponse)Question.Response).TrueStatement, DisplayFont, new Size(width - 2 - TrueStatement.Margin.Horizontal - StimulusSize.Width - 6 
                - StatementPadding.Horizontal, 0), TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.Left) + new Size(TrueStatement.Margin.Horizontal + 2, TrueStatement.Margin.Vertical + 2);
            TrueStatement.AllowDrop = true;
            TrueStatement.DragOver += new DragEventHandler(TrueFalseSpecifierControl_DragOver);
            TrueStatement.DragDrop += new DragEventHandler(TrueFalseSpecifierControl_DragDrop);
            TrueStatement.BackColor = System.Drawing.Color.White;
            TrueStatement.Text = ((CBoolResponse)Question.Response).TrueStatement;
            Controls.Add(TrueStatement);

            FalseStatement = new TextBox();
            FalseStatement.Font = DisplayFont;
            FalseStatement.ForeColor = System.Drawing.Color.Black;
            FalseStatement.BorderStyle = BorderStyle.None;
            FalseStatement.BackColor = System.Drawing.Color.White;
            FalseStatement.Location = new Point(StatementPadding.Left, TrueStatement.Bottom + StatementPadding.Vertical);
            FalseStatement.AllowDrop = true;
            FalseStatement.DragOver += new DragEventHandler(TrueFalseSpecifierControl_DragOver);
            FalseStatement.DragDrop += new DragEventHandler(TrueFalseSpecifierControl_DragDrop);
            FalseStatement.BackColor = System.Drawing.Color.White;
            FalseStatement.Size = TextRenderer.MeasureText(((CBoolResponse)Question.Response).FalseStatement, DisplayFont, new Size(width - 2 - FalseStatement.Margin.Horizontal - StimulusSize.Width - 6
                - StatementPadding.Horizontal, 0), TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.Left) + new Size(TrueStatement.Margin.Horizontal + 2, FalseStatement.Margin.Vertical + 2);
            FalseStatement.Text = ((CBoolResponse)Question.Response).FalseStatement;
            Controls.Add(FalseStatement);

            StimulusPane = new Panel();
            StimulusPane.BackColor = System.Drawing.Color.White;
            StimulusPane.Location = new Point(3, 16);
            StimulusPane.Size = StimulusSize;
            StimulusPane.BackgroundImage = null;
            StimulusPane.AllowDrop = true;
            StimulusPane.DragEnter += new DragEventHandler(StimulusPane_DragEnter);
            StimulusPane.MouseEnter += new EventHandler(StimulusPane_MouseEnter);

            StimulusGroup = new GroupBox();
            StimulusGroup.Text = "Stimulus";
            StimulusGroup.Location = new Point(width - StimulusSize.Width - 6, QuestionEdit.Bottom + StimulusPadding.Top);
            StimulusGroup.BackColor = System.Drawing.Color.White;
            StimulusGroup.Size = StimulusSize + new Size(6, 19);
            StimulusGroup.AllowDrop = true;
            StimulusGroup.DragOver += new DragEventHandler(TrueFalseSpecifierControl_DragOver);
            StimulusGroup.DragDrop += new DragEventHandler(TrueFalseSpecifierControl_DragDrop);
            StimulusGroup.Controls.Add(StimulusPane);
            Controls.Add(StimulusGroup);

            this.Height = FalseStatement.Bottom + StatementPadding.Bottom;
            if (StimulusGroup.Bottom + StimulusPadding.Bottom > FalseStatement.Bottom + StatementPadding.Bottom)
                this.Height = StimulusGroup.Bottom + StimulusPadding.Bottom;

            _Specifier = new CTrueFalseSpecifier(Survey.Name, Survey.GetItemNum(Question), IATItems);
        }

        private void TrueFalseSpecifierControl_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
        }

        private void TrueFalseSpecifierControl_DragDrop(object sender, DragEventArgs e)
        {
        }

        private void StimulusPane_DragEnter(object sender, DragEventArgs e)
        {
            StimulusGroupDisplay display = new StimulusGroupDisplay();
            display.Stimuli = IATItems;
            display.OnClosed += new StimulusGroupDisplay.OnClosedHandler(StimulusDisplay_Closed);
            display.RetrieveIATItem = new DynamicIATPanel.RetrieveIATItemWithNdx(IATItemRetriever);
            display.CaptionText = "Select the keyed direction for the stimuli given a true response";
            display.Show();
            display.SetDesktopLocation(PointToScreen(StimulusGroup.Location).X, PointToScreen(StimulusGroup.Location).Y);
        }

        private void StimulusPane_MouseEnter(object sender, EventArgs e)
        {
            StimulusGroupDisplay display = new StimulusGroupDisplay();
            display.Stimuli = IATItems;
            display.OnClosed += new StimulusGroupDisplay.OnClosedHandler(StimulusDisplay_Closed);
            display.RetrieveIATItem = new DynamicIATPanel.RetrieveIATItemWithNdx(IATItemRetriever);
            display.CaptionText = "Select the keyed direction for the stimuli given a true response";
            display.Show();
            display.SetDesktopLocation(PointToScreen(StimulusGroup.Location).X, PointToScreen(StimulusGroup.Location).Y);
        }

        private void StimulusDisplay_Closed(List<CIATItem> stimuli)
        {
            _Specifier.ClearIATItems();
            foreach (CIATItem i in stimuli)
                _Specifier.AddIATItem(i, String.Empty);
            if (StimulusPane.BackgroundImage != null)
                StimulusPane.BackgroundImage.Dispose();
            if (stimuli.Count > 0)
            {
                stimuli[stimuli.Count - 1].StimulusImage.CreateThumbnail();
                stimuli[stimuli.Count - 1].ThumbnailPreviewPanel = StimulusPane;
            }
            else
            {
                Image img = StimulusPane.BackgroundImage;
                StimulusPane.BackgroundImage = null;
                if (img != null)
                    img.Dispose();
            }
        }
    }
}
*/