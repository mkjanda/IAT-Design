namespace IATClient.Text
{/*
        public class UsedAs : Enumeration
        {
            public static UsedAs Stimulus = new UsedAs(1, "Stimulus", new Func<Size>(() => { return CIAT.Layout.StimulusSize; }));
            public static UsedAs ContinueInstructions = new UsedAs(2, "ContinueInstructions", new Func<Size>(() => { return CIAT.Layout.ContinueInstructionsSize; }));
            public static UsedAs ResponseKey = new UsedAs(3, "ResponseKey", new Func<Size>(() => { return CIAT.Layout.KeyValueSize; }));
            public static UsedAs Conjunction = new UsedAs(4, "Conjunction", new Func<Size>(() => { return CIAT.Layout.KeyValueSize; }));
            public static UsedAs MockItemInstructions = new UsedAs(5, "MockItemInstructions", new Func<Size>(() => { return CIAT.Layout.MockItemInstructionsRectangle.Size; }));
            public static UsedAs IatBlockInstructions = new UsedAs(6, "IatBlockInstructions", new Func<Size>(() => { return CIAT.Layout.InstructionsSize; }));
            public static UsedAs TextInstructionsScreen = new UsedAs(7, "TextInstructionsScreen", new Func<Size>(() => { return CIAT.Layout.InstructionScreenTextAreaRectangle.Size; }));
            public static UsedAs KeyedInstructionsScreen = new UsedAs(8, "KeyedInstructionsScreen", new Func<Size>(() => { return CIAT.Layout.KeyInstructionScreenTextAreaRectangle.Size; }));
            private UsedAs(int id, String name, Func<Size> retrieveSize) : base(id, name) {
                RetrieveSize = retrieveSize;
            }
            public static IEnumerable<UsedAs> All = new UsedAs[] { Stimulus, ContinueInstructions, ResponseKey, MockItemInstructions, IatBlockInstructions, TextInstructionsScreen, KeyedInstructionsScreen };
            public Func<Size> RetrieveSize { get; private set; }
            public static UsedAs FromString(String str)
            {
                return All.Where((val) => val.Name == str).FirstOrDefault();
            }
        }

    class TextDefinition : IPackagePart
    {
        protected Uri URI;
        protected String phrase = String.Empty, phraseFontFamily;
        protected bool imageStale = true;
        protected readonly object lockObject = new object();
        protected float phraseFontSize = -1;
        protected Color phraseFontColor = Color.White;
        protected Font phraseFont;
        protected UsedAs usedAs;
        public enum EAlignment { left, center, right };
        protected EAlignment alignment;
        private List<DIText> Users = new List<DIText>();
        protected Func<Size> RetrieveBoundingSize;

        public EAlignment Alignment {
            get {
                return alignment;
            }
            set {
                if (alignment == value)
                    return;
                alignment = value;
                Invalidate();
            }
        }

        protected Font PhraseFont
        {
            get
            {
                return phraseFont;
            }
            set
            {
                lock (lockObject)
                {
                    if (phraseFont != null)
                        phraseFont.Dispose();
                    phraseFont = value;
                    Invalidate();
                }
            }
        }

        public Color PhraseFontColor
        {
            get
            {
                return phraseFontColor;
            }
            set
            {
                phraseFontColor = value;
                CIAT.Preferences[usedAs].FontColor = value;
                Invalidate();
            }
        }

        public String PhraseFontFamily
        {
            get
            {
                return phraseFontFamily;
            }
            set
            {
                lock (lockObject)
                {
                    phraseFontFamily = value;
                    CIAT.Preferences[usedAs].FontFamily = value;
                    Font f = new Font(phraseFontFamily, phraseFontSize);
                    PhraseFont = f;
                }
            }
        }

        public float PhraseFontSize
        {
            get
            {
                return phraseFontSize;
            }
            set
            {
                lock (lockObject)
                {
                    phraseFontSize = value;
                    CIAT.Preferences[usedAs].FontSize = value;
                    Font f = new Font(phraseFontFamily, phraseFontSize);
                    PhraseFont = f;
                }
            }
        }

        public String Phrase {
            get {
                return phrase;
            }
            set {
                if (phrase == value)
                    return;
                phrase = value;
                Invalidate();
            }
        }

        private void SetFont(CIATPreferences.CFontPreferences preference)
        {
            lock (lockObject)
            {
                phraseFontFamily = preference.FontFamily;
                phraseFontColor = preference.FontColor;
                phraseFontSize = preference.FontSize;
                Font f = new Font(phraseFontFamily, phraseFontSize);
                PhraseFont = f;
            }
        }

        public TextDefinition(UsedAs usedAs) 
        {
            SetFont(CIAT.Preferences[usedAs]);
            this.usedAs = usedAs;
            alignment = EAlignment.center;
            RetrieveBoundingSize = usedAs.RetrieveSize;
        }

        public TextDefinition(Uri uri) {
            this.URI = uri;
        }
        
        protected void Invalidate() {
            imageStale = true;
            Users.ForEach((u) => u.Invalidate());
        }

        public virtual void Generate(Action<Bitmap> update) {
            if (!Monitor.TryEnter(lockObject))
                return;
            Font f = PhraseFont;
            String str = Phrase;
            Size bSz = RetrieveBoundingSize();
            Monitor.Exit(lockObject);
            Bitmap bmp = new Bitmap(bSz.Width, bSz.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(Brushes.Transparent, new Rectangle(new Point(0, 0), bSz));
            SizeF sz = g.MeasureString(str, f);
            PointF ptDraw;
            switch (Alignment) {
                case EAlignment.left:
                    ptDraw = new PointF(0, (gSz.Height - sz.Height) / 2);
                    break;

                case EAlignment.center:
                    ptDraw = new PointF((bSz.Width - sz.Width) / 2, (bSz.Height - sz.Height) / 2);
                    break;

                case EAlignment.right:
                    ptDraw = new PointF(bSz.Width - sz.Width, (bSz.Height - sz.Height) / 2);
                    break;
            }
            Brush br = new SolidBrush(PhraseFontColor);
            g.DrawString(Phrase, PhraseFont, br, ptDraw);
            g.Dispose();
            update(bmp);
        }

        public void Save() {
            XDocument xDoc = new XDocument();
            XmlWriter xWriter = xDoc.CreateWriter();
            xWriter.WriteStartElement(this.GetType().ToString());
            xWriter.WriteElementString("Phrase", Phrase);
            xWriter.WriteElementString("PhraseFontFamily", PhraseFontFamily);
            xWriter.WriteElementString("PhraseFontSize", PhraseFontSize.ToString());
            xWriter.WriteElementString("PhraseFontColor", PhraseFontColor.ToString());
            xWriter.WriteElementString("Alignment", Alignment.ToString());
            xWriter.WriteEndElement();
            xWriter.WriteEndDocument();
            xWriter.Close();
            Stream s = CIAT.SaveFile.GetPart(this).GetStream(FileMode.Create, FileAccess.Write);
            xDoc.Save(s);
            s.Close();
        }

        public void Load() {
            Stream s = CIAT.SaveFile.GetPart(this).GetStream(FileMode.Open, FileAccess.Read);
            XDocument xDoc = XDocument.Load(s);
            s.Close();
            phrase =  xDoc.Root.Element("Phrase").Value;
            phraseFontFamily = xDoc.Root.Element("PhraseFontFamily").Value;
            phraseFontSize = Convert.ToSingle(xDoc.Root.Element("PhraseFontSize").Value);
            phraseFontColor = Color.FromName(xDoc.Root.Element("PhraseFontColor").Value);
            alignment = (EAlignment)Enum.Parse(typeof(EAlignment), xDoc.Root.Element("Alignment").Value);
            phraseFont = new Font(phraseFontFamily, phraseFontSize);
            Invalidate();
        }
    }*/
}
