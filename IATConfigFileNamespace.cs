using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.Schema;
using System.Security.Cryptography;
using System.Linq;
using System.Threading.Tasks;

namespace IATClient
{
    namespace IATConfigFileNamespace
    {

        public class PackagingException : Exception
        {
            public PackagingException(String msg)
                : base(msg)
            { }
        }

        public class IATImageContainer
        {
            class IATImage
            {
                public List<int> IDs = new List<int>();
                public String FileName;
                public byte[] SHA;
                public byte[] ImageData;
                public System.Drawing.Imaging.ImageFormat Format;
            }
            private SHA512Managed SHA512 = new SHA512Managed();
            private List<IATImage> ImageList = new List<IATImage>();

            public IEnumerable<String> Filenames
            {
                get
                {
                    return from image in ImageList select image.FileName;
                }
            }

            public IEnumerable<int> getDisplayItemIDsWithFilename(String fName)
            {
                return from image in ImageList where image.FileName == fName select image.IDs.First();
            }
            private object imageListLock = new object();

            public String AddImage(int id, MemoryStream memStream, System.Drawing.Imaging.ImageFormat format)
            {
                memStream.Seek(0, SeekOrigin.Begin);
                byte[] sha = SHA512.ComputeHash(memStream);
                lock (imageListLock)
                {
                    List<IATImage> images = new List<IATImage>(ImageList);
                    foreach (IATImage i in images)
                    {
                        bool bSame = true;
                        for (int ctr = 0; ctr < sha.Length; ctr++)
                            if (sha[ctr] != i.SHA[ctr])
                            {
                                bSame = false;
                                break;
                            }
                        if (bSame)
                        {
                            i.IDs.Add(id);
                            return i.FileName;
                        }
                    }
                    IATImage image = new IATImage();
                    image.IDs.Add(id);
                    image.FileName = String.Format("image{0}.{1}", ImageList.Count + 1, format.ToString().ToLower());
                    image.Format = format;
                    image.SHA = sha;
                    image.ImageData = memStream.ToArray();
                    ImageList.Add(image);
                    return image.FileName;
                }
            }

            public byte[] ConstructSlideManifest()
            {
                ItemSlideManifest SlideManifest = new ItemSlideManifest();
                SlideManifest.ItemSlideEntries = new TItemSlideEntry[ImageList.Count];
                for (int ctr = 0; ctr < ImageList.Count; ctr++)
                {
                    TItemSlideEntry slideEntry = new TItemSlideEntry();
                    slideEntry.Items = new uint[ImageList[ctr].IDs.Count];
                    for (int ctr2 = 0; ctr2 < ImageList[ctr].IDs.Count; ctr2++)
                        slideEntry.Items[ctr2] = (uint)ImageList[ctr].IDs[ctr2];
                    slideEntry.SlideFileName = ImageList[ctr].FileName;
                    SlideManifest.ItemSlideEntries[ctr] = slideEntry;
                }
                MemoryStream memStream = new MemoryStream();
                XmlSerializer ser = new XmlSerializer(typeof(ItemSlideManifest));
                ser.Serialize(memStream, SlideManifest);
                byte[] data = memStream.ToArray();
                memStream.Dispose();
                return data;
            }

            public String AddImage(IATDisplayItem DI)
            {
                float xScale = 1.0F /* / CIATLayout.XScale */, yScale = 1.0F /* / CIATLayout.YScale */;
                DI.X += (int)((float)((DI.SourceDisplayItem.BoundingSize.Width - DI.SourceDisplayItem.AbsoluteBounds.Width) >> 1) * xScale);
                if ((DI.SourceDisplayItem.Type != DIType.TextInstructionsScreen) && (DI.SourceDisplayItem.Type != DIType.KeyedInstructionsScreen))
                    DI.Y += (int)((float)((DI.SourceDisplayItem.BoundingSize.Height - DI.SourceDisplayItem.AbsoluteBounds.Height) >> 1) * yScale);
                MemoryStream memStream = new MemoryStream();
                DI.Width = (int)(DI.SourceDisplayItem.AbsoluteBounds.Width * xScale);
                DI.Height = (int)(DI.SourceDisplayItem.AbsoluteBounds.Height * yScale);
                if ((DI.Width == 0) || (DI.Height == 0))
                {
                    Bitmap transparentPixel = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                    transparentPixel.SetPixel(0, 0, Color.Transparent);
                    transparentPixel.Save(memStream, System.Drawing.Imaging.ImageFormat.Png);
                    transparentPixel.Dispose();
                    String fName = AddImage(DI.ID, memStream, System.Drawing.Imaging.ImageFormat.Png);
                    memStream.Dispose();
                    return fName;
                }
                else
                {
                    Image img = DI.SourceDisplayItem.IImage.Image;
                    Bitmap bmp = new Bitmap(DI.Width, DI.Height);
                    using (Graphics g = Graphics.FromImage(bmp))
                    {
                        g.SmoothingMode = SmoothingMode.HighQuality;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(img, new Rectangle(new Point(0, 0), bmp.Size), DI.SourceDisplayItem.AbsoluteBounds, GraphicsUnit.Pixel);
                    }
                    System.Drawing.Imaging.ImageFormat imgFormat = (DI.SourceDisplayItem.IImage.OriginalImage != null) ? (DI.SourceDisplayItem.IImage.OriginalImage.Format) :
                        (System.Drawing.Imaging.ImageFormat.Png);
                    bmp.Save(memStream, imgFormat);
                    img.Dispose();
                    bmp.Dispose();
                    String fName = AddImage(DI.ID, memStream, imgFormat);
                    memStream.Dispose();
                    return fName;
                }
            }
            
        

            public ManifestFile[] ConstructFileManifest()
            {
                ManifestFile[] fileManifest = new ManifestFile[ImageList.Count];
                for (int ctr = 0; ctr < ImageList.Count; ctr++)
                {
                    fileManifest[ctr] = new ManifestFile();
                    fileManifest[ctr].Name = ImageList[ctr].FileName;
                    fileManifest[ctr].Size = ImageList[ctr].ImageData.Length;
                }
                return fileManifest;
            }

            public byte[][] GetImageData()
            {
                byte[][] data = new byte[ImageList.Count][];
                for (int ctr = 0; ctr < ImageList.Count; ctr++)
                    data[ctr] = ImageList[ctr].ImageData;
                return data;
            }
        }

        public class IATLayout : INamedXmlSerializable
        {
            private int _InteriorWidth, _InteriorHeight, _BorderWidth;
            private int _ResponseWidth, _ResponseHeight;
            private System.Drawing.Color _BorderColor, _BackColor, _OutlineColor, _PageBackColor;

            public int ResponseWidth
            {
                get
                {
                    return _ResponseWidth;
                }
                set
                {
                    _ResponseWidth = value;
                }
            }

            public int ResponseHeight
            {
                get
                {
                    return _ResponseHeight;
                }
                set
                {
                    _ResponseHeight = value;
                }
            }

            public int InteriorWidth
            {
                get
                {
                    return _InteriorWidth;
                }
                set
                {
                    _InteriorWidth = value;
                }
            }

            public int InteriorHeight
            {
                get
                {
                    return _InteriorHeight;
                }
                set
                {
                    _InteriorHeight = value;
                }
            }

            public int BorderWidth
            {
                get
                {
                    return _BorderWidth;
                }
                set
                {
                    _BorderWidth = value;
                }
            }

            public System.Drawing.Color BorderColor
            {
                get
                {
                    return _BorderColor;
                }
                set
                {
                    _BorderColor = value;
                }
            }

            public System.Drawing.Color BackColor
            {
                get
                {
                    return _BackColor;
                }
                set
                {
                    _BackColor = value;
                }
            }

            public System.Drawing.Color PageBackColor
            {
                get
                {
                    return _PageBackColor;
                }
                set
                {
                    _PageBackColor = value;
                }
            }

            public System.Drawing.Color OutlineColor
            {
                get
                {
                    return _OutlineColor;
                }
                set
                {
                    _OutlineColor = value;
                }
            }


            public IATLayout()
            {
                BorderColor = CIAT.SaveFile.Layout.BorderColor;
                BackColor = CIAT.SaveFile.Layout.BackColor;
                OutlineColor = CIAT.SaveFile.Layout.OutlineColor;
                PageBackColor = CIAT.SaveFile.Layout.WebpageBackColor;
                InteriorWidth = (int)((double)CIAT.SaveFile.Layout.InteriorSize.Width);
                InteriorHeight = (int)((double)CIAT.SaveFile.Layout.InteriorSize.Height);
                BorderWidth = (int)((double)CIAT.SaveFile.Layout.BorderWidth);
                ResponseWidth = (int)((double)CIAT.SaveFile.Layout.KeyValueSize.Width);
                ResponseHeight = (int)((double)CIAT.SaveFile.Layout.KeyValueSize.Height);
            }

            public IATLayout(CIATLayout layout)
            {
                BorderColor = layout.BorderColor;
                BackColor = layout.BackColor;
                OutlineColor = layout.OutlineColor;
                PageBackColor = layout.WebpageBackColor;
                InteriorWidth = (int)((double)layout.InteriorSize.Width);
                InteriorHeight = (int)((double)layout.InteriorSize.Height);
                BorderWidth = (int)((double)layout.BorderWidth);
                ResponseWidth = (int)((double)layout.KeyValueSize.Width);
                ResponseHeight = (int)((double)layout.KeyValueSize.Height);
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("Layout");
                writer.WriteElementString("InteriorWidth", InteriorWidth.ToString());
                writer.WriteElementString("InteriorHeight", InteriorHeight.ToString());
                writer.WriteElementString("BorderWidth", BorderWidth.ToString());
                writer.WriteElementString("ResponseWidth", ResponseWidth.ToString());
                writer.WriteElementString("ResponseHeight", ResponseHeight.ToString());
                writer.WriteElementString("BorderColorR", String.Format("{0:X2}", BorderColor.R));
                writer.WriteElementString("BorderColorG", String.Format("{0:X2}", BorderColor.G));
                writer.WriteElementString("BorderColorB", String.Format("{0:X2}", BorderColor.B));
                writer.WriteElementString("BackColorR", String.Format("{0:X2}", BackColor.R));
                writer.WriteElementString("BackColorG", String.Format("{0:X2}", BackColor.G));
                writer.WriteElementString("BackColorB", String.Format("{0:X2}", BackColor.B));
                writer.WriteElementString("OutlineColorR", String.Format("{0:X2}", OutlineColor.R));
                writer.WriteElementString("OutlineColorG", String.Format("{0:X2}", OutlineColor.G));
                writer.WriteElementString("OutlineColorB", String.Format("{0:X2}", OutlineColor.B));
                writer.WriteElementString("PageBackColorR", String.Format("{0:X2}", PageBackColor.R));
                writer.WriteElementString("PageBackColorG", String.Format("{0:X2}", PageBackColor.G));
                writer.WriteElementString("PageBackColorB", String.Format("{0:X2}", PageBackColor.B));
                writer.WriteEndElement();
            }

            public String GetName()
            {
                return "Layout";
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                InteriorWidth = Convert.ToInt32(reader.ReadElementString());
                InteriorHeight = Convert.ToInt32(reader.ReadElementString());
                BorderWidth = Convert.ToInt32(reader.ReadElementString());
                ResponseWidth = Convert.ToInt32(reader.ReadElementString());
                ResponseHeight = Convert.ToInt32(reader.ReadElementString());
                int r, g, b;
                r = Convert.ToInt32(reader.ReadElementString(), 16);
                g = Convert.ToInt32(reader.ReadElementString(), 16);
                b = Convert.ToInt32(reader.ReadElementString(), 16);
                BorderColor = System.Drawing.Color.FromArgb(r, g, b);
                r = Convert.ToInt32(reader.ReadElementString(), 16);
                g = Convert.ToInt32(reader.ReadElementString(), 16);
                b = Convert.ToInt32(reader.ReadElementString(), 16);
                BackColor = System.Drawing.Color.FromArgb(r, g, b);
                r = Convert.ToInt32(reader.ReadElementString(), 16);
                g = Convert.ToInt32(reader.ReadElementString(), 16);
                b = Convert.ToInt32(reader.ReadElementString(), 16);
                OutlineColor = System.Drawing.Color.FromArgb(r, g, b);
                r = Convert.ToInt32(reader.ReadElementString(), 16);
                g = Convert.ToInt32(reader.ReadElementString(), 16);
                b = Convert.ToInt32(reader.ReadElementString(), 16);
                PageBackColor = System.Drawing.Color.FromArgb(r, g, b);
                reader.ReadEndElement();
            }

            public XmlSchema GetSchema()
            {
                return null;
            }
        }

        public class IATSurvey : INamedXmlSerializable
        {
            private String _SurveyName;
            private int _NumItems;
            private int _AlternationSet;
            private int _NumOnceOnlyResponses;
            private String _FileNameBase;
            public enum EType { BeforeSurvey, AfterSurvey };
            private EType _Type;
            private List<CResponse.EResponseType> _ResponseTypes = new List<CResponse.EResponseType>();

            public EType SurveyType
            {
                get
                {
                    return _Type;
                }
            }

            public List<CResponse.EResponseType> ResponseTypes
            {
                get
                {
                    return _ResponseTypes;
                }
            }

            public String FileNameBase
            {
                get
                {
                    return _FileNameBase;
                }
            }

            public String SurveyName
            {
                get
                {
                    return _SurveyName;
                }
            }

            public int NumItems
            {
                get
                {
                    return _NumItems;
                }
            }


            public bool IsBeforeSurvey
            {
                get
                {
                    return (_Type == EType.BeforeSurvey);
                }
            }

            public bool IsAfterSurvey
            {
                get
                {
                    return (_Type == EType.AfterSurvey);
                }
            }

            public int AlternationSet
            {
                get
                {
                    return _AlternationSet;
                }
                set
                {
                    _AlternationSet = value;
                }
            }

            public int NumOnceOnlyResponses
            {
                get
                {
                    return _NumOnceOnlyResponses;
                }
            }

            protected IATSurvey()
            {
                _NumItems = 0;
                _AlternationSet = -1;
                _SurveyName = String.Empty;
                _FileNameBase = String.Empty;
                _NumOnceOnlyResponses = 0;
            }

            protected IATSurvey(EType type)
            {
                _NumItems = 0;
                _AlternationSet = -1;
                _SurveyName = String.Empty;
                _FileNameBase = String.Empty;
                _NumOnceOnlyResponses = 0;
                _Type = type;
            }

            public static IATSurvey GetIATSurvey(EType type)
            {
                return new IATSurvey(type);
            }

            public static IATSurvey GetIATSurvey(XmlReader xReader)
            {
                IATSurvey s = new IATSurvey();
                s.ReadXml(xReader);
                return s;
            }

            public IATSurvey(CSurvey s, int surveyNum, EType type)
            {
                _NumItems = 0;
                if (s.AlternationGroup != null)
                    _AlternationSet = s.AlternationGroup.AlternationPriority;
                else
                    _AlternationSet = -1;
                _SurveyName = s.Name;
                _FileNameBase = s.FileNameBase;
                for (int ctr = 0; ctr < s.Items.Count; ctr++)
                    if (!s.Items[ctr].IsCaption)
                    {
                        if (s.Items[ctr].Response.ResponseType != CResponse.EResponseType.Instruction)
                        {
                            _NumItems++;
                            _ResponseTypes.Add(s.Items[ctr].Response.ResponseType);
                        }
                        /*
                        if ((s.Items[ctr].Response.ResponseType == CResponse.EResponseType.FixedDig) &&
                            (s.Items[ctr].IsScored))
                        {
                            if (((CFixedDigResponse)s.Items[ctr].Response).IsOneUse)
                                AddUniqueIDResponse(surveyNum + ctr, NumItems, (CFixedDigResponse)s.Items[ctr].Response.DefinedResponse);
                        }
                        */
                    }
                _NumOnceOnlyResponses = s.NumOnceOnlyResponses;
                _Type = type;
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATSurvey");
                writer.WriteAttributeString("SurveyType", _Type.ToString());
                writer.WriteElementString("SurveyName", SurveyName);
                writer.WriteElementString("FileNameBase", FileNameBase);
                writer.WriteElementString("NumItems", NumItems.ToString());
                writer.WriteElementString("AlternationSet", AlternationSet.ToString());
                //            writer.WriteElementString("NumOnceOnlyResponses", NumOnceOnlyResponses.ToString());
                for (int ctr = 0; ctr < _ResponseTypes.Count; ctr++)
                    writer.WriteElementString("ResponseType", _ResponseTypes[ctr].ToString());
                writer.WriteEndElement();
            }

            public String GetName()
            {
                return "IATSurvey";
            }

            public void ReadXml(XmlReader reader)
            {
                _Type = (EType)Enum.Parse(typeof(EType), reader["SurveyType"]);
                reader.ReadStartElement();
                _SurveyName = reader.ReadElementString();
                _FileNameBase = reader.ReadElementString();
                _NumItems = Convert.ToInt32(reader.ReadElementString());
                _AlternationSet = Convert.ToInt32(reader.ReadElementString());
                _ResponseTypes.Clear();
                while (reader.Name == "ResponseType")
                    _ResponseTypes.Add((CResponse.EResponseType)Enum.Parse(typeof(CResponse.EResponseType), reader.ReadElementString()));
                reader.ReadEndElement();
            }

            public XmlSchema GetSchema()
            {
                return null;
            }
        }

        public class IATDisplayItem : INamedXmlSerializable
        {
            private int _ID, _X, _Y, _Width, _Height;
            private String _Filename;
            private DIBase _SourceDisplayItem;
            private String FilenameBase;

            public int ID
            {
                get
                {
                    return _ID;
                }
                set
                {
                    _ID = value;
                }
            }

            public int X
            {
                get
                {
                    return _X;
                }
                set
                {
                    _X = value;
                }
            }

            public int Y
            {
                get
                {
                    return _Y;
                }
                set
                {
                    _Y = value;
                }
            }

            public int Width
            {
                get
                {
                    return _Width;
                }
                set
                {
                    _Width = value;
                }
            }

            public int Height
            {
                get
                {
                    return _Height;
                }
                set
                {
                    _Height = value;
                }
            }

            public String Filename
            {
                get
                {
                    return _Filename;
                }
                set
                {
                    _Filename = value;
                }
            }

            public DIBase SourceDisplayItem
            {
                get
                {
                    return _SourceDisplayItem;
                }
            }

            public IATDisplayItem()
            {
                ID = -1;
                X = -1;
                Y = -1;
                Width = -1;
                Height = -1;
                Filename = String.Empty;
                _SourceDisplayItem = null;
                FilenameBase = String.Empty;
            }

            public IATDisplayItem(int id, DIBase sourceDisplayItem, Point ptOrigin, String filenameBase)
            {
                ID = id;
                _SourceDisplayItem = sourceDisplayItem;
                FilenameBase = filenameBase;
                Filename = String.Empty;
                X = ptOrigin.X;
                Y = ptOrigin.Y;
                Width = -1;
                Height = -1;
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATDisplayItem");
                writer.WriteElementString("ID", ID.ToString());
                writer.WriteElementString("Filename", Filename);
                writer.WriteElementString("X", X.ToString());
                writer.WriteElementString("Y", Y.ToString());
                writer.WriteElementString("Width", Width.ToString());
                writer.WriteElementString("Height", Height.ToString());
                writer.WriteEndElement();
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                ID = Convert.ToInt32(reader.ReadElementString());
                Filename = reader.ReadElementString();
                X = Convert.ToInt32(reader.ReadElementString());
                Y = Convert.ToInt32(reader.ReadElementString());
                Width = Convert.ToInt32(reader.ReadElementString());
                Height = Convert.ToInt32(reader.ReadElementString());
                reader.ReadEndElement();
            }

            public String GetName()
            {
                return "IATDisplayItem";
            }

            public XmlSchema GetSchema()
            {
                return null;
            }
        }

        public class DisplayItemList : INamedXmlSerializable
        {
            private List<IATDisplayItem> _DisplayItems;
            /*
            public List<IATDisplayItem> DisplayItems
            {
                get
                {
                    return _DisplayItems;
                }
            }
            */

            public int Count
            {
                get
                {
                    return _DisplayItems.Count;
                }
            }

            public void Add(IATDisplayItem di)
            {
                _DisplayItems.Add(di);
            }

            public void AddRange(List<IATDisplayItem> dis)
            {
                _DisplayItems.AddRange(dis);
            }

            public DisplayItemList()
            {
                _DisplayItems = new List<IATDisplayItem>();
            }



            public DisplayItemList(List<IATDisplayItem> list)
            {
                _DisplayItems = new List<IATDisplayItem>();
                _DisplayItems.AddRange(list);
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("DisplayItemList");
                writer.WriteAttributeString("NumDisplayItems", _DisplayItems.Count.ToString());
                foreach (IATDisplayItem di in _DisplayItems)
                    di.WriteXml(writer);
                writer.WriteEndElement();
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                _DisplayItems.Clear();
                int ndi = Convert.ToInt32(reader["NumDisplayItems"]);
                reader.ReadStartElement();
                for (int ctr = 0; ctr < ndi; ctr++)
                {
                    _DisplayItems.Add(new IATDisplayItem());
                    _DisplayItems[_DisplayItems.Count - 1].ReadXml(reader);
                }
                reader.ReadEndElement();
            }

            public String GetName()
            {
                return "DisplayItemList";
            }

            public IATDisplayItem this[int ndx]
            {
                get
                {
                    return _DisplayItems[ndx];
                }
            }
        }

        public abstract class IATEvent : INamedXmlSerializable
        {
            public enum EEventType { BeginIATBlock, EndIATBlock, IATItem, BeginInstructionBlock, TextInstructionScreen, MockItemInstructionScreen, KeyedInstructionScreen };

            private EEventType _EventType;

            public EEventType EventType
            {
                get
                {
                    return _EventType;
                }
                set
                {
                    _EventType = value;
                }
            }

            public IATEvent(EEventType type)
            {
                _EventType = type;
            }

            static public IATEvent CreateFromXml(XmlReader reader)
            {
                IATEvent e = null;
                EEventType eType = (EEventType)Enum.Parse(typeof(EEventType), reader["EventType"]);
                switch (eType)
                {
                    case EEventType.BeginIATBlock:
                        e = new BeginIATBlock();
                        break;

                    case EEventType.BeginInstructionBlock:
                        e = new BeginInstructionBlock();
                        break;

                    case EEventType.EndIATBlock:
                        e = new EndIATBlock();
                        break;

                    case EEventType.IATItem:
                        e = new IATItem();
                        break;

                    case EEventType.KeyedInstructionScreen:
                        e = new KeyedInstructionScreen();
                        break;

                    case EEventType.MockItemInstructionScreen:
                        e = new MockItemInstructionScreen();
                        break;

                    case EEventType.TextInstructionScreen:
                        e = new TextInstructionScreen();
                        break;
                }
                e.ReadXml(reader);
                return e;
            }

            public abstract void WriteXml(XmlWriter writer);
            public abstract void ReadXml(XmlReader reader);
            public String GetName()
            {
                return "IATEvent";
            }
        }

        abstract class IATInstructionScreen : IATEvent
        {
            private int _ContinueASCIIKeyCode, _ContinueInstructionsDisplayID;

            public int ContinueASCIIKeyCode
            {
                get
                {
                    return _ContinueASCIIKeyCode;
                }
                set
                {
                    _ContinueASCIIKeyCode = value;
                }
            }

            public int ContinueInstructionsDisplayID
            {
                get
                {
                    return _ContinueInstructionsDisplayID;
                }
                set
                {
                    _ContinueInstructionsDisplayID = value;
                }
            }

            public IATInstructionScreen(EEventType type)
                : base(type)
            {
                _ContinueASCIIKeyCode = 0;
                _ContinueInstructionsDisplayID = -1;
            }

            public IATInstructionScreen(EEventType type, int continueASCIIKeyCode, int continueInstructionsDisplayID)
                : base(type)
            {
                ContinueASCIIKeyCode = continueASCIIKeyCode;
                ContinueInstructionsDisplayID = continueInstructionsDisplayID;
            }

            protected IATInstructionScreen(EEventType type, char continueChar, int continueInstructionsDisplayItemID)
                : base(type)
            {
                ContinueASCIIKeyCode = Encoding.ASCII.GetBytes(continueChar.ToString())[0];
                ContinueInstructionsDisplayID = continueInstructionsDisplayItemID;
            }

        }

        class TextInstructionScreen : IATInstructionScreen
        {
            private int _InstructionsDisplayID;

            public int InstructionsDisplayID
            {
                get
                {
                    return _InstructionsDisplayID;
                }
                set
                {
                    _InstructionsDisplayID = value;
                }
            }

            public TextInstructionScreen()
                : base(EEventType.TextInstructionScreen)
            {
                _InstructionsDisplayID = -1;
            }

            public TextInstructionScreen(char continueChar, int continueInstructionsID, int instructionsID)
                : base(EEventType.TextInstructionScreen, continueChar, continueInstructionsID)
            {
                InstructionsDisplayID = instructionsID;
            }


            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATEvent");
                writer.WriteAttributeString("EventType", EventType.ToString());
                writer.WriteElementString("ContinueASCIIKeyCode", ContinueASCIIKeyCode.ToString());
                writer.WriteElementString("ContinueInstructionsDisplayID", ContinueInstructionsDisplayID.ToString());
                writer.WriteElementString("InstructionsDisplayID", InstructionsDisplayID.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                ContinueASCIIKeyCode = Convert.ToInt32(reader.ReadElementString("ContinueASCIIKeyCode"));
                ContinueInstructionsDisplayID = Convert.ToInt32(reader.ReadElementString("ContinueInstructionsDisplayID"));
                InstructionsDisplayID = Convert.ToInt32(reader.ReadElementString("InstructionsDisplayID"));
                reader.ReadEndElement();
            }
        }

        class MockItemInstructionScreen : IATInstructionScreen
        {
            private int _LeftResponseDisplayID, _RightResponseDisplayID;
            private int _StimulusDisplayID, _InstructionsDisplayID;
            private bool _ErrorMarkIsDisplayed, _OutlineLeftResponse, _OutlineRightResponse;

            public int LeftResponseDisplayID
            {
                get
                {
                    return _LeftResponseDisplayID;
                }
                set
                {
                    _LeftResponseDisplayID = value;
                }
            }

            public int RightResponseDisplayID
            {
                get
                {
                    return _RightResponseDisplayID;
                }
                set
                {
                    _RightResponseDisplayID = value;
                }
            }

            public int StimulusDisplayID
            {
                get
                {
                    return _StimulusDisplayID;
                }
                set
                {
                    _StimulusDisplayID = value;
                }
            }

            public int InstructionsDisplayID
            {
                get
                {
                    return _InstructionsDisplayID;
                }
                set
                {
                    _InstructionsDisplayID = value;
                }
            }

            public bool ErrorMarkIsDisplayed
            {
                get
                {
                    return _ErrorMarkIsDisplayed;
                }
                set
                {
                    _ErrorMarkIsDisplayed = value;
                }
            }

            public bool OutlineLeftResponse
            {
                get
                {
                    return _OutlineLeftResponse;
                }
                set
                {
                    _OutlineLeftResponse = value;
                }
            }

            public bool OutlineRightResponse
            {
                get
                {
                    return _OutlineRightResponse;
                }
                set
                {
                    _OutlineRightResponse = value;
                }
            }

            public MockItemInstructionScreen()
                : base(EEventType.MockItemInstructionScreen)
            {
                InstructionsDisplayID = -1;
                RightResponseDisplayID = -1;
                StimulusDisplayID = -1;
                LeftResponseDisplayID = -1;
                OutlineLeftResponse = false;
                OutlineRightResponse = false;
                ErrorMarkIsDisplayed = false;
            }

            public MockItemInstructionScreen(char continueChar, int continueInstructionsID, int leftResponseDisplayItemID, int rightResponseDisplayItemID,
                int stimulusDisplayItemID, int instructionsDisplayItemID, bool errorMarkIsDisplayed, bool outlineLeftResponse, bool outlineRightResponse)
                : base(EEventType.MockItemInstructionScreen, continueChar, continueInstructionsID)
            {
                LeftResponseDisplayID = leftResponseDisplayItemID;
                RightResponseDisplayID = rightResponseDisplayItemID;
                StimulusDisplayID = stimulusDisplayItemID;
                InstructionsDisplayID = instructionsDisplayItemID;
                ErrorMarkIsDisplayed = errorMarkIsDisplayed;
                OutlineLeftResponse = outlineLeftResponse;
                OutlineRightResponse = outlineRightResponse;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATEvent");
                writer.WriteAttributeString("EventType", EventType.ToString());
                writer.WriteElementString("ContinueASCIIKeyCode", ContinueASCIIKeyCode.ToString());
                writer.WriteElementString("ContinueInstructionsDisplayID", ContinueInstructionsDisplayID.ToString());
                writer.WriteElementString("LeftResponseDisplayID", LeftResponseDisplayID.ToString());
                writer.WriteElementString("RightResponseDisplayID", RightResponseDisplayID.ToString());
                writer.WriteElementString("StimulusDisplayID", StimulusDisplayID.ToString());
                writer.WriteElementString("InstructionsDisplayID", InstructionsDisplayID.ToString());
                writer.WriteElementString("ErrorMarkIsDisplayed", ErrorMarkIsDisplayed.ToString());
                writer.WriteElementString("OutlineLeftResponse", OutlineLeftResponse.ToString());
                writer.WriteElementString("OutlineRightResponse", OutlineRightResponse.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                ContinueASCIIKeyCode = Convert.ToInt32(reader.ReadElementString());
                ContinueInstructionsDisplayID = Convert.ToInt32(reader.ReadElementString());
                LeftResponseDisplayID = Convert.ToInt32(reader.ReadElementString());
                RightResponseDisplayID = Convert.ToInt32(reader.ReadElementString());
                StimulusDisplayID = Convert.ToInt32(reader.ReadElementString());
                InstructionsDisplayID = Convert.ToInt32(reader.ReadElementString());
                ErrorMarkIsDisplayed = Convert.ToBoolean(reader.ReadElementString());
                OutlineLeftResponse = Convert.ToBoolean(reader.ReadElementString());
                OutlineRightResponse = Convert.ToBoolean(reader.ReadElementString());
                reader.ReadEndElement();
            }
        }

        class KeyedInstructionScreen : IATInstructionScreen
        {
            private int _LeftResponseDisplayID, _RightResponseDisplayID, _InstructionsDisplayID;

            public int LeftResponseDisplayID
            {
                get
                {
                    return _LeftResponseDisplayID;
                }
                set
                {
                    _LeftResponseDisplayID = value;
                }
            }

            public int RightResponseDisplayID
            {
                get
                {
                    return _RightResponseDisplayID;
                }
                set
                {
                    _RightResponseDisplayID = value;
                }
            }

            public int InstructionsDisplayID
            {
                get
                {
                    return _InstructionsDisplayID;
                }
                set
                {
                    _InstructionsDisplayID = value;
                }
            }

            public KeyedInstructionScreen()
                : base(EEventType.KeyedInstructionScreen)
            {
                LeftResponseDisplayID = -1;
                RightResponseDisplayID = -1;
                InstructionsDisplayID = -1;
            }

            public KeyedInstructionScreen(char continueChar, int continueInstructionsID, int instructionsID, int rightResponseID, int leftResponseID)
                : base(EEventType.KeyedInstructionScreen, continueChar, continueInstructionsID)
            {
                InstructionsDisplayID = instructionsID;
                RightResponseDisplayID = rightResponseID;
                LeftResponseDisplayID = leftResponseID;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATEvent");
                writer.WriteAttributeString("EventType", EventType.ToString());
                writer.WriteElementString("ContinueASCIIKeyCode", ContinueASCIIKeyCode.ToString());
                writer.WriteElementString("ContinueInstructionsDisplayID", ContinueInstructionsDisplayID.ToString());
                writer.WriteElementString("LeftResponseDisplayID", LeftResponseDisplayID.ToString());
                writer.WriteElementString("RightResponseDisplayID", RightResponseDisplayID.ToString());
                writer.WriteElementString("InstructionsDisplayID", InstructionsDisplayID.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                ContinueASCIIKeyCode = Convert.ToInt32(reader.ReadElementString());
                ContinueInstructionsDisplayID = Convert.ToInt32(reader.ReadElementString());
                LeftResponseDisplayID = Convert.ToInt32(reader.ReadElementString());
                RightResponseDisplayID = Convert.ToInt32(reader.ReadElementString());
                InstructionsDisplayID = Convert.ToInt32(reader.ReadElementString());
                reader.ReadEndElement();
            }
        }

        class BeginIATBlock : IATEvent
        {
            private int _NumPresentations, _AlternatedWith;
            private int _BlockNum, _NumItems;
            private bool _PracticeBlock;
            private int _InstructionsDisplayID, _LeftResponseDisplayID, _RightResponseDisplayID;


            public int NumPresentations
            {
                get
                {
                    return _NumPresentations;
                }
                set
                {
                    _NumPresentations = value;
                }
            }

            public int AlternatedWith
            {
                get
                {
                    return _AlternatedWith;
                }
                set
                {
                    _AlternatedWith = value;
                }
            }

            public int BlockNum
            {
                get
                {
                    return _BlockNum;
                }
                set
                {
                    _BlockNum = value;
                }
            }

            public int NumItems
            {
                get
                {
                    return _NumItems;
                }
                set
                {
                    _NumItems = value;
                }
            }

            public bool PracticeBlock
            {
                get
                {
                    return _PracticeBlock;
                }
                set
                {
                    _PracticeBlock = value;
                }
            }

            public int InstructionsDisplayID
            {
                get
                {
                    return _InstructionsDisplayID;
                }
                set
                {
                    _InstructionsDisplayID = value;
                }
            }

            public int LeftResponseDisplayID
            {
                get
                {
                    return _LeftResponseDisplayID;
                }
                set
                {
                    _LeftResponseDisplayID = value;
                }
            }

            public int RightResponseDisplayID
            {
                get
                {
                    return _RightResponseDisplayID;
                }
                set
                {
                    _RightResponseDisplayID = value;
                }
            }

            public BeginIATBlock()
                : base(EEventType.BeginIATBlock)
            {
                PracticeBlock = false;
                NumItems = 0;
                BlockNum = -1;
                NumPresentations = 0;
                AlternatedWith = -1;
                LeftResponseDisplayID = -1;
                RightResponseDisplayID = -1;
                InstructionsDisplayID = -1;
            }

            public BeginIATBlock(int blockNum, int instructionsDisplayID, int leftResponseDisplayID, int rightResponseDisplayID)
                : base(EEventType.BeginIATBlock)
            {
                BlockNum = blockNum;
                InstructionsDisplayID = instructionsDisplayID;
                LeftResponseDisplayID = leftResponseDisplayID;
                RightResponseDisplayID = rightResponseDisplayID;
                PracticeBlock = false;
                NumItems = 0;
                NumPresentations = 0;
                AlternatedWith = -1;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATEvent");
                writer.WriteAttributeString("EventType", EventType.ToString());
                writer.WriteElementString("NumPresentations", NumPresentations.ToString());
                writer.WriteElementString("AlternatedWith", AlternatedWith.ToString());
                writer.WriteElementString("BlockNum", BlockNum.ToString());
                writer.WriteElementString("NumItems", NumItems.ToString());
                writer.WriteElementString("PracticeBlock", PracticeBlock.ToString());
                writer.WriteElementString("InstructionsDisplayID", InstructionsDisplayID.ToString());
                writer.WriteElementString("LeftResponseDisplayID", LeftResponseDisplayID.ToString());
                writer.WriteElementString("RightResponseDisplayID", RightResponseDisplayID.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                NumPresentations = Convert.ToInt32(reader.ReadElementString());
                AlternatedWith = Convert.ToInt32(reader.ReadElementString());
                BlockNum = Convert.ToInt32(reader.ReadElementString());
                NumItems = Convert.ToInt32(reader.ReadElementString());
                PracticeBlock = Convert.ToBoolean(reader.ReadElementString());
                InstructionsDisplayID = Convert.ToInt32(reader.ReadElementString());
                LeftResponseDisplayID = Convert.ToInt32(reader.ReadElementString());
                RightResponseDisplayID = Convert.ToInt32(reader.ReadElementString());
                reader.ReadEndElement();
            }
        }

        class EndIATBlock : IATEvent
        {
            public EndIATBlock()
                : base(EEventType.EndIATBlock)
            {
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATEvent");
                writer.WriteAttributeString("EventType", EventType.ToString());
                writer.WriteElementString("DummyValue", "abcdefg");
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                reader.ReadStartElement();
                reader.ReadElementString(); // read dummy value
                reader.ReadEndElement();
            }
        }

        class IATItem : IATEvent
        {
            public Task SlideSaveTask { get; set; } = null;
            public String SpecifierArg { get; set; } = String.Empty;
            public int SpecifierID { get; set; } = -1;
            public KeyedDirection KeyedDir { get; set; } = KeyedDirection.None;
            public int ItemNum { get; set; } = -1;
            public int BlockNum { get; set; } = -1;
            public int StimulusDisplayID { get; set; } = -1;
            public bool IsPracticeItem { get; set; } = false;
            public int OriginatingBlock { get; set; } = -1;
            public String ItemSlideFilename { get; set; } = null;
            private static int ItemCounter = 1;

            /// <summary>
            /// resets the auto-incremented item counter that assigns item numbers to non-pracitice IAT items to 1
            /// </summary>
            public static void ResetItemCounter()
            {
                ItemCounter = 1;
            }


            public IATItem()
                : base(EEventType.IATItem)
            {
            }
            /*
            public IATItem(int stimulusDisplayItemID, EKeyedDir keyedDir, int specifierID, String specifierArg, int originatingBlock)
                : base(EEventType.IATItem)
            {
                StimulusDisplayID = stimulusDisplayItemID;
                KeyedDir = keyedDir;
                ItemNum = ItemCounter++;
                BlockNum = -1;
                SpecifierID = specifierID;
                SpecifierArg = specifierArg;
                OriginatingBlock = originatingBlock;
            }
            */
            public IATItem(int stimulusDisplayItemID, KeyedDirection keyedDir, int specifierID, int blockNum, String specifierArg, int originatingBlock)
                : base(EEventType.IATItem)
            {
                StimulusDisplayID = stimulusDisplayItemID;
                KeyedDir = keyedDir;
                ItemNum = ItemCounter++;
                BlockNum = blockNum;
                SpecifierID = specifierID;
                SpecifierArg = specifierArg;
                OriginatingBlock = originatingBlock;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATEvent");
                writer.WriteAttributeString("EventType", EventType.ToString());
                writer.WriteElementString("ItemNum", ItemNum.ToString());
                writer.WriteElementString("BlockNum", BlockNum.ToString());
                writer.WriteElementString("IsPracticeItem", IsPracticeItem.ToString());
                writer.WriteElementString("StimulusDisplayID", StimulusDisplayID.ToString());
                writer.WriteElementString("OriginatingBlock", OriginatingBlock.ToString());
                writer.WriteElementString("KeyedDir", KeyedDir.ToString());
                writer.WriteElementString("SpecifierID", SpecifierID.ToString());
                writer.WriteElementString("SpecifierArg", SpecifierArg);
                writer.WriteElementString("ItemSlideFileName", ItemSlideFilename);
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                ItemNum = Convert.ToInt32(reader.ReadElementString());
                BlockNum = Convert.ToInt32(reader.ReadElementString());
                IsPracticeItem = Convert.ToBoolean(reader.ReadElementString());
                StimulusDisplayID = Convert.ToInt32(reader.ReadElementString());
                OriginatingBlock = Convert.ToInt32(reader.ReadElementString());
                KeyedDir = KeyedDirection.FromString(reader.ReadElementString());
                SpecifierID = Convert.ToInt32(reader.ReadElementString());
                SpecifierArg = reader.ReadElementString();
                ItemSlideFilename = reader.ReadElementString();
                reader.ReadEndElement();
            }
        }

        class BeginInstructionBlock : IATEvent
        {
            private int _NumInstructionScreens, _AlternatedWith;

            public int NumInstructionScreens
            {
                get
                {
                    return _NumInstructionScreens;
                }
                set
                {
                    _NumInstructionScreens = value;
                }
            }

            public int AlternatedWith
            {
                get
                {
                    return _AlternatedWith;
                }
                set
                {
                    _AlternatedWith = value;
                }
            }

            public BeginInstructionBlock()
                : base(EEventType.BeginInstructionBlock)
            {
                NumInstructionScreens = -1;
                AlternatedWith = -1;
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATEvent");
                writer.WriteAttributeString("EventType", EventType.ToString());
                writer.WriteElementString("NumInstructionScreens", NumInstructionScreens.ToString());
                writer.WriteElementString("AlternatedWith", AlternatedWith.ToString());
                writer.WriteEndElement();
            }

            public override void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                reader.ReadStartElement();
                NumInstructionScreens = Convert.ToInt32(reader.ReadElementString());
                AlternatedWith = Convert.ToInt32(reader.ReadElementString());
                reader.ReadEndElement();
            }
        }

        public class IATEventList : IXmlSerializable, IEnumerable
        {
            private List<IATEvent> _IATEvents;
            /*
            public List<IATEvent> IATEvents
            {
                get
                {
                    return _IATEvents;
                }
            }
            */


            public IEnumerator GetEnumerator()
            {
                return ((IEnumerable<IATEvent>)_IATEvents).GetEnumerator();
            }

            public int IndexOf(Object val)
            {
                return _IATEvents.IndexOf((IATEvent)val);
            }


            public void Add(IATEvent e)
            {
                _IATEvents.Add(e);
            }

            public int Count
            {
                get
                {
                    return _IATEvents.Count;
                }
            }

            public IATEvent this[int ndx]
            {
                get
                {
                    return _IATEvents[ndx];
                }
            }

            public IATEventList()
            {
                _IATEvents = new List<IATEvent>();
            }

            public IATEventList(List<IATEvent> list)
            {
                _IATEvents = new List<IATEvent>();
                _IATEvents.AddRange(list.ToArray());
            }


            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("IATEventList");
                writer.WriteAttributeString("NumEvents", _IATEvents.Count.ToString());
                for (int ctr = 0; ctr < _IATEvents.Count; ctr++)
                    _IATEvents[ctr].WriteXml(writer);
                writer.WriteEndElement();
            }

            public void ReadXml(XmlReader reader)
            {
                if (Convert.ToBoolean(reader["HasException"]))
                    throw new CXmlSerializationException(reader);
                int nEvents = Convert.ToInt32(reader["NumEvents"]);
                reader.ReadStartElement();
                _IATEvents.Clear();
                for (int ctr = 0; ctr < nEvents; ctr++)
                    _IATEvents.Add(IATEvent.CreateFromXml(reader));
                reader.ReadEndElement();
            }

            public XmlSchema GetSchema()
            {
                return null;
            }
        }

        public abstract class DynamicSpecifier
        {
            private int _ID;
            private String _SurveyName;
            private int _ItemNum;
            public enum ESpecifierType { Range, Selection, Mask, TrueFalse };
            protected ESpecifierType Type;

            public int ID
            {
                get
                {
                    return _ID;
                }
                set
                {
                    _ID = value;
                }
            }

            public String SurveyName
            {
                get
                {
                    return _SurveyName;
                }
                set
                {
                    _SurveyName = value;
                }
            }

            public int ItemNum
            {
                get
                {
                    return _ItemNum;
                }
                set
                {
                    _ItemNum = value;
                }
            }

            public DynamicSpecifier(ESpecifierType specifierType)
            {
                Type = specifierType;
                _ID = -1;
                _SurveyName = String.Empty;
                _ItemNum = 0;
            }

            public DynamicSpecifier(ESpecifierType specifierType, int id, String surveyName, int itemNum)
            {
                Type = specifierType;
                ID = id;
                SurveyName = surveyName;
                ItemNum = itemNum;
            }

            public static DynamicSpecifier CreateFromXml(XmlReader reader)
            {
                ESpecifierType type = (ESpecifierType)Enum.Parse(typeof(ESpecifierType), reader["SpecifierType"]);
                DynamicSpecifier specifier = null;
                switch (type)
                {
                    case ESpecifierType.Range:
                        specifier = new RangeSpecifier();
                        break;

                    case ESpecifierType.Mask:
                        specifier = new MaskSpecifier();
                        break;

                    case ESpecifierType.Selection:
                        specifier = new SelectionSpecifier();
                        break;

                    case ESpecifierType.TrueFalse:
                        specifier = new TrueFalseSpecifier();
                        break;
                }
                specifier.ReadXml(reader);
                return specifier;
            }

            public virtual void ReadXml(XmlReader reader)
            {
                ID = Convert.ToInt32(reader.ReadElementString());
                SurveyName = reader.ReadElementString();
                ItemNum = Convert.ToInt32(reader.ReadElementString());
            }

            public virtual void WriteXml(XmlWriter writer)
            {
                writer.WriteElementString("TestSpecifierID", ID.ToString());
                writer.WriteElementString("SurveyName", SurveyName);
                writer.WriteElementString("ItemNum", ItemNum.ToString());
            }
        }

        class RangeSpecifier : DynamicSpecifier
        {
            private int _Cutoff;
            private bool _IsReverseScored;
            private bool _CutoffExcludes;

            public bool CutoffExcludes
            {
                get
                {
                    return _CutoffExcludes;
                }
                set
                {
                    _CutoffExcludes = value;
                }
            }


            public bool IsReverseScored
            {
                get
                {
                    return _IsReverseScored;
                }
                set
                {
                    _IsReverseScored = value;
                }
            }

            public int Cutoff
            {
                get
                {
                    return _Cutoff;
                }
                set
                {
                    _Cutoff = value;
                }
            }

            public RangeSpecifier()
                : base(ESpecifierType.Range)
            {
                _Cutoff = -1;
                _IsReverseScored = false;
            }

            public RangeSpecifier(int id, String surveyName, int itemNum, int cutoff, bool cutoffExcludes, bool reverseScored)
                : base(ESpecifierType.Range, id, surveyName, itemNum)
            {
                _Cutoff = cutoff;
                _CutoffExcludes = cutoffExcludes;
                _IsReverseScored = reverseScored;
            }

            public override void ReadXml(XmlReader reader)
            {
                reader.ReadStartElement();
                base.ReadXml(reader);
                Cutoff = Convert.ToInt32(reader.ReadElementString());
                CutoffExcludes = Convert.ToBoolean(reader.ReadElementString());
                IsReverseScored = Convert.ToBoolean(reader.ReadElementString());
                reader.ReadEndElement();
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("DynamicSpecifier");
                writer.WriteAttributeString("SpecifierType", Type.ToString());
                writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi:type", "RangeSpecifier");
                base.WriteXml(writer);
                writer.WriteElementString("Cutoff", Cutoff.ToString());
                writer.WriteElementString("CutoffExcludes", CutoffExcludes.ToString());
                writer.WriteElementString("IsReverseScored", IsReverseScored.ToString());
                writer.WriteEndElement();
            }
        }

        class MaskSpecifier : DynamicSpecifier
        {
            private String _Mask;

            public String Mask
            {
                get
                {
                    return _Mask;
                }
                set
                {
                    _Mask = value;
                }
            }

            public MaskSpecifier()
                : base(ESpecifierType.Mask)
            {
                Mask = String.Empty;
            }

            public MaskSpecifier(int id, String surveyName, int itemNum, String mask)
                : base(ESpecifierType.Mask, id, surveyName, itemNum)
            {
                Mask = mask;
            }

            public override void ReadXml(XmlReader reader)
            {
                reader.ReadStartElement();
                base.ReadXml(reader);
                Mask = reader.ReadElementString();
                reader.ReadEndElement();
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("DynamicSpecifier");
                writer.WriteAttributeString("SpecifierType", Type.ToString());
                writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi:type", "MaskSpecifier");
                base.WriteXml(writer);
                writer.WriteElementString("Mask", Mask);
                writer.WriteEndElement();
            }
        }

        class SelectionSpecifier : DynamicSpecifier
        {
            private List<String> _ResponseVals = new List<String>();

            public List<String> ResponseVals
            {
                get
                {
                    return _ResponseVals;
                }
            }

            public SelectionSpecifier()
                : base(ESpecifierType.Selection)
            {
            }

            public SelectionSpecifier(int id, String surveyName, int itemNum, List<String> responseVals)
                : base(ESpecifierType.Selection, id, surveyName, itemNum)
            {
                ResponseVals.AddRange(responseVals);
            }

            public override void ReadXml(XmlReader reader)
            {
                reader.ReadStartElement();
                base.ReadXml(reader);
                int nKeys = Convert.ToInt32(reader["NumKeySpecifiers"]);
                ResponseVals.Clear();
                reader.ReadStartElement();
                for (int ctr = 0; ctr < nKeys; ctr++)
                    ResponseVals.Add(reader.ReadElementString());
                reader.ReadEndElement();
                reader.ReadEndElement();
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("DynamicSpecifier");
                writer.WriteAttributeString("SpecifierType", Type.ToString());
                writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi:type", "SelectionSpecifier");
                base.WriteXml(writer);
                writer.WriteStartElement("KeySpecifiers");
                writer.WriteAttributeString("NumKeySpecifiers", ResponseVals.Count.ToString());
                foreach (String key in ResponseVals)
                    writer.WriteElementString("KeySpecifier", key);
                writer.WriteEndElement();
                writer.WriteEndElement();
            }
        }

        class TrueFalseSpecifier : DynamicSpecifier
        {
            public TrueFalseSpecifier() : base(ESpecifierType.TrueFalse) { }

            public TrueFalseSpecifier(int id, String surveyName, int itemNum)
                : base(ESpecifierType.TrueFalse, id, surveyName, itemNum) { }

            public override void ReadXml(XmlReader reader)
            {
                reader.ReadStartElement();
                base.ReadXml(reader);
                reader.ReadEndElement();
            }

            public override void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement("DynamicSpecifier");
                writer.WriteAttributeString("SpecifierType", Type.ToString());
                writer.WriteAttributeString("xmlns:xsi", "http://www.w3.org/2001/XMLSchema-instance");
                writer.WriteAttributeString("xsi:type", "TrueFalseSpecifier");
                base.WriteXml(writer);
                writer.WriteEndElement();
            }
        }
        /*
        class UniqueResponseItem : INamedXmlSerializable
        {
            private bool _Additive = false;
            private String _SurveyName;
            private int _ItemNum;
            private List<String> _ValueList = new List<String>();

            public bool Additive
            {
                get
                {
                    return _Additive;
                }
            }

            public String SurveyName
            {
                get
                {
                    return _SurveyName;
                }
            }

            public int ItemNum
            {
                get
                {
                    return _ItemNum;
                }
            }

            public List<String> ValueList
            {
                get
                {
                    return _ValueList;
                }
            }

            public bool Exists
            {
                get
                {
                    return (_ItemNum != -1);
                }
            }

            public UniqueResponseItem()
            {
                _Additive = false;
                _SurveyName = String.Empty;
                _ItemNum = -1;
            }

            public UniqueResponseItem(String surveyName, int itemNum, bool additive)
            {
                _SurveyName = surveyName;
                _ItemNum = itemNum;
                _Additive = additive;
            }

            public String GetName()
            {
                return "UniqueResponseItem";
            }

            public void WriteXml(XmlWriter writer)
            {
                writer.WriteStartElement(GetName());
                writer.WriteAttributeString("Additive", Additive.ToString());
                writer.WriteElementString("SurveyName", SurveyName);
                writer.WriteElementString("ItemNum", ItemNum.ToString());
                writer.WriteStartElement("UniqueResponses");
                for (int ctr = 0; ctr < ValueList.Count; ctr++) {
                    writer.WriteStartElement("UniqueResponse");
                    writer.WriteElementString("Value", ValueList[ctr]);
                    writer.WriteElementString("Consumed", false.ToString());
                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
                writer.WriteEndElement();
            }

            public void ReadXml(XmlReader reader)
            {
                ValueList.Clear();
                reader.ReadStartElement();
                _Additive = Convert.ToBoolean(reader["Additive"]);
                _SurveyName = reader.ReadElementString();
                _ItemNum = Convert.ToInt32(reader.ReadElementString());
                if (!Exists)
                    reader.Read();
                else { 
                    reader.ReadStartElement();
                    while (reader.IsStartElement())
                    {
                        reader.ReadStartElement();
                        String val = reader.ReadElementString("Value");
                        bool consumed = Convert.ToBoolean(reader.ReadElementString("Comsumed"));
                        ValueList.Add(val);
                        reader.ReadEndElement();
                    }
                    reader.ReadEndElement();
                }
                reader.ReadEndElement();
            }
        }
        */
        public class ConfigFile : INamedXmlSerializable, IXmlSerializable
        {
            private String _ServerDomain = String.Empty, _ServerPath = String.Empty;
            private int _ServerPort = -1;
            private int _NumIATItems;
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
            private DisplayItemList _DisplayItems;
            private String _Name = String.Empty;
            private String _DataRetrievalPassword = "xxx";
            private List<DynamicSpecifier> _DynamicSpecifiers = new List<DynamicSpecifier>();
            private IATImageContainer _ItemSlides = new IATImageContainer();
            private IATImageContainer _DisplayItemImages = new IATImageContainer();
            private CIAT IAT;
            private bool _HasUniqueResponses;
            private long _UploadTimeMillis = -1;

            public int TotalPresentations
            {
                get
                {
                    int nPres = 0;
                    for (int ctr = 0; ctr < EventList.Count; ctr++)
                        if (EventList[ctr].EventType == IATEvent.EEventType.BeginIATBlock)
                            nPres += ((BeginIATBlock)EventList[ctr]).NumPresentations;
                    return nPres;
                }
            }

            public IATImageContainer ItemSlides
            {
                get
                {
                    return _ItemSlides;
                }
            }

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

            public int NumIATItems
            {
                get
                {
                    return _NumIATItems;
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

            public DisplayItemList DisplayItems
            {
                get
                {
                    return _DisplayItems;
                }
            }

            public IATImageContainer DisplayItemImages
            {
                get
                {
                    return _DisplayItemImages;
                }
            }

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

            private int FindIdenticalDisplayItem(DIBase di, Point ptOrigin)
            {
                for (int ctr = 0; ctr < _DisplayItems.Count; ctr++)
                    if (_DisplayItems[ctr].SourceDisplayItem.URI.Equals(di.URI))
                        return ctr;
                return -1;
            }


            private String SaveItemSlide(MemoryStream slideData, int ItemNum, String arg)
            {
                return ItemSlides.AddImage(ItemNum, slideData, System.Drawing.Imaging.ImageFormat.Jpeg);
            }

            private int ProcessIATItem(IATClient.CIATItem item, CIATBlock block)
            {
                IATDisplayItem IATDisplayItem;
                IATItem PackagedIATItem;
                int stimulusID;

                // grab the stimulus
                DIBase DisplayItem = item.Stimulus as DIBase;
                Point ptOrigin = CIAT.SaveFile.Layout.StimulusRectangle.Location;
                if (DisplayItem.Type == DIType.StimulusText)
                {
                    stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
                    if (stimulusID == -1)
                    {
                        stimulusID = _DisplayItems.Count;
                        IATDisplayItem = new IATDisplayItem(stimulusID, DisplayItem, ptOrigin, IAT.Name);
                        _DisplayItems.Add(IATDisplayItem);
                    }
                }
                else if (DisplayItem.Type == DIType.StimulusImage)
                {
                    stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
                    if (stimulusID == -1)
                    {
                        stimulusID = _DisplayItems.Count;
                        IATDisplayItem = new IATDisplayItem(stimulusID, DisplayItem, ptOrigin, IAT.Name);
                        _DisplayItems.Add(IATDisplayItem);
                    }
                }
                else
                {
                    throw new InvalidDataException("Unknown stimulus type");
                }
                PackagedIATItem = new IATItem(stimulusID, item.GetKeyedDirection(block.URI), item.KeySpecifierID, block.IndexInContainer, item.SpecifierArg, item.OriginatingBlock + 1);
                _EventList.Add(PackagedIATItem);
                PackagedIATItem.SlideSaveTask = Task.Run(() =>
                {
                    using (var memStream = item.GetPreview(block.URI).SaveToJpeg())
                        PackagedIATItem.ItemSlideFilename = SaveItemSlide(memStream, PackagedIATItem.ItemNum, String.Empty);
                });
                return PackagedIATItem.ItemNum;
            }

            private bool ProcessIATBlock(CIATBlock Block, bool IsPracticeBlock, int blockNum)
            {
                IATDisplayItem IATDisplayItem;
                DIBase DisplayItem;
                IATEvent IATEvent;
                Point ptOrigin = new Point();
                int leftKeyID, rightKeyID, instructionsID;

                // grab the left response key value
                DisplayItem = Block.Key.LeftValue;
                ptOrigin.X = CIAT.SaveFile.Layout.LeftKeyValueRectangle.Left;
                ptOrigin.Y = CIAT.SaveFile.Layout.LeftKeyValueRectangle.Top;
                leftKeyID = _DisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(leftKeyID, DisplayItem, ptOrigin, IAT.Name);
                _DisplayItems.Add(IATDisplayItem);

                // grab the right response key value
                DisplayItem = Block.Key.RightValue;
                ptOrigin.X = CIAT.SaveFile.Layout.RightKeyValueRectangle.Left;
                ptOrigin.Y = CIAT.SaveFile.Layout.RightKeyValueRectangle.Top;
                rightKeyID = _DisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(rightKeyID, DisplayItem, ptOrigin, IAT.Name);
                _DisplayItems.Add(IATDisplayItem);

                // grab the instructions display item
                DisplayItem = CIAT.SaveFile.GetDI(Block.InstructionsUri);
                ptOrigin = CIAT.SaveFile.Layout.InstructionsRectangle.Location;
                instructionsID = _DisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(instructionsID, DisplayItem, ptOrigin, IAT.Name);
                _DisplayItems.Add(IATDisplayItem);

                // generate the start block event
                IATEvent = new BeginIATBlock(blockNum + 1, instructionsID, leftKeyID, rightKeyID);
                ((BeginIATBlock)IATEvent).NumPresentations = Block.NumPresentations;
                ((BeginIATBlock)IATEvent).NumItems = Block.NumItems;
                if (Block.AlternateBlock == null)
                    ((BeginIATBlock)IATEvent).AlternatedWith = -1;
                else
                    ((BeginIATBlock)IATEvent).AlternatedWith = Block.AlternateBlock.IndexInContainer + 1;
                _EventList.Add(IATEvent);

                // process the items
                for (int ctr = 0; ctr < Block.NumItems; ctr++)
                    ProcessIATItem(Block[ctr], Block);

                // generate the end block event
                IATEvent = new EndIATBlock();
                _EventList.Add(IATEvent);
                return true;
            }

            private void ProcessTextInstructionScreen(IATClient.CTextInstructionScreen screen)
            {
                // grab the instructions image
                Point ptOrigin = CIAT.SaveFile.Layout.InstructionScreenTextAreaRectangle.Location;
                int instructionsID = _DisplayItems.Count;
                IATDisplayItem pdi = new IATDisplayItem(instructionsID, CIAT.SaveFile.GetDI(screen.InstructionsUri), ptOrigin, IAT.Name);
                _DisplayItems.Add(pdi);

                // grab the continue instructions image
                DIBase di = CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri);
                ptOrigin = CIAT.SaveFile.Layout.ContinueInstructionsRectangle.Location;
                int continueInstructionsID = _DisplayItems.Count;
                pdi = new IATDisplayItem(continueInstructionsID, di, ptOrigin, IAT.Name);
                _DisplayItems.Add(pdi);

                // add the instruction screen event
                TextInstructionScreen InstrScr = new TextInstructionScreen(screen.ContinueKeyChar, continueInstructionsID, instructionsID);
                _EventList.Add(InstrScr);
            }

            private void ProcessMockItemInstructionScreen(IATClient.CMockItemScreen screen)
            {
                IATDisplayItem IATDisplayItem;
                DIBase DisplayItem;
                Point ptOrigin = new Point();
                int leftKeyID, rightKeyID, instructionsID;
                bool outlineLeftResponse, outlineRightResponse;

                // grab the left response key value
                DisplayItem = CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).LeftValue;
                ptOrigin.X = CIAT.SaveFile.Layout.LeftKeyValueRectangle.Left;
                ptOrigin.Y = CIAT.SaveFile.Layout.LeftKeyValueRectangle.Top;
                leftKeyID = _DisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(leftKeyID, DisplayItem, ptOrigin, IAT.Name);
                _DisplayItems.Add(IATDisplayItem);

                // grab the right response key value
                DisplayItem = CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).RightValue;
                ptOrigin.X = CIAT.SaveFile.Layout.RightKeyValueRectangle.Left;
                ptOrigin.Y = CIAT.SaveFile.Layout.RightKeyValueRectangle.Top;
                rightKeyID = _DisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(rightKeyID, DisplayItem, ptOrigin, IAT.Name);
                _DisplayItems.Add(IATDisplayItem);

                // grab the stimulus
                int stimulusID;
                DisplayItem = CIAT.SaveFile.GetDI(screen.StimulusUri);
                ptOrigin = CIAT.SaveFile.Layout.StimulusRectangle.Location;
                if (DisplayItem.Type == DIType.StimulusText)
                {
                    stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
                    if (stimulusID == -1)
                    {
                        stimulusID = _DisplayItems.Count;
                        IATDisplayItem = new IATDisplayItem(stimulusID, DisplayItem, ptOrigin, IAT.Name);
                        _DisplayItems.Add(IATDisplayItem);
                    }
                }
                else if (DisplayItem.Type == DIType.StimulusImage)
                {
                    Size idiSize = DisplayItem.ItemSize;
                    stimulusID = FindIdenticalDisplayItem(DisplayItem, ptOrigin);
                    if (stimulusID == -1)
                    {
                        stimulusID = _DisplayItems.Count;
                        IATDisplayItem = new IATDisplayItem(stimulusID, DisplayItem, ptOrigin, IAT.Name);
                        _DisplayItems.Add(IATDisplayItem);
                    }
                }
                else
                    stimulusID = int.MinValue;

                // grab the instructions display item
                DisplayItem = CIAT.SaveFile.GetDI(screen.InstructionsUri);
                ptOrigin = CIAT.SaveFile.Layout.MockItemInstructionsRectangle.Location;
                instructionsID = _DisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(instructionsID, DisplayItem, ptOrigin, IAT.Name);
                _DisplayItems.Add(IATDisplayItem);

                // grab the continue instructions display item
                DisplayItem = CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri);
                ptOrigin = CIAT.SaveFile.Layout.ContinueInstructionsRectangle.Location;
                int continueInstructionsID = _DisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(continueInstructionsID, DisplayItem, ptOrigin, IAT.Name);
                _DisplayItems.Add(IATDisplayItem);

                // determine if responses are to be outlined
                outlineLeftResponse = outlineRightResponse = false;
                if (screen.KeyedDirOutlined)
                {
                    if (screen.KeyedDirection == KeyedDirection.Left)
                        outlineLeftResponse = true;
                    else if (screen.KeyedDirection == KeyedDirection.Right)
                        outlineRightResponse = true;
                }

                // create the mock item screeen event
                MockItemInstructionScreen InstrScr = new MockItemInstructionScreen(screen.ContinueKeyChar, continueInstructionsID, leftKeyID, rightKeyID, stimulusID,
                    instructionsID, screen.InvalidResponseFlag, outlineLeftResponse, outlineRightResponse);
                _EventList.Add(InstrScr);
            }

            private void ProcessKeyedInstructionScreen(IATClient.CKeyInstructionScreen screen)
            {
                // grab the instructions image
                Point ptOrigin = CIAT.SaveFile.Layout.KeyInstructionScreenTextAreaRectangle.Location;
                int instructionsID = _DisplayItems.Count;
                IATDisplayItem pdi = new IATDisplayItem(instructionsID, CIAT.SaveFile.GetDI(screen.InstructionsUri), ptOrigin, IAT.Name);
                _DisplayItems.Add(pdi);

                DIBase DisplayItem;
                IATDisplayItem IATDisplayItem;
                int leftKeyID, rightKeyID;

                // grab the left response key value
                DisplayItem = CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).LeftValue;
                ptOrigin.X = CIAT.SaveFile.Layout.LeftKeyValueRectangle.Left;
                ptOrigin.Y = CIAT.SaveFile.Layout.LeftKeyValueRectangle.Top;
                leftKeyID = _DisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(leftKeyID, DisplayItem, ptOrigin, IAT.Name);
                _DisplayItems.Add(IATDisplayItem);

                // grab the right response key value
                DisplayItem = CIAT.SaveFile.GetIATKey(screen.ResponseKeyUri).RightValue;
                ptOrigin.X = CIAT.SaveFile.Layout.RightKeyValueRectangle.Left;
                ptOrigin.Y = CIAT.SaveFile.Layout.RightKeyValueRectangle.Top;
                rightKeyID = _DisplayItems.Count;
                IATDisplayItem = new IATDisplayItem(rightKeyID, DisplayItem, ptOrigin, IAT.Name);
                _DisplayItems.Add(IATDisplayItem);

                // grab the continue instructions image
                DisplayItem = CIAT.SaveFile.GetDI(screen.ContinueInstructionsUri);
                ptOrigin = CIAT.SaveFile.Layout.ContinueInstructionsRectangle.Location;
                int continueInstructionsID = _DisplayItems.Count;
                pdi = new IATDisplayItem(continueInstructionsID, DisplayItem, ptOrigin, IAT.Name);
                _DisplayItems.Add(pdi);

                // add the instruction screen event
                KeyedInstructionScreen InstrScr = new KeyedInstructionScreen(screen.ContinueKeyChar, continueInstructionsID, instructionsID, rightKeyID, leftKeyID);
                _EventList.Add(InstrScr);
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
                IATItem.ResetItemCounter();
                _BeforeSurveys = new List<IATSurvey>();
                _AfterSurveys = new List<IATSurvey>();
                _DisplayItems = new DisplayItemList();
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
                IATItem.ResetItemCounter();
                IAT = iat;
                _Name = IAT.Name;
                _BeforeSurveys = new List<IATSurvey>();
                for (int ctr = 0; ctr < IAT.BeforeSurvey.Count; ctr++)
                    _BeforeSurveys.Add(new IATSurvey(IAT.BeforeSurvey[ctr], 0, IATSurvey.EType.BeforeSurvey));
                _AfterSurveys = new List<IATSurvey>();
                for (int ctr = 0; ctr < IAT.AfterSurvey.Count; ctr++)
                    _AfterSurveys.Add(new IATSurvey(IAT.AfterSurvey[ctr], IAT.BeforeSurvey.Count + 1 + ctr, IATSurvey.EType.AfterSurvey));
                _DisplayItems = new DisplayItemList();
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
                _NumIATItems = 0;
                _RedirectOnComplete = IAT.RedirectionURL;
                _LeftResponseASCIIKeyCodeLower = System.Text.Encoding.ASCII.GetBytes(IAT.LeftResponseChar.ToLower())[0];
                _LeftResponseASCIIKeyCodeUpper = System.Text.Encoding.ASCII.GetBytes(IAT.LeftResponseChar.ToUpper())[0];
                _RightResponseASCIIKeyCodeLower = System.Text.Encoding.ASCII.GetBytes(IAT.RightResponseChar.ToLower())[0];
                _RightResponseASCIIKeyCodeUpper = System.Text.Encoding.ASCII.GetBytes(IAT.RightResponseChar.ToUpper())[0];
                CIAT.ImageManager.CachePeriod = 600000;
                for (int ctr = 0; ctr < IAT.Contents.Count; ctr++)
                {
                    if (IAT.Contents[ctr].Type == ContentsItemType.IATBlock)
                    {
                        CIATBlock block = (CIATBlock)IAT.Contents[ctr];
                        _NumIATItems += block.NumItems;
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
                CIAT.ImageManager.CachePeriod = Images.ImageManager.DefaultCachePeriod;
                _ErrorMarkID = _DisplayItems.Count;
                _DisplayItems.Add(new IATDisplayItem(_ErrorMarkID, CIAT.SaveFile.Layout.ErrorMark, CIAT.SaveFile.Layout.ErrorRectangle.Location, IAT.Name));


                Pen outlinePen = new Pen(CIAT.SaveFile.Layout.OutlineColor, CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1);
                _LeftKeyOutlineID = _DisplayItems.Count;
                _DisplayItems.Add(new IATDisplayItem(_LeftKeyOutlineID, CIAT.SaveFile.Layout.LeftKeyValueOutline, CIAT.SaveFile.Layout.LeftKeyValueRectangle.Location, IAT.Name));

                _RightKeyOutlineID = _DisplayItems.Count;
                _DisplayItems.Add(new IATDisplayItem(_RightKeyOutlineID, CIAT.SaveFile.Layout.RightKeyValueOutline, CIAT.SaveFile.Layout.RightKeyValueRectangle.Location, IAT.Name));
                outlinePen.Dispose();
                for (int ctr = 0; ctr < _DisplayItems.Count; ctr++)
                    DisplayItems[ctr].Filename = DisplayItemImages.AddImage(DisplayItems[ctr]);
                CDynamicSpecifier.CompactSpecifierDictionary(IAT);
                foreach (CDynamicSpecifier ds in CDynamicSpecifier.GetAllSpecifiers())
                    DynamicSpecifiers.Add(ds.GetSerializableSpecifier());
            }

            public int CountIATItems()
            {
                if (EventList == null)
                    return 0;
                int nItems = 0;
                for (int ctr = 0; ctr < EventList.Count; ctr++)
                    if (EventList[ctr].EventType == IATEvent.EEventType.IATItem)
                        nItems++;
                return nItems;
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
                DisplayItems.WriteXml(writer);
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
                _NumIATItems = Convert.ToInt32(reader.ReadElementString());
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
                DisplayItems.ReadXml(reader);
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
}