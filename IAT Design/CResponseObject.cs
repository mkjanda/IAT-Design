using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.Text.RegularExpressions;

namespace IATClient
{
    class CResponseObjectCollection {
        private List<CResponseObject> ResponseObjects = new List<CResponseObject>();

        public CResponseObject this[int n]
        {
            get
            {
                return ResponseObjects[n];
            }
        }

        public int Count
        {
            get
            {
                return ResponseObjects.Count;
            }
        }

        public void Add(CResponseObject obj)
        {
            ResponseObjects.Add(obj);
        }

        public void AddRange(List<CResponseObject> objList)
        {
            ResponseObjects.AddRange(objList);
        }

        public void Clear()
        {
            ResponseObjects.Clear();
        }

        public void Remove(CResponseObject obj)
        {
            ResponseObjects.Remove(obj);
        }
    }


    public abstract class CResponseObject
    {
        protected static Size RadioSize = new Size(16, 16);
        protected static Size CheckSize = new Size(16, 16);
        protected static Padding RadioPadding = new Padding(6, 6, 6, 6);
        protected static Padding CheckPadding = new Padding(6, 6, 6, 6); 
        protected static Padding ElementPadding = new Padding(5, 5, 5, 5); 
        protected static Padding PanelPadding = new Padding(25, 25, 25, 25); 

        public class OverlapException : Exception
        {
            public OverlapException(String msg) : base(msg) { }
        }

        protected IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            }
        }

        static public CResponseObject CreateFromResultData(IATSurveyFile.Response resp)
        {
            CResponseObject respObj = null;
            switch (resp.ResponseType)
            {
                case IATClient.IATSurveyFile.ResponseType.Boolean:
                    respObj = new CBoolResponseObject(EType.actual, (IATSurveyFile.Boolean)resp);
                    break;

                case IATClient.IATSurveyFile.ResponseType.BoundedLength:
                    respObj = new CBoundedLengthResponseObject(EType.actual, resp);
                    break;

                case IATClient.IATSurveyFile.ResponseType.BoundedNum:
                    respObj = new CBoundedNumResponseObject(EType.actual, resp);
                    break;

                case IATClient.IATSurveyFile.ResponseType.Date:
                    respObj = new CDateResponseObject(EType.actual, resp);
                    break;

                case IATClient.IATSurveyFile.ResponseType.FixedDig:
                    respObj = new CFixedDigResponseObject(EType.actual, resp);
                    break;

                case IATClient.IATSurveyFile.ResponseType.Likert:
                    respObj = new CLikertResponseObject(EType.actual, resp);
                    break;

                case IATClient.IATSurveyFile.ResponseType.MultiBoolean:
                    respObj = new CMultiBooleanResponseObject(EType.actual, resp);
                    break;

                case IATClient.IATSurveyFile.ResponseType.Multiple:
                    respObj = new CMultipleResponseObject(EType.actual, resp);
                    break;

                case IATClient.IATSurveyFile.ResponseType.RegEx:
                    respObj = new CRegExResponseObject(EType.actual, resp);
                    break;


                case IATSurveyFile.ResponseType.WeightedMultiple:
                    respObj = new CWeightedMultipleResponseObject(EType.actual, resp);
                    break;
            }
            return respObj;
        }

        //public abstract void ReadXml(XmlReader reader);
//        public abstract void WriteXml(XmlWriter writer);
            
        public abstract class CResponseSpecifier : INamedXmlSerializable, IStoredInXml
        {
            public String GetName()
            {
                return "ResponseSpecifier";
            }

            protected enum EType { singleton, range, dateSingleton, dateRange };
            protected abstract EType Type { get; }
            public abstract String Specifier { get; }
            public abstract bool IsSearchMatch(String val);
            public abstract bool TestBetween(String minVal, String maxVal);
            public abstract bool FallsBefore(String value);
            public abstract bool FallsAfter(String value);
            public abstract bool IsRange { get; }
            public abstract bool Contains(String respVal);

            public static CResponseSpecifier CreateFromXml(XmlReader reader)
            {
                EType type = (EType)Enum.Parse(typeof(EType), reader["SpecifierType"]);
                CResponseSpecifier spec = null;
                switch (type)
                {
                    case EType.singleton:
                        spec = new CSingleton();
                        break;

                    case EType.range:
                        spec = new CRange();
                        break;

                    case EType.dateSingleton:
                        spec = new CDateSingleton();
                        break;

                    case EType.dateRange:
                        spec = new CDateRange();
                        break;
                }
                spec.ReadXml(reader);
                return spec;
            }

            public static CResponseSpecifier CreateFromStoredXml(XmlNode node)
            {
                EType type = (EType)Enum.Parse(typeof(EType), node.Attributes["SpecifierType"].Value);
                CResponseSpecifier spec = null;
                switch (type)
                {
                    case EType.singleton:
                        spec = new CSingleton();
                        break;

                    case EType.range:
                        spec = new CRange();
                        break;

                    case EType.dateSingleton:
                        spec = new CDateSingleton();
                        break;

                    case EType.dateRange:
                        spec = new CDateRange();
                        break;
                }
                spec.LoadFromXml(node);
                return spec;
            }

            public abstract void WriteXml(XmlWriter writer);
            public abstract void ReadXml(XmlReader reader);
            public abstract bool LoadFromXml(XmlNode node);
            public abstract void WriteToXml(XmlTextWriter writer);
        }

        public class CSingleton : CResponseSpecifier
        {
            private decimal Value;

            public CSingleton()
            {
                Value = Decimal.MinValue;
            }

            public CSingleton(String val)
            {
                Value = Convert.ToDecimal(val.Trim());
            }

            public CSingleton(decimal val)
            {
                Value = val;
            }

            public override String Specifier
            {
                get
                {
                    return Value.ToString();
                }
            }

            public override bool IsRange
            {
                get
                {
                    return false;
                }
            }

            protected override EType Type
            {
                get {
                    return EType.singleton;
                }
            }

            public override bool IsSearchMatch(String val)
            {
                decimal searchVal = Convert.ToDecimal(val);
                if (Value == searchVal)
                    return true;
                return false;
            }

            public override bool TestBetween(String sMinVal, String sMaxVal)
            {
                decimal minVal = Convert.ToDecimal(sMinVal);
                decimal maxVal = Convert.ToDecimal(sMaxVal);
                if ((Value < minVal) || (Value > maxVal))
                    return false;
                return true;
            }

            public override bool FallsBefore(String value)
            {
                decimal compVal;
                if (value.Contains('-'))
                    compVal = Convert.ToDecimal(value.Substring(0, value.IndexOf('-')));
                else
                    compVal = Convert.ToDecimal(value);
                if (Value < compVal)
                    return true;
                return false;
            }

            public override bool FallsAfter(String value)
            {
                decimal compVal;
                if (value.Contains('-'))
                    compVal = Convert.ToDecimal(value.Substring(0, value.IndexOf('-')));
                else
                    compVal = Convert.ToDecimal(value);
                if (Value > compVal)
                    return true;
                return false;
            }

            public override bool Contains(String respVal)
            {
                decimal dResp = Convert.ToDecimal(respVal);
                if (dResp == Value)
                    return true;
                return false;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("SpecifierType", EType.singleton.ToString());
                writer.WriteElementString("Value", Value.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                Value = Convert.ToDecimal(reader.ReadElementString());
                reader.ReadEndElement();
            }

            public override bool LoadFromXml(XmlNode node)
            {
                Value = Convert.ToDecimal(node.ChildNodes[0].Value);
                return true;
            }

            public override void WriteToXml(XmlTextWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("SpecifierType", EType.singleton.ToString());
                writer.WriteElementString("SingletonValue", Specifier);
                writer.WriteEndElement();
            }
        }

        public class CRange : CResponseSpecifier
        {
            private decimal Min, Max;

            public CRange()
            {
                Min = Max = Decimal.MinValue;
            }

            public CRange(String min, String max)
            {
                Min = Convert.ToDecimal(min);
                Max = Convert.ToDecimal(max);
            }

            public CRange(decimal min, decimal max)
            {
                Min = min;
                Max = max;
            }

            public override String Specifier
            {
                get
                {
                    return String.Format("{0} - {1}", Min, Max);
                }
            }

            public override bool IsRange
            {
                get
                {
                    return true;
                }
            }

            protected override EType Type
            {
                get
                {
                    return EType.range;
                }
            }

            public override bool IsSearchMatch(String sVal)
            {
                decimal respVal = Convert.ToDecimal(sVal);
                if ((respVal >= Min) && (respVal <= Max))
                    return true;
                return false;
            }

            public override bool TestBetween(String sMinVal, String sMaxVal)
            {
                decimal minVal = Convert.ToDecimal(sMinVal);
                decimal maxVal = Convert.ToDecimal(sMaxVal);
                if ((Min < minVal) || (Max > maxVal))
                    return false;
                return true;
            }

            public override bool FallsBefore(String value)
            {
                decimal compVal;
                if (value.Contains('-'))
                    compVal = Convert.ToDecimal(value.Substring(0, value.IndexOf('-')));
                else
                    compVal = Convert.ToDecimal(value);
                if (Max < compVal)
                    return true;
                return false;
            }

            public override bool FallsAfter(String value)
            {
                decimal compVal;
                if (value.Contains('-'))
                    compVal = Convert.ToDecimal(value.Substring(0, value.IndexOf('-')));
                else
                    compVal = Convert.ToDecimal(value);
                if (Min > compVal)
                    return true;
                return false;
            }

            public override bool  Contains(String respVal)
{
                decimal dResp = Convert.ToDecimal(respVal);
                if ((dResp >= Min) && (dResp <= Max))
                    return true;
                return false;
}

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("SpecifierType", EType.range.ToString());
                writer.WriteElementString("MinValue", Min.ToString());
                writer.WriteElementString("MaxValue", Max.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                Min = Convert.ToDecimal(reader.ReadElementString());
                Max = Convert.ToDecimal(reader.ReadElementString());
                reader.ReadEndElement();
            }

            public override bool LoadFromXml(XmlNode node)
            {
                Min = Convert.ToDecimal(node.ChildNodes[0].Value);
                Max = Convert.ToDecimal(node.ChildNodes[1].Value);
                return true;
            }

            public override void WriteToXml(XmlTextWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("SpecifierType", EType.range.ToString());
                writer.WriteElementString("MinValue", Min.ToString());
                writer.WriteElementString("MaxValue", Max.ToString());
                writer.WriteEndElement();
            }
        }

        public class CDateSingleton : CResponseSpecifier
        {
            private DateTime Date;

            public CDateSingleton()
            {
                Date = DateTime.MinValue;
            }

            public CDateSingleton(String val)
            {
                Date = DateFromString(val);
            }

            public CDateSingleton(DateTime val)
            {
                Date = val;
            }

            public override String Specifier
            {
                get
                {
                    return String.Format("{0}/{1}/{2}", Date.Day, Date.Month, Date.Year);
                }
            }

            public override bool IsRange
            {
                get
                {
                    return false;
                }
            }

            protected override CResponseSpecifier.EType Type
            {
                get
                {
                    return EType.dateSingleton;
                }
            }

            public DateTime DateFromString(String val)
            {
                int m, d, y;
                String str = val;
                Regex exp = new Regex("^(0?[1-9]|1[0-2])/(0?[1-9]|[1-2][0-9]|3[0-1])/[1-9][0-9]{3}$");
                if (!exp.IsMatch(str))
                    throw new FormatException(Properties.Resources.sInvalidDateRange);
                m = Convert.ToInt32(str.Substring(0, str.IndexOf("/")));
                str = str.Substring(str.IndexOf("/") + 1);
                d = Convert.ToInt32(str.Substring(0, str.IndexOf("/")));
                str = str.Substring(str.IndexOf("/") + 1);
                y = Convert.ToInt32(str);
                if (DateTime.DaysInMonth(y, m) > d)
                    throw new FormatException(Properties.Resources.sInvalidDaysInMonth);
                DateTime date = new DateTime(y, m, d);
                date.AddHours(-date.Hour);
                date.AddMinutes(-date.Minute);
                date.AddSeconds(-date.Second);
                date.AddMilliseconds(-date.Millisecond);
                return date;
            }

            public override bool IsSearchMatch(String val)
            {
                DateTime d = DateFromString(val);
                if ((d.Year == Date.Year) && (d.Month == Date.Month) && (d.Day == Date.Day))
                    return true;
                return false;
            }

            public override bool TestBetween(String date1, String date2)
            {
                DateTime d1 = DateFromString(date1);
                DateTime d2 = DateFromString(date2);
                if ((Date.CompareTo(d1) < 0) || (Date.CompareTo(d2) > 0))
                    return false;
                return true;
            }

            public override bool FallsBefore(String value)
            {
                DateTime compVal;
                if (value.Contains('-'))
                    compVal = DateFromString(value.Substring(0, value.IndexOf('-')));
                else
                    compVal = DateFromString(value);
                if (Date.CompareTo(compVal) < 0)
                    return true;
                return false;
            }

            public override bool FallsAfter(String value)
            {
                DateTime compVal;
                if (value.Contains('-'))
                    compVal = DateFromString(value.Substring(0, value.IndexOf('-')));
                else
                    compVal = DateFromString(value);
                if (Date.CompareTo(compVal) > 0)
                    return true;
                return false;
            }

            public override bool Contains(String respVal)
            {
                DateTime d = DateTime.Parse(respVal);
                if (d.Date.CompareTo(Date.Date) == 0)
                    return true;
                return false;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("SpecifierType", EType.dateSingleton.ToString());
                writer.WriteElementString("Month", Date.Month.ToString());
                writer.WriteElementString("Day", Date.Day.ToString());
                writer.WriteElementString("Year", Date.Year.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                String str = String.Empty;
                reader.ReadStartElement();
                str += reader.ReadElementString() + "/";
                str += reader.ReadElementString() + "/";
                str += reader.ReadElementString();
                reader.ReadEndElement();
                Date = DateTime.Parse(str);
            }

            public override bool LoadFromXml(XmlNode node)
            {
                Date = DateTime.Parse(node.ChildNodes[0].Value);
                return true;
            }

            public override void WriteToXml(XmlTextWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("SpecifierType", EType.dateSingleton.ToString());
                writer.WriteString(Date.ToString());
                writer.WriteEndElement();
            }
        }

        public class CDateRange : CResponseSpecifier
        {
            private DateTime MinDate, MaxDate;

            public CDateRange()
            {
                MinDate = DateTime.MinValue;
                MaxDate = DateTime.MinValue;
            }

            public CDateRange(String val)
            {
                Regex exp = new Regex("^(0?[1-9]|1[0-2])/(0?[1-9]|[1-2][0-9]|3[0-1])/[1-9][0-9]{3}-(0?[1-9]|1[0-2])/(0?[1-9]|[1-2][0-9]|3[0-1])/[1-9][0-9]{3}$");
                if (!exp.IsMatch(val))
                    throw new FormatException(Properties.Resources.sInvalidDateRange);
                MinDate = DateFromString(val.Substring(0, val.IndexOf("-")));
                MaxDate = DateFromString(val.Substring(val.IndexOf("-") + 1));
            }

            public CDateRange(DateTime min, DateTime max)
            {
                MinDate = min;
                MaxDate = max;
            }

            static public CDateRange Combine(CDateRange r1, CDateRange r2)
            {
                DateTime min, max;
                if (r1.MinDate.CompareTo(r2.MinDate) <= 0)
                    min = r1.MinDate;
                else
                    min = r2.MinDate;
                if (r1.MaxDate.CompareTo(r2.MaxDate) >= 0)
                    max = r1.MaxDate;
                else
                    max = r2.MaxDate;
                return new CDateRange(min, max);
            }

            public override String Specifier
            {
                get
                {
                    return String.Format("{0}/{1}/{2} - {3}/{4}/{5}", MinDate.Month, MinDate.Day, MinDate.Year, MaxDate.Month, MaxDate.Day, MaxDate.Year);
                }
            }

            public override bool IsRange
            {
                get
                {
                    return true;
                }
            }

            protected override EType Type
            {
                get
                {
                    return EType.dateRange;
                }
            }

            public DateTime DateFromString(String val)
            {
                int m, d, y;
                m = Convert.ToInt32(val.Substring(0, val.IndexOf("/")));
                val = val.Substring(val.IndexOf("/") + 1);
                d = Convert.ToInt32(val.Substring(0, val.IndexOf("/")));
                val = val.Substring(val.IndexOf("/") + 1);
                y = Convert.ToInt32(val);
                if (DateTime.DaysInMonth(y, m) > d)
                    throw new FormatException(Properties.Resources.sInvalidDaysInMonth);
                DateTime date = new DateTime(y, m, d);
                date.AddHours(-date.Hour);
                date.AddMinutes(-date.Minute);
                date.AddSeconds(-date.Second);
                date.AddMilliseconds(-date.Millisecond);
                return date;
            }

            public override bool IsSearchMatch(String val)
            {
                DateTime d = DateFromString(val);
                if ((d.CompareTo(MinDate) >= 0) && (d.CompareTo(MaxDate) <= 0))
                    return true;
                return false;
            }

            public override bool TestBetween(String date1, String date2)
            {
                DateTime d1 = DateFromString(date1);
                DateTime d2 = DateFromString(date2);
                if (d1.CompareTo(d2) >= 0)
                    throw new FormatException(Properties.Resources.sDateRangeReversedException);
                if ((MinDate.CompareTo(d1) < 0) || (MaxDate.CompareTo(d2) <= 0))
                    return false;
                else if ((MinDate.CompareTo(d2) >= 0) && (MaxDate.CompareTo(d2) > 0))
                    return true;
                return true;
            }

            public override bool FallsBefore(String value)
            {
                DateTime compVal;
                if (value.Contains('-'))
                    compVal = DateFromString(value.Substring(0, value.IndexOf('-')));
                else
                    compVal = DateFromString(value);
                if (MaxDate.CompareTo(compVal) < 0)
                    return true;
                return false;
            }

            public override bool FallsAfter(String value)
            {
                DateTime compVal;
                if (value.Contains('-'))
                    compVal = DateFromString(value.Substring(0, value.IndexOf('-')));
                else
                    compVal = DateFromString(value);
                if (MinDate.CompareTo(compVal) > 0)
                    return true;
                return false;
            }

            public override bool Contains(String respVal)
            {
                DateTime d = DateTime.Parse(respVal).Date;
                if ((d.CompareTo(MinDate.Date) >= 0) && (d.CompareTo(MaxDate.Date) <= 0))
                    return true;
                return false;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("SpecifierType", EType.dateRange.ToString());
                writer.WriteStartElement("StartDate");
                writer.WriteElementString("Month", MinDate.Month.ToString());
                writer.WriteElementString("Day", MinDate.Day.ToString());
                writer.WriteElementString("Year", MinDate.Day.ToString());
                writer.WriteEndElement();
                writer.WriteStartElement("EndDate");
                writer.WriteElementString("Month", MaxDate.Month.ToString());
                writer.WriteElementString("Day", MaxDate.Day.ToString());
                writer.WriteElementString("Year", MaxDate.Year.ToString());
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                String str = String.Empty;
                reader.ReadStartElement();
                reader.ReadStartElement();
                str += reader.ReadElementString() + "/";
                str += reader.ReadElementString() + "/";
                str += reader.ReadElementString();
                reader.ReadEndElement();
                MinDate = DateTime.Parse(str);
                str = String.Empty;
                reader.ReadStartElement();
                str += reader.ReadElementString() + "/";
                str += reader.ReadElementString() + "/";
                str += reader.ReadElementString();
                reader.ReadEndElement();
                MaxDate = DateTime.Parse(str);
                reader.ReadEndElement();
            }

            public override bool LoadFromXml(XmlNode node)
            {
                MinDate = DateTime.Parse(node.ChildNodes[0].Value);
                MaxDate = DateTime.Parse(node.ChildNodes[1].Value);
                return true;
            }

            public override void WriteToXml(XmlTextWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("SpecifierType", EType.dateRange.ToString());
                writer.WriteElementString("MinDate", MinDate.ToString());
                writer.WriteElementString("MaxDate", MaxDate.ToString());
                writer.WriteEndElement();
            }
        }

        protected Func<String> GetSurveyName = null;
        private ResultSetDescriptor _RSD = null;
        protected ISurveyItemResponse _Response = null;

        protected ResultSetDescriptor RSD
        {
            get
            {
                return RSD;
            }
        }

        public String SurveyName
        {
            get
            {
                if (GetSurveyName == null)
                    return String.Empty;
                return GetSurveyName();
            }
        }

        public enum EType { search, correct, actual, dummy };
        protected IATResultSetNamespaceV1.AnswerState _AnswerState;
        protected EType _Type;
        private String _Name;

        public EType Type
        {
            get
            {
                return _Type;
            }
        }

        protected CResponseObject(EType type, IATSurveyFile.Response resp)
        {
            _Type = type;
            GetSurveyName = new Func<String>(resp.GetSurveyName);
        }

        protected CResponseObject(EType type, ResultSetDescriptor rsd)
        {
            _Type = type;
            _RSD = rsd;    
        }

        protected CResponseObject(EType type, CSurveyItem csi)
        {
            _Type = type;
            GetSurveyName = new Func<String>(csi.GetSurveyName);
        }

        public static CResponseObject CloneResponseObject(CResponseObject obj, IATSurveyFile.Response resp)
        {
            CResponseObject respObj = null;
            switch (resp.ResponseType)
            {
                case IATSurveyFile.ResponseType.Boolean:
                    respObj = new CBoolResponseObject((CBoolResponseObject)obj, (IATSurveyFile.Boolean)resp);
                    break;

                case IATSurveyFile.ResponseType.BoundedLength:
                    respObj = new CBoundedLengthResponseObject((CBoundedLengthResponseObject)obj, (IATSurveyFile.BoundedLength)resp);
                    break;

                case IATSurveyFile.ResponseType.BoundedNum:
                    respObj = new CBoundedNumResponseObject((CBoundedNumResponseObject)obj, (IATSurveyFile.BoundedNum)resp);
                    break;

                case IATSurveyFile.ResponseType.Date:
                    respObj = new CDateResponseObject((CDateResponseObject)obj, (IATSurveyFile.Date)resp);
                    break;

                case IATSurveyFile.ResponseType.FixedDig:
                    respObj = new CFixedDigResponseObject((CFixedDigResponseObject)obj, (IATSurveyFile.FixedDig)resp);
                    break;

                case IATSurveyFile.ResponseType.Likert:
                    respObj = new CLikertResponseObject((CLikertResponseObject)obj, (IATSurveyFile.Likert)resp);
                    break;

                case IATSurveyFile.ResponseType.MultiBoolean:
                    respObj = new CMultiBooleanResponseObject((CMultiBooleanResponseObject)obj, (IATSurveyFile.MultiBoolean)resp);
                    break;

                case IATSurveyFile.ResponseType.Multiple:
                    respObj = new CMultipleResponseObject((CMultipleResponseObject)obj, (IATSurveyFile.Multiple)resp);
                    break;

                case IATSurveyFile.ResponseType.None:
                    respObj = null;
                    break;

                case IATSurveyFile.ResponseType.RegEx:
                    respObj = new CRegExResponseObject((CRegExResponseObject)obj, (IATSurveyFile.RegEx)resp);
                    break;

                case IATSurveyFile.ResponseType.WeightedMultiple:
                    respObj = new CWeightedMultipleResponseObject((CWeightedMultipleResponseObject)obj, (IATSurveyFile.WeightedMultiple)resp);
                    break;
            }
            return respObj;
        }


        public abstract bool IsSearchMatch(String val);
        public abstract ISurveyItemResponse Response { get; set; }
        public abstract Panel GenerateResponseObjectPanel(System.Drawing.Color backColor, System.Drawing.Color foreColor, String fontFamily, float fontSize, int clientWidth);
        public abstract void DisposeOfControls();

        protected virtual List<CResponseSpecifier> ResponseSpecifiers
        {
            get
            {
                throw new NotImplementedException();
            }
        }


        public String GetName()
        {
            return "ResponseObject";
        }
        protected bool bIsNew = true;
        public virtual void UpdateResponseObject() {}
        protected virtual void CopyResponseObject(CResponseObject original) { }
    }
}
