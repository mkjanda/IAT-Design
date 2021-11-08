using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

namespace LegacyIATDataRetriever
{
    namespace IATSurveyFileNamespace
    {
        [XmlRoot("WeightedChoice")]
        class WeightedChoice 
        {
            public int Weight;
            public String Choice;
            public WeightedChoice()
            {
                Weight = 0;
                Choice = String.Empty;
            }

            public WeightedChoice(int weight, String choice)
            {
                Weight = weight;
                Choice = choice;
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("WeightedChoice");
                writer.WriteElementString("Weight", Weight.ToString());
                writer.WriteElementString("Choice", Choice.ToString());
                writer.WriteEndElement();
            }

            public void ReadXml(XmlNode node)
            {
                Weight = Convert.ToInt32(node.SelectSingleNode("Weight").InnerText);
                Choice = node.SelectSingleNode("Choice").InnerText;
            }

            public XmlSchema GetSchema()
            {
                return null;
            }
        }

        [XmlRoot("Response")]
        class Response
        {
            public enum EResponseType { None, Instruction, Boolean, Likert, Date, Multiple, WeightedMultiple, RegEx, MultiBoolean, FixedDig, BoundedNum, BoundedLength };
            private CResponseObjectCollection _ResponseObjects = new CResponseObjectCollection();
            protected SurveyItem ParentItem = null;

            public String GetSurveyName()
            {
                return ParentItem.GetSurveyName();
            }

            public int GetItemNum()
            {
                return ParentItem.GetItemNum();
            }

            [XmlAttribute("ResponseType")]
            public virtual EResponseType ResponseType
            {
                get
                {
                    return EResponseType.None;
                }
            }

            [XmlIgnore]
            public CResponseObjectCollection ResponseObjects
            {
                get
                {
                    return _ResponseObjects;
                }
            }

            public Response(SurveyItem parentItem)
            {
                ParentItem = parentItem;
            }

            [XmlIgnore]
            public virtual int NumDescriptionSubItems
            {
                get
                {
                    return 0;
                }
            }

            public virtual String GetDescriptionSubItem(int ndx)
            {
                return String.Empty;
            }

            public virtual CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                throw new NotImplementedException();
            }

            public void AttachResponseObject(CResponseObject obj)
            {
                _ResponseObjects.Add(obj);
            }


            public virtual String GetResponseDesc()
            {
                return String.Empty;
            }

            public virtual void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                writer.WriteElementString("DummyValue", "abcdefg");
                writer.WriteEndElement();
            }

            public static Response CreateFromXml(XmlNode node, SurveyItem si)
            {
                EResponseType type = (EResponseType)Enum.Parse(typeof(EResponseType), node["ResponseType"].InnerText);
                Response r = null;
                switch (type)
                {
                    case EResponseType.Instruction:
                        r = new Response(si);
                        break;

                    case EResponseType.None:
                        r = new Response(si);
                        break;

                    case EResponseType.Boolean:
                        r = new BoolResponseType(si);
                        break;

                    case EResponseType.BoundedLength:
                        r = new BoundedLength(si);
                        break;

                    case EResponseType.BoundedNum:
                        r = new BoundedNum(si);
                        break;

                    case EResponseType.Date:
                        r = new DateResponse(si);
                        break;

                    case EResponseType.FixedDig:
                        r = new FixedDigResponse(si);
                        break;

                    case EResponseType.Likert:
                        r = new LikertResponse(si);
                        break;

                    case EResponseType.MultiBoolean:
                        r = new MultiBooleanResponse(si);
                        break;

                    case EResponseType.Multiple:
                        r = new MultipleResponse(si);
                        break;

                    case EResponseType.RegEx:
                        r = new RegExResponse(si);
                        break;

                    case EResponseType.WeightedMultiple:
                        r = new WeightedMultipleResponse(si);
                        break;
                }

                r.ReadXml(node);
                return r;
            }

            public virtual void ReadXml(XmlNode node)
            {
                throw new NotImplementedException();
            }
        }

        [XmlRoot("Boolean")]
        class BoolResponseType : Response
        {
            public String TrueStatement, FalseStatement;

            public String GetTrueStatement()
            {
                return TrueStatement;
            }

            public String GetFalseStatement()
            {
                return FalseStatement;
            }

            [XmlAttribute("ResponseType")]
            public override EResponseType ResponseType
            {
                get
                {
                    return EResponseType.Boolean;
                }
            }

            public BoolResponseType(SurveyItem parentItem) : base(parentItem) { }

            [XmlIgnore]
            public override int NumDescriptionSubItems
            {
                get
                {
                    return 2;
                }
            }

            public override string GetDescriptionSubItem(int ndx)
            {
                if (ndx == 0)
                    return TrueStatement;
                else if (ndx == 1)
                    return FalseStatement;
                return String.Empty;
            }


            public override String GetResponseDesc()
            {
                return "True/False question";
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                writer.WriteElementString("TrueStatement", TrueStatement.ToString());
                writer.WriteElementString("FalseStatement", FalseStatement.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlNode node)
            {
                TrueStatement = node.SelectSingleNode("TrueStatement").InnerText;
                FalseStatement = node.SelectSingleNode("FalseStatement").InnerText;
            }

            public override CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                CBoolResponseObject ResponseObject = new CBoolResponseObject(type, this);
                ResponseObjects.Add(ResponseObject);
                return ResponseObject;
            }
        }

        [XmlRoot("Likert")]
        class LikertResponse : Response
        {
            public bool ReverseScored;
            public List<String> LikertStatements;

            public bool ReverseScored
            {
                get
                {
                    return _ReverseScored;
                }
                set
                {
                    _ReverseScored = value;
                }
            }

            public List<String> LikertStatements
            {
                get
                {
                    return _LikertStatements;
                }
            }

            public override EResponseType ResponseType
            {
                get
                {
                    return EResponseType.Likert;
                }
            }

            public LikertResponse(SurveyItem parentItem)
                : base(parentItem)
            {
                _LikertStatements = new List<String>();
                _ReverseScored = false;
                ParentItem = parentItem;
            }

            public override int NumDescriptionSubItems
            {
                get
                {
                    return LikertStatements.Count;
                }
            }

            public override string GetDescriptionSubItem(int ndx)
            {
                return LikertStatements[ndx];
            }

            public override String GetResponseDesc()
            {
                if (ReverseScored)
                    return String.Format("Reverse scored Likert item with response range 1 to {0}.  (Answers have already been reversed.)\r\n", LikertStatements.Count);
                else
                    return String.Format("Likert item with response range 1 to {0}\r\n", LikertStatements.Count);
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                writer.WriteAttributeString("NumStatements", LikertStatements.Count.ToString());
                writer.WriteElementString("ReverseScored", ReverseScored.ToString());
                for (int ctr = 0; ctr < LikertStatements.Count; ctr++)
                    writer.WriteElementString("LikertStatement", LikertStatements[ctr]);
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlNode node)
            {
                int nStatements = Convert.ToInt32(node["NumStatements"].InnerText);
                ReverseScored = Convert.ToBoolean(node.SelectSingleNode("ReverseScored").InnerText);
                LikertStatements.Clear();
                foreach (XmlNode n in node.SelectNodes("LikertStatement"))
                    LikertStatements.Add(n.InnerText);
            }

            public String GetStatement(int nStatement)
            {
                return LikertStatements[nStatement];
            }

            public int GetNumStatements()
            {
                return LikertStatements.Count;
            }

            public bool IsReverseScored()
            {
                return ReverseScored;
            }

            public override CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                CLikertResponseObject ResponseObject = new CLikertResponseObject(type, this);
                ResponseObjects.Add(ResponseObject);
                return ResponseObject;
            }
        }

        class DateResponse : Response
        {
            private bool _HasEndDate, _HasStartDate;
            private DateTime _StartDate, _EndDate;

            public bool HasStartDate
            {
                get
                {
                    return _HasStartDate;
                }
                set
                {
                    _HasStartDate = value;
                }
            }

            public bool HasEndDate
            {
                get
                {
                    return _HasEndDate;
                }
                set
                {
                    _HasEndDate = value;
                }
            }

            public DateTime StartDate
            {
                get
                {
                    return _StartDate.Date;
                }
                set
                {
                    _StartDate = value;
                }
            }

            public DateTime EndDate
            {
                get
                {
                    return _EndDate.Date;
                }
                set
                {
                    _EndDate = value;
                }
            }

            public DateTime GetStartDate()
            {
                if (HasStartDate)
                    return StartDate;
                return DateTime.MinValue;
            }

            public DateTime GetEndDate()
            {
                if (HasEndDate)
                    return EndDate;
                return DateTime.MaxValue;
            }

            public CResponseObject.CResponseSpecifier GetDateBounds()
            {
                if (!HasStartDate)
                    return new CResponseObject.CDateRange(DateTime.MinValue, EndDate);
                else if (!HasEndDate)
                    return new CResponseObject.CDateRange(StartDate, DateTime.MaxValue);
                else
                    return new CResponseObject.CDateRange(StartDate, EndDate);
            }

            public override EResponseType ResponseType
            {
                get
                {
                    return EResponseType.Date;
                }
            }

            public DateResponse(SurveyItem parentItem)
                : base(parentItem)
            {
                StartDate = DateTime.MinValue;
                EndDate = DateTime.MaxValue;
                HasStartDate = false;
                HasEndDate = false;
                ParentItem = parentItem;
            }

            public override int NumDescriptionSubItems
            {
                get
                {
                    return 1;
                }
            }

            public override string GetDescriptionSubItem(int ndx)
            {
                return GetResponseDesc();
            }

            public override String GetResponseDesc()
            {
                if (HasEndDate && HasStartDate)
                    return String.Format("A date that falls between {0:d} and {1:d}, inclusively", StartDate, EndDate);
                else if (HasEndDate)
                    return String.Format("A date that falls on or before {0:d}", EndDate);
                else if (HasStartDate)
                    return String.Format("A date that falls on or after {0:d}", StartDate);
                else
                    return "A date";
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                writer.WriteAttributeString("HasStartDate", HasStartDate.ToString());
                writer.WriteAttributeString("HasEndDate", HasEndDate.ToString());
                writer.WriteElementString("StartDay", StartDate.Day.ToString());
                writer.WriteElementString("StartMonth", StartDate.Month.ToString());
                writer.WriteElementString("StartYear", StartDate.Year.ToString());
                writer.WriteElementString("EndDay", EndDate.Day.ToString());
                writer.WriteElementString("EndMonth", EndDate.Month.ToString());
                writer.WriteElementString("EndYear", EndDate.Year.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlNode node)
            {
                HasStartDate = Convert.ToBoolean(node["HasStartDate"].InnerText);
                HasEndDate = Convert.ToBoolean(node["HasEndDate"].InnerText);
                int d, m, y;
                d = Convert.ToInt32(node.SelectSingleNode("StartDay").InnerText);
                m = Convert.ToInt32(node.SelectSingleNode("StartMonth").InnerText);
                y = Convert.ToInt32(node.SelectSingleNode("StartYear").InnerText);
                StartDate = new DateTime(y, m, d);
                d = Convert.ToInt32(node.SelectSingleNode("EndDay").InnerText);
                m = Convert.ToInt32(node.SelectSingleNode("EndMonth").InnerText);
                y = Convert.ToInt32(node.SelectSingleNode("EndYear").InnerText);
                EndDate = new DateTime(y, m, d);
            }

            public override CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                CDateResponseObject ResponseObject = new CDateResponseObject(type, this);
                ResponseObjects.Add(ResponseObject);
                return ResponseObject;
            }
        }

        class MultipleResponse : Response
        {
            private List<String> _Choices;

            public List<String> Choices
            {
                get
                {
                    return _Choices;
                }
            }

            public override EResponseType ResponseType
            {
                get
                {
                    return EResponseType.Multiple;
                }
            }

            public MultipleResponse(SurveyItem parentItem)
                : base(parentItem)
            {
                _Choices = new List<String>();
            }

            public int GetNumStatements()
            {
                return Choices.Count;
            }

            public String GetStatement(int n)
            {
                return Choices[n];
            }

            public override String GetResponseDesc()
            {
                String desc = String.Empty;
                for (int ctr = 0; ctr < Choices.Count; ctr++)
                    desc += String.Format("\t{0}: {1}\r\n", ctr + 1, Choices[ctr]);
                return desc;
            }

            public override int NumDescriptionSubItems
            {
                get
                {
                    return Choices.Count;
                }
            }

            public override String GetDescriptionSubItem(int ndx)
            {
                return Choices[ndx];
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                writer.WriteAttributeString("NumChoices", Choices.Count.ToString());
                for (int ctr = 0; ctr < Choices.Count; ctr++)
                    writer.WriteElementString("Choice", Choices[ctr]);
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlNode node)
            {
                Choices.Clear();
                int nChoices = Convert.ToInt32(node["NumChoices"].InnerText);
                foreach (XmlNode n in node.SelectNodes("Choice"))
                    Choices.Add(n.InnerText);
            }

            public override CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                CMultipleResponseObject ResponseObject = new CMultipleResponseObject(type, this);
                ResponseObjects.Add(ResponseObject);
                return ResponseObject;
            }
        }

        class WeightedMultipleResponse : Response
        {
            private List<WeightedChoice> _Choices;

            public List<WeightedChoice> Choices
            {
                get
                {
                    return _Choices;
                }
            }

            public override EResponseType ResponseType
            {
                get
                {
                    return EResponseType.WeightedMultiple;
                }
            }

            public WeightedMultipleResponse(SurveyItem parentItem)
                : base(parentItem)
            {
                _Choices = new List<WeightedChoice>();
            }

            public override String GetResponseDesc()
            {
                return String.Empty;
            }

            public int GetNumStatements()
            {
                return Choices.Count;
            }

            public String GetChoice(int n)
            {
                return Choices[n].Choice;
            }

            public int GetChoiceWeight(int n)
            {
                return Choices[n].Weight;
            }

            public override int NumDescriptionSubItems
            {
                get
                {
                    return Choices.Count;
                }
            }

            public override String GetDescriptionSubItem(int ndx)
            {
                return Choices[ndx].Choice;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                writer.WriteAttributeString("NumChoices", Choices.Count.ToString());
                for (int ctr = 0; ctr < Choices.Count; ctr++)
                    Choices[ctr].WriteXml(writer);
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlNode node)
            {
                int nChoices = Convert.ToInt32(node["NumChoices"].InnerText);
                Choices.Clear();
                foreach (XmlNode n in node.SelectNodes("WeightedChoice")) {
                    WeightedChoice wc = new WeightedChoice();
                    wc.ReadXml(n);
                    Choices.Add(wc);
                }
            }

            public override CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                CWeightedMultipleResponseObject resp = new CWeightedMultipleResponseObject(type, this);
                ResponseObjects.Add(resp);
                return resp;
            }
        }

        class RegExResponse : Response
        {
            private String _RegEx;

            public String RegEx
            {
                get
                {
                    return _RegEx;
                }
                set
                {
                    _RegEx = value;
                }
            }

            public override Response.EResponseType ResponseType
            {
                get
                {
                    return EResponseType.RegEx;
                }
            }

            public RegExResponse(SurveyItem parentItem)
                : base(parentItem)
            {
                RegEx = String.Empty;
            }

            public String GetRegEx()
            {
                return RegEx;
            }

            public override int NumDescriptionSubItems
            {
                get
                {
                    return 1;
                }
            }

            public override string GetDescriptionSubItem(int ndx)
            {
                return GetResponseDesc();
            }

            public override String GetResponseDesc()
            {
                return String.Format("Text that matches the regular expression \"{0}\"", RegEx);
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                writer.WriteElementString("RegEx", RegEx);
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlNode node)
            {
                RegEx = node.SelectSingleNode("RegEx").InnerText;
            }

            public override CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                CRegExResponseObject resp = new CRegExResponseObject(type, this);
                ResponseObjects.Add(resp);
                return resp;
            }
        }

        class MultiBooleanResponse : Response
        {
            private List<String> _Statements;

            public List<String> Statements
            {
                get
                {
                    return _Statements;
                }
            }

            public override Response.EResponseType ResponseType
            {
                get
                {
                    return EResponseType.MultiBoolean;
                }
            }

            public MultiBooleanResponse(SurveyItem parentItem)
                : base(parentItem)
            {
                _Statements = new List<String>();
            }

            public int GetNumStatements()
            {
                return Statements.Count;
            }

            public String GetStatement(int ndx)
            {
                return Statements[ndx];
            }

            public override int NumDescriptionSubItems
            {
                get
                {
                    return 1;
                }
            }

            public override String GetDescriptionSubItem(int ndx)
            {
                return Statements[ndx];
            }

            public override String GetResponseDesc()
            {
                String desc = "Each zero or one in the response is a bit that specifies if a certain selection was made.\r\n ";
                desc += "Below is a synopsis of the bits included in the response in left-to-right order.\r\n";
                for (int ctr = 0; ctr < Statements.Count; ctr++)
                    desc += String.Format("\tBit #{0}: {1}\r\n", ctr + 1, Statements[ctr]);
                return desc;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                writer.WriteAttributeString("NumStatements", Statements.Count.ToString());
                for (int ctr = 0; ctr < Statements.Count; ctr++)
                    writer.WriteElementString("Statement", Statements[ctr]);
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlNode node)
            {
                int nStatements = Convert.ToInt32(node["NumStatements"].InnerText);
                Statements.Clear();
                foreach (XmlNode n in node.SelectNodes("Statement"))
                    Statements.Add(n.InnerText);
            }

            public override CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                CMultiBooleanResponseObject resp = new CMultiBooleanResponseObject(type, this);
                ResponseObjects.Add(resp);
                return resp;
            }
        }

        class FixedDigResponse : Response
        {
            private int _NumDigits;

            public int NumDigits
            {
                get
                {
                    return _NumDigits;
                }
                set
                {
                    _NumDigits = value;
                }
            }

            public FixedDigResponse(SurveyItem parentItem)
                : base(parentItem)
            {
                NumDigits = -1;
            }

            public int GetNumDigits()
            {
                return NumDigits;
            }

            public override Response.EResponseType ResponseType
            {
                get
                {
                    return EResponseType.FixedDig;
                }
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                writer.WriteElementString("NumDigits", NumDigits.ToString());
                writer.WriteEndElement();
            }

            public override int NumDescriptionSubItems
            {
                get
                {
                    return 1;
                }
            }

            public override string GetDescriptionSubItem(int ndx)
            {
                return GetResponseDesc();
            }

            public override string GetResponseDesc()
            {
                return String.Format("A response of {0} digits.", NumDigits);
            }

            public override void ReadXml(XmlNode node)
            {
                NumDigits = Convert.ToInt32(node.SelectSingleNode("NumDigits").InnerText);
            }

            public override CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                CFixedDigResponseObject respObj = new CFixedDigResponseObject(type, this);
                ResponseObjects.Add(respObj);
                return respObj;
            }
        }

        class BoundedNum : Response
        {
            private decimal _MinValue = Decimal.MinValue, _MaxValue = Decimal.MaxValue;

            public decimal MinValue
            {
                get
                {
                    return _MinValue;
                }
                set
                {
                    _MaxValue = value;
                }
            }

            public decimal MaxValue
            {
                get
                {
                    return _MaxValue;
                }
                set
                {
                    _MaxValue = value;
                }
            }

            public decimal GetMinBound()
            {
                return MinValue;
            }

            public decimal GetMaxBound()
            {
                return MaxValue;
            }

            public override EResponseType ResponseType
            {
                get
                {
                    return EResponseType.BoundedNum;
                }
            }

            public BoundedNum(SurveyItem parentItem)
                : base(parentItem)
            {
                _MinValue = _MaxValue = 0;
            }

            public CResponseObject.CResponseSpecifier GetBounds()
            {
                return new CResponseObject.CRange(MinValue.ToString(), MaxValue.ToString());
            }

            public override int NumDescriptionSubItems
            {
                get
                {
                    return 1;
                }
            }

            public override string GetDescriptionSubItem(int ndx)
            {
                return GetResponseDesc();
            }

            public override String GetResponseDesc()
            {
                return String.Format("\tA number between {0} and {1}\r\n", MinValue, MaxValue);
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                if (MinValue == Decimal.MinValue)
                    writer.WriteElementString("MinValue", "NULL");
                else
                    writer.WriteElementString("MinValue", MinValue.ToString());
                if (MaxValue == Decimal.MaxValue)
                    writer.WriteElementString("MaxValue", "NULL");
                else
                    writer.WriteElementString("MaxValue", MaxValue.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlNode node)
            {
                String str = node.SelectSingleNode("MinValue").InnerText;
                if (str == "NULL")
                    MinValue = Decimal.MinValue;
                else
                    MinValue = Convert.ToDecimal(str);
                str = node.SelectSingleNode("MaxValue").InnerText;
                if (str == "NULL")
                    MaxValue = Decimal.MinValue;
                else
                    MaxValue = Convert.ToDecimal(str);
            }

            public override CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                CBoundedNumResponseObject obj = new CBoundedNumResponseObject(type, this);
                ResponseObjects.Add(obj);
                return obj;
            }
        }

        class BoundedLength : Response
        {
            private int _MinLength, _MaxLength;

            public int MinLength
            {
                get
                {
                    return _MinLength;
                }
                set
                {
                    _MinLength = value;
                }
            }

            public int MaxLength
            {
                get
                {
                    return _MaxLength;
                }
                set
                {
                    _MaxLength = value;
                }
            }

            public int GetMinChars()
            {
                return MinLength;
            }

            public int GetMaxChars()
            {
                return MaxLength;
            }

            public override EResponseType ResponseType
            {
                get
                {
                    return EResponseType.BoundedLength;
                }
            }

            public BoundedLength(SurveyItem parentItem)
                : base(parentItem)
            {
                _MinLength = _MaxLength = 0;
            }

            public CResponseObject.CResponseSpecifier GetBounds()
            {
                return new CResponseObject.CRange(MinLength.ToString(), MaxLength.ToString());
            }

            public override int NumDescriptionSubItems
            {
                get
                {
                    return 1;
                }
            }

            public override string GetDescriptionSubItem(int ndx)
            {
                return GetResponseDesc();
            }

            public override String GetResponseDesc()
            {
                return String.Format("A string of text between {0} and {1} characters in length", MinLength, MaxLength);
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Response");
                writer.WriteAttributeString("ResponseType", ResponseType.ToString());
                writer.WriteElementString("MinLength", MinLength.ToString());
                writer.WriteElementString("MaxLength", MaxLength.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlNode node)
            {
                MinLength = Convert.ToInt32(node.SelectSingleNode("MinLength").InnerText);
                MaxLength = Convert.ToInt32(node.SelectSingleNode("MaxLength").InnerText);
            }

            public override CResponseObject AttachResponseObject(CResponseObject.EType type)
            {
                CBoundedLengthResponseObject obj = new CBoundedLengthResponseObject(type, this);
                ResponseObjects.Add(obj);
                return obj;
            }
        }

        class SurveyItem 
        {
            private String _Text;
            private Response _TheResponse;
            private String SurveyName;
            private int ItemNum;

            public String GetSurveyName()
            {
                return SurveyName;
            }

            public int GetItemNum()
            {
                return ItemNum;
            }

            public String Text
            {
                get
                {
                    return _Text;
                }
                set
                {
                    _Text = value;
                }
            }

            public Response TheResponse
            {
                get
                {
                    return _TheResponse;
                }
                set
                {
                    _TheResponse = value;
                }
            }

            protected SurveyItem(String surveyName, int itemNum)
            {
                SurveyName = surveyName;
                ItemNum = itemNum;
            }

            public static SurveyItem CreateFromXml(String surveyName, int itemNum, XmlNode node)
            {
                SurveyItem si = new SurveyItem(surveyName, itemNum);
                si.ReadXml(node);
                return si;
            }

            public SurveyItem(String surveyName, int itemNum, Response.EResponseType respType)
            {
                Text = String.Empty;
                switch (respType)
                {
                    case Response.EResponseType.Boolean:
                        TheResponse = new BoolResponseType(this);
                        break;

                    case Response.EResponseType.BoundedLength:
                        TheResponse = new BoundedLength(this);
                        break;

                    case Response.EResponseType.BoundedNum:
                        TheResponse = new BoundedNum(this);
                        break;

                    case Response.EResponseType.Date:
                        TheResponse = new DateResponse(this);
                        break;

                    case Response.EResponseType.FixedDig:
                        TheResponse = new FixedDigResponse(this);
                        break;

                    case Response.EResponseType.Likert:
                        TheResponse = new LikertResponse(this);
                        break;

                    case Response.EResponseType.MultiBoolean:
                        TheResponse = new MultiBooleanResponse(this);
                        break;

                    case Response.EResponseType.Multiple:
                        TheResponse = new MultipleResponse(this);
                        break;

                    case Response.EResponseType.None:
                        TheResponse = new Response(this);
                        break;

                    case Response.EResponseType.RegEx:
                        TheResponse = new RegExResponse(this);
                        break;

                    case Response.EResponseType.WeightedMultiple:
                        TheResponse = new WeightedMultipleResponse(this);
                        break;
                }
                SurveyName = surveyName;
                ItemNum = itemNum;
            }

            public SurveyItem(String surveyName, int itemNum, CSurveyItem si)
            {
                Text = String.Empty;
                SurveyName = surveyName;
                ItemNum = itemNum;
                if (si.Response.ResponseType == CResponse.EResponseType.Instruction)
                    _TheResponse = new Response(this);
                else
                    _TheResponse = si.Response.GenerateSerializableResponse(this);
                if (si.IsScored)
                    _TheResponse.AttachResponseObject(CResponseObject.CloneResponseObject(si.DefinedResponse, _TheResponse));
            }

            public String GetDescription()
            {
                if (TheResponse.ResponseType == Response.EResponseType.None)
                    return String.Empty;
                return String.Format("{0}\r\n{1}", Text, TheResponse.GetResponseDesc());
            }

            public virtual void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("SurveyName", SurveyName);
                if (TheResponse.ResponseType == Response.EResponseType.None)
                    writer.WriteAttributeString("ItemNum", "-1");
                else
                    writer.WriteAttributeString("ItemNum", ItemNum.ToString());
                writer.WriteElementString("Text", Text.ToString());
                TheResponse.WriteXml(writer);
                writer.WriteEndElement();
            }

            public virtual void ReadXml(XmlNode node)
            {
                if (Convert.ToBoolean(node["HasException"].InnerText))
                    throw new CXmlSerializationException(node);
                SurveyName = node["SurveyName"].InnerText;
                ItemNum = Convert.ToInt32(node["ItemNum"].InnerText);
                Text = node.SelectSingleNode("Text").InnerText;
                TheResponse = Response.CreateFromXml(node.SelectSingleNode("Response"), this);
            }

            public String GetName()
            {
                return "SurveyItem";
            }
        }

        class Color
        {
            private int _Red, _Green, _Blue;

            public int Red
            {
                get
                {
                    return _Red;
                }
                set
                {
                    _Red = value;
                }
            }

            public int Green
            {
                get
                {
                    return _Green;
                }
                set
                {
                    _Green = value;
                }
            }

            public int Blue
            {
                get
                {
                    return _Blue;
                }
                set
                {
                    _Blue = value;
                }
            }

            public Color()
            {
                _Red = _Blue = _Green = 0;
            }

            public Color(int r, int g, int b)
            {
                Red = r;
                Green = g;
                Blue = b;
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Color");
                writer.WriteElementString("Red", Red.ToString());
                writer.WriteElementString("Green", Green.ToString());
                writer.WriteElementString("Blue", Blue.ToString());
                writer.WriteEndElement();
            }

            public void ReadXml(XmlNode node)
            {
                Red = Convert.ToInt32(node.SelectSingleNode("Red").InnerText);
                Green = Convert.ToInt32(node.SelectSingleNode("Green").InnerText);
                Blue = Convert.ToInt32(node.SelectSingleNode("Blue").InnerText);
            }

            public XmlSchema GetSchema()
            {
                return null;
            }
        }

        class SurveyCaption : SurveyItem
        {
            private IATSurveyFileNamespace.Color _FontColor, _BackColor, _BorderColor;
            private int _FontSize, _BorderWidth;

            public IATSurveyFileNamespace.Color FontColor
            {
                get
                {
                    return _FontColor;
                }
                set
                {
                    _FontColor = value;
                }
            }

            public IATSurveyFileNamespace.Color BackColor
            {
                get
                {
                    return _BackColor;
                }
                set
                {
                    _BackColor = value;
                }
            }

            public IATSurveyFileNamespace.Color BorderColor
            {
                get
                {
                    return _BorderColor;
                }
                set
                {
                    _BorderColor = value;
                }
            }

            public int FontSize
            {
                get
                {
                    return _FontSize;
                }
                set
                {
                    _FontSize = value;
                }
            }

            public int BorderWidth
            {
                get
                {
                    return _BorderWidth;
                }
                set
                {
                    _BorderWidth = value;
                }
            }

            public SurveyCaption(String surveyName, int itemNum)
                : base(surveyName, itemNum, Response.EResponseType.None)
            {
                FontColor = new IATSurveyFileNamespace.Color();
                BorderColor = new IATSurveyFileNamespace.Color();
                BackColor = new IATSurveyFileNamespace.Color();
                BorderWidth = 0;
                FontSize = 16;
            }

            public XmlNode GetSourceXMLNode(XmlDocument doc)
            {
                XmlNode parent = doc.CreateElement("Caption");
                return parent;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Caption");
                writer.WriteElementString("Text", Text);
                TheResponse.WriteXml(writer);
                FontColor.WriteXml(writer);
                BackColor.WriteXml(writer);
                BorderColor.WriteXml(writer);
                writer.WriteElementString("FontSize", FontSize.ToString());
                writer.WriteElementString("BorderWidth", BorderWidth.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlNode node)
            {
                Text = node.SelectSingleNode("Text").InnerText;
                TheResponse = new Response(this);
                FontColor.ReadXml(node.SelectSingleNode("Color[position() eq 1]"));
                BackColor.ReadXml(node.SelectSingleNode("Color[position() eq 2]"));
                BorderColor.ReadXml(node.SelectSingleNode("Color[position() eq 3]"));
                FontSize = Convert.ToInt32(node.SelectSingleNode("FontSize").InnerText);
                BorderWidth = Convert.ToInt32(node.SelectSingleNode("BorderWidth").InnerText);
            }
        }

        class Survey 
        {
            private String _SurveyName;
            private String _FileName;
            private List<SurveyItem> _SurveyItems;
            private bool _HasCaption;
            private int _ClientID;
            private decimal _Timeout;

            public int NumItems
            {
                get
                {
                    int nItems = 0;
                    for (int ctr = 0; ctr < SurveyItems.Count; ctr++)
                        if (SurveyItems[ctr].TheResponse.ResponseType != Response.EResponseType.None)
                            nItems++;
                    return nItems;
                }
            }


            public decimal Timeout
            {
                get
                {
                    return _Timeout;
                }
                set
                {
                    _Timeout = value;
                }
            }

            public String FileName
            {
                get
                {
                    return _FileName;
                }
                set
                {
                    _FileName = value;
                }
            }

            public String SurveyName
            {
                get
                {
                    return _SurveyName;
                }
                set
                {
                    _SurveyName = value;
                }
            }

            public List<SurveyItem> SurveyItems
            {
                get
                {
                    return _SurveyItems;
                }
            }

            public SurveyItem GetSurveyItem(int itemNum)
            {
                int ndx = 0;
                int ctr = 0;
                while (ctr != itemNum - 1)
                    if (SurveyItems[ndx++].TheResponse.ResponseType != Response.EResponseType.None)
                        ctr++;
                return SurveyItems[ctr];
            }

            public bool HasCaption
            {
                get
                {
                    return _HasCaption;
                }
                set
                {
                    _HasCaption = value;
                }
            }

            public int ClientID
            {
                get
                {
                    return _ClientID;
                }
                set
                {
                    _ClientID = value;
                }
            }

            public Survey(String surveyName, String fileName, decimal timeout, List<IATClient.CSurveyItem> Items)
            {
                SurveyName = surveyName;
                FileName = fileName;
                ClientID = -1;
                _SurveyItems = new List<SurveyItem>();
                if (Items.Count == 0)
                    return;
                if (Items[0].IsCaption)
                    HasCaption = true;
                else
                    HasCaption = false;
                Timeout = timeout;
                for (int ctr = 0; ctr < Items.Count; ctr++)
                {
                    SurveyItems.Add(Items[ctr].GenerateSerializableItem());
                }
            }

            protected Survey()
            {
                SurveyName = String.Empty;
                FileName = String.Empty;
                ClientID = -1;
                _SurveyItems = new List<SurveyItem>();
            }

            public static Survey CreateFromXml(XmlDocument doc)
            {
                Survey s = new Survey();
                s.ReadXml(doc);
                return s;
            }

            public XmlDocument GetSourceXMLDocument()
            {
                MemoryStream memStream = new MemoryStream();
                XmlTextWriter writer = new XmlTextWriter(memStream, System.Text.Encoding.UTF8);
                for (int ctr = 0; ctr < NumItems; ctr++)
                    SurveyItems[ctr].WriteXml(writer);
                writer.Flush();
                writer.Close();
                memStream.Seek(0, SeekOrigin.Begin);
                XmlDocument doc = new XmlDocument();
                doc.Load(memStream);
                return doc;
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Survey");
                writer.WriteAttributeString("HasCaption", HasCaption.ToString());
                writer.WriteAttributeString("NumSurveyItems", SurveyItems.Count.ToString());
                writer.WriteAttributeString("SurveyName", SurveyName);
                writer.WriteAttributeString("FileName", FileName);
                writer.WriteAttributeString("ClientID", ClientID.ToString());
                writer.WriteAttributeString("TimeoutMillis", Math.Floor(Timeout * 60000).ToString());
                for (int ctr = 0; ctr < SurveyItems.Count; ctr++)
                    SurveyItems[ctr].WriteXml(writer);
                writer.WriteEndElement();
            }

            public void ReadXml(XmlDocument xDoc)
            {
                if (Convert.ToBoolean(xDoc.DocumentElement["HasException"].Value))
                    throw new CXmlSerializationException(xDoc.DocumentElement);
                SurveyName = xDoc.DocumentElement["SurveyName"].Value;
                FileName = xDoc.DocumentElement["FileName"].Value;
                HasCaption = Convert.ToBoolean(xDoc.DocumentElement["HasCaption"].Value);
                int nItems = Convert.ToInt32(xDoc.DocumentElement["NumSurveyItems"].Value);
                ClientID = Convert.ToInt32(xDoc.DocumentElement["ClientID"].Value);
                Timeout = Convert.ToInt32(xDoc.DocumentElement["TimeoutMillis"].Value) / 60000;
                XmlNode captionNode = xDoc.SelectSingleNode("Caption");
                if (captionNode != null)
                {
                    SurveyCaption caption = new SurveyCaption(SurveyName, -1);
                    caption.ReadXml(captionNode);
                }
                int itemNumCtr = 0;
                foreach (XmlNode n in xDoc.SelectNodes("SurveyItem")) {
                        SurveyItem item = SurveyItem.CreateFromXml(SurveyName, itemNumCtr, xDoc.DocumentElement);
                        if (item.TheResponse.ResponseType != Response.EResponseType.None)
                            itemNumCtr++;
                        SurveyItems.Add(item);
                    
                }
            }

            public XmlSchema GetSchema()
            {
                return null;
            }
        }
    }
}