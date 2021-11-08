using System;
using System.Xml;
using System.Xml.Schema;

namespace IATClient.IATConfig
{
    public class IATLayout
    {
        private int _InteriorWidth, _InteriorHeight, _BorderWidth;
        private int _ResponseWidth, _ResponseHeight;
        private System.Drawing.Color _BorderColor, _BackColor, _OutlineColor, _PageBackColor;

        public int ResponseWidth
        {
            get
            {
                return _ResponseWidth;
            }
            set
            {
                _ResponseWidth = value;
            }
        }

        public int ResponseHeight
        {
            get
            {
                return _ResponseHeight;
            }
            set
            {
                _ResponseHeight = value;
            }
        }

        public int InteriorWidth
        {
            get
            {
                return _InteriorWidth;
            }
            set
            {
                _InteriorWidth = value;
            }
        }

        public int InteriorHeight
        {
            get
            {
                return _InteriorHeight;
            }
            set
            {
                _InteriorHeight = value;
            }
        }

        public int BorderWidth
        {
            get
            {
                return _BorderWidth;
            }
            set
            {
                _BorderWidth = value;
            }
        }

        public System.Drawing.Color BorderColor
        {
            get
            {
                return _BorderColor;
            }
            set
            {
                _BorderColor = value;
            }
        }

        public System.Drawing.Color BackColor
        {
            get
            {
                return _BackColor;
            }
            set
            {
                _BackColor = value;
            }
        }

        public System.Drawing.Color PageBackColor
        {
            get
            {
                return _PageBackColor;
            }
            set
            {
                _PageBackColor = value;
            }
        }

        public System.Drawing.Color OutlineColor
        {
            get
            {
                return _OutlineColor;
            }
            set
            {
                _OutlineColor = value;
            }
        }


        public IATLayout()
        {
            BorderColor = CIAT.SaveFile.Layout.BorderColor;
            BackColor = CIAT.SaveFile.Layout.BackColor;
            OutlineColor = CIAT.SaveFile.Layout.OutlineColor;
            PageBackColor = CIAT.SaveFile.Layout.WebpageBackColor;
            InteriorWidth = (int)((double)CIAT.SaveFile.Layout.InteriorSize.Width);
            InteriorHeight = (int)((double)CIAT.SaveFile.Layout.InteriorSize.Height);
            BorderWidth = (int)((double)CIAT.SaveFile.Layout.BorderWidth);
            ResponseWidth = (int)((double)CIAT.SaveFile.Layout.KeyValueSize.Width);
            ResponseHeight = (int)((double)CIAT.SaveFile.Layout.KeyValueSize.Height);
        }

        public IATLayout(CIATLayout layout)
        {
            BorderColor = layout.BorderColor;
            BackColor = layout.BackColor;
            OutlineColor = layout.OutlineColor;
            PageBackColor = layout.WebpageBackColor;
            InteriorWidth = (int)((double)layout.InteriorSize.Width);
            InteriorHeight = (int)((double)layout.InteriorSize.Height);
            BorderWidth = (int)((double)layout.BorderWidth);
            ResponseWidth = (int)((double)layout.KeyValueSize.Width);
            ResponseHeight = (int)((double)layout.KeyValueSize.Height);
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Layout");
            writer.WriteElementString("InteriorWidth", InteriorWidth.ToString());
            writer.WriteElementString("InteriorHeight", InteriorHeight.ToString());
            writer.WriteElementString("BorderWidth", BorderWidth.ToString());
            writer.WriteElementString("ResponseWidth", ResponseWidth.ToString());
            writer.WriteElementString("ResponseHeight", ResponseHeight.ToString());
            writer.WriteElementString("BorderColorR", String.Format("{0:X2}", BorderColor.R));
            writer.WriteElementString("BorderColorG", String.Format("{0:X2}", BorderColor.G));
            writer.WriteElementString("BorderColorB", String.Format("{0:X2}", BorderColor.B));
            writer.WriteElementString("BackColorR", String.Format("{0:X2}", BackColor.R));
            writer.WriteElementString("BackColorG", String.Format("{0:X2}", BackColor.G));
            writer.WriteElementString("BackColorB", String.Format("{0:X2}", BackColor.B));
            writer.WriteElementString("OutlineColorR", String.Format("{0:X2}", OutlineColor.R));
            writer.WriteElementString("OutlineColorG", String.Format("{0:X2}", OutlineColor.G));
            writer.WriteElementString("OutlineColorB", String.Format("{0:X2}", OutlineColor.B));
            writer.WriteElementString("PageBackColorR", String.Format("{0:X2}", PageBackColor.R));
            writer.WriteElementString("PageBackColorG", String.Format("{0:X2}", PageBackColor.G));
            writer.WriteElementString("PageBackColorB", String.Format("{0:X2}", PageBackColor.B));
            writer.WriteEndElement();
        }

        public String GetName()
        {
            return "Layout";
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            InteriorWidth = Convert.ToInt32(reader.ReadElementString());
            InteriorHeight = Convert.ToInt32(reader.ReadElementString());
            BorderWidth = Convert.ToInt32(reader.ReadElementString());
            ResponseWidth = Convert.ToInt32(reader.ReadElementString());
            ResponseHeight = Convert.ToInt32(reader.ReadElementString());
            int r, g, b;
            r = Convert.ToInt32(reader.ReadElementString(), 16);
            g = Convert.ToInt32(reader.ReadElementString(), 16);
            b = Convert.ToInt32(reader.ReadElementString(), 16);
            BorderColor = System.Drawing.Color.FromArgb(r, g, b);
            r = Convert.ToInt32(reader.ReadElementString(), 16);
            g = Convert.ToInt32(reader.ReadElementString(), 16);
            b = Convert.ToInt32(reader.ReadElementString(), 16);
            BackColor = System.Drawing.Color.FromArgb(r, g, b);
            r = Convert.ToInt32(reader.ReadElementString(), 16);
            g = Convert.ToInt32(reader.ReadElementString(), 16);
            b = Convert.ToInt32(reader.ReadElementString(), 16);
            OutlineColor = System.Drawing.Color.FromArgb(r, g, b);
            r = Convert.ToInt32(reader.ReadElementString(), 16);
            g = Convert.ToInt32(reader.ReadElementString(), 16);
            b = Convert.ToInt32(reader.ReadElementString(), 16);
            PageBackColor = System.Drawing.Color.FromArgb(r, g, b);
            reader.ReadEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
