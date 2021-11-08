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
            int scaledWidth, scaledHeight;
            if ((Width >= img.Width) && (Height >= img.Height))
            {
                if (arPreview > arImg)
                {
                    scaledHeight = Height;
                    scaledWidth = (int)(arImg * scaledHeight);
                }
                else
                {
                    scaledWidth = Width;
                    scaledHeight = (int)(scaledWidth / arImg);
                }
            }
            else if (Width >= img.Width)
            {
                scaledHeight = Height;
                scaledWidth = (int)(arImg * scaledHeight);
            }
            else if (Height >= img.Height)
            {
                scaledWidth = Width;
                scaledHeight = (int)(scaledWidth / arImg);
            }
            else
            {
                if (arPreview > arImg)
                {
                    scaledHeight = Height;
                    scaledWidth = (int)(arImg * scaledHeight);
                }
                else
                {
                    scaledWidth = Width;
                    scaledHeight = (int)(scaledWidth / arImg);
                }
            }
            g.SmoothingMode = SmoothingMode.HighQuality;
            g.InterpolationMode = InterpolationMode.HighQualityBicubic;
            g.PixelOffsetMode = PixelOffsetMode.HighQuality;
            g.DrawImage(img, new Rectangle(new Point(0, 0), new Size(scaledWidth, scaledHeight)));
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

        public virtual void SetImage(Bitmap bmp)
        {
            if (bmp == null)
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
                    lock (lockObj)
                    {
                        Image i = ImageBox.Image;
                        SuspendLayout();
                        ImageBox.BackColor = CIAT.SaveFile.Layout.BackColor;
                        if (i != null)
                        {
                            ImageBox.Image = null;
                            CIAT.ImageManager.ReleaseImage(i);
                        }
                        ImageBox.Image = ScalePreview(bmp);
                        ResumeLayout(false);
                    }
                }
                catch (Exception ex)
                {
                    IATConfigMainForm.ShowErrorReport("Error updating preview", new CReportableException("Error updating preview", ex));
                }
            });
            this.BeginInvoke(setImage);
        }

        public virtual void SetImage(Images.IImageMedia image)
        {
            System.Drawing.Image img = null;
            if (image != null)
                img = image.Image;
            if ((image == null) || (img == null))
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
                    ImageBox.Image = ScalePreview(img);
                    ResumeLayout(true);
                }
                catch (Exception ex)
                {
                    IATConfigMainForm.ShowErrorReport("Error updating preview", new CReportableException("Error updating preview", ex));
                }
            });
            if (!IsHandleCreated)
                this.HandleCreated += (s, a) => setImage(s, a);
            else
                this.BeginInvoke(setImage);
        }

        public ImageDisplay()
        {
            ImageBox.Dock = DockStyle.Fill;
            ImageBox.SizeMode = PictureBoxSizeMode.CenterImage;
            base.Controls.Add(ImageBox); 
/*
            Paint += (sender, args) =>
            {
                var backBr = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                args.Graphics.FillRectangle(backBr, args.ClipRectangle);
                try
                {
                    if (Image == null)
                        return;
                    args.Graphics.DrawImage(Image, 0, 0);
                }
                catch (Exception ex) { }
            };*/
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
