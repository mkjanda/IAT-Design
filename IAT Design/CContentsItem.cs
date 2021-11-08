using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace IATClient
{
    public class CContentsItem : IStoredInXml, IAlternationGroupItem
    {
        public enum EType : int { IATBlock, IATPracticeBlock, InstructionBlock, BeforeSurvey, AfterSurvey };
        private EType _Type;
        public EType Type
        {
            get
            {
                return _Type;
            }
        }
        private object _Item;
        public object Item
        {
            get
            {
                return _Item;
            }
        }

        public Type ItemType
        {
            get
            {
                switch (_Type)
                {
                    case EType.IATBlock:
                        return typeof(CIATBlock);

                    case EType.IATPracticeBlock:
                        return typeof(CIATBlock);

                    case EType.AfterSurvey:
                        return typeof(List<CSurveyItem>);

                    case EType.BeforeSurvey:
                        return typeof(List<CSurveyItem>);

                    case EType.InstructionBlock:
                        return typeof(List<CInstructionScreen>);

                    default:
                        throw new Exception();
                }
            }
        }

        private String _Name;
        public String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        private CIAT OwningIAT;

        public int IndexInIATContainer
        {
            get
            {
                switch (Type)
                {
                    case EType.BeforeSurvey:
                        return OwningIAT.BeforeSurvey.IndexOf((CSurvey)Item);

                    case EType.AfterSurvey:
                        return OwningIAT.AfterSurvey.IndexOf((CSurvey)Item);

                    case EType.IATBlock:
                        return OwningIAT.Blocks.IndexOf((CIATBlock)Item);

                    case EType.IATPracticeBlock:
                        return OwningIAT.PracticeBlocks.IndexOf((CIATBlock)Item);

                    case EType.InstructionBlock:
                        return OwningIAT.InstructionBlocks.IndexOf((CInstructionBlock)Item);

                    default:
                        throw new Exception();
                }
            }
        }

        public CContentsItem(CIAT IAT, object Item, EType type)
        {
            _Type = type;
            _Item = Item;
            OwningIAT = IAT;
        }

        public bool IsValid()
        {
            if (Name != String.Empty)
                return true;
            return false;
        }

        public void WriteToXml(XmlTextWriter writer)
        {
            writer.WriteStartElement("ContentsItem");
            writer.WriteAttributeString("Type", ((int)Type).ToString());
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Index", IndexInIATContainer.ToString());
            writer.WriteEndElement();
        }

        public bool LoadFromXml(XmlNode node)
        {
            throw new NotImplementedException();
        }

        public void DeleteFromIAT()
        {
            CSurvey s = null;
            switch (Type)
            {
                case EType.AfterSurvey:
                    s = (CSurvey)Item;
                    OwningIAT.DeleteAfterSurvey(s);
                    break;

                case EType.BeforeSurvey:
                    s = (CSurvey)Item;
                    OwningIAT.DeleteBeforeSurvey(s);
                    break;

                case EType.IATBlock:
                    OwningIAT.Blocks.RemoveAt(IndexInIATContainer);
                    break;

                case EType.IATPracticeBlock:
                    OwningIAT.PracticeBlocks.RemoveAt(IndexInIATContainer);
                    break;

                case EType.InstructionBlock:
                    OwningIAT.InstructionBlocks.RemoveAt(IndexInIATContainer);
                    break;
            }
            OwningIAT.Contents.Remove(this);
        }

        public void AddToIAT(int InsertionIndex)
        {
            int IATContainerInsertNdx = 0;
            CSurvey s;
            switch (Type)
            {
                case EType.AfterSurvey:
                    s = (CSurvey)Item;
                    if (s.Name == String.Empty)
                        s.Name = String.Format("Post IAT Survey #{0}", OwningIAT.AfterSurvey.Count + 1);
                    OwningIAT.AfterSurvey.Add(s);
                    break;

                case EType.BeforeSurvey:
                    s = (CSurvey)Item;
                    if (s.Name == String.Empty)
                        s.Name = String.Format("Pre IAT Survey #{0}", OwningIAT.BeforeSurvey.Count + 1);
                    OwningIAT.BeforeSurvey.Add(s);
                    break;

                case EType.IATBlock:
                    for (int ctr = 0; ctr < InsertionIndex; ctr++)
                        if (OwningIAT.Contents[ctr].Type == EType.IATBlock)
                            IATContainerInsertNdx++;
                    OwningIAT.Blocks.Insert(IATContainerInsertNdx, (CIATBlock)Item);
                    break;

                case EType.IATPracticeBlock:
                    for (int ctr = 0; ctr < InsertionIndex; ctr++)
                        if (OwningIAT.Contents[ctr].Type == EType.IATPracticeBlock)
                            IATContainerInsertNdx++;
                    OwningIAT.PracticeBlocks.Insert(IATContainerInsertNdx, (CIATBlock)Item);
                    break;

                case EType.InstructionBlock:
                    for (int ctr = 0; ctr < InsertionIndex; ctr++)
                        if (OwningIAT.Contents[ctr].Type == EType.InstructionBlock)
                            IATContainerInsertNdx++;
                    OwningIAT.InstructionBlocks.Insert(IATContainerInsertNdx, (CInstructionBlock)Item);
                    break;
            }
            OwningIAT.Contents.Insert(InsertionIndex, this);
        }

        public static CContentsItem CreateFromXml(XmlNode node, CIAT OwningIAT)
        {
            EType type = (EType)Convert.ToInt32(node.Attributes["Type"].InnerText);
            int ndx = Convert.ToInt32(node.ChildNodes[1].InnerText);
            object Item;
            switch (type)
            {
                case EType.AfterSurvey:
                    Item = OwningIAT.AfterSurvey[ndx];
                    break;

                case EType.BeforeSurvey:
                    Item = OwningIAT.BeforeSurvey[ndx];
                    break;

                case EType.IATBlock:
                    Item = OwningIAT.Blocks[ndx];
                    break;

                case EType.IATPracticeBlock:
                    Item = OwningIAT.PracticeBlocks[ndx];
                    break;

                case EType.InstructionBlock:
                    Item = OwningIAT.InstructionBlocks[ndx];
                    break;

                default:
                    return null;
            }

            CContentsItem ContentsItem = new CContentsItem(OwningIAT, Item, type);
            ContentsItem.Name = node.ChildNodes[0].InnerText;

            return ContentsItem;
        }

        public bool IsInstructionBlock
        {
            get
            {
                if (Type == EType.InstructionBlock)
                    return true;
                return false;
            }
        }

        public bool IsIATBlock
        {
            get
            {
                if (Type == EType.IATBlock)
                    return true;
                return false;
            }
        }

        public bool IsIATPracticeBlock
        {
            get
            {
                if (Type == EType.IATPracticeBlock)
                    return true;
                return false;
            }
        }

        public bool IsSurvey
        {
            get
            {
                if ((Type == EType.AfterSurvey) || (Type == EType.BeforeSurvey))
                    return true;
                return false;
            }
        }

        public object Self
        {
            get
            {
                return Item;
            }
        }

        public bool HasAlternateItem
        {
            get
            {
                if (IsInstructionBlock)
                    return (((CInstructionBlock)Item).AlternateInstructionBlock != null);
                else if (IsIATBlock || IsIATPracticeBlock)
                    return (((CIATBlock)Item).AlternateBlock != null);
                else if (IsSurvey)
                    return (AlternationGroup != null);
                return false;
            }
        }

        AlternationGroup _AlternationGroup = null;

        public AlternationGroup AlternationGroup
        {
            get
            {
                return _AlternationGroup;
            }
            set
            {
                if ((_AlternationGroup != null) && (value != null))
                    throw new Exception("Attempt was made to modify the value of an alternation group value as opposed to disposing of the owning alternation group.");
                _AlternationGroup = value;
            }
        }

    }
}
