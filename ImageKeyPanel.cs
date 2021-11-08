using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    partial class ImageKeyPanel : UserControl
    {
        private DIResponseKeyImage RKI { get; set; } = null;
        public IImageDisplay Preview { get; set; }
        public Action ValidateData { get; set; }

        public Uri DisplayItemUri
        {
            get
            {
                if (RKI == null)
                    return null;
                return RKI.URI;
            }
            set
            {
                if ((value == null) && (RKI.URI != null))
                    CIAT.SaveFile.GetDI(RKI.URI).Dispose();
                else if (value == null)
                    return;
                else
                {
                    RKI = CIAT.SaveFile.GetDI(value) as DIResponseKeyImage;
                    RKI.PreviewPanel = Preview;
                    FileName.Text = RKI.Description;
                }
            }
        }

        /// <summary>
        /// gets the main form
        /// </summary>
        public SimpleResponseKeyPanel ParentControl
        {
            get
            {
                return (SimpleResponseKeyPanel)Parent.Parent;
            }
        }

        /// <summary>
        /// gets the user-selected image file name
        /// </summary>
        public String ImageFileName
        {
            get
            {
                return FileName.Text;
            }
            set
            {
                FileName.Text = value;
            }
        }


        public ImageKeyPanel()
        {
            InitializeComponent();
        }

        public void Clear()
        {
            FileName.Text = String.Empty;
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
            if (RKI == null)
                RKI = new DIResponseKeyImage()
                {
                    PreviewPanel = Preview
                };
            RKI.Description = dialog.FileName;
            RKI.LoadImageFromFile(dialog.FileName);
            ImageFileName = dialog.FileName;
            ValidateData?.Invoke();
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            ImageBrowser browser = new ImageBrowser();
            if (browser.ShowDialog(this) == DialogResult.OK)
            {
                DIBase di = CIAT.SaveFile.GetDI(browser.SelectedImageUri);
                if (RKI != null)
                    RKI.Dispose();
                RKI = CIAT.SaveFile.GetDI(browser.SelectedImageUri).Clone() as DIResponseKeyImage;
                ImageFileName = RKI.Description;
                ValidateData?.Invoke();
            }
        }
    }
}
