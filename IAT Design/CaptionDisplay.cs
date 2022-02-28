using System;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Linq;
using Saxon.Api;
namespace IATClient
{
    class CaptionDisplay : SurveyItemDisplay
    {
        private ToolStrip CaptionFormatStrip;
        private ToolStripTextBox Caption;
        private ToolStripLabel CaptionLabel, FontSizeLabel, FontColorLabel, BackColorLabel, BorderColorLabel, BorderWidthLabel;
        private ToolStripDropDownButton FontSizeDrop, FontColorDrop, BackColorDrop, BorderColorDrop, BorderWidthDrop, FontFamilyDrop;
        private WebBrowser CaptionPreview = new WebBrowser();
        private static Font DropMenuFont { get; set; } = new Font(System.Drawing.SystemFonts.DialogFont.FontFamily, 7.5F);

        private static float[] CaptionFontSizes = { 32, 36, 42, 48, 56, 64, 72 };

        private static int[] BorderWidths = { 0, 4, 8, 12, 16 };


        public class ColorSelection
        {
            private static readonly SizeF RectSize;
            static ColorSelection()
            {
                using (Font f = new Font(SystemFonts.DialogFont.FontFamily, 7.5F))
                {
                    RectSize = new SizeF(4F / 3F * f.Height, f.Height);
                }
            }
            public Color Color { get; private set; }
            public String Name { get; private set; }
            public int R { get { return Color.R; } }
            public int G { get { return Color.G; } }
            public int B { get { return Color.B; } }

            public Image ColorRect { get; private set; }
            public ColorSelection(String name, Color c)
            {
                Name = name;
                Color = c;
                Bitmap bmp = new Bitmap((int)RectSize.Width, (int)RectSize.Height);
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (Brush br = new SolidBrush(c))
                        g.FillRectangle(br, 0, 0, bmp.Width, bmp.Height);
                    g.DrawRectangle(Pens.Black, 0, 0, bmp.Width, bmp.Height);
                }
                ColorRect = bmp;
            }
        }
        /*
        public class ColorCollection : IEnumerable<ColorSelection>
        {
            public static ColorSelection Black = new ColorSelection("Black", Color.Black);
            public static ColorSelection Blue = new ColorSelection("Blue", Color.Blue);
            public static ColorSelection Chartreuse = new ColorSelection("Chartreuse", Color.Chartreuse);
            public static ColorSelection Gray = new ColorSelection("Gray", Color.Gray);
            public static ColorSelection Green = new ColorSelection("Black", Color.Green);
            public static ColorSelection Indigo = new ColorSelection("Indigo", Color.Indigo);
            public static ColorSelection Pink = new ColorSelection("Pink", Color.Pink);
            public static ColorSelection Red = new ColorSelection("Red", Color.Red);
            public static ColorSelection White = new ColorSelection("White", Color.White);
            public static ColorSelection UltraViolet = new ColorSelection("Ultra Violet", Color.FromArgb(0x5F, 0x4B, 0x8B));
            public static ColorSelection TurkishSea = new ColorSelection("Turkish Sea", Color.FromArgb(0x19, 0x51, 0x90));
            public static ColorSelection BlueMoon = new ColorSelection("Blue Moon", Color.FromArgb(54, 134, 160));
            public static ColorSelection CallisteGreen = new ColorSelection("Calliste Green", Color.FromArgb(117, 122, 78));
            public static ColorSelection BlackenedPearl = new ColorSelection("Blackened Pearl", Color.FromArgb(77, 75, 80));
            public static ColorSelection PaleGold = new ColorSelection("Pale Gold", Color.FromArgb(189, 152, 101));
            public static ColorSelection Silver = new ColorSelection("Silver", Color.FromArgb(166, 169, 170));
            public static ColorSelection XenonBlue = new ColorSelection("Xenon Blue", Color.FromArgb(117, 122, 78));
            public static ColorSelection HawaiianSurf = new ColorSelection("Hawaiian Surf", Color.FromArgb(77, 75, 80));
            public static ColorSelection Citrus = new ColorSelection("Pale Gold", Color.FromArgb(189, 152, 101));
            public static ColorSelection Raspberry = new ColorSelection("Raspberry", Color.FromArgb(211, 46, 94));
            public static ColorSelection Oriole = new ColorSelection("Oriole", Color.FromArgb(255, 121, 19));
            public static ColorSelection Bodacious = new ColorSelection("Bodacious", Color.FromArgb(183, 107, 163));
            public static ColorSelection Bamboo = new ColorSelection("Bamboo", Color.FromArgb(210, 176, 76));
            public static ColorSelection Butterum = new ColorSelection("Butterum", Color.FromArgb(198, 143, 101));

            public static IEnumerable<ColorSelection> All = new ColorSelection[]
            {
                Black, Blue, Chartreuse, Gray, Green, Indigo, Pink, Red, White, UltraViolet, TurkishSea, BlueMoon, CallisteGreen,
                BlackenedPearl, PaleGold, Silver, XenonBlue, HawaiianSurf, Citrus, Raspberry, Oriole, Bodacious, Bamboo, Butterum
            };

            public static ColorSelection Parse(String str)
            {
                return All.Where(c => c.Name == str).FirstOrDefault();
            }

            public ColorCollection() { }

            public ColorSelection this[int ndx]
            {
                get
                {
                    return All.Where((cs, n) => ndx == n).FirstOrDefault();
                }
            }

            public IEnumerator<ColorSelection> GetEnumerator()
            {
                return All.GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }

            static public int IndexOf(ColorSelection cs)
            {
                return All.Select((c, ndx) => new Tuple<ColorSelection, int>(c, ndx)).Where(tup => tup.Item1 == cs).Select(tup => tup.Item2).FirstOrDefault();
            }

            public int Length { get { return All.Count(); } }

        }
        */
        private String PreviewFilename { get; } = LocalStorage.ActivationFileDirectory + Path.DirectorySeparatorChar + "caption-preview.html";
        private CSurveyItem _SurveyItem = null;
        public override CSurveyItem SurveyItem
        {
            get
            {
                var c = _SurveyItem as CSurveyCaption;
                c.FontSize = (int)FontSize;
                c.BorderWidth = BorderWidth;
                c.FontColor = FontColor.Value;
                c.BackColor = CaptionBackColor.Value;
                c.BorderColor = BorderColor.Value;
                c.FontName = FontFamilyDrop.Text;
                c.Text = Caption.Text;
                return _SurveyItem;
            }
            set
            {
                if (_SurveyItem == null)
                    _SurveyItem = value;
                CSurveyCaption sc = (CSurveyCaption)value;
                FontSize = sc.FontSize;
                BorderWidth = sc.BorderWidth;
                FontColor = sc.FontColor;
                CaptionBackColor = sc.BackColor;
                BorderColor = sc.BorderColor;
                FontFamilyDrop.Text = sc.FontName;
                Caption.Text = sc.Text;
                if (IsHandleCreated)
                {
                    RecalcSize(false);
                    LayoutControl();
                }
            }
        }

        public override bool IsUnique
        {
            get
            {
                return false;
            }
            set
            {
            }
        }


        public override bool Selected
        {
            get
            {
                return false;
            }
            set
            {

            }
        }
        /*
        private int FontColorNdx
        {
            get
            {
                return _FontColorNdx;
            }
            set
            {
                if (_FontColorNdx != value)
                {
                    FontColorDrop.Image = FontColorDrop.DropDownItems[value].Image;
                    FontColorDrop.Text = FontColorDrop.DropDownItems[value].Text;
                    _FontColorNdx = value;
                    CaptionEdit.ForeColor = FontColor;
                }
            }
        }
          */


        private Color? FontColor
        {
            get
            {
                return (FontColorDrop.Image.Tag as NamedColor).Color;
            }
            set
            {
                if (!value.HasValue)
                    return;
                var namedColor = NamedColor.GetNamedColor(value.Value);
                FontColorDrop.Image = FontColorDrop.DropDownItems[namedColor.Name].Image;
            }
        }

        private System.Drawing.Color? CaptionBackColor
        {
            get
            {
                return (BackColorDrop.Image.Tag as NamedColor).Color;
            }
            set
            {
                if (!value.HasValue)
                    return;
                var namedColor = NamedColor.GetNamedColor(value.Value);
                BackColorDrop.Image = BackColorDrop.DropDownItems[namedColor.Name].Image;
            }
        }

        private System.Drawing.Color? BorderColor
        {
            get
            {
                return (BorderColorDrop.Image.Tag as NamedColor).Color;
            }
            set
            {
                if (!value.HasValue)
                    return;
                var namedColor = NamedColor.GetNamedColor(value.Value);
                BorderColorDrop.Image = BorderColorDrop.DropDownItems[namedColor.Name].Image;
            }
        }

        private float FontSize
        {
            get
            {
                return Convert.ToSingle(FontSizeDrop.Text.Substring(0, FontSizeDrop.Text.Length - 2));
            }
            set
            {
                FontSizeDrop.Text = String.Format("{0}pt", value);
            }
        }

        private int BorderWidth
        {
            get
            {
                return Convert.ToInt32(BorderWidthDrop.Text.Substring(0, BorderWidthDrop.Text.Length - 2));
            }
            set
            {
                BorderWidthDrop.Text = String.Format("{0}px", value);
            }
        }

        private static Processor xsltProcessor;
        private static XsltCompiler xsltCompiler;
        private static XsltExecutable xsltExecutable;
        private static XsltTransformer xsltTransformer;
        static CaptionDisplay()
        {
            try
            {
                xsltProcessor = new Processor();
                xsltCompiler = xsltProcessor.NewXsltCompiler();
                xsltExecutable = xsltCompiler.Compile(new StringReader(Properties.Resources.GenerateCaption));
                xsltTransformer = xsltExecutable.Load();
            }
            catch (Exception ex)
            {
                int n = 1;
            }
        }

        protected void LayoutControl()
        {
            XDocument xDoc = new XDocument();
            xDoc.Add(new XElement("Caption", new XElement("Width", ClientSize.Width.ToString()), new XElement("Height", CaptionPreview.Height.ToString()),
                new XElement("Text", Caption.Text), new XElement("FontSize", FontSize.ToString()),
                new XElement("BorderWidth", BorderWidth.ToString()), new XElement("BackColorR", CaptionBackColor.Value.R.ToString("x2")),
                new XElement("BackColorG", CaptionBackColor.Value.G.ToString("x2")), new XElement("BackColorB", CaptionBackColor.Value.B.ToString("x2")),
                new XElement("FontColorR", FontColor.Value.R.ToString("x2")), new XElement("FontColorG", FontColor.Value.G.ToString("x2")),
                new XElement("FontColorB", FontColor.Value.B.ToString("x2")), new XElement("BorderColorR", BorderColor.Value.R.ToString("x2")),
                new XElement("BorderColorG", BorderColor.Value.G.ToString("x2")), new XElement("BorderColorB", BorderColor.Value.B.ToString("x2")),
                new XElement("FontFamily", PrivateFont.Fonts.Where(f => f.DisplayName == FontFamilyDrop.Text).Select(f => f.DisplayName).FirstOrDefault())));
            MemoryStream memStream = new MemoryStream();
            xDoc.Save(memStream);
            memStream.Seek(0, SeekOrigin.Begin);
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(memStream);
            var txtWriter = new StringWriter();
            XdmNode inputNode = xsltProcessor.NewDocumentBuilder().Build(xmlDoc.DocumentElement);
            Serializer ser = xsltProcessor.NewSerializer(txtWriter);
            xsltTransformer.InitialContextNode = inputNode;
            CaptionPreview.BackColor = Color.Green;
            xsltTransformer.Run(ser);
            File.WriteAllText(PreviewFilename, "<!DOCTYPE html>\r\n" + txtWriter.ToString().Replace("\n", "\r\n"));
            CaptionPreview.Navigate(new Uri(PreviewFilename).AbsoluteUri);
            //            CaptionPreview.DocumentText = "<!DOCTYPE html>" + txtWriter.ToString();
        }

        public void RefreshCaptionPreview()
        {
            //      CaptionPreview.DocumentText = CaptionPreview.DocumentText;
            //    Refresh();
        }

        public CaptionDisplay()
        {
            CaptionFormatStrip = null;
            FontSizeLabel = FontColorLabel = BackColorLabel = BorderColorLabel = BorderWidthLabel = null;
            FontSizeDrop = FontColorDrop = BackColorDrop = BorderColorDrop = BorderWidthDrop = null;
            CaptionFormatStrip = new ToolStrip();
            CaptionFormatStrip.SuspendLayout();

            // init tool strip
            CaptionFormatStrip.Location = new Point(0, 0);
            CaptionFormatStrip.Width = ClientSize.Width;

            CaptionLabel = new ToolStripLabel("Caption:");
            CaptionLabel.Font = DropMenuFont;
            CaptionLabel.Size = TextRenderer.MeasureText(CaptionLabel.Text, DropMenuFont);
            CaptionFormatStrip.Items.Add(CaptionLabel);

            Caption = new ToolStripTextBox();
            Caption.BorderStyle = BorderStyle.FixedSingle;
            Caption.AutoSize = false;
            Caption.Width = 300;
            Caption.TextChanged += (sender, args) => LayoutControl();
            CaptionFormatStrip.Items.Add(Caption);

            FontColorLabel = new ToolStripLabel(Properties.Resources.sCaptionFontColorLabel);
            FontColorLabel.Font = DropMenuFont;
            FontColorLabel.Size = TextRenderer.MeasureText(FontColorLabel.Text, DropMenuFont);
            CaptionFormatStrip.Items.Add(FontColorLabel);

            FontFamilyDrop = new ToolStripDropDownButton();
            FontFamilyDrop.DropDownItems.AddRange(PrivateFont.Fonts.Select(f => new ToolStripMenuItem(f.DisplayName, null, (sender, args) =>
            {
                FontFamilyDrop.Text = (sender as ToolStripMenuItem).Text;
                RecalcSize(false);
            }, f.FamilyName)).ToArray());
            FontFamilyDrop.Text = PrivateFont.JosefinSans.DisplayName;
            FontFamilyDrop.TextChanged += (sender, args) => { RecalcSize(false); LayoutControl(); };
            CaptionFormatStrip.Items.Add(FontFamilyDrop);
            FontColorDrop = new ToolStripDropDownButton();
            FontColorDrop.Font = DropMenuFont;
            FontColorDrop.TextAlign = ContentAlignment.MiddleRight;
            FontColorDrop.ImageAlign = ContentAlignment.MiddleLeft;
            CaptionFormatStrip.Items.Add(FontColorDrop);
            FontSizeDrop = new ToolStripDropDownButton();
            foreach (var sz in CaptionFontSizes)
            {
                FontSizeDrop.DropDownItems.Add(new ToolStripMenuItem(sz.ToString() + "pt", null, (sender, args) =>
                {
                    var menuItem = sender as ToolStripMenuItem;
                    FontSize = Convert.ToSingle(menuItem.Name);
                    RecalcSize(false);
                }, sz.ToString()));
            }
            FontSizeDrop.Font = DropMenuFont;
            FontSizeDrop.TextAlign = ContentAlignment.MiddleLeft;
            FontSizeDrop.Text = CaptionFontSizes[2].ToString() + "pt";
            CaptionFormatStrip.Items.Add(FontSizeDrop);

            BackColorDrop = new ToolStripDropDownButton();
            BackColorLabel = new ToolStripLabel(Properties.Resources.sCaptionBackColorLabel);
            BackColorLabel.Font = DropMenuFont;
            BackColorLabel.Size = TextRenderer.MeasureText(BackColorLabel.Text, DropMenuFont);
            CaptionFormatStrip.Items.Add(BackColorLabel);
            BackColorDrop.Font = DropMenuFont;
            BackColorDrop.TextAlign = ContentAlignment.MiddleRight;
            BackColorDrop.ImageAlign = ContentAlignment.MiddleLeft;
            CaptionFormatStrip.Items.Add(BackColorDrop);

            BorderColorLabel = new ToolStripLabel(Properties.Resources.sCaptionBorderColorLabel);
            BorderColorLabel.Font = DropMenuFont;
            BorderColorLabel.Size = TextRenderer.MeasureText(BorderColorLabel.Text, DropMenuFont);
            CaptionFormatStrip.Items.Add(BorderColorLabel);
            BorderColorDrop = new ToolStripDropDownButton();
            BorderColorDrop.Font = DropMenuFont;
            BorderColorDrop.TextAlign = ContentAlignment.MiddleRight;
            BorderColorDrop.ImageAlign = ContentAlignment.MiddleLeft;
            CaptionFormatStrip.Items.Add(BorderColorDrop);

            var rectSize = new Size((int)(DropMenuFont.Height * 4F / 3F), DropMenuFont.Height);
            var colorRect = new Bitmap(rectSize.Width, rectSize.Height);
            foreach (var nc in NamedColor.All)
            {
                using (Graphics g = Graphics.FromImage(colorRect))
                {
                    var br = new SolidBrush(nc.Color);
                    g.FillRectangle(br, new Rectangle(new Point(0, 0), rectSize));
                    br.Dispose();
                }
                colorRect.Tag = nc;
                var backColorImage = new Bitmap(colorRect) { Tag = nc };
                var borderColorImage = new Bitmap(colorRect) { Tag = nc };
                var fontColorImage = new Bitmap(colorRect) { Tag = nc };
                BackColorDrop.DropDownItems.Add(new ToolStripMenuItem(nc.Name, backColorImage, (sender, args) =>
                {
                    var menuItem = sender as ToolStripMenuItem;
                    CaptionBackColor = NamedColor.GetNamedColor(menuItem.Name).Color;
                    LayoutControl();
                }, nc.Name));
                BorderColorDrop.DropDownItems.Add(new ToolStripMenuItem(nc.Name, borderColorImage, (sender, args) =>
                {
                    var menuItem = sender as ToolStripMenuItem;
                    BorderColor = NamedColor.GetNamedColor(menuItem.Name).Color;
                    LayoutControl();
                }, nc.Name));
                FontColorDrop.DropDownItems.Add(new ToolStripMenuItem(nc.Name, fontColorImage, (sender, args) =>
                {
                    var menuItem = sender as ToolStripMenuItem;
                    FontColor = NamedColor.GetNamedColor(menuItem.Name).Color;
                    LayoutControl();
                }, nc.Name));
            }
            colorRect.Dispose();
            FontColorDrop.Image = FontColorDrop.DropDownItems[NamedColor.UltraViolet.Name].Image;
            BackColorDrop.Image = FontColorDrop.DropDownItems[NamedColor.StarSapphire.Name].Image;
            BorderColorDrop.Image = FontColorDrop.DropDownItems[NamedColor.Raspberry.Name].Image;

            BorderWidthLabel = new ToolStripLabel(Properties.Resources.sCaptionBorderWidthLabel);
            BorderWidthLabel.Font = DropMenuFont;
            BorderWidthLabel.Size = TextRenderer.MeasureText(BorderWidthLabel.Text, DropMenuFont);
            CaptionFormatStrip.Items.Add(BorderWidthLabel);

            BorderWidthDrop = new ToolStripDropDownButton();
            foreach (var w in BorderWidths)
            {
                BorderWidthDrop.DropDownItems.Add(new ToolStripMenuItem(w.ToString() + "px", null, (sender, args) =>
                {
                    var menuItem = sender as ToolStripMenuItem;
                    BorderWidth = Convert.ToInt32(menuItem.Name);
                    RecalcSize(false);
                }, w.ToString()));
            }
            BorderWidthDrop.Font = DropMenuFont;
            BorderWidthDrop.TextAlign = ContentAlignment.MiddleLeft;
            BorderWidthDrop.Text = BorderWidths[2].ToString() + "px";
            CaptionFormatStrip.Items.Add(BorderWidthDrop);


            CaptionFormatStrip.ResumeLayout();
            Controls.Add(CaptionFormatStrip);
            CaptionFormatStrip.Visible = true;
            Controls.Add(CaptionPreview);
            CaptionPreview.Location = new Point(0, CaptionFormatStrip.Height);
            CaptionPreview.Width = ClientSize.Width;
            CaptionPreview.ResumeLayout();
            Caption.Enter += (sender, args) =>
            {
                if (Caption.Text == Properties.Resources.sSurveyCaptionDefaultText)
                    Caption.Text = String.Empty;
            };
            Caption.Leave += (sender, args) =>
            {
                if (Caption.Text == String.Empty)
                    Caption.Text = Properties.Resources.sSurveyCaptionDefaultText;
            };
        }

        public override Task<int> RecalcSize(bool recalcChildren)
        {
            this.Invoke(new Action(() =>
            {
                if (recalcChildren)
                    this.Width = Parent.Width;
                int captionPreviewHeight = (int)((double)PrivateFont.Fonts.Where(f => f.DisplayName == FontFamilyDrop.Text).FirstOrDefault().GetFontHeight(FontSize)) + BorderWidth;
                captionPreviewHeight = captionPreviewHeight * 7 / 5;
                CaptionFormatStrip.Location = new Point(0, 0);
                CaptionFormatStrip.Width = ClientSize.Width;
                CaptionPreview.Location = new Point(0, CaptionFormatStrip.ClientRectangle.Bottom);
                CaptionPreview.Size = new Size(ClientSize.Width, captionPreviewHeight);
                this.Size = new Size(ClientSize.Width, CaptionFormatStrip.Height + CaptionPreview.Height);
                LayoutControl();
                if (!recalcChildren)
                    Task.Run(() => (Parent as SurveyDisplay)?.RecalcSize(false));
            }));
            return Task.Run(() => Height);
        }

        protected override void OnActivate(bool BecomingActive)
        {
            base.OnActivate(BecomingActive);
        }

        protected override void SurveyItemDisplay_Paint(object sender, PaintEventArgs e)
        {
            Brush borderBr = new SolidBrush(BorderColor.Value);
            if (BorderWidth != 0)
                e.Graphics.FillRectangle(borderBr, new Rectangle(new Point(0, ClientRectangle.Bottom - BorderWidth), new Size(ClientSize.Width, ClientRectangle.Bottom)));
            borderBr.Dispose();
        }
    }
}
