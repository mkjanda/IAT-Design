namespace IATClient
{
    /*
    public class CIATPackager : IStoredInXml
    {
        private List<IATConfigFileNamespace.IATDisplayItem> IATDisplayItems;
        private List<IATConfigFileNamespace.IATEvent> IATEvents;
        private String _OutputDirectory, _IATName;
        private CIAT IAT;
        private String outputDirectory;
        private String packageFilename;
        private IATConfigMainForm MainForm;
        private FileStream ItemSlideStream;
        private int nItemSlides;
        private DataPasswordForm progressWindow;

        class PackagingException : Exception { }

        public delegate bool TargetDirectoryExistsHandler(String Directory);
        public TargetDirectoryExistsHandler OnDirectoryExists;

        /// <summary>
        /// an enumaration of the available randomization types
        /// </summary>

        private IATConfigFileNamespace.ConfigFile.ERandomizationType _RandomizationType;

        /// <summary>
        /// gets the name of the IAT
        /// </summary>
        public String IATName
        {
            get
            {
                return _IATName;
            }
            set
            {
                _IATName = value;
            }
        }

        public String OutputDirectory
        {
            get
            {
                return _OutputDirectory;
            }
            set
            {
                _OutputDirectory = value;
            }
        }

        /// <summary>
        /// gets or sets the randomization type
        /// </summary>
        public IATConfigFileNamespace.ConfigFile.ERandomizationType RandomizationType
        {
            get
            {
                return _RandomizationType;
            }
            set
            {
                _RandomizationType = value;
            }
        }

        public IATConfigFileNamespace.ConfigFile.ERandomizationType GetRandomizationType()
        {
            return RandomizationType;
        }

        // the left and right response characters
        private String _LeftResponseChar, _RightResponseChar;

        /// <summary>
        /// gets or sets the left response key value character
        /// </summary>
        public String LeftResponseChar
        {
            get
            {
                return _LeftResponseChar;
            }
            set
            {
                _LeftResponseChar = value;
            }
        }

        /// <summary>
        /// gets or sets the right response key value character
        /// </summary>
        public String RightResponseChar
        {
            get
            {
                return _RightResponseChar;
            }
            set
            {
                _RightResponseChar = value;
            }
        }

        /// <summary>
        /// the password used to retrieve data from the server
        /// </summary>
        private String _DataRetrievalPassword;

        /// <summary>
        /// gets or sets the password used to retrieve data from the server
        /// </summary>
        public String DataRetrievalPassword
        {
            get
            {
                return _DataRetrievalPassword;
            }
            set
            {
                _DataRetrievalPassword = value;
            }
        }

        /// <summary>
        /// the URL to redirect the testee to upon completion of the IAT and surveys
        /// </summary>
        private String _RedirectionURL;

        /// <summary>
        /// gets or sets the URL to redirect the testee to upon completion of the IAT and surveys
        /// </summary>
        public String RedirectionURL
        {
            get
            {
                return _RedirectionURL;
            }
            set
            {
                _RedirectionURL = value;
            }
        }

        /// <summary>
        /// Saves the passed bitmap as an item slide image in the "ItemSlide" directory of the packaged test
        /// </summary>
        /// <param name="ItemSlide">the bitmap to save</param>
        /// <param name="ItemNum">the item number the bitmap represents</param>
        private void SaveItemSlide(Image ItemSlide, int ItemNum, String arg)
        {
            BinaryWriter bWriter = new BinaryWriter(ItemSlideStream, Encoding.Unicode);
            bWriter.Write(String.Format("Item_{0:D3}.jpg", ItemNum));
            MemoryStream memStream = new MemoryStream();
            ItemSlide.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);
            bWriter.Write(Convert.ToInt32(memStream.Length));
            bWriter.Write(memStream.ToArray(), 0, (int)memStream.Length);
            memStream.Dispose();
            bWriter.Flush();
            nItemSlides++;
        }

        private int FindIdenticalDisplayItem(CDisplayItem di, Point ptOrigin)
        {
            for (int ctr = 0; ctr < IATDisplayItems.Count; ctr++)
                if (IATDisplayItems[ctr].IsIdenticalTo(di, ptOrigin))
                    return ctr;
            return -1;
        }

        private int ProcessIATItem(IATClient.CIATItem item, bool IsPracticeItem, int blockNum)
        {
            IATConfigFileNamespace.IATDisplayItem IATDisplayItem;
            IATConfigFileNamespace.IATItem PackagedIATItem;
            int stimulusID;

            // grab the stimulus
            CDisplayItem DisplayItem = item.Stimulus;
            Point ptOrigin = CIAT.Layout.StimulusRectangle.Location;
            switch (DisplayItem.Type)
            {
                case CDisplayItem.EType.text:
                    ptOrigin.X += (CIAT.Layout.StimulusRectangle.Width - DisplayItem.ItemSize.Width) >> 1; 
                    ptOrigin.Y += CIAT.Layout.TextStimulusPaddingTop;
                    stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
                    if (stimulusID == -1)
                    {
                        stimulusID = IATDisplayItems.Count;
                        IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(stimulusID, DisplayItem, ptOrigin, IATName);
                        IATDisplayItems.Add(IATDisplayItem);
                    }
                    break;

                case CDisplayItem.EType.stimulusImage:
                    CStimulusImageItem idi = (CStimulusImageItem)DisplayItem;
                    Size idiSize = idi.IATImage.ImageSize;
                    ptOrigin.Y += (CIAT.Layout.StimulusRectangle.Height - idiSize.Height) >> 1;
                    ptOrigin.X += (CIAT.Layout.StimulusRectangle.Width - idiSize.Width) >> 1;
                    stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
                    if (stimulusID == -1)
                    {
                        stimulusID = IATDisplayItems.Count;
                        IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(stimulusID, DisplayItem, ptOrigin, IATName);
                        IATDisplayItems.Add(IATDisplayItem);
                    }
                    break;

                default:
                    throw new Exception("Invalid Display Item type employed as stimulus.");
            }

            // add the IATItem to the event list
            IATConfigFileNamespace.IATItem.EKeyedDir keyedDir = IATConfigFileNamespace.IATItem.EKeyedDir.Left;
            switch (item.KeyedDir)
            {
                case CIATItem.EKeyedDir.Right:
                    keyedDir = IATConfigFileNamespace.IATItem.EKeyedDir.Right;
                    break;

                case CIATItem.EKeyedDir.Left:
                    keyedDir = IATConfigFileNamespace.IATItem.EKeyedDir.Left;
                    break;

                case CIATItem.EKeyedDir.DynamicRight:
                    keyedDir = IATConfigFileNamespace.IATItem.EKeyedDir.DynamicRight;
                    break;

                case CIATItem.EKeyedDir.DynamicLeft:
                    keyedDir = IATConfigFileNamespace.IATItem.EKeyedDir.DynamicLeft;
                    break;
            }
            PackagedIATItem = new IATConfigFileNamespace.IATItem(stimulusID, keyedDir, item.KeySpecifierID, blockNum, item.SpecifierArg, item.OriginatingBlock);
            IATEvents.Add(PackagedIATItem);
            return PackagedIATItem.ItemNum;
        }

        private void ProcessIATBlock(CIATBlock Block, bool IsPracticeBlock, int blockNum)
        {
            IATConfigFileNamespace.IATDisplayItem IATDisplayItem;
            CDisplayItem DisplayItem;
            IATConfigFileNamespace.IATEvent IATEvent;
            Point ptOrigin = new Point();
            int leftKeyID, rightKeyID, instructionsID;
            Bitmap ItemSlide = null;
            Graphics g = null;
            Brush EraseBrush = null;
            Pen OutlinePen = null, ErasePen = null;
            Rectangle LeftOutlineRectangle = new Rectangle(0, 0, 0, 0), RightOutlineRectangle = new Rectangle(0, 0, 0, 0);

            // initialize the item slide bitmap and graphics objects if this is not a practice block
            if (!IsPracticeBlock)
            {
                ItemSlide = new Bitmap(CIAT.Layout.TotalSize.Width, CIAT.Layout.TotalSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                g = Graphics.FromImage(ItemSlide);
                EraseBrush = new SolidBrush(CIAT.Layout.BackColor);
                g.FillRectangle(EraseBrush, 0, 0, CIAT.Layout.InteriorSize.Width, CIAT.Layout.InteriorSize.Height);
                OutlinePen = new Pen(CIAT.Layout.OutlineColor, CIAT.Layout.ResponseValueRectMargin >> 1);
                ErasePen = new Pen(CIAT.Layout.BackColor, CIAT.Layout.ResponseValueRectMargin >> 1);
                LeftOutlineRectangle = CIAT.Layout.LeftKeyValueRectangle;
                LeftOutlineRectangle.Inflate(CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.ResponseValueRectMargin);
                RightOutlineRectangle = CIAT.Layout.RightKeyValueRectangle;
                RightOutlineRectangle.Inflate(CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.ResponseValueRectMargin);

            }

            // grab the left response key value
            DisplayItem = Block.Key.LeftValue;
            ptOrigin.X = CIAT.Layout.LeftKeyValueRectangle.Left +
                ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
            ptOrigin.Y = CIAT.Layout.LeftKeyValueRectangle.Top +
                ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
            leftKeyID = IATDisplayItems.Count;
            IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(leftKeyID, DisplayItem, ptOrigin, IATName);
            IATDisplayItems.Add(IATDisplayItem);
            if (!IsPracticeBlock)
            {
                DisplayItem.IATImage.Lock();
                g.DrawImage(DisplayItem.IATImage.theImage, ptOrigin);
                DisplayItem.IATImage.Unlock();
            }

            // grab the right response key value
            DisplayItem = Block.Key.RightValue;
            ptOrigin.X = CIAT.Layout.RightKeyValueRectangle.Left +
                ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
            ptOrigin.Y = CIAT.Layout.RightKeyValueRectangle.Top +
                ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
            rightKeyID = IATDisplayItems.Count;
            IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(rightKeyID, DisplayItem, ptOrigin, IATName);
            IATDisplayItems.Add(IATDisplayItem);
            if (!IsPracticeBlock)
            {
                DisplayItem.IATImage.Lock();
                g.DrawImage(DisplayItem.IATImage.theImage, ptOrigin);
                DisplayItem.IATImage.Unlock();
            }

            // grab the instructions display item
            DisplayItem = Block.Instructions;
            ptOrigin = CIAT.Layout.InstructionsRectangle.Location;
            instructionsID = IATDisplayItems.Count;
            IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(instructionsID, DisplayItem, ptOrigin, IATName);
            IATDisplayItems.Add(IATDisplayItem);
            if (!IsPracticeBlock)
            {
                DisplayItem.IATImage.Lock();
                g.DrawImage(DisplayItem.IATImage.theImage, ptOrigin);
                DisplayItem.IATImage.Unlock();
            }

            // generate the start block event
            IATEvent = new IATConfigFileNamespace.BeginIATBlock(Block.GetIndex(Block), instructionsID, leftKeyID, rightKeyID);
            ((IATConfigFileNamespace.BeginIATBlock)IATEvent).NumPresentations = Block.NumPresentations;
            ((IATConfigFileNamespace.BeginIATBlock)IATEvent).NumItems = Block.NumItems;
            if (Block.AlternateBlock == null)
                ((IATConfigFileNamespace.BeginIATBlock)IATEvent).AlternatedWith = -1;
            else
                ((IATConfigFileNamespace.BeginIATBlock)IATEvent).AlternatedWith = Block.GetIndex(Block.AlternateBlock);
            IATEvents.Add(IATEvent);


            // process the items
            for (int ctr = 0; ctr < Block.NumItems; ctr++)
            {
                int ItemNum = ProcessIATItem(Block[ctr], IsPracticeBlock, blockNum);
                if (!IsPracticeBlock)
                {
                    Image img = Block[ctr].GeneratePreviewImage();
                    SaveItemSlide(img, ItemNum, String.Empty);
                    img.Dispose();
                }
            }

            // dispose of the graphics objects, if item slides were written
            if (!IsPracticeBlock)
            {
                g.Dispose();
                ItemSlide.Dispose();
                EraseBrush.Dispose();
                ErasePen.Dispose();
                OutlinePen.Dispose();
            }

            // generate the end block event
            IATEvent = new IATConfigFileNamespace.EndIATBlock();
            IATEvents.Add(IATEvent);
        }

        private void ProcessTextInstructionScreen(IATClient.CTextInstructionScreen screen)
        {
            // grab the instructions image
            Point ptOrigin = CIAT.Layout.InstructionScreenTextAreaRectangle.Location;
            int instructionsID = IATDisplayItems.Count;
            IATConfigFileNamespace.IATDisplayItem pdi = new IATConfigFileNamespace.IATDisplayItem(instructionsID, screen.Instructions, ptOrigin, IATName);
            IATDisplayItems.Add(pdi);

            // grab the continue instructions image
            CDisplayItem di = screen.ContinueInstructions;
            ptOrigin = CIAT.Layout.ContinueInstructionsRectangle.Location;
            ptOrigin.X += (CIAT.Layout.ContinueInstructionsRectangle.Width - di.ItemSize.Width) >> 1;
            ptOrigin.Y += CIAT.Layout.ContinueInstructionsRectangle.Height - di.ItemSize.Height;
            int continueInstructionsID = IATDisplayItems.Count;
            pdi = new IATConfigFileNamespace.IATDisplayItem(continueInstructionsID, di, ptOrigin, IATName);
            IATDisplayItems.Add(pdi);

            // add the instruction screen event
            IATConfigFileNamespace.TextInstructionScreen InstrScr = new IATConfigFileNamespace.TextInstructionScreen(screen.ContinueKeyChar, continueInstructionsID, instructionsID);
            IATEvents.Add(InstrScr);
        }

        private void ProcessMockItemInstructionScreen(IATClient.CMockItemScreen screen)
        {
            IATConfigFileNamespace.IATDisplayItem IATDisplayItem;
            CDisplayItem DisplayItem;
            Point ptOrigin = new Point();
            int leftKeyID, rightKeyID, instructionsID;
            bool outlineLeftResponse, outlineRightResponse;

            // grab the left response key value
            DisplayItem = CIATKey.KeyDictionary[screen.ResponseKeyName].LeftValue;
            ptOrigin.X = CIAT.Layout.LeftKeyValueRectangle.Left +
                ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
            ptOrigin.Y = CIAT.Layout.LeftKeyValueRectangle.Top +
                ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
            leftKeyID = IATDisplayItems.Count;
            IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(leftKeyID, DisplayItem, ptOrigin, IATName);
            IATDisplayItems.Add(IATDisplayItem);

            // grab the right response key value
            DisplayItem = CIATKey.KeyDictionary[screen.ResponseKeyName].RightValue;
            ptOrigin.X = CIAT.Layout.RightKeyValueRectangle.Left +
                ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
            ptOrigin.Y = CIAT.Layout.RightKeyValueRectangle.Top +
                ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
            rightKeyID = IATDisplayItems.Count;
            IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(rightKeyID, DisplayItem, ptOrigin, IATName);
            IATDisplayItems.Add(IATDisplayItem);

            // grab the stimulus
            int stimulusID;
            DisplayItem = screen.MockItemStimulus;
            ptOrigin = CIAT.Layout.StimulusRectangle.Location;
            switch (DisplayItem.Type)
            {
                case CDisplayItem.EType.text:
                    ptOrigin.X += (CIAT.Layout.StimulusRectangle.Width - DisplayItem.ItemSize.Width) >> 1;
                    ptOrigin.Y += CIAT.Layout.TextStimulusPaddingTop;
                    stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
                    if (stimulusID == -1)
                    {
                        stimulusID = IATDisplayItems.Count;
                        IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(stimulusID, DisplayItem, ptOrigin, IATName);
                        IATDisplayItems.Add(IATDisplayItem);
                    }
                    break;

                case CDisplayItem.EType.stimulusImage:
                    CStimulusImageItem idi = (CStimulusImageItem)DisplayItem;
                    Size idiSize = idi.ItemSize;
                    ptOrigin.Y += (CIAT.Layout.StimulusRectangle.Height - idiSize.Height) >> 1;
                    ptOrigin.X += (CIAT.Layout.StimulusRectangle.Width - idiSize.Width) >> 1;
                    stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
                    if (stimulusID == -1)
                    {
                        stimulusID = IATDisplayItems.Count;
                        IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(stimulusID, DisplayItem, ptOrigin, IATName);
                        IATDisplayItems.Add(IATDisplayItem);
                    }
                    break;

                default:
                    stimulusID = int.MinValue;
                    break;
            }

            // grab the instructions display item
            DisplayItem = screen.BriefInstructions;
            ptOrigin = CIAT.Layout.MockItemInstructionsRectangle.Location;
            instructionsID = IATDisplayItems.Count;
            IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(instructionsID, DisplayItem, ptOrigin, IATName);
            IATDisplayItems.Add(IATDisplayItem);

            // grab the continue instructions display item
            DisplayItem = screen.ContinueInstructions;
            ptOrigin = CIAT.Layout.ContinueInstructionsRectangle.Location;
            ptOrigin.X += (CIAT.Layout.ContinueInstructionsRectangle.Width - DisplayItem.ItemSize.Width) >> 1;
            ptOrigin.Y += CIAT.Layout.ContinueInstructionsRectangle.Height - DisplayItem.ItemSize.Height;
            int continueInstructionsID = IATDisplayItems.Count;
            IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(continueInstructionsID, DisplayItem, ptOrigin, IATName);
            IATDisplayItems.Add(IATDisplayItem);

            // determine if responses are to be outlined
            outlineLeftResponse = outlineRightResponse = false;
            if (screen.KeyedDirOutlined)
            {
                if (screen.MockItemKeyedDir == IATClient.CIATItem.EKeyedDir.Left)
                    outlineLeftResponse = true;
                else if (screen.MockItemKeyedDir == IATClient.CIATItem.EKeyedDir.Right)
                    outlineRightResponse = true;
            }

            // create the mock item screeen event
            IATConfigFileNamespace.MockItemInstructionScreen InstrScr = new IATConfigFileNamespace.MockItemInstructionScreen(screen.ContinueKeyChar, continueInstructionsID, leftKeyID, rightKeyID, stimulusID,
                instructionsID, screen.InvalidResponseFlag, outlineLeftResponse, outlineRightResponse);
            IATEvents.Add(InstrScr);
        }

        private void ProcessKeyedInstructionScreen(IATClient.CKeyInstructionScreen screen)
        {
            // grab the instructions image
            Point ptOrigin = CIAT.Layout.KeyInstructionScreenTextAreaRectangle.Location;
            int instructionsID = IATDisplayItems.Count;
            IATConfigFileNamespace.IATDisplayItem pdi = new IATConfigFileNamespace.IATDisplayItem(instructionsID, screen.Instructions, ptOrigin, IATName);
            IATDisplayItems.Add(pdi);

            CDisplayItem DisplayItem;
            IATConfigFileNamespace.IATDisplayItem IATDisplayItem;
            int leftKeyID, rightKeyID;

            // grab the left response key value
            DisplayItem = CIATKey.KeyDictionary[screen.ResponseKeyName].LeftValue;
            ptOrigin.X = CIAT.Layout.LeftKeyValueRectangle.Left +
                ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
            ptOrigin.Y = CIAT.Layout.LeftKeyValueRectangle.Top +
                ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
            leftKeyID = IATDisplayItems.Count;
            IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(leftKeyID, DisplayItem, ptOrigin, IATName);
            IATDisplayItems.Add(IATDisplayItem);

            // grab the right response key value
            DisplayItem = CIATKey.KeyDictionary[screen.ResponseKeyName].RightValue;
            ptOrigin.X = CIAT.Layout.RightKeyValueRectangle.Left +
                ((CIAT.Layout.KeyValueSize.Width - DisplayItem.ItemSize.Width) >> 1);
            ptOrigin.Y = CIAT.Layout.RightKeyValueRectangle.Top +
                ((CIAT.Layout.KeyValueSize.Height - DisplayItem.ItemSize.Height) >> 1);
            rightKeyID = IATDisplayItems.Count;
            IATDisplayItem = new IATConfigFileNamespace.IATDisplayItem(rightKeyID, DisplayItem, ptOrigin, IATName);
            IATDisplayItems.Add(IATDisplayItem);

            // grab the continue instructions image
            CDisplayItem di = screen.ContinueInstructions;
            ptOrigin = CIAT.Layout.ContinueInstructionsRectangle.Location;
            ptOrigin.X += (CIAT.Layout.ContinueInstructionsRectangle.Width - di.ItemSize.Width) >> 1;
            ptOrigin.Y += CIAT.Layout.ContinueInstructionsRectangle.Height - di.ItemSize.Height;
            int continueInstructionsID = IATDisplayItems.Count;
            pdi = new IATConfigFileNamespace.IATDisplayItem(continueInstructionsID, di, ptOrigin, IATName);
            IATDisplayItems.Add(pdi);

            // add the instruction screen event
            IATConfigFileNamespace.KeyedInstructionScreen InstrScr = new IATConfigFileNamespace.KeyedInstructionScreen(screen.ContinueKeyChar, continueInstructionsID, instructionsID, rightKeyID, leftKeyID);
            IATEvents.Add(InstrScr);
        }

        private void ProcessInstructionBlock(IATClient.CInstructionBlock InstructionBlock)
        {
            IATConfigFileNamespace.BeginInstructionBlock beginInstructions = new IATConfigFileNamespace.BeginInstructionBlock();
            if (InstructionBlock.AlternateInstructionBlock == null)
                beginInstructions.AlternatedWith = -1;
            else
                beginInstructions.AlternatedWith = InstructionBlock.GetBlockIndex(InstructionBlock.AlternateInstructionBlock);
            beginInstructions.NumInstructionScreens = InstructionBlock.NumScreens;
            IATEvents.Add(beginInstructions);
            for (int ctr = 0; ctr < InstructionBlock.NumScreens; ctr++)
            {
                if (InstructionBlock[ctr].Type == IATClient.CInstructionScreen.EType.MockItem)
                    ProcessMockItemInstructionScreen((CMockItemScreen)InstructionBlock[ctr]);
                else if (InstructionBlock[ctr].Type == IATClient.CInstructionScreen.EType.Text)
                    ProcessTextInstructionScreen((IATClient.CTextInstructionScreen)InstructionBlock[ctr]);
                else if (InstructionBlock[ctr].Type == IATClient.CInstructionScreen.EType.Key)
                    ProcessKeyedInstructionScreen((IATClient.CKeyInstructionScreen)InstructionBlock[ctr]);
                else
                    throw new Exception("Instruction screen of unspecified type encountered during IAT Packaging");
            }
        }

        private void ProcessSurveys(BinaryWriter bWriter)
        {
            bWriter.Write(IAT.BeforeSurvey.Count);
            for (int ctr1 = 0; ctr1 < IAT.BeforeSurvey.Count; ctr1++)
            {
                // store the schema-less XML used by the XSLT code
                MemoryStream beforeSurveyStream = new MemoryStream();
                XmlTextWriter xmlWriter = new XmlTextWriter(beforeSurveyStream, Encoding.UTF8);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Survey");
                xmlWriter.WriteAttributeString("IAT", IATName);
                xmlWriter.WriteAttributeString("FileName", IAT.BeforeSurvey[ctr1].FileNameBase);
                xmlWriter.WriteAttributeString("SurveyName", IAT.BeforeSurvey[ctr1].Name);
                xmlWriter.WriteAttributeString("TimeoutMillis", (IAT.BeforeSurvey[ctr1].Timeout * 60000).ToString());
                for (int ctr2 = 0; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++)
                    IAT.BeforeSurvey[ctr1].Items[ctr2].WriteToXml(xmlWriter);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                bWriter.Write(Convert.ToInt32(beforeSurveyStream.Length));
                bWriter.Write(beforeSurveyStream.ToArray(), 0, (int)beforeSurveyStream.Length);
                beforeSurveyStream.Dispose();
                bWriter.Flush();
                
                // store the schema-ed XML used for result file processing
                Survey s = new Survey(IAT.BeforeSurvey[ctr1].Name);
                s.Timeout = (int)(IAT.BeforeSurvey[ctr1].Timeout * 60000);
                if (IAT.BeforeSurvey[ctr1].Items[0].IsCaption)
                {
                    CSurveyItem[] surveyItems = new CSurveyItem[IAT.BeforeSurvey[ctr1].Items.Count - 1];
                    s.HasCaption = true;
                    int itemCtr = 0;
                    s.SetCaption(IAT.BeforeSurvey[ctr1].Items[0]);
                    for (int ctr2 = 1; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++)
                    {
                        if (IAT.BeforeSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                            itemCtr++;
                        surveyItems[ctr2 - 1] = IAT.BeforeSurvey[ctr1].Items[ctr2];
                    }
                    s.NumItems = itemCtr;
                    s.SetItems(surveyItems);
                }
                else
                {
                    CSurveyItem[] surveyItems = new CSurveyItem[IAT.BeforeSurvey[ctr1].Items.Count];
                    s.HasCaption = false;
                    int itemCtr = 0;
                    for (int ctr2 = 0; ctr2 < IAT.BeforeSurvey[ctr1].Items.Count; ctr2++)
                    {
                        if (IAT.BeforeSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                            itemCtr++;
                        surveyItems[ctr2] = IAT.BeforeSurvey[ctr1].Items[ctr2];
                    }
                    s.NumItems = itemCtr;
                    s.SetItems(surveyItems);
                }
                XmlSerializer ser = new XmlSerializer(typeof(Survey));
                MemoryStream schemaedStream = new MemoryStream();
                ser.Serialize(schemaedStream, s);
                bWriter.Write(Convert.ToInt32(schemaedStream.Length));
                bWriter.Write(schemaedStream.ToArray(), 0, (int)schemaedStream.Length);
                schemaedStream.Dispose();
                bWriter.Flush();
            }

            bWriter.Write(IAT.AfterSurvey.Count);
            for (int ctr1 = 0; ctr1 < IAT.AfterSurvey.Count; ctr1++) {
                // store the schema-less XML used by the XSLT code
                MemoryStream afterSurveyStream = new MemoryStream();
                XmlTextWriter xmlWriter = new XmlTextWriter(afterSurveyStream, Encoding.UTF8);
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Survey");
                xmlWriter.WriteAttributeString("IAT", IATName);
                xmlWriter.WriteAttributeString("FileName", IAT.AfterSurvey[ctr1].FileNameBase);
                xmlWriter.WriteAttributeString("SurveyName", IAT.AfterSurvey[ctr1].Name);
                xmlWriter.WriteAttributeString("TimeoutMillis", (IAT.AfterSurvey[ctr1].Timeout * 60000).ToString());
                for (int ctr2 = 0; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
                    IAT.AfterSurvey[ctr1].Items[ctr2].WriteToXml(xmlWriter);
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
                xmlWriter.Flush();
                bWriter.Write(Convert.ToInt32(afterSurveyStream.Length));
                bWriter.Write(afterSurveyStream.ToArray(), 0, (int)afterSurveyStream.Length);
                afterSurveyStream.Dispose();
                bWriter.Flush();

                // store the schema-ed XML used for result file processing
                Survey s = new Survey(IAT.AfterSurvey[ctr1].Name);
                s.Timeout = (int)(IAT.AfterSurvey[ctr1].Timeout * 60000);
                if (IAT.AfterSurvey[ctr1].Items[0].IsCaption)
                {
                    CSurveyItem[] surveyItems = new CSurveyItem[IAT.AfterSurvey[ctr1].Items.Count - 1];
                    s.HasCaption = true;
                    int itemCtr = 0;
                    s.SetCaption(IAT.AfterSurvey[ctr1].Items[0]);
                    for (int ctr2 = 1; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
                    {
                        if (IAT.AfterSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                            itemCtr++;
                        surveyItems[ctr2 - 1] = IAT.AfterSurvey[ctr1].Items[ctr2];
                    }
                    s.NumItems = itemCtr;
                    s.SetItems(surveyItems);
                }
                else
                {
                    CSurveyItem[] surveyItems = new CSurveyItem[IAT.AfterSurvey[ctr1].Items.Count];
                    s.HasCaption = false;
                    int itemCtr = 0;
                    for (int ctr2 = 0; ctr2 < IAT.AfterSurvey[ctr1].Items.Count; ctr2++)
                    {
                        if (IAT.AfterSurvey[ctr1].Items[ctr2].Response.ResponseType != CResponse.EResponseType.Instruction)
                            itemCtr++;
                        surveyItems[ctr2] = IAT.AfterSurvey[ctr1].Items[ctr2];
                    }
                    s.NumItems = itemCtr;
                    s.SetItems(surveyItems);
                }
                XmlSerializer ser = new XmlSerializer(typeof(Survey));
                MemoryStream schemaedStream = new MemoryStream();
                ser.Serialize(schemaedStream, s);
                bWriter.Write(Convert.ToInt32(schemaedStream.Length));
                bWriter.Write(schemaedStream.ToArray(), 0, (int)schemaedStream.Length);
                schemaedStream.Dispose();
                bWriter.Flush();
            }
        }

        public void run()
        {
            FileStream fStream = null;
            ItemSlideStream = null;

            try
            {
                IATConfigFileNamespace.ConfigFile CF = new IATConfigFileNamespace.ConfigFile(IAT);
                nItemSlides = 0;
                IATDisplayItems = new List<IATConfigFileNamespace.IATDisplayItem>();
                ItemSlideStream = new FileStream(Properties.Resources.sTempItemSlideFileName, FileMode.Create);
                IATEvents = new List<IATConfigFileNamespace.IATEvent>();
                IATConfigFileNamespace.IATItem.ResetItemCounter();
                OutputDirectory = outputDirectory;
                SetProgressRange(0, IAT.Contents.Count);
                SetStatusMessage(Properties.Resources.sProcessingIATFile);
                ResetProgress();

                // create streams for IAT packaging
                fStream = new FileStream(outputDirectory + System.IO.Path.DirectorySeparatorChar + packageFilename, FileMode.Create);
                BinaryWriter bWriter = new BinaryWriter(fStream, Encoding.Unicode);
                
                // process the iat
                int nBlock = 1;
                int nIATItems = 0;
                for (int ctr = 0; ctr < IAT.Contents.Count; ctr++)
                {
                    ProgressIncrement(1);
                    switch (IAT.Contents[ctr].Type)
                    {
                        case ContentsItemType.IATBlock:
                            ProcessIATBlock((CIATBlock)IAT.Contents[ctr], false, nBlock++);
                            if (RandomizationType == IATConfigFileNamespace.ConfigFile.ERandomizationType.SetNumberOfPresentations)
                                nIATItems += ((CIATBlock)IAT.Contents[ctr]).NumPresentations;
                            else
                                nIATItems += ((CIATBlock)IAT.Contents[ctr]).NumItems;
                            break;

                        case ContentsItemType.IATPracticeBlock:
                            ProcessIATBlock((CIATBlock)IAT.Contents[ctr], true, -1);
                            break;

                        case ContentsItemType.InstructionBlock:
                            ProcessInstructionBlock((CInstructionBlock)IAT.Contents[ctr]);
                            break;
                    }
                }

                // generate the error mark
                int ErrorMarkID = IATDisplayItems.Count;
                CIAT.Layout.ErrorMarkIATImage.Lock(); 
                CMemoryImageDisplayItem mdi = new CMemoryImageDisplayItem(CIAT.Layout.ErrorMarkIATImage.ImageSize.Width, CIAT.Layout.ErrorMarkIATImage.ImageSize.Height, false,
                    CIAT.Layout.ErrorMarkIATImage.theImage);
                CIAT.Layout.ErrorMarkIATImage.Unlock();
                IATConfigFileNamespace.IATDisplayItem pdi = new IATConfigFileNamespace.IATDisplayItem(ErrorMarkID, mdi, CIAT.Layout.ErrorRectangle.Location, IATName);
                IATDisplayItems.Add(pdi);

                Point ptOrigin;
                Graphics g;

                // generate key outline boxes
                Brush outlineBrush = new SolidBrush(CIAT.Layout.OutlineColor);
                Brush backBrush = new SolidBrush(CIAT.Layout.BackColor);
                Pen outlinePen = new Pen(outlineBrush, CIAT.Layout.ResponseValueRectMargin >> 1);

                int LeftKeyOutlineID = IATDisplayItems.Count;
                CMemoryImageDisplayItem LeftKeyOutline = new CMemoryImageDisplayItem(CIAT.Layout.KeyValueSize.Width + (CIAT.Layout.ResponseValueRectMargin << 1),
                    CIAT.Layout.KeyValueSize.Height + (CIAT.Layout.ResponseValueRectMargin << 1), false);
                g = Graphics.FromImage(LeftKeyOutline.MemoryBmp);
                g.FillRectangle(backBrush, new Rectangle(0, 0, LeftKeyOutline.ItemSize.Width, LeftKeyOutline.ItemSize.Height));
                g.DrawRectangle(outlinePen, new Rectangle(CIAT.Layout.ResponseValueRectMargin >> 1, CIAT.Layout.ResponseValueRectMargin >> 1, CIAT.Layout.KeyValueSize.Width +
                    CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.KeyValueSize.Height + CIAT.Layout.ResponseValueRectMargin));
                g.Dispose();
                ptOrigin = CIAT.Layout.LeftKeyValueRectangle.Location - new Size(CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.ResponseValueRectMargin) +
                    new Size(CIAT.Layout.BorderWidth, CIAT.Layout.BorderWidth);
                pdi = new IATConfigFileNamespace.IATDisplayItem(LeftKeyOutlineID, LeftKeyOutline, ptOrigin, IATName);
                IATDisplayItems.Add(pdi);

                int RightKeyOutlineID = IATDisplayItems.Count;
                CMemoryImageDisplayItem RightKeyOutline = new CMemoryImageDisplayItem(CIAT.Layout.KeyValueSize.Width + (CIAT.Layout.ResponseValueRectMargin << 1),
                    CIAT.Layout.KeyValueSize.Height + (CIAT.Layout.ResponseValueRectMargin << 1), false);
                g = Graphics.FromImage(RightKeyOutline.MemoryBmp);
                g.FillRectangle(backBrush, new Rectangle(0, 0, LeftKeyOutline.ItemSize.Width, LeftKeyOutline.ItemSize.Height));
                g.DrawRectangle(outlinePen, new Rectangle(CIAT.Layout.ResponseValueRectMargin >> 1, CIAT.Layout.ResponseValueRectMargin >> 1, CIAT.Layout.KeyValueSize.Width +
                    CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.KeyValueSize.Height + CIAT.Layout.ResponseValueRectMargin));
                g.Dispose();
                ptOrigin = CIAT.Layout.RightKeyValueRectangle.Location - new Size(CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.ResponseValueRectMargin) +
                    new Size(CIAT.Layout.BorderWidth, CIAT.Layout.BorderWidth);
                pdi = new IATConfigFileNamespace.IATDisplayItem(RightKeyOutlineID, RightKeyOutline, ptOrigin, IATName);
                IATDisplayItems.Add(pdi);

                outlinePen.Dispose();
                outlineBrush.Dispose();
                backBrush.Dispose();
                

                // construct IATSurvey arrays
                IATConfigFileNamespace.IATSurvey[] beforeSurveys = new IATConfigFileNamespace.IATSurvey[IAT.BeforeSurvey.Count];
                IATConfigFileNamespace.IATSurvey[] afterSurveys = new IATConfigFileNamespace.IATSurvey[IAT.AfterSurvey.Count];
                for (int ctr = 0; ctr < IAT.AfterSurvey.Count; ctr++)
                    afterSurveys[ctr] = new IATConfigFileNamespace.IATSurvey(IAT.AfterSurvey[ctr], 1 + IAT.BeforeSurvey.Count, false);
                for (int ctr = 0; ctr < IAT.BeforeSurvey.Count; ctr++)
                    beforeSurveys[ctr] = new IATConfigFileNamespace.IATSurvey(IAT.BeforeSurvey[ctr], 1 + ctr, false);
                ResetProgress();
                SetProgressRange(0, IATDisplayItems.Count + 2 + nItemSlides);
                SetStatusMessage(Properties.Resources.sSavingPackageFile);

                // save the images
                bWriter.Write(Convert.ToInt32(IATDisplayItems.Count));
                bWriter.Flush();
                for (int ctr = 0; ctr < IATDisplayItems.Count; ctr++)
                {
                    IATDisplayItems[ctr].SaveImageToFile(bWriter);
                    ProgressIncrement(1);
                }
                LeftKeyOutline.Dispose();
                RightKeyOutline.Dispose();

                
                // output it to packaged IAT file to XML
                MemoryStream configFileStream = new MemoryStream();
                XmlTextWriter xmlWriter = new XmlTextWriter(configFileStream, Encoding.Unicode);
                
                // build the config file
                IATConfigFileNamespace.ConfigFile cf = new IATConfigFileNamespace.ConfigFile(IAT);
                foreach (DynamicSpecifier ds in DynamicSpecifier.GetAllSpecifiers())
                    
                
                // serialize the config file
                cf.WriteXml(xmlWriter);
                xmlWriter.Flush();
                bWriter.Write(Convert.ToInt32(configFileStream.Length));
                bWriter.Write(configFileStream.ToArray(), 0, (int)configFileStream.Length);
                configFileStream.Dispose();
                bWriter.Flush();
                ProgressIncrement(1);
                ProgressIncrement(1);


                ProcessSurveys(bWriter);

                // append item slides onto package file
                ItemSlideStream.Seek(0, SeekOrigin.Begin);
                BinaryReader bReader = new BinaryReader(ItemSlideStream, Encoding.Unicode);
                bWriter.Write(Convert.ToInt32(nItemSlides));
                for (int ctr = 0; ctr < nItemSlides; ctr++)
                {
                    String FileName = bReader.ReadString();
                    bWriter.Write(FileName);
                    int nSlideSize = bReader.ReadInt32();
                    bWriter.Write(Convert.ToInt32(nSlideSize));
                    bWriter.Write(bReader.ReadBytes(nSlideSize), 0, nSlideSize);
                    ProgressIncrement(1);
                }
                bWriter.Flush();
                
                fStream.Close();
                ItemSlideStream.Close();
                System.IO.File.Delete(Properties.Resources.sTempItemSlideFileName);
                OperationComplete();
            }
            catch (Exception)
            {
                if (fStream != null)
                {
                    fStream.Close();
                    System.IO.File.Delete(outputDirectory + System.IO.Path.DirectorySeparatorChar + packageFilename);
                }
                if (File.Exists(Properties.Resources.sTempItemSlideFileName))
                {
                    if (ItemSlideStream != null)
                        ItemSlideStream.Close();
                    File.Delete(Properties.Resources.sTempItemSlideFileName);
                }
            }
        }

        /// <summary>
        /// Processes a CIAT object and packages it for consumption by the IATServlet Java application.
        /// Notes: The results are output to a directory, not a file. This directory should be placed in the parent directory
        /// of the IAT servlet, which is the "web" directory on a GlassFish server.
        /// </summary>
        /// <param name="outputDirectory">The directory to ouput the packaged IAT to.  Must end in a directory separator character.</param>
        /// <param name="iatName">The name of the IAT.  This should be the same as the name of the subdirectory the IAT files are
        /// to be placed in.  The XML file with the packaged IAT data is given this string as a file name base.</param>
        public bool ProcessIAT(IATConfigMainForm mainForm, String outputDirectory, String iatName, String packageFilename, DataPasswordForm progressWindow)
        {
            CIAT.ImageManager.Halt();
            this.outputDirectory = outputDirectory;
            this._IATName = iatName;
            this.packageFilename = packageFilename;
            MainForm = mainForm;
            ThreadStart threadStart = new ThreadStart(run);
            Thread packager = new Thread(threadStart);
            packager.Start();
            
            return true;
        }

        public CIATPackager(CIAT iat)
        {
            IATDisplayItems = null;
            IATEvents = null;
            _IATName = String.Empty;
            _OutputDirectory = String.Empty;
            IAT = iat;
            _LeftResponseChar = _RightResponseChar = String.Empty;
            _RandomizationType = IATConfigFileNamespace.ConfigFile.ERandomizationType.None;
            _LeftResponseChar = "E";
            _RightResponseChar = "I";
            _DataRetrievalPassword = String.Empty;
            _RedirectionURL = String.Empty;
            OnDirectoryExists = null;
        }

        public bool IsValid()
        {
            if (LeftResponseChar == RightResponseChar)
                return false;
            if ((DataRetrievalPassword.Length < 6) || (DataRetrievalPassword.Length > 15))
                return false;
            return true;
        }

        public void WriteToXml(XmlTextWriter writer)
        {
            writer.WriteStartElement("IATPackager");
            writer.WriteElementString("IATName", IATName);
            writer.WriteElementString("DataRetrievalPassword", DataRetrievalPassword);
            writer.WriteElementString("RedirectionURL", RedirectionURL);
            writer.WriteElementString("LeftResponseChar", LeftResponseChar);
            writer.WriteElementString("RigthResponseChar", RightResponseChar);
            writer.WriteElementString("RandomizationType", ((int)RandomizationType).ToString());

            writer.WriteEndElement();
        }

        public bool LoadFromXml(XmlNode node)
        {
            if (node.ChildNodes.Count != 6)
                return false;

            _IATName = node.ChildNodes[0].InnerText;
            DataRetrievalPassword = node.ChildNodes[1].InnerText;
            RedirectionURL = node.ChildNodes[2].InnerText;
            LeftResponseChar = node.ChildNodes[3].InnerText;
            RightResponseChar = node.ChildNodes[4].InnerText;
            RandomizationType = (IATConfigFileNamespace.ConfigFile.ERandomizationType)(Convert.ToInt32(node.ChildNodes[5].InnerText));

            return true;
        }
    }*/
}

