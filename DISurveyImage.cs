using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
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
            switch (ext)
            {
                case "jpg":
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;

                case "jpeg":
                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;

                case "tiff":
                    format = System.Drawing.Imaging.ImageFormat.Tiff;
                    break;

                case "tif":
                    format = System.Drawing.Imaging.ImageFormat.Tiff;
                    break;

                case "png":
                    format = System.Drawing.Imaging.ImageFormat.Png;
                    break;

                default:
                    format = System.Drawing.Imaging.ImageFormat.Png;
                    break;
            }
            var i = System.Drawing.Image.FromFile(filename);
            IImage = new Images.ImageManager.Image(i, format, DIType.SurveyImage, new Action<Images.ImageChangedEvent, IImageMedia, object>(OnImageChanged));
            CIAT.SaveFile.CreateRelationship(BaseType, IImage.BaseType, URI, IImage.URI);
        }

        protected override void OnImageChanged(ImageChangedEvent evt, IImageMedia img, object arg)
        {
            if (IsDisposed)
                return;
            base.OnImageChanged(evt, img, arg);
            if (evt == ImageChangedEvent.Resized)
                AbsoluteBounds = (Rectangle)arg;
            if ((evt == ImageChangedEvent.ResizeUpdate) || (evt == ImageChangedEvent.Updated) || (evt == ImageChangedEvent.ResizeNotNeeded)) {
                if (PreviewPanel != null)
                {
                    if ((evt == ImageChangedEvent.Resized) || (evt == ImageChangedEvent.ResizeNotNeeded))
                        PreviewPanel.Height = img.Size.Height;
                    PreviewPanel.SetImage(img);
                }
            }
        }

        public DISurveyImage(Uri u) : base(u)
        {
        }

        protected override void Invalidate() {
            Validate();
        }
        

        public override bool Resize()
        {
            if (PreviewPanel != null)
            {
                IImage.Resize(new Size(PreviewPanel.Width, PreviewPanel.Height));
                return true;
            }
            return false;
        }

        public override IImageDisplay PreviewPanel
        {
            get
            {
                return _PreviewPanel;
            }
            set
            {
                if ((_PreviewPanel != null) && (value == null))
                {
                    _PreviewPanel.SetImage(null as Bitmap);
                    _PreviewPanel.Tag = null;
                }
                _PreviewPanel = value;
                if (value == null)
                    return;
                if (value.Equals(Size.Empty))
                    return;
                if ((IImage.Size.Width < value.Width) && (IImage.Size.Height < value.Height))
                {
                    value.Height = IImage.Size.Height;
                    value.SetImage(IImage);
                }
                else
                    IImage.Resize(value.Size);
            }
        }


        public override void Save()
        {
            XDocument xDoc = new XDocument();
            String rImageId = CIAT.SaveFile.GetRelationship(this, IImage);
            if (rImageId != null)
            {
                xDoc.Add(new XElement("DISurveyImage", new XAttribute("rImageId", rImageId), new XElement("AbsoluteBounds", 
                    new XElement("X", "0"), new XElement("Y", "0"), new XElement("Width", AbsoluteBounds.Width.ToString()), new XElement("Height", AbsoluteBounds.Height.ToString()))));
            }
            using (Stream s = CIAT.SaveFile.GetWriteStream(this))
                xDoc.Save(s);
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
            }
            s.Dispose();
            if (IsDisposed)
                return;
            String rImageId = xDoc?.Root.Attribute("rImageId").Value;
            if (rImageId != null)
            {
                SetImage(CIAT.SaveFile.GetRelationship(this, rImageId).TargetUri);
                if (CIAT.SaveFile.Version.CompareTo(new CVersion("1.1.1.22")) > 0)
                {
                    XElement absElem = xDoc.Root.Element("AbsoluteBounds");
                    int x = Convert.ToInt32(absElem.Element("X").Value);
                    int y = Convert.ToInt32(absElem.Element("Y").Value);
                    int width = Convert.ToInt32(absElem.Element("Width").Value);
                    int height = Convert.ToInt32(absElem.Element("Height").Value);
                    AbsoluteBounds = new Rectangle(x, y, width, height);
                }
            }
        }

        public override object Clone()
        {
            DISurveyImage di = new DISurveyImage();
            di.IImage = (Images.IImage)IImage.Clone();
            di.IImage.Changed += (evt, iMedia, arg) => di.OnImageChanged(evt, iMedia, arg);
            CIAT.SaveFile.CreateRelationship(di.BaseType, di.IImage.BaseType, di.URI, di.IImage.URI);
            di.Save();
            return di;
        }
    }
}
