using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using System.Xml;

namespace IATClient.IATConfig
{
    public class IATEventList : IEnumerable<IATEvent>
    {

        private List<IATEvent> IATEvents { get; set; } = new List<IATEvent>();

        public IEnumerator<IATEvent> GetEnumerator()
        {
            return ((IEnumerable<IATEvent>)IATEvents).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(Object val)
        {
            return IATEvents.IndexOf((IATEvent)val);
        }


        public void Add(IATEvent e)
        {
            IATEvents.Add(e);
        }

        public int Count
        {
            get
            {
                return IATEvents.Count;
            }
        }

        public IATEvent this[int ndx]
        {
            get
            {
                return IATEvents[ndx];
            }
        }

        public IATEventList()
        {
            IATEvents = new List<IATEvent>();
        }

        public IATEventList(List<IATEvent> list)
        {
            IATEvents = new List<IATEvent>();
            IATEvents.AddRange(list.ToArray());
        }


        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("EventList");
            for (int ctr = 0; ctr < IATEvents.Count; ctr++)
                IATEvents[ctr].WriteXml(writer);
            writer.WriteEndElement();
        }


        public void Load(XElement elem)
        {
            foreach(var evt in elem.Elements())
            {
                IATEvents.Add(IATEvent.CreateFromXElement(evt));
            }
        }

        public String GetName()
        {
            return "IATEventList";
        }
    }
}
