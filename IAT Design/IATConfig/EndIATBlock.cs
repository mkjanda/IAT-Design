using System;
using System.Xml;

namespace IATClient.IATConfig
{
    class EndIATBlock : IATEvent
    {
        public EndIATBlock()
            : base(EEventType.EndIATBlock)
        {
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("EndIATBlock");
            writer.WriteElementString("DummyValue", "abcdefg");
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            reader.ReadElementString(); // read dummy value
            reader.ReadEndElement();
        }
    }
}
