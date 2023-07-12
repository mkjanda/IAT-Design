using System;
using System.Xml;
using System.Xml.Linq;

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

        public override void Load(XElement elem)
        {
            base.Load(elem);
            ContinueASCIIKeyCode = Convert.ToInt32(elem.Element("ContinueASCIIKeyCode").Value);
            InstructionsDisplayID = Convert.ToInt32(elem.Element("InstructionsDisplayID").Value);
        }

    }
}
