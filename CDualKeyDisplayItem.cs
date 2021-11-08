using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;

namespace IATClient
{
    /// <summary>
    /// NOTE: THIS CLASS IS ONLY MEANT FOR USE BY CIATDualKey
    /// CDualKeyDisplayItem provides limited functionality for a display item that has multiple child items.
    /// These child items must be specified at construction.  This class will throw an exception if either
    /// WriteToXml, ReadFromXml, or the default constructor is invoked.
    /// </summary>
    class CDualKeyDisplayItem : CDisplayItem, IComponentImageSource, ImageManager.INonUserImageSource
    {
        public enum ELayout { horizontal, vertical};

        private CDisplayItem []ChildItems = new CDisplayItem[3];
        private ELayout Layout;
        private int _Padding;
        private bool bCompositeImageValid = false;
        private bool bIsMultipleUpdating = false;
        private Control InvokeTarget = null;
        private System.Threading.Timer timer = null;
        private bool bComponentImageValid = false;
        private bool []ChildImageValid = { false, false, false };

        public override CComponentImage.ESourceType SourceType
        {
            get
            {
                return CComponentImage.ESourceType.responseKey;
            }
        }
        
        
        public void SetInvokeTarget(Control invokeTarget)
        {
            InvokeTarget = invokeTarget;
        }

        private bool bHalt = false;

        public bool HaltFlag
        {
            get
            {
                Lock();
                bool b = bHalt;
                Unlock();
                return b;
            }
            set
            {
                Lock();
                bHalt = value;
                Unlock();
            }
        }

        public void InvalidateKeyPreviewSource()
        {
            InvalidateDualKeyImage();
        }
        /*
        public void OpenForDisplay()
        {
            HaltFlag = false;
            ThreadStart proc = new ThreadStart(KeyPreviewUpdateProc);
            Thread th = new Thread(proc);
            th.Start();
        }
        
        private void KeyPreviewUpdateProc()
        {
            while (!HaltFlag)
            {
                lock (PreviewValidLock)
                {
                    if (!PreviewValid)
                    {
                        IATImage.Lock();
                        Lock();
                        Size sz = IATImage.ImageSize;
                        double r1 = (double)KeyPreviewPanel.PreviewSize.Width / (double)sz.Width;
                        double r2 = (double)KeyPreviewPanel.PreviewSize.Height / (double)sz.Height;
                        Image img;
                        Size szFinal = Size.Empty;
                        if (r1 > r2)
                            szFinal = new Size((int)((double)KeyPreviewPanel.PreviewSize.Height * (double)sz.Width / (double)sz.Height),
                                KeyPreviewPanel.PreviewSize.Height);
                        else
                            szFinal = new Size(KeyPreviewPanel.PreviewSize.Width, (int)((double)KeyPreviewPanel.PreviewSize.Width *
                                (double)sz.Height / (double)sz.Width));
                        PreviewValid = true;
                        Unlock();
                        img = new Bitmap(IATImage.theImage);
                        IATImage.Unlock();
                        Image finalImg = new Bitmap(img, szFinal);
                        if (!HaltFlag)
                            InvokeTarget.Invoke(OnUpdate, finalImg);
                        img.Dispose();
                        finalImg.Dispose();
                    }
                }
                Thread.Sleep(50);
            }
            if ((InvokeTarget.IsHandleCreated)  && (!IATConfigMainForm.IsInShutdown))
                InvokeTarget.Invoke(OnUpdate, (Object)null);
        }
        
        public void CloseForDisplay()
        {
            HaltFlag = true;
        }
        */
        private object PreviewValidLock = new object();
        private bool _PreviewValid = false;

        public bool PreviewValid
        {
            get
            {
                lock (PreviewValidLock)
                {
                    bool b = _PreviewValid;
                    for (int ctr = 0; ctr < ChildItems.Length; ctr++)
                        if (ChildItems[ctr] != null)
                            if (!ChildItems[ctr].IsValid)
                                return false;
                    return b;
                }
            }
            set
            {
                lock (PreviewValidLock)
                    _PreviewValid = value;
            }
        }

        public override bool IsValid
        {
            get
            {
                Lock();
                bool b = bComponentImageValid;
                if (ChildItems[0] != null)
                    if (ChildItems[0].IsValid == false)
                    {
                        ChildImageValid[0] = false;
                        b = false;
                    }
                if (ChildItems[1] != null)
                    if (ChildItems[1].IsValid == false)
                    {
                        b = false;
                        ChildImageValid[1] = false;
                    }
                if (ChildItems[2] != null)
                    if (ChildItems[2].IsValid == false)
                    {
                        b = false;
                        ChildImageValid[2] = false;
                    }
                bComponentImageValid = b;
                Unlock();
                return b;
            }
        }



        public override void Validate()
        {
            Lock();
            bool bAllValid = true;
            for (int ctr = 0; ctr < 3; ctr++)
            {
                if (!ChildImageValid[ctr] && (ChildItems[ctr] != null))
                {
                    ChildImageValid[ctr] = true;
                    ChildItems[ctr].Validate();
                }
                else if (ChildItems[ctr] != null)
                    if (!ChildItems[ctr].IsValid)
                        bAllValid = false;
            }
            if (bAllValid)
                bComponentImageValid = true;
            Unlock();
        }

        public override void Invalidate()
        {
            Lock();
            bComponentImageValid = false;
            for (int ctr = 0; ctr < 3; ctr++)
                if (ChildItems[ctr] != null)
                    ChildItems[ctr].Invalidate();
            IATImage.Invalidate();
            Unlock();
        }

        public void InvalidateDualKeyImage()
        {
            Lock();
            ((INonUserImage)IATImage).Invalidate(this);
            Unlock();
        }
        
        public CIATDualKey.ItemLocationCallback GetDualKeyChildItemRects;
        public CIATDualKey.LayoutCallback LayoutCallback;

        /// <summary>
        /// gets the list of child item locations
        /// </summary>
        public List<Rectangle> ChildItemLocations
        {
            get
            {
                Lock();
                List<Rectangle> result;
                if (GetDualKeyChildItemRects != null)
                    result = GetDualKeyChildItemRects();
                else
                    throw new Exception("No callback provided to retrieve child item rectangles");
                Unlock();
                return result;
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public CDualKeyDisplayItem(CIATDualKey.ItemLocationCallback sizeCallback, CIATDualKey.LayoutCallback layoutCallback, ELayout Layout)
            : base(EType.dualKey)
        {
            ChildItems[0] = null;
            ChildItems[1] = null;
            ChildItems[2] = null;
            GetDualKeyChildItemRects = sizeCallback;
            LayoutCallback = layoutCallback;
            this.Layout = Layout;
            _Padding = 0;
            _IATImage = CIAT.ImageManager.AddNonUserImage(this, false);
        }

        /// <summary>
        /// This constructor instantiates a CDualKeyDisplayItem with a list of child items, a layout mode, and a padding value
        /// </summary>
        /// <param name="Layout">The layout mode</param>
        /// <param name="Padding">The padding value</param>
        /// <param name="ChildItems">The array of child items</param>
        public CDualKeyDisplayItem(CIATDualKey.ItemLocationCallback LocationCallback, CIATDualKey.LayoutCallback layoutCallback, ELayout Layout, int Padding, CDisplayItem Key1Value, CDisplayItem Conjunction, CDisplayItem Key2Value)
            : base(EType.dualKey)
        {
            GetDualKeyChildItemRects = (CIATDualKey.ItemLocationCallback)LocationCallback;
            this.Layout = Layout;
            this._Padding = Padding;
            ChildItems[0] = Key1Value;
            ChildItems[1] = Conjunction;
            ChildItems[2] = Key2Value;
            _IATImage = CIAT.ImageManager.AddNonUserImage(this, false);
            LayoutCallback = layoutCallback;
        }

        public int Padding
        {
            get
            {
                Lock();
                int p = _Padding;
                Unlock();
                return p;
            }
            set
            {
                Lock();
                _Padding = value;
                if (!bIsMultipleUpdating)
                {
                    ((INonUserImage)IATImage).Invalidate();
                    bComponentImageValid = false;
                }
                Unlock();
            }
        }
        /*
        public void InvalidateKeyValue()
        {
            ((INonUserImage)IATImage).Invalidate(this);
        }
        */
        public void BeginMultipleUpdate()
        {
            Lock();
            bIsMultipleUpdating = true;
            Unlock();
        }

        public void EndMultipleUpdate()
        {
            Lock();
            ((INonUserImage)IATImage).Invalidate();
            bComponentImageValid = false;
            Unlock();
        }

        public CDisplayItem Key1Value
        {
            get
            {
                Lock();
                CDisplayItem di = ChildItems[0];
                Unlock();
                return di;
            }
            set
            {
                Lock();
                ChildItems[0] = value;
                if (!bIsMultipleUpdating)
                {
                    ((INonUserImage)IATImage).Invalidate(this);
                    bComponentImageValid = false;
                }
                Unlock();
            }
        }

        public CDisplayItem Key2Value
        {
            get
            {
                Lock();
                CDisplayItem di = ChildItems[2];
                Unlock();
                return di;
            }
            set
            {
                Lock();
                ChildItems[2] = value;
                if (!bIsMultipleUpdating)
                {
                    ((INonUserImage)IATImage).Invalidate();
                    bComponentImageValid = false;
                }
                Unlock();
            }
        }


        public CDisplayItem Conjunction
        {
            get
            {
                Lock();
                CDisplayItem di = ChildItems[1];
                Unlock();
                return di;
            }
            set
            {
                Lock();
                ChildItems[1] = value;
                if (!bIsMultipleUpdating)
                {
                    ((INonUserImage)IATImage).Invalidate();
                    bComponentImageValid = false;
                }
                Unlock();
            }
        }


        /// <summary>
        /// gets the minimum size of the item, if all items are displayed overlapped
        /// </summary>
        /// <returns>The maximum width and height among the child items</returns>
        protected Size GetMaxChildSize()
        {
            Size sz = new Size(0, 0);
            Size szChild;
            for (int ctr = 0; ctr < ChildItems.Length; ctr++)
            {
                if (ChildItems[ctr] == null)
                    continue;
                szChild = ChildItems[ctr].ItemSize;
                if (szChild.Width > sz.Width)
                    sz.Width = szChild.Width;
                if (szChild.Height > sz.Height)
                    sz.Height = szChild.Height;
            }
            return sz;
        }

        /// <summary>
        /// Positions the child items horizontally
        /// </summary>
        /// <returns>The size of the multi-item</returns>
        private Size PerformHorizontalLayout()
        {
            if ((Key1Value == null) && (Conjunction == null) && (Key2Value == null))
                return Size.Empty;
            int Height = GetMaxChildSize().Height;
            int Width = ChildItemLocations[0].Width;
            for (int ctr = 1; ctr < ChildItemLocations.Count; ctr++)
                Width += ChildItemLocations[ctr].Width + Padding;

            return new Size(Width, Height);
        }

        /// <summary>
        /// Positions the child items vertically
        /// </summary>
        /// <returns>The size of the multi-item</returns>
        private Size PerformVerticalLayout()
        {
            int Width = GetMaxChildSize().Width;
            int Height = ChildItemLocations[0].Height;
            for (int ctr = 1; ctr < ChildItemLocations.Count; ctr++)
                Height += ChildItemLocations[ctr].Height + Padding;

            return new Size(Width, Height);
        }
        
        /// <summary>
        /// Positions the child items
        /// </summary>
        /// <returns>The size of the multi-item</returns>
        protected Size PerformLayout()
        {
            Lock();
            Size sz;
            switch (Layout)
            {
                case ELayout.horizontal:
                    sz = PerformHorizontalLayout();
                    break;

                case ELayout.vertical:
                    sz = PerformVerticalLayout();
                    break;

                default:
                    Unlock();
                    throw new Exception();
            }
            Unlock();
            return sz;
        }

        /// <summary>
        /// Performs no work.  Child items are disposed of on their own.
        /// </summary>

        /// <summary>
        /// Gets the size of the item -- CMultiItemDisplayItems are only used by CIATDualKey objects and are laid out to take up the entirity
        /// of the response key value rectangle
        /// </summary>
        /// <returns>The size of the multi-item</returns>
        protected override Size GetItemSize()
        {
            return CIAT.Layout.LeftKeyValueRectangle.Size;
        }

        /// <summary>
        /// Draws the child items at the specified location
        /// </summary>
        /// <param name="g">The graphics context</param>
        /// <param name="location">The upper-left corner to draw the child items at</param>
        /// <returns>"true" on success, "false" on error</returns>
        /*
        public override bool Display(Graphics g, Point location)
        {
            if (!IsValid())
                return false;
            PerformLayout();
            
            // display each child item, offsetting the display coordinates by the upper-left hand coordinate of the bounding rectangle
            // for text items
            Point ptDisplay = new Point();
            for (int ctr = 0; ctr < ChildItems.Count; ctr++) {
                ptDisplay.X = location.X + ChildItemLocations[ctr].X;
                ptDisplay.Y = location.Y + ChildItemLocations[ctr].Y;
                if (ChildItems[ctr].Type == EType.text)
                {
                    ptDisplay.X -= ((CTextDisplayItem)ChildItems[ctr]).GetBoundingRectangle().Left;
                    ptDisplay.Y -= ((CTextDisplayItem)ChildItems[ctr]).GetBoundingRectangle().Top;
                }
                ChildItems[ctr].Display(g, ptDisplay);
            }
            return true;
        }
        */

        protected void DrawTextDisplayItem(Graphics g, CTextDisplayItem tdi, Point loc)
        {
            Brush br = new SolidBrush(tdi.PhraseColor);
            Font f = tdi.PhraseFont;
            g.DrawString(tdi.Phrase, f, br, loc);
            br.Dispose();
        }

        protected void DrawImageDisplayItem(Graphics g, CImageDisplayItem idi, Point loc)
        {
            lock (idi.IATImage.LockObject)
            {
                Image img = idi.IATImage.theImage;
                g.DrawImage(img, loc);
                img.Dispose();
            }
        }

        public bool CompositeImageValid
        {
            get
            {
                Lock();
                bool b = bCompositeImageValid;
                Unlock();
                return b;
            }
            set
            {
                Lock();
                bCompositeImageValid = value;
                Unlock();
            }
        }

        public Dictionary<IComponentImageSource, Rectangle> ComponentImages
        {
            get
            {
                Lock();
                Dictionary<IComponentImageSource, Rectangle> result = new Dictionary<IComponentImageSource, Rectangle>();
                for (int ctr = 0; ctr < ChildItems.Length; ctr++)
                    result[ChildItems[ctr]] = ChildItemLocations[ctr];
                Unlock();
                return result;
            }
        }
        /*
        public CCompositeImage GenerateCompositeImage(Size szPreviewWin, Control previewWin, CCompositeImage.ImageGeneratedHandler callback)
        {
            return new CCompositeImage(CIAT.Layout.KeyValueSize, szPreviewWin, false, previewWin, this, callback);
        }
        */
        public Image GenerateImage()
        {
            Lock();
            if (!IsValid)
                LayoutCallback();
            Image i = new Bitmap(CIAT.Layout.KeyValueSize.Width, CIAT.Layout.KeyValueSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(i);
            Brush backBrush = new SolidBrush(CIAT.Layout.BackColor);
            g.FillRectangle(backBrush, new Rectangle(0, 0, CIAT.Layout.KeyValueSize.Width, CIAT.Layout.KeyValueSize.Height));
            backBrush.Dispose();
            Point ptDisplay = new Point();
            for (int ctr = 0; ctr < ChildItems.Length; ctr++)
            {
                if (ChildItems[ctr] == null)
                    continue;
                ptDisplay.X = ChildItemLocations[ctr].X;
                ptDisplay.Y = ChildItemLocations[ctr].Y;
                lock (ChildItems[ctr].IATImage.LockObject)
                {
                    Image childImg = ChildItems[ctr].IATImage.theImage;
                    g.DrawImage(childImg, ChildItemLocations[ctr].Location);
                    childImg.Dispose();
                }
            }
            g.Dispose();
            Unlock();
            return i;
        }

        public Image TryGenerateImage()
        {
            if (!TryLock())
                return null;
            Image img = GenerateImage();
            Unlock();
            return img;
        }


        /// <summary>
        /// Determines if the object's data is valid
        /// </summary>
        /// <returns>"true" if the object's data is valid, otherwise "false"</returns>
        public override bool IsDefined()
        {
            return true;
        }

        public override bool LoadFromXml(System.Xml.XmlNode node)
        {
            throw new Exception("CDualKeyDisplayItem cannot be loaded from XML");
        }

        public override void WriteToXml(System.Xml.XmlTextWriter writer)
        {
            throw new Exception("CDualKeyDisplayItem should not be written to a file");
        }

        public Size GetContainerSize()
        {
            return CIAT.Layout.KeyValueSize;
        }
    }
}
