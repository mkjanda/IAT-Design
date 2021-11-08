using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Xml;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace IATClient
{
    public class SurveyItemFormat : ICloneable
    {

        public FontFamily FontFamily { get; set; }
        private String _FontSize { get; set; }
        public String FontSize
        {
            get
            {
                return _FontSize;
            }
            set
            {
                _FontSize = value;
                _FontSizeAsPixels = Convert.ToInt32(new Regex("([0-9]+).*").Match(_FontSize).Groups[1].Value);
            }
        }
        public Color Color { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public enum EFor { Item, Response };
        public EFor For { get; set;}

        private int _FontSizeAsPixels { get; set; }
        public int FontSizeAsPixels {
            get
            {
                Regex regex = new Regex("([0-9]+).*");
                return Convert.ToInt32(regex.Match(FontSize).Groups[1].Value);
            }
        }

        public FontStyle FontStyle
        {
            get
            {
                FontStyle fs;
                if (Bold && Italic)
                    fs = FontStyle.Bold | FontStyle.Italic;
                else if (Bold)
                    fs = FontStyle.Bold;
                else if (Italic)
                    fs = FontStyle.Italic;
                else
                    fs = FontStyle.Regular;
                return fs;
            }
        }

        public SurveyItemFormat(EFor f)
        {

            if (f == EFor.Response)
            {
                FontFamily = PrivateFont.Lora.FontFamily;
                FontSize = "14px";
            }
            else
            {
                FontSize = "16px";
                FontFamily = PrivateFont.JosefinSans.FontFamily;
            }
            Color = Color.Black;
            For = f;
            Italic = false;
            Bold = false;
        }

        public SurveyItemFormat(EFor f, CResponse.EResponseType respType)
        {
            if ((f == EFor.Item) && (respType == CResponse.EResponseType.Instruction))
                FontFamily = PrivateFont.Lora.FontFamily;
            else
                FontFamily = PrivateFont.JosefinSans.FontFamily;
            if (f == EFor.Response)
            {
                FontFamily = PrivateFont.Lora.FontFamily;
                FontSize = "14px";
            }
            else
            {
                FontSize = "16px";
                FontFamily = PrivateFont.JosefinSans.FontFamily;
            }
            Color = Color.Black;
            For = f;
        }

        public SurveyItemFormat(SurveyItemFormat o)
        {
            FontFamily = o.FontFamily;
            FontSize = o.FontSize;
            Color = o.Color;
            Bold = o.Bold;
            Italic = o.Italic;
            For = o.For;
        }

        public object Clone()
        {
            return new SurveyItemFormat(this);
        }

        public double LineHeight
        {
            get
            {
                return ((double)FontFamily.GetLineSpacing(FontStyle) / (double)FontFamily.GetEmHeight(FontStyle)) *
                    FontSizeAsPixels;
            }
        }

        public XElement AsXElement()
        {
            return new XElement("Format", new XElement("FontFamily", FontFamily.Name.ToString()), new XElement("FontSize", FontSize), new XElement("R", Color.R.ToString()),
                new XElement("G", Color.G.ToString()), new XElement("B", Color.B.ToString()), new XElement("Bold", Bold.ToString()), new XElement("Italic", Italic.ToString()));
        }

        public void Load(XElement elem)
        {
            FontFamily = PrivateFont.Fonts.Where(fam => fam.FamilyName == elem.Element("FontFamily").Value).Select(f => f.FontFamily).FirstOrDefault();
            FontSize = elem.Element("FontSize").Value;
            Color = Color.FromArgb(Convert.ToInt32(elem.Element("R").Value), Convert.ToInt32(elem.Element("G").Value), Convert.ToInt32(elem.Element("B").Value));
            Bold = Convert.ToBoolean(elem.Element("Bold").Value);
            Italic = Convert.ToBoolean(elem.Element("Italic").Value);
        }

        public void WriteToXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement("Format");
            xWriter.WriteElementString("Font", PrivateFont.Fonts.Where(f => f.FontFamily.Name == FontFamily.Name).Select(f => f.DisplayName).FirstOrDefault());
            xWriter.WriteElementString("FontSize", FontSize);
            xWriter.WriteElementString("ColorR", Color.R.ToString("x2"));
            xWriter.WriteElementString("ColorG", Color.G.ToString("x2"));
            xWriter.WriteElementString("ColorB", Color.B.ToString("x2"));
            xWriter.WriteElementString("Bold", Bold.ToString());
            xWriter.WriteElementString("Italic", Italic.ToString());
            xWriter.WriteEndElement();
        }

        public void ReadXml(XmlReader xReader)
        {
            throw new NotImplementedException();
        }

        public void WriteXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement("SurveyItemFormat");
            var f = PrivateFont.Fonts.Where(f => f.FontFamily.Name == FontFamily.Name).Select(f => f.DisplayName).FirstOrDefault();
            if (f != null)
                xWriter.WriteElementString("Font", f);
            else
                xWriter.WriteElementString("Font", PrivateFont.Fonts[1].DisplayName);
            xWriter.WriteElementString("FontSize", FontSize);
            xWriter.WriteElementString("ColorR", Color.R.ToString("x2"));
            xWriter.WriteElementString("ColorG", Color.G.ToString("x2"));
            xWriter.WriteElementString("ColorB", Color.B.ToString("x2"));
            xWriter.WriteElementString("Bold", Bold.ToString());
            xWriter.WriteElementString("Italic", Italic.ToString());
            xWriter.WriteEndElement();
        }
    }
}
