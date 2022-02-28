using IATClient.IATConfig;
using IATClient.ResultData;
using System;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CBoundedLengthResponse provides a class that represents a definition for a text response
    /// with a specified minimum and maximum length
    /// </summary>
    class CBoundedLengthResponse : CTextResponse
    {
        // the minimum and maximum length of the text to be provided by the survey taker
        private int _nMinLength, _nMaxLength;

        /// <summary>
        /// gets or sets the minimum length of the text to be provided by the survey taker
        /// </summary>
        public int MinLength
        {
            get
            {
                return _nMinLength;
            }
            set
            {
                _nMinLength = value;
            }
        }

        /// <summary>
        /// gets or sets the maximum length of the text to be provided by the survey taker
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

        public CResponseObject.CResponseSpecifier GetBounds()
        {
            return new CResponseObject.CRange(MinLength.ToString(), MaxLength.ToString());
        }

        /// <summary>
        /// the "invalid value" for MinLength and MaxLength
        /// </summary>
        public const int InvalidValue = int.MinValue;


        /// <summary>
        /// The default constructor.  assigns the invalid value to MinLength and MaxLength and
        /// calls the base class constructor with the appropriate enumerated response type
        /// </summary>
        public CBoundedLengthResponse()
            : base(EResponseType.BoundedLength)
        {
            _nMinLength = _nMaxLength = 0;
        }


        /// <summary>
        /// This constructor assigns the passed values to MinLength and MaxLength and
        /// calls the base class constructor with the appropriate enumerated response type
        /// </summary>
        /// <param name="minLength">The minimum length of the text to be provided by the survey taker</param>
        /// <param name="maxLength">The maximum length of the text to be provided by the survey taker</param>
        public CBoundedLengthResponse(int minLength, int maxLength)
            : base(EResponseType.BoundedLength)
        {
            _nMinLength = minLength;
            _nMaxLength = maxLength;
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CBoundedLengthResponse object to be copied</param>
        public CBoundedLengthResponse(CBoundedLengthResponse r)
            : base(EResponseType.BoundedLength, r)
        {
            _nMinLength = r._nMinLength;
            _nMaxLength = r._nMaxLength;
        }

        public override object Clone()
        {
            return new CBoundedLengthResponse(this);
        }


        /// <summary>
        /// Tests MinValue and MaxValue against the invalid value and returns "true" if both
        /// contain valid values and if MinLength is less than MaxLength, "false" otherwise
        /// </summary>
        /// <returns>"true" if the object contains valid data, otherwise "false"</returns>
        public override bool IsValid()
        {
            if ((MinLength != InvalidValue) && (MaxLength != InvalidValue) && (MinLength < MaxLength))
                return true;
            return false;
        }

        public int GetMinChars()
        {
            return MinLength;
        }

        public int GetMaxChars()
        {
            return MaxLength;
        }

        public override XElement AsXElement() => new XElement("Response", new XAttribute("MinLength", MinLength.ToString()), new XAttribute("MaxLength", MaxLength.ToString()), Format.AsXElement());

        public override void Load(XElement elem)
        {
            MinLength = Convert.ToInt32(elem.Attribute("MinLength").Value);
            MaxLength = Convert.ToInt32(elem.Attribute("MaxLength").Value);
            Format.Load(elem.Element("Format"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Response");
            writer.WriteAttributeString("Type", sTypeBoundedLength);
            Format.WriteXml(writer);
            writer.WriteElementString("MinLength", MinLength.ToString());
            writer.WriteElementString("MaxLength", MaxLength.ToString());
            writer.WriteEndElement();
        }


        public override string GetResponseDesc()
        {
            return String.Format("\tA string of text between {0} and {1} characters in length\r\n", MinLength, MaxLength);
        }

        public override Response GenerateSerializableResponse(SurveyItem parentItem)
        {
            BoundedLength r = new BoundedLength(parentItem);
            r.MinLength = MinLength;
            r.MaxLength = MaxLength;

            return r;
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            return new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.None);
        }
    }
}
