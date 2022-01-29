using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using IATClient.ResultData;

namespace IATClient
{
    public class ResultsGridPanel : UserControl
    {
        private ResultData.ResultData ResultData;
        private Panel ResultsPanel = new Panel();
        private Dictionary<String, List<int>> ColumnWidths = new Dictionary<String, List<int>>();
        private List<List<Rectangle>> CellRects = new List<List<Rectangle>>();
        private List<List<String>> Results = new List<List<String>>();
        private Label IATScoreLabel = new Label();
        private List<Panel> ResultContentsPanels = new List<Panel>();
        private List<float> MaxCellWidth = new List<float>();
        private Dictionary<Panel, List<SizeF>> CellSizeMap = new Dictionary<Panel, List<SizeF>>();
        private Font ResultFont;
        private static Padding _CellPadding = new Padding(3, 2, 3, 2);
        private static Padding _RowPadding = new Padding(3, 2, 3, 2);
        private int[] MaxColWidths = null;
        private int LabelColWidth = -1;
        private int NumLabelCols = -1;
        private Action<int, String, int, Control> OnLabelClick;
        private List<CGridResultRow> ResultRowPanels = new List<CGridResultRow>();
        private List<int> LabelOffsets = new List<int>();

        private readonly IATConfigMainForm MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];

        public static Padding CellPadding
        {
            get
            {
                return _CellPadding;
            }
        }
        public static Padding RowPadding
        {
            get
            {
                return _RowPadding;
            }
        }

        public ResultsGridPanel(Action<int, String, int, Control> LabelClickHandler)
        {
            AutoScaleMode = AutoScaleMode.Dpi;
            ResultFont = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 10F, FontStyle.Regular);
            OnLabelClick = LabelClickHandler;
        }

        public void Initialize(CResultData resultData)
        {
            try
            {
                this.SuspendLayout();
                this.BackColor = Color.White;
                Controls.Clear();
                this.ResultData = resultData;
                int fontHeight = (int)System.Drawing.SystemFonts.DialogFont.Height;
                ResultsPanel = new Panel();
                ResultsPanel.Location = new Point(0, 0);
                ResultsPanel.Height = this.Height;
                ResultsPanel.BackColor = Color.White;
                this.Dock = DockStyle.Fill;
                Controls.Add(ResultsPanel);
                LabelColWidth = ResultFont.Height << 1;
                NumLabelCols = 1 + resultData.ResultDescriptor.BeforeSurveys.Count + resultData.ResultDescriptor.AfterSurveys.Count;
                InitResultGridRows();
                ResultsPanel.Paint += new PaintEventHandler(ResultsPanel_Paint);
                ResultsPanel.BackColor = Color.Transparent;
                this.AutoScroll = true;
                this.ResumeLayout(true);
                ResultsPanel.MouseClick += new MouseEventHandler(ResultsPanel_Click);
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error laying out result grid", ex));
            }
        }

        void ResultsPanel_Click(object sender, MouseEventArgs e)
        {
            for (int ctr = ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 0 : 1); ctr < LabelOffsets.Count; ctr++)
            {
                if ((e.X >= LabelOffsets[ctr]) && (e.X <= LabelOffsets[ctr] + LabelColWidth))
                    OnLabelClick(ctr - ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 0 : 1), ResultData.ResultDescriptor.ConfigFile.Name, 0, this);
            }
        }

        private void ResultsPanel_Paint(Object sender, PaintEventArgs e)
        {
            for (int ndx = 0; ndx < LabelOffsets.Count; ndx++)
            {
                int offset = LabelOffsets[ndx];
                if ((offset + LabelColWidth >= this.HorizontalScroll.Value) && (offset <= this.HorizontalScroll.Value + ResultsPanel.Width))
                {
                    e.Graphics.FillRectangle(Brushes.CornflowerBlue, new Rectangle(new Point(offset, this.VerticalScroll.Value), new Size(LabelColWidth, this.Height)));
                    e.Graphics.DrawRectangle(Pens.Black, new Rectangle(new Point(offset, this.VerticalScroll.Value), new Size(LabelColWidth, this.Height)));
                    String txt;
                    if ((ndx == 0) && (ResultData.ResultDescriptor.TokenType != ETokenType.NONE))
                        txt = ResultData.ResultDescriptor.TokenName;
                    else if (ndx < ResultData.ResultDescriptor.BeforeSurveys.Count + ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 0 : 1))
                        txt = String.Format("Survey #{0}", ndx + ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 1 : 0));
                    else if (ndx == ResultData.ResultDescriptor.BeforeSurveys.Count + ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 0 : 1))
                        txt = String.Format("IAT Scores");
                    else
                        txt = String.Format("Survey #{0}", ndx - 1 + ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 1 : 0));
                    SizeF szText = e.Graphics.MeasureString(txt, ResultFont);
                    e.Graphics.DrawString(txt, ResultFont, Brushes.Black, new PointF(offset + ((LabelColWidth - szText.Height) / 2), this.VerticalScroll.Value + 75), new StringFormat(StringFormatFlags.DirectionVertical));
                }

            }
            foreach (CGridResultRow r in ResultRowPanels)
            {
                if (r != null)
                    if (r.InClipRect(e.ClipRectangle))
                        r.Draw(e.Graphics, ResultFont);
            }
        }

        private Dictionary<Point, List<String>> GetBoundedTextItems()
        {
            Dictionary<Point, List<String>> boundedTextResponses = new Dictionary<Point, List<String>>();
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.BeforeSurveys.Count; ctr++)
            {
                for (int ctr2 = 0; ctr2 < ResultData.ResultDescriptor.BeforeSurveys[ctr].NumItems; ctr2++)
                {
                    if (ResultData.ResultDescriptor.BeforeSurveys[ctr].SurveyItems[ctr2].Response.ResponseType == ResponseType.BoundedLength)
                    {
                        Point pt = new Point(ctr, ctr2);
                        List<String> responses = new List<String>();
                        for (int ctr3 = 0; ctr3 < ResultData.IATResults.NumResultSets; ctr3++)
                            responses.Add(ResultData.IATResults[ctr3].BeforeSurveys[ctr][ctr2].Value);
                        boundedTextResponses[pt] = responses;
                    }
                }
            }
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.AfterSurveys.Count; ctr++)
            {
                for (int ctr2 = 0; ctr2 < ResultData.ResultDescriptor.AfterSurveys[ctr].NumItems; ctr2++)
                {
                    if (ResultData.ResultDescriptor.AfterSurveys[ctr].SurveyItems[ctr2].Response.ResponseType == ResponseType.BoundedLength)
                    {
                        Point pt = new Point(ctr + ResultData.ResultDescriptor.BeforeSurveys.Count, ctr2);
                        List<String> responses = new List<String>();
                        for (int ctr3 = 0; ctr3 < ResultData.IATResults.NumResultSets; ctr3++)
                            responses.Add(ResultData.IATResults[ctr3].AfterSurveys[ctr][ctr2].Value);
                        boundedTextResponses[pt] = responses;
                    }
                }
            }
            return boundedTextResponses;
        }

        private void CalcColumnWidths()
        {
            int fontHeight = (int)ResultFont.Height;
            int answerWidth = 0, width, segmentWidth, labelWidth;
            if (this.ResultData.ResultDescriptor.TokenType != ETokenType.NONE)
            {
                ColumnWidths["Token"] = new List<int>();
                int tokenWidth = 0;
                for (int ctr = 0; ctr < this.ResultData.IATResults.NumResultSets; ctr++)
                {
                    width = TextRenderer.MeasureText(ResultData.IATResults[ctr].Token, ResultFont).Width;
                    if (width > tokenWidth)
                        tokenWidth = width;
                }
                ColumnWidths["Token"].Add(tokenWidth + CellPadding.Horizontal);
            }
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.BeforeSurveys.Count; ctr++)
            {
                ColumnWidths[ResultData.ResultDescriptor.BeforeSurveys[ctr].Name] = new List<int>();
                segmentWidth = 0;
                for (int ctr2 = 0; ctr2 < ResultData.ResultDescriptor.BeforeSurveys[ctr].NumItems; ctr2++)
                {
                    answerWidth = 0;
                    for (int ctr3 = 0; ctr3 < ResultData.IATResults.NumResultSets; ctr3++)
                    {
                        width = TextRenderer.MeasureText(ResultData.IATResults[ctr3].BeforeSurveys[ctr][ctr2].Value, ResultFont).Width;
                        if (width > answerWidth)
                            answerWidth = width;
                    }
                    ColumnWidths[ResultData.ResultDescriptor.BeforeSurveys[ctr].Name].Add(answerWidth + CellPadding.Horizontal);
                    segmentWidth += answerWidth + CellPadding.Horizontal;
                }
                segmentWidth = 0;
            }
            answerWidth = 0;
            ColumnWidths["IAT Score"] = new List<int>();
            for (int ctr = 0; ctr < ResultData.IATResults.NumResultSets; ctr++)
            {
                width = TextRenderer.MeasureText(ResultData.IATResults[ctr].IATScore.ToString("F6"), ResultFont).Width;
                if (answerWidth < width)
                    answerWidth = width;
            }
            ColumnWidths["IAT Score"].Add(answerWidth + CellPadding.Horizontal);
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.AfterSurveys.Count; ctr++)
            {
                ColumnWidths[ResultData.ResultDescriptor.AfterSurveys[ctr].Name] = new List<int>();
                segmentWidth = 0;
                for (int ctr2 = 0; ctr2 < ResultData.ResultDescriptor.AfterSurveys[ctr].NumItems; ctr2++)
                {
                    answerWidth = 0;
                    for (int ctr3 = 0; ctr3 < ResultData.IATResults.NumResultSets; ctr3++)
                    {
                        width = TextRenderer.MeasureText(ResultData.IATResults[ctr3].AfterSurveys[ctr][ctr2].Value, ResultFont).Width;
                        if (width > answerWidth)
                            answerWidth = width;
                    }
                    ColumnWidths[ResultData.ResultDescriptor.AfterSurveys[ctr].Name].Add(answerWidth + CellPadding.Horizontal);
                    segmentWidth += answerWidth + CellPadding.Horizontal;
                }
                segmentWidth = 0;
            }
        }

        private double GetScaleWeight(Size[] sizes)
        {
            double mean = 0;
            double weight = 0;
            int[] widths = new int[sizes.Length];
            for (int ctr = 0; ctr < sizes.Length; ctr++)
            {
                widths[ctr] = sizes[ctr].Width;
                mean += widths[ctr];
            }
            mean /= (double)widths.Length;
            double top = 0;
            for (int ctr = 0; ctr < widths.Length; ctr++)
                top += Math.Pow(widths[ctr] - mean, 2);
            return mean + Math.Sqrt(top / (widths.Length - 1));
        }

        private Dictionary<String, List<int>> GetBoundedLengthItems()
        {
            Dictionary<String, List<int>> BoundedLengthItems = new Dictionary<String, List<int>>();
            String elemName;
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.BeforeSurveys.Count; ctr++)
            {
                elemName = ResultData.ResultDescriptor.BeforeSurveys[ctr].Name;
                BoundedLengthItems[elemName] = new List<int>();
                for (int ctr2 = 0; ctr2 < ResultData.ResultDescriptor.BeforeSurveys[ctr].Questions.Length; ctr2++)
                {
                    if (ResultData.ResultDescriptor.BeforeSurveys[ctr].Questions[ctr2].Response.ResponseType == ResponseType.BoundedLength)
                        BoundedLengthItems[elemName].Add(ctr2);
                }
            }
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.AfterSurveys.Count; ctr++)
            {
                elemName = ResultData.ResultDescriptor.AfterSurveys[ctr].Name;
                BoundedLengthItems[elemName] = new List<int>();
                for (int ctr2 = 0; ctr2 < ResultData.ResultDescriptor.AfterSurveys[ctr].Questions.Length; ctr2++)
                {
                    if (ResultData.ResultDescriptor.AfterSurveys[ctr].Questions[ctr2].Response.ResponseType == ResponseType.BoundedLength)
                        BoundedLengthItems[elemName].Add(ctr2);
                }
            }
            return BoundedLengthItems;
        }

        private int GetResultsContentsWidth()
        {
            int width = 0;
            foreach (List<int> li in ColumnWidths.Values)
                foreach (int i in li)
                    width += i;
            return width;
        }

        private List<String> GetSurveyResults(String surveyName, int itemNdx)
        {
            List<String> responses = new List<String>();
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.BeforeSurveys.Count; ctr++)
                if (surveyName == ResultData.ResultDescriptor.BeforeSurveys[ctr].Name)
                    for (int ctr2 = 0; ctr2 < ResultData.IATResults.NumResultSets; ctr2++)
                        responses.Add(ResultData.IATResults[ctr2].BeforeSurveys[ctr][itemNdx].Value);
            for (int ctr = 0; ctr < ResultData.ResultDescriptor.AfterSurveys.Count; ctr++)
                if (surveyName == ResultData.ResultDescriptor.AfterSurveys[ctr].Name)
                    for (int ctr2 = 0; ctr2 < ResultData.IATResults.NumResultSets; ctr2++)
                        responses.Add(ResultData.IATResults[ctr2].AfterSurveys[ctr][itemNdx].Value);
            return responses;
        }

        private int maxWidth(Size[] szs)
        {
            int width = 0;
            for (int ctr = 0; ctr < szs.Length; ctr++)
                if (width < szs[ctr].Width)
                    width = szs[ctr].Width;
            return width;
        }

        private void LayoutResults(int trimAmount)
        {
            double[] trims = null;
            Dictionary<String, List<int>> boundedLengthItemNums = GetBoundedLengthItems();
            if (this.ResultData.ResultDescriptor.TokenType == ETokenType.BASE64_UTF8) {
                boundedLengthItemNums[this.ResultData.ResultDescriptor.TokenName] = new List<int>();
                boundedLengthItemNums[this.ResultData.ResultDescriptor.TokenName].Add(0);
            }
            List<Size> tokenSizes = new List<Size>();
            List<Size[]> boundedSizes = new List<Size[]>();
            if (trimAmount > 0)
            {
                List<double> scaleWeights = new List<double>();
                if (this.ResultData.ResultDescriptor.TokenType == ETokenType.BASE64_UTF8)
                {
                    for (int ctr2 = 0; ctr2 < this.ResultData.IATResults.NumResultSets; ctr2++) 
                        tokenSizes.Add(TextRenderer.MeasureText(this.ResultData.IATResults[ctr2].Token, ResultFont, new Size(this.Width >> 2, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl) + new Size(CellPadding.Horizontal, CellPadding.Vertical));
                    int nWeights = 0;
                    double sWeight = 0;
                    for (int ctr3 = 0; ctr3 < tokenSizes.Count; ctr3++) {
                        sWeight += tokenSizes[ctr3].Width;
                        if (tokenSizes[ctr3].Width > 0)
                            nWeights++;
                    }
                    scaleWeights.Add(sWeight / nWeights);
                }
                int ctr = 0;
                foreach (String str in boundedLengthItemNums.Keys)
                {
                    if (boundedLengthItemNums[str].Count > 0)
                    {
                        foreach (int itemNdx in boundedLengthItemNums[str])
                        {
                            List<String> responses = GetSurveyResults(str, itemNdx);
                            boundedSizes.Add(new Size[responses.Count]);
                            int ctr2 = 0;
                            foreach (String resp in responses)
                                boundedSizes[ctr][ctr2++] = TextRenderer.MeasureText(resp, ResultFont, new Size(this.Width >> 2, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl) + new Size(CellPadding.Horizontal, CellPadding.Vertical);
                        }
                        int nWeights = 0;
                        double sWeight = 0;
                        for (int ctr3 = 0; ctr3 < boundedSizes[ctr].Length; ctr3++)
                        {
                            sWeight += boundedSizes[ctr][ctr3].Width;
                            if (boundedSizes[ctr][ctr3].Width > 0)
                                nWeights++;
                        }
                        scaleWeights.Add(sWeight / nWeights);
                        ctr++;
                    }
                }
                double weightSum = 0;
                foreach (double d in scaleWeights)
                    weightSum += d;
                double coeff = weightSum / (double)trimAmount;
                trims = new double[scaleWeights.Count];
                for (ctr = 0; ctr < trims.Length; ctr++)
                    trims[ctr] = coeff * scaleWeights[ctr];
            }
            int nResultItems = 0;
            foreach (String s in ColumnWidths.Keys)
                nResultItems += ColumnWidths[s].Count;
            MaxColWidths = new int[nResultItems];
            for (int ctr = 0; ctr < ResultData.IATResults.NumResultSets; ctr++)
            {
                int xOffset = 0;
                CellRects.Add(new List<Rectangle>());
                Results.Add(new List<String>());
                if (this.ResultData.ResultDescriptor.TokenType != ETokenType.NONE)  {
                    xOffset += LabelColWidth + RowPadding.Left;
                    Results[ctr].Add(ResultData.IATResults[ctr].Token);
                    if ((trimAmount == 0) || (this.ResultData.ResultDescriptor.TokenType != ETokenType.BASE64_UTF8)) {
                        CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(this.ResultData.IATResults[ctr].Token, ResultFont) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                        if (ctr == 0)
                            MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                        else if (CellRects[ctr].Last().Width > MaxColWidths[CellRects[ctr].Count - 1])
                            MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                        xOffset += CellRects[ctr].Last().Width;
                    } else if ((trimAmount != 0) && (this.ResultData.ResultDescriptor.TokenType == ETokenType.BASE64_UTF8)) {
                        CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(ResultData.IATResults[ctr].Token, ResultFont, new Size(this.Width >> 2, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                        xOffset += CellRects[ctr].Last().Width;
                        if (ctr == 0)
                            MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                        else if (CellRects[ctr].Last().Width > MaxColWidths[CellRects[ctr].Count - 1])
                            MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                    }
                }
                for (int ctr2 = 0; ctr2 < ResultData.ResultDescriptor.BeforeSurveys.Count; ctr2++)
                {
                    if (ResultData.ResultDescriptor.BeforeSurveys[ctr2].NumQuestions == 0)
                        xOffset += LabelColWidth;
                    else
                        xOffset += LabelColWidth + RowPadding.Left;
                    for (int ctr3 = 0; ctr3 < ResultData.ResultDescriptor.BeforeSurveys[ctr2].NumQuestions; ctr3++)
                        Results[ctr].Add(ResultData.IATResults[ctr].BeforeSurveys[ctr2][ctr3].Value);
                    if (trimAmount == 0)
                    {
                        for (int ctr3 = 0; ctr3 < ResultData.ResultDescriptor.BeforeSurveys[ctr2].NumQuestions; ctr3++)
                        {
                            CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(ResultData.IATResults[ctr].BeforeSurveys[ctr2][ctr3].Value, ResultFont) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                            if (ctr == 0)
                                MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                            else if (CellRects[ctr].Last().Width > MaxColWidths[CellRects[ctr].Count - 1])
                                MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                            xOffset += CellRects[ctr].Last().Width;
                        }
                    }
                    else
                    {
                        String elemName = ResultData.ResultDescriptor.BeforeSurveys[ctr2].Name;
                        int boundedCtr = 0;
                        if (boundedLengthItemNums[elemName].Count == 0)
                        {
                            for (int ctr3 = 0; ctr3 < ResultData.ResultDescriptor.BeforeSurveys[ctr2].NumItems; ctr3++)
                            {
                                CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(ResultData.IATResults[ctr].BeforeSurveys[ctr2][ctr3].Value, ResultFont) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                                xOffset += CellRects[ctr].Last().Width;
                                if (ctr == 0)
                                    MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                                else if (CellRects[ctr].Last().Width > MaxColWidths[CellRects[ctr].Count - 1])
                                    MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;

                            }
                        }
                        else
                        {
                            for (int ctr3 = 0; ctr3 < ResultData.ResultDescriptor.BeforeSurveys[ctr2].NumItems; ctr3++)
                            {
                                if (boundedLengthItemNums[elemName][boundedCtr] == ctr3)
                                    CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(ResultData.IATResults[ctr].BeforeSurveys[ctr2][ctr3].Value, ResultFont, new Size(this.Width >> 2, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                                else
                                    CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(ResultData.IATResults[ctr].BeforeSurveys[ctr2][ctr3].Value, ResultFont) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                                xOffset += CellRects[ctr].Last().Width;
                                if (ctr == 0)
                                    MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                                else if (CellRects[ctr].Last().Width > MaxColWidths[CellRects[ctr].Count - 1])
                                    MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                            }
                        }
                    }
                    if (ResultData.ResultDescriptor.BeforeSurveys[ctr2].NumItems != 0)
                        xOffset += RowPadding.Right;
                }
                xOffset += LabelColWidth + RowPadding.Left;
                if (double.IsNaN(ResultData.IATResults[ctr].IATScore))
                {
                    CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText("Unscored", ResultFont) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                    Results[ctr].Add("Unscored");
                }
                else
                {
                    CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(ResultData.IATResults[ctr].IATScore.ToString("F6"), ResultFont) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                    Results[ctr].Add(ResultData.IATResults[ctr].IATScore.ToString("F6"));
                }
                xOffset += CellRects[ctr].Last().Width;
                if (ctr == 0)
                    MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                else if (CellRects[ctr].Last().Width > MaxColWidths[CellRects[ctr].Count - 1])
                    MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                xOffset += RowPadding.Right;
                for (int ctr2 = 0; ctr2 < ResultData.ResultDescriptor.AfterSurveys.Count; ctr2++)
                {
                    if (ResultData.ResultDescriptor.AfterSurveys[ctr2].NumItems == 0)
                        xOffset += LabelColWidth;
                    else
                        xOffset += LabelColWidth + RowPadding.Left;
                    for (int ctr3 = 0; ctr3 < ResultData.ResultDescriptor.AfterSurveys[ctr2].NumItems; ctr3++)
                        Results[ctr].Add(ResultData.IATResults[ctr].AfterSurveys[ctr2][ctr3].Value);
                    if (trimAmount == 0)
                    {
                        for (int ctr3 = 0; ctr3 < ResultData.ResultDescriptor.AfterSurveys[ctr2].NumItems; ctr3++)
                        {
                            CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(ResultData.IATResults[ctr].AfterSurveys[ctr2][ctr3].Value, ResultFont) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                            xOffset += CellRects[ctr].Last().Width;
                            if (ctr == 0)
                                MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                            else if (CellRects[ctr].Last().Width > MaxColWidths[CellRects[ctr].Count - 1])
                                MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                        }
                    }
                    else
                    {
                        String elemName = ResultData.ResultDescriptor.AfterSurveys[ctr2].Name;
                        int boundedCtr = 0;
                        if (boundedLengthItemNums[elemName].Count == 0)
                        {
                            for (int ctr3 = 0; ctr3 < ResultData.ResultDescriptor.AfterSurveys[ctr2].NumItems; ctr3++)
                            {
                                CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(ResultData.IATResults[ctr].AfterSurveys[ctr2][ctr3].Value, ResultFont) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                                xOffset += CellRects[ctr].Last().Width;
                                if (ctr == 0)
                                    MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                                else if (CellRects[ctr].Last().Width > MaxColWidths[CellRects[ctr].Count - 1])
                                    MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                            }
                        }
                        else
                        {
                            for (int ctr3 = 0; ctr3 < ResultData.ResultDescriptor.AfterSurveys[ctr2].NumItems; ctr3++)
                            {
                                if (boundedLengthItemNums[elemName][boundedCtr] == ctr3)
                                    CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(ResultData.IATResults[ctr].AfterSurveys[ctr2][ctr3].Value, ResultFont, new Size(this.Width >> 2, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                                else
                                    CellRects[ctr].Add(new Rectangle(new Point(xOffset, 0), TextRenderer.MeasureText(ResultData.IATResults[ctr].AfterSurveys[ctr2][ctr3].Value, ResultFont) + new Size(CellPadding.Horizontal, CellPadding.Vertical)));
                                xOffset += CellRects[ctr].Last().Width;
                                if (ctr == 0)
                                    MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                                else if (CellRects[ctr].Last().Width > MaxColWidths[CellRects[ctr].Count - 1])
                                    MaxColWidths[CellRects[ctr].Count - 1] = CellRects[ctr].Last().Width;
                            }
                        }
                    }
                    if (ResultData.ResultDescriptor.AfterSurveys[ctr2].NumItems != 0)
                        xOffset += RowPadding.Right;
                }
                int maxHeight = 0;
                for (int ctr4 = 0; ctr4 < CellRects[ctr].Count; ctr4++)
                    if (CellRects[ctr][ctr4].Height > maxHeight)
                        maxHeight = CellRects[ctr][ctr4].Height;
            }
            for (int ctr = 0; ctr < MaxColWidths.Length; ctr++)
            {
                for (int ctr2 = 0; ctr2 < ResultData.IATResults.NumResultSets; ctr2++)
                {
                    int offset = MaxColWidths[ctr] - CellRects[ctr2][ctr].Width;
                    CellRects[ctr2][ctr] = new Rectangle(new Point(CellRects[ctr2][ctr].X, CellRects[ctr2][ctr].Y), new Size(MaxColWidths[ctr], CellRects[ctr2][ctr].Height));
                    for (int ctr3 = ctr + 1; ctr3 < MaxColWidths.Length; ctr3++)
                        CellRects[ctr2][ctr3] = new Rectangle(new Point(CellRects[ctr2][ctr3].X + offset, CellRects[ctr2][ctr3].Y), new Size(CellRects[ctr2][ctr3].Width, CellRects[ctr2][ctr3].Height));
                }
            }

        }

        private void InitResultGridRows()
        {
            ResultsPanel.SuspendLayout();
            CalcColumnWidths();
            int resultsWidth = GetResultsContentsWidth();
            if (resultsWidth > this.Width + (NumLabelCols * LabelColWidth))
                LayoutResults(resultsWidth - (this.Width + (NumLabelCols * LabelColWidth)));
            else
                LayoutResults(0);
            int yOffset = 0;
            ResultRowPanels.Clear();
            LabelOffsets.Clear();
            int gridsPlaced = 0;
            List<int> hasResults = new List<int>();
            for (int ctr = 0; ctr < Results.Count; ctr++)
            {
                List<Rectangle> cellRects = new List<Rectangle>();
                List<String> results = new List<String>();
                int nResultsUsed = 0;
                int ctr2 = 0;
                int maxHeight = 0;
                int lastRowWidth = 0;
                for (ctr2 = 0; ctr2 < ResultData.ResultDescriptor.BeforeSurveys.Count + 1 + ResultData.ResultDescriptor.AfterSurveys.Count + ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 0 : 1); ctr2++)
                {
                    cellRects.Clear();
                    results.Clear();
                    if ((ctr2 == 0) && (ResultData.ResultDescriptor.TokenType != ETokenType.NONE))
                    {
                        cellRects.AddRange(CellRects[ctr].GetRange(0, 1));
                        results.AddRange(Results[ctr].GetRange(0, 1));
                        nResultsUsed++;
                    }
                    else if (ctr2 < ResultData.ResultDescriptor.BeforeSurveys.Count + ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 0 : 1))
                    {
                        int surveyNum = ctr2 - ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 0 : 1);
                        cellRects.AddRange(CellRects[ctr].GetRange(nResultsUsed, ResultData.ResultDescriptor.BeforeSurveys[surveyNum].NumItems));
                        results.AddRange(Results[ctr].GetRange(nResultsUsed, ResultData.ResultDescriptor.BeforeSurveys[surveyNum].NumItems));
                        nResultsUsed += ResultData.ResultDescriptor.BeforeSurveys[surveyNum].NumItems;
                    }
                    else if (ctr2 == ResultData.ResultDescriptor.BeforeSurveys.Count + ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 0 : 1))
                    {
                        cellRects.Add(CellRects[ctr][nResultsUsed]);
                        results.Add(Results[ctr][nResultsUsed]);
                        nResultsUsed += 1;
                    }
                    else
                    {
                        int surveyNum = ctr2 - ((ResultData.ResultDescriptor.TokenType == ETokenType.NONE) ? 0 : 1) - ResultData.ResultDescriptor.BeforeSurveys.Count - 1;
                        cellRects.AddRange(CellRects[ctr].GetRange(nResultsUsed, ResultData.ResultDescriptor.AfterSurveys[surveyNum].NumItems));
                        results.AddRange(Results[ctr].GetRange(nResultsUsed, ResultData.ResultDescriptor.AfterSurveys[surveyNum].NumItems));
                        nResultsUsed += ResultData.ResultDescriptor.AfterSurveys[surveyNum].NumItems;
                    }
                    if (ctr == 0)
                    {
                        if (cellRects.Count == 0)
                        {
                            if (ctr2 == 0)
                                LabelOffsets.Add(0);
                            else
                                LabelOffsets.Add(LabelOffsets.Last() + LabelColWidth + lastRowWidth);
                            if (hasResults.Count == 0)
                                hasResults.Add(0);
                            else
                                hasResults.Add(hasResults.Last());
                        }
                        else
                        {
                            LabelOffsets.Add(cellRects[0].Left - LabelColWidth - RowPadding.Left);
                            lastRowWidth = cellRects.Last().Right + RowPadding.Horizontal - cellRects[0].Left;
                            if (hasResults.Count == 0)
                                hasResults.Add(1);
                            else
                                hasResults.Add(hasResults.Last() + 1);
                        }
                    }
                    if (cellRects.Count > 0)
                    {
                        CGridResultRow gridRow = new CGridResultRow(cellRects, results, RowPadding, CellPadding);
                        gridRow.SetDimensions(new Rectangle(new Point(cellRects[0].Left, 1), new Size(cellRects.Last().Right - cellRects[0].Left, 1)));
                        ResultRowPanels.Add(gridRow);
                    }
                    else
                        ResultRowPanels.Add(null);
                }
                for (int ctr3 = 0; ctr3 < CellRects[ctr].Count; ctr3++)
                {
                    if (CellRects[ctr][ctr3].Height > maxHeight)
                        maxHeight = CellRects[ctr][ctr3].Height;
                }
                for (int ctr3 = 0; ctr3 < ctr2; ctr3++)
                {
                    if (ResultRowPanels[ctr3 + gridsPlaced] != null)
                    {
                        ResultRowPanels[ctr3 + gridsPlaced].Size = new Size(ResultRowPanels[ctr3 + gridsPlaced].Width, maxHeight);
                        ResultRowPanels[ctr3 + gridsPlaced].Location = new Point(ResultRowPanels[ctr3 + gridsPlaced].Left, yOffset);
                    }
                }
                yOffset += maxHeight + RowPadding.Vertical;
                gridsPlaced += ctr2;

            }
            int bottom = 0;
            for (int ctr = ResultRowPanels.Count - 1; ctr >= ResultRowPanels.Count - (ResultData.ResultDescriptor.AfterSurveys.Count + 1 + ResultData.ResultDescriptor.BeforeSurveys.Count); ctr--)
                if (ResultRowPanels[ctr] != null)
                {
                    bottom = ResultRowPanels[ctr].Bottom;
                    break;
                }
            ResultsPanel.Size = new Size(ResultsPanel.Size.Width, (this.Height < bottom) ? bottom : this.Height);
            foreach (CGridResultRow r in ResultRowPanels)
            {
                if (r == null)
                    continue;
                if (r.Right + RowPadding.Right > ResultsPanel.Width)
                    ResultsPanel.Width = r.Right + RowPadding.Right + 10;
            }
            int ctr4 = 0;
            while (ctr4 < ResultRowPanels.Count)
            {
                if (ResultRowPanels[ctr4] == null)
                    ResultRowPanels.RemoveAt(ctr4);
                else
                    ctr4++;
            }
            ResultsPanel.ResumeLayout(true);
        }
    }
}
