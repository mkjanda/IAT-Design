using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    public partial class ImageStimulusPanel : UserControl
    {
        private IATBlockItemsPanel ItemsPanel
        {
            get
            {
                return (IATBlockItemsPanel)Parent.Parent;
            }
        }

        private IATBlockPanel BlockPanel
        {
            get
            {
                return (IATBlockPanel)Parent.Parent.Parent;
            }
        }

        private CImageDisplayItem ImageStimulus
        {
            get
            {
                return (CImageDisplayItem)BlockPanel.ActiveItem.Stimulus;
            }
        }

        private IATItemPreviewPanel PreviewPanel
        {
            get
            {
                return BlockPanel.PreviewPanel;
            }
        }

        private bool UpdatingFromCode;

        public ImageStimulusPanel()
        {
            InitializeComponent();
            UpdatingFromCode = false;
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
            FileName.Text = dialog.FileName;
            ImageStimulus.FullFilePath = FileName.Text;
            if (!ImageStimulus.IsValid())
            {
                MessageBox.Show(Properties.Resources.sInvalidImageFileFormat);
                FileName.Text = String.Empty;
                ImageStimulus.Dispose();
                ImageStimulus.FullFilePath = String.Empty;
                return;
            }
            StretchToFitCheck.Enabled = (ImageStimulus.ItemSize.Width < CIAT.Layout.StimulusRectangle.Width) &&
                (ImageStimulus.ItemSize.Height < CIAT.Layout.StimulusRectangle.Height);
            if (StretchToFitCheck.Enabled)
                ImageStimulus.StretchToFit = StretchToFitCheck.Checked;
            PreviewPanel.InvalidateStimulus();
            ItemsPanel.UpdateSelectedItem(ImageStimulus.FileName);
        }

        private void ImageStimulusPanel_Load(object sender, EventArgs e)
        {
            StretchToFitCheck.Checked = false;
            StretchToFitCheck.Enabled = false;
        }

        private void StretchToFitCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!UpdatingFromCode)
            {
                ImageStimulus.StretchToFit = StretchToFitCheck.Checked;
                PreviewPanel.InvalidateStimulus();
            }
        }

        public void ResetControlsForStimulus(CImageDisplayItem stimulus)
        {
            UpdatingFromCode = true;
            if (stimulus == null)
            {
                FileName.Text = String.Empty;
                StretchToFitCheck.Enabled = false;
                StretchToFitCheck.Checked = false;
                CopyToOutputDirectoryCheck.Checked = false;
            } 
            else if (!stimulus.IsValid())
            {
                FileName.Text = String.Empty;
                StretchToFitCheck.Enabled = false;
                StretchToFitCheck.Checked = false;
                CopyToOutputDirectoryCheck.Checked = false;
            }
            else
            {
                FileName.Text = stimulus.DisplayFileName;
                StretchToFitCheck.Enabled = (stimulus.ItemSize.Width < CIAT.Layout.StimulusRectangle.Width) &&
                    (stimulus.ItemSize.Height < CIAT.Layout.StimulusRectangle.Height);
                StretchToFitCheck.Checked = stimulus.StretchToFit;
                CopyToOutputDirectoryCheck.Checked = stimulus.CopyToOutputDirOnSave;
            }
            UpdatingFromCode = false;
        }

        private void CopyToOutputDirectoryCheck_CheckedChanged(object sender, EventArgs e)
        {
            if (!UpdatingFromCode)
                ImageStimulus.CopyToOutputDirOnSave = CopyToOutputDirectoryCheck.Checked;
        }
    }
}
