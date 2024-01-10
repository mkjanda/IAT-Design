using System;
using System.Threading;
using System.Threading.Tasks;
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

        private readonly ManualResetEvent queued = new ManualResetEvent(true), release = new ManualResetEvent(true);
        private readonly object Queued = new object(), Ready = new object(), Empty = new object();
        private object QueueState;
        public virtual void SetImage(Images.IImageMedia image)
        {
            if (!IsHandleCreated)
                return;
            if (image == null)
                ImageBox.Image = null;
            Task.Run(() =>
            {
                if (QueueState == Queued)
                    release.Set();
                var t = new Task(() =>
                {
                    if (IsHandleCreated)
                        ImageBox.BeginInvoke(new Action(() =>
                        {
                            var i = ImageBox.Image;
                            if (i != null)
                                CIAT.ImageManager.ReleaseImage(i);
                            ImageBox.BackColor = CIAT.SaveFile.Layout.BackColor;
                            ImageBox.Image = image.Img;
                            queued.Set();
                        }));
                    else
                        this.HandleCreated += (sender, args) => ImageBox.BeginInvoke(new Action(() =>
                        {
                            var i = ImageBox.Image;
                            if (i != null)
                                CIAT.ImageManager.ReleaseImage(i);
                            ImageBox.BackColor = CIAT.SaveFile.Layout.BackColor;
                            ImageBox.Image = image.Img;
                            queued.Set();
                        }));
                });

                t.RunSynchronously();
            });
        }

        public ImageDisplay()
        {
            ImageBox.SizeMode = PictureBoxSizeMode.Zoom;
            ImageBox.BackColor = CIAT.SaveFile.Layout.BackColor;
            ImageBox.Dock = DockStyle.Fill;
            QueueState = Empty;
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
