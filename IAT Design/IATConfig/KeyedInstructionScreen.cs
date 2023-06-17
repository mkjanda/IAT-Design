using System;
using System.Linq;
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
                var tups = ConfigFile.GetIATImages(CIAT.SaveFile.GetIATKey(KeyedScreen.ResponseKeyUri).LeftValueUri);
                tups.Sort((t1, t2) => t1.Bounds.X.CompareTo(t2.Bounds.X));
                _LeftResponseDisplayID = tups.First().Id;
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
                var tups = ConfigFile.GetIATImages(CIAT.SaveFile.GetIATKey(KeyedScreen.ResponseKeyUri).RightValueUri);
                tups.Sort((t1, t2) => t1.Bounds.X.CompareTo(t2.Bounds.X));
                _RightResponseDisplayID = tups.Last().Id;
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
