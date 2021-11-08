using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IATClient
{
    class CUploadRequest : INamedXmlSerializable
    {
        private long _DeploymentID;
        private String _DataUploadKey;
        private String _ItemSlideUploadKey;
        private String _ReconnectionKey;

        public long DeploymentID
        {
            get
            {
                return _DeploymentID;
            }
        }

        public String DataUploadKey
        {
            get
            {
                return _DataUploadKey;
            }
        }

        public String ItemSlideUploadKey
        {
            get
            {
                return _ItemSlideUploadKey;
            }
        }

        public String ReconnectionKey
        {
            get
            {
                return _ReconnectionKey;
            }
        }

        public CUploadRequest() { }

        public String GetName()
        {
            return "CUploadRequest";
        }

        public void WriteXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement("UploadRequest");
            xWriter.WriteElementString("DeploymentID", DeploymentID.ToString());
            xWriter.WriteElementString("DataUploadKey", DataUploadKey);
            xWriter.WriteElementString("ItemSlideUploadKey", ItemSlideUploadKey);
            xWriter.WriteElementString("ReconnectionKey", ReconnectionKey);
            xWriter.WriteEndElement();
        }

        public void ReadXml(XmlReader xReader)
        {
            xReader.ReadStartElement("UploadRequest");
            _DeploymentID = Convert.ToInt64(xReader.ReadElementString("DeploymentID"));
            _DataUploadKey = xReader.ReadElementString("DataUploadKey");
            _ItemSlideUploadKey = xReader.ReadElementString("ItemSlideUploadKey");
            _ReconnectionKey = xReader.ReadElementString("ReconnectionKey");
            xReader.ReadEndElement();
        }

    }
}
