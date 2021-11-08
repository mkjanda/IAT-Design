using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Forms;

namespace IATClient
{
    class DIStimulusImage : DIImage, IStimulus
    {
        private bool stretchToFit = false;
        private IImageDisplay _ThumbnailPreviewPanel = null;
        public IImageDisplay ThumbnailPreviewPanel
        {
            get
            {
                return _ThumbnailPreviewPanel;
            }
            set
            {
                if (_ThumbnailPreviewPanel == value)
                    return;
                _ThumbnailPreviewPanel = value;
                if (this.IImage == null)
                    return;
                this.IImage.Thumbnail.ClearChanged();
                this.IImage.Thumbnail.Changed += new Action<Images.ImageChangedEvent, Images.IImageMedia, object>(ThumbnailNotification);
                if (value != null)
                    ThumbnailPreviewPanel.SetImage(IImage.Thumbnail);
            }
        }

        private void ThumbnailNotification(Images.ImageChangedEvent evt, Images.IImageMedia img, object arg)
        {
            if (ThumbnailPreviewPanel == null)
                return;
            if (ThumbnailPreviewPanel.IsDisposed)
                IImage.Thumbnail.ClearChanged();
            else if (evt == Images.ImageChangedEvent.Updated)
                ThumbnailPreviewPanel.SetImage(img);
        }

        public bool StretchToFit
        {
            get
            {
                return stretchToFit;
            }
            set
            {
                if (value == stretchToFit)
                    return;
                stretchToFit = value;
                lock (lockObject)
                {
                    if (this.IImage != null)
                    {
                        if (value && (this.IImage.OriginalSize.Width < BoundingSize.Width) && (this.IImage.OriginalSize.Height < BoundingSize.Height))
                            Resize();
                        else if (!value && (this.IImage.OriginalSize.Width < BoundingSize.Width) && (this.IImage.OriginalSize.Height < BoundingSize.Height))
                            Resize();
                    }
                }
            }
        }


        public DIStimulusImage() : base()
        {
        }

        public DIStimulusImage(Uri uri) : base(uri)
        {
            ThumbnailPreviewPanel = null;
        }

        public DIStimulusImage(Images.IImage img) : base(img) 
        {
            this.IImage.CreateThumbnail();
            ThumbnailPreviewPanel = null;
        }

        public override void LoadImageFromFile(string path)
        {
            base.LoadImageFromFile(path);
            Resize();
            IImage.CreateThumbnail();
            IImage.Thumbnail.Changed += new Action<Images.ImageChangedEvent, Images.IImageMedia, object>(ThumbnailNotification);
        }

        public override void SetImage(Uri imgUri)
        {
            base.SetImage(imgUri);
            if (IImage.Thumbnail == null)
                IImage.CreateThumbnail();
            IImage.Thumbnail.Changed += new Action<Images.ImageChangedEvent, Images.IImageMedia, object>(ThumbnailNotification);
            if (ThumbnailPreviewPanel != null)
                ThumbnailPreviewPanel.SetImage(IImage.Thumbnail);
        }
/*
        public override bool Resize()
        {
            if (((this.IImage.Size.Width == BoundingSize.Width) && (this.IImage.Size.Height <= BoundingSize.Height)) ||
                ((this.IImage.Size.Width <= BoundingSize.Width) && (this.IImage.Size.Height == BoundingSize.Height))) {
                return false;
            }
            return base.Resize();
        }
*/
        protected override void OnImageChanged(Images.ImageChangedEvent evt, Images.IImageMedia imageMedia, object arg)
        {
            base.OnImageChanged(evt, imageMedia, arg);
            if ((evt == Images.ImageChangedEvent.Updated)|| (evt == Images.ImageChangedEvent.Initialized) || (evt == Images.ImageChangedEvent.ResizeUpdate))
            {
                if (IImage != null)
                {
                    if (IImage.Thumbnail == null)
                        IImage.CreateThumbnail();
                    else
                        CIAT.ImageManager.GenerateThumb(IImage);
                }
            }
        }

        public override object Clone()
        {
            DIStimulusImage o = new DIStimulusImage();
            o.stretchToFit = stretchToFit;
            o.Description = Description;
            o.SetImage(this.IImage.Image, this.IImage.Format);
            return o;
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            if (IImage != null)
            {
                String ImageRelId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, this.IImage.BaseType).First().Id;
                xDoc.Document.Add(new XElement(BaseType.ToString(), new XAttribute("rId", ImageRelId),
                    new XElement("Description", Description),
                    new XElement("StretchToFit", stretchToFit.ToString()), new XElement("Size", new XElement("Width", ItemSize.Width.ToString()),
                    new XElement("Height", ItemSize.Height.ToString())), new XElement("AbsoluteBounds", new XElement("X", AbsoluteBounds.X.ToString()),
                    new XElement("Y", AbsoluteBounds.Y.ToString()), new XElement("Width", AbsoluteBounds.Width.ToString()),
                    new XElement("Height", AbsoluteBounds.Height.ToString()))));
            }
            else
                xDoc.Document.Add(new XElement(BaseType.ToString(), new XElement("Description", Description), new XElement("StretchToFit", stretchToFit.ToString())));
            using (Stream s = CIAT.SaveFile.GetWriteStream(this))
                xDoc.Save(s);
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Close();
            CIAT.SaveFile.ReleaseReadStreamLock();
            if (xDoc.Root.Attribute("rId") != null)
            {
                String ImageRelId = xDoc.Root.Attribute("rId").Value;
                Uri imageUri = CIAT.SaveFile.GetRelationship(this, ImageRelId).TargetUri;
                Description = xDoc.Root.Element("Description").Value;
                StretchToFit = Convert.ToBoolean(xDoc.Root.Element("StretchToFit").Value);
                XElement szElem = xDoc.Root.Element("Size");
                ItemSize = new Size(Convert.ToInt32(szElem.Element("Width").Value), Convert.ToInt32(szElem.Element("Height").Value));
                SetImage(CIAT.SaveFile.GetRelationship(this, ImageRelId).TargetUri);
                if (CIAT.SaveFile.Version.CompareTo(new CVersion("1.1.1.22")) <= 0)
                    AbsoluteBounds = IImage.AbsoluteBounds;
                else
                {
                    XElement absSize = xDoc.Root.Element("AbsoluteBounds");
                    int x = Convert.ToInt32(absSize.Element("X").Value);
                    int y = Convert.ToInt32(absSize.Element("Y").Value);
                    int width = Convert.ToInt32(absSize.Element("Width").Value);
                    int height = Convert.ToInt32(absSize.Element("Height").Value);
                    AbsoluteBounds = new Rectangle(x, y, width, height);
                }
            }
            else
            {
                Description = xDoc.Root.Element("Description").Value;
                stretchToFit = Convert.ToBoolean(xDoc.Root.Element("StretchToFit").Value);
            }
        }

        protected override void Invalidate()
        {
            Resize();
        }

        public bool Equals(IStimulus stim)
        {
            if (Type != stim.Type)
                return false;
            if (Description == stim.Description)
                return true;
            return false;
        }
    }
}
