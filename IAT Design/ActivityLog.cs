using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    [Serializable]
    public class ActivityLog 
    {
        public class EventType : Enumeration
        {
            public static EventType Create = new EventType(1, "Create");
            public static EventType Delete = new EventType(1, "Delete");
            public static EventType Open = new EventType(3, "Open");
            public static EventType Close = new EventType(4, "Close");
            public static EventType Display = new EventType(5, "Display");
            public static EventType ImageLoad = new EventType(6, "ImageLoad");
            public static EventType Dispose = new EventType(7, "Display");
            public static EventType Attached = new EventType(8, "Attached")
            {
                ParameterName = "attached to"
            };
            public static EventType Detached = new EventType(9, "Detached")
            {
                ParameterName = "detached from"
            };

            public String ParameterName { get; private set; } = null;
            private EventType(int id, String name) : base(id, name) { }
        }
        public class Event
        {
            [XmlAttribute(AttributeName = "Id", Form = XmlSchemaForm.Unqualified)]
            public int ID { get; set; }
            [XmlAttribute(AttributeName = "EventType", Form = XmlSchemaForm.Unqualified)]
            public String EventType { get; set; }
            [XmlAttribute(AttributeName = "TargetType", Form = XmlSchemaForm.Unqualified)]
            public String TargetType { get; set; }
            [XmlAttribute(AttributeName = "Time", Form = XmlSchemaForm.Unqualified)]
            public String Time { get { return new DateTime(Ticks).ToString("MM/dd/yyyy hh:mm:ss.fff"); } set { } }
            [XmlIgnore]
            public long Ticks { get; set; } = DateTime.Now.Ticks;

            [Serializable]
            public class Parameter
            {
                [XmlElement(ElementName = "Name", Form = XmlSchemaForm.Unqualified)]
                public String Name { get; set; } = null;
                [XmlElement(ElementName = "Value", Form = XmlSchemaForm.Unqualified)]
                public String Value { get; set; } = null;

                public Parameter() { }

                public Parameter(String name, String value)
                {
                    Name = name;
                    Value = value;
                }
            }
            [XmlElement(ElementName = "Parameter", Form = XmlSchemaForm.Unqualified, IsNullable = true, Type = typeof(Parameter))]
            public Parameter[] Parameters { get; set; } = null;

            public Event() { }
            public Event(EventType eType, Uri target)
            {
                EventType = eType.Name;
                TargetType = new Regex(@"/.+\.([^\.]+\.[^.]+)$").Match(target.ToString()).Groups[1].Value;
            }

            public Event(EventType eType, Uri target, String[] paramNames, String[] paramValues)
            {
                EventType = eType.Name;
                TargetType = new Regex(@"/.+\.([^\.]+\.[^.]+)$").Match(target.ToString()).Groups[1].Value;
                List<Parameter> parameters = new List<Parameter>();
                for (int ctr = 0; ctr < paramNames.Length; ctr++)
                    parameters.Add(new Parameter(paramNames[ctr], paramValues[ctr]));
                Parameters = parameters.ToArray();
            }
        }

        public static String UriToTypeName(Uri u)
        {
            return new Regex(@"/.+\.([^\.]+\.[^.]+)$").Match(u.ToString()).Groups[1].Value;
        }

        [XmlIgnore]
        private Dictionary<Uri, List<Event>> EventMap = new Dictionary<Uri, List<Event>>();

        public void Clear()
        {
            EventMap.Clear();
        }

        public void LogEvent(EventType type, Uri target)
        {
            if (!EventMap.ContainsKey(target))
                EventMap[target] = new List<Event>();
            EventMap[target].Add(new Event(type, target));
        }

        public void LogEvent(EventType type, Uri target, Uri affected)
        {
            if (!EventMap.ContainsKey(target))
                EventMap[target] = new List<Event>();
            EventMap[target].Add(new Event(type, target, new String[] { type.ParameterName }, new string[] { UriToTypeName(affected) }));
        }


        public void LogEvent(EventType type, Uri target, String[] paramNames, String[] paramValues)
        {
            if (!EventMap.ContainsKey(target))
                EventMap[target] = new List<Event>();
            EventMap[target].Add(new Event(type, target, paramNames, paramValues));
        }

        [XmlElement(ElementName = "Event", Form = XmlSchemaForm.Unqualified, Type = typeof(Event))]
        public Event[] EventList
        {
            get
            {
                return EventMap.Values.SelectMany<List<Event>, Event>(evtList => evtList).OrderBy(evt => evt.Time).ToArray<Event>();
            }
            set { }
        }
    }
}
