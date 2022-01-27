using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IATClient.Messages
{
    public abstract class FileEntity : INamedXmlSerializable
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
}
