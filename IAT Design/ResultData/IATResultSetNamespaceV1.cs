using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;

namespace IATClient.ResultData
{
    namespace IATResultSetNamespaceV1
    {
        public sealed class AnswerState
        {
            private readonly int val;
            private readonly String name;
            public static readonly AnswerState Answered, Unanswered, ForceSubmitted;

            static AnswerState()
            {
                Answered = new AnswerState(1, "__Answered__");
                Unanswered = new AnswerState(2, "Unanswered");
                ForceSubmitted = new AnswerState(3, "NULL");
            }

            public AnswerState(int val, String name)
            {
                this.val = val;
                this.name = name;
            }

            public override string ToString()
            {
                return this.name;
            }
        }

        public class SurveyItemResponse : ISurveyItemResponse
        {
            private AnswerState AnswerState = AnswerState.Unanswered;
            private String Answer;

            public SurveyItemResponse()
            {
                Answer = "N/A";
            }

            public void ReadXml(XmlReader reader)
            {
                Answer = reader.ReadElementString();
                if (Answer == "NULL")
                    AnswerState = AnswerState.ForceSubmitted;
                else if (Answer == "Unanswered")
                    AnswerState = AnswerState.Unanswered;
                else
                    AnswerState = AnswerState.Answered;
            }

            public void WriteXml(XmlWriter writer)
            {
                if (AnswerState == AnswerState.Answered)
                    writer.WriteElementString("SurveyResult", Answer);
                else if (AnswerState == AnswerState.ForceSubmitted)
                    writer.WriteElementString("SurveyResult", "NULL");
                else if (AnswerState == AnswerState.Unanswered)
                    writer.WriteElementString("SurveyResult", "Unanswered");
            }

            public String Value
            {
                get
                {
                    if (AnswerState == AnswerState.Unanswered)
                        return AnswerState.Unanswered.ToString();
                    if (AnswerState == AnswerState.ForceSubmitted)
                        return AnswerState.ForceSubmitted.ToString();
                    return Answer;
                }
                set
                {
                    Answer = value;
                }
            }

            public bool IsAnswered
            {
                get
                {
                    return AnswerState == AnswerState.Answered;
                }
            }

            public bool IsBlank
            {
                get
                {
                    return AnswerState == AnswerState.Unanswered;
                }
            }

            public bool WasForceSubmitted
            {
                get
                {
                    return AnswerState == AnswerState.ForceSubmitted;
                }
            }
        }


        public class IATResultSetElementListEnumerator : IEnumerator<IIATItemResponse>
        {
            private int ndx = -1;
            private List<IIATItemResponse> elements = new List<IIATItemResponse>();

            public IATResultSetElementListEnumerator(List<IIATItemResponse> elements)
            {
                this.elements.AddRange(elements);
            }

            Object IEnumerator.Current
            {
                get
                {
                    return elements[ndx];
                }
            }

            public IIATItemResponse Current
            {
                get
                {
                    return elements[ndx];
                }
            }

            public void Dispose()
            {
                elements.Clear();
            }

            public bool MoveNext()
            {
                if (++ndx >= elements.Count)
                    return false;
                return true;
            }

            public void Reset()
            {
                ndx = -1;
            }
        }

        public class IATResultSetElementList : IIATResponse
        {
            protected List<IIATItemResponse> ElementList = new List<IIATItemResponse>();
            protected IResultElemFactory ElemFactory;
            protected double _SD = double.NaN;

            public IIATItemResponse this[int ndx]
            {
                get
                {
                    return ElementList[ndx];
                }
            }

            public int NumDataElements
            {
                get
                {
                    return ElementList.Count;
                }
            }

            public int NumItems
            {
                get
                {
                    return ElementList.Count;
                }
            }

            public IEnumerator<IIATItemResponse> GetEnumerator()
            {
                return new IATResultSetElementListEnumerator(ElementList);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IATResultSetElementList(IResultElemFactory elemFactory)
            {
                ElemFactory = elemFactory;
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATResultSetElementList");
                writer.WriteAttributeString("NumElements", ElementList.Count.ToString().ToString());
                for (int ctr = 0; ctr < ElementList.Count; ctr++)
                    ElementList[ctr].WriteXml(writer);
                writer.WriteEndElement();
            }

            public void ReadXml(XmlReader reader)
            {
                ElementList.Clear();
                reader.ReadStartElement("IATResultSetElementList");
                while (reader.Name == "IATResultSetElement")
                {
                    IIATItemResponse element = ElemFactory.CreateIATItemResponse();
                    element.ReadXml(reader);
                    ElementList.Add(element);
                }
                reader.ReadEndElement();
            }

            public virtual void Load(XDocument doc)
            {
                foreach (var elem in doc.Root.Elements("IATResultSetElement"))
                {
                    var responseElem = ElemFactory.CreateIATItemResponse();
                    responseElem.Load(elem);
                    ElementList.Add(responseElem);
                }
            }

            public String GetName()
            {
                return "IATResultSetElementList";
            }
        }




        public class IATItemResponse : INamedXmlSerializable, IIATItemResponse
        {
            private int _BlockNumber, _ItemNumber, _PresentationNum;
            private long _ResponseTime;

            public int PresentationNumber
            {
                get
                {
                    return _PresentationNum;
                }
                set
                {
                    _PresentationNum = value;
                }
            }

            public int BlockNumber
            {
                get
                {
                    return _BlockNumber;
                }
                set
                {
                    _BlockNumber = value;
                }
            }

            public int ItemNumber
            {
                get
                {
                    return _ItemNumber;
                }
                set
                {
                    _ItemNumber = value;
                }
            }

            public long ResponseTime
            {
                get
                {
                    return _ResponseTime;
                }
                set
                {
                    _ResponseTime = value;
                }
            }

            public virtual bool Error
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }


            public IATItemResponse()
            {
                _ItemNumber = -1;
                _ResponseTime = -1;
                _BlockNumber = -1;
            }

            public virtual void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATResultSetElement");
                writer.WriteElementString("BlockNum", BlockNumber.ToString());
                writer.WriteElementString("ItemNum", ItemNumber.ToString());
                writer.WriteElementString("ResponseTime", ResponseTime.ToString());
                writer.WriteElementString("PresentationNum", PresentationNumber.ToString());
                writer.WriteEndElement();
            }

            public virtual void Load(XElement elem)
            {
                throw new NotImplementedException();
            }

            public virtual void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                BlockNumber = reader.ReadElementContentAsInt();
                ItemNumber = reader.ReadElementContentAsInt();
                ResponseTime = reader.ReadElementContentAsLong();
                PresentationNumber = reader.ReadElementContentAsInt();
                reader.ReadEndElement();
            }

            public String GetName()
            {
                return "IATResultSetElement";
            }
        }

        public class SurveyResponseSet : ISurveyResponse
        {
            protected ISurveyItemResponse[] SurveyResults = null;
            protected IResultElemFactory Factory;

            private int _NumSurveyResults = -1;

            public int NumSurveyResults
            {
                get
                {
                    return _NumSurveyResults;
                }
                set
                {
                    _NumSurveyResults = value;
                }
            }

            public int NumItems
            {
                get
                {
                    return NumDataElements;
                }
            }

            public ISurveyItemResponse this[int ndx]
            {
                get
                {
                    return SurveyResults[ndx];
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IEnumerator<ISurveyItemResponse> GetEnumerator()
            {
                return SurveyResults.ToList().GetEnumerator();
            }

            public SurveyResponseSet(IResultElemFactory factory)
            {
                Factory = factory;
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("SurveyResults");
                writer.WriteAttributeString("NumSurveyResults", NumSurveyResults.ToString());
                for (int ctr = 0; ctr < SurveyResults.Length; ctr++)
                    SurveyResults[ctr].WriteXml(writer);
                writer.WriteEndElement();
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                NumSurveyResults = Convert.ToInt32(reader["NumSurveyResults"]);
                reader.ReadStartElement();
                if (NumSurveyResults == 0)
                {
                    SurveyResults = Factory.CreateSurveyItemResponseArray(1);
                }
                else
                {
                    SurveyResults = Factory.CreateSurveyItemResponseArray(NumSurveyResults);
                    for (int ctr = 0; ctr < NumSurveyResults; ctr++)
                        SurveyResults[ctr].ReadXml(reader);
                    reader.ReadEndElement();
                }
            }

            public virtual void Load(XDocument doc)
            {
                throw new NotImplementedException();
            }

            public String GetName()
            {
                return "SurveyResponseSet";
            }

            public int NumDataElements
            {
                get
                {
                    if (SurveyResults == null)
                        return 0;
                    if (NumSurveyResults == 0)
                        return 0;
                    return SurveyResults.Length;
                }
            }
        }



        public class IATResultSetEnumerator : IEnumerator<IResultSetElem>
        {
            private int ndx = -1;
            public List<IResultSetElem> Elements = new List<IResultSetElem>();

            public IATResultSetEnumerator(ISurveyResponse[] beforeSurveys, IIATResponse iatResponse, ISurveyResponse[] afterSurveys)
            {
                Elements.AddRange(beforeSurveys);
                Elements.Add(iatResponse);
                Elements.AddRange(afterSurveys);
            }

            Object IEnumerator.Current
            {
                get
                {
                    return Elements[ndx];
                }
            }

            public IResultSetElem Current
            {
                get
                {
                    return Elements[ndx];
                }
            }

            public void Dispose()
            {
                Elements.Clear();
            }

            public bool MoveNext()
            {
                ndx++;
                if (ndx < Elements.Count)
                    return true;
                return false;
            }

            public void Reset()
            {
                ndx = -1;
            }
        }

        public class IATResultSet : INamedXmlSerializable, IResultSet
        {
            protected ISurveyResponse[] _BeforeSurveys, _AfterSurveys;
            protected IIATResponse _IATResults;
            protected int _NumResultSetElements = 0;
            protected long _ResultID = -1;
            protected DateTime _Timestamp;
            protected double _Percentile = double.NaN;
            protected IResultElemFactory ElemFactory;

            public IResultSetElem this[int ndx]
            {
                get
                {
                    if (ndx < _BeforeSurveys.Length)
                        return _BeforeSurveys[ndx];
                    else if (ndx == _BeforeSurveys.Length)
                        return _IATResults;
                    else if ((ndx > _BeforeSurveys.Length) && (ndx < _BeforeSurveys.Length + _AfterSurveys.Length))
                        return _AfterSurveys[ndx];
                    throw new IndexOutOfRangeException();
                }
            }

            public double Percentile
            {
                get
                {
                    return _Percentile;
                }
            }

            public long ResultID
            {
                get
                {
                    return _ResultID;
                }
                set
                {
                    _ResultID = value;
                }
            }

            public ISurveyResponse[] BeforeSurveys
            {
                get
                {
                    return _BeforeSurveys;
                }
            }

            public ISurveyResponse[] AfterSurveys
            {
                get
                {
                    return _AfterSurveys;
                }
            }

            public IIATResponse IATResponse
            {
                get
                {
                    return _IATResults;
                }
            }

            public IEnumerator<IResultSetElem> GetEnumerator()
            {
                return new IATResultSetEnumerator(BeforeSurveys, IATResponse, AfterSurveys);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new IATResultSetEnumerator(BeforeSurveys, IATResponse, AfterSurveys);
            }

            public virtual String Token
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public virtual bool HasToken
            {
                get
                {
                    return false;
                }
            }


            public int NumParts
            {
                get
                {
                    return BeforeSurveys.Length + AfterSurveys.Length + 1;
                }
            }

            public DateTime Timestamp
            {
                get
                {
                    return _Timestamp;
                }
                set
                {
                    _Timestamp = value;
                }
            }

            protected double _IATScore;

            public double IATScore
            {
                get
                {
                    if ((double.IsNaN(_IATScore)) && (_IATResults.NumDataElements != 0))
                        Score();
                    return _IATScore;
                }
            }

            public void SetPercentileScore(double percentile)
            {
                _Percentile = percentile;
            }



            public void Score()
            {
                long LatencySum = 0;
                int nLT300 = 0;
                for (int ctr = 0; ctr < _IATResults.NumDataElements; ctr++)
                {
                    LatencySum += IATResponse[ctr].ResponseTime;
                    if (IATResponse[ctr].ResponseTime < 300)
                        nLT300++;
                }
                if (nLT300 * 10 >= _IATResults.NumDataElements)
                {
                    _IATScore = double.NaN;
                    return;
                }

                List<IIATItemResponse> Block3 = new List<IIATItemResponse>();
                List<IIATItemResponse> Block4 = new List<IIATItemResponse>();
                List<IIATItemResponse> Block6 = new List<IIATItemResponse>();
                List<IIATItemResponse> Block7 = new List<IIATItemResponse>();

                for (int ctr = 0; ctr < _IATResults.NumDataElements; ctr++)
                {
                    if (IATResponse[ctr].ResponseTime > 10000)
                        continue;

                    switch (IATResponse[ctr].BlockNumber)
                    {
                        case 3:
                            Block3.Add(IATResponse[ctr]);
                            break;

                        case 4:
                            Block4.Add(IATResponse[ctr]);
                            break;

                        case 6:
                            Block6.Add(IATResponse[ctr]);
                            break;

                        case 7:
                            Block7.Add(IATResponse[ctr]);
                            break;
                    }
                }

                List<IIATItemResponse> InclusiveSDList1 = new List<IIATItemResponse>();
                List<IIATItemResponse> InclusiveSDList2 = new List<IIATItemResponse>();

                InclusiveSDList1.AddRange(Block3);
                InclusiveSDList1.AddRange(Block6);
                double mean = InclusiveSDList1.Select(r => r.ResponseTime).Average();
                double sd3_6 = Math.Sqrt(InclusiveSDList1.Select(r => (double)r.ResponseTime).Aggregate<double, double>(0, (sd, rt) => sd + Math.Pow(rt - mean, 2)) / (double)(InclusiveSDList1.Count - 1));

                InclusiveSDList2.AddRange(Block4);
                InclusiveSDList2.AddRange(Block7);
                mean = InclusiveSDList2.Select(r => r.ResponseTime).Average();
                double sd4_7 = Math.Sqrt(InclusiveSDList2.Select(r => (double)r.ResponseTime).Aggregate<double, double>(0, (sd, rt) => sd + Math.Pow(rt - mean, 2)) / (double)(InclusiveSDList2.Count - 1));

                double mean3 = Block3.Select(r => r.ResponseTime).Average();
                double mean4 = Block4.Select(r => r.ResponseTime).Average();
                double mean6 = Block6.Select(r => r.ResponseTime).Average();
                double mean7 = Block7.Select(r => r.ResponseTime).Average();

                _IATScore = (((mean6 - mean3) / sd3_6) + ((mean7 - mean4) / sd4_7)) / 2;
            }

            public List<IIATItemResponse> GetResultsForItem(int ItemNum)
            {
                List<IIATItemResponse> ResultList = new List<IIATItemResponse>();
                return IATResponse.Where(r => r.ItemNumber == ItemNum).ToList();
            }

            public IATResultSet(ResultSetDescriptor rsd)
            {
                ElemFactory = rsd;
                _IATScore = double.NaN;
                _BeforeSurveys = ElemFactory.CreateSurveyResponseArray(rsd.BeforeSurveys.Count);
                _AfterSurveys = ElemFactory.CreateSurveyResponseArray(rsd.AfterSurveys.Count);
                _IATResults = rsd.CreateIATResponse();
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATResultSet");
                writer.WriteAttributeString("NumBeforeSurveys", BeforeSurveys.Length.ToString());
                writer.WriteAttributeString("NumResultSetElements", _IATResults.NumDataElements.ToString());
                writer.WriteAttributeString("NumAfterSurveys", AfterSurveys.Length.ToString());
                for (int ctr = 0; ctr < BeforeSurveys.Length; ctr++)
                    _BeforeSurveys[ctr].WriteXml(writer);
                _IATResults.WriteXml(writer);
                for (int ctr = 0; ctr < AfterSurveys.Length; ctr++)
                    _AfterSurveys[ctr].WriteXml(writer);
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                int nBeforeSurveys = Convert.ToInt32(reader["NumBeforeSurveys"]);
                int nItems = Convert.ToInt32(reader["NumResultSetElements"]);
                int nAfterSurveys = Convert.ToInt32(reader["NumAfterSurveys"]);
                reader.ReadStartElement();
                for (int ctr = 0; ctr < nBeforeSurveys; ctr++)
                {
                    _BeforeSurveys[ctr].ReadXml(reader);
                }
                _IATResults.ReadXml(reader);
                for (int ctr = 0; ctr < nAfterSurveys; ctr++)
                {
                    _AfterSurveys[ctr].ReadXml(reader);
                }

                reader.ReadEndElement();
            }

            public String GetName()
            {
                return "IATResultSet";
            }
        }

        public class IATResultSetList : INamedXmlSerializable, IResultData, IEnumerable<IResultSet>
        {
            protected List<IResultSet> IATResultSets = new List<IResultSet>();
            private double _SD = double.NaN, _Mean = double.NaN;
            private const double b0 = 0.2316419, b1 = 0.319381530, b2 = -0.356563782, b3 = 1.781477937, b4 = -1.821255978, b5 = 1.330274429;
            private ResultSetDescriptor Descriptor;
            private IResultElemFactory Factory;

            public IEnumerator<IResultSet> GetEnumerator()
            {
                return IATResultSets.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }

            public IATConfigMainForm MainForm
            {
                get
                {
                    return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
                }
            }

            public void AppendResultSet(IResultSet rSet)
            {
                IATResultSets.Add((IATResultSet)rSet);
            }

            public IResultElemFactory GetFactory()
            {
                return Descriptor;
            }

            public double SD
            {
                get
                {
                    if (!double.IsNaN(_SD))
                        return _SD;
                    double squaredSums = 0;
                    foreach (IResultSet ir in IATResultSets)
                        if (!double.IsNaN(ir.IATScore))
                            squaredSums += (ir.IATScore - Mean) * (ir.IATScore - Mean);
                    _SD = Math.Sqrt(squaredSums / (IATResultSets.Count - 1));
                    return _SD;
                }
            }

            public double Mean
            {
                get
                {
                    if (!double.IsNaN(_Mean))
                        return _Mean;
                    int nItems = 0;
                    double mean = 0;
                    foreach (IResultSet ir in IATResultSets)
                        if (!double.IsNaN(ir.IATScore))
                        {
                            mean += ir.IATScore;
                            nItems++;
                        }
                    _Mean = mean / nItems;
                    return _Mean;
                }
            }


            public IResultSet this[int n]
            {
                get
                {
                    return IATResultSets[n];
                }
            }

            public int NumResultSets
            {
                get
                {
                    return IATResultSets.Count;
                }
            }

            public IATResultSetList(ResultSetDescriptor rsd)
            {
                Descriptor = rsd;
                Factory = rsd;
            }

            public void CalcPercentileScores()
            {
                for (int ctr = 0; ctr < IATResultSets.Count; ctr++)
                    CalcPercentileScore(ctr);
            }

            protected void CalcPercentileScore(int nItem)
            {
                double PDF = (1.0 / (Math.Sqrt(2 * Math.PI * SD * SD))) * Math.Exp(-(IATResultSets[nItem].IATScore - Mean) / (2 * SD * SD));
                double t = 1.0 / (1 + b0 * IATResultSets[nItem].IATScore);
                double Percentile = 1 - PDF * ((b1 * t) + (b2 * t * t) + (b3 * t * t * t) + (b4 * t * t * t * t) + (b5 * t * t * t * t * t));
                IATResultSets[nItem].SetPercentileScore(Percentile);
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATResultSetList");
                writer.WriteAttributeString("NumResultSets", NumResultSets.ToString());
                for (int ctr = 0; ctr < NumResultSets; ctr++)
                    IATResultSets[ctr].WriteXml(writer);
                writer.WriteEndElement();
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                MainForm.Invoke(ResultPackage.SetProgressRange, 0, NumResultSets);
                reader.ReadStartElement();
                IATResultSets = new List<IResultSet>();
                for (int ctr = 0; ctr < NumResultSets; ctr++)
                {
                    IATResultSets.Add(Factory.CreateResultSet());
                    IATResultSets[ctr].ReadXml(reader);
                    MainForm.Invoke(ResultPackage.ProgressIncrement, 1);
                }
                reader.ReadEndElement();
            }

            public String GetName()
            {
                return "IATResultSetList";
            }
        }
    }
}