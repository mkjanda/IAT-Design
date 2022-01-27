using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient.ResultData {

    [Serializable]
    public class SurveyItem
    {
        [XmlElement(ElementName = "Text", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public String Text { get; set; }
        [XmlElement(ElementName = "Response", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Type = typeof(Response))]
        public Response Response { get; set; }

        private Survey ParentSurvey = null;
        private int ItemNum = -1;
        private Panel PreviewPanel = null;
        private Padding PreviewPadding = new Padding(30, 10, 30, 5);
        private CResponseObject ResponseObj = null;

        public SurveyItem(Survey parentSurvey, int itemNum)
        {
            ParentSurvey = parentSurvey;
            ItemNum = itemNum;
        }

        public SurveyItem() { }

        public String GetSurveyName()
        {
            if (ParentSurvey == null)
                return String.Empty;
            return ParentSurvey.Name;
        }

        public int GetItemNum()
        {
            return ItemNum;
        }

        public void PostSerialize(Survey parent, int itemNum)
        {
            ParentSurvey = parent;
            ItemNum = itemNum;
            Response.PostSerialize(this);
        }

        public String GetDescription()
        {
            if (Response.ResponseType == ResponseType.None)
                return String.Empty;
            return String.Format("{0}\r\n{1}", Text, Response.GetResponseDesc());
        }

        public virtual Panel GeneratePreviewPanel(int nWidth)
        {
            if (PreviewPanel != null)
            {
                //  ClearAnswers();
                return PreviewPanel;
            }
            PreviewPanel = new Panel();
            PreviewPanel.BackColor = System.Drawing.Color.White;
            TextBox tb = new TextBox();
            Font questFont = null;
            if (Response.ResponseType == ResponseType.None)
                questFont = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 11F, FontStyle.Italic);
            else
                questFont = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 11F);
            tb.Multiline = true;
            tb.Size = TextRenderer.MeasureText(Text, questFont, new Size(nWidth - PreviewPadding.Horizontal, 0), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            tb.Location = new Point(PreviewPadding.Left, PreviewPadding.Top);
            PreviewPanel.Controls.Add(tb);
            tb.Text = Text;
            tb.Font = questFont;
            tb.ForeColor = (Response.ResponseType == ResponseType.None) ? System.Drawing.Color.ForestGreen : System.Drawing.Color.Black;
            tb.ReadOnly = true;
            tb.BackColor = System.Drawing.Color.White;
            tb.BorderStyle = BorderStyle.None;
            PreviewPanel.Size = new Size(nWidth, PreviewPadding.Vertical + tb.Height);
            if (Response.ResponseType != ResponseType.None)
            {
                ResponseObj = CResponseObject.CreateFromResultData(Response);
                Panel respPanel = ResponseObj.GenerateResponseObjectPanel(System.Drawing.Color.White, System.Drawing.Color.Black, System.Drawing.SystemFonts.DefaultFont.FontFamily.ToString(), 10F, nWidth - PreviewPadding.Horizontal);
                respPanel.Location = new Point(PreviewPadding.Left, tb.Bottom + PreviewPadding.Top);
                PreviewPanel.Controls.Add(respPanel);
                PreviewPanel.Size = new Size(nWidth, tb.Bottom + respPanel.Height + PreviewPadding.Vertical);
            }
            return PreviewPanel;
        }

        public void SetDisplayResponse(ISurveyItemResponse resp)
        {
            ResponseObj.Response = resp;
        }
    }

    [Serializable]
    public enum ResponseType { None, Boolean, Likert, Date, Multiple, WeightedMultiple, RegEx, MultiBoolean, FixedDig, BoundedNum, BoundedLength, Picture };

    [Serializable]
    [XmlInclude(typeof(BoundedLength))]
    [XmlInclude(typeof(BoundedNumber))]
    [XmlInclude(typeof(FixedDigit))]
    [XmlInclude(typeof(RegEx))]
    [XmlInclude(typeof(WeightedMultiple))]
    [XmlInclude(typeof(MultiBoolean))]
    [XmlInclude(typeof(Date))]
    [XmlInclude(typeof(Likert))]
    [XmlInclude(typeof(Multiple))]
    [XmlInclude(typeof(Boolean))]
}