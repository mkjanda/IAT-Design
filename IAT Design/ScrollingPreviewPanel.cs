using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
namespace IATClient
{
    public class ScrollingPreviewPanel : UserControl
    {
        private List<ScrollingPreviewPanelPane> PreviewPanels;

        private int ScrollValue = 0, ScrollMin;
        private Panel PreviewPanel;
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
                    {
                        PreviewPanels[ctr].Selected = true;
                        PreviewPanels[ctr].ImageBox.Invalidate();
                    }
                    else
                    {
                        PreviewPanels[ctr].Selected = false;
                        PreviewPanels[ctr].ImageBox.Invalidate();
                    }
                nSelectedPreview = value;
                PreviewClickCallback(value);
            }
        }

        public Size PreviewSize
        {
            get
            {
                return Images.ImageManager.ThumbnailSize;
            }
            set
            {
                if (IsLoaded)
                    throw new Exception("Cannot set a preview size on an initialized control");
                _PreviewSize = value - new Size(PreviewPadding.Horizontal, PreviewPadding.Vertical);
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
            //            this.AutoScaleDimensions = new SizeF(72F, 72F);
            //          this.AutoScaleMode = AutoScaleMode.Dpi;
            //        this.Width = IATConfigMainForm.MainForm.Width;
            this.Height = PreviewSize.Height + PreviewPadding.Vertical;
            this.BorderStyle = BorderStyle.Fixed3D;
            PreviewPanels = new List<ScrollingPreviewPanelPane>();
            _Orientation = EOrientation.horizontal;
            NextButton = new Button();
            PrevButton = new Button();
            PreviewPanel = new Panel();
            PreviewPanelBackground = new Panel();
            SuspendLayout();
            NextButton.Size = new Size(Properties.Resources.RightButtonArrow.Width, this.ClientRectangle.Height - 4);
            NextButton.Image = Properties.Resources.RightButtonArrow;
            NextButton.ImageAlign = ContentAlignment.MiddleCenter;
            NextButton.Location = new Point(this.Width - NextButton.Width, 0);
            this.Resize += (sender, evt) =>
            {
                NextButton.Location = new Point(IATConfigMainForm.MainForm.Width - (NextButton.Width << 1), 0);
                PreviewPanel.Width = this.Width - (NextButton.Width << 1) - PrevButton.Width;
            };
            PrevButton.Size = new Size(Properties.Resources.LeftButtonArrow.Width, this.ClientRectangle.Height - 4);
            PrevButton.Image = Properties.Resources.LeftButtonArrow;
            PrevButton.ImageAlign = ContentAlignment.MiddleCenter;
            PrevButton.Location = new Point(0, 0);
            NextButton.Location = new Point(PreviewPanelBackground.Right, 0);
            NextButton.Click += new EventHandler(NextButton_Click);
            PrevButton.Click += new EventHandler(PrevButton_Click);
            //            PreviewPanel.Anchor = AnchorStyles.Right | AnchorStyles.Left;
            PreviewPanelBackground.Location = new Point(0, 0);
            PreviewPanelBackground.Size = new Size(0, 0);
            PreviewPanel.Location = new Point(PrevButton.Right, 0);
            PreviewPanel.Width = NextButton.Left - PrevButton.Right;
            PreviewPanel.Top = 0;
            PreviewPanel.Height = ClientSize.Height;
            PreviewPanelBackground.BackColor = System.Drawing.Color.Black;
            PreviewPanel.Controls.Add(PreviewPanelBackground);
            PreviewPanel.BackColor = Color.Black;
            Controls.Add(PrevButton);
            Controls.Add(NextButton);
            Controls.Add(PreviewPanel);
            ResumeLayout(true);
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
                        PreviewPanelBackground.BeginInvoke(new Action<int>((n) =>
                        {
                            PreviewPanelBackground.Location = new Point(PreviewPanelBackground.Left + n,
                            PreviewPanelBackground.Top);
                        }), -sign * ScrollDiff);
                        pixMoved += ScrollDiff;
                    }
                    else
                    {
                        PreviewPanelBackground.BeginInvoke(new Action<int>((n) =>
                        {
                            PreviewPanelBackground.Location = new Point(PreviewPanelBackground.Left + n,
                            PreviewPanelBackground.Top);
                        }), -sign * (Math.Abs(numPixels) - Math.Abs(pixMoved)));
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
            int pWidth = PreviewPanels.Select(p => p.Width).Max();
            pWidth += pWidth * PreviewPadding.Horizontal / PreviewSize.Width;
            int val = Math.Max(ScrollVal - (ScrollVal % pWidth == 0 ? pWidth : ScrollVal % pWidth), 0);
            ScrollValue = val;
            new Thread(() =>
            {
                Action<Object> scrollAction = (o) =>
                {
                    Control c = (o as Tuple<Control, int>).Item1;
                    int scroll = (o as Tuple<Control, int>).Item2;
                    c.Invoke(new Action(() => { c.Location = new Point(c.Left + scroll, c.Top); c.Parent.Invalidate(); }));
                };
                if (PreviewPanelBackground.HorizontalScroll.Enabled)
                {
                    while (ScrollVal > val)
                    {
                        DateTime startTime = DateTime.Now;
                        if (ScrollVal - ScrollDiff >= val)
                        {
                            new Task(scrollAction, new Tuple<Control, int>(PreviewPanelBackground, ScrollDiff)).Start();
                        }
                        else
                        {
                            new Task(scrollAction, new Tuple<Control, int>(PreviewPanelBackground, ScrollVal - val)).Start();
                        }
                        ScrollVal -= ScrollDiff;
                        while ((DateTime.Now - startTime).Milliseconds < ScrollMaxRate)
                            Thread.Yield();
                    }
                }
            }).Start();
        }


        void NextButton_Click(object sender, EventArgs e)
        {
            int ScrollVal = ScrollValue;
            if (ScrollVal >= ScrollMax)
                return;
            int pWidth = PreviewPanels.Select(p => p.Width).Max();
            pWidth += pWidth * PreviewPadding.Horizontal / PreviewSize.Width;
            int val = Math.Min(ScrollVal + pWidth - (PreviewPanel.Width - (pWidth - (ScrollVal % pWidth))) % pWidth, ScrollMax);
            ScrollValue = val;
            new Thread(() =>
            {
                Action<Object> scrollAction = (o) =>
                {
                    Control c = (o as Tuple<Control, int>).Item1;
                    int scroll = (o as Tuple<Control, int>).Item2;
                    c.Invoke(new Action(() => { c.Location = new Point(c.Left - scroll, c.Top); c.Parent.Invalidate(); }));
                };
                if (PreviewPanelBackground.HorizontalScroll.Enabled)
                {
                    while (ScrollVal < val)
                    {
                        DateTime startTime = DateTime.Now;
                        if (ScrollVal + ScrollDiff <= val)
                        {
                            new Task(scrollAction, new Tuple<Control, int>(PreviewPanelBackground, ScrollDiff)).Start();
                        }
                        else
                        {
                            new Task(scrollAction, new Tuple<Control, int>(PreviewPanelBackground, val - ScrollVal)).Start();
                        }
                        ScrollVal += ScrollDiff;
                        while ((DateTime.Now - startTime).Milliseconds < ScrollMaxRate)
                            Thread.Yield();
                    }
                }
            }).Start();
        }
        private void RecalcLayout()
        {
            if (RecalcSuspended)
                return;
            PreviewPanelBackground.SuspendLayout();
            PreviewPanelBackground.Size = new Size(this.ClientRectangle.Width - this.NextButton.Width - this.PrevButton.Width, PreviewPanelBackground.Height);
            if (PreviewPanels.Count * (PreviewSize.Width + PreviewPadding.Horizontal) < this.Width - PrevButton.Width - NextButton.Width)
                PreviewPanelBackground.HorizontalScroll.Enabled = false;
            else
            {
                PreviewPanelBackground.HorizontalScroll.Enabled = true;
                PreviewPanelBackground.HorizontalScroll.Visible = false;
                ScrollMin = 0;
                int maxScrollVal = PreviewPanels.Count * (PreviewSize.Width + PreviewPadding.Horizontal) - (this.Width - PrevButton.Width - NextButton.Width);
                if (ScrollValue > maxScrollVal)
                {
                    int scrollDiff = ScrollValue - maxScrollVal;
                    foreach (ScrollingPreviewPanelPane p in PreviewPanels)
                        p.Location += new Size(scrollDiff, 0);
                    ScrollValue = maxScrollVal;
                }
            }
            PreviewPanelBackground.ResumeLayout(true);
            Invalidate();
        }

        public int ScrollMax
        {
            get
            {
                return Math.Max(0, PreviewPanelBackground.Width - PreviewPanel.Width);
            }
        }

        private EOrientation GetOrientation()
        {
            return Orientation;
        }

        public void ResetScroll()
        {
            if (IsHandleCreated)
            {
                ScrollValue = 0;
                PreviewPanelBackground.VerticalScroll.Value = 0;
            }
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
            //            p.AutoScaleDimensions = new SizeF(72F, 72F);
            //          p.AutoScaleMode = AutoScaleMode.Dpi;
            p.ImageBox.Click += new EventHandler(Preview_Click);
            p.ParentOrientation += new ScrollingPreviewPanelPane.ParentOrientationCallback(GetOrientation);
            p.BackColor = CIAT.SaveFile.Layout.BackColor;
            p.ForeColor = CIAT.SaveFile.Layout.BackColor;
            PreviewPanels.Insert(ndx, p);
            if (Orientation == EOrientation.horizontal)
            {
                p.Location = new Point(LogicalToDeviceUnits(PreviewPadding.Left + (ndx == 0 ? 0 : (PreviewPanels[ndx - 1].Right + PreviewPadding.Right))),
                    LogicalToDeviceUnits(PreviewPadding.Top));
                for (int ctr = ndx + 1; ctr < PreviewPanels.Count; ctr++)
                    PreviewPanels[ctr].Location = PreviewPanels[ctr].Location + LogicalToDeviceUnits(new Size(PreviewSize.Width + PreviewPadding.Horizontal, 0));
            }
            else if (Orientation == EOrientation.vertical)
            {
                p.Location = new Point(PreviewPadding.Left, PreviewPadding.Top + (ndx * (PreviewSize.Height + PreviewPadding.Vertical)) - PreviewPanelBackground.VerticalScroll.Value);
                for (int ctr = ndx + 1; ctr < PreviewPanels.Count; ctr++)
                    PreviewPanels[ctr].Location = PreviewPanels[ctr].Location + new Size(0, PreviewSize.Height + PreviewPadding.Vertical);

            }
            else
                throw new Exception("Orientation of control not set.");
            PreviewPanelBackground.Size = new Size(PreviewPanels.Select(p => p.ClientSize.Width).Aggregate((p1, p2) => p1 + p2) +
                PreviewPanels.Count * LogicalToDeviceUnits(PreviewPadding.Horizontal), PreviewPanels.Select(p => p.ClientSize.Height).Max() + LogicalToDeviceUnits(PreviewPadding.Vertical));
            PreviewPanelBackground.Controls.Add(p);
            p.PreviewedItem = scrn;
        }

        public void Replace(int ndx, IThumbnailPreviewable item)
        {
            this[ndx].PreviewedItem = item;
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
            SelectedPreview = PreviewPanels.IndexOf((sender as PictureBox).Parent as ScrollingPreviewPanelPane);
        }

        public new void Dispose()
        {
            foreach (ScrollingPreviewPanelPane p in PreviewPanels)
                p.Dispose();
            base.Dispose();
        }
    }
}
