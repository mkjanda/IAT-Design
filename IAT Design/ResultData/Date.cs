using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IATClient.ResultData
{
    [Serializable]
    public class Date : Response
    {
        [XmlElement(ElementName = "StartDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Type = typeof(DateEntry))]
        public DateEntry StartDate { get; set; }

        [XmlElement(ElementName = "EndDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Type = typeof(DateEntry))]
        public DateEntry EndDate { get; set; }

        [XmlAttribute(AttributeName = "HasStartDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool HasStartDate { get; set; }

        [XmlAttribute(AttributeName = "HasEndDate", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool HasEndDate { get; set; }

        public Date(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.Date;
        }

        public Date() { }
        private DateTime StartDateTime = DateTime.MinValue, EndDateTime = DateTime.MaxValue;

        public override void PostSerialize(SurveyItem Parent)
        {
            base.PostSerialize(Parent);
            if (HasStartDate)
                StartDateTime = new DateTime(Convert.ToInt32(StartDate.Year), Convert.ToInt32(StartDate.Month), Convert.ToInt32(StartDate.Day));
            else
                StartDateTime = DateTime.MinValue;
            if (HasEndDate)
                EndDateTime = new DateTime(Convert.ToInt32(EndDate.Year), Convert.ToInt32(EndDate.Month), Convert.ToInt32(EndDate.Day));
            else
                EndDateTime = DateTime.MaxValue;
        }

        public override String GetResponseDesc()
        {
            if (HasEndDate && HasStartDate)
                return String.Format("\tA date that falls between {0:d} and {1:d}, inclusively\r\n", StartDateTime, EndDateTime);
            else if (HasEndDate)
                return String.Format("\tA date that falls on or before {0:d}\r\n", EndDateTime);
            else if (HasStartDate)
                return String.Format("\tA date that falls on or after {0:d}\r\n", StartDateTime);
            else
                return "A date";
        }

        public CResponseObject.CResponseSpecifier GetDateBounds()
        {
            return new CResponseObject.CDateRange(StartDateTime, EndDateTime);
        }

        public override int GetNumDescriptionSubItems()
        {
            return 1;
        }
    }
}
