using System;
using System.Xml;
using System.Xml.Linq;

namespace IATClient.IATConfig
{
    class BeginInstructionBlock : IATEvent
    {
        private int _NumInstructionScreens, _AlternatedWith;

        public int NumInstructionScreens
        {
            get
            {
                return _NumInstructionScreens;
            }
            set
            {
                _NumInstructionScreens = value;
            }
        }

        public int AlternatedWith
        {
            get
            {
                return _AlternatedWith;
            }
            set
            {
                _AlternatedWith = value;
            }
        }

        public BeginInstructionBlock()
            : base(EEventType.BeginInstructionBlock)
        {
            NumInstructionScreens = -1;
            AlternatedWith = -1;
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("BeginInstructionBlock");
            writer.WriteElementString("NumInstructionScreens", NumInstructionScreens.ToString());
            writer.WriteElementString("AlternatedWith", AlternatedWith.ToString());
            writer.WriteEndElement();
        }

        public override void Load(XElement elem)
        {
            NumInstructionScreens = Convert.ToInt32(elem.Element("NumInstructionScreens").Value);
            AlternatedWith = Convert.ToInt32(elem.Element("AlternatedWith").Value);
        }
    }
}
