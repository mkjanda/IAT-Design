using IATClient.IATConfig;
using IATClient.ResultData;
using System;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CInstruction represents a null response type, it is assigned to a CSurveyItem object 
    /// that does not request a response
    /// </summary>
    class CInstruction : CResponse
    {
        /// <summary>
        /// The default constructor
        /// </summary>
        public CInstruction()
            : base(EResponseType.Instruction)
        { }

        public override object Clone()
        {
            return new CInstruction();
        }

        /// <summary>
        /// Necessary for infrastructure, not that the IsValid functions are ever called that I know of
        /// </summary>
        /// <returns>"true" -- CInstruction objects contain no data other than the response type</returns>
        public override bool IsValid()
        {
            return true;
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Response");
            writer.WriteAttributeString("Type", sTypeInstruction);
            writer.WriteEndElement();
        }

        public override XElement AsXElement() => new XElement("Response", Format.AsXElement());

        public override void Load(XElement elem)
        {
            Format.Load(elem.Element("Format"));
        }

        public override string GetResponseDesc()
        {
            return String.Empty;
        }

        public override Response GenerateSerializableResponse(SurveyItem parentItem)
        {
            return new Response(parentItem);
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            return new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.None);
        }
    }
}
