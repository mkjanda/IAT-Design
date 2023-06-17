using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
namespace IATClient
{
    public class CTestVersion
    {
        private String _Version;
        public String Version { get
            {
                return _Version;
            } set
            {
                Regex ex = new Regex("iat-([0-9]+)\\.([0-9]+)\\.([0-9]+)");
                var m = ex.Match(value);
                Major = Convert.ToInt32(m.Groups[1].Value);
                Minor = Convert.ToInt32(m.Groups[2].Value);
                Revision = Convert.ToInt32(m.Groups[3].Value);
            }
        }
        public int Major { get; private set; }
        public int Minor { get; private set; }
        public int Revision { get; private set; }

        public CTestVersion(string version)
        {
            Version = version;
        }

        public int CompareTo(CTestVersion other)
        {
            if (Major != other.Major)
                return Major - other.Major;
            if (Minor != other.Minor)
                return Minor - other.Minor;
            if (Revision != other.Revision)
                return Revision - other.Revision;
            return 0;
        }

    }


    class CIATReport
    {
        private String _AuthorTitle, _AuthorFName, _AuthorLName, _AuthorEMail, _LastDataRetrieval, _IATName, _URL;
        private int _TestSizeKB, _NumAdministrations, _NumResultSets;
        private bool _Deploying = false;

        public String URL
        {
            get
            {
                return _URL;
            }
        }

        public String AuthorTitle
        {
            get
            {
                return _AuthorTitle;
            }
        }

        public String AuthorFName
        {
            get
            {
                return _AuthorFName;
            }
        }

        public CTestVersion Version { get; private set; }

        public String AuthorLName
        {
            get
            {
                return _AuthorLName;
            }
        }

        public String AuthorEMail
        {
            get
            {
                return _AuthorEMail;
            }
        }

        public String LastDataRetrieval
        {
            get
            {
                return _LastDataRetrieval;
            }
        }

        public String IATName
        {
            get
            {
                return _IATName;
            }
        }

        public int TestSizeKB
        {
            get
            {
                return _TestSizeKB;
            }
        }

        public int NumAdministrations
        {
            get
            {
                return _NumAdministrations;
            }
        }

        public int NumResultSets
        {
            get
            {
                return _NumResultSets;
            }
            set
            {
                _NumResultSets = value;
            }
        }

        public bool Deploying
        {
            get
            {
                return _Deploying;
            }
        }

        public CIATReport() { }

        public String GetName()
        {
            return "IATReport";
        }

        public void ReadXml(XmlReader reader)
        {
            _Deploying = Convert.ToBoolean(reader.GetAttribute("Deploying"));
            reader.ReadStartElement();
            _IATName = reader.ReadElementString();
            _URL = reader.ReadElementString();
            _NumAdministrations = Convert.ToInt32(reader.ReadElementString());
            _NumResultSets = Convert.ToInt32(reader.ReadElementString());
            _TestSizeKB = Convert.ToInt32(reader.ReadElementString());
            _LastDataRetrieval = reader.ReadElementString();
            _AuthorTitle = reader.ReadElementString();
            _AuthorFName = reader.ReadElementString();
            _AuthorLName = reader.ReadElementString();
            _AuthorEMail = reader.ReadElementString();
            var ver = reader.ReadElementString();
            Version = new CTestVersion(ver);
            reader.ReadEndElement();
        }
    }

    class CServerReport : INamedXmlSerializable
    {
        private String _ContactFName, _ContactLName, _Organization, _DiskAlottmentRemainingMB, _DiskAlottmentMB;
        private int _NumAdministrationsRemaining, _NumAdministrations, _NumIATsAlotted;
        private Dictionary<String, CIATReport> _IATReports = new Dictionary<String, CIATReport>();

        public Dictionary<String, CIATReport> IATReports
        {
            get
            {
                return _IATReports;
            }
        }

        public String ContactFName
        {
            get
            {
                return _ContactFName;
            }
        }

        public String ContactLName
        {
            get
            {
                return _ContactLName;
            }
        }

        public String Organization
        {
            get
            {
                return _Organization;
            }
        }

        public String DiskAlottmentRemainingMB
        {
            get
            {
                return _DiskAlottmentRemainingMB;
            }
        }

        public String DiskAlottmentMB
        {
            get
            {
                return _DiskAlottmentMB;
            }
        }

        public int NumAdministrations
        {
            get
            {
                return _NumAdministrations;
            }
        }

        public int NumAdministrationsRemaining
        {
            get
            {
                return _NumAdministrationsRemaining;
            }
        }

        public int NumIATsAlotted
        {
            get
            {
                return _NumIATsAlotted;
            }
        }

        public CServerReport()
        {
        }

        public void RegisterIATDataDeletion(String iatName)
        {
            IATReports[iatName].NumResultSets = 0;
        }

        public void RegisterNewIAT(String iatName)
        {
            IATReports[iatName] = new CIATReport();
        }

        public void RegisterIATDeletion(String iatName)
        {
            _DiskAlottmentRemainingMB = (Convert.ToDouble(_DiskAlottmentRemainingMB) + (double)IATReports[iatName].TestSizeKB / 1024.0).ToString("F2");
            IATReports.Remove(iatName);
            LocalStorage.DeleteIAT(iatName);
        }

        public String GetName()
        {
            return "ServerReport";
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            _ContactFName = reader.ReadElementString();
            _ContactLName = reader.ReadElementString();
            _Organization = reader.ReadElementString();
            _NumIATsAlotted = Convert.ToInt32(reader.ReadElementString());
            _NumAdministrations = Convert.ToInt32(reader.ReadElementString());
            _NumAdministrationsRemaining = Convert.ToInt32(reader.ReadElementString());
            _DiskAlottmentMB = reader.ReadElementString();
            double diskRemaining = Convert.ToDouble(reader.ReadElementString()) / 1024.0;
            _DiskAlottmentRemainingMB = diskRemaining.ToString("F02");
            while (reader.Name == "IATReport")
            {
                CIATReport rep = new CIATReport();
                rep.ReadXml(reader);
                IATReports[rep.IATName] = rep;
            }
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
