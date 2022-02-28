using System;
using System.Collections;
using System.Xml;

namespace IATClient.IATConfig
{
    class IATItem : IATEvent, IEqualityComparer
    {
        public ConfigFile ConfigFile { get; set; }
        public Uri ItemUri { get; set; }

        public new bool Equals(object A, object B)
        {
            var a = A as IATItem;
            var b = B as IATItem;
            if ((a.StimulusDisplayID == b.StimulusDisplayID) && (a.BlockNum == b.BlockNum))
                return true;
            return false;
        }

        public int GetHashCode(object A)
        {
            unchecked
            {
                var a = A as IATItem;
                int hash = 17;
                hash = hash * 23 + a.ItemNum.GetHashCode();
                hash = hash * 23 + a.BlockNum.GetHashCode();
                return hash;
            }
        }

        /*
        private String _SpecifierArg = String.Empty;
        public String SpecifierArg
        {
            get
            {
                if (_SpecifierArg != String.Empty)
                    return _SpecifierArg;
                _SpecifierArg = CIAT.SaveFile.GetIATItem(ItemUri).SpecifierArg;
                return _SpecifierArg;
            }
            set
            {
                _SpecifierArg = value;
            }
        }
        private int _SpecifierID = -1;
        public int SpecifierID
        {
            get
            {
                if (_SpecifierID != -1)
                    return _SpecifierID;
                _SpecifierID = CIAT.SaveFile.GetIATItem(ItemUri).KeySpecifierID;
                return _SpecifierID;
            }
            set
            {
                _SpecifierID = value;
            }
        }
        */
        private KeyedDirection _KeyedDir = KeyedDirection.None;
        public KeyedDirection KeyedDir
        {
            get
            {
                return _KeyedDir;
            }
            set
            {
                _KeyedDir = value;
            }
        }
        private static int ItemCtr = 0;
        private int _ItemNum = ++ItemCtr;
        public int ItemNum
        {
            get
            {
                return _ItemNum;
            }
            set
            {
                _ItemNum = value;
            }
        }
        public int BlockNum { get; set; }
        public int OriginatingBlock { get; set; }
        private int _StimulusDisplayID = -1;
        public int StimulusDisplayID
        {
            get
            {
                if (_StimulusDisplayID != -1)
                    return _StimulusDisplayID;
                _StimulusDisplayID = ConfigFile.GetIATImage(CIAT.SaveFile.GetIATItem(ItemUri).StimulusUri).Id;
                return _StimulusDisplayID;
            }
            set
            {
                _StimulusDisplayID = value;
            }
        }

        public IATItem()
            : base(EEventType.IATItem)
        {
        }

        public CIATItem GetItem()
        {
            return CIAT.SaveFile.GetIATItem(ItemUri);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("IATItem");
            writer.WriteElementString("ItemNum", ItemNum.ToString());
            writer.WriteElementString("BlockNum", BlockNum.ToString());
            writer.WriteElementString("StimulusDisplayID", StimulusDisplayID.ToString());
            writer.WriteElementString("OriginatingBlock", OriginatingBlock.ToString());
            writer.WriteElementString("KeyedDir", KeyedDir.ToString());
            //        writer.WriteElementString("SpecifierID", SpecifierID.ToString());
            //      writer.WriteElementString("SpecifierArg", SpecifierArg);
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            ItemNum = Convert.ToInt32(reader.ReadElementString());
            BlockNum = Convert.ToInt32(reader.ReadElementString());
            StimulusDisplayID = Convert.ToInt32(reader.ReadElementString());
            OriginatingBlock = Convert.ToInt32(reader.ReadElementString());
            KeyedDir = KeyedDirection.FromString(reader.ReadElementString());
            //          SpecifierID = Convert.ToInt32(reader.ReadElementString());
            //            SpecifierArg = reader.ReadElementString();
            reader.ReadEndElement();
        }
    }
}
