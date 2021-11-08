using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Threading;
using System.Threading.Tasks;
using System.Text.RegularExpressions;

namespace IATClient
{
    public class DIType : Enumeration
    {
        public static DIType Null = new DIType(0, "Null")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DINull(uri); }),
            Type = typeof(DINull),
            GetBoundingSize = new Func<Size>(() => new Size(1, 1)),
            GetBoundingRectangle = new Func<Rectangle>(() => new Rectangle(0, 0, 1, 1)),
            InvalidationInterval = 0,
            HasPreviewPanel = false,
            IsGenerated = false,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType StimulusImage = new DIType(1, "StimulusImage")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIStimulusImage(uri); }),
            Type = typeof(DIStimulusImage),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.StimulusSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.StimulusRectangle),
            InvalidationInterval = 250,
            HasPreviewPanel = false,
            IsGenerated = false,
            HasThumbnail = true,
            StoreOriginalImage = true
        };
        public static DIType ResponseKeyImage = new DIType(2, "ResponseKeyImage")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIResponseKeyImage(uri); }),
            Type = typeof(DIResponseKeyImage),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => { throw new NotImplementedException(); }),
            InvalidationInterval = 250,
            HasPreviewPanel = true,
            IsGenerated = false,
            HasThumbnail = false,
            StoreOriginalImage = true
        };
        public static DIType StimulusText = new DIType(3, "StimulusText")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIStimulusText(uri); }),
            Type = typeof(DIStimulusText),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.StimulusSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.StimulusRectangle),
            InvalidationInterval = 50,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = true,
            StoreOriginalImage = false
        };
        public static DIType ContinueInstructions = new DIType(4, "ContinueInstructions")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIContinueInstructions(uri); }),
            Type = typeof(DIContinueInstructions),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.ContinueInstructionsRectangle.Size),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.ContinueInstructionsRectangle),
            InvalidationInterval = 200,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType ResponseKeyText = new DIType(5, "ResponseKeyText")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIResponseKeyText(uri); }),
            Type = typeof(DIResponseKeyText),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => { throw new NotImplementedException(); }),
            InvalidationInterval = 150,
            HasPreviewPanel = true,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType Conjunction = new DIType(6, "Conjunction")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIConjunction(uri); }),
            Type = typeof(DIConjunction),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => { throw new NotImplementedException(); }),
            InvalidationInterval = 150,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType MockItemInstructions = new DIType(7, "MockItemInstructions")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIMockItemInstructions(uri); }),
            Type = typeof(DIMockItemInstructions),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.MockItemInstructionsRectangle.Size),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.MockItemInstructionsRectangle),
            InvalidationInterval = 300,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType IatBlockInstructions = new DIType(8, "IatBlockInstructions")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIIatBlockInstructions(uri); }),
            Type = typeof(DIIatBlockInstructions),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.InstructionsSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.InstructionsRectangle),
            InvalidationInterval = 200,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType TextInstructionsScreen = new DIType(9, "TextInstructionsScreen")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DITextInstructionsScreen(uri); }),
            Type = typeof(DITextInstructionsScreen),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.InstructionScreenTextAreaRectangle.Size),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.InstructionScreenTextAreaRectangle),
            InvalidationInterval = 200,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = true,
            StoreOriginalImage = false
        };
        public static DIType KeyedInstructionsScreen = new DIType(10, "KeyedInstructionsScreen")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIKeyedInstructionsScreen(uri); }),
            Type = typeof(DIKeyedInstructionsScreen),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyInstructionScreenTextAreaRectangle.Size),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.KeyInstructionScreenTextAreaRectangle),
            InvalidationInterval = 200,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = true,
            StoreOriginalImage = false
        };
        public static DIType Preview = new DIType(11, "Preview")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIPreview(uri); }),
            Type = typeof(DIPreview),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.InteriorSize),
            GetBoundingRectangle = new Func<Rectangle>(() => new Rectangle(0, 0, CIAT.SaveFile.Layout.InteriorSize.Width, CIAT.SaveFile.Layout.InteriorSize.Height)),
            InvalidationInterval = 150,
            HasPreviewPanel = true,
            IsGenerated = true,
            HasThumbnail = true,
            StoreOriginalImage = false
        };
        public static DIType DualKey = new DIType(12, "DualKey")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIDualKey(uri); }),
            Type = typeof(DIDualKey),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => { throw new NotImplementedException(); }),
            InvalidationInterval = 150,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType ErrorMark = new DIType(13, "ErrorMark")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIErrorMark(uri); }),
            Type = typeof(DIErrorMark),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.ErrorSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.ErrorRectangle),
            InvalidationInterval = 500,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType LeftKeyValueOutline = new DIType(14, "LeftKeyValueOutline")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIKeyValueOutline(uri); }),
            Type = typeof(DIKeyValueOutline),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle),
            InvalidationInterval = 150,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType RightKeyValueOutline = new DIType(15, "RightKeyValueOutline")
        {
            Create = new Func<Uri, DIBase>((uri) => { return new DIKeyValueOutline(uri); }),
            Type = typeof(DIKeyValueOutline),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.KeyValueSize),
            GetBoundingRectangle = new Func<Rectangle>(() => CIAT.SaveFile.Layout.RightKeyValueOutlineRectangle),
            InvalidationInterval = 150,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType LambdaGenerated = new DIType(16, "LambdaGenerated")
        {
            Create = new Func<Uri, DIBase>((uri) => { throw new NotImplementedException(); }),
            Type = typeof(DILambdaGenerated),
            GetBoundingSize = new Func<Size>(() => CIAT.SaveFile.Layout.InteriorSize),
            GetBoundingRectangle = new Func<Rectangle>(() => new Rectangle(0, 0, CIAT.SaveFile.Layout.InteriorSize.Width, CIAT.SaveFile.Layout.InteriorSize.Height)),
            InvalidationInterval = 100,
            HasPreviewPanel = false,
            IsGenerated = true,
            HasThumbnail = false,
            StoreOriginalImage = false
        };
        public static DIType SurveyImage = new DIType(17, "SurveyImage")
        {
            Create = new Func<Uri, DIBase>((uri) => new DISurveyImage(uri)),
            Type = typeof(DISurveyImage),
            GetBoundingSize = new Func<Size>(() => { throw new NotImplementedException(); }),
            GetBoundingRectangle = new Func<Rectangle>(() => { throw new NotImplementedException(); }),
            InvalidationInterval = 200,
            HasPreviewPanel = true,
            IsGenerated = false,
            HasThumbnail = false,
            StoreOriginalImage = true
        };
        public Type Type { get; private set; }
        public String MimeType
        {
            get
            {
                return "text/xml+display-item+" + ToString();
            }
        }
        public Func<Uri, DIBase> Create { get; private set; }
        public Func<Size> GetBoundingSize { get; private set; }
        public Func<Rectangle> GetBoundingRectangle { get; private set; }
        public int InvalidationInterval { get; private set; }
        public bool HasPreviewPanel { get; private set; }
        public bool IsGenerated { get; private set; }
        public bool HasThumbnail { get; private set; }
        public bool StoreOriginalImage { get; private set; }
        public bool IsText
        {
            get
            {
                return TextTypes.Where(t => t == this).FirstOrDefault() != null;
            }
        }
        public bool IsComposite
        {
            get
            {
                return CompositeTypes.Where(t => t == this).FirstOrDefault() != null;
            }
        }

        private DIType(int id, String name) : base(id, name) { }
        private DIType(int id, String name, int ii, Func<Uri, DIBase> f, Type t, Func<Size> bSz, Func<Rectangle> bRect)
            : base(id, name)
        {
            Type = t;
            Create = f;
            InvalidationInterval = ii;
            GetBoundingSize = bSz;
            GetBoundingRectangle = bRect;
        }
        private static readonly DIType[] CompositeTypes = new DIType[] { DualKey, Preview };
        private static readonly DIType[] TextTypes = new DIType[] { StimulusText, ContinueInstructions, ResponseKeyText, Conjunction, MockItemInstructions, IatBlockInstructions, TextInstructionsScreen };
        public static readonly IEnumerable<DIType> All = new DIType[]{ Null, StimulusImage, ResponseKeyImage, StimulusText, ContinueInstructions, ResponseKeyText, Conjunction, MockItemInstructions, IatBlockInstructions,
            TextInstructionsScreen, KeyedInstructionsScreen, Preview, DualKey, ErrorMark, LeftKeyValueOutline, RightKeyValueOutline, LambdaGenerated, SurveyImage };

        public static DIType FromString(String name)
        {
            return All.FirstOrDefault((diType) => diType.Name == name);
        }

        public static DIType FromTypeName(String tName)
        {
            return All.FirstOrDefault((diType) => diType.Type.ToString() == tName);
        }

        public static DIType FromType(Type t)
        {
            return All.FirstOrDefault((diType) => diType.Type == t);
        }

        public static DIType FromUri(Uri u)
        {
            Regex urlMatcher = new Regex(@"^/IATClient\.[^/]+/(IATClient\..*?)[1-9][0-9]*.*?(?!=\.rel)");
            if (!urlMatcher.IsMatch(u.ToString()))
                return null;
            String typeName = urlMatcher.Match(u.ToString()).Groups[1].Value;
            return All.FirstOrDefault((diType) => diType.Type.ToString() == typeName);
        }
    }

    public abstract class DIBase : IDisposable, IPackagePart, ICloneable
    {
        protected bool IsDisposed { get; set; } = false;
        private bool Replaced { get; set; } = false;
        protected bool Broken { get; set; } = false;
        protected Func<Size> GetBoundingSize;
        public bool IsValid { get; protected set; }
        protected readonly object lockObject = new object();
        public virtual Size ItemSize { get; protected set; } = Size.Empty;
        public virtual Images.IImage IImage { get; protected set; }
        public virtual Uri URI { get; set; } 
        public virtual bool IsObservable { get { return false; } }
        public Type BaseType { get { return typeof(DIBase); } }
        public long Expiration { get; set; }
        public virtual IImageDisplay PreviewPanel { get; set; } = null;
        public virtual bool IsComposite { get { return false; } }
        private bool WaitingOnInvalidation { get; set; } = false;
        public virtual IUri IUri { get; private set; }

        public virtual Rectangle ItemRect
        {
            get { return new Rectangle(new Point(0, 0), ItemSize); }
            protected set {}
        }

        public static DIType GetDiType(Uri uri)
        {
            return DIType.FromTypeName(CIAT.SaveFile.GetTypeName(uri));
        }

        public String MimeType
        {
            get
            {
                return "text/xml+display-item+" + DIType.FromType(this.GetType()).ToString();
            }
        }

        public DIType Type { get { return DIType.FromType(this.GetType()); } }

        public DIBase(Uri URI)
        {
            InvalidationState = InvalidationReady;
            this.URI = URI;
            IUri = new UriContainer(URI);
            this.IImage = null;
            GetBoundingSize = DIType.FromType(this.GetType()).GetBoundingSize;
            Load(URI);
            CIAT.SaveFile.Register(this);
            if (IImage != null)
            {
                IImage.Changed += new Action<Images.ImageChangedEvent, Images.IImageMedia, object>(OnImageChanged);
                IsValid = true;
                ItemSize = IImage.Size;
            }
            else
                IsValid = false;
        }

        public DIBase()
        {
            InvalidationState = InvalidationReady;
            if (Type.IsComposite)
                CompositeInvalidationEvents.Add(InvalidationEvent);
            else
                ComponentInvalidationEvents.Add(InvalidationEvent);
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            IUri = new UriContainer(URI);
            GetBoundingSize = DIType.FromType(this.GetType()).GetBoundingSize;
            IsValid = false;
            CIAT.SaveFile.Register(this);
        }

        public DIBase(Images.IImage imgObj)
        {
            InvalidationState = InvalidationReady;
            if (Type.IsComposite)
                CompositeInvalidationEvents.Add(InvalidationEvent);
            else
                ComponentInvalidationEvents.Add(InvalidationEvent);
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            IUri = new UriContainer(URI);
            GetBoundingSize = DIType.FromType(this.GetType()).GetBoundingSize;
            this.IImage = imgObj.Clone() as Images.IImage;
            IsValid = true;
            ItemSize = imgObj.Size;
            CIAT.SaveFile.CreateRelationship(BaseType, this.IImage.BaseType, URI, this.IImage.URI);
            IImage.Changed += new Action<Images.ImageChangedEvent, Images.IImageMedia, object>(OnImageChanged);
            CIAT.SaveFile.Register(this);
            Save();
        }

        public virtual Size BoundingSize
        {
            get
            {
                return GetBoundingSize();
            }
        }
        public virtual Rectangle AbsoluteBounds { get; protected set; } = Rectangle.Empty;
        private static readonly List<ManualResetEventSlim> ComponentInvalidationEvents = new List<ManualResetEventSlim>();
        private static readonly List<ManualResetEventSlim> CompositeInvalidationEvents = new List<ManualResetEventSlim>();
        protected abstract void Invalidate();
        public virtual bool IsGenerated { get { return false; } }
        public WaitHandle InvalidationWaitHandle { get { return InvalidationEvent.WaitHandle; } }
        private readonly object InvalidationLock = new object();
        private readonly object InvalidationReady = new object(), NoInvalidationQueued = new object(), InvalidationQueued = new object(), InvalidationQueueReady = new object();
        private object InvalidationState;
        private ManualResetEventSlim InvalidationEvent = new ManualResetEventSlim(true);
        public virtual void ScheduleInvalidation()
        {
            DIType t = DIType.FromUri(URI);
            Task.Run(() =>
            {
                if (Interlocked.CompareExchange(ref InvalidationState, InvalidationQueueReady, InvalidationReady).Equals(InvalidationReady))
                {
                    InvalidationEvent.Reset();
                    Invalidate();
                    return;
                }
                if (Interlocked.CompareExchange(ref InvalidationState, InvalidationQueued, InvalidationQueueReady).Equals(InvalidationQueueReady))
                {
                    InvalidationEvent.Wait();
                    InvalidationEvent.Reset();
                    Invalidate();
                 }
            });
        }
        protected virtual void Validate()
        {
            Interlocked.Exchange(ref InvalidationState, InvalidationReady);
            InvalidationEvent.Set();
            IsValid = true;
        }
        protected abstract void DoLoad(Uri uri);

        public void Load(Uri uri)
        {
            DoLoad(uri);
//            Invalidate();
        }
        public abstract void Save();
        public void Save(Uri uri)
        {
            this.URI = uri;
            Save();
            CIAT.SaveFile.Register(this);
        }

        public virtual bool ImageStale { get; protected set; } = false;
        public virtual bool ComponentStale
        {
            get
            {
                return ImageStale || (IImage == null);
            }
        }

        public virtual void SetImage(System.Drawing.Image img, System.Drawing.Imaging.ImageFormat format)
        {
            lock (lockObject)
            {
                if (IsDisposed)
                    return;
                ItemSize = img.Size;
                if (this.IImage != null)
                {
                    this.IImage.Format = format;
                    this.IImage.Image = img;                }
                else
                {
                    IImage = CIAT.ImageManager.CreateImage(img, format, Type, (evt, im, arg) => OnImageChanged(evt, im, arg));
                    CIAT.SaveFile.CreateRelationship(BaseType, IImage.BaseType, URI, IImage.URI);
                }
            }
        }

        public virtual void SetImage(Uri imgUri)
        {
            lock (lockObject)
            {
                if (this.IImage != null)
                {
                    if (!this.IImage.URI.Equals(imgUri))
                    {
                        CIAT.SaveFile.DeleteRelationship(URI, IImage.URI);
                        this.IImage.Dispose();
                    }
                    else
                        return;
                }
                this.IImage = CIAT.SaveFile.GetIImage(imgUri);
                CIAT.SaveFile.CreateRelationship(BaseType, IImage.BaseType, URI, IImage.URI);
                ItemSize = IImage.Size;
                this.IImage.Changed += new Action<Images.ImageChangedEvent, Images.IImageMedia, object>(OnImageChanged);
            }
        }

        public virtual void ClearImage()
        {
            lock (lockObject)
            {
                if (this.IImage != null)
                {
                    this.IImage.Dispose();
                }
                this.IImage = null;
            }
        }

        public virtual void Replace(DIBase target)
        {
            lock (lockObject)
            {
                Uri oldUri = URI;
                CIAT.SaveFile.Replace(this, target);
                Save();
                target.Replaced = true;
                target.Dispose();
                CIAT.SaveFile.DeletePart(oldUri);
            }
        }

        protected virtual void OnImageChanged(Images.ImageChangedEvent evt, Images.IImageMedia img, object arg)
        {
            if (IsDisposed)
                return;
            if ((evt == Images.ImageChangedEvent.Updated) || (evt == Images.ImageChangedEvent.Initialized) || (evt == Images.ImageChangedEvent.ResizeUpdate))
            {
                ItemSize = img.Size;
                try
                {
                    List<Uri> uris = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(DIBase), "owned-by").Select(pr => pr.TargetUri).ToList();
                    foreach (Uri u in uris)
                    {
                        try
                        {
                            DIBase owner = CIAT.SaveFile.GetDI(u);
                            if (!owner.Type.HasPreviewPanel)
                                owner.ScheduleInvalidation();
                            else if (owner.PreviewPanel != null)
                                if (!owner.PreviewPanel.IsDisposed)
                                    owner.ScheduleInvalidation();
                        }
                        catch (KeyNotFoundException) { }
                    }
                }
                catch (InvalidOperationException) { }
                finally
                {
                    Validate();
                }
            }
            else if (evt == Images.ImageChangedEvent.Resized)
            {
                ItemSize = img.Size;
            }
        }
            
        

        public Size OriginalSize
        {
            get
            {
                return this.IImage.OriginalSize;
            }
        }

        public virtual bool Resize()
        {
            if (this.IImage == null)
                return false;
            if (Type.IsGenerated)
                ScheduleInvalidation();
            else if (((this.IImage.OriginalSize.Width > BoundingSize.Width) || (this.IImage.OriginalSize.Height > BoundingSize.Height)) &&
                (this.IImage.Size.Width != BoundingSize.Width) || (this.IImage.Size.Height != BoundingSize.Height))
            {
                this.IImage.Resize(BoundingSize);
                return true;
            }
            return false;
        }


        public void AddOwner(Uri ownerUri) {
            foreach (PackageRelationship pr in CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(DIBase), "owned-by"))
                if (pr.TargetUri.Equals(ownerUri))
                    return;
            CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), URI, ownerUri, "owned-by");
            CIAT.SaveFile.CreateRelationship(typeof(DIBase), BaseType, ownerUri, URI, "owns");
        }

        public void CopyOwners(Uri srcUri, Uri destUri)
        {
            var owningUris = CIAT.SaveFile.GetRelationshipsByType(srcUri, typeof(DIBase), typeof(DIBase), "owned-by").Select(pr => pr.SourceUri).ToList();
            foreach (var u in owningUris)
            {
                CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), destUri, u, "owned-by");
                CIAT.SaveFile.CreateRelationship(typeof(DIBase), BaseType, u, destUri, "owns");
            }
        }

        public void ReleaseOwner(Uri ownerUri)
        {
            if (!CIAT.SaveFile.PartExists(ownerUri))
                return;
            CIAT.SaveFile.DeleteRelationship(ownerUri, URI);
            if (CIAT.SaveFile.GetDI(ownerUri).IsComposite)
                (CIAT.SaveFile.GetDI(ownerUri) as DIComposite).ReleaseDI(URI);
        }

        public void ReleaseSubject(Uri subjectUri)
        {
            if (!CIAT.SaveFile.PartExists(subjectUri))
                return;
            CIAT.SaveFile.DeleteRelationship(URI, subjectUri);
            if (CIAT.SaveFile.GetDI(URI).IsComposite)
                (CIAT.SaveFile.GetDI(URI) as DIComposite).ReleaseDI(subjectUri);
        }

        public abstract object Clone();

        public virtual void Dispose()
        {
            lock (lockObject)
            {
                if (IsDisposed)
                    return;
                if (!Replaced)
                {
                    List<Uri> owns = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(DIBase), "owns").Select(pr => pr.TargetUri).ToList();
                    foreach (Uri u in owns)
                        ReleaseSubject(URI);
                    List<Uri> owners = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(DIBase), "owned-by").Select(pr => pr.TargetUri).ToList();
                    foreach (Uri u in owners)
                        ReleaseOwner(u);
                    CIAT.SaveFile.DeletePart(this.URI);
                }
            }
            if (this.IImage != null)
                this.IImage.Dispose();
            IsDisposed = true;
        }
    }

    public class DINull : DIBase, IStimulus
    {
        public Action ValidateData { get; set; }

        public DINull()
        {
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.Null);
            bmp.SetPixel(0, 0, Color.Transparent);
            SetImage(bmp, System.Drawing.Imaging.ImageFormat.Png);
        }

        public DINull(Uri uri)
            : base(uri)
        {
        }

        protected override void Invalidate() { Validate(); }

        public void AddKeyOwner()
        {

        }

        public String Description
        {
            get
            {
                return String.Empty;
            }
        }

        public IImageDisplay ThumbnailPreviewPanel { get; set; }


        protected override void DoLoad(Uri uri)
        {
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.Null);
            bmp.SetPixel(0, 0, Color.Transparent);
            this.IImage = CIAT.SaveFile.ImageManager.CreateImage(System.Drawing.Imaging.ImageFormat.Png, DIType.Null);
            this.IImage.Image = bmp;
        }

        public override void Save()
        {
        }

        public override object Clone()
        {
            return new DINull();
        }

        public bool Equals(IStimulus stim)
        {
            if (stim.Type == DIType.Null)
                return true;
            return false;
        }

        public override void Replace(DIBase target)
        {
            if ((target.Type == DIType.ResponseKeyImage) || (target.Type == DIType.ResponseKeyText) || (target.Type == DIType.Conjunction))
            {
                PreviewPanel = target.PreviewPanel;
                ValidateData = (target as IResponseKeyDI).ValidateData;
            }
            if (target.Type == DIType.Null)
            {
                PreviewPanel = target.PreviewPanel;
                ValidateData = (target as IResponseKeyDI).ValidateData;
            }
            if (target.Type == DIType.Null)
                return;
            base.Replace(target);
        }
    }
}
