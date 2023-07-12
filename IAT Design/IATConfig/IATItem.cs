using System;
using System.Collections;
using System.Xml;
using System.Xml.Linq;

namespace IATClient.IATConfig
{
    class IATItem : IATEvent, IEqualityComparer
    {
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

        public KeyedDirection KeyedDir { get; set; }
        private static int ItemCtr = 0;
        public int ItemNum { get; set; } = ++ItemCtr;
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
            private set
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

        public override void Load(XElement elem)
        {
            ItemNum = Convert.ToInt32(elem.Element("ItemNum").Value);
            BlockNum = Convert.ToInt32(elem.Element("BlockNum").Value);
            StimulusDisplayID = Convert.ToInt32(elem.Element("StimulusDisplayID").Value);
            OriginatingBlock = Convert.ToInt32(elem.Element("OriginatingBlock").Value);
            KeyedDir = KeyedDirection.FromString(elem.Element("KeyedDir").Value);
        }
    }
}
