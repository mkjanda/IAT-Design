using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace IATClient
{
    public interface INamedXmlSerializable
    {
        void ReadXml(XmlReader reader);
        void WriteXml(XmlWriter writer);
        String GetName();
    }
}
