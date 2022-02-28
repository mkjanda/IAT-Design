using System;
using System.Drawing;
using System.IO;
using System.Xml.Linq;

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
                if (this.IImage.Thumbnail == null)
                    this.IImage.CreateThumbnail();
                else
                    this.IImage.Thumbnail.ClearChanged();
                this.IImage.Thumbnail.Changed += new Action<Images.ImageEvent, Images.IImageMedia, object>(ThumbnailNotification);
                if (value != null)
                    ThumbnailPreviewPanel.SetImage(IImage.Thumbnail);
            }
        }

        private void ThumbnailNotification(Images.ImageEvent evt, Images.IImageMedia img, object arg)
        {
            if (ThumbnailPreviewPanel == null)
                return;
            if (ThumbnailPreviewPanel.IsDisposed)
                IImage.Thumbnail.ClearChanged();
            else if (evt == Images.ImageEvent.Updated)
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
        }

        public DIStimulusImage(Images.IImage img) : base(img)
        {
            this.IImage.CreateThumbnail();
        }

        public override void LoadImageFromFile(string path)
        {
            base.LoadImageFromFile(path);
            Resize();
        }

        public override void SetImage(String rId)
        {
            base.SetImage(rId);
            if (IImage.Thumbnail == null)
                IImage.CreateThumbnail();
            IImage.Thumbnail.Changed += new Action<Images.ImageEvent, Images.IImageMedia, object>(ThumbnailNotification);
            if (ThumbnailPreviewPanel != null)
                ThumbnailPreviewPanel.SetImage(IImage.Thumbnail);
        }

        protected override void OnImageEvent(Images.ImageEvent evt, Images.IImageMedia imageMedia, object arg)
        {
            base.OnImageEvent(evt, imageMedia, arg);
            if ((evt == Images.ImageEvent.Resized) || (evt == Images.ImageEvent.ResizeNotNeeded))
                AbsoluteBounds = (Rectangle)arg;
            if (evt == Images.ImageEvent.ResizeNotNeeded)
                Validate();
            if ((evt == Images.ImageEvent.Updated) || (evt == Images.ImageEvent.Initialized))
            {
                if (IImage != null)
                {
                    if (IImage.Thumbnail == null)
                    {
                        IImage.CreateThumbnail();
                        IImage.Thumbnail.Changed += (evt, img, obj) => ThumbnailNotification(evt, img, obj);
                    }
                    CIAT.ImageManager.GenerateThumb(IImage);
                }
            }
        }

        public override object Clone()
        {
            DIStimulusImage o = new DIStimulusImage();
            o.stretchToFit = stretchToFit;
            o.Description = Description;
            o.IImage = IImage.Clone() as Images.IImage;
            o.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument.URI, o.IImage.URI);
            o.IImage.Changed += (evt, img, args) => o.OnImageEvent(evt, img, args);
            return o;
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            if (IImage != null)
            {
                xDoc.Document.Add(new XElement(BaseType.ToString(), new XAttribute("rImageId", rImageId), new XAttribute("InvaldationState", InvalidationState.ToString()),
                    new XElement("Description", Description),
                    new XElement("StretchToFit", stretchToFit.ToString()), new XElement("AbsoluteBounds", new XElement("X", AbsoluteBounds.X.ToString()),
                    new XElement("Y", AbsoluteBounds.Y.ToString()), new XElement("Width", AbsoluteBounds.Width.ToString()),
                    new XElement("Height", AbsoluteBounds.Height.ToString()))));
            }
            else
                xDoc.Document.Add(new XElement(BaseType.ToString(), new XElement("Description", Description), new XElement("StretchToFit", stretchToFit.ToString())));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = Stream.Synchronized(CIAT.SaveFile.GetReadStream(this));
            XDocument xDoc;
            try
            {
                xDoc = XDocument.Load(s);
            }
            finally
            {
                s.Dispose();
                CIAT.SaveFile.ReleaseReadStreamLock();
            }
            if (xDoc.Root.Attribute("rImageId") != null)
            {
                String rImageId = xDoc.Root.Attribute("rImageId").Value;
                var iStateAttr = xDoc.Root.Attribute("InvalidationState");
                if (iStateAttr != null)
                    InvalidationState = InvalidationStates.Parse(iStateAttr.Value);
                else
                    InvalidationState = InvalidationStates.InvalidationReady;
                Description = xDoc.Root.Element("Description").Value;
                StretchToFit = Convert.ToBoolean(xDoc.Root.Element("StretchToFit").Value);
                XElement szElem = xDoc.Root.Element("Size");
                SetImage(rImageId);
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

        public override bool Resize()
        {
            if (this.IImage == null)
                return false;
            if (IImage.OriginalImage == null)
                return false;
            if (AbsoluteBounds == Rectangle.Empty)
            {
                IImage.Resize(CIAT.SaveFile.Layout.StimulusSize);
                return true;
            }
            else if ((IImage.OriginalImage.Size.Width < CIAT.SaveFile.Layout.StimulusSize.Width) && (IImage.OriginalImage.Size.Height <= CIAT.SaveFile.Layout.StimulusSize.Height))
                if (Math.Abs(IImage.AbsoluteBounds.Left - CIAT.SaveFile.Layout.StimulusSize.Width + IImage.AbsoluteBounds.Right) < 5)
                    if (Math.Abs(IImage.AbsoluteBounds.Top - CIAT.SaveFile.Layout.StimulusSize.Height + IImage.AbsoluteBounds.Bottom) < 5)
                        return false;
            IImage.Resize(CIAT.SaveFile.Layout.StimulusSize);
            return true;
        }

        protected override void Invalidate()
        {
            if (!Resize())
                Validate();
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
