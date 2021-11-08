using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Windows.Forms;

namespace IATClient.ImageManager
{
    abstract class CIATImage : IIATImage
    {
        protected int _TempImgID = -1;
        protected bool _TempImgValid = false;
        protected int _ID = -1;
        protected int _NumInstances = 0;
        protected int _ThumbnailID = -1;
        protected bool _ThumbnailImageValid = false;
        protected Size _ImageSize = Size.Empty;
        protected Size szThumbnail;
        protected Color ThumbnailBackColor;
        protected object lockObject = new object();
        protected bool bMarkedForResize = false;
        protected List<Size> szResize = new List<Size>();
        public enum EType { UserImage, NonUserImage };
        protected EType _Type;

        public abstract bool IsUserImage { get; }
        public abstract Size OriginalImageSize { get; }

        protected CIATImage(EType type)
        {
            _Type = type;
        }

        public Size ImageSize
        {
            get
            {
                lock (lockObject)
                {
                    if (_ImageSize == Size.Empty)
                    {
                        if (szResize.Count != 0)
                            return szResize.Last();
                        else
                            return OriginalImageSize;
                    }
                    return _ImageSize;
                }
            }
        }

        public abstract Image theImage { get; }

        /// <summary>
        /// provided for the implementation of ITempImage -- gets or sets the unique ID of the image
        /// thread-safe
        /// </summary>
        public int UID
        {
            get
            {
                lock (lockObject)
                {
                    return _ID;
                }
            }
            set
            {
                lock (lockObject)
                {
                    _ID = value;
                }
            }
        }

        /// <summary>
        /// returns a value that represents if the thumbnail temp image is valid
        /// thread-safe
        /// </summary>
        public bool ThumbnailImageValid
        {
            get
            {
                lock (lockObject)
                {
                    return _ThumbnailImageValid;
                }
            }
        }

        /// <summary>
        /// gets or sets the temporary image id
        /// </summary>
        public int TempImageID
        {
            get
            {
                return _TempImgID;
            }
            set
            {
                _TempImgID = value;
            }
        }

        /// <summary>
        /// gets the thumbnail image
        /// </summary>
        public abstract Image theThumbnail { get; }

        /// <summary>
        /// resizes the underlying image data to the specified size and stores it in the temporary image file
        /// </summary>
        /// <param name="sz">the new size of the image</param>
        public void Resize(Size sz)
        {
            lock (lockObject)
            {
                bMarkedForResize = true;
                double arImg = (double)OriginalImageSize.Width / (double)OriginalImageSize.Height;
                double arSizeRect = (double)sz.Width / (double)sz.Height;
                if (arImg > arSizeRect)
                    szResize.Add(new Size(sz.Width, (int)(sz.Width * ((double)OriginalImageSize.Height / (double)OriginalImageSize.Width))));
                else
                    szResize.Add(new Size((int)(sz.Height * ((double)OriginalImageSize.Width / (double)OriginalImageSize.Height)), sz.Height));
                CImageManager.AddImageToResizer(UID);
            }
        }

        public void PerformResize()
        {
            lock (lockObject)
            {
                if (szResize.Count == 0)
                    return;
                Image img = CImageManager.LoadImage(UID);
                Image dest;
                dest = new System.Drawing.Bitmap(img, szResize[0]);
                if (_TempImgValid)
                    CTempImgFile.FreeImage(_TempImgID);
                _TempImgID = CTempImgFile.AppendImage(dest);
                _TempImgValid = true;
                _ImageSize = szResize[0];
                szResize.RemoveAt(0);
                if (szResize.Count == 0)
                    bMarkedForResize = false;
            }
        }


        public void Dispose(bool bMarkForDeletion)
        {
            if ((DecrementInstances() == 0) && bMarkForDeletion)
                DisposeOfData();
        }

        public abstract void DisposeOfData();

        /// <summary>
        /// increments the counter of the number of instances of the image in the IAT
        /// thread-safe
        /// </summary>
        public void IncrementInstances()
        {
            lock (lockObject)
            {
                _NumInstances++;
            }
        }

        public int NumInstances
        {
            get
            {
                lock (lockObject)
                {
                    return _NumInstances;
                }
            }
        }

        public void CreateCopy()
        {
            lock (lockObject)
            {
                IncrementInstances();
            }
        }


        /// <summary>
        /// decrements the counter of the number of instances of the image in the IAT
        /// thread-safe
        /// </summary>
        /// <returns></returns>
        private int DecrementInstances()
        {
            lock (lockObject)
            {
                _NumInstances--;
                return NumInstances;

            }
        }

        /// <summary>
        /// generates a thumbnail for the passed image
        /// thread-safe
        /// </summary>
        /// <param name="img">the image to generate a thumbnail for</param>
        protected abstract void GenerateThumbnail(Image img);

        /// <summary>
        /// the thread-start procedure for generating a thumbnail image in a worker thread
        /// </summary>
        /// <param name="img">the image to generate a thumbnail for, of type System.Drawing.Image</param>
        public void GenerateThumbnail(object img)
        {
            GenerateThumbnail((Image)img);
        }

        /// <summary>
        /// spawns a thread that generates a thumbnail for the image described by the CImageInfo object
        /// </summary>
        /// <param name="img">the image to generate a thumbnail for</param>
        public void GenerateThumbnailInBackground(Image img)
        {
            ParameterizedThreadStart proc = new ParameterizedThreadStart(GenerateThumbnail);
            Thread thread = new Thread(proc);
            thread.Start(img);
        }

        public abstract void FreeTempImage();
        public abstract void FreeTempData();
    }

    class CUserImage : CIATImage, IUserImage
    {
        private long _OffsetInFile = -1;
        private int _StimulusImageID = -1;
        private String _FileName;
        private String _FullFilePath;
        private Size _OriginalImageSize = Size.Empty;

        public override bool IsUserImage
        {
            get
            {
                return true;
            }
        }

        public override Size OriginalImageSize
        {
            get
            {
                lock (lockObject)
                {
                    if (_OriginalImageSize == Size.Empty)
                    {
                        Image img = CImageManager.LoadImage(UID);
                        _OriginalImageSize = img.Size;
                        img.Dispose();
                    }
                    return _OriginalImageSize;
                }
            }
        }

        /// <summary>
        /// gets the thumbnail image
        /// </summary>
        public override Image theThumbnail
        {
            get
            {
                lock (lockObject)
                {
                    if (!_ThumbnailImageValid)
                    {
                        Image img = theImage;
                        GenerateThumbnail(img);
                        img.Dispose();
                    }
                    return CTempImgFile.GetImage(_ThumbnailID);
                }
            }
        }

        /// <summary>
        /// returns the underlying image
        /// </summary>
        public override Image theImage
        {
            get
            {
                lock (lockObject)
                {
                    while (bMarkedForResize)
                        PerformResize();

                    if (_TempImgValid)
                        return CTempImgFile.GetImage(TempImageID);
                    else
                        return CImageManager.LoadImage(UID);
                }
            }
        }

        /*
        public Image theStimulusImage
        {
            get
            {
                lock (lockObject)
                {
                    if (!_StimulusImageValid)
                    {
                        Image img = theImage;
                        GenerateStimulusImage(img);
                        img.Dispose();
                    }
                    return CTempImgFile.GetImage(_StimulusImageID);
                }
            }
        }
        */
        public override void DisposeOfData()
        {
            if (_TempImgValid)
                CTempImgFile.FreeImage(_TempImgID);
            if (_ThumbnailImageValid)
                CTempImgFile.FreeImage(_ThumbnailID);
            _TempImgValid = false;
            _ThumbnailImageValid = false;
        }


        public override void FreeTempImage()
        {
            if (_TempImgValid)
                CTempImgFile.FreeImage(_TempImgID);
            _TempImgValid = false;
        }

        public override void FreeTempData()
        {
            if (_TempImgValid)
                CTempImgFile.FreeImage(_TempImgID);
            if (_ThumbnailImageValid)
                CTempImgFile.FreeImage(_ThumbnailID);
            _TempImgValid = false;
            _ThumbnailImageValid = false;
        }



        /// <summary>
        /// gets a description for the image
        /// </summary>
        public String Description
        {
            get
            {

                return FileName;
            }
        }



        /// <summary>
        /// provided for the implementation of ITempImage -- called to mark the temporary image file data for the image as valid
        /// thread-safe
        /// </summary>
        public void Validate()
        {
            lock (lockObject)
            {
                _ThumbnailImageValid = true;
                //                    _StimulusImageValid = true;
            }
        }

        /// <summary>
        /// provided for the implementation of ITempImage -- called to mark the temporary image file data for the image as invalid
        /// thread-safe
        /// </summary>
        public void Invalidate()
        {
            lock (lockObject)
            {
                _ThumbnailImageValid = false;
                //                    _StimulusImageValid = false;
            }
        }

        /// <summary>
        /// the no-arg constructor
        /// </summary>
        public CUserImage()
            : base(EType.UserImage)
        {
            _OffsetInFile = -1;
            _FileName = String.Empty;
            _FullFilePath = String.Empty;
            _NumInstances = 0;
            _ThumbnailID = -1;
            _ThumbnailImageValid = false;
            bMarkedForResize = false;
            lockObject = new object();
            szThumbnail = ImageBrowser.ThumbnailSize;
            ThumbnailBackColor = Color.Transparent;
        }
        /*
                    public CImageInfo(Image img, Size szThumbnail, Color ThumbnailBackColor)
                    {
                        _OffsetInFile = -1;
                        _FileName = String.Empty;
                        _FullFilePath = String.Empty;
                        _NumInstances = 1;
                        _ThumbnailID = -1;
                        _ThumbnailImageValid = false;
                        bMarkedForResize = false;
                        lockObject = new object();
                        bUserImage = false;
                        this.szThumbnail = szThumbnail;
                        this.ThumbnailBackColor = ThumbnailBackColor;
                        _NonUserTempImgFileID = CTempImgFile.AppendImage(img);
                        _OriginalImageSize = img.Size;
                    }
        */
        /// <summary>
        /// constructs a CImageInfo object from the file path of an image
        /// </summary>
        /// <param name="FullFilePath">the file path of the image</param>
        public CUserImage(String FullFilePath)
            : base(EType.UserImage)
        {
            _OffsetInFile = -1;
            _FileName = Path.GetFileName(FullFilePath);
            _FullFilePath = FullFilePath;
            _NumInstances = 1;
            _ThumbnailID = -1;
            _ThumbnailImageValid = false;
            bMarkedForResize = false;
            lockObject = new object();
            szThumbnail = ImageBrowser.ThumbnailSize;
            ThumbnailBackColor = Color.Transparent;
        }

        /// <summary>
        /// constructs a CImageInfo object from a from an IAT save file
        /// </summary>
        /// <param name="filename">the original filename of the image</param>
        /// <param name="numInstances">the number of times the image occurs in the IAT</param>
        /// <param name="offsetInFile">the offset of the image in the save file</param>
        public CUserImage(String filename, int numInstances, long offsetInFile)
            : base(EType.UserImage)
        {
            _FileName = filename;
            _NumInstances = numInstances;
            _OffsetInFile = offsetInFile;
            _FullFilePath = String.Empty;
            _ThumbnailID = -1;
            _ThumbnailImageValid = false;
            bMarkedForResize = false;
            lockObject = new object();
            szThumbnail = ImageBrowser.ThumbnailSize;
            ThumbnailBackColor = Color.Transparent;

        }


        /// <summary>
        /// clears the full file path of the CImageInfo object
        /// thread-safe
        /// </summary>
        public void ClearFullFilePath()
        {
            lock (lockObject)
            {
                _FullFilePath = String.Empty;
            }
        }

        /// <summary>
        /// gets the full file path of the CImageInfo object
        /// thread-safe
        /// </summary>
        public String FullFilePath
        {
            get
            {
                lock (lockObject)
                {
                    return _FullFilePath;
                }
            }
        }

        /// <summary>
        /// gets the file name of the CImageInfo object
        /// </summary>
        public String FileName
        {
            get
            {
                lock (lockObject)
                {
                    return _FileName;
                }
            }
        }

        /// <summary>
        /// gets or sets the value that stores the CImageInfo's image data offset in the save file
        /// </summary>
        public long OffsetInFile
        {
            get
            {
                lock (lockObject)
                {
                    return _OffsetInFile;
                }
            }
            set
            {
                lock (lockObject)
                {
                    _OffsetInFile = value;
                }
            }
        }

        /// <summary>
        /// gets the number of instances of the image in the IAT
        /// thread-safe
        /// </summary>

        /*
        protected void GenerateStimulusImage(Image img)
        {
            lock (lockObject)
            {
                if (StimulusImageValid)
                    CTempImgFile.FreeImage(_StimulusImageID);
                Size sz = CIAT.Layout.StimulusSize;
                Image stimulus = new Bitmap(sz.Width, sz.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(stimulus);
                Brush backBrush = new SolidBrush(CIAT.Layout.BackColor);
                g.FillRectangle(backBrush, new Rectangle(new Point(0, 0), sz));
                backBrush.Dispose();
                float aspectRatio = (float)sz.Width / (float)sz.Height;
                SizeF szStimulusImage;
                PointF ptStimulusImage;
                if (aspectRatio > 1)
                {
                    szStimulusImage = new SizeF(sz.Width, sz.Height / aspectRatio);
                    ptStimulusImage = new PointF(0, (sz.Height - szStimulusImage.Height) / 2);
                }
                else
                {
                    szStimulusImage = new SizeF(sz.Width * aspectRatio, sz.Height);
                    ptStimulusImage = new PointF((sz.Width - szStimulusImage.Width) / 2, 0);
                }
                RectangleF stimulusRect = new RectangleF(ptStimulusImage, szStimulusImage);
                g.DrawImage(img, stimulusRect);
                g.Dispose();
                _StimulusImageID = CTempImgFile.AppendImage(stimulus);
                g.Dispose();
                stimulus.Dispose();
            }
        }
        */

        protected override void GenerateThumbnail(Image img)
        {
            lock (lockObject)
            {
                if (ThumbnailImageValid)
                    CTempImgFile.FreeImage(_ThumbnailID);
                Size sz = szThumbnail;
                Bitmap thumbnail = new Bitmap(szThumbnail.Width, szThumbnail.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                //                    for (int ctr1 = 0; ctr1 < thumbnail.Width; ctr1++)
                //                       for (int ctr2 = 0; ctr2 < thumbnail.Height; ctr2++)
                //                         thumbnail.SetPixel(ctr1, ctr2, cTrans);
                Graphics g = Graphics.FromImage(thumbnail);
                Brush backBrush = new SolidBrush(CImageManager.TransparentColor);
                g.FillRectangle(backBrush, new Rectangle(new Point(0, 0), szThumbnail));
                float aspectRatio = (float)img.Width / (float)img.Height;
                SizeF szThumbImg;
                PointF ptThumbImg;
                if (aspectRatio > 1)
                {
                    szThumbImg = new SizeF(szThumbnail.Width, szThumbnail.Height / aspectRatio);
                    ptThumbImg = new PointF(0, (szThumbnail.Height - szThumbImg.Height) / 2);
                }
                else
                {
                    szThumbImg = new SizeF(szThumbnail.Width * aspectRatio, szThumbnail.Height);
                    ptThumbImg = new PointF((szThumbnail.Width - szThumbImg.Width) / 2, 0);
                }
                RectangleF thumbImgRect = new RectangleF(ptThumbImg, szThumbImg);
                g.DrawImage(img, thumbImgRect);
                g.Dispose();
                thumbnail.MakeTransparent(CImageManager.TransparentColor);
                _ThumbnailID = CTempImgFile.AppendImage(thumbnail);
                thumbnail.Dispose();
                img.Dispose();
            }
        }
    }

    class CNonUserImage : CIATImage, INonUserImage
    {
        private Image _theImage = null, _theThumbnail = null;
        private Size _OriginalImageSize = Size.Empty;
        private bool bCommited = false;

        public CNonUserImage(Image img, Size szThumbnail, Color ThumbnailBackColor)
            : base(EType.NonUserImage)
        {
            lock (lockObject)
            {
                _theImage = img;
                _theThumbnail = null;
                this.szThumbnail = szThumbnail;
                this.ThumbnailBackColor = ThumbnailBackColor;
                _ImageSize = img.Size;
                _OriginalImageSize = img.Size;
                GenerateThumbnailInBackground(img);
            }
        }

        public override bool IsUserImage
        {
            get
            {
                return false;
            }
        }

        public override Image theThumbnail
        {
            get
            {
                if (!bCommited)
                {
                    while (!ThumbnailImageValid)
                        Thread.Sleep(100);
                    return _theThumbnail;
                }
                return CTempImgFile.GetImage(_ThumbnailID);
            }
        }

        public override Image theImage
        {
            get
            {
                if (_TempImgValid)
                    return CTempImgFile.GetImage(TempImageID);
                else
                    return _theImage;
            }
        }

        public void SetImage(Image img)
        {
            lock (lockObject)
            {
                FreeTempData();
                if (_theImage != null)
                    _theImage.Dispose();
                _theImage = img;
                _OriginalImageSize = img.Size;
                _ImageSize = img.Size;
                _ThumbnailImageValid = false;
                GenerateThumbnailInBackground(img);
                bCommited = false;
            }
        }

        public void Commit()
        {
            lock (lockObject)
            {
                if (_TempImgValid)
                    CTempImgFile.FreeImage(_TempImgID);
                _TempImgID = CTempImgFile.AppendImage(_theImage);
            }
            while (!ThumbnailImageValid)
                Thread.Sleep(100);
            lock (lockObject)
            {
                _ThumbnailID = CTempImgFile.AppendImage(_theThumbnail);
                _theImage.Dispose();
                _theThumbnail.Dispose();
                _TempImgValid = true;
                bCommited = true;
            }
        }


        public override Size OriginalImageSize
        {
            get
            {
                lock (lockObject)
                {
                    return _OriginalImageSize;
                }
            }
        }

        public override void FreeTempData()
        {
            if (_TempImgValid)
                CTempImgFile.FreeImage(_TempImgID);
            if (_ThumbnailImageValid)
                CTempImgFile.FreeImage(_ThumbnailID);
            _TempImgValid = false;
            _ThumbnailImageValid = false;
            _TempImgID = -1;
            _ThumbnailID = -1;
        }

        public override void FreeTempImage()
        {
            if (_TempImgValid)
                CTempImgFile.FreeImage(_TempImgID);
            _TempImgValid = false;
            _TempImgID = -1;
        }

        public override void DisposeOfData()
        {
            FreeTempData();
            if (_theImage != null)
                _theImage.Dispose();
        }

        protected override void GenerateThumbnail(Image img)
        {
            lock (lockObject)
            {
                if (ThumbnailImageValid)
                    CTempImgFile.FreeImage(_ThumbnailID);
                Size sz = szThumbnail;
                Image thumbnail = new Bitmap(szThumbnail.Width, szThumbnail.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(thumbnail);
                Brush backBrush = new SolidBrush(ThumbnailBackColor);
                g.FillRectangle(backBrush, new Rectangle(new Point(0, 0), szThumbnail));
                backBrush.Dispose();
                float aspectRatio = (float)img.Width / (float)img.Height;
                SizeF szThumbImg;
                PointF ptThumbImg;
                if (aspectRatio > 1)
                {
                    szThumbImg = new SizeF(szThumbnail.Width, szThumbnail.Height / aspectRatio);
                    ptThumbImg = new PointF(0, (szThumbnail.Height - szThumbImg.Height) / 2);
                }
                else
                {
                    szThumbImg = new SizeF(szThumbnail.Width * aspectRatio, szThumbnail.Height);
                    ptThumbImg = new PointF((szThumbnail.Width - szThumbImg.Width) / 2, 0);
                }
                RectangleF thumbImgRect = new RectangleF(ptThumbImg, szThumbImg);
                g.DrawImage(img, thumbImgRect);
                g.Dispose();
                _theThumbnail = thumbnail;
                _ThumbnailImageValid = true;
            }
        }
    }


    public class CImageManager
    {
        public static Color TransparentColor = Color.FromArgb(0, 0, 0, 0);
        /// <summary>
        /// the ImagesFileBlock object provides a (hopefully) transparent layer between the image IDs
        /// stored in the CDisplayItem derived classes and either the image files or the save file
        /// from which they are loaded.  It provides for the generation of thumbnail images in worker threads
        /// and for resizing images to the size they appear in the IAT.  It (should) allow for images to be 
        /// retrieved while the image sizing work is being done on the load of a save file, irrespective
        /// of whether the image has been sized yet, forcing an immediate resize on any image that has
        /// yet to be sized in the background
        /// </summary>
        private static Dictionary<int, CNonUserImage> NonUserImages;
        private static Dictionary<int, CUserImage> UserImages;
        private static String Filename;
        private static object fileLock = new object();
        public const long MaxImageFileSize = (1 << 19) * 3;
        private static CTempImgFile TempImgFile = new CTempImgFile();

        /// <summary>
        /// returns a unique key for a new item in the image dictionary
        /// </summary>
        /// <returns></returns>
        private int GetImageDictionaryID()
        {
            int ctr = 0;
            while ((UserImages.ContainsKey(ctr)) || (NonUserImages.ContainsKey(ctr)))
                ctr++;
            return ctr;
        }

        /// <summary>
        /// the no-arg constructor
        /// </summary>
        public CImageManager()
        {
            UserImages = new Dictionary<int, CUserImage>();
            NonUserImages = new Dictionary<int, CNonUserImage>();
            Filename = String.Empty;
            fileLock = new object();
        }

        /// <summary>
        /// adds an image to the image dictionary
        /// </summary>
        /// <param name="fullFilePath">the full file-path of the image</param>
        /// <returns>returns the ID of the image added to the image dictionary</returns>
        public int AddImage(String fullFilePath)
        {
            int id = -1;
            if (fullFilePath == String.Empty)
                throw new Exception("An empty string was passed to the ImagesFileBlock as a file path.");
            foreach (int i in UserImages.Keys)
            {
                if (UserImages[i].FullFilePath == fullFilePath)
                {
                    UserImages[i].IncrementInstances();
                    return i;
                }
            }
            id = GetImageDictionaryID();
            CUserImage ii = new CUserImage(fullFilePath);
            ii.UID = id;
            UserImages.Add(id, ii);
            return id;
        }

        public int AddNonUserImage(Image img, Size ThumbnailSize, Color ThumbnailBackColor)
        {
            int id = GetImageDictionaryID();
            CNonUserImage ii = new CNonUserImage(img, ThumbnailSize, ThumbnailBackColor);
            ii.UID = id;
            NonUserImages.Add(id, ii);
            return id;
        }

        /// <summary>
        /// called to determine if the images file block contains a specified image
        /// </summary>
        /// <param name="filePath">the full file path of the image that is being tested for inclusion in the image dictionary</param>
        /// <returns>true if the images file block contains the image, otherwise false</returns>
        public bool Includes(String filePath)
        {
            foreach (int i in UserImages.Keys)
            {
                if ((UserImages[i].FullFilePath != String.Empty) && (UserImages[i].FullFilePath == filePath))
                    return true;
            }
            return false;
        }

        /// <summary>
        /// returns the full file path of an image in the images file block
        /// </summary>
        /// <param name="ndx">the index of the image in the images file block</param>
        /// <returns>the full file path of the image</returns>
        public String GetFilePath(int ndx)
        {
            if (!UserImages.ContainsKey(ndx))
                throw new Exception("The hash table for the image block of the save file does not contain a key with the supplied value.");
            return UserImages[ndx].FullFilePath;
        }


        /// <summary>
        /// compacts the image dictionary, removing all entries for images with zero instances remaining in the IAT
        /// </summary>
        private void CompactImageDictionary()
        {
            List<int> keyList = new List<int>();
            foreach (int i in UserImages.Keys)
            {
                if (UserImages[i].NumInstances == 0)
                {
                    UserImages[i].FreeTempData();
                    keyList.Add(i);
                }
            }
            foreach (int i in keyList)
                UserImages.Remove(i);
            keyList.Clear();
            foreach (int i in NonUserImages.Keys)
            {
                if (NonUserImages[i].NumInstances == 0)
                {
                    NonUserImages[i].FreeTempData();
                    keyList.Add(i);
                }
            }
            foreach (int i in keyList)
                NonUserImages.Remove(i);

        }

        /// <summary>
        /// loads the specified image in the images file block
        /// thread-safe
        /// </summary>
        /// <param name="ndx">the index of the image in the images file block</param>
        /// <param name="GenerateThumbnail">a value of true will cause the function to generate a thumbnail of the image</param>
        /// <returns>a System.Drawing.Image object that contains the image data</returns>
        public static Image LoadImage(int ndx)
        {
            Image img;
            if (NonUserImages.ContainsKey(ndx))
                return CTempImgFile.GetImage(NonUserImages[ndx].TempImageID);
            if (UserImages[ndx].OffsetInFile == -1)
            {
                try
                {
                    img = Image.FromFile(UserImages[ndx].FullFilePath);
                    if (!UserImages[ndx].ThumbnailImageValid)
                        UserImages[ndx].GenerateThumbnailInBackground((Image)img.Clone());
                    return img;
                }
                catch (Exception)
                {
                    throw new Exception("Attempt made to load an image that neither exists in the save file nor has a valid file path.");
                }
            }
            lock (fileLock)
            {
                if (!File.Exists(Filename))
                    return null;
                FileStream fStream = new FileStream(Filename, FileMode.Open, FileAccess.Read);
                BinaryReader bReader = new BinaryReader(fStream);
                bReader.BaseStream.Seek(UserImages[ndx].OffsetInFile, SeekOrigin.Begin);
                int nLen = bReader.ReadInt32();
                MemoryStream memStream = new MemoryStream(bReader.ReadBytes(nLen), 0, nLen);
                img = Image.FromStream(memStream);
                memStream.Dispose();
                bReader.Close();
                return img;
            }
        }

        /// <summary>
        /// gets a dictionary collection of thumbnail images for the images contained in the image dictionary
        /// </summary>
        /// <returns>the dictionary collection</returns>
        public Dictionary<int, Image> GetThumbnails()
        {
            Dictionary<int, Image> result = new Dictionary<int, Image>();
            foreach (int i in UserImages.Keys)
                result.Add(i, UserImages[i].theThumbnail);
            return result;
        }

        /// <summary>
        /// gets the description of an image in the images file block
        /// </summary>
        /// <param name="ndx">the index of the image to get the description of</param>
        /// <returns>the description of the image</returns>
        public String GetImageDescription(int ndx)
        {
            if (!UserImages.ContainsKey(ndx))
                throw new Exception("Attempt made to retrieve the description of an image that does not exist in the save file dictionary.");
            return UserImages[ndx].Description;
        }

        /// <summary>
        /// saves the image file block to a binary writer
        /// thread-safe
        /// </summary>
        /// <param name="bWriter">the binary writer to save the images file block to</param>
        /// <param name="filename">the file that the binary writer outputs to</param>
        /// <returns>true on success, otherwise false</returns>
        public bool Save(BinaryWriter bWriter, String filename)
        {
            lock (fileLock)
            {
                Filename = filename;
                FileStream inStream = null;
                BinaryReader bReader = null;
                try
                {
                    bool bPreSaveFileExists = false;
                    foreach (int i in UserImages.Keys)
                        if (UserImages[i].OffsetInFile != -1)
                            bPreSaveFileExists = true;
                    if (bPreSaveFileExists)
                    {
                        inStream = new FileStream(Properties.Resources.sImageDictionaryTempBackupFileName, FileMode.Open);
                        bReader = new BinaryReader(inStream);
                    }
                    bWriter.Write(Convert.ToInt32(UserImages.Count));
                    foreach (int i in UserImages.Keys)
                    {
                        bWriter.Write(Convert.ToInt32(i));
                        bWriter.Write(UserImages[i].FileName);
                        bWriter.Write(Convert.ToInt32(UserImages[i].NumInstances));
                        bWriter.Flush();
                        if (UserImages[i].OffsetInFile == -1)
                        {
                            UserImages[i].OffsetInFile = bWriter.BaseStream.Position;
                            Image img = Image.FromFile(UserImages[i].FullFilePath);
                            MemoryStream memStream = new MemoryStream();
                            System.Drawing.Imaging.ImageFormat format;
                            String extension = System.IO.Path.GetExtension(UserImages[i].FullFilePath);
                            switch (extension.ToLower())
                            {
                                case ".jpeg":
                                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                    break;

                                case ".jpg":
                                    format = System.Drawing.Imaging.ImageFormat.Jpeg;
                                    break;

                                case ".bmp":
                                    format = System.Drawing.Imaging.ImageFormat.Bmp;
                                    break;

                                case ".tiff":
                                    format = System.Drawing.Imaging.ImageFormat.Tiff;
                                    break;

                                case ".gif":
                                    format = System.Drawing.Imaging.ImageFormat.Gif;
                                    break;

                                case ".png":
                                    format = System.Drawing.Imaging.ImageFormat.Png;
                                    break;

                                default:
                                    throw new Exception("Unrecognized image file extension.");
                            }
                            img.Save(memStream, format);
                            bWriter.Write(Convert.ToInt32(memStream.Length));
                            bWriter.Write(memStream.GetBuffer(), 0, (int)memStream.Length);
                            bWriter.Flush();
                        }
                        else
                        {
                            inStream.Seek(UserImages[i].OffsetInFile, SeekOrigin.Begin);
                            int nLen = bReader.ReadInt32();
                            MemoryStream memStream = new MemoryStream(bReader.ReadBytes(nLen), 0, nLen, false, true);
                            UserImages[i].OffsetInFile = bWriter.BaseStream.Position;
                            bWriter.Write(Convert.ToInt32(nLen));
                            bWriter.Write(memStream.GetBuffer(), 0, nLen);
                            bWriter.Flush();
                        }
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error saving images");
                    return false;
                }
                finally
                {
                    if (bReader != null)
                        bReader.Close();
                    if (File.Exists(Properties.Resources.sImageDictionaryTempBackupFileName))
                        File.Delete(Properties.Resources.sImageDictionaryTempBackupFileName);
                }
                return true;
            }
        }

        /// <summary>
        /// loads the images file block from a binary reader object
        /// </summary>
        /// <param name="bReader">the binary reader that data is input from</param>
        /// <param name="filename">the name of the file the binary reader reads from</param>
        public void Load(BinaryReader bReader, String filename)
        {
            lock (fileLock)
            {
                UserImages.Clear();
                Filename = filename;
                int nImages = bReader.ReadInt32();
                for (int ctr = 0; ctr < nImages; ctr++)
                {
                    int id = bReader.ReadInt32();
                    String imgFilename = bReader.ReadString();
                    int nInstances = bReader.ReadInt32();
                    long offset = bReader.BaseStream.Position;
                    CUserImage ii = new CUserImage(imgFilename, nInstances, offset);
                    ii.UID = id;
                    UserImages.Add(id, ii);
                    int nLen = bReader.ReadInt32();
                    bReader.BaseStream.Seek(nLen, SeekOrigin.Current);
                }
            }
        }

        /// <summary>
        /// should be called before the images file block is saved -- outputs image data for the images stored in the images file block on load
        /// necessary for saving because the images file block is presumed to be saved the the same file it was loaded from
        /// </summary>
        /// <param name="fileName">the name of the IAT save file that contains the images file block</param>
        /// <returns>true on success, false otherwise</returns>
        public bool CreatePreSaveBackup(String fileName)
        {
            lock (fileLock)
            {
                Filename = fileName;
                CompactImageDictionary();
                try
                {
                    FileStream outStream = new FileStream(Properties.Resources.sImageDictionaryTempBackupFileName, FileMode.Create);
                    FileStream inStream = new FileStream(Filename, FileMode.Open);
                    BinaryReader bReader = new BinaryReader(inStream);
                    BinaryWriter bWriter = new BinaryWriter(outStream);
                    bWriter.Write(Convert.ToInt32(UserImages.Keys.Count));
                    foreach (int i in UserImages.Keys)
                    {
                        if (UserImages[i].OffsetInFile != -1)
                        {
                            bWriter.Write(Convert.ToInt32(i));
                            bWriter.Write(UserImages[i].FileName);
                            bWriter.Write(Convert.ToInt32(UserImages[i].NumInstances));
                            bWriter.Flush();
                            inStream.Seek(UserImages[i].OffsetInFile, SeekOrigin.Begin);
                            int nLen = bReader.ReadInt32();
                            MemoryStream memStream = new MemoryStream(bReader.ReadBytes(nLen), 0, nLen, false, true);
                            UserImages[i].OffsetInFile = bWriter.BaseStream.Position;
                            bWriter.Write(Convert.ToInt32(nLen));
                            bWriter.Write(memStream.GetBuffer(), 0, nLen);
                            memStream.Dispose();
                        }
                    }
                    bReader.Close();
                    bWriter.Close();
                    outStream.Close();
                    inStream.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error occured while preprocessing save file.");
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// frees data allocated for the object, namely the temporary images that represent thumbnails of the images in the images file block
        /// </summary>
        public void Dispose()
        {
            ImageResizer.HaltFlag = true;
            while (ImageResizer.ResizerRunning)
                System.Threading.Thread.Sleep(100);
            foreach (int i in UserImages.Keys)
            {
                UserImages[i].Dispose(true);
            }
            foreach (int i in NonUserImages.Keys)
            {
                NonUserImages[i].Dispose(true);
            }
            CTempImgFile.Delete();
        }

        /// <summary>
        /// spawns worker threads to generate thumbnail images for the images in the images file block
        /// </summary>
        public void CreateThumbnails()
        {
            foreach (int i in UserImages.Keys)
                UserImages[i].GenerateThumbnail(LoadImage(i));
            //            foreach (int i in NonUserImages.Keys)
            //                NonUserImages[i].GenerateThumbnail(LoadImage(i));
        }

        /// <summary>
        /// redundant -- spanws a thread that spawns worker threads to generate thumbnail images for the images in the images file block
        /// </summary>
        public void CreateThumbnailsInBackground()
        {
            ThreadStart proc = new ThreadStart(CreateThumbnails);
            Thread thread = new Thread(proc);
            thread.Start();
        }

        private class CImageResizer
        {

            private List<IIATImage> ImagesToResize = new List<IIATImage>();
            private bool bResizerRunning = false;
            private object runningLock = new object();
            private bool bHaltFlag = false;


           

            public void AddToResizer(IIATImage i)
            {
                lock (ImagesToResize)
                {
                    ImagesToResize.Add(i);
                }
            }

            public bool HaltFlag
            {
                get
                {
                    lock (runningLock)
                    {
                        return bHaltFlag;
                    }
                }
                set
                {
                    lock (runningLock)
                    {
                        bHaltFlag = value;
                    }
                }
            }



            public bool ResizerRunning
            {
                get
                {
                    lock (runningLock)
                    {
                        return bResizerRunning;
                    }
                }
            }


            public void DoResize()
            {
                lock (runningLock)
                {
                    bResizerRunning = true;
                }

                while (!HaltFlag)
                {
                    IIATImage i;
                    lock (ImagesToResize)
                    {
                        if (ImagesToResize.Count == 0)
                        {
                            System.Threading.Thread.Sleep(100);
                            continue;
                        }
                        i = ImagesToResize[0];
                        ImagesToResize.RemoveAt(0);
                    }
                    i.PerformResize();
                }

                lock (runningLock)
                {
                    bResizerRunning = false;
                }
            }

        }

        private static CImageResizer ImageResizer = new CImageResizer();

        public bool ResizerRunning
        {
            get
            {
                return ImageResizer.ResizerRunning;
            }
        }

        public void HaltResizer()
        {
            ImageResizer.HaltFlag = true;
        }

        public static void AddImageToResizer(int ID)
        {
            if (UserImages.ContainsKey(ID))
                ImageResizer.AddToResizer(UserImages[ID]);
            if (NonUserImages.ContainsKey(ID))
                ImageResizer.AddToResizer(NonUserImages[ID]);
        }

        public static void AddImageToResizer(int ID, Size sz)
        {
            if (UserImages.ContainsKey(ID))
                UserImages[ID].Resize(sz);
            if (NonUserImages.ContainsKey(ID))
                NonUserImages[ID].Resize(sz);
        }

        public void StartResizer()
        {
            ThreadStart proc = new ThreadStart(ImageResizer.DoResize);
            Thread thread = new Thread(proc);
            thread.Start();
        }

        public IIATImage this[int ID]
        {
            get
            {
                if (UserImages.Keys.Contains(ID))
                    return UserImages[ID];
                if (NonUserImages.Keys.Contains(ID))
                    return NonUserImages[ID];
                return null;
            }
        }
    }
}
