using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO.Packaging;

namespace IATClient.Messages
{
    public class ResourceReference : INamedXmlSerializable
    {
        public Uri ImageUri { get; set; }
        public List<int> ReferenceIndex { get; set; } = new List<int>();

        public void ReadXml(XmlReader xReader)
        {
            xReader.ReadStartElement(GetName());
            ImageUri = new Uri(xReader.ReadElementString("ImageUriOriginalString"), UriKind.Relative);
            while (xReader.Name == "Reference")
                ReferenceIndex.Add(Convert.ToInt32(xReader.ReadElementString()));
        }

        public void WriteXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement(GetName());
            xWriter.WriteElementString("ImageUriOriginalString", ImageUri.OriginalString);
            foreach (var ndx in ReferenceIndex) 
                xWriter.WriteElementString("Reference", ndx.ToString());
            xWriter.WriteEndElement();
        }

        public String GetName()
        {
            return "FileReference";
        }
    }
}
