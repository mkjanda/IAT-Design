using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    partial class StimulusEdit : UserControl
    {
        private bool bUpdatingFromCode;
        private ComboBox FontFamilyCombo, FontColorCombo, FontSizeCombo;
        private RadioButton TextRadio, ImageRadio;
        private Panel StimulusTypePanel;
        private TextBox ImageFilename, StimulusText, StimulusName;
        private Button BrowseButton, SelectButton;
        private RadioButton LeftKeyed, RightKeyed;
        private enum EState { expanded, collapsed, uninitialized };
        private EState State;
        private static Padding CombomMargin = new Padding(5, 10, 5, 10), OutlinePadding = new Padding(12, 5, 3, 5);
        private static Padding ButtonPadding = new Padding(0);
        private static int TypeRadioSpacing = 30;
        private static int ControlLabelSpacing = 3;
        private static int ControlVerticalSpacing = 10;
        private static int ComboSpacing = 10;
        private CheckBox SizeImageToFit;
        private CDisplayItem _StoredStimulus;
        private IIATImage StimulusImage;

        public delegate void StimulusChangedEventHandler(StimulusEdit sender, CDisplayItem newStimulus);
        public delegate void KeyedDirectionChangedEventHandler(StimulusEdit sender, KeyedDirection keyedDir);
        public StimulusChangedEventHandler OnStimulusChanged;
        public KeyedDirectionChangedEventHandler OnKeyedDirectionChanged;

        public String StimulusFontFace
        {
            set
            {
                if (FontFamilyCombo.Items.Count == 0)
                    return;
                for (int ctr = 0; ctr < FontFamilyCombo.Items.Count; ctr++)
                {
                    if (((CFontComboItem)FontFamilyCombo.Items[ctr]).FontFaceName == value)
                    {
                        FontFamilyCombo.SelectedItem = FontFamilyCombo.Items[ctr];
                        break;
                    }
                }
            }
        }

        public float StimulusFontSize
        {
            get
            {
                String sFontSize = FontSizeCombo.SelectedItem.ToString();
                sFontSize = sFontSize.Substring(0, sFontSize.Length - "pt".Length);
                return Convert.ToSingle(sFontSize);
            }
            set
            {
                for (int ctr = 0; ctr < StimulusPanel.FontSizes.Length; ctr++)
                {
                    if (value <= StimulusPanel.FontSizes[ctr])
                    {
                        FontSizeCombo.SelectedIndex = ctr;
                        break;
                    }
                }
            }
        }

        public System.Drawing.Color StimulusColor
        {
            set
            {
                for (int ctr = 0; ctr < FontColorCombo.Items.Count; ctr++)
                {
                    if (value == ((CColorComboItem)FontColorCombo.Items[ctr]).ItemColor)
                    {
                        FontColorCombo.SelectedItem = FontColorCombo.Items[ctr];
                        break;
                    }
                }
            }
        }

        public CDisplayItem StoredStimulus
        {
            get {
                return _StoredStimulus;
            }
            set {
                if ((_StoredStimulus != null) && (_StoredStimulus != value))
                    _StoredStimulus.Dispose();
                _StoredStimulus = value;
            }
        }
                   

        public CDisplayItem Stimulus
        {
            get {
                if (StoredStimulus != null)
                    return StoredStimulus;
                CDisplayItem result;
                if (TextRadio.Checked)
                {
                    CTextDisplayItem textStimulus = new CTextDisplayItem(CTextDisplayItem.EUsedAs.stimulus);
                    textStimulus.Phrase = StimulusText.Text;
                    textStimulus.PhraseFontFamily = ((CFontComboItem)FontFamilyCombo.SelectedItem).FontFaceName;
                    textStimulus.PhraseFontSize = Convert.ToSingle(FontSizeCombo.Text.Substring(0, FontSizeCombo.Text.Length - 2));
                    textStimulus.PhraseColor = ((CColorComboItem)FontColorCombo.SelectedItem).ItemColor;
                    result = textStimulus;
                }
                else if (ImageRadio.Checked)
                {
                    CStimulusImageItem imageStimulus = new CStimulusImageItem(StimulusImage);
                    imageStimulus.StretchToFit = SizeImageToFit.Checked;
                    imageStimulus.Description = StimulusName.Text;
                    result = imageStimulus;
                }
                else 
                    result = null;
                StoredStimulus = result;
                return result;
            }
            set {
                bUpdatingFromCode = true;
                StoredStimulus = value;
                if (value == null)
                {
                    StimulusName.Text = "New Stimulus";
                    ImageRadio.Checked = false;
                    TextRadio.Checked = false;
                    StimulusText.Text = String.Empty;
                    LeftKeyed.Checked = false;
                    RightKeyed.Checked = false;
                    ImageFilename.Text = String.Empty;
                    SizeImageToFit.Checked = false;
                    Invalidate();
                } else {
                    if (value.Type == CDisplayItem.EType.stimulusImage)
                    {
                        CStimulusImageItem stimulus = (CStimulusImageItem)value;
                        StimulusName.Text = stimulus.Description;
                        TextRadio.Checked = false;
                        ImageRadio.Checked = true;
                        ImageFilename.Text = stimulus.FullFilePath;
                        SizeImageToFit.Checked = stimulus.StretchToFit;
                    }
                    else if (value.Type == CDisplayItem.EType.text)
                    {
                        CTextDisplayItem stimulus = (CTextDisplayItem)value;
                        StimulusName.Text = stimulus.Phrase;
                        TextRadio.Checked = true;
                        ImageRadio.Checked = false;
                        StimulusText.Text = stimulus.Phrase;
                        for (int ctr = 0; ctr < FontFamilyCombo.Items.Count; ctr++)
                            if (((CFontComboItem)FontFamilyCombo.Items[ctr]).FontFaceName == stimulus.PhraseFontFamily)
                            {
                                FontFamilyCombo.SelectedIndex = ctr;
                                break;
                            }
                        for (int ctr = 0; ctr < FontSizeCombo.Items.Count; ctr++)
                            if (Convert.ToInt32(FontSizeCombo.Items[ctr].ToString().Substring(0, FontSizeCombo.Items[ctr].ToString().Length - 2)) == Convert.ToInt32(stimulus.PhraseFontSize))
                            {
                                FontSizeCombo.SelectedIndex = ctr;
                                break;
                            }
                        for (int ctr = 0; ctr < FontColorCombo.Items.Count; ctr++)
                            if (((CColorComboItem)FontColorCombo.Items[ctr]).ItemColor == stimulus.PhraseColor)
                            {
                                FontColorCombo.SelectedIndex = ctr;
                                break;
                            }
                    }
                    else
                    {
                        StoredStimulus = null;
                        throw new Exception("Unexpected CDisplayItem type while setting image in a StimulusEdit.");
                    }
                }
                Invalidate();
                bUpdatingFromCode = false;
            }
        }

        public String Description
        {
            get
            {
                return StimulusName.Text;
            }
        }

        public KeyedDirection KeyedDir
        {
            get
            {
                if (LeftKeyed.Checked)
                    return KeyedDirection.Left;
                else if (RightKeyed.Checked)
                    return KeyedDirection.Right;
                else
                    return KeyedDirection.None;
            }
        }

        public bool IsLeftKeyed
        {
            get
            {
                return LeftKeyed.Checked;
            }
            set
            {
                RightKeyed.Checked = false;
                LeftKeyed.Checked = true;
            }
        }

        public bool IsRightKeyed
        {
            get
            {
                return RightKeyed.Checked;
            }
            set
            {
                LeftKeyed.Checked = false;
                RightKeyed.Checked = true;
            }
        }

        new public StimulusPanel Parent
        {
            get
            {
                if (base.Parent == null)
                    return null;
                else
                    return (StimulusPanel)(base.Parent);
            }
        }

        public override System.Drawing.Color BackColor
        {
            get
            {
                if (Parent == null)
                    return base.BackColor;
                return Parent.ChildBackColor;
            }
        }

        public System.Drawing.Color DataFontColor
        {
            get
            {
                return Parent.DataFontColor;
            }
        }

        public System.Drawing.Color LabelFontColor
        {
            get
            {
                return Parent.LabelFontColor;
            }
        }

        public System.Drawing.Color InstructionsFontColor
        {
            get
            {
                return Parent.InstructionsFontColor;
            }
        }

        public System.Drawing.Color ControlForeColor
        {
            get
            {
                return Parent.SubControlForeColor;
            }
        }

        public System.Drawing.Color StimulusNameColor
        {
            get
            {
                return Parent.StimulusNameColor;
            }
        }

        public Font LabelFont
        {
            get
            {
                return Parent.LabelFont;
            }
        }

        public Font DataFont
        {
            get
            {
                return Parent.DataFont;
            }
        }

        public Font InstructionsFont
        {
            get
            {
                return Parent.InstructionsFont;
            }
        }

        public StimulusEdit()
        {
            bUpdatingFromCode = true;
            InitializeComponent();
            FontFamilyCombo = new ComboBox();
            FontFamilyCombo.DrawMode = DrawMode.OwnerDrawVariable;
            FontFamilyCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            FontFamilyCombo.MeasureItem += new MeasureItemEventHandler(FontCombo_MeasureItem);
            FontFamilyCombo.DrawItem += new DrawItemEventHandler(FontCombo_DrawItem);
            FontFamilyCombo.SelectedIndexChanged += new EventHandler(FontFamily_Changed);
            FontColorCombo = new ComboBox();
            FontColorCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            FontColorCombo.DrawMode = DrawMode.OwnerDrawVariable;
            FontColorCombo.MeasureItem += new MeasureItemEventHandler(ColorCombo_MeasureItem);
            FontColorCombo.DrawItem += new DrawItemEventHandler(ColorCombo_DrawItem);
            FontColorCombo.SelectedIndexChanged += new EventHandler(FontColor_Changed);
            FontSizeCombo = new ComboBox();
            FontSizeCombo.DrawMode = DrawMode.OwnerDrawFixed;
            FontSizeCombo.DrawItem += new DrawItemEventHandler(FontSizeCombo_DrawItem);
            FontSizeCombo.SelectedIndexChanged += new EventHandler(TextStimulus_Changed);
            ImageFilename = new TextBox();
            ImageFilename.ReadOnly = true;
            StimulusText = new TextBox();
            StimulusText.TextChanged += new EventHandler(TextStimulus_Changed);
            StimulusText.Leave += new EventHandler(StimulusText_Leave);
            StimulusName = new TextBox();
            StimulusName.ReadOnly = true;
            StimulusName.BorderStyle = BorderStyle.None;
            StimulusName.TextAlign = HorizontalAlignment.Center;
            StimulusName.HideSelection = true;
            StimulusName.Text = "New Stimulus";
            StimulusName.Click += new EventHandler(StimulusName_Click);
            BrowseButton = new Button();
            BrowseButton.Text = "Browse";
            BrowseButton.Padding = ButtonPadding;
            BrowseButton.Click += new EventHandler(BrowseButton_Click);
            SelectButton = new Button();
            SelectButton.Text = "Select";
            SelectButton.Padding = ButtonPadding;
            SelectButton.Click += new EventHandler(SelectButton_Click);
            StimulusTypePanel = new Panel();
            TextRadio = new RadioButton();
            TextRadio.Size = new Size(16, 16);
            TextRadio.Click += new EventHandler(StimulusType_Changed);
            StimulusTypePanel.Controls.Add(TextRadio);
            ImageRadio = new RadioButton();
            ImageRadio.Size = new Size(16, 16);
            ImageRadio.Click += new EventHandler(StimulusType_Changed);
            StimulusTypePanel.Controls.Add(ImageRadio);
            StimulusTypePanel.Paint += new PaintEventHandler(StimulusTypePanel_Paint);
            SizeImageToFit = new CheckBox();
            SizeImageToFit.Size = new Size(16, 16);
            LeftKeyed = new RadioButton();
            LeftKeyed.Size = new Size(16, 16);
            LeftKeyed.Checked = false;
            LeftKeyed.CheckedChanged += new EventHandler(KeyedDir_Changed);
            RightKeyed = new RadioButton();
            RightKeyed.Size = new Size(16, 16);
            RightKeyed.Checked = false;
            RightKeyed.CheckedChanged += new EventHandler(KeyedDir_Changed);
            State = EState.uninitialized;
            this.Padding = new Padding(5);
            OnStimulusChanged = null;
            OnKeyedDirectionChanged = null;
            this.KeyUp += new KeyEventHandler(StimulusEdit_KeyUp);
            StimulusImage = null;
            _StoredStimulus = null;
            InitFontSizeCombo();
            bUpdatingFromCode = false;
        }

        void FontSizeCombo_DrawItem(object sender, DrawItemEventArgs e)
        {
            Brush backBr = new SolidBrush(e.BackColor);
            e.Graphics.FillRectangle(backBr, e.Bounds);
            Brush txtBrush = new SolidBrush(DataFontColor);
            e.Graphics.DrawString(FontSizeCombo.Items[e.Index].ToString(), FontSizeCombo.Font, txtBrush, e.Bounds.Location);
            txtBrush.Dispose();
            backBr.Dispose();
        }

        void StimulusText_Leave(object sender, EventArgs e)
        {
            StimulusName.Text = StimulusText.Text;
        }

        void StimulusEdit_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                if (this == Parent.ActiveStimulusEdit)
                    Parent.DeleteActiveStimulus();
            }
        }

        void KeyedDir_Changed(object sender, EventArgs e)
        {
            if (bUpdatingFromCode)
                return;
            if (LeftKeyed.Checked)
                OnKeyedDirectionChanged(this, CIATItem.EKeyedDir.Left);
            else if (RightKeyed.Checked)
                OnKeyedDirectionChanged(this, CIATItem.EKeyedDir.Right);
            else
                OnKeyedDirectionChanged(this, CIATItem.EKeyedDir.None);
        }


        private void InitFontSizeCombo()
        {
            FontSizeCombo.Items.Clear();
            for (int ctr = 0; ctr < StimulusPanel.FontSizes.Length; ctr++)
                FontSizeCombo.Items.Add(String.Format("{0:F00}pt", StimulusPanel.FontSizes[ctr]));
        }

        private void StimulusType_Changed(object sender, EventArgs e)
        {
            SuspendLayout();
            RecalcLayout();
            ResumeLayout();
            Invalidate();
        }

        private void StimulusName_Click(object sender, EventArgs e)
        {
            if (State == EState.collapsed)
                Expand();
            else if (State == EState.expanded)
                Collapse();
        }


        private void FontFamily_Changed(object sender, EventArgs e)
        {
            if (bUpdatingFromCode)
                return;
            CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextStimulus].FontFamily = ((CFontComboItem)FontFamilyCombo.SelectedItem).FontFaceName;
            TextStimulus_Changed(sender, e);
        }

        private void FontColor_Changed(object sender, EventArgs e)
        {
            if (bUpdatingFromCode)
                return;
            CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextStimulus].FontColor = ((CColorComboItem)FontColorCombo.SelectedItem).ItemColor;
            TextStimulus_Changed(sender, e);
        }

        private void FontSize_Changed(object sender, EventArgs e)
        {
            if (bUpdatingFromCode)
                return;
            CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextStimulus].FontSize = StimulusFontSize;
            TextStimulus_Changed(sender, e);
        }

        private void TextStimulus_Changed(object sender, EventArgs e)
        {
            if (bUpdatingFromCode)
                return;
            StoredStimulus = null;
            OnStimulusChanged(this, Stimulus);
        }
        
        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = Properties.Resources.sOpenImageFileDialogTitle;
            dialog.Filter = Properties.Resources.sImageFileFilter;
            dialog.FilterIndex = 0;
            dialog.AddExtension = false;
            if (dialog.ShowDialog() != DialogResult.OK)
                return;
            System.IO.FileInfo fi = new System.IO.FileInfo(dialog.FileName);
            if (fi.Length > ImageManager.CImageManager.MaxImageFileSize)
                if (MessageBox.Show(Properties.Resources.sLargeImageFile, "Large Image File", MessageBoxButtons.YesNo) == DialogResult.No)
                    return;
            if (StimulusImage != null)
            {
                if (StimulusImage.IsUserImage)
                    ((IUserImage)Stimulus).Dispose();
                else
                    ((INonUserImage)Stimulus).Dispose((CTextDisplayItem)Stimulus);
            }
            StimulusImage = CIAT.ImageManager[CIAT.ImageManager.AddImage(dialog.FileName, new IATClient.ImageManager.ImageSizeCallback(CStimulusImageItem.CalcImageSize))];
            StoredStimulus = null;
            ImageFilename.Text = dialog.FileName;
            StimulusName.Text = System.IO.Path.GetFileNameWithoutExtension(dialog.FileName);
            OnStimulusChanged(this, Stimulus);
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            ImageBrowser browser = new ImageBrowser();
            if (browser.ShowDialog(this) == DialogResult.OK)
            {
                if (StimulusImage != null)
                {
                    if (StimulusImage.IsUserImage)
                        ((IUserImage)Stimulus).Dispose();
                    else
                        ((INonUserImage)Stimulus).Dispose((CTextDisplayItem)Stimulus);
                }
                StimulusImage = CIAT.ImageManager[browser.SelectedImageNdx];
                StoredStimulus = null;
                StimulusImage.CreateCopy();
                String str = ((IUserImage)StimulusImage).FullFilePath;
                if (str == String.Empty)
                    ImageFilename.Text = Properties.Resources.sImageLoadedFromSaveFile;
                else
                    ImageFilename.Text = str;
                StimulusName.Text = System.IO.Path.GetFileNameWithoutExtension(ImageFilename.Text);
                OnStimulusChanged(this, Stimulus);
            }
        }

        public void InitFontFamilyCombo(CFontComboItem[] fontNameItems)
        {
            FontFamilyCombo.SuspendLayout();
            FontFamilyCombo.Items.AddRange(fontNameItems);
            FontFamilyCombo.ResumeLayout();
        }

        public void InitFontColorCombo(CColorComboItem[] colorItems)
        {
            FontColorCombo.SuspendLayout();
            FontColorCombo.Items.AddRange(colorItems);
            FontColorCombo.ResumeLayout();
        }

        private void FontCombo_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            Size sz = ((CFontComboItem)FontFamilyCombo.Items[e.Index]).Measure();
            e.ItemHeight = sz.Height;
            e.ItemWidth = sz.Width;
        }

        private void FontCombo_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                if (e.State == DrawItemState.ComboBoxEdit)
                {
                    Brush backBrush = new SolidBrush(e.BackColor);
                    e.Graphics.FillRectangle(backBrush, e.Bounds);
                    backBrush.Dispose();
                }
                return;
            }
            CFontComboItem ci = (CFontComboItem)FontFamilyCombo.Items[e.Index];
            if ((!FontFamilyCombo.DroppedDown) || (e.State == DrawItemState.ComboBoxEdit))
            {
                Brush br = new SolidBrush(DataFontColor);
                Brush backBrush = new SolidBrush(e.BackColor);
                e.Graphics.FillRectangle(backBrush, e.Bounds);
                e.Graphics.DrawString(ci.FontFaceName, FontFamilyCombo.Font, br, e.Bounds.Location);
                br.Dispose();
                backBrush.Dispose();
            }
            else
            {
                Brush backBrush = new SolidBrush(e.BackColor);
                ci.Draw(e.Graphics, backBrush, e.Bounds);
                backBrush.Dispose();
            }
        }

        private void ColorCombo_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            Size sz = ((CColorComboItem)FontColorCombo.Items[e.Index]).Measure();
            e.ItemHeight = sz.Height;
            e.ItemWidth = sz.Width;
        }

        private void ColorCombo_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
            {
                if (e.State == DrawItemState.ComboBoxEdit)
                {
                    Brush eraseBr = new SolidBrush(e.BackColor);
                    e.Graphics.FillRectangle(eraseBr, e.Bounds);
                    eraseBr.Dispose();
                }
                return;
            }
            CColorComboItem ci = (CColorComboItem)FontColorCombo.Items[e.Index];
            Brush br = new SolidBrush(e.BackColor);
            ci.Draw(e.Graphics, br, e.Bounds);
            br.Dispose();
        }

        private void StimulusEdit_Load(object sender, EventArgs e)
        {
            // set background colors
            base.BackColor = BackColor;
            FontFamilyCombo.BackColor = BackColor;
            FontColorCombo.BackColor = BackColor;
            FontSizeCombo.BackColor = BackColor;
            StimulusTypePanel.BackColor = BackColor;
            ImageFilename.BackColor = BackColor;
            StimulusText.BackColor = BackColor;
            StimulusName.BackColor = BackColor;
            BrowseButton.BackColor = BackColor;
            LeftKeyed.BackColor = BackColor;
            RightKeyed.BackColor = BackColor;
            SizeImageToFit.BackColor = BackColor;
            TextRadio.BackColor = BackColor;
            ImageRadio.BackColor = BackColor;

            // set fore colors
            base.ForeColor = ControlForeColor;
  //          FontFamilyCombo.ForeColor = ControlForeColor;
  //          FontColorCombo.ForeColor = ControlForeColor;
            FontSizeCombo.ForeColor = DataFontColor;
            StimulusTypePanel.ForeColor = ControlForeColor;
            ImageFilename.ForeColor = ControlForeColor;
            StimulusText.ForeColor = ControlForeColor;
            StimulusName.ForeColor = StimulusNameColor;
            BrowseButton.ForeColor = DataFontColor;
            SelectButton.ForeColor = DataFontColor;
            LeftKeyed.ForeColor = ControlForeColor;
            RightKeyed.ForeColor = ControlForeColor;
            SizeImageToFit.ForeColor = ControlForeColor;
            TextRadio.ForeColor = ControlForeColor;
            ImageRadio.ForeColor = ControlForeColor;

            // set fonts
            FontFamilyCombo.Font = LabelFont;
            FontSizeCombo.Font = LabelFont;
            FontColorCombo.Font = LabelFont;
            ImageFilename.Font = DataFont;
            StimulusText.Font = DataFont;
            StimulusName.Font = DataFont;
            BrowseButton.Font = LabelFont;
            SelectButton.Font = LabelFont;

            Collapse();   
        }

        private void StimulusEdit_Click(object sender, EventArgs e)
        {
            if (State == EState.collapsed)
                Expand();
            else
                Collapse();
        }

        public void Collapse()
        {
            SuspendLayout();
            State = EState.collapsed;
            Controls.Clear();
            Controls.Add(StimulusName);
            Controls.Add(LeftKeyed);
            Controls.Add(RightKeyed);
            RecalcLayout();
            ResumeLayout();
            Parent.RecalcLayout();
        }

        public void Expand()
        {
            SuspendLayout();
            State = EState.expanded;
            Controls.Clear();
            Controls.Add(StimulusName);
            Controls.Add(LeftKeyed);
            Controls.Add(RightKeyed);
            Controls.Add(StimulusTypePanel);
            if (ImageRadio.Checked)
            {
                Controls.Add(BrowseButton);
                Controls.Add(ImageFilename);
            }
            else if (TextRadio.Checked)
            {
                Controls.Add(FontFamilyCombo);
                Controls.Add(FontSizeCombo);
                Controls.Add(FontColorCombo);
                Controls.Add(StimulusText);
            }
            RecalcLayout();
            ResumeLayout();
            Parent.SetActiveEdit(this);
            Parent.RecalcLayout();
        }

        public void RecalcLayout()
        {
            BrowseButton.Font = LabelFont;
            StimulusName.Font = DataFont;
            StimulusText.Font = DataFont;
            ImageFilename.Font = DataFont;
            int nWidth = Parent.ClientSize.Width;
            int nHeight = Parent.StimulusEditPadding.Top;
            Size szText;
            LeftKeyed.Location = new Point(Padding.Left, nHeight + (LeftKeyed.Height > LabelFont.Height ? 0 : ((LabelFont.Height - LeftKeyed.Height) >> 1)));
            RightKeyed.Location = new Point(nWidth - Padding.Right - RightKeyed.Size.Width, nHeight + (RightKeyed.Height > LabelFont.Height ? 0 : ((LabelFont.Height - RightKeyed.Height) >> 1)));
            if (DataFont.Height > LabelFont.Height)
            {
                LeftKeyed.Location = LeftKeyed.Location + new Size(0, (DataFont.Height - LabelFont.Height) >> 1);
                RightKeyed.Location = RightKeyed.Location + new Size(0, (DataFont.Height - LabelFont.Height) >> 1);
            }
            szText = TextRenderer.MeasureText("Left Keyed", LabelFont) + TextRenderer.MeasureText("Right Keyed", LabelFont) + LeftKeyed.Size + RightKeyed.Size;
            StimulusName.Size = new Size(nWidth - szText.Width - ((ComboSpacing + ControlLabelSpacing) * 2), DataFont.Height);
            StimulusName.Location = new Point((nWidth - StimulusName.Size.Width) >> 1, nHeight + (DataFont.Height > LeftKeyed.Height ? 0 : ((LeftKeyed.Height - DataFont.Height) >> 1)));
            nHeight += (DataFont.Height > LabelFont.Height) ? DataFont.Height : LabelFont.Height;
            nHeight += ControlVerticalSpacing;
            if (State == EState.expanded)
            {
                if (!Controls.Contains(StimulusTypePanel))
                    Controls.Add(StimulusTypePanel);

                // layout stimulus type panel
                OutlinePadding.Top = OutlinePadding.Bottom + InstructionsFont.Height;
                StimulusTypePanel.Size = new Size(ImageRadio.Size.Width + TextRadio.Size.Width + TextRenderer.MeasureText("Image", LabelFont).Width +
                    TextRenderer.MeasureText("Text", LabelFont).Width + TypeRadioSpacing + (ControlLabelSpacing << 1) + OutlinePadding.Horizontal,
                    OutlinePadding.Vertical + (LabelFont.Height >> 1) + (TextRadio.Size.Height > LabelFont.Height ? TextRadio.Size.Height : LabelFont.Height));
                StimulusTypePanel.Location = new Point((nWidth - StimulusTypePanel.Width) >> 1, nHeight);
                nHeight += StimulusTypePanel.Height + ControlVerticalSpacing;
                TextRadio.Location = new Point(OutlinePadding.Left, OutlinePadding.Top +
                    (TextRadio.Size.Height > LabelFont.Height ? 0 : ((LabelFont.Height - TextRadio.Size.Height) >> 1)));
                ImageRadio.Location = new Point(OutlinePadding.Left + TextRadio.Size.Width + TextRenderer.MeasureText("Text", LabelFont).Width +
                    TypeRadioSpacing + ControlLabelSpacing, OutlinePadding.Top + 
                    (ImageRadio.Size.Height > LabelFont.Height ? 0 : ((LabelFont.Height - ImageRadio.Size.Height) >> 1)));

                int nImageControlsHeight = 0, nTextControlsHeight = 0;
                
                // size text stimulus controls
                int nComboHeight = DataFont.Height > Parent.ColorRectSize.Height ? DataFont.Height : Parent.ColorRectSize.Height;
                FontFamilyCombo.ClientSize = new Size(Parent.MaxFontFaceWidth + nComboHeight, nComboHeight);
                FontFamilyCombo.DropDownWidth = Parent.MaxFontFaceImageWidth;
                FontSizeCombo.ClientSize = new Size(Parent.MaxFontSizeWidth + nComboHeight, nComboHeight);
                FontColorCombo.ClientSize = new Size(Parent.MaxColorWidth + nComboHeight, nComboHeight);
                int nComboWidths = FontFamilyCombo.Width + FontSizeCombo.Width + FontColorCombo.Width + (ComboSpacing * 2)
                    + TextRenderer.MeasureText("Font:", LabelFont).Width + TextRenderer.MeasureText("Size:", LabelFont).Width + 
                    TextRenderer.MeasureText("Color:", LabelFont).Width + (ControlLabelSpacing * 3);
                    
                StimulusText.Width = nComboWidths;
                StimulusText.ClientSize = new Size(StimulusText.ClientSize.Width - TextRenderer.MeasureText("Stimulus Text:", LabelFont).Width, DataFont.Height);
                nTextControlsHeight = FontFamilyCombo.Height + ControlVerticalSpacing + StimulusText.Height;

                // size image stimulus controls
                Size szBrowse, szSelect, szImageButtons;
                szBrowse = TextRenderer.MeasureText(BrowseButton.Text, LabelFont) + new Size(23 - System.Drawing.SystemFonts.DialogFont.Height, 23 - System.Drawing.SystemFonts.DialogFont.Height);
                szSelect = TextRenderer.MeasureText(SelectButton.Text, LabelFont) + new Size(23 - System.Drawing.SystemFonts.DialogFont.Height, 23 - System.Drawing.SystemFonts.DialogFont.Height);
                szImageButtons = new Size(szBrowse.Width > szSelect.Width ? szBrowse.Width : szSelect.Width, szBrowse.Height);
                BrowseButton.ClientSize = szImageButtons + new Size(BrowseButton.Padding.Horizontal, BrowseButton.Padding.Vertical);
                SelectButton.ClientSize = szImageButtons + new Size(SelectButton.Padding.Horizontal, SelectButton.Padding.Vertical) + SelectButton.MinimumSize;
                ImageFilename.Size = new Size((2 * (nComboWidths - TextRenderer.MeasureText("Image Filename:", LabelFont).Width) / 3) - (ComboSpacing >> 1), ImageFilename.Height);
                ImageFilename.ClientSize = new Size(ImageFilename.ClientSize.Width, DataFont.Height);
                nImageControlsHeight = BrowseButton.Height > ImageFilename.Height ? BrowseButton.Height : ImageFilename.Height;
                nImageControlsHeight += ControlVerticalSpacing + (SizeImageToFit.Height > LabelFont.Height ? SizeImageToFit.Height : LabelFont.Height);


                if (TextRadio.Checked)
                {
                    SuspendLayout();
                    if (Controls.Contains(ImageFilename))
                        Controls.Remove(ImageFilename);
                    if (Controls.Contains(BrowseButton))
                        Controls.Remove(BrowseButton);
                    if (Controls.Contains(SelectButton))
                        Controls.Remove(SelectButton);
                    if (Controls.Contains(SizeImageToFit))
                        Controls.Remove(SizeImageToFit);
                    if (!Controls.Contains(FontFamilyCombo))
                        Controls.Add(FontFamilyCombo);
                    if (!Controls.Contains(FontSizeCombo))
                        Controls.Add(FontSizeCombo);
                    if (!Controls.Contains(FontColorCombo))
                        Controls.Add(FontColorCombo);
                    if (!Controls.Contains(StimulusText))
                        Controls.Add(StimulusText);
                    int nLeftOffset = (nWidth - nComboWidths) >> 1;
                    FontFamilyCombo.Location = new Point(nLeftOffset + TextRenderer.MeasureText("Font:", LabelFont).Width + ControlLabelSpacing, nHeight);
                    FontSizeCombo.Location = FontFamilyCombo.Location + new Size(FontFamilyCombo.Width + TextRenderer.MeasureText("Size:", LabelFont).Width + 
                        ControlLabelSpacing + ComboSpacing, 0);
                    FontColorCombo.Location = FontSizeCombo.Location + new Size(FontSizeCombo.Width + ComboSpacing + ControlLabelSpacing + 
                        TextRenderer.MeasureText("Color:", LabelFont).Width, 0);
                    StimulusText.Location = new Point(nLeftOffset + TextRenderer.MeasureText("Stimulus Text:", LabelFont).Width, nHeight + ControlVerticalSpacing + (LabelFont.Height > FontFamilyCombo.Height ? LabelFont.Height : FontFamilyCombo.Height));
                    ResumeLayout(false);
                }
                else if (ImageRadio.Checked)
                {
                    SuspendLayout();
                    if (!Controls.Contains(ImageFilename))
                        Controls.Add(ImageFilename);
                    if (!Controls.Contains(BrowseButton))
                        Controls.Add(BrowseButton);
                    if (!Controls.Contains(SelectButton))
                        Controls.Add(SelectButton);
                    if (!Controls.Contains(SizeImageToFit))
                        Controls.Add(SizeImageToFit);
                    if (Controls.Contains(FontFamilyCombo))
                        Controls.Remove(FontFamilyCombo);
                    if (Controls.Contains(FontSizeCombo))
                        Controls.Remove(FontSizeCombo);
                    if (Controls.Contains(FontColorCombo))
                        Controls.Remove(FontColorCombo);
                    if (Controls.Contains(StimulusText))
                        Controls.Remove(StimulusText);
                    int nLeftOffset = (nWidth - nComboWidths) >> 2;
                    ImageFilename.Location = new Point(nLeftOffset + TextRenderer.MeasureText("Image Filename:", LabelFont).Width, nHeight + (ImageFilename.Height > BrowseButton.Height ? 0 : ((BrowseButton.Height - ImageFilename.Height) >> 1)));
                    SizeImageToFit.Location = new Point(nLeftOffset,
                        nHeight + ControlVerticalSpacing + ImageFilename.Height + (LabelFont.Height > BrowseButton.Height ? ((LabelFont.Height - SizeImageToFit.Height) >> 1)
                        : ((BrowseButton.Height - SizeImageToFit.Height) >> 1)));
                    Size szSizeToFitLabel = TextRenderer.MeasureText("Size image to fit stimulus area", LabelFont);
                    SelectButton.Location = new Point(((ImageFilename.Right - (SizeImageToFit.Right + szSizeToFitLabel.Width) - ComboSpacing - SelectButton.Width - BrowseButton.Width) >> 1)
                        + (ComboSpacing >> 1) + SelectButton.Width + SizeImageToFit.Right + szSizeToFitLabel.Width, nHeight + ImageFilename.Height + ControlVerticalSpacing + (BrowseButton.Height > LabelFont.Height ? 0 :
                        ((LabelFont.Height - BrowseButton.Height) >> 1)));
                    BrowseButton.Location = new Point(SelectButton.Left - ComboSpacing - BrowseButton.Width, nHeight + ImageFilename.Height + ControlVerticalSpacing + (BrowseButton.Height > LabelFont.Height ? 0 :
                        ((LabelFont.Height - BrowseButton.Height) >> 1)));
                    ResumeLayout(false);
                }
                else
                {
                    SuspendLayout();
                    if (Controls.Contains(ImageFilename))
                        Controls.Remove(ImageFilename);
                    if (Controls.Contains(BrowseButton))
                        Controls.Remove(BrowseButton);
                    if (Controls.Contains(SelectButton))
                        Controls.Remove(SelectButton);
                    if (Controls.Contains(SizeImageToFit))
                        Controls.Remove(SizeImageToFit);
                    if (Controls.Contains(FontFamilyCombo))
                        Controls.Remove(FontFamilyCombo);
                    if (Controls.Contains(FontSizeCombo))
                        Controls.Remove(FontSizeCombo);
                    if (Controls.Contains(FontColorCombo))
                        Controls.Remove(FontColorCombo);
                    if (Controls.Contains(StimulusText))
                        Controls.Remove(StimulusText);
                    ResumeLayout(false);
                }
                nHeight += nImageControlsHeight > nTextControlsHeight ? nImageControlsHeight : nTextControlsHeight;
            }
            else if (State == EState.collapsed)
            {
                SuspendLayout();
                if (Controls.Contains(ImageFilename))
                    Controls.Remove(ImageFilename);
                if (Controls.Contains(BrowseButton))
                    Controls.Remove(BrowseButton);
                if (Controls.Contains(SelectButton))
                    Controls.Remove(SelectButton);
                if (Controls.Contains(SizeImageToFit))
                    Controls.Remove(SizeImageToFit);
                if (Controls.Contains(FontFamilyCombo))
                    Controls.Remove(FontFamilyCombo);
                if (Controls.Contains(FontSizeCombo))
                    Controls.Remove(FontSizeCombo);
                if (Controls.Contains(FontColorCombo))
                    Controls.Remove(FontColorCombo);
                if (Controls.Contains(StimulusText))
                    Controls.Remove(StimulusText);
                if (Controls.Contains(StimulusTypePanel))
                    Controls.Remove(StimulusTypePanel);
                ResumeLayout(false);
            }
            nHeight += ControlVerticalSpacing + Parent.StimulusEditPadding.Bottom;
            this.Size = new Size(nWidth, nHeight);
            Invalidate();
        }

        private void StimulusEdit_Paint(object sender, PaintEventArgs e)
        {
            Brush backBr = new SolidBrush(BackColor);
            e.Graphics.FillRectangle(backBr, ClientRectangle);
            backBr.Dispose();
            Brush LabelBr = new SolidBrush(LabelFontColor);
            Point ptText = new Point(LeftKeyed.Right + ControlLabelSpacing, Padding.Top + (LabelFont.Height > LeftKeyed.Height ? 0 : (LeftKeyed.Height - LabelFont.Height) >> 1));
            e.Graphics.DrawString("Left Keyed", LabelFont, LabelBr, ptText);
            ptText.X = RightKeyed.Left - ControlLabelSpacing - TextRenderer.MeasureText("Right Keyed", LabelFont).Width;
            e.Graphics.DrawString("Right Keyed", LabelFont, LabelBr, ptText);
            if (State == EState.expanded)
            {
                if (TextRadio.Checked)
                {

                    ptText.Y = StimulusTypePanel.Bottom + ControlVerticalSpacing + (LabelFont.Height > FontFamilyCombo.Height ? 0 : (FontFamilyCombo.Height - LabelFont.Height) >> 1);
                    ptText.X = FontFamilyCombo.Left - ControlLabelSpacing - TextRenderer.MeasureText("Font:", LabelFont).Width;
                    e.Graphics.DrawString("Font:", LabelFont, LabelBr, ptText);
                    ptText.X = FontSizeCombo.Left - ControlLabelSpacing - TextRenderer.MeasureText("Size:", LabelFont).Width;
                    e.Graphics.DrawString("Size:", LabelFont, LabelBr, ptText);
                    ptText.X = FontColorCombo.Left - ControlLabelSpacing - TextRenderer.MeasureText("Color:", LabelFont).Width;
                    e.Graphics.DrawString("Color:", LabelFont, LabelBr, ptText);

                    ptText.Y = StimulusText.Top + (LabelFont.Height > StimulusText.Height ? -1 : 1) * (Math.Abs(LabelFont.Height - StimulusText.Height) >> 1);
                    ptText.X = FontFamilyCombo.Left - ControlLabelSpacing - TextRenderer.MeasureText("Font:", LabelFont).Width;
                    e.Graphics.DrawString("Stimulus Text:", LabelFont, LabelBr, ptText);
                }
                else if (ImageRadio.Checked)
                {
                    ptText.Y = StimulusTypePanel.Bottom + ControlVerticalSpacing + (LabelFont.Height > ImageFilename.Height ? 0 : ((ImageFilename.Height - LabelFont.Height) >> 1));
                    ptText.X = ImageFilename.Left - ControlLabelSpacing - TextRenderer.MeasureText("Image Filename:", LabelFont).Width;
                    e.Graphics.DrawString("Image Filename:", LabelFont, LabelBr, ptText);
                    ptText.Y = SizeImageToFit.Top + (LabelFont.Height > SizeImageToFit.Height ? -1 : 1) * (Math.Abs(LabelFont.Height - SizeImageToFit.Height) >> 1);
                    ptText.X = SizeImageToFit.Right + ControlLabelSpacing;
                    e.Graphics.DrawString("Size image to fit stimulus area", LabelFont, LabelBr, ptText);
                    Brush instrBr = new SolidBrush(InstructionsFontColor);
                    Rectangle instrBounds = new Rectangle(ImageFilename.Right + ComboSpacing, StimulusTypePanel.Bottom, this.ClientSize.Width - ImageFilename.Right - (ComboSpacing), BrowseButton.Bottom);
                    e.Graphics.DrawString(Properties.Resources.sImageStimulusEditInstructions, InstructionsFont, instrBr, instrBounds);
                    instrBr.Dispose();
                }
            }
            LabelBr.Dispose();
            e.Graphics.DrawLine(Pens.Black, new Point(0, Height - 1), new Point(Width - 1, Height - 1));
        }

        private void StimulusTypePanel_Paint(object sender, PaintEventArgs e)
        {
            Brush backBr = new SolidBrush(StimulusTypePanel.BackColor);
            e.Graphics.FillRectangle(backBr, StimulusTypePanel.ClientRectangle);
            
            Point ptText = new Point(TextRadio.Right + ControlLabelSpacing, TextRadio.Top + (LabelFont.Height > TextRadio.Height ? -1 : 1) * (Math.Abs(LabelFont.Height - TextRadio.Height) >> 1));
            Brush LabelBr = new SolidBrush(LabelFontColor);
            e.Graphics.DrawString("Text", LabelFont, LabelBr, ptText);
            ptText.X = ImageRadio.Right + ControlLabelSpacing;
            e.Graphics.DrawString("Image", LabelFont, LabelBr, ptText);
            Brush instructionsBrush = new SolidBrush(InstructionsFontColor);
            Pen rectPen = new Pen(instructionsBrush);
            e.Graphics.DrawRectangle(rectPen, new Rectangle(0, (InstructionsFont.Height + 1) >> 1, StimulusTypePanel.Width - 1, StimulusTypePanel.Height - 1 - ((InstructionsFont.Height + 1) >> 1)));
            SizeF szLabel = e.Graphics.MeasureString("Stimulus Type", InstructionsFont);
            RectangleF labelRect = new RectangleF(StimulusTypePanel.Width >> 4, 0, szLabel.Width, InstructionsFont.Height);
            labelRect.Inflate(1, 0);
            e.Graphics.FillRectangle(backBr, labelRect);
            e.Graphics.DrawString("Stimulus Type", InstructionsFont, instructionsBrush, labelRect.Location + new SizeF(2, 0));
            rectPen.Dispose();
            instructionsBrush.Dispose();
            backBr.Dispose();
            LabelBr.Dispose();
        }
    }
}
