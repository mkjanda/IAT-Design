using System;
using System.Xml.Serialization;

namespace IATClient.ResultData
{

    [Serializable]
    public class RegEx : Response
    {
        [XmlElement(ElementName = "RegEx", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String RegularExpression { get; set; }

        public RegEx(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.RegEx;
        }

        public RegEx() { }

        public String GetRegEx()
        {
            return RegularExpression;
        }

        public override String GetResponseDesc()
        {
            return String.Format("\tText that matches the regular expression \"{0}\"\r\n", RegularExpression);
        }
    }
}
