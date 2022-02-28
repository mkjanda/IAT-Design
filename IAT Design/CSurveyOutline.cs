using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace IATClient
{
    class CSurveyOutline
    {
        private Survey Survey;
        private List<Label> _CaptionLabels = new List<Label>();
        private Dictionary<SurveyItem, List<Label>> _SurveyItemLabels = new Dictionary<SurveyItem, List<Label>>();
        public enum EResponseLabel { numeric, alphabetical };
        private EResponseLabel ResponseLabel;
        private int Indent, LineSpacing, XOffset, Width;
        private Padding CaptionPadding, QuestionPadding, ResponsePadding;
        private Font Font;

        public Dictionary<SurveyItem, List<Label>> SurveyItemLabels
        {
            get
            {
                return _SurveyItemLabels;
            }
        }

        public List<Label> CaptionLabels
        {
            get
            {
                return _CaptionLabels;
            }
        }

        public CSurveyOutline(Survey survey)
        {
            this.Survey = survey;
            _SurveyItemLabels = null;
        }

        public List<Label> GetSurveyLabels()
        {
            List<Label> labels = new List<Label>();
            labels.AddRange(CaptionLabels);
            for (int ctr = 0; ctr < this.Survey.SurveyItems.Length; ctr++)
                labels.AddRange(SurveyItemLabels[this.Survey.SurveyItems[ctr]]);
            return labels;
        }

        public void CalcOutline(int xOffset, int width, Font font, EResponseLabel responseLabel)
        {
            int indent = font.Height * 5;
            int lineSpacing = font.Height >> 4;
            Padding captionPadding = new Padding(0, 0, 0, (font.Height / 3) + lineSpacing);
            Padding questionPadding = new Padding(0, font.Height >> 2, 0, (font.Height >> 2) + lineSpacing);
            Padding responsePadding = new Padding(0, font.Height >> 3, 0, (font.Height >> 3) + lineSpacing);
            CalcOutline(xOffset, width, font, responseLabel, indent, lineSpacing, captionPadding, questionPadding, responsePadding);
        }

        public void CalcOutline(int xOffset, int width, Font font, EResponseLabel responseLabel, int indent, int lineSpacing, Padding captionPadding, Padding questionPadding, Padding responsePadding)
        {
            XOffset = xOffset;
            Width = width;
            this.Font = font;
            ResponseLabel = responseLabel;
            Indent = indent;
            LineSpacing = lineSpacing;
            CaptionPadding = captionPadding;
            QuestionPadding = questionPadding;
            ResponsePadding = responsePadding;
            int yOffset = 0;

            _CaptionLabels = GetNameLabel();
            yOffset = (CaptionLabels.Count * ((new Font(font, FontStyle.Bold)).Height) + (lineSpacing << 1)) + captionPadding.Bottom;

            int nStart = this.Survey.HasCaption ? 1 : 0;
            for (int ctr = nStart; ctr < this.Survey.NumItems; ctr++)
            {
                yOffset += questionPadding.Top;
                List<Label> itemLabels = new List<Label>();
                itemLabels.AddRange(GetSurveyTextLabel(ctr, yOffset));
                yOffset = itemLabels.Last().Bottom;
                itemLabels.AddRange(GetResponseLabels(ctr, yOffset));
                yOffset = itemLabels.Last().Bottom + questionPadding.Bottom;
                SurveyItemLabels[this.Survey.SurveyItems[ctr]] = itemLabels;
            }
        }

        private List<Label> CreateLabelList(String str, Font f, int xOffset, int indent, int lineSpacing, int yOffset)
        {
            String strCopy = str;
            Size sz = TextRenderer.MeasureText(str, f);
            List<Label> result = new List<Label>();
            bool bIsFirstItr = true;
            while (strCopy != String.Empty)
            {
                int ndx = 0;
                String line = String.Empty;
                Size lineSize = new Size(0, f.Height);
                Label l;
                while (lineSize.Width < Width - xOffset - (bIsFirstItr ? 0 : indent))
                    lineSize = TextRenderer.MeasureText(str.Substring(0, ++ndx), f);
                try
                {
                    while (!Char.IsWhiteSpace(strCopy[--ndx])) ;
                    line = strCopy.Substring(0, ndx).Trim();
                }
                catch (IndexOutOfRangeException)
                {
                    line = strCopy;
                }
                finally
                {
                    l = new Label();
                    l.Text = line;
                    l.Font = Font;
                    if (bIsFirstItr)
                        l.Location = new Point(xOffset, yOffset);
                    else
                        l.Location = new Point(xOffset + indent, yOffset);
                    l.Size = TextRenderer.MeasureText(l.Text, Font);
                }
                if (line.Length < strCopy.Length)
                {
                    strCopy = strCopy.Substring(ndx + 1);
                    if (strCopy.Trim() == String.Empty)
                        strCopy = String.Empty;
                }
                else
                    strCopy = String.Empty;
                result.Add(l);
                yOffset += l.Height + lineSpacing;
            }
            return result;
        }

        private List<Label> GetNameLabel()
        {
            String captionString;
            if (Survey.HasCaption)
                captionString = this.Survey.SurveyItems[0].Text;
            else
                captionString = this.Survey.Name;

            Font captionFont = new Font(this.Font, FontStyle.Bold);
            return CreateLabelList(captionString, captionFont, 0, 0, LineSpacing * 2, 0);
        }

        private List<Label> GetSurveyTextLabel(int ndx, int yOffset)
        {
            return CreateLabelList(this.Survey.SurveyItems[ndx].Text, this.Font, 0, Indent, LineSpacing, yOffset);
        }

        private List<Label> GetResponseLabels(int nSurveyItem, int yOffset)
        {
            List<Label> result = new List<Label>();
            int nResps = this.Survey.SurveyItems[nSurveyItem].Response.GetNumDescriptionSubItems();
            for (int ctr = 0; ctr < nResps; ctr++)
            {
                yOffset += ResponsePadding.Top;
                List<Label> respLabels = CreateLabelList(this.Survey.SurveyItems[nSurveyItem].Response.GetDescriptionSubItem(ctr), this.Font, Indent, Indent, LineSpacing, yOffset);
                if (nResps > 1)
                {
                    String preResp = ((ResponseLabel == EResponseLabel.alphabetical) ? ((char)('a' + ctr)).ToString() : (ctr + 1).ToString()) + ")";
                    respLabels.First().Text = preResp + respLabels.First().Text;
                    respLabels.First().Location = new Point(respLabels.First().Location.X - TextRenderer.MeasureText(preResp, this.Font).Width, respLabels.First().Location.Y);
                }
                yOffset += (LineSpacing + this.Font.Height) * respLabels.Count + ResponsePadding.Bottom;
            }
            return result;
        }
    }
}
