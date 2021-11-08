using System;
using System.Xml;

namespace IATClient.IATConfig
{
    class RangeSpecifier : DynamicSpecifier
    {
        private int _Cutoff;
        private bool _IsReverseScored;
        private bool _CutoffExcludes;

        public bool CutoffExcludes
        {
            get
            {
                return _CutoffExcludes;
            }
            set
            {
                _CutoffExcludes = value;
            }
        }


        public bool IsReverseScored
        {
            get
            {
                return _IsReverseScored;
            }
            set
            {
                _IsReverseScored = value;
            }
        }

        public int Cutoff
        {
            get
            {
                return _Cutoff;
            }
            set
            {
                _Cutoff = value;
            }
        }

        public RangeSpecifier()
            : base(ESpecifierType.Range)
        {
            _Cutoff = -1;
            _IsReverseScored = false;
        }

        public RangeSpecifier(int id, String surveyName, int itemNum, int cutoff, bool cutoffExcludes, bool reverseScored)
            : base(ESpecifierType.Range, id, surveyName, itemNum)
        {
            _Cutoff = cutoff;
            _CutoffExcludes = cutoffExcludes;
            _IsReverseScored = reverseScored;
        }

        public override void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            base.ReadXml(reader);
            Cutoff = Convert.ToInt32(reader.ReadElementString());
            CutoffExcludes = Convert.ToBoolean(reader.ReadElementString());
            IsReverseScored = Convert.ToBoolean(reader.ReadElementString());
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("DynamicSpecifier");
            writer.WriteAttributeString("SpecifierType", Type.ToString());
            writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xsi:type", "RangeSpecifier");
            base.WriteXml(writer);
            writer.WriteElementString("Cutoff", Cutoff.ToString());
            writer.WriteElementString("CutoffExcludes", CutoffExcludes.ToString());
            writer.WriteElementString("IsReverseScored", IsReverseScored.ToString());
            writer.WriteEndElement();
        }
    }
}
