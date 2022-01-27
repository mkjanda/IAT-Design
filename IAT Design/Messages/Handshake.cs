using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Security.Cryptography;
using System.Xml.Schema;

namespace IATClient.Messages
{
    class HandShake : INamedXmlSerializable
    {
        private static readonly RSACryptoServiceProvider rsaCrypt;

        private String Value { get; set; }

        static HandShake()
        {
            rsaCrypt = new RSACryptoServiceProvider();
            rsaCrypt.ImportCspBlob(Convert.FromBase64String(Properties.Resources.HandshakeCSP));
        }

        public HandShake()
        {
        }

        public static HandShake CreateResponse(HandShake hand)
        {
            HandShake resp = new HandShake();
            resp.Value = Convert.ToBase64String(rsaCrypt.Decrypt(Convert.FromBase64String(hand.Value), false));
            return resp;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Handshake");
            writer.WriteElementString("Value", Value);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement("Handshake");
            Value = reader.ReadElementString("Value");
            reader.ReadEndElement();
        }

        public String GetName()
        {
            return "Handshake";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
