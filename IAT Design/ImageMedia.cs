using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Threading;

namespace IATClient.Images
{
    public class ImageEvent : Enumeration
    {
        public static readonly ImageEvent Stored = new ImageEvent(1, "Stored");
        public static readonly ImageEvent Resized = new ImageEvent(2, "Resized");
        public static readonly ImageEvent Deleted = new ImageEvent(3, "Deleted");
        public static readonly ImageEvent Updated = new ImageEvent(4, "Updated");
        public static readonly ImageEvent Fetched = new ImageEvent(5, "Fetched");
        public static readonly ImageEvent Initialized = new ImageEvent(7, "Initialized");
        public static readonly ImageEvent ResizeUpdate = new ImageEvent(8, "ResizeUpdate");
        public static readonly ImageEvent ResizeNotNeeded = new ImageEvent(9, "ResizeNotNeeded");
        public static readonly ImageEvent LoadedFromDisk = new ImageEvent(9, "LoadedFromDisk");
        private ImageEvent(int val, String name) : base(val, name) { }
        private static readonly IEnumerable<ImageEvent> All =
            new ImageEvent[] { Stored, Resized, Deleted, Updated, Fetched, Initialized };
        public static ImageEvent Parse(String name)
        {
            return All.Where(ice => ice.Name == name).FirstOrDefault();
        }
    }

    public class ImageMediaType : Enumeration
    {
        public static readonly ImageMediaType FullWindow = new ImageMediaType(1, "FullWindow")
        {
            InitialSize = 10,
            IdealSize = 10,
            MaxSize = 500,
            GrowRate = 20,
            ShrinkRate = 100,
            GetSize = () => CIAT.SaveFile.Layout.InteriorSize,
            DITypes = new DIType[] { DIType.LambdaGenerated, DIType.Preview }
        };
        public static readonly ImageMediaType ResponseKey = new ImageMediaType(2, "ResponseKey")
        {
            InitialSize = 10,
            IdealSize = 0,
            MaxSize = 500,
            GrowRate = 5,
            ShrinkRate = 50,
            GetSize = () => CIAT.SaveFile.Layout.KeyValueSize,
            DITypes = new DIType[] { DIType.DualKey, DIType.ResponseKeyText, DIType.ResponseKeyImage, DIType.Conjunction }
        };
        public static readonly ImageMediaType ResponseKeyOutline = new ImageMediaType(3, "ResponseKeyOutline")
        {
            InitialSize = 2,
            IdealSize = 2,
            MaxSize = 50,
            GrowRate = 1,
            ShrinkRate = 1,
            GetSize = () => CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle.Size,
            DITypes = new DIType[] { DIType.LeftKeyValueOutline, DIType.RightKeyValueOutline }
        };
        public static readonly ImageMediaType Stimulus = new ImageMediaType(4, "Stimulus")
        {
            InitialSize = 25,
            IdealSize = 50,
            MaxSize = 500,
            GrowRate = 20,
            ShrinkRate = 100,
            GetSize = () => CIAT.SaveFile.Layout.StimulusSize,
            DITypes = new DIType[] { DIType.StimulusImage, DIType.StimulusText }
        };
        public static readonly ImageMediaType ErrorMark = new ImageMediaType(5, "ErrorMark")
        {
            InitialSize = 1,
            IdealSize = 1,
            MaxSize = 10,
            GrowRate = 1,
            ShrinkRate = 1,
            GetSize = () => CIAT.SaveFile.Layout.ErrorSize,
            DITypes = new DIType[] { DIType.ErrorMark }
        };
        public static readonly ImageMediaType BlockInstructions = new ImageMediaType(6, "BlockInstructions")
        {
            InitialSize = 10,
            IdealSize = 10,
            MaxSize = 50,
            GrowRate = 5,
            ShrinkRate = 50,
            GetSize = () => CIAT.SaveFile.Layout.InstructionsSize,
            DITypes = new DIType[] { DIType.IatBlockInstructions }
        };
        public static readonly ImageMediaType TextInstructionScreen = new ImageMediaType(7, "TextInstructionScreen")
        {
            InitialSize = 5,
            IdealSize = 5,
            MaxSize = 50,
            GrowRate = 5,
            ShrinkRate = 50,
            GetSize = () => CIAT.SaveFile.Layout.InstructionScreenTextAreaRectangle.Size,
            DITypes = new DIType[] { DIType.TextInstructionsScreen }
        };
        public static readonly ImageMediaType KeyedInstructionScreen = new ImageMediaType(8, "KeyedInstructionScreen")
        {
            InitialSize = 5,
            IdealSize = 5,
            MaxSize = 50,
            GrowRate = 5,
            ShrinkRate = 50,
            GetSize = () => CIAT.SaveFile.Layout.KeyInstructionScreenTextAreaRectangle.Size,
            DITypes = new DIType[] { DIType.KeyedInstructionsScreen }
        };
        public static readonly ImageMediaType MockItemInstructions = new ImageMediaType(9, "MockItemInstructions")
        {
            InitialSize = 5,
            IdealSize = 5,
            MaxSize = 50,
            GrowRate = 5,
            ShrinkRate = 50,
            GetSize = () => CIAT.SaveFile.Layout.MockItemInstructionsRectangle.Size,
            DITypes = new DIType[] { DIType.MockItemInstructions }
        };
        public static readonly ImageMediaType ContinueInstructions = new ImageMediaType(10, "ContinueInstructions")
        {
            InitialSize = 5,
            IdealSize = 5,
            MaxSize = 75,
            GrowRate = 5,
            ShrinkRate = 50,
            GetSize = () => CIAT.SaveFile.Layout.ContinueInstructionsRectangle.Size,
            DITypes = new DIType[] { DIType.ContinueInstructions }
        };
        public static readonly ImageMediaType VariableSize = new ImageMediaType(11, "VariableSize")
        {
            InitialSize = 0,
            IdealSize = 0,
            MaxSize = 0,
            GrowRate = 0,
            ShrinkRate = 0,
            GetSize = () => Size.Empty,
            DITypes = new DIType[] { DIType.SurveyImage }
        };
        public static readonly ImageMediaType Thumbnail = new ImageMediaType(12, "Thumbnail")
        {
            InitialSize = 25,
            IdealSize = 15,
            MaxSize = 250,
            GrowRate = 5,
            ShrinkRate = 15,
            GetSize = () => ImageManager.ThumbnailSize,
            DITypes = new DIType[] { }
        };
        public static readonly ImageMediaType Null = new ImageMediaType(13, "Null")
        {
            InitialSize = 1,
            IdealSize = 1,
            MaxSize = 3,
            GrowRate = 1,
            ShrinkRate = 1,
            GetSize = () => new Size(1, 1),
            DITypes = new DIType[] { DIType.Null }
        };
        public static readonly ImageMediaType FullPreview = new ImageMediaType(14, "FullPreview")
        {
            InitialSize = 10,
            IdealSize = 15,
            MaxSize = 500,
            GrowRate = 20,
            ShrinkRate = 100,
            GetSize = () =>
            {
                double arPreview = (double)CIAT.SaveFile.Layout.InteriorSize.Width / (double)CIAT.SaveFile.Layout.InteriorSize.Height;
                if (arPreview > 1)
                    return new Size(500, (int)((double)500 / (double)arPreview));
                else
                    return new Size((int)(500.0 * arPreview), 500);
            },
            DITypes = new DIType[] { }
        };
        public static readonly ImageMediaType ResponseKeyPreview = new ImageMediaType(15, "ResponseKeyPreview")
        {
            InitialSize = 0,
            IdealSize = 0,
            MaxSize = 50,
            GrowRate = 10,
            ShrinkRate = 10,
            GetSize = () =>
            {
                double arPreview = (double)CIAT.SaveFile.Layout.KeyValueSize.Width / (double)CIAT.SaveFile.Layout.KeyValueSize.Height;
                if (arPreview > 1)
                    return new Size(150, (int)(150 / arPreview));
                else
                    return new Size((int)(100 * arPreview), 100);

            },
            DITypes = new DIType[] { }
        };
        public int InitialSize { get; private set; }
        public int IdealSize { get; private set; }
        public int GrowRate { get; private set; }
        public int ShrinkRate { get; private set; }
        public int MaxSize { get; private set; }
        public int LastSize { get; set; } = 0;
        public int CurrentSize { get; set; } = 0;
        public long TicksAtIdeal { get; private set; } = 1000;
        public DateTime IdealAt { get; set; } = DateTime.Now;
        private IEnumerable<DIType> DITypes { get; set; }
        private Func<Size> GetSize { get; set; }
        public object lockObject { get; private set; }
        public Size ImageSize
        {
            get
            {
                return GetSize();
            }
        }
        private ImageMediaType(int val, String name) : base(val, name)
        {
            lockObject = new object();
        }
        public static readonly IEnumerable<ImageMediaType> All = new ImageMediaType[] { FullWindow, ResponseKey, ResponseKeyOutline, Stimulus,
            ErrorMark, BlockInstructions, TextInstructionScreen, KeyedInstructionScreen, MockItemInstructions, ContinueInstructions,
            VariableSize, Thumbnail, Null, FullPreview, ResponseKeyPreview
        };
        public static ImageMediaType FromDIType(DIType t)
        {
            return All.Where(imt => imt.DITypes.Contains(t)).FirstOrDefault();
        }
    }

    public class ImageChangedEventArgs
    {
        public IImageMedia Image { get; private set; }
        public ImageEvent Event { get; private set; }
        public object Arg { get; private set; }
        public ImageChangedEventArgs(IImageMedia image, ImageEvent e, object o)
        {
            Image = image;
            Event = e;
            Arg = o;
        }
    }

    public interface IImageMedia : IDisposable, IPackagePart, ICloneable
    {
        System.Drawing.Image Img { get; set; }
        Size Size { get; }
        ImageFormat ImageFormat { get; set; }
        String FileExtension { get; }
        event Action<ImageEvent, IImageMedia, object> Changed;
        void ClearChanged();
        bool IsCached { get; }
        void PauseChangeEvents();
        void ResumeChangeEvents();
        void LoadImage();
        void DisposeOfImage();

    }

    partial class ImageManager
    {
        public class ImageMedia : IImageMedia
        {
            public const long CacheDelay = 5000000;
            public bool IsDisposed { get; protected set; } = false;
            protected bool ChangeEventsPaused { get; set; } = false;

            public bool IsCached
            {
                get
                {
                    return ((image != null) && (CacheEntryTime != DateTime.MaxValue));
                }
            }

            public bool CacheExpired
            {
                get
                {
                    //                    if ((CacheEntryTime == DateTime.MaxValue) || (CacheEntryTime == DateTime.MinValue))
                    //                      return false;
                    //                return CacheEntryTime.AddMilliseconds(CacheDelay).CompareTo(DateTime.Now) < 0;
                    return true;
                }
            }

            public virtual void PauseChangeEvents()
            {
                ChangeEventsPaused = true;
            }

            public virtual void ResumeChangeEvents()
            {
                ChangeEventsPaused = false;
            }


            private readonly ConcurrentQueue<System.Drawing.Image> ImagesQueuedForCache = new ConcurrentQueue<System.Drawing.Image>();
            public Uri URI { get; set; } = null;
            private Size _Size = Size.Empty;
            private ImageFormat _ImageFormat = null;
            public ImageFormat ImageFormat
            {
                get
                {
                    if (_ImageFormat == null)
                        _ImageFormat = ImageFormat.FromMimeType(MimeType);
                    return _ImageFormat;
                }
                set
                {
                    _ImageFormat = value;
                }
            }
            protected System.Drawing.Image image = null;
            public bool PendingFetch { get; private set; } = false;
            public bool PendingWrite { get; private set; } = false;
            public virtual bool PendingResize { get { return false; } protected set { } }
            public readonly object lockObj = new object();
            public readonly object imageLock = new object();
            public readonly object cacheLock = new object();
            public ManualResetEventSlim pendingFetchEvent = new ManualResetEventSlim(true), pendingWriteEvent = new ManualResetEventSlim(true);
            public ManualResetEventSlim CachedEvent { get; private set; } = new ManualResetEventSlim(false);
            public String MimeType { get { return ImageFormat.MimeType; } }
            public event Action<ImageEvent, IImageMedia, object> Changed = null;
            public DateTime CacheEntryTime { get; set; } = DateTime.MaxValue;
            protected static Action<IntPtr, byte, int> Memset;
            public ImageMediaType ImageMediaType { get; protected set; }
            static ImageMedia()
            {
                DynamicMethod dynMethod = new DynamicMethod("Memset", MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard,
                    null, new[] { typeof(IntPtr), typeof(byte), typeof(int) }, typeof(ImageMedia), true);
                var generator = dynMethod.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Ldarg_2);
                generator.Emit(OpCodes.Initblk);
                generator.Emit(OpCodes.Ret);
                Memset = dynMethod.CreateDelegate(typeof(Action<IntPtr, byte, int>)) as Action<IntPtr, byte, int>;
            }

            protected ImageMedia()
            {
            }

            public ImageMedia(Uri uri, Size sz, ImageFormat format, ImageMediaType type)
            {
                URI = uri;
                this.Size = sz;
                ImageFormat = format;
                ImageMediaType = type;
            }

            public ImageMedia(ImageFormat format, ImageMediaType type)
            {
                this.Size = type.ImageSize;
                ImageMediaType = type;
                ImageFormat = format;
                URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + ImageFormat.Format.ToString());
            }

            public ImageMedia(System.Drawing.Image img, ImageFormat format, ImageMediaType type)
            {
                ImageFormat = format;
                URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + format.Format.ToString());
                this.Size = img.Size;
                ImageMediaType = type;
                Img = img;
            }

            public virtual Type BaseType
            {
                get
                {
                    return typeof(ImageMedia);
                }
            }

            public String FileExtension
            {
                get
                {
                    return ImageFormat.Extension;
                }
            }

            public Size Size
            {
                get
                {
                    if (_Size.IsEmpty || (_Size.Width == 0) || (_Size.Height == 0))
                        return ImageMediaType.ImageSize;
                    return _Size;
                }
                protected set
                {
                    _Size = value;
                }
            }


            protected virtual System.Drawing.Image CreateCopy(System.Drawing.Image img)
            {
                lock (imageLock)
                {
                    if (img == null)
                        return null;
                    CacheEntryTime = DateTime.Now;
                    System.Drawing.Image clone = img.Clone() as System.Drawing.Image;
                    clone.Tag = img.Tag;
                    return clone;
                }
            }

            public void RemoveFromCache()
            {
                lock (imageLock)
                {
                    if (image != null)
                    {
                        //WriteImage();
                        CIAT.ImageManager.ReleaseImage(Img);
                        CacheEntryTime = DateTime.MaxValue;
                        if (URI != null)
                            CIAT.SaveFile.DeletePart(URI);
                        URI = null;
                    }
                }
            }

            public void WriteImage(System.Drawing.Image val)
            {
                lock (imageLock)
                {
                    if (URI == null)
                        URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + ImageFormat.ToString());
                    Stream s = null;
                    try
                    {
                        s = CIAT.SaveFile.GetWriteStream(this);
                        val.Save(s, ImageFormat.Format);
                    }
                    finally
                    {
                        s?.Dispose();
                        CIAT.SaveFile.ReleaseWriteStreamLock();
                    }
                }
            }

            public void LoadImage()
            {
                lock (imageLock)
                {
                    if (IsCached)
                        return;
                    image = CIAT.ImageManager.FetchImageMedia(this);
                    if (image == null)
                        return;
                    CacheEntryTime = DateTime.Now;
                    FireChanged(ImageEvent.LoadedFromDisk);
                }
            }

            public virtual System.Drawing.Image Img
            {
                get
                {
                    System.Drawing.Image img;
                    lock (imageLock)
                    {
                        if (IsDisposed)
                            return null;
                        if (IsCached)
                            return CreateCopy(image);
                        img = CIAT.ImageManager.FetchImageMedia(this);
                        if (img == null)
                            return null;
                        img.Tag = ImageMediaType;
                        //       retVal = CreateCopy(image);
                    }
                    (img as Bitmap).SetResolution(72F, 72F);
                    return img;
                }
                set
                {
                    lock (imageLock)
                    {
                        value.Tag = ImageMediaType;
                        this.Size = value.Size;
                        WriteImage(value);
                        (value as Bitmap).SetResolution(72F, 72F);
                        //                      this.Size = value.Size;
                        //                        CIAT.SaveFile.ImageManager.AddImageToCache(this);
                        //                    CacheEntryTime = DateTime.Now;
                    }
                    FireChanged(ImageEvent.Updated);
                    CIAT.ImageManager.ReleaseImage(value);
                }
            }


            protected virtual void FireChanged(ImageEvent evt)
            {
                if ((Changed != null) && !CIAT.ImageManager.IsDisposed && !CIAT.ImageManager.IsDisposing)
                {
                    Delegate[] dels = Changed.GetInvocationList();
                    foreach (Delegate d in dels)
                        d.DynamicInvoke(evt, this, null);
                }
            }

            protected virtual void FireChanged(ImageEvent evt, object arg)
            {
                if ((Changed != null) && !CIAT.ImageManager.IsDisposed && !CIAT.ImageManager.IsDisposing)
                {
                    Delegate[] dels = Changed.GetInvocationList();
                    foreach (Delegate d in dels)
                    {
                        if (d.Target != null)
                            d.DynamicInvoke(evt, this, arg);
                    }
                }
            }

            public virtual void Dispose()
            {
                lock (imageLock)
                {
                    IsDisposed = true;
                    Changed = null;
                    DisposeOfImage();
                    CIAT.SaveFile.DeletePart(this.URI);
                    URI = null;
                    PendingWrite = PendingFetch = false;
                }
            }

            public void DisposeOfImage()
            {
                lock (imageLock)
                {
                    if (image != null)
                    {
                        CIAT.ImageManager.ReleaseImage(image);
                        image = null;
                    }
                }
            }

            public virtual object Clone()
            {
                return new ImageMedia(Img, ImageFormat, ImageMediaType);
            }

            public void ClearChanged()
            {
                Changed = null;
            }
        }
    }
}
