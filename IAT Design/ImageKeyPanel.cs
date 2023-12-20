using System;
using System.IO;
using System.Windows.Forms;

namespace IATClient
{
    partial class ImageKeyPanel : UserControl
    {
        private Uri ImageUri { get; set; } = DIBase.DINull.URI;
        public IImageDisplay Preview { get; set; }
        public Action ValidateData { get; set; }

        public Uri DisplayItemUri
        {
            get
            {
                return ImageUri;
            }
            set
            {
                if (ImageUri != DIBase.DINull.URI)
                    CIAT.SaveFile.GetDI(ImageUri).Dispose();
                ImageUri = value;
                if (value == DIBase.DINull.URI)
                    return;
                DIResponseKeyImage rki = CIAT.SaveFile.GetDI(ImageUri) as DIResponseKeyImage;
                rki.PreviewPanel = Preview;
                FileName.Text = rki.Description;
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
            ImageUri = DIBase.DINull.URI;
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
            if (new FileInfo(dialog.FileName).Length > DIImage.MaxFileSize)
            {
                MessageBox.Show("Only pictures of 4MB or smaller are allowed.", "File Too Large");
                return;
            }
            if (ImageUri != DIBase.DINull.URI)
                CIAT.SaveFile.GetDI(ImageUri).Dispose();
            var rki = new DIResponseKeyImage()
            {
                PreviewPanel = Preview
            };
            rki.Description = dialog.FileName;
            rki.LoadImageFromFile(dialog.FileName);
            ImageFileName = dialog.FileName;
            rki.ScheduleInvalidation();
            ImageUri = rki.URI;
            ValidateData?.Invoke();
        }

        private void SelectButton_Click(object sender, EventArgs e)
        {
            ImageBrowser browser = new ImageBrowser();
            if (browser.ShowDialog(this) == DialogResult.OK)
            {
                DIBase di = CIAT.SaveFile.GetDI(browser.SelectedImageUri);
                if (ImageUri != DIBase.DINull.URI)
                    CIAT.SaveFile.GetDI(ImageUri).Dispose();
                var RKI = CIAT.SaveFile.GetDI(browser.SelectedImageUri).Clone() as DIResponseKeyImage;
                ImageFileName = RKI.Description;
                ValidateData?.Invoke();
                ImageUri = RKI.URI;
            }
        }
    }
}
