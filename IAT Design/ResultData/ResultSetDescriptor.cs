using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using System.Xml.Linq;
using IATClient.ResultData;
namespace IATClient.ResultData
{
    public partial class ResultSetDescriptor : IResultElemFactory, INamedXmlSerializable
    {
        public String TestAuthor { get; private set; }
        public IATConfig.ConfigFile ConfigFile { get; private set; }
        public List<Survey> BeforeSurveys { get; private set; } = new List<Survey>();
        public List<Survey> AfterSurveys { get; private set; } = new List<Survey>();
        public int NumResults { get; private set; }
        public PartiallyEncryptedRSAData RSADataKey { get; private set; } = new PartiallyEncryptedRSAData(PartiallyEncryptedRSAData.EKeyType.Data);
        public ETokenType TokenType { get; protected set; }
        public String TokenName { get; private set; }

        public void ReadXml(XmlReader reader)
        {
            ResultDataVersion = Convert.ToInt32(reader["DataVersion"]);
            reader.ReadStartElement();
            TestAuthor = reader.ReadElementString("TestAuthor");
            MemoryStream memStream = new MemoryStream(Convert.FromBase64String(reader.ReadElementString()));
            StreamReader sReader = new StreamReader(memStream, true);
            XmlReader innerReader = new XmlTextReader(sReader);
            innerReader.MoveToContent();
            ConfigFile = IATConfig.ConfigFile.LoadFromXml(innerReader);
            XmlRootAttribute rootAttr = new XmlRootAttribute("Survey");
            XmlSerializer ser = new XmlSerializer(typeof(Survey), rootAttr);
            while (reader.Name == "BeforeSurvey")
            {
                memStream = new MemoryStream(Convert.FromBase64String(reader.ReadElementString()));
                Survey survey = (Survey)ser.Deserialize(memStream);
                BeforeSurveys.Add(survey);
            }
            while (reader.Name == "AfterSurvey")
            {
                memStream = new MemoryStream(Convert.FromBase64String(reader.ReadElementString()));
                Survey survey = (Survey)ser.Deserialize(memStream);
                AfterSurveys.Add(survey);
            }
            NumResults = Convert.ToInt32(reader.ReadElementString());
            RSADataKey.ReadXml(reader);
            TokenType = (ETokenType)Enum.Parse(typeof(ETokenType), reader.ReadElementString());
            if (reader.NodeType == XmlNodeType.Element)
            {
                if (reader.Name == "TokenName")
                    TokenName = reader.ReadElementString();
            }
            reader.ReadEndElement();
        }

        public void Load(XElement root)
        {
            ResultDataVersion = Convert.ToInt32(root.Attribute("DataVersion").Value);
            TestAuthor = root.Element("TestAuthor").Value;
            MemoryStream memStream = new MemoryStream(Convert.FromBase64String(root.Element("ConfigFile").Value));
            XmlReader xReader = new XmlTextReader(new StreamReader(memStream, true));
            ConfigFile = IATConfig.ConfigFile.LoadFromXml(xReader);
            memStream.Dispose();
            XmlRootAttribute surveyRoot = new XmlRootAttribute("Survey");
            XmlSerializer ser = new XmlSerializer(typeof(Survey), surveyRoot);
            foreach (XElement surveyElem in root.Elements("BeforeSurvey"))
            {
                memStream = new MemoryStream(Convert.FromBase64String(surveyElem.Value));
                BeforeSurveys.Add((Survey)ser.Deserialize(memStream));
                memStream.Dispose();
            }
            foreach (XElement surveyElem in root.Elements("AfterSurvey"))
            {
                memStream = new MemoryStream(Convert.FromBase64String(surveyElem.Value));
                AfterSurveys.Add((Survey)ser.Deserialize(memStream));
                memStream.Dispose();
            }
            NumResults = Convert.ToInt32(root.Element("NumResults").Value);
            RSADataKey.Load(root.Element("RSAKey"));
            Enum.TryParse<ETokenType>(root.Element("TokenType").Value, out ETokenType tokenType);
            TokenType = tokenType;
            if (TokenType != ETokenType.NONE)
                TokenName = root.Element("TokenName").Value;
        }


        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }

        public String GetName()
        {
            return "ResultSetDescriptor";
        }


        public int ResultDataVersion { get; private set; }
        private CNorms Norms { get; set; }

        private int _ResultDataVersion;

        public ResultSetDescriptor()
        {
        }

        public bool VerifyResultSet(IResultSet rs)
        {
            int ctr = 0;
            foreach (IResultSetElem rse in rs)
            {
                if (ctr < BeforeSurveys.Count)
                {
                    if (rse.NumDataElements != BeforeSurveys[ctr].NumQuestions)
                        return false;
                }
                else if (ctr > BeforeSurveys.Count)
                {
                    if (rse.NumDataElements != AfterSurveys[ctr - BeforeSurveys.Count - 1].NumQuestions)
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
            switch (ResultDataVersion)
            {
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
                    return null; ;

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
    }
}
