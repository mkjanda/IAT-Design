using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace IATClient
{
    /// <summary>
    /// CMaxLengthResponse provides a class that represents a definition for a text response with a length
    /// less than or equal to a specified number of digits
    /// </summary>
    public class CMaxLengthResponse : CTextResponse
    {
        // the maximum length of the text to be entered
        private int _nMaxLength;

        /// <summary>
        /// gets or sets the maximum length of the text to be entered
        /// </summary>
        public int MaxLength
        {
            get
            {
                return _nMaxLength;
            }
            set
            {
                _nMaxLength = value;
            }
        }

        /// <summary>
        /// An "Invalid Value" that MaxLength can be set to
        /// </summary>
        public const int InvalidValue = int.MinValue;

        /// <summary>
        /// Instantiates a CMaxLengthResponse object with a maximum length of CMaxLengthResponse.InvalidValue
        /// </summary>
        public CMaxLengthResponse()
            : base(EResponseType.MaxLength)
        {
            MaxLength = InvalidValue;
        }

        /// <summary>
        /// Instantiates a CMaxLengthResponse object with a maximum length equal to the passed value
        /// </summary>
        /// <param name="maxLength">The maximum length of the text to be entered</param>
        public CMaxLengthResponse(int maxLength)
            : base(EResponseType.MaxLength)
        {
            MaxLength = maxLength;
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CMaxLengthResponse object to be copied</param>
        public CMaxLengthResponse(CMaxLengthResponse r)
            : base(EResponseType.MaxLength)
        {
            _nMaxLength = r._nMaxLength;
        }

        /// <summary>
        /// Validates the object's data
        /// </summary>
        /// <returns>"true" if MaxLength is positive and not equal to CMaxLengthResponse.InvalidValue, otherwise "false"</returns>
        public override bool IsValid()
        {
            if ((MaxLength > 0) && (MaxLength != InvalidValue))
                return true;
            return false;
        }

        /// <summary>
        /// Writes the object's data members to the given XmlTextWriter
        /// </summary>
        /// <param name="writer">The XmlTextWriter object to use for output</param>
        public override void WriteToXml(XmlTextWriter writer)
        {
            // write the start of the "Response" element to signify the beginning of a new response type
            writer.WriteStartElement("Response");

            // write the type of the response as an attribute of the "Response" element
            writer.WriteStartAttribute("Type");
            writer.WriteString(sTypeMaxLength);
            writer.WriteEndAttribute();

            // write the value of MaxLength
            writer.WriteElementString("MaxLength", MaxLength.ToString());

            // close the "Response" element
            writer.WriteEndElement();
        }

        /// <summary>
        /// Loads data into the object from the passed XmlNode
        /// </summary>
        /// <param name="node">The XmlNode object to load data from</param>
        /// <returns>"true" on success, "false" on error</returns>
        public override bool LoadFromXml(XmlNode node)
        {
            // ensure node has the appropriate number of child nodes
            if (node.ChildNodes.Count != 1)
                return false;  // return false otherwise

            // load the maximum length from the only child node
            MaxLength = Convert.ToInt32(node.FirstChild.InnerText);

            // return "true" on success
            return true;
        }
    }
}
