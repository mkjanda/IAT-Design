using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using System.Linq;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace IATClient
{
    public class CFontFile
    {
        public class FontData
        {
            private Image img = null;

            public String FamilyName { get; set; }
            public bool IsRegular { get; set; }
            public bool IsSized { get; set; }
            public bool Persist { get; set; }
            public Size szFontLabel { get; set; }

            private MemoryStream _ImageData = new MemoryStream();

            public MemoryStream ImageData
            {
                get
                {
                    _ImageData.Seek(0, SeekOrigin.Begin);
                    return _ImageData;
                }
                set
                {
                    _ImageData = value;
                }
            }

            public FontData()
            {
                szFontLabel = Size.Empty;
                FamilyName = String.Empty;
                IsRegular = false;
                IsSized = false;
                Persist = false;
            }

            public FontData(String familyName)
            {
                szFontLabel = Size.Empty;
                FamilyName = familyName;
                IsRegular = false;
                IsSized = false;
                Persist = false;
            }

            public FontData(String familyName, bool isRegular, bool isSized, byte[] imageData)
            {
                szFontLabel = Size.Empty;
                FamilyName = familyName;
                IsRegular = isRegular;
                IsSized = isSized;
                Persist = false;
                if (imageData != null)
                    ImageData.Write(imageData, 0, imageData.Length);
            }

            public FontData(String familyName, bool isRegular, bool isSized, Size FontLabelSize, byte[] imageData)
            {
                szFontLabel = FontLabelSize;
                FamilyName = familyName;
                IsRegular = isRegular;
                IsSized = isSized;
                Persist = false;
                if (imageData != null)
                    ImageData.Write(imageData, 0, imageData.Length);
            }

            private Image _FontImage = null;
            public Image FontImage
            {
                get
                {
                    if (_FontImage != null)
                        return _FontImage;
                    if (ImageData == null)
                        return null;
                    ImageData.Seek(0, SeekOrigin.Begin);
                    _FontImage = Image.FromStream(ImageData);
                    return _FontImage;
                }
            }
        }

        public class FontItem : IPackagePart
        {
            public Uri URI { get; set; }
            public Type BaseType { get { return typeof(FontItem); } }
            public String MimeType { get { return "text/xml+" + typeof(FontItem).ToString(); } }
            private List<Uri> TDIUris = new List<Uri>();
            public String FamilyName { get; private set; }
            public String Description { get; private set; }
            private byte[] _ImageData = null;
            public byte[] ImageData
            {
                get
                {
                    if (_ImageData != null)
                        return _ImageData;
                    FontData fData = AvailableFonts.Where(fd => fd.FamilyName == FamilyName).FirstOrDefault();
                    if (fData != null)
                        return fData.ImageData.ToArray();
                    return null;
                }
                set
                {
                    _ImageData = value;
                }
            }
            public void AddItemWithFont(DIText tdi)
            {
                TDIUris.Add(tdi.URI);
            }

            public FontItem(Uri u)
            {
                URI = u;
                Load();
                CIAT.SaveFile.Register(this);
            }

            public FontItem(String familyName, String descriptionBase, IEnumerable<int> ndxs, IEnumerable<DIText> textDisplayItems)
            {
                URI = CIAT.SaveFile.CreatePart(BaseType, GetType(), MimeType, ".xml");
                CIAT.SaveFile.Register(this);
                FamilyName = familyName;
                CIAT.SaveFile.CreateRelationship(CIAT.SaveFile.IAT.BaseType, BaseType, CIAT.SaveFile.IAT.URI, URI);
                foreach (DIText tdi in textDisplayItems)
                    AddItemWithFont(tdi);
                if (ndxs == null)
                {
                    Description = descriptionBase;
                    return;
                }
                List<Tuple<int, int>> ndxRanges = new List<Tuple<int, int>>();
                int lower, upper, n = 0;
                List<int> stimNdxs = ndxs.ToList();
                lower = upper = stimNdxs[0];
                while (++n < stimNdxs.Count)
                {
                    if (stimNdxs[n] == stimNdxs[n - 1] + 1)
                        upper = stimNdxs[n];
                    else
                    {
                        ndxRanges.Add(Tuple.Create<int, int>(lower, upper));
                        lower = upper = stimNdxs[n];
                    }
                }
                String desc = descriptionBase, descDetails;
                ndxRanges.Add(Tuple.Create<int, int>(lower, upper));
                if (ndxRanges.Count == 1)
                {
                    if (ndxRanges[0].Item1 == ndxRanges[0].Item2)
                        descDetails = ndxRanges[0].Item1.ToString();
                    else
                        descDetails = String.Format("{0}-{1}", ndxRanges[0].Item1, ndxRanges[0].Item2);
                }
                else if (ndxRanges.Count == 2)
                {
                    if (ndxRanges[0].Item1 == ndxRanges[0].Item2)
                        descDetails = ndxRanges[0].Item1.ToString();
                    else
                        descDetails = String.Format("{0}-{1}", ndxRanges[0].Item1, ndxRanges[0].Item2);
                    if (ndxRanges[1].Item1 == ndxRanges[1].Item2)
                        descDetails += " and " + ndxRanges[1].Item1.ToString();
                    else
                        descDetails += String.Format(", and {0}-{1}", ndxRanges[1].Item1, ndxRanges[1].Item2);
                }
                else
                {
                    descDetails = String.Empty;
                    for (int i = 0; i < ndxRanges.Count - 1; i++)
                    {
                        if (ndxRanges[i].Item1 == ndxRanges[i].Item2)
                            descDetails += String.Format("{0}, ", ndxRanges[i].Item1);
                        else
                            descDetails += String.Format("{0}-{1}, ", ndxRanges[i].Item1, ndxRanges[i].Item2);
                    }
                    if (ndxRanges.Last().Item1 == ndxRanges.Last().Item2)
                        descDetails += String.Format("and {0}", ndxRanges.Last().Item1);
                    else
                        descDetails += String.Format("and {0}-{1}", ndxRanges.Last().Item1, ndxRanges.Last().Item2);
                }
                Description = String.Format(descriptionBase, descDetails);
            }

            public void SetReplacementFontFamily(String newFam)
            {
                foreach (Uri u in TDIUris)
                    (CIAT.SaveFile.GetDI(u) as DIText).PhraseFontFamily = newFam;
            }

            public void Save()
            {
                XDocument xDoc = new XDocument();
                xDoc.Add(new XElement("FontItem", new XElement("FamilyName", FamilyName), new XElement("Description", Description)));
                foreach (Uri u in TDIUris)
                    xDoc.Root.Add(new XElement("TextDisplayItem", u.ToString()));
                if (ImageData != null)
                    xDoc.Root.Add(new XElement("ImageData", Convert.ToBase64String(ImageData.ToArray(), Base64FormattingOptions.None)));
                else
                    xDoc.Root.Add(new XElement("ImageData", Convert.ToBase64String(new byte[] { 0 })));
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
                FamilyName = xDoc.Root.Element("FamilyName").Value;
                Description = xDoc.Root.Element("Description").Value;
                foreach (XElement elem in xDoc.Root.Elements("TextDisplayItem"))
                    TDIUris.Add(new Uri(elem.Value, UriKind.Relative));
                ImageData = Convert.FromBase64String(xDoc.Root.Element("ImageData").Value);
            }
            public void Dispose()
            {
                CIAT.SaveFile.DeleteRelationship(CIAT.SaveFile.IAT.URI, URI);
                CIAT.SaveFile.DeletePart(URI);
            }
        }

        public static readonly float FontSize = 14F;

        private static String FontFilePath = LocalStorage.ActivationFileDirectory + Path.DirectorySeparatorChar + "fonts.xml";

        private static IATConfigMainForm _MainForm;

        static CFontFile()
        {
            _MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
        }


        public static FontData[] AvailableFonts
        {
            get
            {
                return (from cf in CollectedFonts where (cf.IsRegular == true) && (cf.IsSized == true) select cf).ToArray();
            }
        }

        private static IATConfigMainForm MainForm
        {
            get
            {
                while (_MainForm == null)
                {
                    ((Func<Task>)(async () => await Task.Delay(100)))().Wait();
                    _MainForm = (IATConfigMainForm)Application.OpenForms[Properties.Resources.sMainFormName];
                }
                return _MainForm;
            }
        }

        // the maximum font image size
        private static Size _MaxFontSize = new Size();


        public static List<FontData> GetFontDataForFamilies(List<String> families)
        {
            return AvailableFonts.Where(fd => families.Contains(fd.FamilyName)).ToList();
        }

        private static PrivateFontCollection FontCollection = new PrivateFontCollection();
        private readonly static String[] fontFileNames = { "ITCKRIST.TTF", "TektonPro-Bold.otf", "mvboli.ttf", "ARLRDBD.TTF", "FTLTLT.TTF" };
        public FontFamily[] PrivateFontFamilies
        {
            get
            {
                return FontCollection.Families;
            }
        }
        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pvFont, [In] ref uint pcFonts);
        private static void LoadPrivateFonts()
        {
            List<FontFamily> ffList = new List<FontFamily>();
            var fonts = new List<Tuple<String, String, String>>(new Tuple<String, String, String>[] {
                new Tuple<String, String, String>("arial-rounded", Properties.Resources.ArialRoundedFont.Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim(), "ARLRDBD.TTF"),
                new Tuple<String, String, String>("footlight", Properties.Resources.FootlightMTFont.Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim(), "FTLTLT.TTF"),
                new Tuple<String, String, String>("kristin-itc", Properties.Resources.KristinITCFont.Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim(), "ITCKRIST.TTF"),
                new Tuple<String, String, String>("mvboli", Properties.Resources.MVBoliFont.Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim(), "mvboli.ttf"),
                new Tuple<String, String, String>("tekton-pro", Properties.Resources.TektonProFont.Split(new String[] { "\n" }, StringSplitOptions.RemoveEmptyEntries)[1].Trim(), "TektonPro-Bold.otf")
            } as IEnumerable<Tuple<String, String, String>>);
            foreach (var font in fonts)
            {
                uint c = 0;
                byte[] fData = Convert.FromBase64String(font.Item2);
                IntPtr ptr = Marshal.AllocCoTaskMem(fData.Length);
                Marshal.Copy(fData, 0, ptr, fData.Length);
                AddFontMemResourceEx(ptr, (uint)fData.Length, IntPtr.Zero, ref c);
                CFontFile.FontCollection.AddMemoryFont(ptr, fData.Length);
                FontFamily family = CFontFile.FontCollection.Families.Where(ff => !ffList.Contains(ff)).First();
                SurveyItemFormat.EFont.AddCustomFont(family.Name, family, font.Item3, font.Item1);
                ffList.Add(family);
                Marshal.FreeCoTaskMem(ptr);
            }
        }

        public static String[] PrivateFontNames
        {
            get
            {
                return CFontFile.FontCollection.Families.Select(ff => ff.Name).ToArray();
            }
        }


        /// <summary>
        /// gets the maximum font image size
        /// </summary>
        public static Size MaxFontSize
        {
            get
            {
                return _MaxFontSize;
            }
        }

        private static List<FontData> _CollectedFonts = new List<FontData>();
        private static List<FontData> CollectedFonts
        {
            get
            {
                return _CollectedFonts;
            }
        }

        private static List<FontData> MissingFonts = new List<FontData>();

        /// <summary>
        /// gets whether the font file has been generated
        /// </summary>
        public static bool Exists
        {
            get
            {
                return File.Exists(FontFilePath);
            }
        }

        public static bool Loaded { get; private set; } = false;

        protected static Bitmap AbsoluteClip(Bitmap img, Color backColor)
        {
            System.Drawing.Imaging.BitmapData bmpData = img.LockBits(new Rectangle(0, 0, img.Width, img.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr bmpPtr = bmpData.Scan0;
            byte[] bmpBytes = new byte[Math.Abs(bmpData.Stride * bmpData.Height)];
            System.Runtime.InteropServices.Marshal.Copy(bmpPtr, bmpBytes, 0, bmpBytes.Length);
            int top = 0, left = 0, bottom = img.Height - 1, right = img.Width - 1;
            int ctr = 0;
            while ((bmpBytes[ctr] == 0xFF) && (bmpBytes[ctr + 1] == 0xFF) && (bmpBytes[ctr + 2] == 0xFF) && (bmpBytes[ctr + 3] == 0xFF) && (top <= bottom))
            {
                ctr += 4;
                if (ctr % bmpData.Stride == 0)
                    top++;
            }
            ctr = bmpBytes.Length - bmpData.Stride;
            while ((bmpBytes[0] == 0xFF) && (bmpBytes[ctr + 1] == 0xFF) && (bmpBytes[ctr + 2] == 0xFF) && (bmpBytes[ctr + 3] == 0xFF) && (bottom >= top))
            {
                ctr += 4;
                if (ctr % bmpData.Stride == 0)
                    ctr = --bottom * bmpData.Stride;
            }
            ctr = 0;
            while ((bmpBytes[ctr] == 0xFF) && (bmpBytes[ctr + 1] == 0xFF) && (bmpBytes[ctr + 2] == 0xFF) && (bmpBytes[ctr + 3] == 0xFF) && (left <= right))
            {
                ctr += bmpData.Stride;
                if (ctr >= bmpBytes.Length)
                    ctr = ++left << 2;
            }
            ctr = bmpData.Stride - 4;
            while ((bmpBytes[ctr] == 0xFF) && (bmpBytes[ctr + 1] == 0xFF) && (bmpBytes[ctr + 2] == 0xFF) && (bmpBytes[ctr + 3] == 0xFF) && (right >= left))
            {
                ctr += bmpData.Stride;
                if (ctr > bmpData.Height * bmpData.Stride) 
                    ctr = --right << 2;
            }
            img.UnlockBits(bmpData);
            if ((top > bottom) || (left > right))
                return null;
            Bitmap result = new Bitmap(right - left + 1, bottom - top + 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(result))
            {
                Brush br = new SolidBrush(backColor);
                g.FillRectangle(br, new Rectangle(0, 0, result.Width, result.Height));
                g.DrawImage(img, 0, 0, new Rectangle(left, top, left + right - left + 1, top + bottom - top + 1), GraphicsUnit.Pixel);
                br.Dispose();
            }
            return result;
        }


        private static FontData GenerateFontImage(String familyName)
        {
            FontData fd = new FontData(familyName);
            fd.IsRegular = true;
            Font font = new Font(familyName, CFontFile.FontSize);

            // measure the font family name and make sure it's not too big
            if ((font.Height <= 100) && (familyName != String.Empty))
            {
                fd.IsSized = true;
                // increment the max font name size if necessary
                Bitmap fontBmp = new Bitmap(1000, 100, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                Graphics g = Graphics.FromImage(fontBmp);
                Brush EraseBrush = new SolidBrush(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                g.FillRectangle(EraseBrush, new Rectangle(0, 0, fontBmp.Width, fontBmp.Height));
                Brush TextBrush = new SolidBrush(Color.FromArgb(0xFF, Color.Black));
                g.DrawString(familyName, font, TextBrush, 0, 0);
                g.Dispose();
                EraseBrush.Dispose();
                TextBrush.Dispose();
                Bitmap clippedFontBmp = AbsoluteClip(fontBmp, Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                fontBmp.Dispose();
                if (clippedFontBmp != null)
                {
                    clippedFontBmp.MakeTransparent(Color.FromArgb(0xFF, 0xFF, 0xFF, 0xFF));
                    fd.szFontLabel = clippedFontBmp.Size;
                    clippedFontBmp.Save(fd.ImageData, System.Drawing.Imaging.ImageFormat.Png);
                    fd.ImageData.Seek(0, SeekOrigin.Begin);
                    clippedFontBmp.Dispose();
                }
                else
                {
                    fd.ImageData.Dispose();
                    fd.ImageData = null;
                }
            }
            // dispose of the font
            font.Dispose();
            return fd;
        }


        public static void Generate()
        {
            _MaxFontSize = Size.Empty;
            MainForm.BeginInvoke(new Action<EventHandler, IATConfigMainForm.EProgressBarUses>(MainForm.BeginProgressBarUse), null, IATConfigMainForm.EProgressBarUses.GeneratingFontFile);
            MainForm.BeginInvoke(new Action<String>(MainForm.SetStatusMessage), Properties.Resources.sCollectingInstalledFonts);
            FontFamily[] FamilyAry = FontFamily.Families;
            XDocument xDoc = new XDocument();
            MainForm.BeginInvoke(new Action(MainForm.ResetProgress));
            MainForm.BeginInvoke(new Action<int, int>(MainForm.SetProgressRange), 0, FamilyAry.Length);
            MainForm.BeginInvoke(new Action<String>(MainForm.SetStatusMessage), Properties.Resources.sFontGenerationMessage);
            _CollectedFonts.Clear();

            // write the font names and images
            for (int ctr = 0; ctr < FamilyAry.Length; ctr++)
            {
                FontData fd = null;
                fd = GenerateFontImage(FamilyAry[ctr].Name);
                _CollectedFonts.Add(fd);
                MainForm.Invoke(new Action<int>(MainForm.ProgressIncrement), 1);
            }

            List<XElement> xElems = new List<XElement>();
            foreach (FontData fd in CollectedFonts)
            {
                xElems.Add(new XElement("Font", new XElement("FamilyName", fd.FamilyName), new XElement("IsRegular", fd.IsRegular), new XElement("IsSized", fd.IsSized),
                    new XElement("FontLabelWidth", fd.szFontLabel.Width), new XElement("FontLabelHeight", fd.szFontLabel.Height),
                    new XElement("ImageData", (fd.IsRegular && fd.IsSized && (fd.ImageData != null)) ? Convert.ToBase64String(fd.ImageData.ToArray()) : "No Image Data")));
            }

            XDocument fontDoc = new XDocument(new XElement("FontData", new XAttribute("MaxWidth", _MaxFontSize.Width.ToString()), new XAttribute("MaxHeight", _MaxFontSize.Height.ToString()),
                from elem in xElems select elem));
            fontDoc.Save(FontFilePath);
            MainForm.Invoke(new Action(MainForm.EndProgressBarUse));
        }

        /// <summary>
        /// Loads the font data file
        /// </summary>
        public static XDocument Load()
        {
            XDocument fontDoc = null;
            try
            {
                fontDoc = XDocument.Load(FontFilePath);
                _MaxFontSize.Width = Convert.ToInt32(fontDoc.Root.Attribute("MaxWidth").Value);
                _MaxFontSize.Height = Convert.ToInt32(fontDoc.Root.Attribute("MaxHeight").Value);
                CollectedFonts.Clear();
                if (CVersion.Compare(new CVersion(LocalStorage.Activation[LocalStorage.Field.Version]), new CVersion("1.1.0.41")) > 0)
                {
                    var elems = from elem in fontDoc.Descendants(XName.Get("Font"))
                                select new FontData(elem.Element("FamilyName").Value, Convert.ToBoolean(elem.Element("IsRegular").Value), Convert.ToBoolean(elem.Element("IsSized").Value),
                                    new Size(Convert.ToInt32(elem.Element("Sz11PtWidth").Value), Convert.ToInt32(elem.Element("Sz11PtHeight").Value)),
                                    (elem.Element("ImageData").Value == "No Image Data") ? null : Convert.FromBase64String(elem.Element("ImageData").Value));
                    foreach (FontData fontElem in elems)
                        _CollectedFonts.Add(fontElem);
                }
                else
                {
                    var elems = from elem in fontDoc.Descendants(XName.Get("Font"))
                                select new FontData(elem.Element("FamilyName").Value, Convert.ToBoolean(elem.Element("IsRegular").Value), Convert.ToBoolean(elem.Element("IsSized").Value),
                                    new Size(Convert.ToInt32(elem.Element("FontLabelWidth").Value), Convert.ToInt32(elem.Element("FontLabelHeight").Value)),
                                    (elem.Element("ImageData").Value == "No Image Data") ? null : Convert.FromBase64String(elem.Element("ImageData").Value));
                    foreach (FontData fontElem in elems)
                        _CollectedFonts.Add(fontElem);
                }
                Loaded = true;
                return fontDoc;
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// Validates the data read from the font data file to ensure all fonts in it are available on the system
        /// </summary>
        /// <returns>"true" if all fonts in the file are available, "false" otherwise</returns>
        public static bool Validate(XDocument fontDoc)
        {
            if (CVersion.Compare(new CVersion(LocalStorage.Activation[LocalStorage.Field.Version]), new CVersion("1.1.0.41")) > 0)
                return false;
            try
            {
                // get the available system fonts
                FontFamily[] FamilyAry = FontFamily.Families.ToArray();
                var installedFamilies = from ff in FamilyAry select ff.Name;
                var recordedFamilies = from cf in CollectedFonts select cf.FamilyName;
                String[] newFonts = installedFamilies.Except(recordedFamilies).ToArray();
                String[] missingFonts = recordedFamilies.Except(installedFamilies).ToArray();
                if ((newFonts.Length == 0) && (missingFonts.Length == 0))
                    return true;
                MissingFonts = CollectedFonts.Where((cf) => missingFonts.Contains(cf.FamilyName)).ToList();
                List<FontData> remainingFontCollection = CollectedFonts.Where(cf => !missingFonts.Contains(cf.FamilyName)).ToList();
                //                List<FontData> remainingFontCollection = CollectedFonts.SelectMany<FontData, FontData>((cf) => from name in missingFonts.AsEnumerable<String>() where not cf.FamilyName == name select cf).ToList<FontData>();
                var missingFontEntries = fontDoc.Root.Elements(XName.Get("Font")).Where((xElem) => missingFonts.Contains(xElem.Elements(XName.Get("FamilyName")).First().Value)).ToList();
                //                    <XElement, IEnumerable<XElement>>((fnElem) => from newFont in CollectedFonts.Except<FontData>(remainingFontCollection) where newFont.FamilyName == fnElem.Value select fnElem.Parent).ToList();
                foreach (XElement fe in missingFontEntries)
                {
                    fe.Remove();
                }
                List<FontData> NewFonts = new List<FontData>();
                List<XElement> xElems = new List<XElement>();
                foreach (String famName in newFonts.AsEnumerable())
                {
                    FontData newFont;
                    newFont = GenerateFontImage(famName);
                    NewFonts.Add(newFont);
                    xElems.Add(new XElement("Font", new XElement("FamilyName", newFont.FamilyName), new XElement("IsRegular", newFont.IsRegular), new XElement("IsSized", newFont.IsSized),
                        new XElement("FontLabelWidth", newFont.szFontLabel.Width), new XElement("FontLabelHeight", newFont.szFontLabel.Height),
                        new XElement("ImageData", (newFont.IsRegular && newFont.IsSized) ? Convert.ToBase64String(newFont.ImageData.ToArray()) : "No Image Data")));
                }
                fontDoc.Root.Add(xElems);
                List<XElement> fontElems = fontDoc.Root.Elements(XName.Get("Font")).OrderBy((elem) => elem.Element(XName.Get("FamilyName"))).ToList();
                fontDoc.Root.RemoveNodes();
                fontDoc.Root.Add(fontElems);
                CollectedFonts.Clear();
                CollectedFonts.AddRange(remainingFontCollection);
                CollectedFonts.AddRange(NewFonts);
                CollectedFonts.Sort((fd1, fd2) => String.Compare(fd1.FamilyName, fd2.FamilyName));
                fontDoc.Save(Properties.Resources.sFontFileName);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }

        /// <summary>
        /// The default constructor.  Generates the font file if necesssary, otherwise loads it, validates it,
        /// and regenerates it if necessary
        /// </summary>
        public static bool TryLoad()
        {
            LoadPrivateFonts();
            XDocument fontDoc = null;
            CollectedFonts.Clear();
            fontDoc = Load();
            if (fontDoc == null)
                return false;
            if (!Validate(fontDoc))
                return false;
            Loaded = true;
            return true;
        }

        public static void Save()
        {
            XDocument fontDoc = Load();
            foreach (FontData fd in MissingFonts.Where(fd => fd.Persist))
            {
                fontDoc.Root.Add(new XElement("Font", new XElement("FamilyName", fd.FamilyName), new XElement("IsRegular", fd.IsRegular), new XElement("IsSized", fd.IsSized),
                    new XElement("FontLabelWidth", fd.szFontLabel.Width), new XElement("FontLabelHeight", fd.szFontLabel.Height),
                    new XElement("ImageData", (fd.IsRegular && fd.IsSized && (fd.ImageData != null)) ? Convert.ToBase64String(fd.ImageData.ToArray()) : "No Image Data")));
            }
            fontDoc.Save(Properties.Resources.sFontFileName);
        }

        /// <summary>
        /// Disposes of the CFontFile object
        /// </summary>
        public static void Dispose()
        {
            for (int ctr = 0; ctr < CollectedFonts.Count; ctr++)
            {
                CollectedFonts[ctr].ImageData.Dispose();
                CollectedFonts[ctr].FontImage.Dispose();
            }
        }
    }
}
