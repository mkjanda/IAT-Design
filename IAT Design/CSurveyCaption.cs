using System;
using System.Drawing;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
using System.IO;

namespace IATClient
{
    class CSurveyCaption : CSurveyItem
    {
        public override SurveyItemType ItemType { get { return SurveyItemType.Caption; } }

        public override bool IsCaption
        {
            get
            {
                return true;
            }
        }

        public override bool IsQuestion { get { return false; } }

        public override String MimeType { get { return "text/xml+" + typeof(CSurveyCaption).ToString(); } }

        public new static String sMimeType { get { return "text/xml+" + typeof(CSurveyCaption).ToString(); } }

        public int FontSize { get; set; } = 42;

        public int BorderWidth { get; set; } = 8;

        public String FontName { get; set; } = PrivateFont.JosefinSans.FamilyName;

        public Color FontColor { get; set; } = NamedColor.UltraViolet.Color;

        public Color BackColor { get; set; } = NamedColor.StarSapphire.Color;

        public Color BorderColor { get; set; } = NamedColor.Raspberry.Color;

        public Size CaptionSize
        {
            get
            {
                Font f = new Font(FontFamily.GenericSerif, 3F / 5F * FontSize, FontStyle.Bold);
                Size sz = TextRenderer.MeasureText(Text, f);
                f.Dispose();
                return sz;
            }
        }

        public CSurveyCaption()
        {
            Text = Properties.Resources.sSurveyCaptionDefaultText;
            Response = new CInstruction();
        }

        public CSurveyCaption(Uri u) : base(u)
        {
            Response = new CInstruction();
        }


        public override void WriteXml(System.Xml.XmlWriter writer)
        {
            writer.WriteStartElement(sCaption);
            writer.WriteElementString("Text", Text);
            var fam = PrivateFont.Fonts.Where(f => f.DisplayName == FontName).Select(f => f.FontFamily).FirstOrDefault();
            Font f = null;
            if (fam == null)
                fam = PrivateFont.Fonts[1].FontFamily;
                f = new Font(fam, FontSize, FontStyle.Regular, GraphicsUnit.Pixel);

            writer.WriteElementString("TextWidth", TextRenderer.MeasureText(Text, f).Width.ToString());
            writer.WriteElementString("BorderWidth", BorderWidth.ToString());
            writer.WriteElementString("FontName", FontName);
            int height = (int)((double)FontSize / (double)fam.GetEmHeight(FontStyle.Regular) * (double)(fam.GetCellAscent(FontStyle.Regular) + fam.GetCellDescent(FontStyle.Regular)));
            writer.WriteElementString("LineHeight", height.ToString());
            writer.WriteElementString("FontSize", FontSize.ToString());
            writer.WriteElementString("FontColorR", FontColor.R.ToString("X2"));
            writer.WriteElementString("FontColorG", FontColor.G.ToString("X2"));
            writer.WriteElementString("FontColorB", FontColor.B.ToString("X2"));
            writer.WriteElementString("BackColorR", BackColor.R.ToString("X2"));
            writer.WriteElementString("BackColorG", BackColor.G.ToString("X2"));
            writer.WriteElementString("BackColorB", BackColor.B.ToString("X2"));
            writer.WriteElementString("BorderColorR", BorderColor.R.ToString("X2"));
            writer.WriteElementString("BorderColorG", BorderColor.G.ToString("X2"));
            writer.WriteElementString("BorderColorB", BorderColor.B.ToString("X2"));
            writer.WriteEndElement(); 
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement(BaseType.Name));
            xDoc.Root.Add(AsXElement());
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public override XElement AsXElement() => new XElement(GetType().ToString(), new XAttribute("rSurveyId", rSurveyId), new XElement("Text", Text), 
            new XElement("FontSize", FontSize.ToString()), new XElement("FontName", FontName), new XElement("BorderWidth", BorderWidth.ToString()), 
            new XElement("FontColor", new XAttribute("r", FontColor.R.ToString()), 
            new XAttribute("g", FontColor.G.ToString()), new XAttribute("b", FontColor.B.ToString())), new XElement("BackgroundColor", 
            new XAttribute("r", BackColor.R.ToString()), new XAttribute("g", BackColor.G.ToString()), new XAttribute("b", BackColor.B.ToString())), 
            new XElement("BorderColor", new XAttribute("r", BorderColor.R.ToString()), new XAttribute("g", BorderColor.G.ToString()),
            new XAttribute("b", BorderColor.B.ToString())));

        protected override void Load()
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Load(xDoc.Document.Root.Descendants().Where(xe => xe.Name == GetType().ToString()).FirstOrDefault());
        }

        public override void Load(XElement elem)
        {
            rSurveyId = elem.Attribute("rSurveyId").Value;
            Text = elem.Element("Text").Value;
            FontName = elem.Element("FontName").Value;
            FontSize = Convert.ToInt32(elem.Element("FontSize").Value);
            BorderWidth = Convert.ToInt32(elem.Element("BorderWidth").Value);
            FontColor = Color.FromArgb(Convert.ToInt32(elem.Element("FontColor").Attribute("r").Value), Convert.ToInt32(elem.Element("FontColor").Attribute("g").Value),
                Convert.ToInt32(elem.Element("FontColor").Attribute("b").Value));
            BackColor = Color.FromArgb(Convert.ToInt32(elem.Element("BackgroundColor").Attribute("r").Value), Convert.ToInt32(elem.Element("BackgroundColor").Attribute("g").Value),
                Convert.ToInt32(elem.Element("BackgroundColor").Attribute("b").Value));
            BorderColor = Color.FromArgb(Convert.ToInt32(elem.Element("BorderColor").Attribute("r").Value), Convert.ToInt32(elem.Element("BorderColor").Attribute("g").Value),
                Convert.ToInt32(elem.Element("BorderColor").Attribute("b").Value));
        }

        public override IATSurveyFile.SurveyItem GenerateSerializableItem(IATSurveyFile.Survey s)
        {
            IATSurveyFile.Caption caption = new IATSurveyFile.Caption(s, -1);
            caption.Text = Text;
            caption.BorderWidth = BorderWidth;
            caption.FontSize = FontSize;
            caption.BackColor.Red = BackColor.R;
            caption.BackColor.Green = BackColor.G;
            caption.BackColor.Blue = BackColor.B;
            caption.BorderColor.Red = BorderColor.R;
            caption.BorderColor.Green = BorderColor.G;
            caption.BorderColor.Blue = BorderColor.B;
            caption.FontColor.Red = FontColor.R;
            caption.FontColor.Green = FontColor.G;
            caption.FontColor.Blue = FontColor.B;

            return caption;
        }

        public override Control GenerateItemPreviewPanel(int width, System.Drawing.Color backColor, System.Drawing.Color foreColor)
        {
            Panel preview = new Panel();
            preview.BackColor = BackColor;
            Font f = new Font(FontFamily.GenericSerif, FontSize * 3F / 5F, FontStyle.Bold);
            Size szCaption = CaptionSize;
            Bitmap b = new Bitmap(width, (int)(szCaption.Height * 1.5), System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(b);
            Brush backBrush = new SolidBrush(BackColor);
            Brush textBrush = new SolidBrush(FontColor);
            Brush borderBrush = new SolidBrush(BorderColor);
            g.FillRectangle(backBrush, new Rectangle(0, 0, b.Width, b.Height));
            g.FillRectangle(borderBrush, new Rectangle(0, b.Height - BorderWidth, b.Width, b.Height));
            g.DrawString(Text, f, textBrush, new Point((b.Width - szCaption.Width) >> 1, ((b.Height - BorderWidth) - szCaption.Height) >> 1));
            borderBrush.Dispose();
            textBrush.Dispose();
            backBrush.Dispose();
            g.Dispose();
            preview.Size = new Size(b.Width, b.Height);
            preview.BackgroundImage = b;

            return preview;
        }

        public override object Clone()
        {
            CSurveyCaption o = new CSurveyCaption();
            o.FontColor = FontColor;
            o.BackColor = BackColor;
            o.BorderColor = BorderColor;
            o.BorderWidth = BorderWidth;
            o.FontSize = FontSize;
            o.Text = Text;
            o.FontName = FontName;
            o.Response = Response.Clone() as CResponse;
            return o;
        }
    }
}
