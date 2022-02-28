using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IATClient
{
    /// <summary>
    /// CIATReversedKey provides for a key that is the reverse of another key
    /// </summary>
    public class CIATReversedKey : CIATKey
    {
        private Uri BaseKeyUri = null;

        public CIATKey BaseKey
        {
            get
            {
                if (BaseKeyUri == null)
                    return null;
                return CIAT.SaveFile.GetIATKey(BaseKeyUri);
            }
            set
            {
                if ((BaseKeyUri == null) && (value == null))
                    return;
                else if (BaseKeyUri == null)
                {
                    BaseKeyUri = value.URI;
                    value.AddOwner(this);
                }
                else if (value == null)
                {
                    CIAT.SaveFile.GetIATKey(BaseKeyUri).ReleaseOwner(this);
                    BaseKeyUri = null;
                }
                else if (BaseKeyUri.Equals(value.URI))
                {
                    return;
                }
                else
                {
                    CIAT.SaveFile.GetIATKey(BaseKeyUri).ReleaseOwner(this);
                    BaseKeyUri = value.URI;
                    CIATKey key = CIAT.SaveFile.GetIATKey(BaseKeyUri);
                    key.AddOwner(this);
                    ValueChanged();
                    List<CIATDualKey> owningDks = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CIATKey), "owned-by")
                        .Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).Where(k => k.KeyType == IATKeyType.DualKey).Cast<CIATDualKey>().ToList();
                    foreach (CIATDualKey dk in owningDks)
                        dk.GenerateKeyValues();
                }
            }
        }

        public override DIBase LeftValue
        {
            get
            {
                if (BaseKeyUri == null)
                    return DIBase.DINull;
                return CIAT.SaveFile.GetIATKey(BaseKeyUri).RightValue;
            }
        }

        public override DIBase RightValue
        {
            get
            {
                if (BaseKeyUri == null)
                    return DIBase.DINull;
                return CIAT.SaveFile.GetIATKey(BaseKeyUri).LeftValue;
            }
        }

        public override Uri LeftValueUri
        {
            get
            {
                if (BaseKeyUri == null)
                    return DIBase.DINull.URI;
                return LeftValue.URI;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override Uri RightValueUri
        {
            get
            {
                if (BaseKeyUri == null)
                    return DIBase.DINull.URI;
                return RightValue.URI;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public CIATReversedKey()
        {
            BaseKeyUri = null;
        }

        public CIATReversedKey(Uri u) : base(u)
        {
        }

        public CIATReversedKey(String name, Uri baseKeyUri)
        {
            Name = name;
            BaseKey = CIAT.SaveFile.GetIATKey(baseKeyUri);
        }

        public override bool IsValid()
        {
            if ((BaseKeyUri == null) || (Name == String.Empty))
                return false;
            return true;
        }

        public override void Dispose()
        {
            if (BaseKeyUri != null)
            {
                try
                {
                    CIAT.SaveFile.GetIATKey(BaseKeyUri).ReleaseOwner(this);
                }
                catch (KeyNotFoundException ex) { }
                List<CIATKey> keys = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey), "owned-by").Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).ToList();
                foreach (CIATKey key in keys)
                {
                    CIAT.SaveFile.DeleteRelationship(this.URI, key.URI);
                    CIAT.SaveFile.DeleteRelationship(key.URI, this.URI);
                }
                foreach (Uri u in CIAT.SaveFile.GetPartsOfType(CIATBlock._MimeType))
                {
                    String rId;
                    CIATBlock b = CIAT.SaveFile.GetIATBlock(u);
                    if ((rId = CIAT.SaveFile.GetRelationship(b, this)) != null)
                        CIAT.SaveFile.DeleteRelationship(u, rId);
                }
                CIAT.SaveFile.DeletePart(this.URI);
            }
        }

        public override void Save()
        {
            if (this.URI == null)
                this.URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            XDocument xDoc = new XDocument();
            String rBaseKeyId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey), "owns").Select(pr => pr.Id).First();
            xDoc.Document.Add(new XElement("IATKey", new XAttribute("Type", KeyType.ToString()), new XElement("Name", Name), new XElement("BaseKeyRId", rBaseKeyId)));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public override void Load(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Name = xDoc.Root.Element("Name").Value;
            String rBaseKeyId = xDoc.Root.Element("BaseKeyRId").Value;
            BaseKeyUri = CIAT.SaveFile.GetRelationship(this, rBaseKeyId).TargetUri;
        }
    }
}
