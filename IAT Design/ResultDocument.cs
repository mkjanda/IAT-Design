using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using System.Threading.Tasks;


namespace IATClient.ResultDocument
{
    [Serializable()]
    public enum TResponseType
    {
        None, Boolean, Likert, Date, Multiple, WeightedMultiple, RegEx, MultiBoolean, FixedDig, BoundedNum, BoundedLength
    }

    [Serializable()]
    public class TSurveyResponse
    {
        [XmlAttribute()]
        public int ElementNum;
        [XmlElement("Answer")]
        public String[] Answer;
    }

    [Serializable()]
    public class TIATResponse
    {
        [XmlElement()]
        public uint ItemNum;
        [XmlElement()]
        public uint Latency;
        [XmlElement()]
        public bool Error;
    }

    [Serializable()]
    public class TChoiceFormat
    {
        [XmlElement()]
        public String Value;
        [XmlElement()]
        public String Text;
    }

    [Serializable()]
    public class TSurveyQuestionFormat
    {
        [XmlElement()]
        public String QuestionText;
        [XmlElement()]
        public String ResponseSummary;
        [XmlElement("Choices")]
        public TChoiceFormat[] Choices;
        [XmlAttribute("ResponseType")]
        public TResponseType ResponseType;
    }

    [Serializable()]
    public class TSurveyFormat
    {
        [XmlElement()]
        public String CaptionText;
        [XmlElement("Questions")]
        public TSurveyQuestionFormat[] Questions;
        [XmlAttribute()]
        public int ElementNum;
    }

    [Serializable()]
    public class SurveyFormats
    {
        [XmlElement("SurveyFormat")]
        public TSurveyFormat[] SurveyFormat;
    }

    [Serializable()]
    public class TIATResult
    {
        [XmlElement()]
        public double IATScore;
        [XmlElement("IATResponse")]
        public TIATResponse[] IATResponse;
        [XmlAttribute()]
        public int ElementNum;
    }

    [Serializable()]
    public class CDATA
    {
        [XmlIgnore]
        public String Content { get; set; }

        [XmlText]
        public XmlNode[] CData
        {
            get
            {
                XmlDocument doc = new XmlDocument();
                return new XmlNode[]{doc.CreateCDataSection(Content)};
            }
            set
            {
                if (value == null)
                {
                    Content = null;
                    return;
                }
                if (value.Length != 1)
                    throw new InvalidOperationException("Attempt to deserialize a CDATA section of more than one node");
                Content = value[0].Value;
            }
        }
    }

    [Serializable()]
    public class TTestResult
    {
        [XmlElement("Token")]
        public CDATA Token;
        [XmlElement("SurveyResults")]
        public TSurveyResponse[] SurveyResults;
        [XmlElement()]
        public TIATResult IATResult;
    }

    [Serializable()]
    public class TTitlePage
    {
        [XmlElement("PageHeights")]
        public int[] PageHeights;
    }

    [Serializable()]
    public class TItemSlide
    {
        [XmlElement()]
        public int SlideNum;
        [XmlElement()]
        public int ItemNum;
    }

    [Serializable()]
    public class TItemSlideSize
    {
        [XmlElement()]
        public int NumCols;
        [XmlElement()]
        public int ColOffset;
        [XmlElement()]
        public int NumRows;
        [XmlElement()]
        public int RowOffset;
    }

    [XmlRoot("ResultDocument")]
    public class ResultDocument
    {
        [XmlElement()]
        public String TestAuthor;
        [XmlElement()]
        public String RetrievalTime;
        [XmlElement("SurveyDesign")]
        public SurveyFormats SurveyDesign;
        [XmlElement("TokenName")]
        public String TokenName;
        [XmlElement()]
        public TItemSlideSize ItemSlideSize;
        [XmlElement()]
        public TItemSlide[] ItemSlide;
        [XmlElement()]
        public TTitlePage TitlePage;
        [XmlElement("NumBlockPresentations")]
        public uint[] NumBlockPresentations;
        [XmlElement("TestResult")]
        public TTestResult[] TestResult;
        [XmlAttribute("NumResults")]
        public uint NumResults;
        [XmlAttribute("NumScoredResults")]
        public uint NumScoredResults;
        [XmlAttribute("NumIATItems")]
        public uint NumIATItems;
        [XmlAttribute("NumPresentations")]
        public uint NumPresentations;
    }
}
