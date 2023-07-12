using System;
using System.Linq;
using System.Xml.Linq;
using IATClient.ResultData.IATResultSetNamespaceV1;

namespace IATClient.ResultData.IATResultSetNamespaceV4
{
    public class SurveyResponseSet : IATResultSetNamespaceV1.SurveyResponseSet, ISurveyResponse
    {
        public SurveyResponseSet(IResultElemFactory factory) : base(factory) { }

        public override void Load(XDocument doc)
        {
            NumSurveyResults = Convert.ToInt32(doc.Root.Attribute("NumSurveyResults").Value);
            if (NumSurveyResults == 0)
                SurveyResults = Factory.CreateSurveyItemResponseArray(1);
            else
            {
                SurveyResults = Factory.CreateSurveyItemResponseArray(NumSurveyResults);
                int ctr = 0;
                foreach (var resp in doc.Root.Elements("SurveyResults"))
                {
                    SurveyResults[ctr++].Value = resp.Value;
                }
            }
        }
    }
}
