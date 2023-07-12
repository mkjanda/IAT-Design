using IATClient.IATConfig;
using IATClient.ResultData;
using System;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CDateResponse provides a class that represents a definition for an date response
    /// with an optional specified minimum and maximum value
    /// </summary>
    class CDateResponse : CResponse
    {
        public bool HasStartDate { get; set; } = false;
        public bool HasEndDate { get; set; } = false;
        public DateTime StartDate { get; set; } = DateTime.Now;
        public DateTime EndDate { get; set; } = DateTime.Now;


        public CResponseObject.CResponseSpecifier GetDateBounds()
        {
            return new CResponseObject.CDateRange(StartDate, EndDate);
        }

        /// <summary>
        /// The default constructor.  Sets HasStartDate & HasEndDate to false, and sets StartDate & EndDate 
        /// to DateTime.MaxValue.Date
        /// </summary>
        public CDateResponse()
            : base(EResponseType.Date)
        {
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="r">The CDateResponse object to be copied</param>
        public CDateResponse(CDateResponse r)
            : base(EResponseType.Date, r)
        {
            StartDate = r.StartDate;
            EndDate = r.EndDate;
            HasStartDate = r.HasStartDate;
            HasEndDate = r.HasEndDate;
        }

        public override object Clone()
        {
            return new CDateResponse(this);
        }

        /// <summary>
        /// Validates the objects data
        /// </summary>
        /// <returns>"true" unless StartDate falls after EndDate, in which case "false" is returned</returns>
        public override bool IsValid()
        {
            if (DateTime.Compare(StartDate.Date, EndDate.Date) > 0)
                return false;
            return true;
        }

        public override XElement AsXElement() => new XElement("Response", new XElement("StartDate", new XAttribute("HasValue", HasStartDate.ToString()),
            new XElement("Year", StartDate.Year.ToString()), new XElement("Month", StartDate.Month.ToString()), new XElement("Day", StartDate.Day.ToString())),
            new XElement("EndDate", new XAttribute("HasValue", HasEndDate.ToString()), new XElement("Year", EndDate.Year.ToString()), new XElement("Month", EndDate.Month.ToString()),
                new XElement("Day", EndDate.Day.ToString())), Format.AsXElement());

        public override void Load(XElement elem)
        {
            int year, day, month;
            if (Convert.ToBoolean(elem.Element("StartDate").Attribute("HasValue").Value))
            {
                HasStartDate = true;
                year = Convert.ToInt32(elem.Element("StartDate").Element("Year").Value);
                month = Convert.ToInt32(elem.Element("StartDate").Element("Month").Value);
                day = Convert.ToInt32(elem.Element("StartDate").Element("Day").Value);
                StartDate = new DateTime(year, month, day);
            }
            else
                StartDate = DateTime.MinValue;
            if (Convert.ToBoolean(elem.Element("EndDate").Attribute("HasValue").Value))
            {
                HasEndDate = true;
                year = Convert.ToInt32(elem.Element("EndDate").Element("Year").Value);
                month = Convert.ToInt32(elem.Element("EndDate").Element("Month").Value);
                day = Convert.ToInt32(elem.Element("EndDate").Element("Day").Value);
                EndDate = new DateTime(year, month, day);
            }
            else
                EndDate = DateTime.MaxValue;
            Format.Load(elem.Element("Format"));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Date");
            writer.WriteStartElement("StartDate");
            writer.WriteAttributeString("HasValue", HasStartDate.ToString());
            if (HasStartDate)
            {
                writer.WriteElementString("Year", StartDate.Year.ToString());
                writer.WriteElementString("Month", StartDate.Month.ToString());
                writer.WriteElementString("Day", StartDate.Day.ToString());
            }
            else
            {
                writer.WriteElementString("Year", DateTime.MinValue.Year.ToString());
                writer.WriteElementString("Month", DateTime.MinValue.Month.ToString());
                writer.WriteElementString("Day", DateTime.MinValue.Day.ToString());
            }
            writer.WriteEndElement();

            writer.WriteStartElement("EndDate");
            writer.WriteAttributeString("HasValue", HasEndDate.ToString());
            if (HasEndDate)
            {
                writer.WriteElementString("Year", EndDate.Year.ToString());
                writer.WriteElementString("Month", EndDate.Month.ToString());
                writer.WriteElementString("Day", EndDate.Day.ToString());
            }
            else
            {
                writer.WriteElementString("Year", DateTime.MaxValue.Year.ToString());
                writer.WriteElementString("Month", DateTime.MaxValue.Month.ToString());
                writer.WriteElementString("Day", DateTime.MaxValue.Day.ToString());
            }
            writer.WriteEndElement();

            // write the close of the "Response" element
            Format.WriteXml(writer);
            writer.WriteEndElement();
        }

        public override string GetResponseDesc()
        {
            if (HasEndDate && HasStartDate)
                return String.Format("\tA date that falls between {0:d} and {1:d}, inclusively\r\n", StartDate, EndDate);
            else if (HasEndDate)
                return String.Format("\tA date that falls on or before {0:d}\r\n", EndDate);
            else if (HasStartDate)
                return String.Format("\tA date that falls on or after {0:d}\r\n", StartDate);
            else
                return "\tA date\r\n";
        }

        public override Response GenerateSerializableResponse(SurveyItem parentItem)
        {
            Date r = new Date(parentItem);
            r.HasEndDate = HasEndDate;
            r.HasStartDate = HasStartDate;
            r.StartDate = new DateEntry(StartDate);
            r.EndDate = new DateEntry(EndDate);

            return r;
        }

        public override CSpecifierControlDefinition GetSpecifierControlDefinition()
        {
            return new CSpecifierControlDefinition(DynamicSpecifier.ESpecifierType.None);
        }


    }
}