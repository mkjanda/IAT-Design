using System;
using System.Collections.Generic;
using System.Management;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.IO.Packaging;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace IATClient.Images
{
    partial class ImageManager : IDisposable
    {
        public enum RequestType { store, retrieve, delete, regenerateThumb, resize };
        private readonly List<int> IDs = new List<int>();
        private readonly SizeF ScreenDPI;
        public static readonly Size ThumbnailSize = new Size(112, 112);
        private String Path;
        private readonly object thumbItrLock = new object(), disposalItrLock = new object();
        private readonly object cacheItrLock = new object(), resizerItrLock = new object(), bitmapBagItrLock = new object();
        private readonly object thumbGenerationLock = new object();
        private readonly object resizerLock = new object();
        private ConcurrentBag<ImageMedia> FetchBag = new ConcurrentBag<ImageMedia>();
        private ConcurrentBag<IImage> ThumbGenerations = new ConcurrentBag<IImage>();
        private ConcurrentBag<Tuple<Image, Size>> ResizerQueue = new ConcurrentBag<Tuple<Image, Size>>();
        private readonly object dictionaryLock = new object(), requestLock = new object(), cacheLock = new object();
        private System.Threading.Timer ThumbGenerationThread = null, FetchThread = null, CacheThread = null, ResizerThread = null, BitmapBagThread = null;
        private Action<object> ThumbGenerationAction, CacheAction, ResizerAction = null, BitmapBagAction = null;
        private bool Halting { get; set; }
        public bool IsDisposing { get; private set; } = false; 
        public bool IsDisposed { get; private set; } = false;
        private ConcurrentBag<ImageMedia> CachedImages = new ConcurrentBag<ImageMedia>();
        public bool WorkersRunning { get; private set; } = false;
        private readonly Dictionary<ImageMediaType, ConcurrentBag<Bitmap>> BitmapBags = new Dictionary<ImageMediaType, ConcurrentBag<Bitmap>>();

        public ImageManager()
        {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            float xDpi = g.DpiX;
            float yDpi = g.DpiY;
            ScreenDPI = new SizeF(xDpi, yDpi);
            g.Dispose();
            BitmapBagAction = new Action<object>((o) =>
            {
                if (!Monitor.TryEnter(bitmapBagItrLock))
                    return;
                Bitmap bmp;
                int sz;
                List<Bitmap> disposalList = new List<Bitmap>();
                foreach (ImageMediaType t in ImageMediaType.All)
                {
                    if (t.IdealAt.AddMilliseconds(t.TicksAtIdeal).CompareTo(DateTime.Now) >= 0)
                        continue;
                    int bagSize = BitmapBags[t].Count;
                    while ((BitmapBags[t].Count > t.IdealSize) && (bagSize - BitmapBags[t].Count <= t.ShrinkRate))
                    {
                        if (BitmapBags[t].TryTake(out bmp))
                            disposalList.Add(bmp);
                    }
                    if (BitmapBags[t].Count == t.IdealSize)
                        t.IdealAt = DateTime.Now;
                }
                Monitor.Exit(bitmapBagItrLock);
                disposalList.ForEach(b => b.Dispose());
            });
            ThumbGenerationAction = new Action<object>((o) =>
            {
                if (!Monitor.TryEnter(thumbItrLock))
                    return;
                ImageManager owner = o as ImageManager;
                List<IImage> thumbGens = new List<IImage>();
                IImage iImg;
                while (ThumbGenerations.TryTake(out iImg))
                    thumbGens.Add(iImg);
                thumbGens = thumbGens.Distinct().ToList();
                foreach (IImage img in thumbGens)
                {
                    if (Halting)
                        return;
                    (img as Image).UpdateThumbnail();
                }
                Monitor.Exit(thumbItrLock);
            });
            CacheAction = new Action<object>((o) =>
            {
                if (!Monitor.TryEnter(cacheItrLock))
                    return;
                ImageMedia iMedia;
                ConcurrentBag<ImageMedia> remainingCachedImages = new ConcurrentBag<ImageMedia>();
                while (CachedImages.TryTake(out iMedia))
                {
                    if (iMedia.CacheExpired)
                    {
                        iMedia.RemoveFromCache();
                        remainingCachedImages.Add(iMedia);
                    }
                    else if (!iMedia.IsDisposed)
                        remainingCachedImages.Add(iMedia);
                }
                while (remainingCachedImages.TryTake(out iMedia))
                    CachedImages.Add(iMedia);
                Monitor.Exit(cacheItrLock);
            });
            ResizerAction = new Action<object>((o) =>
            {
                if (!Monitor.TryEnter(resizerItrLock))
                    return;
                ImageManager owner = (ImageManager)o;
                List<Tuple<Image, Size>> failedResizes = new List<Tuple<Image, Size>>();
                while ((ResizerQueue.TryTake(out Tuple<Image, Size> resize)) && !Halting)
                {
                    if (!CIAT.SaveFile.PartExists(resize.Item1.URI))
                        continue;
                    else if (!resize.Item1.PerformResize(resize.Item2))
                        failedResizes.Add(resize);
                }
                foreach (var failedResize in failedResizes)
                    ResizerQueue.Add(failedResize);
                Monitor.Exit(resizerItrLock);
            });
        }

        public void InvalidateImageBags(bool ideal)
        {
            foreach (ImageMediaType t in ImageMediaType.All)
            {
                ConcurrentBag<Bitmap> b = null;
                if (BitmapBags.ContainsKey(t))
                    b = BitmapBags[t];
                while (BitmapBags[t].TryTake(out Bitmap bmp))
                    bmp.Dispose();
                BitmapBags[t] = new ConcurrentBag<Bitmap>();
                int capacity = ideal ? t.IdealSize : t.MaxSize;
                for (int ctr = 0; ctr < capacity; ctr++)
                {
                    BitmapBags[t].Add(new Bitmap(t.ImageSize.Width, t.ImageSize.Height, PixelFormat.Format32bppArgb) { Tag = t });
                    t.CurrentSize++;
                }
                t.IdealAt = DateTime.Now;
                if (b != null)
                    while (b.TryTake(out Bitmap bmp))
                        bmp.Dispose();
            }
        }

        public void AddImageToCache(ImageMedia iMedia)
        {

            CachedImages.Add(iMedia);
        }

        public void StartWorkers()
        {
            foreach (ImageMediaType t in ImageMediaType.All)
            {
                BitmapBags[t] = new ConcurrentBag<Bitmap>();
                for (int ctr = 0; ctr < t.InitialSize; ctr++)
                    BitmapBags[t].Add(new Bitmap(t.ImageSize.Width, t.ImageSize.Height, PixelFormat.Format32bppArgb) { Tag = t });
            }
            ThumbGenerationThread = new Timer(new TimerCallback(ThumbGenerationAction), this, 1000, 300);
            ResizerThread = new Timer(new TimerCallback(ResizerAction), this, 1500, 250);
            BitmapBagThread = new Timer(new TimerCallback(BitmapBagAction), this, 500, 500);
        }

        public void StopWorkers()
        {
            Halting = true;
            Bitmap bmp;
            foreach (ConcurrentBag<Bitmap> cb in BitmapBags.Values)
            {
                while (cb.TryTake(out bmp))
                    bmp.Dispose();
            }
            ManualResetEvent thumbThread = new ManualResetEvent(false), fetchThread = new ManualResetEvent(false), storeThread = new ManualResetEvent(false),
                cacheThread = new ManualResetEvent(false), resizerThread = new ManualResetEvent(false);
            ThumbGenerationThread?.Dispose(thumbThread);
            CacheThread?.Dispose(cacheThread);
            ResizerThread?.Dispose(resizerThread);
            CachedImages = null;
            FetchBag = null;
            ThumbGenerations = null;
            ResizerQueue = null;
        }

        public void AddToResizer(IImage obj, Size newSize)
        {
            if (IsDisposing)
            {
                obj.Dispose();
                return;
            }
            ResizerQueue.Add(new Tuple<Image, Size>(obj as Image, newSize));
        }

        public System.Drawing.Image FetchImageMedia(ImageMedia iMedia)
        {
            Stream s = CIAT.SaveFile.GetReadStream(iMedia);
            System.Drawing.Image img = null;
            try
            {
                lock (iMedia.lockObj)
                {
                    Stream memStream = new MemoryStream();
                    s.CopyTo(memStream);
                    memStream.Seek(0, SeekOrigin.Begin);
                    img = System.Drawing.Image.FromStream(memStream);
                }
            }
            catch (Exception ex)
            {
                return null;
            }
            finally
            {
                s?.Dispose();
                CIAT.SaveFile.ReleaseReadStreamLock();
            }
            if (iMedia.ImageMediaType == ImageMediaType.VariableSize)
                return img;
            if (!iMedia.ImageMediaType.ImageSize.Equals(img.Size))
            {
                if (iMedia is Image)
                {
                    AddToResizer(iMedia as Image, iMedia.ImageMediaType.ImageSize);
                    return img;
                }
                Bitmap bmp = RequestBitmap(iMedia.ImageMediaType);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                    g.FillRectangle(backBr, 0, 0, bmp.Width, bmp.Height);
                    g.DrawImage(img, 0, 0);
                    bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                    backBr.Dispose();
                }
                MemoryStream sizedStream = new MemoryStream();
                bmp.Save(sizedStream, iMedia.ImageFormat.Format);
                bmp.Dispose();
            }
            return img;
        }

        public void GenerateThumb(IImage img)
        {
            if (IsDisposing)
            {
                img.Dispose();
                return;
            }
            lock (ThumbGenerations)
            {
                ThumbGenerations.Add(img as Image);
            }
        }


        public Bitmap RequestBitmap(ImageMediaType t)
        {
            if (t == ImageMediaType.VariableSize)
                return null;
            try
            {
                if (!BitmapBags.ContainsKey(t))
                    BitmapBags[t] = new ConcurrentBag<Bitmap>();
                if (BitmapBags[t].TryTake(out Bitmap bmp))
                    return bmp;
            }
            catch (KeyNotFoundException)
            {
                BitmapBags[t] = new ConcurrentBag<Bitmap>();
            }
            lock (t.lockObject)
            {
                if (BitmapBags[t].Count < t.GrowRate)
                {
                    for (int ctr = 0; ctr < t.GrowRate - 1; ctr++)
                    {
                        BitmapBags[t].Add(new Bitmap(t.ImageSize.Width, t.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb) { Tag = t });
                    }
                    if (BitmapBags[t].Count >= t.IdealSize)
                        t.IdealAt = DateTime.Now;
                }
                return new Bitmap(t.ImageSize.Width, t.ImageSize.Height, PixelFormat.Format32bppArgb) { Tag = t };
            }
        }

        public void ReleaseImage(System.Drawing.Image bmp)
        {
            if (bmp is Bitmap)
            {
                if (bmp.Tag == null)
                    bmp.Dispose();
                if ((bmp.Tag as ImageMediaType) == ImageMediaType.Null)
                    return;
                else if (bmp.PixelFormat != System.Drawing.Imaging.PixelFormat.Format32bppArgb)
                    bmp.Dispose();
                else if ((bmp.Tag as ImageMediaType) == ImageMediaType.VariableSize)
                    bmp.Dispose();
                else if (!bmp.Size.Equals((bmp.Tag as ImageMediaType).ImageSize))
                    bmp.Dispose();
                else if (bmp.Tag is ImageMediaType)
                {
                    ImageMediaType t = bmp.Tag as ImageMediaType;
                    if (!BitmapBags.ContainsKey(t))
                        BitmapBags.Add(t, new ConcurrentBag<Bitmap>());
                    if (BitmapBags[t].Count >= t.MaxSize)
                        bmp.Dispose();
                    else
                        BitmapBags[t].Add(bmp as Bitmap);
                }
                else
                    bmp.Dispose();
            }
            else
                bmp?.Dispose();

        }

        public void Dispose()
        {
            if (IsDisposing || IsDisposed)
                return;
            IsDisposing = true;
            StopWorkers();
            IsDisposed = true;
        }
    }
}
