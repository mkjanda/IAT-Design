using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    class ResultPackage : IXmlSerializable
    {
        // progress window delegates
        public static DataPasswordForm.ProgressIncrementHandler ProgressIncrement = null;
        public static DataPasswordForm.SetProgressRangeHandler SetProgressRange = null;
        public static DataPasswordForm.SetStatusMessageHandler SetStatusMessage = null;
        public static DataPasswordForm ProgressWin = null;

        private List<Survey> _BeforeSurveys;
        private List<Survey> _AfterSurveys;
        private IATConfig.ConfigFile _ConfigFile;
        private IResultData _Results;

        public List<Survey> BeforeSurveys
        {
            get
            {
                return _BeforeSurveys;
            }
        }

        public List<Survey> AfterSurveys
        {
            get
            {
                return _AfterSurveys;
            }
        }

        public IATConfig.ConfigFile ConfigFile
        {
            get
            {
                return _ConfigFile;
            }
        }

        public IResultData Results
        {
            get
            {
                return _Results;
            }
        }

        public ResultPackage(ResultSetDescriptor rsd)
        {
            _BeforeSurveys = new List<Survey>();
            _AfterSurveys = new List<Survey>();
            _ConfigFile = IATConfig.ConfigFile.GetConfigFile();
            _Results = rsd.CreateResultData();
        }

        public void ReadXml(XmlReader reader)
        {
            reader.ReadStartElement();
            if (ProgressWin != null)
                ProgressWin.Invoke(SetStatusMessage, "Receiving IAT Configuration");
            int nBeforeSurveys = Convert.ToInt32(reader["NumBeforeSurveys"]);
            int nAfterSurveys = Convert.ToInt32(reader["NumAfterSurveys"]);
            ConfigFile.ReadXml(reader);
            BeforeSurveys.Clear();
            for (int ctr = 0; ctr < nBeforeSurveys; ctr++)
            {
                String str = reader.ReadElementString("BeforeSurvey");
                MemoryStream memStream = new MemoryStream(Convert.FromBase64String(str));
                memStream.Seek(0, SeekOrigin.Begin);
                XmlSerializer ser = new XmlSerializer(typeof(Survey));
                Survey s = (Survey)ser.Deserialize(memStream);
                BeforeSurveys.Add(s);
            }
            AfterSurveys.Clear();
            for (int ctr = 0; ctr < nAfterSurveys; ctr++)
            {
                String str = reader.ReadElementString("AfterSurvey");
                MemoryStream memStream = new MemoryStream(Convert.FromBase64String(str));
                memStream.Seek(0, SeekOrigin.Begin);
                XmlSerializer ser = new XmlSerializer(typeof(Survey));
                Survey s = (Survey)ser.Deserialize(memStream);
                AfterSurveys.Add(s);
            }
            if (ProgressWin != null)
                ProgressWin.Invoke(SetStatusMessage, "Receiving Result Sets");
            Results.ReadXml(reader);
            reader.ReadEndElement();
            ProgressIncrement = null;
            SetProgressRange = null;
            SetStatusMessage = null;
            ProgressWin = null;
        }

        public void WriteXml(XmlWriter writer)
        {
            /*
            writer.WriteStartElement("ResultPackage");
            writer.WriteAttributeString("NumBeforeSurveys", BeforeSurveys.Count.ToString());
            writer.WriteAttributeString("NumAfterSurveys", AfterSurveys.Count.ToString());
            ConfigFile.WriteXml(writer);
            XmlSerializer ser = new XmlSerializer(typeof(Survey));
            ser.Serialize(
            for (int ctr = 0; ctr < BeforeSurveys.Count; ctr++)
                BeforeSurveys[ctr].WriteXml(writer);
            for (int ctr = 0; ctr < AfterSurveys.Count; ctr++)
                AfterSurveys[ctr].WriteXml(writer);
            Results.WriteXml(writer);
            writer.WriteEndElement();
             * */
            throw new NotImplementedException();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
