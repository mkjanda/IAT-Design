using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{

    public partial class TextEditControl : UserControl
    {

        FontFamilyToolstripCombo FontFamilyDropDown = new FontFamilyToolstripCombo();
        protected int SetWidth;
        private bool bUpdatingInternally = false;
        protected System.Windows.Forms.TextBox TextEdit;
        protected int NumLines;

        public Size CalculatedSize
        {
            get
            {
                return new Size(SetWidth, FontToolStrip.Height + TextEdit.Height + 5);
            }
        }
        protected CFontFile.FontData[] AvailableFonts
        {
            get
            {
                return IATConfigMainForm.AvailableFonts;
            }
        }
        private static String[] FontSizes = { "6", "8", "10", "12", "14", "16", "18", "20", "22", "24", "26", "28", "30", "32", "34", "36", "38", "40",
            "42", "44", "46", "48", "52", "54", "56", "60", "62", "66", "68", "70", "72", "76", "80", "84", "88", "92" };

        private const double ColorRectAspectRatio = 4.0 / 3.0;

        private Uri _TextDisplayItemUri = null;

        private DIText.UsedAs UsedFor;

        public void StartInternalUpdate()
        {
            bUpdatingInternally = true;
        }

        public void EndInternalUpdate()
        {
            bUpdatingInternally = false;
        }


        public Uri TextDisplayItemUri
        {
            get
            {
                if (_TextDisplayItemUri != null)
                    return _TextDisplayItemUri;
                bUpdatingInternally = true;
                DIText tdi = null;
                if (UsedFor == DIText.UsedAs.Conjunction)
                    tdi = new DIConjunction();
                else if (UsedFor == DIText.UsedAs.ContinueInstructions)
                    tdi = new DIContinueInstructions();
                else if (UsedFor == DIText.UsedAs.Stimulus)
                    tdi = new DIStimulusText();
                else if (UsedFor == DIText.UsedAs.IatBlockInstructions)
                    tdi = new DIIatBlockInstructions();
                else if (UsedFor == DIText.UsedAs.MockItemInstructions)
                    tdi = new DIMockItemInstructions();
                else if (UsedFor == DIText.UsedAs.TextInstructionsScreen)
                    tdi = new DITextInstructionsScreen();
                else if (UsedFor == DIText.UsedAs.KeyedInstructionsScreen)
                    tdi = new DIKeyedInstructionsScreen();
                else if (UsedFor == DIText.UsedAs.ResponseKey)
                    tdi = new DIResponseKeyText();
                try
                {
                    tdi.PhraseFontFamily = CIAT.SaveFile.FontPreferences[UsedFor].FontFamily;
                    tdi.Phrase = String.Empty;
                    tdi.PhraseFontColor = CIAT.SaveFile.FontPreferences[UsedFor].FontColor;
                    tdi.PhraseFontSize = CIAT.SaveFile.FontPreferences[UsedFor].FontSize;
                    FontFamilyDropDown.FamilyName = tdi.PhraseFontFamily;
                    FontSizeDropDown.Text = String.Format("{0}pt", tdi.PhraseFontSize.ToString("F00"));
                }
                catch (Exception ex)
                {
                    tdi.Phrase = String.Empty;
                    tdi.PhraseFontColor = NamedColor.GetNamedColor(ColorDropDown.Text);
                    tdi.PhraseFontSize = FontSize;
                    tdi.PhraseFontFamily = System.Drawing.SystemFonts.DefaultFont.FontFamily.Name;
                    FontFamilyDropDown.Text = tdi.PhraseFontFamily;
                }
                TextEdit.Text = String.Empty;
                bUpdatingInternally = false;
                _TextDisplayItemUri = tdi.URI;
                return tdi.URI;
            }
            set
            {
                bUpdatingInternally = true;
                if (value != null)
                {
                    DIText tdi = CIAT.SaveFile.GetDI(value) as DIText;
                    FontFamilyDropDown.FamilyName = tdi.PhraseFontFamily;
                    FontSizeDropDown.Text = String.Format("{0}pt", tdi.PhraseFontSize.ToString("F00"));
                    ColorDropDown.Image = ColorDropDown.DropDownItems[tdi.PhraseFontColor.Name].Image;
                    ColorDropDown.Text = ColorDropDown.DropDownItems[tdi.PhraseFontColor.Name].Text;
                    TextEdit.Text = tdi.Phrase;
                    _TextDisplayItemUri = value;
                }
                else
                {
                    _TextDisplayItemUri = null;
                }
                /*
                DIText tdi = null;
                if (UsedFor == DIText.UsedAs.Conjunction)
                    tdi = new DIConjunction();
                else if (UsedFor == DIText.UsedAs.ContinueInstructions)
                    tdi = new DIContinueInstructions();
                else if (UsedFor == DIText.UsedAs.Stimulus)
                    tdi = new DIStimulusText();
                else if (UsedFor == DIText.UsedAs.IatBlockInstructions)
                    tdi = new DIIatBlockInstructions();
                else if (UsedFor == DIText.UsedAs.MockItemInstructions)
                    tdi = new DIMockItemInstructions();
                else if (UsedFor == DIText.UsedAs.TextInstructionsScreen)
                    tdi = new DITextInstructionsScreen();
                else if (UsedFor == DIText.UsedAs.KeyedInstructionsScreen)
                    tdi = new DIKeyedInstructionsScreen();
                else if (UsedFor == DIText.UsedAs.ResponseKey)
                    tdi = new DIResponseKeyText();
                try
                {
                    tdi.PhraseFontFamily = CIAT.SaveFile.FontPreferences[UsedFor].FontFamily;
                    tdi.Phrase = String.Empty;
                    tdi.PhraseFontColor = CIAT.SaveFile.FontPreferences[UsedFor].FontColor;
                    tdi.PhraseFontSize = CIAT.SaveFile.FontPreferences[UsedFor].FontSize;
                    FontFamilyDropDown.FamilyName = tdi.PhraseFontFamily;
                    FontSizeDropDown.Text = String.Format("{0}pt", tdi.PhraseFontSize.ToString("F00"));
                }
                catch (Exception ex)
                {
                    tdi.Phrase = String.Empty;
                    tdi.PhraseFontColor = System.Drawing.Color.FromName(ColorDropDown.Text);
                    tdi.PhraseFontSize = FontSize;
                    tdi.PhraseFontFamily = System.Drawing.SystemFonts.DefaultFont.FontFamily.Name;
                    FontFamilyDropDown.FamilyName = tdi.PhraseFontFamily;
                }
                TextEdit.Text = String.Empty;
                _TextDisplayItemUri = tdi.URI;
            }*/
                bUpdatingInternally = false;
            }
        }

        public String TextValue
        {
            get
            {
                return TextEdit.Text;
            }
            set
            {
                TextEdit.Text = value;
            }
        }

        public System.Drawing.Color FontColor
        {
            get
            {
                return System.Drawing.Color.FromName(ColorDropDown.Text);
            }
            set
            {
                ColorDropDown.Image = ColorDropDown.DropDownItems[NamedColor.GetNamedColor(value).Name].Image;
                ColorDropDown.Text = ColorDropDown.DropDownItems[NamedColor.GetNamedColor(value).Name].Text;
            }
        }

        public float FontSize
        {
            get
            {
                return Convert.ToSingle(FontSizeDropDown.Text.Substring(0, FontSizeDropDown.Text.Length - 2));
            }
            set
            {
                FontSizeDropDown.Text = String.Format("{0}pt", value.ToString("F00"));
            }
        }

        public String FontFamily
        {
            get
            {
                return FontFamilyDropDown.FamilyName;
            }
        }

        public TextEditControl(int width, DIText.UsedAs usedAs, bool senderUpdates)
        {
            _TextDisplayItemUri = null;
            NumLines = 1;
            SetWidth = width;
            InitializeComponent();
            TextEdit = new TextBox();
            TextEdit.Multiline = false;
            TextEdit.Location = new Point(0, FontToolStrip.Height);
            TextEdit.Size = new Size(SetWidth, TextEdit.Font.Height + TextEdit.Margin.Vertical);
            Controls.Add(TextEdit);
            TextEdit.TextChanged += new EventHandler(TextEdit_TextChanged);
            this.UsedFor = usedAs;

            FontToolStrip.SuspendLayout();
            FillFontFamilyDropList();
            FillFontSizeDropList();
            FillColorDropList();
            FontToolStrip.ResumeLayout();
            bSenderUpdates = senderUpdates;
            this.Size = new Size(SetWidth, TextEdit.Size.Height + TextEdit.Margin.Vertical + FontToolStrip.Height);
        }

        public TextEditControl(int Height, int Width, DIText.UsedAs UsedAs, bool senderUpdates)
        {
            _TextDisplayItemUri = null;
            SetWidth = Width;
            InitializeComponent();
            TextEdit = new TextBox();
            TextEdit.Location = new Point(0, FontToolStrip.Height);
            if (Height < TextEdit.Font.Height + TextEdit.Margin.Vertical + FontToolStrip.Height)
                Height = TextEdit.Font.Height + TextEdit.Margin.Vertical + FontToolStrip.Height;
            TextEdit.Size = new Size(SetWidth, Height - FontToolStrip.Height - TextEdit.Margin.Vertical);
            NumLines = (Height - TextEdit.Margin.Vertical - FontToolStrip.Height) / TextEdit.Font.Height;
            Controls.Add(TextEdit);
            if (NumLines > 1)
                TextEdit.Multiline = true;
            else
                TextEdit.Multiline = false;
            TextEdit.TextChanged += new EventHandler(TextEdit_TextChanged);
            this.UsedFor = UsedAs;

            FontToolStrip.SuspendLayout();
            FillFontFamilyDropList();
            FillFontSizeDropList();
            FillColorDropList();
            FontToolStrip.ResumeLayout();
            bSenderUpdates = senderUpdates;
            this.Size = new Size(SetWidth, TextEdit.Size.Height + TextEdit.Margin.Vertical + FontToolStrip.Height);
        }

        private bool bSenderUpdates = false;


        private void FillFontFamilyDropList()
        {
            FontToolStrip.Items.Add(FontFamilyDropDown);
            FontFamilyDropDown.FontCombo.SelectedIndexChanged += new EventHandler(FontFamilyButton_Click);
        }

        protected virtual void FontFamilyButton_Click(object sender, EventArgs e)
        {
            if (bUpdatingInternally)
                return;
            CIAT.SaveFile.FontPreferences[UsedFor].FontFamily = FontFamilyDropDown.FamilyName;
            if (_TextDisplayItemUri == null)
                return;
            DIText tdi = CIAT.SaveFile.GetDI(_TextDisplayItemUri) as DIText;
            if (tdi != null)
                tdi.PhraseFontFamily = FontFamilyDropDown.FamilyName;
        }

        private void FillFontSizeDropList()
        {
            FontSizeDropDown.DropDownItems.Clear();
            for (int ctr = 0; ctr < FontSizes.Length; ctr++)
            {
                FontSizeDropDown.DropDownItems.Add(String.Format("{0}pt", FontSizes[ctr]));
                if (Convert.ToSingle(FontSizes[ctr]) == CIAT.SaveFile.FontPreferences[UsedFor].FontSize)
                    FontSizeDropDown.Text = String.Format("{0}pt", FontSizes[ctr]);
            }
        }

        private void FillColorDropList()
        {
            Bitmap ColorRect;
            Graphics bmpGraph;
            Rectangle BorderRect, FillRect;
            Brush br;
            int nHeight = ColorDropDown.Height;
            int nWidth = (int)(nHeight * ColorRectAspectRatio);

            Graphics g = Graphics.FromHwnd(this.Handle);
            BorderRect = new Rectangle(0, 0, nWidth, nHeight);
            FillRect = new Rectangle(1, 1, nWidth - 2, nHeight - 2);
            foreach (var nc in NamedColor.All)
            {
                ColorRect = new Bitmap(ColorDropDown.Height, ColorDropDown.Height, g);
                bmpGraph = Graphics.FromImage(ColorRect);
                bmpGraph.DrawRectangle(Pens.Black, BorderRect);

                br = new SolidBrush(nc.Color);
                bmpGraph.FillRectangle(br, FillRect);
                br.Dispose();
                ColorDropDown.DropDownItems.Add(new ToolStripMenuItem(nc.Name, ColorRect, (sender, args) =>
                {
                    if (bUpdatingInternally)
                        return;
                    var namedColor = NamedColor.GetNamedColor((sender as ToolStripDropDownItem).Name);
                    ColorDropDown.Image = (sender as ToolStripDropDownItem).Image;
                    ColorDropDown.Text = (sender as ToolStripDropDownItem).Text;
                    CIAT.SaveFile.FontPreferences[UsedFor].FontColor = namedColor;
                    if (_TextDisplayItemUri == null)
                        return;
                    DIText tdi = CIAT.SaveFile.GetDI(_TextDisplayItemUri) as DIText;
                    if (tdi != null)
                        tdi.PhraseFontColor = namedColor;
                }, nc.Name));
                if (nc == CIAT.SaveFile.FontPreferences[UsedFor].FontColor)
                {
                    ColorDropDown.Image = ColorRect;
                    ColorDropDown.Text = nc.Name;
                }
                bmpGraph.Dispose();
            }
            g.Dispose();
        }

        protected virtual void FontSizeDropDown_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (bUpdatingInternally)
                return;
            FontSizeDropDown.Text = e.ClickedItem.Text;
            CIAT.SaveFile.FontPreferences[UsedFor].FontSize = Convert.ToSingle(FontSize);
            if (_TextDisplayItemUri == null)
                return;
            DIText tdi = CIAT.SaveFile.GetDI(_TextDisplayItemUri) as DIText;
            if (tdi != null)
                tdi.PhraseFontSize = Convert.ToSingle(FontSize);
        }

        protected virtual void ColorDropDown_DropDownItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
        }

        virtual protected void TextEdit_TextChanged(object sender, EventArgs e)
        {
            if (bUpdatingInternally)
                return;
            if (_TextDisplayItemUri == null)
                return;
            DIText tdi = CIAT.SaveFile.GetDI(_TextDisplayItemUri) as DIText;
            if (tdi != null)
                tdi.Phrase = TextEdit.Text;
        }
    }
}
