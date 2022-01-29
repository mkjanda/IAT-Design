/*using System;
using System.Collections.Generic;

using System.Text;
using System.Xml;

namespace IATClient
{
    public abstract class DynamicSpecifier : IStoredInXml
    {
        private CSurvey _Survey;
        private String _SurveyName;
        private int _ItemNum;
        private List<CIATItem> _IATItems = new List<CIATItem>();
        protected int _ID;
        public enum ESpecifierType { Range, Mask, Selection, None, TrueFalse };

        private static List<int> IDList = new List<int>();
        private static Dictionary<int, DynamicSpecifier> KeySpecifierDictionary = new Dictionary<int, DynamicSpecifier>();

        public static void ClearSpecifierDictionary()
        {
            IDList.Clear();
            KeySpecifierDictionary.Clear();
        }

        public CSurvey Survey
        {
            get
            {
                return _Survey;
            }
            set
            {
                _Survey = value;
            }
        }

        protected static int GetNewID()
        {
            if (IDList.Contains(KeySpecifierDictionary.Keys.Count))
                for (int ctr = 0; ctr < IDList.Count; ctr++)
                    if (!IDList.Contains(ctr))
                    {
                        IDList.Add(ctr);
                        return ctr;
                    }
            IDList.Add(IDList.Count);
            return IDList.Count;
        }


        public static void DeleteSpecifier(int ID)
        {
            IDList.Remove(ID);
            DynamicSpecifier ds = KeySpecifierDictionary[ID];
            KeySpecifierDictionary.Remove(ID);
            foreach (CIATItem i in ds.IATItems)
            {
                i.KeySpecifierID = -1;
                i.SpecifierArg = String.Empty;
            }
        }

        public virtual void ClearIATItems()
        {
            foreach (CIATItem i in IATItems)
            {
                i.KeySpecifierID = -1;
                i.SpecifierArg = String.Empty;
            }
            IATItems.Clear();
        }

        public static void CompactSpecifierDictionary(CIAT iat)
        {
            List<int> unusedSpecifiers = new List<int>();

            foreach (DynamicSpecifier ds in KeySpecifierDictionary.Values)
            {
                bool bFound = false;
                for (int ctr1 = 0; ctr1 < iat.Blocks.Count; ctr1++)
                {
                    for (int ctr2 = 0; ctr2 < iat.Blocks[ctr1].NumItems; ctr2++)
                    {
                        if (iat.Blocks[ctr1][ctr2].KeySpecifierID == ds.ID)
                        {
                            bFound = true;
                            break;
                        }
                    }
                    if (bFound)
                        break;
                }
                if (!bFound)
                    unusedSpecifiers.Add(ds.ID);
            }
            foreach (int i in unusedSpecifiers)
                KeySpecifierDictionary.Remove(i);
        }

        public static DynamicSpecifier GetSpecifier(int ID)
        {
            return KeySpecifierDictionary[ID];
        }

        public static void AddSpecifier(DynamicSpecifier specifier)
        {
            KeySpecifierDictionary[specifier.ID] = specifier;
        }

        public static void RemoveIATItem(int ID, CIATItem item)
        {
            DynamicSpecifier ds = KeySpecifierDictionary[ID];
            item.SpecifierArg = String.Empty;
            item.KeySpecifierID = -1;
            ds.RemoveIATItem(item);
        }

        public static List<DynamicSpecifier> GetAllSpecifiers()
        {
            return new List<DynamicSpecifier>(KeySpecifierDictionary.Values);
        }

        public virtual void RemoveIATItem(CIATItem item)
        {
            IATItems.Remove(item);
        }

        public List<CIATItem> IATItems
        {
            get
            {
                return _IATItems;
            } 
        }

        public String SurveyName
        {
            get
            {
                return _SurveyName;
            }
            set
            {
                _SurveyName = value;
            }
        }

        public int ID
        {
            get
            {
                return _ID;
            }
        }


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

        public DynamicSpecifier()
        {
            SurveyName = String.Empty;
            ItemNum = -1;
            _ID = GetNewID();
            IDList.Add(ID);
        }

        public DynamicSpecifier(String surveyName, int itemNum, List<CIATItem> IATitems)
        {
            _SurveyName = surveyName;
            _ItemNum = itemNum;
            _ID = GetNewID();
            this.IATItems.AddRange(IATitems);
            foreach (CIATItem i in this.IATItems)
            {
                i.KeySpecifierID = _ID;
                i.SpecifierArg = String.Empty;
            }
        }

        public abstract ESpecifierType SpecifierType { get; }

        static public void LoadKeySpecifierDictionary(XmlNode node, CIAT iat)
        {
            int nSpecifiers = Convert.ToInt32(node.Attributes["NumSpecifiers"].Value);

            for (int ctr = 0; ctr < nSpecifiers; ctr++)
            {

                DynamicSpecifier result = null;

                ESpecifierType type = (ESpecifierType)Enum.Parse(typeof(ESpecifierType), node.ChildNodes[ctr].Attributes["SpecifierType"].Value);
                switch (type)
                {
                    case ESpecifierType.Range:
                        result = new CRangeSpecifier();
                        result.LoadFromXml(node.ChildNodes[ctr]);
                        break;

                    case ESpecifierType.Mask:
                        result = new CMaskSpecifier();
                        result.LoadFromXml(node.ChildNodes[ctr]);
                        break;

                    case ESpecifierType.Selection:
                        result = new CSelectionSpecifier();
                        result.LoadFromXml(node.ChildNodes[ctr]);
                        break;

                    case ESpecifierType.TrueFalse:
                        result = new CTrueFalseSpecifier();
                        result.LoadFromXml(node.ChildNodes[ctr]);
                        break;
                }
                KeySpecifierDictionary.Add(result.ID, result);
            }
            ResolveSpecifierSurveys(iat);
        }

        public static void WriteKeySpecifierDictionary(XmlTextWriter writer)
        {
            writer.WriteStartElement("KeySpecifierDictionary");
            writer.WriteAttributeString("NumSpecifiers", KeySpecifierDictionary.Count.ToString());
            foreach (DynamicSpecifier ds in KeySpecifierDictionary.Values)
                ds.WriteToXml(writer);
            writer.WriteEndElement();
        }

        private static void ResolveSpecifierSurveys(CIAT iat)
        {
            foreach (DynamicSpecifier ds in KeySpecifierDictionary.Values)
            {
                for (int ctr = 0; ctr < iat.BeforeSurvey.Count; ctr++)
                    if (iat.BeforeSurvey[ctr].Name == ds.SurveyName)
                        ds.Survey = iat.BeforeSurvey[ctr];
                for (int ctr = 0; ctr < iat.AfterSurvey.Count; ctr++)
                    if (iat.AfterSurvey[ctr].Name == ds.SurveyName)
                        ds.Survey = iat.AfterSurvey[ctr];
            }
        }

        public abstract bool LoadFromXml(XmlNode node);
        public abstract void WriteToXml(XmlTextWriter writer);
        public virtual bool IsValid()
        {
            if (SurveyName == String.Empty)
                return false;
            if (ItemNum == -1)
                return false;
            return true;
        }

        public abstract IATConfig.DynamicSpecifier GetSerializableSpecifier();
        public abstract void AddIATItem(CIATItem item, String specifierArg);
        
    }
}
*/