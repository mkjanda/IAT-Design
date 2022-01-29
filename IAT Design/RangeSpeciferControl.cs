/*using System;
using System.Collections.Generic;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    class RangeSpecifierControl : SpecifierPanel
    {
        private TrackBar RangeBar;
        private const int RangeBarWidth = 20;
        private CheckBox ExcludeCutoff;
        private CSurvey Survey;
        private CSurveyItem Question;
        private List<TextBox> StatementEdits = new List<TextBox>();
        private TextBox QuestionEdit;
        private GroupBox StimulusGroup;
        private Panel StimulusPane;
        private Padding StimulusPadding = new Padding(10);
        private static Size StimulusSize = new Size(112, 112);
        private List<CIATItem> IATItems = new List<CIATItem>();
        private CRangeSpecifier _Specifier = new CRangeSpecifier();

        public override List<DynamicSpecifier> GetDefinedSpecifiers()
        {
            List<DynamicSpecifier> specifiers = new List<DynamicSpecifier>();
            specifiers.Add(Specifier);
            return specifiers;
        }

        public CRangeSpecifier Specifier
        {
            get
            {
                return _Specifier;
            }
            set
            {
                DynamicSpecifier.DeleteSpecifier(_Specifier.ID);
                _Specifier = value;
                _Specifier.IsReverseScored = ((CLikertResponse)Question.Response).ReverseScored;
                IATItems.Clear();
                IATItems.AddRange(value.IATItems);
                if ((value.SurveyName != Survey.Name) || (value.ItemNum != Survey.GetItemNum(Question)))
                    throw new Exception("Attempt to modify the survey or question in an existing range specifier control.");
                RangeBar.Value = value.Cutoff;
            }
        }

        public RangeSpecifierControl(CSurvey survey, CSurveyItem question, int width)
        {
            Survey = survey;
            Question = question;

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
            QuestionEdit.DragOver += new DragEventHandler(RangeSpecifierControl_DragOver);
            QuestionEdit.DragDrop += new DragEventHandler(RangeSpecifierControl_DragDrop);
            Controls.Add(QuestionEdit);

            StimulusGroup = new GroupBox();
            StimulusGroup.Size = StimulusSize + new Size(6, 19);
            StimulusGroup.Location = new Point(width - StimulusGroup.Width - StimulusPadding.Right, QuestionEdit.Bottom + StimulusPadding.Top);
            StimulusGroup.AllowDrop = true;
            StimulusGroup.DragOver += new DragEventHandler(StimulusGroup_DragOver);
            StimulusGroup.Text = "Stimulus";
            Controls.Add(StimulusGroup);

            StimulusPane = new Panel();
            StimulusPane.Location = new Point(3, 15);
            StimulusPane.Size = StimulusSize;
            StimulusPane.BackColor = System.Drawing.Color.White;
            StimulusPane.AllowDrop = true;
            StimulusPane.MouseEnter += new EventHandler(StimulusPane_MouseEnter);
            StimulusPane.DragEnter += new DragEventHandler(StimulusPane_DragEnter);
            StimulusGroup.Controls.Add(StimulusPane);
            Controls.Add(StimulusGroup);

            RangeBar = new TrackBar();

            CLikertResponse r = (CLikertResponse)question.Response;
            Point ptChoice = new Point(RangeBarWidth, QuestionEdit.Bottom + RangeBar.Margin.Top + RangeBar.Padding.Top);
            for (int ctr = 0; ctr < r.ChoiceDescriptions.Length; ctr++)
            {
                TextBox tb = new TextBox();
                tb.Location = ptChoice;
                tb.Size = TextRenderer.MeasureText(r.ChoiceDescriptions[ctr], DisplayFont, new Size(StimulusGroup.Left - StimulusPadding.Left
                    - RangeBarWidth - 2 - tb.Margin.Horizontal, 0), TextFormatFlags.NoPrefix | TextFormatFlags.Left | TextFormatFlags.WordBreak)
                    + new Size(tb.Margin.Horizontal + 2, tb.Margin.Vertical + 2);
                tb.Text = r.ChoiceDescriptions[ctr];
                tb.Font = DisplayFont;
                tb.ForeColor = System.Drawing.Color.Black;
                tb.BorderStyle = BorderStyle.None;
                tb.MouseEnter += new EventHandler(StatementEdit_MouseEnter);
                tb.MouseLeave += new EventHandler(StatementEdit_MouseLeave);
                tb.DragOver += new DragEventHandler(StatementEdit_DragOver);
                tb.AllowDrop = true;
                StatementEdits.Add(tb);
                Controls.Add(tb);
                ptChoice.Y += tb.Height;
            }
            this.Size = new Size(width, StatementEdits[StatementEdits.Count - 1].Bottom);

            RangeBar.Location = new Point(0, QuestionEdit.Bottom);
            RangeBar.Size = new Size(RangeBarWidth, this.Height - QuestionEdit.Bottom);
            RangeBar.Minimum = 1;
            RangeBar.Maximum = StatementEdits.Count;
            RangeBar.Orientation = Orientation.Vertical;
            RangeBar.Value = (StatementEdits.Count / 2) + ((StatementEdits.Count % 2 == 0) ? 0 : 1);
            RangeBar.ValueChanged += new EventHandler(RangeBar_ValueChanged);
            Controls.Add(RangeBar);

            ExcludeCutoff = new CheckBox();
            ExcludeCutoff.Size = TextRenderer.MeasureText("Exclude stimulus on cutoff", System.Drawing.SystemFonts.DialogFont) + new Size(20, 0);
            if (ExcludeCutoff.Height < 20)
                ExcludeCutoff.Height = 20;
            if (ExcludeCutoff.Width > StimulusGroup.Width)
                ExcludeCutoff.Size = new Size(StimulusGroup.Width, ExcludeCutoff.Height << 1);
            ExcludeCutoff.Location = new Point(StimulusGroup.Left, StimulusGroup.Bottom + StimulusPadding.Bottom);
            ExcludeCutoff.Text = "Exclude stimulus on cutoff";
            ExcludeCutoff.CheckedChanged += new EventHandler(ExcludeCutoff_CheckedChanged);
            Controls.Add(ExcludeCutoff);
            if (this.Height < ExcludeCutoff.Bottom + StimulusPadding.Bottom)
                this.Height = ExcludeCutoff.Bottom + StimulusPadding.Bottom;
            _Specifier.IsReverseScored = ((CLikertResponse)Question.Response).ReverseScored;
            _Specifier.SurveyName = Survey.Name;
            _Specifier.ItemNum = Survey.GetItemNum(Question);
        }

        void ExcludeCutoff_CheckedChanged(object sender, EventArgs e)
        {
            _Specifier.CutoffExcludes = ExcludeCutoff.Checked;
        }

        void RangeBar_ValueChanged(object sender, EventArgs e)
        {
            _Specifier.Cutoff = ((CLikertResponse)Question.Response).ReverseScored ? RangeBar.Value : (RangeBar.Maximum - 1 - RangeBar.Value);
        }

        private void StimulusPane_MouseEnter(object sender, EventArgs e)
        {
            StimulusGroupDisplay GroupDisplay = new StimulusGroupDisplay();
            GroupDisplay.Stimuli = IATItems;
            GroupDisplay.OnClosed += new StimulusGroupDisplay.OnClosedHandler(StimulusDisplay_Closed);
            GroupDisplay.RetrieveIATItem = new DynamicIATPanel.RetrieveIATItemWithNdx(IATItemRetriever);
            GroupDisplay.CaptionText = "Stimuli - select the keyed direction for stimuli preceding but not including the cutoff response";
            GroupDisplay.Show();
            GroupDisplay.SetDesktopLocation(PointToScreen(StimulusGroup.Location).X, PointToScreen(StimulusGroup.Location).Y);
        }

        private void StimulusPane_DragEnter(object sender, DragEventArgs e)
        {
            StimulusGroupDisplay GroupDisplay = new StimulusGroupDisplay();
            GroupDisplay.Stimuli = IATItems;
            GroupDisplay.RetrieveIATItem = new DynamicIATPanel.RetrieveIATItemWithNdx(IATItemRetriever);
            GroupDisplay.OnClosed += new StimulusGroupDisplay.OnClosedHandler(StimulusDisplay_Closed);
            GroupDisplay.CaptionText = "Stimuli - select the keyed direction for stimuli preceding but not including the cutoff response";
            GroupDisplay.Show();
            GroupDisplay.SetDesktopLocation(PointToScreen(StimulusGroup.Location).X, PointToScreen(StimulusGroup.Location).Y);
        }

        private void StimulusDisplay_Closed(List<CIATItem> Stimuli)
        {
            IATItems.Clear();
            IATItems.AddRange(Stimuli);
            _Specifier.ClearIATItems();
            foreach (CIATItem i in Stimuli)
                _Specifier.AddIATItem(i, String.Empty);
            if (StimulusPane.BackgroundImage != null)
                StimulusPane.BackgroundImage.Dispose();
            if (IATItems.Count > 0) {
                if (StimulusPane.BackgroundImage != null)
                    StimulusPane.BackgroundImage.Dispose();
                StimulusPane.BackgroundImage = IATItems[0].StimulusImage.Thumbnail.Image;
            }
            else
            {
                Image i = StimulusPane.BackgroundImage;
                StimulusPane.BackgroundImage = null;
                if (i != null)
                    i.Dispose();
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

        private void QuestionEdit_Enter(object sender, EventArgs e)
        {
            SuspendLayout();
            QuestionEdit.BackColor = System.Drawing.Color.LightBlue;
            QuestionEdit.ForeColor = System.Drawing.Color.Black;
            QuestionEdit.BorderStyle = BorderStyle.FixedSingle;
            QuestionEdit.Location -= new Size(1, 1);
            ResumeLayout(true);
        }

        private void QuestionEdit_Leave(object sender, EventArgs e)
        {
            SuspendLayout();
            QuestionEdit.BackColor = System.Drawing.Color.White;
            QuestionEdit.ForeColor = System.Drawing.Color.Black;
            QuestionEdit.BorderStyle = BorderStyle.None;
            QuestionEdit.Location += new Size(1, 1);
            ResumeLayout(true);
        }

        private void QuestionEdit_TextChanged(object sender, EventArgs e)
        {
            Size newSize = TextRenderer.MeasureText(QuestionEdit.Text, DisplayFont, new Size(QuestionEdit.Width - 2 - QuestionEdit.Margin.Horizontal, 0),
                TextFormatFlags.WordBreak | TextFormatFlags.NoPrefix | TextFormatFlags.Left);
            if (newSize.Height + 2 + QuestionEdit.Margin.Vertical != QuestionEdit.Height)
            {
                Size szOffset = new Size(0, newSize.Height + 2 + QuestionEdit.Margin.Vertical - QuestionEdit.Height);
                SuspendLayout();
                foreach (TextBox tb in StatementEdits)
                    tb.Location += szOffset;
                StimulusGroup.Location += szOffset;
                ExcludeCutoff.Location += szOffset;
                QuestionEdit.Height = szOffset.Height + 2 + QuestionEdit.Margin.Vertical;
                ResumeLayout(true);
            }
            Question.Text = QuestionEdit.Text;
        }

        private void RangeSpecifierControl_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void RangeSpecifierControl_DragDrop(object sender, DragEventArgs e)
        {
        }

        private void StimulusGroup_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }
        private ScrollingPreviewPanel.EOrientation StimulusPane_ParentOrientation()
        {
            return ScrollingPreviewPanel.EOrientation.horizontal;
        }

        private bool StimulusPane_IsDragging()
        {
            return Clipboard.ContainsData("IATItemNdx");
        }

        private void StatementEdit_MouseEnter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!tb.Focused)
                tb.BackColor = System.Drawing.Color.LightGray;
        }

        private void StatementEdit_MouseLeave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (!tb.Focused)
                tb.BackColor = System.Drawing.Color.White;
        }

        private void StatementEdit_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            SuspendLayout();
            tb.Location -= new Size(1, 1);
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.ForeColor = System.Drawing.Color.Black;
            tb.BackColor = System.Drawing.Color.LightBlue;
            ResumeLayout(true);
        }

        private void StatementEdit_Leave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            SuspendLayout();
            tb.Location += new Size(1, 1);
            tb.ForeColor = System.Drawing.Color.Black;
            tb.BackColor = System.Drawing.Color.White;
            tb.BorderStyle = BorderStyle.None;
            ResumeLayout(true);
        }

        private void StatementEdit_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void StatementEdit_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            int boxNdx = StatementEdits.IndexOf(tb);
            Size newSz = TextRenderer.MeasureText(tb.Text, DisplayFont, new Size(tb.Width - 2 - tb.Margin.Horizontal, 0),
                TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak);
            newSz.Height += 2 + tb.Margin.Vertical;
            if (newSz.Height != tb.Height)
            {
                SuspendLayout();
                for (int ctr = boxNdx; ctr < StatementEdits.Count; ctr++)
                    StatementEdits[ctr].Location += new Size(0, newSz.Height - tb.Height);
                tb.Size = newSz;
                ResumeLayout(true);
            }
        }

       
    }
}
*/