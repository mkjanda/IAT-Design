using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing;
using System.Xml;
using System.Windows.Forms;
using System.Xml.Linq;

namespace IATClient
{
    public class CKeyInstructionScreen : CInstructionScreen
    {
        private Uri _ResponseKeyUri = null;

        public override string MimeType
        {
            get
            {
                return "text/xml+" + typeof(CKeyInstructionScreen).ToString();
            }
        }
        public override InstructionScreenType Type { get { return InstructionScreenType.ResponseKey; } }
        public override Uri ResponseKeyUri
        {
            get
            {
                return _ResponseKeyUri;
            }
            set
            {
                DIPreview dic = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                bool suspended = dic.LayoutSuspended;
                if (!suspended)
                    dic.SuspendLayout();
                if (_ResponseKeyUri != null)
                {
                    dic.RemoveComponent(LayoutItem.LeftResponseKey, false);
                    dic.RemoveComponent(LayoutItem.RightResponseKey, false);
                    CIAT.SaveFile.DeleteRelationship(this.URI, ResponseKeyUri);
                    CIAT.SaveFile.DeleteRelationship(ResponseKeyUri, this.URI);
                }
                _ResponseKeyUri = value;
                if (_ResponseKeyUri != null)
                {
                    CIATKey newKey = CIAT.SaveFile.GetIATKey(_ResponseKeyUri);
                    IUri diu = newKey.LeftValue.IUri;
                    dic.AddComponent(diu, LayoutItem.LeftResponseKey);
                    diu = newKey.RightValue.IUri;
                    dic.AddComponent(diu, LayoutItem.RightResponseKey);
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(CIATKey), this.URI, value);
                    CIAT.SaveFile.CreateRelationship(typeof(CIATKey), BaseType, value, this.URI);
                }
                if (!suspended)
                    dic.ResumeLayout(true);
            }
        }

        public override Uri InstructionsUri
        {
            get
            {
                return _InstructionsUri;
            }
            protected set
            {
                DIPreview dic = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                if (InstructionsUri != null)
                {
                    DIBase oldInstr = CIAT.SaveFile.GetDI(InstructionsUri);
                    dic.RemoveComponent(LayoutItem.KeyedInstructionScreen, value == null);
                    CIAT.SaveFile.DeleteRelationship(this.URI, InstructionsUri);
                }
                _InstructionsUri = value;
                if (InstructionsUri != null)
                {
                    DIBase newInstr = CIAT.SaveFile.GetDI(value);
                    dic.AddComponent(newInstr.IUri, LayoutItem.KeyedInstructionScreen);
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), this.URI, value);
                }
            }
        }

        public CKeyInstructionScreen(CInstructionBlock b) : base(b)
        {
            ResponseKeyUri = null;
            DIKeyedInstructionsScreen instructions = new DIKeyedInstructionsScreen();
            InstructionsUri = instructions.URI;
            CIAT.SaveFile.CreateRelationship(BaseType, instructions.BaseType, URI, instructions.URI);
            (CIAT.SaveFile.GetDI(PreviewUri) as DIPreview).AddComponent(instructions.IUri, LayoutItem.KeyedInstructionScreen);
            (CIAT.SaveFile.GetDI(PreviewUri) as DIPreview).ScheduleInvalidation();
        }

        public CKeyInstructionScreen(Uri uri) : base(uri) 
        {
        }

        public override void Validate(int itemNdx)
        {
            base.Validate(itemNdx);
            if (ResponseKeyUri == null)
                throw new Exception(String.Format(Properties.Resources.sNoKeyAssignedToInstructionScreen, itemNdx + 1));
            DIText tdi = CIAT.SaveFile.GetDI(InstructionsUri) as DIText;
            if (tdi.Phrase.Trim() == String.Empty)
                throw new Exception(String.Format(Properties.Resources.sInstructionScreenEmpty, itemNdx + 1));
        }


        public override void Save() {
            XDocument xDoc = new XDocument();
            String rKey = String.Empty;
            if (ResponseKeyUri != null) {
                rKey = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Where(pr => pr.TargetUri.Equals(ResponseKeyUri)).First().Id;
            }
            String rInstructionsId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(InstructionsUri)).First().Id;
            String rContinueInstructionsId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(ContinueInstructionsUri)).First().Id;
            String rPreviewId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(PreviewUri)).First().Id;
            String rParentBlockId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CInstructionBlock)).Where(pr => pr.TargetUri.Equals(ParentBlockUri)).First().Id;
            xDoc.Document.Add(new XElement("InstructionScreen", new XAttribute("Type", InstructionScreenType.ResponseKey.ToString()), new XElement("rParentBlockId", rParentBlockId),
                new XElement("rContinueInstructionsId", rContinueInstructionsId), new XElement("rInstructionsId", rInstructionsId), 
                new XElement("rPreviewId", rPreviewId), new XElement("ContinueKey", ContinueKey)));
            if (rKey != String.Empty)
                xDoc.Root.Add(new XElement("rResponseKeyId", rKey));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public override void Load()
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            base.PreviewUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rPreviewId").Value).TargetUri;
            base.ParentBlockUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rParentBlockId").Value).TargetUri;
            ContinueInstructionsUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rContinueInstructionsId").Value).TargetUri;
            _InstructionsUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rInstructionsId").Value).TargetUri;
            ContinueKey = xDoc.Root.Element("ContinueKey").Value;
            if (xDoc.Root.Element("rResponseKeyId") != null)
            {
                _ResponseKeyUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rResponseKeyId").Value).TargetUri;
                if (CIAT.SaveFile.GetRelationship(CIAT.SaveFile.GetIATKey(ResponseKeyUri), this) == null)
                    CIAT.SaveFile.CreateRelationship(typeof(CIATKey), BaseType, ResponseKeyUri, URI);
            }
            else
                _ResponseKeyUri = null;
        }

        public override void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            CIAT.SaveFile.GetInstructionBlock(ParentBlockUri).RemoveScreen(URI);
            CIAT.SaveFile.GetDI(PreviewUri).Dispose();
            CIAT.SaveFile.GetDI(InstructionsUri).Dispose();
            CIAT.SaveFile.GetDI(ContinueInstructionsUri).Dispose();
            CIAT.SaveFile.DeletePart(this.URI);
            CIAT.ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
        }

        public override void ValidateItem(Dictionary<IValidatedItem, CValidationException> ErrorDictionary)
        {
            base.ValidateItem(ErrorDictionary);
            CLocationDescriptor loc = new CIATInstructionScreenLocationDescriptor(CIAT.SaveFile.GetInstructionBlock(ParentBlockUri), this);
            if (ResponseKeyUri == null)
                ErrorDictionary[this] = new CValidationException(EValidationException.KeyInstructionScreenWithoutResponseKey, loc);
            else
            {
                DIText tdi = CIAT.SaveFile.GetDI(InstructionsUri) as DIText;
                if (tdi.Phrase.Trim() == String.Empty)
                    ErrorDictionary[this] = new CValidationException(EValidationException.TextInstructionsBlank, loc);
            }
        }
    }
}
