using System;
using System.Windows.Forms;

namespace IATClient
{
    public class ImageDisplay : UserControl, IImageDisplay
    {
        protected PictureBox ImageBox = new PictureBox();

        public new ControlCollection Controls
        {
            get
            {
                return ImageBox.Controls;
            }
        }

        private readonly object lockObj = new object();

        public void ClearImage()
        {
            if (IsHandleCreated)
                this.BeginInvoke(new Action(() => ImageBox.Image = DINull.DINull.IImage.Img));
        }

        public virtual void SetImage(Images.IImageMedia image)
        {
            if (!IsHandleCreated)
                return;
            if (image == null)
                ImageBox.Image = null;
            this.BeginInvoke(new Action(() =>
            {

                ImageBox.Image = image.Img;
                if (ImageBox.Image == null)
                    return;
                /*                                double arIS = (double)CIAT.SaveFile.Layout.InteriorSize.Width / CIAT.SaveFile.Layout.InteriorSize.Height;
                                                double arImg = (double)ImageBox.Image.Width / ImageBox.Image.Height;
                                                int w, h;
                                                Point pt;
                                                if (arImg < arIS)
                                                {
                                                    w = ImageBox.Image.Width;
                                                    h = ImageBox.Image.Height / (int)arImg;
                                                    pt = new Point(0, ImageBox.Image.Height - h >> 1);
                                                }
                                                else
                                                {
                                                    w = ImageBox.Image.Width * (int)arImg;
                                                    h = ImageBox.Image.Height;
                                                    pt = new Point(ImageBox.Image.Width - w >> 1, 0);
                                                }
                                                this.Width = w;
                                                this.Height = h;
                                                this.Location = pt;*/
            }));
            /*                        System.Drawing.Image img = null;
                                    if (image != null) 
                                         img = ScalePreview(image.Img);
                                    //img = image.Img;
                                    EventHandler setImage;
                                    if (img == null)
                                        setIm
                                    {
                                        ImageBox.SuspendLayout();
                                        Image i = ImageBox.Image;
                                        ImageBox.BackColor = CIAT.SaveFile.Layout.BackColor;
                                        if (i != null)
                                        {
                                            ImageBox.Image = null;
                                            CIAT.ImageManager.ReleaseImage(i);
                                        }
                                        ImageBox.ResumeLayout(false);
                                        return;
                                    }*/
            EventHandler setImage = new EventHandler((sender, args) =>
            {
                try
                {
                    var i = ImageBox.Image;
                    SuspendLayout();
                    ImageBox.BackColor = CIAT.SaveFile.Layout.BackColor;
                    if (i != null)
                    {
                        CIAT.ImageManager.ReleaseImage(i);
                    }
                    ImageBox.Image = image.Img;
                    ResumeLayout(true);
                }
                catch (Exception ex)
                {
                    ErrorReporter.ReportError(new CReportableException("Error updating preview", ex));
                }
            });
            if (!IsHandleCreated)
                this.HandleCreated += (s, a) => setImage(s, a);
            else
                this.BeginInvoke(setImage);
        }

        public ImageDisplay()
        {
            ImageBox.SizeMode = PictureBoxSizeMode.Zoom;
            ImageBox.BackColor = CIAT.SaveFile.Layout.BackColor;
            ImageBox.Dock = DockStyle.Fill;
            base.Controls.Add(ImageBox);
        }

        public new void Dispose()
        {
            if (ImageBox.Image != null)
                CIAT.ImageManager.ReleaseImage(ImageBox.Image);
            ImageBox.Image = null;
            base.Dispose();
        }
    }
}
