using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;

namespace IATClient
{
    public class CItemSlideContainer : IDisposable
    {
        private static Size MaxThumbSize = new Size(112, 112), MaxDisplaySize = new Size(500, 500);
        private Size _ThumbSize, _DisplaySize;
        private Dictionary<int, CItemSlide> SlideDictionary = new Dictionary<int, CItemSlide>();
        private CItemSlideRetriever SlideRetriever;
        public Messages.ItemSlideManifest SlideManifest { get; private set; }
        private String _IATName, _Password;
        private enum EThreadState { unstarted, restarting, running, paused, disposing, disposed };
        private EThreadState _UpdateProcState = EThreadState.unstarted, _ResizeProcState = EThreadState.unstarted;
        private int NumZeroUpdateLoops = 0, NumZeroResizeLoops = 0;
        private int NumItemSlideFiles = 0, NumItemSlideFilesRetrieved = 0;
        private ManualResetEvent ResizeHalt, UpdateHalt, SlidesProcessed = null;

        private IATConfigMainForm MainForm
        {
            get
            {
                return (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            }
        }

        public Size ThumbSize
        {
            get
            {
                return _ThumbSize;
            }
        }

        public Size DisplaySize
        {
            get
            {
                return _DisplaySize;
            }
        }

        private EThreadState UpdateProcState
        {
            get
            {
                return _UpdateProcState;
            }
            set
            {
                _UpdateProcState = value;
            }
        }

        private EThreadState ResizeProcState
        {
            get
            {
                return _ResizeProcState;
            }
            set
            {
                _ResizeProcState = value;
            }
        }

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

        public CItemSlideContainer(String iatName, String dataPassword, IATConfig.ConfigFile CF)
        {
            ResizeHalt = new ManualResetEvent(false);
            UpdateHalt = new ManualResetEvent(false);
            _IATName = iatName;
            _Password = dataPassword;
            var pairings = from item in CF.EventList.OfType<IATConfig.IATItem>() select new { key = CIAT.SaveFile.IAT.Blocks[item.BlockNum - 1].Key, item.BlockNum, stimID = item.StimulusDisplayID };
            var blocks = (from pairing in pairings select pairing.key).Distinct()
            SlideManifest = new Messages.ItemSlideManifest();
            SlideManifest.ItemSlideEntries = new TItemSlideEntry[filenames.Count()];
            int ctr = 0;
            foreach (String filename in filenames)
            {
                _SlideManifest.ItemSlideEntries[ctr] = new TItemSlideEntry();
                _SlideManifest.ItemSlideEntries[ctr].SlideFileName = filename;
                var itemNums = (from p in pairings where p.filename == filename select p.itemNum).AsEnumerable();
                _SlideManifest.ItemSlideEntries[ctr++].Items = itemNums.Cast<uint>().AsEnumerable<uint>().ToArray<uint>();
                foreach (uint iNum in itemNums)
                    SlideDictionary[(int)iNum] = new CItemSlide();
            }
            if (CF.Layout.InteriorWidth == CF.Layout.InteriorHeight)
            {
                _ThumbSize = MaxThumbSize;
                _DisplaySize = MaxDisplaySize;
            }
            else
            {
                double ar = (double)CF.Layout.InteriorWidth / (double)CF.Layout.InteriorHeight;
                if (ar > 1)
                {
                    _ThumbSize = new Size(MaxThumbSize.Width, (int)((double)MaxThumbSize.Height / ar));
                    _DisplaySize = new Size(MaxDisplaySize.Width, (int)((double)MaxDisplaySize.Height / ar));
                }
                else
                {
                    _ThumbSize = new Size((int)((double)MaxThumbSize.Width * ar), MaxThumbSize.Height);
                    _DisplaySize = new Size((int)((double)MaxDisplaySize.Width * ar), MaxDisplaySize.Height);
                }
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

        public List<long> GetSlideLatencies(int nSlide, int nResultSet)
        {
            return SlideDictionary[nSlide].GetSubjectLatencies(nResultSet);
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

        private bool SetSlideImage(String filename, byte[] imageData)
        {
            try
            {
                uint[] IDs = SlideManifest.GetItemIDs(filename);
                for (int ctr = 0; ctr < IDs.Length; ctr++)
                    SlideDictionary[(int)IDs[ctr]].SetImage(String.Format("Item {0}", IDs[ctr]), imageData);
                NumItemSlideFilesRetrieved++;
                if (NumItemSlideFilesRetrieved == NumItemSlideFiles)
                {
                    ResizeProcState = EThreadState.disposing;
                    SlidesProcessed.Set();
                }
                return true;
            }
            catch (Exception ex)
            {
                ErrorReporter.ReportError(new CReportableException("Error displaying item slides", ex));
                return false;
            }
        }

        public Image GetSlideImage(String filename)
        {
            var ndx = from sme in SlideManifest.ItemSlideEntries where sme.SlideFileName == filename select sme.Items.First();
            return SlideDictionary[(int)ndx.First()].DisplayImage;
        }

        public int NumSlides
        {
            get
            {
                return SlideDictionary.Count;
            }
        }

        public CItemSlide this[int key]
        {
            get
            {
                return SlideDictionary[key];
            }
        }

        public void Clear()
        {
            UpdateProcState = EThreadState.disposing;
            ResizeProcState = EThreadState.disposing;
            WaitHandle[] States = { UpdateHalt, ResizeHalt };
            WaitHandle.WaitAll(States);
            foreach (int ndx in SlideDictionary.Keys)
                SlideDictionary[ndx].Dispose();
            SlideDictionary.Clear();
            UpdateProcState = EThreadState.unstarted;
            ResizeProcState = EThreadState.unstarted;
        }

        public void StopWorkers()
        {
            UpdateProcState = EThreadState.disposing;
            ResizeProcState = EThreadState.disposing;
            WaitHandle[] States = { UpdateHalt, ResizeHalt };
            WaitHandle.WaitAll(States);
            UpdateProcState = EThreadState.unstarted;
            ResizeProcState = EThreadState.unstarted;
        }


        private bool StartUpdateProc()
        {
            if (UpdateProcState == EThreadState.running)
                return true;
            if ((UpdateProcState != EThreadState.paused) && (UpdateProcState != EThreadState.unstarted))
                return false;
            UpdateHalt.Reset();
            UpdateProcState = EThreadState.unstarted;
            Action proc = new Action(UpdateProc);
            proc.BeginInvoke(new AsyncCallback(UpdateLoopCallback), proc);
            return true;
        }

        private void StartResizeProc()
        {
            if (ResizeProcState == EThreadState.running)
                return;
            ResizeHalt.Reset();
            ResizeProcState = EThreadState.unstarted;
            Action proc = new Action(ResizeProc);
            ResizeProcState = EThreadState.running;
            proc.BeginInvoke(new AsyncCallback(ResizeLoopCallback), proc);
            return;
        }

        public void StartRetrieval()
        {
            if ((UpdateProcState != EThreadState.unstarted) || (ResizeProcState != EThreadState.unstarted))
                StopWorkers();
            StartUpdateProc();
            StartResizeProc();
            IATConfigMainForm mainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
            SlideRetriever = new CItemSlideRetriever(IATName, Password, new Func<String, byte[], bool>(SetSlideImage));
            NumItemSlideFiles = SlideManifest.ItemSlideEntries.Length;
            NumItemSlideFilesRetrieved = 0;
            SlidesProcessed = new ManualResetEvent(false);
            SlideRetriever.RetrieveItemSlides(SlidesProcessed);
            SlidesProcessed.WaitOne();
        }
        
        public void Dispose()
        {
            StopWorkers();
            SlidesProcessed.Set();
            SlideRetriever.Abort(this, new EventArgs());
            foreach (int ndx in SlideDictionary.Keys)
                SlideDictionary[ndx].Dispose();
        }
        
        public bool RequestFullImage(int ndx, Control c, Delegate d)
        {
            try
            {
                SlideDictionary[ndx].AddDisplayImageRequest(c, d);
                if (UpdateProcState == EThreadState.paused)
                    StartUpdateProc();
                if ((ResizeProcState == EThreadState.paused) || (ResizeProcState == EThreadState.disposing))
                    StartResizeProc();
                return true;
            }
            catch (KeyNotFoundException ex)
            {
                return false;
            }
        }

        public bool AddMiscRequest(int ndx, Control c, Delegate d, Size sz)
        {
            try
            {
                SlideDictionary[ndx].AddMiscRequest(c, d, sz);
                if (UpdateProcState == EThreadState.paused)
                    StartUpdateProc();
                if (ResizeProcState != EThreadState.paused)
                    StartResizeProc();
                return true;
            }
            catch (KeyNotFoundException ex)
            {
                return false;
            }

        }

        public bool RequestThumbnailImage(int ndx, Control c, Delegate d)
        {
            try
            {
                SlideDictionary[ndx].AddThumbnailRequest(c, d);
                if (UpdateProcState == EThreadState.paused)
                    StartUpdateProc();
                if (ResizeProcState == EThreadState.paused)
                    StartResizeProc();
                return true;
            }
            catch (KeyNotFoundException ex)
            {
                return false;
            }
        }

        private void UpdateLoopCallback(IAsyncResult async)
        {
            Action caller = (Action)async.AsyncState;
            caller.EndInvoke(async);
            if ((UpdateProcState == EThreadState.paused) || (UpdateProcState == EThreadState.disposed))
            {
                UpdateHalt.Set();
                return;
            }
            if (UpdateProcState == EThreadState.disposing)
                UpdateProcState = EThreadState.disposed;
            Action proc = new Action(UpdateProc);
            proc.BeginInvoke(new AsyncCallback(UpdateLoopCallback), proc);
        }

        private void UpdateProc()
        {
            int NumImagesProcessed = 0;

            foreach (CItemSlide slide in SlideDictionary.Values)
            {
                ICollection keys = slide.GetDisplaySizedRequesters();
                int nKeys = keys.Count;
                if (nKeys > 0)
                {
                    if (!slide.WaitingForDisplayImage)
                    {
                        Image i = slide.DisplayImage;
                        if (i != null)
                        {
                            NumImagesProcessed++;
                            foreach (Control c in keys)
                            {
                                Delegate d = slide.GetDisplaySizedRequestDelegate(c);
                                if (--nKeys != 0)
                                    c.BeginInvoke(d, new Bitmap(i));
                                else
                                    c.BeginInvoke(d, i);
                            }
                        }
                    }
                }
                keys = slide.GetThumbnailRequestKeys();
                nKeys = keys.Count;
                if (nKeys > 0)
                {

                    if (!slide.WaitingForThumb)
                    {
                        Image i = slide.ThumbnailImage;
                        if (i != null)
                        {
                            NumImagesProcessed++;
                            foreach (Control c in keys)
                            {
                                Delegate d = slide.GetThumbnailRequestDelegate(c);
                                if (--nKeys != 0)
                                    c.BeginInvoke(d, new Bitmap(i));
                                else
                                    c.BeginInvoke(d, i);
                            }
                        }
                    }
                }
            }
            if (NumImagesProcessed == 0)
            {
                NumZeroUpdateLoops++;
                if (NumZeroUpdateLoops >= 120)
                    UpdateProcState = EThreadState.disposed;
                else if (NumZeroUpdateLoops >= 50)
                    ((Func<Task>)(async () => { await Task.Delay(1000); }))().Wait();
                else if (NumZeroUpdateLoops > 0)
                    ((Func<Task>)(async () => { await Task.Delay(100); }))().Wait();
            }
            else
            {
                NumZeroUpdateLoops = 0;
                Thread.Sleep(10);
            }
        }

        private void ResizeLoopCallback(IAsyncResult async)
        {
            Action caller = (Action)async.AsyncState;
            caller.EndInvoke(async);
            if (ResizeProcState == EThreadState.disposed)
            {
                ResizeHalt.Set();
                return;
            }
            else if (ResizeProcState == EThreadState.disposing)
                ResizeProcState = EThreadState.disposed;
            Action proc = new Action(ResizeProc);
            proc.BeginInvoke(new AsyncCallback(ResizeLoopCallback), proc);
        }

        private void ResizeProc()
        {
            List<CItemSlide> SlideList = new List<CItemSlide>();
            int numImagesProcessed = 0;
            foreach (CItemSlide slide in SlideDictionary.Values)
            {
                if (!slide.WaitingForFull)
                    SlideList.Add(slide);
            }
            foreach (CItemSlide slide in SlideList)
            {
                if (slide.WaitingForDisplayImage)
                {
                    numImagesProcessed++;
                    slide.SizeDisplayImage(DisplaySize);
                }
                if (slide.WaitingForThumb)
                {
                    numImagesProcessed++;
                    slide.SizeThumbnail(ThumbSize);
                }
            }
            foreach (CItemSlide slide in SlideDictionary.Values)
            {
                if (slide.HasMisc)
                {
                    numImagesProcessed++;
                    slide.ProcessNextMisc();
                }
            }
            if (numImagesProcessed == 0)
            {
                NumZeroResizeLoops++;
                if (NumZeroResizeLoops >= 120)
                {
                    ResizeProcState = EThreadState.paused;
                    ResizeHalt.Set();
                }
                else if (NumZeroResizeLoops >= 50)
                    ((Func<Task>)(async () => { await Task.Delay(1000); }))().Wait();
                else if (NumZeroResizeLoops > 0)
                    ((Func<Task>)(async () => { await Task.Delay(100); }))().Wait();
            }
            else
                NumZeroResizeLoops = 0;
        }
    }
}
