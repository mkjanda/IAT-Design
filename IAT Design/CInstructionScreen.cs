using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using System.Xml.Linq;

namespace IATClient
{
    public class InstructionScreenType : Enumeration
    {
        public static readonly InstructionScreenType Blank = new InstructionScreenType(1, "Blank", typeof(CInstructionScreen), new Func<Uri, CInstructionScreen>((uri) => new CInstructionScreen(uri)));
        public static readonly InstructionScreenType Text = new InstructionScreenType(2, "Text", typeof(CTextInstructionScreen), new Func<Uri, CInstructionScreen>((uri) => new CTextInstructionScreen(uri)));
        public static readonly InstructionScreenType ResponseKey = new InstructionScreenType(3, "ResponseKey", typeof(CKeyInstructionScreen), new Func<Uri, CInstructionScreen>((uri) => new CKeyInstructionScreen(uri)));
        public static readonly InstructionScreenType MockItem = new InstructionScreenType(4, "MockItem", typeof(CMockItemScreen), new Func<Uri, CInstructionScreen>((uri) => new CMockItemScreen(uri)));

        private InstructionScreenType(int value, String name, Type t, Func<Uri, CInstructionScreen> c) : base(value, name) 
        { 
            Type = t;
            Create = c;
        }
        private Type Type {get; set;}
        public Func<Uri, CInstructionScreen> Create {get; private set;}
        private readonly IEnumerable<InstructionScreenType> All = new InstructionScreenType[] { Blank, Text, ResponseKey, MockItem };
        public InstructionScreenType FromString(String name)
        {
            return All.Where(ist => ist.Name == name).First();
        }
        public static InstructionScreenType FromTypeName(String tName) {
            if (tName == typeof(CInstructionScreen).ToString())
                return Blank;
            if (tName == typeof(CTextInstructionScreen).ToString())
                return Text;
            if (tName == typeof(CKeyInstructionScreen).ToString())
                return ResponseKey;
            if (tName == typeof(CMockItemScreen).ToString())
                return MockItem;
            return null;
        }
    }


    public class CInstructionScreen : IDisposable, IValidatedItem, IPreviewableItem, IPackagePart, IThumbnailPreviewable
    {
        protected Uri _InstructionsUri { get; set; }
        public virtual Uri InstructionsUri { get; protected set; }
        public bool IsDisposed { get; protected set; }
        public Uri PreviewUri { get; protected set; } = null;
        public Uri URI { get; set; }
        public Type BaseType { get { return typeof(CInstructionScreen); } }
        public virtual String MimeType { get { return "text/xml+" + typeof(CInstructionScreen).ToString(); } }
        public bool IsHeaderItem { get { return false; } }
        public String ContinueKey { get; set; }
        public virtual InstructionScreenType Type { get { return InstructionScreenType.Blank; } }
        public Uri ParentBlockUri { get; protected set; }
        public bool IsSurvey { get { return false; } }
        public int IndexInContainer
        {
            get
            {
                return CIAT.SaveFile.GetInstructionBlock(ParentBlockUri).GetIndexOf(this.URI);
            }
        }

        public virtual Uri ResponseKeyUri { get { throw new NotImplementedException(); } set { throw new NotImplementedException(); } }

        private Uri _ContinueInstructionsUri { get; set; } = null;
        public Uri ContinueInstructionsUri
        {
            get
            {
                return _ContinueInstructionsUri;
            }
            set
            {
                if (_ContinueInstructionsUri != null)
                {
                    if (_ContinueInstructionsUri.Equals(value))
                        return;
                    CIAT.SaveFile.GetDI(_ContinueInstructionsUri).Dispose();
                    _ContinueInstructionsUri = null;
                }
                else if (value == null)
                    return;
                CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), this.URI, value);
                CIAT.SaveFile.CreateRelationship(typeof(DIBase), BaseType, value, this.URI);
                _ContinueInstructionsUri = value;
                var cInstr = CIAT.SaveFile.GetDI(value) as DIContinueInstructions;
                DIPreview.AddComponent(cInstr.IUri, LayoutItem.ContinueInstructions);
            }
        }

        public DIContinueInstructions ContinueInstructions
        {
            get
            {
                return CIAT.SaveFile.GetDI(ContinueInstructionsUri) as DIContinueInstructions;
            }
        }

        /// <summary>
        /// gets the continue key as a character
        /// </summary>
        public char ContinueKeyChar
        {
            get
            {
                if (ContinueKey.Length == 1)
                    return ContinueKey[0];
                else if (ContinueKey == "Space")
                    return ' ';
                else if (ContinueKey == "Enter")
                    return '\n';
                else
                    throw new Exception("Unrecognized continue key character");
            }
        }

        public DIPreview DIPreview
        {
            get
            {
                return CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
            }
        }
            

        public IImageDisplay PreviewPane
        {
            get
            {
                return (CIAT.SaveFile.GetDI(PreviewUri) as DIPreview).PreviewPanel;
            }
            set
            {
                var dip = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                dip.PreviewPanel = value;
            }
        }

        public IImageDisplay ThumbnailPreviewPanel
        {
            get
            {
                return (CIAT.SaveFile.GetDI(PreviewUri) as DIPreview).ThumbnailPreviewPane;
            }
            set
            {
                (CIAT.SaveFile.GetDI(PreviewUri) as DIPreview).ThumbnailPreviewPane = value;
            }
        }

        public CInstructionScreen(CInstructionBlock b)
        {
            this.URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            CIAT.SaveFile.Register(this);
            ParentBlockUri = b.URI;
            DIPreview previewImage = new DIPreview();
            PreviewUri = previewImage.URI;
            CIAT.SaveFile.CreateRelationship(BaseType, typeof(CInstructionBlock), this.URI, b.URI);
            DIContinueInstructions continueInstr = new DIContinueInstructions();
            continueInstr.Phrase = "Press the space key or tap to continue.";
            ContinueInstructionsUri = continueInstr.URI;
            ContinueKey = "Space";
            previewImage.AddComponent(continueInstr.IUri, LayoutItem.ContinueInstructions);
            PreviewUri = previewImage.URI;
            CIAT.SaveFile.CreateRelationship(BaseType, previewImage.BaseType, URI, PreviewUri);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }


        public CInstructionScreen(Uri uri)
        {
            this.URI = uri;
            CIAT.SaveFile.Register(this);
            Load();
        }

        public virtual void Resize()
        {
            throw new NotImplementedException();
        }

        public virtual void Save()
        {
            String rContinueInstructionsId = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(ContinueInstructionsUri)).First().Id;
            String rPreviewId = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(PreviewUri)).First().Id;
            String rParentBlockId = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CInstructionBlock)).Where(pr => pr.TargetUri.Equals(ParentBlockUri)).First().Id;
            XDocument xDoc = new XDocument();
            xDoc.Document.Add(new XElement("InstructionScreen", new XAttribute("Type", InstructionScreenType.Blank.ToString()), new XElement("rParentBlockId", rParentBlockId),
                new XElement("rContinueInstructionsId", rContinueInstructionsId), new XElement("rPreviewId", rPreviewId), new XElement("ContinueKey", ContinueKey)));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public virtual void Load() {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            ParentBlockUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rParentBlockId").Value).TargetUri;
            ContinueInstructionsUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rContinueInstructionsId").Value).TargetUri;
            PreviewUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rPreviewId").Value).TargetUri;
            ContinueKey = xDoc.Root.Element("ContinueKey").Value;
        }

        public virtual void Validate(int itemNdx)
        {
            if (Type == InstructionScreenType.Blank)
                throw new Exception(String.Format(Properties.Resources.sInstructionScreenDefinitionIncompleteException, itemNdx + 1));
            DIText tdi = CIAT.SaveFile.GetDI(ContinueInstructionsUri) as DIText;
            if (tdi.Phrase.Trim() == String.Empty)
                throw new Exception(Properties.Resources.sContinueInstructionsBlankException);
        }

        public virtual void ValidateItem(Dictionary<IValidatedItem, CValidationException> errorDictionary)
        {
            CLocationDescriptor loc = new CInstructionLocationDescriptor(CIAT.SaveFile.GetInstructionBlock(ParentBlockUri), this);
            if (Type == InstructionScreenType.Blank)
            {
                errorDictionary[this] = new CValidationException(EValidationException.InstructionScreenWithoutType, loc);
                return;
            }
            DIText tdi = CIAT.SaveFile.GetDI(ContinueInstructionsUri) as DIText;
            if (tdi.Phrase.Trim() == String.Empty)
                errorDictionary[this] = new CValidationException(EValidationException.ContinueInstructionsBlank, loc);
        }
        
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            PreviewPane?.ClearImage();
            ThumbnailPreviewPanel?.ClearImage();
            PreviewPane = null;
            ThumbnailPreviewPanel = null;
            CIAT.SaveFile.GetInstructionBlock(ParentBlockUri).RemoveScreen(URI);
            CIAT.SaveFile.GetDI(PreviewUri).Dispose();
            CIAT.SaveFile.GetDI(ContinueInstructionsUri).Dispose();
            CIAT.SaveFile.DeletePart(this.URI);
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
        }


        public void OpenItem(IATConfigMainForm mainForm)
        {
            mainForm.ActiveItem = CIAT.SaveFile.GetInstructionBlock(ParentBlockUri);
            mainForm.FormContents = IATConfigMainForm.EFormContents.Instructions;
            mainForm.SetActiveInstructionScreen(this);
        }

        public String PreviewText
        {
            get
            {
                return String.Format("Screen #{0}", CIAT.SaveFile.GetInstructionBlock(ParentBlockUri).GetIndexOf(this.URI) + 1);
            }
        }

        public Button GUIButton { get; set; }

        public void Preview(IImageDisplay previewPanel)
        {
            if (previewPanel.Tag != null)
                (previewPanel.Tag as IPreviewableItem).EndPreview(previewPanel);
            CIAT.SaveFile.GetDI(PreviewUri).PreviewPanel = previewPanel;
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Display, URI, new String[] { "Block" }, new String[] { CIAT.SaveFile.GetInstructionBlock(ParentBlockUri).IndexInContainer.ToString() });
            previewPanel.Tag = this;
        }

        public void EndPreview(IImageDisplay previewPanel)
        {
            CIAT.SaveFile.GetDI(PreviewUri).PreviewPanel = null;
            previewPanel.Tag = null;
        }
    }
}
