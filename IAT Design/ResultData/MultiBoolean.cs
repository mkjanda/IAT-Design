using System;
using System.Xml.Serialization;

namespace IATClient.ResultData
{
    [Serializable]
    public class MultiBoolean : Response
    {
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem(ElementName = "Text", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(String))]
        public String[] Choices { get; set; }

        [XmlAttribute(AttributeName = "MinSelections", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int MinSelections { get; set; }

        [XmlAttribute(AttributeName = "MaxSelections", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public int MaxSelections { get; set; }

        public MultiBoolean(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.MultiBoolean;
        }

        public MultiBoolean() { }

        public int GetNumStatements()
        {
            return Choices.Length;
        }

        public String GetStatement(int ndx)
        {
            return Choices[ndx];
        }

        public override String GetResponseDesc()
        {
            String desc = "Each zero or one in the response is a bit that specifies if a certain selection was made.\r\n ";
            desc += "Below is a synopsis of the bits included in the response in left-to-right order.\r\n";
            for (int ctr = 0; ctr < Choices.Length; ctr++)
                desc += String.Format("\tBit #{0}: {1}\r\n", ctr + 1, Choices[ctr]);
            return desc;
        }

        public override int GetNumDescriptionSubItems()
        {
            return Choices.Length;
        }
    }
}
