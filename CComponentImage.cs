using System;
using System.Collections.Generic;
using System.Threading;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{

    public class CComponentImage
    {
        public Action ClearPanels;
        public enum ESourceType { responseKey, text, multiLineText, image, iatImage, misc };
        private Action<Image> ImageGeneratedHandler;
        private Rectangle _BoundingRect;
        private bool _MarkedForUpdate, bAbortUpdate = false;
        private Control PreviewPanel;
        //        public enum EType { IATStimulus, Instructions, LeftResponseKeyValue, RightResponseKeyValue, IATImage };
        private IImageSource Source;
        private readonly object lockObject = new object();
        private bool SourceChanged = false;
        private bool _Halted = true;
        private Rectangle _BoundingRectangle;
        private bool IsOpenForEditing = false;
        private ManualResetEvent HaltedEvent = new ManualResetEvent(false);

        private bool Halted
        {
            get
            {
                return _Halted;
            }
            set
            {
                _Halted = value;
            }
        }

        public IIATImage IATImage
        {
            get
            {
                return Source.IATImage;
            }
        }

        public void Invalidate()
        {
            if (Source != null)
                Source.Invalidate();
        }

        public bool IsValid
        {
            get
            {
                return Source.IsValid;
            }
        }

        public bool ImageExists
        {
            get
            {
                return Source.ImageExists;
            }
        }

        public void ValidateComponentImage()
        {
            Source.Validate();
        }

        public object LockObject
        {
            get
            {
                return Source.LockObject;
            }
        }
        /*
        public Rectangle BoundingRect
        {
            get
            {
                Rectangle r = _BoundingRect;
                r.Inflate(new Size((IATImage.ImageSize.Width - _BoundingRect.Width) / 2, (IATImage.ImageSize.Height - _BoundingRect.Height) / 2));
                return r;
            }
        }
        */
        public bool MarkedForUpdate
        {
            get
            {
                return _MarkedForUpdate;
            }
            set
            {
                _MarkedForUpdate = value;
            }
        }

        private void SetImageSourceProc(object o)
        {
            IComponentImageSource newSrc = (IComponentImageSource)o;
            lock (lockObject)
            {
                IImageSource oldSrc = Source;
                Source = newSrc;
                if (Source != null)
                {
                    lock (Source.LockObject)
                    {
                        Source.Invalidate();
                    }
                }
                SourceChanged = true;
            }
        }

        public void SetImageSource(IComponentImageSource newSrc)
        {
            ParameterizedThreadStart ts = new ParameterizedThreadStart(SetImageSourceProc);
            Thread th = new Thread(ts);
            th.Start(newSrc);
        }

        public Rectangle BoundingRect
        {
            get
            {
                return _BoundingRectangle;
            }
        }

        public CComponentImage()
        {
            Source = null;
        }

        public CComponentImage(IImageSource src, Rectangle bounds)
        {
            Source = src;
            _BoundingRectangle = bounds;
        }

        public void OpenForEditing(Control PreviewWin, Action<Image> imgGenD, Action clearPanelsD)
        {
            ImageGeneratedHandler = imgGenD;
            ClearPanels = clearPanelsD;
            ThreadStart genProc = new ThreadStart(GenerateComponentImage);
            Thread th = new Thread(genProc);
            Halted = false;
            IsOpenForEditing = true;
            bAbortUpdate = false;
            PreviewPanel = PreviewWin;
            th.Start();
        }

        public void CloseForEditing()
        {
            IsOpenForEditing = false;
        }

        public void AbortUpdate()
        {
            IsOpenForEditing = false;
            bAbortUpdate = true;
        }

        public void GenerateComponentImage()
        {
            HaltedEvent.Reset();
            Random rand = new Random();
            bool bIsFirstRunThrough = true;
            bool lastUpdateNull = false;

            while (IsOpenForEditing)
            {
                bool bUpdate = false;
                lock (lockObject)
                {
                    if (Source == null)
                        continue;
                    Image img = null;
                    lock (Source.LockObject)
                    {
                        SourceChanged = false;
                        if (!Source.ImageExists)
                            continue;
                        if (!Source.IsValid)
                        {
                            ValidateComponentImage();
                            bUpdate = true;
                        }
                        else
                        {
                            if (ImageExists)
                            {
                                if (!IsValid)
                                    bUpdate = true;
                                ValidateComponentImage();
                            }

                        }
                    }

                    if (!bUpdate && !bIsFirstRunThrough)
                    {
                        Thread.Sleep(10);
                        continue;
                    }
                    if (bUpdate || bIsFirstRunThrough)
                    {
                        Graphics g = null;
                        IIATImage srcImage = Source.IATImage;
                        if ((srcImage == null) && (lastUpdateNull))
                        {
                            PreviewPanel.Invoke(ImageGeneratedHandler, (object)null);
                            lastUpdateNull = false;
                            continue;
                        }
                        else if (srcImage == null)
                        {
                            Thread.Sleep(10);
                            continue;
                        }
                        if (SourceChanged)
                            continue;
                        if (IsOpenForEditing)
                        {
                            Bitmap srcCopy = null;
                            Image sImage = srcImage.theImage;
                            srcCopy = new Bitmap(sImage, new Size((int)((float)sImage.Width * CIATLayout.XScale), (int)((float)sImage.Height * CIATLayout.YScale)));
                            sImage.Dispose();
                            PreviewPanel.BeginInvoke(ImageGeneratedHandler, srcCopy);
                        }
                    }
                    if (!IsOpenForEditing)
                    {
                        HaltedEvent.Set();
                        Halted = true;
                    }
                }
                bIsFirstRunThrough = false;
            }
            if (!bAbortUpdate)
                PreviewPanel.Invoke(ImageGeneratedHandler, (object)null);
            Halted = true;
        }
    }
}



