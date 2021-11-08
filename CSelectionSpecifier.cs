using System;
using System.Collections.Generic;

using System.Text;

namespace IATClient
{
    class CSelectionSpecifier : CDynamicSpecifier
    {
        private Dictionary<String, List<CIATItem>> _KeyMap = new Dictionary<String, List<CIATItem>>();

        public Dictionary<String, List<CIATItem>> KeyMap
        {
            get
            {
                return _KeyMap;
            }
        }


        public override CDynamicSpecifier.ESpecifierType SpecifierType
        {
            get { return ESpecifierType.Selection; }
        }

        public override void RemoveIATItem(CIATItem item)
        {
            base.RemoveIATItem(item);
            foreach (List<CIATItem> itemList in _KeyMap.Values)
                itemList.Remove(item);
            item.KeySpecifierID = -1;
            item.SpecifierArg = String.Empty;
        }

        public CSelectionSpecifier()
        {
        }

        public CSelectionSpecifier(String surveyName, int itemNum, List<List<CIATItem>> iatItems, List<String> responseVals)
        {
            _KeyMap.Clear();
            for (int ctr = 0; ctr < responseVals.Count; ctr++)
            {
                KeyMap[responseVals[ctr]] = new List<CIATItem>();
                KeyMap[responseVals[ctr]].AddRange(iatItems[ctr]);
            }
            SurveyName = surveyName;
            ItemNum = itemNum;
            _ID = GetNewID();
            for (int ctr = 0; ctr < iatItems.Count; ctr++)
            {
                foreach (CIATItem i in iatItems[ctr])
                {
                    i.KeySpecifierID = ID;
                    i.SpecifierArg = responseVals[ctr];
                    IATItems.Add(i);
                }
            }
        }

        public override bool IsValid()
        {
            if (base.IsValid() == false)
                return false;
            if (KeyMap.Keys.Count != IATItems.Count)
                return false;
            return true;
        }

        public override void WriteToXml(System.Xml.XmlTextWriter writer)
        {
            writer.WriteStartElement("DynamnicSpecifier");
            writer.WriteAttributeString("SpecifierType", SpecifierType.ToString());
            writer.WriteElementString("SpecifierID", ID.ToString());
            writer.WriteElementString("SurveyName", SurveyName);
            writer.WriteElementString("ItemNum", ItemNum.ToString());
            writer.WriteStartElement("KeySpecifiers");
            writer.WriteAttributeString("NumValues", KeyMap.Keys.Count.ToString());
            foreach (String s in KeyMap.Keys)
                writer.WriteElementString("KeySpecifier", s);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public override bool LoadFromXml(System.Xml.XmlNode node)
        {
            int nodeCtr = 0;
            KeyMap.Clear();
            _ID = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            SurveyName = node.ChildNodes[nodeCtr++].InnerText;
            ItemNum = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            int nVals = Convert.ToInt32(node.ChildNodes[nodeCtr].Attributes["NumValues"].Value);
            for (int ctr = 0; ctr < nVals; ctr++)
                KeyMap[node.ChildNodes[nodeCtr].ChildNodes[ctr].InnerText] = new List<CIATItem>();
            return true;
        }

        public override IATConfigFileNamespace.DynamicSpecifier GetSerializableSpecifier()
        {
            return new IATConfigFileNamespace.SelectionSpecifier(ID, Survey.Name, ItemNum, new List<String>(KeyMap.Keys));
        }

        public override void AddIATItem(CIATItem item, string specifierArg)
        {
            foreach (String responseVal in KeyMap.Keys)
                if (responseVal == specifierArg)
                    KeyMap[responseVal].Add(item);
            IATItems.Add(item);
            item.KeySpecifierID = ID;
            item.SpecifierArg = specifierArg;
        }

        public void ClearIATItems(String responseVal)
        {
            foreach (CIATItem i in KeyMap[responseVal])
            {
                IATItems.Remove(i);
                i.KeySpecifierID = -1;
                i.SpecifierArg = String.Empty;
            }
            KeyMap[responseVal].Clear();
        }

        public override void ClearIATItems()
        {
            base.ClearIATItems();
            foreach (List<CIATItem> l in KeyMap.Values)
                l.Clear();
        }

        public void AddIATItems(String responseVal, List<CIATItem> items)
        {
            KeyMap[responseVal].AddRange(items);
            IATItems.AddRange(items);
            foreach (CIATItem i in items)
            {
                i.KeySpecifierID = ID;
                i.SpecifierArg = responseVal;
            }
        }
    }
}
