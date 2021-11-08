using System;
using System.Collections.Generic;
using System.Drawing;

using System.Text;

namespace IATClient
{
    class CMemoryImageDisplayItem : CDisplayItem, ImageManager.INonUserImageSource
    {
        private Bitmap _MemoryBmp;
        private bool HasThumbnail;

        public override CComponentImage.ESourceType SourceType
        {
            get
            {
                return CComponentImage.ESourceType.misc;
            }
        }

        public Bitmap MemoryBmp
        {
            get
            {
                return _MemoryBmp;
            }
        }

        public CMemoryImageDisplayItem(int width, int height, bool hasThumbnail)
            : base(EType.memoryImage)
        {
            _MemoryBmp = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            _IATImage = CIAT.ImageManager.AddNonUserImage(this, hasThumbnail);
            HasThumbnail = hasThumbnail;
        }

        public CMemoryImageDisplayItem(int width, int height, bool hasThumbnail, Image img)
            : base(EType.memoryImage)
        {
            _MemoryBmp = new Bitmap(img, width, height);
            _IATImage = CIAT.ImageManager.AddNonUserImage(this, hasThumbnail);
            HasThumbnail = hasThumbnail;
        }

        public override void Dispose()
        {
            Lock();
            base.Dispose();
            if (_MemoryBmp != null)
                _MemoryBmp.Dispose();
            _MemoryBmp = null;
            Unlock();
        }

        public Image GenerateImage()
        {
            Lock();
            Image img = MemoryBmp;
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

        protected override Size GetItemSize()
        {
            if (MemoryBmp == null)
                return new Size(0, 0);
            else
                return new Size(MemoryBmp.Width, MemoryBmp.Height);
        }

        public override bool LoadFromXml(System.Xml.XmlNode node)
        {
            throw new Exception("CMemoryImageDisplayItems cannot be loaded from XML");
        }

        public override void WriteToXml(System.Xml.XmlTextWriter writer)
        {
            throw new Exception("CMemoryImageDisplayItems should not be written to XML");
        }

        public override bool IsDefined()
        {
            Lock();
            bool b;
                if (MemoryBmp == null)
                    b = false;
                b = true;
            Unlock();
            return b;
        }

        public override bool IsValid
        {
            get
            {
                return true;
            }
        }

        public override void Validate()
        {
            return;
        }

        public override void Invalidate()
        {
            return;
        }

        public Size GetContainerSize()
        {
            return GetItemSize();
        }
    }
}
