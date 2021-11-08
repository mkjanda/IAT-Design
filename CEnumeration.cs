using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Security.Cryptography;
using System.IO;

namespace IATClient
{
    class CEnumerationCollection : CSyncEvent
    {
        private static long FreeEnumID = -1;
        private RSACryptoServiceProvider Crypt;
        private List<CEnumeration> _Enumerations = new List<CEnumeration>();

        public CEnumerationCollection(RSACryptoServiceProvider crypt)
        {
            Crypt = crypt;
        }

        public override ESyncEvents GetEventType()
        {
            return ESyncEvents.Enumeration;
        }

        public List<CEnumeration> Enumerations
        {
            get
            {
                return _Enumerations;
            }
        }

        public override void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            int nElems = Convert.ToInt32(reader[0]);
            for (int ctr = 0; ctr < nElems; ctr++)
            {
                CEnumeration enumeration = new CEnumeration(Crypt);
                enumeration.ReadXml(reader);
                Enumerations.Add(enumeration);
            }
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetName());
            writer.WriteAttributeString("NumElements", Enumerations.Count.ToString());
            for (int ctr = 0; ctr < Enumerations.Count; ctr++)
            {
                CEnumeration enumeration = new CEnumeration(Crypt);
                enumeration.WriteXml(writer);
                Enumerations.Add(enumeration);
            }
            writer.WriteEndElement();
        }
    }

    public class CEnumeration : CSyncEvent
    {
        public enum EAction { none, store, delete, replace, retrieve, dataModified };
        private long _EnumerationID;
        private List<long> _ResultIDs = new List<long>();
        private EAction _Action;
        private RSACryptoServiceProvider Crypt;
        static long FreeEnumID = -1;
        private Dictionary<int, CEnumeratedValueList> _EnumeratedValues = new Dictionary<int, CEnumeratedValueList>();
        private CEnumerationDescription _Description = null;

        public CEnumerationDescription Description
        {
            get
            {
                return _Description;
            }
        }

        public Dictionary<int, CEnumeratedValueList> EnumeratedValues
        {
            get
            {
                return _EnumeratedValues;
            }
        }


        public CEnumeration(RSACryptoServiceProvider crypt)
        {
            _Action = EAction.none;
            _EnumerationID = FreeEnumID--;
            Crypt = crypt;
            _Description = new CEnumerationDescription(crypt);
        }

        public override ESyncEvents GetEventType()
        {
            return ESyncEvents.Enumeration;
        }

        public CEnumeration(RSACryptoServiceProvider crypt, EAction action)
        {
            _Action = action;
            _EnumerationID = FreeEnumID--;
            Crypt = crypt;
        }

        public CEnumeration(RSACryptoServiceProvider crypt, EAction action, String name, List<String> enumerationNames, List<CEnumeratedValueList> enumValues)
        {
            _Description = new CEnumerationDescription(crypt, name, enumerationNames);
            _Action = action;
            _EnumerationID = FreeEnumID--;
            Crypt = crypt;

        }

        public long EnumerationID
        {
            get
            {
                return _EnumerationID;
            }
        }

        public EAction Action
        {
            get
            {
                return _Action;
            }
        }

        public String Name
        {
            get
            {
                return Description.Name;
            }
        }

        public List<long> ResultIDs
        {
            get
            {
                return _ResultIDs;
            }
        }

        private CEnumeratedValueList GetEnumeratedValue(String value)
        {
            return EnumeratedValues[Description.Ordinal(value)];
        }


        public override void ReadXml(XmlReader reader)
        {
            int nEnumeratedTypes = Convert.ToInt32(reader[0]);
            _Action = (EAction)Enum.Parse(typeof(EAction), reader[1]);
            reader.ReadStartElement();
            _EnumerationID = Convert.ToInt32(reader.ReadElementString());
            Description.ReadXml(reader);
            reader.ReadStartElement();
            for (int ctr = 0; ctr < nEnumeratedTypes; ctr++)
            {
                reader.ReadStartElement();
                int nResultIDs = Convert.ToInt32(reader.ReadElementString());
                int typeID = Convert.ToInt32(reader.ReadElementString());
                CEnumeratedValueList evl = new CEnumeratedValueList(this, typeID, Description[typeID]);
                evl.ReadXml(reader);
                EnumeratedValues[typeID] = evl;
                reader.ReadElementString();
            }
            reader.ReadEndElement();
            reader.ReadEndElement();
        }

        public override void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("Enumeration");
            writer.WriteAttributeString("SyncEventType", CSyncEvent.ESyncEvents.Enumeration.ToString());
            writer.WriteAttributeString("NumElements", Description.NumEnumeratedTypes.ToString());
            writer.WriteElementString("EnumerationID", EnumerationID.ToString());
            Description.WriteXml(writer);
            writer.WriteStartElement("EnumerationValues");
            foreach (int ndx in EnumeratedValues.Keys)
            {
                writer.WriteElementString("EnumeratedTypeID", ndx.ToString());
                EnumeratedValues[ndx].WriteXml(writer);
            }
            writer.WriteEndElement();
            writer.WriteEndElement();
        }

        public String this[long resultID]
        {
            get
            {
                foreach (int id in EnumeratedValues.Keys)
                    if (EnumeratedValues[id].Contains(resultID))
                        return Description[id];
                return String.Empty;
            }
            set
            {
                if (Action == EAction.retrieve)
                    _Action = EAction.dataModified;
                int currNdx = -1;
                foreach (int id in EnumeratedValues.Keys)
                    if (EnumeratedValues[id].Contains(resultID))
                    {
                        currNdx = id;
                        break;
                    }
                if ((value == String.Empty) || (currNdx != -1))
                {
                    EnumeratedValues[currNdx].Remove(resultID);
                    return;
                }
                else if (value != String.Empty)
                    throw new InvalidOperationException();
                EnumeratedValues[currNdx].Remove(resultID);
                int ndx = Description.Ordinal(value);
                EnumeratedValues[ndx].Store(resultID);
            }
        }

        public class CEnumerationDescription : INamedXmlSerializable
        {
            private String _Name = String.Empty;
            private Dictionary<int, String> EnumeratedTypes = new Dictionary<int, String>();
            private RSACryptoServiceProvider Crypt = null;
            private static int FreeID = -1;
            private int ID = -1;

            public String Name
            {
                get
                {
                    return _Name;
                }
            }

            public String this[int ndx]
            {
                get
                {
                    return EnumeratedTypes[ndx];
                }
            }


            public int NumEnumeratedTypes
            {
                get
                {
                    int nTypes = 0;
                    foreach (String type in EnumeratedTypes.Values)
                        if (type != String.Empty)
                            nTypes++;
                    return nTypes;
                }
            }

            public int Ordinal(String value)
            {
                foreach (int ndx in EnumeratedTypes.Keys)
                    if (EnumeratedTypes[ndx] == value)
                        return ndx;
                return -1;
            }

            public List<String> GetAllEnumerationTypes()
            {
                List<String> strList = new List<String>();
                foreach (int ndx in EnumeratedTypes.Keys)
                    strList.Add(EnumeratedTypes[ndx]);
                strList.Sort();
                return strList;
            }

            public CEnumerationDescription(RSACryptoServiceProvider crypt)
            {
                Crypt = crypt;
                ID = FreeID--;
            }

            public CEnumerationDescription(RSACryptoServiceProvider crypt, String name, List<String> values)
            {
                Crypt = crypt;
                ID = FreeID--;
                _Name = name;
                foreach (String str in values)
                    AddElement(str);
            }

            public String GetName()
            {
                return "EnumeratedTypes";
            }

            public void AddElement(String value)
            {
                int ctr = 1;
                while (EnumeratedTypes.ContainsKey(ctr))
                    ctr++;
                EnumeratedTypes[ctr] = value;
            }

            private byte[] CryptBytes(ICryptoTransform desTrans, byte[] input)
            {
                MemoryStream memStream = new MemoryStream();
                CryptoStream cStream = new CryptoStream(memStream, desTrans, CryptoStreamMode.Write);
                cStream.Write(input, 0, input.Length);
                cStream.FlushFinalBlock();
                return memStream.ToArray();
            }

            public void ReadXml(XmlReader reader)
            {
                EnumeratedTypes.Clear();
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                byte[] IVData = new byte[8];
                System.Array.Clear(IVData, 0, 8);
                byte[] encName = Convert.FromBase64String(reader.ReadElementString());
                byte[] encCipher = Convert.FromBase64String(reader.ReadElementString());
                byte[] cipher = Crypt.Decrypt(encCipher, false);
                FromBase64Transform b64Stream = new FromBase64Transform();
                DESCryptoServiceProvider desCrypt = new DESCryptoServiceProvider();
                _Name = System.Text.Encoding.UTF8.GetString(CryptBytes(desCrypt.CreateDecryptor(cipher, IVData), encName));
                int nElems = Convert.ToInt32(reader[0]);
                reader.ReadStartElement();
                for (int ctr = 0; ctr < nElems; ctr++)
                {
                    reader.ReadStartElement();
                    int ID = Convert.ToInt32(reader.ReadElementString());
                    byte[] encTypeName = Convert.FromBase64String(reader.ReadElementString());
                    EnumeratedTypes[ID] = System.Text.Encoding.UTF8.GetString(CryptBytes(desCrypt.CreateDecryptor(cipher, IVData), encTypeName));
                    reader.ReadEndElement();
                }
                reader.ReadEndElement();
                reader.ReadEndElement();
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                DESCryptoServiceProvider desCrypt = new DESCryptoServiceProvider();
                desCrypt.GenerateKey();
                byte[] IVData = new byte[8];
                System.Array.Clear(IVData, 0, 8);
                byte[] encName = CryptBytes(desCrypt.CreateEncryptor(desCrypt.Key, IVData), System.Text.Encoding.UTF8.GetBytes(_Name));
                writer.WriteElementString("Name", Convert.ToBase64String(encName));
                byte[] encCipher = Crypt.Encrypt(desCrypt.Key, false);
                writer.WriteElementString("Cipher", Convert.ToBase64String(encCipher));
                writer.WriteStartElement("EnumerationList");
                writer.WriteAttributeString("NumElements", EnumeratedTypes.Count.ToString());
                foreach (int ndx in EnumeratedTypes.Keys)
                {
                    writer.WriteStartElement("EnumeratedType");
                    writer.WriteElementString("ID", ndx.ToString());
                    byte[] encTypeName = CryptBytes(desCrypt.CreateEncryptor(desCrypt.Key, IVData), System.Text.Encoding.UTF8.GetBytes(EnumeratedTypes[ndx]));
                    writer.WriteElementString("Value", Convert.ToBase64String(encTypeName));
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        public class CEnumeratedValueList : INamedXmlSerializable
        {
            private CEnumeration Parent = null;
            private String _TypeName;
            private int _TypeID;
            private List<long> NewResultIDs = new List<long>();
            private List<long> OriginalResultIDs = new List<long>();
            private List<long> DeletedResultIDs = new List<long>();
            private Dictionary<long, EAction> Actions = new Dictionary<long, EAction>();
            public enum EAction { none, store, delete, retrieve, replace };
            private EAction _Action;

            public String TypeName
            {
                get
                {
                    return _TypeName;
                }
            }

            public int TypeID
            {
                get
                {
                    return _TypeID;
                }
            }

            public List<long> ResultIDs
            {
                get
                {
                    List<long> result = new List<long>();
                    foreach (long id in OriginalResultIDs)
                        if (!DeletedResultIDs.Contains(id))
                            result.Add(id);
                    foreach (long id in NewResultIDs)
                        result.Add(id);
                    return result;
                }
            }

            public EAction Action
            {
                get
                {
                    return _Action;
                }
            }

            private CEnumeratedValueList(CEnumeration parent)
            {
                Parent = parent;
            }

            public CEnumeratedValueList(CEnumeration parent, int typeID, String typeName)
            {
                _TypeName = typeName;
                _TypeID = typeID;
                _Action = EAction.none;
                Parent = parent;
            }

            public void initialize(List<long> resultIDs)
            {
                OriginalResultIDs.Clear();
                NewResultIDs.Clear();
                DeletedResultIDs.Clear();
                OriginalResultIDs.AddRange(resultIDs);
            }

            public void Store(long resultID)
            {
                bool bContainedInOtherOriginal = false;
                bool bContainedInOtherDeleted = false;
                bool bContainedInOtherNew = false;
                CEnumeratedValueList otherList = null;

                foreach (CEnumeratedValueList evl in Parent.EnumeratedValues.Values)
                {
                    if (evl == this)
                        continue;
                    if (evl.NewResultIDs.Contains(resultID))
                    {
                        bContainedInOtherNew = true;
                        otherList = evl;
                        break;
                    }
                    else if (evl.DeletedResultIDs.Contains(resultID))
                    {
                        bContainedInOtherDeleted = true;
                        otherList = evl;
                        break;
                    }
                    else if (evl.OriginalResultIDs.Contains(resultID))
                    {
                        bContainedInOtherOriginal = true;
                        otherList = evl;
                        break;
                    }
                }


                if (NewResultIDs.Contains(resultID))
                    return;
                if (DeletedResultIDs.Contains(resultID))
                {
                    if (!OriginalResultIDs.Contains(resultID))
                    {
                        NewResultIDs.Add(resultID);
                        Actions[resultID] = EAction.store;
                    }
                    else
                    {
                        DeletedResultIDs.Remove(resultID);
                        Actions[resultID] = EAction.retrieve;
                    }
                }
                else if (!OriginalResultIDs.Contains(resultID))
                {
                    NewResultIDs.Add(resultID);
                    if (bContainedInOtherDeleted)
                        Actions[resultID] = EAction.replace;
                    else if (bContainedInOtherOriginal)
                    {
                        NewResultIDs.Add(resultID);
                        Actions[resultID] = EAction.replace;
                    }
                    else if (bContainedInOtherNew)
                    {
                        otherList.NewResultIDs.Remove(resultID);
                        otherList.Actions.Remove(resultID);
                        Actions[resultID] = EAction.store;
                    }
                }
            }

            public void Remove(long resultID)
            {
                if (NewResultIDs.Contains(resultID))
                    NewResultIDs.Remove(resultID);
                else if (!DeletedResultIDs.Contains(resultID))
                {
                    DeletedResultIDs.Add(resultID);
                    Actions[resultID] = EAction.delete;
                }
            }

            public bool Contains(long resultID)
            {
                if (OriginalResultIDs.Contains(resultID) && !DeletedResultIDs.Contains(resultID))
                    return true;
                if (NewResultIDs.Contains(resultID))
                    return true;
                return false;
            }

            public String GetName()
            {
                return "EnumeratedValueList";
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                OriginalResultIDs.Clear();
                NewResultIDs.Clear();
                DeletedResultIDs.Clear();
                Actions.Clear();
                int nValues = Convert.ToInt32(reader[0]);
                reader.ReadStartElement();
                for (int ctr = 0; ctr < nValues; ctr++)
                {
                    OriginalResultIDs.Add(Convert.ToInt64(reader.ReadElementString()));
                    Actions[OriginalResultIDs.Last()] = EAction.retrieve;
                }
                reader.ReadEndElement();
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                List<long> writtenVals = new List<long>();
                foreach (long resultID in Actions.Keys)
                    if (Actions[resultID] != EAction.retrieve)
                        writtenVals.Add(resultID);
                foreach (long resultID in writtenVals)
                {
                    writer.WriteStartElement("ResultID");
                    writer.WriteAttributeString("Action", Actions[resultID].ToString());
                    writer.WriteString(resultID.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }
    }
}