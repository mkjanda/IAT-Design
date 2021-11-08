using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Xml;

namespace IATClient
{
    class CSynchronizationPacket : INamedXmlSerializable 
    {
        private RSACryptoServiceProvider Crypt;
        private List<CSyncEvent> SyncEvents = new List<CSyncEvent>();

        public CSynchronizationPacket(RSAParameters rsaParams)
        {
            Crypt = new RSACryptoServiceProvider();
            Crypt.ImportParameters(rsaParams);
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            int nElems = Convert.ToInt32(reader["NumElements"]);
            reader.ReadStartElement();
            if (nElems > 0)
            {
                for (int ctr = 0; ctr < nElems; ctr++)
                {
                    CSyncWrapper wrapper = new CSyncWrapper(Crypt);
                    wrapper.ReadXml(reader);
                    SyncEvents.Add(wrapper.Event);
                }
            }
            if (reader.NodeType == XmlNodeType.EndElement)
                reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetName());
            writer.WriteAttributeString("NumElements", SyncEvents.Count.ToString());
            for (int ctr = 0; ctr < SyncEvents.Count; ctr++)
                SyncEvents[ctr].WriteXml(writer);
            writer.WriteEndElement();
        }

        public String GetName()
        {
            return "SynchronizationList";
        }

        public CNorms GetNorms()
        {
            for (int ctr = 0; ctr < SyncEvents.Count; ctr++)
                if (SyncEvents[ctr].GetEventType() == CSyncEvent.ESyncEvents.Norms)
                    return (CNorms)SyncEvents[ctr];
            return null;
        }

        public CSearchCriteriaContainer GetSearchCriteria(List<IATSurveyFile.Survey> BeforeSurveys, List<IATSurveyFile.Survey> AfterSurveys)
        {
            List<CSyncEvent> criteria = new List<CSyncEvent>();
            for (int ctr = 0; ctr < SyncEvents.Count; ctr++)
                if (SyncEvents[ctr].GetEventType() == CSyncEvent.ESyncEvents.SearchCriteria)
                    criteria.Add(SyncEvents[ctr]);
            CSearchCriteriaContainer scContainer = new CSearchCriteriaContainer(BeforeSurveys, AfterSurveys, criteria);
            return scContainer;
        }

        public List<CEnumeration> GetEnumerations()
        {
            List<CEnumeration> enumerations = new List<CEnumeration>();
            for (int ctr = 0; ctr < SyncEvents.Count; ctr++)
                if (SyncEvents[ctr].GetEventType() == CSyncEvent.ESyncEvents.Enumeration)
                    enumerations.Add((CEnumeration)SyncEvents[ctr]);
            return enumerations;
        }
    }
}
