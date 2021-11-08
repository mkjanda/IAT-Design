using System;
using System.Xml;

namespace IATClient.IATConfig
{
    class BeginIATBlock : IATEvent
    {
        public ConfigFile ConfigFile { get; set; }
        public Uri BlockUri { get; set; }
        private int _BlockNum = -1, _NumItems = -1, _NumPresentations = -1;
        public int BlockNum
        {
            get
            {
                if (_BlockNum != -1)
                    return _BlockNum;
                _BlockNum = CIAT.SaveFile.GetIATBlock(BlockUri).IndexInContainer + 1;
                return _BlockNum;
            }
            set
            {
                _BlockNum = value;
            }
        }
        public int NumItems
        {
            get
            {
                if (_NumItems != -1)
                    return _NumItems;
                _NumItems = CIAT.SaveFile.GetIATBlock(BlockUri).NumItems;
                return _NumItems;
            }
            set
            {
                _NumItems = value;
            }
        }
        public int NumPresentations
        {
            get
            {
                if (_NumPresentations != -1)
                    return _NumPresentations;
                _NumPresentations= CIAT.SaveFile.GetIATBlock(BlockUri).NumPresentations;
                return _NumPresentations;
            }
            set
            {
                _NumPresentations = value;
            }
        }
        private int _InstructionsDisplayID, _LeftResponseDisplayID, _RightResponseDisplayID;
        public int InstructionsDisplayID
        {
            get
            {
                if (_InstructionsDisplayID != -1)
                    return _InstructionsDisplayID;
                _InstructionsDisplayID = ConfigFile.GetIATImage(CIAT.SaveFile.GetIATBlock(BlockUri).InstructionsUri).Id;
                return _InstructionsDisplayID;
            }
            set
            {
                _InstructionsDisplayID = value;
            }
        }
        public int LeftResponseDisplayID
        {
            get
            {
                if (_LeftResponseDisplayID != -1)
                    return _LeftResponseDisplayID;
                _LeftResponseDisplayID = ConfigFile.GetIATImage(CIAT.SaveFile.GetIATBlock(BlockUri).Key.LeftValueUri).Id;
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
                _RightResponseDisplayID = ConfigFile.GetIATImage(CIAT.SaveFile.GetIATBlock(BlockUri).Key.RightValueUri).Id;
                return _RightResponseDisplayID;
            }
            set
            {
                _RightResponseDisplayID = value;
            }
        }
        private int _AlternatedWith = -1;
        public int AlternatedWith
        {
            get
            {
                if (_AlternatedWith != -1)
                    return _AlternatedWith;
                if (CIAT.SaveFile.GetIATBlock(BlockUri).AlternateBlock == null)
                    return -1;
                _AlternatedWith = CIAT.SaveFile.GetIATBlock(BlockUri).AlternateBlock.IndexInContainer + 1;
                return _AlternatedWith;
            }
            set
            {
                _AlternatedWith = value;
            }
        }

        public BeginIATBlock()
            : base(EEventType.BeginIATBlock)
        {
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("BeginIATBlock");
            writer.WriteElementString("NumPresentations", NumPresentations.ToString());
            writer.WriteElementString("AlternatedWith", AlternatedWith.ToString());
            writer.WriteElementString("BlockNum", BlockNum.ToString());
            writer.WriteElementString("NumItems", NumItems.ToString());
            writer.WriteElementString("InstructionsDisplayID", InstructionsDisplayID.ToString());
            writer.WriteElementString("LeftResponseDisplayID", LeftResponseDisplayID.ToString());
            writer.WriteElementString("RightResponseDisplayID", RightResponseDisplayID.ToString());
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            NumPresentations = Convert.ToInt32(reader.ReadElementString());
            AlternatedWith = Convert.ToInt32(reader.ReadElementString());
            BlockNum = Convert.ToInt32(reader.ReadElementString());
            NumItems = Convert.ToInt32(reader.ReadElementString());
            InstructionsDisplayID = Convert.ToInt32(reader.ReadElementString());
            LeftResponseDisplayID = Convert.ToInt32(reader.ReadElementString());
            RightResponseDisplayID = Convert.ToInt32(reader.ReadElementString());
            reader.ReadEndElement();
        }
    }
}
