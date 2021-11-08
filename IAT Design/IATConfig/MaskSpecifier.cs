using System;
using System.Xml;

namespace IATClient.IATConfig
{
    class MaskSpecifier : DynamicSpecifier
    {
        private String _Mask;

        public String Mask
        {
            get
            {
                return _Mask;
            }
            set
            {
                _Mask = value;
            }
        }

        public MaskSpecifier()
            : base(ESpecifierType.Mask)
        {
            Mask = String.Empty;
        }

        public MaskSpecifier(int id, String surveyName, int itemNum, String mask)
            : base(ESpecifierType.Mask, id, surveyName, itemNum)
        {
            Mask = mask;
        }

        public override void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            base.ReadXml(reader);
            Mask = reader.ReadElementString();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("DynamicSpecifier");
            writer.WriteAttributeString("SpecifierType", Type.ToString());
            writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xsi:type", "MaskSpecifier");
            base.WriteXml(writer);
            writer.WriteElementString("Mask", Mask);
            writer.WriteEndElement();
        }
    }
}
