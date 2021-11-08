using System;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    class HandShake : INamedXmlSerializable
    {
        private long _HandshakeID;
        private long _Value1, _Value2;
        private bool _Valid = true;

        public bool Valid
        {
            get
            {
                return _Valid;
            }
        }

        public long Value1
        {
            get
            {
                return _Value1;
            }
            set
            {
                _Value1 = value;
            }
        }

        public long Value2
        {
            get
            {
                return _Value2;
            }
            set
            {
                _Value2 = value;
            }
        }

        public long HandshakeID
        {
            get
            {
                return _HandshakeID;
            }
            set
            {
                _HandshakeID = value;
            }
        }

        public HandShake()
        {
            _Value1 = _Value2 = 0;
        }

        public static HandShake CreateResponse(HandShake hand)
        {
            HandShake resp = new HandShake();
            double a = (double)hand.Value1 * 3.14159;
            double b = (double)hand.Value2 * 3.14159;
            Random rand = new Random();
            resp.Value1 = (long)Math.Floor(Math.Pow(.5 * (Math.Exp(a) * Math.Sin(b) - Math.Exp(-a) * Math.Sin(-b)), (1 / 3.14159)));
            resp.Value1 <<= 8;
            resp.Value1 |= (byte)rand.Next(256);
            resp.Value2 = (long)Math.Floor(Math.Pow(.5 * (Math.Exp(a) * Math.Cos(b) - Math.Exp(-a) * Math.Cos(-b)), (1 / 3.14159)));
            resp.Value2 <<= 8;
            resp.Value2 |= (byte)rand.Next(256);
            resp.HandshakeID = hand.HandshakeID;
            return resp;
        }

        public HandShake(long v1, long v2)
        {
            Value1 = v1;
            Value2 = v2;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Handshake");
            writer.WriteElementString("Value1", Value1.ToString());
            writer.WriteElementString("Value2", Value2.ToString());
            writer.WriteElementString("HandshakeID", HandshakeID.ToString());
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            Value1 = Convert.ToInt64(reader.ReadElementString());
            Value2 = Convert.ToInt64(reader.ReadElementString());
            HandshakeID = Convert.ToInt64(reader.ReadElementString());
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