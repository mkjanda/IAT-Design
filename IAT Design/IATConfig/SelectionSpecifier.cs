using System;
using System.Collections.Generic;
using System.Xml;

namespace IATClient.IATConfig
{
    class SelectionSpecifier : DynamicSpecifier
    {
        private List<String> _ResponseVals = new List<String>();

        public List<String> ResponseVals
        {
            get
            {
                return _ResponseVals;
            }
        }

        public SelectionSpecifier()
            : base(ESpecifierType.Selection)
        {
        }

        public SelectionSpecifier(int id, String surveyName, int itemNum, List<String> responseVals)
            : base(ESpecifierType.Selection, id, surveyName, itemNum)
        {
            ResponseVals.AddRange(responseVals);
        }

        public override void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            base.ReadXml(reader);
            int nKeys = Convert.ToInt32(reader["NumKeySpecifiers"]);
            ResponseVals.Clear();
            reader.ReadStartElement();
            for (int ctr = 0; ctr < nKeys; ctr++)
                ResponseVals.Add(reader.ReadElementString());
            reader.ReadEndElement();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("DynamicSpecifier");
            writer.WriteAttributeString("SpecifierType", Type.ToString());
            writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
            writer.WriteAttributeString("xsi:type", "SelectionSpecifier");
            base.WriteXml(writer);
            writer.WriteStartElement("KeySpecifiers");
            writer.WriteAttributeString("NumKeySpecifiers", ResponseVals.Count.ToString());
            foreach (String key in ResponseVals)
                writer.WriteElementString("KeySpecifier", key);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }
    }
}
