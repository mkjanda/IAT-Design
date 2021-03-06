using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.IO;

namespace Launcher
{
    public abstract class FileEntity 
    {
        public enum EFileEntityType { File, Directory };
        protected long _Size;
        public abstract long Size { get; set; }
        public virtual String Path
        {
            get
            {
                return _Path;
            }
            set
            {
                _Path = value;
            }
        }
        protected EFileEntityType _FileEntityType;
        protected String _Name = String.Empty;
        protected String _Path = String.Empty;

        public abstract long TotalSize { get; }

        public abstract String Name { get; set; }

        public EFileEntityType FileEntityType
        {
            get
            {
                return _FileEntityType;
            }
        }

        public abstract int NumEntities { get; }

        public FileEntity()
        {
            Name = String.Empty;
        }

        public String GetName()
        {
            return "FileEntity";
        }

        public abstract void WriteXml(XmlWriter writer);
        public abstract void ReadXml(XmlReader reader);

        public static FileEntity CreateFromXml(XmlReader reader)
        {
            FileEntity e = null;
            EFileEntityType eType = (EFileEntityType)Enum.Parse(typeof(EFileEntityType), reader["FileEntityType"]);
            switch (eType)
            {
                case EFileEntityType.Directory:
                    e = new ManifestDirectory();
                    e.ReadXml(reader);
                    break;

                case EFileEntityType.File:
                    e = new ManifestFile();
                    e.ReadXml(reader);
                    break;
            }
            return e;
        }
    }

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
                _Name = value;
            }
        }


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
            writer.WriteEndElement();
        }

        public override void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            Path = reader.ReadElementString("Path").Replace("/", "\\");
            _Name = reader.ReadElementString("Name");
            _Size = Convert.ToInt64(reader.ReadElementString());
            reader.ReadEndElement();
        }
    }

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

    public class Manifest 
    {
        private List<FileEntity> Contents = new List<FileEntity>();


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
            reader.ReadStartElement("Manifest");
            reader.ReadElementString("IATName");
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