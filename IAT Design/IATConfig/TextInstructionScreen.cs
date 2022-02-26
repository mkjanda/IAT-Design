using System;
using System.Xml;

namespace IATClient.IATConfig
{
    class TextInstructionScreen : IATInstructionScreen
    {
        private int _InstructionsDisplayID = -1;

        public int InstructionsDisplayID
        {
            get
            {
                if (_InstructionsDisplayID != -1)
                    return _InstructionsDisplayID;
                _InstructionsDisplayID = ConfigFile.GetIATImage(InstructionScreen.InstructionsUri).Id;
                return _InstructionsDisplayID;
            }
            protected set
            {
                _InstructionsDisplayID = value;
            }
        }

        public TextInstructionScreen()
            : base(EEventType.TextInstructionScreen)
        {
        }

        protected TextInstructionScreen(EEventType evtType)
            : base(evtType) { }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("TextInstructionScreen");
            writer.WriteElementString("ContinueASCIIKeyCode", ContinueASCIIKeyCode.ToString());
            writer.WriteElementString("ContinueInstructionsID", ContinueInstructionsDisplayID.ToString());
            writer.WriteElementString("InstructionsDisplayID", InstructionsDisplayID.ToString());
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            ContinueASCIIKeyCode = Convert.ToInt32(reader.ReadElementString("ContinueASCIIKeyCode"));
            ContinueInstructionsDisplayID = Convert.ToInt32(reader.ReadElementString("ContinueInstructionsDisplayID"));
            InstructionsDisplayID = Convert.ToInt32(reader.ReadElementString("InstructionsDisplayID"));
            reader.ReadEndElement();
        }
    }
}
