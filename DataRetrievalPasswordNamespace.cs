using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    class PasswordEntry
    {
        private String _Password;

        public String Password
        {
            get
            {
                return _Password;
            }
            set
            {
                _Password = value;
            }
        }

        public PasswordEntry()
        {
            _Password = String.Empty;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("PasswordEntry");
            writer.WriteElementString("Password", Password);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            Password = reader.ReadElementString();
            reader.ReadEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}