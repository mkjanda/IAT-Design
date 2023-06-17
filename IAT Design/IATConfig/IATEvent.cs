using System;
using System.Xml;
using System.Collections.Generic;
using System.Drawing;
using System.IO;

namespace IATClient.IATConfig
{
    public abstract class IATEvent : INamedXmlSerializable
    {
        public enum EEventType { BeginIATBlock, EndIATBlock, IATItem, BeginInstructionBlock, TextInstructionScreen, MockItemInstructionScreen, KeyedInstructionScreen };

        public Dictionary<Uri, Rectangle> DIRectangles { get; set; }
        public EEventType EventType { get; set; }

        public IATEvent(EEventType type)
        {
            EventType = type;
        }

        static public IATEvent CreateFromXml(XmlReader reader)
        {
            IATEvent e = null;
            EEventType eType = (EEventType)Enum.Parse(typeof(EEventType), reader.Name);
            switch (eType)
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
            e.ReadXml(reader);
            return e;
        }

        public abstract void WriteXml(XmlWriter writer);
        public abstract void ReadXml(XmlReader reader);
        public String GetName()
        {
            return "IATEvent";
        }
    }
}
