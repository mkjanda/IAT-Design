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

namespace IATClient
{
    class CPacket : INamedXmlSerializable
    {
        private String _StrData = null;
        private byte[] _ByteData = null;
        private int _Length;
        private bool _IsNullPacket = false;
        private bool _IsErrorPacket = false;
        static private ToBase64Transform Transformer = new ToBase64Transform();
        static private FromBase64Transform TransformBack = new FromBase64Transform();

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

        public CPacket(byte[] byteData)
        {
            _ByteData = byteData;
            if (byteData == null)
                _IsNullPacket = true;
        }

        public String StringData
        {
            get
            {
                if (_StrData == null)
                {
                    _StrData = Convert.ToBase64String(_ByteData, Base64FormattingOptions.InsertLineBreaks);
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

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            _StrData = null;
            _ByteData = null;
            _IsNullPacket = Convert.ToBoolean(reader["IsNullPacket"]);
            _IsErrorPacket = Convert.ToBoolean(reader["IsErrorPacket"]);
            _Length = Convert.ToInt32(reader[2]);
            reader.ReadStartElement();
            _StrData = reader.ReadElementString();
            _ByteData = Convert.FromBase64String(_StrData);
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Packet");
            writer.WriteAttributeString("IsNullPacket", IsNullPacket.ToString());
            writer.WriteAttributeString("IsErrorPacket", IsErrorPacket.ToString());
            writer.WriteAttributeString("Length", StringData.Length.ToString());
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
                Monitor.PulseAll(lockObject);
            }
        }

        public void Halt()
        {
                HaltFlag = true;
        }

        public bool Halted
        {
            get
            {
                return HaltFlag;
            }
            set
            {
                HaltFlag = true;
            }
        }

        public CPacket GetPacket()
        {
            CPacket p = null;
            while (p == null)
            {
                if (Monitor.TryEnter(lockObject)) {
                    if (PacketQueue.Count > 0)
                    {
                        p = PacketQueue[0];
                        PacketQueue.RemoveAt(0);
                    }
                    else
                        Monitor.Wait(lockObject);
                }
                Monitor.Exit(lockObject);
                if (Halted)
                    return null;
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


        private void Processor(List<FileEntity> entityList, String path)
        {
            for (int ctr = 0; ctr < entityList.Count; ctr++)
            {
                if (Halted)
                    return;
                if (entityList[ctr].FileEntityType == FileEntity.EFileEntityType.Directory)
                    Processor(((ManifestDirectory)entityList[ctr]).Contents, path + Path.DirectorySeparatorChar + entityList[ctr].Name);
                else
                    ReceiveFile(entityList[ctr].Name, (int)((ManifestFile)entityList[ctr]).Size);
            }
        }

        private void run()
        {
            Processor(FileManifest.Contents, String.Empty);
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

    class CPacketTransmission
    {
        public enum ETransmissionResult { Success, Fail, Cancel };
        private ITransmissionOwner Owner;
        private List<MemoryStream> FileList = new List<MemoryStream>();
        private List<CPacket> PacketList = new List<CPacket>();
        private int currFilePos, currFileLength, byteCount;
        private int _QueueLength = 0;
        private object lockObject = new object();

        bool HasNextPacket
        {
            get
            {
                if (FileList.Count == 0)
                    return false;
                return true;
            }
        }

        public CPacketTransmission(ITransmissionOwner owner)
        {
            currFilePos = 0;
            byteCount = 0;
            Owner = owner;
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
            byte[] finalBAry = new byte[65536];
            currFileLength = (int)fStream.Length;
            while (nBytesRead < 65536)
            {
                if (currFileLength - currFilePos >= 65536 - nBytesRead)
                {
                    fStream.Read(finalBAry, nBytesRead, 65536 - nBytesRead);
                    currFilePos += 65536 - nBytesRead;
                    if (currFilePos == currFileLength)
                    {
                        FileList.RemoveAt(0);
                        currFilePos = 0;
                    }
                    nBytesRead += 65536 - nBytesRead;
                    byteCount += 65536;
                    return new CPacket(finalBAry);
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
                        return new CPacket(bAry);
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

        public ETransmissionResult Transmit(Form win, Delegate ProgressIncrement, TransactionEvent tEvent, String serverURL)
        {
            bool hasLock = false;
            try
            {
                ManualResetEvent timeoutEvent = new ManualResetEvent(false);
                IAsyncResult aResult = null;
                if (PacketList.Count == 0)
                    BuildPacketList();
                CDeploymentProgressUpdate dpu = new CDeploymentProgressUpdate();
                for (int ctr = 0; ctr < PacketList.Count; ctr++)
                {
                    while (!Owner.TryLock(100)) ;
                    hasLock = true;
                    if (Owner.Aborted)
                        return ETransmissionResult.Cancel;
                    MySOAP.CallSOAP(serverURL, Convert.ToInt32(Properties.Resources.sDataProviderPort), MySOAP.ESoapAction.SendPacket, PacketList[ctr], dpu);
                    Owner.Unlock();
                    hasLock = false;
                    tEvent.ProgressValue = ctr + 1;
                    if (aResult != null)
                    {
                        if (!aResult.IsCompleted)
                        {
                            int nWaitBreaker = WaitHandle.WaitAny(new WaitHandle[] { timeoutEvent, aResult.AsyncWaitHandle });
                            if (nWaitBreaker != 1)
                                throw new TimeoutException("The program cannot continue executing without entering mutual deadlock");
                        }
                    }
                    aResult = win.BeginInvoke(ProgressIncrement, 1);
                    ThreadPool.RegisterWaitForSingleObject(aResult.AsyncWaitHandle, new WaitOrTimerCallback(InvokeTimeout), timeoutEvent, 1000, true);
                }
                return ETransmissionResult.Success;
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally
            {
                if (hasLock)
                    Owner.Unlock();
            }
        }

        private void InvokeTimeout(Object timeoutEvent, bool timeout)
        {
            if (timeout)
                ((ManualResetEvent)timeoutEvent).Set();
        }
    }
}

