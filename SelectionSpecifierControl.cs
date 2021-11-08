/*
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Linq;

namespace IATClient
{
    class SelectionSpecifierControl : SpecifierPanel
    {
        private List<TextBox> Statements = new List<TextBox>();
        private List<Panel> StimulusPanes = new List<Panel>();
        private TextBox QuestionEdit;
        private CSurvey Survey;
        private CSurveyItem Question;
        private List<GroupBox> StimulusGroups = new List<GroupBox>();
        private static Padding StimulusPanePadding = new Padding(10);
        private static Padding StatementPadding = new Padding(20, 5, 5, 5);
        private Size ScrollingPreviewPanelPaneSize = new Size(112, 112);
        private List<List<CIATItem>> IATItems = new List<List<CIATItem>>();
        private CSelectionSpecifier _Specifier;
        private TextBox SelectStatementBox = new TextBox();
        private int SelectedStatementNdx = -1;
        private List<String> ResponseValues = new List<String>();
        private bool IsPainting = false;

        public override List<CDynamicSpecifier> GetDefinedSpecifiers()
        {
            List<CDynamicSpecifier> specifiers = new List<CDynamicSpecifier>();
            specifiers.Add(Specifier);
            return specifiers;
        }

        public CSelectionSpecifier Specifier
        {
            get
            {
                return _Specifier;
            }
            set
            {
                CDynamicSpecifier.DeleteSpecifier(_Specifier.ID);
                _Specifier = value;
                if ((value.SurveyName != Survey.Name) || (value.ItemNum != Survey.GetItemNum(Question)))
                    throw new Exception("Error: Attempt made to modify the survey or survey item in a selection specifier control after it has been instantiated.");
                foreach (List<CIATItem> list in IATItems)
                    list.Clear();
                foreach (String responseVal in _Specifier.KeyMap.Keys)
                {
                    int paneNdx = -1;
                    switch (Question.Response.ResponseType)
                    {
                        case CResponse.EResponseType.Boolean:
                            if (Convert.ToInt32(responseVal) == 1)
                                paneNdx = 0;
                            else if (Convert.ToInt32(responseVal) == 0)
                                paneNdx = 1;
                            break;

                        case CResponse.EResponseType.Multiple:
                            paneNdx = Convert.ToInt32(responseVal) - 1;
                            break;

                        case CResponse.EResponseType.WeightedMultiple:
                            for (int ctr = 0; ctr < ((CWeightedMultipleResponse)Question.Response).Weights.Length; ctr++)
                                if (((CWeightedMultipleResponse)Question.Response).Weights[ctr] == Convert.ToInt32(responseVal))
                                    paneNdx = ctr;
                            break;
                    }
                    if (paneNdx == -1)
                        throw new Exception("Unindexed response value encountered while initializing Selection Specifier Control");
                    IATItems[paneNdx].AddRange(_Specifier.KeyMap[responseVal]);
                }
                for (int ctr = 0; ctr < StimulusPanes.Count; ctr++)
                {
                    if (IATItems[ctr].Count > 0)
                        StimulusPanes[ctr].BackgroundImage = IATItems[ctr][0].StimulusImage.theImage;
                    else
                        StimulusPanes[ctr].BackgroundImage = null;
                }
            }
        }

        private void InitializeComponents(int width)
        {
            this.DragOver += new DragEventHandler(SelectionSpecifierControl_DragOver);
            this.DragDrop += new DragEventHandler(SelectionSpecifierControl_DragDrop);
            QuestionEdit = new TextBox();
            QuestionEdit.Font = DisplayFont;
            QuestionEdit.BorderStyle = BorderStyle.None;
            QuestionEdit.ForeColor = System.Drawing.Color.Black;
            QuestionEdit.BackColor = System.Drawing.Color.White;
            QuestionEdit.Location = new Point(1, 1);
            QuestionEdit.Size = TextRenderer.MeasureText(Question.Text, DisplayFont, new Size(width - 2 - QuestionEdit.Margin.Horizontal, 0),
                TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak | TextFormatFlags.Left) + new Size(QuestionEdit.Margin.Horizontal + 2, QuestionEdit.Margin.Vertical + 2);
            QuestionEdit.Text = Question.Text;
            QuestionEdit.MouseEnter += new EventHandler(QuestionEdit_MouseEnter);
            QuestionEdit.MouseLeave += new EventHandler(QuestionEdit_MouseLeave);
//            QuestionEdit.TextChanged += new EventHandler(QuestionEdit_TextChanged);
            QuestionEdit.AllowDrop = true;
            QuestionEdit.DragOver += new DragEventHandler(SelectionSpecifierControl_DragOver);
            QuestionEdit.DragDrop += new DragEventHandler(SelectionSpecifierControl_DragDrop);
            Controls.Add(QuestionEdit);

            ResponseValues = CreateStatementPanels();
            this.Height = Statements.Last().Bottom + StatementPadding.Bottom;
            for (int ctr = 0; ctr < Statements.Count; ctr++)
            {
                GroupBox g = new GroupBox();
                g.Size = ScrollingPreviewPanelPaneSize + new Size(6, 19);
                g.Location = new Point(width - g.Size.Width - StimulusPanePadding.Right, QuestionEdit.Bottom + StimulusPanePadding.Top);
                g.AllowDrop = true;
                g.DragOver += new DragEventHandler(SelectionSpecifierControl_DragOver);
                g.Text = String.Format("Stimulus Group #{0}", ctr);
                StimulusGroups.Add(g);

                Panel p = new Panel();
                p.Size = ScrollingPreviewPanelPaneSize;
                p.Location = new Point(3, 16);
                p.BackColor = System.Drawing.Color.White;
                p.AllowDrop = true;
                p.MouseEnter += new EventHandler(StimulusPane_MouseEnter);
                p.DragEnter += new DragEventHandler(StimulusPane_DragEnter);
                g.Controls.Add(p);
                StimulusPanes.Add(p);

                IATItems.Add(new List<CIATItem>());
            }

            List<List<CIATItem>> iatItems = new List<List<CIATItem>>();
            foreach (String s in ResponseValues)
                iatItems.Add(new List<CIATItem>());

            _Specifier = new CSelectionSpecifier(Survey.Name, Survey.GetItemNum(Question), iatItems, ResponseValues);
            this.Width = width;

            SelectStatementBox.Location = new Point(width - StimulusPanePadding.Right - (ScrollingPreviewPanelPaneSize.Width + 6), 
                QuestionEdit.Bottom + StimulusPanePadding.Bottom);
            SelectStatementBox.Multiline = true;
            SelectStatementBox.ReadOnly = true;
            SelectStatementBox.BorderStyle = BorderStyle.None;
            SelectStatementBox.BackColor = System.Drawing.Color.White;
            SelectStatementBox.ForeColor = System.Drawing.Color.DarkGray;
            SelectStatementBox.Size = ScrollingPreviewPanelPaneSize + new Size(6, 19);
            SelectStatementBox.Text = Properties.Resources.sSelectionSpecifierSelectStatement;
            SelectStatementBox.TextAlign = HorizontalAlignment.Center;
            SelectStatementBox.Font = DisplayFont;
            Controls.Add(SelectStatementBox);

            if (this.Height < SelectStatementBox.Bottom + StimulusPanePadding.Bottom)
                this.Height = SelectStatementBox.Bottom + StimulusPanePadding.Bottom;
        }

        public SelectionSpecifierControl(CSurvey survey, CSurveyItem question, int width)
        {
            Survey = survey;
            Question = question;
            InitializeComponents(width);
            this.Paint += new PaintEventHandler(SelectionSpecifierControl_Paint);
        }

        void SelectionSpecifierControl_Paint(object sender, PaintEventArgs e)
        {
            if (IsPainting)
                return;
            IsPainting = true;
            e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle);
            if (SelectedStatementNdx != -1)
            {

                if (!Statements[SelectedStatementNdx].Focused)
                {
                    Statements[SelectedStatementNdx].BackColor = System.Drawing.Color.LightGray;
                    Statements[SelectedStatementNdx].BorderStyle = BorderStyle.None;
                }

                Rectangle StatementOutlineRect = new Rectangle(Statements[SelectedStatementNdx].Left - (StatementPadding.Left >> 1),
                    Statements[SelectedStatementNdx].Top - StatementPadding.Top, this.Width - StimulusGroups[SelectedStatementNdx].Width - StimulusPanePadding.Horizontal
                    - Statements[SelectedStatementNdx].Left + (StatementPadding.Left >> 1),
                    Statements[SelectedStatementNdx].Height + StatementPadding.Vertical);
                e.Graphics.FillRectangle(Brushes.LightGray, StatementOutlineRect);
                Point ptStimulusOutlineRect = new Point(StatementOutlineRect.Right, (StimulusGroups[SelectedStatementNdx].Top - StimulusPanePadding.Top < StatementOutlineRect.Top)
                    ? StimulusGroups[SelectedStatementNdx].Top - StimulusPanePadding.Top : StatementOutlineRect.Top);
                Size szStimulusOutlineRect = new Size(StimulusGroups[SelectedStatementNdx].Width + StimulusPanePadding.Horizontal,
                    (StimulusGroups[SelectedStatementNdx].Height + StimulusPanePadding.Vertical + ptStimulusOutlineRect.Y > StatementOutlineRect.Bottom) ?
                    StimulusGroups[SelectedStatementNdx].Height + StimulusPanePadding.Vertical : StatementOutlineRect.Bottom - ptStimulusOutlineRect.Y);
                Rectangle StimulusOutlineRect = new Rectangle(ptStimulusOutlineRect, szStimulusOutlineRect);
                e.Graphics.FillRectangle(Brushes.LightGray, StimulusOutlineRect);
            }
            IsPainting = false;
        }

        private List<String> CreateStatementPanels()
        {
            List<String> StatementText = new List<String>();
            List<String> responseVals = new List<String>();

            switch (Question.Response.ResponseType)
            {
                case CResponse.EResponseType.Boolean:
                    StatementText.Add(((CBoolResponse)Question.Response).TrueStatement);
                    StatementText.Add(((CBoolResponse)Question.Response).FalseStatement);
                    responseVals.Add("1");
                    responseVals.Add("0");
                    break;

                case CResponse.EResponseType.Multiple:
                    for (int ctr = 0; ctr < ((CMultipleResponse)Question.Response).Choices.Length; ctr++)
                    {
                        StatementText.Add(((CMultipleResponse)Question.Response).Choices[ctr]);
                        responseVals.Add((ctr + 1).ToString());
                    }
                    break;

                case CResponse.EResponseType.WeightedMultiple:
                    for (int ctr = 0; ctr < ((CWeightedMultipleResponse)Question.Response).Choices.Length; ctr++)
                    {
                        StatementText.Add(((CWeightedMultipleResponse)Question.Response).Choices[ctr]);
                        responseVals.Add(((CWeightedMultipleResponse)Question.Response).Weights[ctr].ToString());
                    }
                    break;
            }

            Point ptStatement = new Point(StatementPadding.Left, QuestionEdit.Bottom + StatementPadding.Vertical);
            for (int ctr = 0; ctr < StatementText.Count; ctr++)
            {
                TextBox tb = new TextBox();
                tb.Text = StatementText[ctr];
                tb.Size = TextRenderer.MeasureText(StatementText[ctr], DisplayFont, new Size(this.Width - ScrollingPreviewPanelPaneSize.Width - 6 - StimulusPanePadding.Horizontal
                    - StatementPadding.Horizontal, 0), TextFormatFlags.WordBreak | TextFormatFlags.Left | TextFormatFlags.NoPrefix)
                    + new Size(tb.Margin.Horizontal + 2, tb.Margin.Vertical + 2);
                tb.Location = ptStatement;
                tb.BorderStyle = BorderStyle.None;
                ptStatement += new Size(0, tb.Size.Height + StatementPadding.Vertical);
                tb.Font = DisplayFont;
                tb.BackColor = System.Drawing.Color.White;
                tb.ForeColor = System.Drawing.Color.Black;
                tb.AllowDrop = true;
                tb.MouseEnter += new EventHandler(Statement_MouseEnter);
                tb.MouseLeave += new EventHandler(Statement_MouseLeave);
                tb.DragOver += new DragEventHandler(Statement_DragOver);
                tb.Click += new EventHandler(Statement_Click);
                Statements.Add(tb);
                Controls.Add(tb);
            }
            return responseVals;
        }

        private void Statement_Click(object sender, EventArgs e)
        {
            if (SelectedStatementNdx == -1)
                Controls.Remove(SelectStatementBox);
            else
                Controls.Remove(StimulusGroups[SelectedStatementNdx]);
            SelectedStatementNdx = Statements.IndexOf((TextBox)sender);
            Controls.Add(StimulusGroups[SelectedStatementNdx]);
            Invalidate();
        }

        private void Statement_MouseLeave(object sender, EventArgs e)
        {
            SuspendLayout();
            TextBox tb = (TextBox)sender;
            tb.BackColor = System.Drawing.Color.White;
            ResumeLayout(true);
        }

        private void Statement_MouseEnter(object sender, EventArgs e)
        {
            SuspendLayout();
            TextBox tb = (TextBox)sender;
            tb.BackColor = System.Drawing.Color.LightGray;
            ResumeLayout(true);
        }

        private void Statement_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            SuspendLayout();
            tb.Location -= new Size(1, 1);
            tb.BackColor = System.Drawing.Color.LightBlue;
            tb.BorderStyle = BorderStyle.FixedSingle;
            ResumeLayout(true);
        }

        private void Statement_Leave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            SuspendLayout();
            tb.Location += new Size(1, 1);
            tb.BorderStyle = BorderStyle.None;
            tb.BackColor = System.Drawing.Color.White;
            ResumeLayout(true);
        }

        private void Statement_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Size szText = TextRenderer.MeasureText(tb.Text, tb.Font, new Size(tb.Width - tb.Margin.Horizontal - 2, 0), TextFormatFlags.Left
                | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak);
            if (szText.Height + tb.Margin.Vertical + 2 != tb.Height)
            {
                int oldHeight = tb.Height;
                tb.Height = szText.Height + tb.Margin.Vertical + 2;
                SuspendLayout();
                int ndx = Statements.IndexOf(tb);
                for (int ctr = ndx + 1; ctr < Statements.Count; ctr++)
                {
                    Statements[ctr].Location += new Size(0, tb.Height - oldHeight);
                }
                ResumeLayout(true);
            }
        }

        private void Statement_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void StimulusPane_DragEnter(object sender, DragEventArgs e)
        {
            int ndx = StimulusPanes.IndexOf((Panel)sender);
            StimulusGroupDisplay display = new StimulusGroupDisplay();
            display.RetrieveIATItem = new DynamicIATPanel.RetrieveIATItemWithNdx(IATItemRetriever);
            display.OnClosed += new StimulusGroupDisplay.OnClosedHandler(StimulusDisplay_Closed);
            display.CaptionText = "Stimuli - select the keyed direction for chosen survey item responses";
            display.Stimuli = IATItems[ndx];
            display.Show();
            display.SetDesktopLocation(PointToScreen(StimulusGroups[ndx].Location).X, PointToScreen(StimulusGroups[ndx].Location).Y);
        }

        private void StimulusPane_MouseEnter(object sender, EventArgs e)
        {
            int ndx = StimulusPanes.IndexOf((Panel)sender);
            StimulusGroupDisplay display = new StimulusGroupDisplay();
            display.RetrieveIATItem = new DynamicIATPanel.RetrieveIATItemWithNdx(IATItemRetriever);
            display.OnClosed += new StimulusGroupDisplay.OnClosedHandler(StimulusDisplay_Closed);
            display.CaptionText = "Stimuli - select the keyed direction for chosen survey item responses";
            display.Stimuli = IATItems[ndx];
            display.Show();
            display.SetDesktopLocation(PointToScreen(StimulusGroups[ndx].Location).X, PointToScreen(StimulusGroups[ndx].Location).Y);
        }

        private void StimulusDisplay_Closed(List<CIATItem> Stimuli)
        {
            IATItems[SelectedStatementNdx].Clear();
            IATItems[SelectedStatementNdx].AddRange(Stimuli);
            Specifier.ClearIATItems(ResponseValues[SelectedStatementNdx]);
            Specifier.AddIATItems(ResponseValues[SelectedStatementNdx], Stimuli);
            if (Stimuli.Count > 0)
            {
                if (StimulusPanes[SelectedStatementNdx].BackgroundImage != null)
                    StimulusPanes[SelectedStatementNdx].BackgroundImage.Dispose();
                StimulusPanes[SelectedStatementNdx].BackgroundImage = Stimuli.Last().StimulusImage.Thumbnail.Image;
            }
            else
            {
                Image i = StimulusPanes[SelectedStatementNdx].BackgroundImage;
                StimulusPanes[SelectedStatementNdx].BackgroundImage = null;
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

        private void QuestionEdit_GotFocus(object sender, EventArgs e)
        {
            SuspendLayout();
            QuestionEdit.BorderStyle = BorderStyle.FixedSingle;
            QuestionEdit.Location -= new Size(1, 1);
            QuestionEdit.BackColor = System.Drawing.Color.LightBlue;
            QuestionEdit.ForeColor = System.Drawing.Color.Black;
            ResumeLayout(true);
        }

        private void QuestionEdit_LostFocus(object sender, EventArgs e)
        {
            SuspendLayout();
            QuestionEdit.BorderStyle = BorderStyle.None;
            QuestionEdit.Location += new Size(1, 1);
            QuestionEdit.BackColor = System.Drawing.Color.White;
            QuestionEdit.ForeColor = System.Drawing.Color.DarkGray;
        }

        private void SelectionSpecifierControl_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        private void SelectionSpecifierControl_DragDrop(object sender, DragEventArgs e)
        {
        }

        private ScrollingPreviewPanel.EOrientation Stimulus_GetOrientation()
        {
            return ScrollingPreviewPanel.EOrientation.horizontal;
        }

        public void Validate()
        {
        }
    }
}
*/