using IATClient.Images;
using System;
using System.Drawing;
using System.Xml.Linq;
namespace IATClient
{
    public class ImageMetaData
    {
        private bool SupplyingSaveFileData { get; set; } = false;
        private ImageMetaDataDocument MetaDataDocument
        {
            get
            {
                return CIAT.SaveFile.ImageMetaDataDocument;
            }
        }

        private Rectangle _AbsoluteBounds = Rectangle.Empty;
        public Rectangle AbsoluteBounds
        {
            get
            {
                return SupplyingSaveFileData ? _AbsoluteBounds : Image.AbsoluteBounds;
            }
        }

        private Size _Size = Size.Empty;
        public Size Size
        {
            get
            {
                return SupplyingSaveFileData ? _Size : Image.Size;
            }
        }
        private Point _Origin = Point.Empty;
        public Point Origin
        {
            get
            {
                return (SupplyingSaveFileData) ? _Origin : Image.Origin;
            }
        }
        private Size _OriginalSize = Size.Empty;
        public Size OriginalSize
        {
            get
            {
                return SupplyingSaveFileData ? _OriginalSize : Image.OriginalSize;
            }
        }
        private Size _ThumbnailSize = Size.Empty;
        public Size ThumbnailSize
        {
            get
            {
                if (SupplyingSaveFileData)
                    return _ThumbnailSize;
                if (Image.Thumbnail == null)
                    return Size.Empty;
                return Image.Thumbnail.Size;
            }
        }

        private ImageFormat _ThumbnailFormat;
        public ImageFormat ThumbnailFormat
        {
            get
            {
                return SupplyingSaveFileData ? _ThumbnailFormat : (Image.Thumbnail?.ImageFormat);
            }
        }

        private ImageFormat _OriginalFormat;
        public ImageFormat OriginalFormat
        {
            get
            {
                return SupplyingSaveFileData ? _OriginalFormat : Image?.OriginalImage?.ImageFormat;
            }
        }

        private ImageFormat _ImageFormat;
        public ImageFormat ImageFormat
        {
            get
            {
                return SupplyingSaveFileData ? _ImageFormat : Image.ImageFormat;
            }
        }


        private DIType _DIType = null;
        public DIType DIType
        {
            get
            {
                return SupplyingSaveFileData ? _DIType : Image.DIType;
            }
        }

        private ImageMediaType _ImageMediaType = null;
        public ImageMediaType ImageMediaType
        {
            get
            {
                return SupplyingSaveFileData ? _ImageMediaType : Image.ImageMediaType;
            }
        }

        private Uri _ImageUri = null;
        public Uri ImageUri
        {
            get
            {
                if (SupplyingSaveFileData)
                    return (_ImageUri == null) ? CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, ImageRelId).TargetUri : _ImageUri;
                if (_Image == null)
                    return _ImageUri;
                return Image.URI;
            }
        }

        private Uri _OriginalImageUri = null;
        public Uri OriginalImageUri
        {
            get
            {
                if (SupplyingSaveFileData)
                    return _OriginalImageUri;
                return Image?.OriginalImage?.URI;
            }
        }

        private Uri _ThumbnailUri = null;
        public Uri ThumbnailUri
        {
            get
            {
                return SupplyingSaveFileData ? _ThumbnailUri : CIAT.SaveFile.GetRelationship(Image, ThumbnailRelId).TargetUri;
            }
        }

        public String ImageRelId { get; private set; } = String.Empty;
        private String ThumbnailRelId { get; set; } = String.Empty;
        private String OriginalImageRelId { get; set; } = String.Empty;

        private ImageManager.Image _Image = null;
        public ImageManager.Image Image
        {
            get
            {
                if (_Image == null)
                    _Image = CIAT.SaveFile.GetIImage(ImageUri) as ImageManager.Image;
                return _Image;
            }
            private set
            {
                _Image = value;
            }
        }

        public ImageMetaData(Images.ImageManager.Image img)
        {
            ImageRelId = CIAT.SaveFile.CreateRelationship(CIAT.SaveFile.ImageMetaDataDocument.GetType(), img.BaseType, CIAT.SaveFile.ImageMetaDataDocument.URI, img.URI);
            CIAT.SaveFile.ImageMetaDataDocument.Entries[ImageRelId] = this;
            Image = img;
            SupplyingSaveFileData = false;
        }

        public ImageMetaData(ImageMetaDataDocument mdd, XElement node)
        {
            Load(mdd, node);
            SupplyingSaveFileData = true;
        }

        public void SupplySaveFileData()
        {
            SupplyingSaveFileData = true;
        }

        public void SupplySourceData()
        {
            SupplyingSaveFileData = false;
        }
        public void Append(XElement root)
        {
            String absX = AbsoluteBounds.X.ToString(), absY = AbsoluteBounds.Y.ToString(), absWidth = AbsoluteBounds.Width.ToString(),
                absHeight = AbsoluteBounds.Height.ToString();
            XElement elem = new XElement(typeof(ImageMetaData).Name);
            elem.Add(new XAttribute("rImageId", ImageRelId));
            elem.Add(new XElement("AbsoluteBounds", new XElement("X", absX), new XElement("Y", absY), new XElement("Width", absWidth),
                new XElement("Height", absHeight)));
            String origX = Origin.X.ToString(), origY = Origin.Y.ToString(), sizeWidth = Size.Width.ToString(), sizeHeight = Size.Height.ToString();
            elem.Add(new XElement("Origin", new XElement("X", origX.ToString()), new XElement("Y", origY.ToString())));
            elem.Add(new XElement("Size", new XElement("Width", sizeWidth.ToString()), new XElement("Height", sizeHeight.ToString())));
            elem.Add(new XElement("MimeType", Image.MimeType));
            elem.Add(new XElement("DIType", Image.DIType.ToString()));
            if (Image.OriginalImage != null)
            {
                if (OriginalImageRelId == String.Empty)
                    OriginalImageRelId = CIAT.SaveFile.CreateRelationship(Image.BaseType, Image.OriginalImage.BaseType, Image.URI, Image.OriginalImage.URI);
                XElement origElem = new XElement("OriginalImage");
                origElem.Add(new XAttribute("rOriginalImageId", OriginalImageRelId),
                    new XElement("ImageFormat", Image.OriginalImage.MimeType),
                    new XElement("Size", new XElement("Width", Image.OriginalImage.Size.Width.ToString()),
                    new XElement("Height", Image.OriginalImage.Size.Height.ToString())));
                elem.Add(origElem);
            }
            if (Image.Thumbnail != null)
            {
                if (ThumbnailRelId == String.Empty)
                    ThumbnailRelId = CIAT.SaveFile.CreateRelationship(Image.BaseType, Image.Thumbnail.BaseType, Image.URI, Image.Thumbnail.URI);
                XElement thumbElem = new XElement("Thumbnail");
                thumbElem.Add(new XAttribute("rThumbnailImageId", ThumbnailRelId),
                    new XElement("ImageFormat", Image.Thumbnail.MimeType),
                    new XElement("Size", new XElement("Width", Image.Thumbnail.Size.Width.ToString()),
                    new XElement("Height", Image.Thumbnail.Size.Height.ToString())));
                elem.Add(thumbElem);
            }
            root.Add(elem);
        }

        public void Load(ImageMetaDataDocument mdd, XElement elem)
        {
            ImageRelId = elem.Attribute("rImageId").Value;
            _ImageUri = CIAT.SaveFile.GetRelationship(mdd, ImageRelId).TargetUri;
            int absX = Convert.ToInt32(elem.Element("AbsoluteBounds").Element("X").Value);
            int absY = Convert.ToInt32(elem.Element("AbsoluteBounds").Element("Y").Value);
            int absWidth = Convert.ToInt32(elem.Element("AbsoluteBounds").Element("Width").Value);
            int absHeight = Convert.ToInt32(elem.Element("AbsoluteBounds").Element("Height").Value);
            _AbsoluteBounds = new Rectangle(absX, absY, absWidth, absHeight);
            int ptX = Convert.ToInt32(elem.Element("Origin").Element("X").Value);
            int ptY = Convert.ToInt32(elem.Element("Origin").Element("Y").Value);
            _Origin = new Point(ptX, ptY);
            int szWidth = Convert.ToInt32(elem.Element("Size").Element("Width").Value);
            int szHeight = Convert.ToInt32(elem.Element("Size").Element("Height").Value);
            _Size = new Size(szWidth, szHeight);
            _ImageFormat = ImageFormat.FromMimeType(elem.Element("MimeType").Value);
            _DIType = DIType.FromString(elem.Element("DIType").Value);
            _ImageMediaType = ImageMediaType.FromDIType(_DIType);
            if (elem.Element("OriginalImage") != null)
            {
                XElement origElem = elem.Element("OriginalImage");
                OriginalImageRelId = origElem.Attribute("rOriginalImageId").Value;
                _OriginalImageUri = CIAT.SaveFile.GetRelationship(_ImageUri, OriginalImageRelId).TargetUri;
                _OriginalFormat = ImageFormat.FromMimeType(origElem.Element("ImageFormat").Value);
                _ImageFormat = _OriginalFormat;
                _OriginalSize = new Size(Convert.ToInt32(origElem.Element("Size").Element("Width").Value),
                    Convert.ToInt32(origElem.Element("Size").Element("Height").Value));
            }
            if (elem.Element("Thumbnail") != null)
            {
                XElement thumbElem = elem.Element("Thumbnail");
                ThumbnailRelId = thumbElem.Attribute("rThumbnailImageId").Value;
                _ThumbnailUri = CIAT.SaveFile.GetRelationship(_ImageUri, ThumbnailRelId).TargetUri;
                _ThumbnailFormat = ImageFormat.FromMimeType(thumbElem.Element("ImageFormat").Value);
                _ThumbnailSize = new Size(Convert.ToInt32(thumbElem.Element("Size").Element("Width").Value),
                    Convert.ToInt32(thumbElem.Element("Size").Element("Height").Value));
            }
        }
    }
}
