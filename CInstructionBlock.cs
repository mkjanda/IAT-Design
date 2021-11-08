using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.IO;
using System.Drawing;
using System.Windows.Forms;
using System.Threading;
using System.Linq;
using System.Xml.Linq;

namespace IATClient
{
    public class CInstructionBlock : IContentsItem, IValidatedItem, IDisposable, IPackagePart
    {
        private bool _IsDisposed = false;
        private List<Uri> ScreenUris = new List<Uri>();
        protected AlternationGroup AltGroup;
        protected String _Name = String.Empty;
        protected CIAT IAT;
        private readonly DILambdaGenerated PreviewLambdaDI;
        private Uri PreviewUri = null;
        public Uri URI { get; set; }
        public Type BaseType { get { return typeof(CInstructionBlock); } }
        public String MimeType { get { return "text/xml+" + BaseType.ToString(); } }
        public bool IsSurvey { get { return false; } }
        public readonly String rIatId;

        public bool IsHeaderItem { get { return true; } }

        public CInstructionBlock AlternateInstructionBlock
        {
            get
            {
                if (AlternationGroup == null)
                    return null;
                if (AlternationGroup.GroupMembers[0] == this)
                    return (CInstructionBlock)AlternationGroup.GroupMembers[1];
                else
                    return (CInstructionBlock)AlternationGroup.GroupMembers[0];
            }
        }

        private void GeneratePreview(Bitmap bmp)
        {
            Graphics g = Graphics.FromImage(bmp);
            Brush br = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            g.FillRectangle(br, new Rectangle(new Point(0, 0), CIAT.SaveFile.Layout.InteriorSize));
            br.Dispose();
            br = new SolidBrush(CIAT.SaveFile.Layout.BorderColor);
            Font f = new Font(System.Drawing.SystemFonts.DefaultFont.FontFamily, 18F);
            String str;
            if (ScreenUris.Count == 0)
                str = "No Instruction Screens Defined";
            else if (ScreenUris.Count == 1)
                str = "1 Instruction Screen Defined";
            else
                str = String.Format("{0} Instruction Screens Defined", ScreenUris.Count);
            Size szStr = TextRenderer.MeasureText(str, f);
            float ar = CIAT.SaveFile.Layout.InteriorSize.Width / CIAT.SaveFile.Layout.InteriorSize.Height;
            g.DrawString(str, f, br, new PointF((CIAT.SaveFile.Layout.InteriorSize.Width - szStr.Width) / 2, (CIAT.SaveFile.Layout.InteriorSize.Height - szStr.Height) / 2 - (2 * szStr.Height) + (ar > 1 ? (szStr.Height / ar) : (-ar * szStr.Height))));
            br.Dispose();
            f.Dispose();
            g.Dispose();
            bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
        }

        public CInstructionBlock(CIAT theIAT)
        {
            this.URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            CIAT.SaveFile.Register(this);
            rIatId = CIAT.SaveFile.CreateRelationship(typeof(CIAT), typeof(CInstructionBlock), theIAT.URI, this.URI);
            IAT = theIAT;
            PreviewLambdaDI = new DILambdaGenerated(GeneratePreview);
            PreviewLambdaDI.ScheduleInvalidation();

        }

        public CInstructionBlock(CIAT iat, Uri uri)
        {
            this.URI = uri;
            this.IAT = iat;
            rIatId = CIAT.SaveFile.GetRelationshipsByType(iat.URI, typeof(CIAT), BaseType).Where(pr => pr.TargetUri.Equals(uri)).Select(pr => pr.Id).First();
            CIAT.SaveFile.Register(this);
            Load();
            PreviewLambdaDI = new DILambdaGenerated(GeneratePreview);
            PreviewLambdaDI.ScheduleInvalidation();
        }


        public void Validate()
        {
            for (int ctr = 0; ctr < ScreenUris.Count; ctr++)
            {
                CInstructionScreen scrn = CIAT.SaveFile.GetInstructionScreen(ScreenUris[ctr]);
                scrn.Validate(ctr);
            }
        }


        /*
                public void WriteToXml(XmlTextWriter writer)
                {
                    writer.WriteStartElement("InstructionBlock");
                    writer.WriteElementString("IndexInContents", IndexInContents.ToString());
                    writer.WriteElementString("Name", Name);
                    for (int ctr = 0; ctr < InstructionScreens.Count; ctr++)
                        InstructionScreens[ctr].WriteToXml(writer);
                    writer.WriteEndElement();
                }

                public bool LoadFromXml(XmlNode node)
                {
                    ClearCompositeImageDictionary();
                    InstructionScreens.Clear();
                    _IndexInContents = Convert.ToInt32(node.ChildNodes[0].InnerText);
                    Name = node.ChildNodes[1].InnerText;
                    for (int ctr = 2; ctr < node.ChildNodes.Count; ctr++)
                    {
                        AddScreen(CInstructionScreen.CreateFromXml(node.ChildNodes[ctr]));
                        InstructionScreens.Last().ParentBlock = this;
                    }
                    return true;
                }
         */
        public ContentsItemType Type
        {
            get
            {
                return ContentsItemType.InstructionBlock;
            }
        }

        public bool HasAlternateItem
        {
            get
            {
                return (AltGroup != null);
            }
        }

        public AlternationGroup AlternationGroup
        {
            get
            {
                return AltGroup;
            }
            set
            {
                if ((AltGroup != null) && (value != null))
                    if (!AltGroup.URI.Equals(value.URI))
                {
                    MessageBox.Show("Dispose of the alternation group and instantiate a new one.");
                    return;
                }
                AltGroup = value;
            }
        }

        public String Name
        {
            get
            {
                return _Name;
            }
            set
            {
                _Name = value;
            }
        }

        public int IndexInContainer
        {
            get
            {
                return IAT.InstructionBlocks.IndexOf(this);
            }
        }

        public void DeleteFromIAT()
        {
            IAT.DeleteInstructionBlock(this);
        }

        public void AddToIAT(int InsertionNdx)
        {
            int containerNdx = 0;
            for (int ctr = 0; ctr < InsertionNdx; ctr++)
                if (IAT.Contents[ctr].Type == Type)
                    containerNdx++;
            Name = "Instruction Block #" + (containerNdx + 1).ToString();
            IAT.InsertInstructionBlock(this, InsertionNdx);
        }

        protected int _IndexInContents = -1;

        public int IndexInContents
        {
            get
            {
                if (IAT.Contents.Contains(this))
                    return IAT.Contents.IndexOf(this);
                return _IndexInContents;
            }
        }

        public int InstructionBlockNum
        {
            get
            {
                int ndxInContents = IndexInContents;
                int blockCtr = 0;
                for (int ctr = 0; ctr < IAT.Contents.Count; ctr++)
                {
                    if (IAT.Contents[ctr].Type == ContentsItemType.InstructionBlock)
                        blockCtr++;
                    if (ctr == ndxInContents)
                        return blockCtr;
                }
                return -1;
            }
        }

        public CInstructionScreen this[int ndx]
        {
            get
            {
                return CIAT.SaveFile.GetInstructionScreen(ScreenUris[ndx]);
            }
        }

        public int GetIndexOf(Uri u)
        {
            return ScreenUris.IndexOf(u);
        }

        public void RemoveScreen(Uri scrnUri)
        {
            CIAT.SaveFile.DeleteRelationship(URI, scrnUri);
            ScreenUris.Remove(scrnUri);
        }

        public void AddScreen(CInstructionScreen Screen)
        {
            CIAT.SaveFile.CreateRelationship(BaseType, Screen.BaseType, this.URI, Screen.URI);
            ScreenUris.Add(Screen.URI);
        }

        public void InsertScreen(CInstructionScreen Screen, int ndx)
        {
            String rId = CIAT.SaveFile.CreateRelationship(BaseType, Screen.BaseType, this.URI, Screen.URI);
            ScreenUris.Insert(ndx, Screen.URI);
        }


        public void MoveScreen(int startNdx, int endNdx)
        {
            Uri scrnUri = ScreenUris[startNdx];
            ScreenUris.RemoveAt(startNdx);
            if (startNdx <= endNdx)
                ScreenUris.Insert(endNdx - 1, scrnUri);
            else
                ScreenUris.Insert(endNdx, scrnUri);
        }

        public int NumScreens
        {
            get
            {
                return ScreenUris.Count;
            }
        }

        public void ValidateItem(Dictionary<IValidatedItem, CValidationException> ErrorDictionary)
        {
            if (ScreenUris.Count == 0)
                ErrorDictionary.Add(this, new CValidationException(EValidationException.InstructionBlockHasNoItems, new CIATInstructionScreenLocationDescriptor(this, null)));
            foreach (CInstructionScreen scrn in ScreenUris.Select(uri => CIAT.SaveFile.GetInstructionScreen(uri)))
                scrn.ValidateItem(ErrorDictionary);
        }

        public void OpenItem(IATConfigMainForm mainForm)
        {
            mainForm.ActiveItem = this;
            mainForm.FormContents = IATConfigMainForm.EFormContents.Instructions;
            mainForm.SetActiveInstructionScreen(this[0]);
        }

        public List<IPreviewableItem> SubContentsItems
        {
            get
            {
                List<IPreviewableItem> result = new List<IPreviewableItem>();
                result.AddRange(ScreenUris.Select(tup => CIAT.SaveFile.GetInstructionScreen(tup)).ToArray());
                return result;
            }
        }

        public String PreviewText
        {
            get
            {
                return Name;
            }
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("InstructionBlock", new XAttribute("Name", Name), new XAttribute("IndexInContents", IndexInContents.ToString())));
            foreach (Uri scrnUri in ScreenUris)
                xDoc.Root.Add(new XElement("rScreenId", CIAT.SaveFile.GetRelationship(this, scrnUri)));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        protected void Load()
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Name = xDoc.Root.Attribute("Name").Value;
            ScreenUris.Clear();
            foreach (XElement elem in xDoc.Root.Elements("rScreenId"))
            {
                String rId = elem.Value;
                Uri u = CIAT.SaveFile.GetRelationship(this, rId).TargetUri;
                ScreenUris.Add(u);
            }
        }

        public void Preview(IImageDisplay previewPanel)
        {
            if (previewPanel.Tag != null)
                (previewPanel.Tag as IPreviewableItem).EndPreview(previewPanel);
            DIPreview dip = new DIPreview();
            dip.AddComponent(PreviewLambdaDI.IUri);
            dip.PreviewPanel = previewPanel;
            PreviewUri = dip.URI;
            previewPanel.Tag = this;
        }

        public void EndPreview(IImageDisplay previewPanel)
        {
            if (previewPanel.Tag == this)
            {
                previewPanel.SetImage(null as Bitmap);
                previewPanel.Tag = null;
                CIAT.SaveFile.GetDI(PreviewUri).Dispose();
            }
        }

        public void Dispose()
        {
            if (IsDisposed)
                return;
            if (PreviewUri != null)
                if (CIAT.SaveFile.GetDI(PreviewUri).PreviewPanel != null)
                   EndPreview(CIAT.SaveFile.GetDI(PreviewUri).PreviewPanel);
            foreach (var u in ScreenUris)
                RemoveScreen(u);
            List<CInstructionScreen> scrns = ScreenUris.Select(tup => CIAT.SaveFile.GetInstructionScreen(tup)).Where((scrn) => scrn != null).ToList();
            foreach (CInstructionScreen scrn in scrns)
                scrn.Dispose();
            CIAT.SaveFile.DeletePart(this.URI);
            if (AlternationGroup != null)
                AlternationGroup.Remove(this);
            CIAT.ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
            _IsDisposed = true;
        }

        public bool IsDisposed
        {
            get
            {
                return _IsDisposed;
            }
        }

        public Button GUIButton { get; set; }

        public List<CFontFile.FontItem> UtilizedFontFamilies
        {
            get
            {
                List<CInstructionScreen> InstructionScreens = ScreenUris.Select(tup => CIAT.SaveFile.GetInstructionScreen(tup)).ToList();
                List<CFontFile.FontItem> resultList = new List<CFontFile.FontItem>();

                resultList.AddRange(InstructionScreens.Select((scrn, ndx) => new { s = scrn, n = ndx }).Where(t => t.s.Type != InstructionScreenType.Blank).
                    Select((t) => new { idi = CIAT.SaveFile.GetDI(t.s.InstructionsUri) as DIText, ndx = t.n }).
                    Where((t) => t.idi.PhraseFontFamily != String.Empty).Aggregate(new Dictionary<String, List<Tuple<int, DIText>>>(), (dic, t) =>
                    {
                        if (!dic.ContainsKey(t.idi.PhraseFontFamily))
                            dic[t.idi.PhraseFontFamily] = new List<Tuple<int, DIText>>();
                        dic[t.idi.PhraseFontFamily].Add(new Tuple<int, DIText>(t.ndx, t.idi));
                        return dic;
                    }).Select((t) => new CFontFile.FontItem(t.Key, "is used by instruction screen {0} in instruction screen block #" + InstructionBlockNum.ToString(),
                        t.Value.Select(tup => tup.Item1), t.Value.Select(tup => tup.Item2))));
                resultList.AddRange(InstructionScreens.Select((instrScrn, i) => new
                {
                    cdi = CIAT.SaveFile.GetDI(instrScrn.ContinueInstructionsUri) as DIText, ndx = i + 1}).
                    Where((t) => t.cdi.PhraseFontFamily != String.Empty).Aggregate(new Dictionary<String, List<Tuple<int, DIText>>>(), (dic, t) => { 
                        if (!dic.ContainsKey(t.cdi.PhraseFontFamily))
                            dic[t.cdi.PhraseFontFamily] = new List<Tuple<int, DIText>>();
                        dic[t.cdi.PhraseFontFamily].Add(new Tuple<int, DIText>(t.ndx, t.cdi));
                        return dic;
                    }).Select(t => new CFontFile.FontItem(t.Key, "is used by continue instructions in instruction screens {0} in instruction block #" + InstructionBlockNum.ToString(),
                        t.Value.Select(tup => tup.Item1), t.Value.Select(tup => tup.Item2))));
                resultList.AddRange(InstructionScreens.Select((scrn, i) => new { s = scrn, ndx = i + 1 }).Where(t => t.s.Type == InstructionScreenType.MockItem).
                    Where(t => (t.s as CMockItemScreen).StimulusUri != null).Select(t => new { mdi = CIAT.SaveFile.GetDI((t.s as CMockItemScreen).StimulusUri), ndx = t.ndx }).
                    Where(t => t.mdi.Type == DIType.StimulusText).Select(t => new { idi = t.mdi as DIText, ndx = t.ndx }).Where(t => t.idi.PhraseFontFamily != String.Empty).
                    Aggregate(new Dictionary<String, List<Tuple<int, DIText>>>(), (dic, t) =>
                    {
                        if (!dic.ContainsKey(t.idi.PhraseFontFamily))
                            dic[t.idi.PhraseFontFamily] = new List<Tuple<int, DIText>>();
                        dic[t.idi.PhraseFontFamily].Add(new Tuple<int, DIText>(t.ndx, t.idi));
                        return dic;
                    }).Select((t) => new CFontFile.FontItem(t.Key, "is used by instruction screen {0} in instruction screen block #" + InstructionBlockNum.ToString(),
                        t.Value.Select(tup => tup.Item1), t.Value.Select(tup => tup.Item2))));
                return resultList;
            }
        }
    }
}
