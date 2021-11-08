using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace IATClient
{
    [Serializable]
    public class ErrorReportResponse
    {
        public enum EResponseCode { success, invalidHandshake, killFiled, serverError };

        [XmlAttribute(AttributeName = "Response", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public EResponseCode Response { get; set; }
        [XmlElement(ElementName = "Caption", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Type = typeof(String))]
        public String Caption { get; set; }
        [XmlElement(ElementName = "Message", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Type = typeof(String))]
        public String Message { get; set; }
        [XmlElement(ElementName = "Redirect", Form = System.Xml.Schema.XmlSchemaForm.Unqualified, Type = typeof(String), IsNullable = true)]
        public String Redirect { get; set; }
    }
}
