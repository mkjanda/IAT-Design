using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Security.Cryptography;

namespace IATClient
{
    namespace IATResultSet
    {
        class EncryptedResultSet : INamedXmlSerializable
        {
            private List<String> BeforeSurveyStringData, AfterSurveyStringData;
            private String IATStringData;

            public EncryptedResultSet()
            {
                BeforeSurveyStringData = new List<String>();
                AfterSurveyStringData = new List<String>();
                IATStringData = String.Empty;
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                int nBeforeSurveys = Convert.ToInt32(reader["NumBeforeSurveys"]);
                int nAfterSurveys = Convert.ToInt32(reader["NumAfterSurveys"]);
                reader.ReadStartElement();
                for (int ctr = 0; ctr < nBeforeSurveys; ctr++)
                    BeforeSurveyStringData.Add(reader.ReadElementString());
                IATStringData = reader.ReadElementString();
                for (int ctr = 0; ctr < nAfterSurveys; ctr++)
                    AfterSurveyStringData.Add(reader.ReadElementString());
                reader.ReadEndElement();
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("EncryptedResultSet");
                writer.WriteAttributeString("NumBeforeSurveys", BeforeSurveyStringData.Count.ToString());
                writer.WriteAttributeString("NumAfterSurveys", AfterSurveyStringData.Count.ToString());
                for (int ctr = 0; ctr < BeforeSurveyStringData.Count; ctr++)
                    writer.WriteElementString("Results", BeforeSurveyStringData[ctr]);
                writer.WriteElementString("Results", IATStringData);
                for (int ctr = 0; ctr < AfterSurveyStringData.Count; ctr++)
                    writer.WriteElementString("Results", AfterSurveyStringData[ctr]);
                writer.WriteEndElement();
            }

            public XmlSchema GetSchema()
            {
                return null;
            }

            public IATResultSet Decrypt(RSACryptoServiceProvider crypt)
            {
                IATResultSet resultSet = new IATResultSet();
                byte[] encryptedData, decryptedData;
                XmlTextReader xReader;
                MemoryStream memStream;

                encryptedData = Convert.FromBase64String(IATStringData);
                decryptedData = crypt.Decrypt(encryptedData, false);
                memStream = new MemoryStream(decryptedData);
                xReader = new XmlTextReader(memStream);
                int nIATResults = Convert.ToInt32(xReader[0]);
                xReader.ReadStartElement();
                resultSet.IATResults = new IATResultSetElement[nIATResults];
                for (int ctr = 0; ctr < nIATResults; ctr++)
                {
                    IATResultSetElement element = new IATResultSetElement();
                    element.ReadXml(xReader);
                    resultSet.IATResults[ctr] = element;
                }

                resultSet.BeforeSurvey = new SurveyResponseSet[BeforeSurveyStringData.Count];
                for (int ctr = 0; ctr < BeforeSurveyStringData.Count; ctr++)
                {
                    encryptedData = Convert.FromBase64String(BeforeSurveyStringData[ctr]);
                    decryptedData = crypt.Decrypt(encryptedData, false);
                    memStream = new MemoryStream(decryptedData);
                    xReader = new XmlTextReader(memStream);
                    SurveyResponseSet surveyResponse = new SurveyResponseSet();
                    surveyResponse.ReadXml(xReader);
                }

                resultSet.AfterSurvey = new SurveyResponseSet[AfterSurveyStringData.Count];
                for (int ctr = 0; ctr < AfterSurveyStringData.Count; ctr++)
                {
                    encryptedData = Convert.FromBase64String(AfterSurveyStringData[ctr]);
                    decryptedData = crypt.Decrypt(encryptedData, false);
                    memStream = new MemoryStream(decryptedData);
                    xReader = new XmlTextReader(memStream);
                    SurveyResponseSet surveyResponse = new SurveyResponseSet();
                    surveyResponse.ReadXml(xReader);
                }

                return resultSet;
            }

            public String GetName()
            {
                return "EncryptedResultSet";
            }
        }

        class IATResultSetElementList : INamedXmlSerializable
        {
            private List<IATResultSetElement> _ElementList = new List<IATResultSetElement>();


            public List<IATResultSetElement> ElementList
            {
                get
                {
                    return _ElementList;
                }
            }

            public IATResultSetElementList()
            {
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
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                int nElements = Convert.ToInt32(reader["NumElements"]);
                ElementList.Clear();
                reader.ReadStartElement();
                for (int ctr = 0; ctr < nElements; ctr++)
                {
                    IATResultSetElement element = new IATResultSetElement();
                    element.ReadXml(reader);
                    ElementList.Add(element);
                }
            }

            public String GetName()
            {
                return "IATResultSetElementList";
            }
        }




        public class IATResultSetElement : INamedXmlSerializable
        {
            private int _BlockNumber, _ItemNumber, _PresentationNum;
            private long _ResponseTime;
            private bool _Error;

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

            public bool Error
            {
                get
                {
                    return _Error;
                }
                set
                {
                    _Error = value;
                }
            }

            public IATResultSetElement()
            {
                _ItemNumber = -1;
                _ResponseTime = -1;
                _BlockNumber = -1;
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATResultSetElement");
                writer.WriteElementString("BlockNum", BlockNumber.ToString());
                writer.WriteElementString("ItemNum", ItemNumber.ToString());
                writer.WriteElementString("ResponseTime", ResponseTime.ToString());
                writer.WriteElementString("PresentationNum", PresentationNumber.ToString());
                writer.WriteEndElement();
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                BlockNumber = reader.ReadElementContentAsInt();
                ItemNumber = reader.ReadElementContentAsInt();
                ResponseTime = reader.ReadElementContentAsLong();
                Error = reader.ReadElementContentAsBoolean();
                PresentationNumber = reader.ReadElementContentAsInt();
                reader.ReadEndElement();
            }

            public String GetName()
            {
                return "IATResultSetElement";
            }
        }

        public class SurveyResponseSet : INamedXmlSerializable
        {
            private string[] _SurveyResults = null;
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

            public string[] SurveyResults
            {
                get
                {
                    return _SurveyResults;
                }
                set
                {
                    _SurveyResults = value;
                }
            }

            public SurveyResponseSet() { }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("SurveyResults");
                writer.WriteAttributeString("NumSurveyResults", NumSurveyResults.ToString());
                for (int ctr = 0; ctr < SurveyResults.Length; ctr++)
                    writer.WriteElementString("SurveyResult", SurveyResults[ctr]);
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
                    SurveyResults = new string[1];
                    SurveyResults[0] = "N/A";
                }
                else
                {
                    SurveyResults = new string[NumSurveyResults];
                    for (int ctr = 0; ctr < NumSurveyResults; ctr++)
                        SurveyResults[ctr] = reader.ReadElementContentAsString();
                    reader.ReadEndElement();
                }
            }

            public String GetName()
            {
                return "SurveyResponseSet";
            }
        }

        public class IATResultSet : INamedXmlSerializable
        {
            private SurveyResponseSet[] _BeforeSurvey, _AfterSurvey;
            private IATResultSetElement[] _IATResults;
            private int _NumResultSetElements = 0;
            private long _ResultID = -1;
            private DateTime _Timestamp;
            private double _Percentile = double.NaN;

            public double Percentile
            {
                get
                {
                    return _Percentile;
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

            public int NumResultSetElements
            {
                get
                {
                    return _NumResultSetElements;
                }
                set
                {
                    _NumResultSetElements = value;
                }
            }

            public SurveyResponseSet[] BeforeSurvey
            {
                get
                {
                    return _BeforeSurvey;
                }
                set
                {
                    _BeforeSurvey = value;
                }
            }

            public SurveyResponseSet[] AfterSurvey
            {
                get
                {
                    return _AfterSurvey;
                }
                set
                {
                    _AfterSurvey = value;
                }
            }

            public IATResultSetElement[] IATResults
            {
                get
                {
                    return _IATResults;
                }
                set
                {
                    _IATResults = value;
                }
            }

            private double _IATScore;

            public double IATScore
            {
                get
                {
                    if ((double.IsNaN(_IATScore)) && (NumResultSetElements != 0))
                        Score();
                    return _IATScore;
                }
            }

            public void SetPercentileScore(double percentile)
            {
                _Percentile = percentile;
            }

            protected double MeanTotalLatency(List<IATResultSetElement> elements)
            {
                double mean = 0;
                for (int ctr = 0; ctr < elements.Count; ctr++)
                    mean += elements[ctr].ResponseTime;
                return mean / elements.Count;
            }

            protected double SDTotalLatency(List<IATResultSetElement> elements)
            {
                double mean = MeanTotalLatency(elements);
                double sd = 0;
                for (int ctr = 0; ctr < elements.Count; ctr++)
                    sd += ((double)elements[ctr].ResponseTime - mean) * ((double)elements[ctr].ResponseTime - mean);
                return Math.Sqrt(sd / (double)(elements.Count - 1));
            }

            public void Score()
            {
                // test for an invalid administration (mean latency < 300ms)
                long LatencySum = 0;
                int nLT300 = 0;
                for (int ctr = 0; ctr < NumResultSetElements; ctr++)
                {
                    LatencySum += IATResults[ctr].ResponseTime;
                    if (IATResults[ctr].ResponseTime < 300)
                        nLT300++;
                }
                if (nLT300 * 10 >= NumResultSetElements)
                {
                    _IATScore = double.NaN;
                    return;
                }

                List<IATResultSetElement> Block3 = new List<IATResultSetElement>();
                List<IATResultSetElement> Block4 = new List<IATResultSetElement>();
                List<IATResultSetElement> Block6 = new List<IATResultSetElement>();
                List<IATResultSetElement> Block7 = new List<IATResultSetElement>();

                for (int ctr = 0; ctr < NumResultSetElements; ctr++)
                {
                    if (IATResults[ctr].ResponseTime > 10000)
                        continue;

                    switch (IATResults[ctr].BlockNumber)
                    {
                        case 3:
                            Block3.Add(IATResults[ctr]);
                            break;

                        case 4:
                            Block4.Add(IATResults[ctr]);
                            break;

                        case 6:
                            Block6.Add(IATResults[ctr]);
                            break;

                        case 7:
                            Block7.Add(IATResults[ctr]);
                            break;
                    }
                }

                List<IATResultSetElement> InclusiveSDList1 = new List<IATResultSetElement>();
                List<IATResultSetElement> InclusiveSDList2 = new List<IATResultSetElement>();

                InclusiveSDList1.AddRange(Block3);
                InclusiveSDList1.AddRange(Block6);
                double sd3_6 = SDTotalLatency(InclusiveSDList1);

                InclusiveSDList2.AddRange(Block4);
                InclusiveSDList2.AddRange(Block7);
                double sd4_7 = SDTotalLatency(InclusiveSDList2);

                double mean3 = MeanTotalLatency(Block3);
                double mean4 = MeanTotalLatency(Block4);
                double mean6 = MeanTotalLatency(Block6);
                double mean7 = MeanTotalLatency(Block7);

                _IATScore = (((mean6 - mean3) / sd3_6) + ((mean7 - mean4) / sd4_7)) / 2;
            }

            public List<IATResultSetElement> GetResultsForItem(int ItemNum)
            {
                List<IATResultSetElement> ResultList = new List<IATResultSetElement>();

                for (int ctr = 0; ctr < IATResults.Length; ctr++)
                {
                    if (IATResults[ctr].ItemNumber == ItemNum)
                    {
                        ResultList.Add(IATResults[ctr]);
                    }
                }

                return ResultList;
            }

            public void Save(BinaryWriter bWriter)
            {
                bWriter.Write(Convert.ToDouble(IATScore));
                bWriter.Write(Convert.ToInt32(IATResults.Length));
                for (int ctr = 0; ctr < IATResults.Length; ctr++)
                {
                    bWriter.Write(Convert.ToInt32(IATResults[ctr].ItemNumber));
                    bWriter.Write(Convert.ToInt32(IATResults[ctr].BlockNumber));
                    bWriter.Write(Convert.ToInt64(IATResults[ctr].ResponseTime));
                }
                bWriter.Write(Convert.ToInt32(BeforeSurvey.Length));
                for (int ctr1 = 0; ctr1 < BeforeSurvey.Length; ctr1++)
                {
                    bWriter.Write(Convert.ToInt32(BeforeSurvey[ctr1].SurveyResults.Length));
                    for (int ctr2 = 0; ctr2 < BeforeSurvey[ctr1].SurveyResults.Length; ctr2++)
                    {
                        bWriter.Write(Convert.ToInt32(BeforeSurvey[ctr1].SurveyResults[ctr2].Length));
                        bWriter.Write(BeforeSurvey[ctr1].SurveyResults[ctr2].ToCharArray());
                    }
                }
                bWriter.Write(Convert.ToInt32(AfterSurvey.Length));
                for (int ctr1 = 0; ctr1 < AfterSurvey[ctr1].SurveyResults.Length; ctr1++)
                {
                    bWriter.Write(Convert.ToInt32(AfterSurvey[ctr1].SurveyResults.Length));
                    for (int ctr2 = 0; ctr2 < AfterSurvey[ctr1].SurveyResults.Length; ctr1++)
                    {
                        bWriter.Write(Convert.ToInt32(AfterSurvey[ctr1].SurveyResults[ctr2].Length));
                        bWriter.Write(AfterSurvey[ctr1].SurveyResults[ctr2].ToCharArray());
                    }
                }
            }

            public void Load(BinaryReader bReader)
            {
                _IATScore = bReader.ReadDouble();
                NumResultSetElements = bReader.ReadInt32();
                IATResults = new IATResultSetElement[NumResultSetElements];
                for (int ctr = 0; ctr < NumResultSetElements; ctr++)
                {
                    IATResults[ctr].ItemNumber = bReader.ReadInt32();
                    IATResults[ctr].BlockNumber = bReader.ReadInt32();
                    IATResults[ctr].ResponseTime = bReader.ReadInt64();
                }
                int nSurveys = bReader.ReadInt32();
                BeforeSurvey = new SurveyResponseSet[nSurveys];
                for (int ctr1 = 0; ctr1 < nSurveys; ctr1++)
                {
                    int nSurveyItems = bReader.ReadInt32();
                    BeforeSurvey[ctr1].SurveyResults = new string[nSurveyItems];
                    for (int ctr2 = 0; ctr2 < nSurveyItems; ctr2++)
                    {
                        int nLen = bReader.ReadInt32();
                        BeforeSurvey[ctr1].SurveyResults[ctr2] = new string(bReader.ReadChars(nLen));
                    }
                }
                nSurveys = bReader.ReadInt32();
                AfterSurvey = new SurveyResponseSet[nSurveys];
                for (int ctr1 = 0; ctr1 < nSurveys; ctr1++)
                {
                    int nSurveyItems = bReader.ReadInt32();
                    AfterSurvey[ctr1].SurveyResults = new string[nSurveyItems];
                    for (int ctr2 = 0; ctr2 < nSurveyItems; ctr2++)
                    {
                        int nLen = bReader.ReadInt32();
                        AfterSurvey[ctr1].SurveyResults[ctr2] = new string(bReader.ReadChars(nLen));
                    }
                }
            }


            public IATResultSet()
            {
                _IATScore = double.NaN;
                _BeforeSurvey = null;
                _AfterSurvey = null;
                _IATResults = null;
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATResultSet");
                writer.WriteAttributeString("NumBeforeSurveys", BeforeSurvey.Length.ToString());
                writer.WriteAttributeString("NumResultSetElements", NumResultSetElements.ToString());
                writer.WriteAttributeString("NumAfterSurveys", AfterSurvey.Length.ToString());
                for (int ctr = 0; ctr < BeforeSurvey.Length; ctr++)
                    BeforeSurvey[ctr].WriteXml(writer);
                for (int ctr = 0; ctr < IATResults.Length; ctr++)
                    IATResults[ctr].WriteXml(writer);
                for (int ctr = 0; ctr < AfterSurvey.Length; ctr++)
                    AfterSurvey[ctr].WriteXml(writer);
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                int nBeforeSurveys = Convert.ToInt32(reader["NumBeforeSurveys"]);
                NumResultSetElements = Convert.ToInt32(reader["NumResultSetElements"]);
                int nAfterSurveys = Convert.ToInt32(reader["NumAfterSurveys"]);
                reader.ReadStartElement();
                BeforeSurvey = new SurveyResponseSet[nBeforeSurveys];
                for (int ctr = 0; ctr < nBeforeSurveys; ctr++)
                {
                    BeforeSurvey[ctr] = new SurveyResponseSet();
                    BeforeSurvey[ctr].ReadXml(reader);
                }
                IATResults = new IATResultSetElement[NumResultSetElements];
                for (int ctr = 0; ctr < NumResultSetElements; ctr++)
                {
                    IATResults[ctr] = new IATResultSetElement();
                    IATResults[ctr].ReadXml(reader);
                }
                AfterSurvey = new SurveyResponseSet[nAfterSurveys];
                for (int ctr = 0; ctr < nAfterSurveys; ctr++)
                {
                    AfterSurvey[ctr] = new SurveyResponseSet();
                    AfterSurvey[ctr].ReadXml(reader);
                }

                reader.ReadEndElement();
            }

            public String GetName()
            {
                return "IATResultSet";
            }
        }

        public class IATResultSetList : INamedXmlSerializable
        {
            private IATResultSet[] _IATResultSets = null;
            private int _NumResultSets = -1;
            private double _SD, _Mean;
            private const double b0 = 0.2316419, b1 = 0.319381530, b2 = -0.356563782, b3 = 1.781477937, b4 = -1.821255978, b5 = 1.330274429;


            public double SD
            {
                get
                {
                    return _SD;
                }
                set
                {
                    _SD = value;
                }
            }

            public double Mean
            {
                get
                {
                    return _Mean;
                }
                set
                {
                    _Mean = value;
                }
            }



            public IATResultSet[] IATResultSets
            {
                get
                {
                    return _IATResultSets;
                }
                set
                {
                    _IATResultSets = value;
                }
            }

            public int NumResultSets
            {
                get
                {
                    return _NumResultSets;
                }
                set
                {
                    _NumResultSets = value;
                }
            }

            public IATResultSetList()
            {
            }

            public void CalcPercentileScores()
            {
                for (int ctr = 0; ctr < _IATResultSets.Length; ctr++)
                    CalcPercentileScore(ctr);
            }

            protected void CalcPercentileScore(int nItem)
            {
                double PDF = (1.0 / (Math.Sqrt(2 * Math.PI * SD * SD))) * Math.Exp(-(_IATResultSets[nItem].IATScore - Mean) / (2 * SD * SD));
                double t = 1.0 / (1 + b0 * _IATResultSets[nItem].IATScore);
                double Percentile = 1 - PDF * ((b1 * t) + (b2 * t * t) + (b3 * t * t * t) + (b4 * t * t * t * t) + (b5 * t * t * t * t * t));
                _IATResultSets[nItem].SetPercentileScore(Percentile);
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
                NumResultSets = Convert.ToInt32(reader["NumResultSets"]);
                if (ResultPackage.ProgressWin != null)
                    ResultPackage.ProgressWin.Invoke(ResultPackage.SetProgressRange, 0, NumResultSets);
                reader.ReadStartElement();
                _IATResultSets = new IATResultSet[NumResultSets];
                for (int ctr = 0; ctr < NumResultSets; ctr++)
                {
                    IATResultSets[ctr] = new IATResultSet();
                    IATResultSets[ctr].ReadXml(reader);
                    if (ResultPackage.ProgressWin != null)
                        ResultPackage.ProgressWin.Invoke(ResultPackage.ProgressIncrement, 1);
                }
                reader.ReadEndElement();
            }

            public void Save(BinaryWriter bWriter)
            {
                bWriter.Write(Convert.ToInt32(NumResultSets));
                for (int ctr = 0; ctr < NumResultSets; ctr++)
                    IATResultSets[ctr].Save(bWriter);
            }

            public void Load(BinaryReader bReader)
            {
                NumResultSets = bReader.ReadInt32();
                IATResultSets = new IATResultSet[NumResultSets];
                for (int ctr = 0; ctr < NumResultSets; ctr++)
                    IATResultSets[ctr].Load(bReader);
            }

            public String GetName()
            {
                return "IATResultSetList";
            }
        }
    }
}