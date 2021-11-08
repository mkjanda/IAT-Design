using System;
using System.Collections.Generic;

using System.Text;

namespace IATClient
{
    class CTrueFalseSpecifier : CDynamicSpecifier
    {
        public CTrueFalseSpecifier() { }

        public CTrueFalseSpecifier(String surveyName, int itemNum, List<CIATItem> iatItems)
            : base(surveyName, itemNum, iatItems) { }

        public override CDynamicSpecifier.ESpecifierType SpecifierType
        {
            get
            {
                return ESpecifierType.TrueFalse;
            }
        }

        public override void WriteToXml(System.Xml.XmlTextWriter writer)
        {
            writer.WriteStartElement("DynamicSpecifier");
            writer.WriteAttributeString("SpecifierType", SpecifierType.ToString());
            writer.WriteElementString("SpecifierID", ID.ToString());
            writer.WriteElementString("SurveyName", SurveyName);
            writer.WriteElementString("ItemNum", ItemNum.ToString());
            writer.WriteEndElement();
        }

        public override bool LoadFromXml(System.Xml.XmlNode node)
        {
            int nodeCtr = 0;
            _ID = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            SurveyName = node.ChildNodes[nodeCtr++].InnerText;
            ItemNum = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            return true;
        }

        public override IATConfigFileNamespace.DynamicSpecifier GetSerializableSpecifier()
        {
            return new IATConfigFileNamespace.TrueFalseSpecifier(ID, Survey.Name, ItemNum);
        }

        public override void AddIATItem(CIATItem item, string specifierArg)
        {
            IATItems.Add(item);
            item.KeySpecifierID = ID;
            item.SpecifierArg = String.Empty;
        }
    }
}
