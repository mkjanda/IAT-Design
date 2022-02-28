using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Launcher
{
    [Serializable]
    public class UpdateNotification
    {

        public class TNotification
        {
            [XmlAttribute(AttributeName = "Version", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
            public String Version { get; set; }
            [XmlAttribute(AttributeName = "Flags", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
            public UInt64 Flags { get; set; }
            [XmlText(Type=typeof(String))]
            public String Content { get; set; }
        }
        [XmlElement]
        public List<TNotification> Notification { get; set; } = new List<TNotification>();

        [XmlIgnore]
        public String UpdateNotificationHTML
        {
            get
            {
                String header = "<!doctype html><html><head><title>Update Notification</title>" +
                    "<link rel=\"stylesheet\" href=\"https://www.iatsoftware.net/IAT/css/update-notification.css\" ></head>" +
                    "<body><div id=\"header\" ><div class=\"text\"><img src=\"https://www.iatsoftware.net/IAT/images/email-header.png\" />" +
                    "</div><div class=\"logo\"><img src=\"https://www.iatsoftware.net/IAT/images/logo.png\" /></div></div><div style=\"text-align: left;\">";

                Regex exp = new Regex(@"^.+?<body.*?>(.+)</body>", RegexOptions.Singleline);
                String body = "<body>" + Notification.Select(n => n).Select(n =>
                {
                    return "<div class=\"notification\"><h1>Update " + n.Version + "</h1>" + exp.Match(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(n.Content))).Groups[1].Value + "</div>";
                }).Aggregate((s1, s2) => s1 + s2);
                return header + body + "</div></html>";
            }
        }
    }
}
