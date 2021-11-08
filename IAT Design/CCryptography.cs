using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    public class CCryptography : IXmlSerializable 
    {
        private uint PageNumber1, PageNumber2;
        private uint Index1, Index2;
        private uint Prime1, Prime2;
        private UInt64 PrivateKeyExponent;
        private UInt64 PublicKeyExponent;
        private UInt64 Modulus;

        public CCryptography(String password)
        {
            byte[] data = Convert.FromBase64String(password);
            byte[] result = new byte[6];
            for (int ctr = 0; ctr < 6; ctr++)
                result[ctr] = 0x00;
            int nBytes = data.Length;
            for (int ctr1 = 0; ctr1 < data.Length / 6; ctr1++)
                for (int ctr2 = 0; ctr2 < 6; ctr2++)
                    data[ctr2] = (byte)((data[ctr2] | data[ctr1 * 6 + ctr2]) & ~(data[ctr2] & data[ctr1 * 6 + ctr2]));
            for (int ctr = data.Length - (data.Length % 6); ctr < data.Length; ctr++)
                data[-(ctr - data.Length)] = (byte)((data[ctr] | data[-(ctr - data.Length)]) & ~(data[ctr] & data[-(ctr - data.Length)]));
            result[0] &= 0x0F;
            MemoryStream memStream = new MemoryStream(data);
            BinaryReader bReader = new BinaryReader(memStream);
            UInt64 n = bReader.ReadUInt64();
            PageNumber1 = (uint)(0x3F & n);
            n >>= 6;
            Index1 = (uint)(0xFFFF & n);
            n >>= 16;
            PageNumber2 = (uint)(0x3F & n);
            n >>= 6;
            Index2 = (uint)(0xFFFF & n);
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            PageNumber1 = UInt32.Parse(reader.ReadElementString());
            PageNumber2 = UInt32.Parse(reader.ReadElementString());
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("PrimePages");
            writer.WriteElementString("PageNumber1", PageNumber1.ToString());
            writer.WriteElementString("PageNumber2", PageNumber2.ToString());
            writer.WriteEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public void RetrievePrimes()
        {
            try
            {
                TransactionRequest transRequest = new TransactionRequest();
                transRequest.Transaction = TransactionRequest.ETransaction.RequestTransmission;
                transRequest.ServerPassword = IATConfigMainForm.ServerPassword;
                String msg = MySOAP.CreateSOAPEnvelope(transRequest);
                HandShake hand = new HandShake();
                long sessID = MySOAP.CallSOAP(Properties.Resources.sActivationServer, "RequestSOAPExchange", msg, hand, "Handshake", 0);
                HandShake returnedHand = HandShake.CreateResponse(hand);
                msg = MySOAP.CreateSOAPEnvelope(returnedHand);
                MySOAP.CallSOAP(Properties.Resources.sActivationServer, "Handshake", msg, transRequest, "TransactionRequest", sessID);
                msg = MySOAP.CreateSOAPEnvelope(this);
                MySOAP.CallSOAP(Properties.Resources.sActivationServer, "PrimePages", msg, transRequest, "TransactionRequestion", sessID);
                if (transRequest.Transaction != TransactionRequest.ETransaction.TransactionSuccess)
                    throw new Exception("Could not retrieve prime pages");
                CPacket packet = new CPacket();
                List<CPacket> PacketQueue = new List<CPacket>();
                bool bDone = false;
                while (!bDone)
                {
                    transRequest = new TransactionRequest();
                    transRequest.Transaction = TransactionRequest.ETransaction.RequestPrimePagePacket;
                    transRequest.ServerPassword = IATConfigMainForm.ServerPassword;
                    msg = MySOAP.CreateSOAPEnvelope(transRequest);
                    MySOAP.CallSOAP(Properties.Resources.sActivationServer, "TransactionRequest", msg, packet, "Packet", sessID);
                    if (packet.Length == 0)
                        bDone = true;
                    else
                        PacketQueue.Add(packet);
                }
                MemoryStream memStream = new MemoryStream();
                for (int ctr = 0; ctr < PacketQueue.Count; ctr++)
                    memStream.Write(PacketQueue[ctr].ByteData, 0, PacketQueue[ctr].ByteData.Length);
                memStream.Seek(0, SeekOrigin.Begin);
                BinaryReader bReader = new BinaryReader(memStream);
                uint[] Page1, Page2;
                Page1 = new uint[memStream.Length / (sizeof(uint) * 2)];
                Page2 = new uint[memStream.Length / (sizeof(uint) * 2)];
                for (int ctr = 0; ctr < Page1.Length; ctr++)
                    Page1[ctr] = bReader.ReadUInt32();
                for (int ctr = 0; ctr < Page2.Length; ctr++)
                    Page2[ctr] = bReader.ReadUInt32();
                Prime1 = Page1[Index1];
                Prime2 = Page2[Index2];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error Initializing Encryption", MessageBoxButtons.OK);
            }

        }


        class S64Pair
        {
            public UInt64 X;
            public UInt64 Y;
            public S64Pair(UInt64 x, UInt64 y)
            {
                X = x;
                Y = y;
            }

        }

        private S64Pair ExtendedEuclid(UInt64 X, UInt64 Y)
        {
            UInt64 Q = X / Y;
            UInt64 R = X % Y;
            S64Pair ST = ExtendedEuclid(Y, R);
            return new S64Pair(ST.Y, ST.X - Q * ST.Y);
        }


        public void CalcKeys()
        {
            UInt64 N = Prime1 * Prime2;
            UInt64 TotientN = (Prime1 - 1) * (Prime2 - 1);
            UInt64 E = 65537;
            List<UInt64> Factors = new List<UInt64>();
            UInt64 x = TotientN;
            UInt64 f = 2;
            while (x != 1)
            {
                if (x % f == 0)
                {
                    Factors.Add(f);
                    while (x % f == 0)
                        x /= f;
                }
                f++;
            }
            bool bCoprime = false;
            while (!bCoprime)
            {
                bCoprime = true;
                for (int ctr = 0; ctr < Factors.Count; ctr++)
                {
                    if (E % Factors[ctr] == 0)
                    {
                        bCoprime = false;
                        break;
                    }
                }
                E++;
            }
            UInt64 D = ExtendedEuclid(E, TotientN).X;
            Modulus = N;
            PrivateKeyExponent = D;
            PublicKeyExponent = E;
        }

        public UInt64 Encrypt(UInt64 data)
        {
            UInt64 cipher = 1;
            for (UInt64 ctr = 0; ctr < PublicKeyExponent; ctr++)
            {
                cipher *= data % Modulus;
                cipher = cipher % Modulus;
            }
            return cipher;
        }

        public UInt64 Decrypt(UInt64 cipher)
        {
            UInt64 data = 1;
            for (UInt64 ctr = 0; ctr < PrivateKeyExponent; ctr++)
            {
                data *= cipher % Modulus;
                data = data % Modulus;
            }
            return data;
        }

        public byte[] EncryptBytes(byte[] data)
        {
            UInt64 word, cipher;
            byte[] result = new byte[data.Length];
            int ctr1 = 0;
            while (ctr1 < data.Length / 8)
            {
                word = 0;
                for (int ctr2 = ctr1 * 8; ctr2 < ctr1 * 8 + 8; ctr2++)
                    word |= (UInt64)data[ctr2] << (8 * (ctr1 * 8 + 7 - ctr2));
                cipher = Encrypt(word);
                for (int ctr2 = ctr1 * 8; ctr2 < ctr1 * 8 + 8; ctr2++)
                    result[ctr2] = (byte)((cipher & ((UInt64)0xFF << (ctr1 * 8 + 7 - ctr2))) >> (ctr1 * 8 + 7 - ctr2));
            }
            word = 0;
            for (int ctr2 = ctr1; ctr2 < data.Length; ctr2++)
                word |= (UInt64)data[ctr2] << (data.Length - ctr2);
            cipher = Encrypt(word);
            for (int ctr2 = ctr1; ctr2 < data.Length; ctr2++)
                result[ctr2] = (byte)((cipher & ((UInt64)0xFF << (data.Length - ctr2))) >> (data.Length - ctr2));
            return result;
        }

        public byte[] DecryptBytes(byte[] data)
        {
            UInt64 word, cipher;
            byte[] result = new byte[data.Length];
            int ctr1 = 0;
            while (ctr1 < data.Length / 8)
            {
                word = 0;
                for (int ctr2 = ctr1 * 8; ctr2 < ctr1 * 8 + 8; ctr2++)
                    word |= (UInt64)data[ctr2] << (8 * (ctr1 * 8 + 7 - ctr2));
                cipher = Decrypt(word);
                for (int ctr2 = ctr1 * 8; ctr2 < ctr1 * 8 + 8; ctr2++)
                    result[ctr2] = (byte)((cipher & ((UInt64)0xFF << (ctr1 * 8 + 7 - ctr2))) >> (ctr1 * 8 + 7 - ctr2));
            }
            word = 0;
            for (int ctr2 = ctr1; ctr2 < data.Length; ctr2++)
                word |= (UInt64)data[ctr2] << (data.Length - ctr2);
            cipher = Decrypt(word);
            for (int ctr2 = ctr1; ctr2 < data.Length; ctr2++)
                result[ctr2] = (byte)((cipher & ((UInt64)0xFF << (data.Length - ctr2))) >> (data.Length - ctr2));
            return result;
        }

    }
}
