/*using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    class MaskSpecifierControl : SpecifierPanel
    {
        private TextBox QuestionEdit;
        private int SelectedStatementNdx = -1;
        private CSurvey Survey;
        private CSurveyItem Question;
        private List<TextBox> StatementEdits = new List<TextBox>();
        private List<Panel> StimulusPanels = new List<Panel>();
        // private List<ScrollingPreviewPanelPane> StimulusPanels = new List<ScrollingPreviewPanelPane>();
        private Padding StatementPadding = new Padding(20, 5, 5, 5);
        private Padding StimulusPadding = new Padding(10);
        private List<List<CIATItem>> IATItems = new List<List<CIATItem>>();
        private List<GroupBox> StimulusGroups = new List<GroupBox>();
        private Size StimulusSize = new Size(112, 112);
        private List<CMaskSpecifier> _Specifiers = new List<CMaskSpecifier>();
        private TextBox SelectStatementBox;
        private bool IsPainting = false;

        public override List<CDynamicSpecifier> GetDefinedSpecifiers()
        {
            List<CDynamicSpecifier> specifiers = new List<CDynamicSpecifier>();
            foreach (CMaskSpecifier s in _Specifiers)
                specifiers.Add(s);
            return specifiers;
        }

        public List<CMaskSpecifier> Specifiers
        {
            get
            {
                foreach (CMaskSpecifier ms in _Specifiers)
                {
                    ms.SurveyName = Survey.Name;
                    ms.ItemNum = Survey.GetItemNum(Question);
                }
                return _Specifiers;
            }
            set
            {
                foreach (CMaskSpecifier s in _Specifiers)
                    CDynamicSpecifier.DeleteSpecifier(s.ID);
                _Specifiers.Clear();
                _Specifiers.AddRange(value);
                for (int ctr = 0; ctr < IATItems.Count; ctr++)
                {
                    IATItems[ctr].Clear();
                    if (StimulusPanels[ctr].BackgroundImage != null)
                        StimulusPanels[ctr].BackgroundImage.Dispose();
                    StimulusPanels[ctr].BackgroundImage = null;
                }
                foreach (CMaskSpecifier ms in value)
                {
                    IATItems[ms.ItemNum - 1].Clear();
                    IATItems[ms.ItemNum - 1].AddRange(ms.IATItems);
                    if (ms.IATItems.Count > 0)
                        StimulusPanels[ms.ItemNum - 1].BackgroundImage = ms.IATItems[0].Stimulus.IImage.Image;
                    else
                        StimulusPanels[ms.ItemNum - 1].BackgroundImage = null;
                }
            }
        }


        public MaskSpecifierControl(CSurvey survey, CSurveyItem question, int width)
        {
            Question = question;
            Survey = survey;
            QuestionEdit = new TextBox();
            QuestionEdit.Location = new Point(0, 0);
            QuestionEdit.Size = TextRenderer.MeasureText(Question.Text, DisplayFont, new Size(width - 2 - QuestionEdit.Margin.Horizontal, 0),
                TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak) + new Size(2 + QuestionEdit.Margin.Horizontal, 2 + QuestionEdit.Margin.Vertical);
            QuestionEdit.BackColor = System.Drawing.Color.White;
            QuestionEdit.ForeColor = System.Drawing.Color.Black;
            QuestionEdit.BorderStyle = BorderStyle.None;
            QuestionEdit.TextChanged += new EventHandler(QuestionEdit_TextChanged);
            QuestionEdit.MouseEnter += new EventHandler(QuestionEdit_MouseEnter);
            QuestionEdit.MouseLeave += new EventHandler(QuestionEdit_MouseLeave);
            QuestionEdit.DragOver += new DragEventHandler(QuestionEdit_DragOver);
            QuestionEdit.AllowDrop = true;
            QuestionEdit.Font = DisplayFont;
            QuestionEdit.Text = question.Text;
            Controls.Add(QuestionEdit);

            // calculate number of stimuli in row
            int nStimuli = (width * 2 / 3) / (StimulusSize.Width + 6);

            // instantiate statement panels
            int statementWidth = width - (nStimuli + (StimulusSize.Width + 6));
            CMultiBooleanResponse r = (CMultiBooleanResponse)question.Response;
            String mask = "";
            for (int ctr = 0; ctr < r.LabelList.Length; ctr++)
                mask += "0";
            Point ptStatement = new Point(0, QuestionEdit.Bottom);
            for (int ctr = 0; ctr < r.LabelList.Length; ctr++)
            {
                TextBox tb = new TextBox();
                tb.BackColor = System.Drawing.Color.White;
                tb.ForeColor = System.Drawing.Color.Black;
                tb.Font = DisplayFont;
                tb.Size = TextRenderer.MeasureText(r.LabelList[ctr], DisplayFont, new Size(statementWidth, 0), TextFormatFlags.NoPrefix |
                    TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl) + new Size(tb.Margin.Horizontal + 2, tb.Margin.Vertical + 2);
                tb.BorderStyle = BorderStyle.None;
                tb.Text = r.LabelList[ctr];
                tb.Multiline = true;
                tb.MouseEnter += new EventHandler(Statement_MouseEnter);
                tb.MouseLeave += new EventHandler(Statement_MouseLeave);
                tb.Enter += new EventHandler(Statement_Enter);
                tb.Leave += new EventHandler(Statement_Leave);
                tb.Click += new EventHandler(Statement_Click);
                tb.TextChanged += new EventHandler(Statement_TextChanged);
                tb.Location = ptStatement + new Size(StatementPadding.Left, StatementPadding.Top);
                ptStatement += new Size(0, StatementPadding.Vertical + tb.Height);
                Controls.Add(tb);
                StatementEdits.Add(tb);
            }
            this.Height = StatementEdits.Last().Bottom + StatementPadding.Bottom;

            int ctr1 = 0;
            while (ctr1 < r.LabelList.Length)
            {
                GroupBox g = new GroupBox();
                g.Location = new Point(width - (StimulusSize.Width + 6) - StimulusPadding.Right, QuestionEdit.Bottom + StimulusPadding.Top);
                g.Size = StimulusSize + new Size(6, 19);
                g.Text = String.Format("Stimulus Group #{0}", ctr1 + 1);
                g.BackColor = System.Drawing.Color.LightGray;

                Panel StimulusPane = new Panel();
                StimulusPane.Location = new Point(3, 16);
                StimulusPane.Size = StimulusSize;
                StimulusPane.MouseEnter += new EventHandler(StimulusPane_MouseEnter);
                StimulusPane.DragEnter += new DragEventHandler(StimulusPane_DragEnter);
                StimulusPane.BackColor = System.Drawing.Color.White;
                StimulusPane.AllowDrop = true;
                g.Controls.Add(StimulusPane);

                StimulusPanels.Add(StimulusPane);
                StimulusGroups.Add(g);
                IATItems.Add(new List<CIATItem>());
                Specifiers.Add(new CMaskSpecifier());
                ctr1++;
            }

            SelectStatementBox = new TextBox();
            SelectStatementBox.Text = Properties.Resources.sMaskSpecifierSelectStatement;
            SelectStatementBox.Multiline = true;
            SelectStatementBox.Font = DisplayFont;
            SelectStatementBox.ReadOnly = true;
            SelectStatementBox.BorderStyle = BorderStyle.None;
            SelectStatementBox.TextAlign = HorizontalAlignment.Center;
            SelectStatementBox.ForeColor = System.Drawing.Color.DarkGray;
            SelectStatementBox.BackColor = System.Drawing.Color.White;
            SelectStatementBox.Size = StimulusSize + new Size(6, 19);
            SelectStatementBox.Location = new Point(width - (StimulusSize.Width + 6), QuestionEdit.Bottom + StimulusPadding.Top);
            Controls.Add(SelectStatementBox);

            this.Width = width;
            if (SelectStatementBox.Bottom + StimulusPadding.Bottom > this.Height)
                this.Height = SelectStatementBox.Bottom + StimulusPadding.Bottom;

            this.Paint += new PaintEventHandler(MaskSpecifierControl_Paint);
        }

        void MaskSpecifierControl_Paint(object sender, PaintEventArgs e)
        {
            if (IsPainting)
                return;
            IsPainting = true;
            e.Graphics.FillRectangle(Brushes.White, e.ClipRectangle);
            if (SelectedStatementNdx != -1)
            {
                
                if (!StatementEdits[SelectedStatementNdx].Focused)
                {
                    StatementEdits[SelectedStatementNdx].BackColor = System.Drawing.Color.LightGray;
                    StatementEdits[SelectedStatementNdx].BorderStyle = BorderStyle.None;
                }
                 
                Rectangle StatementOutlineRect = new Rectangle(StatementEdits[SelectedStatementNdx].Left - (StatementPadding.Left >> 1),
                    StatementEdits[SelectedStatementNdx].Top - StatementPadding.Top, this.Width - StimulusGroups[SelectedStatementNdx].Width - StimulusPadding.Horizontal
                    - StatementEdits[SelectedStatementNdx].Left + (StatementPadding.Left >> 1),
                    StatementEdits[SelectedStatementNdx].Height + StatementPadding.Vertical);
                e.Graphics.FillRectangle(Brushes.LightGray, StatementOutlineRect);
                Point ptStimulusOutlineRect = new Point(StatementOutlineRect.Right, (StimulusGroups[SelectedStatementNdx].Top - StimulusPadding.Top < StatementOutlineRect.Top) 
                    ? StimulusGroups[SelectedStatementNdx].Top - StimulusPadding.Top : StatementOutlineRect.Top);
                Size szStimulusOutlineRect = new Size(StimulusGroups[SelectedStatementNdx].Width + StimulusPadding.Horizontal, 
                    (StimulusGroups[SelectedStatementNdx].Height + StimulusPadding.Vertical + ptStimulusOutlineRect.Y > StatementOutlineRect.Bottom) ?
                    StimulusGroups[SelectedStatementNdx].Height + StimulusPadding.Vertical : StatementOutlineRect.Bottom - ptStimulusOutlineRect.Y);
                Rectangle StimulusOutlineRect = new Rectangle(ptStimulusOutlineRect, szStimulusOutlineRect);
                e.Graphics.FillRectangle(Brushes.LightGray, StimulusOutlineRect);
            }
            IsPainting = false;
        }

        private void Statement_MouseEnter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (StatementEdits.IndexOf(tb) != SelectedStatementNdx)
                tb.BackColor = System.Drawing.Color.LightGray;
        }

        private void Statement_MouseLeave(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            if (StatementEdits.IndexOf(tb) != SelectedStatementNdx)
                tb.BackColor = System.Drawing.Color.White;
        }

        private void Statement_Enter(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            SuspendLayout();
            foreach (TextBox t in StatementEdits)
                t.BackColor = System.Drawing.Color.White;
            tb.Location -= new Size(1, 1);
            tb.BorderStyle = BorderStyle.FixedSingle;
            tb.BackColor = System.Drawing.Color.LightBlue;
            if (SelectedStatementNdx == -1)
                Controls.Remove(SelectStatementBox);
            else
                Controls.Remove(StimulusGroups[SelectedStatementNdx]);
            SelectedStatementNdx = StatementEdits.IndexOf(tb);
            Controls.Add(StimulusGroups[SelectedStatementNdx]);
            ResumeLayout(true);
            Invalidate();
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

        private void Statement_Click(object sender, EventArgs e)
        {
        }

        private void Statement_TextChanged(object sender, EventArgs e)
        {
            TextBox tb = (TextBox)sender;
            Size newSz = TextRenderer.MeasureText(tb.Text, DisplayFont, new Size(QuestionEdit.Width - 2 - QuestionEdit.Margin.Horizontal, 0),
                TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak) + new Size(0, QuestionEdit.Margin.Vertical + 2);
            int selectedNdx = StatementEdits.IndexOf(tb);
            if (newSz.Height != tb.Height)
            {
                SuspendLayout();
                for (int ctr = selectedNdx + 1; ctr < StatementEdits.Count; ctr++)
                    StatementEdits[ctr].Location += new Size(0, newSz.Height - tb.Height);
                tb.Height = newSz.Height;
                ResumeLayout(true);
            }
            ((CMultiBooleanResponse)Question.Response).LabelList[selectedNdx] = tb.Text;
        }

        private void MaskSpecifierControl_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.None;
        }


        private void StimulusPane_MouseEnter(object sender, EventArgs e)
        {
            StimulusGroupDisplay GroupDisplay = new StimulusGroupDisplay();
            Panel sp = (Panel)sender;
            GroupDisplay.Stimuli = IATItems[StimulusPanels.IndexOf(sp)];
            GroupDisplay.CaptionText = "Stimuli - select the keyed direction for stimuli when this response is selected";
            GroupDisplay.RetrieveIATItem = new DynamicIATPanel.RetrieveIATItemWithNdx(IATItemRetriever);
            GroupDisplay.OnClosed += new StimulusGroupDisplay.OnClosedHandler(StimulusGroupDisplay_Closed);
            GroupDisplay.Show();
            GroupDisplay.SetDesktopLocation(PointToScreen(StimulusGroups[StimulusPanels.IndexOf(sp)].Location).X, 
                PointToScreen(StimulusGroups[StimulusPanels.IndexOf(sp)].Location).Y);
        }

        private void StimulusPane_DragEnter(object sender, DragEventArgs e)
        {
            StimulusGroupDisplay GroupDisplay = new StimulusGroupDisplay();
            Panel sp = (Panel)sender;
            GroupDisplay.Stimuli = IATItems[StimulusPanels.IndexOf(sp)];
            GroupDisplay.CaptionText = "Stimuli - select the keyed direction for stimuli when this response is selected";
            GroupDisplay.RetrieveIATItem = new DynamicIATPanel.RetrieveIATItemWithNdx(IATItemRetriever);
            GroupDisplay.OnClosed += new StimulusGroupDisplay.OnClosedHandler(StimulusGroupDisplay_Closed);
            GroupDisplay.Show();
            GroupDisplay.SetDesktopLocation(PointToScreen(StimulusGroups[StimulusPanels.IndexOf(sp)].Location).X,
                PointToScreen(StimulusGroups[StimulusPanels.IndexOf(sp)].Location).Y);
        }

        private void StimulusGroupDisplay_Closed(List<CIATItem> Stimuli)
        {
            IATItems[SelectedStatementNdx].Clear();
            IATItems[SelectedStatementNdx].AddRange(Stimuli);
            if (Stimuli.Count > 0) {
                if (StimulusPanels[SelectedStatementNdx].BackgroundImage != null)
                    StimulusPanels[SelectedStatementNdx].BackgroundImage.Dispose();
                StimulusPanels[SelectedStatementNdx].BackgroundImage = Stimuli.Last().StimulusImage.Thumbnail.Image;
            }
            else
            {
                Image img = StimulusPanels[SelectedStatementNdx].BackgroundImage;
                StimulusPanels[SelectedStatementNdx].BackgroundImage = null;
                if (img != null)
                    img.Dispose();
            }
            String mask = "";
            for (int ctr = 0; ctr < ((CMultiBooleanResponse)Question.Response).LabelList.Length; ctr++)
                if (ctr == SelectedStatementNdx)
                    mask += "1";
                else
                    mask += "0";
            Specifiers[SelectedStatementNdx].Mask = mask;
            Specifiers[SelectedStatementNdx].ClearIATItems();
            foreach (CIATItem i in Stimuli)
                Specifiers[SelectedStatementNdx].AddIATItem(i, mask);
        }

        private void QuestionEdit_TextChanged(object sender, EventArgs e)
        {
            Size newSz = TextRenderer.MeasureText(QuestionEdit.Text, DisplayFont, new Size(QuestionEdit.Width - 2 - QuestionEdit.Margin.Horizontal, 0),
                TextFormatFlags.Left | TextFormatFlags.NoPrefix | TextFormatFlags.WordBreak) + new Size(0, QuestionEdit.Margin.Vertical + 2);
            if (newSz.Height != QuestionEdit.Height)
            {
                SuspendLayout();
                foreach (TextBox sp in StatementEdits)
                    sp.Location += new Size(0, newSz.Height - QuestionEdit.Height);
                foreach (GroupBox gb in StimulusGroups)
                    gb.Location += new Size(0, newSz.Height - QuestionEdit.Height);
                QuestionEdit.Height = newSz.Height;
                ResumeLayout(true);
            }
            Question.Text = QuestionEdit.Text;
        }

        private ScrollingPreviewPanel.EOrientation StimulusPane_ParentOrientation()
        {
            return ScrollingPreviewPanel.EOrientation.horizontal;
        }

        private bool StimulusPane_IsDragging()
        {
            return Clipboard.ContainsData("IATItemNdx");
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

        private void QuestionEdit_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
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
            QuestionEdit.BackColor = System.Drawing.Color.White;
            QuestionEdit.ForeColor = System.Drawing.Color.DarkGray;
            QuestionEdit.Location += new Size(1, 1);
            ResumeLayout(true);
        }
    }
}
*/