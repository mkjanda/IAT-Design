using System;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Security.Permissions;
using System.Text;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using System.Security.Cryptography;
using System.Windows.Forms;

namespace IATClient
{
    namespace IATResultSetNamespaceV1
    {
        /*
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
        */
        public class IATResultSetElementList : IResultSetElem
        {
            protected List<IATResultSetElem> ElementList = new List<IATResultSetElem>();
            protected IResultElemFactory ElemFactory;
            protected double _SD = double.NaN;
            /*
                        public List<IATResultSetElement> ElementList
                        {
                            get
                            {
                                return _ElementList;
                            }
                        }
            */
            public IATResultSetElement this[int ndx]
            {
                get
                {
                    return (IATResultSetElement)ElementList[ndx];
                }
            }

            public int NumItems
            {
                get
                {
                    return ElementList.Count;
                }
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

            public bool ReadXml(XmlReader reader)
            {
                try
                {
                    ElementList.Clear();
                    reader.ReadStartElement("IATResultSetElementList");
                    while (reader.Name == "IATResultSetElement")
                    {
                        IATResultSetElem element = ElemFactory.CreateIATResultSetElem();
                        element.ReadXml(reader);
                        ElementList.Add(element);
                    }
                    reader.ReadEndElement();
                    return true;
                }
                catch (Exception ex)
                {
           //         MessageBox.Show("Corrupt result set encountered and discarded");
                    return false;
                }
            }
            /*
            public void Save(BinaryWriter bWriter)
            {
                bWriter.Write(Convert.ToInt32(ElementList.Count));
                foreach (IATResultSetElement rse in ElementList)
                    rse.Save(bWriter);
            }

            public void Load(BinaryReader bReader)
            {
                ElementList.Clear();
                int nElems = bReader.ReadInt32();
                for (int ctr = 0; ctr < nElems; ctr++)
                    ElementList.Add(new IATResultSetElement(bReader));
            }
            */
            public String GetName()
            {
                return "IATResultSetElementList";
            }
        }




        public class IATResultSetElement : INamedXmlSerializable, IATResultSetElem
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

            public IATResultSetElement()
            {
                _ItemNumber = -1;
                _ResponseTime = -1;
                _BlockNumber = -1;
            }

            public IATResultSetElement(BinaryReader bReader)
            {
                _BlockNumber = bReader.ReadInt32();
                _ItemNumber = bReader.ReadInt32();
                _ResponseTime = bReader.ReadInt64();
                _PresentationNum = bReader.ReadInt32();
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

            public void Save(BinaryWriter bWriter)
            {
                bWriter.Write(Convert.ToInt32(BlockNumber));
                bWriter.Write(Convert.ToInt32(ItemNumber));
                bWriter.Write(Convert.ToInt64(ResponseTime));
                bWriter.Write(Convert.ToInt32(PresentationNumber));
            }

            public String GetName()
            {
                return "IATResultSetElement";
            }
        }

        public class SurveyResponseSet : IResultSetElem
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

            public bool ReadXml(XmlReader reader)
            {
                try
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
                    return true;
                }
                catch (Exception ex)
                {
 //                   MessageBox.Show("Corrupt result set encountered and discarded.");
                    return false;
                }
            }

            public String GetName()
            {
                return "SurveyResponseSet";
            }
        }

        public class IATResultSetEnumerator : IEnumerator<IResultSetElem>
        {
            private int ndx = -1;
            public List<IResultSetElem> Elements = new List<IResultSetElem>();

            public IATResultSetEnumerator(SurveyResponseSet[] befSurveys, IATResultSetElementList iatElemList, SurveyResponseSet[] aftSurveys)
            {
                Elements.AddRange(befSurveys);
                Elements.Add(iatElemList);
                Elements.AddRange(aftSurveys);
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
            protected SurveyResponseSet[] _BeforeSurveys, _AfterSurveys;
            protected IATResultSetElementList _IATResults;
            protected int _NumResultSetElements = 0;
            protected long _ResultID = -1;
            protected DateTime _Timestamp;
            protected double _Percentile = double.NaN;
            protected IResultElemFactory ElemFactory;

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

            public SurveyResponseSet[] BeforeSurveys
            {
                get
                {
                    return _BeforeSurveys;
                }
            }

            public SurveyResponseSet[] AfterSurveys
            {
                get
                {
                    return _AfterSurveys;
                }
            }

            public IATResultSetElementList IATResults
            {
                get
                {
                    return _IATResults;
                }
            }

            public IEnumerator<IResultSetElem> GetEnumerator()
            {
                return new IATResultSetEnumerator(BeforeSurveys, IATResults, AfterSurveys);
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return new IATResultSetEnumerator(BeforeSurveys, IATResults, AfterSurveys);
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
                    if ((double.IsNaN(_IATScore)) && (IATResults.NumItems != 0))
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
                for (int ctr = 0; ctr < IATResults.NumItems; ctr++)
                {
                    LatencySum += IATResults[ctr].ResponseTime;
                    if (IATResults[ctr].ResponseTime < 300)
                        nLT300++;
                }
                if (nLT300 * 10 >= IATResults.NumItems)
                {
                    _IATScore = double.NaN;
                    return;
                }

                List<IATResultSetElement> Block3 = new List<IATResultSetElement>();
                List<IATResultSetElement> Block4 = new List<IATResultSetElement>();
                List<IATResultSetElement> Block6 = new List<IATResultSetElement>();
                List<IATResultSetElement> Block7 = new List<IATResultSetElement>();

                for (int ctr = 0; ctr < IATResults.NumItems; ctr++)
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

                for (int ctr = 0; ctr < IATResults.NumItems; ctr++)
                {
                    if (IATResults[ctr].ItemNumber == ItemNum)
                    {
                        ResultList.Add(IATResults[ctr]);
                    }
                }

                return ResultList;
            }
            /*
            public void Save(BinaryWriter bWriter)
            {
                bWriter.Write(Convert.ToDouble(IATScore));
                IATResults.Save(bWriter);
                bWriter.Write(Convert.ToInt32(BeforeSurveys.Length));
                for (int ctr1 = 0; ctr1 < BeforeSurveys.Length; ctr1++)
                {
                    bWriter.Write(Convert.ToInt32(BeforeSurveys[ctr1].SurveyResults.Length));
                    for (int ctr2 = 0; ctr2 < BeforeSurveys[ctr1].SurveyResults.Length; ctr2++)
                    {
                        bWriter.Write(Convert.ToInt32(BeforeSurveys[ctr1].SurveyResults[ctr2].Length));
                        bWriter.Write(BeforeSurveys[ctr1].SurveyResults[ctr2].ToCharArray());
                    }
                }
                bWriter.Write(Convert.ToInt32(AfterSurveys.Length));
                for (int ctr1 = 0; ctr1 < AfterSurveys[ctr1].SurveyResults.Length; ctr1++)
                {
                    bWriter.Write(Convert.ToInt32(AfterSurveys[ctr1].SurveyResults.Length));
                    for (int ctr2 = 0; ctr2 < AfterSurveys[ctr1].SurveyResults.Length; ctr1++)
                    {
                        bWriter.Write(Convert.ToInt32(AfterSurveys[ctr1].SurveyResults[ctr2].Length));
                        bWriter.Write(AfterSurveys[ctr1].SurveyResults[ctr2].ToCharArray());
                    }
                }
            }

            public void Load(BinaryReader bReader)
            {
                _IATScore = bReader.ReadDouble();
                int nItems = bReader.ReadInt32();
                _IATResults = new IATResultSetElementList();
                IATResults.Load(bReader);
                int nSurveys = bReader.ReadInt32();
                _BeforeSurveys = new SurveyResponseSet[nSurveys];
                for (int ctr1 = 0; ctr1 < nSurveys; ctr1++)
                {
                    int nSurveyItems = bReader.ReadInt32();
                    BeforeSurveys[ctr1].SurveyResults = new string[nSurveyItems];
                    for (int ctr2 = 0; ctr2 < nSurveyItems; ctr2++)
                    {
                        int nLen = bReader.ReadInt32();
                        BeforeSurveys[ctr1].SurveyResults[ctr2] = new string(bReader.ReadChars(nLen));
                    }
                }
                nSurveys = bReader.ReadInt32();
                _AfterSurveys = new SurveyResponseSet[nSurveys];
                for (int ctr1 = 0; ctr1 < nSurveys; ctr1++)
                {
                    int nSurveyItems = bReader.ReadInt32();
                    AfterSurveys[ctr1].SurveyResults = new string[nSurveyItems];
                    for (int ctr2 = 0; ctr2 < nSurveyItems; ctr2++)
                    {
                        int nLen = bReader.ReadInt32();
                        AfterSurveys[ctr1].SurveyResults[ctr2] = new string(bReader.ReadChars(nLen));
                    }
                }
            }
            */
            protected IATResultSet() { }

            public IATResultSet(ResultSetDescriptor rsd)
            {
                _IATScore = double.NaN;
                _BeforeSurveys = new SurveyResponseSet[rsd.BeforeSurveys.Count];
                for (int ctr = 0; ctr < BeforeSurveys.Length; ctr++)
                    BeforeSurveys[ctr] = new SurveyResponseSet();
                _AfterSurveys = new SurveyResponseSet[rsd.AfterSurveys.Count];
                for (int ctr = 0; ctr < AfterSurveys.Length; ctr++)
                    AfterSurveys[ctr] = new SurveyResponseSet();
                _IATResults = new IATResultSetElementList(rsd);
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATResultSet");
                writer.WriteAttributeString("NumBeforeSurveys", BeforeSurveys.Length.ToString());
                writer.WriteAttributeString("NumResultSetElements", IATResults.NumItems.ToString());
                writer.WriteAttributeString("NumAfterSurveys", AfterSurveys.Length.ToString());
                for (int ctr = 0; ctr < BeforeSurveys.Length; ctr++)
                    BeforeSurveys[ctr].WriteXml(writer);
                IATResults.WriteXml(writer);
                for (int ctr = 0; ctr < AfterSurveys.Length; ctr++)
                    AfterSurveys[ctr].WriteXml(writer);
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                int nBeforeSurveys = Convert.ToInt32(reader["NumBeforeSurveys"]);
                int nItems = Convert.ToInt32(reader["NumResultSetElements"]);
                int nAfterSurveys = Convert.ToInt32(reader["NumAfterSurveys"]);
                reader.ReadStartElement();
                _BeforeSurveys = new SurveyResponseSet[nBeforeSurveys];
                for (int ctr = 0; ctr < nBeforeSurveys; ctr++)
                {
                    BeforeSurveys[ctr] = new SurveyResponseSet();
                    BeforeSurveys[ctr].ReadXml(reader);
                }
                _IATResults = new IATResultSetElementList(ElemFactory);
                IATResults.ReadXml(reader);
                _AfterSurveys = new SurveyResponseSet[nAfterSurveys];
                for (int ctr = 0; ctr < nAfterSurveys; ctr++)
                {
                    AfterSurveys[ctr] = new SurveyResponseSet();
                    AfterSurveys[ctr].ReadXml(reader);
                }

                reader.ReadEndElement();
            }

            public String GetName()
            {
                return "IATResultSet";
            }
        }

        public class IATResultSetList : INamedXmlSerializable, IResultData
        {
            protected List<IResultSet> IATResultSets = new List<IResultSet>();
            private int _NumResultSets = -1;
            private double _SD = double.NaN, _Mean = double.NaN;
            private const double b0 = 0.2316419, b1 = 0.319381530, b2 = -0.356563782, b3 = 1.781477937, b4 = -1.821255978, b5 = 1.330274429;
            private ResultSetDescriptor Descriptor;
            private Control InvokeTarget;
            private IResultElemFactory Factory;

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
                    return _NumResultSets;
                }
                set
                {
                    _NumResultSets = value;
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
                NumResultSets = Convert.ToInt32(reader["NumResultSets"]);
                InvokeTarget.Invoke(ResultPackage.SetProgressRange, 0, NumResultSets);
                reader.ReadStartElement();
                IATResultSets = new List<IResultSet>();
                for (int ctr = 0; ctr < NumResultSets; ctr++)
                {
                    IATResultSets.Add(Factory.CreateResultSet());
                    IATResultSets[ctr].ReadXml(reader);
                    InvokeTarget.Invoke(ResultPackage.ProgressIncrement, 1);
                }
                reader.ReadEndElement();
            }
            /*
            public void Save(BinaryWriter bWriter)
            {
                bWriter.Write(Convert.ToInt32(NumResultSets));
                for (int ctr = 0; ctr < NumResultSets; ctr++)
                    IATResultSets[ctr].Save(bWriter);
            }
            
            public void Load(BinaryReader bReader)
            {
                NumResultSets = bReader.ReadInt32();
                IATResultSets = new List<IResultSet>();
                for (int ctr = 0; ctr < NumResultSets; ctr++)
                {
                    IATResultSets.Add(new IATResultSet(Descriptor));
                    IATResultSets[ctr].Load(bReader);
                }
            }
            */
            public void ExportIATLatenciesByItem(String filename)
            {
                FileStream fStream = File.Open(filename, FileMode.Create);
                TextWriter writer = new StreamWriter(fStream);
                writer.WriteLine("// Output File Format");
                writer.WriteLine("//\t This output file is grouped by item.  Slides of the various items are available in");
                writer.WriteLine("//\t the directory that contains the packaged IAT, in the subdirectory \"ItemSlides.\"");
                writer.WriteLine("//\t Alternatively, slides of the items can be retrieved from the server in via the");
                writer.WriteLine("//\t Server Interface by clicking on the \"Retrieve Item Slides\" button.");
                writer.WriteLine("//");
                writer.WriteLine("//\tEach item is followed by a row of comma delimited data that contains, first, a number");
                writer.WriteLine("//\tthat anonymously identifies the testee followed by the a list of response latencies");
                writer.WriteLine("//\tfor each administration of that item.  The last value in the list is the testee's");
                writer.WriteLine("//\ttotal score on the IAT");
                writer.WriteLine("//");
                writer.WriteLine("//\tThis file contains data for all administered items, including those that were not employed in");
                writer.WriteLine("//\tscoring the IAT.");
                writer.WriteLine();

                int nItems = Descriptor.ConfigFile.NumIATItems;
                for (int ctr1 = 1; ctr1 <= nItems; ctr1++)
                {
                    writer.WriteLine(String.Format("Item #{0}", ctr1));
                    int ctr2 = 0;

                        List<IATResultSet> rsList = new List<IATResultSet>();
                        foreach (IResultSet rs in IATResultSets)
                            rsList.Add((IATResultSet)rs);
                        foreach (IATResultSet rs in rsList)
                        {
                            String str = String.Empty;
                            for (int ctr3 = 0; ctr3 < rs.IATResults.NumItems; ctr3++)
                                if (rs.IATResults[ctr3].ItemNumber == ctr1)
                                    str += String.Format("{0}, ", rs.IATResults[ctr3].ResponseTime);
                            if (str != String.Empty)
                                writer.WriteLine(String.Format("{0}, {1} {2:F6}", ctr2 + 1, str, rs.IATScore));
                            ctr2++;
                        }
                    writer.Write("\r\n");
                }
                writer.Flush();
                fStream.Close();
            }

            public void ExportIATLatenciesByTestee(String filename)
            {
                FileStream fStream = File.Open(filename, FileMode.Create);
                TextWriter writer = new StreamWriter(fStream);
                writer.WriteLine("// Output File Format");
                writer.WriteLine("//\t This output file is grouped by testee.  Each testee is identified anonymously with");
                writer.WriteLine("//\t a unique ID number.  On the following line is that testee's IAT score.  Beneath");
                writer.WriteLine("//\t the IAT score, a line appears for each item in the IAT that was administered to the");
                writer.WriteLine("//\t testee.  This item correlates to a numbered slide that can be found in the directory");
                writer.WriteLine("//\t that contains the packaged IAT.  Alternatively, the item slides can be retrieved from");
                writer.WriteLine("//\t the server via the Server Interface by clicking the \"Retrieve Item Slides\" button.");
                writer.WriteLine("//\t Following the item number, on the same line, is a comma delimited list of response");
                writer.WriteLine("//\t latencies.");
                writer.WriteLine("//");
                /*            if (bExcludeUnscoredItems)
                                writer.WriteLine("//\tItems that were not used in the calculation of the IAT score are omitted.");
                            else */
                //            {
                writer.WriteLine("//\tThis file contains data for all administered items, including those that were not employed in");
                writer.WriteLine("//\tscoring the IAT.");
                //           }
                writer.WriteLine();

                int nItems = Descriptor.ConfigFile.NumIATItems;
                for (int ctr1 = 0; ctr1 < IATResultSets.Count; ctr1++)
                {
                    writer.WriteLine(String.Format("Testee #{0}", ctr1 + 1));
                    writer.WriteLine(String.Format("IAT Score: {0}", IATResultSets[ctr1].IATScore));
                    IATResultSetElementList rsel = ((IATResultSet)IATResultSets[ctr1]).IATResults;
                    for (int ctr2 = 1; ctr2 <= nItems; ctr2++)
                    {
                        List<String> latList = new List<String>();
                        List<IATResultSetElement> iResults = new List<IATResultSetElement>();
                        for (int ctr3 = 0; ctr3 < rsel.NumItems; ctr3++)
                            if (rsel[ctr3].ItemNumber == ctr2)
                                iResults.Add(rsel[ctr3]);
                        foreach (IATResultSetElement ire in iResults)
                            latList.Add(ire.ResponseTime.ToString());
                        if (latList.Count > 0)
                            writer.WriteLine(String.Join(", ", ((String[])latList.ToArray())));
                    }
                }
                writer.Write("\r\n");

                writer.Flush();
                fStream.Close();
            }

            public void ExportSummaryFile(String filename)
            {
                FileStream fOut = new FileStream(filename, FileMode.Create);
                StreamWriter writer = new StreamWriter(fOut);
                int nItem = 0;
                //                if (bIncludeHeader)
                //              {
                for (int ctr1 = 0; ctr1 < Descriptor.BeforeSurveys.Count; ctr1++)
                {
                    for (int ctr2 = 0; ctr2 < Descriptor.BeforeSurveys[ctr1].SurveyItems.Length; ctr2++)
                        if (Descriptor.BeforeSurveys[ctr1].SurveyItems[ctr2].Response.ResponseType != IATSurveyFile.ResponseType.None)
                            writer.Write(String.Format("Column {0}: {1}", ++nItem, Descriptor.BeforeSurveys[ctr1].SurveyItems[ctr2].GetDescription()));
                }
                writer.WriteLine(String.Format("Column {0}: IAT Score", ++nItem));
                for (int ctr1 = 0; ctr1 < Descriptor.AfterSurveys.Count; ctr1++)
                {
                    for (int ctr2 = 0; ctr2 < Descriptor.AfterSurveys[ctr1].SurveyItems.Length; ctr2++)
                        if (Descriptor.AfterSurveys[ctr1].SurveyItems[ctr2].Response.ResponseType != IATSurveyFile.ResponseType.None)
                            writer.Write(String.Format("Column {0}: {1}", ++nItem, Descriptor.AfterSurveys[ctr1].SurveyItems[ctr2].GetDescription()));
                }
                writer.WriteLine();
                //            }

                String delim = String.Empty;
                delim = ",";

                for (int ctr1 = 0; ctr1 < IATResultSets.Count; ctr1++)
                {
                    for (int ctr2 = 0; ctr2 < ((IATResultSet)IATResultSets[ctr1]).BeforeSurveys.Length; ctr2++)
                        for (int ctr3 = 0; ctr3 < ((IATResultSet)IATResultSets[ctr1]).BeforeSurveys[ctr2].NumSurveyResults; ctr3++)
                            writer.Write(String.Format("{0}{1}", ((IATResultSet)IATResultSets[ctr1]).BeforeSurveys[ctr2].SurveyResults[ctr3], delim));
                    if (((IATResultSet)IATResultSets[ctr1]).AfterSurveys.Length == 0)
                        writer.Write(String.Format("{0:F6}", IATResultSets[ctr1].IATScore));
                    else {
                        writer.Write(String.Format("{0:F6}{1}", IATResultSets[ctr1], delim));
                        for (int ctr2 = 0; ctr2 < ((IATResultSet)IATResultSets[ctr1]).AfterSurveys.Length; ctr2++)
                            for (int ctr3 = 0; ctr3 < ((IATResultSet)IATResultSets[ctr1]).AfterSurveys[ctr2].NumSurveyResults; ctr3++)
                            {
                                if ((ctr2 != ((IATResultSet)IATResultSets[ctr1]).AfterSurveys.Length - 1) || (ctr3 != ((IATResultSet)IATResultSets[ctr1]).AfterSurveys[ctr2].NumSurveyResults - 1))
                                    writer.Write(String.Format("{0}{1}", ((IATResultSet)IATResultSets[ctr1]).BeforeSurveys[ctr2].SurveyResults[ctr3], delim));
                                else
                                    writer.Write(String.Format("{0}", ((IATResultSet)IATResultSets[ctr1]).BeforeSurveys[ctr2].SurveyResults[ctr3]));
                            }
                    }
                }

                writer.Flush();
                fOut.Close();
            }

            public String GetName()
            {
                return "IATResultSetList";
            }
        }
    }
}