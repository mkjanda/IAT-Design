using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;
using System.IO;

namespace IATClient
{
    partial class StimulusDefinitionPanel : UserControl
    {
        public static Size StimulusDefinitionPanelSize = new Size(412, 215);
        private static int StimulusTextWidth = 392;
        private static int StimulusTextLines = 1;
        private static Point StimulusTextPos = new Point(5, 95);
        private bool IgnoreEvents { get; set; } = false;
        private bool bIsLoaded = false;
        private Uri ParentBlockUri;
        public KeyedDirection KeyedDir { get; set; } = KeyedDirection.None;
        public IATBlockPanel.IsDynamicallyKeyedCallback BlockDynamicallyKeyed;
        

        private const int MaxImageSize = 1 << 22;

        public delegate void KeyedDirChangedHandler(KeyedDirection KeyedDir);
        public KeyedDirChangedHandler OnKeyedDirChanged;

        private CIATItem DefinedItem = null;
        private TextEditControl StimulusText;

        public CIATItem IATItem
        {
            get
            {
                return DefinedItem;
            }
            set
            {
                if (DefinedItem != null)
                    if (DefinedItem.URI.Equals(value.URI))
                        return;
                IgnoreEvents = true;
                DefinedItem = value;
                KeyedDirection keyedDir = DefinedItem.GetKeyedDirection(ParentBlockUri);
                if (keyedDir == KeyedDirection.Left)
                {
                    KeyedLeft.Checked = true;
                    KeyedRight.Checked = false;
                }
                else if (keyedDir == KeyedDirection.Right)
                {
                    KeyedLeft.Checked = false;
                    KeyedRight.Checked = true;
                }
                else
                {
                    KeyedLeft.Checked = false;
                    KeyedRight.Checked = false;
                }
                if (BlockDynamicallyKeyed())
                {
                    KeyedLeft.Enabled = false;
                    KeyedRight.Enabled = false;
                }
                else
                {
                    KeyedLeft.Enabled = true;
                    KeyedRight.Enabled = true;
                }
                if (DefinedItem.Stimulus.Type == DIType.Null)
                {
                    ImageRadio.Checked = false;
                    TextRadio.Checked = false;
                    StimulusImage.Text = String.Empty;
                    StimulusImage.Enabled = false;
                    StimulusImageLabel.Enabled = false;
                    Browse.Enabled = false;
                    SelectButton.Enabled = false;
                    StimulusText.Enabled = false;
                }
                else if (DefinedItem.Stimulus.Type == DIType.StimulusImage)
                {
                    StimulusImage.Text = DefinedItem.Stimulus.Description;
                    ImageRadio.Checked = true;
                    StimulusImage.Enabled = true;
                    StimulusImageLabel.Enabled = true;
                    Browse.Enabled = true;
                    SelectButton.Enabled = true;
                    StimulusText.Enabled = false;
                    TextRadio.Checked = false;
                }
                else
                {   
                    ImageRadio.Checked = false;
                    TextRadio.Checked = true;
                    StimulusImage.Enabled = false;
                    StimulusImageLabel.Enabled = false;
                    StimulusText.TextDisplayItemUri = DefinedItem.Stimulus.URI;
                    Browse.Enabled = false;
                    SelectButton.Enabled = false;
                    StimulusText.Enabled = true;
                    StimulusImage.Text = String.Empty;
                }
                IgnoreEvents = false;
            }
        }

        public StimulusDefinitionPanel(Uri parentBlockUri)
        {
            ParentBlockUri = parentBlockUri;
//            this.AutoScaleMode = AutoScaleMode.Dpi;
            InitializeComponent();
            StimulusText = new TextEditControl(StimulusTextWidth, DIText.UsedAs.Stimulus, true);
            StimulusText.Location = new Point(StimulusTextPos.X, StimulusTextPos.Y);
            StimulusGroup.Controls.Add(StimulusText);
        }

        private void ImageRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (IgnoreEvents)
                return;
            if (ImageRadio.Checked)
            {
                CIAT.SaveFile.GetDI(StimulusText.TextDisplayItemUri)?.Dispose();
                StimulusText.Enabled = false;
                DefinedItem.StimulusUri = new DIStimulusImage().URI;
                StimulusImageLabel.Enabled = true;
                StimulusImage.Enabled = true;
                Browse.Enabled = true;
                SelectButton.Enabled = true;
            }
        }

        private void TextRadio_CheckedChanged(object sender, EventArgs e)
        {
            if (IgnoreEvents)
                return;
            if (TextRadio.Checked)
            {
                Browse.Enabled = false;
                SelectButton.Enabled = false;
                StimulusText.Enabled = true;
                StimulusText.TextDisplayItemUri = new DIStimulusText().URI;
                DefinedItem.StimulusUri = StimulusText.TextDisplayItemUri;
            }
        }

        private void Browse_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = Properties.Resources.sImageFileFilter;
            dlg.Title = Properties.Resources.sOpenImageFileDialogTitle;
            if (dlg.ShowDialog(this) == DialogResult.OK)
            {
                (DefinedItem.Stimulus as DIStimulusImage).LoadImageFromFile(dlg.FileName);
                StimulusImage.Text = DefinedItem.Stimulus.Description;
            }
        }

        private void Select_Click(object sender, EventArgs e)
        {
            ImageBrowser browser = new ImageBrowser();
            if (browser.ShowDialog(this) == DialogResult.OK)
            {
                Images.IImage img = CIAT.SaveFile.GetIImage(browser.SelectedImageUri);
                DefinedItem.StimulusUri = new DIStimulusImage(CIAT.SaveFile.GetIImage(browser.SelectedImageUri)).URI;
                StimulusImage.Text = "Selected From Browser";
            }
        }


        private void StimulusDefinitionPanel_Load(object sender, EventArgs e)
        {
            bIsLoaded = true;
        }

        private void KeyedLeft_CheckedChanged(object sender, EventArgs e)
        {
            if (IgnoreEvents)
                return;
            if (KeyedLeft.Checked)
            {
                DefinedItem.SetKeyedDirection(ParentBlockUri, KeyedDirection.Left);
            }
        }

        private void KeyedRight_CheckedChanged(object sender, EventArgs e)
        {
            if (IgnoreEvents)
                return;
            if (KeyedRight.Checked) 
            {
                DefinedItem.SetKeyedDirection(ParentBlockUri, KeyedDirection.Right);
            }
        }
    }
}
