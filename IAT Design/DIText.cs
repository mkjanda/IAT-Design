using IATClient.Images;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading;
using System.Xml.Linq;

namespace IATClient
{
    public class FontPreference : Enumeration
    {
        public static readonly FontPreference Stimulus = new FontPreference(1, "Stimulus", DIText.UsedAs.Stimulus, 32);
        public static readonly FontPreference ContinueInstructions = new FontPreference(2, "ContinueInstructions", DIText.UsedAs.ContinueInstructions, 16);
        public static readonly FontPreference ResponseKey = new FontPreference(3, "ResponseKey", DIText.UsedAs.ResponseKey, 22);
        public static readonly FontPreference Conjunction = new FontPreference(4, "Conjunction", DIText.UsedAs.Conjunction, 12);
        public static readonly FontPreference IatBlockInstructions = new FontPreference(5, "IatBlockInstructions", DIText.UsedAs.IatBlockInstructions, 14);
        public static readonly FontPreference TextInstructionsScreen = new FontPreference(6, "TextInstructionsScreen", DIText.UsedAs.TextInstructionsScreen, 18);

        private FontPreference(int value, String name, DIText.UsedAs usedAs, float fontSize) : base(value, name)
        {
            UsedAs = usedAs;
            FontSize = fontSize;
            FontColor = NamedColor.White;
            FontFamily = System.Drawing.SystemFonts.DefaultFont.FontFamily.Name;
            LineSpacing = 1F;
            Justification = TextJustification.Center;
        }

        public static readonly IEnumerable<FontPreference> All = new FontPreference[] { Stimulus, ContinueInstructions, ResponseKey, IatBlockInstructions,
                    TextInstructionsScreen };
        public static FontPreference FromString(String name)
        {
            return All.Where(fp => fp.Name == name).FirstOrDefault();
        }
        public float FontSize { get; set; }
        public NamedColor FontColor { get; set; }
        public String FontFamily { get; set; }
        public float LineSpacing { get; set; }
        public TextJustification Justification { get; set; }
        public DIText.UsedAs UsedAs { get; private set; }

        public void Save(XElement elem)
        {
            elem.Add(new XElement(Name, new XAttribute("for", UsedAs.Name), new XElement("FontSize", FontSize.ToString()),
                new XElement("FontColor", FontColor.Name), new XElement("FontFamily", FontFamily), new XElement("LineSpacing", LineSpacing.ToString()),
                new XElement("Justification", Justification.ToString())));
        }

        public void Load(XElement elem)
        {
            FontSize = Convert.ToSingle(elem.Element("FontSize").Value);
            FontColor = NamedColor.GetNamedColor(elem.Element("FontColor").Value);
            FontFamily = elem.Element("FontFamily").Value;
            LineSpacing = Convert.ToSingle(elem.Element("LineSpacing").Value);
            Justification = TextJustification.FromString(elem.Element("Justification").Value);
            UsedAs = DIText.UsedAs.FromString(elem.Attribute("for").Value);
        }
    }

    public class TextJustification : Enumeration
    {
        public static readonly TextJustification Left = new TextJustification(1, "Left");
        public static readonly TextJustification Center = new TextJustification(2, "Center");
        public static readonly TextJustification Right = new TextJustification(3, "Right");

        private TextJustification(int id, String name) : base(id, name) { }

        private static readonly IEnumerable<TextJustification> All = new TextJustification[] { Left, Center, Right };
        public static TextJustification FromString(String str)
        {
            try
            {
                return All.Where(j => j.Name == str).First();
            }
            catch (ArgumentException ex)
            {
                ErrorReporter.ReportError(new CReportableException(String.Format("Cannot convert {0} to a text justification value.", str), ex));
                return null;
            }
        }
    }

    public class FontPreferences : IPackagePart
    {

        public Uri URI { get; set; }
        public Type BaseType { get { return typeof(FontPreferences); } }
        public String MimeType { get { return "text/xml+" + typeof(FontPreferences).ToString(); } }

        public FontPreference this[DIText.UsedAs ua]
        {
            get
            {
                if (ua == DIText.UsedAs.Conjunction)
                    return FontPreference.Conjunction;
                if (ua == DIText.UsedAs.ContinueInstructions)
                    return FontPreference.ContinueInstructions;
                if (ua == DIText.UsedAs.IatBlockInstructions)
                    return FontPreference.IatBlockInstructions;
                if (ua == DIText.UsedAs.ResponseKey)
                    return FontPreference.ResponseKey;
                if (ua == DIText.UsedAs.Stimulus)
                    return FontPreference.Stimulus;
                if ((ua == DIText.UsedAs.TextInstructionsScreen) || (ua == DIText.UsedAs.KeyedInstructionsScreen) || (ua == DIText.UsedAs.MockItemInstructions))
                    return FontPreference.TextInstructionsScreen;
                return null;
            }
        }

        public FontPreferences()
        {
            this.URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
            CIAT.SaveFile.CreatePackageLevelRelationship(URI, typeof(FontPreferences));
        }
        public FontPreferences(Uri uri)
        {
            this.URI = uri;
            Load();
        }
        public void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("FontPreferences"));
            foreach (FontPreference fp in FontPreference.All)
                fp.Save(xDoc.Root);
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }
        public void Load()
        {
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            foreach (FontPreference fp in FontPreference.All)
                fp.Load(xDoc.Root.Element(fp.Name));
        }
        public void Dispose()
        {
            CIAT.SaveFile.DeletePart(this.URI);
        }
    }

    public abstract class DIText : DIGenerated
    {
        public class UsedAs : Enumeration
        {
            public static readonly UsedAs Stimulus = new UsedAs(1, "Stimulus", new Func<Size>(() => { return CIAT.SaveFile.Layout.StimulusSize; }));
            public static readonly UsedAs ContinueInstructions = new UsedAs(2, "ContinueInstructions", new Func<Size>(() => { return Size.Empty; }));
            public static readonly UsedAs ResponseKey = new UsedAs(3, "ResponseKey", new Func<Size>(() => { return CIAT.SaveFile.Layout.KeyValueSize; }));
            public static readonly UsedAs Conjunction = new UsedAs(4, "Conjunction", new Func<Size>(() => { return CIAT.SaveFile.Layout.KeyValueSize; }));
            public static readonly UsedAs MockItemInstructions = new UsedAs(5, "MockItemInstructions", new Func<Size>(() => { return CIAT.SaveFile.Layout.MockItemInstructionsRectangle.Size; }));
            public static readonly UsedAs IatBlockInstructions = new UsedAs(6, "IatBlockInstructions", new Func<Size>(() => { return CIAT.SaveFile.Layout.InstructionsSize; }));
            public static readonly UsedAs TextInstructionsScreen = new UsedAs(7, "TextInstructionsScreen", new Func<Size>(() => { return CIAT.SaveFile.Layout.InstructionScreenTextAreaRectangle.Size; }));
            public static readonly UsedAs KeyedInstructionsScreen = new UsedAs(8, "KeyedInstructionsScreen", new Func<Size>(() => { return CIAT.SaveFile.Layout.KeyInstructionScreenTextAreaRectangle.Size; }));
            private UsedAs(int id, String name, Func<Size> retrieveSize)
                : base(id, name)
            {
                RetrieveSize = retrieveSize;
            }
            private static readonly IEnumerable<UsedAs> All = new UsedAs[] { Stimulus, ContinueInstructions, ResponseKey, MockItemInstructions, IatBlockInstructions, TextInstructionsScreen, KeyedInstructionsScreen };
            public Func<Size> RetrieveSize { get; private set; }
            public FontPreference FontPreference { get; private set; }
            public static UsedAs FromString(String str)
            {
                return All.Where((val) => val.Name == str).FirstOrDefault();
            }
        }


        private Font phraseFont = null;
        public Font PhraseFont
        {
            get
            {
                lock (lockObject)
                {
                    return phraseFont;
                }
            }
            set
            {
                if (value == phraseFont)
                    return;
                lock (lockObject)
                {
                    if (phraseFont != null)
                        phraseFont.Dispose();
                    phraseFont = value;
                    ScheduleInvalidation();
                }
            }
        }

        private float phraseFontSize;
        public float PhraseFontSize
        {
            get
            {
                return phraseFontSize;
            }
            set
            {
                CIAT.SaveFile.FontPreferences[usedAs].FontSize = value;
                if (value == phraseFontSize)
                    return;
                phraseFontSize = value;
                PhraseFont = new Font(PhraseFontFamily, value);
            }
        }

        private String phraseFontFamily;
        public virtual String PhraseFontFamily
        {
            get
            {
                return phraseFontFamily;
            }
            set
            {
                CIAT.SaveFile.FontPreferences[usedAs].FontFamily = value;
                if (value == phraseFontFamily)
                    return;
                phraseFontFamily = value;
                PhraseFont = new Font(value, PhraseFontSize);
            }
        }

        private NamedColor phraseFontColor;
        public NamedColor PhraseFontColor
        {
            get
            {
                return phraseFontColor;
            }
            set
            {
                CIAT.SaveFile.FontPreferences[usedAs].FontColor = value;
                phraseFontColor = value;
                ScheduleInvalidation();
            }
        }

        private String phrase = String.Empty;
        public String Phrase
        {
            get
            {
                return phrase;
            }
            set
            {
                if (value == phrase)
                    return;
                phrase = value;
                ScheduleInvalidation();
            }
        }


        private TextJustification justification;
        public TextJustification Justification
        {
            get
            {
                return justification;
            }
            set
            {
                CIAT.SaveFile.FontPreferences[usedAs].Justification = value;
                if (value == Justification)
                    return;
                justification = value;
                ScheduleInvalidation();
            }
        }

        private float lineSpacing;
        public float LineSpacing
        {
            get
            {
                return lineSpacing;
            }
            set
            {
                CIAT.SaveFile.FontPreferences[usedAs].LineSpacing = value;
                if (LineSpacing == value)
                    return;
                lineSpacing = value;
                ScheduleInvalidation();
            }
        }


        public void SetFont(DIText.UsedAs usedAs)
        {
            lock (lockObject)
            {
                FontPreference preference = CIAT.SaveFile.FontPreferences[usedAs];
                phraseFontFamily = preference.FontFamily;
                phraseFontColor = preference.FontColor;
                phraseFontSize = preference.FontSize;
                lineSpacing = preference.LineSpacing;
                justification = preference.Justification;
                phraseFont = new Font(phraseFontFamily, phraseFontSize);
            }
        }


        public override void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Document.Add(new XElement(BaseType.ToString(),
                new XElement("PhraseFontFamily", PhraseFontFamily), new XElement("PhraseFontSize", PhraseFontSize.ToString()),
                new XElement("PhraseFontColor", PhraseFontColor.Name), new XElement("Justification", Justification.ToString()),
                new XElement("LineSpacing", LineSpacing.ToString()), new XElement("AbsoluteBounds", new XElement("Top", AbsoluteBounds.Top.ToString()),
                new XElement("Left", AbsoluteBounds.Left.ToString()), new XElement("Width", AbsoluteBounds.Width.ToString()),
                new XElement("Height", AbsoluteBounds.Height.ToString()))));
            if (this.IImage != null)
                xDoc.Root.Add(new XAttribute("rImageId", rImageId));
            foreach (var str in Phrase.Split(new String[] { "\r\n" }, StringSplitOptions.None))
                xDoc.Root.Add(new XElement("Phrase", str));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            phrase = String.Empty;
            foreach (XElement elem in xDoc.Root.Elements("Phrase"))
                phrase += elem.Value + "\r\n";
            phrase = phrase.TrimEnd();
            phraseFontFamily = xDoc.Root.Element("PhraseFontFamily").Value;
            phraseFontSize = Convert.ToSingle(xDoc.Root.Element("PhraseFontSize").Value);
            phraseFontColor = NamedColor.GetNamedColor(xDoc.Root.Element("PhraseFontColor").Value);
            justification = TextJustification.FromString(xDoc.Root.Element("Justification").Value);
            lineSpacing = Convert.ToSingle(xDoc.Root.Element("LineSpacing").Value);
            AbsoluteBounds = new Rectangle(Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Left").Value),
                Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Top").Value), Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Width").Value),
                Convert.ToInt32(xDoc.Root.Element("AbsoluteBounds").Element("Height").Value));
            phraseFont = new Font(PhraseFontFamily, PhraseFontSize);
            if (xDoc.Root.Attribute("rImageId") != null)
            {
                var iStateAttr = xDoc.Root.Attribute("InvalidationState");
                if (iStateAttr != null)
                    InvalidationState = InvalidationStates.Parse(iStateAttr.Value);
                else
                    InvalidationState = InvalidationStates.InvalidationReady;
                rImageId = xDoc.Root.Attribute("rImageId").Value;
                SetImage(rImageId);
            }
        }

        private UsedAs usedAs;

        public DIText(UsedAs usedAs)
        {
            SetFont(usedAs);
            this.usedAs = usedAs;
        }

        public DIText(Uri uri, UsedAs usedAs)
            : base(uri)
        {
            this.usedAs = usedAs;
        }

        protected Bitmap GenerateText()
        {
            String str = Phrase;
            Size bSz = BoundingSize;
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FromDIType(Type));
            Graphics g = Graphics.FromImage(bmp);
            Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            g.FillRectangle(backBr, new Rectangle(new Point(0, 0), bSz));
            backBr.Dispose();
            SizeF sz = g.MeasureString(str, PhraseFont);
            PointF ptDraw = new PointF();
            if (Justification == TextJustification.Left)
                ptDraw = new PointF(0, (bSz.Height - sz.Height) / 2);
            else if (Justification == TextJustification.Center)
                ptDraw = new PointF((bSz.Width - sz.Width) / 2, (bSz.Height - sz.Height) / 2);
            else if (Justification == TextJustification.Right)
                ptDraw = new PointF(bSz.Width - sz.Width, (bSz.Height - sz.Height) / 2);
            Brush br = new SolidBrush(PhraseFontColor.Color);
            g.DrawString(Phrase, PhraseFont, br, ptDraw);
            g.Dispose();
            return bmp;
        }

        protected override Bitmap Generate()
        {
            if (Broken || IsDisposed)
                return null;
            Bitmap bmp = GenerateText();
            CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
            bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
            return bmp;
        }
        public override void Dispose()
        {
            base.Dispose();
            phraseFont.Dispose();
        }
    }

    public abstract class DIMultiLineText : DIText
    {
        public DIMultiLineText(DIText.UsedAs usedAs) : base(usedAs) { }
        public DIMultiLineText(Uri uri, DIText.UsedAs usedAs) : base(uri, usedAs) { }

        protected override Bitmap Generate()
        {
            if (Broken || IsDisposed)
                return null;
            if (PreviewPanel != null)
            {
                if (!PreviewPanel.IsHandleCreated)
                {
                    PreviewPanel.HandleCreated += (sender, evt) => { Generate(); };
                    return null;
                }
            }
            String str = Phrase;
            Size bSz = BoundingSize;
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FromDIType(Type));
            Graphics g = Graphics.FromImage(bmp);
            Brush backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            g.FillRectangle(backBr, new Rectangle(new Point(0, 0), bSz));
            backBr.Dispose();
            SizeF sz = g.MeasureString(str, PhraseFont, bSz.Width);
            List<String> lines = new List<String>();
            String phrase = Phrase.TrimEnd();
            Brush phraseBr = new SolidBrush(PhraseFontColor.Color);
            int offset = 0, count = 1;
            while (offset < phrase.Length)
            {
                count = 0;
                int ndx = phrase.IndexOf("\r\n", offset) + 2;
                String subPhrase;
                if (ndx == 1)
                    subPhrase = phrase.Substring(offset, phrase.Length - offset);
                else
                    subPhrase = phrase.Substring(offset, ndx - offset);

                while (count <= subPhrase.Length)
                {
                    if (g.MeasureString(subPhrase.Substring(0, count).Trim(), PhraseFont).Width > bSz.Width)
                    {
                        while (!Char.IsWhiteSpace(subPhrase[--count]))
                        {
                            if (count == 0)
                            {
                                count = subPhrase.Trim().Length;
                                break;
                            }
                        }
                        if (count == 0)
                            break;
                        lines.Add(subPhrase.Substring(0, count).Trim());
                        break;
                        //                            offset += count - 1;
                    }
                    /*                    else if (subPhrase.Substring(offset).Length > 1 + count)
                                        {
                                            if (subPhrase.Substring(offset, count - 1).EndsWith("\r\n"))
                                            {
                                                lines.Add(phrase.Substring(offset, ((newLineNdx != -1) ? Convert.ToInt32(newLineNdx) : count) - 1).Replace("\r\n", ""));
                                                offset += ((newLineNdx != -1) ? Convert.ToInt32(newLineNdx) : count) - 1;
                                                count = 0;
                                            }
                                        }*/
                    else if (count == subPhrase.Length)
                        lines.Add(subPhrase.Trim());
                    count++;
                }
                offset += count;
            }
            float lineHeight = PhraseFont.GetHeight(g) * LineSpacing;
            for (int nLine = 0; nLine < lines.Count; nLine++)
            {
                SizeF szLine = g.MeasureString(lines[nLine], PhraseFont);
                PointF ptDraw = new PointF();
                if (Justification == TextJustification.Left)
                    ptDraw = new PointF(0, nLine * lineHeight);
                else if (Justification == TextJustification.Center)
                    ptDraw = new PointF((bSz.Width - szLine.Width) / 2, nLine * lineHeight);
                else if (Justification == TextJustification.Right)
                    ptDraw = new PointF(bSz.Width - szLine.Width, nLine * lineHeight);
                g.DrawString(lines[nLine], PhraseFont, phraseBr, ptDraw);
            }
            g.Dispose();
            phraseBr.Dispose();
            CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
            bmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
            return bmp;
        }
    }

    public class DIStimulusText : DIText, IStimulus
    {

        private void OnThumbnailChanged(ImageEvent evt, IImageMedia iMedia, object arg)
        {
            if (ThumbnailPreviewPanel == null)
                return;
            if (!ThumbnailPreviewPanel.IsHandleCreated)
                ThumbnailPreviewPanel.HandleCreated += (sender, args) => { ThumbnailPreviewPanel.SetImage(iMedia); };
            else
                ThumbnailPreviewPanel.SetImage(iMedia);
        }

        private IImageDisplay _ThumbnailPreviewPanel = null;
        public IImageDisplay ThumbnailPreviewPanel
        {
            get
            {
                return _ThumbnailPreviewPanel;
            }
            set
            {
                _ThumbnailPreviewPanel = value;
                if ((value != null) && (IImage != null))
                {
                    if (IImage.Thumbnail != null)
                        value.SetImage(IImage.Thumbnail);
                    else
                    {
                        IImage.CreateThumbnail();
                        IImage.Thumbnail.Changed += (evt, iMedia, arg) => OnThumbnailChanged(evt, iMedia, arg);
                    }
                }
            }
        }
        public DIStimulusText()
            : base(DIText.UsedAs.Stimulus)
        {
        }
        public DIStimulusText(Uri uri)
            : base(uri, DIText.UsedAs.Stimulus)
        {
        }
        public String Description
        {
            get
            {
                return Phrase;
            }
        }

        protected override void OnImageEvent(ImageEvent evt, IImageMedia img, object arg)
        {
            base.OnImageEvent(evt, img, arg);
            if (IsDisposed)
                return;
            if ((evt == Images.ImageEvent.Updated) || (evt == Images.ImageEvent.Resized))
                CIAT.ImageManager.GenerateThumb(IImage);
        }

        public bool Equals(IStimulus stim)
        {
            if (Type != stim.Type)
                return false;
            DIStimulusText textStim = stim as DIStimulusText;
            if (!PhraseFontColor.Color.ToArgb().Equals(textStim.PhraseFontColor.Color.ToArgb()))
                return false;
            if (PhraseFontFamily != textStim.PhraseFontFamily)
                return false;
            if (Phrase != textStim.Phrase)
                return false;
            if (PhraseFontSize != textStim.PhraseFontSize)
                return false;
            return true;
        }
        public override object Clone()
        {
            DIStimulusText di = new DIStimulusText();
            di.SuspendLayout();
            di.Justification = this.Justification;
            di.LineSpacing = this.LineSpacing;
            di.Phrase = this.Phrase;
            di.PhraseFontColor = this.PhraseFontColor;
            di.PhraseFontFamily = this.PhraseFontFamily;
            di.IImage = IImage.Clone() as IImage;
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, di.IImage.URI);
            di.IImage.Changed += (evt, img, arg) => di.OnImageEvent(evt, img, arg);
            di.IImage.Thumbnail.Changed += (evt, img, args) => di.OnImageEvent(evt, img, args);
            di.ResumeLayout(true);
            return di;
        }
        public override void Dispose()
        {
            if (IsDisposed)
                return;
            if (ThumbnailPreviewPanel != null)
                ThumbnailPreviewPanel.ClearImage();
            base.Dispose();
        }
    }
    public class DIContinueInstructions : DIText
    {
        public DIContinueInstructions() : base(DIText.UsedAs.ContinueInstructions)
        {
            Justification = TextJustification.Center;
        }
        public DIContinueInstructions(Uri uri) : base(uri, DIText.UsedAs.ContinueInstructions) { }
        public override object Clone()
        {
            DIContinueInstructions di = new DIContinueInstructions();
            di.SuspendLayout();
            di.Justification = this.Justification;
            di.LineSpacing = this.LineSpacing;
            di.Phrase = this.Phrase;
            di.PhraseFontColor = this.PhraseFontColor;
            di.PhraseFontFamily = this.PhraseFontFamily;
            di.IImage = IImage.Clone() as IImage;
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, di.IImage.URI);
            di.IImage.Changed += (evt, img, arg) => di.OnImageEvent(evt, img, arg);
            di.AbsoluteBounds = AbsoluteBounds;
            di.ResumeLayout(true);
            return di;
        }
        public override void Dispose()
        {
            var prevRel = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(DIBase), "owned-by").FirstOrDefault();
            if (prevRel != null)
            {
                var prev = CIAT.SaveFile.GetDI(prevRel.TargetUri) as DIPreview;
                prev.RemoveComponent(LayoutItem.ContinueInstructions, false);
            }
            var rel = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CInstructionScreen)).FirstOrDefault();
            if (rel != null)
            {
                CInstructionScreen instr = CIAT.SaveFile.GetInstructionScreen(rel.TargetUri);
                CIAT.SaveFile.DeleteRelationship(URI, rel.Id);
                String rId = CIAT.SaveFile.GetRelationship(instr, this);
                CIAT.SaveFile.DeleteRelationship(instr.URI, rId);
            }
            base.Dispose();
        }
    }
    public class DIResponseKeyText : DIText, IResponseKeyDI
    {
        private object invalidationLockOwner = null;
        public Action ValidateData { get; set; }
        public DIResponseKeyText() : base(DIText.UsedAs.ResponseKey)
        {
            PreviewPanel = null;
            ValidateData = null;
        }
        public DIResponseKeyText(Uri uri) : base(uri, DIText.UsedAs.ResponseKey)
        {
            PreviewPanel = null;
            ValidateData = null;
        }
        private IImageDisplay _PreviewPanel = null;
        public override IImageDisplay PreviewPanel
        {
            get
            {
                return _PreviewPanel;
            }
            set
            {
                if (value == _PreviewPanel)
                    return;
                if (_PreviewPanel != null)
                {
                    if (_PreviewPanel.IsHandleCreated)
                        _PreviewPanel.ClearImage();
                }
                _PreviewPanel = value;
                if (value == null)
                    return;
                PreviewPanel.SetImage(IImage);
            }
        }
        public List<CIATKey> KeyOwners
        {
            get
            {
                try
                {
                    return CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).ToList();
                }
                catch (Exception)
                {
                    return new List<CIATKey>();
                }
            }
        }
        public override string PhraseFontFamily
        {
            get => base.PhraseFontFamily; set
            {
                base.PhraseFontFamily = value;
            }
        }

        public void AddKeyOwner(IPackagePart pp)
        {
            if (CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, pp.BaseType).Where(pr => pr.TargetUri.Equals(pp.URI)).Count() != 0)
                return;
            CIAT.SaveFile.CreateRelationship(BaseType, pp.BaseType, this.URI, pp.URI);
            CIAT.SaveFile.CreateRelationship(pp.BaseType, BaseType, pp.URI, URI);
        }
        public void SetKeyOwners(List<CIATKey> owners)
        {
            List<IPackagePart> oldOwners = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CIATKey)).Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).ToList<IPackagePart>();
            foreach (var oldOwner in oldOwners)
            {
                CIAT.SaveFile.DeleteRelationship(URI, oldOwner.URI);
                CIAT.SaveFile.DeleteRelationship(oldOwner.URI, URI);
            }
            foreach (var pp in owners)
                AddKeyOwner(pp);
            if (owners.Count == 0)
                return;
        }
        public void ReleaseKeyOwner(IPackagePart owner)
        {
            CIAT.SaveFile.DeleteRelationship(URI, owner.URI);
            CIAT.SaveFile.DeleteRelationship(owner.URI, URI);
            if (CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, owner.BaseType).Count == 0)
                this.Dispose();
        }
        public void ReleaseKeyOwners(List<CIATKey> owners)
        {
            foreach (var owner in owners)
                ReleaseKeyOwner(owner);
        }
        protected override void OnImageEvent(Images.ImageEvent evt, Images.IImageMedia img, object arg)
        {
            base.OnImageEvent(evt, img, arg);
            if ((evt == Images.ImageEvent.Updated) || (evt == ImageEvent.Initialized))
            {
                if (PreviewPanel != null)
                    if (!PreviewPanel.IsDisposed)
                        PreviewPanel.SetImage(img);
                ValidateData?.Invoke();
            }
        }
        public override void Replace(DIBase target)
        {
            if (!(target is IResponseKeyDI))
                throw new ArgumentException("A DIResponseKeyText object can only be replaced by an object that implements IRessponseKeyDI.");
            IResponseKeyDI rkDi = target as IResponseKeyDI;
            this.PreviewPanel = rkDi.PreviewPanel;
            this.ValidateData = rkDi.ValidateData;
            base.Replace(target);
        }

        public override object Clone()
        {
            DIResponseKeyText di = new DIResponseKeyText();
            di.IImage = IImage.Clone() as IImage;
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, di.IImage.URI);
            di.IImage.Changed += (evt, img, arg) => di.OnImageEvent(evt, img, arg);
            di.SuspendLayout();
            di.Justification = this.Justification;
            di.SetKeyOwners(KeyOwners);
            di.LineSpacing = this.LineSpacing;
            di.Phrase = this.Phrase;
            di.PhraseFontColor = this.PhraseFontColor;
            di.PhraseFontFamily = this.PhraseFontFamily;
            di.PhraseFontSize = this.PhraseFontSize;
            di.ValidateData = this.ValidateData;
            di.AbsoluteBounds = AbsoluteBounds;
            foreach (CIATKey k in KeyOwners)
                di.AddKeyOwner(k);
            di.ResumeLayout(true);
            return di;
        }
        public override void Dispose()
        {
            if (IsDisposed || IsDisposing)
                return;
            IsDisposing = true;
            if (PreviewPanel != null)
            {
                PreviewPanel.ClearImage();
                PreviewPanel = null;
            }
            ReleaseKeyOwners(KeyOwners);
            base.Dispose();
            IsDisposed = true;
        }
    }
    public class DIConjunction : DIText, IResponseKeyDI
    {
        private ManualResetEvent BoundsEvent = new ManualResetEvent(false);
        public override Rectangle AbsoluteBounds
        {
            get
            {
                BoundsEvent.WaitOne(1000);
                return base.AbsoluteBounds;
            }
            protected set
            {
                base.AbsoluteBounds = value;
                BoundsEvent.Set();
            }
        }
        public Action ValidateData { get; set; }
        public DIConjunction() : base(DIText.UsedAs.Conjunction)
        {
            ValidateData = null;
            PreviewPanel = null;
            Phrase = "or";
        }
        public DIConjunction(Uri uri) : base(uri, DIText.UsedAs.Conjunction)
        {
            ValidateData = null;
            PreviewPanel = null;
        }

        protected override void OnImageEvent(Images.ImageEvent evt, Images.IImageMedia img, object arg)
        {
            base.OnImageEvent(evt, img, arg);
            if ((evt == Images.ImageEvent.Updated) || (evt == Images.ImageEvent.Initialized))
            {
                ValidateData?.Invoke();
                BoundsEvent.Set();
            }
        }

        protected override Bitmap Generate()
        {
            BoundsEvent.Reset();
            return base.Generate();
        }

        public List<CIATKey> KeyOwners
        {
            get
            {
                try
                {
                    return CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Select(pr => CIAT.SaveFile.GetIATKey(pr.TargetUri)).ToList();
                }
                catch (Exception)
                {
                    return new List<CIATKey>();
                }
            }
        }
        public void AddKeyOwner(IPackagePart p)
        {
            if (CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Where(pr => pr.TargetUri.Equals(p.URI)).Count() != 0)
                return;
            CIAT.SaveFile.CreateRelationship(BaseType, p.BaseType, URI, p.URI);
            CIAT.SaveFile.CreateRelationship(p.BaseType, BaseType, p.URI, URI);
        }
        public void SetKeyOwners(List<CIATKey> newOwners)
        {
            var owners = CIAT.SaveFile.GetRelationshipsByType(URI, BaseType, typeof(CIATKey)).Select(pr => pr.TargetUri).ToList();
            foreach (var o in owners)
            {
                CIAT.SaveFile.DeleteRelationship(URI, o);
                CIAT.SaveFile.DeleteRelationship(o, URI);
            }
            foreach (var pp in newOwners)
                AddKeyOwner(pp);
            if (newOwners.Count == 0)
                Dispose();
        }
        public void ReleaseKeyOwner(IPackagePart pp)
        {
            CIAT.SaveFile.DeleteRelationship(URI, pp.URI);
            CIAT.SaveFile.DeleteRelationship(pp.URI, URI);
            if (CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, typeof(CIATKey)).Count() == 0)
                this.Dispose();
        }
        public void ReleaseKeyOwners(List<CIATKey> pps)
        {
            foreach (var pp in pps)
                ReleaseKeyOwner(pp);
        }
        public override void Replace(DIBase target)
        {
            if (target.Type != DIType.Conjunction)
                throw new ArgumentException("A DIConjunction object can only be replaced by another object of the same type.");
            IResponseKeyDI rkDi = target as IResponseKeyDI;
            this.PreviewPanel = rkDi.PreviewPanel;
            this.ValidateData = rkDi.ValidateData;
            base.Replace(target);
        }
        public override void Dispose()
        {
            if (IsDisposed || IsDisposing)
                return;
            IsDisposing = true;
            ReleaseKeyOwners(KeyOwners);
            base.Dispose();
            IsDisposed = true;
        }
        public override object Clone()
        {
            DIConjunction di = new DIConjunction();
            di.SuspendLayout();
            di.Justification = this.Justification;
            di.SetKeyOwners(KeyOwners);
            di.LineSpacing = this.LineSpacing;
            di.Phrase = this.Phrase;
            di.PhraseFontColor = this.PhraseFontColor;
            di.PhraseFontFamily = this.PhraseFontFamily;
            di.PreviewPanel = this.PreviewPanel;
            di.ValidateData = this.ValidateData;
            di.IImage = IImage.Clone() as IImage;
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, di.IImage.URI);
            di.IImage.Changed += (evt, img, arg) => di.OnImageEvent(evt, img, arg);
            di.AbsoluteBounds = AbsoluteBounds;
            di.ResumeLayout(true);
            foreach (CIATKey k in KeyOwners)
                di.AddKeyOwner(k);
            return di;
        }
    }
    public class DIMockItemInstructions : DIMultiLineText
    {
        public DIMockItemInstructions() : base(DIText.UsedAs.MockItemInstructions) { }
        public DIMockItemInstructions(Uri uri) : base(uri, DIText.UsedAs.MockItemInstructions) { }
        public override object Clone()
        {
            DIMockItemInstructions di = new DIMockItemInstructions();
            di.SuspendLayout();
            di.Justification = this.Justification;
            di.LineSpacing = this.LineSpacing;
            di.Phrase = this.Phrase;
            di.PhraseFontColor = this.PhraseFontColor;
            di.PhraseFontFamily = this.PhraseFontFamily;
            di.IImage = IImage.Clone() as IImage;
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, di.IImage.URI);
            di.IImage.Changed += (evt, img, arg) => di.OnImageEvent(evt, img, arg);
            di.AbsoluteBounds = AbsoluteBounds;
            di.ResumeLayout(true);
            return di;
        }
    }
    public class DIIatBlockInstructions : DIMultiLineText
    {
        public DIIatBlockInstructions() : base(DIText.UsedAs.IatBlockInstructions) { }
        public DIIatBlockInstructions(Uri uri) : base(uri, DIText.UsedAs.IatBlockInstructions) { }
        public override object Clone()
        {
            DIIatBlockInstructions di = new DIIatBlockInstructions();
            di.SuspendLayout();
            di.Justification = this.Justification;
            di.LineSpacing = this.LineSpacing;
            di.Phrase = this.Phrase;
            di.PhraseFontColor = this.PhraseFontColor;
            di.PhraseFontFamily = this.PhraseFontFamily;
            di.IImage = IImage.Clone() as IImage;
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, di.IImage.URI);
            di.IImage.Changed += (evt, img, arg) => di.OnImageEvent(evt, img, arg);
            di.AbsoluteBounds = AbsoluteBounds;
            di.ResumeLayout(true);
            return di;
        }
    }
    public class DITextInstructionsScreen : DIMultiLineText
    {
        public DITextInstructionsScreen() : base(DIText.UsedAs.TextInstructionsScreen) { }
        public DITextInstructionsScreen(Uri uri) : base(uri, DIText.UsedAs.TextInstructionsScreen) { }
        public override object Clone()
        {
            DITextInstructionsScreen di = new DITextInstructionsScreen();
            di.SuspendLayout();
            di.Justification = this.Justification;
            di.LineSpacing = this.LineSpacing;
            di.Phrase = this.Phrase;
            di.PhraseFontColor = this.PhraseFontColor;
            di.PhraseFontFamily = this.PhraseFontFamily;
            di.IImage = IImage.Clone() as IImage;
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, di.IImage.URI);
            di.IImage.Changed += (evt, img, arg) => di.OnImageEvent(evt, img, arg);
            di.AbsoluteBounds = AbsoluteBounds;
            di.ResumeLayout(true);
            return di;
        }
    }
    public class DIKeyedInstructionsScreen : DIMultiLineText
    {
        public DIKeyedInstructionsScreen() : base(DIText.UsedAs.MockItemInstructions) { }
        public DIKeyedInstructionsScreen(Uri uri) : base(uri, DIText.UsedAs.MockItemInstructions) { }
        public override object Clone()
        {
            DIKeyedInstructionsScreen di = new DIKeyedInstructionsScreen();
            di.SuspendLayout();
            di.Justification = this.Justification;
            di.LineSpacing = this.LineSpacing;
            di.Phrase = this.Phrase;
            di.PhraseFontColor = this.PhraseFontColor;
            di.PhraseFontFamily = this.PhraseFontFamily;
            if (di.IImage != null)
                di.IImage.Dispose();
            di.IImage = IImage.Clone() as IImage;
            di.rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, di.IImage.URI);
            di.IImage.Changed += (evt, img, arg) => di.OnImageEvent(evt, img, arg);
            di.AbsoluteBounds = AbsoluteBounds;
            di.ResumeLayout(true);
            return di;
        }
    }
}


