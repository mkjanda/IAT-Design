using System;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace IATClient
{
    class ItemSlideThumbnailPanel : UserControl
    {
        private object lockObject = new object();
        private PictureBox ImageBox;

        public ItemSlideThumbnailPanel(EventHandler onClick, Size sz)
        {
            Size = sz;
            this.BorderStyle = BorderStyle.FixedSingle;
            BackColor = System.Drawing.Color.White;
            this.Click += new EventHandler(onClick);
            BackgroundImage = null;
            ImageBox = new PictureBox();
            ImageBox.SizeMode = PictureBoxSizeMode.Zoom;
            ImageBox.Dock = DockStyle.Fill;
            ImageBox.BackColor = Color.White;
            ImageBox.BorderStyle = BorderStyle.None;
            Controls.Add(ImageBox);
            ImageBox.Click += (sender, args) => { onClick(this, args); };
            this.HandleCreated += (sender, args) =>
            {
                this.AutoScaleDimensions = new SizeF(72F, 72F);
                this.AutoScaleMode = AutoScaleMode.Dpi;
            };
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
