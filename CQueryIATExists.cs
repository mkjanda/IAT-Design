using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace IATClient
{
    class CQueryIATExists : INamedXmlSerializable
    {
        private String _ProductKey;
        private String _TestName;

        public String ProductKey
        {
            get
            {
                return _ProductKey;
            }
        }

        public String TestName
        {
            get
            {
                return _TestName;
            }
        }

        public CQueryIATExists(String testName)
        {
            _TestName = testName;
            _ProductKey = LocalStorage.Activation[LocalStorage.Field.ProductKey];
        }

        public String GetName()
        {
            return "QueryIATExists";
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("QueryIATExists");
            writer.WriteElementString("ProductKey", ProductKey);
            writer.WriteElementString("IATName", TestName);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

    }
}
