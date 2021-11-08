using System;
using System.Xml.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Xml;
using System.IO;

namespace IATClient
{
    public class CSurveyItemImage : CSurveyItem, IPackagePart
    {
        public Uri SurveyImageUri { get; private set; }
        public DISurveyImage SurveyImage { get { return CIAT.SaveFile.GetDI(SurveyImageUri) as DISurveyImage; } }
        public override SurveyItemType ItemType { get { return SurveyItemType.SurveyImage; } }
        private String rImgId { get; set; } = String.Empty;
        public override String MimeType { get { return "text/xml+" + typeof(CSurveyItemImage).ToString(); } }
        public static new String sMimeType { get { return "text/xml+" + typeof(CSurveyItemImage).ToString(); } }
        public String OnlineFilename { get; set; } = String.Empty;
        private bool bIsLoading = false;
        public override bool IsImage { get { return true; } }

        public CSurveyItemImage()
        {
            Optional = false;
            Response = new CInstruction();
        }

        public CSurveyItemImage(DISurveyImage di)
        {
            SurveyImageUri = di.URI;
            Response = new CInstruction();
            Optional = false;
        }

        public CSurveyItemImage(Uri u) : base(u)
        {
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
            xDoc.Root.Add(AsXElement());
            xDoc.Root.Add(base.AsXElement());
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public override XElement AsXElement() { return new XElement(SurveyItemType.SurveyImage.Name, new XAttribute("ImageUri", SurveyImageUri.ToString())); }

        protected override void Load()
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Load(xDoc.Root.Element(SurveyItemType.SurveyImage.Name));
            base.Load(xDoc.Root.Element(SurveyItemType.Item.Name));
        }

        public override void Load(XElement elem)
        {
            bIsLoading = true;
            SurveyImageUri = new Uri(elem.Attribute("ImageUri").Value, UriKind.Relative);
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(sSurveyItem);
            writer.WriteAttributeString("Image", "True");
            writer.WriteAttributeString("Optional", "False");
            writer.WriteElementString("Filename", OnlineFilename);
            Response.WriteXml(writer);
            writer.WriteEndElement();
        }

        public void WritePreviewXml(XmlWriter writer)
        {
            writer.WriteStartElement(sSurveyItem);
            writer.WriteAttributeString("Image", "True");
            writer.WriteAttributeString("Optional", "False");
            Images.IImage surveyImage = CIAT.SaveFile.GetIImage(SurveyImage.IImage.URI);
            writer.WriteElementString("MimeType", surveyImage.ImageMimeType);
            MemoryStream imgStream = new MemoryStream();
            Image img = SurveyImage.IImage.Image;
            if (img.Width > 500)
            {
                Bitmap bmp = new Bitmap(img, new Size(500, img.Height * 500 / img.Width));
                bmp.Save(imgStream, SurveyImage.IImage.Format);
                bmp.Dispose();
            }
            else
                img.Save(imgStream, SurveyImage.IImage.Format);
            img.Dispose();
            writer.WriteElementString("ImageData", Convert.ToBase64String(imgStream.ToArray()));
            imgStream.Dispose();
            writer.WriteElementString("Id", "survey-image-" + GetItemIndex().ToString());
            writer.WriteEndElement();
        }
        
        public override object Clone()
        {
            CSurveyItemImage si = new CSurveyItemImage();
            si.URI = CIAT.SaveFile.CreatePart(BaseType, typeof(CSurveyItem), MimeType, ".xml");
            si.SurveyImageUri = (SurveyImage.Clone() as DISurveyImage).URI;
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
