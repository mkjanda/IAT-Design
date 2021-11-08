using System;
using System.Xml;

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

        public override void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            NumInstructionScreens = Convert.ToInt32(reader.ReadElementString());
            AlternatedWith = Convert.ToInt32(reader.ReadElementString());
            reader.ReadEndElement();
        }
    }
}
