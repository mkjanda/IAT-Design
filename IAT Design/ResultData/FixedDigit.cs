using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IATClient.ResultData
{

    [Serializable]
    public class FixedDigit : Response
    {
        [XmlElement(ElementName = "NumDigs", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int NumDigs { get; set; }

        public FixedDigit(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.FixedDigit;
        }

        public FixedDigit() { }

        public int GetNumDigits()
        {
            return NumDigs;
        }

        public override String GetResponseDesc()
        {
            return String.Format("\tA response of {0} digits.\r\n", NumDigs);
        }

        public override int GetNumDescriptionSubItems()
        {
            return 1;
        }
    }
}
