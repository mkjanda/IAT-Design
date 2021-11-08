using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IATClient
{
    class PurchaseInitiation : INamedXmlSerializable
    {
        private String productKey;

        public PurchaseInitiation()
        {
            productKey = LocalStorage.Activation[LocalStorage.Field.ProductKey];
        }

        public String GetName()
        {
            return "PurchaseInitiation";
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("PurchaseInitiation");
            writer.WriteElementString("ProductKey", productKey);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            productKey = reader.ReadElementString("ProductKey");
            reader.ReadEndElement();
        }

       
    }
}
