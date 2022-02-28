using System;
using System.Xml.Serialization;

namespace IATClient.ResultData
{

    [Serializable]
    public class BoundedLength : Response
    {
        [XmlElement(ElementName = "MinLength", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int MinLength { get; set; }

        [XmlElement(ElementName = "MaxLength", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int MaxLength { get; set; }

        public BoundedLength(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.BoundedLength;
        }

        public BoundedLength() { }

        public override String GetResponseDesc()
        {
            return String.Format("\tA string of text between {0} and {1} characters in length\r\n", MinLength, MaxLength);
        }

        public override int GetNumDescriptionSubItems()
        {
            return 1;
        }

        public CResponseObject.CResponseSpecifier GetBounds()
        {
            return new CResponseObject.CRange(MinLength, MaxLength);
        }

    }
}
