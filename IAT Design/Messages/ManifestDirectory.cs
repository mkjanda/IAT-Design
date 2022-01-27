using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IATClient.Messages
{
    public class ManifestDirectory : FileEntity
    {
        private List<FileEntity> Contents;

        public void AddFile(ManifestFile f)
        {
            f.Path = Path + "\\" + f.Name;
            Contents.Add(f);
        }

        public void AddFiles(ManifestFile[] files)
        {
            for (int ctr = 0; ctr < files.Length; ctr++)
            {
                files[ctr].Path = Path + "\\" + files[ctr].Name;
                Contents.Add(files[ctr]);
            }
        }

        public FileEntity this[int ctr]
        {
            get
            {
                return Contents[ctr];
            }
        }


        public void AddSubdirectory(ManifestDirectory d)
        {
            d.Path = this.Path + "\\" + d.Name;
            Contents.Add(d);
        }

        public override String Path
        {
            get
            {
                return _Path;
            }
            set
            {
                foreach (FileEntity f in Contents)
                    if (f.FileEntityType == EFileEntityType.File)
                        f.Path = value + "\\" + f.Name;
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
                if (Path.EndsWith(Name))
                {
                    Path = Path.Substring(0, Path.LastIndexOf('\\'));
                    Path += "\\" + value;
                }
                _Name = value;
            }
        }

        public bool RemoveFile(String filename)
        {
            for (int ctr = 0; ctr < Contents.Count; ctr++)
            {
                if (Contents[ctr].FileEntityType == EFileEntityType.File)
                {
                    if (Contents[ctr].Path == filename)
                    {
                        Contents.RemoveAt(ctr);
                        return true;
                    }
                }
                else
                {
                    if (Contents[ctr].Path == filename)
                    {
                        Contents.RemoveAt(ctr);
                        return true;
                    }
                    return ((ManifestDirectory)Contents[ctr]).RemoveFile(filename);
                }
            }
            return false;
        }

        public override long TotalSize
        {
            get
            {
                long retVal = 0;
                foreach (FileEntity e in Contents)
                    retVal += e.TotalSize;
                return retVal;
            }
        }

        public override int NumEntities
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

        public int CountFiles()
        {
            int nFiles = 0;
            foreach (FileEntity fe in Contents)
            {
                if (fe.FileEntityType == EFileEntityType.Directory)
                    nFiles += ((ManifestDirectory)fe).CountFiles();
                else
                    nFiles++;
            }
            return nFiles;
        }

        public override long Size
        {
            get
            {
                return TotalSize;
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public int NumChildren
        {
            get
            {
                return Contents.Count;
            }
        }

        public ManifestDirectory()
        {
            Contents = new List<FileEntity>();
            _FileEntityType = EFileEntityType.Directory;
            Name = String.Empty;
        }

        public ManifestDirectory(String name)
        {
            Name = name;
            Contents = new List<FileEntity>();
            _FileEntityType = EFileEntityType.Directory;
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Directory");
            writer.WriteElementString("Path", Path.Replace("\\", "/"));
            writer.WriteElementString("Name", Name);
            writer.WriteElementString("Size", Size.ToString());
            foreach (FileEntity fe in Contents)
                if (fe.FileEntityType == EFileEntityType.Directory)
                    fe.WriteXml(writer);
            foreach (FileEntity fe in Contents)
                if (fe.FileEntityType == EFileEntityType.File)
                    fe.WriteXml(writer);
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            int nItems = Convert.ToInt32(reader["NumItems"]);
            reader.ReadStartElement();
            Path = reader.ReadElementString("Path").Replace("/", "\\");
            Name = reader.ReadElementString("Name");
            _Size = Convert.ToInt64(reader.ReadElementString("Size"));
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
        }
    }
}
