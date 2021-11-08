using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Timers;
using System.Windows.Forms;

namespace IATClient.ImageManager
{
    public delegate Image ImageGenerator();
    public interface INonUserImageSource
    {
        Image GenerateImage();
        Image TryGenerateImage();
        Size GetContainerSize();
        void Invalidate();
        object LockObject { get; }
    }

    public delegate Size ImageSizeCallback(Size OriginalImageSize, bool bSizeToFit);

    public class CResizeEvent
    {
        protected IIATImage _TheImage;
        protected List<Size> ResizeQueue = new List<Size>();
        protected Size _FinalImageSize = Size.Empty;
        protected Size _LastResize = Size.Empty;
        private object lockObject = new object();


        /*        public void Lock()
                {
                    Monitor.Enter(lockObject);
                }

                public void Unlock()
                {
                    Monitor.Pulse(lockObject);
                    Monitor.Exit(lockObject);
                }
        */
        protected Size FinalImageSize
        {
            get
            {
                lock (lockObject)
                {
                    if (ResizeQueue.Count > 0)
                        _FinalImageSize = ResizeQueue.Last();
                    return _FinalImageSize;
                }
            }
        }

        public CResizeEvent(IIATImage theImage)
        {
            _TheImage = theImage;
        }

        public Size LastResize
        {
            get
            {
                return _LastResize;
            }
        }

        public bool ResizePending
        {
            get
            {
                lock (lockObject)
                {
                    return (ResizeQueue.Count > 0);
                }
            }
        }

        public void AddResize(Size sz)
        {
            lock (lockObject)
            {
                ResizeQueue.Add(sz);
            }
        }

        public void AddResizes(CResizeEvent re)
        {
            lock (lockObject)
            {
                ResizeQueue.AddRange(re.ResizeQueue);
            }
        }

        public Size GetNewSize()
        {
            lock (lockObject)
            {
                if (ResizeQueue.Count == 0)
                    return Size.Empty;
                _FinalImageSize = ResizeQueue.Last();
                ResizeQueue.Clear();
                _LastResize = FinalImageSize;
                return FinalImageSize;
            }
        }

        public Size ImageSize
        {
            get
            {
                lock (lockObject)
                {
                    return FinalImageSize;
                }
            }
        }

        public IIATImage TheImage
        {
            get
            {
                lock (lockObject)
                {
                    return _TheImage;
                }
            }
        }
    }

    class CThumbnail : IDisposable
    {
        private IImage Parent, Thumbnail;
        private bool bValid = false;
        private bool _IsDisposed = false;

        public void Invalidate()
        {
            bValid = false;
        }

        private object generateLock = new object();
        static private System.Timers.Timer ThumbnailTimer = new System.Timers.Timer(100);
        static private List<CThumbnail> ThumbnailList = new List<CThumbnail>();
        static private ManualResetEvent HaltEvent = new ManualResetEvent(false);
        static private object queueLock = new object();
        private Control _Background = null;
        private static void QueueThumbnail(CThumbnail thumb)
        {
            lock (queueLock)
            {
                ThumbnailList.Add(thumb);
            }
        }

        private static void RemoveThumbnail(CThumbnail thumb)
        {
            lock (queueLock)
            {
                ThumbnailList.Remove(thumb);
            }
        }

        public void Dispose()
        {
            Background = null;
            Thumbnail.Delete();
            Thumbnail = null;
            lock (queueLock)
            {
                ThumbnailList.Remove(this);
            }
        }

        public CThumbnail(IImage parentImage)
        {
            Parent = parentImage;
            lock (queueLock)
            {
                ThumbnailList.Add(this);
            }
            Background = null;
        }

        public Control Background
        {
            get
            {
                return _Background;
            }
            set
            {
                lock (generateLock)
                {
                    _Background = value;
                }
            }
        }

        public IImage Image
        {
            get
            {
                return Thumbnail;
            }
        }
        /*
                public Image GenerateImmediately()
                {
                    Bitmap thumbnail = null;
                    MemoryStream parentImgStream = new MemoryStream();
                    lock (ParentImage.LockObject)
                    {
                        if (ParentImage.theImage == null)
                            return null;
                        ParentImage.theImage.Save(parentImgStream, System.Drawing.Imaging.ImageFormat.Png);
                        Size origParentSize = ParentImage.OriginalImageSize, boundedParentSize = ParentImage.BoundingSize;
                    }
                    if (IsCurrent)
                    {
                        CTempImgFile.FreeImage(_TempImageID);
                        _TempImageID = -1;
                    }
                    Size sz = ImageSize;
                    thumbnail = new Bitmap(ImageSize.Width, ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    Graphics g = Graphics.FromImage(thumbnail);
                    Brush backBrush = new SolidBrush(CIAT.Layout.BackColor);
                    g.FillRectangle(backBrush, new Rectangle(new Point(0, 0), ImageSize));
                    Size szParentOrig = ParentImage.OriginalImageSize;
                    Size szParentFinal = ParentImage.BoundingSize;
                    float aspectRatio = (float)szParentFinal.Width / (float)szParentFinal.Height;
                    SizeF szThumbImg;
                    PointF ptThumbImg;
                    if (aspectRatio > 1)
                    {
                        szThumbImg = new SizeF(ImageSize.Width * szParentOrig.Width / szParentFinal.Width, (int)((double)ImageSize.Height / aspectRatio) * szParentOrig.Height / szParentFinal.Height);
                        ptThumbImg = new PointF((ImageSize.Width - szThumbImg.Width) / 2, (ImageSize.Height - szThumbImg.Height) / 2);
                    }
                    else
                    {
                        szThumbImg = new SizeF((int)((double)ImageSize.Width * aspectRatio) * szParentOrig.Width / szParentFinal.Width, ImageSize.Height * szParentOrig.Height / szParentFinal.Height);
                        ptThumbImg = new PointF((ImageSize.Width - szThumbImg.Width) / 2, (ImageSize.Height - szThumbImg.Height) / 2);
                    }
                    RectangleF thumbImgRect = new RectangleF(ptThumbImg, szThumbImg);
                    Image parentImg = Image.FromStream(parentImgStream);
                    g.DrawImage(parentImg, thumbImgRect);
                    g.Dispose();
                    thumbnail.MakeTransparent(CIAT.Layout.BackColor);
                    _TempImageID = CTempImgFile.AppendImage(thumbnail);
                    _IsCurrent = true;
                    return thumbnail;
                }
        */
        public void Generate()
        {
            lock (generateLock)
            {
                Image parentImg = Parent.Image;
                Size ImageSize = CImageManager.ThumbnailSize;
                Bitmap thumbnail = new Bitmap(ImageSize.Width, ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Graphics g = Graphics.FromImage(thumbnail);
                Brush backBrush = new SolidBrush(Color.Transparent);
                g.FillRectangle(backBrush, new Rectangle(new Point(0, 0), ImageSize));
                float aspectRatio = (float)Parent.Size.Width / (float)Parent.Size.Height;
                SizeF szThumbImg;
                PointF ptThumbImg;
                if (aspectRatio > 1)
                {
                    szThumbImg = new SizeF(ImageSize.Width, ImageSize.Width * Parent.Size.Height / Parent.Size.Width);
                    ptThumbImg = new PointF((ImageSize.Width - szThumbImg.Width) / 2, (ImageSize.Height - szThumbImg.Height) / 2);
                }
                else
                {
                    szThumbImg = new SizeF(ImageSize.Height * Parent.Size.Width / Parent.Size.Height, ImageSize.Height);
                    ptThumbImg = new PointF((ImageSize.Width - szThumbImg.Width) / 2, (ImageSize.Height - szThumbImg.Height) / 2);
                }
                RectangleF thumbImgRect = new RectangleF(ptThumbImg, szThumbImg);
                g.DrawImage(parentImg, thumbImgRect);
                g.Dispose();
                parentImg.Dispose();
                if (Background != null)
                    if (!Background.IsDisposed)
                        Background.BeginInvoke(new Action(() => { Background.BackgroundImage = thumbnail; }));
                bValid = true;
            }
        }

        public static void ClearThumbnailList()
        {
            lock (queueLock)
            {
                ThumbnailList.Clear();
            }
        }

        private static void ProcessThumbnailRequests(object sender, ElapsedEventArgs e)
        {
            try
            {
                if (!Monitor.TryEnter(queueLock))
                    return;
                List<CThumbnail> thumbs = ThumbnailList.Where(t => !t.bValid).ToList();
                Monitor.Exit(queueLock);
                foreach (CThumbnail thumb in thumbs)
                {
                    if (ThumbnailTimer.Enabled)
                        thumb.Generate();
                }
                if (ThumbnailTimer.Enabled == false)
                    HaltEvent.Set();
            }
            catch (Exception ex)
            {
                IATConfigMainForm.ShowErrorReport("Error generating thumbnail", new CReportableException(ex.Message, ex));
                ThumbnailTimer.Enabled = false;
                if (HaltEvent != null)
                    HaltEvent.Set();
            }
        }

        public static void HaltThumbnailGenerator(ManualResetEvent evt)
        {
            HaltEvent = evt;
            ThumbnailTimer.Enabled = false;
            ProcessThumbnailRequests(null, null);
        }

        public static void StartThumbnailGenerator()
        {
            ThumbnailTimer = new System.Timers.Timer(100);
            ThumbnailTimer.AutoReset = true;
            ThumbnailTimer.Elapsed += new ElapsedEventHandler(ProcessThumbnailRequests);
            ThumbnailTimer.Enabled = true;
        }

    }


    abstract class CIATImage : IIATImage, IDisposable
    {
        protected Images.IImage ImageObj;
        protected CThumbnail Thumbnail = null;
        private CResizeEvent ResizeEvent;
        private static System.Timers.Timer ResizerTimer;
        public enum EType { UserImage, NonUserImage, CompositeImage };
        protected readonly object monitorLock = new object();
        private static readonly object resizerLock = new object();
        private static readonly List<CResizeEvent> ResizeEventList = new List<CResizeEvent>();
        private static ManualResetEvent HaltEvent = null;
        protected EType _Type;

        protected abstract IImage OriginalImage { get; }

        protected bool ImageExists
        {
            get
            {
                return ImageObj != null;
            }
        }

        protected IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            }
        }

        public void Resize(Size sz)
        {
            if (ImageObj == null)
                return;
            if (ImageObj.Size == sz)
                return;
            Size OriginalSize = ImageObj.Size;
            lock (resizerLock)
            {
                Size szResize;
                double arImg = (double)OriginalSize.Width / (double)OriginalSize.Height;
                double arSizeRect = (double)sz.Width / (double)sz.Height;
                if (arImg > arSizeRect)
                    szResize = new Size(sz.Width, (int)(sz.Width * ((double)OriginalSize.Height / (double)OriginalSize.Width)));
                else
                    szResize = new Size((int)(sz.Height * ((double)OriginalSize.Width / (double)OriginalSize.Height)), sz.Height);
                ResizeEvent.AddResize(szResize);
                if (!ResizeEventList.Contains(ResizeEvent))
                    ResizeEventList.Add(ResizeEvent);
            }
        }


        public CIATImage(EType type)
        {
            ResizeEvent = new CResizeEvent(this);
            _Type = type;
        }


        public void Update(Image img)
        {
            lock (monitorLock)
            {
                ImageObj.Update(img);
                if (ImageObj != null)
                {
                    Resize(ResizeEvent.LastResize);
                }
            }
            if (Thumbnail != null)
                Thumbnail.Invalidate();
        }

        public abstract bool IsUserImage { get; }
        public Size OriginalImageSize
        {
            get
            {
                return ImageObj.Size;
            }
        }

        public IIATImage IATImage { get { return this; } }
        public abstract bool IsValid { get; }
//        public abstract void Validate();
//        public abstract void Invalidate();

        public virtual Size ImageSize
        {
            get
            {
                if (ImageObj != null)
                    return ImageObj.Size;
                else if (ImageObj != null)
                    return ImageObj.Size;
                return Size.Empty;
            }
        }
        public abstract Size BoundingSize { get; }

        public virtual IImage theImage
        {
            get
            {
                lock (monitorLock)
                {
                    if (ImageObj == null)
                        return null;
                    if (!ResizeEvent.ResizePending && (ImageObj == null))
                        return ImageObj;
                    else if (!ResizeEvent.ResizePending)
                        return ImageObj;
                    else
                    {
                        PerformResize(ResizeEvent.GetNewSize());
                        return ImageObj;
                    }
                }
            }
        }

        public void PerformResize(Size sz)
        {
            lock (monitorLock)
            {
                try
                {
                    if (sz == Size.Empty)
                    {
                        throw new Exception("Cannot resize image to empty size.");
                    }
                    Image img = OriginalImage.Image;
                    if (ImageObj == null)
                        ImageObj = CIAT.ImageManager.CreateImage(new Bitmap(img, sz));
                    else
                        ImageObj.Update(new Bitmap(img, sz));
                    img.Dispose();
                }
                catch (Exception ex)
                {
                    CReportableException rex = new CReportableException(ex.Message, ex);
                    IATConfigMainForm.ShowErrorReport("Error in image resize", rex);
                    ResizerTimer.Enabled = false;
                }
            }
        }

        static public void HaltResizer(ManualResetEvent evt)
        {
            HaltEvent = evt;
            ResizerTimer.Enabled = false;
            ProcessResizeEvents(null, null);
        }

        static private void ProcessResizeEvents(object source, ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(resizerLock))
                return;
            List<Tuple<IIATImage, Size>> resizes = ResizeEventList.Distinct().Select(re => new Tuple<IIATImage, Size>(re.TheImage, re.GetNewSize())).ToList();
            Monitor.Exit(resizerLock);
            foreach (Tuple<IIATImage, Size> resize in resizes)
                resize.Item1.PerformResize(resize.Item2);
            if (!ResizerTimer.Enabled)
                HaltEvent.Set();
        }


        public virtual void Dispose()
        {
            if (ImageObj != null)
                ImageObj.Delete();
            if (Thumbnail != null)
                Thumbnail.Dispose();
        }

        static public void StartResizer()
        {
            ResizerTimer = new System.Timers.Timer(100);
            ResizerTimer.AutoReset = true;
            ResizerTimer.Elapsed += new ElapsedEventHandler(ProcessResizeEvents);
            ResizerTimer.Enabled = true;
        }

        public Control ThumbnailBackground
        {
            get
            {
                return Thumbnail.Background;
            }
            set
            {
                Thumbnail.Background = value;
            }
        }

        public void CreateThumbnail()
        {
            if (Thumbnail == null)
            {
                Thumbnail = new CThumbnail(OriginalImage);
            }
            else
            {
                Thumbnail.Dispose();
                Thumbnail = new CThumbnail(OriginalImage);
            }
        }

        public void DestroyThumbnail()
        {
            Thumbnail.Dispose();
            Thumbnail = null;
        }

        public void InvalidateThumbnail()
        {
            Thumbnail.Invalidate();
        }

        public abstract CComponentImage.ESourceType SourceType { get; }
    }

    class CUserImage : CIATImage, IUserImage
    {
        protected override IImage OriginalImage
        {
            get
            {
                return OrigImage;
            }
        }

        private IImage OrigImage = null;
        private String _FileName;
        private String _FullFilePath;
        public ImageSizeCallback GetImageSize = null;
        private System.Drawing.Imaging.ImageFormat _ImageFileFormat;


        public override CComponentImage.ESourceType SourceType
        {
            get
            {
                return CComponentImage.ESourceType.iatImage;
            }
        }


        public override bool IsValid
        {
            get { return true; }
        }

//        public override void Validate()
  //      {
    //        return;
      //  }

        public override Size BoundingSize
        {
            get
            {
                return OriginalImage.Size;
            }
        }

        public override bool IsUserImage
        {
            get
            {
                return true;
            }
        }

        public IImage ThumbnailImage
        {
            get
            {
                return Thumbnail.Image;
            }
        }

        /// <summary>
        /// gets a description for the image
        /// </summary>
        public String Description
        {
            get
            {
                lock (monitorLock)
                {
                    return FileName;
                }
            }
        }

        public CUserImage(IImage origImage, String fileName) : base(EType.UserImage)
        {
            _FileName = fileName;
            OrigImage = origImage;
        }

        /// <summary>
        /// constructs a CImageInfo object from the file path of an image
        /// </summary>
        /// <param name="FullFilePath">the file path of the image</param>
        public CUserImage(String FullFilePath)
            : base(EType.UserImage)
        {
            Regex regex = new Regex("\\.(jpg|jpeg|bmp|gif|png|tiff)$");
            if (!regex.IsMatch(FullFilePath.ToLower()))
            {
                throw new Exception("Invalid image file format");
            }
            Match m = regex.Match(FullFilePath.ToLower());
            String fileExtension = m.Groups[1].Value;
            switch (fileExtension.ToLower())
            {
                case "jpg":
                    _ImageFileFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;

                case "jpeg":
                    _ImageFileFormat = System.Drawing.Imaging.ImageFormat.Jpeg;
                    break;

                case "bmp":
                    _ImageFileFormat = System.Drawing.Imaging.ImageFormat.Bmp;
                    break;

                case "gif":
                    _ImageFileFormat = System.Drawing.Imaging.ImageFormat.Gif;
                    break;

                case "png":
                    _ImageFileFormat = System.Drawing.Imaging.ImageFormat.Png;
                    break;

                case "tiff":
                    _ImageFileFormat = System.Drawing.Imaging.ImageFormat.Tiff;
                    break;

            }
            Image img = Image.FromFile(FullFilePath);
            OrigImage = CIAT.ImageManager.ImageFile.CreateImage(img, _ImageFileFormat);
            _FileName = Path.GetFileName(FullFilePath);
            _FullFilePath = FullFilePath;
        }

        /// <summary>
        /// constructs a CImageInfo object from a from an IAT save file
        /// </summary>
        /// <param name="filename">the original filename of the image</param>
        /// <param name="numInstances">the number of times the image occurs in the IAT</param>
        /// <param name="offsetInFile">the offset of the image in the save file</param>
        public CUserImage(String filename, int numInstances, byte[] imgData)
            : base(EType.UserImage)
        {
            _FileName = filename;
            _FullFilePath = String.Empty;
            MemoryStream memStream = new MemoryStream(imgData);
            Image img = Image.FromStream(memStream);
            memStream.Dispose();
            OrigImage = CIAT.ImageManager.ImageFile.CreateImage(img);
        }

        public void SetSizeCallback(ImageSizeCallback callback)
        {
            GetImageSize = callback;
        }

        public void Resize()
        {
            Resize(GetImageSize(OriginalImageSize, false));
        }


        /// <summary>
        /// clears the full file path of the CImageInfo object
        /// thread-safe
        /// </summary>
        public void ClearFullFilePath()
        {
            lock (monitorLock)
            {
                _FullFilePath = String.Empty;
            }
        }

        /// <summary>
        /// gets the full file path of the CImageInfo object
        /// thread-safe
        /// </summary>
        public String FullFilePath
        {
            get
            {
                lock (monitorLock)
                {
                    return _FullFilePath;
                }
            }
        }

        /// <summary>
        /// gets the file name of the CImageInfo object
        /// </summary>
        public String FileName
        {
            get
            {
                lock (monitorLock)
                {
                    return _FileName;
                }
            }
        }
    }

    class CNonUserImage : CIATImage, INonUserImage
    {
        private bool bValid = false;
        private bool HasThumbnail;
        protected List<INonUserImageSource> Sources = new List<INonUserImageSource>();

        public override CComponentImage.ESourceType SourceType
        {
            get
            {
                return CComponentImage.ESourceType.iatImage;
            }
        }

        protected override IImage OriginalImage
        {
            get
            {
                return base.ImageObj;
            }
        }

        public override bool IsValid
        {
            get { return bValid; }
        }

        public void Validate()
        {
            bValid = true;
        }

        public CNonUserImage(INonUserImageSource source)
            : base(EType.NonUserImage)
        {
            HasThumbnail = false;
            Sources.Add(source);
        }

        public void CreateCopy(INonUserImageSource source)
        {
            Sources.Add(source);
        }

        private object validLock = new object();

        public override bool IsUserImage
        {
            get
            {
                return false;
            }
        }

        public override IImage theImage
        {
            get
            {
                lock (monitorLock)
                {
                    if (!ImageExists || !IsValid)
                        Generate();
                    Validate();
                    return base.theImage;
                }
            }
        }

        public void SetImage(Image img)
        {
            lock (monitorLock)
            {
                Update(img);
                if (HasThumbnail)
                    Thumbnail.Invalidate();
            }
        }

        public void InvalidateSource(INonUserImageSource source)
        {
            bValid = false;
            source.Invalidate();
            if (HasThumbnail)
                Thumbnail.Invalidate();
        }


        public void Invalidate()
        {
            bValid = false;
            if (HasThumbnail)
                Thumbnail.Invalidate();
        }


        public override Size ImageSize
        {
            get
            {
                return Sources.Last().GetContainerSize();
            }
        }

        public override Size BoundingSize
        {
            get
            {
                return ImageSize;
            }
        }


        public void Dispose(INonUserImageSource source)
        {
            base.Dispose();
            Sources.Remove(source);
        }


        public enum EGenerateResult { success, failure, imageDisposedOf, locked };

        public EGenerateResult Generate()
        {
            lock (monitorLock)
            {
                try
                {
                    bValid = true;
                    if (Sources.Count == 0)
                        return EGenerateResult.failure;
                    Image img = Sources.Last().GenerateImage();
                    Update(img);
                    Sources.RemoveRange(0, Sources.Count - 1);
                }
                catch (Exception)
                {
                    return EGenerateResult.imageDisposedOf;
                }
                return EGenerateResult.success;
            }
        }

        public EGenerateResult TryGenerate()
        {
            if (!Monitor.TryEnter(monitorLock))
                return EGenerateResult.locked;

            try
            {
                if (Sources.Count == 0)
                {
                    Monitor.Exit(monitorLock);
                    return EGenerateResult.failure;
                }
                Image img = Sources.Last().TryGenerateImage();
                Sources.RemoveRange(0, Sources.Count - 1);
                if (img == null)
                {
                    Monitor.Exit(monitorLock);
                    return EGenerateResult.failure;
                }
                Update(img);
                Validate();
            }
            catch (Exception)
            {
                Monitor.Exit(monitorLock);
                return EGenerateResult.imageDisposedOf;
            }
            Monitor.Exit(monitorLock);
            return EGenerateResult.success;
        }

    }

    class CCompositeImage : CIATImage, ICompositeImage
    {
        private Func<Size> PresentImageSize = null;

        public override CComponentImage.ESourceType SourceType
        {
            get
            {
                return CComponentImage.ESourceType.iatImage;
            }
        }

        protected override IImage OriginalImage
        {
            get
            {
                return base.ImageObj;
            }
        }

        private bool _HasThumbnail;

        public override bool IsValid
        {
            get { throw new Exception("Cannot utilize a composite image as a componenet image."); }
        }

        public void Validate()
        {
            throw new NotImplementedException();
        }

        public void Invalidate()
        {
            if (HasThumbnail)
                Thumbnail.Invalidate();
        }

        public CCompositeImage(Image img, bool hasThumbnail, Size imageSize, Func<Size> presentImageSize)
            : base(EType.CompositeImage)
        {
            Update(img);
            _HasThumbnail = hasThumbnail;
            PresentImageSize = presentImageSize;
        }

        public bool HasThumbnail
        {
            get
            {
                return _HasThumbnail;
            }
        }

        public override bool IsUserImage
        {
            get
            {
                return false;
            }
        }

        public override Size ImageSize
        {
            get
            {
                return PresentImageSize();
            }
        }

        public override Size BoundingSize
        {
            get
            {
                return PresentImageSize();
            }
        }

        public void UpdateImage(Image img)
        {
            Update(img);
            if (HasThumbnail)
                Thumbnail.Invalidate();
        }
    }

    public class CImageManager
    {
        private static SizeF _ScreenDPI;
        public static SizeF ScreenDPI
        {
            get
            {
                return _ScreenDPI;
            }
            set
            {
                _ScreenDPI = value;
                _ThumbnailSize = new Size((int)(112 * value.Width / 96.0), (int)(112 * value.Height / 96.0));
                CIATLayout.Scale(value);
            }
        }

        private IATConfigMainForm MainForm { get { return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName]; } }

        private static Size _ThumbnailSize;
        public static Size ThumbnailSize
        {
            get
            {
                return _ThumbnailSize;
            }
        }
        public static System.Drawing.Color TransparentColor = System.Drawing.Color.FromArgb(0, 0, 0, 0);
        /// <summary>
        /// the ImagesFileBlock object provides a (hopefully) transparent layer between the image IDs
        /// stored in the CDisplayItem derived classes and either the image files or the save file
        /// from which they are loaded.  It provides for the generation of thumbnail images in worker threads
        /// and for resizing images to the size they appear in the IAT.  It (should) allow for images to be 
        /// retrieved while the image sizing work is being done on the load of a save file, irrespective
        /// of whether the image has been sized yet, forcing an immediate resize on any image that has
        /// yet to be sized in the background
        /// </summary>
        private static Dictionary<int, CNonUserImage> NonUserImages;
        private static Dictionary<int, CUserImage> UserImages;
        private static Dictionary<int, CCompositeImage> CompositeImages;
        private static String Filename;
        private static object fileLock = new object();
        private readonly object savedImageLock = new object(), tempImageLock = new object();
        public const long MaxImageFileSize = (1 << 19) * 3;
        private static object dictionaryLock = new object();
        private static object generatorLock = new object();
        private enum EImageType { user, nonUser, composite };
        private bool Running = false;
        private System.Timers.Timer NonUserImageGenTimer;
        private bool IsHaltingForSave = false;
        private MemoryStream SavedImages = null, TempImages = null;

        /// <summary>
        /// returns a unique key for a new item in the image dictionary
        /// </summary>
        /// <returns></returns>
        private int GetImageDictionaryID(EImageType imageType)
        {
            int ctr = 0;
            lock (dictionaryLock)
            {
                while ((UserImages.ContainsKey(ctr)) || (NonUserImages.ContainsKey(ctr)) || (CompositeImages.ContainsKey(ctr)))
                    ctr++;
                switch (imageType)
                {
                    case EImageType.user:
                        UserImages[ctr] = null;
                        break;

                    case EImageType.composite:
                        CompositeImages[ctr] = null;
                        break;

                    case EImageType.nonUser:
                        NonUserImages[ctr] = null;
                        break;
                }
            }
            return ctr;
        }

        /// <summary>
        /// the no-arg constructor
        /// </summary>
        public CImageManager()
        {
            UserImages = new Dictionary<int, CUserImage>();
            NonUserImages = new Dictionary<int, CNonUserImage>();
            CompositeImages = new Dictionary<int, CCompositeImage>();

            Filename = String.Empty;
            fileLock = new object();
        }

        /// <summary>
        /// adds an image to the image dictionary
        /// </summary>
        /// <param name="fullFilePath">the full file-path of the image</param>
        /// <returns>returns the ID of the image added to the image dictionary</returns>
        public IIATImage AddImage(String fullFilePath, ImageSizeCallback callback)
        {
            int id = -1;
            if (fullFilePath == String.Empty)
                throw new Exception("An empty string was passed to the ImagesFileBlock as a file path.");
            lock (dictionaryLock)
            {
                id = GetImageDictionaryID(EImageType.user);
                CUserImage ii = new CUserImage(fullFilePath, callback);
                UserImages[id] = ii;
            }
            return id;
        }

        public long StoreUserImageInTempFile(byte[] imageData)
        {
            lock (tempImageLock)
            {
                if (TempImages == null)
                    TempImages = new MemoryStream();
                long offset = TempImages.Length;
                TempImages.Seek(0, SeekOrigin.End);
                TempImages.Write(imageData, 0, imageData.Length);
                return offset;
            }
        }

        public INonUserImage AddNonUserImage(INonUserImageSource source)
        {
            int id = GetImageDictionaryID(EImageType.nonUser);
            CNonUserImage ii = new CNonUserImage(source);
            lock (dictionaryLock)
            {
                NonUserImages[id] = ii;
            }
            return ii;
        }

        public INonUserImage AddNonUserImage(INonUserImageSource source, INonUserImage i)
        {
            ((CNonUserImage)i).CreateCopy(source);
            return i.UID;
        }

        public INonUserImage AddNonUserImage(INonUserImageSource source, bool hasThumbnail, int forcedID)
        {
            CNonUserImage ii = new CNonUserImage(source);
            ii.UID = forcedID;
            lock (dictionaryLock)
            {
                NonUserImages.Add(forcedID, ii);
            }
            return forcedID;
        }

        public static bool ContainsImage(int uid)
        {
            return (NonUserImages.Keys.Contains(uid) || CompositeImages.Keys.Contains(uid) || UserImages.Keys.Contains(uid));
        }

        /// <summary>
        /// called to determine if the images file block contains a specified image
        /// </summary>
        /// <param name="filePath">the full file path of the image that is being tested for inclusion in the image dictionary</param>
        /// <returns>true if the images file block contains the image, otherwise false</returns>
        public bool Includes(String filePath)
        {
            lock (dictionaryLock)
            {
                foreach (int i in UserImages.Keys)
                {
                    if ((UserImages[i].FullFilePath != String.Empty) && (UserImages[i].FullFilePath == filePath))
                        return true;
                }
                return false;
            }
        }

        /// <summary>
        /// returns the full file path of an image in the images file block
        /// </summary>
        /// <param name="ndx">the index of the image in the images file block</param>
        /// <returns>the full file path of the image</returns>
        public String GetFilePath(IIATImage img)
        {
                if (img.GetType() == typeof(CUserImage))
                    return (img as CUserImage).FullFilePath;
                return null;
        }


        /// <summary>
        /// compacts the image dictionary, removing all entries for images with zero instances remaining in the IAT
        /// </summary>
        private void CompactImageDictionary()
        {
            lock (dictionaryLock)
            {
                List<int> keyList = new List<int>();
                foreach (int i in UserImages.Keys)
                {
                    if (UserImages[i] == null)
                        continue;
                    if (UserImages[i].NumInstances == 0)
                    {
                        UserImages[i].Dispose();
                        keyList.Add(i);
                    }
                }
                foreach (int i in keyList)
                    UserImages.Remove(i);
                keyList.Clear();
                foreach (int i in NonUserImages.Keys)
                {
                    if (NonUserImages[i] == null)
                        continue;
                    if (NonUserImages[i].NumInstances == 0)
                    {
                        NonUserImages[i].Dispose();
                        keyList.Add(i);
                    }
                }
                foreach (int i in keyList)
                    NonUserImages.Remove(i);
            }
        }

        public void InvalidateUserImages()
        {
            lock (dictionaryLock)
            {
                foreach (CUserImage ui in UserImages.Values)
                {
                    ui.Invalidate();
                }
            }
        }

        public void InvalidateNonUserImages()
        {
            lock (dictionaryLock)
            {
                foreach (CNonUserImage nui in NonUserImages.Values)
                {
                    nui.Invalidate();
                }
            }
        }

        /*

                /// <summary>
                /// loads the specified image in the images file block
                /// thread-safe
                /// </summary>
                /// <param name="ndx">the index of the image in the images file block</param>
                /// <param name="GenerateThumbnail">a value of true will cause the function to generate a thumbnail of the image</param>
                /// <returns>a System.Drawing.Image object that contains the image data</returns>
                public Image LoadImage(int ndx)
                {
                    Image img, finalImg;
                    lock (dictionaryLock)
                    {
                        if (NonUserImages.ContainsKey(ndx))
                            return CTempImgFile.GetImage(NonUserImages[ndx].TempImageID);
                        if (CompositeImages.ContainsKey(ndx))
                            return CTempImgFile.GetImage(CompositeImages[ndx].TempImageID);
                        if (!UserImages.ContainsKey(ndx))
                            return null;
                        if (UserImages[ndx].OffsetInFile == -1)
                        {
                            lock (tempImageLock)
                            {
                                TempImages.Seek(UserImages[ndx].OffsetInTempFile, SeekOrigin.Begin);
                                byte[] imgData = new byte[(int)(UserImages[ndx].ImageDataLength)];
                                TempImages.Read(imgData, 0, imgData.Length);
                                MemoryStream tmpStream = new MemoryStream(imgData);
                                tmpStream.Seek(0, SeekOrigin.Begin);
                                img = Image.FromStream(tmpStream);
                                tmpStream.Dispose();
                                finalImg = new Bitmap(img);
                                img.Dispose();
                                return finalImg;
                            }
                        }
                        else
                        {
                            lock (fileLock)
                            {
                                SavedImages.Seek(UserImages[ndx].OffsetInFile, SeekOrigin.Begin);
                                byte[] imgData = new byte[(int)UserImages[ndx].ImageDataLength];
                                SavedImages.Read(imgData, 0, imgData.Length);
                                MemoryStream tmpStream = new MemoryStream(imgData);
                                tmpStream.Seek(0, SeekOrigin.Begin);
                                img = Image.FromStream(tmpStream);
                                tmpStream.Dispose();
                                finalImg = new Bitmap(img);
                                img.Dispose();
                                return finalImg;
                            }
                        }
                    }
                }
        */
        /// <summary>
        /// gets a dictionary collection of thumbnail images for the images contained in the image dictionary
        /// </summary>
        /// <returns>the dictionary collection</returns>
        public Dictionary<int, IImage> GetThumbnails()
        {
            Dictionary<int, IImage> result = new Dictionary<int, IImage>();
            lock (dictionaryLock)
            {
                foreach (int i in UserImages.Keys)
                    result.Add(i, UserImages[i].ThumbnailImage);
            }
            return result;
        }

        /// <summary>
        /// gets the description of an image in the images file block
        /// </summary>
        /// <param name="ndx">the index of the image to get the description of</param>
        /// <returns>the description of the image</returns>
        public String GetImageDescription(int ndx)
        {
            lock (dictionaryLock)
            {
                if (!UserImages.ContainsKey(ndx))
                    throw new Exception("Attempt made to retrieve the description of an image that does not exist in the save file dictionary.");
            }
            return UserImages[ndx].Description;
        }

        /// <summary>
        /// saves the image file block to a binary writer
        /// thread-safe
        /// </summary>
        /// <param name="bWriter">the binary writer to save the images file block to</param>
        /// <param name="filename">the file that the binary writer outputs to</param>
        /// <returns>true on success, otherwise false</returns>
        /// 
        public bool Save(BinaryWriter bWriter, String filename)
        {
            Filename = filename;
            return Save(bWriter);
        }

        public bool Save(BinaryWriter bWriter)
        {
            lock (dictionaryLock)
            {
                lock (fileLock)
                {
                    try
                    {
                        bWriter.Write(Convert.ToInt32(UserImages.Count));
                        if (SavedImages == null)
                            SavedImages = new MemoryStream();
                        foreach (int i in UserImages.Keys)
                        {
                            bWriter.Write(Convert.ToInt32(i));
                            bWriter.Write(UserImages[i].FileName);
                            bWriter.Write(Convert.ToInt32(UserImages[i].NumInstances));
                            bWriter.Flush();
                            if (UserImages[i].OffsetInFile == -1)
                            {
                                lock (tempImageLock)
                                {
                                    UserImages[i].OffsetInFile = SavedImages.Length;
                                    SavedImages.Seek(0, SeekOrigin.End);
                                    TempImages.Seek(UserImages[i].OffsetInTempFile, SeekOrigin.Begin);
                                    int imageSize = (int)UserImages[i].ImageDataLength;
                                    byte[] imgData = new byte[imageSize];
                                    TempImages.Read(imgData, 0, imageSize);
                                    bWriter.Write(Convert.ToInt32(imageSize));
                                    bWriter.Write(imgData, 0, imgData.Length);
                                    bWriter.Flush();
                                    SavedImages.Write(imgData, 0, imgData.Length);
                                    UserImages[i].RemoveFromTempFile();
                                }
                                /*
                                Image img = Image.FromFile(UserImages[i].FullFilePath);
                                MemoryStream memStream = new MemoryStream();
                                System.Drawing.Imaging.ImageFormat format;
                                String extension = System.IO.Path.GetExtension(UserImages[i].FullFilePath);
                                switch (extension.ToLower())
                                {
                                    case ".jpeg":
                                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                        break;

                                    case ".jpg":
                                        format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                        break;

                                    case ".bmp":
                                        format = System.Drawing.Imaging.ImageFormat.Bmp;
                                        break;

                                    case ".tiff":
                                        format = System.Drawing.Imaging.ImageFormat.Tiff;
                                        break;

                                    case ".gif":
                                        format = System.Drawing.Imaging.ImageFormat.Gif;
                                        break;

                                    case ".png":
                                        format = System.Drawing.Imaging.ImageFormat.Png;
                                        break;

                                    default:
                                        throw new Exception("Unrecognized image file extension.");
                                }
                                img.Save(memStream, format);
                                bWriter.Write(Convert.ToInt32(memStream.Length));
                                bWriter.Write(memStream.GetBuffer(), 0, (int)memStream.Length);
                                bWriter.Flush();*/
                            }
                            else
                            {
                                lock (savedImageLock)
                                {
                                    SavedImages.Seek(UserImages[i].OffsetInFile, SeekOrigin.Begin);
                                    bWriter.Write(Convert.ToInt32(UserImages[i].ImageDataLength));
                                    byte[] buff = new byte[(int)UserImages[i].ImageDataLength];
                                    SavedImages.Read(buff, 0, buff.Length);
                                    bWriter.Write(buff, 0, buff.Length);
                                    bWriter.Flush();
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        CReportableException reportable = new CReportableException(ex.Message, ex);
                        IATConfigMainForm.ShowErrorReport("Error saving images to .iat file", reportable);
                        return false;
                    }
                    finally
                    {
                        if (TempImages != null)
                        {
                            TempImages.Dispose();
                            TempImages = null;
                        }
                    }
                    return true;
                }
            }
        }
        /*
        public bool IsHalted
        {
            get
            {
                if ((!CIATImage.ResizerHalted) || (!CThumbnail.GeneratorHalted))
                    return false;
                return true;
            }
        }
        */


        /// <summary>
        /// loads the images file block from a binary reader object
        /// </summary>
        /// <param name="bReader">the binary reader that data is input from</param>
        /// <param name="filename">the name of the file the binary reader reads from</param>
        public void Load(BinaryReader bReader, String filename, ToolStripProgressBar bar)
        {
            lock (dictionaryLock)
            {
                lock (fileLock)
                {
                    UserImages.Clear();
                    Filename = filename;
                    int nImages = 0;
                    nImages = bReader.ReadInt32();
                    if (bar != null)
                    {
                        MainForm.Invoke(new Action<int, int>(MainForm.SetProgressRange), 0, nImages);
                        MainForm.Invoke(new Action<int>(MainForm.SetProgressValue), 0);
                    }
                        for (int ctr = 0; ctr < nImages; ctr++)
                        {
                            int id = bReader.ReadInt32();
                            String imgFilename = bReader.ReadString();
                            int nInstances = bReader.ReadInt32();
                            long offset = SavedImages.Length;
                            int nLen = bReader.ReadInt32();
                            byte[] imgData = new byte[nLen];
                            bReader.Read(imgData, 0, nLen);
                            CUserImage ii = new CUserImage(imgFilename, nInstances, imgData);
                            UserImages.Add(id, ii);
                            if (bar != null)
                                MainForm.BeginInvoke(new Action<int>(MainForm.ProgressIncrement), 1);
                        }
                        if (bar != null)
                            MainForm.BeginInvoke(new Action(MainForm.EndProgressBarUse));

                    }
            }
        }


        public void Load(BinaryReader bReader, String filename)
        {
            Load(bReader, filename, null);
            /*        lock (dictionaryLock)
                    {
                        lock (fileLock)
                        {
                            UserImages.Clear();
                            Filename = filename;
                            int nImages = bReader.ReadInt32();
                            for (int ctr = 0; ctr < nImages; ctr++)
                            {
                                int id = bReader.ReadInt32();
                                String imgFilename = bReader.ReadString();
                                int nInstances = bReader.ReadInt32();
                                long offset = bReader.BaseStream.Position;
                                CUserImage ii = new CUserImage(imgFilename, nInstances, offset);
                                ii.UID = id;
                                UserImages.Add(id, ii);
                                int nLen = bReader.ReadInt32();
                                bReader.BaseStream.Seek(nLen, SeekOrigin.Current);
                            }
                        }
                    }*/
        }
        /// <summary>
        /// should be called before the images file block is saved -- outputs image data for the images stored in the images file block on load
        /// necessary for saving because the images file block is presumed to be saved the the same file it was loaded from
        /// </summary>
        /// <param name="fileName">the name of the IAT save file that contains the images file block</param>
        /// <returns>true on success, false otherwise</returns>
        /// 
        /*
        public bool CreatePreSaveBackup(String fileName)
        {
            lock (dictionaryLock)
            {
                lock (fileLock)
                {
                    Filename = fileName;
                    FileStream outStream = null;
                    CompactImageDictionary();
                    try
                    {
                        File.Delete(Properties.Resources.sImageDictionaryTempBackupFileName);
                        outStream = new FileStream(Properties.Resources.sImageDictionaryTempBackupFileName, FileMode.Create, FileAccess.ReadWrite, FileShare.Delete);
                        FileStream inStream;
                        BinaryReader bReader;
                        if (fileName != String.Empty)
                        {
                            inStream = new FileStream(Filename, FileMode.Open);
                            bReader = new BinaryReader(inStream);
                        }
                        else
                        {
                            inStream = null;
                            bReader = null;
                        }
                        BinaryWriter bWriter = new BinaryWriter(outStream);
                        bWriter.Write(Convert.ToInt32(UserImages.Keys.Count));
                        foreach (int i in UserImages.Keys)
                        {
                            if (UserImages[i].OffsetInFile != -1)
                            {
                                bWriter.Write(Convert.ToInt32(i));
                                bWriter.Write(UserImages[i].FileName);
                                bWriter.Write(Convert.ToInt32(UserImages[i].NumInstances));
                                bWriter.Flush();
                                inStream.Seek(UserImages[i].OffsetInFile, SeekOrigin.Begin);
                                int nLen = bReader.ReadInt32();
                                MemoryStream memStream = new MemoryStream(bReader.ReadBytes(nLen), 0, nLen, false, true);
                                UserImages[i].OffsetInFile = bWriter.BaseStream.Position;
                                bWriter.Write(Convert.ToInt32(nLen));
                                bWriter.Write(memStream.GetBuffer(), 0, nLen);
                                memStream.Dispose();
                            }
                        }
                        if (fileName != String.Empty)
                        {
                            bReader.Close();
                            inStream.Close();
                        }
                        bWriter.Close();
                        outStream.Close();
                    }
                    catch (Exception ex)
                    {
                        if (outStream != null)
                            outStream.Close();
                        CReportableException reportable = new CReportableException("Error occurred while pre-processing save file", ex);
                        MainForm.BeginInvoke(new Action<String, CReportableException>(MainForm.ShowErrorReport), reportable.Caption, reportable);
                        return false;
                    }
                    return true;
                }
            }
        }
        */
        public void Clear()
        {
            Halt(false);
            if (TempImages != null)
                TempImages.Dispose();
            TempImages = null;
            if (SavedImages != null)
                SavedImages.Dispose();
            SavedImages = null;
            lock (dictionaryLock)
            {
                foreach (int i in UserImages.Keys)
                    UserImages[i].Dispose();
                UserImages.Clear();
                foreach (int i in NonUserImages.Keys)
                    NonUserImages[i].Dispose();
                NonUserImages.Clear();
                foreach (int i in CompositeImages.Keys)
                    CompositeImages[i].Dispose();
                CompositeImages.Clear();
                CThumbnail.ClearThumbnailList();
            }
            Start();
        }


        public void Flush()
        {
            lock (dictionaryLock)
            {
                if (TempImages != null)
                    TempImages.Dispose();
                TempImages = null;
                if (SavedImages != null)
                    SavedImages.Dispose();
                SavedImages = null;
                foreach (int i in UserImages.Keys)
                    UserImages[i].Dispose();
                UserImages.Clear();
                foreach (int i in NonUserImages.Keys)
                    NonUserImages[i].Dispose();
                NonUserImages.Clear();
                foreach (int i in CompositeImages.Keys)
                    CompositeImages[i].Dispose();
                CompositeImages.Clear();
                CThumbnail.ClearThumbnailList();
                CCompositeImageGenerator.ClearImageList();
            }
        }

        /// <summary>
        /// frees data allocated for the object, namely the temporary images that represent thumbnails of the images in the images file block
        /// </summary>
        public void Dispose()
        {
            Halt(false);
            IsDisposed = true;
            if (TempImages != null)
                TempImages.Dispose();
            TempImages = null;
            if (SavedImages != null)
                SavedImages.Dispose();
            SavedImages = null;
            lock (dictionaryLock)
            {
                foreach (int i in UserImages.Keys)
                {
                    UserImages[i].Dispose();
                }
                foreach (int i in NonUserImages.Keys)
                {
                    NonUserImages[i].Dispose();
                }
                UserImages.Clear();
                NonUserImages.Clear();
                CompositeImages.Clear();
                CThumbnail.ClearThumbnailList();
            }
            ImageFile.Dispose();
        }

        private bool IsDisposed = false;
        public void Start()
        {
            this.IsHaltingForSave = false;
            CCompositeImageGenerator.StartGeneration();
            StartResizer();
            StartThumbnailGenerator();
            StartNonUserImageGenerator();
            StartImageFileCompactor();
            this.Running = true;
        }

        private ManualResetEvent NonUserImageGenHaltEvent = null;

        public void Halt(bool bIsHaltingForSave)
        {
            if (!this.Running)
                return;
            CCompositeImageGenerator.EndGeneration();
            ManualResetEvent ResizerEvent = new ManualResetEvent(false), ThumbnailEvent = new ManualResetEvent(false), NonUserImageGeneratorEvent = new ManualResetEvent(false);
            CIATImage.HaltResizer(ResizerEvent);
            CThumbnail.HaltThumbnailGenerator(ThumbnailEvent);
            HaltNonUserImageGenerator(NonUserImageGeneratorEvent);
            ResizerEvent.WaitOne();
            ThumbnailEvent.WaitOne();
            NonUserImageGeneratorEvent.WaitOne();
            this.IsHaltingForSave = bIsHaltingForSave;
            this.Running = false;
        }

        public void AddImageToResizer(int ID, Size sz)
        {
            lock (dictionaryLock)
            {
                if (UserImages.ContainsKey(ID))
                    UserImages[ID].Resize(sz);
                if (NonUserImages.ContainsKey(ID))
                    NonUserImages[ID].Resize(sz);
            }
        }

        protected void StartResizer()
        {
            CIATImage.StartResizer();
        }

        protected void StartThumbnailGenerator()
        {
            CThumbnail.StartThumbnailGenerator();
        }

        public IIATImage this[int ID]
        {
            get
            {
                lock (dictionaryLock)
                {
                    if (IsDisposed)
                        return null;
                    if (UserImages.Keys.Contains(ID))
                        return UserImages[ID];
                    if (NonUserImages.Keys.Contains(ID))
                        return NonUserImages[ID];
                    if (CompositeImages.Keys.Contains(ID))
                        return CompositeImages[ID];
                    return null;
                }
            }
        }

        public void GenerateImage(int ID)
        {
            if (this.Running)
                throw new Exception("Invalid attempt to force-generate an image while the image manager is running.");
            if (UserImages.Keys.Contains(ID))
                UserImages[ID].Resize();
            if (NonUserImages.Keys.Contains(ID))
                NonUserImages[ID].InvalidateNow();
            //            if (CompositeImages.Keys.Contains(ID))
            //              CompositeImages[ID].InvalidateNow();
        }

        private static object NonUserImageGeneratorLock = new object();

        private void HaltNonUserImageGenerator(ManualResetEvent evt)
        {
            NonUserImageGenHaltEvent = evt;
            NonUserImageGenHaltEvent.Reset();
            NonUserImageGenTimer.Enabled = false;
            GenerateNonUserImages(null, null);
        }

        private void GenerateNonUserImages(object sender, ElapsedEventArgs e)
        {
            if (!Monitor.TryEnter(generatorLock))
                return;
            try
            {
                List<CNonUserImage> images = null;
                if (!Monitor.TryEnter(dictionaryLock))
                    if (sender != null)
                        return;
                images = new List<CNonUserImage>(NonUserImages.Values);
                Monitor.Exit(dictionaryLock);
                foreach (CNonUserImage nui in images)
                {
                    if (nui != null)
                    {
                        if (nui.IsValid)
                            continue;
                        CNonUserImage.EGenerateResult result = CNonUserImage.EGenerateResult.success;
                        result = nui.TryGenerate();
                        if (result == CNonUserImage.EGenerateResult.success)
                        {
                            nui.Validate();
                        }
                        if (result == CNonUserImage.EGenerateResult.imageDisposedOf)
                        {
                            lock (dictionaryLock)
                            {
                                NonUserImages.Remove(nui.UID);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                IATConfigMainForm.ShowErrorReport("Image Manager Error", new CReportableException("Error generating non-user images", ex));
            }
            finally
            {
                Monitor.Exit(generatorLock);
            }
            if (NonUserImageGenTimer.Enabled == false)
                NonUserImageGenHaltEvent.Set();
        }

        protected void StartNonUserImageGenerator()
        {
            NonUserImageGenTimer = new System.Timers.Timer(100);
            NonUserImageGenTimer.AutoReset = true;
            NonUserImageGenTimer.Elapsed += new ElapsedEventHandler(GenerateNonUserImages);
            NonUserImageGenTimer.Enabled = true;
        }


        private void ResizeUserImages(object o)
        {
            List<CUserImage> images = (List<CUserImage>)o;
            foreach (CUserImage i in images)
                i.Resize();
        }

        public void InvalidateImages()
        {
            lock (dictionaryLock)
            {
                foreach (CUserImage ui in UserImages.Values)
                    ui.Invalidate();
                foreach (CNonUserImage nui in NonUserImages.Values)
                    nui.Invalidate();
            }
        }

        public int AddCompositeImage(Image img, bool hasThumbnail, Size imageSize, Func<Size> currImageSize)
        {
            int id = GetImageDictionaryID(EImageType.composite);
            CCompositeImage ci = new CCompositeImage(img, hasThumbnail, imageSize, currImageSize);
            ci.UID = id;
            CompositeImages[id] = ci;
            return id;
        }


        protected void StartImageFileCompactor()
        {
            //CTempImgFile.StartCompacting();
        }

        public void ClearTempImageFile()
        {
            ImageFile.Dispose();
        }
    }
}
