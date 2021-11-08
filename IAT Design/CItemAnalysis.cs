using System;
using System.Collections.Generic;

using System.Text;

namespace IATClient
{
    class CItemAnalysis
    {
        /*
        private CResultData ResultData;
        private CIAT IAT;

        private double ItemTotalCorrelation(int nItemNum, IATResultSet.IATResultSetList data)
        {
            double sumX, sumXSq, sumY, sumYSq, sumXY;
            double meanX, meanY;

            sumX = sumXSq = sumY = sumYSq = sumXY = 0;
            meanX = meanY = 0;

            int nIncludedItems = 0;

            for (int ctr1 = 0; ctr1 < data.NumResultSets; ctr1++)
            {
                for (int ctr2 = 0; ctr2 < data.ResultSets[ctr1].NumResultSetElements; ctr2++)
                    if (data.ResultSets[ctr1].ResultSetElements[ctr2].ItemNumber == nItemNum)
                    {
                        if ((data.ResultSets[ctr1].ResultSetElements[ctr2].TotalResponseTime > 10000) || (data.ResultSets[ctr1].IATScore == double.NaN))
                            break;
                        meanX += data.ResultSets[ctr1].ResultSetElements[ctr2].TotalResponseTime;
                        meanY += data.ResultSets[ctr1].IATScore;
                        nIncludedItems++;
                    }
            }
            meanX /= (double)nIncludedItems;
            meanY /= (double)nIncludedItems;

            for (int ctr1 = 0; ctr1 < data.NumResultSets; ctr1++)
            {
                for (int ctr2 = 0; ctr2 < data.ResultSets[ctr1].NumResultSetElements; ctr2++)
                    if (data.ResultSets[ctr1].ResultSetElements[ctr2].ItemNumber == nItemNum)
                    {
                        if ((data.ResultSets[ctr1].ResultSetElements[ctr2].TotalResponseTime > 10000) || (data.ResultSets[ctr2].IATScore == double.NaN))
                            break;
                        sumX = (double)data.ResultSets[ctr1].ResultSetElements[ctr2].TotalResponseTime - meanX;
                        sumXSq += sumX * sumX;
                        sumY = data.ResultSets[ctr1].IATScore - meanY;
                        sumYSq += sumY * sumY;
                        sumXY += sumX * sumY;
                    }
            }

            return sumXY / (Math.Sqrt(sumXSq) * Math.Sqrt(sumYSq));
        }

        protected IATResultSet.IATResultSetList ConstructResultSubset(List<int> itemNums)
        {
            IATResultSet.IATResultSetList resultSetList = new IATResultSet.IATResultSetList();
            resultSetList.ResultSets = new IATResultSet.IATResultSet[ResultData.IATResults.NumResultSets];

            int ItemCtr = 0;
            for (int ctr1 = 0; ctr1 < ResultData.IATResults.NumResultSets; ctr1++)
            {
                resultSetList.ResultSets[ctr1].ResultSetElements = new IATResultSet.IATResultSetElement[itemNums.Count];
                for (int ctr2 = 0; ctr2 < itemNums.Count; ctr2++)
                {
                    if (ResultData.IATResults.ResultSets[ctr1].ResultSetElements[ctr2].ItemNumber == itemNums[ctr2])
                    {
                        resultSetList.ResultSets[ctr1].ResultSetElements[ItemCtr++] = ResultData.IATResults.ResultSets[ctr1].ResultSetElements[ctr2];
                        break;
                    }
                }
                resultSetList.ResultSets[ctr1].Score();
            }

            return resultSetList;
        }

        protected int CountScoredItems(IATResultSet.IATResultSetList ResultSetList)
        {
            int nScoredItems = 0;
            for (int ctr = 0; ctr < ResultData.IATConfiguration.EventList.NumEvents; ctr++)
            {
                if (ResultData.IATConfiguration.EventList.IATEvent[ctr].EventType == (int)XmlPackaging.IATEvent.EType.IATItem)
                {
                    int nBlock = ((XmlPackaging.IATItem)ResultData.IATConfiguration.EventList.IATEvent[ctr]).BlockNum;
                    if ((nBlock == 3) || (nBlock == 4) || (nBlock == 6) || (nBlock == 7))
                        nScoredItems++;
                }
            }

            return nScoredItems;
        }

        protected bool IncrementCounterList(List<int> CtrList, int MaxCtrVal)
        {
            for (int ctr = CtrList.Count - 1; ctr >= 0; ctr--)
            {
                if (CtrList[ctr] < MaxCtrVal - (CtrList.Count - ctr))
                {
                    CtrList[ctr]++;
                    return true;
                }
            }
            return true;
        }

                

        protected List<IATResultSet.IATResultSetList> PermuteResultSets(int nMinSize)
        {
            List<IATResultSet.IATResultSetList> results = new List<IATResultSet.IATResultSetList>();
            List<int> ctrList = new List<int>();
            for (int ctr = 0; ctr < nMinSize; ctr++)
                ctrList.Add(ctr);

            int nScoredItems = CountScoredItems(ResultData.IATResults);
            //for (int ctr = nMinSize; ctr <= nScoredItems; ctr++)
            return null;
             
        }


        public CItemAnalysis(CResultData resultData, CIAT iat)
        {
            ResultData = resultData;
            IAT = iat;
        }
         */
    }
}
