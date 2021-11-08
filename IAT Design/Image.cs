using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.IO.Packaging;
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
                return All.Where(f => f.Extension == ext.ToLower()).First();
            }
            catch (InvalidOperationException ex)
            {
                return Png;
            }
        }
        public static ImageFormat FromMimeType(String mimeType)
        {
            try
            {
                return All.Where(mt => mimeType == mt.MimeType).First();
            }
            catch (InvalidOperationException)
            {
                return Png;
            }
        }
    }


    public interface IImage : IImageMedia, ICloneable
    {
        void CreateThumbnail();
        void Resize(Size sz);
        Size OriginalSize { get; }
        Rectangle AbsoluteBounds { get; set; }
        IImageMedia OriginalImage { get; }
        IImageMedia Thumbnail { get; }
        void Load(Uri uri);
        DIType DIType { get; }
        new System.Drawing.Image Img { get; set; }
        ImageMetaData MetaData { get; }

    }



    public partial class ImageManager
    {
        public class Image : ImageMedia, IImage
        {

            public ImageMetaData MetaData { get; private set; }
            private ImageMedia _OriginalImage = null;
            private ImageMedia _Thumbnail = null;
            public IImageMedia OriginalImage { get { return _OriginalImage; } }
            public IImageMedia Thumbnail { get { return _Thumbnail; } }
            public override bool PendingResize { get; protected set; } = false;
            private Dictionary<DIBase, String> DIUsers = new Dictionary<DIBase, String>();
            public Rectangle AbsoluteBounds { get; set; }
            public DIType DIType { get; private set; }
            public Point Origin { get; private set; }

            public void PopulateFromMetaData(ImageMetaData data)
            {
                MetaData = data;
                MetaData.SupplySaveFileData();
                if (MetaData.ThumbnailUri != null)
                    _Thumbnail = new ImageMedia(MetaData.ThumbnailUri, MetaData.ThumbnailSize, MetaData.ThumbnailFormat, ImageMediaType.Thumbnail);
                if (MetaData.OriginalImageUri != null)
                    _OriginalImage = new ImageMedia(MetaData.OriginalImageUri, MetaData.OriginalSize, MetaData.OriginalFormat, ImageMediaType.VariableSize);
                Origin = MetaData.Origin;
                AbsoluteBounds = MetaData.AbsoluteBounds;
                DIType = MetaData.DIType;
                Size = MetaData.Size;
                ImageMediaType = MetaData.ImageMediaType;
                ImageFormat = MetaData.ImageFormat;
                MetaData.SupplySourceData();
            }
            public bool HighQualityResizes { get; set; } = true;
            public override Type BaseType
            {
                get
                {
                    return typeof(ImageMedia);
                }
            }

            public override void PauseChangeEvents()
            {
                ChangeEventsPaused = true;
                if (OriginalImage != null)
                    OriginalImage.PauseChangeEvents();
                if (Thumbnail != null)
                    Thumbnail.PauseChangeEvents();
            }

            public override void ResumeChangeEvents()
            {
                ChangeEventsPaused = false;
                if (OriginalImage != null)
                    OriginalImage.ResumeChangeEvents();
                if (Thumbnail != null)
                    Thumbnail.ResumeChangeEvents();

            }

            public void CreateThumbnail()
            {
                if (_Thumbnail == null)
                    _Thumbnail = new ImageMedia(ImageFormat, ImageMediaType.Thumbnail);
                CIAT.SaveFile.CreateRelationship(BaseType, _Thumbnail.BaseType, URI, _Thumbnail.URI);
                
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
/*
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
                        base.Img = OriginalImage.Img;
                        OriginalImage.Dispose();
                    }
                    if (Thumbnail != null)
                        Thumbnail.Dispose();
                }
            }
*/
            protected Image()
            {
            }

            public Image(Uri uri)
            {
                URI = uri;
                Load(uri);
                CIAT.SaveFile.Register(this);
            }

            public Image(ImageFormat format, DIType t)
            {
                DIType = t;
                ImageFormat = format;
                ImageMediaType = ImageMediaType.FromDIType(t);
                URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + format.Format.ToString());
                this.Size = Size.Empty;
                CIAT.SaveFile.Register(this);
                MetaData = new ImageMetaData(this);
            }

            public Image(System.Drawing.Image img, ImageFormat format, DIType diType) 
            {
                DIType = diType;
                ImageFormat = format;
                ImageMediaType = ImageMediaType.FromDIType(diType);
                URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + format.Format.ToString());
                this.Size = img.Size;
                _OriginalImage = new ImageMedia(img.Clone() as System.Drawing.Image, format, ImageMediaType.VariableSize);
                if (ImageMediaType == ImageMediaType.VariableSize)
                {
                    img.Tag = ImageMediaType;
                    base.Img = img.Clone() as System.Drawing.Image;
                    AbsoluteBounds = new Rectangle(new Point(0, 0), img.Size);
                    img.Dispose();
                }
                else if ((ImageMediaType.ImageSize.Width >= img.Width) || (ImageMediaType.ImageSize.Height >= img.Height))
                {
                    Bitmap bmp = CIAT.ImageManager.RequestBitmap(ImageMediaType);
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
                    bmp.Tag = ImageMediaType;
                    base.Img = bmp;
                    img.Dispose();
                }
                else if (!ImageMediaType.ImageSize.Equals(img.Size))
                {
                    Size szResize, pendingResize = ImageMediaType.ImageSize;
                    double arImg = (double)img.Size.Width / (double)img.Size.Height;
                    double arSizeRect = (double)Math.Min(pendingResize.Width, img.Size.Width) / (double)Math.Min(pendingResize.Height, img.Size.Height);
                    if ((img.Size.Width <= pendingResize.Width) && (img.Size.Height <= pendingResize.Height))
                        szResize = img.Size;
                    else if (arImg > arSizeRect)
                        szResize = new Size(Math.Min(pendingResize.Width, img.Size.Width), (int)(Math.Min(pendingResize.Width, img.Size.Width) / arImg));
                    else
                        szResize = new Size((int)(Math.Min(pendingResize.Height, img.Size.Height) * arImg), Math.Min(pendingResize.Height, img.Size.Height));
                    Bitmap finalImg = CIAT.ImageManager.RequestBitmap(ImageMediaType);
                    if (finalImg == null)
                        finalImg = new Bitmap(ImageMediaType.ImageSize.Width, ImageMediaType.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                    Rectangle destRect = new Rectangle((finalImg.Width - szResize.Width) >> 1, (finalImg.Height - szResize.Height) >> 1, szResize.Width, szResize.Height);
                    AbsoluteBounds = destRect;
                    if (destRect.Size.Equals(img.Size))
                    {
                        img.Tag = ImageMediaType;
                        base.Img = img;
                        finalImg.Dispose();
                    }
                    else
                    {
                        using (Graphics gr = Graphics.FromImage(finalImg))
                        {
                            gr.FillRectangle(backBr, new Rectangle(0, 0, ImageMediaType.ImageSize.Width, ImageMediaType.ImageSize.Height));
                            gr.SmoothingMode = SmoothingMode.HighQuality;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gr.DrawImage(img, destRect);
                        }
                        backBr.Dispose();
                        finalImg.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                        img.Dispose();
                        finalImg.Tag = ImageMediaType;
                        base.Img = finalImg;
                    }
                }
                else
                {
                    double arImg = (double)img.Width / (double)img.Height;
                    Size szResize;
                    double arSizeRect = (double)Math.Min(ImageMediaType.ImageSize.Width, img.Size.Width) / 
                        (double)Math.Min(ImageMediaType.ImageSize.Height, img.Size.Height);
                    if (arImg > arSizeRect)
                        szResize = new Size(Math.Min(ImageMediaType.ImageSize.Width, img.Size.Width), 
                            (int)(Math.Min(ImageMediaType.ImageSize.Width, img.Size.Width) / arImg));
                    else
                        szResize = new Size((int)(Math.Min(ImageMediaType.ImageSize.Height, img.Size.Height) * arImg), 
                            Math.Min(ImageMediaType.ImageSize.Height, img.Size.Height));
                    Bitmap bmp = CIAT.ImageManager.RequestBitmap(ImageMediaType);
                    Rectangle destRect = new Rectangle((img.Width - szResize.Width) >> 1, (img.Height - szResize.Height) >> 1, szResize.Width, szResize.Height);
                    Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                    destRect = new Rectangle(new Point((bmp.Width - img.Width) >> 1, (bmp.Height - img.Height) >> 1), img.Size);
                    AbsoluteBounds = destRect;
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.FillRectangle(backBr, new Rectangle(0, 0, bmp.Width, bmp.Height));
                        g.DrawImage(img, destRect);
                    }
                    backBr.Dispose();
                    bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                    bmp.Tag = ImageMediaType;
                    base.Img = bmp;
                    img.Dispose();
                }
                CIAT.SaveFile.Register(this);
                MetaData = new ImageMetaData(this);
            }
/*
            public Image(Uri u, System.Drawing.Image img, ImageFormat format, DIType diType)
            {
                ImageMediaType = ImageMediaType.FromDIType(diType);
                DIType = diType;
                ImageFormat = ImageFormat.Png;
                URI = CIAT.SaveFile.CreatePart(u, MimeType);
                this.Size = img.Size;
                _OriginalImage = new ImageMedia(img.Clone() as System.Drawing.Image, format, ImageMediaType.VariableSize);
                if (ImageMediaType == ImageMediaType.VariableSize)
                {
                    img.Tag = ImageMediaType;
                    base.Img = img.Clone() as System.Drawing.Image;
                    AbsoluteBounds = new Rectangle(new Point(0, 0), img.Size);
                    img.Dispose();
                }
                else if ((ImageMediaType.ImageSize.Width >= img.Width) || (ImageMediaType.ImageSize.Height >= img.Height))
                {
                    Bitmap bmp = CIAT.ImageManager.RequestBitmap(ImageMediaType);
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
                    bmp.Tag = ImageMediaType;
                    base.Img = bmp;
                    img.Dispose();
                }
                else if (!ImageMediaType.ImageSize.Equals(img.Size))
                {
                    Size szResize, pendingResize = ImageMediaType.ImageSize;
                    double arImg = (double)img.Size.Width / (double)img.Size.Height;
                    double arSizeRect = (double)Math.Min(pendingResize.Width, img.Size.Width) / (double)Math.Min(pendingResize.Height, img.Size.Height);
                    if ((img.Size.Width <= pendingResize.Width) && (img.Size.Height <= pendingResize.Height))
                        szResize = img.Size;
                    else if (arImg > arSizeRect)
                        szResize = new Size(Math.Min(pendingResize.Width, img.Size.Width), (int)(Math.Min(pendingResize.Width, img.Size.Width) / arImg));
                    else
                        szResize = new Size((int)(Math.Min(pendingResize.Height, img.Size.Height) * arImg), Math.Min(pendingResize.Height, img.Size.Height));
                    Bitmap finalImg = CIAT.ImageManager.RequestBitmap(ImageMediaType);
                    if (finalImg == null)
                        finalImg = new Bitmap(ImageMediaType.ImageSize.Width, ImageMediaType.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                    Rectangle destRect = new Rectangle((finalImg.Width - szResize.Width) >> 1, (finalImg.Height - szResize.Height) >> 1, szResize.Width, szResize.Height);
                    AbsoluteBounds = destRect;
                    if (destRect.Size.Equals(img.Size))
                    {
                        base.Img = img;
                        finalImg.Dispose();
                    }
                    else
                    {
                        using (Graphics gr = Graphics.FromImage(finalImg))
                        {
                            gr.FillRectangle(backBr, new Rectangle(0, 0, ImageMediaType.ImageSize.Width, ImageMediaType.ImageSize.Height));
                            gr.SmoothingMode = SmoothingMode.HighQuality;
                            gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                            gr.DrawImage(img, destRect);
                        }
                        backBr.Dispose();
                        finalImg.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                        img.Dispose();
                        base.Img = finalImg;
                    }
                }
                else
                {
                    double arImg = (double)img.Width / (double)img.Height;
                    Size szResize;
                    double arSizeRect = (double)Math.Min(ImageMediaType.ImageSize.Width, img.Size.Width) / (double)Math.Min(ImageMediaType.ImageSize.Height, img.Size.Height);
                    if (arImg > arSizeRect)
                        szResize = new Size(Math.Min(ImageMediaType.ImageSize.Width, img.Size.Width), (int)(Math.Min(ImageMediaType.ImageSize.Width, img.Size.Width) / arImg));
                    else
                        szResize = new Size((int)(Math.Min(ImageMediaType.ImageSize.Height, img.Size.Height) * arImg), Math.Min(ImageMediaType.ImageSize.Height, img.Size.Height));
                    Bitmap bmp = CIAT.ImageManager.RequestBitmap(ImageMediaType);
                    Rectangle destRect = new Rectangle((img.Width - szResize.Width) >> 1, (img.Height - szResize.Height) >> 1, szResize.Width, szResize.Height);
                    Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                    destRect = new Rectangle(new Point((bmp.Width - img.Width) >> 1, (bmp.Height - img.Height) >> 1), img.Size);
                    AbsoluteBounds = destRect;
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.FillRectangle(backBr, new Rectangle(0, 0, bmp.Width, bmp.Height));
                        g.DrawImage(img, destRect);
                    }
                    backBr.Dispose();
                    bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                    bmp.Tag = ImageMediaType;
                    base.Img = bmp;
                    img.Dispose();
                }
                this.URI = u;
                CIAT.SaveFile.Register(this);
            }
*/
            public void Resize(Size sz)
            {
                CIAT.ImageManager.AddToResizer(this, sz);
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
                Rectangle srcRect;
                if (OriginalImage == null)
                {
                    origImage = base.Img;
                    srcRect = new Rectangle(new Point((origImage.Width - Size.Width) >> 1, (origImage.Height - Size.Height) >> 1), Size);
                }
                else
                {
                    origImage = OriginalImage.Img;
                    srcRect = new Rectangle(new Point(0, 0), OriginalImage.Size);
                }
                Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                Bitmap img = CIAT.ImageManager.RequestBitmap(ImageMediaType);
                if ((img == null) && (ImageMediaType != ImageMediaType.VariableSize))
                    img = new Bitmap(ImageMediaType.ImageSize.Width, ImageMediaType.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                else if (ImageMediaType == ImageMediaType.VariableSize)
                    img = new Bitmap(szResize.Width, szResize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Rectangle destRect = new Rectangle((img.Width - szResize.Width) >> 1, (img.Height - szResize.Height) >> 1, szResize.Width, szResize.Height);
                if (destRect.Equals(AbsoluteBounds))
                {
                    FireChanged(ImageEvent.ResizeNotNeeded, destRect);
                    origImage.Dispose();
                    return true;
                }
                AbsoluteBounds = destRect;
                if (HighQualityResizes)
                {
                    using (Graphics gr = Graphics.FromImage(img))
                    {
                        gr.FillRectangle(backBr, new Rectangle(0, 0, ImageMediaType.ImageSize.Width, ImageMediaType.ImageSize.Height));
                        gr.SmoothingMode = SmoothingMode.HighQuality;
                        gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        gr.DrawImage(origImage, destRect, srcRect, GraphicsUnit.Pixel);
                    }
                } else
                {
                    using (Graphics gr = Graphics.FromImage(img))
                    {
                        gr.FillRectangle(backBr, new Rectangle(0, 0, ImageMediaType.ImageSize.Width, ImageMediaType.ImageSize.Height));
                        gr.DrawImage(origImage, destRect, srcRect, GraphicsUnit.Pixel);
                    }
                }
                backBr.Dispose();
                img.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                PendingResize = false;
                origImage.Dispose();
                FireChanged(ImageEvent.Resized, destRect);
                base.Img = img;
                return true;
            }

            public void UpdateThumbnail()
            {
                System.Drawing.Image img;
                Rectangle origBounds;
                if (OriginalImage == null)
                {
                    img = base.Img;
                    origBounds = new Rectangle(new Point(0, 0), DIType.GetBoundingSize());
                }
                else
                {
                    img = OriginalImage.Img;
                    origBounds = new Rectangle(0, 0, OriginalSize.Width, OriginalSize.Height);
                }
                if (img == null)
                    return;
                double arImg = (double)origBounds.Width / (double)origBounds.Height;
                Size szResize;
                double arSizeRect = (double)Math.Min(ThumbnailSize.Width, origBounds.Width) / (double)Math.Min(ThumbnailSize.Height, origBounds.Height);
                    if ((origBounds.Width <= ThumbnailSize.Width) && (origBounds.Height <= ThumbnailSize.Height))
                        szResize = AbsoluteBounds.Size;
                    else if (arImg > arSizeRect)
                        szResize = new Size(ThumbnailSize.Width, (int)(ThumbnailSize.Width / arImg));
                    else
                        szResize = new Size((int)(ThumbnailSize.Height * arImg), ThumbnailSize.Height);
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
                    _Thumbnail = new ImageMedia(thumb, ImageFormat.Png, ImageMediaType.Thumbnail);
                else
                    Thumbnail.Img = thumb;
            }
            public override object Clone()
            {
                Uri u = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + ImageFormat.ToString());
                Image o = new Image();
                o.URI = u;
                o.AbsoluteBounds = AbsoluteBounds;
                o.Size = Size;
                o.ImageFormat = ImageFormat;
                o.DIType = DIType;
                o.ImageMediaType = ImageMediaType;
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
                o.Img = Img;
                o.MetaData = new ImageMetaData(o);
                CIAT.SaveFile.Register(o);
                return o;
            }


            public void Load(Uri uri)
            {
                String rId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, uri);
                MetaData = CIAT.SaveFile.ImageMetaDataDocument.Entries[rId];
                this.URI = MetaData.ImageUri;
                PopulateFromMetaData(MetaData);
            }

            public void Load(Uri uri, ImageMetaData md)
            {
                this.URI = uri;
                MetaData = md;
                PopulateFromMetaData(md);
            }

            public override void Dispose()
            {
                if (IsDisposed)
                    return;
                if (DIType == DIType.Null)
                    return;
                lock (imageLock)
                {
                    CIAT.SaveFile.ImageMetaDataDocument.RemoveEntry(this);
                    if (Thumbnail != null)
                        Thumbnail.Dispose();
                    if (OriginalImage != null)
                        OriginalImage.Dispose();
                    base.Dispose();
                }
            }
        }
    }
}
