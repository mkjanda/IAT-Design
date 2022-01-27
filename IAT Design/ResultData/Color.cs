using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
namespace IATClient.ResultData
{
    [Serializable]
    public class Color
    {
        [XmlElement(ElementName = "Red", Type = typeof(int), Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Red { get; set; }
        [XmlElement(ElementName = "Blue", Type=typeof(int), Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Blue { get; set; }
        [XmlElement(ElementName = "Green", Type=typeof(int), Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
        public int Green { get; set; }

        public Color() { }

        public System.Drawing.Color ToSystemColor()
        {
            return System.Drawing.Color.FromArgb(Red, Green, Blue);
        }
    }
}
