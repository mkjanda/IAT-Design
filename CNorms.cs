using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using System.IO;

namespace IATClient
{
    public class CNorms : CSyncEvent
    {
        public enum EAction { none, store, delete, replace, retrieve };
        private EAction _Action;
        private double _Mean, _SD;
        private String NormData = String.Empty;
        private RSACryptoServiceProvider Crypt = null;
        private RSAParameters RSAParams;

        public CNorms(RSACryptoServiceProvider crypt)
        {
            _Mean = Double.NaN;
            _SD = Double.NaN;
            _Action = EAction.none;
            Crypt = crypt;
            RSAParams = crypt.ExportParameters(true);
        }

        public CNorms(EAction action, double mean, double sd)
        {
            _Mean = mean;
            _SD = sd;
            _Action = action;
            Crypt = null;
        }

        public EAction Action
        {
            get
            {
                return _Action;
            }
        }

        public double Mean
        {
            get
            {
                if (_Mean == Double.NaN)
                {
                    if (Crypt == null)
                        return Double.NaN;
                    else
                    {
                        Decrypt();
                        return _Mean;
                    }
                }
                else
                    return _Mean;
            }
        }

        public double SD
        {
            get
            {
                if (_SD == Double.NaN)
                {
                    if (Crypt == null)
                        return Double.NaN;
                    else
                    {
                        Decrypt();
                        return _SD;
                    }
                }
                else
                    return _SD;
            }
        }

        public override ESyncEvents GetEventType()
        {
            return ESyncEvents.Norms;
        }

        private void Decrypt()
        {
            String desStr = NormData.Substring(0, NormData.IndexOf('|'));
            byte[] desKeyBytes = Crypt.Decrypt(Convert.FromBase64String(desStr), false);
            DESCryptoServiceProvider desCrypt = new DESCryptoServiceProvider();
            byte []IVBytes = new byte[8];
            for (int ctr = 0; ctr < 8; ctr++)
                IVBytes[ctr] = 0;
            desCrypt.IV = IVBytes;
            desCrypt.Key = desKeyBytes;
            String normDataStr = NormData.Substring(NormData.IndexOf('|') + 1);
            MemoryStream normStream = new MemoryStream(Convert.FromBase64String(normDataStr));
            CryptoStream cStream = new CryptoStream(normStream, desCrypt.CreateDecryptor(desKeyBytes, IVBytes), CryptoStreamMode.Read);
            byte[] normStrData = new byte[cStream.Length];
            cStream.Read(normStrData, 0, (int)cStream.Length);
            String normString = System.Text.Encoding.UTF8.GetString(normStrData);
            String meanStr = normString.Substring(0, normString.IndexOf('|'));
            _Mean = Convert.ToDouble(meanStr);
            String sdStr = normString.Substring(NormData.IndexOf('|') + 1);
            _SD = Convert.ToDouble(sdStr);
        }

        public override void ReadXml(XmlReader reader)
        {
            _Action = (EAction)Enum.Parse(typeof(EAction), reader["Action"]);
            reader.ReadStartElement();
            NormData = reader.ReadElementString();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetName());
            writer.WriteAttributeString("Action", Action.ToString());
            writer.WriteElementString("Mean", Mean.ToString());
            writer.WriteElementString("SD", SD.ToString());
            writer.WriteEndElement();
        }
    }
}
