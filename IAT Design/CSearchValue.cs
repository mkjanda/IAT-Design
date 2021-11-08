using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using IATClient.IATResultSetNamespaceV2;

namespace IATClient
{
    class CSearchValue : INamedXmlSerializable
    {
        public enum EOrigin { survey, timestamp, iatScore, iatPercentile };
        private EOrigin Origin;
        private String CriterionName;
        int SurveyNum, ItemNum;

        public CSearchValue(EOrigin origin)
        {
            Origin = origin;
            switch (Origin)
            {
                case EOrigin.timestamp:
                    CriterionName = "Test Timestamp";
                    break;

                case EOrigin.iatPercentile:
                    CriterionName = "IAT Percentile Score";
                    break;

                case EOrigin.iatScore:
                    CriterionName = "IAT Score";
                    break;

                default:
                    throw new InvalidOperationException();
            }
        }

        public CSearchValue(CSearchCriteria searchCriteria)
        {
            Origin = EOrigin.survey;
            CriterionName = searchCriteria.CriterionName;
            SurveyNum = searchCriteria.SurveyNum;
            ItemNum = searchCriteria.ItemNum;
        }

        public String ExtractValue(IResultSet resultSet)
        {
            switch (Origin)
            {
                case EOrigin.iatScore:
                    return resultSet.IATScore.ToString();

                case EOrigin.iatPercentile:
                    return resultSet.IATScore.ToString();

                case EOrigin.timestamp:
                    return resultSet.IATScore.ToString();

                case EOrigin.survey:
                    int ndx = 0;
                    if (SurveyNum >= resultSet.BeforeSurveys.Length)
                    {
                        ndx = SurveyNum - resultSet.BeforeSurveys.Length;
                        return resultSet.AfterSurveys[ndx][ItemNum].Value;
                    }
                    else
                        return resultSet.BeforeSurveys[ndx][ItemNum].Value;
            }
            return String.Empty;
        }

        public String GetName()
        {
            return "SearchValueExtractor";
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(GetName());
            writer.WriteAttributeString("Origin", Origin.ToString());
            writer.WriteElementString("CriterionName", CriterionName);
            if (Origin == EOrigin.survey)
            {
                writer.WriteElementString("SurveyNum", SurveyNum.ToString());
                writer.WriteElementString("ItemNum", ItemNum.ToString());
            }
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            Origin = (EOrigin)Enum.Parse(typeof(EOrigin), reader["Origin"]);
            reader.ReadStartElement();
            CriterionName = reader.ReadElementString();
            if (Origin == EOrigin.survey)
            {
                SurveyNum = Convert.ToInt32(reader.ReadElementString());
                ItemNum = Convert.ToInt32(reader.ReadElementString());
            }
            reader.ReadEndElement();
        }
    }
}
