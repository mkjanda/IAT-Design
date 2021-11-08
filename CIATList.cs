using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace IATClient
{
    class CIATList : INamedXmlSerializable
    {
        private List<String> _IATNames = new List<String>();
        private List<CIATAuthorInfo> _Authors = new List<CIATAuthorInfo>();

        public List<String> IATNames
        {
            get {
                return _IATNames;
            }
        }

        public List<CIATAuthorInfo> Authors
        {
            get {
                return _Authors;
            }
        }

        public void Retrieve(String ServerURL)
        {
        }

        public String GetName()
        {
            return "IATList";
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.IsEmptyElement)
                return;
            reader.ReadStartElement("IATList");
            while (reader.IsStartElement())
            {
                reader.ReadStartElement("IAT");
                _IATNames.Add(reader.ReadElementString("IATName"));
                CIATAuthorInfo info = new CIATAuthorInfo();
                info.ReadXml(reader);
                _Authors.Add(info);
            }
            reader.ReadEndElement();
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        public class CIATAuthorInfo 
        {
            private String _FName, _LName, _EMail, _Title;

            public String FName
            {
                get
                {
                    return _FName;
                }
            }

            public String LName
            {
                get
                {
                    return _LName;
                }
            }

            public String Title
            {
                get
                {
                    return _Title;
                }
            }

            public String FullName
            {
                get
                {
                    return Title + " " + FName + " " + LName;
                }
            }

            public String EMail
            {
                get
                {
                    return _EMail;
                }
            }


            public void ReadXml(XmlReader reader)
            {
                reader.ReadStartElement("UserInfo");
                _Title = reader.ReadElementString();
                _FName = reader.ReadElementString();
                _LName = reader.ReadElementString();
                _EMail = reader.ReadElementString();
                reader.ReadEndElement();
            }
        }
    }
}
