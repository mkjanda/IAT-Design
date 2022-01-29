/*
using System;
using System.Collections.Generic;

using System.Text;

namespace IATClient
{
    class CMaskSpecifier : DynamicSpecifier
    {
        public String Mask { get; set; }
        public KeyedDirection KeyedDir { get; set; }

        public override DynamicSpecifier.ESpecifierType SpecifierType
        {
            get
            {
                return ESpecifierType.Mask;
            }
        }

        public CMaskSpecifier()
        {
            Mask = String.Empty;
        }

        public CMaskSpecifier(String surveyName, int itemNum, List<CIATItem> iatItems, String mask)
            : base(surveyName, itemNum, iatItems)
        {
            Mask = mask;
        }

        public override bool IsValid()
        {
            if (base.IsValid() == false)
                return false;
            if (Mask == String.Empty)
                return false;
            for (int ctr = 0; ctr < Mask.Length; ctr++)
                if ((Mask[ctr] != '0') && (Mask[ctr] != '1'))
                    return false;
            return true;
        }

        public override bool LoadFromXml(System.Xml.XmlNode node)
        {
            int nodeCtr = 0;
            _ID = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            SurveyName = node.ChildNodes[nodeCtr++].InnerText;
            ItemNum = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            Mask = node.ChildNodes[nodeCtr++].InnerText;
            return true;
        }

        public override void WriteToXml(System.Xml.XmlTextWriter writer)
        {
            writer.WriteStartElement("DynamicSpecifier");
            writer.WriteAttributeString("SpecifierType", SpecifierType.ToString());
            writer.WriteElementString("ID", ID.ToString());
            writer.WriteElementString("SurveyName", SurveyName);
            writer.WriteElementString("ItemNum", ItemNum.ToString());
            writer.WriteElementString("Mask", Mask);
            writer.WriteEndElement();
        }

        public override void AddIATItem(CIATItem item, string specifierArg)
        {
            IATItems.Add(item);
            item.KeySpecifierID = ID;
            item.SpecifierArg = String.Empty;
        }
    }
}
*/