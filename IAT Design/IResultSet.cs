using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.IO;

namespace IATClient
{
    public interface ISurveyItemResponse
    {
        String Value { get; }
        bool IsAnswered { get; }
        bool IsBlank { get; }
        bool WasForceSubmitted { get; }
        void ReadXml(XmlReader reader);
        void WriteXml(XmlWriter writer);
    }

    public interface IResultElemFactory
    {
        IResultSet CreateResultSet();
        IResultSet CreateResultSet(String token);
        IResultData CreateResultData();
        IIATItemResponse CreateIATItemResponse();
        ISurveyItemResponse[] CreateSurveyItemResponseArray(int nResponses);
        ISurveyResponse[] CreateSurveyResponseArray(int nElems);
        IIATResponse CreateIATResponse();
        bool VerifyResultSet(IResultSet rs);
        int ResultDataVersion { get; }
    }

    public interface IResultData
    {
        int NumResultSets { get; }
        void AppendResultSet(IResultSet rs);
        IResultElemFactory GetFactory();
        double Mean { get; }
        double SD { get; }
        IResultSet this[int ndx] { get; }
        void ReadXml(XmlReader reader);
    }

    public interface IResultSet : IEnumerable<IResultSetElem>
    {
        String Token { get; }
        bool HasToken { get; }
        double Percentile { get; }
        long ResultID { get; set; }
        ISurveyResponse[] BeforeSurveys { get; }
        ISurveyResponse[] AfterSurveys { get; }
        IIATResponse IATResponse { get; }
        void Score();
        double IATScore { get; }
        DateTime Timestamp { get; }
        IResultSetElem this[int ndx] { get; }
        void SetPercentileScore(double percentile);
        void WriteXml(XmlWriter writer);
        void ReadXml(XmlReader reader);
    }

    public interface IResultSetElem
    {
        int NumDataElements { get; }
        int NumItems { get; }
        void ReadXml(XmlReader reader);
    }

    public interface IIATItemResponse
    {
        int PresentationNumber { get; set; }
        int BlockNumber { get; set; }
        int ItemNumber { get; set; }
        bool Error { get; set; }
        long ResponseTime { get; set; }
        void ReadXml(XmlReader xReader);
        void WriteXml(XmlWriter xWriter);
    }

    public interface IIATResponse : IResultSetElem, IEnumerable<IIATItemResponse>
    {
        IIATItemResponse this[int ndx] { get; }
        void WriteXml(XmlWriter writer);
    }

    public interface ISurveyResponse : IResultSetElem
    {
        ISurveyItemResponse this[int ndx] { get; }
        void WriteXml(XmlWriter writer);
    }

    public class ItemUnansweredException : Exception
    {
        public ItemUnansweredException() { }
    }

    public class ItemForceSubmittedException : Exception
    {
        public ItemForceSubmittedException() { }
    }
}
