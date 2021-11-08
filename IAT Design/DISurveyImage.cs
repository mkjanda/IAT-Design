using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using IATClient.Images;

namespace IATClient
{
    public class DISurveyImage : DIBase, IPackagePart, IDisposable, ICloneable
    {
        public String Description { get; private set; }
        private readonly object lockObj = new object();
        private DISurveyImage()
        {}
        private IImageDisplay _PreviewPanel = null;

        public DISurveyImage(String filename)
        {
            System.Drawing.Imaging.ImageFormat format;
            Regex exp1 = new Regex(@"\.([A-Za-z]+)$");
            String ext = exp1.Match(filename).Groups[1].Value.ToLower();
            Regex exp2 = new Regex(@"^(.+)\.(?!" + ext + ")");
            Description = exp2.Match(filename).Groups[1].Value;
            var i = System.Drawing.Image.FromFile(filename);
            IImage = new Images.ImageManager.Image(i, Images.ImageFormat.FromExtension(ext), DIType.SurveyImage);
            AbsoluteBounds = IImage.AbsoluteBounds;
        }


        protected override void Invalidate()
        {
            Resize();
        }
    

        protected override void OnImageEvent(ImageEvent evt, IImageMedia img, object arg)
        {
            if (IsDisposed)
                return;
            base.OnImageEvent(evt, img, arg);
            if (evt == ImageEvent.Resized)
                AbsoluteBounds = (Rectangle)arg;
            if (evt == ImageEvent.Updated) {
                PreviewPanel.SetImage(IImage);
            }
            Validate();
        }

        public DISurveyImage(Uri u) : base(u)
        {
        }

        public override bool Resize()
        {
            if (_PreviewPanel == null)
                return false;
            if (PreviewPanel.Size.Equals(Size.Empty))
                return false;
            IImage.Resize(new Size(PreviewPanel.Width, IImage.OriginalSize.Height));
            return true;
        }

        public override IImageDisplay PreviewPanel
        {
            get
            {
                return _PreviewPanel;
            }
            set
            {
                if (value == null)
                {
                    _PreviewPanel = null;
                    return;
                }
                _PreviewPanel = value;
                PreviewPanel.SetImage(IImage);
            }
        }


        public override void Save()
        {
            XDocument xDoc = new XDocument();
            if (rImageId != null)
            {
                xDoc.Add(new XElement("DISurveyImage", new XAttribute("rImageId", rImageId), new XAttribute("InvalidationState", InvalidationState.ToString()),
                    new XElement("AbsoluteBounds", new XElement("X", "0"), new XElement("Y", "0"), new XElement("Width", IImage.AbsoluteBounds.Width), 
                    new XElement("Height", IImage.AbsoluteBounds.Height.ToString()))));
            }
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public override void Dispose()
        {
            base.Dispose();
        }

        protected override void DoLoad(Uri u)
        {
            URI = u;
            XDocument xDoc = null;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            try
            {
                xDoc = XDocument.Load(s);
            }
            catch (XmlException ex)
            {
                Dispose();
            }
            finally
            {
                CIAT.SaveFile.ReleaseReadStreamLock();
                s.Dispose();
            }
            if (IsDisposed)
                return;
            rImageId = xDoc.Root.Attribute("rImageId")?.Value;
            if (rImageId != null)
            {
                SetImage(rImageId);
                var iInvalidationAttr = xDoc.Root.Attribute("InvalidationState");
                if (iInvalidationAttr != null)
                    InvalidationState = InvalidationStates.Parse(iInvalidationAttr.Value);
                else
                    InvalidationState = InvalidationStates.InvalidationReady;
                AbsoluteBounds = IImage.AbsoluteBounds;
            }
        }

        public override object Clone()
        {
            DISurveyImage di = new DISurveyImage();
            di.IImage = (Images.IImage)IImage.Clone();
            di.IImage.Changed += (evt, iMedia, arg) => di.OnImageEvent(evt, iMedia, arg);
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument.URI, di.IImage.URI);
            di.Save();
            return di;
        }
    }
}
