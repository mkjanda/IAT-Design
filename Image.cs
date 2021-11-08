using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace IATClient.Images
{
    public class ImageFormat {
        public static readonly ImageFormat Jpeg = new ImageFormat("jpeg", "image/jpeg", System.Drawing.Imaging.ImageFormat.Jpeg);
        public static readonly ImageFormat Jpg = new ImageFormat("jpg", "image/jpg", System.Drawing.Imaging.ImageFormat.Jpeg);
        public static readonly ImageFormat Tiff = new ImageFormat("tiff", "image/tiff", System.Drawing.Imaging.ImageFormat.Tiff);
        public static readonly ImageFormat Tif = new ImageFormat("tif", "image/tiff", System.Drawing.Imaging.ImageFormat.Tiff);
        public static readonly ImageFormat Png = new ImageFormat("png", "image/png", System.Drawing.Imaging.ImageFormat.Png);
        public static readonly ImageFormat Bmp = new ImageFormat("bmp", "image/bmp", System.Drawing.Imaging.ImageFormat.Bmp);
        private ImageFormat(String ext, String mimeType, System.Drawing.Imaging.ImageFormat format)
        {
            Extension = ext;
            MimeType = mimeType;
            Format = format;
        }
        public String Extension { get; private set; }
        public String MimeType { get; private set; } 
        public System.Drawing.Imaging.ImageFormat Format { get; private set; }
        private static IEnumerable<ImageFormat> All = new ImageFormat[] { Jpeg, Jpg, Tiff, Tif, Png, Bmp };
        public static ImageFormat FromExtension(String ext)
        {
            try
            {
                return All.Where(f => f.Extension == ext).First();
            }
            catch (InvalidOperationException ex)
            {
                return Png;
            }
        }
    }


    public interface IImage : IImageMedia, ICloneable
    {
        void CreateThumbnail();
        bool Resize(Size sz);
        Size OriginalSize { get; }
        Rectangle AbsoluteBounds { get; }
        IImageMedia OriginalImage { get; }
        String AttachUser(DIBase user);
        void DetachUser(DIBase user);
        IImageMedia Thumbnail { get; }
        String ImageMimeType { get; }
        void Save();
        void Load(Uri uri);
        void SaveMetaData();
        DIType DIType { get; }
    }



    public partial class ImageManager
    {
        public class Image : ImageMedia, IImage
        {
            public class ImageMetaData : IPackagePart, ICloneable
            {
                public Uri URI { get; set; }
                private Image Image = null;
                public Uri OriginalImageUri { get; private set; } = null;
                public Uri ThumbnailUri { get; private set; } = null;
                public Uri ImageUri { get; private set; } = null;
                public Size ImageSize { get; private set; } = Size.Empty;
                public Point ImageOrigin { get; private set; } = Point.Empty;
                public Size OriginalSize { get; private set; }
                public Size ThumbnailSize { get; private set; }
                public System.Drawing.Imaging.ImageFormat ImageFormat { get; private set; }
                public System.Drawing.Imaging.ImageFormat OriginalFormat { get; private set; }
                public System.Drawing.Imaging.ImageFormat ThumbnailFormat { get; private set; }
                private String ImageRelId = null, ThumbnailRelId = null, OriginalImageRelId = null;
                public String ImageMimeType { get; private set; }
                public String OriginalMimeType { get; private set; }
                public String ThumbnailMimeType { get; private set; }
                public DIType DIType { get; private set; }
                private bool Populated { get; set; }

                private ImageMetaData InitialState = null;

                public Type BaseType
                {
                    get
                    {
                        return typeof(ImageMetaData);
                    }
                }

                public String MimeType
                {
                    get
                    {
                        return "text/xml+" + typeof(ImageMetaData).ToString();
                    }
                }

                private ImageMetaData()
                {
                    Populated = false;
                }

                public ImageMetaData(Image img)
                {
                    URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
                    Image = img;
                    Populated = false;
                }

                public ImageMetaData(Uri uri, Image img)
                {
                    URI = uri;
                    Image = img;
                    Load(uri);
                    Populated = true;
                    InitialState = Clone() as ImageMetaData;
                }

                private void Populate()
                {
                    DIType = Image.DIType;
                    ImageSize = Image.AbsoluteBounds.Size;
                    ImageOrigin = Image.AbsoluteBounds.Location;
                    ImageFormat = Image.Format;
                    ImageUri = Image.URI;
                    ImageMimeType = "image/" + Image.Format.ToString().ToLower();
                    if (Image.OriginalImage != null)
                    {
                        OriginalMimeType = Image.OriginalImage.MimeType;
                        OriginalSize = Image.OriginalImage.Size;
                        OriginalFormat = Image.OriginalImage.Format;
                        OriginalImageUri = Image.OriginalImage.URI;
                    }
                    else
                        OriginalImageUri = null;
                    if (Image.Thumbnail != null)
                    {
                        ThumbnailMimeType = Image.Thumbnail.MimeType;
                        ThumbnailSize = Image.Thumbnail.Size;
                        ThumbnailFormat = Image.Thumbnail.Format;
                        ThumbnailUri = Image.Thumbnail.URI;
                    }
                    else
                        ThumbnailUri = null;
                    Populated = true;
                }

                public System.Drawing.Imaging.ImageFormat ToImageFormat(String str)
                {
                    switch (str.ToLower())
                    {
                        case "jpg": return System.Drawing.Imaging.ImageFormat.Jpeg;
                        case "jpeg": return System.Drawing.Imaging.ImageFormat.Jpeg;
                        case "png": return System.Drawing.Imaging.ImageFormat.Png;
                        case "tiff": return System.Drawing.Imaging.ImageFormat.Tiff;
                        case "gif": return System.Drawing.Imaging.ImageFormat.Gif;
                        case "exif": return System.Drawing.Imaging.ImageFormat.Exif;
                        case "bmp": return System.Drawing.Imaging.ImageFormat.Bmp;
                        case "emf": return System.Drawing.Imaging.ImageFormat.Emf;
                        case "icon": return System.Drawing.Imaging.ImageFormat.Icon;
                        case "memorybmp": return System.Drawing.Imaging.ImageFormat.MemoryBmp;
                        case "wmf": return System.Drawing.Imaging.ImageFormat.Wmf;
                    }
                    return null;
                }

                private XDocument WriteToXDocument()
                {
                    XDocument xDoc = new XDocument();
                    XmlWriter xWriter = xDoc.CreateWriter();
                    xWriter.WriteStartDocument();
                    xWriter.WriteStartElement(typeof(ImageMetaData).Name);
                    if ((ImageUri != null) && (ImageRelId == null))
                        ImageRelId = CIAT.SaveFile.CreateRelationship(BaseType, typeof(ImageMedia), URI, ImageUri);
                    if ((OriginalImageUri != null) && (OriginalImageRelId == null))
                        OriginalImageRelId = CIAT.SaveFile.CreateRelationship(BaseType, typeof(ImageMedia), URI, OriginalImageUri);
                    if ((ThumbnailUri != null) && (ThumbnailRelId == null))
                        ThumbnailRelId = CIAT.SaveFile.CreateRelationship(BaseType, typeof(ImageMedia), URI, ThumbnailUri);
                    if (ImageRelId != null)
                        xWriter.WriteAttributeString("ImageRelId", ImageRelId);
                    if (OriginalImageRelId != null)
                        xWriter.WriteAttributeString("OriginalImageRelId", OriginalImageRelId);
                    if (ThumbnailRelId != null)
                        xWriter.WriteAttributeString("ThumbnailRelId", ThumbnailRelId);
                    xWriter.WriteElementString("Format", ImageFormat.ToString());
                    xWriter.WriteElementString("MimeType", ImageMimeType);
                    xWriter.WriteStartElement("Origin");
                    xWriter.WriteElementString("X", ImageOrigin.X.ToString());
                    xWriter.WriteElementString("Y", ImageOrigin.Y.ToString());
                    xWriter.WriteEndElement();
                    xWriter.WriteStartElement("Size");
                    xWriter.WriteElementString("Width", ImageSize.Width.ToString());
                    xWriter.WriteElementString("Height", ImageSize.Height.ToString());
                    xWriter.WriteEndElement();
                    if (OriginalImageUri != null)
                    {
                        xWriter.WriteStartElement("OriginalImage");
                        xWriter.WriteElementString("Format", OriginalFormat.ToString());
                        xWriter.WriteElementString("MimeType", OriginalMimeType);
                        xWriter.WriteStartElement("Size");
                        xWriter.WriteElementString("Width", OriginalSize.Width.ToString());
                        xWriter.WriteElementString("Height", OriginalSize.Height.ToString());
                        xWriter.WriteEndElement();
                        xWriter.WriteEndElement();
                    }
                    if (ThumbnailUri != null)
                    {
                        xWriter.WriteStartElement("Thumbnail");
                        xWriter.WriteElementString("Format", ThumbnailFormat.ToString());
                        xWriter.WriteElementString("MimeType", ThumbnailMimeType);
                        xWriter.WriteStartElement("Size");
                        xWriter.WriteElementString("Width", ThumbnailSize.Width.ToString());
                        xWriter.WriteElementString("Height", ThumbnailSize.Height.ToString());
                        xWriter.WriteEndElement();
                        xWriter.WriteEndElement();
                    }
                    xWriter.WriteEndElement();
                    xWriter.WriteEndDocument();
                    xWriter.Close();
                    return xDoc;
                }

                public void PopulateAndSave()
                {
                    Populate();
                    if (InitialState != null)
                        if (this.Equals(InitialState))
                            return;
                    XDocument xDoc = WriteToXDocument();
                    Stream s = CIAT.SaveFile.GetWriteStream(this);
                    try
                    {
                        xDoc.Save(s);
                        s.Dispose();
                    }
                    catch (InvalidOperationException ex)
                    {
                        Dispose();
                    }
                    CIAT.SaveFile.ReleaseWriteStreamLock();
                }

                private void LoadFromXDocument(XDocument xDoc)
                {
                    XAttribute rIdAttr = xDoc.Root.Attribute("ThumbnailRelId");
                    if (rIdAttr != null)
                    {
                        ThumbnailRelId = rIdAttr.Value;
                        if (ThumbnailRelId != String.Empty)
                            ThumbnailUri = CIAT.SaveFile.GetRelationship(this, ThumbnailRelId).TargetUri;
                    }
                    rIdAttr = xDoc.Root.Attribute("OriginalImageRelId");
                    if (rIdAttr != null)
                    {
                        OriginalImageRelId = rIdAttr.Value;
                        if (OriginalImageRelId != String.Empty)
                            OriginalImageUri = CIAT.SaveFile.GetRelationship(this, OriginalImageRelId).TargetUri;
                    }
                    rIdAttr = xDoc.Root.Attribute("ImageRelId");
                    if (rIdAttr != null)
                    {
                        ImageRelId = rIdAttr.Value;
                        if (ImageRelId != String.Empty)
                            ImageUri = CIAT.SaveFile.GetRelationship(this, ImageRelId).TargetUri;
                    }
                    ImageFormat = ToImageFormat(xDoc.Root.Element(XName.Get("Format")).Value);
                    ImageMimeType = xDoc.Root.Element("MimeType").Value;
                    XElement originElem = xDoc.Root.Element("Origin"), szElem = xDoc.Root.Element(XName.Get("Size")), origElem = xDoc.Root.Element("OriginalImage"),
                        thumbElem = xDoc.Root.Element("Thumbnail");
                    if (originElem == null)
                        ImageOrigin = Point.Empty;
                    else
                    {
                        try
                        {
                            ImageOrigin = new Point(Convert.ToInt32(originElem.Element("X").Value), Convert.ToInt32(originElem.Element("Y").Value));
                        }
                        catch (Exception ex)
                        {
                            ImageOrigin = Point.Empty;
                            szElem = originElem.Element("Size");
                            origElem = originElem.Element("OriginalImage");
                            thumbElem = originElem.Element("Thumbnail");
                        }
                    }
                    ImageSize = new Size(Convert.ToInt32(szElem.Element(XName.Get("Width")).Value), Convert.ToInt32(szElem.Element(XName.Get("Height")).Value));
                    if (OriginalImageUri != null)
                    {
                        OriginalFormat = ToImageFormat(origElem.Element("Format").Value);
                        OriginalMimeType = origElem.Element("MimeType").Value;
                        szElem = origElem.Element("Size");
                        OriginalSize = new Size(Convert.ToInt32(szElem.Element("Width").Value), Convert.ToInt32(szElem.Element("Height").Value));
                    }
                    if (ThumbnailUri != null)
                    {
                        ThumbnailFormat = ToImageFormat(thumbElem.Element("Format").Value);
                        ThumbnailMimeType = thumbElem.Element("MimeType").Value;
                        szElem = thumbElem.Element("Size");
                        ThumbnailSize = new Size(Convert.ToInt32(szElem.Element("Width").Value), Convert.ToInt32(szElem.Element("Height").Value));
                    }
                }

                public void Load(Uri uri)
                {
                    this.URI = uri;
                    Stream s = CIAT.SaveFile.GetReadStream(this);
                    XDocument xDoc = XDocument.Load(s);
                    s.Dispose();
                    CIAT.SaveFile.ReleaseReadStreamLock();
                    LoadFromXDocument(xDoc);
                }

                public object Clone()
                {
                    if (!Populated)
                        Populate();
                    ImageMetaData o = new ImageMetaData();
                    o.URI = CIAT.SaveFile.CreatePart(o.BaseType, o.GetType(), o.MimeType, ".xml");
                    if (Image != null)
                    {
                        o.Image = Image.Clone() as Image;
                        CIAT.SaveFile.CreateRelationship(o.BaseType, o.Image.BaseType, o.URI, o.Image.URI);
                    }
                    return o;
                }

                public override bool Equals(object o)
                {
                    ImageMetaData metaData = o as ImageMetaData;
                    if ((metaData.Image == null) && (Image != null))
                        return false;
                    if ((metaData.Image != null) && (Image == null))
                        return false;
                    if (!metaData.Image.URI.Equals(Image.URI))
                        return false;
                    if (!metaData.Populated)
                        metaData.Populate();
                    if (Populated)
                        Populate();
                    MemoryStream metaData1 = new MemoryStream(), metaData2 = new MemoryStream();
                    metaData.WriteToXDocument().Save(metaData1);
                    WriteToXDocument().Save(metaData2);
                    return metaData1.ToArray().SequenceEqual<byte>(metaData2.ToArray());
                }

                public void Dispose()
                {
                    CIAT.SaveFile.DeletePart(this.URI);
                }
            }

            private ImageMedia _OriginalImage = null;
            private ImageMedia _Thumbnail = null;
            public IImageMedia OriginalImage { get { return _OriginalImage; } }
            public IImageMedia Thumbnail { get { return _Thumbnail; } }
            public Control ThumbnailCanvas { get; set; }
            public override bool PendingResize { get; protected set; } = false;
            private Dictionary<DIBase, String> DIUsers = new Dictionary<DIBase, String>();
            private ImageMetaData MetaData { get; set; } = null;
            public String ImageMimeType { get { return MetaData.ImageMimeType; } }
            public bool HighQualityThumbnails { get; set; } = true;
            public bool HighQualityResizes { get; set; } = true;
            public DIType DIType { get; private set; }
            private Point Origin { get; set; } = Point.Empty;

            private Rectangle _AbsoluteBounds = Rectangle.Empty;
            public Rectangle AbsoluteBounds
            {
                get
                {
                    return _AbsoluteBounds;
                }
                protected set
                {
                    _AbsoluteBounds = value;
                    Size = value.Size;
                }
            }
            public override Type BaseType
            {
                get
                {
                    return typeof(ImageMedia);
                }
            }



            public void CreateThumbnail()
            {
                if (_Thumbnail == null)
                {
                    _Thumbnail = new ImageMedia(Format, ImageMediaType.Thumbnail);

                }
                CIAT.ImageManager.GenerateThumb(this);
                
            }

            public Size OriginalSize
            {
                get
                {
                    if (OriginalImage == null)
                        return Size;
                    return OriginalImage.Size;
                }
            }

            public String AttachUser(DIBase di)
            {
                if (DIUsers.ContainsKey(di))
                    return DIUsers[di];
                DIUsers[di] = CIAT.SaveFile.CreateRelationship(di.BaseType, BaseType, di.URI, URI);
                return DIUsers[di];
            }

            public void DetachUser(DIBase di)
            {
                DIUsers.Remove(di);
                if (DIUsers.Count == 0)
                {
                    if (OriginalImage != null)
                    {
                        Image = OriginalImage.Image;
                        OriginalImage.Dispose();
                    }
                    if (Thumbnail != null)
                        Thumbnail.Dispose();
                }
            }

            protected Image(String mimeType)
            {
                MimeType = mimeType;
                MetaData = new ImageMetaData(this);
            }

            public Image(Uri uri) 
            {
                URI = uri;
                Load(uri);
                CIAT.SaveFile.Register(this);
            }

            public Image(System.Drawing.Imaging.ImageFormat format, DIType t)
            {
                DIType = t;
                MimeType = ImageMediaType.FromDIType(t).MimeType;
                Format = System.Drawing.Imaging.ImageFormat.Png;
                URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + Format.ToString());
                this.Size = Size.Empty;
                MetaData = new ImageMetaData(this);
                CIAT.SaveFile.CreateRelationship(BaseType, typeof(ImageMetaData), URI, MetaData.URI);
                CIAT.SaveFile.Register(this);
            }

            public Image(System.Drawing.Image img, System.Drawing.Imaging.ImageFormat format, DIType diType, Action<ImageChangedEvent, IImageMedia, object> onChanged) 
            {
                ImageMediaType t = ImageMediaType.FromDIType(diType);
                MetaData = new ImageMetaData(this);
                MimeType = t.MimeType;
                Format = System.Drawing.Imaging.ImageFormat.Png;
                URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + Format.ToString());
                this.Size = img.Size;
                if (onChanged != null)
                    Changed += onChanged;
                if (t == ImageMediaType.VariableSize)
                {
                    img.Tag = t;
                    Image = img.Clone() as System.Drawing.Image;
                    _OriginalImage = new ImageMedia(img.Clone() as System.Drawing.Image, format, ImageMediaType.VariableSize);
                    Size = img.Size;
                    img.Dispose();
                }
                else if ((t.ImageSize.Width >= img.Width) && (t.ImageSize.Height >= img.Height))
                {
                    if (diType.StoreOriginalImage)
                        _OriginalImage = new ImageMedia(img.Clone() as System.Drawing.Image, format, ImageMediaType.VariableSize);
                    Bitmap bmp = CIAT.ImageManager.RequestBitmap(t);
                    AbsoluteBounds = new Rectangle(new Point((bmp.Width - img.Width) >> 1, (bmp.Height - img.Height) >> 1), img.Size);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        Brush backBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.FillRectangle(backBrush, new Rectangle(0, 0, bmp.Width, bmp.Height));
                        g.DrawImage(img, AbsoluteBounds);
                        backBrush.Dispose();
                    }
                    bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                    bmp.Tag = t;
                    Image = bmp;
                    CIAT.ImageManager.ReleaseImage(img);
                }
                else if (!t.ImageSize.Equals(img.Size))
                {
                    if (diType.StoreOriginalImage)
                    {
                        System.Drawing.Image origImage = img.Clone() as System.Drawing.Image;
                        _OriginalImage = new ImageMedia(origImage, format, ImageMediaType.VariableSize);
                    }
                    Size szResize, pendingResize = t.ImageSize;
                    double arImg = (double)img.Size.Width / (double)img.Size.Height;
                    double arSizeRect = (double)Math.Min(pendingResize.Width, img.Size.Width) / (double)Math.Min(pendingResize.Height, img.Size.Height);
                    if ((img.Size.Width <= pendingResize.Width) && (img.Size.Height <= pendingResize.Height))
                        szResize = img.Size;
                    else if (arImg > arSizeRect)
                        szResize = new Size(Math.Min(pendingResize.Width, img.Size.Width), (int)(Math.Min(pendingResize.Width, img.Size.Width) / arImg));
                    else
                        szResize = new Size((int)(Math.Min(pendingResize.Height, img.Size.Height) * arImg), Math.Min(pendingResize.Height, img.Size.Height));
                    Bitmap finalImg = CIAT.ImageManager.RequestBitmap(t);
                    if (finalImg == null)
                        finalImg = new Bitmap(t.ImageSize.Width, t.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                    Rectangle destRect = new Rectangle((finalImg.Width - szResize.Width) >> 1, (finalImg.Height - szResize.Height) >> 1, szResize.Width, szResize.Height);
                    AbsoluteBounds = destRect;
                    if (destRect.Size.Equals(img.Size))
                    {
                        Image = img;
                        CIAT.ImageManager.ReleaseImage(finalImg);
                    }
                    else
                    {
                        using (Graphics gr = Graphics.FromImage(finalImg))
                        {
                            gr.FillRectangle(backBr, new Rectangle(0, 0, t.ImageSize.Width, t.ImageSize.Height));
                            gr.SmoothingMode = SmoothingMode.HighQuality;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gr.DrawImage(img, destRect);
                        }
                        backBr.Dispose();
                        finalImg.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                        Image = finalImg;
                        img.Dispose();
                    }
                }
                else
                {
                    if (diType.StoreOriginalImage)
                        _OriginalImage = new ImageMedia(img.Clone() as System.Drawing.Image, format, ImageMediaType.VariableSize);
                    double arImg = (double)img.Width / (double)img.Height;
                    Size szResize;
                    double arSizeRect = (double)Math.Min(t.ImageSize.Width, img.Size.Width) / (double)Math.Min(t.ImageSize.Height, img.Size.Height);
                    if (arImg > arSizeRect)
                        szResize = new Size(Math.Min(t.ImageSize.Width, img.Size.Width), (int)(Math.Min(t.ImageSize.Width, img.Size.Width) / arImg));
                    else
                        szResize = new Size((int)(Math.Min(t.ImageSize.Height, img.Size.Height) * arImg), Math.Min(t.ImageSize.Height, img.Size.Height));
                    Bitmap bmp = CIAT.ImageManager.RequestBitmap(t);
                    Rectangle destRect = new Rectangle((img.Width - szResize.Width) >> 1, (img.Height - szResize.Height) >> 1, szResize.Width, szResize.Height);
                    Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.FillRectangle(backBr, new Rectangle(0, 0, bmp.Width, bmp.Height));
                        g.DrawImage(img, new Rectangle(new Point((bmp.Width - img.Width) >> 1, (bmp.Height - img.Height) >> 1), img.Size));
                    }
                    AbsoluteBounds = destRect;
                    backBr.Dispose();
                    bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                    bmp.Tag = t;
                    Image = bmp;
                    CIAT.ImageManager.ReleaseImage(img);
                }
                MetaData.PopulateAndSave();
                CIAT.SaveFile.CreateRelationship(BaseType, typeof(ImageMetaData), URI, MetaData.URI);
                CIAT.SaveFile.Register(this);
                Task.Run(() =>
                {
                    onChanged(ImageChangedEvent.Resized, this, AbsoluteBounds);
                    onChanged(ImageChangedEvent.Updated, this, null);
                });
            }

            public void SaveMetaData()
            {
                if (MetaData == null)
                    MetaData = new ImageMetaData(this);
                MetaData.PopulateAndSave();
            }

            public bool Resize(Size sz)
            {
                CIAT.ImageManager.AddToResizer(this, sz);
                return true;
            }
            public bool PerformResize(Size pendingResize)
            {
                if (pendingResize == Size.Empty)
                {
                    AbsoluteBounds = Rectangle.Empty;
                    PendingResize = false;
                    return true;
                }
                Size szResize;
                Size OriginalSize = (OriginalImage == null) ? Size : OriginalImage.Size;
                double arImg = (double)OriginalSize.Width / (double)OriginalSize.Height;
                double arSizeRect = (double)pendingResize.Width / (double)pendingResize.Height;
                if ((OriginalSize.Width <= pendingResize.Width) && (OriginalSize.Height <= pendingResize.Height))
                    szResize = OriginalSize;
                else if (arImg > arSizeRect)
                {
                    int width = pendingResize.Width;
                    int height = (int)((double)width / (double)arImg);
                    szResize = new Size(width, height);
                }
                else
                {
                    int height = pendingResize.Height;
                    int width = (int)((double)height * arImg);
                    szResize = new Size(width, height);
                }
                System.Drawing.Image origImage;
                ImageMediaType iType = ImageMediaType.FromMimeType(CIAT.SaveFile.GetMimeType(URI));
                Rectangle srcRect;
                if (OriginalImage == null)
                {
                    origImage = Image;
                    srcRect = new Rectangle(new Point((origImage.Width - Size.Width) >> 1, (origImage.Height - Size.Height) >> 1), Size);
                }
                else
                {
                    origImage = OriginalImage.Image;
                    srcRect = new Rectangle(new Point(0, 0), OriginalImage.Size);
                }
                Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                Bitmap img = CIAT.ImageManager.RequestBitmap(iType);
                if ((img == null) && (iType != ImageMediaType.VariableSize))
                    img = new Bitmap(iType.ImageSize.Width, iType.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                else if (iType == ImageMediaType.VariableSize)
                    img = new Bitmap(szResize.Width, szResize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Rectangle destRect = new Rectangle((img.Width - szResize.Width) >> 1, (img.Height - szResize.Height) >> 1, szResize.Width, szResize.Height);
                if (destRect.Equals(AbsoluteBounds))
                {
                    FireChanged(ImageChangedEvent.ResizeNotNeeded, destRect);
                    return true;
                }
                AbsoluteBounds = destRect;
                if (HighQualityResizes)
                {
                    using (Graphics gr = Graphics.FromImage(img))
                    {
                        gr.FillRectangle(backBr, new Rectangle(0, 0, iType.ImageSize.Width, iType.ImageSize.Height));
                        gr.SmoothingMode = SmoothingMode.HighQuality;
                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        gr.DrawImage(origImage, destRect, srcRect, GraphicsUnit.Pixel);
                    }
                } else
                {
                    using (Graphics gr = Graphics.FromImage(img))
                    {
                        gr.FillRectangle(backBr, new Rectangle(0, 0, iType.ImageSize.Width, iType.ImageSize.Height));
                        gr.DrawImage(origImage, destRect, srcRect, GraphicsUnit.Pixel);
                    }
                }
                backBr.Dispose();
                img.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                PendingResize = false;
                origImage.Dispose();
                FireChanged(ImageChangedEvent.Resized, destRect);
                ResizeUpdate(img);
                return true;
            }

            public void UpdateThumbnail()
            {
                Size szResize;
                System.Drawing.Image img;
                Rectangle origBounds;
                if (OriginalImage == null)
                {
                    img = Image;
                    origBounds = AbsoluteBounds;
                }
                else
                {
                    img = OriginalImage.Image;
                    origBounds = new Rectangle(0, 0, OriginalSize.Width, OriginalSize.Height);
                }
                double arImg = (double)origBounds.Width / (double)origBounds.Height;
                double arSizeRect = (double)Math.Min(ThumbnailSize.Width, origBounds.Width) / (double)Math.Min(ThumbnailSize.Height, origBounds.Height);
                if ((origBounds.Width <= ThumbnailSize.Width) && (origBounds.Height <= ThumbnailSize.Height))
                    szResize = AbsoluteBounds.Size;
                else if (arImg > arSizeRect)
                    szResize = new Size(Math.Min(ThumbnailSize.Width, origBounds.Width), (int)(Math.Min(origBounds.Width, ThumbnailSize.Width) / arImg));
                else
                    szResize = new Size((int)(Math.Min(ThumbnailSize.Width, origBounds.Width) * arImg), Math.Min(ImageManager.ThumbnailSize.Height, origBounds.Height));
                Bitmap thumb = CIAT.SaveFile.ImageManager.RequestBitmap(ImageMediaType.Thumbnail);
                Graphics g;
                try
                {
                    g = Graphics.FromImage(thumb);
                }
                catch
                {
                    thumb = new Bitmap(ImageMediaType.Thumbnail.ImageSize.Width, ImageMediaType.Thumbnail.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb) { Tag = ImageMediaType.Thumbnail };
                    g = Graphics.FromImage(thumb);
                }
                using (Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor))
                    g.FillRectangle(backBr, new Rectangle(new Point(0, 0), ImageMediaType.Thumbnail.ImageSize));
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(img, new Rectangle(new Point((ThumbnailSize.Width - szResize.Width) >> 1, (ThumbnailSize.Height - szResize.Height) >> 1), szResize),
                    origBounds, GraphicsUnit.Pixel);
                g.Dispose();
                CIAT.ImageManager.ReleaseImage(img);
                thumb.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                if (Thumbnail == null)
                    _Thumbnail = new ImageMedia(thumb, System.Drawing.Imaging.ImageFormat.Png, ImageMediaType.Thumbnail);
                else
                    Thumbnail.Image = thumb;
            }
            public override object Clone()
            {
                Uri u = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + Format.ToString());
                Image o = new Image(MimeType);
                o.URI = u;
                o.AbsoluteBounds = AbsoluteBounds;
                o.Size = Size;
                o.Format = Format;
                o.MimeType = MimeType;
                if (OriginalImage != null)
                {
                    o._OriginalImage = OriginalImage.Clone() as ImageMedia;
                    CIAT.SaveFile.CreateRelationship(o.BaseType, o.OriginalImage.BaseType, o.URI, o.OriginalImage.URI);
                }
                if (Thumbnail != null)
                {
                    o._Thumbnail = Thumbnail.Clone() as ImageMedia;
                    CIAT.SaveFile.CreateRelationship(o.BaseType, o.Thumbnail.BaseType, o.URI, o.Thumbnail.URI);
                }
                o.Image = Image;
                o.MetaData = new ImageMetaData(o);
                o.MetaData.PopulateAndSave();
                CIAT.SaveFile.CreateRelationship(o.BaseType, typeof(ImageMetaData), o.URI, o.MetaData.URI);
                CIAT.SaveFile.Register(o);
                return o;
            }

            public override void Written()
            {
                if (IsDisposed)
                    return;
                MetaData.PopulateAndSave();
                base.Written();
            }

            public void Save()
            {
                if (MetaData == null)
                {
                    MetaData = new ImageMetaData(this);
                    CIAT.SaveFile.CreateRelationship(BaseType, MetaData.BaseType, URI, MetaData.URI);
                }
                MetaData.PopulateAndSave();
            }

            public void Load(Uri uri)
            {
                this.URI = uri;
                MimeType = CIAT.SaveFile.GetMimeType(uri);
                MetaData = new ImageMetaData(CIAT.SaveFile.GetRelationshipsByType(uri, typeof(ImageMedia), typeof(ImageMetaData)).First().TargetUri, this);
                CIAT.SaveFile.CreateRelationship(MetaData.BaseType, BaseType, MetaData.URI, URI);
                AbsoluteBounds = new Rectangle(MetaData.ImageOrigin, MetaData.ImageSize);
                Format = MetaData.ImageFormat;
                if (MetaData.OriginalImageUri != null)
                {
                    _OriginalImage = new ImageMedia(MetaData.OriginalImageUri, MetaData.OriginalSize, MetaData.OriginalFormat);
                }
                if (MetaData.ThumbnailUri != null)
                {
                    _Thumbnail = new ImageMedia(MetaData.ThumbnailUri, MetaData.ThumbnailSize, MetaData.ThumbnailFormat);
                }
                IsNull = false;
            }

            public override void Dispose()
            {
                if (IsDisposed)
                    return;
                if (Thumbnail != null)
                    Thumbnail.Dispose();
                if (OriginalImage != null)
                    OriginalImage.Dispose();
                MetaData.Dispose();
                base.Dispose();
            }
        }
    }
}
