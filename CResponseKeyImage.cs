using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO.Packaging;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Threading;

namespace IATClient
{
    /// <summary>
    /// This class represents a response key image.  It allows for scaling by multiple response keys and maintains the minimum value
    /// it is scaled to.  Owning keys can scale the image if they set the minimum value, up to the next smallest minimum value.  Any
    /// owning key can scale the image below its current minimum value.
    /// </summary>
    public class CResponseKeyImage : CImageDisplayItem, IStoredInXml, IComponentImageSource
    {
        private List<Size> ScaledSizes;
        private List<CIATKey> Owners;
        private int MinimalWidthNdx = -1, MinimalHeightNdx = -1;
        private List<String> OwnerNames;
        private Size SizeOnLoad;
        private bool bIsOpenForDisplay = false;
        private Control InvokeTarget;
        private bool bHaltFlag = false;
        private bool _PreviewValid = false;
        private IUserImage _IATImage;

        public override IIATImage IATImage
        {
            get
            {
                return _IATImage;
            }
        }

        public override CComponentImage.ESourceType SourceType
        {
            get {
                return CComponentImage.ESourceType.responseKey;
            }
        }

        public void SetInvokeTarget(Control c)
        {
            InvokeTarget = c;
        }

        public void InvalidateKeyPreviewSource()
        {
            ThreadStart proc = new ThreadStart(InvalidatePreviewProc);
            Thread th = new Thread(proc);
            th.Start();
        }

        public void CloseForDisplay()
        {
            HaltFlag = true;
            bIsOpenForDisplay = false;
        }
        private object haltLock = new object();

        public bool HaltFlag
        {
            get
            {
                lock (haltLock)
                {
                    return bHaltFlag;
                }
            }
            set
            {
                lock (haltLock)
                {
                    bHaltFlag = value;
                }
            }
        }

        private object PreviewValidLock = new object();
        private bool bComponentImageValid = false;


        public override bool IsValid
        {
            get {
                return bComponentImageValid;
            }
        }


        public bool PreviewValid
        {
            get
            {
                lock (PreviewValidLock)
                    return _PreviewValid;
            }
            set
            {
                lock (PreviewValidLock)
                    _PreviewValid = value;
            }
        }

        private void InvalidatePreviewProc()
        {
            lock (PreviewValidLock)
                PreviewValid = false;
        }


        public override bool IsDefined()
        {
            return true;
        }

        public override void Validate()
        {
            Lock();
            bComponentImageValid = true;
            Unlock();
        }

        public override void Invalidate()
        {
            Lock();
            bComponentImageValid = false;
            Unlock();
        }

        // the scaled size of the image
        private Size _ScaledSize;

        /// <summary>
        /// gets or sets the scaled size of the image
        /// </summary>
        private Size ScaledSize 
        {
            get 
            {
                return ItemSize;
            }
            set 
            {
                IATImage.Resize(CIAT.Layout.KeyValueSize);
            }
        }

        public bool IsScaled
        {
            get 
            {
                return (Owners.Count != 0);
            }
        }

        /// <summary>
        /// gets or sets whether the image is scaled
        /// </summary>
        public override bool StretchToFit
        {
            get
            {
                Lock();
                bool b = _StretchToFit;
                Unlock();
                return b;
            }
            set
            {
                Lock();
                _StretchToFit = value;
                if ((value == false) && (IATImage != null))
                    ((IUserImage)CIAT.ImageManager[IATImageID]).Dispose();
                Unlock();
            }
        }

        /// <summary>
        /// gets the aspect ratio of the image
        /// </summary>
        public double AspectRatio
        {
            get
            {
                return (double)IATImage.OriginalImageSize.Width / (double)IATImage.OriginalImageSize.Height;
            }
        }

        public override string FullFilePath
        {
            set
            {
                Lock();
                base.FullFilePath = value;
                Invalidate();
                int ctr = 0;
                while (ctr < Owners.Count)
                {
                    if (Owners[ctr].Type == CIATKey.EType.simple)
                        ReleaseSize(Owners[ctr]);
                    else
                        ctr++;
                }
                foreach (CIATKey responseKey in Owners)
                    if (responseKey.Type == CIATKey.EType.dual)
                        ((CIATDualKey)responseKey).ResizeValues();
                Unlock();
            }
        }
        
        /// <summary>
        /// scales the response key image to have the given width, preserving the aspect ratio
        /// </summary>
        /// <param name="Width">the new width of the response key image</param>
        private void ScaleToNewWidth(int Width)
        {
            ((IUserImage)IATImage).Resize(new Size(Width, (int)(Width / AspectRatio)));
        }

        /// <summary>
        /// scales the response key image to have the given height, preserving the aspect ratio
        /// </summary>
        /// <param name="Height">the new height of the response key image</param>
        private void ScaleToNewHeight(int Height)
        {
            ((IUserImage)IATImage).Resize(new Size((int)(Height * AspectRatio), Height));
        }

        /// <summary>
        /// returns the index of the minimum width value in ScaledSizes, excluding the given index
        /// </summary>
        /// <param name="ExcludedNdx">the index to be excluded from the search</param>
        /// <returns></returns>
        private int FindMinWidthNdxExcluding(int ExcludedNdx)
        {
            int min = int.MaxValue;
            int minNdx = -1;
            for (int ctr = 0; ctr < ScaledSizes.Count; ctr++)
                if ((ScaledSizes[ctr].Width < min) && (ctr != ExcludedNdx))
                {
                    min = ScaledSizes[ctr].Width;
                    minNdx = ctr;
                }

            return minNdx;
        }

        /// <summary>
        /// returns the index of the minimum height value in ScaledSizes, excluding the given index
        /// </summary>
        /// <param name="ExcludedNdx">the index to be excluded from the search</param>
        /// <returns></returns>
        private int FindMinHeightNdxExcluding(int ExcludedNdx)
        {
            int min = int.MaxValue;
            int minNdx = -1;
            for (int ctr = 0; ctr < ScaledSizes.Count; ctr++)
                if ((ScaledSizes[ctr].Height < min) && (ctr != ExcludedNdx))
                {
                    min = ScaledSizes[ctr].Height;
                    minNdx = ctr;
                }

            return minNdx;
        }

        /// <summary>
        /// Releases the size-claim on the image held by the specified owner
        /// </summary>
        /// <param name="Owner">the owning key that is releasing the image</param>
        public void ReleaseSize(CIATKey Owner)
        {
            Lock();
            int OwnerNdx = Owners.IndexOf(Owner);
            if (MinimalWidthNdx == OwnerNdx)
                MinimalWidthNdx = FindMinWidthNdxExcluding(OwnerNdx);
            if (MinimalWidthNdx > OwnerNdx)
                MinimalWidthNdx--;
            if (MinimalHeightNdx == OwnerNdx)
                MinimalHeightNdx = FindMinHeightNdxExcluding(OwnerNdx);
            if (MinimalHeightNdx > OwnerNdx)
                MinimalHeightNdx--;
            ScaledSizes.RemoveAt(OwnerNdx);
            Owners.RemoveAt(OwnerNdx);
            Unlock();
        }

        /// <summary>
        /// scales the response key image to fit into the passed rectangle, preserving the aspect ratio
        /// </summary>
        /// <param name="rect">the rectangle that is to bound the image</param>
        /// <param name="Owner">the owning key performing the scaling operation.  if null, the image will be scaled to fit the passed
        /// rectangle provided it is smaller than the current minimal size, but the scaling key will not maintain ownership of this
        /// value and any subsequent call to ScaleToRectangle will effectively ignore this scaling attempt</param>
        public void ScaleToRectangle(Rectangle rect, CIATKey Owner)
        {
            Lock();
            ScaleToRectangle(rect, Owner, true);
            Unlock();
        }

        /// <summary>
        /// scales the response key image to fit into the passed rectangle, preserving the aspect ratio, optionally postponing the
        /// image resize
        /// </summary>
        /// <param name="rect">the rectangle that is to bound the image</param>
        /// <param name="Owner">the owning key performing the scaling operation.  if null, the image will be scaled to fit the passed
        /// rectangle provided it is smaller than the current minimal size, but the scaling key will not maintain ownership of this
        /// value and any subsequent call to ScaleToRectangle will effectively ignore this scaling attempt</param>
        /// <param name="bScaleNow">set to true if the image is to be resized immediately</param>
        public void ScaleToRectangle(Rectangle rect, CIATKey Owner, bool bScaleNow)
        {
            Lock();
            int OwnerNdx;

            // perform a scaling operation that doesn't claim ownership of the value being scaled to
            if (Owner == null)
            {
                Unlock();
                throw new Exception("Scaling requests made of response key images must have keys that own the requests.");
            }
            // if this key has never scaled the item before, add it to the owners list
            if (!Owners.Contains(Owner))
            {
                OwnerNdx = Owners.Count;
                Owners.Add(Owner);
                ScaledSizes.Add(rect.Size);
                if ((MinimalWidthNdx == -1) && (MinimalHeightNdx == -1))
                    MinimalWidthNdx = MinimalHeightNdx = 0;
            }
            OwnerNdx = Owners.IndexOf(Owner);
            double rectAspect = (double)rect.Width / (double)rect.Height;
            ScaledSizes[OwnerNdx] = (rectAspect > AspectRatio) ? new Size((int)(rect.Height * AspectRatio), rect.Height) :
                new Size(rect.Width, (int)(rect.Width / AspectRatio));
            if (OwnerNdx == MinimalWidthNdx)
            {
                int MinWidthNdx = FindMinWidthNdxExcluding(OwnerNdx);
                if (MinWidthNdx != -1)
                {
                    if (ScaledSizes[OwnerNdx].Width > ScaledSizes[MinWidthNdx].Width)
                        MinimalWidthNdx = MinWidthNdx;
                }
            }
            else if (ScaledSizes[OwnerNdx].Width < ScaledSizes[MinimalWidthNdx].Width)
                MinimalWidthNdx = OwnerNdx;
            if (OwnerNdx == MinimalHeightNdx)
            {
                int MinHeightNdx = FindMinHeightNdxExcluding(OwnerNdx);
                if (MinHeightNdx != -1)
                {
                    if (ScaledSizes[OwnerNdx].Height > ScaledSizes[MinHeightNdx].Height)
                        MinimalHeightNdx = MinHeightNdx;
                }
            }
            else if (ScaledSizes[OwnerNdx].Height < ScaledSizes[MinimalHeightNdx].Height)
                MinimalHeightNdx = OwnerNdx;

            // return before scaling if scaling is to be performed later
            if (bScaleNow == false)
            {
                IATImage.Resize(new Size(ScaledSizes[MinimalWidthNdx].Width, ScaledSizes[MinimalHeightNdx].Height));
                Invalidate();
                Unlock();
                return;
            }

            Size newSize = new Size(ScaledSizes[MinimalWidthNdx].Width, ScaledSizes[MinimalHeightNdx].Height);
            rectAspect = (double)newSize.Width / (double)newSize.Height;
            if (rectAspect > AspectRatio)
            {
                ScaleToNewHeight(newSize.Height);
            }
            else
            {
                ScaleToNewWidth(newSize.Width);
            }
            Unlock();
        }
    

        /// <summary>
        /// the default constructor
        /// </summary>
        public CResponseKeyImage()
            : base (CDisplayItem.EType.responseKeyImage)
        {
            _ScaledSize = new Size(0, 0);
            ScaledSizes = new List<Size>();
            Owners = new List<CIATKey>();
            OwnerNames = null;
            MinimalWidthNdx = -1;
            MinimalHeightNdx = -1;
            SizeOnLoad = Size.Empty;
        }
        /*
        public void SetUpdateCallback(UpdateKeyPreview update)
        {
            Update = update;
        }
        */

        public override void Load(XElement elem)
        {
            String id = elem.Attribute(XName.Get("ref", CIAT.ns)).Value;
            PackageRelationship rel = CIAT.SavePackage.GetRelationship(id);
            _IATImage = CIAT.ImageManager.AddImage()
            
        }


        /// <summary>
        /// Writes the object's data to an XmlTextWriter
        /// </summary>
        /// <param name="writer">The XmlTextWriter object to use for output</param>
        public override void WriteToXml(XmlTextWriter writer)
        {

            writer.WriteStartElement("ResponseKeyImage");
            writer.WriteStartAttribute("Type");
            writer.WriteString(sResponseKeyImage);
            writer.WriteEndAttribute();
            writer.WriteElementString("ImageID", IATImageID.ToString());
            writer.WriteElementString("StretchToFit", StretchToFit.ToString());
            writer.WriteElementString("IsScaled", IsScaled.ToString());
            writer.WriteElementString("MinimalWidth", ScaledSize.Width.ToString());
            writer.WriteElementString("MinimalHeight", ScaledSize.Height.ToString());
            writer.WriteElementString("MinimalWidthIndex", MinimalWidthNdx.ToString());
            writer.WriteElementString("MinimalHeightIndex", MinimalHeightNdx.ToString());

            // write the size owner list
            writer.WriteStartElement("SizeOwnerList");
            for (int ctr = 0; ctr < Owners.Count; ctr++)
            {
                writer.WriteStartElement("SizeOwner");
                writer.WriteElementString("OwnerName", Owners[ctr].Name);
                writer.WriteElementString("OwnerWidth", ScaledSizes[ctr].Width.ToString());
                writer.WriteElementString("OwnerHeight", ScaledSizes[ctr].Height.ToString());
                writer.WriteEndElement();
            }
            writer.WriteEndElement();

            writer.WriteEndElement();
        }
        
        public void AddOwner(CIATKey owner)
        {
            if (!Owners.Contains(owner))
                Owners.Add(owner);
        }
        
        /// <summary>
        /// Load's the object's data from an XmlNode
        /// </summary>
        /// <param name="node">The XmlNode object to load data from</param>
        /// <returns>"true" on success, "false" on error</returns>
        public override bool LoadFromXml(XmlNode node)
        {
            // ensure that node has the correct number of children
            if (node.ChildNodes.Count != 8)
                return false;

            // load the file name and directory
            int nodeCtr = 0;
            _IATImage = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
            ((IUserImage)CIAT.ImageManager[IATImageID]).SetSizeCallback(new ImageManager.ImageSizeCallback(CalcImageSize));
            StretchToFit = Convert.ToBoolean(node.ChildNodes[nodeCtr++].InnerText);
            bool bIsScaled = Convert.ToBoolean(node.ChildNodes[nodeCtr++].InnerText);
            if (bIsScaled)
            {
                // scale image to minimal size
                Size sz = new Size();
                sz.Width = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
                sz.Height = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
                SizeOnLoad = sz;

                // read in owner list data
                MinimalWidthNdx = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
                MinimalHeightNdx = Convert.ToInt32(node.ChildNodes[nodeCtr++].InnerText);
                OwnerNames = new List<String>();
                for (int ctr = 0; ctr < node.ChildNodes[nodeCtr].ChildNodes.Count; ctr++)
                {
                    OwnerNames.Add(node.ChildNodes[nodeCtr].ChildNodes[ctr].ChildNodes[0].InnerText);
                    ScaledSizes.Add(new Size(Convert.ToInt32(node.ChildNodes[nodeCtr].ChildNodes[ctr].ChildNodes[1].InnerText),
                        Convert.ToInt32(node.ChildNodes[nodeCtr].ChildNodes[ctr].ChildNodes[2].InnerText)));
                }
            }
            _IATImage.Resize(GetSizeCallback()(CIAT.ImageManager[_IATImage].OriginalImageSize, StretchToFit));
            CIATKey.IncompletelyInitializedResponseKeyImages.Add(this);

            // success
            return true;
        }
        
        /// <summary>
        /// loads the list of owners from the OwnerNames list
        /// </summary>
        public void ResolveOwners()
        {
            Lock();
            if (OwnerNames == null)
            {
                Unlock();
                return;
            }
            Owners.Clear();
            for (int ctr = 0; ctr < OwnerNames.Count; ctr++)
                Owners.Add(CIATKey.KeyDictionary[OwnerNames[ctr]]);
            OwnerNames.Clear();
            OwnerNames = null;
            CIAT.ImageManager[IATImageID].Resize(new Size(ScaledSizes[MinimalWidthNdx].Width, ScaledSizes[MinimalHeightNdx].Height));
            Unlock();
        }

        protected override Size GetItemSize()
        {
            if (CIATKey.IncompletelyInitializedResponseKeyImages.Contains(this))
                return SizeOnLoad;
            Size szResult = new Size();
            if (IsScaled)
            {
                double SizedAR = (double)ScaledSizes[MinimalWidthNdx].Width / (double)ScaledSizes[MinimalHeightNdx].Height;
                if (SizedAR > AspectRatio)
                {
                    szResult.Width = (int)(ScaledSizes[MinimalHeightNdx].Height * AspectRatio);
                    szResult.Height = ScaledSizes[MinimalHeightNdx].Height;
                }
                else
                {
                    szResult.Width = ScaledSizes[MinimalWidthNdx].Width;
                    szResult.Height = (int)(ScaledSizes[MinimalWidthNdx].Width / AspectRatio);
                }
            }
            else if (StretchToFit)
            {
                double SizedAR = (double)CIAT.Layout.KeyValueSize.Width / (double)CIAT.Layout.KeyValueSize.Height;
                if (SizedAR > AspectRatio)
                {
                    szResult.Width = (int)(CIAT.Layout.KeyValueSize.Height * AspectRatio);
                    szResult.Height = CIAT.Layout.KeyValueSize.Height;
                }
                else
                {
                    szResult.Width = CIAT.Layout.KeyValueSize.Width;
                    szResult.Height = (int)(CIAT.Layout.KeyValueSize.Width / AspectRatio);
                }
            }
            else
            {
                szResult = IATImage.OriginalImageSize;
            }

            return szResult;
        }

        public Size CalcImageSize(Size originalSize, bool StretchToFit)
        {
            if (CIATKey.IncompletelyInitializedResponseKeyImages.Contains(this))
                throw new Exception("Attempt made to scale a response key image with an uninitialized owner list.");
            Size sz;
            Lock();
            if (IsScaled)
            {
                sz = new Size(ScaledSizes[MinimalWidthNdx].Width, ScaledSizes[MinimalHeightNdx].Height);
            }
            else if (StretchToFit)
            {
                ScaledSize = ItemSize;
                sz = ScaledSize;
            }
            else
            {
                sz = originalSize;
            }
            Unlock();
            return sz;
        }

        public override void Dispose()
        {
            Lock();
            base.Dispose();
            Owners.Clear();
            ScaledSizes.Clear();
            MinimalHeightNdx = MinimalWidthNdx = -1;
            _ScaledSize = new Size(0, 0);
            StretchToFit = false;
            Unlock();
            HaltFlag = true;
        }

        protected override IATClient.ImageManager.ImageSizeCallback GetSizeCallback()
        {
            return new IATClient.ImageManager.ImageSizeCallback(CalcImageSize);
        }
    }
}
