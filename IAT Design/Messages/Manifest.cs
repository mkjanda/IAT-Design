using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace IATClient.Messages
{
    public class Manifest : INamedXmlSerializable
    {
        private String _IATName = String.Empty;
        private List<FileEntity> Contents = new List<FileEntity>();
        public enum EType { ItemSlides, DeploymentFiles };
        private EType _Type = EType.DeploymentFiles;

        public EType Type
        {
            get
            {
                return _Type;
            }
            set
            {
                _Type = value;
            }
        }

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

        public int FileCount
        {
            get
            {
                int nFiles = 0;
                for (int ctr = 0; ctr < Contents.Count; ctr++)
                {
                    if (Contents[ctr].FileEntityType == FileEntity.EFileEntityType.File)
                        nFiles++;
                    else
                        nFiles += CountFiles((ManifestDirectory)Contents[ctr]);
                }
                return nFiles;
            }
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
            writer.WriteAttributeString("Size", TotalSize.ToString());
            writer.WriteAttributeString("Type", Type.ToString());
            writer.WriteElementString("IATName", IATName);
            writer.WriteStartElement("ManifestItems");
            foreach (FileEntity fe in Contents)
                if (fe.FileEntityType == FileEntity.EFileEntityType.Directory)
                    fe.WriteXml(writer);
            foreach (FileEntity fe in Contents)
                if (fe.FileEntityType == FileEntity.EFileEntityType.File)
                    fe.WriteXml(writer);
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            _Type = (EType)Enum.Parse(typeof(EType), reader["Type"]);
            reader.ReadStartElement("Manifest");
            IATName = reader.ReadElementString("IATName");
            reader.ReadStartElement("ManifestItems");
            while (reader.IsStartElement())
            {
                if (reader.Name == "Directory")
                {
                    ManifestDirectory dir = new ManifestDirectory();
                    dir.ReadXml(reader);
                    Contents.Add(dir);
                }
                else if (reader.Name == "File")
                {
                    ManifestFile f = new ManifestFile();
                    f.ReadXml(reader);
                    Contents.Add(f);
                }
            }
            reader.ReadEndElement();
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
