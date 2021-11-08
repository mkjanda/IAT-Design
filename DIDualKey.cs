using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Xml.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace IATClient
{
    public class DIDualKey : DIComposite, IResponseKeyDI
    {
        public Action ValidateData { get; set; } = null;
        private bool GenerationPending { get; set; } = false;
        private ManualResetEventSlim GenerateEvent = new ManualResetEventSlim(true);
        public bool HaltGeneration { get; set; } = false;
        public readonly ConcurrentDictionary<Uri, Rectangle> DIDictionary = new ConcurrentDictionary<Uri, Rectangle>();
        private IImageDisplay _PreviewPanel = null;
        private bool LayoutSuspended = false;

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
                        _PreviewPanel.SetImage(null as Bitmap);
                }
                _PreviewPanel = value;
                ResumeLayout(true);
            }
        }

        private readonly object DIDictionaryLock = new object();
        public void LockImageDictionary()
        {
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
                    if (value == null)
                    {
                        if (DIDictionary.TryRemove(u, out r))
                        {
                            try
                            {
                                di.ReleaseOwner(URI);
                                foreach (var uri in KeyOwners.Where(ko => !(di as IResponseKeyDI).KeyOwners.Contains(ko)))
                                    (di as IResponseKeyDI).ReleaseKeyOwner(CIAT.SaveFile.GetIATKey(uri));
                            }
                            catch (KeyNotFoundException) { }
                        }
                        return;
                    }
                    if (!DIDictionary.TryGetValue(u, out r))
                    {
                        DIDictionary.TryAdd(u, value.Value);
                        try
                        {
                            di.AddOwner(URI);
                            foreach (var uri in KeyOwners.Where(ko => !(di as IResponseKeyDI).KeyOwners.Contains(ko)))
                                (di as IResponseKeyDI).AddKeyOwner(CIAT.SaveFile.GetIATKey(uri));
                        }
                        catch (KeyNotFoundException) { }
                    }
                    else
                        DIDictionary.TryUpdate(u, value.Value, r);
                }
            }
        }

        public override void ReleaseDI(Uri uri)
        {
            lock (DIDictionaryLock)
            {
                if (DIDictionary.TryRemove(uri, out Rectangle r))
                    CIAT.SaveFile.GetDI(uri).ReleaseOwner(URI);
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

        public void SetKeyOwners(List<IPackagePart> PPs)
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

        public void ReleaseKeyOwners(List<IPackagePart> PPs)
        {
            foreach (var pp in PPs)
                ReleaseKeyOwner(pp);
        }

        public List<Uri> KeyOwners
        {
            get
            {
                return CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Select(pr => pr.TargetUri).ToList();
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

        public override bool ComponentsValid
        {
            get
            {
                    List<DIBase> dis = DIDictionary.Keys.Select(k => CIAT.SaveFile.GetDI(k)).ToList();
                    foreach (DIBase di in dis)
                        if (!di.IsValid)
                            return false;
                    return true;
            }
        }

        private readonly object genLock = new object();
        protected override bool Generate()
        {
            lock (genLock)
            {
                if (PreviewPanel != null)
                {
                    if (!PreviewPanel.IsHandleCreated)
                    {
                        PreviewPanel.HandleCreated += (sender, args) => ScheduleInvalidation();
                        return true;
                    }
                }
                Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.ResponseKey);
                Graphics g = Graphics.FromImage(bmp);
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                Brush backBR = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                g.FillRectangle(backBR, new Rectangle(new Point(0, 0), BoundingSize));
                backBR.Dispose();
                Dictionary<Uri, Rectangle> DIRects = new Dictionary<Uri, Rectangle>();
                lock (DIDictionaryLock)
                {
                    foreach (var u in DIDictionary.Keys)
                        DIRects[u] = DIDictionary[u];
                }
                foreach (Uri u in DIRects.Keys)
                {
                    try
                    {
                        Rectangle r = DIRects[u];
                        Rectangle srcRect;
                        DIBase di = CIAT.SaveFile.GetDI(u);
                        if (di == null)
                            continue;
                        if (di.IImage == null)
                            continue;
                        Image img = di.IImage.Image;
                        if (img == null)
                            continue;
//                        if (di.Type == DIType.ResponseKeyImage)
  //                          srcRect = new Rectangle((img.Width - di.IImage.Size.Width) >> 1, (img.Height - di.IImage.Size.Height) >> 1, di.IImage.Size.Width,
    //                            di.IImage.Size.Height);
      //                  else
                            srcRect = di.AbsoluteBounds;
//                        if ((srcRect.Width < r.Width) || (srcRect.Height < r.Height))
  //                          r = new Rectangle(new Point(r.Left + (r.Width - srcRect.Width >> 1), r.Top), srcRect.Size);
                        g.DrawImage(img, r, srcRect, GraphicsUnit.Pixel);
                        img.Dispose();
                    }
                    catch (KeyNotFoundException) { }
                }
                g.Dispose();
                CalcAbsoluteBounds(bmp);
                bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                if (IImage == null)
                    SetImage(bmp, System.Drawing.Imaging.ImageFormat.Png);
                else
                    IImage.Image = bmp;
                return true;
            }
        }

        protected override void OnImageChanged(Images.ImageChangedEvent evt, Images.IImageMedia img, object arg)
        {
            base.OnImageChanged(evt, img, arg);
            if (PreviewPanel != null)
                PreviewPanel.SetImage(img);
            if ((evt == Images.ImageChangedEvent.Updated) || (evt == Images.ImageChangedEvent.Initialized))
                ValidateData?.Invoke();
            Validate();
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
            List<Uri> uris = DIDictionary.Keys.ToList();
            foreach (Uri u in uris)
            {
                DIBase diBase = CIAT.SaveFile.GetDI(u);
                if (diBase.Type == DIType.Conjunction)
                {
                    DIConjunction dic = CIAT.SaveFile.GetDI(u).Clone() as DIConjunction;
                    if (DIDictionary.TryGetValue(u, out Rectangle conjunctionRect))
                        if (di.DIDictionary.TryAdd(u, conjunctionRect))
                            CIAT.SaveFile.GetDI(u).AddOwner(u);
                }
                else
                {
                    if (DIDictionary.TryGetValue(u, out Rectangle elementRect))
                        if (di.DIDictionary.TryAdd(u, elementRect))
                            CIAT.SaveFile.GetDI(u).AddOwner(u);
                }
            }
            di.ScheduleInvalidation();
            return di;
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            XElement root = new XElement("DualKeyDI");
            if (IImage != null)
            {
                PackageRelationship ImageRel = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, this.IImage.BaseType).FirstOrDefault();
                if (ImageRel != null)
                    root.Add(new XAttribute("rId", ImageRel.Id));
            }
            foreach (Uri u in DIDictionary.Keys)
            {
                String rId = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(DIBase), "owns").Where(pr => pr.TargetUri.Equals(u)).Select(pr => pr.Id).First();
                Rectangle rect = DIDictionary[u];
                root.Add(new XElement("Component", new XAttribute("rId", rId), new XElement("Top", rect.Top.ToString()), new XElement("Left", rect.Left.ToString()),
                    new XElement("Width", rect.Width.ToString()), new XElement("Height", rect.Height.ToString())));
            }
            root.Add(new XElement("Size", new XElement("Width", ItemSize.Width.ToString()), new XElement("Height", ItemSize.Height.ToString())));
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
            if (xDoc.Root.Attribute("rId") != null)
            {
                String ImageRelId = xDoc.Root.Attribute("rId").Value;
                base.SetImage(CIAT.SaveFile.GetRelationship(this, ImageRelId).TargetUri);
            }
            foreach (XElement keyElem in xDoc.Root.Elements("Component"))
            {
                Uri componentUri = CIAT.SaveFile.GetRelationship(this, keyElem.Attribute("rId").Value).TargetUri;
                DIDictionary[componentUri] = new Rectangle(Convert.ToInt32(keyElem.Element("Left").Value), Convert.ToInt32(keyElem.Element("Top").Value),
                    Convert.ToInt32(keyElem.Element("Width").Value), Convert.ToInt32(keyElem.Element("Height").Value));
            }
            XElement szElem = xDoc.Root.Element("Size");
            ItemSize = new Size(Convert.ToInt32(szElem.Element("Width").Value), Convert.ToInt32(szElem.Element("Height").Value));
            AbsoluteBounds = new Rectangle(Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Left").Value),
                Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Top").Value), Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Width").Value),
                Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Height").Value));
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            ReleaseKeyOwners(KeyOwners.Select(u => CIAT.SaveFile.GetIATKey(u)).ToList<IPackagePart>());
            lock (lockObject)
            {
                List<DIBase> components = CIAT.SaveFile.GetRelationshipsByType(this.URI, this.BaseType, typeof(DIBase), "owns").Select(pr => CIAT.SaveFile.GetDI(pr.TargetUri)).Where(di => di != null).ToList();
                foreach (DIBase di in components)
                {
                    try
                    {
                        di.ReleaseOwner(URI);
                        di.ScheduleInvalidation();
                    }
                    catch (Exception) { }
                }
                base.Dispose();
            }
        }
    }
}
