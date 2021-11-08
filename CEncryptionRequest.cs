using System;
using System.Collections.Generic;

using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    class CEncryptionRequest : INamedXmlSerializable
    {
        private String UserPassword = String.Empty;

        public CEncryptionRequest(String userPassword)
        {
            UserPassword = userPassword;
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            UserPassword = reader.ReadElementString();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("EncryptionRequest");
            writer.WriteElementString("UserPassword", UserPassword);
            writer.WriteEndElement();
        }

        public String GetName()
        {
            return "EncryptionRequest";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
