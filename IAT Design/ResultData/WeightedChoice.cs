using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IATClient.ResultData
{

    [Serializable]
    public class WeightedChoice
    {
        [XmlElement(ElementName = "Text", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public String Text { get; set; }
        [XmlElement(ElementName = "Weight", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Weight { get; set; }

        public WeightedChoice()
        {
            Weight = 0;
            Text = String.Empty;
        }

        public WeightedChoice(int weight, String text)
        {
            Weight = weight;
            Text = text;
        }
    }
}
