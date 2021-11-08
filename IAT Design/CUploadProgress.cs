using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IATClient
{
    class CUploadProgress : INamedXmlSerializable
    {
        private long _TotalBytesUploaded = 0, _NumBytesExpected = 0;
        int _NumBytesUploaded = 0;

        public long TotalBytesUploaded {
            get
            {
                return _TotalBytesUploaded;
            }
        }

        public long NumBytesExpected
        {
            get
            {
                return _NumBytesExpected;
            }
        }

        public int NumBytesUploaded
        {
            get
            {
                return _NumBytesUploaded;
            }
        }

        public CUploadProgress() { }

        public String GetName()
        {
            return "UploadProgress";
        }

        public void ReadXml(XmlReader xReader)
        {
            xReader.ReadStartElement(GetName());
            _NumBytesUploaded = Convert.ToInt32(xReader.ReadElementString("NumBytesUploaded"));
            _TotalBytesUploaded = Convert.ToInt64(xReader.ReadElementString("TotalBytesUploaded"));
            _NumBytesExpected = Convert.ToInt64(xReader.ReadElementString("NumBytesExpected"));
            xReader.ReadEndElement();
        }

        public void WriteXml(XmlWriter xWriter)
        {
            throw new NotImplementedException();
        }
    }
}
