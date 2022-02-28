using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace IATClient
{
    class ItemSlideThumbnailPanel : Panel
    {
        private object lockObject = new object();
        private PictureBox ImageBox;
        private EventHandler OnClick;

        public ItemSlideThumbnailPanel(EventHandler onClick, Size sz)
        {
            Size = sz;
            this.BorderStyle = BorderStyle.FixedSingle;
            BackColor = System.Drawing.Color.White;
            this.Click += new EventHandler(onClick);
            OnClick = onClick;
            BackgroundImage = null;
            ImageBox = new PictureBox();
            ImageBox.SizeMode = PictureBoxSizeMode.CenterImage;
            ImageBox.Dock = DockStyle.Fill;
            ImageBox.BackColor = Color.White;
            ImageBox.BorderStyle = BorderStyle.None;
            Controls.Add(ImageBox);
            ImageBox.Click += new EventHandler(ImageBox_OnClick);
        }

        private void ImageBox_OnClick(object sender, EventArgs e)
        {
            OnClick(this, e);
        }

        public void Lock()
        {
            Monitor.Enter(lockObject);
        }

        public void Unlock()
        {
            Monitor.Exit(lockObject);
        }

        public bool TryLock()
        {
            return Monitor.TryEnter(lockObject);
        }

        public void SetBackgroundImage(Image img)
        {
            if (ImageBox.Image != null)
            {
                Image i = ImageBox.Image;
                ImageBox.Image = img;
                i.Dispose();
            }
            else
                ImageBox.Image = img;
        }
    }
}
