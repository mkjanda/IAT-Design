using System;
using System.Xml;
using System.Xml.Schema;

namespace IATClient.Messages
{

    class ActivationRequest : INamedXmlSerializable
    {
        private String _ActivationCode;
        private String _FName = String.Empty, _LName = String.Empty, _EMail = String.Empty, _Title = String.Empty;

        public String ActivationCode
        {
            get
            {
                return _ActivationCode;
            }
            set
            {
                _ActivationCode = value;
            }
        }

        public String EMail
        {
            get
            {
                return _EMail;
            }
            set
            {
                _EMail = value;
            }
        }

        public String FName
        {
            get
            {
                return _FName;
            }
            set
            {
                _FName = value;
            }
        }

        public String LName
        {
            get
            {
                return _LName;
            }
            set
            {
                _LName = value;
            }
        }

        public String Title
        {
            get
            {
                return _Title;
            }
            set
            {
                _Title = value;
            }
        }


        public ActivationRequest()
        {
            _ActivationCode = String.Empty;
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            _ActivationCode = reader.ReadElementString();
            _EMail = reader.ReadElementString();
            _FName = reader.ReadElementString();
            _LName = reader.ReadElementString();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("ActivationRequest");
            writer.WriteElementString("ProductCode", ActivationCode);
            writer.WriteElementString("EMail", EMail);
            writer.WriteElementString("Title", Title);
            writer.WriteElementString("FName", FName);
            writer.WriteElementString("LName", LName);
            writer.WriteEndElement();
        }

        public String GetName()
        {
            return "ActivationRequest";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
