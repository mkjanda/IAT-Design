using IATClient.ResultData;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient.IATConfig
{


    public class ConfigFile : INamedXmlSerializable
    {
        private int NumIATItems { get; set; } = 0;
        public int NumBeforeSurveys
        {
            get
            {
                return BeforeSurveys.Count;
            }
        }
        public int NumAfterSurveys
        {
            get
            {
                return AfterSurveys.Count;
            }
        }
        private int _ClientID = -1;
        public enum ERandomizationType { None, RandomOrder, SetNumberOfPresentations };
        private ERandomizationType _RandomizationType;
        public List<IATImage> DisplayItems { get; private set; } = new List<IATImage>();
        private int _ErrorMarkID;
        private int _LeftKeyOutlineID, _RightKeyOutlineID;
        private bool _PrefixSelfAlternatingSurveys;
        public List<ResultData.Survey> IATBeforeSurveys { get; private set; } = new List<ResultData.Survey>();
        public List<ResultData.Survey> IATAfterSurveys { get; private set; } = new List<ResultData.Survey>();
        private IATLayout _Layout;
        private IATEventList _EventList;
        private String _DataRetrievalPassword = "xxx";
        private List<DynamicSpecifier> _DynamicSpecifiers = new List<DynamicSpecifier>();
        private CIAT IAT;
        private int ItemCtr = 0;
        private bool _HasUniqueResponses;
        private long _UploadTimeMillis = -1;
        private SHA512Managed SHA512 = new SHA512Managed();
        private ManualResetEvent IATImagesProcessed = new ManualResetEvent(false);
        public ManualResetEvent SlidesProcessed { get; private set; } = new ManualResetEvent(false);
        public ManualResetEvent SurveyImagesProcessed { get; private set; } = new ManualResetEvent(false);
        private ImageContainer _IATImages;
        public ImageContainer IATImages
        {
            get
            {
                IATImagesProcessed.WaitOne();
                return _IATImages;
            }
        }
        private ImageContainer _SlideImages;
        public ImageContainer SlideImages
        {
            get
            {
                SlidesProcessed.WaitOne();
                return _SlideImages;
            }
        }
        public CUniqueResponse UniqueResponse { get; set; }
        public UniqueResponseItem URI { get; private set; } = null;

        public long UploadTimeMillis { get; set; }

        public String Name { get; set; }

        public int NumItems
        {
            get
            {
                return EventList.Where(evt => evt.EventType == IATEvent.EEventType.IATItem).Count();
            }
        }

        public int NumPresentations
        {
            get
            {
                return EventList.Where(evt => evt.EventType == IATEvent.EEventType.BeginIATBlock).Cast<BeginIATBlock>().Select(evt => evt.NumPresentations).Sum();
            }
        }

        public String ServerDomain { get; set; }

        public String ServerPath { get; set; }

        public int ServerPort { get; set; }

        public bool Is7Block { get; set; } = true;

        public String RedirectOnComplete { get; set; }

        public String LeftResponseKey { get; private set; } 

        public String RightResponseKey { get; private set; } 

        public ERandomizationType RandomizationType { get; set; }

        public int ErrorMarkID { get; set; }

        public int LeftKeyOutlineID { get; set; }

        public int RightKeyOutlineID { get; set; }

        public bool PrefixSelfAlternatingSurveys { get; set; }

        public List<IATClient.CSurvey> BeforeSurveys { get; private set; } = new List<IATClient.CSurvey>();
        public List<String> SurveyB64Xml { get; private set; } = new List<String>();

        public List<IATClient.CSurvey> AfterSurveys { get; private set; } = new List<IATClient.CSurvey>();

        public IATLayout Layout { get; set; }

        public IATEventList EventList
        {
            get
            {
                return _EventList;
            }
        }

        public int ClientID { get; set; }

        public List<DynamicSpecifier> DynamicSpecifiers
        {
            get
            {
                return _DynamicSpecifiers;
            }
        }

        public int NumBlocks
        {
            get
            {
                int nBlocks = 0;
                for (int ctr = 0; ctr < EventList.Count; ctr++)
                    if (EventList[ctr].EventType == IATEvent.EEventType.BeginIATBlock)
                        nBlocks++;
                return nBlocks;
            }
        }

        public uint GetNumPresentationsInBlock(int nBlock)
        {
            int blockCtr = 0;
            for (int ctr = 0; ctr < EventList.Count; ctr++)
                if (EventList[ctr].EventType == IATEvent.EEventType.BeginIATBlock)
                    if (++blockCtr == nBlock)
                        return (uint)((BeginIATBlock)EventList[ctr]).NumPresentations;
            return 0;
        }


        public IATImage GetIATImage(Uri u)
        {
            return _IATImages.GetImage(u);
        }

        public List<IATImage> GetIATImages(Uri u)
        {
            return _IATImages.GetImages(u);
        }

        private void ProcessIATItem(IATClient.CIATItem item, CIATBlock block)
        {
            DIBase DisplayItem = item.Stimulus as DIBase;
     //       _IATImages.AddDI(DisplayItem, new Rectangle(new Point(DIType.StimulusImage.GetBoundingRectangle().X + (DIType.StimulusImage.GetBoundingRectangle().Width -
       //         item.Stimulus.AbsoluteBounds.Width >> 1), DIType.StimulusImage.GetBoundingRectangle().Top +
         //       (DIType.StimulusImage.GetBoundingRectangle().Height - item.Stimulus.AbsoluteBounds.Height >> 1)), item.Stimulus.AbsoluteBounds.Size));
            _IATImages.AddDI(DisplayItem, DIType.StimulusImage.GetBoundingRectangle());
            _SlideImages.AddDI(item.GetDIPreview(block.URI), DIType.Preview.GetBoundingRectangle());
            EventList.Add(new IATItem()
            {
                ConfigFile = this,
                ItemUri = item.URI,
                KeyedDir = item.GetKeyedDirection(block.URI),
                BlockNum = block.IndexInContainer + 1,
                OriginatingBlock = item.OriginatingBlock,
                ItemNum = ++ItemCtr
            });

        }

        private bool ProcessIATBlock(CIATBlock Block, bool IsPracticeBlock, int blockNum)
        {
            var lValue = Block.Key.LeftValue;
            var rValue = Block.Key.RightValue;
            var instructions = CIAT.SaveFile.GetDI(Block.InstructionsUri);
            //            _IATImages.AddDI(CIAT.SaveFile.GetDI(Block.InstructionsUri), new Rectangle(new Point(DIType.IatBlockInstructions.GetBoundingRectangle().Left +
            //              (DIType.IatBlockInstructions.GetBoundingRectangle().Width - instructions.AbsoluteBounds.Width >> 1),
            //            DIType.IatBlockInstructions.GetBoundingRectangle().Top + (DIType.IatBlockInstructions.GetBoundingRectangle().Height -
            //          instructions.AbsoluteBounds.Height >> 1)), instructions.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(Block.InstructionsUri), DIType.IatBlockInstructions.GetBoundingRectangle());
//            _IATImages.AddDI(CIAT.SaveFile.GetDI(Block.Key.LeftValue.URI), new Rectangle(new Point(CIAT.SaveFile.Layout.LeftKeyValueRectangle.X + 
  //              (CIAT.SaveFile.Layout.LeftKeyValueRectangle.Width - lValue.AbsoluteBounds.Width >> 1), CIAT.SaveFile.Layout.LeftKeyValueRectangle.Top +
    //            (CIAT.SaveFile.Layout.LeftKeyValueRectangle.Height - lValue.AbsoluteBounds.Height >> 1)), lValue.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(Block.Key.LeftValue.URI), CIAT.SaveFile.Layout.LeftKeyValueRectangle);
//            _IATImages.AddDI(CIAT.SaveFile.GetDI(Block.Key.RightValue.URI), new Rectangle(new Point(CIAT.SaveFile.Layout.RightKeyValueRectangle.X + 
  //              (CIAT.SaveFile.Layout.RightKeyValueRectangle.Width - rValue.AbsoluteBounds.Width >> 1), CIAT.SaveFile.Layout.RightKeyValueRectangle.Top +
    //            (CIAT.SaveFile.Layout.RightKeyValueRectangle.Height - rValue.AbsoluteBounds.Height >> 1)), rValue.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(Block.Key.RightValue.URI), CIAT.SaveFile.Layout.RightKeyValueRectangle);
            EventList.Add(new BeginIATBlock()
            {
                ConfigFile = this,
                BlockUri = Block.URI
            });
            for (int ctr = 0; ctr < Block.NumItems; ctr++)
                ProcessIATItem(Block[ctr], Block);
            EventList.Add(new EndIATBlock());
            return true;
        }

        private void ProcessTextInstructionScreen(CTextInstructionScreen screen)
        {
            var instructions = CIAT.SaveFile.GetDI(screen.InstructionsUri);
            var continueInstructions = CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri);
            //            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.InstructionsUri), new Rectangle(new Point(DIType.TextInstructionsScreen.GetBoundingRectangle().X +
            //              (DIType.TextInstructionsScreen.GetBoundingRectangle().Width - instructions.AbsoluteBounds.Width >> 1),
            //            DIType.TextInstructionsScreen.GetBoundingRectangle().Y +
            //          (DIType.TextInstructionsScreen.GetBoundingRectangle().Height - instructions.AbsoluteBounds.Height >> 1)),
            //        instructions.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.InstructionsUri), DIType.TextInstructionsScreen.GetBoundingRectangle());
            //            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri), new Rectangle(new Point(DIType.ContinueInstructions.GetBoundingRectangle().X +
            //               (DIType.ContinueInstructions.GetBoundingRectangle().Width - continueInstructions.AbsoluteBounds.Width >> 1),
            //              DIType.ContinueInstructions.GetBoundingRectangle().Y + (DIType.ContinueInstructions.GetBoundingRectangle().Height - 
            //             continueInstructions.AbsoluteBounds.Height >> 1)), instructions.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri), DIType.ContinueInstructions.GetBoundingRectangle());
            var processedScreen = new TextInstructionScreen()
            {
                ConfigFile = this,
                InstructionScreen = screen
            };
            EventList.Add(processedScreen);
        }

        private void ProcessMockItemInstructionScreen(IATClient.CMockItemScreen screen)
        {
            var stim = CIAT.SaveFile.GetDI(screen.StimulusUri);
            var lKey = CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).LeftValue;
            var rKey = CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).RightValue;
            var instructions = CIAT.SaveFile.GetDI(screen.InstructionsUri);
            var continueInstr = CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri);
            //            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.StimulusUri), new Rectangle(new Point(DIType.StimulusImage.GetBoundingRectangle().X + 
            //               (DIType.StimulusImage.GetBoundingRectangle().Width - stim.AbsoluteBounds.Width >> 1), DIType.StimulusImage.GetBoundingRectangle().Top +
            //             (DIType.StimulusImage.GetBoundingRectangle().Height - stim.AbsoluteBounds.Height >> 1)), stim.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.StimulusUri), DIType.StimulusImage.GetBoundingRectangle());
            //            _IATImages.AddDI(CIAT.SaveFile.GetDI(CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).LeftValueUri), 
            //              new Rectangle(new Point(CIAT.SaveFile.Layout.LeftKeyValueRectangle.X + (CIAT.SaveFile.Layout.LeftKeyValueRectangle.Width -
            //            lKey.AbsoluteBounds.Width >> 1), CIAT.SaveFile.Layout.LeftKeyValueRectangle.Top +
            //          (CIAT.SaveFile.Layout.LeftKeyValueRectangle.Height - lKey.AbsoluteBounds.Height >> 1)), lKey.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).LeftValueUri), CIAT.SaveFile.Layout.LeftKeyValueRectangle);
            //            _IATImages.AddDI(CIAT.SaveFile.GetDI(CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).RightValueUri), 
            //               new Rectangle(new Point(CIAT.SaveFile.Layout.RightKeyValueRectangle.X + (CIAT.SaveFile.Layout.RightKeyValueRectangle.Width -
            //              rKey.AbsoluteBounds.Width >> 1), CIAT.SaveFile.Layout.RightKeyValueRectangle.Top +
            //             (CIAT.SaveFile.Layout.RightKeyValueRectangle.Height - rKey.AbsoluteBounds.Height >> 1)), rKey.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).RightValueUri), CIAT.SaveFile.Layout.RightKeyValueRectangle);
            //                new Rectangle(new Point(CIAT.SaveFile.Layout.RightKeyValueRectangle.X + (CIAT.SaveFile.Layout.RightKeyValueRectangle.Width -
            //              rKey.AbsoluteBounds.Width >> 1), CIAT.SaveFile.Layout.RightKeyValueRectangle.Top +
            //            (CIAT.SaveFile.Layout.RightKeyValueRectangle.Height - rKey.AbsoluteBounds.Height >> 1)), rKey.AbsoluteBounds.Size));
            //      _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.InstructionsUri), new Rectangle(new Point(DIType.MockItemInstructions.GetBoundingRectangle().X + 
            //                (DIType.MockItemInstructions.GetBoundingRectangle().Width - instructions.AbsoluteBounds.Width >> 1), 
            //              DIType.MockItemInstructions.GetBoundingRectangle().Top + (DIType.MockItemInstructions.GetBoundingRectangle().Height - 
            //            instructions.AbsoluteBounds.Height >> 1)), instructions.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.InstructionsUri), DIType.MockItemInstructions.GetBoundingRectangle());
            //        (DIType.MockItemInstructions.GetBoundingRectangle().Width - instructions.AbsoluteBounds.Width >> 1),
            //      DIType.MockItemInstructions.GetBoundingRectangle().Top + (DIType.MockItemInstructions.GetBoundingRectangle().Height -
            //    instructions.AbsoluteBounds.Height >> 1)), instructions.AbsoluteBounds.Size));
            //            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri), new Rectangle(new Point(DIType.ContinueInstructions.GetBoundingRectangle().X +
            //              (DIType.ContinueInstructions.GetBoundingRectangle().Width - continueInstr.AbsoluteBounds.Width >> 1), DIType.ContinueInstructions.GetBoundingRectangle().Top +
            //            (DIType.ContinueInstructions.GetBoundingRectangle().Height - continueInstr.AbsoluteBounds.Height >> 1)), continueInstr.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri), DIType.ContinueInstructions.GetBoundingRectangle());
            
            EventList.Add(new MockItemInstructionScreen()
            {
                ConfigFile = this,
                InstructionScreen = screen
            });
        }

        private void ProcessKeyedInstructionScreen(IATClient.CKeyInstructionScreen screen)
        {
            var lKey = CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).LeftValue;
            var rKey = CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).RightValue;
            var instructions = CIAT.SaveFile.GetDI(screen.InstructionsUri);
            var continueInstr = CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri);
            var diRects = new Dictionary<Uri, Rectangle>();
           _IATImages.AddDI(lKey, CIAT.SaveFile.Layout.LeftKeyValueRectangle);
            _IATImages.AddDI(rKey, CIAT.SaveFile.Layout.RightKeyValueRectangle);

            //            _IATImages.AddDI(CIAT.SaveFile.GetDI(CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).LeftValueUri, CIAT.SaveFile.Layout.LeftKeyValueRectangle);
            //            _IATImages.AddDI(CIAT.SaveFile.GetDI(CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).RightValueUri), 
            //                new Rectangle(new Point(CIAT.SaveFile.Layout.RightKeyValueRectangle.X + (CIAT.SaveFile.Layout.RightKeyValueRectangle.Width -
            //               rKey.AbsoluteBounds.Width >> 1), CIAT.SaveFile.Layout.RightKeyValueRectangle.Top +
            //              (CIAT.SaveFile.Layout.RightKeyValueRectangle.Height - rKey.AbsoluteBounds.Height >> 1)), rKey.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.InstructionsUri), DIType.KeyedInstructionsScreen.GetBoundingRectangle());

            //           _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri), new Rectangle(new Point(DIType.ContinueInstructions.GetBoundingRectangle().X + (DIType.ContinueInstructions.GetBoundingRectangle().Width -
            //               continueInstr.AbsoluteBounds.Width >> 1), DIType.ContinueInstructions.GetBoundingRectangle().Top +
            //              (DIType.ContinueInstructions.GetBoundingRectangle().Height - continueInstr.AbsoluteBounds.Height >> 1)), continueInstr.AbsoluteBounds.Size));
            _IATImages.AddDI(CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri), DIType.ContinueInstructions.GetBoundingRectangle());
            EventList.Add(new KeyedInstructionScreen()
            {
                ConfigFile = this,
                InstructionScreen = screen
            });
        }


        private bool ProcessInstructionBlock(IATClient.CInstructionBlock InstructionBlock)
        {
            BeginInstructionBlock beginInstructions = new BeginInstructionBlock();
            if (InstructionBlock.AlternateInstructionBlock == null)
                beginInstructions.AlternatedWith = -1;
            else
                beginInstructions.AlternatedWith = InstructionBlock.AlternateInstructionBlock.IndexInContainer + 1;
            beginInstructions.NumInstructionScreens = InstructionBlock.NumScreens;
            EventList.Add(beginInstructions);
            for (int ctr = 0; ctr < InstructionBlock.NumScreens; ctr++)
            {
                if (InstructionBlock[ctr].Type == InstructionScreenType.MockItem)
                    ProcessMockItemInstructionScreen((CMockItemScreen)InstructionBlock[ctr]);
                else if (InstructionBlock[ctr].Type == InstructionScreenType.Text)
                    ProcessTextInstructionScreen((IATClient.CTextInstructionScreen)InstructionBlock[ctr]);
                else if (InstructionBlock[ctr].Type == InstructionScreenType.ResponseKey)
                    ProcessKeyedInstructionScreen((IATClient.CKeyInstructionScreen)InstructionBlock[ctr]);
                else
                    throw new Exception("Instruction screen of unspecified type encountered during IAT Packaging");
            }
            return true;
        }

        static public ConfigFile GetConfigFile()
        {
            return new ConfigFile();
        }

        protected ConfigFile()
        {
            _EventList = new IATEventList();
            Layout = new IATLayout();
        }

        static public ConfigFile LoadFromXml(XDocument doc)
        {
            ConfigFile cf = new ConfigFile();
            cf.Load(doc);
            return cf;
        }

        public ConfigFile(CIAT iat)
        {
            IAT = iat;
            Name = IAT.Name;
            XmlSerializer surveySerializer = new XmlSerializer(typeof(Survey));
            _IATImages = new ImageContainer((DIBase di) =>
            {
                var memStream = new MemoryStream();
                var bmp = di.IImage.Img;
                IATImage retVal = null;
                ManualResetEvent mEvt = new ManualResetEvent(true);
                if (bmp == null)
                {
                    di.IImage.Changed += (evt, i, o) =>
                    {
                        i.Img.Save(memStream, i.ImageFormat.Format);
                        retVal = new IATImage()
                        {
                            ImageData = memStream.ToArray(),
                            Format = i.ImageFormat.Format
                        };
                        mEvt.Set();
                    };
                    mEvt.Reset();
                    di.ScheduleInvalidation();
                    mEvt.WaitOne();
                }
                else
                {
                    bmp.Save(memStream, di.IImage.ImageFormat.Format);
                    retVal = new IATImage()
                    {
                        ImageData = memStream.ToArray(),
                        Format = di.IImage.ImageFormat.Format
                    };
                    memStream.Dispose();
                    bmp.Dispose();
                }
                return retVal;
            }, IATImagesProcessed);
            _SlideImages = new ImageContainer((DIBase di) =>
            {
                var img = (di as DIPreview).SaveToJpeg();
                var memStream = new MemoryStream();
                img.Save(memStream, System.Drawing.Imaging.ImageFormat.Jpeg);
                img.Dispose();
                var iImg = new IATImage()
                {
                    ImageData = memStream.ToArray(),
                    Format = System.Drawing.Imaging.ImageFormat.Jpeg
                };
                memStream.Dispose();
                return iImg;
            }, SlidesProcessed);
            BeforeSurveys.AddRange(IAT.BeforeSurvey);
            AfterSurveys.AddRange(IAT.AfterSurvey);
            var surveyStream = new MemoryStream();
            var b64Encode = new ToBase64Transform();
            var surveyImageItems = new List<CSurveyItemImage>();
            for (int ctr = 0; ctr < BeforeSurveys.Count + AfterSurveys.Count; ctr++)
            {
                var survey = (ctr < BeforeSurveys.Count) ? BeforeSurveys[ctr] : AfterSurveys[ctr - BeforeSurveys.Count];
                var s = new Survey(survey.Name);
                s.Timeout = (int)(survey.Timeout * 60000);
                s.HasCaption = survey.Items[0].IsCaption;
                if (s.HasCaption)
                    s.SetCaption(survey.Items[0]);
                s.SetItems(survey.Items.Where(si => si.ItemType == SurveyItemType.Item).ToArray());
                s.NumItems = survey.Items.Where(si => (si.ItemType == SurveyItemType.Item) &&
                    (si.Response.ResponseType != CResponse.EResponseType.Instruction)).Count();
                var cStream = new CryptoStream(surveyStream, b64Encode, CryptoStreamMode.Write);
                surveySerializer.Serialize(cStream, s);
                cStream.FlushFinalBlock();
                SurveyB64Xml.Add(System.Text.Encoding.UTF8.GetString(surveyStream.ToArray()));
                surveyStream.Dispose();
  //              foreach (var si in survey.Items.Where(i => i is CSurveyItemImage).Cast<CSurveyItemImage>()) {
    //                _IATImages.AddDI(si.SurveyImage);
      //              surveyImageItems.Add(si);
//                }
            }
            _EventList = new IATEventList();
            Layout = new IATLayout(CIAT.SaveFile.Layout);
            if (iat.UniqueResponse.ItemNum != -1)
            {
                _HasUniqueResponses = true;
                URI = new UniqueResponseItem(iat.UniqueResponse);
            }
            _PrefixSelfAlternatingSurveys = true; // AlternationGroup.PrefixSelfAlternatingSurveys;
            _RandomizationType = ERandomizationType.SetNumberOfPresentations;
            Is7Block = true; // IAT.Is7Block;
            if (!Is7Block)
            {
                throw new NotImplementedException("As of yet, only the upload of 7-Block IATs is permitted.  Please consult the documentation for information on how to construct them.");
            }
            NumIATItems = IAT.Contents.Where(c => c.Type == ContentsItemType.IATBlock).Cast<CIATBlock>().Select(b => b.NumItems).Sum();
            RedirectOnComplete = IAT.RedirectionURL;
            _IATImages.AddDI(CIAT.SaveFile.Layout.ErrorMark, CIAT.SaveFile.Layout.ErrorRectangle);
            _IATImages.AddDI(CIAT.SaveFile.Layout.LeftKeyValueOutline, CIAT.SaveFile.Layout.LeftKeyValueOutlineRectangle);
            _IATImages.AddDI(CIAT.SaveFile.Layout.RightKeyValueOutline, CIAT.SaveFile.Layout.RightKeyValueOutlineRectangle);
            for (int ctr = 0; ctr < IAT.Contents.Count; ctr++)
            {
                if (IAT.Contents[ctr].Type == ContentsItemType.IATBlock)
                {
                    CIATBlock block = (CIATBlock)IAT.Contents[ctr];
                    if (!ProcessIATBlock(block, false, block.IndexInContainer))
                        throw new PackagingException(String.Format("Error Packaging IAT Block #{0}", ((CIATBlock)IAT.Contents[ctr]).IndexInContainer + 1));
                }
                if (IAT.Contents[ctr].Type == ContentsItemType.InstructionBlock)
                {
                    CInstructionBlock iBlock = (CInstructionBlock)IAT.Contents[ctr];
                    if (!ProcessInstructionBlock(iBlock))
                        throw new PackagingException(String.Format("Error Packaging IAT Instruction #{0}", ((CInstructionBlock)IAT.Contents[ctr]).IndexInContainer + 1));
                }
            }
            //            foreach (DynamicSpecifier ds in DynamicSpecifier.GetAllSpecifiers())
            //              DynamicSpecifiers.Add(ds.GetSerializableSpecifier());
            //            DynamicSpecifier.CompactSpecifierDictionary(IAT);
            _SlideImages.AddDI(null, Rectangle.Empty);
            _IATImages.AddDI(null, Rectangle.Empty);
            IATImagesProcessed.WaitOne();
   //         foreach (var si in surveyImageItems)
   //             si.ResourceId = _IATImages.GetImage(si.SurveyImage.IImage.URI).Indexes[0];
        }


        public String GetName()
        {
            return "ConfigFile";
        }

        public void WriteXml(XmlWriter writer)
        {
            IATImagesProcessed.WaitOne();
            writer.WriteStartElement("ConfigFile");
            writer.WriteAttributeString("NumBeforeSurveys", NumBeforeSurveys.ToString());
            writer.WriteAttributeString("NumAfterSurveys", NumAfterSurveys.ToString());
            writer.WriteElementString("IATName", Name);  
            writer.WriteElementString("ServerDomain", ServerDomain);
            writer.WriteElementString("ServerPath", ServerPath);
            writer.WriteElementString("ServerPort", ServerPort.ToString());
            writer.WriteElementString("ClientID", ClientID.ToString());
            writer.WriteElementString("NumIATItems", NumIATItems.ToString());
            writer.WriteElementString("IsSevenBlock", Is7Block.ToString());
            writer.WriteElementString("RedirectOnComplete", CIAT.SaveFile.IAT.RedirectionURL.ToString());
            writer.WriteElementString("LeftResponseKey", IAT.LeftResponseChar.ToLower());
            writer.WriteElementString("RightResponseKey", IAT.RightResponseChar.ToLower());
            writer.WriteElementString("RandomizationType", RandomizationType.ToString());
            writer.WriteElementString("ErrorMarkID", ErrorMarkID.ToString());
            writer.WriteElementString("LeftKeyOutlineID", _IATImages.GetImage(CIAT.SaveFile.Layout.LeftKeyValueOutline.URI).Id.ToString());
            writer.WriteElementString("RightKeyOutlineID", _IATImages.GetImage(CIAT.SaveFile.Layout.RightKeyValueOutline.URI).Id.ToString());
            writer.WriteElementString("PrefixSelfAlternatingSurveys", PrefixSelfAlternatingSurveys.ToString());
            foreach (DynamicSpecifier ds in DynamicSpecifiers)
                ds.WriteXml(writer);
            foreach (var survey in IAT.BeforeSurvey)
                survey.WriteXml(writer);
            foreach (var survey in IAT.AfterSurvey)
                survey.WriteXml(writer);
            foreach (var b64Survey in SurveyB64Xml)
                writer.WriteElementString("SurveyB64Xml", b64Survey);
            if (UniqueResponse != null)
            {
                var uri = new UniqueResponseItem(UniqueResponse);
                uri.WriteXml(writer);
            }
            Layout.WriteXml(writer);
            EventList.WriteXml(writer);
            _IATImages.WriteXml(writer);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            var doc = XDocument.Load(reader);
            Load(doc);
        }

        public IATConfig.IATSurvey GetSurvey(int ndx)
        {
            XmlSerializer ser = new XmlSerializer(typeof(IATConfig.IATSurvey));
            MemoryStream memStream = new MemoryStream(Convert.FromBase64String(SurveyB64Xml[ndx]));
            var survey = (IATConfig.IATSurvey)ser.Deserialize(memStream);
            memStream.Dispose();
            return survey;
        }

        public void Load(XDocument doc)
        {
            var bSurveys = Convert.ToInt32(doc.Root.Attribute("NumBeforeSurveys").Value);
            var aSurveys = Convert.ToInt32(doc.Root.Attribute("NumAfterSurveys").Value);
            _HasUniqueResponses = Convert.ToBoolean(doc.Root.Attribute("HasUniqueResponse").Value);
            Name = doc.Root.Element("IATName").Value;
            ServerDomain = doc.Root.Element("ServerDomain").Value;
            ServerPath = doc.Root.Element("ServerPath").Value;
            ServerPort = Convert.ToInt32(doc.Root.Element("ServerPort").Value);
            ClientID = Convert.ToInt32(doc.Root.Element("ClientID").Value);
            NumIATItems= Convert.ToInt32(doc.Root.Element("NumIATItems").Value);
            Is7Block = true;
            RedirectOnComplete = doc.Root.Element("RedirectOnComplete").Value;
            LeftResponseKey = doc.Root.Element("LeftResponseKey").Value;
            RightResponseKey = doc.Root.Element("RightResponseKey").Value;
            PrefixSelfAlternatingSurveys = Convert.ToBoolean(doc.Root.Element("PrefixSelfAlternatingSurveys").Value);
            foreach (var s in doc.Root.Elements("SurveyB64Xml"))
            {
                var ser = new XmlSerializer(typeof(ResultData.Survey));
                var survey = ser.Deserialize(new StringReader(System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(s.Value)))) as ResultData.Survey;
                if (s.ElementsBeforeSelf("SurveyB64Xml").Count() < bSurveys)
                    IATBeforeSurveys.Add(survey);
                else
                    IATAfterSurveys.Add(survey);
            }
            URI = new UniqueResponseItem();
            URI.Additive = Convert.ToBoolean(doc.Root.Element("UniqueResponse").Attribute("Additive").Value);
            URI.SurveyName = doc.Root.Element("UniqueResponse").Element("SurveyName").Value;
            URI.ItemNum = Convert.ToInt32(doc.Root.Element("UniqueResponse").Element("ItemNum").Value);
            Layout.Load(doc.Root.Element("Layout"));
            EventList.Load(doc.Root.Element("EventList"));
            DisplayItems.Clear();
            foreach (var di in doc.Root.Element("DisplayItemList").Elements("IATDisplayItem"))
                DisplayItems.Add(IATImage.Create(di));
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }

    public class UniqueResponseItem
    {
        private List<String> UniqueResponses = new List<String>();
        public String SurveyName { get; set; } = String.Empty;
        public int ItemNum { get; set; } = -1;
        public bool Additive { get; set; } = false;

        public UniqueResponseItem() { } 

        public UniqueResponseItem(CUniqueResponse resp)
        {
            if (resp.SurveyUri == null)
            {
                ItemNum = -1;
                SurveyName = String.Empty;
                return;
            }
            Additive = resp.Additive;
            ItemNum = resp.ItemNum;
            Regex exp = new Regex("[^A-Z0-9a-z]");
            SurveyName = exp.Replace(CIAT.SaveFile.GetSurvey(resp.SurveyUri).Name, "");
            if (!Additive)
                UniqueResponses.AddRange(resp.Values);
        }

        public void ReadXml(XmlReader reader)
        {
            if (reader.Name != "UniqueResponse")
                return;
            reader.ReadStartElement();
            Additive = Convert.ToBoolean(reader.GetAttribute("Additive"));
            SurveyName = reader.ReadElementString("SurveyName");
            ItemNum = Convert.ToInt32(reader.ReadElementString("ItemNum"));
            UniqueResponses.Clear();
            while (reader.Name == "Value")
                UniqueResponses.Add(reader.ReadElementString("Value"));
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("UniqueResponse");
            writer.WriteAttributeString("Additive", Additive.ToString());
            writer.WriteElementString("SurveyName", SurveyName);
            writer.WriteElementString("ItemNum", ItemNum.ToString());
            foreach (String val in UniqueResponses)
                writer.WriteElementString("Value", val);
            writer.WriteEndElement();
        }

        public void WriteXmlDocument(XmlWriter writer)
        {
            writer.WriteStartDocument();
            writer.WriteStartElement("UniqueResponse");
            writer.WriteAttributeString("Additive", Additive.ToString());
            writer.WriteElementString("SurveyName", SurveyName);
            writer.WriteElementString("ItemNum", ItemNum.ToString());
            foreach (String val in UniqueResponses)
                writer.WriteElementString("Value", val);
            writer.WriteEndElement();
            writer.WriteEndDocument();
        }
    }
}