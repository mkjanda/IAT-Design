using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IATClient.ResultData
{

    [Serializable]
    public class Likert : Response
    {
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem(ElementName = "Text", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public String[] Choices { get; set; }

        [XmlAttribute(AttributeName = "ReverseScored", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public bool ReverseScored { get; set; }

        public Likert(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.Likert;
        }

        public Likert() { }

        public String GetStatement(int ndx)
        {
            return Choices[ndx];
        }

        public bool IsReverseScored()
        {
            return ReverseScored;
        }

        public void SetStatements(List<String> statements)
        {
            Choices = new String[statements.Count];
            for (int ctr = 0; ctr < statements.Count; ctr++)
                Choices[ctr] = statements[ctr];
        }

        public int GetNumStatements()
        {
            return Choices.Length;
        }

        public override String GetResponseDesc()
        {
            if (ReverseScored)
                return String.Format("\tReverse scored Likert item with response range 1 to {0}.  (Answers have already been reversed.)\r\n", Choices.Length);
            else
                return String.Format("\tLikert item with response range 1 to {0}\r\n", Choices.Length);
        }

        public override int GetNumDescriptionSubItems()
        {
            return Choices.Length;
        }
    }
}
