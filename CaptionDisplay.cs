using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Threading;

namespace IATClient
{
    class CaptionDisplay : SurveyItemDisplay
    {
        private ToolStrip CaptionFormatStrip;
        private ToolStripLabel FontSizeLabel, FontColorLabel, BackColorLabel, BorderColorLabel, BorderWidthLabel;
        private ToolStripDropDownButton FontSizeDrop, FontColorDrop, BackColorDrop, BorderColorDrop, BorderWidthDrop;
        private TextBox CaptionEdit;
        private int _FontColorNdx, _BackColorNdx, _BorderColorNdx, _FontSizeNdx, _BorderWidthNdx;
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

        private readonly ColorCollection Colors = new ColorCollection();

        private CSurveyItem _SurveyItem = null;
        public override CSurveyItem SurveyItem
        {
            get
            {
                if (_SurveyItem == null)
                {
                    CSurveyCaption sc = new CSurveyCaption();
                    int? ndx = CaptionFontSizes.Select((sz, n) => new Tuple<float, int>(sz, n)).Where(t => t.Item1 == sc.FontSize).FirstOrDefault()?.Item2;
                    FontSizeNdx = ndx.HasValue ? ndx.Value : 2;
                    ndx = BorderWidths.Select((sz, n) => new Tuple<int, int>(sz, n)).Where(t => t.Item1 == sc.BorderWidth).FirstOrDefault()?.Item2;
                    BorderWidthNdx = ndx.HasValue ? ndx.Value : 2;
                    ndx = Colors.Select((c, n) => new Tuple<Color, int>(c.Color, n)).Where(t => (t.Item1.R == sc.FontColor.R) && (t.Item1.G == sc.FontColor.G) && (t.Item1.B == sc.FontColor.B)).FirstOrDefault()?.Item2;
                    FontColorNdx = ndx.HasValue ? ndx.Value : 0;
                    ndx = Colors.Select((c, n) => new Tuple<Color, int>(c.Color, n)).Where(t => (t.Item1.R == sc.BackColor.R) && (t.Item1.G == sc.BackColor.G) && (t.Item1.B == sc.BackColor.B)).FirstOrDefault()?.Item2;
                    BackColorNdx = ndx.HasValue ? ndx.Value : 9;
                    ndx = Colors.Select((c, n) => new Tuple<Color, int>(c.Color, n)).Where(t => (t.Item1.R == sc.BorderColor.R) && (t.Item1.G == sc.BorderColor.G) && (t.Item1.B == sc.BorderColor.B)).FirstOrDefault()?.Item2;
                    BorderColorNdx = ndx.HasValue ? ndx.Value : 3;
                    _SurveyItem = sc;
                }
                return _SurveyItem;
            }
            set
            {
                if (SurveyItem != null)
                    if (SurveyItem.URI.Equals(value.URI))
                        return;
                _SurveyItem?.Dispose();
                _SurveyItem = value;
                CSurveyCaption sc = (CSurveyCaption)value;
                int? ndx = CaptionFontSizes.Select((sz, n) => new Tuple<float, int>(sz, n)).Where(t => t.Item1 == sc.FontSize).FirstOrDefault()?.Item2;
                FontSizeNdx = ndx.HasValue ? ndx.Value : 2;
                ndx = BorderWidths.Select((sz, n) => new Tuple<int, int>(sz, n)).Where(t => t.Item1 == sc.BorderWidth).FirstOrDefault()?.Item2;
                BorderWidthNdx = ndx.HasValue ? ndx.Value : 2;
                ndx = Colors.Select((c, n) => new Tuple<Color, int>(c.Color, n)).Where(t => (t.Item1.R == sc.FontColor.R) && (t.Item1.G == sc.FontColor.G) && (t.Item1.B == sc.FontColor.B)).FirstOrDefault()?.Item2;
                FontColorNdx = ndx.HasValue ? ndx.Value : 0;
                ndx = Colors.Select((c, n) => new Tuple<Color, int>(c.Color, n)).Where(t => (t.Item1.R == sc.BackColor.R) && (t.Item1.G == sc.BackColor.G) && (t.Item1.B == sc.BackColor.B)).FirstOrDefault()?.Item2;
                BackColorNdx = ndx.HasValue ? ndx.Value : 9;
                ndx = Colors.Select((c, n) => new Tuple<Color, int>(c.Color, n)).Where(t => (t.Item1.R == sc.BorderColor.R) && (t.Item1.G == sc.BorderColor.G) && (t.Item1.B == sc.BorderColor.B)).FirstOrDefault()?.Item2;
                BorderColorNdx = ndx.HasValue ? ndx.Value : 3;
                CaptionEdit.Text = sc.Text;
                if (IsHandleCreated)
                {
                    RecalcSize();
                    Invalidate();
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
                    CaptionFormatStrip.SuspendLayout();
                    FontColorDrop.Image = Colors[value].ColorRect;
                    CaptionFormatStrip.ResumeLayout();
                    _FontColorNdx = value;
                    CaptionEdit.ForeColor = FontColor;
                }
            }
        }
                
        private System.Drawing.Color FontColor
        {
            get
            {
                return Colors[FontColorNdx].Color;
            }
            set
            {
                FontColorDrop.Image = ColorCollection.All.Where(c => (c.R == value.R) && (c.G == value.G) && (c.B == value.B)).FirstOrDefault()?.ColorRect;
                CaptionFormatStrip.Invalidate();
                FontColorNdx = ColorCollection.IndexOf(ColorCollection.All.Where(c => (c.R == value.R) && (c.G == value.G) && (c.B == value.B)).FirstOrDefault());
            }
        }

        private int BackColorNdx
        {
            get
            {
                return _BackColorNdx;
            }
            set
            {
                if (_BackColorNdx != value)
                {
                    CaptionFormatStrip.SuspendLayout();
                    BackColorDrop.Image = Colors[value].ColorRect;
                    CaptionFormatStrip.ResumeLayout();
                    _BackColorNdx = value;
                    this.BackColor = Colors[value].Color;
                    CaptionEdit.BackColor = Colors[value].Color;
                }
            }
        }

        private System.Drawing.Color BackgroundColor
        {
            get
            {
                return Colors[BackColorNdx].Color;
            } set
            {
                BackColorDrop.Image = ColorCollection.All.Where(c => (c.R == value.R) && (c.G == value.G) && (c.B == value.B)).FirstOrDefault()?.ColorRect;
                CaptionFormatStrip.Invalidate();
                _BackColorNdx = ColorCollection.IndexOf(ColorCollection.All.Where(c => (c.R == value.R) && (c.G == value.G) && (c.B == value.B)).FirstOrDefault());
                CaptionEdit.BackColor = value;
                BackColor = value;
            }
        }

        private int BorderColorNdx
        {
            get
            {
                return _BorderColorNdx;
            }
            set
            {
                if (_BorderColorNdx != value)
                {
                    CaptionFormatStrip.SuspendLayout();
                    BorderColorDrop.Image = Colors[value].ColorRect;
                    _BorderColorNdx = value;
                    CaptionFormatStrip.ResumeLayout();
                }
            }
        }

        private System.Drawing.Color BorderColor
        {
            get
            {
                return Colors[BorderColorNdx].Color;
            } set
            {
                BorderColorDrop.Image = ColorCollection.All.Where(c => (c.R == value.R) && (c.G == value.G) && (c.B == value.B)).FirstOrDefault()?.ColorRect;
                CaptionFormatStrip.Invalidate();
                _BorderColorNdx = ColorCollection.IndexOf(ColorCollection.All.Where(c => (c.R == value.R) && (c.G == value.G) && (c.B == value.B)).FirstOrDefault());
            }
        }

        private int FontSizeNdx
        {
            get 
            {
                return _FontSizeNdx;
            }
            set 
            {
                if (_FontSizeNdx != value)
                {
                    FontSizeDrop.Text = String.Format("{0}pt", CaptionFontSizes[value]);
                    _FontSizeNdx = value;
                }
            }
        }

        private float FontSize
        {
            get
            {
                return CaptionFontSizes[FontSizeNdx] * .975F;
            } set
            {
                FontSizeDrop.Text = String.Format("{0}pt", value);
                _FontSizeNdx = CaptionFontSizes.Select((fs, ndx) => new Tuple<float, int>(fs, ndx)).Where(tup => tup.Item1 == value).Select(tup => tup.Item2).First();
            }
        }

        private int BorderWidth
        {
            get
            {
                return BorderWidths[BorderWidthNdx];
            } set
            {
                BorderWidthDrop.Text = String.Format("{0}px", value);
                _BorderWidthNdx = BorderWidths.Select((fs, ndx) => new Tuple<float, int>(fs, ndx)).Where(tup => tup.Item1 == value).Select(tup => tup.Item2).First();
            }
        }

        private int BorderWidthNdx
        {
            get 
            {
                return _BorderWidthNdx;
            }
            set 
            {
                if (_BorderWidthNdx != value)
                {
                    BorderWidthDrop.Text = String.Format("{0}px", BorderWidths[value]);
                    _BorderWidthNdx = value;
                }
            }
        }


        public CaptionDisplay()
        {
            CaptionFormatStrip = null;
            FontSizeLabel = FontColorLabel = BackColorLabel = BorderColorLabel = BorderWidthLabel = null;
            FontSizeDrop = FontColorDrop = BackColorDrop = BorderColorDrop = BorderWidthDrop = null;
            this.Load += new EventHandler(CaptionDisplay_Load);
            _BackColorNdx = _BorderColorNdx = _BorderWidthNdx = _FontColorNdx = _FontSizeNdx = -1;
            CaptionFormatStrip = new ToolStrip();
            CaptionFormatStrip.SuspendLayout();

            // init tool strip
            CaptionFormatStrip.Location = new Point(0, 0);
            CaptionFormatStrip.Size = new Size(ClientSize.Width, DropMenuFont.Height);

            FontSizeLabel = new ToolStripLabel(Properties.Resources.sCaptionFontSizeLabel);
            FontSizeLabel.Font = DropMenuFont;
            FontSizeLabel.Size = TextRenderer.MeasureText(FontSizeLabel.Text, DropMenuFont);
            _FontSizeNdx = 2;
            CaptionFormatStrip.Items.Add(FontSizeLabel);

            FontSizeDrop = new ToolStripDropDownButton();
            for (int ctr = 0; ctr < CaptionFontSizes.Length; ctr++)
            {
                ToolStripMenuItem FontItem = new ToolStripMenuItem(String.Format("{0}pt", CaptionFontSizes[ctr]));
                FontItem.Font = DropMenuFont;
                FontSizeDrop.DropDown.Items.Add(FontItem);
            }
            FontSizeDrop.DropDownItemClicked += new ToolStripItemClickedEventHandler(FontSize_Clicked);
            FontSizeDrop.Font = DropMenuFont;
            FontSizeDrop.TextAlign = ContentAlignment.MiddleLeft;
            CaptionFormatStrip.Items.Add(FontSizeDrop);

            FontColorLabel = new ToolStripLabel(Properties.Resources.sCaptionFontColorLabel);
            FontColorLabel.Font = DropMenuFont;
            FontColorLabel.Size = TextRenderer.MeasureText(FontColorLabel.Text, DropMenuFont);
            CaptionFormatStrip.Items.Add(FontColorLabel);

            FontColorDrop = new ToolStripDropDownButton();
            for (int ctr = 0; ctr < Colors.Length; ctr++)
            {
                ToolStripMenuItem FontColorItem = new ToolStripMenuItem();
                FontColorItem.Image = Colors[ctr].ColorRect;
                FontColorItem.ImageAlign = ContentAlignment.MiddleLeft;
                FontColorItem.Text = Colors[ctr].Name;
                FontColorItem.Font = DropMenuFont;
                FontColorItem.TextAlign = ContentAlignment.MiddleRight;
                FontColorDrop.DropDown.Items.Add(FontColorItem);
            }
            FontColorDrop.DropDownItemClicked += new ToolStripItemClickedEventHandler(FontColor_Clicked);
            FontColorDrop.Font = DropMenuFont;
            FontColorDrop.TextAlign = ContentAlignment.MiddleRight;
            FontColorDrop.ImageAlign = ContentAlignment.MiddleLeft;
            CaptionFormatStrip.Items.Add(FontColorDrop);

            BackColorLabel = new ToolStripLabel(Properties.Resources.sCaptionBackColorLabel);
            BackColorLabel.Font = DropMenuFont;
            BackColorLabel.Size = TextRenderer.MeasureText(BackColorLabel.Text, DropMenuFont);
            CaptionFormatStrip.Items.Add(BackColorLabel);

            BackColorDrop = new ToolStripDropDownButton();
            for (int ctr = 0; ctr < Colors.Length; ctr++)
            {
                ToolStripMenuItem BackColorItem = new ToolStripMenuItem();
                BackColorItem.Image = Colors[ctr].ColorRect;
                BackColorItem.ImageAlign = ContentAlignment.MiddleLeft;
                BackColorItem.Text = Colors[ctr].Name;
                BackColorItem.Font = DropMenuFont;
                BackColorItem.TextAlign = ContentAlignment.MiddleRight;
                BackColorDrop.DropDown.Items.Add(BackColorItem);
            }
            BackColorDrop.DropDownItemClicked += new ToolStripItemClickedEventHandler(BackColor_Clicked);
            BackColorDrop.Font = DropMenuFont;
            BackColorDrop.TextAlign = ContentAlignment.MiddleRight;
            BackColorDrop.ImageAlign = ContentAlignment.MiddleLeft;
            CaptionFormatStrip.Items.Add(BackColorDrop);

            BorderColorLabel = new ToolStripLabel(Properties.Resources.sCaptionBorderColorLabel);
            BorderColorLabel.Font = DropMenuFont;
            BorderColorLabel.Size = TextRenderer.MeasureText(BorderColorLabel.Text, DropMenuFont);
            CaptionFormatStrip.Items.Add(BorderColorLabel);

            BorderColorDrop = new ToolStripDropDownButton();
            for (int ctr = 0; ctr < Colors.Length; ctr++)
            {
                ToolStripMenuItem BorderColorItem = new ToolStripMenuItem();
                BorderColorItem.Image = Colors[ctr].ColorRect;
                BorderColorItem.ImageAlign = ContentAlignment.MiddleLeft;
                BorderColorItem.Text = Colors[ctr].Name;
                BorderColorItem.Font = DropMenuFont;
                BorderColorItem.TextAlign = ContentAlignment.MiddleRight;
                BorderColorDrop.DropDown.Items.Add(BorderColorItem);
            }
            BorderColorDrop.DropDownItemClicked += new ToolStripItemClickedEventHandler(BorderColor_Clicked);
            BorderColorDrop.Font = DropMenuFont;
            BorderColorDrop.TextAlign = ContentAlignment.MiddleRight;
            BorderColorDrop.ImageAlign = ContentAlignment.MiddleLeft;
            _BorderWidthNdx = 2;
            CaptionFormatStrip.Items.Add(BorderColorDrop);

            BorderWidthLabel = new ToolStripLabel(Properties.Resources.sCaptionBorderWidthLabel);
            BorderWidthLabel.Font = DropMenuFont;
            BorderWidthLabel.Size = TextRenderer.MeasureText(BorderWidthLabel.Text, DropMenuFont);
            CaptionFormatStrip.Items.Add(BorderWidthLabel);

            BorderWidthDrop = new ToolStripDropDownButton();
            for (int ctr = 0; ctr < BorderWidths.Length; ctr++)
            {
                ToolStripMenuItem BorderWidthItem = new ToolStripMenuItem();
                BorderWidthItem.Text = String.Format("{0}px", BorderWidths[ctr]);
                BorderWidthItem.Font = DropMenuFont;
                BorderWidthDrop.DropDown.Items.Add(BorderWidthItem);
            }
            BorderWidthDrop.DropDownItemClicked += new ToolStripItemClickedEventHandler(BorderWidth_Clicked);
            BorderWidthDrop.Font = DropMenuFont;
            BorderWidthDrop.TextAlign = ContentAlignment.MiddleLeft;
            CaptionFormatStrip.Items.Add(BorderWidthDrop);
            CaptionFormatStrip.ResumeLayout();
            Controls.Add(CaptionFormatStrip);
            CaptionFormatStrip.Visible = true;

            CaptionEdit = new TextBox();
            CaptionEdit.TextAlign = HorizontalAlignment.Center;
            CaptionEdit.Text = Properties.Resources.sSurveyCaptionDefaultText;
            CaptionEdit.BorderStyle = BorderStyle.None;
            CaptionEdit.Font = new Font(FontFamily.GenericSerif, 12, FontStyle.Bold);
            CaptionEdit.MouseEnter += new EventHandler(CaptionEdit_MouseEnter);
            CaptionEdit.MouseLeave += new EventHandler(CaptionEdit_MouseLeave);
            CaptionEdit.Enter += new EventHandler(CaptionEdit_Enter);
            CaptionEdit.Leave += new EventHandler(CaptionEdit_Leave);
            CaptionEdit.TextChanged += (sender, args) => { (SurveyItem as CSurveyCaption).Text = CaptionEdit.Text; };
            Controls.Add(CaptionEdit);
            this.Resize += (sender, args) => { RecalcSize(); };
        }

        void CaptionDisplay_Load(object sender, EventArgs e)
        {
            RecalcSize();
            FontSize = Convert.ToSingle((SurveyItem as CSurveyCaption).FontSize);
            BorderWidth = Convert.ToInt32((SurveyItem as CSurveyCaption).BorderWidth);
            BackgroundColor = (SurveyItem as CSurveyCaption).BackColor;
            FontColor = (SurveyItem as CSurveyCaption).FontColor;
            BorderColor = (SurveyItem as CSurveyCaption).BorderColor;
        }

        void CaptionEdit_Leave(object sender, EventArgs e)
        {
            if (CaptionEdit.Text == String.Empty)
                CaptionEdit.Text = Properties.Resources.sSurveyCaptionDefaultText;
            (SurveyItem as CSurveyCaption).Text = CaptionEdit.Text;
        }

        void CaptionEdit_Enter(object sender, EventArgs e)
        {
            if (CaptionEdit.Text == Properties.Resources.sSurveyCaptionDefaultText)
                CaptionEdit.Text = String.Empty;
            (SurveyItem as CSurveyCaption).Text = CaptionEdit.Text;
        }

        void CaptionEdit_MouseLeave(object sender, EventArgs e)
        {
            CaptionEdit.BorderStyle = BorderStyle.None;
        }

        void CaptionEdit_MouseEnter(object sender, EventArgs e)
        {
            CaptionEdit.BorderStyle = BorderStyle.FixedSingle;
            if (!Active)
                Active = true;
        }

        private void FontSize_Clicked(object sender, ToolStripItemClickedEventArgs e)
        {
            FontSizeNdx = FontSizeDrop.DropDown.Items.IndexOf(e.ClickedItem);
            (SurveyItem as CSurveyCaption).FontSize = Convert.ToInt32(new Regex(@"^([0-9]+).+$").Match(e.ClickedItem.Text).Groups[1].Value);
            RecalcSize();
            Invalidate();
        }

        private void FontColor_Clicked(object sender, ToolStripItemClickedEventArgs e)
        {
            FontColorNdx = FontColorDrop.DropDown.Items.IndexOf(e.ClickedItem);
            (SurveyItem as CSurveyCaption).FontColor = ColorCollection.Parse(e.ClickedItem.Text).Color;
            Invalidate();
        }

        private void BackColor_Clicked(object sender, ToolStripItemClickedEventArgs e)
        {
            BackColorNdx = BackColorDrop.DropDown.Items.IndexOf(e.ClickedItem);
            (SurveyItem as CSurveyCaption).BackColor = ColorCollection.Parse(e.ClickedItem.Text).Color;
            this.BackColor = BackgroundColor;
            Invalidate();
        }

        private void BorderColor_Clicked(object sender, ToolStripItemClickedEventArgs e)
        {
            BorderColorNdx = BorderColorDrop.DropDown.Items.IndexOf(e.ClickedItem);
            (SurveyItem as CSurveyCaption).BorderColor = ColorCollection.Parse(e.ClickedItem.Text).Color;
            Invalidate();
        }

        private void BorderWidth_Clicked(object sender, ToolStripItemClickedEventArgs e)
        {
            BorderWidthNdx = BorderWidthDrop.DropDown.Items.IndexOf(e.ClickedItem);
            (SurveyItem as CSurveyCaption).BorderWidth = BorderWidths[BorderWidthNdx];
            RecalcSize();
            Invalidate();
        }


        private readonly ManualResetEventSlim recalcEvent = new ManualResetEventSlim(true);
        public override void RecalcSize()
        {
            if (!recalcEvent.IsSet)
                return;
            recalcEvent.Reset();
            Size sz = this.Size;
            Font captionFont = new Font(FontFamily.GenericSerif, FontSize * 3 / 5, FontStyle.Bold);
            if (CaptionEdit.Font.Size != FontSize)
            {
                CaptionEdit.Font.Dispose();
                CaptionEdit.Font = captionFont;
            }
            CaptionEdit.Size = new Size(ClientSize.Width, captionFont.Height);
            sz.Height = (int)(1.5 * CaptionEdit.Height) + BorderWidth;
            if (CaptionFormatStrip.Visible)
                sz.Height += CaptionFormatStrip.Height;
            this.Size = sz;
            if (CaptionFormatStrip.Visible)
                CaptionEdit.Location = new Point(0, CaptionFormatStrip.Height + (captionFont.Height >> 2));
            else
                CaptionEdit.Location = new Point(0, captionFont.Height >> 2);
            CaptionEdit.Invalidate();
            Invalidate();
            (Parent as SurveyDisplay)?.RecalcSize();
            recalcEvent.Set();
        }

        protected override void OnActivate(bool BecomingActive)
        {
//            CaptionFormatStrip.Visible = BecomingActive;
            RecalcSize();
            base.OnActivate(BecomingActive);
        }

        protected override void SurveyItemDisplay_Paint(object sender, PaintEventArgs e)
        {
            Brush borderBr = new SolidBrush(BorderColor);
            if (BorderWidth != 0)
                e.Graphics.FillRectangle(borderBr, new Rectangle(new Point(0, ClientRectangle.Bottom - BorderWidth), new Size(ClientSize.Width, ClientRectangle.Bottom)));
            borderBr.Dispose();
        }
    }
}
