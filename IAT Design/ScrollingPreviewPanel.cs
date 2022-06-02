using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace IATClient
{
    public class ScrollingPreviewPanel : UserControl
    {
        private List<ScrollingPreviewPanelPane> PreviewPanels;

        private int ScrollValue, ScrollMin, ScrollMax;
        private Panel PreviewPanelBackground;
        private Size _PreviewSize = Size.Empty;
        public enum EOrientation { horizontal, vertical, unset };
        private EOrientation _Orientation;
        private Button NextButton, PrevButton;
        private bool IsLoaded = false;
        private int nSelectedPreview = -1;
        public static readonly Padding PreviewPadding = new Padding(3);
        public Action<int> PreviewClickCallback = null;
        public Action<int, int> OnMoveContainerItem;
        private delegate void OffsetDelegate(Control c, Size offset);
        private delegate void PositionDelegate(Control c, Point pt);
        private delegate void SetScrollDelegate(int val);
        private bool IsDragging = false;
        private int nDraggingNdx = -1;
        private object ScrollingLockObject = new object();
        private bool HaltScroll = false;
        private int ScrollMaxRate { get { return ScrollDiff; } }
        public const int DefaultScrollDiff = 5;
        public int ScrollDiff { get; set; } = 5;

        public ScrollingPreviewPanelPane this[int ndx]
        {
            get
            {
                return PreviewPanels[ndx];
            }
        }

        public int SelectedPreview
        {
            get
            {
                return nSelectedPreview;
            }
            set
            {
                for (int ctr = 0; ctr < PreviewPanels.Count; ctr++)
                    if (ctr == value)
                        PreviewPanels[ctr].Selected = true;
                    else
                        PreviewPanels[ctr].Selected = false;
                nSelectedPreview = value;
                PreviewClickCallback(value);
            }
        }

        public Size PreviewSize
        {
            get
            {
                return _PreviewSize;
            }
            set
            {
                if (IsLoaded)
                    throw new Exception("Cannot set a preview size on an initialized control");
                _PreviewSize = value;
            }
        }

        public int NumPreviewsDisplayed
        {
            get
            {
                if (PreviewSize == Size.Empty)
                    throw new Exception("Attempt to retrieve number of visible previews for an incompletely initialized contorl.");
                switch (Orientation)
                {
                    case EOrientation.horizontal:
                        return (this.Width - NextButton.Width - PrevButton.Width) / (PreviewPadding.Horizontal + PreviewSize.Width);

                    case EOrientation.vertical:
                        return (this.Height - NextButton.Height - PrevButton.Height) / (PreviewPadding.Vertical + PreviewSize.Height);

                    default:
                        throw new Exception("Attempt to retrieve number of visible previews for an incompletely initialized contorl.");
                }
            }
        }


        public EOrientation Orientation
        {
            get
            {
                return _Orientation;
            }
            set
            {
                if (IsLoaded)
                    throw new Exception("Cannot set orientation after control has been initialized");
                _Orientation = value;
            }
        }


        public ScrollingPreviewPanel()
        {
            this.BorderStyle = BorderStyle.Fixed3D;
            PreviewPanels = new List<ScrollingPreviewPanelPane>();
            this.Load += new EventHandler(ScrollingPreviewPane_Load);
            this.ParentChanged += new EventHandler(ScrollingPreviewPanel_ParentChanged);
            _Orientation = EOrientation.unset;
            NextButton = new Button();
            NextButton.AllowDrop = true;
            NextButton.DragEnter += new DragEventHandler(NextButton_DragEnter);
            NextButton.DragDrop += new DragEventHandler(NextButton_DragDrop);
            NextButton.DragLeave += new EventHandler(NextButton_DragLeave);
            PrevButton = new Button();
            PrevButton.AllowDrop = true;
            PrevButton.DragEnter += new DragEventHandler(PrevButton_DragEnter);
            PrevButton.DragDrop += new DragEventHandler(PrevButton_DragDrop);
            PrevButton.DragLeave += new EventHandler(PrevButton_DragLeave);
            NextButton.Click += new EventHandler(NextButton_Click);
            PrevButton.Click += new EventHandler(PrevButton_Click);
            PreviewPanelBackground = new Panel();
            PreviewPanelBackground.AllowDrop = true;
            PreviewPanelBackground.DragOver += new DragEventHandler(PreviewPanelBackground_DragOver);
            PreviewPanelBackground.DragDrop += new DragEventHandler(PreviewPanelBackground_DragDrop);
            //            PreviewPanelBackground.Paint += new PaintEventHandler(ScrollingPreviewPanel_Paint);
            //            timer = new System.Windows.Forms.Timer();
            //          timer.Tick += new EventHandler(DoPreviewUpdates);
            //        timer.Interval = 250;
            PreviewPanelBackground.BackColor = System.Drawing.Color.Black;
            this.SizeChanged += new EventHandler(ScrollingPreviewPanel_Resize);
            this.AllowDrop = true;
        }

        public int PreviewBarWidth
        {
            get
            {
                return this.Width - NextButton.Width - PrevButton.Width;
            }
        }

        public int ScrollPosition
        {
            get
            {
                return ScrollValue;
            }
        }

        void ScrollingPreviewPanel_Resize(object sender, EventArgs e)
        {
            if (Orientation == EOrientation.horizontal)
            {
                PrevButton.Size = new Size(Properties.Resources.LeftButtonArrow.Width, this.ClientRectangle.Height - 4);
                PrevButton.Image = Properties.Resources.LeftButtonArrow;
                PrevButton.ImageAlign = ContentAlignment.MiddleCenter;
                PrevButton.Location = new Point(0, 0);
                NextButton.Size = new Size(Properties.Resources.RightButtonArrow.Width, this.ClientRectangle.Height - 4);
                NextButton.Image = Properties.Resources.RightButtonArrow;
                NextButton.ImageAlign = ContentAlignment.MiddleCenter;
                NextButton.Location = new Point(this.ClientRectangle.Width - NextButton.Width, 0);
                PreviewPanelBackground.Size = new Size(this.ClientRectangle.Width - NextButton.Width - PrevButton.Width, this.ClientRectangle.Height);
                PreviewPanelBackground.Location = new Point(PrevButton.Size.Width, 0);
            }
            else if (Orientation == EOrientation.vertical)
            {
                PrevButton.Size = new Size(this.Width - 4, Properties.Resources.UpButtonArrow.Height);
                PrevButton.Image = Properties.Resources.UpButtonArrow;
                PrevButton.ImageAlign = ContentAlignment.MiddleCenter;
                PrevButton.Location = new Point(0, 0);
                NextButton.Size = new Size(this.Width - 4, Properties.Resources.DownButtonArrow.Height);
                NextButton.Image = Properties.Resources.DownButtonArrow;
                NextButton.ImageAlign = ContentAlignment.MiddleCenter;
                NextButton.Location = new Point(0, this.Height - Properties.Resources.DownButtonArrow.Height);
                PreviewPanelBackground.Size = new Size(this.ClientRectangle.Width, this.ClientRectangle.Height - PrevButton.Height - NextButton.Height);
                PreviewPanelBackground.Location = new Point(0, PrevButton.Size.Height);
            }
            else
                throw new Exception("Must set orientation of a Scrolling Preview Pane before it loads");

        }

        void ScrollingPreviewPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                PreviewPanelBackground.BackColor = System.Drawing.Color.Black;
                Controls.Add(PrevButton);
                Controls.Add(NextButton);
                Controls.Add(PreviewPanelBackground);
            }
            else
            {
                Controls.Remove(PrevButton);
                Controls.Remove(NextButton);
                Controls.Remove(PreviewPanelBackground);
            }
        }

        void NextButton_DragLeave(object sender, EventArgs e)
        {
            lock (ScrollingLockObject)
            {
                HaltScroll = true;
            }
        }

        void NextButton_DragDrop(object sender, DragEventArgs e)
        {
            lock (ScrollingLockObject)
            {
                HaltScroll = true;
            }
        }


        void NextButton_DragEnter(object sender, DragEventArgs e)
        {
            ThreadStart proc = new ThreadStart(ScrollForward);
            HaltScroll = false;
            Thread scroller = new Thread(ScrollForward);
            scroller.Start();
            e.Effect = DragDropEffects.Scroll;
        }

        private void PrevButton_DragLeave(object sender, EventArgs e)
        {
            lock (ScrollingLockObject)
            {
                HaltScroll = true;
            }
        }

        private void PrevButton_DragDrop(object sender, EventArgs e)
        {
            lock (ScrollingLockObject)
            {
                HaltScroll = true;
            }
        }

        private void PrevButton_DragEnter(object sender, DragEventArgs e)
        {
            ThreadStart proc = new ThreadStart(ScrollBack);
            HaltScroll = false;
            Thread scroller = new Thread(proc);
            scroller.Start();
            e.Effect = DragDropEffects.Scroll;
        }

        void PreviewPanelBackground_DragDrop(object sender, DragEventArgs e)
        {
            int ctr = 0;
            Point ptClient = PreviewPanelBackground.PointToClient(new Point(e.X, e.Y));
            if ((ptClient.Y < PreviewPadding.Top) || (ptClient.Y > PreviewPanelBackground.Height - PreviewPadding.Bottom))
                return;
            while (ctr < PreviewPanels.Count)
            {
                if (ptClient.X < PreviewPanels[ctr].Location.X + ((PreviewSize.Width + PreviewPadding.Horizontal) >> 1))
                    break;
                ctr++;
            }
            if (ctr == PreviewPanels.Count)
                ctr = PreviewPanels.Count - 1;
            if ((ctr == nDraggingNdx) || (ctr == nDraggingNdx + 1))
            {
                nDraggingNdx = -1;
                return;
            }
            MovePanel(nDraggingNdx, ctr);
            nDraggingNdx = -1;
        }

        private void PreviewPanelBackground_DragOver(object sender, DragEventArgs e)
        {
            Point ptClient = PreviewPanelBackground.PointToClient(new Point(e.X, e.Y));
            if ((ptClient.Y >= PreviewPadding.Top) && (ptClient.Y <= PreviewPanelBackground.Height - PreviewPadding.Bottom))
                e.Effect = DragDropEffects.Move;
            else
                e.Effect = DragDropEffects.None;
        }

        public void Clear()
        {
            foreach (ScrollingPreviewPanelPane p in PreviewPanels)
            {
                if (p.BackgroundImage != null)
                    p.BackgroundImage.Dispose();
                PreviewPanelBackground.Controls.Remove(p);
            }
            PreviewPanels.Clear();

        }

        void ScrollingPreviewPanel_Paint(object sender, PaintEventArgs e)
        {
            for (int ctr = 0; ctr < PreviewPanels.Count; ctr++)
                if (ctr == nSelectedPreview)
                    e.Graphics.DrawRectangle(Pens.LimeGreen,
                        Rectangle.Inflate(new Rectangle(PreviewPanels[ctr].Location, PreviewPanels[ctr].Size), 1, 1));
                else
                    e.Graphics.DrawRectangle(Pens.Gray,
                        Rectangle.Inflate(new Rectangle(PreviewPanels[ctr].Location, PreviewPanels[ctr].Size), 1, 1));
        }


        private void OffsetControl(Control c, Size sz)
        {
            c.Location = c.Location + sz;
        }

        private void PositionControl(Control c, Point pt)
        {
            c.Location = pt;
        }

        private void SetHScrollVal(int val)
        {
            ScrollValue = val;
        }

        private void SetVScrollVal(int val)
        {
            ScrollValue = val;
        }

        private void ScrollBack()
        {
            int scrollVal = ScrollValue;
            OffsetDelegate offset = new OffsetDelegate(OffsetControl);
            PositionDelegate position = new PositionDelegate(PositionControl);
            SetScrollDelegate setScroll;
            if (Orientation == EOrientation.horizontal)
                setScroll = new SetScrollDelegate(SetHScrollVal);
            else
                setScroll = new SetScrollDelegate(SetVScrollVal);
            bool bHalt;
            lock (ScrollingLockObject)
            {
                bHalt = HaltScroll;
            }
            while ((scrollVal >= 0) && !bHalt)
            {
                DateTime startTime = DateTime.Now;
                switch (Orientation)
                {
                    case EOrientation.horizontal:
                        if (PreviewPanelBackground.HorizontalScroll.Enabled)
                        {
                            SuspendLayout();
                            foreach (Control c in PreviewPanelBackground.Controls)
                            {
                                if (ScrollValue - ScrollDiff > 0)
                                    Invoke(offset, c, new Size(ScrollDiff, 0));
                                else
                                    Invoke(position, c, new Point(c.Location.X + (ScrollValue), c.Location.Y));
                            }
                            ResumeLayout(true);
                            if (ScrollValue - ScrollDiff > 0)
                                Invoke(setScroll, ScrollValue - ScrollDiff);
                            else
                                Invoke(setScroll, 0);
                        }
                        break;

                    case EOrientation.vertical:
                        if (PreviewPanelBackground.VerticalScroll.Enabled)
                        {
                            PreviewPanelBackground.SuspendLayout();
                            foreach (Control c in PreviewPanelBackground.Controls)
                            {
                                if (PreviewPanelBackground.VerticalScroll.Value - ScrollDiff > 0)
                                    Invoke(offset, c, new Size(ScrollDiff, 0));
                                else
                                    Invoke(position, c, new Point(c.Location.X, c.Location.Y + (ScrollValue - 0)));
                            }
                            PreviewPanelBackground.ResumeLayout(true);

                            if (PreviewPanelBackground.VerticalScroll.Value - ScrollDiff > 0)
                                Invoke(setScroll, ScrollValue - ScrollDiff);
                            else
                                Invoke(setScroll, 0);
                        }
                        break;
                }
                scrollVal = (Orientation == EOrientation.horizontal) ? ScrollValue : PreviewPanelBackground.VerticalScroll.Value;
                while ((DateTime.Now - startTime).Milliseconds < ScrollMaxRate)
                    Thread.Sleep(5);
                lock (ScrollingLockObject)
                {
                    bHalt = HaltScroll;
                }
            }
        }

        public void ScrollBy(int numPixels)
        {
            if (ScrollValue + numPixels < 0)
                numPixels = -ScrollValue;
            if (ScrollValue + numPixels > ScrollMax)
                numPixels = ScrollMax - ScrollValue;
            ScrollValue += numPixels;
            int pixMoved = 0;
            int sign = (numPixels > 0) ? 1 : -1;
            Task.Run(() =>
            {
                while (pixMoved != numPixels)
                {
                    DateTime startTime = DateTime.Now;
                    if (Math.Abs(pixMoved) + ScrollDiff <= Math.Abs(numPixels))
                    {
                        foreach (Control c in PreviewPanelBackground.Controls)
                            c.BeginInvoke(new Action<int>((n) => { c.Location = new Point(c.Left + n, c.Top); }), -sign * ScrollDiff);
                        pixMoved += ScrollDiff;
                    }
                    else
                    {
                        foreach (Control c in PreviewPanelBackground.Controls)
                            c.BeginInvoke(new Action<int>((n) => { c.Location = new Point(c.Left + n, c.Top); }), -sign * (Math.Abs(numPixels) - Math.Abs(pixMoved)));
                        pixMoved = numPixels;
                    }
                    while ((DateTime.Now - startTime).Milliseconds < ScrollMaxRate)
                        Thread.Yield();
                }
                ScrollDiff = DefaultScrollDiff;
            });
        }

        void PrevButton_Click(object sender, EventArgs e)
        {
            int ScrollVal = ScrollValue;
            if (ScrollVal < 0)
                return;
            int val = Math.Max(ScrollVal - PreviewSize.Width - PreviewPadding.Horizontal, 0);
            ScrollValue = val;
            new Thread(() =>
            {
                Action<Object> scrollAction = (o) =>
                {
                    Control c = (o as Tuple<Control, int>).Item1;
                    int scroll = (o as Tuple<Control, int>).Item2;
                    c.Invoke(new Action(() => { c.Location = new Point(c.Left + scroll, c.Top); }));
                };
                if (PreviewPanelBackground.HorizontalScroll.Enabled)
                {
                    while (ScrollVal > val)
                    {
                        DateTime startTime = DateTime.Now;
                        foreach (Control c in PreviewPanelBackground.Controls)
                        {
                            if (ScrollVal - ScrollDiff >= val)
                            {
                                new Task(scrollAction, new Tuple<Control, int>(c, ScrollDiff)).Start();
                            }
                            else
                            {
                                new Task(scrollAction, new Tuple<Control, int>(c, ScrollVal - val)).Start();
                            }
                        }
                        ScrollVal -= ScrollDiff;
                        while ((DateTime.Now - startTime).Milliseconds < ScrollMaxRate)
                            Thread.Yield();
                    }
                }
            }).Start();
        }


        private void ScrollForward()
        {
            int ScrollVal = ScrollValue;
            OffsetDelegate offset = new OffsetDelegate(OffsetControl);
            PositionDelegate position = new PositionDelegate(PositionControl);
            SetScrollDelegate setScroll;
            if (Orientation == EOrientation.horizontal)
                setScroll = new SetScrollDelegate(SetHScrollVal);
            else
                setScroll = new SetScrollDelegate(SetVScrollVal);
            bool bHalt;
            lock (ScrollingLockObject)
            {
                bHalt = HaltScroll;
            }
            while ((ScrollVal < ScrollMax) && (!HaltScroll))
            {
                DateTime startTime = DateTime.Now;
                switch (Orientation)
                {
                    case EOrientation.horizontal:
                        if (PreviewPanelBackground.HorizontalScroll.Enabled)
                        {
                            PreviewPanelBackground.SuspendLayout();
                            foreach (Control c in PreviewPanelBackground.Controls)
                            {
                                if (ScrollValue + ScrollDiff < ScrollMax)
                                    Invoke(offset, c, new Size(-ScrollDiff, 0));
                                else
                                    Invoke(position, c, new Point(c.Location.X + (ScrollValue - ScrollMax), c.Location.Y));
                            }
                            PreviewPanelBackground.ResumeLayout(true);
                            if (ScrollValue + ScrollDiff < ScrollMax)
                                Invoke(setScroll, ScrollValue + ScrollDiff);
                            else
                                Invoke(setScroll, ScrollMax);
                        }
                        break;

                    case EOrientation.vertical:
                        if (PreviewPanelBackground.VerticalScroll.Enabled)
                        {
                            PreviewPanelBackground.SuspendLayout();
                            foreach (Control c in PreviewPanelBackground.Controls)
                            {
                                if (PreviewPanelBackground.VerticalScroll.Value + ScrollDiff < PreviewPanelBackground.VerticalScroll.Maximum)
                                    Invoke(offset, c, new Size(0, -ScrollDiff));
                                else
                                    Invoke(position, c, new Point(c.Location.X, c.Location.Y + (PreviewPanelBackground.VerticalScroll.Value - PreviewPanelBackground.VerticalScroll.Maximum)));
                                c.Invalidate();
                                PreviewPanelBackground.ResumeLayout(true);
                                if (PreviewPanelBackground.VerticalScroll.Value + ScrollDiff < PreviewPanelBackground.VerticalScroll.Maximum)
                                    Invoke(setScroll, PreviewPanelBackground.VerticalScroll.Value + ScrollDiff);
                                else
                                    Invoke(setScroll, PreviewPanelBackground.VerticalScroll.Maximum);
                            }
                        }
                        break;
                }
                lock (ScrollingLockObject)
                {
                    bHalt = HaltScroll;
                }
                ScrollVal = (Orientation == EOrientation.horizontal) ? ScrollValue : PreviewPanelBackground.VerticalScroll.Value;
                while ((DateTime.Now - startTime).Milliseconds < ScrollMaxRate)
                    Thread.Sleep(5);
            }

        }

        public void ScrollForwardOne()
        {

        }

        void NextButton_Click(object sender, EventArgs e)
        {
            int ScrollVal = ScrollValue;
            if (ScrollVal >= ScrollMax)
                return;
            int val = Math.Min(ScrollVal + PreviewSize.Width + PreviewPadding.Horizontal, ScrollMax);
            ScrollValue = val;
            new Thread(() =>
            {
                Action<Object> scrollAction = (o) =>
                {
                    Control c = (o as Tuple<Control, int>).Item1;
                    int scroll = (o as Tuple<Control, int>).Item2;
                    c.Invoke(new Action(() => { c.Location = new Point(c.Left - scroll, c.Top); }));
                };
                if (PreviewPanelBackground.HorizontalScroll.Enabled)
                {
                    while (ScrollVal < val)
                    {
                        DateTime startTime = DateTime.Now;
                        foreach (Control c in PreviewPanelBackground.Controls)
                        {
                            if (ScrollVal + ScrollDiff <= val)
                            {
                                new Task(scrollAction, new Tuple<Control, int>(c, ScrollDiff)).Start();
                            }
                            else
                            {
                                new Task(scrollAction, new Tuple<Control, int>(c, val - ScrollVal)).Start();
                            }
                        }
                        ScrollVal += ScrollDiff;
                        while ((DateTime.Now - startTime).Milliseconds < ScrollMaxRate)
                            Thread.Yield();
                    }
                }
            }).Start();
        }

        private void ScrollingPreviewPane_Load(object sender, EventArgs e)
        {
            /*
            if (Orientation == EOrientation.horizontal)
            {
                PrevButton.Size = new Size(Properties.Resources.LeftButtonArrow.Width, this.Height - 4);
                PrevButton.Image = Properties.Resources.LeftButtonArrow;
                PrevButton.ImageAlign = ContentAlignment.MiddleCenter;
                PrevButton.Location = new Point(0, 0);
                NextButton.Size = new Size(Properties.Resources.RightButtonArrow.Width, this.Height - 4);
                NextButton.Image = Properties.Resources.RightButtonArrow;
                NextButton.ImageAlign = ContentAlignment.MiddleCenter;
                NextButton.Location = new Point(this.Width - NextButton.Width, 0);
                PreviewPanelBackground.Size = new Size(this.Width - NextButton.Width - PrevButton.Width, this.Height);
                PreviewPanelBackground.Location = new Point(PrevButton.Size.Width, 0);
            }
            else if (Orientation == EOrientation.vertical)
            {
                PrevButton.Size = new Size(this.Width - 4, Properties.Resources.UpButtonArrow.Height);
                PrevButton.Image = Properties.Resources.UpButtonArrow;
                PrevButton.ImageAlign = ContentAlignment.MiddleCenter;
                PrevButton.Location = new Point(0, 0);
                NextButton.Size = new Size(this.Width - 4, Properties.Resources.DownButtonArrow.Height);
                NextButton.Image = Properties.Resources.DownButtonArrow;
                NextButton.ImageAlign = ContentAlignment.MiddleCenter;
                NextButton.Location = new Point(0, this.Height - Properties.Resources.DownButtonArrow.Height);
                PreviewPanelBackground.Size = new Size(this.Width, this.Height - PrevButton.Height - NextButton.Height);
                PreviewPanelBackground.Location = new Point(0, PrevButton.Size.Height);
            }
            else
                throw new Exception("Must set orientation of a Scrolling Preview Pane before it loads");
            PreviewPanelBackground.BackColor = System.Drawing.Color.Black;
            Controls.Add(PrevButton);
            Controls.Add(NextButton);
            Controls.Add(PreviewPanelBackground);
            
             */
            //            DoPreviewUpdates(null, null);

        }
        /*
                public void SetPreview(int ndx, IIATImage img)
                {
                    if ((ndx < 0) || (ndx >= PreviewPanels.Count))
                        throw new Exception("Index out of bounds");
                    PreviewPanels[ndx].PreviewImage = img;
                }
        */
        private void RecalcLayout()
        {
            if (RecalcSuspended)
                return;
            PreviewPanelBackground.SuspendLayout();
            switch (Orientation)
            {
                case EOrientation.horizontal:
                    if (PreviewPanels.Count * (PreviewSize.Width + PreviewPadding.Horizontal) < this.Width - PrevButton.Width - NextButton.Width)
                    {
                        PreviewPanelBackground.Width = this.Width - PrevButton.Width - NextButton.Width;
                        PreviewPanelBackground.HorizontalScroll.Enabled = false;
                    }
                    else
                    {
                        PreviewPanelBackground.Width = (PreviewSize.Width + PreviewPadding.Horizontal) * PreviewPanels.Count;
                        PreviewPanelBackground.HorizontalScroll.Enabled = true;
                        PreviewPanelBackground.HorizontalScroll.Visible = false;
                        ScrollMin = 0;
                        ScrollMax = PreviewPanels.Count * (PreviewSize.Width + PreviewPadding.Horizontal) - (this.Width - PrevButton.Width - NextButton.Width);
                        int maxScrollVal = PreviewPanels.Count * (PreviewSize.Width + PreviewPadding.Horizontal) - (this.Width - PrevButton.Width - NextButton.Width);
                        if (ScrollValue > maxScrollVal)
                        {
                            int scrollDiff = ScrollValue - maxScrollVal;
                            foreach (ScrollingPreviewPanelPane p in PreviewPanels)
                                p.Location += new Size(scrollDiff, 0);
                            ScrollValue = maxScrollVal;
                        }
                        ScrollMax = maxScrollVal;
                    }
                    break;

                case EOrientation.vertical:
                    if (PreviewPanels.Count * (PreviewSize.Height + PreviewPadding.Vertical) < this.Height - PrevButton.Height - NextButton.Height)
                    {
                        PreviewPanelBackground.Height = this.Height - PrevButton.Height - NextButton.Height;
                        PreviewPanelBackground.VerticalScroll.Enabled = false;
                    }
                    else
                    {
                        PreviewPanelBackground.Height = (PreviewSize.Height + PreviewPadding.Vertical) * PreviewPanels.Count;
                        PreviewPanelBackground.VerticalScroll.Enabled = true;
                        PreviewPanelBackground.VerticalScroll.Visible = false;
                        ScrollMin = 0;
                        ScrollMax = PreviewPanels.Count * (PreviewSize.Width + PreviewPadding.Horizontal) - (this.Width - PrevButton.Width - NextButton.Width);
                        int maxScrollVal = PreviewPanels.Count * (PreviewSize.Width + PreviewPadding.Horizontal) - (this.Width - PrevButton.Width - NextButton.Width);
                        if (ScrollValue > maxScrollVal)
                        {
                            int scrollDiff = ScrollValue - maxScrollVal;
                            foreach (ScrollingPreviewPanelPane p in PreviewPanels)
                                p.Location += new Size(scrollDiff, 0);
                            ScrollValue = maxScrollVal;
                        }
                        ScrollMax = maxScrollVal;
                    }
                    break;

                default:
                    throw new Exception("Orientation unset");
            }
            PreviewPanelBackground.ResumeLayout(true);
            Invalidate();
        }

        private EOrientation GetOrientation()
        {
            return Orientation;
        }

        public void ResetScroll()
        {
            ScrollValue = 0;
            PreviewPanelBackground.VerticalScroll.Value = 0;
        }

        private bool RecalcSuspended { get; set; } = false;
        public void SuspendRecalcLayout()
        {
            RecalcSuspended = true;
        }
        public void ResumeRecalcLayout()
        {
            RecalcSuspended = false;
        }


        public void InsertPreview(int ndx, IThumbnailPreviewable scrn)
        {
            if ((ndx < 0) || (ndx > PreviewPanels.Count))
                throw new Exception("Index out of bounds");
            ScrollingPreviewPanelPane p = new ScrollingPreviewPanelPane();
            p.Size = PreviewSize;
            p.Click += new EventHandler(Preview_Click);
            p.OnDragStart += new ScrollingPreviewPanelPane.DragStartHandler(ScrollingPreviewPanel_DragStartHandler);
            p.OnDragEnd += new ScrollingPreviewPanelPane.DragEndHandler(ScrollingPreviewPanel_DragEndHandler);
            p.IsDragging += new ScrollingPreviewPanelPane.IsDraggingCallback(ScrollingPreviewPanel_IsDragging);
            p.ParentOrientation += new ScrollingPreviewPanelPane.ParentOrientationCallback(GetOrientation);
            p.BackColor = CIAT.SaveFile.Layout.BackColor;
            p.ForeColor = CIAT.SaveFile.Layout.BackColor;
            PreviewPanels.Insert(ndx, p);
            if (Orientation == EOrientation.horizontal)
            {
                p.Location = new Point(PreviewPadding.Left + (ndx * (PreviewSize.Width + PreviewPadding.Horizontal)) - ScrollValue,
                    PreviewPadding.Top);
                for (int ctr = ndx + 1; ctr < PreviewPanels.Count; ctr++)
                    PreviewPanels[ctr].Location = PreviewPanels[ctr].Location + new Size(PreviewSize.Width + PreviewPadding.Horizontal, 0);
            }
            else if (Orientation == EOrientation.vertical)
            {
                p.Location = new Point(PreviewPadding.Left, PreviewPadding.Top + (ndx * (PreviewSize.Height + PreviewPadding.Vertical)) - PreviewPanelBackground.VerticalScroll.Value);
                for (int ctr = ndx + 1; ctr < PreviewPanels.Count; ctr++)
                    PreviewPanels[ctr].Location = PreviewPanels[ctr].Location + new Size(0, PreviewSize.Height + PreviewPadding.Vertical);

            }
            else
                throw new Exception("Orientation of control not set.");
            PreviewPanelBackground.Controls.Add(p);
            p.PreviewedItem = scrn;
            RecalcLayout();
        }

        public void Replace(int ndx, IThumbnailPreviewable item)
        {
            this[ndx].PreviewedItem = item;
        }
        /*
    public void InsertIATItemPreview(int ndx, CIATItem item)
    {
        SuspendLayout();
        if ((ndx < 0) || (ndx > PreviewPanels.Count))
            throw new Exception("Index out of bounds");
        ScrollingPreviewPanelPane p = new ScrollingPreviewPanelPane();
        p.Size = PreviewSize;
        p.Click += new EventHandler(Preview_Click);
        p.OnDragStart += new ScrollingPreviewPanelPane.DragStartHandler(ScrollingPreviewPanel_DragStartHandler);
        p.OnDragEnd += new ScrollingPreviewPanelPane.DragEndHandler(ScrollingPreviewPanel_DragEndHandler);
        p.IsDragging += new ScrollingPreviewPanelPane.IsDraggingCallback(ScrollingPreviewPanel_IsDragging);
        p.ParentOrientation += new ScrollingPreviewPanelPane.ParentOrientationCallback(GetOrientation);
        PreviewPanels.Insert(ndx, p);
        if (Orientation == EOrientation.horizontal)
        {
            p.Location = new Point(PreviewPadding.Left + (ndx * (PreviewSize.Width + PreviewPadding.Horizontal)) - ScrollValue,
                PreviewPadding.Top);
            for (int ctr = ndx + 1; ctr < PreviewPanels.Count; ctr++)
                PreviewPanels[ctr].Location = PreviewPanels[ctr].Location + new Size(PreviewSize.Width + PreviewPadding.Horizontal, 0);
        }
        else if (Orientation == EOrientation.vertical)
        {
            p.Location = new Point(PreviewPadding.Left, PreviewPadding.Top + (ndx * (PreviewSize.Height + PreviewPadding.Vertical)) - PreviewPanelBackground.VerticalScroll.Value);
            for (int ctr = ndx + 1; ctr < PreviewPanels.Count; ctr++)
                PreviewPanels[ctr].Location = PreviewPanels[ctr].Location + new Size(0, PreviewSize.Height + PreviewPadding.Vertical);

        }
        else
            throw new Exception("Orientation of control not set.");
        PreviewPanelBackground.Controls.Add(p);
        ResumeLayout(false);
        p.PreviewedItem = item;
        item.Stimulus.ThumbnailPreviewPanel = p;
        RecalcLayout();
    }
    */
        private void ScrollingPreviewPanel_DragStartHandler(ScrollingPreviewPanelPane sender)
        {
            nDraggingNdx = PreviewPanels.IndexOf(sender);
            if (sender.PreviewedItem != null)
                Clipboard.SetData("IATItemNdx", nDraggingNdx);
            IsDragging = true;
            DoDragDrop(sender, DragDropEffects.Move);
        }

        private bool ScrollingPreviewPanel_IsDragging()
        {
            return IsDragging;
        }

        private void MovePanel(int StartNdx, int DestNdx)
        {
            ScrollingPreviewPanelPane DraggedPanel = PreviewPanels[StartNdx];
            PreviewPanelBackground.SuspendLayout();
            if (DestNdx < StartNdx)
            {
                switch (Orientation)
                {
                    case EOrientation.horizontal:
                        PreviewPanels[StartNdx].Location -= new Size((StartNdx - DestNdx) * (PreviewSize.Width + PreviewPadding.Horizontal), 0);
                        break;

                    case EOrientation.vertical:
                        PreviewPanels[StartNdx].Location -= new Size(0, (StartNdx - DestNdx) * (PreviewSize.Height + PreviewPadding.Vertical));
                        break;
                }
                for (int ctr = DestNdx; ctr < StartNdx; ctr++)
                {
                    switch (Orientation)
                    {
                        case EOrientation.horizontal:
                            PreviewPanels[ctr].Location += new Size(PreviewSize.Width + PreviewPadding.Horizontal, 0);
                            break;

                        case EOrientation.vertical:
                            PreviewPanels[ctr].Location += new Size(0, PreviewSize.Height + PreviewPadding.Vertical);
                            break;
                    }
                }
                PreviewPanels.RemoveAt(StartNdx);
                PreviewPanels.Insert(DestNdx, DraggedPanel);
                OnMoveContainerItem(StartNdx, DestNdx);
            }
            else
            {
                switch (Orientation)
                {
                    case EOrientation.horizontal:
                        PreviewPanels[StartNdx].Location += new Size((DestNdx - StartNdx) * (PreviewSize.Width + PreviewPadding.Horizontal), 0);
                        break;

                    case EOrientation.vertical:
                        PreviewPanels[StartNdx].Location += new Size(0, (DestNdx - StartNdx) * (PreviewSize.Height + PreviewPadding.Vertical));
                        break;
                }
                for (int ctr = StartNdx + 1; ctr <= DestNdx; ctr++)
                {
                    switch (Orientation)
                    {
                        case EOrientation.horizontal:
                            PreviewPanels[ctr].Location -= new Size(PreviewSize.Width + PreviewPadding.Horizontal, 0);
                            break;

                        case EOrientation.vertical:
                            PreviewPanels[ctr].Location -= new Size(0, PreviewSize.Height + PreviewPadding.Vertical);
                            break;
                    }
                }
                PreviewPanels.RemoveAt(StartNdx);
                PreviewPanels.Insert(DestNdx, DraggedPanel);
                OnMoveContainerItem(StartNdx, DestNdx);
            }
            PreviewPanelBackground.HorizontalScroll.Visible = false;
            PreviewPanelBackground.ResumeLayout(true);
        }

        private void ScrollingPreviewPanel_DragEndHandler(ScrollingPreviewPanelPane sender, bool InsertBefore)
        {
            IsDragging = false;
            int destNdx = PreviewPanels.IndexOf(sender);
            if ((destNdx > nDraggingNdx) && InsertBefore)
                destNdx--;
            else if ((destNdx < nDraggingNdx) && !InsertBefore)
                destNdx++;
            if (destNdx == nDraggingNdx)
            {
                SelectedPreview = nDraggingNdx;
                PreviewClickCallback(nDraggingNdx);
                nDraggingNdx = -1;
                return;
            }
            MovePanel(nDraggingNdx, destNdx);
            nDraggingNdx = -1;
            PreviewClickCallback(destNdx);
        }

        public void DeletePreview(int ndx, bool selectNew = true)
        {
            SuspendLayout();
            if ((ndx < 0) || (ndx >= PreviewPanels.Count))
                throw new Exception("Index out of bounds");
            ScrollingPreviewPanelPane p = PreviewPanels[ndx];
            try
            {
                if (p.PreviewedItem.ThumbnailPreviewPanel != null)
                    p.PreviewedItem.ThumbnailPreviewPanel = null;
            }
            catch (KeyNotFoundException) { }
            PreviewPanelBackground.Controls.Remove(p);
            PreviewPanels.Remove(p);
            if (selectNew)
            {
                if (ndx == 0)
                    SelectedPreview = 0;
                else
                    SelectedPreview = ndx - 1;
            }
            if (Orientation == EOrientation.horizontal)
            {
                for (int ctr = ndx; ctr < PreviewPanels.Count; ctr++)
                    PreviewPanels[ctr].Location = PreviewPanels[ctr].Location - new Size(PreviewSize.Width + PreviewPadding.Horizontal, 0);
            }
            else if (Orientation == EOrientation.vertical)
            {
                for (int ctr = ndx; ctr < PreviewPanels.Count; ctr++)
                    PreviewPanels[ctr].Location = PreviewPanels[ctr].Location - new Size(0, PreviewSize.Height + PreviewPadding.Vertical);
            }
            ResumeLayout(true);
            RecalcLayout();
        }

        private void Preview_Click(object sender, EventArgs e)
        {
            SelectedPreview = PreviewPanels.IndexOf((ScrollingPreviewPanelPane)sender);
        }

        public new void Dispose()
        {
            foreach (ScrollingPreviewPanelPane p in PreviewPanels)
                p.Dispose();
            base.Dispose();
        }
    }
}
