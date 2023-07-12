using System;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using System.Xml.Linq;

namespace IATClient.IATConfig
{
    public abstract class IATEvent {
        public enum EEventType { BeginIATBlock, EndIATBlock, IATItem, BeginInstructionBlock, TextInstructionScreen, MockItemInstructionScreen, KeyedInstructionScreen };

        public ConfigFile ConfigFile { get; set; }
        public Dictionary<Uri, Rectangle> DIRectangles { get; set; }
        public EEventType EventType { get; set; }

        public IATEvent(EEventType type)
        {
            EventType = type;
        }

        static private IATEvent CreateEventOfType(EEventType type)
        {
            IATEvent e = null;
            switch (type)
            {
                case EEventType.BeginIATBlock:
                    e = new BeginIATBlock();
                    break;

                case EEventType.BeginInstructionBlock:
                    e = new BeginInstructionBlock();
                    break;

                case EEventType.EndIATBlock:
                    e = new EndIATBlock();
                    break;

                case EEventType.IATItem:
                    e = new IATItem();
                    break;

                case EEventType.KeyedInstructionScreen:
                    e = new KeyedInstructionScreen();
                    break;

                case EEventType.MockItemInstructionScreen:
                    e = new MockItemInstructionScreen();
                    break;

                case EEventType.TextInstructionScreen:
                    e = new TextInstructionScreen();
                    break;
            }
            return e;
        }

        static public IATEvent CreateFromXElement(XElement elem)
        {
            if (Enum.TryParse<EEventType>(elem.Name.LocalName, true, out EEventType eType))
            {
                var evt = CreateEventOfType(eType);
                evt.Load(elem);
                return evt;
            }
            return null;
        }

        public abstract void WriteXml(XmlWriter writer);
        public abstract void Load(XElement elem);
        public String GetName()
        {
            return "IATEvent";
        }
    }
}
