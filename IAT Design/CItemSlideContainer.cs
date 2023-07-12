using IATClient.Messages;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace IATClient
{
    public class CItemSlideContainer : IDisposable
    {
        private static Size MaxThumbSize = new Size(112, 112), MaxDisplaySize = new Size(500, 500);
        public Dictionary<int, CItemSlide> SlideDictionary { get; private set; } = new Dictionary<int, CItemSlide>();
        public Size DisplaySize { get ; private set; }  
        public readonly Size ThumbnailSize = new Size(96, 96);
        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();
        private ResultData.ResultData ResultData;
        private List<byte[]> SlideData { get; set; }
        public Manifest SlideManifest { get; private set; }

        public void SaveItemSlides(String path, String iatName)
        {
            foreach (int i in SlideDictionary.Keys)
            {
                String filename = path + System.IO.Path.DirectorySeparatorChar + iatName + "_Slide" + i.ToString();
                SlideDictionary[i].Save(filename);
            }
        }

        public CItemSlideContainer(ResultData.ResultData rd, List<byte[]> slideData, List<ManifestFile> mfList)
        {
            try
            {
                ResultData = rd;
                SlideData = slideData;
                List<CItemSlide> slideList = new List<CItemSlide>();
                int ctr = 0;
                SlideManifest = new Manifest();
                foreach (var file in mfList)
                {
                    SlideManifest.AddFile(file);
                    var slide = new CItemSlide();
                    slide.ResourceId = file.ResourceId;
                    slide.ReferenceIds.AddRange(file.ReferenceIds);
                    slide.ImageDataSize = file.Size;
                    slide.SetResultData(ResultData, mfList.IndexOf(file));
                    var ms = new MemoryStream(slideData[ctr++]);
                    ms.Dispose();
                    SlideDictionary[slide.ResourceId] = slide;
                }
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        public List<Image> GetSlideImages()
        {
            List<Image> slides = new List<Image>();
            foreach (int i in SlideDictionary.Keys)
                slides.Add(SlideDictionary[i].DisplayImage);
            return slides;
        }

        public void SetResultData(ResultData.ResultData rData)
        {
            foreach (int i in SlideDictionary.Keys)
                SlideDictionary[i].SetResultData(rData, i);
        }

        public List<long> GetSlideLatencies(int itemNum)
        {
            return ResultData.IATResults.Aggregate(new List<IIATItemResponse>(),
                (a, b) => a.Concat(b.IATResponse.Where(a => a.ItemNumber == itemNum)).ToList(),
                a => a.Select(a => a.ResponseTime)).ToList();
        }

        public double GetMeanSlideLatency(int nSlide)
        {
            return SlideDictionary[nSlide].MeanLatency;
        }

        public double GetMeanNumErrors(int nSlide)
        {
            return SlideDictionary[nSlide].MeanNumErrors;
        }

        public Image GetSizedImage(Image full, Size imgSize)
        {
            Bitmap di = new Bitmap(imgSize.Width, imgSize.Height);
            double arImg = (double)full.Width / (double)full.Height;
            double arDisplay = (double)imgSize.Width / (double)imgSize.Height;
            Size sz;
            if (arImg >= arDisplay)
            {
                sz = new Size(imgSize.Width, (int)(imgSize.Width / arDisplay));
            }
            else
            {
                sz = new Size((int)(imgSize.Height / arDisplay), imgSize.Height);
            }
            Point pt = new Point(imgSize.Width - sz.Width >> 1, imgSize.Height - sz.Height >> 1);
            using (Graphics g = Graphics.FromImage(di))
            {
                g.FillRectangle(Brushes.Transparent, new Rectangle(0, 0, imgSize.Width, imgSize.Height));
                g.SmoothingMode = SmoothingMode.HighQuality;
                g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.DrawImage(full, new Rectangle(pt, sz), new Rectangle(0, 0, full.Width, full.Height), GraphicsUnit.Pixel);
            }
            return di;
        }

        public void RequestDisplayImage(int ndx, ManualResetEvent evt = null)
        {
            Task.Run(() =>
            {
                var slide = SlideDictionary[ndx + 1];
                slide.ImageRetrievedEvent.WaitOne();
                Image full = slide.FullSizedImage;
                var di = GetSizedImage(full, DisplaySize);
                slide.DisplayImage = di.Clone() as Image;
                di.Dispose();
                evt?.Set();
            }, CancellationSource.Token);
        }

        public int NumSlides
        {
            get
            {
                return SlideDictionary.Count;
            }
        }


        public void Dispose()
        {
            CancellationSource.Cancel();
            foreach (int ndx in SlideDictionary.Keys)
                SlideDictionary[ndx].Dispose();
        }


        public void ProcessSlidesSync()
        {
            try {
                Rectangle thumbImageDestRect = Rectangle.Empty;
                Rectangle displayImageDestRect = Rectangle.Empty;
                var slideTuples = SlideDictionary.Values.Select(s => new { s.ReferenceIds, resourceId = s.ResourceId }).OrderBy(a => a.resourceId);
                //                List<ManifestFile> files = SlideManifest.Contents.Where(fe => (fe as ManifestFile).ResourceType == ManifestFile.EResourceType.ItemSlide)
                //                  .Cast<ManifestFile>().Select()
                foreach (var entry in slideTuples)
                {
                    var memStream = new MemoryStream(SlideData[entry.resourceId - 1]);
                    var fullSizedImage = Image.FromStream(memStream);
                    memStream.Dispose();
                    if (entry.resourceId == 1)
                    {
                        double fullAr = (double)fullSizedImage.Width / (double)fullSizedImage.Height;
                        double displayAr = (double)MaxDisplaySize.Width / (double)MaxDisplaySize.Height;
                        double thumbAr = (double)ThumbnailSize.Width / (double)ThumbnailSize.Height;
                        if (fullAr > displayAr)
                        {
                            DisplaySize = new Size((int)(MaxDisplaySize.Height * fullAr), MaxDisplaySize.Height);
                            displayImageDestRect = new Rectangle(new Point(0, 0), DisplaySize);
                        }
                        else
                        {
                            DisplaySize = new Size(MaxDisplaySize.Width, (int)(MaxDisplaySize.Width / fullAr));
                            displayImageDestRect = new Rectangle(new Point(0, 0), DisplaySize);
                        }
                        if (fullAr > thumbAr)
                        {
                            Size thumbSize = new Size((int)(ThumbnailSize.Height * fullAr), ThumbnailSize.Height);
                            thumbImageDestRect = new Rectangle(new Point((ThumbnailSize.Width - thumbSize.Width) >> 1, 0), thumbSize);
                        }
                        else
                        {
                            Size thumbSize = new Size(ThumbnailSize.Width, (int)(ThumbnailSize.Width / fullAr));
                            thumbImageDestRect = new Rectangle(new Point(0, (ThumbnailSize.Height - thumbSize.Height) >> 1), thumbSize);
                        }
                    }
                    SlideDictionary[entry.resourceId].FullSizedImage = fullSizedImage;
                    SlideDictionary[entry.resourceId].DisplayImage = GetSizedImage(fullSizedImage, displayImageDestRect.Size); 
                    SlideDictionary[entry.resourceId].ThumbnailImage = GetSizedImage(fullSizedImage, new Size(96, 96)); 
                }
            }
            catch (Exception ex) {
                throw ex;
            }
        }
        public void ProcessSlides()
        {
            Task.Run(() => ProcessSlidesSync());
        }
    }
}
