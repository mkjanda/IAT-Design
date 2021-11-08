using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    partial class ImageBrowser : Form
    {

        private ImageList Images;
        public Uri SelectedImageUri { get; private set; }

        public ImageBrowser()
        {
            InitializeComponent();
            ImageList.View = View.LargeIcon;
            Images = new ImageList();
            Images.ColorDepth = ColorDepth.Depth32Bit;
            Images.ImageSize = IATClient.Images.ImageManager.ThumbnailSize;
            ImageList.LargeImageList = Images;
            ListViewItem lvi;
            int ctr = 0;
            List<Uri> StimulusImageUris = CIAT.SaveFile.GetPartsOfType(DIType.StimulusImage.MimeType);
            foreach (Uri u in StimulusImageUris)
            {
                DIStimulusImage diStim = CIAT.SaveFile.GetDI(u) as DIStimulusImage;
                lvi = new ListViewItem(diStim.Description);
                lvi.ImageIndex = ctr;
                lvi.Tag = diStim.IImage.URI;
                Images.Images.Add(diStim.IImage.Thumbnail.Image);
                CIAT.ImageManager.GenerateThumb(diStim.IImage);
                lvi.ImageIndex = Images.Images.Count - 1;
                ImageList.Items.Add(lvi);
            }
            /*
            foreach (Uri u in CIAT.SaveFile.GetPartsOfType(DIType.ResponseKeyImage.MimeType))
            {
                DIResponseKeyImage diRKI = CIAT.SaveFile.GetDI(u) as DIResponseKeyImage;
                lvi = new ListViewItem(diRKI.Description);
                lvi.ImageIndex = ctr;
                lvi.Tag = diRKI.IImage.URI;
                Images.Images.Add(diRKI.IImage.Thumbnail.Image);
                ImageList.Items.Add(lvi);
            }
            foreach (Uri u in CIAT.SaveFile.GetPartsOfType(DIType.SurveyImage.MimeType))
            {
                DISurveyImage diSurveyImage = CIAT.SaveFile.GetDI(u) as DISurveyImage;
                lvi = new ListViewItem(diSurveyImage.Description);
                lvi.ImageIndex = ctr;
                lvi.Tag = diSurveyImage.IImage.URI;
                Images.Images.Add(diSurveyImage.IImage.Thumbnail.Image);
                ImageList.Items.Add(lvi);
            }*/
            SelectedImageUri = null;
            Accept.Enabled = false;
        }

        private void Accept_Click(object sender, EventArgs e)
        {
            SelectedImageUri = (Uri)ImageList.SelectedItems[0].Tag;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void Cancel_Click(object sender, EventArgs e)
        {
            SelectedImageUri = null;
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void ImageList_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.ImageList.SelectedItems.Count == 0)
                Accept.Enabled = false;
            else
                Accept.Enabled = true;
        }
    }
}
