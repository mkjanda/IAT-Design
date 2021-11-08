using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Security.Cryptography;
using System.Xml;
using System.Linq;

namespace IATClient.IATConfig
{
    public class IATImage
    {
        public List<Uri> SourceUris { get; private set; } = new List<Uri>();
        private String sha;
        public int Id { get; set; }
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
            xWriter.WriteElementString("X", di.AbsoluteBounds.X.ToString());
            xWriter.WriteElementString("Y", di.AbsoluteBounds.Y.ToString());
            xWriter.WriteElementString("Width", di.AbsoluteBounds.Width.ToString());
            xWriter.WriteElementString("Height", di.AbsoluteBounds.Height.ToString());
            xWriter.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            var di = CIAT.SaveFile.GetDI(SourceUris.First());
            reader.ReadStartElement("IATDisplayItem");
            Id = Convert.ToInt32(reader.ReadElementString("ID"));
            reader.ReadElementString("Filename");
            reader.ReadElementString("X");
            reader.ReadElementString("Y");
            reader.ReadElementString("Width");
            reader.ReadElementString("Height");
            reader.ReadEndElement();
        }


        static private SHA512Managed sha512 = new SHA512Managed();
    }
}
