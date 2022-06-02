using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IATClient
{
    public class CMockItemScreen : CKeyInstructionScreen
    {
        public override string MimeType
        {
            get
            {
                return "text/xml+" + typeof(CMockItemScreen).ToString();
            }
        }
        public override InstructionScreenType Type { get { return InstructionScreenType.MockItem; } }

        private bool _InvalidResponseFlag = false, _KeyedDirOutlined = false;
        private Uri _StimulusUri = null, _ResponseKeyUri = null, _InstructionsUri = null;
        private KeyedDirection _KeyedDirection = KeyedDirection.None;

        private bool KeyOutlined { get; set; }
        public Uri StimulusUri
        {
            get
            {
                return _StimulusUri;
            }
            set
            {
                if ((value == null) && (StimulusUri == null))
                    return;
                else if (value != null)
                    if (value.Equals(StimulusUri))
                        return;
                DIPreview dic = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                if (StimulusUri != null)
                {
                    DIBase oldStim = CIAT.SaveFile.GetDI(StimulusUri);
                    dic.RemoveComponent(LayoutItem.Stimulus, value == null);
                    CIAT.SaveFile.DeleteRelationship(this.URI, StimulusUri);
                    oldStim.Dispose();
                }
                _StimulusUri = value;
                if (value != null)
                {
                    DIBase newStim = CIAT.SaveFile.GetDI(value);
                    dic.AddComponent(newStim.IUri, LayoutItem.Stimulus);
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), this.URI, value);
                }
            }
        }

        public override Uri ResponseKeyUri
        {
            get
            {
                return _ResponseKeyUri;
            }
            set
            {
                bool wasNull = ResponseKeyUri == null;
                if (value == _ResponseKeyUri)
                    return;
                DIPreview dic = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                bool suspended = dic.LayoutSuspended;
                if (!suspended)
                    dic.SuspendLayout();
                if (ResponseKeyUri != null)
                {
                    CIATKey oldKey = CIAT.SaveFile.GetIATKey(ResponseKeyUri);
                    dic.RemoveComponent(LayoutItem.LeftResponseKey, false);
                    dic.RemoveComponent(LayoutItem.RightResponseKey, false);
                    CIAT.SaveFile.DeleteRelationship(this.URI, ResponseKeyUri);
                    CIAT.SaveFile.DeleteRelationship(ResponseKeyUri, URI);
                }
                _ResponseKeyUri = value;
                if (value != null)
                {
                    CIATKey newKey = CIAT.SaveFile.GetIATKey(value);
                    dic.AddComponent(newKey.LeftValue.IUri, LayoutItem.LeftResponseKey);
                    dic.AddComponent(newKey.RightValue.IUri, LayoutItem.RightResponseKey);
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(CIATKey), this.URI, value);
                    CIAT.SaveFile.CreateRelationship(typeof(CIATKey), BaseType, value, URI);
                    if (KeyOutlined)
                    {
                        if (KeyedDirection == KeyedDirection.Left)
                            dic.RemoveComponent(LayoutItem.LeftResponseKeyOutline, false);
                        if (KeyedDirection == KeyedDirection.Right)
                            dic.RemoveComponent(LayoutItem.RightResponseKeyOutline, false);
                    }
                    dic.ScheduleInvalidation();
                }
                if (!suspended)
                    dic.ResumeLayout(true);
                if (wasNull)
                    CheckKeyOutline();
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
                    CIAT.SaveFile.DeleteRelationship(this.URI, InstructionsUri);
                    dic.RemoveComponent(LayoutItem.MockItemInstructions, value == null);
                }
                _InstructionsUri = value;
                if (value != null)
                {
                    DIBase newInstr = CIAT.SaveFile.GetDI(value);
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), this.URI, value);
                    dic.AddComponent(newInstr.IUri, LayoutItem.MockItemInstructions);
                }
                else
                    throw new ArgumentException("Cannot null a display item component of an instruction screen.");

            }
        }

        public bool KeyedDirOutlined
        {
            get
            {
                return _KeyedDirOutlined;
            }
            set
            {
                if (value == _KeyedDirOutlined)
                    return;
                DIPreview dic = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                _KeyedDirOutlined = value;
                if (value)
                {
                    if (KeyedDirection == KeyedDirection.Left)
                        dic.AddComponent(CIATLayout.ILeftKeyValueOutlineUri, LayoutItem.LeftResponseKeyOutline);
                    if (KeyedDirection == KeyedDirection.Right)
                        dic.AddComponent(CIATLayout.IRightKeyValueOutlineUri, LayoutItem.RightResponseKeyOutline);
                }
                else
                {
                    if (KeyedDirection == KeyedDirection.Left)
                        dic.RemoveComponent(LayoutItem.LeftResponseKeyOutline, false);
                    if (KeyedDirection == KeyedDirection.Right)
                        dic.RemoveComponent(LayoutItem.RightResponseKeyOutline, false);
                    dic.ScheduleInvalidation();
                }
                if (value == true)
                    CheckKeyOutline();
            }
        }

        public bool InvalidResponseFlag
        {
            get
            {
                return _InvalidResponseFlag;
            }
            set
            {
                if (value == _InvalidResponseFlag)
                    return;
                _InvalidResponseFlag = value;
                DIPreview preview = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                if (value)
                    preview.AddComponent(CIATLayout.IErrorMarkUri, LayoutItem.ErrorMark);
                else
                    preview.RemoveComponent(LayoutItem.ErrorMark, true);
            }
        }

        public KeyedDirection KeyedDirection
        {
            get
            {
                return _KeyedDirection;
            }
            set
            {
                if (value == _KeyedDirection)
                    return;
                if (!KeyOutlined)
                {
                    _KeyedDirection = value;
                    return;
                }
                if ((KeyedDirection == KeyedDirection.Left) || (KeyedDirection == KeyedDirection.Right))
                {
                    DIPreview dic = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                    if ((KeyedDirection == KeyedDirection.Left) && (value == KeyedDirection.Right) && KeyedDirOutlined)
                    {
                        dic.RemoveComponent(LayoutItem.LeftResponseKeyOutline, false);
                        dic.AddComponent(CIATLayout.IRightKeyValueOutlineUri, LayoutItem.RightResponseKeyOutline);
                    }
                    else if ((KeyedDirection == KeyedDirection.Left) && KeyedDirOutlined)
                        dic.RemoveComponent(LayoutItem.LeftResponseKeyOutline, true);
                    if ((KeyedDirection == KeyedDirection.Right) && (value == KeyedDirection.Left) && KeyedDirOutlined)
                    {
                        dic.RemoveComponent(LayoutItem.RightResponseKeyOutline, false);
                        dic.AddComponent(CIATLayout.ILeftKeyValueOutlineUri, LayoutItem.LeftResponseKeyOutline);
                    }
                    else if ((KeyedDirection == KeyedDirection.Right) && KeyedDirOutlined)
                        dic.RemoveComponent(LayoutItem.RightResponseKeyOutline, true);
                }
                _KeyedDirection = value;
            }
        }

        private void CheckKeyOutline()
        {
            if (((KeyedDirection == KeyedDirection.Left) || (KeyedDirection == KeyedDirection.Right)) && KeyedDirOutlined && (ResponseKeyUri != null))
            {
                KeyOutlined = true;
                DIPreview dic = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                if (KeyedDirection == KeyedDirection.Left)
                    dic.AddComponent(CIATLayout.ILeftKeyValueOutlineUri, LayoutItem.LeftResponseKeyOutline);
                if (KeyedDirection == KeyedDirection.Right)
                    dic.AddComponent(CIATLayout.IRightKeyValueOutlineUri, LayoutItem.RightResponseKeyOutline);
            }
            else
                KeyOutlined = false;
        }

        public CMockItemScreen(CInstructionBlock b) : base(b)
        {
            InvalidResponseFlag = false;
            KeyOutlined = false;
            StimulusUri = DIBase.DINull.URI;
            ResponseKeyUri = null;
            DIMockItemInstructions instr = new DIMockItemInstructions();
            InstructionsUri = instr.URI;
            DIPreview preview = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
            preview.AddComponent(instr.IUri, LayoutItem.MockItemInstructions);
            preview.AddComponent(DIBase.DINull.IUri, LayoutItem.Stimulus);
        }

        public CMockItemScreen(Uri uri) : base(uri)
        {
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            String rContinueInstructionsId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(ContinueInstructionsUri)).First().Id;
            String rStimulusId = (StimulusUri != null) ? CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(StimulusUri)).First().Id : null;
            String rInstructionsId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(InstructionsUri)).First().Id;
            String rPreviewId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(PreviewUri)).First().Id;
            String rParentBlockId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CInstructionBlock)).Where(pr => pr.TargetUri.Equals(ParentBlockUri)).First().Id;
            String rResponseKeyId = String.Empty;
            if (ResponseKeyUri != null)
                rResponseKeyId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).First().Id;
            xDoc.Document.Add(new XElement("InstructionScreen", new XAttribute("Type", InstructionScreenType.MockItem.ToString()), new XElement("rParentBlockId", rParentBlockId),
                new XElement("rContinueInstructionsId", rContinueInstructionsId), new XElement("rInstructionsId", rInstructionsId),
                new XElement("rPreviewId", rPreviewId)));
            if (ResponseKeyUri != null)
                xDoc.Root.Add(new XElement("rResponseKeyId", rResponseKeyId));
            if (rStimulusId != null)
                xDoc.Root.Add(new XElement("rStimulusId", rStimulusId));
            xDoc.Root.Add(new XElement("ContinueKey", ContinueKey), new XElement("ErrorMarkDisplayed", InvalidResponseFlag.ToString()), new XElement("KeyedDirection", KeyedDirection.ToString()),
                new XElement("KeyedDirOutlined", KeyedDirOutlined.ToString()));
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
            PreviewUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rPreviewId").Value).TargetUri;
            ContinueInstructionsUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rContinueInstructionsId").Value).TargetUri;
            _InstructionsUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rInstructionsId").Value).TargetUri;
            if (xDoc.Root.Element("rStimulusId") != null)
                _StimulusUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rStimulusId").Value).TargetUri;
            else
                _StimulusUri = null;
            ParentBlockUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rParentBlockId").Value).TargetUri;
            if (xDoc.Root.Element("rResponseKeyId") == null)
                _ResponseKeyUri = null;
            else
            {
                _ResponseKeyUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rResponseKeyId").Value).TargetUri;
                if (CIAT.SaveFile.GetRelationship(CIAT.SaveFile.GetIATKey(ResponseKeyUri), this) == null)
                    CIAT.SaveFile.CreateRelationship(typeof(CIATKey), BaseType, ResponseKeyUri, URI);
                if (CIAT.SaveFile.GetRelationship(this, CIAT.SaveFile.GetIATKey(ResponseKeyUri)) == null)
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(CIATKey), URI, ResponseKeyUri);
            }
            ContinueKey = xDoc.Root.Element("ContinueKey").Value;
            InvalidResponseFlag = Convert.ToBoolean(xDoc.Root.Element("ErrorMarkDisplayed").Value);
            KeyedDirection = KeyedDirection.FromString(xDoc.Root.Element("KeyedDirection").Value);
            KeyedDirOutlined = Convert.ToBoolean(xDoc.Root.Element("KeyedDirOutlined").Value);
        }

        public override void Validate(int itemNdx)
        {
            base.Validate(itemNdx);
            if (StimulusUri == null)
                throw new Exception(String.Format(Properties.Resources.sNoStimulusForMockItemException, itemNdx + 1));
            DIBase di = CIAT.SaveFile.GetDI(StimulusUri);
            DIText tdi;
            if (di.Type == DIType.Null)
                throw new Exception(String.Format(Properties.Resources.sNoStimulusForMockItemException, itemNdx + 1));
            else if (di.Type == DIType.StimulusText)
            {
                tdi = di as DIText;
                if (tdi.Phrase.Trim() == String.Empty)
                    throw new Exception(String.Format(Properties.Resources.sNoStimulusForMockItemException, itemNdx + 1));
            }
            if (ResponseKeyUri == null)
                throw new Exception(String.Format(Properties.Resources.sNoResponseKeySelectedForMockItemException, itemNdx + 1));
        }


        public override void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            CIAT.SaveFile.GetInstructionBlock(ParentBlockUri).RemoveScreen(URI);
            CIAT.SaveFile.GetDI(PreviewUri).Dispose();
            CIAT.SaveFile.GetDI(InstructionsUri).Dispose();
            if (StimulusUri != null)
                CIAT.SaveFile.GetDI(StimulusUri).Dispose();
            CIAT.SaveFile.GetDI(ContinueInstructionsUri).Dispose();
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
            CIAT.SaveFile.DeletePart(this.URI);
        }

        public override void ValidateItem(Dictionary<IValidatedItem, CValidationException> ErrorDictionary)
        {
            base.ValidateItem(ErrorDictionary);
            CLocationDescriptor loc = new CInstructionLocationDescriptor(CIAT.SaveFile.GetInstructionBlock(ParentBlockUri), this);
            CValidationException ex = null;
            if (ResponseKeyUri == null)
                ex = new CValidationException(EValidationException.MockItemScreenWithoutResponseKey, loc);
            else if (StimulusUri == null)
                ex = new CValidationException(EValidationException.MockItemScreenWithoutStimulus, loc);
            else if (CIAT.SaveFile.GetDI(StimulusUri).Type == DIType.Null)
                ex = new CValidationException(EValidationException.MockItemScreenWithoutStimulus, loc);
            else if (CIAT.SaveFile.GetDI(StimulusUri).Type == DIType.StimulusText)
            {
                DIText tdi = CIAT.SaveFile.GetDI(StimulusUri) as DIText;
                if (tdi.Phrase.Trim() == String.Empty)
                    ex = new CValidationException(EValidationException.MockItemScreenWithIncompletelyInitializedTextStimulus, loc);
            }
            if (ex != null)
                ErrorDictionary[this] = ex;
        }
    }
}

