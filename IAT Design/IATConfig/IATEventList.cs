using System;
using System.Collections.Generic;
using System.Collections;
using System.Xml;

namespace IATClient.IATConfig
{
    public class IATEventList : INamedXmlSerializable, IEnumerable<IATEvent>
    {
        private List<IATEvent> _IATEvents;

        public IEnumerator<IATEvent> GetEnumerator()
        {
            return ((IEnumerable<IATEvent>)_IATEvents).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(Object val)
        {
            return _IATEvents.IndexOf((IATEvent)val);
        }


        public void Add(IATEvent e)
        {
            _IATEvents.Add(e);
        }

        public int Count
        {
            get
            {
                return _IATEvents.Count;
            }
        }

        public IATEvent this[int ndx]
        {
            get
            {
                return _IATEvents[ndx];
            }
        }

        public IATEventList()
        {
            _IATEvents = new List<IATEvent>();
        }

        public IATEventList(List<IATEvent> list)
        {
            _IATEvents = new List<IATEvent>();
            _IATEvents.AddRange(list.ToArray());
        }


        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("IATEventList");
            writer.WriteAttributeString("NumEvents", _IATEvents.Count.ToString());
            for (int ctr = 0; ctr < _IATEvents.Count; ctr++)
                _IATEvents[ctr].WriteXml(writer);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            int nEvents = Convert.ToInt32(reader["NumEvents"]);
            reader.ReadStartElement();
            _IATEvents.Clear();
            for (int ctr = 0; ctr < nEvents; ctr++)
                _IATEvents.Add(IATEvent.CreateFromXml(reader));
            reader.ReadEndElement();
        }

        public String GetName()
        {
            return "IATEventList";
        }
    }
}
