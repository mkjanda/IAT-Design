using java.io;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Windows.Interop;
using System.Xml;
using System.Xml.Linq;

namespace IATClient.IATConfig
{
    public class IATImage
    {
        public List<Uri> SourceUris { get; private set; } = new List<Uri>();
        public List<int> Indexes { get; private set; } = new List<int>();
        private String sha = String.Empty;
        public int Id { get; set; }
        public Rectangle Bounds { get; set; } = Rectangle.Empty;
        public String SHA
        {
            get
            {
                if (sha != String.Empty)
                    return sha;
                if (ImageData == null)
                    return String.Empty;
                sha = Convert.ToBase64String(sha512.ComputeHash(ImageData));
                return sha;
            }
        }
        public IATImage(int id = -1)
        {
            Id = id;
        }
        public byte[] ImageData { get; set; } = null;
        public ImageFormat Format { get; set; } = null;
        public String FileName
        {
            get
            {
                return String.Format("image{0}.{1}", Id.ToString(), Format.ToString().ToLower());
            }
        }

        public void WriteXml(XmlWriter xWriter)
        {
            var di = CIAT.SaveFile.GetDI(SourceUris.First());
            xWriter.WriteStartElement("IATDisplayItem");
            xWriter.WriteElementString("ID", Id.ToString());
            xWriter.WriteElementString("Filename", FileName);
            xWriter.WriteElementString("Format", typeof(ImageFormat).GetProperties().Where(pInfo => pInfo.Name == Format.ToString()).Select(p => p.Name).First());
            try
            {
                xWriter.WriteElementString("X", Bounds.Left.ToString());
                xWriter.WriteElementString("Y", Bounds.Top.ToString());
            }
            catch (NotImplementedException)
            {
                xWriter.WriteElementString("X", di.AbsoluteBounds.X.ToString());
                xWriter.WriteElementString("Y", di.AbsoluteBounds.Y.ToString());
            }
            xWriter.WriteElementString("Width", Bounds.Width.ToString());
            xWriter.WriteElementString("Height", Bounds.Height.ToString());
            xWriter.WriteEndElement();
        }

        static public IATImage Create(XElement elem)
        {
            return new IATImage(Convert.ToInt32(elem.Element("ID").Value))
            {
                Format = (ImageFormat)typeof(ImageFormat).GetProperties().Where(g => g.Name == elem.Element("Format").Value).Select(g => g.GetValue(null)).First(),
                Bounds = new Rectangle(Convert.ToInt32(elem.Element("X").Value), Convert.ToInt32(elem.Element("Y").Value),
                    Convert.ToInt32(elem.Element("Width").Value), Convert.ToInt32(elem.Element("Height").Value))
            };
        }

        static private SHA512Managed sha512 = new SHA512Managed();
    }
}
