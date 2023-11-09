using IATClient.Images;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IATClient
{
    public class DIResponseKeyImage : DIImage, IResponseKeyDI
    {
        public override bool LayoutSuspended { get; protected set; } = false;
        public override void SuspendLayout()
        {
            LayoutSuspended = true;
        }
        public override void ResumeLayout(bool invalidate)
        {
            if (!LayoutSuspended)
                return;
            LayoutSuspended = false;
            if (invalidate)
                ScheduleInvalidation();
        }

        public Action ValidateData { get; set; } = null;
        public DIResponseKeyImage() : base()
        {
        }

        public DIResponseKeyImage(Uri uri) : base(uri)
        {

        }

        private IImageDisplay _PreviewPanel = null;
        public override IImageDisplay PreviewPanel
        {
            get
            {
                return _PreviewPanel;
            }
            set
            {
                if (value == _PreviewPanel)
                    return;
                if (_PreviewPanel != null)
                    _PreviewPanel.ClearImage();
                if (value == null)
                {
                    _PreviewPanel = value;
                    return;
                }
                _PreviewPanel = value;
                PreviewPanel.SetImage(this.IImage);
            }
        }

        protected override void OnImageEvent(ImageEvent evt, IImageMedia img, object arg)
        {
            base.OnImageEvent(evt, img, arg);
            if (evt == Images.ImageEvent.Resized)
                AbsoluteBounds = (Rectangle)arg;
            if (evt == Images.ImageEvent.ResizeNotNeeded)
                Validate();
            if ((evt == Images.ImageEvent.Updated) || (evt == ImageEvent.Initialized))
            {
                PreviewPanel?.SetImage(img);
                Validate();
            }
        }

        protected override void Validate()
        {
            if (ValidationLockQueue.TryDequeue(out ValidationLock validationLock))
                validationLock.Validate(this);
            base.Validate();
        }

        public Size MinimalSize
        {
            get
            {
                Size bounds = CIAT.SaveFile.Layout.KeyValueSize;
                Size minSize;
                double arBounds = (double)bounds.Width / (double)bounds.Height, arImage = (double)IImage.OriginalImage.Size.Width / (double)IImage.OriginalImage.Size.Height;
                if ((IImage.OriginalImage.Size.Width <= bounds.Width) && (IImage.OriginalImage.Size.Height <= bounds.Height))
                    minSize = IImage.OriginalImage.Size;
                else if (arBounds > arImage)
                    minSize = new Size((int)((double)bounds.Height * arImage), bounds.Height);
                else
                    minSize = new Size(bounds.Width, (int)((double)bounds.Width / arImage));
                var kos = KeyOwners.ToList();
                var sizes = kos.Select(ko => ko.GetDISize(this)).ToList();
                foreach (var sz in sizes)
                {
                    if (sz == Size.Empty)
                        continue;
                    if (minSize.Width >= sz.Width)
                        minSize.Width = sz.Width;
                    if (minSize.Height >= sz.Height)
                        minSize.Height = sz.Height;
                }
                return minSize;
            }
        }
        private ConcurrentQueue<ValidationLock> ValidationLockQueue = new ConcurrentQueue<ValidationLock>();
        public override void LockValidation(ValidationLock validationLock)
        {
            ValidationLockQueue.Enqueue(validationLock);
        }
        private ManualResetEvent invalidationEntryEvt = new ManualResetEvent(true);
        protected override void Invalidate()
        {
            Task.Run(() =>
            {
                try
                {
                    Monitor.TryEnter(this);
                    try
                    {
                        invalidationEntryEvt.WaitOne();
                        invalidationEntryEvt.Reset();
                    }
                    catch
                    {
                        throw;
                    }
                    finally
                    {
                        Monitor.Exit(this);
                    }
                    if (ValidationLockQueue.TryPeek(out ValidationLock validationLock))
                        if (!validationLock.DoInvalidation(this))
                            return;
                    if (LayoutSuspended || (AbsoluteBounds.Size == MinimalSize))
                    {
                        Validate();
                        return;
                    }
                    if (!Resize())
                        Validate();
                }
                catch (ObjectDisposedException ex)
                {
                    return;
                }
                catch (Exception ex)
                {
                    ErrorReporter.ReportError(new CReportableException("Error occurred in image generation", ex));
                }
                finally
                {
                    invalidationEntryEvt.Set();
                }
            });
        }


        public override bool Resize()
        {
            if (IImage.AbsoluteBounds.Size == MinimalSize)
                return false;
            this.IImage.Resize(MinimalSize);
            return true;
        }

        public List<CIATKey> KeyOwners
        {
            get
            {
                try
                {
                    return (CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).ToList().Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri))).ToList();
                }
                catch (Exception ex)
                {
                    return new List<CIATKey>();
                }
            }
        }

        public void ReleaseKeyOwner(IPackagePart pp)
        {
            Size oldSize = MinimalSize;
            CIAT.SaveFile.DeleteRelationship(this.URI, pp.URI);
            if (CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, pp.BaseType).Count() == 0)
            {
                this.Dispose();
                return;
            }
            Size newSize = MinimalSize;
            if (!newSize.Equals(oldSize) || (this.IImage.Size.Width > newSize.Width) || (this.IImage.Size.Height > newSize.Height))
                this.IImage.Resize(newSize);
        }

        public void ReleaseKeyOwners(List<CIATKey> PPs)
        {
            foreach (var pp in PPs)
                ReleaseKeyOwner(pp);
        }

        public void SetKeyOwners(List<CIATKey> PPs)
        {
            Size oldSize = MinimalSize;
            foreach (Uri uri in CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Select(rel => rel.TargetUri))
            {
                CIAT.SaveFile.DeleteRelationship(this.URI, uri);
                CIAT.SaveFile.DeleteRelationship(uri, URI);
            }
            foreach (var pp in PPs)
                AddKeyOwner(pp);
            Size newSize = MinimalSize;
            if (!newSize.Equals(oldSize) || (this.IImage.Size.Width > newSize.Width) || (this.IImage.Size.Height > newSize.Height))
                this.IImage.Resize(newSize);
        }


        public void AddKeyOwner(IPackagePart pp)
        {
            if (CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, pp.BaseType).Where(pr => pr.TargetUri.Equals(pp.URI)).Count() != 0)
                return;
            CIAT.SaveFile.CreateRelationship(BaseType, typeof(CIATKey), this.URI, pp.URI);
            CIAT.SaveFile.CreateRelationship(pp.BaseType, BaseType, pp.URI, URI);
            var key = CIAT.SaveFile.GetIATKey(pp.URI);
            var sz = key.GetDISize(this);
            if ((sz.Width < IImage.Size.Width) || (sz.Height < IImage.Size.Height))
                ScheduleInvalidation();
        }


        public override void SetImage(String rImageId)
        {
            base.SetImage(rImageId);
            if ((PreviewPanel != null) && !IsDisposed)
            {
                PreviewPanel.SetImage(IImage);
            }
            ValidateData?.Invoke();
        }

        public override void Replace(DIBase target)
        {
            if ((target.Type == DIType.ResponseKeyText) || (target.Type == DIType.ResponseKeyImage))
                SetKeyOwners((target as IResponseKeyDI).KeyOwners);
            base.Replace(target);
        }

        public override object Clone()
        {
            DIResponseKeyImage o = new DIResponseKeyImage();
            if (o.IImage != null)
                o.IImage.Dispose();
            o.IImage = IImage.Clone() as IImage;
            o.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument.URI, o.IImage.URI);
            o.Description = this.Description;
            o.ValidateData = this.ValidateData;
            o.SetKeyOwners(KeyOwners);
            return o;
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Description = xDoc.Root.Element("Description").Value;
            if (xDoc.Root.Attribute("rImageId") != null)
            {
                rImageId = xDoc.Root.Attribute("rImageId").Value;
                SetImage(rImageId);
                var iStateAttr = xDoc.Root.Attribute("InvalidationState");
                if (iStateAttr != null)
                    InvalidationState = InvalidationStates.Parse(iStateAttr.Value);
                else
                    InvalidationState = InvalidationStates.InvalidationReady;
                if (CIAT.SaveFile.Version.CompareTo(new CVersion("1.1.1.22")) <= 0)
                    AbsoluteBounds = IImage.AbsoluteBounds;
                else
                {
                    XElement absSize = xDoc.Root.Element("AbsoluteBounds");
                    int x = Convert.ToInt32(absSize.Element("X").Value);
                    int y = Convert.ToInt32(absSize.Element("Y").Value);
                    int width = Convert.ToInt32(absSize.Element("Width").Value);
                    int height = Convert.ToInt32(absSize.Element("Height").Value);
                    AbsoluteBounds = new Rectangle(x, y, width, height);
                    if (AbsoluteBounds == Rectangle.Empty)
                    {
                        if ((this.IImage.OriginalImage.Size.Width <= CIAT.SaveFile.Layout.KeyValueSize.Width) &&
                            (this.IImage.OriginalImage.Size.Height <= CIAT.SaveFile.Layout.KeyValueSize.Height))
                        {
                            AbsoluteBounds = new Rectangle((CIAT.SaveFile.Layout.KeyValueSize.Width - this.IImage.OriginalImage.Size.Width) >> 1,
                                (CIAT.SaveFile.Layout.KeyValueSize.Height - this.IImage.OriginalImage.Size.Height) >> 1,
                                this.IImage.OriginalImage.Size.Width, this.IImage.OriginalImage.Size.Height);
                        }
                        else
                        {
                            double arImg = (double)this.IImage.OriginalImage.Size.Width / (double)this.IImage.OriginalImage.Size.Height;
                            double arFrame = CIAT.SaveFile.Layout.KeyValueSize.Width / (double)CIAT.SaveFile.Layout.KeyValueSize.Height;
                            Size sz;
                            if (arImg < arFrame)
                            {
                                sz = new Size((int)((double)CIAT.SaveFile.Layout.KeyValueSize.Height * arImg), CIAT.SaveFile.Layout.KeyValueSize.Height);
                                AbsoluteBounds = new Rectangle((CIAT.SaveFile.Layout.KeyValueSize.Width - sz.Width) >> 1,
                                    (CIAT.SaveFile.Layout.KeyValueSize.Height - sz.Height) >> 1, sz.Width, sz.Height);
                            }
                            else
                            {
                                sz = new Size(CIAT.SaveFile.Layout.KeyValueSize.Width, (int)((double)CIAT.SaveFile.Layout.KeyValueSize.Width / arImg));
                                AbsoluteBounds = new Rectangle((CIAT.SaveFile.Layout.KeyValueSize.Width - sz.Width) >> 1,
                                    (CIAT.SaveFile.Layout.KeyValueSize.Height - sz.Height) >> 1, sz.Width, sz.Height);
                            }
                        }
                    }
                }
            }
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            if (IImage != null)
            {
                xDoc.Add(new XElement(BaseType.ToString()));
                xDoc.Root.Add(new XAttribute("rImageId", rImageId));
                xDoc.Root.Add(new XAttribute("InvalidationState", InvalidationState.ToString()));
                xDoc.Root.Add(new XElement("Description", Description));
                xDoc.Root.Add(new XElement("AbsoluteBounds", new XElement("X", AbsoluteBounds.X.ToString()), new XElement("Y", AbsoluteBounds.Y.ToString()),
                    new XElement("Width", AbsoluteBounds.Width.ToString()), new XElement("Height", AbsoluteBounds.Height.ToString())));
            }
            else
                xDoc.Add(new XElement(BaseType.ToString(), new XElement("Description", Description)));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public override void Dispose()
        {
            if (IsDisposed || IsDisposing)
                return;
            IsDisposing = true;
            PreviewPanel = null;
            ReleaseKeyOwners(KeyOwners);
            base.Dispose();
            IsDisposed = true;
        }
    }
}
