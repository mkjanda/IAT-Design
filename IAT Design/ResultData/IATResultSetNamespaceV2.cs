using System;
using System.Xml;

namespace IATClient.ResultData
{
    namespace IATResultSetNamespaceV2
    {
        public class IATItemResponse : IATResultSetNamespaceV1.IATItemResponse
        {
            protected bool _Error;

            public override bool Error
            {
                get
                {
                    return _Error;
                }
                set
                {
                    _Error = value;
                }
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                BlockNumber = reader.ReadElementContentAsInt();
                ItemNumber = reader.ReadElementContentAsInt();
                ResponseTime = reader.ReadElementContentAsLong();
                PresentationNumber = reader.ReadElementContentAsInt();
                _Error = reader.ReadElementContentAsBoolean();
                reader.ReadEndElement();
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATResultSetElement");
                writer.WriteElementString("BlockNum", BlockNumber.ToString());
                writer.WriteElementString("ItemNum", ItemNumber.ToString());
                writer.WriteElementString("ResponseTime", ResponseTime.ToString());
                writer.WriteElementString("Error", Error.ToString());
                writer.WriteElementString("PresentationNum", PresentationNumber.ToString());
                writer.WriteEndElement();
            }
        }

    }
}
