using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization; 
namespace IATClient.ResultData
{

    [Serializable]
    public class Boolean : Response
    {
        [XmlElement(ElementName = "TrueStatement", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String TrueStatement { get; set; }

        [XmlElement(ElementName = "FalseStatement", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String FalseStatement { get; set; }

        public Boolean(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.Boolean;
        }

        public Boolean() { }

        public override String GetResponseDesc()
        {
            return "\tTrue/False question\r\n";
        }

        public override int GetNumDescriptionSubItems()
        {
            return 2;
        }
        public String GetTrueStatement()
        {
            return TrueStatement;
        }

        public String GetFalseStatement()
        {
            return FalseStatement;
        }
    }

}
