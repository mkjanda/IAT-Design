using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using IATClient.ResultData;

namespace IATClient
{
    class ResultGridPanel : Panel
    {
        class ResultCell
        {
            public enum EResultType { iat, beforeSurvey, afterSurvey, enumerated, timestamp, percentile };
            private EResultType _ResultType;
            private int _SurveyNum, _ItemNum;
            private String _Result;
            private long _ResultID;

            public EResultType ResultType
            {
                get
                {
                    return _ResultType;
                }
            }

            public int SurveyNum
            {
                get
                {
                    if (ResultType == EResultType.iat)
                        return int.MinValue;
                    return SurveyNum;
                }
            }

            public int ItemNum
            {
                get
                {
                    if (ResultType == EResultType.iat)
                        return int.MinValue;
                    return _ItemNum;
                }
            }

            public String Result
            {
                get
                {
                    return _Result;
                }
            }

            public long ResultID
            {
                get
                {
                    return _ResultID;
                }
            }

            public ResultCell(EResultType resultType, long resultID, String result)
            {
                _ResultType = resultType;
                _ResultID = resultID;
                _Result = result;
            }

            public ResultCell(EResultType resultType, long resultID, String result, int surveyNum, int itemNum)
            {
                _ResultType = resultType;
                _ResultID = resultID;
                _Result = result;
                _SurveyNum = surveyNum;
                _ItemNum = itemNum;
            }
        }


        private Dictionary<Rectangle, ResultCell> ResponseRectMap = new Dictionary<Rectangle, ResultCell>();
        private int[] RectRights = null;
        private IResultData _ResultSets = null;
        private VScrollBar SurveyScroll;
        private Padding CellPadding = new Padding(8, 3, 8, 3);
        private Padding SurveyDescriptionPadding = new Padding(8, 12, 8, 12);
        private Padding SurveyItemDescriptionPadding = new Padding(0, 5, 0, 5);
        private const int SurveyDescriptionLineIndent = 15;
        private ResultSetDescriptor _Descriptor;
        private Panel SurveyDescriptorPanel;
        private Panel DataPanel;
        private int SurveyDescriptorPanelWidth;
        private int HighlightedRow = -1;
        public const int nCaptionSeperation = 3;
        private List<int> SurveyItemYOffsets = new List<int>();
        private DataGridView ResultGrid;
        private int LineHeight;

        public ResultSetDescriptor Descriptor
        {
            get
            {
                return _Descriptor;
            }
        }

        public IResultData ResultSets
        {
            get
            {
                return _ResultSets;
            }
        }

        public ResultGridPanel(Size sz)
        {
            this.Size = sz;
            ResultGrid = new DataGridView();
            ResultGrid.AllowUserToAddRows = false;
            ResultGrid.AllowUserToDeleteRows = false;
            ResultGrid.AllowUserToOrderColumns = false;
            ResultGrid.AllowUserToResizeColumns = true;
            ResultGrid.AllowUserToResizeRows = false;
            ResultGrid.CellClick += new DataGridViewCellEventHandler(ResultGrid_CellClick);
//            ResultGrid.CellDoubleClick += new DataGridViewCellEventHandler(ResultGrid_CellDoubleClick);
            ResultGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        public void PopulateResultGrid()
        {
            int nMaxIATResultWidth = 0;
            int nMaxIATPercentileWidth = 0;
            int nMaxIATTimestampWidth = 0;
            int nResults = ResultSets.NumResultSets;
            ResultGrid.Size = new Size(DataPanel.Width, DataPanel.Height);
            int nBeforeSurveyItems = 0;
            for (int ctr1 = 0; ctr1 < Descriptor.BeforeSurveys.Count; ctr1++)
                nBeforeSurveyItems += Descriptor.BeforeSurveys[ctr1].NumItems;
            int nAfterSurveyItems = 0;
            for (int ctr1 = 0; ctr1 < Descriptor.AfterSurveys.Count; ctr1++)
                nAfterSurveyItems += Descriptor.AfterSurveys[ctr1].NumItems;
            int[][] widths = new int[nBeforeSurveyItems + nAfterSurveyItems][];
            int[] maxColWidths = new int[nBeforeSurveyItems + nAfterSurveyItems];
            System.Array.Clear(maxColWidths, 0, nBeforeSurveyItems + nAfterSurveyItems);
            RectRights = new int[widths.Length];
            int maxWidth = 0;
            int ctr3 = 0, ctr4 = 0, ctr5 = 0;
            List<DataGridViewColumn> GridColumns = new List<DataGridViewColumn>();
            DataGridViewColumn col;
            DataGridViewComboBoxColumn comboCol;
            Dictionary<DataGridViewComboBoxColumn, List<String>> EnumeratedColumns;


            maxWidth = 0;
            for (int ctr = 0; ctr < nResults; ctr++)
            {
                Size sz = TextRenderer.MeasureText(ResultSets[ctr].IATScore.ToString("F4"), System.Drawing.SystemFonts.DefaultFont);
                if (maxWidth < sz.Width)
                    maxWidth = sz.Width;
            }
            col = new DataGridViewColumn();
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            col.ReadOnly = true;
            col.Resizable = DataGridViewTriState.True;
            col.SortMode = DataGridViewColumnSortMode.Automatic;
            col.Width = maxWidth + CellPadding.Horizontal;
            nMaxIATResultWidth = maxWidth + CellPadding.Horizontal;
            GridColumns.Add(col);


            maxWidth = 0;
            for (int ctr = 0; ctr < nResults; ctr++)
            {
                Size sz = TextRenderer.MeasureText(ResultSets[ctr].Timestamp.ToShortDateString() + ResultSets[ctr].Timestamp.ToShortTimeString() + " ", System.Drawing.SystemFonts.DefaultFont);
                if (maxWidth < sz.Width)
                    maxWidth = sz.Width;
            }
            col = new DataGridViewColumn();
            col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
            col.ReadOnly = true;
            col.Resizable = DataGridViewTriState.True;
            col.SortMode = DataGridViewColumnSortMode.Automatic;
            col.Width = maxWidth + CellPadding.Horizontal;
            nMaxIATTimestampWidth = maxWidth + CellPadding.Horizontal;
            GridColumns.Add(col);


            EnumeratedColumns = new Dictionary<DataGridViewComboBoxColumn, List<String>>();

            ctr3 = ctr4 = ctr5 = 0;
            for (int ctr1 = 0; ctr1 < nBeforeSurveyItems; ctr1++)
            {
                widths[ctr1] = new int[nResults];
                for (int ctr2 = 0; ctr2 < nResults; ctr2++)
                {
                    Size szText = TextRenderer.MeasureText(ResultSets[ctr3].BeforeSurveys[ctr4][ctr5].Value, System.Drawing.SystemFonts.DefaultFont);
                    widths[ctr1][ctr2] = szText.Width + CellPadding.Horizontal;
                    if (maxColWidths[ctr1] < widths[ctr1][ctr2])
                        maxColWidths[ctr1] = widths[ctr1][ctr2];
                    col = new DataGridViewColumn();
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    col.ReadOnly = true;
                    col.Resizable = DataGridViewTriState.True;
                    col.SortMode = DataGridViewColumnSortMode.Automatic;
                    GridColumns.Add(col);
                    ctr5++;
                    if (ctr5 > ResultSets[ctr3].AfterSurveys[ctr4].NumItems)
                    {
                        ctr5 = 0;
                        ctr4++;
                        if (ctr4 >= ResultSets.NumResultSets)
                        {
                            ctr4 = 0;
                            ctr3++;
                        }
                    }
                }
            }

            ctr3 = 0;
            ctr4 = 0;
            ctr5 = 0;
            for (int ctr1 = nBeforeSurveyItems; ctr1 < nBeforeSurveyItems + nAfterSurveyItems; ctr1++)
            {
                widths[ctr1] = new int[nResults];
                for (int ctr2 = 0; ctr2 < nResults; ctr2++)
                {
                    Size szText = TextRenderer.MeasureText(ResultSets[ctr3].AfterSurveys[ctr4][ctr5].Value, System.Drawing.SystemFonts.DefaultFont);
                    widths[ctr1][ctr2] = szText.Width + CellPadding.Horizontal;
                    if (maxColWidths[ctr1] < widths[ctr1][ctr2])
                        maxColWidths[ctr1] = widths[ctr1][ctr2];
                    col = new DataGridViewColumn();
                    col.AutoSizeMode = DataGridViewAutoSizeColumnMode.None;
                    col.ReadOnly = true;
                    col.Resizable = DataGridViewTriState.True;
                    col.SortMode = DataGridViewColumnSortMode.Automatic;
                    GridColumns.Add(col);
                    ctr5++;
                    if (ctr5 > ResultSets[ctr3].AfterSurveys[ctr4][ctr5].Value.Length)
                    {
                        ctr5 = 0;
                        ctr4++;
                        if (ctr4 > ResultSets[ctr3].AfterSurveys[ctr4].NumItems)
                        {
                            ctr4 = 0;
                            ctr3++;
                        }
                    }
                }
            }

            for (int ctr = 0; ctr < nResults; ctr++)
            {
                long resultID = ResultSets[ctr].ResultID;
                DataGridViewRow row = new DataGridViewRow();
                DataGridViewTextBoxCell txtCell = new DataGridViewTextBoxCell();
                txtCell.ReadOnly = true;
                txtCell.Value = ResultSets[ctr].IATScore.ToString("F4");
                row.Cells.Add(txtCell);
                txtCell.Tag = new ResultCell(ResultCell.EResultType.iat, ResultSets[ctr].ResultID, ResultSets[ctr].IATScore.ToString("F4"));
                txtCell = new DataGridViewTextBoxCell();
                txtCell.ReadOnly = true;
                txtCell.Value = ResultSets[ctr].Timestamp;
                row.Cells.Add(txtCell);
                txtCell.Tag = new ResultCell(ResultCell.EResultType.timestamp, resultID, ResultSets[ctr].Timestamp.ToShortDateString() + " " + ResultSets[ctr].Timestamp.ToShortTimeString());
                for (int ctr2 = 0; ctr2 < nBeforeSurveyItems; ctr2++)
                {
                    for (ctr3 = 0; ctr3 < ResultSets[ctr].BeforeSurveys[ctr2].NumItems; ctr3++)
                    {
                        txtCell = new DataGridViewTextBoxCell();
                        txtCell.ReadOnly = true;
                        txtCell.Value = ResultSets[ctr].BeforeSurveys[ctr2][ctr3];
                        row.Cells.Add(txtCell);
                        txtCell.Tag = new ResultCell(ResultCell.EResultType.beforeSurvey, resultID, ResultSets[ctr].BeforeSurveys[ctr2][ctr3].Value, ctr2, ctr3);
                    }
                }
                for (int ctr2 = 0; ctr2 < nAfterSurveyItems; ctr2++)
                {
                    for (ctr3 = 0; ctr3 < ResultSets[ctr].AfterSurveys[ctr2].NumItems; ctr3++)
                    {
                        txtCell = new DataGridViewTextBoxCell();
                        txtCell.ReadOnly = true;
                        txtCell.Value = ResultSets[ctr].AfterSurveys[ctr2][ctr3];
                        row.Cells.Add(txtCell);
                        txtCell.Tag = new ResultCell(ResultCell.EResultType.afterSurvey, resultID, ResultSets[ctr].AfterSurveys[ctr2][ctr3].Value, ctr2, ctr3);
                    }
                }
                ResultGrid.Rows.Add(row);
            }

            int maxRowWidth = 0;
            for (int ctr = 0; ctr < maxColWidths.Length; ctr++)
                maxRowWidth += maxColWidths[ctr];
            int adjustableRowWidth = maxRowWidth;
            maxRowWidth += nMaxIATPercentileWidth + nMaxIATResultWidth + nMaxIATTimestampWidth;
            bool hScroll = false, vScroll = false;
            if (maxRowWidth >= ResultGrid.ClientRectangle.Width)
                hScroll = true;
            if (nResults * (CellPadding.Vertical + System.Drawing.SystemFonts.DefaultFont.Height) >= DataPanel.Height)
                vScroll = true;
            if (vScroll && hScroll)
                ResultGrid.ScrollBars = ScrollBars.Both;
            else if (vScroll)
                ResultGrid.ScrollBars = ScrollBars.Vertical;
            else if (hScroll)
                ResultGrid.ScrollBars = ScrollBars.Horizontal;
            else
                ResultGrid.ScrollBars = ScrollBars.None;
            int[] adjustedCellWidths = new int[nBeforeSurveyItems + nAfterSurveyItems];

            double[] SDs = new double[widths.Length];
            for (int ctr = 0; ctr < widths.Length; ctr++)
                SDs[ctr] = SD(widths[ctr]);

            double[] means = new double[widths.Length];
            for (int ctr1 = 0; ctr1 < widths.Length; ctr1++)
            {
                means[ctr1] = 0;
                for (int ctr2 = 0; ctr2 < widths[ctr1].Length; ctr2++)
                    means[ctr1] += (double)widths[ctr1][ctr2];
                means[ctr1] /= (double)widths[ctr1].Length;
            }

            for (int ctr = 0; ctr < nBeforeSurveyItems + nAfterSurveyItems; ctr++)
            {
                adjustedCellWidths[ctr] = -1;
                if (maxColWidths[ctr] > (2 * SDs[ctr]) + means[ctr])
                    adjustedCellWidths[ctr] = (int)((2 * SDs[ctr]) + means[ctr]);
                else
                    adjustedCellWidths[ctr] = maxColWidths[ctr];
            }
        }

        private double SD(int[] vals)
        {
            double mean = 0;
            for (int ctr = 0; ctr < vals.Length; ctr++)
                mean += (double)vals[ctr];
            mean /= vals.Length;

            double sum = 0;
            for (int ctr = 0; ctr < vals.Length; ctr++)
                sum += Math.Pow(mean - vals[ctr], 2);

            return sum / (double)(vals.Length - 1);
        }

        public void Initialize(IResultData resultSet, ResultSetDescriptor descriptor)
        {
            _ResultSets = resultSet;
            _Descriptor = descriptor;
            int LineHeight = System.Drawing.SystemFonts.DefaultFont.Height + CellPadding.Vertical;
            DataPanel = new Panel();
            DataPanel.Size = new Size(4 * this.Width / 5, this.Height);
            DataPanel.Location = new Point(this.Width / 5, 0);
            ConstructSurveyDescription(this.Width / 5);
            PopulateResultGrid();
            ResultGrid.Size = DataPanel.ClientRectangle.Size;
            ResultGrid.Location = new Point(0, 0);
            DataPanel.Controls.Add(ResultGrid);
        }

        private void ResultGrid_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            ResultCell rCell = (ResultCell)ResultGrid.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag;
            SurveyScroll.Value = SurveyItemYOffsets[GetSurveyItemNum(rCell.SurveyNum, rCell.ItemNum)];
        }

        private void SurveyScroll_ValueChanged(object sender, EventArgs e)
        {
            SurveyDescriptorPanel.Invalidate();
        }

        private int GetSurveyItemNum(int surveyNum, int itemNum)
        {
            int n = -1;
            for (int ctr1 = 0; ctr1 < Descriptor.BeforeSurveys.Count; ctr1++)
            {
                for (int ctr2 = 0; ctr2 < Descriptor.BeforeSurveys[ctr1].SurveyItems.Length; ctr2++)
                {
                    n++;
                    if ((ctr1 == surveyNum) && (ctr2 == itemNum))
                        return n;
                }
            }
            for (int ctr1 = 0; ctr1 < Descriptor.AfterSurveys.Count; ctr1++)
            {
                for (int ctr2 = 0; ctr2 < Descriptor.AfterSurveys[ctr1].SurveyItems.Length; ctr1++)
                {
                    n++;
                    if ((ctr1 == surveyNum) && (ctr2 == itemNum))
                        return n;
                }
            }
            return n;
        }


        private void SurveyDescriptorPanel_Paint(object sender, PaintEventArgs e)
        {
            SuspendLayout();

            ResumeLayout(false);
        }

        private void ConstructSurveyDescription(int nWidth)
        {
            SurveyDescriptorPanel = new Panel();
            SurveyDescriptorPanel.Location = new Point(0, 0);
            SurveyDescriptorPanel.Size = new Size(this.Width / 5, this.ClientRectangle.Height);
            VScrollBar scrollBar = new VScrollBar();
            scrollBar.Height = SurveyDescriptorPanel.Height;
            SurveyDescriptorPanel.Controls.Add(scrollBar);
            List<CSurveyOutline> surveyOutlineList = new List<CSurveyOutline>();
            List<Label> AllSurveyLabels = new List<Label>();
            for (int ctr = 0; ctr < Descriptor.BeforeSurveys.Count; ctr++)
                surveyOutlineList.Add(new CSurveyOutline(Descriptor.BeforeSurveys[ctr]));
            for (int ctr = 0; ctr < Descriptor.AfterSurveys.Count; ctr++)
                surveyOutlineList.Add(new CSurveyOutline(Descriptor.AfterSurveys[ctr]));
            int yOffset = SurveyDescriptionPadding.Top;
            for (int ctr = 0; ctr < surveyOutlineList.Count; ctr++)
            {
                surveyOutlineList[ctr].CalcOutline(SurveyDescriptionPadding.Left, nWidth - scrollBar.Width - SurveyDescriptionPadding.Horizontal, new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 10F), CSurveyOutline.EResponseLabel.alphabetical);
                List<Label> labelList = surveyOutlineList[ctr].CaptionLabels;
                foreach (Label l in labelList)
                    l.Location = new Point(l.Location.X, l.Location.Y + yOffset);
                AllSurveyLabels.AddRange(labelList);
                labelList = surveyOutlineList[ctr].GetSurveyLabels();
                foreach (Label l in labelList)
                    l.Location = new Point(l.Location.X, l.Location.Y + yOffset);
                AllSurveyLabels.AddRange(labelList);
                yOffset += labelList.Last().Bottom + SurveyDescriptionPadding.Vertical;
                if (ctr < Descriptor.BeforeSurveys.Count)
                    for (int ctr2 = 0; ctr2 < Descriptor.BeforeSurveys[ctr].SurveyItems.Length; ctr2++)
                        SurveyItemYOffsets.Add(surveyOutlineList[ctr].SurveyItemLabels[Descriptor.BeforeSurveys[ctr].SurveyItems[ctr2]].First().Top);
                else
                    for (int ctr2 = 0; ctr2 < Descriptor.AfterSurveys[ctr - Descriptor.BeforeSurveys.Count].SurveyItems.Length; ctr2++)
                        SurveyItemYOffsets.Add(surveyOutlineList[ctr].SurveyItemLabels[Descriptor.AfterSurveys[ctr - Descriptor.BeforeSurveys.Count].SurveyItems[ctr2]].First().Top);
            }
            SurveyDescriptorPanel.Controls.Remove(scrollBar);
            foreach (Label l in AllSurveyLabels)
                SurveyDescriptorPanel.Controls.Add(l);
            SurveyDescriptorPanel.Height = SurveyDescriptionPadding.Bottom + AllSurveyLabels.Last().Bottom + SurveyItemDescriptionPadding.Bottom;
            SurveyDescriptorPanel.AutoScroll = true;
        }

        public int DrawString(String text, Font font, Point ptDraw, int outdent, int width, Graphics g)
        {
            String str = String.Empty;
            Size sz = new Size(0, 0);
            PointF ptFDraw = new PointF(0, 0);
            int n = 0;
            int nLines = 0;
            while (sz.Width < width)
            {
                str += text[n++];
                sz = TextRenderer.MeasureText(str, font);
                if (n >= text.Length)
                    break;
            }
            ptFDraw.X = ptDraw.X;
            ptFDraw.Y = ptDraw.Y;
            int m = n;
            if (n < text.Length)
            {
                while (!Char.IsWhiteSpace(str[--m])) ;
                while (Char.IsWhiteSpace(str[--m])) ;
                str = str.Substring(0, m + 1);
            }
            g.DrawString(str, font, Brushes.Black, ptFDraw);
            if (n >= text.Length)
                return System.Drawing.SystemFonts.DefaultFont.Height;
            width -= outdent;
            ptFDraw.X += outdent;
            nLines++;
            while (n < text.Length)
            {
                ptFDraw.Y += System.Drawing.SystemFonts.DefaultFont.Height;
                str = String.Empty;
                sz = new Size(0, 0);
                while (sz.Width < width)
                {
                    str += text[n++];
                    sz = TextRenderer.MeasureText(str, font);
                    if (n >= text.Length)
                        break;
                }
                m = n;
                if (n < text.Length)
                {
                    while (!Char.IsWhiteSpace(str[--m])) ;
                    while (Char.IsWhiteSpace(str[--m])) ;
                    if (m < n)
                        break;
                    str = str.Substring(0, m + 1 - n);
                }
                g.DrawString(str, font, Brushes.Black, ptFDraw);
                nLines++;
            }
            return System.Drawing.SystemFonts.DefaultFont.Height * nLines;
        }

        public int MeasureOutdentedString(String text, Font font, int outdent, int width)
        {
            String str = String.Empty;
            Size sz = new Size(0, 0);
            int n = 0;
            int nLines = 0;
            while (sz.Width < width)
            {
                str += text[n++];
                sz = TextRenderer.MeasureText(str, font);
                if (n >= text.Length)
                    break;
            }
            int m = n;
            if (n < text.Length)
            {
                while (!Char.IsWhiteSpace(str[--m])) ;
                while (Char.IsWhiteSpace(str[--m])) ;
                str = str.Substring(0, m + 1);
            }
            if (n >= text.Length)
                return System.Drawing.SystemFonts.DefaultFont.Height;
            width -= outdent;
            nLines++;
            while (n < text.Length)
            {
                str = String.Empty;
                sz = new Size(0, 0);
                while (sz.Width < width)
                {
                    str += text[n++];
                    sz = TextRenderer.MeasureText(str, font);
                    if (n >= text.Length)
                        break;
                }
                m = n;
                if (n < text.Length)
                {
                    while (!Char.IsWhiteSpace(str[--m])) ;
                    while (Char.IsWhiteSpace(str[--m])) ;
                    if (m < n)
                        break;
                    str = str.Substring(0, m + 1 - n);
                }
                nLines++;
            }
            return System.Drawing.SystemFonts.DefaultFont.Height * nLines;
        }
    }
}
