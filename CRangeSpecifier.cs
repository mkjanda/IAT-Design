using System;
using System.Collections.Generic;

using System.Text;

namespace IATClient
{
    class CRangeSpecifier : CDynamicSpecifier
    {
        public int Cutoff { get; set; }
        public bool IsReverseScored { get; set; }
        public readonly List<KeyedDirection> BelowCutoffKeys = new List<KeyedDirection>();
        public bool CutoffExcludes { get; set; }
        public override ESpecifierType SpecifierType
        {
            get
            {
                return ESpecifierType.Range;
            }
        }

        public CRangeSpecifier()
        {
            Cutoff = -1;
            IsReverseScored = false;
        }

        public CRangeSpecifier(String SurveyName, int itemNum, List<CIATItem> iatItems,
            int cutoff, bool cutoffExcludes)
            : base(SurveyName, itemNum, iatItems)
        {
            Cutoff = cutoff;
            IsReverseScored = false;
            CutoffExcludes = cutoffExcludes;
            BelowCutoffKeys.Clear();
        }

        public override void WriteToXml(System.Xml.XmlTextWriter writer)
        {
            writer.WriteStartElement("DynamicSpecifier");
            writer.WriteAttributeString("SpecifierType", SpecifierType.ToString());
            writer.WriteElementString("SpecifierID", ID.ToString());
            writer.WriteElementString("SurveyName", SurveyName);
            writer.WriteElementString("ItemNum", ItemNum.ToString());
            writer.WriteElementString("Cutoff", Cutoff.ToString());
            writer.WriteElementString("CutoffExcludes", CutoffExcludes.ToString());
            writer.WriteElementString("IsReverseScored", IsReverseScored.ToString());
            writer.WriteEndElement();
        }

        public override bool LoadFromXml(System.Xml.XmlNode node)
        {
            int nodeCtr = 0;
            _ID = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            SurveyName = node.ChildNodes[nodeCtr++].InnerText;
            ItemNum = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            Cutoff = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            CutoffExcludes = Convert.ToBoolean(node.ChildNodes[nodeCtr++].InnerText);
            IsReverseScored = Convert.ToBoolean(node.ChildNodes[nodeCtr++].InnerText);
            return true;
        }

        public override bool IsValid()
        {
            if (base.IsValid() == false)
                return false;
            if (Cutoff != -1)
                return true;
            return false;
        }

        public override IATConfigFileNamespace.DynamicSpecifier GetSerializableSpecifier()
        {
            return new IATConfigFileNamespace.RangeSpecifier(ID, Survey.Name, ItemNum, Cutoff, CutoffExcludes, IsReverseScored);
        }

        public override void AddIATItem(CIATItem item, string specifierArg)
        {
            IATItems.Add(item);
            item.KeySpecifierID = ID;
            item.SpecifierArg = String.Empty;
        }
    }
}
