using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using IATClient.ResultData;
 
namespace IATClient
{
    class CDateResponseObject : CResponseObject
    {
        private List<CResponseSpecifier> DateSpecList = new List<CResponseSpecifier>();
        private List<int> DateEndNdxs = new List<int>();
        private int SelectedSpecNdx = -1;
        private String _Answer;
        private Func<CResponseObject.CResponseSpecifier> GetDateBounds = null;
        private new bool bIsNew = true;

        public String Answer
        {
            get
            {
                if (Type != EType.actual)
                    throw new InvalidOperationException();
                return _Answer;
            }
            set
            {
                if (Type != EType.actual)
                    throw new InvalidOperationException();
                _Answer = value;
            }
        }

        public CDateResponseObject(EType type, Response response)
            : base(type, response)
        {
            Answer = String.Empty;
            GetDateBounds = new Func<CResponseObject.CResponseSpecifier>(((Date)response).GetDateBounds);
        }

        public CDateResponseObject(EType type, ResultSetDescriptor rsd) : base(type, rsd) { }

        public CDateResponseObject(CDateResponseObject obj, Date resp)
            : base(obj.Type, resp)
        {
            _Answer = obj._Answer;
            DateSpecList.AddRange(obj.DateSpecList);
            DateEndNdxs.AddRange(obj.DateEndNdxs);
            GetDateBounds = new Func<CResponseObject.CResponseSpecifier>(resp.GetDateBounds);
        }

        public CDateResponseObject(EType type, CSurveyItem csi)
            : base(type, csi)
        {
            _Answer = String.Empty;
            GetDateBounds = new Func<CResponseObject.CResponseSpecifier>(((CDateResponse)csi.Response).GetDateBounds);
        }

        public override ISurveyItemResponse Response
        {
            get
            {
                return _Response;
            }
            set
            {
                _Response = value;
                if (value.IsAnswered)
                {
                    if (AnswerBox != null)
                        AnswerBox.Text = value.Value;
                }
                else if (AnswerBox != null)
                    AnswerBox.Text = String.Empty;
            }
        }

        public override bool IsSearchMatch(String val)
        {
            if ((Type != EType.search) || (Type != EType.correct))
                throw new InvalidOperationException();
            foreach (CResponseSpecifier spec in DateSpecList)
                if (spec.Contains(val))
                    return true;
            return false;
        }

        protected override List<CResponseSpecifier> ResponseSpecifiers
        {
            get
            {
                return DateSpecList;
            }
        }

        private TextBox AnswerBox = null, DateSpecBox = null;
        private RichTextBox SpecListBox = null;
        private Label AnswerLabel = null, SpecLabel = null, SpecListLabel = null;
        private Button AddDateRangeButton, DeleteSpecItemButton, ClearSpecItemButton;
        private Font ControlFont = null, SelectionFont = null;
        private Panel PreviewPanel = null;
        private System.Drawing.Color ControlBackColor, ControlForeColor;
/*
        public CBoundedLengthResponseObject DisplayedResponse
        {
            get
            {
                CResponseObject thisResp = (CResponseObject)DefinedResponse;
                if (MonthCheck.Checked)
                {
                    if (!thisResp.ParseRange(MonthBox.Text, CResponseObject.ERangeType.month))
                        return null;
                }
                else
                    thisResp.MonthRange = String.Empty;
                if (DayCheck.Checked)
                {
                    if (!thisResp.ParseRange(DayBox.Text, CResponseObject.ERangeType.day))
                        return null;
                }
                else
                    thisResp.DayRange = String.Empty;
                if (YearCheck.Checked)
                {
                    if (!thisResp.ParseRange(YearBox.Text, CResponseObject.ERangeType.year))
                        return null;
                }
                else
                    thisResp.YearRange = String.Empty;
                return DefinedResponse;
            }
            set
            {
                CResponseObject thisResp = (CResponseObject)DefinedResponse;
                CResponseObject valResp = (CResponseObject)value;
                if ((DisplayMode == EDisplayMode.correct) || (DisplayMode == EDisplayMode.search))
                {
                    thisResp.YearRange = valResp.YearRange;
                    if (thisResp.YearRange == String.Empty)
                        YearCheck.Checked = false;
                    else
                    {
                        YearCheck.Checked = true;
                        YearBox.Text = thisResp.YearRange;
                    }

                    thisResp.MonthRange = valResp.MonthRange;
                    if (thisResp.MonthRange == String.Empty)
                        MonthCheck.Checked = false;
                    else
                    {
                        MonthCheck.Checked = true;
                        MonthBox.Text == thisResp.MonthRange;
                    }

                    thisResp.DayRange = valResp.DayRange;
                    if (thisResp.DayRange == String.Empty)
                        DayCheck.Checked = false;
                    else
                    {
                        DayCheck.Checked = true;
                        DayBox.Text = thisResp.DayRange;
                    }
                }
                else if ((DisplayMode == EDisplayMode.display) || (DisplayMode == EDisplayMode.actual))
                {
                    AnswerBox.Text = value.Value;
                    if (DisplayMode == EDisplayMode.actual)
                        thisResp.Answer = value.Value;
                }
                else if (DisplayMode == EDisplayMode.none)
                    DefinedResponse = value;
            }
        }
        */
        public override void DisposeOfControls()
        {
            if (PreviewPanel != null)
                foreach (Control c in PreviewPanel.Controls)
                    c.Dispose();
            PreviewPanel = null;
            ControlFont.Dispose();
            if (SelectionFont != null)
                SelectionFont.Dispose();
        }

        private void PopulateSpecListBox()
        {
            SpecListBox.Clear();
            DateEndNdxs.Clear();
            for (int ctr = 0; ctr < DateSpecList.Count - 1; ctr++)
            {
                SpecListBox.AppendText(DateSpecList[ctr].Specifier + ", ");
                DateEndNdxs.Add(SpecListBox.Text.Length);
            }
            SpecListBox.AppendText(DateSpecList.Last().Specifier);
            DateEndNdxs.Add(SpecListBox.Text.Length);
        }

        private void SpecListBox_Click(Object sender, MouseEventArgs e)
        {
            int ndx = SpecListBox.GetCharIndexFromPosition(new Point(e.X, e.Y));
            int ctr = 0;
            while (ctr < DateSpecList.Count)
            {
                if (ndx < DateEndNdxs[ctr])
                    break;
                ctr++;
            }
            int startNdx, length;
            if (ctr == 0)
                startNdx = 0;
            else
                startNdx = DateEndNdxs[ctr - 1] + 1;
            length= DateEndNdxs[ctr] - startNdx;
            SpecListBox.SuspendLayout();
            SpecListBox.SelectAll();
            SpecListBox.SelectionColor = ControlForeColor;
            SpecListBox.SelectionFont = ControlFont;
            if (SelectedSpecNdx == ctr)
            {
                SelectedSpecNdx = -1;
                SpecListBox.Select(0, 0);
                SpecListBox.ResumeLayout(false);
            }
            SpecListBox.Select(startNdx, length);
            SpecListBox.SelectionColor = System.Drawing.Color.Green;
            SpecListBox.SelectionFont = SelectionFont;
            SpecListBox.Select(0, 0);
            SpecListBox.ResumeLayout(false);
        }

        private void AddDateRangeButton_Click(object sender, EventArgs e)
        {
            List<CResponseSpecifier> specList = new List<CResponseSpecifier>();
            String[] specs = DateSpecBox.Text.Split(',');
            int ctr = 0;
            try
            {
                foreach (CResponseSpecifier spec in specList)
                {
                    int ndx = 0;
                    try {
                    while (spec.FallsBefore(DateSpecList[ndx].Specifier))
                        if (++ndx == DateSpecList.Count)
                            break;
                    if (ndx != 0)
                    {
                        if (DateSpecList[ndx].IsRange)
                        {
                            if (spec.TestBetween(DateSpecList[ndx].Specifier.Substring(0, DateSpecList[ndx].Specifier.IndexOf('-')), DateSpecList[ndx].Specifier.Substring(DateSpecList[ndx].Specifier.IndexOf('-') + 1)))
                            {
                                if (spec.IsRange)
                                    throw new OverlapException(String.Format(Properties.Resources.sDateRangeOverlap, spec.Specifier, DateSpecList[ndx].Specifier));
                                else 
                                    throw new OverlapException(String.Format(Properties.Resources.sDateFallsBetween, spec.Specifier, DateSpecList[ndx].Specifier));
                            }
                        }
                        else
                        {
                            if (spec.IsRange)
                            {
                                if (DateSpecList[ndx].TestBetween(spec.Specifier.Substring(0, spec.Specifier.IndexOf('-')), spec.Specifier.Substring(spec.Specifier.IndexOf('-') + 1)))
                                    throw new OverlapException(String.Format(Properties.Resources.sDateEncapsulates, spec.Specifier, DateSpecList[ndx].Specifier));
                            }
                            else {
                                if (DateSpecList[ndx].Specifier == DateSpecList[ndx].Specifier)
                                    throw new OverlapException(String.Format(Properties.Resources.sDatesEqual, spec.Specifier));
                            }
                        }
                    }
                    }
                    catch (OverlapException ex)
                    {
                        if (MessageBox.Show(MainForm, ex.Message, "Conflicting Date Values", MessageBoxButtons.YesNo) == DialogResult.Yes)
                        {
                            if (spec.IsRange && DateSpecList[ndx].IsRange)
                                DateSpecList[ndx] = CDateRange.Combine((CDateRange)spec, (CDateRange)DateSpecList[ndx]);
                            else if (spec.IsRange)
                                DateSpecList[ndx] = spec;
                        }
                    }
                }
                PopulateSpecListBox();
            }
            catch (FormatException ex)
            {
                if (ex.Message == Properties.Resources.sInvalidDaysInMonth)
                    MessageBox.Show(MainForm, String.Format(Properties.Resources.sInvalidDaysInMonth, specs[ctr]), "Invalid Number of Days");
                else
                    MessageBox.Show(MainForm, String.Format(Properties.Resources.sInvalidDateRange, specs[ctr]), "Invalid Date Format");
                return;
            }
        }

        private void DeleteSpecItemButton_Click(object sender, EventArgs e)
        {
            if (SelectedSpecNdx == -1)
                return;
            DateSpecList.RemoveAt(SelectedSpecNdx);
            PopulateSpecListBox();
            SelectedSpecNdx = -1;
        }

        private void ClearSpecItemButton_Click(object send, EventArgs e)
        {
            DateSpecList.Clear();
            SelectedSpecNdx = -1;
        }

        public override Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, string fontFamily, float fontSize, int clientWidth)
        {
            if (!bIsNew)
                DisposeOfControls();
            bIsNew = false;
            PreviewPanel = new Panel();
            UpdateResponseObject();
            ControlFont = new Font(fontFamily, fontSize);
            ControlForeColor = foreColor;
            ControlBackColor = backColor;
            SelectionFont = new Font(ControlFont, FontStyle.Bold);
            if ((Type == EType.dummy) || (Type == EType.actual))
            {
                AnswerLabel = new Label();
                AnswerLabel.Text = "Answer: ";
                AnswerLabel.Font = ControlFont;
                AnswerLabel.Size = TextRenderer.MeasureText(AnswerLabel.Text, ControlFont);
                AnswerLabel.ForeColor = foreColor;
                AnswerLabel.BackColor = backColor;
                AnswerBox = new TextBox();
                AnswerBox.Font = ControlFont;
                AnswerBox.ForeColor = foreColor;
                AnswerBox.BackColor = backColor;
                AnswerBox.Location = new Point((int)(.15 * clientWidth + AnswerLabel.Width + ElementPadding.Horizontal), (AnswerBox.Height > AnswerLabel.Height) ? 0 : Math.Abs((AnswerLabel.Height - AnswerBox.Height) >> 1));
                AnswerBox.Width = (int)(.2 * clientWidth);
                AnswerLabel.Location = new Point((int)(.15 * clientWidth), (AnswerLabel.Height > AnswerBox.Height) ? 0 : Math.Abs((AnswerBox.Height - AnswerLabel.Height) >> 1));
                if (Type == EType.actual)
                {
                    AnswerBox.Enabled = false;
                    AnswerBox.Text = Answer;
                    AnswerBox.BackColor = backColor;
                    AnswerBox.ForeColor = foreColor;
                }
                PreviewPanel.Controls.Add(AnswerLabel);
                PreviewPanel.Controls.Add(AnswerBox);
                PreviewPanel.Size = new Size(clientWidth, AnswerBox.Bottom);
            }
            else if ((Type == EType.search) || (Type == EType.correct))
            {
                SpecListLabel = new Label();
                SpecListLabel.Text = "Date Ranges: ";
                SpecListLabel.Font = ControlFont;
                SpecListLabel.Size = TextRenderer.MeasureText(SpecListLabel.Text, ControlFont);
                SpecListLabel.ForeColor = foreColor;
                SpecListLabel.BackColor = backColor;
                SpecListBox = new RichTextBox();
                SpecListBox.ReadOnly = true;
                SpecListBox.Multiline = true;
                SpecListBox.Font = ControlFont;
                SpecListBox.ReadOnly = true;
                SpecListBox.ForeColor = foreColor;
                SpecListBox.BackColor = backColor;
                PopulateSpecListBox();
                SpecListBox.MouseClick += new MouseEventHandler(SpecListBox_Click);
                PreviewPanel.Controls.Add(SpecListBox);
                PreviewPanel.Controls.Add(SpecListLabel);

                DeleteSpecItemButton = new Button();
                DeleteSpecItemButton.Text = "Delete Date";
                DeleteSpecItemButton.Enabled = false;
                DeleteSpecItemButton.Font = ControlFont;
                DeleteSpecItemButton.ForeColor = foreColor;
                DeleteSpecItemButton.BackColor = backColor;
                DeleteSpecItemButton.Width = TextRenderer.MeasureText(DeleteSpecItemButton.Text, ControlFont).Width + ElementPadding.Horizontal;
                ClearSpecItemButton = new Button();
                ClearSpecItemButton.Text = "Clear Dates";
                if (DateSpecList.Count > 0)
                    ClearSpecItemButton.Enabled = true;
                else
                    ClearSpecItemButton.Enabled = false;
                ClearSpecItemButton.Font = ControlFont;
                ClearSpecItemButton.ForeColor = foreColor;
                ClearSpecItemButton.BackColor = backColor;
                ClearSpecItemButton.Width = TextRenderer.MeasureText(ClearSpecItemButton.Text, ControlFont).Width + ElementPadding.Horizontal;
                
                AddDateRangeButton = new Button();
                AddDateRangeButton.Text = "Add Dates";
                AddDateRangeButton.Enabled = false;
                AddDateRangeButton.Font = ControlFont;
                AddDateRangeButton.ForeColor = foreColor;
                AddDateRangeButton.BackColor = backColor;
                AddDateRangeButton.Width = TextRenderer.MeasureText(AddDateRangeButton.Text, ControlFont).Width + ElementPadding.Horizontal;

                int maxButtonWidth = DeleteSpecItemButton.Width;
                if (maxButtonWidth < ClearSpecItemButton.Width)
                    maxButtonWidth = ClearSpecItemButton.Width;
                if (maxButtonWidth < AddDateRangeButton.Width)
                    maxButtonWidth = AddDateRangeButton.Width;
                DeleteSpecItemButton.Width = maxButtonWidth;
                ClearSpecItemButton.Width = maxButtonWidth;
                AddDateRangeButton.Width = maxButtonWidth;

                SpecLabel = new Label();
                SpecLabel.Text = "Enter date ranges:";
                SpecLabel.Width = TextRenderer.MeasureText(SpecLabel.Text, System.Drawing.SystemFonts.DefaultFont).Width;

                DateSpecBox = new TextBox();
                DateSpecBox.Font = ControlFont;
                DateSpecBox.ForeColor = foreColor;
                DateSpecBox.BackColor = backColor;
                DateSpecBox.Width = (int)(.6 * clientWidth);
                DateSpecBox.Location = new Point((int)(.15 * clientWidth + SpecLabel.Width + ElementPadding.Horizontal), ((DateSpecBox.Height < AddDateRangeButton.Height) ? ((AddDateRangeButton.Height - DateSpecBox.Height) >> 1) : 0)
                    + SpecListBox.Bottom + ElementPadding.Vertical);
                SpecLabel.Location = new Point((int)(.15 * clientWidth), DateSpecBox.Top + ((DateSpecBox.Height < SpecLabel.Height) ? ((SpecLabel.Height - DateSpecBox.Height) >> 1) : ((DateSpecBox.Height - SpecLabel.Height) >> 1)));
                AddDateRangeButton.Location = new Point(DateSpecBox.Right + ElementPadding.Horizontal, ((AddDateRangeButton.Height < DateSpecBox.Height) ? (DateSpecBox.Top + ((DateSpecBox.Height - AddDateRangeButton.Height) >> 1)) : (SpecListBox.Bottom + ElementPadding.Vertical)));
                SpecListBox.Size = new Size((int)(.6 * clientWidth), (int)(3 * ControlFont.Height * 3 / .975F));
                int nDiff = SpecListBox.Height - 2 * ClearSpecItemButton.Height;
                if (nDiff >= 0)
                {
                    SpecListBox.Location = new Point((int)(.15 * clientWidth + SpecListLabel.Width + ElementPadding.Horizontal), (SpecListBox.Height > SpecListLabel.Height) ? 0 : Math.Abs((SpecListLabel.Height - SpecListBox.Height) >> 1));
                    DeleteSpecItemButton.Location = new Point(SpecListBox.Right + ElementPadding.Horizontal, SpecListBox.Top + (nDiff / 3));
                    ClearSpecItemButton.Location = new Point(SpecListBox.Right + ElementPadding.Horizontal, SpecListBox.Bottom - (nDiff / 3));
                } else {
                    SpecListBox.Location = new Point((int)(.15 * clientWidth + SpecListLabel.Width + ElementPadding.Horizontal), (int)((SpecListBox.Height > SpecListLabel.Height) ? 0 : Math.Abs((SpecListLabel.Height - SpecListBox.Height) >> 1) - nDiff));
                    DeleteSpecItemButton.Location = new Point(SpecListBox.Right + ElementPadding.Horizontal, SpecListBox.Top - (nDiff / 3));
                    ClearSpecItemButton.Location = new Point(SpecListBox.Right + ElementPadding.Horizontal, SpecListBox.Bottom + (nDiff / 3));
                }
                PreviewPanel.Controls.Add(SpecListLabel);
                PreviewPanel.Controls.Add(SpecListBox);
                PreviewPanel.Controls.Add(DateSpecBox);
                PreviewPanel.Controls.Add(SpecLabel);
                PreviewPanel.Controls.Add(AddDateRangeButton);
                PreviewPanel.Controls.Add(DeleteSpecItemButton);
                PreviewPanel.Controls.Add(ClearSpecItemButton);
                AddDateRangeButton.Click += new EventHandler(AddDateRangeButton_Click);
                DeleteSpecItemButton.Click += new EventHandler(DeleteSpecItemButton_Click);
                ClearSpecItemButton.Click += new EventHandler(ClearSpecItemButton_Click);
            }
            return PreviewPanel;
        }
    }
}
