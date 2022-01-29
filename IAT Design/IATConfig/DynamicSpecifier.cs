using System;
using System.Xml;

namespace IATClient.IATConfig
{
    public abstract class DynamicSpecifier
    {
        private int _ID;
        private String _SurveyName;
        private int _ItemNum;
        public enum ESpecifierType { None, Range, Selection, Mask, TrueFalse };
        protected ESpecifierType Type;

        public int ID
        {
            get
            {
                return _ID;
            }
            set
            {
                _ID = value;
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

        public int ItemNum
        {
            get
            {
                return _ItemNum;
            }
            set
            {
                _ItemNum = value;
            }
        }

        public DynamicSpecifier(ESpecifierType specifierType)
        {
            Type = specifierType;
            _ID = -1;
            _SurveyName = String.Empty;
            _ItemNum = 0;
        }

        public DynamicSpecifier(ESpecifierType specifierType, int id, String surveyName, int itemNum)
        {
            Type = specifierType;
            ID = id;
            SurveyName = surveyName;
            ItemNum = itemNum;
        }

        public static DynamicSpecifier CreateFromXml(XmlReader reader)
        {
            ESpecifierType type = (ESpecifierType)Enum.Parse(typeof(ESpecifierType), reader["SpecifierType"]);
            DynamicSpecifier specifier = null;
            switch (type)
            {
                case ESpecifierType.Range:
                    specifier = new RangeSpecifier();
                    break;

                case ESpecifierType.Mask:
                    specifier = new MaskSpecifier();
                    break;

                case ESpecifierType.Selection:
                    specifier = new SelectionSpecifier();
                    break;

                case ESpecifierType.TrueFalse:
                    specifier = new TrueFalseSpecifier();
                    break;
            }
            specifier.ReadXml(reader);
            return specifier;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            ID = Convert.ToInt32(reader.ReadElementString());
            SurveyName = reader.ReadElementString();
            ItemNum = Convert.ToInt32(reader.ReadElementString());
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteElementString("TestSpecifierID", ID.ToString());
            writer.WriteElementString("SurveyName", SurveyName);
            writer.WriteElementString("ItemNum", ItemNum.ToString());
        }
    }
}
