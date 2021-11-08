using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace IATClient
{
    public class CSearchCriteria : CSyncEvent
    {
        private int _SurveyNum, _ItemNum;
        private String _CriterionName;
        public enum EAction { none, store, delete, replace, retrieve };
        private EAction _Action;

        public CSearchCriteria()
        {
            _SurveyNum = -1;
            _ItemNum = -1;
            _CriterionName = String.Empty;
            _Action = EAction.none;
        }

        public CSearchCriteria(EAction action, int surveyNum, int itemNum, String criterionName)
        {
            _Action = action;
            _SurveyNum = surveyNum;
            _ItemNum = itemNum;
            _CriterionName = criterionName;
        }

        public int SurveyNum
        {
            get
            {
                return SurveyNum;
            }
        }

        public int ItemNum
        {
            get
            {
                return _ItemNum;
            }
        }

        public String CriterionName
        {
            get
            {
                return _CriterionName;
            }
            set
            {
                _CriterionName = value;
                Action = EAction.replace;
            }
        }

        public EAction Action
        {
            get
            {
                return _Action;
            }
            set {
                _Action = value;
            }
        }

        public override ESyncEvents GetEventType()
        {
            return ESyncEvents.SearchCriteria;
        }

        public override void ReadXml(XmlReader reader)
        {
            _Action = (EAction)Enum.Parse(typeof(EAction), reader["Action"]);
            reader.ReadStartElement();
            _SurveyNum = Convert.ToInt32(reader.ReadElementString());
            _ItemNum = Convert.ToInt32(reader.ReadElementString());
            _CriterionName = reader.ReadElementString();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetName());
            writer.WriteAttributeString("Action", Action.ToString());
            writer.WriteElementString("SurveyNum", SurveyNum.ToString());
            writer.WriteElementString("ItemNum", ItemNum.ToString());
            writer.WriteElementString("CriterionName", CriterionName);
            writer.WriteEndElement();
        }
    }

    public class CSearchCriteriaContainer
    {
        private CSearchCriteria [][]Criteria = null;
        private Dictionary<String, CSearchCriteria> _CriteriaDictionary = new Dictionary<String, CSearchCriteria>();
        private bool DictionaryValid = false;

        public CSearchCriteriaContainer(List<IATSurveyFile.Survey> beforeSurveys, List<IATSurveyFile.Survey> afterSurveys, List<CSyncEvent> syncEvents)
        {
            Criteria = new CSearchCriteria[beforeSurveys.Count + afterSurveys.Count][];
            for (int ctr = 0; ctr < beforeSurveys.Count; ctr++)
                Criteria[ctr] = new CSearchCriteria[beforeSurveys[ctr].NumItems];
            for (int ctr = 0; ctr < afterSurveys.Count; ctr++)
                Criteria[beforeSurveys.Count + ctr] = new CSearchCriteria[afterSurveys[ctr].NumItems];
            for (int ctr = 0; ctr < Criteria.Length; ctr++)
                Array.Clear(Criteria[ctr], 0, Criteria[ctr].Length);
            for (int ctr = 0; ctr < syncEvents.Count; ctr++)
            {
                if (syncEvents[ctr].GetEventType() == CSyncEvent.ESyncEvents.SearchCriteria)
                {
                    CSearchCriteria crit = (CSearchCriteria)syncEvents[ctr];
                    Criteria[crit.SurveyNum - 1][crit.ItemNum - 1] = crit;
                }
            }
            ConstructCriteriaDictionary();
        }

        public Dictionary<String, CSearchCriteria> CriteriaDictionary
        {
            get
            {
                if (!DictionaryValid)
                    ConstructCriteriaDictionary();
                return _CriteriaDictionary;
            }
        }

        private void ConstructCriteriaDictionary()
        {
            _CriteriaDictionary.Clear();
            for (int ctr1 = 0; ctr1 < Criteria.Length; ctr1++)
                for (int ctr2 = 0; ctr2 < Criteria[ctr1].Length; ctr2++)
                    if (Criteria[ctr1][ctr2] != null)
                    {
                        if (Criteria[ctr1][ctr2].Action != CSearchCriteria.EAction.delete)
                            _CriteriaDictionary[Criteria[ctr1][ctr2].CriterionName] = Criteria[ctr1][ctr2];
                    }
            DictionaryValid = true;
        }

        public void Put(CSearchCriteria crit)
        {
            if (Criteria[crit.SurveyNum - 1][crit.ItemNum - 1] == null)
                crit.Action = CSearchCriteria.EAction.store;
            else
                crit.Action = CSearchCriteria.EAction.replace;
            Criteria[crit.SurveyNum - 1][crit.ItemNum - 1] = crit;
            DictionaryValid = false;
        }

        public void Remove(CSearchCriteria crit)
        {
            if (Criteria[crit.SurveyNum - 1][crit.ItemNum - 1] != null)
                Criteria[crit.SurveyNum - 1][crit.ItemNum - 1].Action = CSearchCriteria.EAction.delete;
            DictionaryValid = false;
        }

        public List<CSearchCriteria> GetAll()
        {
            List<CSearchCriteria> results = new List<CSearchCriteria>();
            for (int ctr1 = 0; ctr1 < Criteria.Length; ctr1++)
                for (int ctr2 = 0; ctr2 < Criteria[ctr1].Length; ctr2++)
                    if (Criteria[ctr1][ctr2] != null)
                    {
                        if (Criteria[ctr1][ctr2].Action != CSearchCriteria.EAction.delete)
                            results.Add(Criteria[ctr1][ctr2]);
                    }
            return results;
        }

        public List<CSyncWrapper> GetUpdateList()
        {
            List<CSyncWrapper> results = new List<CSyncWrapper>();
            for (int ctr1 = 0; ctr1 < Criteria.Length; ctr1++)
                for (int ctr2 = 0; ctr2 < Criteria[ctr1].Length; ctr2++)
                    if (Criteria[ctr1][ctr2] != null)
                    {
                        if (Criteria[ctr1][ctr2].Action != CSearchCriteria.EAction.retrieve)
                            results.Add(new CSyncWrapper(Criteria[ctr1][ctr2]));
                    }
            return results;
        }
    }
}
