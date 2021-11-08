using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using IATClient.IATResultSetNamespaceV2;

namespace IATClient
{
    class CSubjectID : CSyncEvent
    {
        private List<int> _SurveyNumList = new List<int>();
        private List<int> _ItemNumList = new List<int>();
        public enum EAction { none, update, store, delete, retrieve };
        private EAction _Action = EAction.none;

        public EAction Action
        {
            get
            {
                return _Action;
            }
            set
            {
                _Action = value;
            }
        }

        public List<int> SurveyNumList
        {
            get {
                return _SurveyNumList;
            }
        }

        public List<int> ItemNumList
        {
            get {
                return _ItemNumList;
            }
        }

        public CSubjectID()
        {
            Action = EAction.retrieve;
        }

        public CSubjectID(EAction action)
        {
            Action = action;
        }

        public override ESyncEvents GetEventType()
        {
            return ESyncEvents.SubjectID;
        }

        public String GetSubjectID(IResultSet resultSet)
        {
            String result = String.Empty;
            for (int ctr = 0; ctr < SurveyNumList.Count; ctr++)
            {
                if (SurveyNumList[ctr] < resultSet.BeforeSurveys.Length)
                    result += resultSet.BeforeSurveys[SurveyNumList[ctr]][ItemNumList[ctr]];
                else
                    result += resultSet.AfterSurveys[SurveyNumList[ctr] - resultSet.BeforeSurveys.Length][ItemNumList[ctr]];
                if (ctr != SurveyNumList.Count - 1)
                    result += " ";
            }
            return result;
        }

        public override void ReadXml(XmlReader reader)
        {
            SurveyNumList.Clear();
            ItemNumList.Clear();
            Action = (EAction)Enum.Parse(typeof(EAction), reader[0]);
            if (Action == EAction.delete)
            {
                reader.MoveToContent();
                return;
            }
            int nElems = Convert.ToInt32(reader[1]);
            reader.ReadStartElement();
            for (int ctr = 0; ctr < nElems; ctr++)
            {
                reader.ReadStartElement();
                SurveyNumList.Add(Convert.ToInt32(reader.ReadElementString()));
                ItemNumList.Add(Convert.ToInt32(reader.ReadElementString()));
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetName());
            writer.WriteAttributeString("Action", Action.ToString());
            if (Action == EAction.delete)
            {
                writer.WriteEndElement();
                return;
            }
            writer.WriteAttributeString("NumIDElements", SurveyNumList.Count.ToString());
            for (int ctr = 0; ctr < SurveyNumList.Count; ctr++)
            {
                writer.WriteStartElement("IDElement");
                writer.WriteElementString("SurveyNum", SurveyNumList[ctr].ToString());
                writer.WriteElementString("ItemNum", ItemNumList[ctr].ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();
        }
    }
}
