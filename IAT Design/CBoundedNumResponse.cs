using IATClient.IATConfig;
using IATClient.ResultData;
using System;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CBoundedLengthResponse provides a class that represents a definition for an integer response
    /// with a specified minimum and maximum value
    /// </summary>
    class CBoundedNumResponse : CTextResponse
    {
        // the minimum and maximum integer values that the survey taker can enter
        private decimal _dMinValue, _dMaxValue;

        /// <summary>
        /// gets or sets the minimum value that the survey taker can enter
        /// </summary>
        public decimal MinValue
        {
            get
            {
                return _dMinValue;
            }
            set
            {
                _dMinValue = value;
            }
        }

        /// <summary>
        /// gets or sets the maximum value that the survey taker can enter
        /// </summary>
        public decimal MaxValue
        {
            get
            {
                return _dMaxValue;
            }
            set
            {
                _dMaxValue = value;
            }
        }

        public decimal GetMinBound()
        {
            return MinValue;
        }

        public decimal GetMaxBound()
        {
            return MaxValue;
        }

        public CResponseObject.CResponseSpecifier GetBounds()
        {
            return new CResponseObject.CRange(MinValue.ToString(), MaxValue.ToString());
        }

        /// <summary>
        /// The "invalid value" for both MinValue and MaxValue
        /// </summary>
        public const decimal InvalidValue = decimal.MinusOne;

        /// <summary>
        /// The default constructor.  Assigns InvalidValue to both MinLength and MaxLength
        /// </summary>
        public CBoundedNumResponse()
            : base(EResponseType.BoundedNum)
        {
            _dMinValue = _dMaxValue = 0;
        }

        /// <summary>
        /// This constructor assigns the passed minValue & maxValue to the MinValue & MaxValue data members, respectively
        /// it then calls the base class constructor with the appropriate enumerated response type
        /// </summary>
        /// <param name="minValue">The minimum integer value the survey taker can enter</param>
        /// <param name="maxValue">The maximum integer value the survey taker can enter</param>
        public CBoundedNumResponse(int minValue, int maxValue)
            : base(EResponseType.BoundedNum)
        {
            _dMinValue = minValue;
            _dMaxValue = maxValue;
        }

        public CBoundedNumResponse(CBoundedNumResponse r)
            : base(EResponseType.BoundedNum, r)
        {
            _dMinValue = r._dMinValue;
            _dMaxValue = r._dMaxValue;
        }

        public override object Clone()
        {
            return new CBoundedNumResponse(this);
        }

        /// <summary>
        /// Tests the object's data to ensure it is valid
        /// </summary>
        /// <returns>"true" if both MinValue & MaxValue != InvalidValue and if MinValue is less than MaxValue
        /// returns "false" otherwise</returns>
        public override bool IsValid()
        {
            if ((MinValue != InvalidValue) && (MaxValue != InvalidValue) && (MinValue < MaxValue))
                return true;
            return false;
        }


        public override XElement AsXElement() => new XElement("Response", new XAttribute("MinValue", MinValue.ToString()), new XAttribute("MaxValue", MaxValue.ToString()), Format.AsXElement());

        public override void Load(XElement elem)
        {
            MinValue = Convert.ToInt32(elem.Attribute("MinValue").Value);
            MaxValue = Convert.ToInt32(elem.Attribute("MaxValue").Value);
            Format.Load(elem.Element("Format"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("BoundedNumber");
            writer.WriteElementString("MinValue", MinValue.ToString());
            writer.WriteElementString("MaxValue", MaxValue.ToString());
            Format.WriteXml(writer);
            writer.WriteEndElement();
        }

        public override string GetResponseDesc()
        {
            return String.Format("\tA number between {0} and {1}\r\n", MinValue, MaxValue);
        }

        public override Response GenerateSerializableResponse(SurveyItem ParentItem)
        {
            BoundedNumber r = new BoundedNumber(ParentItem);
            r.MinValue = MinValue;
            r.MaxValue = MaxValue;

            return r;
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            return new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.None);
        }
    }
}
