using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.IO;

namespace IATClient
{
    public class ResultSetDescriptor : IResultElemFactory
    {
        private IATConfig.ConfigFile _ConfigFile;
        private List<IATSurveyFile.Survey> _BeforeSurveys = new List<IATSurveyFile.Survey>();
        private List<IATSurveyFile.Survey> _AfterSurveys = new List<IATSurveyFile.Survey>();
        private PartiallyEncryptedRSAData RSADataKey = new PartiallyEncryptedRSAData(PartiallyEncryptedRSAData.EKeyType.Data);
        private CNorms _Norms = null;
        private int _NumResults;
        private String DataPassword = String.Empty;
        private String _TestAuthor = String.Empty;
        private ETokenType _TokenType;
        private String _TokenName = String.Empty;

        public ResultSetDescriptor()
        {
        }

        public bool VerifyResultSet(IResultSet rs)
        {
            int ctr = 0;
            foreach (IResultSetElem rse in rs)
            {
                if (ctr < _BeforeSurveys.Count) {
                    if (rse.NumDataElements != _BeforeSurveys[ctr].NumQuestions)
                        return false;
                }
                else if (ctr > _BeforeSurveys.Count)
                {
                    if (rse.NumDataElements != _AfterSurveys[ctr - _BeforeSurveys.Count - 1].NumQuestions)
                        return false;
                }
                ctr++;
            }
            return true;
        }

        public ISurveyItemResponse[] CreateSurveyItemResponseArray(int nElems)
        {
            ISurveyItemResponse[] respArray = null;
            switch (ResultDataVersion)
            {
                case 1:
                    respArray = new IATResultSetNamespaceV1.SurveyItemResponse[nElems];
                    for (int ctr = 0; ctr < nElems; ctr++)
                        respArray[ctr] = new IATResultSetNamespaceV1.SurveyItemResponse();
                    break;

                case 2:
                    respArray = new IATResultSetNamespaceV1.SurveyItemResponse[nElems];
                    for (int ctr = 0; ctr < nElems; ctr++)
                        respArray[ctr] = new IATResultSetNamespaceV1.SurveyItemResponse();
                    break;

                case 3:
                    respArray = new IATResultSetNamespaceV3.SurveyItemResponse[nElems];
                    for (int ctr = 0; ctr < nElems; ctr++)
                        respArray[ctr] = new IATResultSetNamespaceV3.SurveyItemResponse();
                    break;

            }
            return respArray;
        }

        public ISurveyResponse[] CreateSurveyResponseArray(int nElems)
        {
            ISurveyResponse[] respAry = null;
            switch (ResultDataVersion)
            {
                case 1:
                    respAry = new IATResultSetNamespaceV1.SurveyResponseSet[nElems];
                    for (int ctr = 0; ctr < nElems; ctr++)
                        respAry[ctr] = new IATResultSetNamespaceV1.SurveyResponseSet(this);
                    break;

                case 2:
                    respAry = new IATResultSetNamespaceV1.SurveyResponseSet[nElems];
                    for (int ctr = 0; ctr < nElems; ctr++)
                        respAry[ctr] = new IATResultSetNamespaceV1.SurveyResponseSet(this);
                    break;

                case 3:
                    respAry = new IATResultSetNamespaceV1.SurveyResponseSet[nElems];
                    for (int ctr = 0; ctr < nElems; ctr++)
                        respAry[ctr] = new IATResultSetNamespaceV1.SurveyResponseSet(this);
                    break;
            }
            return respAry;
        }

        public IIATResponse CreateIATResponse()
        {
            switch (ResultDataVersion)
            {
                case 1:
                    return new IATResultSetNamespaceV1.IATResultSetElementList(this);
                case 2:
                    return new IATResultSetNamespaceV1.IATResultSetElementList(this);
                case 3:
                    return new IATResultSetNamespaceV1.IATResultSetElementList(this);
                default:
                    return null;
            }
        }

        public IIATItemResponse CreateIATItemResponse()
        {
            switch (ResultDataVersion)
            {
                case 1:
                    return new IATResultSetNamespaceV1.IATItemResponse();

                case 2:
                    return new IATResultSetNamespaceV2.IATItemResponse();

                case 3:
                    return new IATResultSetNamespaceV2.IATItemResponse();
                default:
                    return null;
            }
        }


        public IResultSet CreateResultSet()
        {
            switch (ResultDataVersion)
            {
                case 1:
                    return new IATResultSetNamespaceV1.IATResultSet(this);

                case 2:
                    return new IATResultSetNamespaceV1.IATResultSet(this);

                case 3:
                    return new IATResultSetNamespaceV3.IATResultSet(this, String.Empty);

                default:
                    return null;
            }
        }

        public IResultSet CreateResultSet(String token)
        {
            if (ResultDataVersion < 3)
                throw new NotImplementedException();
            switch (ResultDataVersion) {
                case 3:
                    return new IATResultSetNamespaceV3.IATResultSet(this, CreateToken(token));

                default:
                    return null;
            }
        }

        public IResultData CreateResultData()
        {
            switch (ResultDataVersion)
            {
                case 1:
                    return new IATResultSetNamespaceV1.IATResultSetList(this);

                case 2:
                    return new IATResultSetNamespaceV1.IATResultSetList(this);

                case 3:
                    return new IATResultSetNamespaceV1.IATResultSetList(this);

                default:
                    return null;
            }
        }

        private String CreateToken(String tokString)
        {
            switch (ResultDataVersion)
            {
                case 1:
                    return null;;

                case 2:
                    return null;

                case 3:
                    switch (TokenType)
                    {
                        case ETokenType.NONE:
                            return null;

                        case ETokenType.VALUE:
                            return Encoding.UTF8.GetString(Convert.FromBase64String(tokString));

                        case ETokenType.HEX:
                            String tok = "0x";
                            byte[] tokBytes = Convert.FromBase64String(tokString);
                            for (int ctr = 0; ctr < tokBytes.Length; ctr++)
                                tok += Convert.ToString(tokBytes[ctr], 16);
                            return tok;

                        case ETokenType.BASE64:
                            return tokString;

                        case ETokenType.BASE64_UTF8:
                            return Encoding.UTF8.GetString(Convert.FromBase64String(tokString));

                        default:
                            return null;
                    }

                default:
                    return null;
            }
        }


        public int NumResults
        {
            get
            {
                return _NumResults;
            }
            set
            {
                _NumResults = value;
            }
        }


        public List<IATSurveyFile.Survey> BeforeSurveys
        {
            get
            {
                return _BeforeSurveys;
            }
        }

        public List<IATSurveyFile.Survey> AfterSurveys
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

        public PartiallyEncryptedRSAData GetRSADataKey(String password)
        {
            if (!RSADataKey.IsDecrypted)
                RSADataKey.DecryptKey(password);
            return RSADataKey;
        }



        public IATSurveyFile.SurveyItem GetSurveyItem(String SurveyName, int ItemNum)
        {
            foreach (IATSurveyFile.Survey s in BeforeSurveys)
                if (s.Name == SurveyName)
                    return s.SurveyItems[ItemNum];
            foreach (IATSurveyFile.Survey s in AfterSurveys)
                if (s.Name == SurveyName)
                    return s.SurveyItems[ItemNum];
            return null;
        }

        public void Load(XElement root)
        {
            _ResultDataVersion = Convert.ToInt32(root.Attribute("DataVersion").Value);
            _TestAuthor = root.Element("TestAuthor").Value;
            MemoryStream memStream = new MemoryStream(Convert.FromBase64String(root.Element("ConfigFile").Value));
            XmlReader xReader = new XmlTextReader(new StreamReader(memStream, true));
            _ConfigFile = IATConfig.ConfigFile.LoadFromXml(xReader);
            memStream.Dispose();
            XmlRootAttribute surveyRoot = new XmlRootAttribute("Survey");
            XmlSerializer ser = new XmlSerializer(typeof(IATSurveyFile.Survey), surveyRoot);
            foreach (XElement surveyElem in root.Elements("BeforeSurvey"))
            {
                memStream = new MemoryStream(Convert.FromBase64String(surveyElem.Value));
                BeforeSurveys.Add((IATSurveyFile.Survey)ser.Deserialize(memStream));
                memStream.Dispose();
            }
            foreach (XElement surveyElem in root.Elements("AfterSurvey"))
            {
                memStream = new MemoryStream(Convert.FromBase64String(surveyElem.Value));
                AfterSurveys.Add((IATSurveyFile.Survey)ser.Deserialize(memStream));
                memStream.Dispose();
            }
            _NumResults = Convert.ToInt32(root.Element("NumResults").Value);
            RSADataKey.Load(root.Element("RSAKey"));
            Enum.TryParse<ETokenType>(root.Element("TokenType").Value, out ETokenType tokenType);
            _TokenType = tokenType;
            if (_TokenType != ETokenType.NONE)
                _TokenName = root.Element("TokenName").Value;
        }


        public CNorms Norms
        {
            get
            {
                return _Norms;
            }
        }
    }
}
