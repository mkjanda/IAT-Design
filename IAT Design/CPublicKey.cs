using System;
using System.Xml;
using System.Xml.Schema;

namespace IATClient
{
    class CPublicKey : INamedXmlSerializable
    {
        private byte[] _Modulus, _Exponent;

        public byte[] Modulus
        {
            get
            {
                return _Modulus;
            }
        }

        public byte[] Exponent
        {
            get
            {
                return _Exponent;
            }
        }

        public CPublicKey(byte[] modulus, byte[] exponent)
        {
            _Modulus = (byte[])modulus.Clone();
            _Exponent = (byte[])exponent.Clone();
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            _Modulus = Convert.FromBase64String(reader.ReadElementString());
            _Exponent = Convert.FromBase64String(reader.ReadElementString());
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("PublicKey");
            writer.WriteElementString("Modulus", Convert.ToBase64String(Modulus));
            writer.WriteElementString("Exponent", Convert.ToBase64String(Exponent));
            writer.WriteEndElement();
        }

        public String GetName()
        {
            return "PublicKey";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
