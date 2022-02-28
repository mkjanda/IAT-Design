using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace IATClient
{
    public class DIDualKey : DIComposite, IResponseKeyDI
    {
        private bool _InvalidationSuspended = false;
        public void SuspendInvalidations()
        {
            _InvalidationSuspended = true;
        }
        public bool ResumeInvalidations(bool val)
        {
            bool oldVal = _InvalidationSuspended;
            _InvalidationSuspended = false;
            if (val == true)
                ScheduleInvalidation();
            return oldVal;
        }

        public Action ValidateData { get; set; } = null;
        private bool GenerationPending { get; set; } = false;
        private ManualResetEventSlim GenerateEvent = new ManualResetEventSlim(true);
        public bool HaltGeneration { get; set; } = false;
        public readonly ConcurrentDictionary<Uri, Rectangle> DIDictionary = new ConcurrentDictionary<Uri, Rectangle>();
        private IImageDisplay _PreviewPanel = null;

        public override IImageDisplay PreviewPanel
        {
            get
            {
                return _PreviewPanel;
            }
            set
            {
                if (value == _PreviewPanel)
                    return;
                if (_PreviewPanel != null)
                {
                    if (_PreviewPanel.IsHandleCreated)
                        _PreviewPanel.ClearImage();
                }
                _PreviewPanel = value;
                ResumeLayout(true);
            }
        }

        private readonly object DIDictionaryLock = new object();
        public void LockImageDictionary()
        {
            SuspendLayout();
            Monitor.Enter(DIDictionaryLock);
        }
        public void UnlockImageDictionary()
        {
            Monitor.Exit(DIDictionaryLock);
        }
        public Nullable<Rectangle> this[Uri u]
        {
            get
            {
                if (DIDictionary.TryGetValue(u, out Rectangle rect))
                    return rect;
                return Rectangle.Empty;
            }
            set
            {
                lock (DIDictionaryLock)
                {
                    DIBase di = CIAT.SaveFile.GetDI(u);
                    Rectangle r;
                    if ((value == null) || (value.Value == Rectangle.Empty))
                    {
                        if (DIDictionary.TryRemove(u, out r))
                            di.ReleaseOwner(URI);
                        return;
                    }
                    if (!DIDictionary.TryGetValue(u, out r))
                    {
                        DIDictionary.TryAdd(u, value.Value);
                        di.AddOwner(URI);
                    }
                    else if (DIDictionary.TryRemove(u, out r))
                        DIDictionary.TryAdd(u, value.Value);
                    ScheduleInvalidation();
                }
            }
        }

        public void ClearComponents()
        {
            lock (DIDictionaryLock)
            {
                DIDictionary.Clear();
            }
        }

        public DIDualKey()
        {
            PreviewPanel = null;
            ValidateData = null;
        }

        public DIDualKey(Uri uri)
            : base(uri)
        {
            PreviewPanel = null;
            ValidateData = null;
            SuspendLayout();
        }

        public bool ContainsDI(Uri diUri)
        {
            return DIDictionary.TryGetValue(diUri, out Rectangle r);
        }

        public void AddKeyOwner(IPackagePart pp)
        {
            if (CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Where(pr => pr.TargetUri.Equals(pp.URI)).Count() != 0)
                return;
            CIAT.SaveFile.CreateRelationship(BaseType, pp.BaseType, URI, pp.URI);
            CIAT.SaveFile.CreateRelationship(pp.BaseType, BaseType, pp.URI, URI);
        }

        public void SetKeyOwners(List<CIATKey> PPs)
        {
            var owners = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CIATKey)).Select(pr => pr.TargetUri).ToList();
            foreach (var u in owners)
            {
                CIAT.SaveFile.DeleteRelationship(this.URI, u);
                CIAT.SaveFile.DeleteRelationship(u, URI);
            }
            foreach (var newOwner in PPs)
                AddKeyOwner(newOwner);
            if (PPs.Count == 0)
                Dispose();
        }

        public void ReleaseKeyOwner(IPackagePart pp)
        {
            CIAT.SaveFile.DeleteRelationship(this.URI, pp.URI);
            CIAT.SaveFile.DeleteRelationship(pp.URI, URI);
            if (CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Count() == 0)
                this.Dispose();
        }

        public void ReleaseKeyOwners(List<CIATKey> PPs)
        {
            foreach (var pp in PPs)
                ReleaseKeyOwner(pp);
        }

        public List<CIATKey> KeyOwners
        {
            get
            {
                return CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).ToList();
            }
        }

        public override bool ComponentsExist
        {
            get
            {
                try
                {
                    List<Uri> uris = DIDictionary.Keys.ToList();
                    foreach (Uri u in uris)
                        CIAT.SaveFile.GetDI(u);
                }
                catch (KeyNotFoundException)
                {
                    return false;
                }
                return true;
            }
        }

        public override bool ComponentsStale
        {
            get
            {
                List<DIBase> dis = DIDictionary.Keys.Select(k => CIAT.SaveFile.GetDI(k)).ToList();
                foreach (DIBase di in dis)
                    if (di.ComponentStale)
                        return true;
                return false;
            }
        }

        public List<Uri> GetComponentUris()
        {
            lock (DIDictionaryLock)
            {
                return DIDictionary.Keys.ToList();
            }
        }

        protected override Bitmap Generate()
        {
            SuspendLayout();
            if (PreviewPanel != null)
            {
                if (!PreviewPanel.IsHandleCreated)
                    PreviewPanel.HandleCreated += (sender, args) => ScheduleInvalidation();
            }
            if (!IImage.Size.Equals(CIAT.SaveFile.Layout.KeyValueSize))
                SuspendLayout();
            Dictionary<Uri, Rectangle> DIRects = new Dictionary<Uri, Rectangle>();
            var componentDis = DIDictionary.Keys.Select(u => CIAT.SaveFile.GetDI(u) as IResponseKeyDI);
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.ResponseKey);
            Graphics g = Graphics.FromImage(bmp);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            Brush backBR = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            g.FillRectangle(backBR, new Rectangle(new Point(0, 0), BoundingSize));
            backBR.Dispose();
            foreach (Uri u in DIDictionary.Keys)
            {
                if (!DIDictionary.TryGetValue(u, out Rectangle r))
                    continue;
                Rectangle srcRect;
                var di = CIAT.SaveFile.GetDI(u);
                Image img = di.IImage.Img;
                if (img == null)
                {
                    di.ResumeLayout(true);
                    ScheduleInvalidationSync();
                }
                srcRect = (di as DIBase).AbsoluteBounds;
                g.DrawImage(img, r, srcRect, GraphicsUnit.Pixel);
                img.Dispose();
            }
            g.Dispose();
            CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
            bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
            return bmp;
        }

        public override void ResumeLayout(bool invalidate)
        {
            foreach (var uri in DIDictionary.Keys)
            {
                if (DIDictionary.TryGetValue(uri, out Rectangle rect))
                    CIAT.SaveFile.GetDI(uri).ResumeLayout(invalidate);
            }
            base.ResumeLayout(invalidate);
        }

        protected override void OnImageEvent(Images.ImageEvent evt, Images.IImageMedia img, object arg)
        {
            base.OnImageEvent(evt, img, arg);
            if ((evt == Images.ImageEvent.Updated) || (evt == Images.ImageEvent.Initialized))
            {
                ValidateData?.Invoke();
                if (PreviewPanel != null)
                    PreviewPanel.SetImage(img);
            }
        }

        protected override void Validate()
        {
            base.Validate();
            if (HaltGeneration)
                StopGenerating();
        }


        public override object Clone()
        {
            DIDualKey di = new DIDualKey()
            {
                PreviewPanel = this.PreviewPanel,
                ValidateData = this.ValidateData
            };
            di.IImage = IImage.Clone() as Images.IImage;
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, di.IImage.URI);
            di.IImage.Changed += new Action<Images.ImageEvent, Images.IImageMedia, object>(di.OnImageEvent);
            List<Uri> uris = DIDictionary.Keys.ToList();
            foreach (Uri u in uris)
            {
                DIBase diBase = CIAT.SaveFile.GetDI(u);
                if (diBase.Type == DIType.Conjunction)
                {
                    DIConjunction dic = CIAT.SaveFile.GetDI(u).Clone() as DIConjunction;
                    if (DIDictionary.TryGetValue(u, out Rectangle conjunctionRect))
                        if (di.DIDictionary.TryAdd(u, conjunctionRect))
                            CIAT.SaveFile.GetDI(u).AddOwner(di.URI);
                }
                else
                {
                    if (DIDictionary.TryGetValue(u, out Rectangle elementRect))
                        if (di.DIDictionary.TryAdd(u, elementRect))
                            CIAT.SaveFile.GetDI(u).AddOwner(di.URI);
                }
            }
            return di;
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            XElement root = new XElement("DualKeyDI");
            if (IImage != null)
                root.Add(new XAttribute("rImageId", rImageId));
            root.Add(new XAttribute("InvalidationState", InvalidationState.ToString()));
            foreach (Uri u in DIDictionary.Keys)
            {

                String rId = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(DIBase), "owns").Where(pr => pr.TargetUri.Equals(u)).Select(pr => pr.Id).First();
                Rectangle rect = DIDictionary[u];
                root.Add(new XElement("Component", new XAttribute("rId", rId), new XElement("Top", rect.Top.ToString()), new XElement("Left", rect.Left.ToString()),
                    new XElement("Width", rect.Width.ToString()), new XElement("Height", rect.Height.ToString())));
            }
            root.Add(new XElement("AbsoluteBounds", new XElement("Top", AbsoluteBounds.Top.ToString()),
                new XElement("Left", AbsoluteBounds.Left.ToString()), new XElement("Width", AbsoluteBounds.Width.ToString()),
                new XElement("Height", AbsoluteBounds.Height.ToString())));
            xDoc.Document.Add(root);
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            if (xDoc.Root.Attribute("rImageId") != null)
            {
                rImageId = xDoc.Root.Attribute("rImageId").Value;
                SetImage(rImageId);
            }
            if (xDoc.Root.Attribute("InvalidationState") != null)
            {
                InvalidationState = InvalidationStates.Parse(xDoc.Root.Attribute("InvalidationState").Value);
            }
            else
            {
                InvalidationState = InvalidationStates.InvalidationReady;
            }
            foreach (XElement keyElem in xDoc.Root.Elements("Component"))
            {
                Uri componentUri = CIAT.SaveFile.GetRelationship(this, keyElem.Attribute("rId").Value).TargetUri;
                DIDictionary[componentUri] = new Rectangle(Convert.ToInt32(keyElem.Element("Left").Value), Convert.ToInt32(keyElem.Element("Top").Value),
                    Convert.ToInt32(keyElem.Element("Width").Value), Convert.ToInt32(keyElem.Element("Height").Value));
            }
            XElement szElem = xDoc.Root.Element("Size");
            AbsoluteBounds = new Rectangle(Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Left").Value),
                Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Top").Value), Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Width").Value),
                Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Height").Value));
            if (xDoc.Root.Attribute("rId") == null)
                ScheduleInvalidation();
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            ReleaseKeyOwners(KeyOwners);
            lock (lockObject)
            {
                var components = DIDictionary.Keys;
                foreach (DIBase di in components.Select((c) => CIAT.SaveFile.GetDI(c)).ToList())
                    di?.ReleaseOwner(URI);
                base.Dispose();
            }
        }
    }
}
