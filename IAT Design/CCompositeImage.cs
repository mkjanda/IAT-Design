using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace IATClient
{
    public interface ICompositeImageSource
    {
        bool CompositeImageValid { get; set; }
        Dictionary<IImageSource, Rectangle> ComponentImages { get; }
        object LockObject { get; }
        void Wait();
    }

    public class CCompositeImage : IDisposable
    {
        private List<CComponentImage> ComponentImages = new List<CComponentImage>();
        public delegate void ImageGeneratedHandler(Image img);
        private ImageGeneratedHandler OnImageGenerated;
        private System.Threading.Timer GenerateTimer = null;
        private object GenerateLock = new object();
        private bool FreshlyOpened = false;
        private Control PreviewWindow;
        private bool _IsOpenForEditing = false;
        private int ID;
        private ICompositeImageSource Source = null;
        private bool HasThumbnail;
        private object lockObject = new object();
        private static Random random = new Random();
        private bool _IsValid = false;


        protected bool IsOpenForEditing
        {
            get
            {
                lock (lockObject)
                {
                    return _IsOpenForEditing;
                }
            }
            set
            {
                lock (lockObject)
                {
                    _IsOpenForEditing = value;
                }
            }
        }

        public IImage CloneCompositeImage()
        {
            TryGenerate(true);
            return CIAT.ImageManager[ID].IATImage.theImage;
        }

        public CCompositeImage(Size sz, Func<Size> finalImageSize, bool hasThumbnail, ICompositeImageSource source)
        {
            ID = CIAT.ImageManager.AddCompositeImage(null, hasThumbnail, sz, finalImageSize);
            OnImageGenerated = null;
            PreviewWindow = null;
            Source = source;
            CCompositeImageGenerator.AddCompositeImage(this);
            _IsValid = false;
        }

        public void UnwireThumbnailCallback()
        {
            CIAT.ImageManager[ID].IATImage.DestroyThumbnail();
        }

        private void AddImage(IImageSource source, Point pos, Size sz)
        {
            CComponentImage c = new CComponentImage(source, new Rectangle(pos, sz));
            lock (lockObject)
            {
                ComponentImages.Add(c);
            }
        }

        public void Invalidate()
        {
            _IsValid = false;
        }

        private void AddImage(IImageSource source, Rectangle rect)
        {
            CComponentImage c = new CComponentImage(source, rect);
            lock (lockObject)
            {
                ComponentImages.Add(c);
            }
        }

        public void InvalidateSource(bool invalidateComponents)
        {
            Source.CompositeImageValid = false;
            if (invalidateComponents)
            {
                Dictionary<IImageSource, Rectangle> dic = Source.ComponentImages;
                foreach (IImageSource ci in dic.Keys)
                    ci.Invalidate();
            }
        }

        public void TryGenerate(bool bForce)
        {
            if (!bForce)
            {
                if (!Monitor.TryEnter(GenerateLock))
                    return;
            }
            else
            {
                Monitor.Enter(GenerateLock);
                if (IsValid)
                {
                    Monitor.Exit(GenerateLock);
                    return;
                }
            }
            try
            {
                Random rand = new Random();
                bool bUpdate;
                Image img = null;
                    bUpdate = false;
                    lock (Source.LockObject)
                    {
                        if (!Source.CompositeImageValid)
                        {
                            ComponentImages.Clear();
                            UpdateLock();
                            Dictionary<IImageSource, Rectangle> dic = Source.ComponentImages;
                            UpdateUnlock();
                            foreach (IImageSource cis in dic.Keys)
                                AddImage(cis, dic[cis].Location, dic[cis].Size);
                            bUpdate = true;
                            Source.CompositeImageValid = true;
                        }
                        else
                        {
                            foreach (CComponentImage ci in ComponentImages)
                            {
                                lock (ci.LockObject)
                                {
                                    if (ci.ImageExists)
                                    {
                                        if (!ci.IsValid)
                                            bUpdate = true;
                                        ci.ValidateComponentImage();
                                    }
                                }
                            }
                        }
                    }
                    if (!bUpdate && IsValid)
                        return;
                    if (bUpdate || !IsValid)
                    {
                        Size finalSize = CIAT.ImageManager[ID].ImageSize;
                        Graphics g = null;
                        Image compositeImage = CIAT.ImageManager[ID].theImage.Image;
                        if (compositeImage == null)
                        {
                            img = new Bitmap(finalSize.Width, finalSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            g = Graphics.FromImage(img);
                        }
                        else if (!compositeImage.Size.Equals(finalSize))
                        {
                            img = new Bitmap(finalSize.Width, finalSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                            g = Graphics.FromImage(img);
                        }
                        else
                        {
                            g = Graphics.FromImage(compositeImage);
                        }
                        Brush backBrush = new SolidBrush(CIAT.Layout.BackColor);
                        g.FillRectangle(backBrush, new Rectangle(0, 0, finalSize.Width - 1, finalSize.Height - 1));
                        backBrush.Dispose();
                        lock (CIAT.ImageManager[ID].LockObject)
                        {
                            foreach (CComponentImage c in ComponentImages)
                            {
                                Rectangle boundingRect;
                                IIATImage cImage;
                                bool bImageExists;
                                lock (c.LockObject)
                                {
                                    boundingRect = c.BoundingRect;
                                    cImage = c.IATImage;
                                    bImageExists = c.ImageExists;
                                }
                                if (cImage == null)
                                {
                                    continue;
                                }
                                Image cImg = cImage.theImage;
                                g.SetClip(boundingRect);
                                g.DrawImage(cImg, boundingRect.Location + new Size((boundingRect.Width - cImg.Width) >> 1,
                                        (boundingRect.Height - cImg.Height) >> 1));
                                cImg.Dispose();
                            }
                            if (img != null)
                                CIAT.ImageManager[ID].Update(img);
                            g.Dispose();
                            if (compositeImage != null)
                                compositeImage.Dispose();
                        }
                    }
                if (bUpdate || !IsValid)
                {
                    if (IsOpenForEditing)
                    {
                        PreviewWindow.BeginInvoke(OnImageGenerated, CIAT.ImageManager[ID].theImage);
                    }
                    FreshlyOpened = false;
                }
                _IsValid = true;
            }
            catch (Exception ex) { }
            finally
            {
                Monitor.Exit(GenerateLock);
            }
            return;
        }

        private object updateLock = new object();

        public void UpdateLock()
        {
            Monitor.Enter(updateLock);
        }

        public void UpdateUnlock()
        {
            Monitor.Exit(updateLock);
        }

        public void OpenForEditing(Control previewWin, ImageGeneratedHandler callback)
        {
            PreviewWindow = previewWin;
            OnImageGenerated = callback;
            IsOpenForEditing = true;
            IATConfigMainForm.OpenCompositeImages.Add(this);
            _IsValid = false;
        }

        public void CloseForEditing()
        {
            PreviewWindow = null;
            OnImageGenerated = null;
            IsOpenForEditing = false;
            IATConfigMainForm.OpenCompositeImages.Remove(this);
        }

        public bool IsValid
        {
            get
            {
                if (!_IsValid)
                    return false;
                if (!Source.CompositeImageValid)
                    return false;
                lock (lockObject)
                {
                    foreach (CComponentImage ci in ComponentImages)
                        if (!ci.IsValid)
                            return false;
                }
                return true;
            }
        }

        public void Dispose()
        {
            CCompositeImageGenerator.RemoveImage(this);
            ((ICompositeImage)CIAT.ImageManager[ID]).Dispose();
        }
    }
}
