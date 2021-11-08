using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    public interface IPreviewableItem 
    {
        void Preview(IImageDisplay previewPanel);
        void EndPreview(IImageDisplay previewPanel);
        void OpenItem(IATConfigMainForm MainForm);
        String PreviewText { get; }
        bool IsHeaderItem { get; }
        Button GUIButton { get; set; }
        bool IsDisposed { get; }
        bool IsSurvey { get; }
    }

    public class CollapsableTreeButton : UserControl
    {
        public delegate IImageDisplay RetrievePreviewPanelCallback(IPreviewableItem UpdatingItem);
        public delegate void RecalcLayoutHandler(IContentsItem sender, Size szControl);
        
        public RetrievePreviewPanelCallback OnRetrievePreviewPanel = null;
        public RecalcLayoutHandler RecalcLayout= null;
        public static System.Drawing.Color HeaderBaseColor = System.Drawing.Color.FromArgb(100, System.Drawing.Color.DeepSkyBlue);
        public static System.Drawing.Color HeaderHighlightColor = System.Drawing.Color.FromArgb(200, System.Drawing.Color.DeepSkyBlue);
        public static System.Drawing.Color ChildBaseColor = System.Drawing.Color.FromArgb(120, System.Drawing.Color.LimeGreen);
        public static System.Drawing.Color ChildHighlightColor = System.Drawing.Color.FromArgb(240, System.Drawing.Color.LimeGreen);
        public static System.Drawing.Color[] AlternateBaseColors = new System.Drawing.Color[] { System.Drawing.Color.FromArgb(100, System.Drawing.Color.Red), System.Drawing.Color.FromArgb(100, System.Drawing.Color.Green), System.Drawing.Color.FromArgb(100, System.Drawing.Color.Yellow),
           System.Drawing.Color.FromArgb(100, System.Drawing.Color.Purple), System.Drawing.Color.FromArgb(100, System.Drawing.Color.Azure), System.Drawing.Color.FromArgb(100, System.Drawing.Color.Orange), System.Drawing.Color.FromArgb(100, System.Drawing.Color.GreenYellow), System.Drawing.Color.FromArgb(100, System.Drawing.Color.Blue),
           System.Drawing.Color.FromArgb(100, System.Drawing.Color.Gold), System.Drawing.Color.FromArgb(100, System.Drawing.Color.Cyan) };
        public static System.Drawing.Color[] AlternateHighlightColors = new System.Drawing.Color[] { System.Drawing.Color.FromArgb(200, System.Drawing.Color.Red), System.Drawing.Color.FromArgb(200, System.Drawing.Color.Green), System.Drawing.Color.FromArgb(200, System.Drawing.Color.Yellow),
           System.Drawing.Color.FromArgb(200, System.Drawing.Color.Purple), System.Drawing.Color.FromArgb(200, System.Drawing.Color.Azure), System.Drawing.Color.FromArgb(200, System.Drawing.Color.Orange), System.Drawing.Color.FromArgb(200, System.Drawing.Color.GreenYellow), System.Drawing.Color.FromArgb(200, System.Drawing.Color.Blue),
           System.Drawing.Color.FromArgb(200, System.Drawing.Color.Gold), System.Drawing.Color.FromArgb(200, System.Drawing.Color.Cyan) };


        private bool _Expanded = false;
        private int ChildButtonMargin 
        {
            get {
                return this.Width / 20;
            }
        }
        private Button HeaderButton = new Button();
        private IContentsItem HeaderItem = null;
        private Dictionary<Button, IPreviewableItem> ChildItems = new Dictionary<Button, IPreviewableItem>();
        private IATConfigMainForm _MainForm;


        public IATConfigMainForm MainForm
        {
            get {
                return _MainForm;
            }
        }

        public bool Expanded
        {
            get
            {
                return _Expanded;
            }
        }

        public CollapsableTreeButton(IContentsItem Header, IATConfigMainForm mainForm, int width)
        {
            this.Size = new Size(width, 26);
            HeaderItem = Header;
            this.ParentChanged += new EventHandler(CollapsableTreeButton_ParentChanged);
            this.SizeChanged += new EventHandler(CollapsableTreeButton_SizeChanged);
            _MainForm = mainForm;
            SetChildItems(Header.SubContentsItems);
        }

        void CollapsableTreeButton_SizeChanged(object sender, EventArgs e)
        {
            SuspendLayout();
            int oldWidth = HeaderButton.Width;
            HeaderButton.Size = new Size(this.Width, 26);
            int mod = 0;
            foreach (Button b in ChildItems.Keys)
            {
                int oldBWidth = b.Width;
                b.Size = new Size((this.Width - (ChildButtonMargin << 1)) / 3, 26);
                b.Location = new Point((ChildButtonMargin) + (b.Width * mod), b.Location.Y);
                if (++mod > 2)
                    mod = 0;
            }
            ResumeLayout(false);
        }

        public bool Contains(IPreviewableItem item)
        {
            foreach (IPreviewableItem i in ChildItems.Values)
                if (item == i)
                    return true;
            return false;
        }


        public void SetChildItems(List<IPreviewableItem> childItems)
        {
            foreach (Button b in ChildItems.Keys)
                Controls.Remove(b);
            ChildItems.Clear();
            Point loc = new Point(ChildButtonMargin, HeaderButton.Bottom);            
            for (int ctr = 0; ctr < childItems.Count; ctr++)
            {
                Button childButton = new Button();
                childButton.Size = new Size((this.Width - (ChildButtonMargin << 1)) / 3, 26);
                childButton.Text = childItems[ctr].PreviewText;
                childButton.Click += new EventHandler(ChildButton_Click);
                childButton.DoubleClick += new EventHandler(ChildButton_DoubleClick);
                childButton.Location = loc;
                childButton.BackColor = ChildBaseColor;
                ChildItems[childButton] = childItems[ctr];
                childItems[ctr].GUIButton = childButton;
                if (ctr % 3 == 2)
                {
                    loc.Y += childButton.Height;
                    loc.X = ChildButtonMargin;
                }
                else
                    loc.X += (this.Width - (ChildButtonMargin << 1)) / 3;
                if (Expanded)
                    Controls.Add(childButton);
            }
            if (Expanded && (ChildItems.Count > 0))
            {
                this.Size = new Size(this.Width, loc.Y + ((ChildItems.Count % 3 != 0) ? 26 : 0));
                RecalcLayout(HeaderItem, this.Size);
            }
        }

        private void ChildButton_Click(object sender, EventArgs e)
        {
            if (OnRetrievePreviewPanel != null)
                ChildItems[(Button)sender].Preview(OnRetrievePreviewPanel(ChildItems[(Button)sender]));
        }

        private void ChildButton_DoubleClick(object sender, EventArgs e) 
        {
            ChildItems[(Button)sender].OpenItem(MainForm);
        }

        private void CollapsableTreeButton_ParentChanged(object sender, EventArgs e)
        {
            if (Parent == null)
            {
                Controls.Clear();
            }
            else
            {
                HeaderButton = new Button();
                HeaderButton.Size = new Size(this.Width, 26);
                HeaderButton.Text = HeaderItem.Name;
                if (HeaderItem.AlternationGroup == null)
                    HeaderButton.BackColor = HeaderBaseColor;
                else
                    HeaderButton.BackColor = AlternateBaseColors[HeaderItem.AlternationGroup.GroupID];
                HeaderButton.Click += new EventHandler(HeaderButton_Click);
                HeaderItem.GUIButton = HeaderButton;
                Controls.Add(HeaderButton);
            }
        }

        private void HeaderButton_MouseEnter(object sender, EventArgs e)
        {
            HeaderButton.BackColor = HeaderHighlightColor;
        }

        private void HeaderButton_MouseLeave(object sender, EventArgs e)
        {
            HeaderButton.BackColor = HeaderBaseColor;
        }

        private void HeaderButton_Click(object sender, EventArgs e)
        {
            if (ChildItems.Count != 0)
            {
                SuspendLayout();
                if (Expanded)
                {
                    foreach (Button b in ChildItems.Keys)
                        Controls.Remove(b);
                    this.Size = HeaderButton.Size;
                    RecalcLayout(HeaderItem, this.Size);
                    _Expanded = false;
                }
                else
                {
                    int bottom = 0;
                    foreach (Button b in ChildItems.Keys)
                    {
                        Controls.Add(b);
                        if (b.Bottom > bottom)
                            bottom = b.Bottom;
                    }
                    this.Height = bottom;
                    RecalcLayout(HeaderItem, this.Size);
                    _Expanded = true;
                }
                ResumeLayout(false);
            }
            HeaderItem.Preview(OnRetrievePreviewPanel(HeaderItem));
        }

        private void HeaderButton_DoubleClick(object sender, EventArgs e)
        {
            HeaderItem.OpenItem(MainForm);
        }

        private void ChildButton_MouseEnter(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = ChildHighlightColor;
        }

        private void ChildButton_MouseLeave(object sender, EventArgs e)
        {
            ((Button)sender).BackColor = ChildBaseColor;
        }
    }
}
