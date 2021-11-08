using System;
using System.Reflection;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace IATClient
{
    public class CIAT : IDisposable, IPackagePart
    {
        public Type BaseType { get { return typeof(CIAT); } }
        public Uri URI { get; set; }
        public String MimeType { get { return "text/xml+" + typeof(CIAT).ToString(); } }
        public ETokenType TokenType { get; set; }
        public String TokenName { get; set; }
        public static SaveFile SaveFile { get; private set; } = null;
        public static readonly ActivityLog ActivityLog = new ActivityLog();

        public static bool Open(String filename, bool compressed, bool hidden)
        {
            try
            {
                CIAT.SaveFile = new SaveFile(filename, compressed, hidden);
                SaveFile.ImageManager.StartWorkers();
                CFontFile.FontItem[] missingFonts = CIAT.SaveFile.CheckForMissingFonts().ToArray();
                foreach (var fi in missingFonts)
                    if ((fi.ImageData.Length == 1) && (fi.ImageData[0] == 0))
                        return true;
                if (missingFonts.Length > 0)
                {
                    MissingFontForm mff = new MissingFontForm(missingFonts);
                    if (mff.ShowDialog() == DialogResult.OK)
                    {
                        Task.Run(() =>
                        {
                            String[] replacementFamilies = mff.GetReplacementFontFamilies();
                            for (int ctr = 0; ctr < missingFonts.Length; ctr++)
                            {
                                missingFonts[ctr].SetReplacementFontFamily(replacementFamilies[ctr]);
                                missingFonts[ctr].Dispose();
                            }
                        });
                    }
                }
                CIAT.SaveFile.ClearDIDictionary();
                return true;
            }
            catch (InvalidSaveFileException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                MessageBox.Show("This is not a valid save file.", "Invalid File");
                return false;
            }
        }

        public static void Recover()
        {
            CIAT.SaveFile = new SaveFile(SaveFile.RecoveryFilePath, false, false);
            CIAT.SaveFile.ImageManager.StartWorkers();
        }
        static CIAT() { }
        public static void Create()
        {
            SaveFile?.Dispose();
            SaveFile = new SaveFile();
            SaveFile.IAT = new CIAT();
            SaveFile.ImageManager.StartWorkers();
        }

        private IATConfigMainForm MainForm
        {
            get
            {
                return Application.OpenForms[Properties.Resources.sMainFormName] as IATConfigMainForm;
            }
        }

        public readonly Dictionary<int, AlternationGroup> AlternationGroups = new Dictionary<int, AlternationGroup>();
        private CUniqueResponse _UniqueResponse = null;
        public ContentsList Contents { get; private set; } = new ContentsList();
        public String Name { get; set; }
        public String LeftResponseChar { get; set; }
        public String RightResponseChar { get; set; }
        public String RedirectionURL { get; set; }
        public enum ESelfAlternationType { none, prepended, postpended, rotated };
        public ESelfAlternationType AlternationType { get; set; }
        static public EventDispatcher.ApplicationEventDispatcher Dispatcher = new EventDispatcher.ApplicationEventDispatcher();
        private List<Uri> _InstructionBlocks { get; set; } = new List<Uri>();
        private List<Uri> _Blocks { get; set; } = new List<Uri>();
        private List<Uri> _BeforeSurvey { get; set; } = new List<Uri>();
        private List<Uri> _AfterSurvey { get; set; } = new List<Uri>();

        public bool Is7Block { get { return _Blocks.Count == 7; } }
        static public Images.ImageManager ImageManager
        {
            get
            {
                return SaveFile.ImageManager;
            }
        }

        public IList<CIATBlock> Blocks
        {
            get
            {
                return _Blocks.Select(u => CIAT.SaveFile.GetIATBlock(u)).ToList().AsReadOnly();
            }
        }
        public IList<CInstructionBlock> InstructionBlocks
        {
            get
            {
                return _InstructionBlocks.Select(u => CIAT.SaveFile.GetInstructionBlock(u)).ToList().AsReadOnly();
            }
        }        
        public IList<CSurvey> BeforeSurvey
        {
            get
            {
                return _BeforeSurvey.Select(u => CIAT.SaveFile.GetSurvey(u)).ToList().AsReadOnly();
            }
        }
        public IList<CSurvey> AfterSurvey
        {
            get
            {
                return _AfterSurvey.Select(u => CIAT.SaveFile.GetSurvey(u)).ToList().AsReadOnly();
            }
        }
        private readonly object UniqueResponseLock = new object();
        public CUniqueResponse UniqueResponse
        {
            get
            {
                lock (UniqueResponseLock)
                {
                    if (_UniqueResponse == null)
                    {
                        try
                        {
                            _UniqueResponse = new CUniqueResponse(CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CUniqueResponse)).Select(pr => pr.TargetUri).First());
                            return _UniqueResponse;
                        }
                        catch (Exception)
                        {
                            _UniqueResponse = new CUniqueResponse();
                            CIAT.SaveFile.CreateRelationship(BaseType, typeof(CUniqueResponse), this.URI, _UniqueResponse.URI);
                            return _UniqueResponse;
                        }
                    }
                    else if (_UniqueResponse.IsDisposed)
                    {
                        _UniqueResponse = new CUniqueResponse();
                        CIAT.SaveFile.CreateRelationship(BaseType, typeof(CUniqueResponse), URI, _UniqueResponse.URI);
                    }
                    return _UniqueResponse;
                }
            } 
            set
            {
                lock (UniqueResponseLock)
                {
                    if (_UniqueResponse != null)
                        _UniqueResponse.Dispose();
                    _UniqueResponse = value;
                    CIAT.SaveFile.CreateRelationship(BaseType, typeof(CUniqueResponse), URI, _UniqueResponse.URI);
                }
            }
        }


        public void MoveContentsItem(IContentsItem ci, int diff)
        {
            if (diff == 0)
                return;
            if (!Contents.Contains(ci))
                throw new InvalidOperationException();
            int ndx = Contents.IndexOf(ci);
            if (ndx + diff < 0)
                return;
            if (ndx + diff >= Contents.Count)
                return;
            if ((ci.Type == ContentsItemType.BeforeSurvey) && (ndx + diff >= BeforeSurvey.Count)) {
                (ci as CSurvey).Ordinality = CSurvey.EOrdinality.After;
                int newNdx = (_AfterSurvey.Count == 0) ? (Contents.Count) : (ndx + diff + (AfterSurvey[0].IndexInContents - 1));
                _AfterSurvey.Insert((ndx + diff) - BeforeSurvey.Count, ci.URI);
                _BeforeSurvey.Remove(ci.URI);
                Contents.Remove(ci);
                Contents.Insert(newNdx - 1, ci);
            }
            else if ((ci.Type == ContentsItemType.AfterSurvey) && (ndx + diff - AfterSurvey[0].IndexInContents < 0)) {
                (ci as CSurvey).Ordinality = CSurvey.EOrdinality.Before;
                int newNdx = (_BeforeSurvey.Count == 0) ? 0 : (ndx + diff - AfterSurvey[0].IndexInContents + BeforeSurvey.Count + 1);
                _BeforeSurvey.Insert((ndx + diff) - AfterSurvey[0].IndexInContents + BeforeSurvey.Count + 1, ci.URI);
                _AfterSurvey.Remove(ci.URI);
                Contents.Remove(ci);
                Contents.Insert(newNdx, ci);
            }
            else if (((ci.Type != ContentsItemType.IATBlock) && (ci.Type != ContentsItemType.InstructionBlock)) || 
                ((diff + ndx <= Contents.Where(c => (c.Type == ContentsItemType.InstructionBlock) ||
                (c.Type == ContentsItemType.IATBlock)).Select(c => c.IndexInContents).Max())
                && (diff + ndx >= _BeforeSurvey.Count)))
            {
                Contents.Remove(ci);
                if (ndx + diff < 0)
                    return;
                else if (ndx - 1 + diff > Contents.Count)
                    Contents.Add(ci);
                else if (diff < 0)
                    Contents.Insert(ndx + diff, ci);
                else
                    Contents.Insert(ndx + diff, ci);
                int ndxInContainer = Contents.Where(c => !c.URI.Equals(ci.URI)).Where(c => c.Type == ci.Type).Where(c => c.IndexInContents <= ndx).Count();
                if (ci.Type == ContentsItemType.IATBlock)
                {
                    _Blocks.Remove(ci.URI);
                    _Blocks.Insert(ndxInContainer, ci.URI);
                }
                if (ci.Type == ContentsItemType.InstructionBlock)
                {
                    _InstructionBlocks.Remove(ci.URI);
                    _InstructionBlocks.Insert(ndxInContainer, ci.URI);
                }
                if (ci.Type == ContentsItemType.BeforeSurvey)
                {
                    _BeforeSurvey.Remove(ci.URI);
                    _BeforeSurvey.Insert(ndxInContainer, ci.URI);
                }
                if (ci.Type == ContentsItemType.AfterSurvey)
                {
                    _AfterSurvey.Remove(ci.URI);
                    _AfterSurvey.Insert(ndxInContainer, ci.URI);
                }
            }
        }
        public void ReplaceSurvey(CSurvey newSurvey, CSurvey oldSurvey)
        {
            if (_BeforeSurvey.Contains(oldSurvey.URI))
            {
                int ndx = _BeforeSurvey.IndexOf(oldSurvey.URI);
                _BeforeSurvey.RemoveAt(ndx);
                _BeforeSurvey.Insert(ndx, newSurvey.URI);
            }
            if (_AfterSurvey.Contains(oldSurvey.URI))
            {
                int ndx = _AfterSurvey.IndexOf(oldSurvey.URI);
                _AfterSurvey.RemoveAt(ndx);
                _AfterSurvey.Insert(ndx, newSurvey.URI);
            }
        }

        public void DeleteContentsItem(IContentsItem ci)
        {
            if (ci.Type == ContentsItemType.BeforeSurvey)
                _BeforeSurvey.Remove(ci.URI);
            else if (ci.Type == ContentsItemType.AfterSurvey)
                _AfterSurvey.Remove(ci.URI);
            else if (ci.Type == ContentsItemType.IATBlock)
                _Blocks.Remove(ci.URI);
            else if (ci.Type == ContentsItemType.InstructionBlock)
                _InstructionBlocks.Remove(ci.URI);
            Contents.Remove(ci);
            CIAT.SaveFile.DeleteRelationship(URI, ci.URI);
            ci.Dispose();
        }
        public void AddIATBlock(CIATBlock b)
        {
            _Blocks.Add(b.URI);
            Contents.Add(b);
            CIAT.SaveFile.CreateRelationship(this.BaseType, b.BaseType, this.URI, b.URI);
        }
        public void InsertIATBlock(CIATBlock b, int contentsNdx)
        {
            int blockNdx = 0;
            for (int ctr = 0; ctr < contentsNdx; ctr++)
                if (Contents[ctr].Type == ContentsItemType.IATBlock)
                    blockNdx++;
            _Blocks.Insert(blockNdx, b.URI);
            Contents.Insert(contentsNdx, b);
            CIAT.SaveFile.CreateRelationship(this.BaseType, b.BaseType, this.URI, b.URI);
        }
        public void DeleteIATBlock(CIATBlock b)
        {
            _Blocks.Remove(b.URI);
            Contents.Remove(b);
            CIAT.SaveFile.DeleteRelationship(this.URI, b.URI);
            b.Dispose();
        }
        public void AddInstructionBlock(CInstructionBlock b)
        {
            _InstructionBlocks.Add(b.URI);
            Contents.Add(b);
            CIAT.SaveFile.CreateRelationship(BaseType, b.BaseType, URI, b.URI);
        }
        public void InsertInstructionBlock(CInstructionBlock b, int contentsNdx)
        {
            int blockNum = 0;
            for (int ctr = 0; ctr < contentsNdx; ctr++)
                if (Contents[ctr].Type == ContentsItemType.InstructionBlock)
                    blockNum++;
            _InstructionBlocks.Insert(blockNum, b.URI);
            Contents.Insert(contentsNdx, b);
        }
        public void DeleteInstructionBlock(CInstructionBlock b)
        {
            Contents.Remove(b);
            _InstructionBlocks.Remove(b.URI);
            CIAT.SaveFile.DeleteRelationship(URI, b.URI);
            b.Dispose();
        }
        public void AddBeforeSurvey(CSurvey s)
        {
            _BeforeSurvey.Add(s.URI);
            CIAT.SaveFile.CreateRelationship(BaseType, s.BaseType, this.URI, s.URI);
        }
        public void InsertBeforeSurvey(CSurvey s, int contentsNdx)
        {
            int blockNum = 0;
            for (int ctr = 0; ctr < contentsNdx; ctr++)
                if (Contents[ctr].Type == ContentsItemType.BeforeSurvey)
                    blockNum++;
            _BeforeSurvey.Insert(blockNum, s.URI);
            Contents.Insert(contentsNdx, s);
        }
        public void DeleteBeforeSurvey(CSurvey survey)
        {
            Contents.Remove(survey);
            _BeforeSurvey.Remove(survey.URI);
            CIAT.SaveFile.DeleteRelationship(this.URI, survey.URI);
            survey.Dispose();
        }
        public void CreateAfterSurvey(CSurvey s)
        {
            _AfterSurvey.Add(s.URI);
            Contents.Add(s);
            CIAT.SaveFile.CreateRelationship(this.BaseType, s.BaseType, this.URI, s.URI);
        }
        public void InsertAfterSurvey(CSurvey s, int contentsNdx)
        {
            int blockNum = 0;
            for (int ctr = 0; ctr < contentsNdx; ctr++)
                if (Contents[ctr].Type == ContentsItemType.AfterSurvey)
                    blockNum++;
            _AfterSurvey.Insert(blockNum, s.URI);
            Contents.Insert(contentsNdx, s);
        }
        public void DeleteAfterSurvey(CSurvey survey)
        {
            Contents.Remove(survey);
            _AfterSurvey.Remove(survey.URI);
            CIAT.SaveFile.DeleteRelationship(this.URI, survey.URI);
            survey.Dispose();
        }
        private readonly object SurveyIndiciesLock = new object();
        private SurveyIndicies _SurveyIndicies = null;
        public SurveyIndicies SurveyIndicies
        {
            get
            {
                lock (SurveyIndiciesLock)
                {
                    if (_SurveyIndicies == null)
                    {
                        Uri indiciesUri = CIAT.SaveFile.GetRelationshipsByType(URI, typeof(CIAT), typeof(SurveyIndicies)).Select(pr => pr.TargetUri).FirstOrDefault();
                        if (indiciesUri == null)
                        {
                            _SurveyIndicies = new SurveyIndicies();
                            CIAT.SaveFile.CreateRelationship(BaseType, typeof(SurveyIndicies), URI, _SurveyIndicies.URI);
                        }
                        else
                            _SurveyIndicies = new SurveyIndicies(indiciesUri);
                    }
                    return _SurveyIndicies;
                }
            }
        }

        /*
        public void ReplaceBeforeSurvey(int ndx, CSurvey survey)
        {
            CSurvey old = BeforeSurvey[ndx];
            CIAT.SaveFile.DeleteRelationship(this.URI, old.URI);
            old.Dispose();
            _BeforeSurvey[ndx] = survey.URI;
            Contents[ndx] = survey;
            CIAT.SaveFile.CreateRelationship(BaseType, survey.BaseType, URI, survey.URI);
        }
        public void ReplaceAfterSurvey(int ndx, CSurvey survey)
        {
            CSurvey old = AfterSurvey[ndx];
            CIAT.SaveFile.DeleteRelationship(this.URI, old.URI);
            old.Dispose();
            _AfterSurvey[ndx] = survey.URI;
            Contents[ndx] = survey;
            CIAT.SaveFile.CreateRelationship(BaseType, survey.BaseType, URI, survey.URI);
        }
        */

        public int GetNumItems()
        {
            int nItems = 0;
            for (int ctr = 0; ctr < Blocks.Count; ctr++)
                nItems += Blocks[ctr].NumItems;
            for (int ctr = 0; ctr < InstructionBlocks.Count; ctr++)
                nItems += InstructionBlocks[ctr].NumScreens;
            return nItems;
        }
        public int NumNonDualKeyBlocks
        {
            get
            {
                return Blocks.Where(b => (b.Key != null)).Where(b => b.Key.KeyType != IATKeyType.DualKey).Count();
            }
        }

        public CIAT()
        {
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            TokenType = ETokenType.NONE;
            ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }

        public CIAT(Uri uri)
        {
            this.URI = uri;
            Load();
            ActivityLog.LogEvent(ActivityLog.EventType.Create, URI);
        }

        public void Dispose()
        {
            _BeforeSurvey.Clear();
            _AfterSurvey.Clear();
            _Blocks.Clear();
            _InstructionBlocks.Clear();
            CDynamicSpecifier.ClearSpecifierDictionary();
            Contents.Clear();
            Name = String.Empty;
            ActivityLog.LogEvent(ActivityLog.EventType.Delete, URI);
        }

        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("IAT", new XElement("Name", Name)));
            if (_UniqueResponse != null)
            {
                String uniqueRId = CIAT.SaveFile.GetRelationship(this, UniqueResponse);
                xDoc.Root.Add(new XElement("UniqueResponseRelId", uniqueRId));
                UniqueResponse.Save();
            }
            XElement xElem = new XElement("Contents");
            foreach (IContentsItem iItem in Contents)
            {
                String rId = CIAT.SaveFile.GetRelationship(this, iItem);
                if (iItem.Type == ContentsItemType.BeforeSurvey)
                    xElem.Add(new XElement(ContentsItemType.BeforeSurvey.ToString(), rId));
                else if (iItem.Type == ContentsItemType.AfterSurvey)
                    xElem.Add(new XElement(ContentsItemType.AfterSurvey.ToString(), rId));
                else if (iItem.Type == ContentsItemType.IATBlock)
                    xElem.Add(new XElement(ContentsItemType.IATBlock.ToString(), rId));
                else if (iItem.Type == ContentsItemType.InstructionBlock)
                    xElem.Add(new XElement(ContentsItemType.InstructionBlock.ToString(), rId));
            }
            xDoc.Root.Add(xElem);
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
            foreach (PackageRelationship pr in CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CFontFile.FontItem)).ToList())
                CIAT.SaveFile.GetFontItem(pr.TargetUri).Dispose();
            List<CFontFile.FontItem> fontItems = UtilizedFonts;
            if (fontItems.Count > 0)
                foreach (var fi in fontItems)
                    fi.Save();
            SurveyIndicies.Save();
        }

        public void Load()
        {
            Stream s = SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Name = xDoc.Root.Element("Name").Value;
            foreach (XElement xElem in xDoc.Root.Element("Contents").Elements())
            {
                String rId = xElem.Value;
                Uri itemUri = SaveFile.GetRelationship(this, xElem.Value).TargetUri;
               
                if (ContentsItemType.Parse(xElem.Name.LocalName) == ContentsItemType.BeforeSurvey)
                {
                    _BeforeSurvey.Add(itemUri);
                    Contents.Add(ContentsItemType.BeforeSurvey, itemUri);
                }
                if (ContentsItemType.Parse(xElem.Name.LocalName) == ContentsItemType.AfterSurvey)
                {
                    _AfterSurvey.Add(itemUri);
                    Contents.Add(ContentsItemType.AfterSurvey, itemUri);
                }
                
                if (ContentsItemType.Parse(xElem.Name.LocalName) == ContentsItemType.IATBlock)
                {
                    _Blocks.Add(itemUri);
                    Contents.Add(ContentsItemType.IATBlock, itemUri);
                }
                if (ContentsItemType.Parse(xElem.Name.LocalName) == ContentsItemType.InstructionBlock)
                {
                    _InstructionBlocks.Add(itemUri);
                    Contents.Add(ContentsItemType.InstructionBlock, itemUri);
                }
            }
        }

        /// <summary>
        /// Calculates the number of distinct items in the IAT
        /// </summary>
        /// <returns>the number of distinct items in the IAT</returns>
        public int CountItems()
        {
            int nItems = 0;
            for (int ctr = 0; ctr < Blocks.Count; ctr++)
                nItems += Blocks[ctr].NumItems;

            return nItems;
        }

        /// <summary>
        /// Returns the 1-based block number of the given item in the IAT
        /// </summary>
        /// <param name="ItemNum">the 1-based index of the item</param>
        /// <returns>the 1-based index of the block that contins the item</returns>
        public int GetBlockNumber(int ItemNum)
        {
            int ctr1 = 0;
            int BlockCtr = 1;
            int ItemCtr = 1;
            for (ctr1 = 0; ctr1 < Contents.Count; ctr1++)
            {
                if (Contents[ctr1].Type == ContentsItemType.IATBlock)
                {
                    if (ItemCtr + ((CIATBlock)Contents[ctr1]).NumItems > ItemNum)
                        return BlockCtr;
                    BlockCtr++;
                    ItemCtr += ((CIATBlock)Contents[ctr1]).NumItems;
                }
            }
            return -1;
        }

        public void MakeAfterSurvey(CSurvey s)
        {
            if (!BeforeSurvey.Contains(s))
                return;
            _BeforeSurvey.Remove(s.URI);
            s.Ordinality = CSurvey.EOrdinality.After;
            _AfterSurvey.Insert(0, s.URI);

        }

        public void MakeBeforeSurvey(CSurvey s)
        {
            if (!AfterSurvey.Contains(s))
                return;
            _AfterSurvey.Remove(s.URI);
            s.Ordinality = CSurvey.EOrdinality.Before;
            _BeforeSurvey.Add(s.URI);
        }
/*
        public int GetBlockIndex(CIATBlock block)
        {
            int ctr = 0;
            int nBlockCtr = 0;
            while (ctr < Contents.Count)
            {
                IContentsItem cItem = Contents[ctr++];
                if (cItem.Type == ContentsItemType.IATBlock)
                {
                    nBlockCtr++;
                    if (block == (CIATBlock)cItem)
                        return nBlockCtr;
                }
            }
            return -1;
        }

        public int GetInstructionBlockIndex(CInstructionBlock block)
        {
            int ctr = 0;
            int nBlockCtr = 0;
            while (ctr < Contents.Count)
            {
                IContentsItem cItem = Contents[ctr++];
                if (cItem.Type == ContentsItemType.InstructionBlock)
                {
                    nBlockCtr++;
                    if (block == (CInstructionBlock)cItem)
                        return nBlockCtr;
                }
            }
            return -1;
        }
*/
        private IATConfigFileNamespace.ConfigFile.ERandomizationType _RandomizationType = IATClient.IATConfigFileNamespace.ConfigFile.ERandomizationType.None;
        public IATConfigFileNamespace.ConfigFile.ERandomizationType RandomizationType
        {
            get
            {
                return IATConfigFileNamespace.ConfigFile.ERandomizationType.SetNumberOfPresentations;
            }
        }

        public IATConfigFileNamespace.ConfigFile.ERandomizationType GetRandomizationType()
        {
            return RandomizationType;
        }

        public List<CFontFile.FontItem> UtilizedFonts
        {
            get
            {
                List<CFontFile.FontItem> fontItems = new List<CFontFile.FontItem>();
                List<DIIatBlockInstructions> blockInstructions = new List<DIIatBlockInstructions>();
                foreach (CIATBlock b in Blocks)
                {
                    if (b.IndexInContainer < 2)
                        fontItems.AddRange(b.UtilizedStimuliFonts);
                    blockInstructions.Add(CIAT.SaveFile.GetDI(b.InstructionsUri) as DIIatBlockInstructions);
                }
                var textInstructions = blockInstructions.Select((tdi, ndx) => new { ndx = ndx + 1, tdi });
                var textInstructionFonts = from ff in textInstructions.Select(tdi => tdi.tdi).Select(tdi => tdi.PhraseFontFamily).Distinct()
                                           select new { familyName = ff, indicies = textInstructions.Where(tdi => tdi.tdi.PhraseFontFamily == ff).Select(instructions => instructions.ndx) };
                foreach (var txtInstruction in textInstructionFonts)
                {
                    fontItems.Add(new CFontFile.FontItem(txtInstruction.familyName, "is used by instructions in IAT Blocks #{0}", txtInstruction.indicies,
                        textInstructions.Where(i => txtInstruction.indicies.Contains(i.ndx)).Select(i => i.tdi as DIText)));
                }
                foreach (CInstructionBlock instrBlock in InstructionBlocks)
                    fontItems.AddRange(instrBlock.UtilizedFontFamilies);
                fontItems.AddRange(CIATKey.UtilizedFontFamilies);

                return fontItems;
            }
        }
    }
}
