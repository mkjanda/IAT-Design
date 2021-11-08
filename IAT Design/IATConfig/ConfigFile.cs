using System;
using System.Timers;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Threading;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Concurrent;

namespace IATClient.IATConfig
{


    public class ConfigFile : INamedXmlSerializable, IXmlSerializable
    {
        private String _ServerDomain = String.Empty, _ServerPath = String.Empty;
        private int _ServerPort = -1;
        private List<IATEvent> IATEventList;
        private int NumIATItems { get; set; } = 0;
        private int NumSlidesProcessed { get; set; } = 0;
        private bool _Is7Block;
        private String _RedirectOnComplete;
        private int _LeftResponseASCIIKeyCodeUpper, _RightResponseASCIIKeyCodeUpper, _LeftResponseASCIIKeyCodeLower, _RightResponseASCIIKeyCodeLower;
        private int _ClientID = -1;
        public enum ERandomizationType { None, RandomOrder, SetNumberOfPresentations };
        private ERandomizationType _RandomizationType;
        private int _ErrorMarkID;
        private int _LeftKeyOutlineID, _RightKeyOutlineID;
        private bool _PrefixSelfAlternatingSurveys;
        private List<IATSurvey> _BeforeSurveys, _AfterSurveys;
        private IATLayout _Layout;
        private IATEventList _EventList;
        //        private DisplayItemList _DisplayItems;
        private String _Name = String.Empty;
        private String _DataRetrievalPassword = "xxx";
        private List<DynamicSpecifier> _DynamicSpecifiers = new List<DynamicSpecifier>();
        private CIAT IAT;
        private int ItemCtr = 0;
        private bool _HasUniqueResponses;
        private long _UploadTimeMillis = -1;
        private SHA512Managed SHA512 = new SHA512Managed();
        private ManualResetEvent IATImagesProcessed = new ManualResetEvent(false);
        public ManualResetEvent SlidesProcessed { get; private set; } = new ManualResetEvent(false);
        public ImageContainer IATImages { get; private set; }
        public ImageContainer SlideImages { get; private set; }


        public long UploadTimeMillis
        {
            get
            {
                return _UploadTimeMillis;
            }
            set
            {
                _UploadTimeMillis = value;
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

        public String ServerDomain
        {
            get
            {
                return _ServerDomain;
            }
            set
            {
                _ServerDomain = value;
            }
        }

        public String ServerPath
        {
            get
            {
                return _ServerPath;
            }
            set
            {
                _ServerPath = value;
            }
        }

        public int ServerPort
        {
            get
            {
                return _ServerPort;
            }
            set
            {
                _ServerPort = value;
            }
        }

        public bool Is7Block
        {
            get
            {
                return _Is7Block;
            }
            set
            {
                _Is7Block = value;
            }
        }

        public String RedirectOnComplete
        {
            get
            {
                return _RedirectOnComplete;
            }
            set
            {
                _RedirectOnComplete = value;
            }
        }

        public int LeftResponseASCIIKeyCodeUpper
        {
            get
            {
                return _LeftResponseASCIIKeyCodeUpper;
            }
            set
            {
                _LeftResponseASCIIKeyCodeUpper = value;
            }
        }

        public int RightResponseASCIIKeyCodeUpper
        {
            get
            {
                return _RightResponseASCIIKeyCodeUpper;
            }
            set
            {
                _RightResponseASCIIKeyCodeUpper = value;
            }
        }

        public int LeftResponseASCIIKeyCodeLower
        {
            get
            {
                return _LeftResponseASCIIKeyCodeLower;
            }
            set
            {
                _LeftResponseASCIIKeyCodeLower = value;
            }
        }

        public int RightResponseASCIIKeyCodeLower
        {
            get
            {
                return _RightResponseASCIIKeyCodeLower;
            }
            set
            {
                _RightResponseASCIIKeyCodeLower = value;
            }
        }

        public ERandomizationType RandomizationType
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

        public int ErrorMarkID
        {
            get
            {
                return _ErrorMarkID;
            }
            set
            {
                _ErrorMarkID = value;
            }
        }

        public int LeftKeyOutlineID
        {
            get
            {
                return _LeftKeyOutlineID;
            }
            set
            {
                _LeftKeyOutlineID = value;
            }
        }

        public int RightKeyOutlineID
        {
            get
            {
                return _RightKeyOutlineID;
            }
            set
            {
                _RightKeyOutlineID = value;
            }
        }

        public bool PrefixSelfAlternatingSurveys
        {
            get
            {
                return _PrefixSelfAlternatingSurveys;
            }
            set
            {
                _PrefixSelfAlternatingSurveys = value;
            }
        }

        public List<IATSurvey> BeforeSurveys
        {
            get
            {
                return _BeforeSurveys;
            }
        }

        public List<IATSurvey> AfterSurveys
        {
            get
            {
                return _AfterSurveys;
            }
        }

        public IATLayout Layout
        {
            get
            {
                return _Layout;
            }
            set
            {
                _Layout = value;
            }
        }

        public IATEventList EventList
        {
            get
            {
                return _EventList;
            }
        }
        /*
        public DisplayItemList DisplayItems
        {
            get
            {
                return _DisplayItems;
            }
        }

        
        public ImageContainer DisplayItemImages
        {
            get
            {
                return _DisplayItemImages;
            }
        }
        */
        public bool HasUniqiueResponses
        {
            get
            {
                return _HasUniqueResponses;
            }
        }

        public int ClientID
        {
            get
            {
                return _ClientID;
            }
            set
            {
                _ClientID = value;
            }
        }

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
            return IATImages.GetImage(u);
        }

        private void ProcessIATItem(IATClient.CIATItem item, CIATBlock block)
        {
            DIBase DisplayItem = item.Stimulus as DIBase;
            IATImages.AddDI(DisplayItem);
            SlideImages.AddDI(item.GetDIPreview(block.URI));
            IATEventList.Add(new IATItem()
            {
                ConfigFile = this,
                ItemUri = DisplayItem.URI,
                KeyedDir = item.GetKeyedDirection(block.URI),
                BlockNum = block.IndexInContainer + 1,
                OriginatingBlock = item.OriginatingBlock,
                ItemNum = ++ItemCtr
            });
            
        }

        private bool ProcessIATBlock(CIATBlock Block, bool IsPracticeBlock, int blockNum)
        {
            IATImages.AddDI(CIAT.SaveFile.GetDI(Block.InstructionsUri));
            IATImages.AddDI(CIAT.SaveFile.GetDI(Block.Key.LeftValue.URI));
            IATImages.AddDI(CIAT.SaveFile.GetDI(Block.Key.RightValue.URI));
            IATEventList.Add(new BeginIATBlock()
            {
                ConfigFile = this,
                BlockUri = Block.URI
            });
            for (int ctr = 0; ctr < Block.NumItems; ctr++)
                ProcessIATItem(Block[ctr], Block);
            IATEventList.Add(new EndIATBlock());
            return true;
        }

        private void ProcessTextInstructionScreen(CTextInstructionScreen screen)
        {
            IATImages.AddDI(CIAT.SaveFile.GetDI(screen.InstructionsUri));
            IATImages.AddDI(CIAT.SaveFile.GetDI(screen.URI));
            IATImages.AddDI(CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri));
            var processedScreen= new TextInstructionScreen()
            {
                ConfigFile = this,
                InstructionScreen = screen
            };
            IATEventList.Add(processedScreen);
        }

        private void ProcessMockItemInstructionScreen(IATClient.CMockItemScreen screen)
        {
            IATImages.AddDI(CIAT.SaveFile.GetDI(screen.StimulusUri));
            IATImages.AddDI(CIAT.SaveFile.GetDI(CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).LeftValueUri));
            IATImages.AddDI(CIAT.SaveFile.GetDI(CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).RightValueUri));
            IATImages.AddDI(CIAT.SaveFile.GetDI(screen.InstructionsUri));
            IATImages.AddDI(CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri));
            IATEventList.Add(new MockItemInstructionScreen()
            {
                ConfigFile = this,
                InstructionScreen = screen
            });
        }

        private void ProcessKeyedInstructionScreen(IATClient.CKeyInstructionScreen screen)
        {
            IATImages.AddDI(CIAT.SaveFile.GetDI(CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).LeftValueUri));
            IATImages.AddDI(CIAT.SaveFile.GetDI(CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).RightValueUri));
            IATImages.AddDI(CIAT.SaveFile.GetDI(screen.InstructionsUri));
            IATImages.AddDI(CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri));
            IATEventList.Add(new KeyedInstructionScreen()
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
            _EventList.Add(beginInstructions);
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
            _BeforeSurveys = new List<IATSurvey>();
            _AfterSurveys = new List<IATSurvey>();
 //           _DisplayItems = new DisplayItemList();
            _EventList = new IATEventList();
            Layout = new IATLayout();
        }

        static public ConfigFile LoadFromXml(XmlReader reader)
        {
            ConfigFile cf = new ConfigFile();
            cf.ReadXml(reader);
            return cf;
        }

        public ConfigFile(CIAT iat)
        {
            IAT = iat;
            _Name = IAT.Name;
            _BeforeSurveys = new List<IATSurvey>();
            for (int ctr = 0; ctr < IAT.BeforeSurvey.Count; ctr++)
                _BeforeSurveys.Add(new IATSurvey(IAT.BeforeSurvey[ctr], 0, IATSurvey.EType.BeforeSurvey));
            _AfterSurveys = new List<IATSurvey>();
            for (int ctr = 0; ctr < IAT.AfterSurvey.Count; ctr++)
                _AfterSurveys.Add(new IATSurvey(IAT.AfterSurvey[ctr], IAT.BeforeSurvey.Count + 1 + ctr, IATSurvey.EType.AfterSurvey));
            IATImages = new ImageContainer((DIBase di) =>
            {
                var memStream = new MemoryStream();
                var bmp = di.IImage.Img;
                bmp.Save(memStream, di.IImage.ImageFormat.Format);
                IATImage img = new IATImage()
                {
                    ImageData = memStream.ToArray(),
                    Format = di.IImage.ImageFormat.Format
                };
                bmp.Dispose();
                memStream.Dispose();
                return img;
            }, IATImagesProcessed);
            SlideImages = new ImageContainer((DIBase di) =>
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
//            _DisplayItems = new DisplayItemList();
            _EventList = new IATEventList();
            Layout = new IATLayout(CIAT.SaveFile.Layout);
            if (iat.UniqueResponse.ItemNum != -1)
                _HasUniqueResponses = true;
            _PrefixSelfAlternatingSurveys = true; // AlternationGroup.PrefixSelfAlternatingSurveys;
            _RandomizationType = ERandomizationType.SetNumberOfPresentations;
            _Is7Block = true; // IAT.Is7Block;
            if (!Is7Block)
            {
                throw new NotImplementedException("As of yet, only the upload of 7-Block IATs is permitted.  Please consult the documentation for information on how to construct them.");
            }
            NumIATItems = IAT.Contents.Where(c => c.Type == ContentsItemType.IATBlock).Cast<CIATBlock>().Select(b => b.NumItems).Sum();
            _RedirectOnComplete = IAT.RedirectionURL;
            _LeftResponseASCIIKeyCodeLower = System.Text.Encoding.ASCII.GetBytes(IAT.LeftResponseChar.ToLower())[0];
            _LeftResponseASCIIKeyCodeUpper = System.Text.Encoding.ASCII.GetBytes(IAT.LeftResponseChar.ToUpper())[0];
            _RightResponseASCIIKeyCodeLower = System.Text.Encoding.ASCII.GetBytes(IAT.RightResponseChar.ToLower())[0];
            _RightResponseASCIIKeyCodeUpper = System.Text.Encoding.ASCII.GetBytes(IAT.RightResponseChar.ToUpper())[0];
            IATImages.AddDI(CIAT.SaveFile.Layout.ErrorMark);
            IATImages.AddDI(CIAT.SaveFile.Layout.LeftKeyValueOutline);
            IATImages.AddDI(CIAT.SaveFile.Layout.RightKeyValueOutline);
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
            //            foreach (CDynamicSpecifier ds in CDynamicSpecifier.GetAllSpecifiers())
            //              DynamicSpecifiers.Add(ds.GetSerializableSpecifier());
//            CDynamicSpecifier.CompactSpecifierDictionary(IAT);
            SlideImages.AddDI(null);
            IATImages.AddDI(null);
            IATImagesProcessed.WaitOne();
        }


        public String GetName()
        {
            return "ConfigFile";
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement("ConfigFile");
            writer.WriteAttributeString("NumBeforeSurveys", BeforeSurveys.Count.ToString());
            writer.WriteAttributeString("NumAfterSurveys", AfterSurveys.Count.ToString());
            writer.WriteAttributeString("HasUniqueResponse", _HasUniqueResponses.ToString().ToLower());
            writer.WriteElementString("IATName", Name);
            writer.WriteElementString("ServerDomain", ServerDomain);
            writer.WriteElementString("ServerPath", ServerPath);
            writer.WriteElementString("ServerPort", ServerPort.ToString());
            writer.WriteElementString("UploadTimeMillis", UploadTimeMillis.ToString());
            writer.WriteElementString("ClientID", ClientID.ToString());
            writer.WriteElementString("NumIATItems", NumIATItems.ToString());
            writer.WriteElementString("Is7Block", Is7Block.ToString());
            writer.WriteElementString("RedirectOnComplete", RedirectOnComplete.ToString());
            writer.WriteElementString("LeftResponseASCIIKeyCodeUpper", LeftResponseASCIIKeyCodeUpper.ToString());
            writer.WriteElementString("RightResponseASCIIKeyCodeUpper", RightResponseASCIIKeyCodeUpper.ToString());
            writer.WriteElementString("LeftResponseASCIIKeyCodeLower", LeftResponseASCIIKeyCodeLower.ToString());
            writer.WriteElementString("RightResponseASCIIKeyCodeLower", RightResponseASCIIKeyCodeLower.ToString());
            writer.WriteElementString("RandomizationType", RandomizationType.ToString());
            writer.WriteElementString("ErrorMarkID", ErrorMarkID.ToString());
            writer.WriteElementString("LeftKeyOutlineID", LeftKeyOutlineID.ToString());
            writer.WriteElementString("RightKeyOutlineID", RightKeyOutlineID.ToString());
            writer.WriteElementString("PrefixSelfAlternatingSurveys", PrefixSelfAlternatingSurveys.ToString());
            foreach (DynamicSpecifier ds in DynamicSpecifiers)
                ds.WriteXml(writer);
            for (int ctr = 0; ctr < BeforeSurveys.Count; ctr++)
                BeforeSurveys[ctr].WriteXml(writer);
            for (int ctr = 0; ctr < AfterSurveys.Count; ctr++)
                AfterSurveys[ctr].WriteXml(writer);
            Layout.WriteXml(writer);
            EventList.WriteXml(writer);
            IATImages.WriteXml(writer);
            writer.WriteEndElement();
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            int nBeforeSurveys = Convert.ToInt32(reader["NumBeforeSurveys"]);
            int nAfterSurveys = Convert.ToInt32(reader["NumAfterSurveys"]);
            _HasUniqueResponses = Convert.ToBoolean(reader["HasUniqueResponse"]);
            reader.ReadStartElement();
            Name = reader.ReadElementString("IATName");
            ServerDomain = reader.ReadElementString("ServerDomain");
            ServerPath = reader.ReadElementString("ServerPath");
            ServerPort = Convert.ToInt32(reader.ReadElementString("ServerPort"));
            if (reader.Name == "UploadTimeMillis")
                UploadTimeMillis = Convert.ToInt64(reader.ReadElementString("UploadTimeMillis"));
            ClientID = Convert.ToInt32(reader.ReadElementString());
            NumIATItems = Convert.ToInt32(reader.ReadElementString());
            Is7Block = Convert.ToBoolean(reader.ReadElementString());
            RedirectOnComplete = reader.ReadElementString();
            LeftResponseASCIIKeyCodeUpper = Convert.ToInt32(reader.ReadElementString());
            RightResponseASCIIKeyCodeUpper = Convert.ToInt32(reader.ReadElementString());
            LeftResponseASCIIKeyCodeLower = Convert.ToInt32(reader.ReadElementString());
            RightResponseASCIIKeyCodeLower = Convert.ToInt32(reader.ReadElementString());
            RandomizationType = (ERandomizationType)Enum.Parse(typeof(ERandomizationType), reader.ReadElementString());
            ErrorMarkID = Convert.ToInt32(reader.ReadElementString());
            LeftKeyOutlineID = Convert.ToInt32(reader.ReadElementString());
            RightKeyOutlineID = Convert.ToInt32(reader.ReadElementString());
            PrefixSelfAlternatingSurveys = Convert.ToBoolean(reader.ReadElementString());
            DynamicSpecifiers.Clear();
            while (reader.Name == "DynamicSpecifier")
                DynamicSpecifiers.Add(DynamicSpecifier.CreateFromXml(reader));
            BeforeSurveys.Clear();
            AfterSurveys.Clear();
            while (reader.Name == "IATSurvey")
            {
                IATSurvey survey = IATSurvey.GetIATSurvey(reader);
                if (survey.SurveyType == IATSurvey.EType.BeforeSurvey)
                    BeforeSurveys.Add(survey);
                else
                    AfterSurveys.Add(survey);
            }
            Layout.ReadXml(reader);
            EventList.ReadXml(reader);
            IATImages.ReadXml(reader);
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }

    class UniqueResponseItem
    {
        private List<String> UniqueResponses = new List<String>();
        private String SurveyName;
        private int ItemNum;
        private bool Additive;

        public UniqueResponseItem(CUniqueResponse resp)
        {
            Additive = resp.Additive;
            ItemNum = resp.ItemNum;
            Regex exp = new Regex("[^A-Z0-9a-z]");
            SurveyName = exp.Replace(CIAT.SaveFile.GetSurvey(resp.SurveyUri).Name, "");
            if (!Additive)
                UniqueResponses.AddRange(resp.Values);
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