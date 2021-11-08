using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using System.IO;
using IATClient.IATResultSetNamespaceV2;

namespace IATClient
{
    public class CItemSlide : IDisposable
    {
        private Dictionary<Control, Delegate> DisplaySizedRequesters = new Dictionary<Control, Delegate>();
        private Dictionary<Control, Delegate> ThumbnailSizedRequesters = new Dictionary<Control, Delegate>();
        private Image FullSizedImage = null, _DisplayImage = null, _ThumbnailImage = null;
        private bool _WaitingForFull = true, _WaitingForThumb = true, _WaitingForDisplayImage = true;
        private List<Size> MiscSizeList = new List<Size>();
        private List<Control> MiscControlList = new List<Control>();
        private List<Delegate> MiscDelegateList = new List<Delegate>();
        private object FullImageLock = new object(), ThumbnailLock = new object(), MiscImagesStateLock = new object(), DisplayImageLock = new object();
        private object FullSizedDictionaryLock = new object(), MediumSizedDictionaryLock = new object(), ThumbnailDictionaryLock = new object(), DisplaySizeDictionaryLock = new object();
        private Dictionary<int, List<long>> SubjectLatencies = new Dictionary<int, List<long>>();
        private double _MeanLatency = Double.NaN;
        private double _MeanNumErrors = Double.NaN;

        public double MeanLatency
        {
            get
            {
                return _MeanLatency;
            }
        }

        public double MeanNumErrors
        {
            get
            {
                return _MeanNumErrors;
            }
        }

        public Image DisplayImage
        {
            get
            {
                lock (DisplayImageLock)
                {
                    if (_DisplayImage == null)
                        return null;
                    return new Bitmap(_DisplayImage);
                }
            }
        }

        public Image ThumbnailImage
        {
            get
            {
                lock (ThumbnailLock)
                {
                    if (_ThumbnailImage == null)
                        return null;
                    return new Bitmap(_ThumbnailImage);
                }
            }
        }


        public bool WaitingForFull
        {
            get
            {
                return _WaitingForFull;
            }
        }

        public bool WaitingForThumb
        {
            get
            {
                return _WaitingForThumb;
            }
        }

        public bool WaitingForDisplayImage
        {
            get
            {
                return _WaitingForDisplayImage;
            }
        }

        public void AddDisplayImageRequest(Control c, Delegate d)
        {
            lock (FullSizedDictionaryLock)
            {
                DisplaySizedRequesters[c] = d;
            }
        }

        public List<long> GetSubjectLatencies(int subjNum)
        {
            return SubjectLatencies[subjNum];
        }

        public void AddMiscRequest(Control c, Delegate d, Size sz)
        {
            lock (MiscImagesStateLock)
            {
                MiscControlList.Add(c);
                MiscDelegateList.Add(d);
                MiscSizeList.Add(sz);
            }
        }

        public void AddThumbnailRequest(Control c, Delegate d)
        {
            lock (ThumbnailDictionaryLock)
            {
                ThumbnailSizedRequesters[c] = d;
            }
        }

        public bool HasMisc
        {
            get {
                bool FullImageAvailable = !WaitingForFull;
                lock (MiscImagesStateLock)
                {
                    return ((MiscControlList.Count > 0) && FullImageAvailable);
                }
            }
        }

        public void ProcessNextMisc()
        {
            Control c;
            Delegate d;
            Size sz;

            lock (MiscImagesStateLock)
            {
                c = MiscControlList[0];
                d = MiscDelegateList[0];
                sz = MiscSizeList[0];
            }

            if (!Monitor.TryEnter(FullImageLock))
                return;
            c.BeginInvoke(d, new Bitmap(FullSizedImage, sz));
            Monitor.Exit(FullImageLock);

            lock (MiscImagesStateLock)
            {
                MiscControlList.RemoveAt(0);
                MiscDelegateList.RemoveAt(0);
                MiscSizeList.RemoveAt(0);
            }
        }

        public void SizeThumbnail(Size sz)
        {
            lock (ThumbnailLock)
            {
                if (!Monitor.TryEnter(FullImageLock))
                    return;
                _ThumbnailImage = new Bitmap(FullSizedImage, sz);
                Monitor.Exit(FullImageLock);
                _WaitingForThumb = false;
            }
        }

        public void SizeDisplayImage(Size sz)
        {
            lock (DisplayImageLock)
            {
                if (!Monitor.TryEnter(DisplayImageLock))
                    return;
                _DisplayImage = new Bitmap(FullSizedImage, sz);
                Monitor.Exit(DisplayImageLock);
                _WaitingForDisplayImage = false;
            }
        }

        public ICollection GetDisplaySizedRequesters()
        {
            List<Control> result = new List<Control>();
            lock (DisplaySizeDictionaryLock)
            {
                foreach (Control c in DisplaySizedRequesters.Keys)
                    result.Add(c);
            }
            return result;
        }

        public Delegate GetDisplaySizedRequestDelegate(Control c)
        {
            Delegate d = null;
            lock (DisplaySizeDictionaryLock)
            {
                d = DisplaySizedRequesters[c];
                DisplaySizedRequesters.Remove(c);
            }
            return d;
        }

        public ICollection GetThumbnailRequestKeys()
        {
            List<Control> result = new List<Control>();
            lock (ThumbnailDictionaryLock)
            {
                foreach (Control c in ThumbnailSizedRequesters.Keys)
                    result.Add(c);
            }
            return result;
        }

        public Delegate GetThumbnailRequestDelegate(Control c)
        {
            Delegate d = null;
            lock (ThumbnailDictionaryLock)
            {
                d = ThumbnailSizedRequesters[c];
                ThumbnailSizedRequesters.Remove(c);
            }
            return d;
        }

        public CItemSlide()
        {
        }

        public void SetResultData(CResultData rData, int slideNum)
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
            _MeanLatency = (double)latencySum / (double)nOccurs;
            _MeanNumErrors = (double)errorSum / (double)nOccurs;
        }

        public void SetImage(String filename, byte[] imgData)
        {
            MemoryStream memStream = new MemoryStream(imgData);
            lock (FullImageLock)
            {
                FullSizedImage = Bitmap.FromStream(memStream);
            }
            _WaitingForFull = false;
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
