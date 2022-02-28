using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

namespace IATClient
{
    public class CUniqueResponse : IPackagePart
    {
        public bool IsDisposed { get; private set; } = false;
        private Uri _SurveyUri = null, _SurveyItemUri = null;
        public List<String> Values { get; private set; } = new List<string>();
        public bool Additive { get; set; }
        public Uri URI { get; set; }
        public Type BaseType { get { return typeof(CUniqueResponse); } }
        public String MimeType { get { return "text/xml+" + typeof(CUniqueResponse).ToString(); } }

        public static IEnumerable<CResponse.EResponseType> UniqueResponseTypes
        {
            get
            {
                return new CResponse.EResponseType[] { CResponse.EResponseType.BoundedLength,
            CResponse.EResponseType.BoundedNum, CResponse.EResponseType.FixedDig, CResponse.EResponseType.RegEx };
            }
        }



        public void SetValues(List<String> values)
        {
            Values.Clear();
            Values.AddRange(values);
        }

        public Uri SurveyUri
        {
            get
            {
                return _SurveyUri;
            }
            set
            {
                if ((value == null) && (_SurveyUri == null))
                    return;
                else if ((value != null) && (_SurveyItemUri != null))
                    if (value.Equals(_SurveyUri))
                        return;
                if (_SurveyUri != null)
                    CIAT.SaveFile.DeleteRelationship(URI, _SurveyUri);
                if (value != null)
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(CSurvey), URI, value);
                _SurveyUri = value;
            }
        }
        public Uri SurveyItemUri
        {
            get
            {
                return _SurveyItemUri;
            }
            set
            {
                if ((value == null) && (_SurveyItemUri == null))
                    return;
                if ((value != null) && (_SurveyItemUri != null))
                    if (value.Equals(_SurveyItemUri))
                        return;
                if (_SurveyItemUri != null)
                    CIAT.SaveFile.DeleteRelationship(URI, _SurveyItemUri);
                if (value != null)
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(CSurveyItem), URI, value);
                _SurveyItemUri = value;
            }
        }

        public int ItemNum
        {
            get
            {
                if ((SurveyItemUri == null) || (SurveyUri == null))
                    return -1;
                CSurveyItem si = CIAT.SaveFile.GetSurveyItem(SurveyItemUri);
                CSurvey s = CIAT.SaveFile.GetSurvey(SurveyUri);
                return s.GetItemNum(si);
            }
        }

        public CUniqueResponse()
        {
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
        }

        public CUniqueResponse(Uri uri)
        {
            this.URI = uri;
            Load();
        }

        public void Clear()
        {
            SurveyUri = null;
            SurveyItemUri = null;
            Values.Clear();
        }

        public void Save()
        {
            String rSurveyId = String.Empty;
            if (SurveyUri != null)
                rSurveyId = CIAT.SaveFile.GetRelationship(this, SurveyUri);
            String rSurveyItemId = String.Empty;
            if (SurveyItemUri != null)
                rSurveyItemId = CIAT.SaveFile.GetRelationship(this, SurveyItemUri);
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("UniqueResponse", new XAttribute("rSurveyId", rSurveyId), new XAttribute("rSurveyItemId", rSurveyItemId), new XElement("Additive", Additive.ToString())));
            foreach (String str in Values)
                xDoc.Root.Add(new XElement("Response", str));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public void Load()
        {
            try
            {
                Stream s = CIAT.SaveFile.GetReadStream(this);
                XDocument xDoc = XDocument.Load(s);
                s.Dispose();
                CIAT.SaveFile.ReleaseReadStreamLock();
                XAttribute xSurvey = xDoc.Root.Attribute("rSurveyId");
                String rSurveyId = String.Empty;
                if (xSurvey != null)
                    rSurveyId = xSurvey.Value;
                if (rSurveyId != String.Empty)
                    _SurveyUri = CIAT.SaveFile.GetRelationship(this, rSurveyId).TargetUri;
                if (CVersion.Compare(CIAT.SaveFile.Version, new CVersion("1.1.0.14")) >= 0)
                {
                    int itemNum = Convert.ToInt32(xDoc.Root.Element("ItemNum").Value);
                    if (itemNum != -1)
                        SurveyItemUri = CIAT.SaveFile.GetSurvey(_SurveyUri).GetItemNumbered(itemNum).URI;
                }
                else
                {
                    XAttribute xItemNum = xDoc.Root.Attribute("rSurveyItemId");
                    if (xItemNum != null)
                        if (xItemNum.Value != String.Empty)
                            _SurveyItemUri = CIAT.SaveFile.GetRelationship(this, xItemNum.Value).TargetUri;
                }
                Additive = Convert.ToBoolean(xDoc.Root.Element("Additive").Value);
                foreach (XElement xElem in xDoc.Root.Elements("Response"))
                    Values.Add(xElem.Value);
            }
            catch (Exception ex)
            {
                CIAT.SaveFile.DeletePart(URI);
                throw ex;
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            if (SurveyUri != null)
                CIAT.SaveFile.DeleteRelationship(URI, SurveyUri);
            CIAT.SaveFile.DeleteRelationship(CIAT.SaveFile.IAT.URI, URI);
            if (SurveyItemUri != null)
                CIAT.SaveFile.DeleteRelationship(URI, SurveyItemUri);
            Values.Clear();
            CIAT.SaveFile.DeletePart(URI);
        }
    }
}
