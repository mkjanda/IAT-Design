using System;
using System.Xml;
using System.Xml.Schema;

namespace IATClient
{
    class TokenDefinition : INamedXmlSerializable
    {
        private String _TokenName;
        private ETokenType _TokenType;

        public String TokenName
        {
            get
            {
                return _TokenName;
            }
        }

        public ETokenType TokenType
        {
            get
            {
                return _TokenType;
            }
        }

        public TokenDefinition(ETokenType tokenType, String tokenName)
        {
            _TokenType = tokenType;
            _TokenName = tokenName;
        }

        public void ReadXml(XmlReader reader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetName());
            writer.WriteElementString("TokenType", TokenType.ToString());
            writer.WriteElementString("TokenName", TokenName);
            writer.WriteEndElement();
        }

        public String GetName()
        {
            return "TokenDefinition";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
