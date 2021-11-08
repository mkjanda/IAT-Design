using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    public class ItemSlidePanel : Panel
    {
        private int NumSlides;
        private List<ItemSlideThumbnailPanel> ThumbnailPanels = new List<ItemSlideThumbnailPanel>();
        private List<Label> ThumbLabels = new List<Label>();
        private ItemSlideDisplayPanel FullSizedSlide;
        private static Padding ThumbnailPadding = new Padding(10, 10, 10, 5);
        private Panel ThumbPanel = new Panel();
        private int nCols, nColsWithSlide;
        private int nRows, nRowsWithSlide;
        private int nImagesReceived;
        private CItemSlideContainer ItemSlideContainer;
        private delegate void UpdateImageHandler(Image img);
        private int FullSizedNdx = -1;
        private bool _IsInitialized = false;
        private int CollapsedPanelWidth = 0;
        private CResultData ResultData = null;
        private int _ResultSet = -1;

        public int ResultSet
        {
            get
            {
                return _ResultSet;
            }
            set
            {
                _ResultSet = value;
                if (FullSizedNdx != -1)
                {
                    FullSizedSlide.SetResultData(ItemSlideContainer.GetSlideLatencies(FullSizedNdx + 1, value), ItemSlideContainer.GetMeanSlideLatency(FullSizedNdx + 1),
                        ItemSlideContainer.GetMeanNumErrors(FullSizedNdx + 1), value + 1);
                    FullSizedSlide.Invalidate();
                }
            }
        }

        public bool IsInitialized
        {
            get
            {
                return _IsInitialized;
            }
        }

        public ItemSlidePanel(Size sz, CResultData rData, int resultSet)
        {
            this.Size = sz;
            this.ResultData = rData;
            this._ResultSet = resultSet;
        }

        public void Initialize(CItemSlideContainer itemSlideContainer)
        {
            ItemSlideContainer = itemSlideContainer;
            ItemSlideContainer.SetResultData(ResultData);
            NumSlides = ItemSlideContainer.NumSlides;
            for (int ctr = 0; ctr < NumSlides; ctr++)
            {
                ItemSlideThumbnailPanel thumbPanel = new ItemSlideThumbnailPanel(new EventHandler(ItemSlide_Click), ItemSlideContainer.ThumbSize);
                ThumbnailPanels.Add(thumbPanel);
                Label l = new Label();
                l.Text = String.Format("Slide #{0}", ctr + 1);
                l.Font = System.Drawing.SystemFonts.DefaultFont;
                l.BackColor = System.Drawing.Color.White;
                l.ForeColor = System.Drawing.Color.Black;
                l.Size = TextRenderer.MeasureText(l.Text, l.Font);
                ThumbLabels.Add(l);
            }
            ThumbPanel.Size = this.Size;
            ThumbPanel.Anchor = AnchorStyles.Top | AnchorStyles.Bottom;
            ThumbPanel.AutoScroll = true;
            nCols = this.Width / (ItemSlideContainer.ThumbSize.Width + ThumbnailPadding.Horizontal);
            nRows = (NumSlides / nCols);
            if (NumSlides % nCols != 0)
                nRows++;
            nColsWithSlide = (this.Width - (ItemSlideContainer.DisplaySize.Width + ItemSlideDisplayPanel.DisplayPadding.Horizontal)) / (ItemSlideContainer.ThumbSize.Width + ThumbnailPadding.Horizontal);
            nRowsWithSlide = (NumSlides / nColsWithSlide);
            if (NumSlides % nColsWithSlide != 0)
                nRowsWithSlide++;
            CollapsedPanelWidth = (nColsWithSlide * (ItemSlideContainer.ThumbSize.Width + ThumbnailPadding.Horizontal)) + 20;
            FullSizedSlide = new ItemSlideDisplayPanel(new ItemSlideDisplayPanel.CloseEventHandler(HideFullSizedSlide), ItemSlideContainer.DisplaySize);
            FullSizedSlide.Location = new Point(CollapsedPanelWidth + ((this.Width - FullSizedSlide.Width - CollapsedPanelWidth) >> 1), (this.Height - FullSizedSlide.Height) >> 1);
            LayoutThumbPanel(nCols, nRows);
            this.VerticalScroll.Value = 0;
            Controls.Add(ThumbPanel);
            for (int ctr = 0; ctr < ThumbnailPanels.Count; ctr++)
            {
                ThumbPanel.Controls.Add(ThumbnailPanels[ctr]);
                ItemSlideContainer.RequestThumbnailImage(ctr + 1, ThumbnailPanels[ctr], new UpdateImageHandler(ThumbnailPanels[ctr].SetBackgroundImage));
            }
            int nRowHeight = System.Drawing.SystemFonts.DefaultFont.Height + ItemSlideContainer.ThumbSize.Height + ThumbnailPadding.Vertical;
            _IsInitialized = true;
        }

        private void LayoutThumbPanel(int Cols, int Rows)
        {
            int xOffset = 0, yOffset = 0;
            ThumbPanel.Controls.Clear();
            for (int ctr1 = 0; ctr1 < Rows; ctr1++)
            {
                yOffset += ThumbnailPadding.Top;
                int cols;
                if (ctr1 != Rows - 1)
                    cols = Cols;
                else
                    cols = NumSlides - ((Rows - 1) * Cols);
                xOffset = 0;
                for (int ctr2 = 0; ctr2 < cols; ctr2++)
                {
                    xOffset += ThumbnailPadding.Left;
                    ThumbnailPanels[ctr1 * Cols + ctr2].Location = new Point(xOffset, yOffset);
                    ThumbPanel.Controls.Add(ThumbnailPanels[ctr1 * Cols + ctr2]);
                    Label l = ThumbLabels[ctr1 * Cols + ctr2];
                    l.Location = new Point(ThumbnailPanels[ctr1 * Cols + ctr2].Left + ((ThumbnailPanels[ctr1 * Cols + ctr2].Width - l.Size.Width) >> 1),
                        ThumbnailPanels[ctr1 * Cols + ctr2].Bottom + ThumbnailPadding.Bottom);
                    ThumbPanel.Controls.Add(l);
                    xOffset += ThumbnailPadding.Horizontal + ItemSlideContainer.ThumbSize.Width;
                }
                yOffset += System.Drawing.SystemFonts.DefaultFont.Height + ItemSlideContainer.ThumbSize.Height;
            }
            ThumbPanel.Invalidate();
            /*
            if (yOffset >= ClientRectangle.Height)
            {
                this.Height = yOffset;
                this.VerticalScroll.Minimum = 0;
                this.VerticalScroll.Maximum = this.Height - this.ClientRectangle.Height;
                this.VerticalScroll.SmallChange = (112 + ThumbnailPadding.Vertical + System.Drawing.SystemFonts.DefaultFont.Height);
                this.VerticalScroll.LargeChange = this.ClientRectangle.Height;
            }*/
        }

        private void ShowFullSizedSlide(int ndx)
        {
            SuspendLayout();
            if (FullSizedNdx == -1)
            {
                ItemSlideThumbnailPanel thumbPanel = ThumbnailPanels[ndx];
                int nRow = ndx / nRows;
                int yPos = thumbPanel.Top - ThumbPanel.VerticalScroll.Value;
                int nNewRow = ndx / nRowsWithSlide;
                ThumbPanel.Width = CollapsedPanelWidth;
                LayoutThumbPanel(nColsWithSlide, nRowsWithSlide);
//                ThumbPanel.VerticalScroll.Value = thumbPanel.Top - yPos;
                Controls.Add(FullSizedSlide);
            }
            FullSizedSlide.SetResultData(ItemSlideContainer.GetSlideLatencies(ndx + 1, ResultSet), ItemSlideContainer.GetMeanSlideLatency(ndx + 1), ItemSlideContainer.GetMeanNumErrors(ndx + 1), ResultSet + 1);
            FullSizedSlide.SetImage(null);
            FullSizedSlide.Invalidate();
            ItemSlideContainer.RequestFullImage(ndx + 1, FullSizedSlide, new UpdateImageHandler(FullSizedSlide.SetImage));
            ResumeLayout(false);
            FullSizedNdx = ndx;
        }

        private void HideFullSizedSlide()
        {
            SuspendLayout();
            Controls.Remove(FullSizedSlide);
            ItemSlideThumbnailPanel thumbPanel = ThumbnailPanels[FullSizedNdx];
            int nRow = FullSizedNdx / nRowsWithSlide;
            int yPos = thumbPanel.Top - ThumbPanel.VerticalScroll.Value;
            ThumbPanel.Width = this.ClientRectangle.Width;
            LayoutThumbPanel(nCols, nRows);
  //          ThumbPanel.VerticalScroll.Value = thumbPanel.Top + yPos;
            FullSizedNdx = -1;
            ResumeLayout(false);
        }

        public void OnDisplaySlideClose(object sender, EventArgs e)
        {
            HideFullSizedSlide();
        }

        public void ItemSlide_Click(object sender, EventArgs e)
        {
            int ndx = ThumbnailPanels.IndexOf((ItemSlideThumbnailPanel)sender);
            if (FullSizedNdx == ndx)
                HideFullSizedSlide();
            else
            {
                ShowFullSizedSlide(ndx);
                FullSizedNdx = ndx;
            }
        }
    }
}
