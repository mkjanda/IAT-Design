using System;
using System.Xml.Serialization;

namespace IATClient.ResultData
{

    [Serializable]
    public class WeightedMultiple : Response
    {
        [XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        [XmlArrayItem(ElementName = "Choice", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false, Type = typeof(WeightedChoice))]
        public WeightedChoice[] Choices { get; set; }

        public WeightedMultiple(SurveyItem si)
            : base(si)
        {
            ResponseType = ResponseType.WeightedMultiple;
        }

        public WeightedMultiple() { }

        public int GetNumStatements()
        {
            return Choices.Length;
        }

        public String GetChoice(int choiceNdx)
        {
            return Choices[choiceNdx].Text;
        }

        public int GetChoiceWeight(int choiceNdx)
        {
            return Choices[choiceNdx].Weight;
        }

        public override String GetResponseDesc()
        {
            String desc = String.Empty;
            for (int ctr = 0; ctr < Choices.Length; ctr++)
                desc += String.Format("\t{0}: {1}\r\n", Choices[ctr].Weight, Choices[ctr].Text);
            return desc;
        }

        public override int GetNumDescriptionSubItems()
        {
            return Choices.Length;
        }

        public override String GetDescriptionSubItem(int ndx)
        {
            return Choices[ndx].Text;
        }
    }
}
