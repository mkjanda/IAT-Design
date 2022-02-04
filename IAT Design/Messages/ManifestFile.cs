﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IATClient.Messages
{
    public class ManifestFile : FileEntity
    {
        public enum EResourceType { ItemSlide, DeploymentFile, UpdateFile };

        public EResourceType ResourceType { get; private set; }

        public int ResourceId { get; set; } = -1;
        public List<int> ReferenceIds { get; private set; } = new List<int>();

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
            FileEntityType = EFileEntityType.File;
            _Size = 0;
        }

        public ManifestFile(String fName, long size)
        {
            FileEntityType = EFileEntityType.File;
            Name = fName;
            _Size = size;
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("File");
            writer.WriteElementString("Path", Path.Replace("\\", "/"));
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("EntityType", FileEntityType.ToString());
            writer.WriteElementString("MimeType", MimeType);
            writer.WriteElementString("Size", Size.ToString());
            writer.WriteElementString("ResourceType", ResourceType.ToString());
            if ((ResourceId != -1) && (ReferenceIds.Count > 0))
            {
                writer.WriteStartElement("ResourceReference");
                writer.WriteElementString("ResourceId", ResourceId.ToString());
                foreach (var i in ReferenceIds)
                    writer.WriteElementString("ReferenceId", i.ToString());
                writer.WriteEndElement();
            }
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
            String fe = reader.ReadElementString("EntityType");
            Enum.TryParse<EFileEntityType>(fe, out EFileEntityType type);
            FileEntityType = type;
            MimeType = reader.ReadElementString("MimeType");
            Size = Convert.ToInt32(reader.ReadElementString("Size"));
            String rt = reader.ReadElementString("ResourceType");
            Enum.TryParse<EResourceType>(rt, out EResourceType rType);
            ResourceType = rType;
            if (reader.Name == "ResourceReference")
            {
                ReferenceIds.Clear();
                reader.ReadStartElement("ResourceReference");
                ResourceId = Convert.ToInt32(reader.ReadElementString("ResourceId"));
                while (reader.Name == "ReferenceId")
                    ReferenceIds.Add(Convert.ToInt32(reader.ReadElementString("ReferenceId")));
                reader.ReadEndElement();
            }
            reader.ReadEndElement();
        }
    }
}
