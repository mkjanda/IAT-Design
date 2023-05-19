using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace IATClient
{
    public class CSurveyItemImage : CSurveyItem, IPackagePart
    {
        public override SurveyItemType ItemType { get { return SurveyItemType.SurveyImage; } }
        private String SurveyImageRel { get; set; } = null;
        private Uri _SurveyImageUri = null;
        public Uri SurveyImageUri
        {
            get
            {
                if (_SurveyImageUri == null)
                    _SurveyImageUri = CIAT.SaveFile.GetRelationship(URI, SurveyImageRel).TargetUri;
                return _SurveyImageUri;
            }
        }
        private DISurveyImage _SurveyImage = null;
        public DISurveyImage SurveyImage
        {
            get
            {
                if (_SurveyImage == null)
                    _SurveyImage = CIAT.SaveFile.GetDI(SurveyImageUri) as DISurveyImage;
                return _SurveyImage;

            }
        }
        public override String MimeType { get { return "text/xml+" + typeof(CSurveyItemImage).ToString(); } }
        public static new String sMimeType { get { return "text/xml+" + typeof(CSurveyItemImage).ToString(); } }
        private bool bIsLoading = false;
        public override bool IsImage { get { return true; } }
        public override bool IsQuestion { get { return false; } }
        public int ResourceId { get; set; }

        public CSurveyItemImage()
        {
            Optional = false;
            Response = new CInstruction();
        }

        public CSurveyItemImage(DISurveyImage di)
        {
            _SurveyImageUri = di.URI;
            SurveyImageRel = CIAT.SaveFile.CreateRelationship(BaseType, di.BaseType, URI, di.URI);
            Response = new CInstruction();
            Optional = false;
        }

        public CSurveyItemImage(Uri u) : base(u)
        {
            _SurveyImageUri = SurveyImage.URI;
            Response = new CInstruction();
            Optional = false;
        }

        public override Control GenerateItemPreviewPanel(int width, Color backColor, Color foreColor)
        {
            SurveyImageDisplay p = new SurveyImageDisplay();
            double ar = (double)SurveyImage.IImage.Size.Width / (double)SurveyImage.IImage.Size.Height;
            p.Size = new Size(width, Math.Min(width, (int)(width / ar)));
            p.BackColor = backColor;
            p.ForeColor = foreColor;
            p.Tag = this;
            SurveyImage.PreviewPanel = p;
            return p;
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("SurveyItemImage"));
            if (SurveyImageRel == null)
                SurveyImageRel = CIAT.SaveFile.CreateRelationship(BaseType, SurveyImage.BaseType, URI, SurveyImageUri);
            xDoc.Root.Add(AsXElement());
            xDoc.Root.Add(base.AsXElement());
            Stream s = Stream.Synchronized(CIAT.SaveFile.GetWriteStream(this));
            try
            {
                xDoc.Save(s);
            }
            finally
            {
                s.Dispose();
                CIAT.SaveFile.ReleaseWriteStreamLock();
            }
        }

        public override XElement AsXElement() { return new XElement(GetType().ToString(), new XAttribute("rId", SurveyImageRel)); }

        protected override void Load()
        {
            Stream s = Stream.Synchronized(CIAT.SaveFile.GetReadStream(this));
            XDocument xDoc;
            try
            {
                xDoc = XDocument.Load(s);
            }
            finally
            {
                CIAT.SaveFile.ReleaseReadStreamLock();
                s.Dispose();
            }
            SurveyImageRel = xDoc.Root.Element(GetType().ToString()).Attribute("rId").Value;
            _SurveyImageUri = CIAT.SaveFile.GetRelationship(this, SurveyImageRel).TargetUri;
            base.Load(xDoc.Root.Element(BaseType.ToString()));
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("SurveyImage");
            writer.WriteAttributeString("Image", "True");
            writer.WriteAttributeString("Optional", "False");
            writer.WriteElementString("MimeType", SurveyImage.IImage.OriginalImage.ImageFormat.MimeType);
            var memStream = new MemoryStream();
            var bmp = SurveyImage.IImage.OriginalImage.Img;
            bmp.Save(memStream, SurveyImage.IImage.OriginalImage.ImageFormat.Format);
            writer.WriteElementString("ImageData", Convert.ToBase64String(memStream.ToArray()));
            writer.WriteElementString("ResourceId", ResourceId.ToString());
            writer.WriteEndElement();
        }

        public void WritePreviewXml(XmlWriter writer)
        {
            writer.WriteStartElement("SurveyImage");
            writer.WriteAttributeString("Image", "True");
            writer.WriteAttributeString("Optional", "False");
            Images.IImage surveyImage = CIAT.SaveFile.GetIImage(SurveyImage.IImage.URI);
            writer.WriteElementString("MimeType", surveyImage.MimeType);
            MemoryStream imgStream = new MemoryStream();
            Image img = SurveyImage.IImage.Img;
            if (img.Width > 500)
            {
                Bitmap bmp = new Bitmap(img, new Size(500, img.Height * 500 / img.Width));
                bmp.Save(imgStream, SurveyImage.IImage.ImageFormat.Format);
                bmp.Dispose();
            }
            else
                img.Save(imgStream, SurveyImage.IImage.ImageFormat.Format);
            img.Dispose();
            writer.WriteElementString("ImageData", Convert.ToBase64String(imgStream.ToArray()));
            imgStream.Dispose();
            writer.WriteElementString("Id", "survey-image-" + GetItemIndex().ToString());
            writer.WriteEndElement();
            Response = new CInstruction();
        }

        public override object Clone()
        {
            CSurveyItemImage si = new CSurveyItemImage();
            si.URI = CIAT.SaveFile.CreatePart(BaseType, typeof(CSurveyItem), MimeType, ".xml");
            si._SurveyImageUri = (SurveyImage.Clone() as DISurveyImage).URI;
            CIAT.SaveFile.Register(si);
            return si;
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            base.Dispose();
            CIAT.SaveFile.GetDI(SurveyImageUri).Dispose();
        }
    }
}
