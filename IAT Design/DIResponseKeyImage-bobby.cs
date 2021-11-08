using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    class DIResponseKeyImage : DIImage
    {
        private Dictionary<CIATKey, Size> OwnerSizes = new Dictionary<CIATKey, Size>();


        public DIResponseKeyImage() : base(CIAT.Layout.KeyValueSize)  { }

        public override string MimeType
        {
            get
            {
                return "text/xml+" + typeof(DIResponseKeyImage).ToString();
            }
        }
        
        private int MinimalWidth
        {
            get
            {
                if (OwnerSizes.Count == 0)
                    return CIAT.Layout.KeyValueSize.Width; ;
                return OwnerSizes.OrderBy(kv => kv.Value.Width).First().Value.Width;
            }
        }
        private int MinimalHeight
        {
            get
            {
                if (OwnerSizes.Count == 0)
                    return CIAT.Layout.KeyValueSize.Height;
                return OwnerSizes.OrderBy(kv => kv.Value.Height).First().Value.Height;
            }
        }

        public override bool IsDefined { get; protected set;}


        public bool IsScaled
        {
            get
            {
                return OwnerSizes.Count > 0;
            }
        }

        public override void LoadImageFromFile(string path)
        {
            lock (lockObject)
            {
                base.LoadImageFromFile(path);
                foreach (CIATDualKey dk in OwnerSizes.Select(kv => kv.Key).Where(k => k is CIATDualKey).Cast<CIATDualKey>())
                    dk.ResizeValues();
                Invalidate();
            }
        }

        public void ReleaseOwner(CIATKey key)
        {
            lock (lockObject)
            {
                int width = MinimalWidth;
                int height = MinimalHeight;
                OwnerSizes.Remove(key);
                if ((width < MinimalWidth) || 
                    
                    
                    zzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzzza(height < MinimalHeight))
                    this.IImage.Resize(new Size(MinimalWidth, MinimalHeight));
            }
        }

        public Size AddOwner(CIATKey key, Size sz)
        {
            lock (lockObject)
            {
                OwnerSizes[key] = sz;
                Size boundingSize = new Size(MinimalWidth, MinimalHeight);
                if ((boundingSize.Width > OriginalSize.Width) && (boundingSize.Height > OriginalSize.Height))
                    return OriginalSize;
                double boundingAR = (double)boundingSize.Width / (double)boundingSize.Height;
                Size newSize;
                if (boundingAR > AspectRatio)
                    newSize = new Size(this.IImage.OriginalSize.Width * boundingSize.Height / this.IImage.OriginalSize.Height, boundingSize.Height);
                else
                    newSize = new Size(boundingSize.Width, this.IImage.OriginalSize.Height * boundingSize.Width / this.IImage.OriginalSize.Width);
                this.IImage.Resize(newSize);
                return newSize;
            }
        }


        public override void Load(Uri uri)
        {
            OwnerSizes.Clear();
            PackagePart part = CIAT.SaveFile.GetPart(uri);
            Stream s = part.GetStream(FileMode.Open, FileAccess.Read);
            XDocument xDoc = XDocument.Load(s);
            s.Close();
            Description = xDoc.Root.Element(XName.Get("Description")).Value;
            XElement imageElem = xDoc.Root.Element(XName.Get("Image"));
            String imageRel = imageElem.Attribute(XName.Get("rId")).Value;
            PackageRelationship rel = CIAT.SaveFile.GetRelationship(imageRel);
            PackagePart imgPart = CIAT.SaveFile.GetPart(rel.TargetUri);
            s = imgPart.GetStream();
            Image img = Image.FromStream(s);
            s.Close();
            SetImage(img);
            foreach (XElement ownerElem in xDoc.Root.Elements(XName.Get("Owner")))
            {
                String ownerRelId = ownerElem.Attribute(XName.Get("rId")).Value;
                PackageRelationship ownerRel = CIAT.SaveFile.GetRelationship(ownerRelId);
                CIATKey owner = CIAT.SaveFile.GetResponseKey(ownerRel.SourceUri);
                Size sz = new Size(Convert.ToInt32(ownerElem.Element(XName.Get("Width"))), Convert.ToInt32(ownerElem.Element(XName.Get("Height"))));
                OwnerSizes[owner] = sz;
            }
        }

        public override void Save() {
            XDocument xDoc = new XDocument();
            XmlWriter xWriter = xDoc.CreateWriter();
            xWriter.WriteElementString("Description", Description);
            xWriter.WriteStartElement("Image");
            String rid = CIAT.SaveFile.CreateRelationship(BaseType, this.IImage.BaseType, URI, this.IImage.URI);
            xWriter.WriteAttributeString("rId", rid);
            xWriter.WriteEndElement();
            foreach (CIATKey key in OwnerSizes.Keys)
            {
                xWriter.WriteStartElement("Owner");
                xWriter.WriteAttributeString("rId", key.GetRelId(this));
                xWriter.WriteElementString("Width", OwnerSizes[key].Width.ToString());
                xWriter.WriteElementString("Height", OwnerSizes[key].Height.ToString());
                xWriter.WriteEndElement();
            }
            xWriter.WriteEndDocument();
            xWriter.Close();
            Stream s = CIAT.SaveFile.GetPart(URI).GetStream();
            xDoc.Save(s);
            s.Close();
        }
    }
}
