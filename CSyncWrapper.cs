using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security.Cryptography;

namespace IATClient
{
    public class CSyncWrapper : INamedXmlSerializable
    {
        private CSyncEvent _Event = null;
        private RSACryptoServiceProvider Crypt;
        public CSyncEvent Event
        {
            get
            {
                return _Event;
            }
        }

        public CSyncWrapper(RSACryptoServiceProvider crypt) 
        {
            Crypt = crypt;
        }

        public CSyncWrapper(CSyncEvent theEvent)
        {
            _Event = theEvent;
        }

        public String GetName()
        {
            return "SyncWrapper";
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            CSyncEvent.ESyncEvents eventType = (CSyncEvent.ESyncEvents)Enum.Parse(typeof(CSyncEvent.ESyncEvents), reader["EventType"]);
            reader.ReadStartElement();
            switch (eventType)
            {
                case CSyncEvent.ESyncEvents.Norms:
                    _Event = new CNorms(Crypt);
                    break;

                case CSyncEvent.ESyncEvents.SearchCriteria:
                    _Event = new CSearchCriteria();
                    break;

                case CSyncEvent.ESyncEvents.Enumeration:
                    _Event = new CEnumeration(Crypt);
                    break;
            }
            Event.ReadXml(reader);
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetName());
            writer.WriteAttributeString("EventType", Event.GetEventType().ToString());
            Event.WriteXml(writer);
            writer.WriteEndElement();
        }
    }
}
