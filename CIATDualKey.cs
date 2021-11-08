using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Linq;
using System.Xml;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IATClient
{
    public class CIATDualKey : CIATKey
    {
        private Uri _BaseKey1Uri = null, _BaseKey2Uri = null, _ConjunctionUri;
        private Uri _LeftValueUri = null, _RightValueUri = null;

        public Uri BaseKey1Uri
        {
            get
            {
                return _BaseKey1Uri;
            }
            set
            {
                if (_BaseKey1Uri != null)
                    CIAT.SaveFile.GetIATKey(_BaseKey1Uri).ReleaseOwner(this);
                _BaseKey1Uri = value;
                CIAT.SaveFile.GetIATKey(value).AddOwner(this);
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
                if (_BaseKey2Uri != null)
                    CIAT.SaveFile.GetIATKey(_BaseKey2Uri).ReleaseOwner(this);
                _BaseKey2Uri = value;
                CIAT.SaveFile.GetIATKey(value).AddOwner(this);
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
                    (CIAT.SaveFile.GetDI(_ConjunctionUri) as IResponseKeyDI).ReleaseKeyOwner(this);
                _ConjunctionUri = value;
                if (value == null)
                    return;
                (CIAT.SaveFile.GetDI(value) as IResponseKeyDI).AddKeyOwner(this);
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
            if (invalidate)
                GenerateKeyValues();
            LayoutSuspended = false;
        }

        public void GenerateKeyValues()
        {
            if ((BaseKey1Uri == null) || (BaseKey2Uri == null) || (ConjunctionUri == null))
                return;
            DIDualKey LeftValue = CIAT.SaveFile.GetDI(_LeftValueUri) as DIDualKey;
            DIDualKey RightValue = CIAT.SaveFile.GetDI(_RightValueUri) as DIDualKey;
            new CDualKeyLayout(LeftValue, RightValue)
            {
                Key1Uri = BaseKey1Uri,
                Key2Uri = BaseKey2Uri,
                ConjunctionUri = ConjunctionUri
            }.PerformLayout();
        }

        public CIATDualKey()
        {
            ValueChangedState = ValueChangedReady;
            DIDualKey rDK = new DIDualKey();
            rDK.AddKeyOwner(this);
            _RightValueUri = rDK.URI;
            DIDualKey lDK = new DIDualKey();
            lDK.AddKeyOwner(this);
            _LeftValueUri = lDK.URI;
        }

        public CIATDualKey(Uri uri) : base(uri)
        {
            ValueChangedState = ValueChangedReady;
        }

        public override Size GetDISize(Uri diUri)
        {
            if ((CIAT.SaveFile.GetDI(LeftValueUri) as DIDualKey)[diUri].Value != Rectangle.Empty)
                return (CIAT.SaveFile.GetDI(LeftValueUri) as DIDualKey)[diUri].Value.Size;
            if ((CIAT.SaveFile.GetDI(RightValueUri) as DIDualKey)[diUri].Value != Rectangle.Empty)
                return (CIAT.SaveFile.GetDI(RightValueUri) as DIDualKey)[diUri].Value.Size;
            return Size.Empty;
        }

        private readonly object ValueChangedReady = new object(), NoValueChangedWaiting = new object(), ValueChangedQueued = new object(), ValueChangedQueueReady = new object();
        private object ValueChangedState;
        private ManualResetEventSlim ValueChangedEvent = new ManualResetEventSlim(true);
        protected override void InvalidateValues()
        {
            new CDualKeyLayout(LeftValue as DIDualKey, RightValue as DIDualKey)
            {
                Key1Uri = BaseKey1Uri,
                Key2Uri = BaseKey2Uri,
                ConjunctionUri = ConjunctionUri
            }.PerformLayout();
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
                CIAT.SaveFile.GetDI(LeftValueUri).Dispose();
            if (RightValueUri != null) 
                CIAT.SaveFile.GetDI(RightValueUri).Dispose();
            try
            {
                CIAT.SaveFile.GetIATKey(BaseKey1Uri).ReleaseOwner(this);
            }
            catch (KeyNotFoundException) { }
            try
            {
                CIAT.SaveFile.GetIATKey(BaseKey2Uri).ReleaseOwner(this);
            }
            catch (KeyNotFoundException) { }
            List<Uri> uris = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey), "owned-by").Select(pr => pr.TargetUri).ToList();
            foreach (Uri u in uris)
                CIAT.SaveFile.GetIATKey(u).ReleaseOwner(this);
            foreach (Uri u in CIAT.SaveFile.GetPartsOfType(CIATBlock._MimeType))
            {
                String rId;
                CIATBlock b = CIAT.SaveFile.GetIATBlock(u);
                if ((rId = CIAT.SaveFile.GetRelationship(b, this)) != null)
                    CIAT.SaveFile.DeleteRelationship(u, rId);
            }
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
