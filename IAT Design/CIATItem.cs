using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace IATClient
{

    public class KeyedDirection : Enumeration
    {
        public static readonly KeyedDirection Left = new KeyedDirection(1, "Left", new Func<KeyedDirection>(() => KeyedDirection.Right));
        public static readonly KeyedDirection Right = new KeyedDirection(2, "Right", new Func<KeyedDirection>(() => KeyedDirection.Left));
        public static readonly KeyedDirection None = new KeyedDirection(3, "None", new Func<KeyedDirection>(() => KeyedDirection.None));
        public static readonly KeyedDirection DynamicLeft = new KeyedDirection(4, "DynamicLeft", new Func<KeyedDirection>(() => KeyedDirection.DynamicRight));
        public static readonly KeyedDirection DynamicRight = new KeyedDirection(5, "DynamicNone", new Func<KeyedDirection>(() => KeyedDirection.DynamicLeft));
        public static readonly KeyedDirection DynamicNone = new KeyedDirection(6, "DynamicRight", new Func<KeyedDirection>(() => KeyedDirection.DynamicNone));

        protected Dictionary<String, KeyedDirection> ParseMap = new Dictionary<string, KeyedDirection>();
        private Func<KeyedDirection> GetOpposite { get; set; } = null;
        public KeyedDirection Opposite
        {
            get
            {
                return GetOpposite();
            }
        }

        protected KeyedDirection(int val, String name, Func<KeyedDirection> getOpposite) : base(val, name)
        {
            ParseMap.Add(name, this);
            GetOpposite = getOpposite;
        }

        private static IEnumerable<KeyedDirection> All = new KeyedDirection[] { Left, Right, None, DynamicLeft, DynamicRight, DynamicNone };

        public static KeyedDirection FromString(String str)
        {
            return All.Where(kd => kd.Name == str).FirstOrDefault();
        }


    }

    public class CIATItemPreview : IPreviewableItem
    {
        private Uri ParentBlockUri, ItemUri;
        public bool IsDisposed
        {
            get
            {
                try
                {
                    if (CIAT.SaveFile.GetIATItem(ItemUri).IsDisposed)
                        return true;
                }
                catch (KeyNotFoundException ex)
                {
                    return true;
                }
                return false;
            }
        }
        public bool IsSurvey { get { return false; } }

        public CIATItemPreview(Uri parentBlockUri, Uri itemUri)
        {
            ParentBlockUri = parentBlockUri;
            ItemUri = itemUri;
        }

        public void Preview(IImageDisplay previewPanel)
        {
            CIAT.SaveFile.GetIATItem(ItemUri).GeneratePreview(previewPanel, ParentBlockUri);
            previewPanel.Tag = this;
        }

        public void EndPreview(IImageDisplay previewPanel)
        {
            CIAT.SaveFile.GetIATItem(ItemUri).EndPreview(previewPanel, ParentBlockUri);
        }

        public void OpenItem(IATConfigMainForm mainForm)
        {
            CIAT.SaveFile.GetIATItem(ItemUri).OpenItem(mainForm, CIAT.SaveFile.GetIATBlock(ParentBlockUri));
        }

        public String PreviewText
        {
            get
            {
                return CIAT.SaveFile.GetIATItem(ItemUri).GetPreviewText(ParentBlockUri);
            }
        }

        public bool IsHeaderItem
        {
            get
            {
                return false;
            }
        }

        public Button GUIButton
        {
            get
            {
                return CIAT.SaveFile.GetIATItem(ItemUri).GUIButton;
            }
            set
            {
                CIAT.SaveFile.GetIATItem(ItemUri).GUIButton = value;
            }
        }
    }


    public class CIATItem : IValidatedItem, IDisposable, IPackagePart, IThumbnailPreviewable
    {
        // constant strings to represent the key direction
        private bool _IsDisposed = false;
        private Uri _StimulusUri = null;
        private String _SpecifierArg = String.Empty;
        private static Random random = new Random();
        public Uri URI { get; set; }
        public Type BaseType { get { return typeof(CIATItem); } }
        public String MimeType { get { return "text/xml+" + typeof(CIATItem).ToString(); } }
        private Dictionary<Uri, Tuple<KeyedDirection, Uri>> ParentBlockUris = new Dictionary<Uri, Tuple<KeyedDirection, Uri>>();
        public String SpecifierArg { get; set; }
        public int KeySpecifierID { get; set; } = -1;
        public bool IsHeaderItem { get { return false; } }
        private readonly object lockObject = new object();

        public int OriginatingBlock
        {
            get
            {
                return ParentBlockUris.Keys.Select(u => CIAT.SaveFile.GetIATBlock(u).IndexInContainer).Min();
            }
        }

        public Uri StimulusUri
        {
            get
            {
                return _StimulusUri;
            }
            set
            {
                if (value == null)
                    throw new Exception("Cannot have null stimulus. Set to DINull.");
                if (value.Equals(StimulusUri))
                    return;
                IStimulus stim = (value != null) ? (CIAT.SaveFile.GetDI(value) as IStimulus) : null;
                IStimulus oldStim = (_StimulusUri == null) ? null : (CIAT.SaveFile.GetDI(_StimulusUri) as IStimulus);
                if (oldStim != null)
                {
                    CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Detached, _StimulusUri, URI);
                    CIAT.SaveFile.DeleteRelationship(URI, _StimulusUri);
                }
                _StimulusUri = value;
                if (stim != null)
                {
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), URI, value);
                    CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Attached, value, URI);
                }
                foreach (DIPreview dic in ParentBlockUris.Values.Select(tup => CIAT.SaveFile.GetDI(tup.Item2) as DIPreview))
                {
                    if (oldStim != null)
                        dic.RemoveComponent(LayoutItem.Stimulus, stim == null);
                    if (stim != null)
                        dic.AddComponent(stim.IUri, LayoutItem.Stimulus);
                }
                stim.ThumbnailPreviewPanel = ThumbnailPreviewPanel;
                oldStim?.Dispose();
            }
        }

        public DIPreview GetDIPreview(Uri parentBlockUri)
        {
            return CIAT.SaveFile.GetDI(ParentBlockUris[parentBlockUri].Item2) as DIPreview;
        }

        public DIBase Stimulus
        {
            get
            {
                try
                {
                    var stim = CIAT.SaveFile.GetDI(StimulusUri);
                    if (stim == DIBase.DINull)
                        return DIBase.DINull;
                    (stim as IThumbnailPreviewable).ThumbnailPreviewPanel = ThumbnailPreviewPanel;
                    if (stim.IImage.Thumbnail == null)
                        stim.IImage.CreateThumbnail();
                    return stim;
                }
                catch (Exception)
                {
                    return DIBase.DINull;
                }
            }
        }

        public void SuspendPreviewLayout(Uri blockUri)
        {
            (CIAT.SaveFile.GetDI(ParentBlockUris[blockUri].Item2) as DIPreview).SuspendLayout();
        }

        public DIPreview GetPreview(Uri blockUri)
        {
            return CIAT.SaveFile.GetDI(ParentBlockUris[blockUri].Item2) as DIPreview;
        }

        public void SetPreviewPane(Uri parentBlockUri, IImageDisplay PreviewPane)
        {
            (CIAT.SaveFile.GetDI(ParentBlockUris[parentBlockUri].Item2) as DIPreview).PreviewPanel = PreviewPane;
        }

        public KeyedDirection GetKeyedDirection(Uri parentBlockUri)
        {
            return ParentBlockUris[parentBlockUri].Item1;
        }

        public void SetKeyedDirection(Uri parentBlockUri, KeyedDirection keyedDir)
        {
            if (ParentBlockUris[parentBlockUri].Item1 == keyedDir)
                return;
            if ((ParentBlockUris[parentBlockUri].Item1 == KeyedDirection.Left) || (ParentBlockUris[parentBlockUri].Item1 == KeyedDirection.Right) || (ParentBlockUris[parentBlockUri].Item1 == KeyedDirection.None))
            {
                CIATBlock baseBlock = ParentBlockUris.Keys.Select(uri => CIAT.SaveFile.GetIATBlock(uri)).OrderBy(b => b.IndexInContainer).First();
                KeyedDirection newBaseKeyedDir, oldBaseKeyedDir = ParentBlockUris[baseBlock.URI].Item1;
                int changingBlock = CIAT.SaveFile.GetIATBlock(parentBlockUri).IndexInContainer;
                if (baseBlock.IndexInContainer == 0)
                    newBaseKeyedDir = keyedDir;
                else if (changingBlock > 3)
                    newBaseKeyedDir = keyedDir.Opposite;
                else
                    newBaseKeyedDir = keyedDir;
                List<Uri> blockUris = ParentBlockUris.Keys.ToList();
                foreach (CIATBlock b in blockUris.Select(u => CIAT.SaveFile.GetIATBlock(u)))
                {
                    KeyedDirection newKeyedDir;
                    if (baseBlock.IndexInContainer == 0)
                        newKeyedDir = keyedDir;
                    else if (((b.IndexInContainer > 3) && (CIAT.SaveFile.GetIATBlock(parentBlockUri).IndexInContainer <= 3)) ||
                        ((b.IndexInContainer <= 3) && (CIAT.SaveFile.GetIATBlock(parentBlockUri).IndexInContainer > 3)))
                        newKeyedDir = keyedDir.Opposite;
                    else
                        newKeyedDir = keyedDir;
                    if (ParentBlockUris[b.URI].Item1.Equals(newKeyedDir))
                        continue;
                    //                    (CIAT.SaveFile.GetDI(ParentBlockUris[b.URI].Item2) as DIPreview).SuspendLayout();
                    if (ParentBlockUris[b.URI].Item1 == KeyedDirection.Left)
                        (CIAT.SaveFile.GetDI(ParentBlockUris[b.URI].Item2) as DIPreview).RemoveComponent(LayoutItem.LeftResponseKeyOutline, false);
                    else if (ParentBlockUris[b.URI].Item1 == KeyedDirection.Right)
                        (CIAT.SaveFile.GetDI(ParentBlockUris[b.URI].Item2) as DIPreview).RemoveComponent(LayoutItem.RightResponseKeyOutline, false);
                    if (newKeyedDir == KeyedDirection.Right)
                        (CIAT.SaveFile.GetDI(ParentBlockUris[b.URI].Item2) as DIPreview).AddComponent(CIAT.SaveFile.Layout.RightKeyValueOutline.IUri, LayoutItem.RightResponseKeyOutline);
                    if (newKeyedDir == KeyedDirection.Left)
                        (CIAT.SaveFile.GetDI(ParentBlockUris[b.URI].Item2) as DIPreview).AddComponent(CIAT.SaveFile.Layout.LeftKeyValueOutline.IUri, LayoutItem.LeftResponseKeyOutline);
                    ParentBlockUris[b.URI] = new Tuple<KeyedDirection, Uri>(newKeyedDir, ParentBlockUris[b.URI].Item2);
                }
            }
            else if ((ParentBlockUris[parentBlockUri].Item1 == KeyedDirection.DynamicLeft) || (ParentBlockUris[parentBlockUri].Item1 == KeyedDirection.DynamicRight))
            {
                foreach (Uri u in ParentBlockUris.Keys)
                {
                    ParentBlockUris[u] = new Tuple<KeyedDirection, Uri>(ParentBlockUris[u].Item1.Opposite, ParentBlockUris[u].Item2);
                }
            }
            foreach (var prev in ParentBlockUris.Select(kv => CIAT.SaveFile.GetDI(kv.Value.Item2)).Cast<DIPreview>())
                prev.ScheduleInvalidation();
        }

        private IImageDisplay _ThumbnailPreviewPanel = null;
        public IImageDisplay ThumbnailPreviewPanel
        {
            get
            {
                return _ThumbnailPreviewPanel;
            }
            set
            {
                _ThumbnailPreviewPanel = value;
                if (!_StimulusUri.Equals(DIBase.DINull.URI))
                    (CIAT.SaveFile.GetDI(_StimulusUri) as IStimulus).ThumbnailPreviewPanel = value;
            }
        }

        public void AddParentBlock(CIATBlock parentBlock, KeyedDirection keyedDir)
        {
            List<Tuple<IUri, LayoutItem>> previewComponents = new List<Tuple<IUri, LayoutItem>>();
            previewComponents.Add(new Tuple<IUri, LayoutItem>(CIAT.SaveFile.GetDI(StimulusUri).IUri, LayoutItem.Stimulus));
            if (keyedDir == KeyedDirection.Left)
                previewComponents.Add(new Tuple<IUri, LayoutItem>(CIATLayout.ILeftKeyValueOutlineUri, LayoutItem.LeftResponseKeyOutline));
            else if (keyedDir == KeyedDirection.Right)
                previewComponents.Add(new Tuple<IUri, LayoutItem>(CIATLayout.IRightKeyValueOutlineUri, LayoutItem.RightResponseKeyOutline));
            previewComponents.AddRange(parentBlock.GetPreviewComponents());
            DIPreview preview = new DIPreview(previewComponents);
            preview.SuspendLayout();
            ParentBlockUris[parentBlock.URI] = new Tuple<KeyedDirection, Uri>(keyedDir, preview.URI);
            CIAT.SaveFile.CreateRelationship(BaseType, typeof(CIATBlock), this.URI, parentBlock.URI);
            CIAT.SaveFile.CreateRelationship(BaseType, preview.BaseType, this.URI, preview.URI);
        }

        public void DetachParentBlock(CIATBlock block)
        {
            DIBase preview = CIAT.SaveFile.GetDI(ParentBlockUris[block.URI].Item2);
            CIAT.SaveFile.DeleteRelationship(this.URI, preview.URI);
            CIAT.SaveFile.DeleteRelationship(this.URI, block.URI);
            preview.Dispose();
            ParentBlockUris.Remove(block.URI);
            if (ParentBlockUris.Count == 0)
                Dispose();
        }

        public CIATItem()
        {
            this.URI = CIAT.SaveFile.Register(this);
            _StimulusUri = DIBase.DINull.URI;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }

        public CIATItem(Uri uri)
        {
            this.URI = uri;
            CIAT.SaveFile.Register(this);
            Load(uri);
        }

        public void Validate(int itemNdx, CIATBlock parent)
        {
            if (Stimulus.Type == DIType.Null)
                throw new CValidationException(String.Format(Properties.Resources.sNoStimulusForItemException, itemNdx + 1));
            else if (Stimulus.Type == DIType.StimulusText)
            {
                DIStimulusText stim = Stimulus as DIStimulusText;
                if (stim.Phrase.Trim() == String.Empty)
                    throw new CValidationException(String.Format(Properties.Resources.sNoStimulusForItemException, itemNdx + 1));
            }
            else if (Stimulus.Type == DIType.StimulusImage)
                if (Stimulus.IImage == null)
                    throw new CValidationException(String.Format(Properties.Resources.sNoStimulusForItemException, itemNdx + 1));
            if (ParentBlockUris[parent.URI].Item1 == KeyedDirection.None)
                throw new CValidationException(String.Format(Properties.Resources.sNoKeyedDirAssignedToStimulusException, itemNdx + 1));
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            String rStimulusId = String.Empty;
            if (StimulusUri != null)
                rStimulusId = CIAT.SaveFile.GetRelationship(this, Stimulus);
            if (KeySpecifierID == -1)
            {
                xDoc.Document.Add(new XElement(typeof(CIATItem).ToString(), new XElement("rStimulusId", rStimulusId), new XElement("Disposed", IsDisposed.ToString())));
            }
            else
            {
                xDoc.Document.Add(new XElement(typeof(CIATItem).ToString()));
                if (rStimulusId != String.Empty)
                    xDoc.Root.Add(new XElement("rStimulusId", rStimulusId));
                xDoc.Root.Add(new XElement("Disposed", IsDisposed.ToString()),
                new XElement("KeySpecifierID", KeySpecifierID.ToString()),
                new XElement("SpecifierArg", SpecifierArg));
            }
            foreach (Uri u in ParentBlockUris.Keys)
            {
                String rParentId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATBlock)).Where(rel => rel.TargetUri.Equals(u)).Select(rel => rel.Id).First();
                Uri previewUri = ParentBlockUris[u].Item2;
                String rPreviewId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(rel => rel.TargetUri.Equals(previewUri)).Select(rel => rel.Id).First();
                xDoc.Root.Add(new XElement("ParentBlock", new XAttribute("rId", rParentId), new XElement("rPreviewId", rPreviewId), new XElement("KeyedDirection", ParentBlockUris[u].Item1.ToString())));
            }
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public void Load(Uri uri)
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            XElement elem = xDoc.Document.Root;
            if (elem.Element("rStimulusId") != null)
                StimulusUri = CIAT.SaveFile.GetRelationship(this, elem.Element("rStimulusId").Value).TargetUri;
            else
                StimulusUri = DINull.DINull.URI;
            _IsDisposed = Convert.ToBoolean(elem.Element("Disposed").Value);
            if (elem.Element("KeySpecifierID") == null)
            {
                KeySpecifierID = -1;
                SpecifierArg = String.Empty;
            }
            else
            {
                KeySpecifierID = Convert.ToInt32(elem.Element("KeySpecifierID").Value);
                SpecifierArg = elem.Element("SpecifierArg").Value;
            }
            foreach (XElement e in xDoc.Root.Elements("ParentBlock"))
            {
                Uri parentUri = CIAT.SaveFile.GetRelationship(this, e.Attribute("rId").Value).TargetUri;
                Uri previewUri = CIAT.SaveFile.GetRelationship(this, e.Element("rPreviewId").Value).TargetUri;
                KeyedDirection keyedDir = KeyedDirection.FromString(e.Element("KeyedDirection").Value);
                ParentBlockUris[parentUri] = new Tuple<KeyedDirection, Uri>(keyedDir, previewUri);
            }
        }
        public Images.IImage StimulusImage
        {
            get
            {
                lock (lockObject)
                {
                    if (Stimulus == null)
                        return null;
                    return Stimulus.IImage;
                }
            }
        }

        public void ClearKeySpecifier()
        {
            if (KeySpecifierID == -1)
                return;
            KeySpecifierID = -1;
        }

        public ScrollingPreviewPanelPane ThumbnailDisplay { get; set; }

        public void ValidateItem(Dictionary<IValidatedItem, CValidationException> ErrorDictionary)
        {
            if (IsDisposed)
                return;
            CValidationException ex = null;
            CIATBlock parentBlock = CIAT.SaveFile.GetIATBlock(ParentBlockUris.Keys.First());
            CLocationDescriptor loc = new CItemLocationDescriptor(parentBlock, this);
            if (Stimulus.Type == DIType.Null)
            {
                ErrorDictionary[this] = new CValidationException(EValidationException.ItemStimulusUndefined, loc);
                return;
            }
            if (Stimulus.Type == DIType.StimulusText)
            {
                if ((Stimulus as DIStimulusText).Phrase.Trim() == String.Empty)
                {
                    ErrorDictionary[this] = new CValidationException(EValidationException.TextStimlusIncompletelyInitialized, loc);
                    return;
                }
            }
            if (Stimulus.Type == DIType.StimulusImage)
            {
                if ((Stimulus as DIStimulusImage).Description == String.Empty)
                {
                    ErrorDictionary[this] = new CValidationException(EValidationException.ImageStimulusIncompletelyInitialized, loc);
                    return;
                }
            }
            if (ParentBlockUris[parentBlock.URI].Item1 == KeyedDirection.None)
            {
                ErrorDictionary[this] = new CValidationException(EValidationException.ItemKeyedDirUndefined, loc);
                return;
            }
        }

        public void OpenItem(IATConfigMainForm mainForm, CIATBlock parentBlock)
        {
            mainForm.ActiveItem = parentBlock;
            mainForm.FormContents = IATConfigMainForm.EFormContents.IATBlock;
            mainForm.SetActiveIATItem(this);
        }

        public String GetPreviewText(Uri parentBlockUri)
        {
            if (Stimulus.Type == DIType.Null)
                return String.Format("Stimulus #{0}", CIAT.SaveFile.GetIATBlock(parentBlockUri).GetItemIndex(this) + 1);
            else if (Stimulus.Type == DIType.StimulusImage)
                return ((DIStimulusImage)Stimulus).Description;
            else
                return ((DIStimulusText)Stimulus).Phrase;
        }

        public void GeneratePreview(IImageDisplay previewPanel, Uri parentBlockUri)
        {
            if (previewPanel.Tag != null)
                (previewPanel.Tag as IPreviewableItem).EndPreview(previewPanel);
            CIAT.SaveFile.GetDI(ParentBlockUris[parentBlockUri].Item2).PreviewPanel = previewPanel;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Display, URI, new String[] { "Block" }, new string[] { CIAT.SaveFile.GetIATBlock(parentBlockUri).IndexInContainer.ToString() });
        }

        public void EndPreview(IImageDisplay previewPanel, Uri parentBlockUri)
        {
            Task.Run(() =>
            {
                CIAT.SaveFile.GetDI(ParentBlockUris[parentBlockUri].Item2).PreviewPanel = null;
                previewPanel.Tag = null;
            });
        }

        public Button GUIButton { get; set; }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
            _IsDisposed = true;
            List<Uri> uris = ParentBlockUris.Keys.ToList();
            foreach (Uri u in uris)
            {
                CIAT.SaveFile.DeleteRelationship(this.URI, ParentBlockUris[u].Item2);
                CIAT.SaveFile.GetDI(ParentBlockUris[u].Item2).Dispose();
                CIAT.SaveFile.DeleteRelationship(this.URI, u);
                CIAT.SaveFile.GetIATBlock(u).RemoveItem(this);
            }
            if (_StimulusUri != null)
            {
                try
                {
                    CIAT.SaveFile.DeleteRelationship(this.URI, _StimulusUri);
                    CIAT.SaveFile.GetDI(_StimulusUri).Dispose();
                }
                catch (KeyNotFoundException ex) { }
                _StimulusUri = null;
            }
            CIAT.SaveFile.DeletePart(this.URI);
        }

        public bool IsDisposed
        {
            get
            {
                return _IsDisposed;
            }
        }
    }
}
