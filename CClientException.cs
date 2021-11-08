using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    [Serializable]
    [XmlRoot(ElementName = "ClientException")]
    public class CClientException 
    {
        [XmlElement(ElementName = "ClientMessage", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        public String Message { get; set; }
        [XmlElement(ElementName = "ProductCode", Form = XmlSchemaForm.Unqualified)]
        public String ProductCode
        {
            get
            {
                return LocalStorage.Activation[LocalStorage.Field.ProductKey];
            }
            set { }
        }

        [XmlElement(ElementName = "ActivationKey", Form = XmlSchemaForm.Unqualified)]
        public String ActivationKey
        {
            get
            {
                return LocalStorage.Activation[LocalStorage.Field.ActivationKey];
            }
            set { }
        }
/*
        [XmlElement(ElementName = "UserEmail", Form = XmlSchemaForm.Unqualified)]
        public String UserKey
        {
            get
            {
                return LocalStorage.Activation[LocalStorage.Field.UserEmail];
            }
            set { }
        }
*/
        [XmlElement(ElementName = "Version", Form = XmlSchemaForm.Unqualified)]
        public String Version
        {
            get
            {
                return LocalStorage.Activation[LocalStorage.Field.Version];
            }
            set { }
        }

        [XmlElement(ElementName = "SaveFileVersion", Form = XmlSchemaForm.Unqualified)]
        public String SaveFileVersion
        {
            get
            {
                if (CIAT.SaveFile == null)
                    return "No file open";
                else
                    return CIAT.SaveFile.Version.ToString();
            } set { }
        }

        [XmlElement(ElementName = "TimeOpened", Form = XmlSchemaForm.Unqualified)]
        public String TimeOpened
        {
            get
            {
                return (CIAT.SaveFile == null) ? "No file open" : CIAT.SaveFile.MetaData.TimeOpened.ToShortDateString() + " " + CIAT.SaveFile.MetaData.TimeOpened.ToShortTimeString();
            }
            set { }
        }

        [XmlElement(ElementName = "ErrorCount", Form = XmlSchemaForm.Unqualified)]
        public int ErrorCount
        {
            get
            {
                return IATConfigMainForm.Errors;
            }
            set { }
        }

        [XmlElement(ElementName = "ErrorsReported", Form = XmlSchemaForm.Unqualified)]
        public int ErrorsReported
        {
            get
            {
                return IATConfigMainForm.ErrorsReported;
            }
            set { }
        }

//        [XmlArray(ElementName = "HistoryEntries", Form = XmlSchemaForm.Unqualified)]
//        [XmlArrayItem(ElementName = "HistoryEntry", Form = XmlSchemaForm.Unqualified, Type = typeof(SaveFile.SaveFileMetaData.HistoryEntry))]
        [XmlElement(ElementName = "HistoryEntry", IsNullable = true, Type = typeof(SaveFile.SaveFileMetaData.HistoryEntry))]
        public List<SaveFile.SaveFileMetaData.HistoryEntry> HistoryEntries
        {
            get
            {
                return (CIAT.SaveFile == null) ? null : CIAT.SaveFile.History;
            }
            set { }
        }

        [XmlElement(ElementName = "Exception", Form = XmlSchemaForm.Unqualified, Type = typeof(CReportableException))]
        public CReportableException ReportableException { get; set; }

        public CClientException() { }

        public CClientException(String message, CReportableException ex)
        {
            Message = message;
            ReportableException = ex;
        }

        [XmlElement(ElementName = "EventLog", Form = XmlSchemaForm.Unqualified, Type = typeof(ActivityLog))]
        public ActivityLog ActivityLog { get { return CIAT.ActivityLog; } set { } }


        public byte[] GetXmlBytes()
        {
            XmlSerializer ser = new XmlSerializer(typeof(CClientException));
            TextWriter writer = new StringWriter();
            XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            ser.Serialize(writer, this, ns);
            return Encoding.Unicode.GetBytes(writer.ToString());
        }
    }
}
