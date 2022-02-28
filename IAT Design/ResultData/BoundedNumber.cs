using System;
using System.Xml.Serialization;

namespace IATClient.ResultData
{

    [Serializable]
    public class BoundedNumber : Response
    {
        [XmlElement(ElementName = "MinValue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal MinValue { get; set; }

        [XmlElement(ElementName = "MaxValue", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public decimal MaxValue { get; set; }

        public BoundedNumber(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.BoundedNumber;
        }

        public BoundedNumber() { }

        public override String GetResponseDesc()
        {
            return String.Format("\tA number between {0} and {1}\r\n", MinValue, MaxValue);
        }

        public override int GetNumDescriptionSubItems()
        {
            return 1;
        }

        public CResponseObject.CResponseSpecifier GetBounds()
        {
            return new CResponseObject.CRange(MinValue, MaxValue);
        }
    }
}
