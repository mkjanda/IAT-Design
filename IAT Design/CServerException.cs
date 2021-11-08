using System;
using System.Xml.Serialization;
using System.Xml;

namespace IATClient
{
    [Serializable]
    public class CServerException : CReportableException, INamedXmlSerializable 
    {
        public CServerException()
        {
        }

        public void ReadXml(XmlReader xReader)
        {
            xReader.ReadStartElement();
            ExceptionMessage = xReader.ReadElementString("ExceptionMessage");
            while (xReader.Name == "StackTraceElement")
                StackTrace.Add(xReader.ReadElementString("StackTraceElement"));
            while (xReader.Name == "InnerException")
            {
                xReader.ReadStartElement();
                CInnerException innerEx = new CInnerException();
                if (xReader.Name == "ExceptionMessage")
                    innerEx.ExceptionMessage = xReader.ReadElementString("ExceptionMessage");
                while (xReader.Name == "StackTraceElement")
                    innerEx.StackTrace.Add(xReader.ReadElementString("StackTraceElement"));
                xReader.ReadEndElement();
                InnerExceptions.Add(innerEx);
            }
            Caption = xReader.ReadElementString("ServerMessage");
            xReader.ReadEndElement();
        }

        public void WriteXml(XmlWriter xWriter)
        {
            throw new NotImplementedException();
        }

        public String GetName()
        {
            return "ServerException";
        }
    }
}
