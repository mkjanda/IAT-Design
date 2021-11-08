using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
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
        private static readonly String []FontSizes = new String[] { "10px", "12px", "14px", "16px", "18px", "20px", "22px", "24px", "26px", "28px", "30px", "32px", "34px", "36px", "38px", "40px", "42px", "44px", "46px", "48px" };
        private SurveyItemFormat.EFor For;

        public SurveyItemFormat ItemFormat
        {
            get
            {
                SurveyItemFormat format = new SurveyItemFormat(For);
                format.Font = SurveyItemFormat.EFont.GetFontByName(FontName.Text);
                format.Color = FontColor;
                format.FontSize = FontSize.Text;
                format.Bold = BoldBox.Checked;
                format.Italic = ItalicBox.Checked;
                return format;
            }
            set
            {
                FontName.Text = value.Font.Name;
                FontSize.Text = value.FontSize;
                FontColor = value.Color;
                BoldBox.Checked = value.Bold;
                ItalicBox.Checked = value.Italic;
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
            this.Dock = DockStyle.Fill;
            this.Load += new EventHandler(SurveyTextFormatControl_Load);

            
            int maxFontSizeWidth = FontSizes.Select(fSizes => TextRenderer.MeasureText(fSizes, System.Drawing.SystemFonts.DialogFont).Width).Max();
            FontSize = new ComboBox();
            FontSize.Items.AddRange(FontSizes);
            FontSize.DropDownStyle = ComboBoxStyle.DropDownList;
            FontSize.Size = new Size(maxFontSizeWidth + 25, FontSize.Font.Height + 4);
            FontSize.Location = new Point(ComponentPadding.Left, ComponentPadding.Top);
            FontSize.SelectedIndexChanged += (s, args) => { CIAT.Dispatcher.Dispatch<ISurveyItemFormatChanged>(new CSurveyItemFormatChanged(ItemFormat)); };
            Controls.Add(FontSize);

            FontName = new ComboBox();
            FontName.Items.AddRange(new String[] { "sans-serif", "serif" }.Concat(CFontFile.PrivateFontNames).ToArray());
            FontName.DropDownStyle = ComboBoxStyle.DropDownList;
            FontName.Size = new Size(TextRenderer.MeasureText("sans-serif", System.Drawing.SystemFonts.DialogFont).Width + 25, FontName.Font.Height + 4);
            int maxWidth = (new String[] { "sans-serif", "serif" }.Concat(CFontFile.PrivateFontNames)).Aggregate(0, (max, val) =>
                (max < TextRenderer.MeasureText(val, System.Drawing.SystemFonts.DialogFont).Width ? 
                TextRenderer.MeasureText(val, System.Drawing.SystemFonts.DialogFont).Width : max));
            FontName.Location = new Point(ComponentPadding.Left, FontSize.Bottom + ComponentPadding.Vertical);
            FontName.SelectedIndex = 0;
            FontName.SelectedIndexChanged += (s, args) => { CIAT.Dispatcher.Dispatch<ISurveyItemFormatChanged>(new CSurveyItemFormatChanged(ItemFormat)); };
            FontName.DropDownWidth = maxWidth + 4;
            Controls.Add(FontName);

            BoldBox.Text = "B";
            BoldBox.Font = new Font(BoldBox.Font, FontStyle.Bold);
            BoldBox.Appearance = Appearance.Button;
            BoldBox.Location = new Point(FontSize.Right + 10, FontSize.Top);
            BoldBox.CheckedChanged += (sender, args) => { CIAT.Dispatcher.Dispatch<ISurveyItemFormatChanged>(new CSurveyItemFormatChanged(ItemFormat)); };
            BoldBox.AutoSize = true;
            Controls.Add(BoldBox);

            ItalicBox.Text = "I";
            ItalicBox.Font = new Font(ItalicBox.Font, FontStyle.Italic);
            ItalicBox.Appearance = Appearance.Button;
            ItalicBox.Location = new Point(BoldBox.Right + 5, FontSize.Top);
            ItalicBox.CheckedChanged += (sender, args) => { CIAT.Dispatcher.Dispatch<ISurveyItemFormatChanged>(new CSurveyItemFormatChanged(ItemFormat)); };
            ItalicBox.AutoSize = true;
            Controls.Add(ItalicBox);

            FontColorBox = new CheckBox();
            FontColorBox.Appearance = Appearance.Button;
            FontColorBox.Text = "Font Color\r\nClick to change";
            FontColorBox.TextAlign = ContentAlignment.MiddleCenter;
            FontColorBox.AutoSize = true;
            FontColorBox.Location = new Point(ComponentPadding.Left, FontName.Bottom + ComponentPadding.Vertical);
            FontColorBox.CheckedChanged += (sender, args) =>
            {
                if (FontColorBox.Checked)
                    Invoke(new Action(() => OnColorSelectionStart(this, new EventArgs())));
                else Invoke(new Action(() => OnColorSelectionEnd(this, new EventArgs())));
            };
            Controls.Add(FontColorBox);
            this.Size = new Size(this.Width - ComponentPadding.Horizontal, FontColorBox.Bottom + ComponentPadding.Bottom);
            FadeTimer.Interval = 10;
            FadeTimer.Tick += (s, args) =>
            {
                if (FontColorBox.Checked) {
                    ColorBrightness = 0;
                    ColorAlpha += 5;
                    ColorAlpha = ColorAlpha % 256;
                }
                else 
                    ColorBrightness = 255;
                FontColorBox.ForeColor= Color.FromArgb(ColorBrightness, ColorBrightness, ColorBrightness);
                FontColorBox.BackColor = Color.FromArgb(FontColorBox.Checked ? ColorAlpha : 255, FontColor);
                FontColorBox.Invalidate();
            };
            FadeTimer.Start();
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
