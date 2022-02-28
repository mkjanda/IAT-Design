using System;
using System.IO;
using System.Security.Cryptography;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;

namespace IATClient
{
    public class PartiallyEncryptedRSAData : INamedXmlSerializable
    {
        private String NString = String.Empty, EString = String.Empty;
        private byte[] D, E, P, Q, N, DP, DQ, InverseQ;
        private String EncryptedKey;
        public enum EKeyType { None, Data, Admin };
        private EKeyType KeyType;
        private bool _Decrypted = false;

        public bool IsDecrypted
        {
            get
            {
                return _Decrypted;
            }
        }

        public static readonly byte[] IV = new byte[] { (byte)0xFA, (byte)0x64, (byte)0x92, (byte)0x21, (byte)0x4A, (byte)0x74, (byte)0x41, (byte)0xE9 };
        public PartiallyEncryptedRSAData(EKeyType keyType)
        {
            KeyType = keyType;
        }

        public static PartiallyEncryptedRSAData CreateNullKey()
        {
            PartiallyEncryptedRSAData key = new PartiallyEncryptedRSAData(EKeyType.None);
            key.NString = "NULL";
            key.EString = "NULL";
            key.EncryptedKey = "NULL";
            return key;
        }

        private int base64Val(char ch)
        {
            if (('A' <= ch) && (ch <= 'Z'))
                return ch - 'A';
            if (('a' <= ch) && (ch <= 'z'))
                return 26 + ch - 'a';
            if (('0' <= ch) && (ch <= '9'))
                return 52 + ch - '0';
            if (ch == '+')
                return 62;
            if (ch == '/')
                return 63;
            return -1;
        }

        private char fromBase64Val(int val)
        {
            if (val < 26)
                return (char)('A' + (char)val);
            if (val < 52)
                return (char)('a' + (char)val - 26);
            if (val < 62)
                return (char)('0' + (char)val - 52);
            if (val == 62)
                return '+';
            return '/';
        }

        public static byte[] stringToDESCipherKey(String input)
        {
            byte[] productHex = System.Text.Encoding.Unicode.GetBytes(input);
            uint[] productNums = new uint[12];
            for (int ctr = 0; ctr < 12; ctr++)
                productNums[ctr] = 0;
            int ndx = 0;


            for (int ctr = 0; ctr < productHex.Length; ctr++)
            {
                productNums[ndx] ^= (uint)(productHex[ctr] & 0xFF);
                productNums[11 - ndx] ^= (uint)(productHex[ctr] << 8) & 0xFF00;
                ndx++;
                if (ndx >= 12)
                    ndx = 0;
            }
            ulong[] cipherNums = new ulong[14];
            cipherNums[0] = productNums[6] * productNums[11];
            cipherNums[5] = productNums[Math.Abs((int)(cipherNums[0] % 12))] * productNums[2];
            cipherNums[11] = productNums[Math.Abs((int)(cipherNums[5] % 12))] * productNums[Math.Abs((int)(cipherNums[0] % 12))];
            cipherNums[2] = productNums[Math.Abs((int)(cipherNums[5] % 12))] * productNums[Math.Abs((int)(cipherNums[5] % 12))];
            cipherNums[13] = productNums[Math.Abs((int)(cipherNums[11] % 12))] * productNums[Math.Abs((int)(cipherNums[2] % 12))];
            cipherNums[1] = productNums[Math.Abs((int)(cipherNums[13] % 12))] * productNums[Math.Abs((int)(cipherNums[0] % 12))];
            cipherNums[7] = productNums[Math.Abs((int)(cipherNums[1] % 12))] * productNums[Math.Abs((int)(cipherNums[11] % 12))];
            cipherNums[3] = productNums[Math.Abs((int)(cipherNums[7] % 12))] * productNums[Math.Abs((int)(cipherNums[5] % 12))];
            cipherNums[9] = productNums[Math.Abs((int)(cipherNums[2] % 12))] * productNums[Math.Abs((int)(cipherNums[2] % 12))];
            cipherNums[4] = productNums[Math.Abs((int)(cipherNums[13] % 12))] * productNums[Math.Abs((int)(cipherNums[1] % 12))];
            cipherNums[6] = productNums[Math.Abs((int)(cipherNums[5] % 12))] * productNums[Math.Abs((int)(cipherNums[2] % 12))];
            cipherNums[8] = productNums[Math.Abs((int)(cipherNums[6] % 12))] * productNums[Math.Abs((int)(cipherNums[4] % 12))];
            cipherNums[10] = productNums[Math.Abs((int)(cipherNums[3] % 12))] * productNums[Math.Abs((int)(cipherNums[9] % 12))];
            cipherNums[12] = productNums[Math.Abs((int)(cipherNums[10] % 12))] * productNums[Math.Abs((int)(cipherNums[13] % 12))];


            byte[] cipher = new byte[8];
            for (int ctr = 0; ctr < 8; ctr++)
                cipher[ctr] = 0;
            for (int ctr = 0; ctr < 7; ctr++)
            {
                ulong val = ((ulong)cipherNums[ctr] << 32) + (ulong)cipherNums[7 + ctr];
                cipher[0] ^= (byte)(0xFF & (val >> 56));
                cipher[1] ^= (byte)(0xFF & (val >> 48));
                cipher[2] ^= (byte)(0xFF & (val >> 40));
                cipher[3] ^= (byte)(0xFF & (val >> 32));
                cipher[4] ^= (byte)(0xFF & (val >> 24));
                cipher[5] ^= (byte)(0xFF & (val >> 16));
                cipher[6] ^= (byte)(0xFF & (val >> 8));
                cipher[7] ^= (byte)(0xFF & (val));
            }
            return cipher;
        }


        public String GetName()
        {
            return "PartiallyEncryptedRSAKey";
        }

        public void DecryptKey(String password)
        {
            if (_Decrypted)
                return;
            byte[] desCipher = stringToDESCipherKey(password);
            MemoryStream memStream = new MemoryStream(Convert.FromBase64String(EncryptedKey));
            DESCryptoServiceProvider desCrypt = new DESCryptoServiceProvider();
            desCrypt.Mode = CipherMode.CBC;
            desCrypt.Padding = PaddingMode.ISO10126;
            MemoryStream keyStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(keyStream, desCrypt.CreateDecryptor(desCipher, IV), CryptoStreamMode.Write);
            memStream.Seek(0, SeekOrigin.Begin);
            cStream.Write(memStream.ToArray(), 0, (int)memStream.Length);
            cStream.FlushFinalBlock();
            keyStream.Seek(0, SeekOrigin.Begin);
            BinaryReader bReader = new BinaryReader(keyStream);
            int len = bReader.ReadInt32();
            N = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            E = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            D = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            P = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            Q = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            DP = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            DQ = bReader.ReadBytes(len);
            len = bReader.ReadInt32();
            InverseQ = bReader.ReadBytes(len);
            _Decrypted = true;
        }

        public void Generate(String password)
        {
            byte[] desCipher = stringToDESCipherKey(password);
            RSACryptoServiceProvider rsaCrypt = new RSACryptoServiceProvider();
            RSAParameters rsaParams = rsaCrypt.ExportParameters(true);
            N = rsaParams.Modulus;
            E = rsaParams.Exponent;
            D = rsaParams.D;
            P = rsaParams.P;
            Q = rsaParams.Q;
            DP = rsaParams.DP;
            DQ = rsaParams.DQ;
            InverseQ = rsaParams.InverseQ;
            MemoryStream memStream = new MemoryStream();
            BinaryWriter bWriter = new BinaryWriter(memStream);
            bWriter.Write((Int32)N.Length);
            bWriter.Write(N);
            bWriter.Write((Int32)E.Length);
            bWriter.Write(E);
            bWriter.Write((Int32)D.Length);
            bWriter.Write(D);
            bWriter.Write((Int32)P.Length);
            bWriter.Write(P);
            bWriter.Write((Int32)Q.Length);
            bWriter.Write(Q);
            bWriter.Write((Int32)DP.Length);
            bWriter.Write(DP);
            bWriter.Write((Int32)DQ.Length);
            bWriter.Write(DQ);
            bWriter.Write((Int32)InverseQ.Length);
            bWriter.Write(InverseQ);
            bWriter.Flush();
            MemoryStream encryptedKey = new MemoryStream();
            DESCryptoServiceProvider desCrypt = new DESCryptoServiceProvider();
            desCrypt.Mode = CipherMode.CBC;
            desCrypt.Padding = PaddingMode.ISO10126;
            CryptoStream cStream = new CryptoStream(encryptedKey, desCrypt.CreateEncryptor(desCipher, IV), CryptoStreamMode.Write);
            memStream.Seek(0, SeekOrigin.Begin);
            cStream.Write(memStream.ToArray(), 0, (int)memStream.Length);
            cStream.FlushFinalBlock();
            EncryptedKey = Convert.ToBase64String(encryptedKey.ToArray());
            NString = Convert.ToBase64String(rsaParams.Modulus);
            EString = Convert.ToBase64String(rsaParams.Exponent);
        }

        public void WriteXml(XmlWriter writer)
        {
            if (KeyType == EKeyType.Admin)
                writer.WriteStartElement("AdminKey");
            else if (KeyType == EKeyType.Data)
                writer.WriteStartElement("DataKey");
            else
                writer.WriteStartElement("PartiallyEncryptedRSAKey");
            writer.WriteElementString("KeyType", KeyType.ToString());
            writer.WriteElementString("Modulus", NString);
            writer.WriteElementString("Exponent", EString);
            writer.WriteElementString("EncryptedKey", EncryptedKey);
            writer.WriteEndElement();
        }

        public void Load(XElement root)
        {
            Enum.TryParse<EKeyType>(root.Element("KeyType").Value, out EKeyType keyType);
            KeyType = keyType;
            NString = root.Element("Modulus").Value;
            EString = root.Element("Exponent").Value;
            EncryptedKey = root.Element("EncryptedKey").Value;
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            KeyType = (EKeyType)Enum.Parse(typeof(EKeyType), reader.ReadElementString());
            NString = reader.ReadElementString();
            EString = reader.ReadElementString();
            EncryptedKey = reader.ReadElementString();
            reader.ReadEndElement();
        }

        public bool IsNullKey()
        {
            if ((KeyType == EKeyType.None) && (NString == "NULL") && (EString == "NULL") && (EncryptedKey == "NULL"))
                return true;
            return false;
        }

        public XmlSchema GetSchema()
        {
            return null;
        }

        public RSAParameters GetRSAParameters()
        {
            RSAParameters rsaParams = new RSAParameters();
            rsaParams.Modulus = N;
            rsaParams.Exponent = E;
            rsaParams.D = D;
            rsaParams.P = P;
            rsaParams.Q = Q;
            rsaParams.DP = DP;
            rsaParams.DQ = DQ;
            rsaParams.InverseQ = InverseQ;
            return rsaParams;
        }
    }

    class CRSAKeyPair : INamedXmlSerializable
    {
        private PartiallyEncryptedRSAData _DataKey, _AdminKey;

        public PartiallyEncryptedRSAData DataKey
        {
            get
            {
                return _DataKey;
            }
            set
            {
                _DataKey = value;
            }
        }

        public PartiallyEncryptedRSAData AdminKey
        {
            get
            {
                return _AdminKey;
            }
            set
            {
                _AdminKey = value;
            }
        }

        public CRSAKeyPair()
        {
            _DataKey = new PartiallyEncryptedRSAData(PartiallyEncryptedRSAData.EKeyType.Data);
            _AdminKey = new PartiallyEncryptedRSAData(PartiallyEncryptedRSAData.EKeyType.Admin);
        }

        public CRSAKeyPair(PartiallyEncryptedRSAData dataKey, PartiallyEncryptedRSAData adminKey)
        {
            _DataKey = dataKey;
            _AdminKey = adminKey;
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("RSAKeyPair");
            DataKey.WriteXml(writer);
            AdminKey.WriteXml(writer);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            DataKey.ReadXml(reader);
            AdminKey.ReadXml(reader);
            reader.ReadEndElement();
        }

        public String GetName()
        {
            return "RSAKeyPair";
        }
    }
}
