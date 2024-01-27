using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace IATClient
{
    class SurveyTextFormatControl : UserControl
    {
        public event EventHandler OnColorSelectionStart;
        public event EventHandler OnColorSelectionEnd;
        private CheckBox BoldBox = new CheckBox(), ItalicBox = new CheckBox();
        private ComboBox FontName, FontSize;
        private Padding ComponentPadding = new Padding(5, 5, 5, 5);
        private int ColorAlpha = 255;
        private int ColorBrightness = 255;
        private Color FontColor;
        private CheckBox FontColorBox;
        private Timer FadeTimer = new Timer();
        private static readonly String[] FontSizes = new String[] { "10px", "12px", "14px", "16px", "18px", "20px", "22px", "24px", "26px", "28px", "30px", "32px", "34px", "36px", "38px", "40px", "42px", "44px", "46px", "48px" };
        private SurveyItemFormat.EFor For;
        private bool DispatchEvents { get; set; } = true;
        private SurveyItemFormat _SurveyItemFormat { get; set; } = null;
        public SurveyItemFormat ItemFormat
        {
            get
            {
                if (_SurveyItemFormat == null)
                    return null;
                _SurveyItemFormat.FontFamily = PrivateFont.Fonts.Where(f => f.DisplayName == (String)FontName.SelectedItem).Select(f => f.FontFamily).FirstOrDefault();
                _SurveyItemFormat.FontSize = (String)FontSize.SelectedItem;
                _SurveyItemFormat.Color = TextColor;
                return _SurveyItemFormat;
            }
            set
            {
                DispatchEvents = false;
                _SurveyItemFormat = value;
                FontName.SelectedIndex = PrivateFont.Fonts.Select((f, ndx) => new { ndx = ndx, f = f }).Where(a => a.f.FontFamily.Name == value.FontFamily.Name).Select(a => a.ndx).FirstOrDefault();
                FontSize.SelectedIndex = FontSize.Items.IndexOf(value.FontSize);
                FontColor = value.Color;
                BoldBox.Checked = value.Bold;
                ItalicBox.Checked = value.Italic;
                DispatchEvents = true;
                CIAT.Dispatcher.Dispatch<ISurveyItemFormatChanged>(new CSurveyItemFormatChanged(_SurveyItemFormat));
            }
        }

        public Color TextColor
        {
            get
            {
                return FontColor;
            }
            set
            {
                if (FontColor != value)
                {
                    FontColor = value;
                    CIAT.Dispatcher.Dispatch<ISurveyItemFormatChanged>(new CSurveyItemFormatChanged(ItemFormat));
                }
            }
        }


        public SurveyTextFormatControl(SurveyItemFormat.EFor f)
        {
            this.For = f;
            this.Load += new EventHandler(SurveyTextFormatControl_Load);

            int maxFontSizeWidth = FontSizes.Select(fSizes => TextRenderer.MeasureText(fSizes, System.Drawing.SystemFonts.DialogFont).Width).Max();
            FontSize = new ComboBox();
            FontSize.Items.AddRange(FontSizes);
            FontSize.DropDownStyle = ComboBoxStyle.DropDownList;
            FontSize.Size = new Size(maxFontSizeWidth + 25, FontSize.Font.Height + 4);
            FontSize.Location = new Point(ComponentPadding.Left, ComponentPadding.Top);
            Controls.Add(FontSize);

            FontName = new ComboBox();
            FontName.Items.AddRange(PrivateFont.Fonts.Select(f => f.DisplayName).ToArray());
            FontName.DropDownStyle = ComboBoxStyle.DropDownList;
            FontName.Size = new Size(TextRenderer.MeasureText("sans-serif", System.Drawing.SystemFonts.DialogFont).Width + 25, FontName.Font.Height + 4);
            int maxWidth = PrivateFont.Fonts.Select(f => f.DisplayName).Aggregate(0, (max, val) =>
                (max < TextRenderer.MeasureText(val, System.Drawing.SystemFonts.DialogFont).Width ?
                TextRenderer.MeasureText(val, System.Drawing.SystemFonts.DialogFont).Width : max));
            FontName.Location = new Point(ComponentPadding.Left, FontSize.Bottom + ComponentPadding.Vertical);
            FontName.SelectedIndex = 0;
            FontName.DropDownWidth = maxWidth + 4;
            Controls.Add(FontName);

            BoldBox.Text = "B";
            BoldBox.Font = new Font(BoldBox.Font, FontStyle.Regular);
            BoldBox.Appearance = Appearance.Button;
            BoldBox.Location = new Point(FontSize.Right + 10, FontSize.Top);
            BoldBox.AutoSize = true;
            BoldBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(BoldBox);

            ItalicBox.Text = "I";
            ItalicBox.Font = new Font(ItalicBox.Font, FontStyle.Regular);
            ItalicBox.Appearance = Appearance.Button;
            ItalicBox.Location = new Point(BoldBox.Right + 5, FontSize.Top);
            ItalicBox.AutoSize = true;
            ItalicBox.Anchor = AnchorStyles.Left | AnchorStyles.Top | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(ItalicBox);

            FontColorBox = new CheckBox();
            FontColorBox.Appearance = Appearance.Button;
            FontColorBox.Text = "Font Color\r\nClick to change";
            FontColorBox.TextAlign = ContentAlignment.MiddleCenter;
            FontColorBox.AutoSize = true;
            FontColorBox.Location = new Point(ComponentPadding.Left, FontName.Bottom + ComponentPadding.Vertical);
            FontColorBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom;
            Controls.Add(FontColorBox);
            this.Size = new Size(this.Width - ComponentPadding.Horizontal, FontColorBox.Bottom + ComponentPadding.Bottom);
            FadeTimer.Interval = 10;
            FadeTimer.Tick += (s, args) =>
            {
                if (FontColorBox.Checked)
                {
                    ColorBrightness = 0;
                    ColorAlpha += 5;
                    ColorAlpha = ColorAlpha % 256;
                }
                else
                    ColorBrightness = 255;
                FontColorBox.ForeColor = Color.FromArgb(ColorBrightness, ColorBrightness, ColorBrightness);
                FontColorBox.BackColor = Color.FromArgb(FontColorBox.Checked ? ColorAlpha : 255, FontColor);
                FontColorBox.Invalidate();
            };
            FadeTimer.Start();

            FontName.SelectedIndexChanged += (s, args) => { if (DispatchEvents) CIAT.Dispatcher.Dispatch<ISurveyItemFormatChanged>(new CSurveyItemFormatChanged(ItemFormat)); };
            FontColorBox.CheckedChanged += (sender, args) =>
            {
                if (FontColorBox.Checked)
                    Invoke(new Action(() => OnColorSelectionStart(this, new EventArgs())));
                else Invoke(new Action(() => OnColorSelectionEnd(this, new EventArgs())));
            };
            ItalicBox.CheckedChanged += (sender, args) => { if (DispatchEvents) CIAT.Dispatcher.Dispatch<ISurveyItemFormatChanged>(new CSurveyItemFormatChanged(ItemFormat)); };
            BoldBox.CheckedChanged += (sender, args) => { if (DispatchEvents) CIAT.Dispatcher.Dispatch<ISurveyItemFormatChanged>(new CSurveyItemFormatChanged(ItemFormat)); };
            FontSize.SelectedIndexChanged += (s, args) => { if (DispatchEvents) CIAT.Dispatcher.Dispatch<ISurveyItemFormatChanged>(new CSurveyItemFormatChanged(ItemFormat)); };
        }


        private void SurveyTextFormatControl_Load(object sender, EventArgs e)
        {
            SuspendLayout();
            Invalidate();
            ResumeLayout(false);
        }

        public void HaltColorSelection()
        {
            this.FontColorBox.Checked = false;
        }

    }
}
