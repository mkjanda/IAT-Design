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
        public class EFont {
            int val;

            public static readonly EFont genericSansSerif = new EFont(1, "sans-serif", System.Drawing.FontFamily.GenericSansSerif, String.Empty, "sans-serif");
            public static readonly EFont genericSerif = new EFont(2, "serif", System.Drawing.FontFamily.GenericSerif, String.Empty, "serif");
            public static readonly List<EFont> customFonts = new List<EFont>();
            
            public static void AddCustomFont(String name, FontFamily family, String fontFileName, String faceName) {
                customFonts.Add(new EFont(3 + customFonts.Count, name, family, fontFileName, faceName));
            }

            public override string ToString()
            {
                return Name;
            }

            public String Name { get; private set; }
            public FontFamily Family { get; private set; }
            public String FaceName { get; private set; }
            public String FontFileName { get; private set; }

            public bool IsCustomFont() {
                return (this.val > 2);
            }

            public EFont(int val, String name, FontFamily family, String fontFileName, String fontFaceName) {
                this.val = val;
                Name = family.Name;
                Family = family;
                FaceName = fontFaceName;
                FontFileName = fontFileName;
            }

            public static EFont GetFontByName(String name)
            {
                if (name == "sans-serif")
                    return genericSansSerif;
                if (name == "serif")
                    return genericSerif;
                EFont ef = customFonts.Where(f => (f.Name == name) || (f.FaceName == name)).FirstOrDefault();
                return ef != null ? ef : EFont.genericSansSerif;
            }

            public static EFont FromIndex(int val)
            {
                if (val == 0)
                    return genericSansSerif;
                if (val == 1)
                    return genericSerif;
                return customFonts[val - 2];
            }
        }

        public EFont Font { get; set; }
        public String FontSize { get; set; }
        public Color Color { get; set; }
        public bool Bold { get; set; }
        public bool Italic { get; set; }
        public enum EFor { Item, Response };
        public EFor For { get; set;}

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
            Font = EFont.genericSansSerif;
            if (f == EFor.Response)
                FontSize = "14px";
            else
                FontSize = "16px";
            Color = Color.Black;
            For = f;
        }

        public SurveyItemFormat(EFor f, CResponse.EResponseType respType)
        {
            if ((f == EFor.Item) && (respType == CResponse.EResponseType.Instruction))
                Font = EFont.genericSerif;
            else
                Font = EFont.genericSansSerif;
            if (f == EFor.Response)
                FontSize = "14px";
            else
                FontSize = "16px";
            Color = Color.Black;
            For = f;
        }

        public SurveyItemFormat(SurveyItemFormat o)
        {
            Font = o.Font;
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
                return ((double)Font.Family.GetLineSpacing(FontStyle) / (double)Font.Family.GetEmHeight(FontStyle)) *
                    FontSizeAsPixels;
            }
        }

        public XElement AsXElement()
        {
            return new XElement("Format", new XElement("Font", Font.ToString()), new XElement("FontSize", FontSize), new XElement("R", Color.R.ToString()),
                new XElement("G", Color.G.ToString()), new XElement("B", Color.B.ToString()), new XElement("Bold", Bold.ToString()), new XElement("Italic", Italic.ToString()));
        }

        public void Load(XElement elem)
        {
            Font = EFont.GetFontByName(elem.Element("Font").Value);
            FontSize = elem.Element("FontSize").Value;
            Color = Color.FromArgb(Convert.ToInt32(elem.Element("R").Value), Convert.ToInt32(elem.Element("G").Value), Convert.ToInt32(elem.Element("B").Value));
            Bold = Convert.ToBoolean(elem.Element("Bold").Value);
            Italic = Convert.ToBoolean(elem.Element("Italic").Value);
        }

        public void WriteToXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement("Format");
            xWriter.WriteElementString("Font", Font.ToString());
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
            if (!Font.IsCustomFont())
            {
                xWriter.WriteElementString("Font", Font.Name);
            }
            else
            {
                xWriter.WriteStartElement("CustomFont");
                xWriter.WriteElementString("FontFileName", Font.FontFileName);
                xWriter.WriteElementString("FontFaceName", Font.FaceName);
                xWriter.WriteEndElement();
                xWriter.WriteElementString("Font", Font.FaceName);
            }
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
