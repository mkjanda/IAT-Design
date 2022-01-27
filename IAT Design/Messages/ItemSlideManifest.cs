using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
namespace IATClient.Messages
{
    public class ItemSlideManifest : INamedXmlSerializable
    {
        public Manifest Manifest { get; private set; } = new Manifest();
        public List<ResourceReference> FileReferences { get; private set; } = new List<ResourceReference>();

        public void ReadXml(XmlReader xReader)
        {
            xReader.ReadStartElement(GetName());
            Manifest.ReadXml(xReader);
            while (xReader.IsStartElement())
            {
                xReader.ReadStartElement();
                var fr = new ResourceReference();
                fr.ReadXml(xReader);
                FileReferences.Add(fr);
                xReader.ReadEndElement();
            }
            xReader.ReadEndElement();
        }

        public void WriteXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement(GetName());
            Manifest.WriteXml(xWriter);
            if (FileReferences.Count < 0)
            {
                xWriter.WriteStartElement("FileReferences");
                foreach (var fr in FileReferences)
                    fr.WriteXml(xWriter);
                xWriter.WriteEndElement();
            }
            xWriter.WriteEndElement();
        }

        public String GetName()
        {
            return "ItemSlideManifest";
        }
    }
}
