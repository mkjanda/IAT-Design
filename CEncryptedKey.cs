using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
using System.Xml.Schema;
using System.Security.Cryptography;

namespace IATClient
{
    class CEncryptedKey : INamedXmlSerializable
    {
        private byte[] N, P, Q, D, E, DP, DQ, InverseQ;
        private RC2CryptoServiceProvider crypt = new RC2CryptoServiceProvider();
        public CEncryptedKey()
        {
            crypt.Mode = CipherMode.CBC;
            crypt.Padding = PaddingMode.PKCS7;
            N = P = Q = D = E = DP = DQ = InverseQ = null;
        }

        private byte[] GetKeyBytes(XmlReader reader)
        {
            MemoryStream memStream = new MemoryStream();
            CryptoStream b64Stream = new CryptoStream(memStream, new FromBase64Transform(), CryptoStreamMode.Write);
            StreamWriter sWriter = new StreamWriter(b64Stream);
            sWriter.Write(reader.ReadElementString());
            sWriter.Flush();
            b64Stream.FlushFinalBlock();
            return memStream.ToArray();
        }

        private byte[] DecryptBytes(byte[] data)
        {
            MemoryStream memStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(memStream, crypt.CreateDecryptor(Convert.FromBase64String(Properties.Resources.sRSADataCipher),
                Convert.FromBase64String(Properties.Resources.sRSADataIV)), CryptoStreamMode.Write);
            BinaryWriter writer = new BinaryWriter(cStream);
            writer.Write(data, 0, data.Length);
            writer.Flush();
            cStream.FlushFinalBlock();
            return memStream.ToArray();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            N = GetKeyBytes(reader);
            P = GetKeyBytes(reader);
            Q = GetKeyBytes(reader);
            D = GetKeyBytes(reader);
            E = GetKeyBytes(reader);
            DP = GetKeyBytes(reader);
            DQ = GetKeyBytes(reader);
            InverseQ = GetKeyBytes(reader);
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        public String GetName()
        {
            return "EncryptedKey";
        }

        public void SaveToFile(String FileName)
        {
            FileStream fStream = File.Open(FileName, FileMode.Create);
            BinaryWriter bWriter = new BinaryWriter(fStream);
            bWriter.Write(N.Length);
            bWriter.Write(N);
            bWriter.Write(P.Length);
            bWriter.Write(P);
            bWriter.Write(Q.Length);
            bWriter.Write(Q);
            bWriter.Write(D.Length);
            bWriter.Write(D);
            bWriter.Write(E.Length);
            bWriter.Write(E);
            bWriter.Write(DP.Length);
            bWriter.Write(DP);
            bWriter.Write(DQ.Length);
            bWriter.Write(DQ);
            bWriter.Write(InverseQ.Length);
            bWriter.Write(InverseQ);
            bWriter.Flush();
            fStream.Close();
        }

        public void LoadFromFile(String FileName)
        {
            FileStream fStream = File.Open(FileName, FileMode.Open);
            BinaryReader bReader = new BinaryReader(fStream);
            int len = bReader.ReadInt32();
            N = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            P = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            Q = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            D = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            E = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            DP = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            DQ = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            InverseQ = bReader.ReadBytes(len);
            fStream.Close();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public RSAParameters GetKey()
        {
            RSAParameters key = new RSAParameters();
            key.Modulus = DecryptBytes(N);
            key.P = DecryptBytes(P);
            key.Q = DecryptBytes(Q);
            key.D = DecryptBytes(D);
            key.Exponent = DecryptBytes(E);
            key.DP = DecryptBytes(DP);
            key.DQ = DecryptBytes(DQ);
            key.InverseQ = DecryptBytes(InverseQ);
            return key;
        }

        public CPublicKey GetPublicKey()
        {
            return new CPublicKey(DecryptBytes(N), DecryptBytes(E));
        }
    }
}
