/*using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace IATClient
{
    public class SaveFileConverter
    {
        private Dictionary<int, Tuple<String, System.Drawing.Image>> ImageDictionary = new Dictionary<int, Tuple<String, System.Drawing.Image>>();
        private Dictionary<Uri, XElement> DILoadDictionary = new Dictionary<Uri, XElement>();
        private Dictionary<Uri, List<Tuple<Uri, Uri>>> StimToItemAndBlockList = new Dictionary<Uri, List<Tuple<Uri, Uri>>>();
        private Dictionary<String, CIATKey> KeyDictionary = new Dictionary<String, CIATKey>();
        private CVersion Version;
        private String IATName = String.Empty;
        private XDocument SaveDocument { get; set; }
        private CIAT IAT { get; set; }

        private enum EFontPreferenceUsedFor : int
            {
                TextStimulus, BlockInstructions, TextResponseKey, Conjunction, TextInstructions, MockItemInstructions,
                ContinueInstructions, KeyedInstructions
            }

        public SaveFileConverter() {}

        private void LoadFile(String filename)
        {
            FileInfo fi = new FileInfo(filename);
            fi.Attributes = fi.Attributes & ~FileAttributes.ReadOnly;
            BinaryReader bReader = null;
            try
            {
                bReader = new BinaryReader(new FileStream(filename, FileMode.Open));
                int xmlLength = bReader.ReadInt32();
                MemoryStream memStream = new MemoryStream(bReader.ReadBytes(xmlLength), 0, xmlLength);
                memStream.Seek(0, SeekOrigin.Begin);
                SaveDocument = XDocument.Load(memStream);
                memStream.Dispose();
                int nImages = bReader.ReadInt32();
                for (int ctr = 0; ctr < nImages; ctr++)
                {
                    int id = bReader.ReadInt32();
                    String imgFilename = bReader.ReadString();
                    int nInstances = bReader.ReadInt32();
                    int nLen = bReader.ReadInt32();
                    memStream = new MemoryStream(bReader.ReadBytes(nLen), 0, nLen);
                    System.Drawing.Image img = Image.FromStream(memStream);
                    ImageDictionary[id] = new Tuple<String, System.Drawing.Image>(imgFilename, img);
                    memStream.Dispose();
                }
            }
            catch (Exception ex)
            {
                CReportableException rex = new CReportableException(ex.Message, ex);
                IATConfigMainForm.ShowErrorReport("Error loading save file", rex);
            }
            finally
            {
                if (bReader != null)
                    bReader.Close();
            }
        }

        private CIATLayout LoadLayout()
        {
            XElement elem = SaveDocument.Root.Element("CIATLayout");
            XDocument layoutDoc = new XDocument();
            layoutDoc.Document.Add(new XElement("Layout"));
            layoutDoc.Root.Add(new XElement("Interior", new XElement("Width", elem.Element("InteriorWidth").Value), new XElement("Height", elem.Element("InteriorHeight").Value)));
            layoutDoc.Root.Add(new XElement("Instructions", new XElement("Width", elem.Element("InstructionsWidth").Value, new XElement("InstructionsHeight").Value)));
            layoutDoc.Root.Add(new XElement("ResponseKey", new XElement("Width", elem.Element("KeyValueWidth").Value), new XElement("Height", elem.Element("KeyValueHeight").Value)));
            layoutDoc.Root.Add(new XElement("ErrorMark", new XElement("Width", elem.Element("ErrorWidth").Value), new XElement("Height", elem.Element("ErrorHeight").Value)));
            layoutDoc.Root.Add(new XElement("Stimulus", new XElement("Width", elem.Element("StimulusWidth").Value, new XElement("StimulusHeight").Value)));
            layoutDoc.Root.Add(new XElement("BackColor", elem.Element("BackColor").Value));
            layoutDoc.Root.Add(new XElement("BorderColor", elem.Element("BorderColor").Value));
            layoutDoc.Root.Add(new XElement("BorderWidth", elem.Element("BorderWidth").Value));
            layoutDoc.Root.Add(new XElement("WebpageBackground", new XElement("WebpageBackgroundColor", elem.Element("WebpageBackground").Element("WebpageBackgroundColor").Value), 
                new XElement("WebpageBackgroundImage")));
            Uri uri = CIAT.SaveFile.CreatePart(typeof(CIATLayout), "text/xml+" + typeof(CIATLayout).ToString());
            Stream s = CIAT.SaveFile.GetWriteStream(uri);
            layoutDoc.Save(s);
            s.Close();
            CIAT.SaveFile.ReleaseWriteStreamLock();
            CIATLayout layout = new CIATLayout(uri);
            return layout; 
        }

        private CIATKey LoadIATKey(XElement elem)
        {
            String name;
            switch (elem.Attribute("Type").Value)
            {
                case "SimpleKey":
                    Uri leftDIUri, rightDIUri;
                    XElement diElem = elem.Elements("DisplayItem").First();
                    if (diElem.Attribute("Type").Value == "Text")
                    {
                        leftDIUri = CIAT.SaveFile.CreatePart(typeof(DIBase), "text/xml+" + typeof(DIResponseKeyText).ToString());
                        DILoadDictionary[leftDIUri] = diElem;
                    }
                    else
                    {
                        leftDIUri = CIAT.SaveFile.CreatePart(typeof(DIBase), "text/xml+" + typeof(DIResponseKeyImage).ToString());
                        DILoadDictionary[leftDIUri] = diElem;
                    }
                    diElem = elem.Elements("DisplayItem").Last();
                    if (diElem.Attribute("Type").Value == "Text")
                    {
                        rightDIUri = CIAT.SaveFile.CreatePart(typeof(DIBase), "text/xml+" + typeof(DIResponseKeyText).ToString());
                        DILoadDictionary[rightDIUri] = diElem;
                    }
                    else
                    {
                        rightDIUri = CIAT.SaveFile.CreatePart(typeof(DIBase), "text/xml+" + typeof(DIResponseKeyImage).ToString());
                        DILoadDictionary[leftDIUri] = diElem;
                    }
                    return new CIATKey(elem.Element("Name").Value, leftDIUri, rightDIUri);

                case "ReversedKey":
                    name = elem.Element("Name").Value;
                    String baseKeyName = elem.Element("BaseKeyName").Value;
                    return new CIATReversedKey(name, CIATKey.GetKeyUriByName(baseKeyName));

                case "DualKey":
                    Uri BaseKey1Uri = KeyDictionary[elem.Element("BaseKey1Name").Value].URI;
                    Uri BaseKey2Uri = KeyDictionary[elem.Element("BaseKey2Name").Value].URI;
                    name = elem.Element("Name").Value;
                    Uri conjunctionUri = CIAT.SaveFile.CreatePart(typeof(DIBase), "text/xml+" + typeof(DIConjunction).ToString());
                    DILoadDictionary[conjunctionUri] = elem.Element("DisplayItem");
                    return new CIATDualKey()
                    {
                        Name = name,
                        BaseKey1Uri = BaseKey1Uri,
                        BaseKey2Uri = BaseKey2Uri,
                        ConjunctionUri = conjunctionUri
                    };
            }
            throw new FormatException("Unknown key type encountered while converting save file.");
        }

        private void LoadIATBlocks(XElement elem)
        {
            Dictionary<CIATBlock, Tuple<XElement, List<IStimulus>>> blocksToStimuli = new Dictionary<CIATBlock, Tuple<XElement, List<IStimulus>>>();
            foreach (XElement blockElem in elem.Elements("IATBlock"))
            {
                Uri blockUri = CIAT.SaveFile.CreatePart(typeof(CIATBlock), "text/xml+" + typeof(CIATBlock).ToString());
                CIAT.SaveFile.CreateRelationship(typeof(CIAT), typeof(CIATBlock), IAT.URI, blockUri);
                XDocument xDoc = new XDocument();
                xDoc.Document.Add(new XElement("IATBlock"), new XAttribute("Name", blockElem.Element("Name").Value), new XAttribute("IndexInContents", blockElem.Element("ContentsIndex")));
                String keyName = blockElem.Element("KeyName").Value;
                if (keyName != String.Empty)
                {
                    String rKeyId = CIAT.SaveFile.CreateRelationship(typeof(CIATBlock), typeof(CIATKey), blockUri, KeyDictionary[keyName].URI);
                    xDoc.Root.Add(new XElement("rKeyId", rKeyId));
                }
                Uri instUri = CIAT.SaveFile.CreatePart(typeof(DIBase), "text/xml+" + typeof(DIIatBlockInstructions).ToString());
                DILoadDictionary[instUri] = blockElem.Element("IATMultiLineTextDisplayItem");
                String rInstId = CIAT.SaveFile.CreateRelationship(typeof(CIATBlock), typeof(DIBase), blockUri, instUri);
                xDoc.Root.Add(new XElement("rInstructionsId", rInstId));
                xDoc.Root.Add(new XElement("IsDynamicallyKeyed", elem.Element("IsDynamicallyKeyed").Value));
                CIATBlock block = new CIATBlock(IAT);
                IAT.AddIATBlock(block);
                blocksToStimuli[block] = new Tuple<XElement, List<IStimulus>>(blockElem, new List<IStimulus>());
                foreach (XElement itemElem in blockElem.Element("Items").Elements("IATItem"))
                {
                    if (itemElem.Element("DisplayItem").Attribute("Type").Value == "Image")
                    {
                        DIStimulusImage stim = new DIStimulusImage();
                        stim.SetImage(ImageDictionary[System.Convert.ToInt32(itemElem.Element("DisplayItem").Element("SaveFileNdx").Value)].Item2);
                        stim.Description = itemElem.Element("DisplayItem").Element("Description").Value;
                        stim.StretchToFit = System.Convert.ToBoolean(itemElem.Element("DisplayItem").Element("StretchToFit").Value);
                        blocksToStimuli[block].Item2.Add(stim);
                    }
                    if (itemElem.Element("DisplayItem").Attribute("Type").Value == "Text")
                    {
                        DIStimulusText stim = new DIStimulusText();
                        stim.Phrase = itemElem.Element("DisplayItem").Element("Phrase").Value;
                        stim.PhraseFontFamily = itemElem.Element("DisplayItem").Element("FontFamily").Value;
                        stim.PhraseFontSize = System.Convert.ToSingle(itemElem.Element("DisplayItem").Element("FontSize").Value);
                        stim.PhraseFontColor = Color.FromName(itemElem.Element("DisplayItem").Element("ColorName").Value);
                        blocksToStimuli[block].Item2.Add(stim);
                    }
                    else
                    {
                        blocksToStimuli[block].Item2.Add(new DINull());
                    }
                }
            }
            Dictionary<IStimulus, List<CIATBlock>> stimuliToBlocks = new Dictionary<IStimulus, List<CIATBlock>>();
            foreach (CIATBlock block in blocksToStimuli.Keys)
            {
                foreach (IStimulus stim in blocksToStimuli[block].Item2)
                {
                    stimuliToBlocks[stim] = new List<CIATBlock>();
                    stimuliToBlocks[stim].Add(block);
                    foreach (CIATBlock b in blocksToStimuli.Keys)
                    {
                        if (b == block)
                            continue;
                        List<IStimulus> dupStims = new List<IStimulus>();
                        foreach (IStimulus s in blocksToStimuli[b].Item2)
                        {
                            if (stim.Equals(s)) {
                                dupStims.Add(s);
                                stimuliToBlocks[stim].Add(b);
                            }
                        }
                        foreach (IStimulus s in dupStims) {
                            blocksToStimuli[b].Item2.Remove(s);
                            s.Dispose();
                        }
                    }
                }
            }
            Dictionary<IStimulus, CIATItem> stimsToItems = new Dictionary<IStimulus, CIATItem>();
            foreach (CIATBlock b in blocksToStimuli.Keys)
            {
                foreach (XElement itemElem in blocksToStimuli[b].Item1.Element("Items").Elements("IATItem"))
                {
                    IStimulus itemStim = null;
                    if (itemElem.Element("DisplayItem").Attribute("Type").Value == "Image")
                    {
                        DIStimulusImage stim = new DIStimulusImage();
                        stim.SetImage(ImageDictionary[System.Convert.ToInt32(itemElem.Element("DisplayItem").Element("SaveFileNdx").Value)].Item2);
                        stim.Description = itemElem.Element("DisplayItem").Element("Description").Value;
                        stim.StretchToFit = System.Convert.ToBoolean(itemElem.Element("DisplayItem").Element("StretchToFit").Value);
                        itemStim = stim as IStimulus;
                    }
                    if (itemElem.Element("DisplayItem").Attribute("Type").Value == "Text")
                    {
                        DIStimulusText stim = new DIStimulusText();
                        stim.Phrase = itemElem.Element("DisplayItem").Element("Phrase").Value;
                        stim.PhraseFontFamily = itemElem.Element("DisplayItem").Element("FontFamily").Value;
                        stim.PhraseFontSize = System.Convert.ToSingle(itemElem.Element("DisplayItem").Element("FontSize").Value);
                        stim.PhraseFontColor = Color.FromName(itemElem.Element("DisplayItem").Element("ColorName").Value);
                        itemStim = stim as IStimulus;
                    }
                    else
                    {
                        itemStim = new DINull();
                    }
                    IStimulus origStim = stimuliToBlocks.Keys.Where(s => s.Equals(itemStim)).First();
                    itemStim.Dispose();
                    if (!stimsToItems.ContainsKey(origStim))
                    {
                        CIATItem item = new CIATItem(origStim);
                        item.SetKeyedDirection(b.URI, KeyedDirection.FromString(itemElem.Element("KeyedDir").Value));
                        if (itemElem.Element("KeySpecifierID") != null)
                            item.KeySpecifierID = System.Convert.ToInt32(itemElem.Element("KeySpecifierID").Value);
                        if (itemElem.Element("SpecifierArg") != null)
                            item.SpecifierArg = itemElem.Element("SpecifierArg").Value;
                    }
                    else
                        stimsToItems[origStim].AddParentBlock(b, KeyedDirection.FromString(itemElem.Element("KeyedDir").Value));
                }
            }
        }

        private CInstructionScreen LoadInstructionScreen(XElement elem, CInstructionBlock b)
        {
            switch (elem.Attribute("Type").Value)
            {
                case "None":
                    CInstructionScreen scrn = new CInstructionScreen(b);
                    scrn.ContinueKey = elem.Element("ContinueKey").Value;
                    DILoadDictionary[scrn.ContinueInstructionsUri] = elem.Element("DisplayItem");
                    return scrn;

                case "Text":
                    CTextInstructionScreen txtScrn = new CTextInstructionScreen(b);
                    txtScrn.ContinueKey = elem.Element("ContinueKey").Value;
                    DILoadDictionary[txtScrn.InstructionsUri] = elem.Element("IATMultiLineTextDisplayItem");
                    DILoadDictionary[txtScrn.ContinueInstructionsUri] = elem.Element("DisplayItem");
                    return txtScrn;

                case "Key":
                    CKeyInstructionScreen keyedScrn = new CKeyInstructionScreen(b);
                    keyedScrn.ContinueKey = elem.Element("ContinueKey").Value;
                    DILoadDictionary[keyedScrn.InstructionsUri] = elem.Element("IATMultiLineDisplayItem");
                    DILoadDictionary[keyedScrn.ContinueInstructionsUri] = elem.Element("DisplayItem");
                    if (elem.Element("ResponseKeyName").Value != String.Empty)
                        keyedScrn.ResponseKeyUri = KeyDictionary[elem.Element("ResponseKeyName").Value].URI;
                    return keyedScrn;

                case "MockItem":
                    CMockItemScreen mockScrn = new CMockItemScreen(b);
                    mockScrn.ContinueKey = elem.Element("ContinueKey").Value;
                    mockScrn.InvalidResponseFlag = System.Convert.ToBoolean(elem.Element("InvalidResponseFlag").Value);
                    mockScrn.KeyedDirOutlined = System.Convert.ToBoolean(elem.Element("KeyedDirOutlined"));
                    DILoadDictionary[mockScrn.InstructionsUri] = elem.Element("IATMultiLineDisplayItem");
                    DILoadDictionary[mockScrn.ContinueInstructionsUri] = elem.Element("DisplayItem");
                    if (elem.Element("ResponseKeyName").Value != String.Empty)
                        mockScrn.ResponseKeyUri = KeyDictionary[elem.Element("ResponseKeyName").Value].URI;
                    switch (elem.Element("KeyedDir").Value)
                    {
                        case "Left":
                            mockScrn.KeyedDirection = KeyedDirection.Left;
                            break;

                        case "Right":
                            mockScrn.KeyedDirection = KeyedDirection.Right;
                            break;

                        case "None":
                            mockScrn.KeyedDirection = KeyedDirection.None;
                            break;
                    }
                    if (elem.Element("DisplayItem").Attribute("Type").Value == "Image")
                    {
                        DIStimulusImage stim = new DIStimulusImage();
                        stim.SetImage(ImageDictionary[System.Convert.ToInt32(elem.Element("DisplayItem").Element("SaveFileNdx").Value)].Item2);
                        stim.Description = elem.Element("DisplayItem").Element("Description").Value;
                        stim.StretchToFit = System.Convert.ToBoolean(elem.Element("DisplayItem").Element("StretchToFit").Value);
                        mockScrn.StimulusUri = stim.URI;
                    }
                    if (elem.Element("DisplayItem").Attribute("Type").Value == "Text")
                    {
                        DIStimulusText stim = new DIStimulusText();
                        stim.Phrase = elem.Element("DisplayItem").Element("Phrase").Value;
                        stim.PhraseFontFamily = elem.Element("DisplayItem").Element("FontFamily").Value;
                        stim.PhraseFontSize = System.Convert.ToSingle(elem.Element("DisplayItem").Element("FontSize").Value);
                        stim.PhraseFontColor = Color.FromName(elem.Element("DisplayItem").Element("ColorName").Value);
                        mockScrn.StimulusUri = stim.URI;
                    }
                    else
                    {
                        mockScrn.StimulusUri = new DINull().URI;
                    }
                    return mockScrn;
            }
            return null;
        }

        private void LoadInstructionBlock(XElement elem)
        {
            CInstructionBlock instBlock = new CInstructionBlock(IAT);
            IAT.AddInstructionBlock(instBlock);
            foreach (XElement instScrnElem in elem.Elements("InstructionScreen"))
            {
                instBlock.AddScreen(LoadInstructionScreen(instScrnElem, instBlock));
            }
        }

        private void LoadFormat(XElement elem, SurveyItemFormat Format)
        {
            Format = new SurveyItemFormat(SurveyItemFormat.EFor.Response, CResponse.EResponseType.Boolean);
            Format.Font = SurveyItemFormat.EFont.GetFontByName(elem.Element("Font").Value);
            Format.FontSize = elem.Element("FontSize").Value;
            Format.Color = Color.FromArgb(System.Convert.ToInt32(elem.Element("ColorR").Value),
                System.Convert.ToInt32(elem.Element("ColorG").Value), System.Convert.ToInt32(elem.Element("ColorB").Value));
            Format.Bold = System.Convert.ToBoolean(elem.Element("Bold").Value);
            Format.Italic = System.Convert.ToBoolean(elem.Element("Italic").Value);
        }


        private CSurvey LoadSurvey(XElement elem, CSurvey.EOrdinality ord)
        {
            CSurvey survey = new CSurvey(ord);
            survey.Name = elem.Element("Name").Value;
            survey.Timeout = System.Convert.ToInt32(elem.Element("Timeout").Value);
            if (elem.Element("Cation") != null)
            {
                XElement cElem = elem.Element("Caption");
                CSurveyCaption caption = new CSurveyCaption(survey);
                caption.Text = cElem.Element("Text").Value;
                caption.FontSize = System.Convert.ToInt32(cElem.Element("FontSize").Value);
                caption.BorderWidth = System.Convert.ToInt32(cElem.Element("BorderWidth").Value);
                caption.FontColor = Color.FromArgb(System.Convert.ToInt32(cElem.Element("FontColorR").Value), System.Convert.ToInt32(cElem.Element("FontColorG").Value),
                    System.Convert.ToInt32(cElem.Element("FontColorB").Value));
                caption.BackColor = Color.FromArgb(System.Convert.ToInt32(cElem.Element("BackColorR").Value), System.Convert.ToInt32(cElem.Element("BackColorG").Value),
                    System.Convert.ToInt32(cElem.Element("BackColorB").Value));
                caption.BorderColor = Color.FromArgb(System.Convert.ToInt32(cElem.Element("BorderColorR").Value), System.Convert.ToInt32(cElem.Element("BorderColorG").Value),
                    System.Convert.ToInt32(cElem.Element("BorderColorB").Value));
            }
            foreach (XElement siElem in elem.Elements("SurveyItem"))
            {
                CSurveyItem si = new CSurveyItem(survey);
                si.Optional = System.Convert.ToBoolean(siElem.Element("Optional").Value);
                si.Text = siElem.Element("Text").Value;
                XElement rElem = siElem.Element("Response");
                switch (rElem.Attribute("Type").Value)
                {
                    case "Boolean":
                        CBoolResponse boolResp = new CBoolResponse();
                        boolResp.TrueStatement = rElem.Element("TrueStatement").Value;
                        boolResp.FalseStatement = rElem.Element("FalseStatement").Value;
                        boolResp.Format = new SurveyItemFormat(SurveyItemFormat.EFor.Response, CResponse.EResponseType.Boolean);
                        LoadFormat(rElem.Element("Format"), boolResp.Format);
                        si.Response = boolResp;
                        break;

                    case "Bounded Length":
                        CBoundedLengthResponse bLenResp = new CBoundedLengthResponse();
                        bLenResp.MinLength = System.Convert.ToInt32(rElem.Element("MinLength").Value);
                        bLenResp.MaxLength = System.Convert.ToInt32(rElem.Element("MaxLenngth").Value);
                        bLenResp.Format = new SurveyItemFormat(SurveyItemFormat.EFor.Response, CResponse.EResponseType.BoundedLength);
                        LoadFormat(rElem.Element("Format"), bLenResp.Format);
                        si.Response = bLenResp;
                        break;

                    case "Bounded Number":
                        CBoundedNumResponse bNumResp = new CBoundedNumResponse();
                        bNumResp.MinValue = System.Convert.ToInt32(rElem.Element("MinValue").Value);
                        bNumResp.MaxValue = System.Convert.ToInt32(rElem.Element("MaxValue").Value);
                        bNumResp.Format = new SurveyItemFormat(SurveyItemFormat.EFor.Response, CResponse.EResponseType.BoundedNum);
                        LoadFormat(rElem.Element("Format"), bNumResp.Format);
                        si.Response = bNumResp;
                        break;


                    case "Date":
                        CDateResponse dResp = new CDateResponse();
                        if (System.Convert.ToBoolean(rElem.Element("StartDate").Attribute("HasValue").Value))
                            dResp.StartDate = new DateTime(System.Convert.ToInt32(rElem.Element("StartDate").Element("Year").Value),
                                System.Convert.ToInt32(rElem.Element("StartDate").Element("Month").Value),
                                System.Convert.ToInt32(rElem.Element("StartDate").Element("Day").Value));
                        else
                            dResp.StartDate = DateTime.MaxValue;
                        if (System.Convert.ToBoolean(rElem.Element("EndDate").Attribute("HasValue").Value))
                            dResp.EndDate = new DateTime(System.Convert.ToInt32(rElem.Element("EndDate").Element("Year").Value),
                                System.Convert.ToInt32(rElem.Element("EndDate").Attribute("Month").Value),
                                System.Convert.ToInt32(rElem.Element("EndDate").Element("Day").Value));
                        else
                            dResp.EndDate = DateTime.MaxValue;
                        LoadFormat(rElem.Element("Format"), dResp.Format);
                        si.Response = dResp;
                        break;

                    case "Fixed Digit":
                        CFixedDigResponse fdResp = new CFixedDigResponse();
                        fdResp.NumDigs = System.Convert.ToInt32(rElem.Element("NumDigs").Value);
                        LoadFormat(rElem.Element("Format"), fdResp.Format);
                        si.Response = fdResp;
                        break;

                    case "Instruction":
                        si.Response = new CInstruction();
                        break;

                    case "Likert":
                        CLikertResponse lResp = new CLikertResponse(System.Convert.ToInt32(rElem.Element("NumChoices").Value), System.Convert.ToBoolean(rElem.Element("IsReverseScored").Value));
                        int choiceCtr = 0;
                        foreach (XElement cElem in rElem.Element("ChoiceDescriptions").Elements("Choice"))
                            lResp.SetChoiceDesc(choiceCtr++, cElem.Value);
                        LoadFormat(rElem.Element("Format"), lResp.Format);
                        si.Response = lResp;
                        break;

                    case "Multiple Selection":
                        CMultiBooleanResponse mbResp = new CMultiBooleanResponse(); ;
                        foreach (XElement lElem in rElem.Element("Labels").Elements("Label"))
                            mbResp.AddLabel(lElem.Value);
                        mbResp.MinSelections = System.Convert.ToInt32(rElem.Element("MinSelections"));
                        mbResp.MaxSelections = System.Convert.ToInt32(rElem.Element("MaxSelections"));
                        LoadFormat(rElem.Element("Format"), mbResp.Format);
                        si.Response = mbResp;
                        break;

                    case "Multiple Chhoice":
                        CMultipleResponse mResp = new CMultipleResponse(System.Convert.ToInt32(rElem.Element("NumChoices").Value));
                        foreach (String s in rElem.Element("Choices").Elements("Choice").Select(e => e.Value))
                            mResp.AddChoice(s);
                        LoadFormat(rElem.Element("Format"), mResp.Format);
                        si.Response = mResp;
                        break;

                    case "Regular Expression":
                        CRegExResponse rResp = new CRegExResponse();
                        rResp.RegEx = rElem.Element("Expression").Value;
                        LoadFormat(rElem.Element("Format"), rResp.Format);
                        si.Response = rResp;
                        break;

                    case "Weighted Multiple Choice":
                        CWeightedMultipleResponse wmResp = new CWeightedMultipleResponse();
                        foreach (XElement mbElem in rElem.Element("WeightedChoices").Elements("WeightedChoice"))
                            wmResp.AddChoice(mbElem.Element("Choice").Value, System.Convert.ToInt32(mbElem.Element("Weight").Value));
                        LoadFormat(rElem.Element("Format"), wmResp.Format);
                        si.Response = wmResp;
                        break;
                }
                survey.Items.Add(si);
            }
            return survey;
        }

        private void LoadDI(XElement elem, Uri uri)
        {
            DIType diType = DIType.FromTypeName(CIAT.SaveFile.GetTypeName(uri));
            if (diType == DIType.StimulusImage)
            {
                int id = System.Convert.ToInt32(elem.Element("SaveFileNdx").Value);
                String description = elem.Element("Description").Value;
                bool stretchToFit = System.Convert.ToBoolean(elem.Element("StretchToFit").Value);
                Image img = ImageDictionary[id].Item2;
                DIStimulusImage di = new DIStimulusImage();
                di.SetImage(img, img.RawFormat);
                di.StretchToFit = stretchToFit;
                di.Description = ImageDictionary[id].Item1;
                di.Save(uri);
            }
            if (diType == DIType.ResponseKeyImage)
            {
                DIResponseKeyImage di = new DIResponseKeyImage();
                int id = System.Convert.ToInt32(elem.Element("ImageID").Value);
                Image img = ImageDictionary[id].Item2;
                di.SetImage(img, img.RawFormat);
                di.Description = ImageDictionary[id].Item1;
                di.Save(uri);
                foreach (String name in elem.Descendants("SizeOwner").Select(e => e.Element("OwerName").Value)) {
                    CIATKey key = CIAT.SaveFile.GetIATKey(CIATKey.GetKeyUriByName(name));
                    CIAT.SaveFile.CreateRelationship(di.BaseType, key.BaseType, uri, key.URI);
                }
            }
            if (diType.IsText)
            {
                XDocument diDoc = new XDocument();
                if (diType == DIType.ResponseKeyText)
                {
                    diDoc.Document.Add(new XElement("Text", new XAttribute("rId", "-1"), new XElement("Phrase", elem.Element("Phrase").Value), new XElement("PhraseFontFamily", elem.Element("FontFamily").Value),
                        new XElement("PhraseFontSize", elem.Element("FontSize").Value), new XElement("PhraseFontColor", elem.Element("ColorName").Value),
                        new XElement("Alignment", TextJustification.Center.ToString()), new XElement("LineSpacing", "1")));
                }
                if (diType == DIType.Conjunction)
                {
                    diDoc.Document.Add(new XElement("Text", new XAttribute("rId", "-1"), new XElement("Phrase", elem.Element("Phrase").Value), new XElement("PhraseFontFamily", elem.Element("FontFamily").Value),
                        new XElement("PhraseFontSize", elem.Element("FontSize").Value), new XElement("PhraseFontColor", elem.Element("ColorName").Value),
                        new XElement("Alignment", TextJustification.Center.ToString()), new XElement("LineSpacing", "1")));
                }
                if (diType == DIType.ContinueInstructions)
                {
                    diDoc.Document.Add(new XElement("Text", new XAttribute("rId", "-1"), new XElement("Phrase", elem.Element("Phrase").Value), new XElement("PhraseFontFamily", elem.Element("FontFamily").Value),
                        new XElement("PhraseFontSize", elem.Element("FontSize").Value), new XElement("PhraseFontColor", elem.Element("ColorName").Value),
                        new XElement("Alignment", TextJustification.Center.ToString()), new XElement("LineSpacing", "1")));
                }
                if (diType == DIType.StimulusText)
                {
                    diDoc.Document.Add(new XElement("Text", new XAttribute("rId", "-1"), new XElement("Phrase", elem.Element("Phrase").Value), new XElement("PhraseFontFamily", elem.Element("FontFamily").Value),
                        new XElement("PhraseFontSize", elem.Element("FontSize").Value), new XElement("PhraseFontColor", elem.Element("ColorName").Value),
                        new XElement("Alignment", TextJustification.Center.ToString()), new XElement("LineSpacing", "1")));
                }
                if (diType == DIType.KeyedInstructionsScreen)
                {
                    diDoc.Document.Add(new XElement("Text", new XAttribute("rId", "-1"), new XElement("Phrase", elem.Element("Text").Value), new XElement("PhraseFontFamily", elem.Element("FontFamily").Value),
                        new XElement("PhraseFontSize", elem.Element("FontSize").Value), new XElement("PhraseFontColor", elem.Element("TextColorName").Value),
                        new XElement("Alignment", TextJustification.Center.ToString()), new XElement("LineSpacing", elem.Element("LineSpacing").Value)));
                }
                if (diType == DIType.IatBlockInstructions)
                {
                    diDoc.Document.Add(new XElement("Text", new XAttribute("rId", "-1"), new XElement("Phrase", elem.Element("Text").Value), new XElement("PhraseFontFamily", elem.Element("FontFamily").Value),
                        new XElement("PhraseFontSize", elem.Element("FontSize").Value), new XElement("PhraseFontColor", elem.Element("TextColorName").Value),
                        new XElement("Alignment", TextJustification.Center.ToString()), new XElement("LineSpacing", elem.Element("LineSpacing").Value)));
                }
                if (diType == DIType.MockItemInstructions)
                {
                    diDoc.Document.Add(new XElement("Text", new XAttribute("rId", "-1"), new XElement("Phrase", elem.Element("Text").Value), new XElement("PhraseFontFamily", elem.Element("FontFamily").Value),
                        new XElement("PhraseFontSize", elem.Element("FontSize").Value), new XElement("PhraseFontColor", elem.Element("TextColorName").Value),
                        new XElement("Alignment", TextJustification.Center.ToString()), new XElement("LineSpacing", elem.Element("LineSpacing").Value)));
                }
                if (diType == DIType.TextInstructionsScreen)
                {
                    diDoc.Document.Add(new XElement("Text", new XAttribute("rId", "-1"), new XElement("Phrase", elem.Element("Text").Value), new XElement("PhraseFontFamily", elem.Element("FontFamily").Value),
                        new XElement("PhraseFontSize", elem.Element("FontSize").Value), new XElement("PhraseFontColor", elem.Element("TextColorName").Value),
                        new XElement("Alignment", TextJustification.Center.ToString()), new XElement("LineSpacing", elem.Element("LineSpacing").Value)));
                }
                Stream s = CIAT.SaveFile.GetWriteStream(uri);
                diDoc.Save(s);
                s.Close();
                CIAT.SaveFile.ReleaseWriteStreamLock();
                DIBase di = diType.Create(uri);
            }
            if (diType == DIType.Null) {

            }
        }

        public void Convert(String filename)
        {
            FileStream fStream = File.Open(filename, FileMode.Open, FileAccess.Read);
            BinaryReader bReader = new BinaryReader(fStream);
            int xmlLen = bReader.ReadInt32();
            byte[] xml = bReader.ReadBytes(xmlLen);
            MemoryStream memStream = new MemoryStream(xml);
            SaveDocument = XDocument.Load(memStream);
            memStream.Dispose();
            int nImages = bReader.ReadInt32();
            for (int ctr = 0; ctr < nImages; ctr++)
            {
                int id = bReader.ReadInt32();
                String name = bReader.ReadString();
                bReader.ReadInt64();
                int len = bReader.ReadInt32();
                byte[] imgData = bReader.ReadBytes(len);
                MemoryStream imgStream = new MemoryStream(imgData);
                Image img = Image.FromStream(imgStream);
                ImageDictionary[id] = new Tuple<String, Image>(name, img);
                imgStream.Dispose();
            }
            bReader.Close();
            fStream.Close();
            IAT = new CIAT();
            if (SaveDocument.Document.Element("IATConfigFile").Attribute("ProductVersion") != null)
                Version = new CVersion(SaveDocument.Document.Element("IATConfigFile").Attribute("ProductVersion").Value);
            else
                Version = new CVersion("0.0.0.0");
            if (SaveDocument.Root.Element("IATName") != null)
                IATName = SaveDocument.Root.Element("IATName").Value;
            XDocument iatDoc = new XDocument();
            iatDoc.Document.Add(new XElement("IAT", new XAttribute("Version", (new Version()).ToString()), new XElement("Name", IATName)));
            Uri iatUri = CIAT.SaveFile.CreatePart(typeof(CIAT), "text/xml+" + typeof(CIAT).ToString());
            Stream s = CIAT.SaveFile.GetWriteStream(iatUri);
            iatDoc.Save(s);
            s.Close();
            CIAT.SaveFile.ReleaseWriteStreamLock();

            List<String> simpleKeyNames = new List<String>();
            List<String> dualKeyNames = new List<String>();
            foreach (XElement elem in SaveDocument.Root.Descendants("IATKey").Where(e => e.Attribute("Type").Value == "SimpleKey"))
            {
                CIATKey key = LoadIATKey(elem);
                key.Save();
                CIAT.SaveFile.CreateRelationship(typeof(CIAT), typeof(CIATKey), iatUri, key.URI);
                simpleKeyNames.Add(key.Name);
                KeyDictionary[key.Name] = key;
            }
            foreach (XElement elem in SaveDocument.Root.Descendants("IATKey").Where(e => (e.Attribute("Type").Value == "ReversedKey") && (simpleKeyNames.Contains(e.Element("BaseKeyName").Value))))
            {
                CIATKey key = LoadIATKey(elem);
                key.Save();
                CIAT.SaveFile.CreateRelationship(typeof(CIAT), typeof(CIATKey), iatUri, key.URI);
                KeyDictionary[key.Name] = key;
            }
            foreach (XElement elem in SaveDocument.Root.Descendants("IATKey").Where(e => (e.Attribute("Type").Value == "DualKey")))
            {
                CIATKey key = LoadIATKey(elem);
                key.Save();
                CIAT.SaveFile.CreateRelationship(typeof(CIAT), typeof(CIATKey), iatUri, key.URI);
                KeyDictionary[key.Name] = key;
                dualKeyNames.Add(key.Name);
            }
            foreach (XElement elem in SaveDocument.Root.Descendants("IATKey").Where(e => (e.Attribute("Type").Value == "ReversedKey") && (dualKeyNames.Contains(e.Element("BaseKeyName").Value))))
            {
                CIATKey key = LoadIATKey(elem);
                key.Save();
                KeyDictionary[key.Name] = key;
                CIAT.SaveFile.CreateRelationship(typeof(CIAT), typeof(CIATKey), iatUri, key.URI);
            }
            LoadIATBlocks(SaveDocument.Root.Element("Blocks"));
            foreach (XElement iBlockElem in SaveDocument.Root.Element("InstructionBlocks").Elements("InstructionBlock"))
                LoadInstructionBlock(iBlockElem);
            foreach (int imgId in ImageDictionary.Keys)
                ImageDictionary[imgId].Item2.Dispose();

            foreach (Uri u in DILoadDictionary.Keys)
                LoadDI(DILoadDictionary[u], u);
        }
    }
}
*/