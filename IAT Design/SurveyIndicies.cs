using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IATClient
{
    public class SurveyIndicies : IPackagePart
    {
        public Uri URI { get; set; }
        public Type BaseType { get { return typeof(SurveyIndicies); } }
        public String MimeType { get { return "text/xml+" + typeof(SurveyIndicies); } }
        private Dictionary<Uri, int> UtilizedIndicies = new Dictionary<Uri, int>();

        public int GetAvailableIndex(CSurvey s)
        {
            CIAT.SaveFile.CreateRelationship(s.BaseType, BaseType, s.URI, URI);
            if (UtilizedIndicies.Count == 0)
            {
                UtilizedIndicies[s.URI] = 1;
                return 1;
            }
            int freeVal = UtilizedIndicies.Values.Distinct().OrderBy(val => val).TakeWhile((val, ndx) => val - 1 == ndx).Count() + 1;
            UtilizedIndicies[s.URI] = freeVal;
            return freeVal;
        }
        public SurveyIndicies()
        {
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            Save();
        }
        public SurveyIndicies(Uri u)
        {
            URI = u;
            Load();
        }
        public void AddSurvey(Uri surveyUri, int index)
        {
            UtilizedIndicies[surveyUri] = index;
            CIAT.SaveFile.CreateRelationship(typeof(CSurvey), BaseType, surveyUri, URI);
        }
        public void RemoveSurvey(Uri surveyUri)
        {
            UtilizedIndicies.Remove(surveyUri);
            CIAT.SaveFile.DeleteRelationship(surveyUri, URI);
        }
        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("SurveyIndicies"));
            KeyValuePair<Uri, int>[] indicies = UtilizedIndicies.ToArray();
            foreach (var kv in UtilizedIndicies.AsQueryable().OrderBy((kv) => kv.Value))
                xDoc.Root.Add(new XElement("UtilizedIndex", new XElement("SurveyUri", kv.Key.ToString()), new XElement("Index", kv.Value.ToString())));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }
        public void Load()
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            UtilizedIndicies.Clear();
            foreach (XElement elem in xDoc.Root.Elements("UtilizedIndex"))
                UtilizedIndicies[new Uri(elem.Element("SurveyUri").Value, UriKind.Relative)] = Convert.ToInt32(elem.Element("Index").Value);
        }
        public void Dispose()
        {
            UtilizedIndicies.Clear();
            foreach (Uri u in UtilizedIndicies.Keys)
                CIAT.SaveFile.DeleteRelationship(u, URI);
            CIAT.SaveFile.DeleteRelationship(CIAT.SaveFile.IAT.URI, URI);
            CIAT.SaveFile.DeletePart(URI);
        }
    }
}
