using System;
using System.CodeDom;
using System.Linq;
using System.Xml;
using System.Xml.Linq;

namespace IATClient.IATConfig
{
    class BeginIATBlock : IATEvent
    {
        public Uri BlockUri { get; set; }
        private int _BlockNum = -1, _NumItems = -1, _NumPresentations = -1;
        private int _InstructionsDisplayID = -1, _LeftResponseDisplayID = -1, _RightResponseDisplayID = -1;
        public int BlockNum
        {
            get
            {
                if (_BlockNum != -1)
                    return _BlockNum;
                _BlockNum = CIAT.SaveFile.GetIATBlock(BlockUri).IndexInContainer + 1;
                return _BlockNum;
            }
            private set
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
            private set
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
                _NumPresentations = CIAT.SaveFile.GetIATBlock(BlockUri).NumPresentations;
                return _NumPresentations;
            }
            private set
            {
                _NumPresentations = value;
            }
        }
        public int InstructionsDisplayID
        {
            get
            {
                if (_InstructionsDisplayID != -1)
                    return _InstructionsDisplayID;
                _InstructionsDisplayID = ConfigFile.GetIATImage(CIAT.SaveFile.GetIATBlock(BlockUri).InstructionsUri).Id;
                return _InstructionsDisplayID; 
            }
            private set
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
                var tups = ConfigFile.GetIATImages(CIAT.SaveFile.GetIATBlock(BlockUri).Key.LeftValueUri);
                tups.Sort((t1, t2) => t1.Bounds.X.CompareTo(t2.Bounds.X));
                _LeftResponseDisplayID = tups.First().Id;
                return _LeftResponseDisplayID;
            }
            private set
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
                var tups = ConfigFile.GetIATImages(CIAT.SaveFile.GetIATBlock(BlockUri).Key.RightValueUri);
                tups.Sort((t1, t2) => t1.Bounds.X.CompareTo(t2.Bounds.X));
                _RightResponseDisplayID = tups.Last().Id;
                return _RightResponseDisplayID;
            }
            private set
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
            private set
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


        public override void Load(XElement elem)
        {
            NumPresentations = Convert.ToInt32(elem.Element("NumPresentations").Value);
            AlternatedWith = Convert.ToInt32(elem.Element("AlternatedWith").Value);
            BlockNum = Convert.ToInt32(elem.Element("BlockNum").Value);
            NumItems = Convert.ToInt32(elem.Element("NumItems").Value);
            InstructionsDisplayID = Convert.ToInt32(elem.Element("InstructionsDisplayID").Value);
            RightResponseDisplayID = Convert.ToInt32(elem.Element("RightResponseDisplayID").Value);
            LeftResponseDisplayID = Convert.ToInt32(elem.Element("LeftResponseDisplayID").Value);
        }
    }
}
