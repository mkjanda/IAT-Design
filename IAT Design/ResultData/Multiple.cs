using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace IATClient.ResultData
{
    [Serializable]
    public class Multiple : Response
    {
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem(ElementName = "Text", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String[] Choices { get; set; }

        public Multiple() { }
        public Multiple(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.Multiple;
        }

        public int GetNumStatements()
        {
            return Choices.Length;
        }

        public string GetStatement(int ndx)
        {
            return Choices[ndx];
        }

        public override String GetResponseDesc()
        {
            String desc = String.Empty;
            for (int ctr = 0; ctr < Choices.Length; ctr++)
                desc += String.Format("\t{0}: {1}\r\n", ctr + 1, Choices[ctr]);
            return desc;
        }

        public override int GetNumDescriptionSubItems()
        {
            return Choices.Length;
        }

        public override String GetDescriptionSubItem(int ndx)
        {
            return Choices[ndx];
        }
    }
}
