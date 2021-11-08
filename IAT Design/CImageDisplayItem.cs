using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;

namespace IATClient
{
    [Serializable]
    public abstract class CImageDisplayItem : CDisplayItem, IStoredInXml, IDisposable
    {
        private String _Description;

        /// <summary>
        /// gets a description of the display item
        /// </summary>
        public String Description
        {
            get
            {
                return _Description;
            }
            set
            {
                _Description = value;
            }
        }


        /// <summary>
        /// gets or sets the full file path of the image file
        /// </summary>
        public String FullFilePath
        {
            get
            {
                Lock();
                String str = String.Empty;
                if (IATImage == null)
                    str = String.Empty;
                else if ((str = CIAT.ImageManager.GetFilePath(IATImage)) == String.Empty)
                    str = Properties.Resources.sImageLoadedFromSaveFile;
                Unlock();
                return str;
            }
            set
            {
                Lock();
                if (IATImage == null)
                    if (value == CIAT.ImageManager.GetFilePath(IATImage))
                    {
                        Unlock();
                        return;
                    }
                if (IATImage != null)
                    ((IUserImage)IATImage).Dispose();
                _IATImage = CIAT.ImageManager.AddImage(value, GetSizeCallback());
                _Description = System.IO.Path.GetFileNameWithoutExtension(value);
                if (IATImage == null)
                    throw new Exception(String.Format("Unable to add the image \'{0}\' to the image file dictionary.", value));
                Invalidate();
                Unlock();
            }
        }

        // a flag indicating if the imaged should be stretched to fit the bounding rectangle
        protected bool _StretchToFit;

        /// <summary>
        /// gets or sets whether the object should be stretched to fit the bounding rectangle when its displayed as 
        /// a stimulus or response key value
        /// </summary>
        public virtual bool StretchToFit
        {
            get
            {
                return _StretchToFit;
            }
            set
            {
                Lock();
                if (_StretchToFit != value)
                    IATImage.Dispose();
                _StretchToFit = value;
                Unlock();
            }
        }
        /*
        /// <summary>
        /// The default constructor
        /// </summary>
        public CImageDisplayItem()
            : base(CDisplayItem.EType.image)
        {
            _FileName = _Directory = String.Empty;
            _CopyToOutputDirOnSave = false;
            _StretchToFit = false;
            GetIATDirectory = IATConfigMainForm.GetIATDirectory;
            _UID = -1;
            TempImgValid = false;
            ItemImage = null;
        }
        */
        /// <summary>
        /// The constructor that is invoked by derived classes
        /// </summary>
        /// <param name="type">The type of the derived class</param>
        protected CImageDisplayItem(CDisplayItem.EType type)
            : base(type)
        {
            _IATImage = -1;
            _StretchToFit = false;
            _Description = String.Empty;
        }

        protected CImageDisplayItem(CImageDisplayItem o)
            : base(o.Type)
        {
            _StretchToFit = o._StretchToFit;
            _IATImage = o._IATImage;
            IATImage.CreateCopy();
            _Description = o.Description;
        }


        /// <summary>
        /// Disposes of the resources used by the object
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();
            _ItemSize = Size.Empty;
        }

        protected Size _ItemSize;
        /*
        /// <summary>
        /// returns the item's size
        /// </summary>
        /// <returns></returns>
        protected override Size GetItemSize()
        {
            if (FullFilePath == String.Empty)
                return Size.Empty;
            lock (this)
            {
                if (_ItemSize == Size.Empty)
                {
                    if (_TempImgValid)
                    {
                        Image img = CTempImgFile.GetImage(UID);
                        _ItemSize = img.Size;
                        img.Dispose();
                    }
                    else
                    {
                        _ItemSize = SizeImage();
                    }
                }
            }
            return _ItemSize;
        }
        */
       
        public override bool IsValid
        {
            get
            {
                if (IATImage == null)
                    return false;
                return true;
            }
        }
        
        /// <summary>
        /// Displays the object at the given location
        /// </summary>
        /// <param name="g">The graphics context</param>
        /// <param name="location">The display location</param>
        /// <returns>"true" on success, otherwise "false"</returns>
        /*
        public override bool Display(Graphics g, Point location)
        {
            if (!IsValid())
                return false;

            Image img = CIAT.ImageManager[ImageID].theImage;
            g.DrawImage(img, new Rectangle(location, ItemSize), new Rectangle(new Point(0, 0), ItemSize), GraphicsUnit.Pixel);
            img.Dispose();
            return true;
        }
        */

        public bool DisplayBounded(Graphics g, Rectangle bounds)
        {
            if (!IsDefined())
                return false;

            // calculate the destination rectangle
            Rectangle ImageRectangle = new Rectangle();
            ImageRectangle.X = bounds.X + ((Math.Abs(bounds.Width - ItemSize.Width)) >> 1);
            ImageRectangle.Y = bounds.Y + ((Math.Abs(bounds.Height - ItemSize.Height)) >> 1);
            ImageRectangle.Width = ItemSize.Width;
            ImageRectangle.Height = ItemSize.Height;
            Image img = IATImage.theImage.Image;
            g.DrawImage(img, ImageRectangle);
            img.Dispose();

            return true;
        }

        protected abstract ImageManager.ImageSizeCallback GetSizeCallback();
    }
}
