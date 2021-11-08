using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace IATClient
{
    public abstract class CSyncEvent : INamedXmlSerializable
    {
        public enum ESyncEvents { SearchCriteria, Norms, Enumeration, SubjectID };


        public String GetName()
        {
            return "SyncEvent";
        }

        public abstract ESyncEvents GetEventType();
        public abstract void ReadXml(XmlReader reader);
        public abstract void WriteXml(XmlWriter writer);
    }
}
