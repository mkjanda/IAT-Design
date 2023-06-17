using IATClient.IATConfig;
using IATClient.ResultData;
using System;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CFixedDigResponse provides a class that represents a definition for a numerical response
    /// that consists of a set number of digits
    /// </summary>
    class CFixedDigResponse : CTextResponse
    {
        // the number of digits the survey taker is to enter
        private int _nDigs;

        /// <summary>
        /// gets or sets the number of digits the survey taker is to enter
        /// </summary>
        public int NumDigs
        {
            get
            {
                return _nDigs;
            }
            set
            {
                _nDigs = value;
            }
        }

        /// <summary>
        /// A constant "invalid value" for that indicates NumDigs is unset
        /// </summary>
        public const int InvalidValue = int.MinValue;

        public int GetNumDigs()
        {
            return NumDigs;
        }

        /// <summary>
        /// The default constructor.
        /// </summary>
        public CFixedDigResponse()
            : base(EResponseType.FixedDig)
        {
            _nDigs = 0;
        }

        /// <summary>
        /// This constructor sets the expected number of digits to the passed parameter
        /// </summary>
        /// <param name="nDigs">The number of digits the survey taker is to enter</param>
        public CFixedDigResponse(int nDigs)
            : base(EResponseType.FixedDig)
        {
            _nDigs = nDigs;
        }

        public CFixedDigResponse(CFixedDigResponse r)
            : base(EResponseType.FixedDig, r)
        {
            _nDigs = r._nDigs;
        }

        public override object Clone()
        {
            return new CFixedDigResponse(this);
        }

        public override bool IsValid()
        {
            if ((NumDigs > 0) && (NumDigs != InvalidValue))
                return true;
            return false;
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("FixedDigit");
            Format.WriteXml(writer);
            writer.WriteElementString("NumDigs", NumDigs.ToString());
            writer.WriteEndElement();
        }

        public override XElement AsXElement() => new XElement("Response", new XElement("NumDigits", NumDigs.ToString()), Format.AsXElement());

        public override void Load(XElement elem)
        {
            _nDigs = Convert.ToInt32(elem.Element("NumDigits").Value);
            Format.Load(elem.Element("Format"));
        }

        public override string GetResponseDesc()
        {
            return String.Format("\t{0} digits\r\n", NumDigs);
        }

        public override Response GenerateSerializableResponse(SurveyItem parentItem)
        {
            FixedDigit r = new FixedDigit(parentItem);
            r.NumDigs = NumDigs;

            return r;
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            return new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.None);
        }
    }
}
