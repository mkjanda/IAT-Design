using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace IATClient
{
    public class CDynamicIATItem : CIATItem
    {
        private CDynamicSpecifier _KeySpecifier, _InclusionSpecifier;

        public CDynamicSpecifier KeySpecifier
        {
            get
            {
                return _KeySpecifier;
            }
            set
            {
                _KeySpecifier = value;
            }
        }

        public CDynamicSpecifier InclusionSpecifier
        {
            get
            {
                return _InclusionSpecifier;
            }
            set
            {
                _InclusionSpecifier = value;
            }
        }

        public CDynamicIATItem()
        {
            _InclusionSpecifier = null;
            _KeySpecifier = null;
        }

        public CDynamicIATItem(CIATItem i)
        {
            _Stimulus = i.Stimulus;
            _KeyedDir = EKeyedDir.Dynamic;
            _KeySpecifier = null;
            _InclusionSpecifier = null;
        }

        public CDynamicIATItem(CIATItem i, CDynamicSpecifier KeySpecifier, CDynamicSpecifier InclusionSpecifier)
        {
            _Stimulus = i.Stimulus;
            _KeyedDir = EKeyedDir.Dynamic;
            _KeySpecifier = KeySpecifier;
            _InclusionSpecifier = InclusionSpecifier;
        }

        public override EDynamicIATItemType GetDynamicType()
        {
            return EDynamicIATItemType.Dynamic;
        }

        public override bool IsValid()
        {
            if (base.IsValid() == false)
                return false;
            if (_KeySpecifier == null)
                return false;
            return true;
        }

        public override bool LoadFromXml(System.Xml.XmlNode node)
        {
            int nodeCtr = 0;
            bool bHasInclusionSpecifier = Convert.ToBoolean(node.Attributes["HasInclusionSpecifier"].Value);
            _KeyedDir = (EKeyedDir)Enum.Parse(typeof(EKeyedDir), node.ChildNodes[nodeCtr++].InnerText);
            if (_Stimulus != null)
                _Stimulus.Dispose();
            _Stimulus = CDisplayItem.CreateFromXml(node.ChildNodes[nodeCtr++]);
            _KeySpecifier = CDynamicSpecifier.CreateFromXml(node.ChildNodes[nodeCtr++]);
            if (bHasInclusionSpecifier)
                _InclusionSpecifier = CDynamicSpecifier.CreateFromXml(node.ChildNodes[nodeCtr++]);
            else
                _InclusionSpecifier = null;
            return true;
        }

        public override void WriteToXml(System.Xml.XmlTextWriter writer)
        {
            writer.WriteStartElement("IATItem");
            writer.WriteAttributeString("DynamicType", GetDynamicType().ToString());
            writer.WriteAttributeString("HasInclusionSpecifier", (_InclusionSpecifier != null).ToString());
            writer.WriteElementString("KeyedDir", KeyedDir.ToString());
            Stimulus.WriteToXml(writer);
            KeySpecifier.WriteToXml(writer);
            if (_InclusionSpecifier != null)
                InclusionSpecifier.WriteToXml(writer);
            writer.WriteEndElement();
        }
    }
}
