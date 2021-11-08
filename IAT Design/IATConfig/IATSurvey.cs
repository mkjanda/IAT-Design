using System;
using System.Collections.Generic;
using System.Xml;

namespace IATClient.IATConfig
{
    public class IATSurvey : INamedXmlSerializable
    {
        private String _SurveyName;
        private int _NumItems;
        private int _AlternationSet;
        private int _NumOnceOnlyResponses;
        private String _FileNameBase;
        public enum EType { BeforeSurvey, AfterSurvey };
        private EType _Type;
        private List<CResponse.EResponseType> _ResponseTypes = new List<CResponse.EResponseType>();

        public EType SurveyType
        {
            get
            {
                return _Type;
            }
        }

        public List<CResponse.EResponseType> ResponseTypes
        {
            get
            {
                return _ResponseTypes;
            }
        }

        public String FileNameBase
        {
            get
            {
                return _FileNameBase;
            }
        }

        public String SurveyName
        {
            get
            {
                return _SurveyName;
            }
        }

        public int NumItems
        {
            get
            {
                return _NumItems;
            }
        }


        public bool IsBeforeSurvey
        {
            get
            {
                return (_Type == EType.BeforeSurvey);
            }
        }

        public bool IsAfterSurvey
        {
            get
            {
                return (_Type == EType.AfterSurvey);
            }
        }

        public int AlternationSet
        {
            get
            {
                return _AlternationSet;
            }
            set
            {
                _AlternationSet = value;
            }
        }

        public int NumOnceOnlyResponses
        {
            get
            {
                return _NumOnceOnlyResponses;
            }
        }

        protected IATSurvey()
        {
            _NumItems = 0;
            _AlternationSet = -1;
            _SurveyName = String.Empty;
            _FileNameBase = String.Empty;
            _NumOnceOnlyResponses = 0;
        }

        protected IATSurvey(EType type)
        {
            _NumItems = 0;
            _AlternationSet = -1;
            _SurveyName = String.Empty;
            _FileNameBase = String.Empty;
            _NumOnceOnlyResponses = 0;
            _Type = type;
        }

        public static IATSurvey GetIATSurvey(EType type)
        {
            return new IATSurvey(type);
        }

        public static IATSurvey GetIATSurvey(XmlReader xReader)
        {
            IATSurvey s = new IATSurvey();
            s.ReadXml(xReader);
            return s;
        }

        public IATSurvey(CSurvey s, int surveyNum, EType type)
        {
            _NumItems = 0;
            if (s.AlternationGroup != null)
                _AlternationSet = s.AlternationGroup.AlternationPriority;
            else
                _AlternationSet = -1;
            _SurveyName = s.Name;
            _FileNameBase = s.FileNameBase;
            for (int ctr = 0; ctr < s.Items.Count; ctr++)
                if (!s.Items[ctr].IsCaption)
                {
                    if (s.Items[ctr].Response.ResponseType != CResponse.EResponseType.Instruction)
                    {
                        _NumItems++;
                        _ResponseTypes.Add(s.Items[ctr].Response.ResponseType);
                    }
                    /*
                    if ((s.Items[ctr].Response.ResponseType == CResponse.EResponseType.FixedDig) &&
                        (s.Items[ctr].IsScored))
                    {
                        if (((CFixedDigResponse)s.Items[ctr].Response).IsOneUse)
                            AddUniqueIDResponse(surveyNum + ctr, NumItems, (CFixedDigResponse)s.Items[ctr].Response.DefinedResponse);
                    }
                    */
                }
            _NumOnceOnlyResponses = s.NumOnceOnlyResponses;
            _Type = type;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("IATSurvey");
            writer.WriteAttributeString("SurveyType", _Type.ToString());
            writer.WriteElementString("SurveyName", SurveyName);
            writer.WriteElementString("FileNameBase", FileNameBase);
            writer.WriteElementString("NumItems", NumItems.ToString());
            writer.WriteElementString("AlternationSet", AlternationSet.ToString());
            //            writer.WriteElementString("NumOnceOnlyResponses", NumOnceOnlyResponses.ToString());
            for (int ctr = 0; ctr < _ResponseTypes.Count; ctr++)
                writer.WriteElementString("ResponseType", _ResponseTypes[ctr].ToString());
            writer.WriteEndElement();
        }

        public String GetName()
        {
            return "IATSurvey";
        }

        public void ReadXml(XmlReader reader)
        {
            _Type = (EType)Enum.Parse(typeof(EType), reader["SurveyType"]);
            reader.ReadStartElement();
            _SurveyName = reader.ReadElementString();
            _FileNameBase = reader.ReadElementString();
            _NumItems = Convert.ToInt32(reader.ReadElementString());
            _AlternationSet = Convert.ToInt32(reader.ReadElementString());
            _ResponseTypes.Clear();
            while (reader.Name == "ResponseType")
                _ResponseTypes.Add((CResponse.EResponseType)Enum.Parse(typeof(CResponse.EResponseType), reader.ReadElementString()));
            reader.ReadEndElement();
        }
    }
}
