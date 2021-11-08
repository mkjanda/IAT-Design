using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Reflection.Emit;

namespace IATClient.Images
{
    public class ImageChangedEvent : Enumeration
    {
        public static readonly ImageChangedEvent Stored = new ImageChangedEvent(1, "Stored");
        public static readonly ImageChangedEvent Resized = new ImageChangedEvent(2, "Resized");
        public static readonly ImageChangedEvent Deleted = new ImageChangedEvent(3, "Deleted");
        public static readonly ImageChangedEvent Updated = new ImageChangedEvent(4, "Updated");
        public static readonly ImageChangedEvent Fetched = new ImageChangedEvent(5, "Fetched");
        public static readonly ImageChangedEvent ThumbnailGenerated = new ImageChangedEvent(6, "ThumbnailGenerated");
        public static readonly ImageChangedEvent Initialized = new ImageChangedEvent(7, "Initialized");
        public static readonly ImageChangedEvent ResizeUpdate = new ImageChangedEvent(8, "ResizeUpdate");
        public static readonly ImageChangedEvent ResizeNotNeeded = new ImageChangedEvent(9, "ResizeNotNeeded");
        private ImageChangedEvent(int val, String name) : base(val, name) { }
        private static readonly IEnumerable<ImageChangedEvent> All =
            new ImageChangedEvent[] { Stored, Resized, Deleted, Updated, Fetched, ThumbnailGenerated, Initialized };
        public static ImageChangedEvent Parse(String name)
        {
            return All.Where(ice => ice.Name == name).FirstOrDefault();
        }
    }

    public class ImageMediaType : Enumeration
    {
        public static readonly ImageMediaType FullWindow = new ImageMediaType(1, "FullWindow", "image/octet-stream+full-window")
        {
            InitialSize = 10,
            IdealSize = 10,
            MaxSize = 50,
            GrowRate = 25,
            ShrinkRate = 25,
            GetSize = () => CIAT.SaveFile.Layout.InteriorSize,
            DITypes = new DIType[] { DIType.LambdaGenerated, DIType.Preview }
        };
        public static readonly ImageMediaType ResponseKey = new ImageMediaType(2, "ResponseKey", "image/octet-stream+response-key")
        {
            InitialSize = 0,
            IdealSize = 0,
            MaxSize = 30,
            GrowRate = 5,
            ShrinkRate = 5,
            GetSize = () => CIAT.SaveFile.Layout.KeyValueSize,
            DITypes = new DIType[] { DIType.DualKey, DIType.ResponseKeyText, DIType.ResponseKeyImage, DIType.Conjunction }
        };
        public static readonly ImageMediaType ResponseKeyOutline = new ImageMediaType(3, "ResponseKeyOutline", "image/octet-stream+response-key-outline")
        {
            InitialSize = 0,
            IdealSize = 2,
            MaxSize = 10,
            GrowRate = 1,
            ShrinkRate = 1,
            GetSize = () => CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle.Size,
            DITypes = new DIType[] { DIType.LeftKeyValueOutline, DIType.RightKeyValueOutline }
        };
        public static readonly ImageMediaType Stimulus = new ImageMediaType(4, "Stimulus", "image/octet-stream+stimulus")
        {
            InitialSize = 5,
            IdealSize = 5,
            MaxSize = 60,
            GrowRate = 15,
            ShrinkRate = 15,
            GetSize = () => CIAT.SaveFile.Layout.StimulusSize,
            DITypes = new DIType[] { DIType.StimulusImage, DIType.StimulusText }
        };
        public static readonly ImageMediaType ErrorMark = new ImageMediaType(5, "ErrorMark", "image/octet-stream+error-mark")
        {
            InitialSize = 0,
            IdealSize = 1,
            MaxSize = 10,
            GrowRate = 1,
            ShrinkRate = 1,
            GetSize = () => CIAT.SaveFile.Layout.ErrorSize,
            DITypes = new DIType[] { DIType.ErrorMark }
        };
        public static readonly ImageMediaType BlockInstructions = new ImageMediaType(6, "BlockInstructions", "image/octet-stream+block-instructions")
        {
            InitialSize = 10,
            IdealSize = 10,
            MaxSize = 50,
            GrowRate = 20,
            ShrinkRate = 20,
            GetSize = () => CIAT.SaveFile.Layout.InstructionsSize,
            DITypes = new DIType[] { DIType.IatBlockInstructions }
        };
        public static readonly ImageMediaType TextInstructionScreen = new ImageMediaType(7, "TextInstructionScreen", "image/octet-stream+text-instruction-screen")
        {
            InitialSize = 0,
            IdealSize = 0,
            MaxSize = 50,
            GrowRate = 20,
            ShrinkRate = 20,
            GetSize = () => CIAT.SaveFile.Layout.InstructionScreenTextAreaRectangle.Size,
            DITypes = new DIType[] { DIType.TextInstructionsScreen }
        };
        public static readonly ImageMediaType KeyedInstructionScreen = new ImageMediaType(8, "KeyedInstructionScreen", "image/octet-stream+keyed-instruction-screen")
        {
            InitialSize = 0,
            IdealSize = 0,
            MaxSize = 50,
            GrowRate = 20,
            ShrinkRate = 20,
            GetSize = () => CIAT.SaveFile.Layout.KeyInstructionScreenTextAreaRectangle.Size,
            DITypes = new DIType[] { DIType.KeyedInstructionsScreen }
        };
        public static readonly ImageMediaType MockItemInstructions = new ImageMediaType(9, "MockItemInstructions", "image/octet-stream+mock-item-instructions")
        {
            InitialSize = 0,
            IdealSize = 0,
            MaxSize = 50,
            GrowRate = 20,
            ShrinkRate = 20,
            GetSize = () => CIAT.SaveFile.Layout.MockItemInstructionsRectangle.Size,
            DITypes = new DIType[] { DIType.MockItemInstructions }
        };
        public static readonly ImageMediaType ContinueInstructions = new ImageMediaType(10, "ContinueInstructions", "image/octet-stream+continue-instructions")
        {
            InitialSize = 0,
            IdealSize = 0,
            MaxSize = 30,
            GrowRate = 10,
            ShrinkRate = 10,
            GetSize = () => CIAT.SaveFile.Layout.ContinueInstructionsRectangle.Size,
            DITypes = new DIType[] { DIType.ContinueInstructions }
        };
        public static readonly ImageMediaType VariableSize = new ImageMediaType(11, "VariableSize", "image/octet-stream+variable-size")
        {
            InitialSize = 0,
            IdealSize = 0,
            MaxSize = 0,
            GrowRate = 0,
            ShrinkRate = 0,
            GetSize = () => Size.Empty,
            DITypes = new DIType[] { DIType.SurveyImage }
        };
        public static readonly ImageMediaType Thumbnail = new ImageMediaType(12, "Thumbnail", "image/octet-stream+thumbnail")
        {
            InitialSize = 0,
            IdealSize = 0,
            MaxSize = 60,
            GrowRate = 15,
            ShrinkRate = 15,
            GetSize = () => ImageManager.ThumbnailSize,
            DITypes = new DIType[] { }
        };
        public static readonly ImageMediaType Null = new ImageMediaType(13, "Null", "image/octet-stream+null")
        {
            InitialSize = 1,
            IdealSize = 1,
            MaxSize = 3,
            GrowRate = 1,
            ShrinkRate = 1,
            GetSize = () => new Size(1, 1),
            DITypes = new DIType[] { DIType.Null }
        };
        public static readonly ImageMediaType FullPreview = new ImageMediaType(14, "FullPreview", "image/octet-stream+full-preview")
        {
            InitialSize = 10,
            IdealSize = 10,
            MaxSize = 150,
            GrowRate = 25,
            ShrinkRate = 25,
            GetSize = () =>
            {
                double arPreview = (double)CIAT.SaveFile.Layout.InteriorSize.Width / (double)CIAT.SaveFile.Layout.InteriorSize.Height;
                if (arPreview > 1)
                    return new Size(500, (int)((double)500/ (double)arPreview));
                else
                    return new Size((int)(500.0 * arPreview), 500);

            },
            DITypes = new DIType[] { }
        };
        public static readonly ImageMediaType ResponseKeyPreview = new ImageMediaType(15, "ResponseKeyPreview", "image/octet-stream+response-key-preview")
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
                    return new Size(254, (int)(254.0 / arPreview));
                else
                    return new Size((int)(156.0 * arPreview), 156);

            },
            DITypes = new DIType[] { }
        };
        public String MimeType { get; private set; }
        public int InitialSize { get; private set; }
        public int IdealSize { get; private set; }
        public int GrowRate { get; private set; }
        public int ShrinkRate { get; private set; }
        public int MaxSize { get; private set; }
        public int LastSize { get; set; } = 0;
        public int CurrentSize { get; set; } = 0;
        public long TicksAtIdeal { get; private set; } = 10000;
        public DateTime IdealAt { get; set; } = DateTime.Now;
        private IEnumerable<DIType> DITypes { get; set; }
        private Func<Size> GetSize { get; set; }
        public Size ImageSize
        {
            get
            {
                return GetSize();
            }
        }
        private ImageMediaType(int val, String name, String mimeType) : base(val, name)
        {
            MimeType = mimeType;
        }
        public static readonly IEnumerable<ImageMediaType> All = new ImageMediaType[] { FullWindow, ResponseKey, ResponseKeyOutline, Stimulus,
            ErrorMark, BlockInstructions, TextInstructionScreen, KeyedInstructionScreen, MockItemInstructions, ContinueInstructions,
            VariableSize, Thumbnail, Null, FullPreview, ResponseKeyPreview
        };
        public static ImageMediaType FromMimeType(String mimeType)
        {
            return All.Where(o => o.MimeType == mimeType).FirstOrDefault();
        }
        public static ImageMediaType FromDIType(DIType t)
        {
            return All.Where(imt => imt.DITypes.Contains(t)).FirstOrDefault();
        }
    }

    public class ImageChangedEventArgs
    {
        public IImageMedia Image { get; private set; }
        public ImageChangedEvent Event { get; private set; }
        public object Arg { get; private set; }
        public ImageChangedEventArgs(IImageMedia image, ImageChangedEvent e, object o)
        {
            Image = image;
            Event = e;
            Arg = o;
        }
    }

    public interface IImageMedia : IDisposable, IPackagePart, ICloneable
    {
        System.Drawing.Image Image { get; set; }
        Size Size { get; }
        System.Drawing.Imaging.ImageFormat Format { get; set; }
        String FileExtension { get; }
        event Action<ImageChangedEvent, IImageMedia, object> Changed;
        void ClearChanged();
    }

    partial class ImageManager
    {
        public class ImageMedia : IImageMedia
        {
            private const long CacheDelay = 100;
            public bool IsDisposed { get; protected set; } = false;
            public bool IsCached { get; set; } = false;
            private readonly ConcurrentQueue<System.Drawing.Image> ImagesQueuedForCache = new ConcurrentQueue<System.Drawing.Image>();
            public Uri URI { get; set; }
            private Size _Size = Size.Empty;
            private System.Drawing.Imaging.ImageFormat _Format = null;
            public System.Drawing.Imaging.ImageFormat Format
            {
                get
                {
                    if (_Format == null)
                        return ImageFormat.FromExtension(FileExtension).Format;
                    return _Format;
                }
                set
                {
                    _Format = value;
                }
            }
            private System.Drawing.Image image = null;
            public bool PendingFetch { get; private set; } = false;
            public bool PendingWrite { get; private set; } = false;
            public ReaderWriterLockSlim ImageReadWriteLock = new ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion);
            public virtual bool PendingResize { get { return false; } protected set { } }
            public readonly object lockObj = new object();
            public ManualResetEventSlim pendingFetchEvent = new ManualResetEventSlim(true), pendingWriteEvent = new ManualResetEventSlim(true);
            public ManualResetEventSlim CachedEvent { get; private set; } = new ManualResetEventSlim(false);
            public String MimeType { get; protected set; } = null;
            public event Action<ImageChangedEvent, IImageMedia, object> Changed = null;
            public DateTime CacheEntryTime { get; set; } = DateTime.MinValue;
            protected bool IsNull { get; set; } = false;
            protected static Action<IntPtr, byte, int> Memset;
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
                IsNull = true;
            }

            public ImageMedia(Uri uri, Size sz, System.Drawing.Imaging.ImageFormat format)
            {
                URI = uri;
                MimeType = CIAT.SaveFile.GetMimeType(URI);
                this.Size = sz;
                Format = format;
            }

            public ImageMedia(System.Drawing.Imaging.ImageFormat format, ImageMediaType type)
            {
                MimeType = type.MimeType;
                URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + format.ToString());
                IsNull = true;
                this.Size = type.ImageSize;
                Format = format;
            }

            public ImageMedia(System.Drawing.Image img, System.Drawing.Imaging.ImageFormat format, ImageMediaType type)
            {
                MimeType = type.MimeType;
                URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + format.ToString());
                this.Size = img.Size;
                Format = format;
                Image = img;
                IsNull = false;
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
                    if (_Format == null)
                        return "png";
                    if (Format == System.Drawing.Imaging.ImageFormat.Bmp)
                        return "bmp";
                    else if (Format == System.Drawing.Imaging.ImageFormat.Gif)
                        return "gif";
                    else if (Format == System.Drawing.Imaging.ImageFormat.Jpeg)
                        return "jpg";
                    else if (Format == System.Drawing.Imaging.ImageFormat.Tiff)
                        return "tiff";
                    return "png";
                }
            }

            public Size Size
            {
                get
                {
                    if (_Size.IsEmpty)
                        return ImageMediaType.FromMimeType(MimeType).ImageSize;
                    else if ((_Size.Width == 0) && (_Size.Height == 0))
                        return ImageMediaType.FromMimeType(MimeType).ImageSize;
                    return _Size;
                }
                protected set
                {
                    _Size = value;
                }
            }


            public System.Drawing.Image CreateCopy(System.Drawing.Image img)
            {
                ImageReadWriteLock.EnterUpgradeableReadLock();
                try
                {
                    if (img == null)
                        return null;
                    System.Drawing.Image clone = img.Clone() as System.Drawing.Image;
                    clone.Tag = img.Tag;
                    return clone;
                }
                finally
                {
                    ImageReadWriteLock.ExitUpgradeableReadLock();
                }
            }

            public virtual System.Drawing.Image Image
            {
                get
                {
                    if (IsDisposed)
                        return null;
                    if (IsNull)
                        return CIAT.ImageManager.RequestBitmap(ImageMediaType.FromMimeType(MimeType));
                    CIAT.ImageManager.FetchImageMedia(this);
                    while (!CachedEvent.Wait(250))
                        CIAT.ImageManager.FetchImageMedia(this); 
                    return CreateCopy(image);
                }
                set
                {
                    IsNull = value == null;
                    if (IsNull)
                    {
                        DisposeOfImage();
                        return;
                    }
                    if (image != null)
                        value.Tag = image.Tag;
                    else
                        value.Tag = ImageMediaType.FromMimeType(CIAT.SaveFile.GetMimeType(URI));
                    ImageUpdate(value);
                }
            }

            protected readonly object ImageUpdateLock = new object();

            protected void ResizeUpdate(System.Drawing.Image val)
            {
                val.Tag = ImageMediaType.FromMimeType(CIAT.SaveFile.GetMimeType(URI));
                ImageReadWriteLock.EnterWriteLock();
                try
                {
                    DisposeOfImage();
                    image = val;
                    CIAT.ImageManager.CacheImage(this);
                }
                finally
                {
                    ImageReadWriteLock.ExitWriteLock();
                }
                FireChanged(ImageChangedEvent.Updated);
            }

            private void ImageUpdate(System.Drawing.Image val)
            {
                ImageReadWriteLock.EnterWriteLock();
                try
                {
                    DisposeOfImage();
                    image = val;
                    this.Size = val.Size;
                    CIAT.ImageManager.CacheImage(this);
                }
                finally
                {
                    ImageReadWriteLock.ExitWriteLock();
                }
                FireChanged(ImageChangedEvent.Updated);
            }

            protected void FireChanged(ImageChangedEvent evt)
            {
                if ((Changed != null) && !CIAT.ImageManager.IsDisposed && !CIAT.ImageManager.IsDisposing)
                {
                    Delegate[] dels = Changed.GetInvocationList();
                    foreach (Delegate d in dels)
                        Task.Run(() => d.DynamicInvoke(evt, this, null));
                }
            }

            protected void FireChanged(ImageChangedEvent evt, object arg)
            {
                if ((Changed != null) && !CIAT.ImageManager.IsDisposed && !CIAT.ImageManager.IsDisposing)
                {
                    Delegate[] dels = Changed.GetInvocationList();
                    foreach (Delegate d in dels)
                        d.DynamicInvoke(evt, this, arg);
                }
            }

            public void Fetched(System.Drawing.Image img)
            {
                DisposeOfImage();
                image = img;
                CIAT.ImageManager.CacheImage(this);
                pendingFetchEvent.Set();
                PendingFetch = false;
            }

            public virtual void Written()
            {
                PendingWrite = false;
                FireChanged(ImageChangedEvent.Stored);
            }

            public virtual void Dispose()
            {
                ImageReadWriteLock.EnterWriteLock();
                try
                {
                    if (IsDisposed)
                        return;
                    IsDisposed = true;
                    if (PendingFetch)
                        pendingFetchEvent.Wait(500);
                    if (PendingWrite)
                        pendingWriteEvent.Wait(500);
                    Changed = null;
                    DisposeOfImage();
                    CIAT.SaveFile.DeletePart(this.URI);
                    PendingWrite = PendingFetch = false;
                }
                finally
                {
                    ImageReadWriteLock.ExitWriteLock();
                }
            }

            public bool DisposeOfImage()
            {
                ImageReadWriteLock.EnterWriteLock();
                try { 
                    CIAT.ImageManager.RemoveFromCache(this);
                    if (image != null) 
                        CIAT.ImageManager.ReleaseImage(image);
                    image = null;
                    return true;
                }
                finally
                {
                    ImageReadWriteLock.ExitWriteLock();
                }
            }

            public virtual object Clone()
            {
                Uri URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, "." + Format.ToString());
                ImageMedia o = new ImageMedia();
                o.URI = URI;
                o.MimeType = MimeType;
                o.Format = Format;
                o.Size = Size;
                o.Image = Image;
                return o;
            }

            public void ClearChanged()
            {
                Changed = null;
            }
        }
    }
}
