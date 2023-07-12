using System.Xml;
using System.Xml.Linq;

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

        public override void Load(XElement elem)
        {
            return;
        }

    }
}
