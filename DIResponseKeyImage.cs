using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Drawing.Imaging;
using IATClient.Images;

namespace IATClient
{
    public class DIResponseKeyImage : DIImage, IResponseKeyDI
    {
        private readonly object PreviewPanelLock = new object();
        public Action ValidateData { get; set; } = null;
        public DIResponseKeyImage() : base()
        {
        }

        public DIResponseKeyImage(Uri uri) : base(uri) 
        {
            
        }

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
                    _PreviewPanel.SetImage(null as Bitmap);
                if (value == null)
                {
                    _PreviewPanel = value;
                    return;
                }
                _PreviewPanel = value;
                PreviewPanel.SetImage(this.IImage);
            }
        }

        protected override void OnImageChanged(ImageChangedEvent evt, IImageMedia img, object arg)
        {
            base.OnImageChanged(evt, img, arg);
            if (evt == Images.ImageChangedEvent.Resized)
            {
                AbsoluteBounds = (Rectangle)arg;
            }
            if ((evt == Images.ImageChangedEvent.Updated) || (evt == ImageChangedEvent.Initialized) || (evt == ImageChangedEvent.ResizeUpdate))
            {
                List<CIATKey> keys = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).ToList();
                foreach (CIATKey key in keys)
                    key.ValueChanged();
                if ((PreviewPanel != null) && !IsDisposed)
                    PreviewPanel.SetImage(img);
            }
            Validate();
        }

        private Size MinimalSize
        {
            get
            {
                Size minSize = CIAT.SaveFile.Layout.KeyValueSize;
                foreach (CIATDualKey key in CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Select(rel => CIAT.SaveFile.GetIATKey(rel.TargetUri)).Where(key => key.KeyType == IATKeyType.DualKey)) {
                    Nullable<Size> sz = key.GetDISize(this.URI);
                    if (sz == Size.Empty)
                        continue;
                    if (minSize.Width >= sz.Value.Width)
                        minSize.Width = sz.Value.Width;
                    if (minSize.Height >= sz.Value.Height)
                        minSize.Height = sz.Value.Height;
                }
                return minSize;
            }
        }

        public void Resize(Rectangle bounds)
        {
            if (!AbsoluteBounds.Equals(bounds.Size))
            {
                AbsoluteBounds = bounds;
                this.IImage.Resize(AbsoluteBounds.Size);
            }
        }

        protected override void Invalidate()
        {
            Resize();
        }

        public override bool Resize()
        {
            if (AbsoluteBounds.Size == MinimalSize)
                return false;
            return this.IImage.Resize(MinimalSize);
        }

        public List<Uri> KeyOwners
        {
            get
            {
                try
                {
                    return (CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Select(pr => pr.TargetUri)).ToList();
                }
                catch (Exception ex)
                {
                    return new List<Uri>();
                }
            }
        }

        public void ReleaseKeyOwner(IPackagePart pp)
        {
            Size oldSize = MinimalSize;
            CIAT.SaveFile.DeleteRelationship(this.URI, pp.URI);
            if (CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, pp.BaseType).Count() == 0)
            {
                this.Dispose();
                return;
            }
            Size newSize = MinimalSize;
            if (!newSize.Equals(oldSize) || (this.IImage.Size.Width > newSize.Width) || (this.IImage.Size.Height > newSize.Height))
                this.IImage.Resize(newSize);
        }

        public void ReleaseKeyOwners(List<IPackagePart> PPs)
        {
            foreach (var pp in PPs)
                ReleaseKeyOwner(pp);
        }

        public void SetKeyOwners(List<IPackagePart> PPs)
        {
            Size oldSize = MinimalSize;
            foreach (Uri uri in CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Select(rel => rel.TargetUri))
            {
                CIAT.SaveFile.DeleteRelationship(this.URI, uri);
                CIAT.SaveFile.DeleteRelationship(uri, URI);
            }
            foreach (var pp in PPs)
                AddKeyOwner(pp);
            Size newSize = MinimalSize;
            if (!newSize.Equals(oldSize) || (this.IImage.Size.Width > newSize.Width) || (this.IImage.Size.Height > newSize.Height))
                this.IImage.Resize(newSize);
        }


        public void AddKeyOwner(IPackagePart pp)
        {
            Size oldSize = MinimalSize;
            if (CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, pp.BaseType).Where(pr => pr.TargetUri.Equals(pp.URI)).Count() != 0)
                return;
            CIAT.SaveFile.CreateRelationship(BaseType, typeof(CIATKey), this.URI, pp.URI);
            CIAT.SaveFile.CreateRelationship(pp.BaseType, BaseType, pp.URI, URI);
            Size newSize = MinimalSize;
            if (!newSize.Equals(oldSize) || (this.IImage.Size.Width > newSize.Width) || (this.IImage.Size.Height > newSize.Height))
                this.IImage.Resize(newSize);
        }

        public override void SetImage(Image img, System.Drawing.Imaging.ImageFormat format)
        {
            bool noImage = this.IImage == null;
            base.SetImage(img, format);
            if (noImage)
            {
                if ((PreviewPanel != null) && !IsDisposed)
                {
                    PreviewPanel.SetImage(IImage);
                }
                ValidateData?.Invoke();
            }
            Resize();
        }

        public override void SetImage(Uri imgUri)
        {
            base.SetImage(imgUri);
            if ((PreviewPanel != null) && !IsDisposed)
            {
                PreviewPanel.SetImage(IImage);
            }
            ValidateData?.Invoke();
        }

        public override void Replace(DIBase target)
        {
            if ((target.Type == DIType.ResponseKeyText) || (target.Type == DIType.ResponseKeyImage))
                SetKeyOwners((target as IResponseKeyDI).KeyOwners.Select(u => CIAT.SaveFile.GetIATKey(u)).ToList<IPackagePart>());
            base.Replace(target);
        }

        public override object Clone()
        {
            DIResponseKeyImage o = new DIResponseKeyImage();
            o.IImage = IImage.Clone() as IImage;
            o.IImage.Changed += new Action<ImageChangedEvent, IImageMedia, object>(o.OnImageChanged);
            CIAT.SaveFile.CreateRelationship(o.BaseType, o.IImage.BaseType, o.URI, o.IImage.URI);
            o.Description = this.Description;
            o.ValidateData = this.ValidateData;
            o.SetKeyOwners(KeyOwners.Select(u => CIAT.SaveFile.GetIATKey(u)).ToList<IPackagePart>());
            return o;
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Description = xDoc.Root.Element("Description").Value;
            if (xDoc.Root.Attribute("rId") != null)
            {
                String ImageRelId = xDoc.Root.Attribute("rId").Value;
                XElement szElem = xDoc.Root.Element("Size");
                ItemSize = new Size(Convert.ToInt32(szElem.Element("Width").Value), Convert.ToInt32(szElem.Element("Height").Value));
                SetImage(CIAT.SaveFile.GetRelationship(this, ImageRelId).TargetUri);
                if (CIAT.SaveFile.Version.CompareTo(new CVersion("1.1.1.22")) <= 0)
                    AbsoluteBounds = IImage.AbsoluteBounds;
                else
                {
                    XElement absSize = xDoc.Root.Element("AbsoluteSize");
                    int x = Convert.ToInt32(absSize.Element("X").Value);
                    int y = Convert.ToInt32(absSize.Element("Y").Value);
                    int width = Convert.ToInt32(absSize.Element("Width").Value);
                    int height = Convert.ToInt32(absSize.Element("Height").Value);
                    AbsoluteBounds = new Rectangle(x, y, width, height);
                }
            }
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            if (IImage != null)
            {
                String ImageRelId = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, IImage.BaseType).First().Id;
                xDoc.Add(new XElement(BaseType.ToString()));
                xDoc.Root.Add(new XAttribute("rId", ImageRelId));
                xDoc.Root.Add(new XElement("Description", Description), new XElement("Size", new XElement("Width", ItemSize.Width.ToString()),
                new XElement("Height", ItemSize.Height.ToString())));
                xDoc.Root.Add(new XElement("AbsoluteBounds", new XElement("X", AbsoluteBounds.X.ToString()), new XElement("Y", AbsoluteBounds.Y.ToString()),
                    new XElement("Width", AbsoluteBounds.Width.ToString()), new XElement("Height", AbsoluteBounds.Height.ToString())));
            }
            else
                xDoc.Add(new XElement(BaseType.ToString(), new XElement("Description", Description)));
            using (Stream s = CIAT.SaveFile.GetWriteStream(this))
                xDoc.Save(s);
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            PreviewPanel = null;
            ReleaseKeyOwners(KeyOwners.Select(u => CIAT.SaveFile.GetIATKey(u)).ToList<IPackagePart>());
            base.Dispose();
            IsDisposed = true;
        }
    }
}
