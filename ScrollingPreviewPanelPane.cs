using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Threading.Tasks;

namespace IATClient
{
    public class ScrollingPreviewPanelPane : Control, IImageDisplay
    {
        private object lockObject = new object();
        private bool bSelected = false;
        private bool bMouseDown = false;
        private Point MouseDownAt = Point.Empty;
        private static Size DragDelta = new Size(15, 15);
        private bool IsDragOriginator = false;

        public delegate void DragStartHandler(ScrollingPreviewPanelPane sender);
        public delegate void DragEndHandler(ScrollingPreviewPanelPane sender, bool bInsertBefore);
        public delegate bool DragLeaveHandler(ScrollingPreviewPanelPane sender, bool IsDragOriginator);
        public delegate bool IsDraggingCallback();
        public delegate ScrollingPreviewPanel.EOrientation ParentOrientationCallback();
        public DragEndHandler OnDragEnd;
        public DragStartHandler OnDragStart;
        public IsDraggingCallback IsDragging;
        public ParentOrientationCallback ParentOrientation;
        public new DragLeaveHandler OnDragLeave = null;
        private IThumbnailPreviewable _PreviewedItem = null;

        private Image Image { get; set; }

        public void SetImage(Images.IImageMedia image)
        {
            Action a = () =>
            {
                Image i = this.Image;
                if (image != null)
                    Image = image.Image;
                else
                    Image = null;
                CIAT.ImageManager.ReleaseImage(i);
                this.Invalidate();
            };
            if (IsHandleCreated)
                this.BeginInvoke(a);
            else
                HandleCreated += (sender, args) => { this.BeginInvoke(a); };
        }

        public void SetImage(Bitmap bmp)
        {
            if (bmp == null)
            {
                Image i = Image;
                Image = null;
                CIAT.ImageManager.ReleaseImage(i);
                Invalidate();
                return;
            }
            Task.Run(() =>
            {
                Image i = Image;
                Image = bmp;
                CIAT.ImageManager.ReleaseImage(i);
                Invalidate();
            });
        }


        public IThumbnailPreviewable PreviewedItem
        {
            get
            {
                return _PreviewedItem;
            }
            set
            {
                if (_PreviewedItem != null)
                    _PreviewedItem.ThumbnailPreviewPanel = null;
                _PreviewedItem = value;
                if (value != null)
                    _PreviewedItem.ThumbnailPreviewPanel = this;
                if (value == null)
                    SetImage(null as Bitmap);
            }
        }

        public bool Selected
        {
            get
            {
                return bSelected;
            }
            set
            {
                if (value != bSelected)
                {
                    bSelected = value;
                    Invalidate();
                }
            }
        }
        /*
        protected IIATImage PreviewImage
        {
            get
            {
                lock (lockObject)
                {
                    if (PreviewedScreen != null)
                        return PreviewedScreen.ScreenImage;
                    else if (PreviewedItem != null)
                        return PreviewedItem.StimulusImage;
                    else
                        return null;
                }
            }
        }
        */
        public ScrollingPreviewPanelPane()
        {
            this.BackColor = CIAT.SaveFile.Layout.BackColor;
            this.BackgroundImage = null;
            this.BackgroundImageLayout = ImageLayout.Center;
            this.ParentChanged += new EventHandler(ScrollingPreviewPanelPane_ParentChanged);
            this.Paint += new PaintEventHandler(ScrollingPreviewPanelPane_Paint);
            this.MouseDown += new MouseEventHandler(ScrollingPreviewPanelPane_MouseDown);
            this.MouseMove += new MouseEventHandler(ScrollingPreviewPanelPane_MouseMove);
            this.MouseUp += new MouseEventHandler(ScrollingPreviewPanelPane_MouseUp);
            this.DragDrop += new DragEventHandler(ScrollingPreviewPanelPane_DragDrop); 
            this.DragOver += new DragEventHandler(ScrollingPreviewPanelPane_DragOver);
            this.DragLeave += new EventHandler(ScrollingPreviewPanelPane_DragLeave);
            this.AllowDrop = true;
        }

        private void ScrollingPreviewPanelPane_DragLeave(object sender, EventArgs e)
        {
            if (OnDragLeave != null)
                IsDragOriginator = OnDragLeave(this, IsDragOriginator);
        }

        void ScrollingPreviewPanelPane_DragDrop(object sender, DragEventArgs e)
        {
            Point ptDrop = PointToClient(new Point(e.X, e.Y));
            if (IsDragging())
                switch (ParentOrientation())
                {
                    case ScrollingPreviewPanel.EOrientation.horizontal:
                        OnDragEnd(this, (ptDrop.X < this.Width >> 1) ? true : false);
                        break;

                    case ScrollingPreviewPanel.EOrientation.vertical:
                        OnDragEnd(this, (ptDrop.Y < this.Height >> 1) ? true : false);
                        break;
                }
        }

        void ScrollingPreviewPanelPane_DragOver(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        void ScrollingPreviewPanelPane_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                bMouseDown = true;
                MouseDownAt = e.Location;
            }
        }

        void ScrollingPreviewPanelPane_MouseUp(object sender, MouseEventArgs e)
        {
            bMouseDown = false;
        }

        void ScrollingPreviewPanelPane_MouseMove(object sender, MouseEventArgs e)
        {
            if (bMouseDown)
            {
                Rectangle rect = new Rectangle(MouseDownAt.X - DragDelta.Width, MouseDownAt.Y - DragDelta.Height, DragDelta.Width, DragDelta.Height);
                if (!rect.Contains(e.Location))
                {
                    IsDragOriginator = true;
                    OnDragStart(this);
                }
                bMouseDown = false;
            }
        }

        void ScrollingPreviewPanelPane_Paint(object sender, PaintEventArgs e)
        {
            Brush bkBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            e.Graphics.FillRectangle(bkBrush, e.ClipRectangle);
            bkBrush.Dispose();
            try
            {
                e.Graphics.DrawImage(Image, 0, 0);
            }
            catch (Exception) { }
            if (bSelected)
                e.Graphics.DrawRectangle(Pens.LimeGreen, new Rectangle(new Point(0, 0), this.Size - new Size(1, 1)));
            else
                e.Graphics.DrawRectangle(Pens.Gray, new Rectangle(new Point(0, 0), this.Size - new Size(1, 1)));

        }

        void ScrollingPreviewPanelPane_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
            }
            else
            {
                if (BackgroundImage != null)
                    BackgroundImage.Dispose();
            }
        }


        /*
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
        public new void Dispose()
        {
            CIAT.ImageManager.ReleaseImage(Image);
            Image = null;
            base.Dispose();
        }
        */
    }
}
