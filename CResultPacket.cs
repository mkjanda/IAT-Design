using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Security.Cryptography;
using IATClient.IATResultSetNamespaceV2;

namespace IATClient
{
    class CResultSet 
    {
        private List<long> KeyOffsets, IVOffsets, DataOffsets;
        private List<int> KeyLengths, IVLengths, DataLengths;

        public String Timestamp { get; private set; } = String.Empty;
        public long ResultID { get; private set; } = -1;
        public String Token { get; private set; } = null;
        public byte[] ByteData { get; private set; } = null;
        static private DESCryptoServiceProvider DESCrypt = new DESCryptoServiceProvider();
        static private RSACryptoServiceProvider RSACrypt = new RSACryptoServiceProvider();
        private IResultElemFactory ResultElemFactory;

        static CResultSet()
        {
            DESCrypt.Mode = CipherMode.CBC;
            DESCrypt.Padding = PaddingMode.ISO10126;
        }

        public CResultSet()
        {
            KeyOffsets = new List<long>();
            IVOffsets = new List<long>();
            DataOffsets = new List<long>();
            KeyLengths = new List<int>();
            IVLengths = new List<int>();
            DataLengths = new List<int>();
        }

        public void SetResultElemFactory(IResultElemFactory fact)
        {
            ResultElemFactory = fact;
        }


        public CResultSet(IResultElemFactory fact)
        {
            KeyOffsets = new List<long>();
            IVOffsets = new List<long>();
            DataOffsets = new List<long>();
            KeyLengths = new List<int>();
            IVLengths = new List<int>();
            DataLengths = new List<int>();
            ResultElemFactory = fact;
        }

        public void Load(XElement elem)
        {
            ResultID = Convert.ToInt64(elem.Attribute("ResultId").Value);
            Timestamp = elem.Element("AdminTime").Value;
            ByteData = Convert.FromBase64String(elem.Element("ResultData").Value);
            foreach (var tocElem in elem.Element("TOC").Elements("ResultTOCEntry"))
            {
                KeyOffsets.Add(Convert.ToInt64(tocElem.Element("KeyOffset").Value));
                KeyLengths.Add(Convert.ToInt32(tocElem.Element("KeyLength").Value));
                IVOffsets.Add(Convert.ToInt64(tocElem.Element("IVOffset").Value));
                IVLengths.Add(Convert.ToInt32(tocElem.Element("IVLength").Value));
                DataOffsets.Add(Convert.ToInt64(tocElem.Element("DataOffset").Value));
                DataLengths.Add(Convert.ToInt32(tocElem.Element("DataLength").Value));
            }
            if (elem.Attribute("Token") != null)
                Token = elem.Attribute("Token").Value;
        }

        private MemoryStream ProcessChunk(int elemNum)
        {
            byte[] encryptedKey = new byte[KeyLengths[elemNum]];
            Array.Copy(ByteData, KeyOffsets[elemNum], encryptedKey, 0, KeyLengths[elemNum]);
            byte[] encryptedIV = new byte[IVLengths[elemNum]];
            Array.Copy(ByteData, IVOffsets[elemNum], encryptedIV, 0, IVLengths[elemNum]);
            byte[] encryptedData = new byte[DataLengths[elemNum]];
            Array.Copy(ByteData, DataOffsets[elemNum], encryptedData, 0, DataLengths[elemNum]);
            byte[] desKey = RSACrypt.Decrypt(encryptedKey, RSAEncryptionPadding.Pkcs1);
            byte[] desIV = RSACrypt.Decrypt(encryptedIV, RSAEncryptionPadding.Pkcs1);
            MemoryStream memStream = new MemoryStream();
            CryptoStream cStream = new CryptoStream(memStream, DESCrypt.CreateDecryptor(desKey, desIV), CryptoStreamMode.Write);
            cStream.Write(encryptedData, 0, encryptedData.Length);
            cStream.FlushFinalBlock();
            memStream.Seek(0, SeekOrigin.Begin);
            return memStream;
        }
        

        public IResultSet GenerateResultSet(CPartiallyEncryptedRSAKey key)
        {
            RSACrypt.ImportParameters(key.GetRSAParameters());
            int elemCtr = 0;
            MemoryStream inStream;
            XmlReader reader;
            IResultSet resultSet;
            if (Token != null)
                resultSet = ResultElemFactory.CreateResultSet(Token);
            else
                resultSet = ResultElemFactory.CreateResultSet();
            resultSet.ResultID = ResultID;
            try
            {
                foreach (IResultSetElem rse in resultSet)
                {
                    inStream = ProcessChunk(elemCtr++);
                    TextReader txtReader = new StreamReader(inStream, Encoding.UTF8);
                    reader = new XmlTextReader(txtReader);
                    reader.MoveToContent();
                    rse.ReadXml(reader);
                }
                if (!ResultElemFactory.VerifyResultSet(resultSet))
                    return null;
                return resultSet;
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
