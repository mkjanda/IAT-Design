using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Net;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using IATClient.ResultData;

namespace IATClient
{
    public class CItemSlideContainer : IDisposable
    {
        private static Size MaxThumbSize = new Size(112, 112), MaxDisplaySize = new Size(500, 500);
        public ItemSlideData SlideData { get; private set; } = null;
        public Dictionary<int, CItemSlide> SlideDictionary { get; private set; } = new Dictionary<int, CItemSlide>();
        private CItemSlideRetriever SlideRetriever;
        public Messages.ItemSlideManifest SlideManifest { get; private set; }
        private String _IATName, _Password;
        private enum EThreadState { unstarted, restarting, running, paused, disposing, disposed };
        private EThreadState _UpdateProcState = EThreadState.unstarted, _ResizeProcState = EThreadState.unstarted;
        private int NumZeroUpdateLoops = 0, NumZeroResizeLoops = 0;
        private int NumItemSlideFiles = 0, NumItemSlideFilesRetrieved = 0;
        private ManualResetEvent ResizeHalt, UpdateHalt, SlidesProcessed = null;
        private int SlidesRetrieved { get; set; } = 0;
        private int SlidesResized { get; set; } = 0;
        private List<CItemSlide> retrievedSlides = new List<CItemSlide>();
        private readonly ManualResetEvent SlideReceivedEvent = new ManualResetEvent(false);
        public readonly Size DisplaySize, ThumbnailSize;
        private readonly CancellationTokenSource CancellationSource = new CancellationTokenSource();
        private ResultData.ResultData ResultData;
        private String IATName { get; set; }
        private String Password { get; set; }

        public void SaveItemSlides(String path, String iatName)
        {
            foreach (int i in SlideDictionary.Keys)
            {
                String filename = path + System.IO.Path.DirectorySeparatorChar + iatName + "_Slide" + i.ToString();
                SlideDictionary[i].Save(filename);
            }
        }

        public CItemSlideContainer(ItemSlideData slideData, Size displaySize, Size thumbSize, ResultData.ResultData rd)
        {
            ResizeHalt = new ManualResetEvent(false);
            UpdateHalt = new ManualResetEvent(false);
            SlideData = slideData;
            DisplaySize = displaySize;
            ThumbnailSize = thumbSize;
            ResultData = rd;
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
                (a, b) =>a.Concat(b.IATResponse.Where(a => a.ItemNumber == itemNum)).ToList(), 
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

        public bool ItemSlideDownloadComplete
        {
            get
            {
                return NumItemSlideFiles == NumItemSlideFilesRetrieved;
            }
        }

        public Image GetSizedImage(CItemSlide slide, Image full, Size imgSize)
        {
            Bitmap di = new Bitmap(imgSize.Width, imgSize.Height);
            double arImg = (double)slide.FullSizedImage.Width / (double)slide.FullSizedImage.Height;
            double arDisplay = (double)imgSize.Width / (double)imgSize.Height;
            Size sz;
            if (arImg >= arDisplay)
            {
                sz = new Size((int)(imgSize.Height * arImg), imgSize.Height);
            }
            else
            {
                sz = new Size(imgSize.Width, (int)(imgSize.Width / arImg));
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
                var fileRefs = SlideManifest.ResourceReferences;
                var slideNum = fileRefs.Where(fr => fr.ReferenceIds.Contains(ndx)).Select(fr => fileRefs.IndexOf(fr)).First();
                var slide = SlideDictionary[slideNum];
                slide.ImageRetrievedEvent.WaitOne();
                Image full = slide.FullSizedImage;
                var di = GetSizedImage(slide, full, DisplaySize);
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

        public void StartRetrieval()
        {
            Task.Run(() => {
                WebClient client = new WebClient();
                client.Headers["sessionId"] = SlideData.SessionId;
                client.Headers["deploymentId"] = SlideData.DeploymentId;
                byte[] data = client.DownloadData(Properties.Resources.sItemSlideDownloadURL);
                while (SlidesRetrieved < SlideData.Resources.Length)
                {
                    var memStream = new MemoryStream(data);
                    foreach (var r in SlideData.Resources)
                    {
                        var itemSlide = new CItemSlide();
                        itemSlide.FullSizedImage = Image.FromStream(memStream);
                        SlideDictionary[SlidesRetrieved++] = itemSlide;
                        itemSlide.ImageRetrievedEvent.Set();
                        SlideReceivedEvent.Set();
                    }
                } 
            }, CancellationSource.Token);
        }
        
        public void Dispose()
        {
            CancellationSource.Cancel();
            foreach (int ndx in SlideDictionary.Keys)
                SlideDictionary[ndx].Dispose();
        }
        

        public void ProcessSlides()
        {
            Task.Run(() =>
            {
                SlidesResized = 0;
                while (SlideData.Resources.Length > SlidesResized)
                {
                    if (SlidesResized == SlidesRetrieved)
                    {
                        SlideReceivedEvent.Reset();
                        SlideReceivedEvent.WaitOne();
                    }
                    var slide = SlideDictionary[SlidesRetrieved++];
                    Image full = slide.FullSizedImage;
                    var di = slide.DisplayImage = GetSizedImage(slide, full, DisplaySize);
                    slide.DisplayImage = di.Clone() as Image;
                    di.Dispose();
                    di = slide.ThumbnailImage = GetSizedImage(slide, full, ThumbnailSize);
                    slide.ThumbnailImage = di.Clone() as Image;
                    di.Dispose();
                    foreach (var action in slide.ThumbnailRequesters.Values)
                        action(slide.ThumbnailImage);
                }
            }, CancellationSource.Token);
        }

    }
}
