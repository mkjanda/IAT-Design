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
        public int ResourceId { get; set; }
        public List<int> ReferenceIds { get; set; } = new List<int>();

        public void ReadXml(XmlReader xReader)
        {
            xReader.ReadStartElement(GetName());
            ResourceId = Convert.ToInt32(xReader.ReadElementString("ResourceId"));
            while (xReader.Name == "Reference")
                ReferenceIds.Add(Convert.ToInt32(xReader.ReadElementString()));
            xReader.ReadEndElement();
        }

        public void WriteXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement(GetName());
            xWriter.WriteElementString("ResourceId", ResourceId.ToString());
            foreach (var ndx in ReferenceIds) 
                xWriter.WriteElementString("Reference", ndx.ToString());
            xWriter.WriteEndElement();
        }

        public String GetName()
        {
            return "ResourceReference";
        }
    }
}
