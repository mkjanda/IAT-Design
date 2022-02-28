using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;

namespace IATClient
{
    public class CItemSlide : IDisposable
    {
        public Action<int> FullSizedUpdate { get; set; }
        public Dictionary<int, Action<Image>> ThumbnailRequesters { get; private set; } = new Dictionary<int, Action<Image>>();
        public int ResourceId { get; set; }
        public List<int> ReferenceIds { get; private set; } = new List<int>();
        public ManualResetEvent ImageRetrievedEvent { get; private set; } = new ManualResetEvent(false);
        public Image FullSizedImage { get; set; }
        public Image DisplayImage { get; set; }
        public Image ThumbnailImage { get; set; }
        public double MeanLatency { get; private set; } = Double.NaN;
        public double MeanNumErrors { get; private set; } = Double.NaN;
        public long ImageDataSize { get; set; } = 0;
        public List<List<long>> SubjectLatencies { get; private set; } = new List<List<long>>();
        public readonly object lockObj = new object();


        public CItemSlide()
        {
        }

        public void SetResultData(ResultData.ResultData rData, int slideNum)
        {
            int nOccurs = 0;
            long latencySum = 0;
            int errorSum = 0;
            for (int ctr = 0; ctr < rData.IATResults.NumResultSets; ctr++)
            {
                SubjectLatencies[ctr] = new List<long>();
                for (int ctr2 = 0; ctr2 < rData.IATResults[ctr].IATResponse.NumItems; ctr2++)
                {
                    if (rData.IATResults[ctr].IATResponse[ctr2].ItemNumber == slideNum)
                    {
                        SubjectLatencies[ctr].Add(rData.IATResults[ctr].IATResponse[ctr2].ResponseTime);
                        latencySum += SubjectLatencies[ctr].Last();
                        if (rData.IATResults[ctr].IATResponse[ctr2].Error)
                            errorSum++;
                        nOccurs++;
                    }
                }
            }
            MeanLatency = (double)latencySum / (double)nOccurs;
            MeanNumErrors = (double)errorSum / (double)nOccurs;
        }

        public void Save(String filename)
        {
            FullSizedImage.Save(filename);
        }

        public void Dispose()
        {
            if (FullSizedImage != null)
                FullSizedImage.Dispose();
            if (ThumbnailImage != null)
                ThumbnailImage.Dispose();
        }
    }
}
