using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IATClient.Messages
{
    public class ManifestFile : FileEntity
    {
        public override long Size
        {
            get
            {
                return _Size;
            }
            set
            {
                _Size = value;
            }
        }

        public override long TotalSize
        {
            get
            {
                return Size;
            }
        }

        public String Addendum { get; set; } = String.Empty;

        public override string Name
        {
            get
            {
                return _Name;
            }
            set
            {
                if ((Path.EndsWith(Name)) && (Path != String.Empty))
                {
                    Path = Path.Substring(0, Path.LastIndexOf('\\'));
                    Path += "\\" + value;
                }
                int ndx = value.LastIndexOf('.');
                if (ndx > 0)
                {
                    switch (value.Substring(ndx + 1))
                    {
                        case "jpg":
                            MimeType = "image/jpeg";
                            break;

                        case "jpeg":
                            MimeType = "image/jpeg";
                            break;

                        case "png":
                            MimeType = "image/png";
                            break;

                        case "xml":
                            MimeType = "text/xml";
                            break;

                        case "txt":
                            MimeType = "text/plain";
                            break;
                    }
                }
                else
                    MimeType = "text/xml";
                _Name = value;
            }
        }


        private String MimeType { get; set; } = "text/plain";
        public override int NumEntities
        {
            get
            {
                return 1;
            }
        }

        public ManifestFile()
        {
            _FileEntityType = EFileEntityType.File;
            _Size = 0;
        }

        public ManifestFile(String fName, long size)
        {
            _FileEntityType = EFileEntityType.File;
            Name = fName;
            _Size = size;
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("File");
            writer.WriteElementString("Path", Path.Replace("\\", "/"));
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Size", Size.ToString());
            writer.WriteElementString("MimeType", MimeType);
            if (Addendum != String.Empty)
                writer.WriteElementString("Addendum", Addendum);
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            Path = reader.ReadElementString("Path").Replace("/", "\\");
            _Name = reader.ReadElementString("Name");
            _Size = Convert.ToInt64(reader.ReadElementString());
            MimeType = reader.ReadElementString("MimeType");
            reader.ReadEndElement();
        }
    }
}
