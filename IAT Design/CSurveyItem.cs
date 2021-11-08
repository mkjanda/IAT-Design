using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Windows.Forms;
using System.IO;
using System.Drawing;
using System.Xml.Linq;

namespace IATClient
{
    public class SurveyItemType : Enumeration {
        public static readonly SurveyItemType Item = new SurveyItemType(1, "Item", typeof(CSurveyItem));
        public static readonly SurveyItemType Caption = new SurveyItemType(2, "Caption", typeof(CSurveyCaption));
        public static readonly SurveyItemType SurveyImage = new SurveyItemType(3, "Image", typeof(CSurveyItemImage));

        private static readonly IEnumerable<SurveyItemType> All = new SurveyItemType[] { Item, Caption, SurveyImage };
        private Type Type { get; set; }
        private SurveyItemType(int val, String name, Type type) : base(val, name)
        {
            Type = type;
        }

        static public SurveyItemType FromTypeName(String tName)
        {
            try
            {
                return All.Where(sit => sit.Type.ToString() == tName).First();
            }
            catch (InvalidOperationException ex)
            {
                return null;
            }
        }

        static public SurveyItemType FromName(String name)
        {
            try
            {
                return All.Where(sit => sit.Name == name).First();
            }
            catch (InvalidOperationException)
            {
                return null;
            }
        }
    }

    public class CSurveyItem : IPackagePart
    {
        public virtual SurveyItemType ItemType { get { return SurveyItemType.Item; } }
        protected static Padding PreviewPanelPadding = new Padding(10);
        protected static Padding ResponsePreviewPadding = new Padding(25);
        protected const float PreviewQuestionFontSize = 11F * .975F;
        protected const float PreviewChoiceFontSize = 10F * .975F;
        protected const string PreviewFontFamily = "Arial";
        protected const String sSurveyItem = "SurveyItem";
        protected const String sCaption = "Caption";
        protected bool IsDisposed { get; set; } = false;
        public Type BaseType { get { return typeof(CSurveyItem); } }
        public virtual String MimeType {get { return "text/xml+" + typeof(CSurveyItem).ToString(); } }
        public static String sMimeType { get { return "text/xml+" + typeof(CSurveyItem).ToString(); } }
        protected CResponseObject _DefinedResponse = null;
        protected CResponseObject DummyResponse = null;
        public SurveyItemFormat Format { get; set; }
        public String Text { get; set; } = String.Empty;
        public Uri URI { get; set; } = null;
        public int ItemIndexInSaveFile { get; private set; }
        public String rSurveyId { get; set; } = null;
        private Uri _ParentSurveyUri = null;



        public virtual Uri ParentSurveyUri
        {
            get
            {
                if (rSurveyId == null)
                    return null;
                return CIAT.SaveFile.GetRelationship(this, rSurveyId).TargetUri;
            }
        }

        public CResponseObject DefinedResponse
        {
            get
            {
                return _DefinedResponse;
            }
        }

        public virtual bool IsCaption
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsImage
        {
            get
            {
                return false;
            }
        }

        public virtual bool IsQuestion
        {
            get
            {
                return true;
            }
        }

        private bool _Optional = false;
        public bool Optional
        {
            get
            {
                return _Optional;
            }
            set
            {
                _Optional = value;
            }
        }

        // the response definition
        private CResponse _Response;

        /// <summary>
        /// gets or sets the response definition
        /// </summary>
        public CResponse Response
        {
            get
            {
                return _Response;
            }
            set
            {
                _Response = value;
            }
        }

        public CSurveyItem()
        {
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            Text = String.Empty;
            _Response = null;
            Format = new SurveyItemFormat(SurveyItemFormat.EFor.Item);
            CIAT.SaveFile.Register(this);
        }

   
        public CSurveyItem(Uri u)
        {
            URI = u;
            Load();
            CIAT.SaveFile.Register(this);
        }
        protected CSurveyItem(CSurveyItem s)
        {
            URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            CIAT.SaveFile.Register(this);
            if (s.Response.ResponseType == CResponse.EResponseType.Instruction)
                Text = (s.Text != String.Empty) ? s.Text : Properties.Resources.sDefaultInstructionText;
            else
                Text = (s.Text != String.Empty) ? s.Text : Properties.Resources.sDefaultQuestionText;
            _Response = s.Response.Clone() as CResponse;
            Format = s.Format.Clone() as SurveyItemFormat;
        }

        /// <summary>
        /// Tests to see if the survey item contains question text and a valid response definition
        /// </summary>
        /// <returns>"true" if the object is valid, otherwise "false"</returns>
        public virtual bool IsValid()
        {
            if (Text == String.Empty)
                return false;
            return Response.IsValid();
        }
/*
        /// <summary>
        /// Writes the object to an XmlTextWriter
        /// </summary>
        /// <param name="writer">The XmlTextWriter object to write data to</param>
        public virtual void WriteToXml(XmlTextWriter writer)
        {
            // write the start of a "SurveyItem" element to signify a survey item definition
            writer.WriteStartElement(sSurveyItem);
            writer.WriteAttributeString("Optional", Optional.ToString());


            // write the question text and the response
            writer.WriteElementString("Text", Text);
            Response.WriteToXml(writer);
            Format.WriteToXml(writer);

            // write the close of the "SurveyItem" element
            writer.WriteEndElement();
        }
*/
        public virtual void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement(BaseType.Name, AsXElement()));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        public virtual XElement AsXElement()
        {
            XElement elem = new XElement(BaseType.ToString(), new XAttribute("ItemIndex", GetItemIndex().ToString()), new XAttribute("Optional", Optional.ToString()), 
                new XAttribute("ResponseType", Response.ResponseType.ToString()), Response.AsXElement(), new XElement("Question", Text), Format.AsXElement());
            if (rSurveyId != null)
                elem.Add(new XAttribute("rSurveyId", rSurveyId));
            return elem;
        }

        protected virtual void Load()
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            Load(xDoc.Document.Root.Descendants().Where(xe => xe.Name == BaseType.ToString()).FirstOrDefault());
        }

        public virtual void Load(XElement elem)
        {
            Optional = Convert.ToBoolean(elem.Attribute("Optional").Value);
            CResponse.EResponseType rType = (CResponse.EResponseType)Enum.Parse(typeof(CResponse.EResponseType), elem.Attribute("ResponseType").Value);
            if (CVersion.Compare(CIAT.SaveFile.Version, new CVersion("1.1.0.14")) < 0)
                ItemIndexInSaveFile = Convert.ToInt32(elem.Attribute("ItemIndex").Value);
            else
                ItemIndexInSaveFile = -1;
            switch (rType)
            {
                case CResponse.EResponseType.Boolean: Response = new CBoolResponse(); break;
                case CResponse.EResponseType.BoundedLength: Response = new CBoundedLengthResponse(); break;
                case CResponse.EResponseType.BoundedNum: Response = new CBoundedNumResponse(); break;
                case CResponse.EResponseType.Date: Response = new CDateResponse(); break;
                case CResponse.EResponseType.FixedDig: Response = new CFixedDigResponse(); break;
                case CResponse.EResponseType.Instruction: Response = new CInstruction(); break;
                case CResponse.EResponseType.Likert: Response = new CLikertResponse(); break;
                case CResponse.EResponseType.MultiBoolean: Response = new CMultiBooleanResponse(); break;
                case CResponse.EResponseType.Multiple: Response = new CMultipleResponse(); break;
                case CResponse.EResponseType.RegEx: Response = new CRegExResponse(); break;
                case CResponse.EResponseType.WeightedMultiple: Response = new CWeightedMultipleResponse(); break;
            }
            if (elem.Attribute("rSurveyId") != null)
            {
                rSurveyId = elem.Attribute("rSurveyId").Value;
                _ParentSurveyUri = CIAT.SaveFile.GetRelationship(this, rSurveyId).TargetUri;
            }
            Response.Load(elem.Element("Response"));
            Text = elem.Element("Question").Value;
            Format = new SurveyItemFormat(SurveyItemFormat.EFor.Item);
            Format.Load(elem.Element("Format"));
        }

        public virtual void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(sSurveyItem);
            writer.WriteAttributeString("Optional", Optional.ToString());
            writer.WriteElementString("Text", Text);
            Response.WriteXml(writer);
            Format.WriteXml(writer);
            writer.WriteEndElement();
        }
/*
        /// <summary>
        /// Creates a CSurveyItem object from the data in an XmlNode
        /// </summary>
        /// <param name="node">The XmlNode to load the data from</param>
        /// <returns>A new CSurveyItem object with the data contained in the provided node.  Returns "null" on failure</returns>
        static public CSurveyItem CreateFromXml(XmlNode node, CSurvey parentSurvey)
        {
            // check for a caption
            if (node.Name == sCaption)
            {
                CSurveyCaption sc = new CSurveyCaption(parentSurvey);
                sc.LoadFromXml(node);
                return sc;
            }

            // ensure the node defines a survey item
            if (node.Name != sSurveyItem)
                return null;
            bool Optional;
            if (node.Attributes["Optional"] == null)
                Optional = false;
            else
                Optional = Convert.ToBoolean(node.Attributes["Optional"].Value);
            // ensure the appropriate child node count

            // load the question text
            String text = node.FirstChild.InnerText;

            // load the response definition
            CResponse response = CResponse.CreateFromXml(node.SelectSingleNode("./Response"));
            if (response == null)  // return null if the response failed to load
                return null;
            SurveyItemFormat format = new SurveyItemFormat(SurveyItemFormat.EFor.Item);
            if (CVersion.Compare(CIAT.SaveFileVersion, new CVersion("1.0.1.1")) < 0)
                format.LoadFromXml(node.SelectSingleNode("./SurveyItemFormat"));

            // create the survey item and assign it the question text and response definition
            CSurveyItem item = new CSurveyItem(parentSurvey);
            item.Text = text;
            item.Response = response;
            item.Optional = Optional;
            item.Format = format;
            // return the survey item
            return item;
        }

        public virtual bool LoadFromXml(XmlNode node)
        {
            throw new NotImplementedException();
        }
*/
        protected bool _IsScored = false;
        
        public bool IsScored
        {
            get
            {
                return _IsScored;
            }
        }

        public virtual CResponseObject MakeScored()
        {
            if (IsScored)
                return DefinedResponse;

            _IsScored = true;

            switch (Response.ResponseType)
            {
                case CResponse.EResponseType.Boolean:
                    _DefinedResponse = new CBoolResponseObject(CResponseObject.EType.correct, this);
                    break;

                case CResponse.EResponseType.BoundedLength:
                    _DefinedResponse = new CBoundedLengthResponseObject(CResponseObject.EType.correct, this);
                    break;

                case CResponse.EResponseType.BoundedNum:
                    _DefinedResponse = new CBoundedNumResponseObject(CResponseObject.EType.correct, this);
                    break;

                case CResponse.EResponseType.Date:
                    _DefinedResponse = new CDateResponseObject(CResponseObject.EType.correct, this);
                    break;

                case CResponse.EResponseType.FixedDig:
                    _DefinedResponse = new CFixedDigResponseObject(CResponseObject.EType.correct, this);
                    break;

                case CResponse.EResponseType.Instruction:
                    _DefinedResponse = null;
                    break;

                case CResponse.EResponseType.Likert:
                    _DefinedResponse = new CLikertResponseObject(CResponseObject.EType.correct, this);
                    break;

                case CResponse.EResponseType.MultiBoolean:
                    _DefinedResponse = new CMultiBooleanResponseObject(CResponseObject.EType.correct, this);
                    break;

                case CResponse.EResponseType.Multiple:
                    _DefinedResponse = new CMultipleResponseObject(CResponseObject.EType.correct, this);
                    break;

                case CResponse.EResponseType.RegEx:
                    _DefinedResponse = new CRegExResponseObject(CRegExResponseObject.EType.correct, this);
                    break;

                case CResponse.EResponseType.WeightedMultiple:
                    _DefinedResponse = new CWeightedMultipleResponseObject(CResponseObject.EType.correct, this);
                    break;
            }
            return DefinedResponse;
        }

        protected void CreateDummyResponse()
        {
            DummyResponse = null;
            switch (Response.ResponseType)
            {
                case CResponse.EResponseType.Boolean:
                    DummyResponse = new CBoolResponseObject(CResponseObject.EType.dummy, this);
                    break;

                case CResponse.EResponseType.BoundedLength:
                    DummyResponse = new CBoundedLengthResponseObject(CResponseObject.EType.dummy, this);
                    break;

                case CResponse.EResponseType.BoundedNum:
                    DummyResponse = new CBoundedNumResponseObject(CResponseObject.EType.dummy, this);
                    break;

                case CResponse.EResponseType.Date:
                    DummyResponse = new CDateResponseObject(CResponseObject.EType.dummy, this);
                    break;

                case CResponse.EResponseType.FixedDig:
                    DummyResponse = new CFixedDigResponseObject(CResponseObject.EType.dummy, this);
                    break;

                case CResponse.EResponseType.Instruction:
                    break;

                case CResponse.EResponseType.Likert:
                    DummyResponse = new CLikertResponseObject(CResponseObject.EType.dummy, this);
                    break;

                case CResponse.EResponseType.MultiBoolean:
                    DummyResponse = new CMultiBooleanResponseObject(CResponseObject.EType.dummy, this);
                    break;

                case CResponse.EResponseType.Multiple:
                    DummyResponse = new CMultipleResponseObject(CResponseObject.EType.dummy, this);
                    break;

                case CResponse.EResponseType.RegEx:
                    DummyResponse = new CRegExResponseObject(CResponseObject.EType.dummy, this);
                    break;

                case CResponse.EResponseType.WeightedMultiple:
                    DummyResponse = new CWeightedMultipleResponseObject(CResponseObject.EType.dummy, this);
                    break;
            }
        }

        public String GetSurveyName()
        {
            if (ParentSurveyUri == null)
                return null;
            return CIAT.SaveFile.GetSurvey(ParentSurveyUri).Name;
        }

        public int GetItemIndex()
        {
            if (ParentSurveyUri == null)
                return -1;
            return CIAT.SaveFile.GetSurvey(ParentSurveyUri).Items.IndexOf(this);
        }

        public String GetItemDesc()
        {
            if (IsCaption)
                return String.Empty;
            return String.Format("{0}\r\n{1}", Text, Response.GetResponseDesc());
        }

        public virtual IATSurveyFile.SurveyItem GenerateSerializableItem(IATSurveyFile.Survey s)
        {
            if (ParentSurveyUri == null)
                return null;
            IATSurveyFile.SurveyItem item = new IATSurveyFile.SurveyItem(s, CIAT.SaveFile.GetSurvey(ParentSurveyUri).GetItemNum(this));
            item.Text = Text;
            return item;
        }

        public virtual Panel GenerateCorrectResponseDefinitionPanel(int width, System.Drawing.Color backColor, System.Drawing.Color foreColor)
        {
            Panel preview = new Panel();
            preview.Size = new Size(width, 100);
            int clientWidth = width - PreviewPanelPadding.Horizontal;
            preview.BackColor = backColor;
            Font QuestionFont = new Font(PreviewFontFamily, PreviewQuestionFontSize);
            Brush br = new SolidBrush(foreColor);
            Size szQuestion = System.Windows.Forms.TextRenderer.MeasureText(Text, QuestionFont, new Size(clientWidth, 0), TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            TextBox QuestionBox = new TextBox();
            QuestionBox.ReadOnly = true;
            QuestionBox.Multiline = true;
            QuestionBox.BorderStyle = BorderStyle.None;
            QuestionBox.BackColor = backColor;
            QuestionBox.Size = new Size(clientWidth, szQuestion.Height);
            QuestionBox.Font = QuestionFont;
            QuestionBox.ForeColor = foreColor;
            QuestionBox.Text = Text;
            QuestionBox.Location = new Point(PreviewPanelPadding.Left, PreviewPanelPadding.Top);
            preview.Controls.Add(QuestionBox);
            Panel ResponsePreview = DefinedResponse.GenerateResponseObjectPanel(backColor, foreColor, PreviewFontFamily, PreviewChoiceFontSize, clientWidth - ResponsePreviewPadding.Horizontal);
            ResponsePreview.Location = new Point(PreviewPanelPadding.Left + ResponsePreviewPadding.Left, (QuestionBox.Bottom + PreviewPanelPadding.Vertical));
            preview.Controls.Add(ResponsePreview);
            return preview;
        }

        public virtual Control GenerateItemPreviewPanel(int width, System.Drawing.Color backColor, System.Drawing.Color foreColor)
        {
            Panel preview = new Panel();
            preview.Size = new Size(width, 100);
            int clientWidth = width - PreviewPanelPadding.Horizontal;
            preview.BackColor = backColor;
            Font QuestionFont = new Font(PreviewFontFamily, PreviewQuestionFontSize);
            Size szQuestion = System.Windows.Forms.TextRenderer.MeasureText(Text, QuestionFont, new Size(clientWidth, 0), TextFormatFlags.TextBoxControl | TextFormatFlags.WordBreak);
            TextBox QuestionBox = new TextBox();
            QuestionBox.ReadOnly = true;
            QuestionBox.Multiline = true;
            QuestionBox.BorderStyle = BorderStyle.None;
            QuestionBox.BackColor = backColor;
            QuestionBox.Size = new Size(clientWidth, szQuestion.Height);
            QuestionBox.Font = QuestionFont;
            QuestionBox.ForeColor = foreColor;
            QuestionBox.Text = Text;
            QuestionBox.Location = new Point(PreviewPanelPadding.Left, PreviewPanelPadding.Top);
            preview.Controls.Add(QuestionBox);
            if (Response.ResponseType != CResponse.EResponseType.Instruction)
            {
                CreateDummyResponse();
                Panel ResponsePreview = DummyResponse.GenerateResponseObjectPanel(backColor, foreColor, PreviewFontFamily, PreviewChoiceFontSize, clientWidth - ResponsePreviewPadding.Horizontal);
                ResponsePreview.Location = new Point(PreviewPanelPadding.Left + ResponsePreviewPadding.Left, (QuestionBox.Bottom + PreviewPanelPadding.Vertical));
                preview.Controls.Add(ResponsePreview);
                preview.Size = new Size(width, ResponsePreview.Bottom + PreviewPanelPadding.Bottom);
            }
            else
                preview.Size = new Size(width, QuestionBox.Bottom + PreviewPanelPadding.Bottom);
            return preview;
        }
        /*
        public void GeneratePreview(Panel previewPanel)
        {
            Size sz = previewPanel.Size;
            System.Drawing.Color backColor = System.Drawing.Color.White;
            System.Drawing.Color foreColor = System.Drawing.Color.Black;
            previewPanel.BackColor = backColor;
            Font QuestionFont = new Font(PreviewFontFamily, PreviewQuestionFontSize);
            Brush br = new SolidBrush(foreColor);
            Size szQuestion = System.Windows.Forms.TextRenderer.MeasureText(Text, QuestionFont, new Size(sz.Width, 0));
            TextBox QuestionBox = new TextBox();
            QuestionBox.ReadOnly = true;
            QuestionBox.Multiline = true;
            QuestionBox.BorderStyle = BorderStyle.None;
            QuestionBox.BackColor = backColor;
            QuestionBox.Size = new Size(sz.Width, szQuestion.Height);
            QuestionBox.Font = QuestionFont;
            QuestionBox.ForeColor = foreColor;
            QuestionBox.Text = Text;
            QuestionBox.Location = new Point(PreviewPanelPadding.Left, PreviewPanelPadding.Top);
            previewPanel.Controls.Add(QuestionBox);
            int responseHeight = Response.AddPreviewControls(previewPanel, backColor, foreColor, PreviewFontFamily, PreviewChoiceFontSize,
                new Rectangle(PreviewPanelPadding.Left, QuestionBox.Bottom + PreviewPanelPadding.Top, sz.Width, sz.Height - PreviewPanelPadding.Bottom));
        }
        */
        
        public virtual object Clone()
        {
            CSurveyItem si = new CSurveyItem(this);
            return si;
        }
        
        public virtual void Dispose()
        {
            if (IsDisposed)
                return;
            IsDisposed = true;
            if (rSurveyId != null) 
                CIAT.SaveFile.GetSurvey(ParentSurveyUri).Items.Remove(this);
            CIAT.SaveFile.DeletePart(URI);
        }
    }
}
