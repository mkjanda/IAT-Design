using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Xml;
using System.IO;
using System.Threading;
using System.Net.WebSockets;

namespace IATClient
{
    class CPacket : INamedXmlSerializable
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

        public CPacket() { }

        public CPacket(byte[] byteData, EType packetType)
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
        private List<CPacket> PacketQueue = new List<CPacket>();
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

        public void QueuePacket(CPacket p)
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

        public CPacket GetPacket()
        {
            CPacket p = null;
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
                    Thread.Sleep(100);
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
                CPacket p = GetPacket();
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
                if (fileManifest[ctr].FileEntityType == FileEntity.EFileEntityType.Directory) {
                    Processor((ManifestDirectory)fileManifest[ctr], fileManifest[ctr].Name);
                }
                else {
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

        private void run()
        {
            Processor(FileManifest);
        }

        public void Start()
        {
            ThreadStart proc = new ThreadStart(run);
            Thread th = new Thread(proc);
            th.Start();
        }
    }


    interface ITransmissionOwner
    {
        bool TryLock(int millis);
        bool Aborted { get; }
        void Unlock();
    }
/*
    class CPacketTransmission
    {
        public enum ETransmissionResult { Success, Fail, Cancel };
        private WebSocket webSocket;
        private List<MemoryStream> FileList = new List<MemoryStream>();
        private List<CPacket> PacketList = new List<CPacket>();
        private int currFilePos, currFileLength, byteCount;
        private int _QueueLength = 0;
        private object lockObject = new object();
        private CPacket.EType PacketType;
        private bool _Halted;

        bool HasNextPacket
        {
            get
            {
                if (FileList.Count == 0)
                    return false;
                return true;
            }
        }

        public CPacketTransmission(WebSocket sock, CPacket.EType packetType)
        {
            currFilePos = 0;
            byteCount = 0;
            webSocket = sock;
            PacketType = packetType;
        }

        public void QueueFile(MemoryStream fileName)
        {
            FileList.Add(fileName);
            _QueueLength += (int)fileName.Length;
        }

        public void QueueFile(byte[] data)
        {
            FileList.Add(new MemoryStream(data));
            _QueueLength += data.Length;
        }

        public long QueueLength
        {
            get
            {
                return _QueueLength;
            }
        }

        private CPacket GetPacket()
        {
            if (FileList.Count == 0)
                return null;
            int nBytesRead = 0;
            MemoryStream fStream = FileList[0];
            fStream.Seek(currFilePos, SeekOrigin.Begin);
            byte[] finalBAry = new byte[CPacket.PacketLength];
            currFileLength = (int)fStream.Length;
            while (nBytesRead < CPacket.PacketLength)
            {
                if (currFileLength - currFilePos >= CPacket.PacketLength - nBytesRead)
                {
                    fStream.Read(finalBAry, nBytesRead, CPacket.PacketLength - nBytesRead);
                    currFilePos += CPacket.PacketLength - nBytesRead;
                    if (currFilePos == currFileLength)
                    {
                        FileList.RemoveAt(0);
                        currFilePos = 0;
                    }
                    nBytesRead += CPacket.PacketLength - nBytesRead;
                    byteCount += CPacket.PacketLength;
                    return new CPacket(finalBAry, PacketType);
                }
                else
                {
                    fStream.Read(finalBAry, nBytesRead, currFileLength - currFilePos);
                    nBytesRead += currFileLength - currFilePos;
                    currFilePos = 0;
                    FileList.RemoveAt(0);
                    if (FileList.Count == 0)
                    {
                        byteCount += nBytesRead;
                        byte[] bAry = new byte[nBytesRead];
                        for (int ctr = 0; ctr < nBytesRead; ctr++)
                            bAry[ctr] = finalBAry[ctr];
                        return new CPacket(bAry, PacketType);
                    }
                    fStream = FileList[0];
                    fStream.Seek(0, SeekOrigin.Begin);
                    currFileLength = (int)fStream.Length;
                    currFilePos = 0;
                }
            }
            return null;
        }

        public void BuildPacketList()
        {
            PacketList.Clear();
            foreach (MemoryStream memStream in FileList)
                memStream.Seek(0, SeekOrigin.Begin);
            CPacket p;
            while ((p = GetPacket()) != null)
                PacketList.Add(p);
        }

        public int NumPackets
        {
            get
            {
                return PacketList.Count;
            }
        }

        public ETransmissionResult Transmit(Form win, Delegate ProgressIncrement, String serverURL, ClientWebSocket webSocket, CancellationToken AbortToken)
        {
            try
            {
                _Halted = false;
                ManualResetEvent timeoutEvent = new ManualResetEvent(false);
                if (PacketList.Count == 0)
                    BuildPacketList();
                CDeploymentProgressUpdate dpu = new CDeploymentProgressUpdate();
                PacketList[PacketList.Count - 1].IsLastPacket = true;
                for (int ctr = 0; ctr < PacketList.Count; ctr++)
                {
                    if (_Halted)
                        return ETransmissionResult.Cancel;
                    CEnvelope env = new CEnvelope(PacketList[ctr]);
                    env.SendMessage(webSocket, AbortToken);
                    win.BeginInvoke(ProgressIncrement, 1);
                }
                return ETransmissionResult.Success;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public void Halt()
        {
            _Halted = true;
        }

        private void InvokeTimeout(Object timeoutEvent, bool timeout)
        {
            if (timeout)
                ((ManualResetEvent)timeoutEvent).Set();
        }
    }
 */
}

