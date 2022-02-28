using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace IATClient
{
    public class ImageDisplay : Control, IImageDisplay
    {
        protected PictureBox ImageBox = new PictureBox();
        private Bitmap ScalePreview(Image img)
        {
            if (img == null)
                return null;
            if ((Width == img.Width) && (Height == img.Height))
            {
                return img as Bitmap;
            }
            Bitmap bmp;
            if (img.Tag as Images.ImageMediaType == Images.ImageMediaType.FullWindow)
                bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FullPreview);
            else if (img.Tag as Images.ImageMediaType == Images.ImageMediaType.ResponseKey)
                bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.ResponseKeyPreview);
            else
                return img as Bitmap;
            Graphics g = Graphics.FromImage(bmp);
            Brush br = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            g.FillRectangle(br, new Rectangle(0, 0, bmp.Width, bmp.Height));
            double arPreview = (double)Width / (double)Height;
            double arImg = (double)img.Width / (double)img.Height;
            Rectangle scaledRect;
            Size scaledSize;
            if ((Width <= img.Width) || (Height <= img.Height))
            {
                if (arImg > arPreview)
                {
                    scaledSize = new Size(Width, (int)(Width / arImg));
                    scaledRect = new Rectangle(new Point(0, (Height - scaledSize.Height) >> 1), scaledSize);
                }
                else
                {
                    scaledSize = new Size((int)(Height * arImg), Height);
                    scaledRect = new Rectangle(new Point((Width - scaledSize.Width) >> 1, (Height - scaledSize.Height) >> 1), scaledSize);
                }
            }
            else
                scaledRect = new Rectangle(new Point((Width - img.Size.Width) >> 1, (Height - img.Size.Height) >> 1), img.Size);
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(img, scaledRect);
            g.Dispose();
            CIAT.ImageManager.ReleaseImage(img);
            return bmp;
        }

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
            this.BeginInvoke(new Action(() => ImageBox.Image = image.Img));
            /*            System.Drawing.Image img = null;
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
                        }
                        EventHandler setImage = new EventHandler((sender, args) =>
                        {
                            try
                            {
                                Image i = ImageBox.Image;
                                SuspendLayout();
                                ImageBox.BackColor = CIAT.SaveFile.Layout.BackColor;
                                if (i != null)
                                {
                                    ImageBox.Image = null;
                                    CIAT.ImageManager.ReleaseImage(i);
                                }
                                ImageBox.Image = img;
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
                            this.BeginInvoke(setImage);*/
        }

        public ImageDisplay()
        {
            ImageBox.Dock = DockStyle.Fill;
            ImageBox.SizeMode = PictureBoxSizeMode.Zoom;
            ImageBox.BackColor = CIAT.SaveFile.Layout.BackColor;
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
