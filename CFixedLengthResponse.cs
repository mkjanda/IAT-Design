using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace IATClient
{
    /// <summary>
    /// CFixedLengthResponse provides a class that represents a definition for a text response
    /// that consists of a set number of characters
    /// </summary>
    public class CFixedLengthResponse : CResponse
    {
        // the set length of the text to be provided 
        private int _nLength;

        /// <summary>
        /// gets or sets the length of the text to be provided
        /// </summary>
        public int Length
        {
            get
            {
                return _nLength;
            }
            set
            {
                _nLength = value;
            }
        }

        /// <summary>
        /// The "Invalid Value" that indicates the Length of the response is unset
        /// </summary>
        public const int InvalidValue = int.MinValue;

        /// <summary>
        /// The default constructor
        /// </summary>
        public CFixedLengthResponse()
            : base(EResponseType.FixedLength)
        {
            _nLength = InvalidValue;
        }

        /// <summary>
        /// This constructor sets the length of the text to be provided equal to the passed parameter
        /// </summary>
        /// <param name="length">The length of the text to be provided</param>
        public CFixedLengthResponse(int length)
            : base(EResponseType.FixedLength)
        {
            _nLength = length;
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CFixedLengthResponse object to be copied</param>
        public CFixedLengthResponse(CFixedLengthResponse r)
            : base(EResponseType.FixedLength)
        {
            _nLength = r._nLength;
        }

        /// <summary>
        /// Validates the object's data
        /// </summary>
        /// <returns>"true" if the set Length is positive and not equal to CFixedLengthResponse.InvalidValue, "false" otherwise</returns>
        public override bool IsValid()
        {
            if ((Length > 0) && (Length != InvalidValue))
                return true;
            return false;
        }

        /// <summary>
        /// writes the object's data memebers to the passed XmlTextWriter
        /// </summary>
        /// <param name="writer">The XmlTextWriter object to use for output</param>
        public override void WriteToXml(XmlTextWriter writer)
        {
            // write the start of the "Response" element to deliniate the start of a new response type
            writer.WriteStartElement("Response");

            // write the type of the response as an attribute of the "Response" element
            writer.WriteStartAttribute("Type");
            writer.WriteString(sTypeFixedLength);
            writer.WriteEndAttribute();

            // write the length of the response
            writer.WriteElementString("Length", Length.ToString());
            
            // write the end of the "Response" element
            writer.WriteEndElement();
        }

        /// <summary>
        /// Loads the object's data members from the passed XmlNode.
        /// </summary>
        /// <param name="node">The XmlNode object to load data from</param>
        /// <returns>"true" on success, "false" on error</returns>
        public override bool LoadFromXml(XmlNode node)
        {
            // ensure that node has only one child node
            if (node.ChildNodes.Count != 1)
                return false; // return false otherwise

            // load Length from the only child node
            Length = Convert.ToInt32(node.FirstChild.InnerText);
            
            // return "true" on success
            return true;
        }
    }
}
