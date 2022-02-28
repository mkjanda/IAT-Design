using IATClient.Images;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;

namespace IATClient
{
    public abstract class DIGenerated : DIBase, IDisposable
    {
        private readonly static List<DIGenerated> GeneratedItems = new List<DIGenerated>();
        public bool Modified { get; protected set; } = false;
        protected abstract Bitmap Generate();
        private static readonly object GeneratedItemsLock = new object();

        public override bool IsGenerated { get { return true; } }

        public void CalcAbsoluteBounds(Bitmap bmp, Color BackColor)
        {
            Size bmpSize = bmp.Size;
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmpSize.Width, bmpSize.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            Rectangle iRect = new Rectangle(0, 0, bmpSize.Width, bmpSize.Height);
            Int32[] imgData = new Int32[bmpData.Stride * bmpSize.Height / 4];
            IntPtr ptrImgData = bmpData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(ptrImgData, imgData, 0, imgData.Length);
            int backColor = BackColor.ToArgb();
            bool bClear = false;
            int y = 0, x = 0;
            while (imgData[x++ + y * bmpData.Stride / 4] == backColor)
            {
                if (x >= bmpData.Stride / 4)
                {
                    y++;
                    x = 0;
                }
                if (y >= bmpSize.Height)
                {
                    bClear = true;
                    break;
                }
            }
            if (bClear)
            {
                bmp.UnlockBits(bmpData);
                AbsoluteBounds = Rectangle.Empty;
                return;
            }
            iRect.Y = y;
            y = bmpSize.Height - 1;
            x = 0;
            while (imgData[x++ + y * bmpData.Stride / 4] == backColor)
            {
                if (x >= bmpData.Stride / 4)
                {
                    y--;
                    x = 0;
                }
                if (y < 0)
                    break;
            }
            iRect.Height = (y + 1) - iRect.Top;
            y = 0;
            x = 0;
            while (imgData[x + y++ * bmpData.Stride / 4] == backColor)
            {
                if (y >= bmpSize.Height)
                {
                    x++;
                    y = 0;
                }
                if (x * 4 >= bmpData.Stride)
                    break;
            }
            iRect.X = x - 1;
            y = 0;
            x = (bmpData.Stride / 4) - 1;
            while (imgData[x + y++ * bmpData.Stride / 4] == backColor)
            {
                if (y >= bmpSize.Height)
                {
                    x--;
                    y = 0;
                }
                if (x < 0)
                    break;
            }
            iRect.Width = (x + 1) - iRect.Left;
            bmp.UnlockBits(bmpData);
            AbsoluteBounds = iRect;
            IImage.AbsoluteBounds = AbsoluteBounds;
        }

        private ConcurrentQueue<ValidationLock> ValidationLockQueue = new ConcurrentQueue<ValidationLock>();
        public override void LockValidation(ValidationLock validationLock)
        {
            ValidationLockQueue.Enqueue(validationLock);
        }

        protected override void Validate()
        {
            if (ValidationLockQueue.TryDequeue(out ValidationLock result))
                result.Validate(this);
            base.Validate();
        }


        private readonly ManualResetEvent invalidationEntryEvt = new ManualResetEvent(true);
        protected override void Invalidate()
        {
            if (!Monitor.TryEnter(this))
                return;
            try
            {
                try
                {
                    invalidationEntryEvt.WaitOne();
                    invalidationEntryEvt.Reset();
                }
                catch
                {
                    throw;
                }
                finally
                {
                    Monitor.Exit(this);
                }
                if (LayoutSuspended)
                {
                    Validate();
                    return;
                }
                if (ValidationLockQueue.TryPeek(out ValidationLock validationLock))
                    if (!validationLock.DoInvalidation(this))
                        return;
                Bitmap bmp = Generate();
                if ((bmp != null) && !IsDisposing && !IsDisposed)
                    IImage.Img = bmp;
                else
                    Validate();
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error occurred in image generation", ex));
            }
            finally
            {
                invalidationEntryEvt.Set();
            }
        }


        public DIGenerated()
        {
            lock (GeneratedItemsLock)
            {
                GeneratedItems.Add(this);
            }
        }

        public DIGenerated(Uri uri) : base(uri)
        {
            lock (GeneratedItemsLock)
            {
                GeneratedItems.Add(this);
            }
        }

        public DIGenerated(Images.IImage img)
            : base(img)
        {
            lock (GeneratedItemsLock)
            {
                GeneratedItems.Add(this);
            }
        }

        protected void StopGenerating()
        {
            lock (GeneratedItemsLock)
            {
                GeneratedItems.Remove(this);
            }
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            lock (GeneratedItemsLock)
            {
                GeneratedItems.Remove(this);
            }
            base.Dispose();
        }
    }
}
