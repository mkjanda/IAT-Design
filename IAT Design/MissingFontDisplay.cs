using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public partial class MissingFontDisplay : UserControl
    {
        private class DisplayText : Panel
        {
            private Size _TextSize;
            private int _Width;
            private int DesiredWidth;
            private String DisplayedText;
            private static readonly Padding TextPadding = new Padding(10, 0, 10, 0);
            private TextBox TextBox;
            private float FontSize;


            public String DisplayFontFamily
            {
                get
                {
                    return this.TextBox.Font.FontFamily.Name;
                }
                set
                {
                    this.TextBox.Font.Dispose();
                    this.TextBox.Font = new Font(value, FontSize);
                    CalcTextSize(this.DesiredWidth - TextPadding.Horizontal);
                    this.Size = _TextSize + new Size(TextPadding.Horizontal, 0);
                    this.TextBox.Size = _TextSize;
                    Invalidate();
                }
            }

            private void CalcTextSize(int width)
            {
                this._TextSize = TextRenderer.MeasureText(this.DisplayedText, this.TextBox.Font, new Size(width, 1), TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);
            }
/*
            public DisplayText(String fontFamilyName, String text, float fontSize)
            {
                this.DisplayedText = text;
                this.Dock = DockStyle.Fill;
                this.TextBox = new TextBox();
                this.TextBox.ReadOnly = true;
                this.TextBox.Multiline = false;
                this.TextBox.BorderStyle = BorderStyle.None;
                this.TextBox.Dock = DockStyle.Left;
                this.TextBox.Padding = Padding;
                this.FontSize = fontSize;
                this.TextBox.Font = new Font(fontFamilyName, FontSize);
                this.TextBox.Margin = Padding;
                this.Multiline = false;
                this._Width = -1;
                this.TextBox.Text = DisplayedText;
                CalcTextSize(-1);
                this.TextBox.Size = _TextSize;
                this.Size = this.TextBox.Size + new Size(TextPadding.Horizontal, TextPadding.Vertical);
                Controls.Add(this.TextBox);
                this.SizeChanged += new EventHandler(DisplayText_SizeChanged);
            }
*/
            public DisplayText(String fontFamilyName, String text, float fontSize, int desiredWidth)
            {
                this.DesiredWidth = desiredWidth;
                this.DisplayedText = text;
                _Width = desiredWidth;
                this.Location = new Point(0, 0);
                this.TextBox = new TextBox();
                this.TextBox.ReadOnly = true;
                this.TextBox.WordWrap = true;
                this.TextBox.Multiline = true;
                this.TextBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                this.TextBox.Padding = TextPadding;
                this.TextBox.BorderStyle = BorderStyle.None;
                this.TextBox.TextAlign = HorizontalAlignment.Left;
                this.TextBox.AcceptsReturn = false;
                this.TextBox.AcceptsTab = false;
                this.TextBox.TabIndex = 200;
                this.FontSize = fontSize;
                this.TextBox.Font = new Font(fontFamilyName, FontSize);
                this.TextBox.Text = DisplayedText;
                CalcTextSize(desiredWidth - TextPadding.Horizontal);
                this.TextBox.Size = _TextSize;
                this.TextBox.Location = new Point(TextPadding.Left, 0);
                this.Size = _TextSize + new Size(TextPadding.Horizontal, 0);
                Controls.Add(this.TextBox);
         //       this.SizeChanged += new EventHandler(DisplayText_SizeChanged);
            }

            private void DisplayText_SizeChanged(object sender, EventArgs e)
            {
                CalcTextSize(this.Size.Width - TextPadding.Horizontal);
                if (this.Height == _TextSize.Height)
                    return;
                this.Height = _TextSize.Height;
            }
        }


        private readonly List<CFontFile.FontItem> MissingFontItems = new List<CFontFile.FontItem>();
        private static readonly String sLowerSample = "the quick brown fox jumped over the lazy dog";
        private static readonly String sUpperSample = "THE QUICK BROWN FOX JUMPED OVER THE LAZY DOG";
        private readonly List<DisplayText> SampleDTs = new List<DisplayText>();
        private readonly List<FontSelectCombo> FontCombos = new List<FontSelectCombo>();

        public MissingFontDisplay(CFontFile.FontItem[] fontItems, int initialWidth)
        {
            InitializeComponent();
            MissingFontItems.AddRange(fontItems);
            MissingFontItemTable.RowCount = MissingFontItems.Count + 1;
            List<Control> descDTs = new List<Control>();
            List<Control> fontImgBoxes = new List<Control>();
            foreach (CFontFile.FontItem fi in MissingFontItems)
            {
                PictureBox fontImgBox = new PictureBox();
                MemoryStream memStream = new MemoryStream(fi.ImageData);
                Image fontImg = Image.FromStream(memStream);
                memStream.Dispose();
                fontImgBox.Image = fontImg;
                fontImgBox.BackColor = System.Drawing.SystemColors.Control;
                fontImgBox.Size = fontImg.Size;
                fontImgBox.SizeMode = PictureBoxSizeMode.Normal;
                fontImgBox.Dock = DockStyle.Left;
                fontImgBox.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
                fontImgBoxes.Add(fontImgBox);
            }
            int maxFontImgWidth = fontImgBoxes.Max(item => item.Width);
            foreach (CFontFile.FontItem fi2 in MissingFontItems)
            {
                FontSelectCombo fsc = new FontSelectCombo();
                fsc.Size = new Size(220, fsc.Size.Height);
                FontCombos.Add(fsc);
                fsc.SelectFontFamily(System.Drawing.SystemFonts.DialogFont.FontFamily.Name);
                fsc.SelectedIndexChanged += new EventHandler(FontCombo_SelectedIndexChanged);

                DisplayText sampleDT = new DisplayText(System.Drawing.SystemFonts.DialogFont.FontFamily.Name, sUpperSample + "\r\n" + sLowerSample, 14, initialWidth - 500 - maxFontImgWidth);
                SampleDTs.Add(sampleDT);
                sampleDT.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right;

                DisplayText descDT = new DisplayText(System.Drawing.SystemFonts.DialogFont.FontFamily.Name, fi2.Description, 10, 200);
                descDTs.Add(descDT);
                descDT.Anchor = AnchorStyles.Left | AnchorStyles.Top;
            }
            int[] columnWidths = new int[4];
            MissingFontItemTable.ColumnStyles.Clear();
            MissingFontItemTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, fontImgBoxes.Select(fib => fib.Size.Width + 40).Max()));
            MissingFontItemTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, descDTs.Select(fib => fib.Size.Width).Max()));
            MissingFontItemTable.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, FontCombos.Select(fib => fib.Size.Width).Max()));
            MissingFontItemTable.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
            MissingFontItemTable.RowStyles.Clear();
            int height = 0;
            for (int ctr = 0; ctr < FontCombos.Count; ctr++)
            {
                int maxHeight = (new int[] { FontCombos[ctr].Height, fontImgBoxes[ctr].Height, SampleDTs[ctr].Height, descDTs[ctr].Height }).Max();
                fontImgBoxes[ctr].Size = new Size(fontImgBoxes[ctr].Size.Width, maxHeight);
                FontCombos[ctr].Location = new Point(FontCombos[ctr].Location.X, (maxHeight - FontCombos[ctr].Height) >> 1);
                SampleDTs[ctr].Size = new Size(SampleDTs[ctr].Size.Width, maxHeight);
                descDTs[ctr].Size = new Size(descDTs[ctr].Size.Width, maxHeight);
                MissingFontItemTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
                MissingFontItemTable.Controls.Add(fontImgBoxes[ctr], 0, ctr);
                MissingFontItemTable.Controls.Add(descDTs[ctr], 1, ctr);
                MissingFontItemTable.Controls.Add(FontCombos[ctr], 2, ctr);
                MissingFontItemTable.Controls.Add(SampleDTs[ctr], 3, ctr);
                height += maxHeight;
            }
            MissingFontItemTable.AutoSize = true;
            this.Size = MissingFontItemTable.ClientSize;
            MissingFontItemTable.SizeChanged += (sender, args) => { this.Size = MissingFontItemTable.ClientSize; };
        }

        private void FontCombo_SelectedIndexChanged(object sender, EventArgs e)
        {
            FontSelectCombo fsc = sender as FontSelectCombo;
            SampleDTs[FontCombos.IndexOf(fsc)].DisplayFontFamily = ((CFontFile.FontData)fsc.SelectedItem).FamilyName;
        }

        public bool ContainsDefaultFonts()
        {
            return FontCombos.Select(fc => ((CFontFile.FontData)fc.SelectedItem).FamilyName).Where(famName => famName == System.Drawing.SystemFonts.DialogFont.FontFamily.Name).Count() > 0;
        }

        public String[] ReplacementFontFamilies
        {
            get
            {
                return SampleDTs.Select(dt => dt.DisplayFontFamily).ToArray();
            }
        }

    }
}

