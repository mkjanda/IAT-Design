using IATClient.IATConfig;
using IATClient.ResultData;
using System;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CRegExResponse provides a class that can define a response that matches a given regular expression
    /// </summary>
    class CRegExResponse : CTextResponse
    {
        // the regular expression that the response type must match
        private String _RegEx;

        /// <summary>
        /// gets or sets the regular expression that the response type must match
        /// </summary>
        public String RegEx
        {
            get
            {
                return _RegEx;
            }
            set
            {
                _RegEx = value;
            }
        }

        /// <summary>
        /// Instantiates a CRegExResponse object with an empty regular expression
        /// </summary>
        public CRegExResponse()
            : base(EResponseType.RegEx)
        {
            RegEx = String.Empty;
        }

        /// <summary>
        /// Instantiates a CRegExResponse object with a given regular expression
        /// </summary>
        /// <param name="exp">The regular expression to assign to the object</param>
        public CRegExResponse(String exp)
            : base(EResponseType.RegEx)
        {
            RegEx = exp;
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CRegExResponse object to copy</param>
        public CRegExResponse(CRegExResponse r)
            : base(EResponseType.RegEx, r)
        {
            _RegEx = r._RegEx;
        }

        public override object Clone()
        {
            return new CRegExResponse(this);
        }

        /// <summary>
        /// Tests the objects data members to make sure they are valid
        /// </summary>
        /// <returns>"true" if the data members are valid, "false" otherwise</returns>
        public override bool IsValid()
        {
            if (RegEx != String.Empty)
                return true;
            return false;
        }

        public String GetRegEx()
        {
            return RegEx;
        }

        public override XElement AsXElement() => new XElement("Response", new XElement("Expression", RegEx), Format.AsXElement());

        public override void Load(XElement elem)
        {
            RegEx = elem.Element("Expression").Value;
            Format.Load(elem.Element("Format"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            // write the start of the "Response" element to signify the beginning of a new response type
            writer.WriteStartElement("Response");
            writer.WriteStartAttribute("Type");
            writer.WriteString(sTypeRegEx);
            writer.WriteEndAttribute();

            Format.WriteXml(writer);

            // write the type of the response as an attribute of the "Response" element

            // write the regular expression the input must match and close the "Response" element
            writer.WriteElementString("Expression", RegEx);
            writer.WriteEndElement();
        }

        public override string GetResponseDesc()
        {
            return String.Format("\tText that matches the regular expression \"{0}\"\r\n", RegEx);
        }

        public override Response GenerateSerializableResponse(SurveyItem parentItem)
        {
            RegEx r = new RegEx(parentItem);
            r.RegularExpression = RegEx;
            return r;
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            return new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.None);
        }
    }
}
