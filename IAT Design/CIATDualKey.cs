using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IATClient
{
    public class CIATDualKey : CIATKey
    {
        private Uri _BaseKey1Uri = null, _BaseKey2Uri = null, _ConjunctionUri;
        private Uri _LeftValueUri = null, _RightValueUri = null;
        private CDualKeyLayout Layout = null;
        public Uri BaseKey1Uri
        {
            get
            {
                return _BaseKey1Uri;
            }
            set
            {
                CIATKey k;
                if (_BaseKey1Uri != null)
                {
                    k = CIAT.SaveFile.GetIATKey(_BaseKey1Uri);
                    k.ReleaseOwner(this);
                    if (k.LeftValue != null)
                    {
                        (k.LeftValue as IResponseKeyDI).ReleaseKeyOwner(this);
                        k.LeftValue.ResumeLayout(false);
                    }
                    if (k.RightValue != null)
                    {
                        (k.RightValue as IResponseKeyDI).ReleaseKeyOwner(this);
                        k.RightValue.ResumeLayout(false);
                    }
                }
                _BaseKey1Uri = value;
                if (value == null)
                    return;
                k = CIAT.SaveFile.GetIATKey(value);
                k.AddOwner(this);
                k.LeftValue.SuspendLayout();
                k.RightValue.SuspendLayout();
                (k.LeftValue as IResponseKeyDI).AddKeyOwner(this);
                (k.RightValue as IResponseKeyDI).AddKeyOwner(this);
                if (!LayoutSuspended)
                    GenerateKeyValues();
            }
        }
        public Uri BaseKey2Uri
        {
            get
            {
                return _BaseKey2Uri;
            }
            set
            {
                CIATKey k;
                if (_BaseKey2Uri != null)
                {
                    k = CIAT.SaveFile.GetIATKey(_BaseKey1Uri);
                    k.ReleaseOwner(this);
                    if (k.LeftValue != null)
                    {
                        (k.LeftValue as IResponseKeyDI).ReleaseKeyOwner(this);
                        k.LeftValue.ResumeLayout(false);
                    }
                    if (k.RightValue != null)
                    {
                        (k.RightValue as IResponseKeyDI).ReleaseKeyOwner(this);
                        k.RightValue.ResumeLayout(false);
                    }
                }
                _BaseKey2Uri = value;
                if (value == null)
                    return;
                k = CIAT.SaveFile.GetIATKey(value);
                k.AddOwner(this);
                k.LeftValue.SuspendLayout();
                k.RightValue.SuspendLayout();
                (k.LeftValue as IResponseKeyDI).AddKeyOwner(this);
                (k.RightValue as IResponseKeyDI).AddKeyOwner(this);
                if (!LayoutSuspended)
                    GenerateKeyValues();
            }
        }

        public Uri ConjunctionUri
        {
            get
            {
                return _ConjunctionUri;
            }
            set
            {
                if (_ConjunctionUri != null)
                {
                    (CIAT.SaveFile.GetDI(_ConjunctionUri) as IResponseKeyDI).ReleaseKeyOwner(this);
                    CIAT.SaveFile.GetDI(_ConjunctionUri).ResumeLayout(false);
                }
                if (value == null)
                    return;
                _ConjunctionUri = value;
                CIAT.SaveFile.GetDI(_ConjunctionUri).SuspendLayout();
                (CIAT.SaveFile.GetDI(_ConjunctionUri) as IResponseKeyDI).AddKeyOwner(this);
                if (!LayoutSuspended)
                    GenerateKeyValues();
            }
        }

        public override Uri LeftValueUri
        {
            get
            {
                return _LeftValueUri;
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
                return _RightValueUri;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private bool LayoutSuspended { get; set; } = false;
        public void SuspendLayout()
        {
            LayoutSuspended = true;
        }
        public void ResumeLayout(bool invalidate)
        {
            LayoutSuspended = false;
            if (invalidate)
                GenerateKeyValues();
        }

        public void GenerateKeyValues()
        {
            if (LayoutSuspended)
                return;
            if ((BaseKey1Uri == null) || (BaseKey2Uri == null) || (ConjunctionUri == null))
                return;
            Layout = new CDualKeyLayout(LeftValue as DIDualKey, RightValue as DIDualKey);
            SuspendLayout();
            Layout.Key1Uri = BaseKey1Uri;
            Layout.Key2Uri = BaseKey2Uri;
            Layout.ConjunctionUri = ConjunctionUri;
            Layout.PerformLayout();
        }

        public CIATDualKey()
        {
            DIDualKey rDK = new DIDualKey();
            rDK.AddKeyOwner(this);
            _RightValueUri = rDK.URI;
            DIDualKey lDK = new DIDualKey();
            lDK.AddKeyOwner(this);
            _LeftValueUri = lDK.URI;
        }

        public CIATDualKey(Uri uri) : base(uri)
        {
        }

        public override Size GetDISize(DIBase di)
        {
            if (di.Type != DIType.ResponseKeyImage)
                return base.GetDISize(di);
            var lrels = CIAT.SaveFile.GetRelationshipsByType(LeftValueUri, typeof(DIBase), typeof(DIBase), "owns").ToList();
            if (lrels.Select(pr => pr.TargetUri).Contains(di.URI))
                return (LeftValue as DIDualKey)[di.URI].Value.Size;
            var rrels = CIAT.SaveFile.GetRelationshipsByType(RightValueUri, typeof(DIBase), typeof(DIBase), "owns").ToList();
            if (rrels.Select(pr => pr.TargetUri).Contains(di.URI))
                return (RightValue as DIDualKey)[di.URI].Value.Size;
            return base.GetDISize(di);
        }

        private static object waiting = new object(), running = new object(), queued = new object();

        protected override void InvalidateValues()
        {
            ResumeLayout(false);
            GenerateKeyValues();
        }
        public override void Dispose()
        {
            if (ConjunctionUri != null)
                (CIAT.SaveFile.GetDI(ConjunctionUri) as DIConjunction).ReleaseKeyOwner(this);
            if (BaseKey1Uri != null)
                CIAT.SaveFile.GetIATKey(BaseKey1Uri).ReleaseOwner(this);
            if (BaseKey2Uri != null)
                CIAT.SaveFile.GetIATKey(BaseKey2Uri).ReleaseOwner(this);
            if (LeftValueUri != null)
                (CIAT.SaveFile.GetDI(LeftValueUri) as IResponseKeyDI).ReleaseKeyOwner(this);
            if (RightValueUri != null)
                (CIAT.SaveFile.GetDI(RightValueUri) as IResponseKeyDI).ReleaseKeyOwner(this);
            foreach (var k in KeyOwners)
                ReleaseOwner(k);
            foreach (var pr in CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CIATBlock)).ToList())
                CIAT.SaveFile.DeleteRelationship(URI, pr.Id);
            CIAT.SaveFile.DeletePart(this.URI);
        }

        public override bool IsValid()
        {
            if ((BaseKey1Uri != null) && (BaseKey2Uri != null) && (ConjunctionUri != null))
                return true;
            return false;
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            String rDILeftKeyValue = CIAT.SaveFile.GetRelationship(this, LeftValueUri);
            String rDIRightKeyValue = CIAT.SaveFile.GetRelationship(this, RightValueUri);
            String rBaseKey1Id = CIAT.SaveFile.GetRelationship(this, BaseKey1Uri);
            String rBaseKey2Id = CIAT.SaveFile.GetRelationship(this, BaseKey2Uri);
            String rConjunctionId = CIAT.SaveFile.GetRelationship(this, ConjunctionUri);
            xDoc.Add(new XElement("IATKey", new XAttribute("Type", KeyType.ToString()), new XElement("Name", Name), new XElement("rBaseKey1", rBaseKey1Id), new XElement("rBaseKey2", rBaseKey2Id),
                new XElement("rConjunction", rConjunctionId), new XElement("rLeftValue", rDILeftKeyValue), new XElement("rRightValue", rDIRightKeyValue)));
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
            _LeftValueUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rLeftValue").Value).TargetUri;
            _RightValueUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rRightValue").Value).TargetUri;
            _BaseKey1Uri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rBaseKey1").Value).TargetUri;
            _BaseKey2Uri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rBaseKey2").Value).TargetUri;
            _ConjunctionUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rConjunction").Value).TargetUri;
        }
    }
}
