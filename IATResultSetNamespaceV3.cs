using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IATClient
{
    namespace IATResultSetNamespaceV3
    {
        public class SurveyItemResponse : ISurveyItemResponse
        {
            public sealed class RawAnswerState
            {
                private readonly String name;
                private readonly int value;

                private static readonly Dictionary<String, RawAnswerState> instance;
                static readonly public RawAnswerState Unanswered;
                static readonly public RawAnswerState ForceSubmittedUnanswered; 
                
                static RawAnswerState() {
                    instance = new Dictionary<String, RawAnswerState>();
                    Unanswered = new RawAnswerState(1, "__Unanswered__");
                    ForceSubmittedUnanswered = new RawAnswerState(2, "__ForceSubmittedUnanswered__");
                }

                public RawAnswerState(int value, String name)
                {
                    this.value = value;
                    this.name = name;
                    instance[this.name] = this;
                }

                public override String ToString()
                {
                    return this.name;
                }


                public static explicit operator RawAnswerState(String str)
                {
                    RawAnswerState result;
                    if (instance.TryGetValue(str, out result))
                        return result;
                    else
                        throw new InvalidCastException();
                }
            }


            private IATResultSetNamespaceV1.AnswerState AnswerState = IATResultSetNamespaceV1.AnswerState.Unanswered;
            private String Answer;

            public SurveyItemResponse() 
            {
                Answer = "N/A";
            }

            public SurveyItemResponse(String resp)
            {
                Answer = resp;
            }

            public void ReadXml(XmlReader reader)
            {
                Answer = reader.ReadElementString();
                try
                {
                    RawAnswerState rawState = (RawAnswerState)Answer;
                    if (rawState == RawAnswerState.Unanswered)
                        AnswerState = IATResultSetNamespaceV1.AnswerState.Unanswered;
                    else if (rawState == RawAnswerState.ForceSubmittedUnanswered)
                        AnswerState = IATResultSetNamespaceV1.AnswerState.ForceSubmitted;
                }
                catch (InvalidCastException ex)
                {
                    AnswerState = IATResultSetNamespaceV1.AnswerState.Answered;
                }
            }

            public void WriteXml(XmlWriter writer)
            {
                if (AnswerState == IATResultSetNamespaceV1.AnswerState.Answered)
                    writer.WriteElementString("SurveyResult", Answer);
                else if (AnswerState == IATResultSetNamespaceV1.AnswerState.ForceSubmitted)
                    writer.WriteElementString("SurveyResult", RawAnswerState.ForceSubmittedUnanswered.ToString());
                else if (AnswerState == IATResultSetNamespaceV1.AnswerState.Unanswered)
                    writer.WriteElementString("SurveyResult", RawAnswerState.Unanswered.ToString());
            }

            public String Value
            {
                get
                {
                    if (AnswerState == IATResultSetNamespaceV1.AnswerState.Unanswered)
                        return IATResultSetNamespaceV1.AnswerState.Unanswered.ToString();
                    if (AnswerState == IATResultSetNamespaceV1.AnswerState.ForceSubmitted)
                        return IATResultSetNamespaceV1.AnswerState.ForceSubmitted.ToString();
                    return Answer;
                }
            }

            public bool IsAnswered
            {
                get
                {
                    return AnswerState == IATResultSetNamespaceV1.AnswerState.Answered;
                }
            }

            public bool IsBlank
            {
                get
                {
                    return AnswerState == IATResultSetNamespaceV1.AnswerState.Unanswered;
                }
            }

            public bool WasForceSubmitted
            {
                get
                {
                    return AnswerState == IATResultSetNamespaceV1.AnswerState.ForceSubmitted;
                }
            }
        }

        public class IATResultSet : IATResultSetNamespaceV1.IATResultSet, INamedXmlSerializable, IResultSet
        {
            public String _Token = String.Empty;
            public override String Token
            {
                get 
                {
                    return _Token;
                }
            }

            public override bool HasToken
            {
                get
                {
                    return (_Token != String.Empty);
                }
            }

            public IATResultSet(ResultSetDescriptor rsd, String token) : base(rsd)
            {
                _Token = token;
            }
        }
    }
}
