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
        public long ResourceId { get; set; }
        public List<long> ReferenceIds { get; set; } = new List<long>();


        public void ReadXml(XmlReader xReader)
        {
            xReader.ReadStartElement(GetName());
            ResourceId = Convert.ToInt64(xReader.ReadElementString("ResourceId"));
            while (xReader.Name == "ReferenceId")
                ReferenceIds.Add(Convert.ToInt64(xReader.ReadElementString()));
            xReader.ReadEndElement();
        }

        public void WriteXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement(GetName());
            xWriter.WriteElementString("ResourceId", ResourceId.ToString());
            foreach (var ndx in ReferenceIds) 
                xWriter.WriteElementString("ReferenceId", ndx.ToString());
            xWriter.WriteEndElement();
        }

        public String GetName()
        {
            return "ResourceReference";
        }
    }
}
