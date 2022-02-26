using System;
using System.Xml;

namespace IATClient.IATConfig
{
    class KeyedInstructionScreen : TextInstructionScreen
    {
        private int _LeftResponseDisplayID = -1, _RightResponseDisplayID = -1;

        private CKeyInstructionScreen KeyedScreen
        {
            get
            {
                return InstructionScreen as CKeyInstructionScreen;
            }
        }
        public int LeftResponseDisplayID
        {
            get
            {
                if (_LeftResponseDisplayID != -1)
                    return _LeftResponseDisplayID;
                _LeftResponseDisplayID = ConfigFile.GetIATImage(CIAT.SaveFile.GetIATKey(InstructionScreen.ResponseKeyUri).LeftValueUri).Id;
                return _LeftResponseDisplayID;
            }
            set
            {
                _LeftResponseDisplayID = value;
            }
        }

        public int RightResponseDisplayID
        {
            get
            {
                if (_RightResponseDisplayID != -1)
                    return _RightResponseDisplayID;
                _RightResponseDisplayID = ConfigFile.GetIATImage(CIAT.SaveFile.GetIATKey(InstructionScreen.ResponseKeyUri).RightValueUri).Id;
                return _RightResponseDisplayID;
            }
            set
            {
                _RightResponseDisplayID = value;
            }
        }
        public KeyedInstructionScreen()
            : base(EEventType.KeyedInstructionScreen)
        {
        }

        protected KeyedInstructionScreen(EEventType evtType) : base(evtType) { }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("KeyedInstructionScreen");
            writer.WriteElementString("ContinueASCIIKeyCode", ContinueASCIIKeyCode.ToString());
            writer.WriteElementString("ContinueInstructionsID", ContinueInstructionsDisplayID.ToString());
            writer.WriteElementString("LeftResponseDisplayID", LeftResponseDisplayID.ToString());
            writer.WriteElementString("RightResponseDisplayID", RightResponseDisplayID.ToString());
            writer.WriteElementString("InstructionsDisplayID", InstructionsDisplayID.ToString());
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            ContinueASCIIKeyCode = Convert.ToInt32(reader.ReadElementString());
            ContinueInstructionsDisplayID = Convert.ToInt32(reader.ReadElementString());
            LeftResponseDisplayID = Convert.ToInt32(reader.ReadElementString());
            RightResponseDisplayID = Convert.ToInt32(reader.ReadElementString());
            InstructionsDisplayID = Convert.ToInt32(reader.ReadElementString());
            reader.ReadEndElement();
        }
    }
}
