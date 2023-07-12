using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    class ResultPackage 
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

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
