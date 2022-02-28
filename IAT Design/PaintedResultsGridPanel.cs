using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class PaintedResultsGridPanel : Panel
    {
        private ResultData.ResultData ResultData;
        private Padding CellPadding = new Padding(3, 2, 3, 2);
        private Font DataFont = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 10);

        public PaintedResultsGridPanel()
        {
            this.Paint += new PaintEventHandler(PaintedResultsGridPanel_Paint);
        }

        private void PaintedResultsGridPanel_Paint(object sender, PaintEventArgs e)
        {
            throw new NotImplementedException();
        }

        private void Scale()
        {
            int widthWithoutBounded = 0;
            int widthWithBounded = 0;
            int maxWidth = 0;
            for (int ctr = 0; ctr < ResultData.IATConfiguration.BeforeSurveys.Count; ctr++)
            {
                int[][] respWidths = new int[ResultData.IATResults.NumResultSets][];
                for (int ctr2 = 0; ctr2 < ResultData.IATResults.NumResultSets; ctr2++)
                {
                    respWidths[ctr2] = new int[ResultData.IATResults[ctr2].BeforeSurveys[ctr].NumItems];
                    //        respWithoutBoundedWidths = new int[ResultData.IATResults[ctr2].BeforeSurveys[ctr].NumSurveyResults];
                    for (int ctr3 = 0; ctr3 < ResultData.IATConfiguration.BeforeSurveys[ctr].NumItems; ctr3++)
                        respWidths[ctr2][ctr3] = TextRenderer.MeasureText(ResultData.IATResults[ctr2].BeforeSurveys[ctr][ctr3].Value, DataFont).Width + CellPadding.Horizontal;
                }
                maxWidth = 0;
                for (int ctr2 = 0; ctr < ResultData.IATConfiguration.BeforeSurveys[ctr].NumItems; ctr2++)
                {
                    for (int ctr3 = 0; ctr3 < ResultData.IATResults.NumResultSets; ctr3++)
                    {
                        if (respWidths[ctr3][ctr2] > maxWidth)
                            maxWidth = respWidths[ctr3][ctr2];
                    }
                    if (ResultData.IATConfiguration.BeforeSurveys[ctr].ResponseTypes[ctr2] != CResponse.EResponseType.BoundedLength)
                        widthWithoutBounded += maxWidth;
                    widthWithBounded += maxWidth;
                    maxWidth = 0;
                }
            }
            maxWidth = 0;
            int txtWidth = 0;
            for (int ctr = 0; ctr < ResultData.IATResults.NumResultSets; ctr++)
            {
                txtWidth = TextRenderer.MeasureText(ResultData.IATResults[ctr].IATScore.ToString("F4"), DataFont).Width + CellPadding.Horizontal;
                if (maxWidth < txtWidth)
                    maxWidth = txtWidth;
            }
            widthWithBounded += maxWidth;
            widthWithoutBounded += maxWidth;
            for (int ctr = 0; ctr < ResultData.IATConfiguration.AfterSurveys.Count; ctr++)
            {
                int[][] respWidths = new int[ResultData.IATResults.NumResultSets][];
                for (int ctr2 = 0; ctr2 < ResultData.IATResults.NumResultSets; ctr2++)
                {
                    respWidths[ctr2] = new int[ResultData.IATResults[ctr2].AfterSurveys[ctr].NumItems];
                    //      respWithoutBoundedWidths = new int[ResultData.IATResults[ctr2].AfterSurveys[ctr].NumSurveyResults];
                    for (int ctr3 = 0; ctr3 < ResultData.IATConfiguration.AfterSurveys[ctr].NumItems; ctr3++)
                        respWidths[ctr2][ctr3] = TextRenderer.MeasureText(ResultData.IATResults[ctr2].AfterSurveys[ctr][ctr3].Value, DataFont).Width + CellPadding.Horizontal;
                }
                maxWidth = 0;
                for (int ctr2 = 0; ctr < ResultData.IATConfiguration.AfterSurveys[ctr].NumItems; ctr2++)
                {
                    for (int ctr3 = 0; ctr3 < ResultData.IATResults.NumResultSets; ctr3++)
                    {
                        if (respWidths[ctr3][ctr2] > maxWidth)
                            maxWidth = respWidths[ctr3][ctr2];
                    }
                    if (ResultData.IATConfiguration.AfterSurveys[ctr].ResponseTypes[ctr2] != CResponse.EResponseType.BoundedLength)
                        widthWithoutBounded += maxWidth;
                    widthWithBounded += maxWidth;
                    maxWidth = 0;
                }
            }


        }

        void Initialize(ResultData.ResultData resultData)
        {
            ResultData = resultData;
            Scale();
        }

    }
}
