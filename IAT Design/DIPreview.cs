using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Windows.Forms;
using IATClient.Images;

namespace IATClient
{
    public class DIPreview : DIComposite
    {
        private IImageDisplay _ThumbnailPreviewPane = null;
        private bool ForcedOneShot { get; set; } = false;
        private ManualResetEvent JpegGenerated = new ManualResetEvent(true);
        public Action<Image> OnJpegGenerated = null;

        protected ConcurrentDictionary<LayoutItem, IUri> PreviewComponents = new ConcurrentDictionary<LayoutItem, IUri>();
        private Bitmap Thumbnail = null;

        public IImageDisplay ThumbnailPreviewPane
        {
            get
            {
                return _ThumbnailPreviewPane;
            }
            set
            {
                if (value == null)
                {
                    _ThumbnailPreviewPane = value;
                    if (IImage == null)
                        return;
                    if (IImage.Thumbnail == null)
                        return;
                    IImage.Thumbnail.ClearChanged();
                    return;
                }
                _ThumbnailPreviewPane = value;
                if (IImage == null)
                    return;
                if (IImage.Thumbnail == null)
                    IImage.CreateThumbnail();
                else
                    this.IImage.Thumbnail.ClearChanged();
                IImage.Thumbnail.Changed += new Action<Images.ImageEvent, Images.IImageMedia, object>(OnThumbnailUpdate);
                ForceOneShot();
            }
        }

        private void OnThumbnailUpdate(Images.ImageEvent evt, Images.IImageMedia img, object arg)
        {
            if ((evt == Images.ImageEvent.Updated) || (evt == Images.ImageEvent.Initialized))
            {

                if (!ThumbnailPreviewPane.IsHandleCreated)
                    ThumbnailPreviewPane.HandleCreated += (sender, args) => { ThumbnailPreviewPane.SetImage(img); };
                if (_ThumbnailPreviewPane.IsDisposed)
                {
                    img.ClearChanged();
                    return;
                }
                ThumbnailPreviewPane.SetImage(img);
            }
        }

        private void ForceOneShot()
        {
            ForcedOneShot = true;
            ResumeLayout(true);
        }

        public DIPreview()
        {
        }

        public DIPreview(Uri uri) : base(uri)
        {
        }

        public DIPreview(List<Tuple<IUri, LayoutItem>> components)
        {
            foreach (var tup in components)
            {
                PreviewComponents[tup.Item2] = tup.Item1;
                CIAT.SaveFile.GetDI(tup.Item1.Value).AddOwner(URI);
                if (tup.Item1.IsObservable) 
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(ObservableUri), URI, tup.Item1.URI);
            }
        }

        public void AddComponent(IUri component, LayoutItem lComp)
        {
            PreviewComponents[lComp] = component;
            if (component.IsObservable)
                CIAT.SaveFile.GetObservableUri(component.URI).AddOwner(this);
            else
                CIAT.SaveFile.GetDI(component.Value).AddOwner(URI);
            if (!LayoutSuspended)
                ScheduleInvalidation();
        }

        public void AddComponent(IUri lambdaUri)
        {
            PreviewComponents[LayoutItem.Lambda] = lambdaUri;
            CIAT.SaveFile.GetDI(lambdaUri.Value).AddOwner(URI);
            if (!LayoutSuspended)
                ScheduleInvalidation();
        }

        public void RemoveComponent(LayoutItem pos, bool invalidate)
        {
            if (PreviewComponents.TryRemove(pos, out IUri iUri))
            {
                CIAT.SaveFile.GetDI(iUri.Value)?.ReleaseOwner(URI);
                iUri.Dispose();
                if (invalidate && !LayoutSuspended)
                    ScheduleInvalidation();
            }
            Modified = true;
        }


        public override bool ComponentsExist
        {
            get
            {
                return PreviewComponents.Values.All(iUri => CIAT.SaveFile.PartExists(iUri.Value));
            }
        }

        public override bool ComponentsStale
        {
            get
            {
                return PreviewComponents.Values.All(iUri => !CIAT.SaveFile.GetDI(iUri.Value).ComponentStale);
            }
        }

        protected override void OnImageEvent(ImageEvent evt, IImageMedia img, object arg)
        {
            base.OnImageEvent(evt, img, arg);
            if (evt == Images.ImageEvent.Updated)
            {
                if (ThumbnailPreviewPane != null)
                    CIAT.ImageManager.GenerateThumb(IImage);
            }
            if (OnJpegGenerated != null)
                OnJpegGenerated(img.Img);
            ForcedOneShot = false;
        }


        protected override Bitmap Generate()
        {
            if (IsDisposed || Broken)
                return null;
            if ((PreviewPanel == null) && (OnJpegGenerated == null) && (!ForcedOneShot))
                return null;
            var bSz = BoundingSize;
            var componentList = PreviewComponents.Keys.ToList();
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FullWindow);
            using (Graphics g = Graphics.FromImage(bmp))
            {
                SolidBrush backBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                g.FillRectangle(backBrush, 0, 0, bSz.Width, bSz.Height);
                backBrush.Dispose();
                foreach (var component in componentList)
                {
                    if (!PreviewComponents.TryGetValue(component, out IUri iUri))
                        continue;
                    DIBase di = CIAT.SaveFile.GetDI(iUri.Value);
                    Image img = di.IImage?.Img;
                    while (img == null)
                    {
                        di.ScheduleInvalidationSync();
                        di.InvalidationEvent.Wait();
                        return null;
                    }
                    Rectangle diRect = component.BoundingRectangle;
                    g.DrawImage(img, diRect.Location);
                    CIAT.ImageManager.ReleaseImage(img);
                }
            }
            return bmp;
        }

        public Image SaveToJpeg()
        {
            JpegGenerated.Reset();
            object lockObj = new object();
            Image jpg = null;
            OnJpegGenerated = (img) =>
            {
                if (!Monitor.TryEnter(lockObj))
                {
                    img.Dispose();
                    return;
                }
                jpg = new Bitmap(ImageMediaType.FullPreview.ImageSize.Width, ImageMediaType.FullPreview.ImageSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                var backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                using (Graphics gr = Graphics.FromImage(img))
                {
                    gr.FillRectangle(backBr, new Rectangle(0, 0, ImageMediaType.FullPreview.ImageSize.Width, ImageMediaType.FullPreview.ImageSize.Height));
                    gr.SmoothingMode = SmoothingMode.HighQuality;
                    gr.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    gr.PixelOffsetMode = PixelOffsetMode.HighQuality;
                    gr.DrawImage(img, new Rectangle(0, 0, jpg.Width, jpg.Height));
                }
                OnJpegGenerated = null;
                JpegGenerated.Set();
                img.Dispose();
                Monitor.Exit(lockObj);
            };
            JpegGenerated.WaitOne();
            return jpg;
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("DIPreview", new XAttribute("Type", Type.ToString())));
            var components = PreviewComponents.Select(kv => new Tuple<IUri, LayoutItem>(kv.Value, kv.Key)).ToList();
            foreach (var component in components) {
                DIBase di = CIAT.SaveFile.GetDI(component.Item1.Value);
                if (di.Type != DIType.LambdaGenerated)
                {
                    if (!component.Item1.IsObservable)
                    {
                        String rId = CIAT.SaveFile.CreateRelationship(BaseType, di.BaseType, URI, di.URI);
                        xDoc.Root.Add(new XElement("ComponentDI", new XAttribute("rId", rId), new XAttribute("Observed", Boolean.FalseString),
                                component.Item2.ToString()));
                    }
                    else
                    {
                        String rId = CIAT.SaveFile.CreateRelationship(BaseType, typeof(ObservableUri), URI, component.Item1.URI);
                        xDoc.Root.Add(new XElement("ComponentDI", new XAttribute("rId", rId), new XAttribute("Observed", Boolean.TrueString),
                            component.Item2.ToString()));
                    }
                }
            }
            Stream s = Stream.Synchronized(CIAT.SaveFile.GetWriteStream(this));
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
            Modified = false;
        }


        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);         

            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            foreach (XElement elem in xDoc.Document.Root.Elements("ComponentDI"))
            {
                
                Uri diUri = CIAT.SaveFile.GetRelationship(this, elem.Attribute("rId").Value).TargetUri;
                var li = LayoutItem.FromString(elem.Value);
                if (Convert.ToBoolean(elem.Attribute("Observed").Value))
                    PreviewComponents[li] = new UriObserver(CIAT.SaveFile.GetObservableUri(diUri));
                else
                    PreviewComponents[li] = CIAT.SaveFile.GetDI(diUri).IUri;
            }
            Modified = false;
            SuspendLayout();
        }

        public override void ResumeLayout(bool immediate)
        {
            base.ResumeLayout(true);
        }

        public override object Clone()
        {
            DIPreview o = new DIPreview();
            var components = PreviewComponents.Select(kv => new Tuple<IUri, LayoutItem>(kv.Value, kv.Key));
            foreach (var component in components)
                o.PreviewComponents[component.Item2] = component.Item1;
            o.IImage = IImage.Clone() as IImage;
            o.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, o.IImage.URI);
            return o;
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            PreviewPanel = null;
            ThumbnailPreviewPane = null;
            foreach (var iUri in PreviewComponents.Values)
                CIAT.SaveFile.GetDI(iUri.Value).ReleaseOwner(URI);
            PreviewComponents.Clear();
            base.Dispose();
            IsDisposed = true;
        }
    }
}
