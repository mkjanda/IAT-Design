using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    class XmlAddress : IXmlSerializable
    {
        private String _Host;
        private int _Port;

        public String Host
        {
            get
            {
                return _Host;
            }
        }

        public int Port
        {
            get
            {
                return _Port;
            }
        }

        public XmlAddress()
        {
            _Host = String.Empty;
            _Port = -1;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            _Host = reader.ReadElementString();
            _Port = Convert.ToInt32(reader.ReadElementString());
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("XMLAddress");
            writer.WriteElementString("Host", Host);
            writer.WriteElementString("Port", Port.ToString());
            writer.WriteEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
