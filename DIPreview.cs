using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Drawing;
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
        protected ConcurrentDictionary<LayoutItem, IUri> PreviewComponents = new ConcurrentDictionary<LayoutItem, IUri>();
        private Bitmap Thumbnail = null;

        public new void SuspendLayout()
        {
            base.SuspendLayout();
        }

        public new void ResumeLayout(bool immediate)
        {
            LayoutSuspended = false;
            if (immediate && (ThumbnailPreviewPane != null) && (Thumbnail == null))
                Thumbnail = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.Thumbnail);
            base.ResumeLayout(immediate);
        }

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
                if (!ThumbnailPreviewPane.IsHandleCreated)
                {
                    ThumbnailPreviewPane.HandleCreated += (sender, eArgs) =>
                    {
                        if (!ThumbnailPreviewPane.IsDisposed)
                        {
                            ThumbnailPreviewPane.SetImage(IImage.Thumbnail);
                        }
                        else
                            this.IImage.Thumbnail.ClearChanged();
                    };
                }
                else
                    ThumbnailPreviewPane.SetImage(IImage.Thumbnail);
                this.IImage.Thumbnail.Changed += new Action<Images.ImageChangedEvent, Images.IImageMedia, object>(OnThumbnailUpdate);
            }
        }

        private void OnThumbnailUpdate(Images.ImageChangedEvent evt, Images.IImageMedia img, object arg)
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

        public DIPreview()
        {
            LayoutSuspended = true;
        }

        public DIPreview(Uri uri) : base(uri)
        {
            LayoutSuspended = true;
        }

        public DIPreview(List<Tuple<IUri, LayoutItem>> components)
        {
            LayoutSuspended = true;
            foreach (var tup in components)
            {
                PreviewComponents[tup.Item2] = tup.Item1;
                CIAT.SaveFile.GetDI(tup.Item1.Value).AddOwner(URI);
                if (tup.Item1.IsObservable) 
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(ObservableUri), URI, tup.Item1.URI);
            }
        }

        public override void ScheduleInvalidation()
        {
            if (!LayoutSuspended)
                base.ScheduleInvalidation();
            else
                ImageStale = true;
        }

        public void AddComponent(IUri component, LayoutItem lComp)
        {
            PreviewComponents[lComp] = component;
            if (component.IsObservable)
                CIAT.SaveFile.GetObservableUri(component.URI).AddOwner(this);
            else
                CIAT.SaveFile.GetDI(component.Value).AddOwner(URI);
            ScheduleInvalidation();
        }

        public void AddComponent(IUri lambdaUri)
        {
            PreviewComponents[LayoutItem.Lambda] = lambdaUri;
            CIAT.SaveFile.GetDI(lambdaUri.Value).AddOwner(URI);
            ScheduleInvalidation();
        }

        public void RemoveComponent(LayoutItem pos, bool invalidate)
        {
            if (PreviewComponents.TryRemove(pos, out IUri iUri))
            {
                CIAT.SaveFile.GetDI(iUri.Value)?.ReleaseOwner(URI);
                iUri.Dispose();
                if (invalidate)
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

        public override bool ComponentsValid
        {
            get
            {
                return PreviewComponents.Values.All(iUri => !CIAT.SaveFile.GetDI(iUri.Value).IsValid);
            }
        }

        protected override void OnImageChanged(ImageChangedEvent evt, IImageMedia img, object arg)
        {
            if (IsDisposed)
                return;
            base.OnImageChanged(evt, img, arg);
            if (evt == Images.ImageChangedEvent.Updated)
            {
                if (ThumbnailPreviewPane != null)
                    CIAT.ImageManager.GenerateThumb(IImage);
            }
        }

        protected override bool Generate()
        {
            if (IsDisposed || Broken || LayoutSuspended)
                return true;
            if (PreviewPanel != null)
            {
                if (!PreviewPanel.IsHandleCreated) {
                    PreviewPanel.HandleCreated += (sender, evt) => { Generate(); };
                    return false;
                }
            }
            var bSz = BoundingSize;
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FullWindow);
            try
            {
                Graphics g = Graphics.FromImage(bmp);
                SolidBrush backBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                g.FillRectangle(backBrush, 0, 0, bSz.Width, bSz.Height);
                backBrush.Dispose();
                var componentList = PreviewComponents.Select(kv => new Tuple<IUri, LayoutItem>(kv.Value, kv.Key)).ToList();
                foreach (var component in componentList)
                {
                    DIBase di = CIAT.SaveFile.GetDI(component.Item1.Value);
                    if (di.IImage == null)
                        continue;
                    Image img = di.IImage.Image;
                    if (img == null)
                        continue;
                    Rectangle diRect = component.Item2.BoundingRectangle;
                    g.DrawImage(img, new Point(diRect.Left + ((diRect.Width - img.Width) >> 1), diRect.Top + ((diRect.Height - img.Height) >> 1)));
                    CIAT.ImageManager.ReleaseImage(img);
                }
                g.Dispose();
                if (IImage != null)
                    IImage.Image = bmp;
                else
                {
                    SetImage(bmp, System.Drawing.Imaging.ImageFormat.Png);
                    IImage.CreateThumbnail();
                    if (ThumbnailPreviewPane != null)
                        this.IImage.Thumbnail.Changed += new Action<Images.ImageChangedEvent, Images.IImageMedia, object>(OnThumbnailUpdate);
                }
            }
            catch (Exception ex)
            {
                bmp.Dispose();
                Broken = true;
                IATConfigMainForm.ShowErrorReport("Error generating preview image", new CReportableException(ex.Message, ex));
            }
            return true;
        }

        public MemoryStream SaveToJpeg()
        {
            var bSz = BoundingSize;
            if (IImage != null)
            {
                Image img = IImage.Image;
                Bitmap previewBmp = new Bitmap(img);
                CIAT.ImageManager.ReleaseImage(img);
                System.Drawing.Imaging.BitmapData previewBitmapData = previewBmp.LockBits(new Rectangle(new Point(0, 0), previewBmp.Size), 
                    System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                Int32 backColor = CIAT.SaveFile.Layout.BackColor.ToArgb();
                Int32[] previewData = new Int32[previewBitmapData.Stride * previewBmp.Height >> 2];
                System.Runtime.InteropServices.Marshal.Copy(previewBitmapData.Scan0, previewData, 0, previewBitmapData.Stride * previewBmp.Height >> 2);
                for (int ctr1 = 0; ctr1 < previewBmp.Height; ctr1++)
                    for (int ctr2 = 0; ctr2 < previewBitmapData.Stride >> 2; ctr2++)
                        if ((previewData[ctr2 + (ctr1 * previewBitmapData.Stride >> 2)] & 0xFF000000) == 0)
                            previewData[ctr2 + (ctr1 * previewBitmapData.Stride >> 2)] = backColor;
                previewBmp.UnlockBits(previewBitmapData);
                MemoryStream memStream = new MemoryStream();
                previewBmp.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                previewBmp.Dispose();
                return memStream;
            }
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FullWindow);
            try
            {
                Graphics g = Graphics.FromImage(bmp);
                SolidBrush backBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                g.FillRectangle(backBrush, 0, 0, bSz.Width, bSz.Height);
                backBrush.Dispose();
                var componentList = PreviewComponents.Select(kv => new Tuple<IUri, LayoutItem>(kv.Value, kv.Key)).ToList();
                foreach (var component in componentList)
                {
                    DIBase di = CIAT.SaveFile.GetDI(component.Item1.Value);
                    if (di.IImage == null)
                        continue;
                    Image img = di.IImage.Image;
                    if (img == null)
                        continue;
                    Rectangle diRect = component.Item2.BoundingRectangle;
                    g.DrawImage(img, new Point(diRect.Left + ((diRect.Width - img.Width) >> 1), diRect.Top + ((diRect.Height - img.Height) >> 1)));
                    CIAT.ImageManager.ReleaseImage(img);
                }
                g.Dispose();
                MemoryStream memStream = new MemoryStream();
                bmp.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                CIAT.ImageManager.ReleaseImage(bmp);
                return memStream;
            }
            catch (Exception ex)
            {
                bmp.Dispose();
                Broken = true;
                IATConfigMainForm.ShowErrorReport("Error generating preview image", new CReportableException(ex.Message, ex));
                return null;
            }
        }

        public override void ReleaseDI(Uri uri)
        {
            LayoutItem li = PreviewComponents.Where(kv => kv.Value.Value.Equals(uri)).Select(kv => kv.Key).FirstOrDefault();
            if (li != null)
                PreviewComponents.TryRemove(li, out IUri iuri);
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
                        xDoc.Root.Add(new XElement("ComponentDI", new XAttribute("Observed", component.Item1.IsObservable.ToString()),
                            new XElement("Uri", component.Item1.Value.ToString()), new XElement("Component", component.Item2.ToString())));
                    else
                        xDoc.Root.Add(new XElement("ComponentDI", new XAttribute("Observed", component.Item1.IsObservable.ToString()),
                            new XElement("Uri", component.Item1.URI.ToString()), new XElement("Component", component.Item2.ToString())));
                }
            }
            xDoc.Root.Add(new XElement("Size", new XElement("Width", ItemSize.Width.ToString()), new XElement("Height", ItemSize.Height.ToString())));
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
                Uri diUri;
                LayoutItem li;
                if (elem.Element("Uri") != null) { 
                    diUri = new Uri(elem.Element("Uri").Value, UriKind.Relative);
                    li = LayoutItem.FromString(elem.Element("Component").Value);
                }
                else
                {
                    String rId = elem.Attribute("rId").Value;
                    diUri = CIAT.SaveFile.GetRelationship(this, rId).TargetUri;
                    li = LayoutItem.FromString(elem.Value);
                }

                if (Convert.ToBoolean(elem.Attribute("Observed").Value))
                {
                    var ObserverableUri = CIAT.SaveFile.GetObservableUri(diUri);
                    PreviewComponents[li] = new UriObserver(ObserverableUri);
                }
                else
                {
                    DIBase di = CIAT.SaveFile.GetDI(diUri);
                    PreviewComponents[li] = di.IUri;
                }
            }
            XElement szElem = xDoc.Root.Element("Size");
            ItemSize = new Size(Convert.ToInt32(szElem.Element("Width").Value), Convert.ToInt32(szElem.Element("Height").Value));
            Modified = false;
            SuspendLayout();
        }

        public override object Clone()
        {
            DIPreview o = new DIPreview();
            var components = PreviewComponents.Select(kv => new Tuple<IUri, LayoutItem>(kv.Value, kv.Key));
            foreach (var component in components)
                o.PreviewComponents[component.Item2] = component.Item1;
            o.SetImage(this.IImage.Image, this.IImage.Format);
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
            base.Dispose();
            IsDisposed = true;
        }
    }
}
