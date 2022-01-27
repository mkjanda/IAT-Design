using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Security.Cryptography;

namespace IATClient.Messages
{
    class Packet : INamedXmlSerializable
    {
        private String _StrData = null;
        protected byte[] _ByteData = null;
        protected int _Length;
        protected bool _IsNullPacket = false;
        protected bool _IsErrorPacket = false;
        protected bool _IsLastPacket = false;
        protected EType _PacketType;
        public enum EType { ItemSlide, DeploymentData, ResultSetDescriptor, ResultData };
        static private ToBase64Transform Transformer = new ToBase64Transform();
        static private FromBase64Transform TransformBack = new FromBase64Transform();
        public const int PacketLength = 65536;
        private int _PacketNum;

        public int PacketNum
        {
            get
            {
                return _PacketNum;
            }
            set
            {
                _PacketNum = value;
            }
        }

        public EType PacketType
        {
            get
            {
                return _PacketType;
            }
        }

        public bool IsLastPacket
        {
            get
            {
                return _IsLastPacket;
            }
            set
            {
                _IsLastPacket = value;
            }
        }

        public bool IsNullPacket
        {
            get
            {
                return _IsNullPacket;
            }
        }

        public bool IsErrorPacket
        {
            get
            {
                return _IsErrorPacket;
            }
        }

        public Packet() { }

        public Packet(byte[] byteData, EType packetType)
        {
            _ByteData = byteData;
            if (byteData == null)
                _IsNullPacket = true;
            _PacketType = packetType;
        }

        public String StringData
        {
            get
            {
                if (_StrData == null)
                {
                    _StrData = Convert.ToBase64String(_ByteData);
                    return _StrData;
                }
                return _StrData;
            }
        }

        public byte[] ByteData
        {
            get
            {
                if (_ByteData == null)
                    _ByteData = Convert.FromBase64String(_StrData);
                return _ByteData;
            }
        }

        public int Length
        {
            get
            {
                return _Length;
            }
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public virtual void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            _StrData = null;
            _ByteData = null;
            _IsNullPacket = Convert.ToBoolean(reader["IsNullPacket"]);
            _IsErrorPacket = Convert.ToBoolean(reader["IsErrorPacket"]);
            _IsLastPacket = Convert.ToBoolean(reader["IsLastPacket"]);
            _PacketType = (EType)Enum.Parse(typeof(EType), reader["Type"]);
            _Length = Convert.ToInt32(reader["Length"]);
            if (IsNullPacket)
            {
                reader.Read();
                return;
            }
            reader.ReadStartElement();
            _StrData = reader.ReadElementString();
            _ByteData = Convert.FromBase64String(_StrData);
            reader.ReadEndElement();
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Packet");
            writer.WriteAttributeString("IsNullPacket", IsNullPacket.ToString().ToLower());
            writer.WriteAttributeString("IsErrorPacket", IsErrorPacket.ToString().ToLower());
            writer.WriteAttributeString("IsLastPacket", IsLastPacket.ToString().ToLower());
            writer.WriteAttributeString("Length", StringData.Length.ToString());
            writer.WriteAttributeString("Type", PacketType.ToString());
            writer.WriteElementString("Data", StringData);
            writer.WriteEndElement();
        }

        public String GetName()
        {
            return "Packet";
        }
    }

    class CPacketReceiver
    {
        private List<Packet> PacketQueue = new List<Packet>();
        private Manifest FileManifest;
        private long BytesReceived = 0;
        private object lockObject = new object();
        private Func<String, byte[], bool> OnFileReceived = null;
        private byte[] AvailableBytes = null;
        private bool HaltFlag = false;

        public CPacketReceiver(Manifest fileManifest, Func<String, byte[], bool> h)
        {
            FileManifest = fileManifest;
            OnFileReceived = h;
        }

        public void QueuePacket(Packet p)
        {
            lock (lockObject)
            {
                PacketQueue.Add(p);
            }
        }

        public void Halt()
        {
            lock (lockObject)
            {
                HaltFlag = true;
            }
        }

        public bool Halted
        {
            get
            {
                lock (lockObject)
                {
                    return HaltFlag;
                }
            }
            set
            {
                lock (lockObject)
                {
                    HaltFlag = true;
                }
            }
        }

        public Packet GetPacket()
        {
            Packet p = null;
            while (p == null)
            {
                lock (lockObject)
                {
                    if (PacketQueue.Count > 0)
                    {
                        p = PacketQueue[0];
                        PacketQueue.RemoveAt(0);
                    }
                }
                if (Halted)
                    return null;
                if (p == null)
                    Task.Run(() => Task.Delay(100));
            }
            return p;
        }

        private void ReceiveFile(String name, int fileSize)
        {
            int bytesRead = 0;
            byte[] fileData = new byte[fileSize];
            if (AvailableBytes != null)
            {
                if (AvailableBytes.Length <= fileSize)
                {
                    Array.Copy(AvailableBytes, fileData, AvailableBytes.Length);
                    bytesRead = AvailableBytes.Length;
                    AvailableBytes = null;
                }
                else
                {
                    Array.Copy(AvailableBytes, fileData, fileSize);
                    bytesRead = fileSize;
                    byte[] bTemp = new byte[AvailableBytes.Length - fileSize];
                    Array.Copy(AvailableBytes, fileSize, bTemp, 0, AvailableBytes.Length - fileSize);
                    AvailableBytes = bTemp;
                }
            }
            while (bytesRead < fileSize)
            {
                Packet p = GetPacket();
                if (p == null)
                    return;
                byte[] packetBytes = p.ByteData;
                if (bytesRead + packetBytes.Length <= fileSize)
                {
                    Array.Copy(packetBytes, 0, fileData, bytesRead, packetBytes.Length);
                    bytesRead += packetBytes.Length;
                }
                else
                {
                    Array.Copy(packetBytes, 0, fileData, bytesRead, fileSize - bytesRead);
                    AvailableBytes = new byte[packetBytes.Length - (fileSize - bytesRead)];
                    Array.Copy(packetBytes, fileSize - bytesRead, AvailableBytes, 0, packetBytes.Length - (fileSize - bytesRead));
                    bytesRead = fileSize;
                }
            }
            OnFileReceived(name, fileData);
        }


        private void Processor(Manifest fileManifest)
        {
            for (int ctr = 0; ctr < fileManifest.NumChildren; ctr++)
            {
                if (Halted)
                    return;
                if (fileManifest[ctr].FileEntityType == FileEntity.EFileEntityType.Directory)
                {
                    Processor((ManifestDirectory)fileManifest[ctr], fileManifest[ctr].Name);
                }
                else
                {
                    ReceiveFile(fileManifest[ctr].Name, (int)FileManifest[ctr].Size);
                }
            }
        }

        private void Processor(ManifestDirectory dir, String path)
        {
            for (int ctr = 0; ctr < dir.NumChildren; ctr++)
            {
                if (Halted)
                    return;
                if (dir[ctr].FileEntityType == FileEntity.EFileEntityType.Directory)
                    Processor(((ManifestDirectory)dir[ctr]), path + Path.DirectorySeparatorChar + dir[ctr].Name);
                else
                    ReceiveFile(dir[ctr].Name, (int)dir[ctr].Size);
            }
        }
    }
}

