using System;
using System.Collections.Generic;
using System.Management;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace IATClient.Images
{
    partial class ImageManager : IDisposable
    {
        public readonly static int DefaultCachePeriod = 5000;
        public enum RequestType { store, retrieve, delete, regenerateThumb, resize };
        private readonly List<int> IDs = new List<int>();
        private readonly SizeF ScreenDPI;
        public static readonly Size ThumbnailSize = new Size(112, 112);
        private String Path;
        private readonly object fetchItrLock = new object(), storeItrLock = new object(), thumbItrLock = new object(), disposalItrLock = new object();
        private readonly object cacheItrLock = new object(), resizerItrLock = new object(), bitmapBagItrLock = new object();
        private ConcurrentBag<ImageMedia> FetchBag = new ConcurrentBag<ImageMedia>(), DisposalBag = new ConcurrentBag<ImageMedia>();
        private ConcurrentBag<IImage> ThumbGenerations = new ConcurrentBag<IImage>();
        private ConcurrentBag<ImageMedia> StoreBag = new ConcurrentBag<ImageMedia>();
        private bool FetchesWaitingOnWrite = false;
        private ConcurrentBag<Tuple<Image, Size>> ResizerQueue = new ConcurrentBag<Tuple<Image, Size>>();
        private readonly object dictionaryLock = new object(), requestLock = new object(), cacheLock = new object(), resizerLock = new object();
        private System.Threading.Timer ThumbGenerationThread = null, FetchThread = null, StoreThread = null, DisposalThread = null, CacheThread = null, ResizerThread = null, BitmapBagThread = null;
        private Action<object> ThumbGenerationAction, FetchAction, StoreAction, DisposalAction, CacheAction, ResizerAction = null, BitmapBagAction = null;
        private bool Halting { get; set; }
        public bool IsDisposing { get; private set; } = false; 
        public bool IsDisposed { get; private set; } = false;
        private ConcurrentDictionary<ImageMedia, DateTime> Cache = new ConcurrentDictionary<ImageMedia, DateTime>();
        public int CachePeriod { get; set; } = DefaultCachePeriod;
        public bool WorkersRunning { get; private set; } = false;
        private readonly Dictionary<ImageMediaType, ConcurrentBag<Bitmap>> BitmapBags = new Dictionary<ImageMediaType, ConcurrentBag<Bitmap>>();

        public void CacheImage(ImageMedia imgMedia)
        {
            lock (imgMedia.lockObj)
            {
                if (!Cache.TryAdd(imgMedia, DateTime.Now))
                {
                    Cache.TryRemove(imgMedia, out DateTime dummy);
                    Cache.TryAdd(imgMedia, DateTime.Now);
                }
                imgMedia.CacheEntryTime = DateTime.Now;
                imgMedia.IsCached = true;
                imgMedia.CachedEvent.Set();
            }
        }


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
                foreach (ImageMediaType t in ImageMediaType.All)
                {
                    if (t.IdealAt.AddMilliseconds(t.TicksAtIdeal).CompareTo(DateTime.Now) >= 0)
                        continue;
                    /*
                    t.CurrentSize = BitmapBags[t].Count();
                    if ((t.CurrentSize > t.IdealSize) && (t.LastSize <= t.CurrentSize))
                    {
                        t.LastSize = t.CurrentSize;
                        for (int ctr = 0; ctr < Math.Min(t.ShrinkRate, t.LastSize - t.IdealSize); ctr++)
                            if (BitmapBags[t].TryTake(out bmp))
                            {
                                bmp.Dispose();
                                t.CurrentSize--;
                            }
                        if (t.CurrentSize <= t.IdealSize + (t.ShrinkRate >> 1))
                            t.IdealAt = DateTime.Now;
                    } else if ((t.CurrentSize > t.IdealSize) && (t.CurrentSize > t.InitialSize) && (t.IdealAt.Ticks + t.TicksAtIdeal < DateTime.Now.Ticks)) {
                        while (t.CurrentSize > t.IdealSize)
                            if (BitmapBags[t].TryTake(out bmp))
                            {
                                bmp.Dispose();
                                t.CurrentSize--;
                            }
                        t.IdealAt = DateTime.Now;
                    }
                    t.LastSize = BitmapBags[t].Count();
                    */
                    while (BitmapBags[t].Count > t.IdealSize)
                    {
                        if (BitmapBags[t].TryTake(out bmp))
                            bmp.Dispose();
                    }
                }
                Monitor.Exit(bitmapBagItrLock);
            });
            ThumbGenerationAction = new Action<object>((o) =>
            {
                if (!Monitor.TryEnter(thumbItrLock))
                    return;
                ImageManager owner = o as ImageManager;
                List<IImage> thumbGens = new List<IImage>();
                ThumbGenerations.TryTake(out IImage im);
                while (im != null) {
                    thumbGens.Add(im);
                    ThumbGenerations.TryTake(out im);
                }
                foreach (Uri u in thumbGens.Select(i => i.URI).Distinct())
                {
                    if (Halting)
                        return;
                    try
                    {
                        Image resizing = CIAT.SaveFile.GetIImage(u) as Image;
                        resizing.UpdateThumbnail();
                    }
                    catch (KeyNotFoundException ex) { }
                }
                Monitor.Exit(thumbItrLock);
            });
            FetchAction = new Action<object>((o) =>
            {
                if (!Monitor.TryEnter(fetchItrLock))
                    return;
                ImageManager owner = o as ImageManager;
                List<ImageMedia> failedFetches = new List<ImageMedia>();
                List<ImageMedia> successfulFetches = new List<ImageMedia>();
                FetchBag.TryTake(out ImageMedia im);
                while (im != null)
                {
                    if (successfulFetches.Contains(im))
                    {
                        FetchBag.TryTake(out im);
                        continue;
                    }
                    if (!owner.BackgroundFetchImage(im))
                    {
                        if (!Halting)
                            failedFetches.Add(im);
                    }
                    else
                        successfulFetches.Add(im);
                    FetchBag.TryTake(out im);
                }
                if (failedFetches.Count > 0)
                    FetchesWaitingOnWrite = true;
                else
                    FetchesWaitingOnWrite = false;
                foreach (ImageMedia failed in failedFetches)
                    FetchBag.Add(failed);
                Monitor.Exit(fetchItrLock);
            });
            StoreAction = new Action<object>((o) =>
            {
                if (!Monitor.TryEnter(storeItrLock))
                    return;
                ImageManager owner = o as ImageManager;
                while (StoreBag.TryTake(out ImageMedia iMedia))
                {
                    if ((iMedia != null) && !Halting)
                        owner.BackgroundWriteImage(iMedia);
                }
                Monitor.Exit(storeItrLock);
            });
            DisposalAction = new Action<object>((o) =>
            {
                if (!Monitor.TryEnter(disposalItrLock))
                    return;
                ImageManager owner = o as ImageManager;
                DisposalBag.TryTake(out ImageMedia im);
                while ((im != null) && !Halting)
                {
                    owner.BackgroundDeleteImage(im);
                    DisposalBag.TryTake(out im);
                }
                Monitor.Exit(disposalItrLock);
            });
            CacheAction = new Action<object>((o) =>
            {
                if (!Monitor.TryEnter(cacheItrLock))
                    return;
                List<Tuple<ImageMedia, DateTime>> cachedImages;
                lock (cacheLock)
                {
                    cachedImages = Cache.Select(kv => new Tuple<ImageMedia, DateTime>(kv.Key, kv.Value)).ToList();
                }
                foreach (Tuple<ImageMedia, DateTime> tup in cachedImages)
                {
                    if (!Monitor.TryEnter(tup.Item1.lockObj))
                        continue;
                    if (tup.Item2 == DateTime.MinValue)
                    {
                        tup.Item1.DisposeOfImage();
                        Monitor.Exit(tup.Item1.lockObj);
                        continue;
                    }
                    DateTime cTime = tup.Item2.AddMilliseconds(CachePeriod);
                    if ((cTime.CompareTo(DateTime.Now) < 0) && !tup.Item1.PendingWrite && !tup.Item1.PendingFetch && !tup.Item1.PendingResize)
                    {
                        System.Drawing.Image img = tup.Item1.Image;
                        if (Cache.TryRemove(tup.Item1, out DateTime dummy))
                        {
                            if (img == null)
                                continue;
                            tup.Item1.CachedEvent.Reset();
                            if (!tup.Item1.IsDisposed)
                            {
                                tup.Item1.ImageReadWriteLock.EnterWriteLock();
                                Stream s = CIAT.SaveFile.GetWriteStream(tup.Item1);
                                Stream syncStream = Stream.Synchronized(s);
                                img.Save(syncStream, tup.Item1.Format);
                                syncStream.Dispose();
                                CIAT.SaveFile.ReleaseWriteStreamLock();
                                ReleaseImage(img);
                                tup.Item1.DisposeOfImage();
                                tup.Item1.IsCached = false;
                                tup.Item1.ImageReadWriteLock.ExitWriteLock();
                            }
                        }
                    }
                    Monitor.Exit(tup.Item1.lockObj);
                }
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
                t.CurrentSize = 0;
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

        public void FlushCache()
        {
            lock (cacheLock)
            {
                foreach (var cachedImage in Cache.Select(kv => new { im = kv.Key, dt = kv.Value }).ToList())
                {
                    System.Drawing.Image img = cachedImage.im.Image;
                    if ((img.Tag as ImageMediaType) != ImageMediaType.FullWindow)
                    {
                        {
                            Stream s = CIAT.SaveFile.GetWriteStream(cachedImage.im);
                            Stream syncStream = Stream.Synchronized(s);
                            img.Save(syncStream, cachedImage.im.Format);
                            syncStream.Dispose();
                            CIAT.SaveFile.ReleaseWriteStreamLock();
                        }
                        Cache.TryUpdate(cachedImage.im, DateTime.MinValue, cachedImage.dt);
                    }
                }
            }
        }

        public void StartWorkers()
        {
            foreach (ImageMediaType t in ImageMediaType.All)
            {
                BitmapBags[t] = new ConcurrentBag<Bitmap>();
                for (int ctr = 0; ctr < t.InitialSize; ctr++)
                    BitmapBags[t].Add(new Bitmap(t.ImageSize.Width, t.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb) { Tag = t });
                t.CurrentSize = t.InitialSize;
            }
            ThumbGenerationThread = new Timer(new TimerCallback(ThumbGenerationAction), this, 1000, 300);
            FetchThread = new Timer(new TimerCallback(FetchAction), this, 1100, 25);
            StoreThread = new Timer(new TimerCallback(StoreAction), this, 1200, 250);
            DisposalThread = new Timer(new TimerCallback(DisposalAction), this, 1300, 1000);
            CacheThread = new Timer(new TimerCallback(CacheAction), this, 1400, 1000);
            ResizerThread = new Timer(new TimerCallback(ResizerAction), this, 1500, 250);
            BitmapBagThread = new Timer(new TimerCallback(BitmapBagAction), this, 1000, 1000);
        }

        public void StopWorker()
        {
 //           CIAT.SaveFile.LayoutPreviews();
            Halting = true;
            Bitmap bmp;
            foreach (ConcurrentBag<Bitmap> cb in BitmapBags.Values)
            {
                while (cb.TryTake(out bmp))
                    bmp.Dispose();
            }
            ManualResetEvent thumbThread = new ManualResetEvent(false), fetchThread = new ManualResetEvent(false), storeThread = new ManualResetEvent(false),
                disposalThread = new ManualResetEvent(false), cacheThread = new ManualResetEvent(false), resizerThread = new ManualResetEvent(false);
            ThumbGenerationThread.Dispose(thumbThread);
            FetchThread.Dispose(fetchThread);
            StoreThread.Dispose(storeThread);
            DisposalThread.Dispose(disposalThread);
            CacheThread.Dispose(cacheThread);
            ResizerThread.Dispose(resizerThread);
            thumbThread.WaitOne();
            fetchThread.WaitOne();
            storeThread.WaitOne();
            disposalThread.WaitOne();
            cacheThread.WaitOne();
            resizerThread.WaitOne();
            lock (cacheLock)
            {
                foreach (ImageMedia iMedia in Cache.Keys)
                    iMedia.DisposeOfImage();
            }
            while (FetchBag.TryTake(out ImageMedia iMedia));
            while (StoreBag.TryTake(out ImageMedia iMedia));
            while (ThumbGenerations.TryTake(out IImage iImg));
            while (DisposalBag.TryTake(out ImageMedia iMedia));
            while (ResizerQueue.TryTake(out Tuple<Image, Size> tup));
            Cache.Clear();
        }

        public void ForceFetch()
        {
            FetchAction.Invoke(this);
        }

        public void AddToResizer(IImage obj, Size newSize)
        {
            if (IsDisposing)
            {
                obj.Dispose();
                return;
            }
            lock (resizerLock)
            {
                ResizerQueue.Add(new Tuple<Image, Size>(obj as Image, newSize));
            }
        }

        public void FetchImageMedia(ImageMedia img)
        {
            lock (img.lockObj)
            {
                if (img.IsCached)
                    return;
                if (IsDisposing)
                    return;
                if (!FetchBag.Contains(img))
                    FetchBag.Add(img);
            }
        }

        public void RemoveFromCache(ImageMedia iMedia)
        {
            if (Cache.TryRemove(iMedia, out DateTime dummy))
                iMedia.CachedEvent.Reset();
            iMedia.IsCached = false;
            iMedia.CacheEntryTime = DateTime.MinValue;
        }

        public void GenerateThumb(IImage img)
        {
            if (IsDisposing)
            {
                img.Dispose();
                return;
            }
            ThumbGenerations.Add(img as Image);
        }

        public IImage CreateImage(System.Drawing.Imaging.ImageFormat format, DIType diType)
        {
            if (IsDisposing)
                return null;
            return new Image(format, diType);
        }

        public IImage CreateImage(System.Drawing.Image img, System.Drawing.Imaging.ImageFormat format, DIType diType, Action<Images.ImageChangedEvent, IImageMedia, object> onChanged)
        {
            if (IsDisposing)
            {
                ReleaseImage(img);
                return null;
            }
            Image imgObj = new Image(img, format, diType, onChanged);
            return imgObj;
        }

        private void BackgroundWriteImage(ImageMedia iMedia)
        {
            if (iMedia.IsDisposed)
            {
                iMedia.Written();
                return;
            }
            String mimeType = iMedia.MimeType;
            System.Drawing.Image img = null;
            Stream s = null;
            try
            {
                img = iMedia.Image;
                s = CIAT.SaveFile.GetWriteStream(iMedia);
                Stream syncPackageStream = Stream.Synchronized(s);
                img.Save(syncPackageStream, iMedia.Format);
                syncPackageStream.Dispose();
                CIAT.SaveFile.ReleaseWriteStreamLock();
                s = null;
                ReleaseImage(img);
                iMedia.Written();
                iMedia.pendingWriteEvent.Set();
            }
            catch {
                if (img != null)
                    ReleaseImage(img);
                iMedia.Written();
                iMedia.pendingWriteEvent.Set();
            }
            if (s != null) {
                s.Dispose();
                CIAT.SaveFile.ReleaseWriteStreamLock();
            }

        }

        private bool BackgroundFetchImage(ImageMedia tImage)
        {
            if (tImage.IsCached)
            {
                CacheImage(tImage);
                return true;
            }
            Stream memStream = new MemoryStream();
            try
            {
                Stream s = CIAT.SaveFile.GetReadStream(tImage);
                Stream syncPackageStream = Stream.Synchronized(s);
                syncPackageStream.CopyTo(memStream);
                memStream.Seek(0, SeekOrigin.Begin);
                syncPackageStream.Dispose();
                CIAT.SaveFile.ReleaseReadStreamLock();
                System.Drawing.Image img = System.Drawing.Image.FromStream(memStream);
                Bitmap bmp = RequestBitmap(ImageMediaType.FromMimeType(CIAT.SaveFile.GetMimeType(tImage.URI)));
                if (bmp == null)
                    bmp = new Bitmap(img) { Tag = ImageMediaType.VariableSize };
                else
                {
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                        g.FillRectangle(backBr, 0, 0, bmp.Width, bmp.Height);
                        g.DrawImage(img, 0, 0);
                        bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                        backBr.Dispose();
                    }
                }
                img.Dispose();
                tImage.Fetched(bmp);
            }
            finally
            {
                memStream.Dispose();
            }
            return true;
        }


        public Bitmap RequestBitmap(ImageMediaType t)
        {
            if (t == ImageMediaType.VariableSize)
                return null;
            try
            {
                if (BitmapBags[t].TryTake(out Bitmap bmp))
                {
                    t.CurrentSize--;
                    return bmp;
                }
                for (int ctr = 0; ctr < t.GrowRate - 1; ctr++)
                {
                    BitmapBags[t].Add(new Bitmap(t.ImageSize.Width, t.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb) { Tag = t });
                    t.CurrentSize++;
                    if (t.CurrentSize == t.IdealSize)
                        t.IdealAt = DateTime.Now;
                }
                return new Bitmap(t.ImageSize.Width, t.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb) { Tag = t };
            }
            catch (KeyNotFoundException ex)
            {
                return new Bitmap(t.ImageSize.Width, t.ImageSize.Height, PixelFormat.Format32bppArgb) { Tag = t };
            }
        }

        public void ReleaseImage(System.Drawing.Image bmp)
        {
            try
            {
                if (bmp is Bitmap)
                {
                    if (bmp.Tag == null)
                        bmp.Dispose();
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
                            bmp.Dispose();
                        else if (BitmapBags[t].Count >= t.MaxSize)
                            bmp.Dispose();
                        else
                        {
                            BitmapBags[t].Add(bmp as Bitmap);
                            t.CurrentSize++;
                        }
                    }
                    else
                        bmp.Dispose();
                }
                else
                    bmp?.Dispose();
            }
            catch (Exception ex)
            {
                try
                {
                    bmp.Dispose();
                }
                catch (Exception) { }
            }
        }        

        public void Dispose()
        {
            if (IsDisposing || IsDisposed)
                return;
            IsDisposing = true;
            StopWorker();
            IsDisposed = true;
        }


        public void DeleteImageMedia(ImageMedia img)
        {
            if (IsDisposing)
                return;
            if (!DisposalBag.Contains(img))
                DisposalBag.Add(img);
        }


        private void BackgroundDeleteImage(ImageMedia tImage)
        {
            CIAT.SaveFile.DeletePart(tImage.URI);
            lock (cacheLock)
            {
                tImage.CacheEntryTime = DateTime.MinValue;
            }
        }
    }
}
