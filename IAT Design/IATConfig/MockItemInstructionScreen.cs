using System;
using System.Xml;

namespace IATClient.IATConfig
{
    class MockItemInstructionScreen : KeyedInstructionScreen
    {
        private int _StimulusDisplayID = -1;
        private Nullable<bool> _ErrorMarkIsDisplayed = null, _OutlineLeftResponse = null, _OutlineRightResponse = null;
        private CMockItemScreen MockItemScreen
        {
            get
            {
                return InstructionScreen as CMockItemScreen;
            }
        }


        public int StimulusDisplayID
        {
            get
            {
                if (_StimulusDisplayID != -1)
                    return _StimulusDisplayID;
                _StimulusDisplayID = ConfigFile.GetIATImage(MockItemScreen.StimulusUri).Id;
                return _StimulusDisplayID;
            }
            set
            {
                _StimulusDisplayID = value;
            }
        }


        public bool ErrorMarkIsDisplayed
        {
            get
            {
                if (_ErrorMarkIsDisplayed.HasValue)
                    return _ErrorMarkIsDisplayed.Value;
                _ErrorMarkIsDisplayed = MockItemScreen.InvalidResponseFlag;
                return _ErrorMarkIsDisplayed.Value;
            }
            set
            {
                _ErrorMarkIsDisplayed = value;
            }
        }

        public bool OutlineLeftResponse
        {
            get
            {
                if (_OutlineLeftResponse.HasValue)
                    return _OutlineLeftResponse.Value;
                _OutlineLeftResponse = MockItemScreen.InvalidResponseFlag;
                return _OutlineLeftResponse.Value;
            }
            set
            {
                _OutlineLeftResponse = value;
            }
        }

        public bool OutlineRightResponse
        {
            get
            {
                if (_OutlineRightResponse.HasValue)
                    return _OutlineRightResponse.Value;
                _OutlineRightResponse = MockItemScreen.InvalidResponseFlag;
                return _OutlineRightResponse.Value;
            }
            set
            {
                _OutlineRightResponse = value;
            }
        }

        public MockItemInstructionScreen()
            : base(EEventType.MockItemInstructionScreen) { }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("IATEvent");
            writer.WriteAttributeString("EventType", EventType.ToString());
            writer.WriteElementString("ContinueASCIIKeyCode", ContinueASCIIKeyCode.ToString());
            writer.WriteElementString("ContinueInstructionsID", ContinueInstructionsDisplayID.ToString());
            writer.WriteElementString("LeftResponseDisplayID", LeftResponseDisplayID.ToString());
            writer.WriteElementString("RightResponseDisplayID", RightResponseDisplayID.ToString());
            writer.WriteElementString("StimulusDisplayID", StimulusDisplayID.ToString());
            writer.WriteElementString("InstructionsDisplayID", InstructionsDisplayID.ToString());
            writer.WriteElementString("ErrorMarkIsDisplayed", ErrorMarkIsDisplayed.ToString());
            writer.WriteElementString("OutlineLeftResponse", OutlineLeftResponse.ToString());
            writer.WriteElementString("OutlineRightResponse", OutlineRightResponse.ToString());
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
            StimulusDisplayID = Convert.ToInt32(reader.ReadElementString());
            InstructionsDisplayID = Convert.ToInt32(reader.ReadElementString());
            ErrorMarkIsDisplayed = Convert.ToBoolean(reader.ReadElementString());
            OutlineLeftResponse = Convert.ToBoolean(reader.ReadElementString());
            OutlineRightResponse = Convert.ToBoolean(reader.ReadElementString());
            reader.ReadEndElement();
        }
    }
}
