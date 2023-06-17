using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace IATClient
{
    public class CTextInstructionScreen : CInstructionScreen
    {
        public override string MimeType
        {
            get
            {
                return "text/xml+" + typeof(CTextInstructionScreen).ToString();
            }
        }


        public override InstructionScreenType Type { get { return InstructionScreenType.Text; } }


        /// <summary>
        /// The default constructor
        /// </summary>
        public CTextInstructionScreen(CInstructionBlock b) : base(b)
        {
            DITextInstructionsScreen instrScreen = new DITextInstructionsScreen();
            InstructionsUri = instrScreen.URI;
        }

        public CTextInstructionScreen(Uri uri)
            : base(uri)
        {
        }

        public override void ValidateItem(Dictionary<IValidatedItem, CValidationException> ErrorDictionary)
        {
            base.ValidateItem(ErrorDictionary);
            DIText tdi = CIAT.SaveFile.GetDI(InstructionsUri) as DIText;
            if (tdi.Phrase.Trim() == String.Empty)
            {
                CLocationDescriptor loc = new CInstructionLocationDescriptor(CIAT.SaveFile.GetInstructionBlock(ParentBlockUri), this);
                ErrorDictionary[this] = new CValidationException(EValidationException.TextInstructionsBlank, loc);
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
                if (_InstructionsUri != null)
                    CIAT.SaveFile.DeleteRelationship(this.URI, _InstructionsUri);
                CIAT.SaveFile.CreateRelationship(BaseType, typeof(DIBase), URI, value);
                DIPreview dic = CIAT.SaveFile.GetDI(PreviewUri) as DIPreview;
                dic.RemoveComponent(LayoutItem.TextInstructionScreen, false);
                _InstructionsUri = value;
                var instructions = CIAT.SaveFile.GetDI(value);
                dic.AddComponent(instructions.IUri, LayoutItem.TextInstructionScreen);
            }
        }


        public override void Validate(int itemNdx)
        {
            base.Validate(itemNdx);
            DIText tdi = CIAT.SaveFile.GetDI(InstructionsUri) as DIText;
            if (tdi.Phrase.Trim() == String.Empty)
                throw new CValidationException(EValidationException.TextInstructionsBlank,
                    new CInstructionLocationDescriptor(CIAT.SaveFile.GetInstructionBlock(ParentBlockUri), this));
        }
        /*
                /// <summary>
                /// Writes the CTextInstructionScreen object to an XmlTextWriter
                /// </summary>
                /// <param name="writer">The XmlTextWriter object to use for output</param>
                public override void WriteToXml(XmlTextWriter writer)
                {
                    if (!IsValid())
                        throw new Exception();

                    writer.WriteStartElement("InstructionScreen");
                    writer.WriteAttributeString("Type", sText);
                    Instructions.WriteToXml(writer);
                    ContinueInstructions.WriteToXml(writer);
                    writer.WriteElementString("ContinueKey", ContinueKey);
                    writer.WriteEndElement();
                }

                /// <summary>
                /// Loads a CTextInstructionScreen object from the passed XmlNode
                /// </summary>
                /// <param name="node">The XmlNode object to use for input</param>
                /// <returns></returns>
                public override bool LoadFromXml(XmlNode node)
                {
                    if (node.ChildNodes.Count != 3)
                        return false;
                    if (!Instructions.LoadFromXml(node.ChildNodes[0]))
                        return false;
                    if (!ContinueInstructions.LoadFromXml(node.ChildNodes[1]))
                        return false;
                    ContinueKey = node.ChildNodes[2].InnerText;
                    return true;
                }
        */
        public override void Save()
        {
            XDocument xDoc = new XDocument();
            String rInstructionsId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(InstructionsUri)).First().Id;
            String rContinueId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(ContinueInstructionsUri)).First().Id;
            String rPreviewId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(DIBase)).Where(pr => pr.TargetUri.Equals(PreviewUri)).First().Id;
            String rParentBlockId = CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CInstructionBlock)).Where(pr => pr.TargetUri.Equals(ParentBlockUri)).First().Id;
            xDoc.Document.Add(new XElement("InstructionScreen", new XAttribute("Type", InstructionScreenType.Text.ToString()), new XElement("rParentBlockId", rParentBlockId),
                new XElement("rInstructionsId", rInstructionsId), new XElement("rContinueInstructionsId", rContinueId), new XElement("rPreviewId", rPreviewId),
                new XElement("ContinueKey", ContinueKey)));
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
            ContinueKey = xDoc.Root.Element("ContinueKey").Value;
            _InstructionsUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rInstructionsId").Value).TargetUri;
            PreviewUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rPreviewId").Value).TargetUri;
            ContinueInstructionsUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rContinueInstructionsId").Value).TargetUri;
            ParentBlockUri = CIAT.SaveFile.GetRelationship(this, xDoc.Root.Element("rParentBlockId").Value).TargetUri;
        }


        public override void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            CIAT.SaveFile.GetDI(PreviewUri).Dispose();
            CIAT.SaveFile.GetInstructionBlock(ParentBlockUri).RemoveScreen(URI);
            CIAT.SaveFile.GetDI(InstructionsUri).Dispose();
            CIAT.SaveFile.GetDI(ContinueInstructionsUri).Dispose();
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
            CIAT.SaveFile.DeletePart(this.URI);
        }
    }
}
