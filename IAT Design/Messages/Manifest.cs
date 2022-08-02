using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;

namespace IATClient.Messages
{
    public class Manifest : INamedXmlSerializable
    {
        private String _IATName = String.Empty;
        public long ClientId { get; set; } = 0;
        
        public List<FileEntity> Contents { get; private set; } = new List<FileEntity>();

        public void AddFile(ManifestFile f)
        {
            f.Path = "\\" + f.Name;
            Contents.Add(f);
        }

        public FileEntity this[int ndx]
        {
            get
            {
                return Contents[ndx];
            }
        }

        public void AddFiles(ManifestFile[] files)
        {
            for (int ctr = 0; ctr < files.Length; ctr++)
            {
                files[ctr].Path = "\\" + files[ctr].Name;
                Contents.Add(files[ctr]);
            }
        }

        public void AddSubdirectory(ManifestDirectory d)
        {
            d.Path = "\\" + d.Name;
            Contents.Add(d);
        }

        public long TotalSize
        {
            get
            {
                long retVal = 0;
                foreach (FileEntity e in Contents)
                    retVal += e.TotalSize;
                return retVal;
            }
        }

        public String IATName
        {
            get
            {
                return _IATName;
            }
            set
            {
                _IATName = value;
            }
        }

        public int NumChildren
        {
            get
            {
                return Contents.Count;
            }
        }

        public int NumEntities
        {
            get
            {
                int nEntities = 0;
                foreach (FileEntity fe in Contents)
                {
                    nEntities += fe.NumEntities;
                }
                return nEntities;
            }
        }

        private int CountFiles(ManifestDirectory d)
        {
            return d.CountFiles();
        }

        public Manifest()
        {
        }

        public void Remove(String path)
        {
            for (int ctr = 0; ctr < Contents.Count; ctr++)
            {
                if (Contents[ctr].FileEntityType == FileEntity.EFileEntityType.Directory)
                {
                    if (Contents[ctr].Path == path)
                    {
                        Contents.RemoveAt(ctr);
                        break;
                    }
                    if (((ManifestDirectory)Contents[ctr]).RemoveFile(path))
                        return;
                }
                else
                    if (Contents[ctr].Path == path)
                {
                    Contents.RemoveAt(ctr);
                    return;
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Manifest");
            writer.WriteElementString("IATName", IATName);
            foreach (FileEntity fe in Contents)
                if (fe.FileEntityType == FileEntity.EFileEntityType.Directory)
                    fe.WriteXml(writer);
            foreach (FileEntity fe in Contents)
                if (fe.FileEntityType == FileEntity.EFileEntityType.File)
                    fe.WriteXml(writer);
            writer.WriteElementString("ClientId", ClientId.ToString());
            writer.WriteElementString("ProductKey", LocalStorage.Activation[LocalStorage.Field.ProductKey]);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement("Manifest");
            IATName = reader.ReadElementString("IATName");
            while (reader.Name == "Directory")
            {
                ManifestDirectory md = new ManifestDirectory();
                md.ReadXml(reader);
                Contents.Add(md);
            }
            while (reader.Name == "File")
            {
                ManifestFile mf = new ManifestFile();
                mf.ReadXml(reader);
                Contents.Add(mf);
            }
            ClientId = Convert.ToInt64(reader.ReadElementString("ClientId"));
            reader.ReadElementString("ProductKey");
            reader.ReadEndElement();
        }

        public String GetName()
        {
            return "Manifest";
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
