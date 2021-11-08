using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;
using System.Threading;

namespace IATClient
{
    class CRectangleComponentSource : IComponentImageSource, ImageManager.INonUserImageSource
    {
        private Rectangle _Rect;
        private System.Drawing.Color _RectColor;
        private bool bComponentImageValid = false;
        private readonly object lockObject = new object();
        private int _LineWidth = -1;
        private Random rand = new Random();
        private int _IATImageID = -1;


        public CComponentImage.ESourceType SourceType
        {
            get
            {
                return CComponentImage.ESourceType.misc;
            }
        }

        public int LineWidth
        {
            get
            {
                lock (lockObject)
                {
                    return _LineWidth;
                }
            }
            set
            {
                lock (lockObject)
                {
                    _LineWidth = value;
                }
            }

        }

        public Size GetContainerSize()
        {
            return CIAT.Layout.KeyValueSize;
        }

        protected bool TryLock()
        {
            return Monitor.TryEnter(lockObject);
        }

        public IIATImage IATImage
        {
            get
            {
                return CIAT.ImageManager[_IATImageID];
            }
        }

        public bool ImageExists
        {
            get
            {
                return true;
            }
        }

        public object LockObject
        {
            get
            {
                return lockObject;
            }
        }

        protected void Lock()
        {
            Monitor.Enter(lockObject);
        }

        protected void Unlock()
        {
            Monitor.Exit(lockObject);
        }

        public bool IsValid
        {
            get
            {
                lock (lockObject)
                {
                    return bComponentImageValid;
                }
            }
        }

        public void Validate()
        {
            lock (lockObject)
            {
                bComponentImageValid = true;
            }
        }

        public void Invalidate()
        {
            lock (lockObject)
            {
                bComponentImageValid = false;
            }
        }


        public Rectangle Rect
        {
            get
            {
                lock (lockObject)
                {
                    return _Rect;
                }
            }
            set
            {
                lock (lockObject)
                {
                    _Rect = value;
                }
            }
        }

        public System.Drawing.Color RectColor
        {
            get
            {
                lock (lockObject)
                {
                    return _RectColor;
                }
            }
            set
            {
                lock (lockObject)
                {
                    _RectColor = value;
                }
            }
        }
        

        public CRectangleComponentSource(Rectangle r, System.Drawing.Color c, int lineWidth)
        {
            _RectColor = c;
            _Rect = r;
            _LineWidth = lineWidth;
        }

        public Image GenerateImage()
        {
            Lock();
            if (!Monitor.TryEnter(lockObject))
                return null;
            Bitmap img = new Bitmap(Rect.Size.Width, Rect.Size.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Brush backBrush = new SolidBrush(CIAT.Layout.BackColor);
            Brush br = new SolidBrush(RectColor);
            Pen p = new Pen(br, LineWidth);
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(backBrush, Rect);
            g.DrawRectangle(p, Rect);
            img.MakeTransparent(CIAT.Layout.BackColor);
            g.Dispose();
            p.Dispose();
            br.Dispose();
            backBrush.Dispose();
            Unlock();
            return img;
        }

        public Image TryGenerateImage()
        {
            if (!TryLock())
                return null;
            Image img = GenerateImage();
            Unlock();
            return img;
        }

    }
}
