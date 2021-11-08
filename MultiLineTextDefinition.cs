using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace IATClient.Text
{
    /*
    class MultiLineTextDefinition : TextDefinition
    {
        protected float lineSpacing = 1F;

        protected float LineSpacing { 
            get {
                return LineSpacing;
            }
            set
            {
                LineSpacing = value;
                Invalidate();
            }
        }

        public MultiLineTextDefinition(UsedAs usedAs)
            : base(usedAs)
        {
            LineSpacing = 1;
        }

        public MultiLineTextDefinition(Uri uri) : base(uri) { }

        public override void Generate(Action<Bitmap> update)
        {
            Size bSize = RetrieveBoundingSize();
            Font f;
            String str;
            if (!Monitor.TryEnter(lockObject))
                return;
                f = PhraseFont;
                str = Phrase;
                Monitor.Exit(lockObject);
            Bitmap bmp = new Bitmap(bSize.Width, bSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(Brushes.Transparent, new Rectangle(new Point(0, 0), bSize));
            SizeF szText = g.MeasureString(str, f, new SizeF(bSize.Width, bSize.Height));
            Brush br = new SolidBrush(PhraseFontColor);
            switch (Alignment)
            {
                case EAlignment.left:
                    g.DrawString(str, f, br, new PointF(0, 0));
                    break;

                case EAlignment.center:
                    g.DrawString(str, f, br, new PointF((bSize.Width - szText.Width) / 2, 0));
                    break;

                case EAlignment.right:
                    g.DrawString(str, f, br, new PointF(bSize.Width - szText.Width, 0));
                    break;
            }
            br.Dispose();
            g.Dispose();
            update(bmp);
        }

        public void Load()
        {
            Stream s = CIAT.SaveFile.GetPart(this).GetStream(FileMode.Open, FileAccess.Read);
            XDocument xDoc = XDocument.Load(s);
            s.Close();
            phrase = xDoc.Root.Element("Phrase").Value;
            phraseFontFamily = xDoc.Root.Element("PhraseFontFamily").Value;
            phraseFontSize = Convert.ToSingle(xDoc.Root.Element("PhraseFontSize").Value);
            phraseFontColor = Color.FromName(xDoc.Root.Element("PhraseFontColor").Value);
            alignment = (EAlignment)Enum.Parse(typeof(EAlignment), xDoc.Root.Element("Alignment").Value);
            phraseFont = new Font(phraseFontFamily, phraseFontSize);
            lineSpacing = Convert.ToSingle(xDoc.Root.Element("LineSpacing").Value);
        }

        public void Save()
        {
            XDocument xDoc = new XDocument()
            XmlWriter xWriter = xDoc.CreateWriter();
            xWriter.WriteStartElement(this.GetType().ToString());
            xWriter.WriteElementString("Phrase", phrase);
            xWriter.WriteElementString("PhraseFontFamily", phraseFontFamily);
            xWriter.WriteElementString("PhraseFontSize", phraseFontSize.ToString());
            xWriter.WriteElementString("PhraseFontColor", phraseFontColor.ToString());
            xWriter.WriteElementString("Alignment", alignment.ToString());
            xWriter.WriteElementString("LineSpacing", lineSpacing.ToString());
            xWriter.WriteEndElement();
            xWriter.WriteEndDocument();
            xWriter.Close();
            Stream s = CIAT.SaveFile.GetPart(this).GetStream(FileMode.Create, FileAccess.Write);
            xDoc.Save(s);
            s.Close();
        }
    }*/
}
