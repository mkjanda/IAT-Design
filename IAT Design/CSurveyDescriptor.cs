using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient.IATSurveyFile
{
    [Serializable]
    public class DateEntry
    {
        [XmlElement(ElementName = "Year", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Year { get; set; }
        [XmlElement(ElementName = "Month", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Month { get; set; }
        [XmlElement(ElementName = "Day", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Day { get; set; }

        public DateEntry()
        {
            Year = Month = Day = -1;
        }

        public DateEntry(DateTime date)
        {
            Year = date.Year;
            Month = date.Month;
            Day = date.Day;
        }
    }

    [Serializable]
    public class Color
    {
        [XmlElement(ElementName = "Red", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Red { get; set; }
        [XmlElement(ElementName = "Blue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Blue { get; set; }
        [XmlElement(ElementName = "Green", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Green { get; set; }

        public Color() { }

        public System.Drawing.Color ToSystemColor()
        {
            return System.Drawing.Color.FromArgb(Red, Green, Blue);
        }
    }

    [Serializable]
    public class Survey
    {
        [XmlElement(ElementName = "Name", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String Name { get; set; }
        [XmlElement(ElementName = "Timeout", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Timeout { get; set; }
        [XmlElement(ElementName = "Caption", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Type = typeof(Caption))]
        public Caption Caption { get; set; }
        [XmlElement(ElementName = "SurveyItems", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SurveyItem[] SurveyItems { get; set; }
        [XmlAttribute(AttributeName = "NumItems", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int NumItems { get; set; }
        [XmlAttribute(AttributeName = "HasCaption", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool HasCaption { get; set; }

        private Panel PreviewPanel = null;
        private SurveyItem[] _Questions = null;
        public Survey() { }

        public SurveyItem[] Questions
        {
            get
            {
                if ((_Questions == null) && (NumQuestions > 0))
                {
                    _Questions = new SurveyItem[NumQuestions];
                    int ndx = 0;
                    for (int ctr = 0; ctr < SurveyItems.Length; ctr++)
                        if ((SurveyItems[ctr].Response.ResponseType != ResponseType.None) && (SurveyItems[ctr].Response.ResponseType != ResponseType.Picture))
                            _Questions[ndx++] = SurveyItems[ctr];
                }
                else if (NumQuestions == 0)
                    return new SurveyItem[0];
                return _Questions;
            }
        }

        public Survey(String name)
        {
            Name = name;
        }

        public int NumQuestions
        {
            get
            {
                int nQuests = 0;
                if (SurveyItems == null)
                    return 0;
                for (int ctr = 0; ctr < SurveyItems.Length; ctr++)
                    if ((SurveyItems[ctr].Response.ResponseType != ResponseType.None) && (SurveyItems[ctr].Response.ResponseType != ResponseType.Picture))
                        nQuests++;
                return nQuests;
            }
        }

        public void SetCaption(CSurveyItem si)
        {
            CSurveyCaption c = (CSurveyCaption)si;
            Caption = new Caption();
            Caption.BackColor = new Color();
            Caption.BackColor.Red = c.BackColor.R;
            Caption.BackColor.Green = c.BackColor.G;
            Caption.BackColor.Blue = c.BackColor.B;
            Caption.FontColor = new Color();
            Caption.FontColor.Red = c.FontColor.R;
            Caption.FontColor.Green = c.FontColor.G;
            Caption.FontColor.Blue = c.FontColor.B;
            Caption.BorderColor = new Color();
            Caption.BorderColor.Red = c.BorderColor.R;
            Caption.BorderColor.Green = c.BorderColor.G;
            Caption.BorderColor.Blue = c.BorderColor.B;
            Caption.Text = c.Text;
            Caption.FontSize = c.FontSize;
            Caption.BorderWidth = c.BorderWidth;
            Caption.Text = c.Text;
        }

        public void SetItems(CSurveyItem[] surveyItems)
        {
            SurveyItems = new SurveyItem[surveyItems.Length];
            for (int ctr = 0; ctr < surveyItems.Length; ctr++)
            {
                SurveyItems[ctr] = new SurveyItem(this, ctr);
                SurveyItems[ctr].Text = surveyItems[ctr].Text;
                SurveyItems[ctr].Response = surveyItems[ctr].Response.GenerateSerializableResponse(SurveyItems[ctr]);
            }
        }


        public void PostSerialize()
        {
            int itemCtr = 1;
            for (int ctr = 0; ctr < SurveyItems.Length; ctr++)
            {
                if (SurveyItems[ctr].Response.ResponseType != ResponseType.None)
                    SurveyItems[ctr].PostSerialize(this, itemCtr++);
                else
                    SurveyItems[ctr].PostSerialize(this, -1);
            }
        }

        public Panel GeneratePreview(int width)
        {
            PreviewPanel = new Panel();
            PreviewPanel.BackColor = System.Drawing.Color.White;
            List<Panel> itemPreviewPanels = new List<Panel>();
            if (HasCaption)
                itemPreviewPanels.Add(Caption.GeneratePreviewPanel(width));
            for (int ctr = 0; ctr < SurveyItems.Length; ctr++)
                itemPreviewPanels.Add(SurveyItems[ctr].GeneratePreviewPanel(width));
            int height = 0;
            for (int ctr = 0; ctr < itemPreviewPanels.Count; ctr++)
            {
                itemPreviewPanels[ctr].Location = new Point(0, height);
                height += itemPreviewPanels[ctr].Height;
                PreviewPanel.Controls.Add(itemPreviewPanels[ctr]);
            }
            PreviewPanel.Size = new Size(width, height);
            return PreviewPanel;
        }

        public void DisplayValues(ISurveyResponse responses)
        {
            int respCtr = 0;
            for (int ctr = 0; ctr < SurveyItems.Length; ctr++)
                if (SurveyItems[ctr].Response.ResponseType != ResponseType.None)
                    SurveyItems[ctr].SetDisplayResponse(responses[respCtr++]);
        }
    }

    [Serializable]
    public class Caption : SurveyItem
    {
        [XmlElement(ElementName = "FontColor", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(Color))]
        public Color FontColor { get; set; }
        [XmlElement(ElementName = "BackColor", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(Color))]
        public Color BackColor { get; set; }
        [XmlElement(ElementName = "BorderColor", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(Color))]
        public Color BorderColor { get; set; }
        [XmlElement(ElementName = "BorderWidth", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int BorderWidth { get; set; }
        [XmlElement(ElementName = "FontSize", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int FontSize { get; set; }

        private Survey ParentItem = null;
        private int ItemNum = -1;
        private Panel PreviewPanel = null;

        public Caption(Survey parent, int itemNum) 
        {
            ParentItem = parent;
            ItemNum = itemNum;
            Response = null;
        }

        public Caption() 
        {
            Response = null;
        }

        public override Panel GeneratePreviewPanel(int nWidth)
        {
            Font f = new Font(FontFamily.GenericSerif, (float)FontSize, FontStyle.Bold);
            Bitmap bmp = new Bitmap(nWidth, (int)(f.Height * 1.5) + BorderWidth, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            SolidBrush br = new SolidBrush(BackColor.ToSystemColor());
            g.FillRectangle(br, new Rectangle(0, 0, nWidth, (int)(f.Height * 1.5)));
            br.Dispose();
            br = new SolidBrush(FontColor.ToSystemColor());
            SizeF sz = g.MeasureString(Text, f);
            g.DrawString(Text, f, br, new PointF((int)(nWidth - sz.Width) >> 1, .25F * f.Height));
            br.Dispose();
            br = new SolidBrush(BorderColor.ToSystemColor());
            g.FillRectangle(br, new Rectangle(0, (int)(1.5 * f.Height), nWidth, BorderWidth));
            br.Dispose();
            g.Dispose();
            PreviewPanel = new Panel();
            PreviewPanel.BackgroundImage = bmp;
            PreviewPanel.Size = bmp.Size;
            return PreviewPanel;
        }

    }

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
    [XmlInclude(typeof(BoundedNum))]
    [XmlInclude(typeof(FixedDig))]
    [XmlInclude(typeof(RegEx))]
    [XmlInclude(typeof(WeightedMultiple))]
    [XmlInclude(typeof(MultiBoolean))]
    [XmlInclude(typeof(Date))]
    [XmlInclude(typeof(Likert))]
    [XmlInclude(typeof(Multiple))]
    [XmlInclude(typeof(Boolean))]
    public class Response
    {
        [XmlAttribute(AttributeName = "ResponseType", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public ResponseType ResponseType { get; set; }

        protected SurveyItem ParentItem;


        public Response(SurveyItem parentItem)
        {
            ParentItem = parentItem;
            ResponseType = ResponseType.None;
        }
        
        public Response()
        {}

        public String GetSurveyName()
        {
            return ParentItem.GetSurveyName();
        }

        public int GetItemNum()
        {
            return ParentItem.GetItemNum();
        }

        public virtual void PostSerialize(SurveyItem si)
        {
            ParentItem = si;
        }

        public virtual String GetResponseDesc()
        {
            return String.Empty;
        }

        public virtual int GetNumDescriptionSubItems()
        {
            throw new NotImplementedException();
        }

        public virtual String GetDescriptionSubItem(int ndx)
        {
            throw new NotImplementedException();
        }
    }

    [Serializable]
    public class Multiple : Response
    {
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem(ElementName = "Text", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String[] Choices { get; set; }

        public Multiple() {}
        public Multiple(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.Multiple;
        }

        public int GetNumStatements()
        {
            return Choices.Length;
        }

        public string GetStatement(int ndx)
        {
            return Choices[ndx];
        }

        public override String GetResponseDesc()
        {
            String desc = String.Empty;
            for (int ctr = 0; ctr < Choices.Length; ctr++)
                desc += String.Format("\t{0}: {1}\r\n", ctr + 1, Choices[ctr]);
            return desc;
        }

        public override int GetNumDescriptionSubItems()
        {
            return Choices.Length;
        }

        public override String GetDescriptionSubItem(int ndx)
        {
            return Choices[ndx];
        }
    }

    [Serializable]
    public class WeightedMultiple : Response
    {
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem(ElementName = "Choice", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(WeightedChoice))]
        public WeightedChoice[] Choices { get; set; }

        public WeightedMultiple(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.WeightedMultiple;
        }

        public WeightedMultiple() { }

        public int GetNumStatements()
        {
            return Choices.Length;
        }

        public String GetChoice(int choiceNdx)
        {
            return Choices[choiceNdx].Text;
        }

        public int GetChoiceWeight(int choiceNdx)
        {
            return Choices[choiceNdx].Weight;
        }

        public override String GetResponseDesc()
        {
            String desc = String.Empty;
            for (int ctr = 0; ctr < Choices.Length; ctr++)
                desc += String.Format("\t{0}: {1}\r\n", Choices[ctr].Weight, Choices[ctr].Text);
            return desc;
        }

        public override int GetNumDescriptionSubItems()
        {
            return Choices.Length;
        }

        public override String GetDescriptionSubItem(int ndx)
        {
            return Choices[ndx].Text;
        }
    }

    [Serializable]
    public class RegEx : Response
    {
        [XmlElement(ElementName = "RegEx", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String RegularExpression { get; set; }
        
        public RegEx(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.RegEx;
        }

        public RegEx() { }

        public String GetRegEx()
        {
            return RegularExpression;
        }

        public override String GetResponseDesc()
        {
            return String.Format("\tText that matches the regular expression \"{0}\"\r\n", RegularExpression);
        }
    }

    [Serializable]
    public class FixedDig : Response
    {
        [XmlElement(ElementName = "NumDigs", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int NumDigs { get; set; }

        public FixedDig(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.FixedDig;
        }

        public FixedDig() { }

        public int GetNumDigits()
        {
            return NumDigs;
        }

        public override String GetResponseDesc()
        {
            return String.Format("\tA response of {0} digits.\r\n", NumDigs);
        }

        public override int GetNumDescriptionSubItems()
        {
            return 1;
        }
    }

    [Serializable]
    public class Likert : Response
    {
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem(ElementName = "Text", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public String[] Choices { get; set; }

        [XmlAttribute(AttributeName = "ReverseScored", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool ReverseScored { get; set; }

        public Likert(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.Likert;
        }

        public Likert() { }

        public String GetStatement(int ndx)
        {
            return Choices[ndx];
        }

        public bool IsReverseScored()
        {
            return ReverseScored;
        }

        public void SetStatements(List<String> statements)
        {
            Choices = new String[statements.Count];
            for (int ctr = 0; ctr < statements.Count; ctr++)
                Choices[ctr] = statements[ctr];
        }

        public int GetNumStatements()
        {
            return Choices.Length;
        }

        public override String GetResponseDesc()
        {
            if (ReverseScored)
                return String.Format("\tReverse scored Likert item with response range 1 to {0}.  (Answers have already been reversed.)\r\n", Choices.Length);
            else
                return String.Format("\tLikert item with response range 1 to {0}\r\n", Choices.Length);
        }

        public override int GetNumDescriptionSubItems()
        {
            return Choices.Length;
        }
    }

    [Serializable]
    public class BoundedLength : Response
    {
        [XmlElement(ElementName = "MinLength", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int MinLength { get; set; }

        [XmlElement(ElementName = "MaxLength", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int MaxLength { get; set; }

        public BoundedLength(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.BoundedLength;
        }

        public BoundedLength() { }

        public override String GetResponseDesc()
        {
            return String.Format("\tA string of text between {0} and {1} characters in length\r\n", MinLength, MaxLength);
        }

        public override int GetNumDescriptionSubItems()
        {
            return 1;
        }

        public CResponseObject.CResponseSpecifier GetBounds()
        {
            return new CResponseObject.CRange(MinLength, MaxLength);
        }
        
    }

    [Serializable]
    public class BoundedNum : Response
    {
        [XmlElement(ElementName = "MinValue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal MinValue { get; set; }

        [XmlElement(ElementName = "MaxValue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal MaxValue { get; set; }

        public BoundedNum(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.BoundedNum;
        }

        public BoundedNum() { }

        public override String GetResponseDesc()
        {
            return String.Format("\tA number between {0} and {1}\r\n", MinValue, MaxValue);
        }

        public override int GetNumDescriptionSubItems()
        {
            return 1;
        }

        public CResponseObject.CResponseSpecifier GetBounds()
        {
            return new CResponseObject.CRange(MinValue, MaxValue);
        }
    }

    [Serializable]
    public class Date : Response
    {
        [XmlElement(ElementName = "StartDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Type = typeof(DateEntry))]
        public DateEntry StartDate { get; set; }

        [XmlElement(ElementName = "EndDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Type = typeof(DateEntry))]
        public DateEntry EndDate { get; set; }

        [XmlAttribute(AttributeName = "HasStartDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool HasStartDate { get; set; }

        [XmlAttribute(AttributeName = "HasEndDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool HasEndDate { get; set; }

        public Date(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.Date;
        }

        public Date() { }
        private DateTime StartDateTime = DateTime.MinValue, EndDateTime = DateTime.MaxValue;

        public override void PostSerialize(SurveyItem Parent)
        {
            base.PostSerialize(Parent);
            if (HasStartDate)
                StartDateTime = new DateTime(Convert.ToInt32(StartDate.Year), Convert.ToInt32(StartDate.Month), Convert.ToInt32(StartDate.Day));
            else
                StartDateTime = DateTime.MinValue;
            if (HasEndDate)
                EndDateTime = new DateTime(Convert.ToInt32(EndDate.Year), Convert.ToInt32(EndDate.Month), Convert.ToInt32(EndDate.Day));
            else
                EndDateTime = DateTime.MaxValue;
        }

        public override String GetResponseDesc()
        {
            if (HasEndDate && HasStartDate)
                return String.Format("\tA date that falls between {0:d} and {1:d}, inclusively\r\n", StartDateTime, EndDateTime);
            else if (HasEndDate)
                return String.Format("\tA date that falls on or before {0:d}\r\n", EndDateTime);
            else if (HasStartDate)
                return String.Format("\tA date that falls on or after {0:d}\r\n", StartDateTime);
            else
                return "A date";
        }

        public CResponseObject.CResponseSpecifier GetDateBounds()
        {
            return new CResponseObject.CDateRange(StartDateTime, EndDateTime);
        }

        public override int GetNumDescriptionSubItems()
        {
            return 1;
        }
    }

    [Serializable]
    public class Boolean : Response
    {
        [XmlElement(ElementName = "TrueStatement", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String TrueStatement { get; set; }

        [XmlElement(ElementName = "FalseStatement", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String FalseStatement { get; set; }

        public Boolean(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.Boolean;
        }

        public Boolean() { }

        public override String GetResponseDesc()
        {
            return "\tTrue/False question\r\n";
        }

        public override int GetNumDescriptionSubItems()
        {
            return 2;
        }
        public String GetTrueStatement()
        {
            return TrueStatement;
        }

        public String GetFalseStatement()
        {
            return FalseStatement;
        }
    }

    [Serializable]
    public class MultiBoolean : Response
    {
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem(ElementName = "Text", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(String))]
        public String[] Choices { get; set; }

        [XmlAttribute(AttributeName = "MinSelections", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int MinSelections { get; set; }

        [XmlAttribute(AttributeName = "MaxSelections", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int MaxSelections { get; set; }

        public MultiBoolean(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.MultiBoolean;
        }

        public MultiBoolean() { }

        public int GetNumStatements()
        {
            return Choices.Length;
        }

        public String GetStatement(int ndx)
        {
            return Choices[ndx];
        }

        public override String GetResponseDesc()
        {
            String desc = "Each zero or one in the response is a bit that specifies if a certain selection was made.\r\n ";
            desc += "Below is a synopsis of the bits included in the response in left-to-right order.\r\n";
            for (int ctr = 0; ctr < Choices.Length; ctr++)
                desc += String.Format("\tBit #{0}: {1}\r\n", ctr + 1, Choices[ctr]);
            return desc;
        }

        public override int GetNumDescriptionSubItems()
        {
            return Choices.Length;
        }
    }

    [Serializable]
    public class WeightedChoice
    {
        [XmlElement(ElementName = "Text", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String Text { get; set; }
        [XmlElement(ElementName = "Weight", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Weight { get; set; }

        public WeightedChoice() 
        {
            Weight = 0;
            Text = String.Empty;
        }

        public WeightedChoice(int weight, String text)
        {
            Weight = weight;
            Text = text;
        }
    }
}
