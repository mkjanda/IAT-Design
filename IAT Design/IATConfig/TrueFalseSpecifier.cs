using System;
using System.Xml;

namespace IATClient.IATConfig
{
    class TrueFalseSpecifier : DynamicSpecifier
    {
        public TrueFalseSpecifier() : base(ESpecifierType.TrueFalse) { }

        public TrueFalseSpecifier(int id, String surveyName, int itemNum)
            : base(ESpecifierType.TrueFalse, id, surveyName, itemNum) { }

        public override void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            base.ReadXml(reader);
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("DynamicSpecifier");
            writer.WriteAttributeString("SpecifierType", Type.ToString());
            writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xsi:type", "TrueFalseSpecifier");
            base.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
