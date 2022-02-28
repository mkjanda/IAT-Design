using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class SummaryItemSlideDisplayPanel : Panel
    {
        private delegate void UpdateSlideImageHandler(Image img);

        class SummaryItemSlidePanel : Panel
        {
            public void UpdateSlideImage(Image img)
            {
                BackgroundImage = img;
            }
        }

        private IATConfig.ConfigFile ConfigFile;
        private CItemSlideContainer SlideContainer;
        private List<Panel> SlidePanels = new List<Panel>();
        private GroupBox SlideGroup;
        private List<Label> LatencyLabels = new List<Label>();
        private List<TextBox> LatencyBoxes = new List<TextBox>();
        private Padding GroupPadding = new Padding(3, 18, 3, 3);
        private Padding SlidePadding = new Padding(0);

        public SummaryItemSlideDisplayPanel(int blockNum, IATConfig.ConfigFile cf, CItemSlideContainer slideContainer, int width, Padding slidePadding)
        {
            /*            SlidePadding = slidePadding;
                        this.ConfigFile = cf;
                        SlideContainer = slideContainer;
                        int nBlockCtr = 0;
                        int nEventNum = 0;
                        SlideGroup = new GroupBox();
                        SlideGroup.Text = String.Format("Block #{0}", blockNum);

                        while (nBlockCtr < blockNum)
                        {
                            if (cf.EventList[nEventNum++].EventType == IATConfigFileNamespace.IATEvent.EEventType.BeginIATBlock)
                                nBlockCtr++;
                        }
                        Size slideSize;
                       int nSlidesPerRow = (width - GroupPadding.Horizontal) / (SlidePadding.Horizontal + CItemSlideContainer.DefaultMediumSize.Width);
                        slideSize = new Size((width - GroupPadding.Horizontal) / nSlidesPerRow, (width - GroupPadding.Horizontal) / nSlidesPerRow);
                        slideSize.Width -= SlidePadding.Horizontal;
                        slideSize.Height -= SlidePadding.Vertical;
                        int xOffset = SlidePadding.Left + GroupPadding.Left;
                        int yOffset = GroupPadding.Top + SlidePadding.Top;
                        int nSlideCtr = 0;
                        while (cf.EventList[nEventNum + nSlideCtr].EventType != IATConfigFileNamespace.IATEvent.EEventType.EndIATBlock)
                        {
                            SummaryItemSlidePanel p = new SummaryItemSlidePanel();
                            p.Size = slideSize;
                            p.Location = new Point(xOffset, yOffset);
                            slideContainer[nEventNum + nSlideCtr].AddMiscRequest(p, new UpdateSlideImageHandler(p.UpdateSlideImage), slideSize);
                            SlideGroup.Controls.Add(p);
                            if (nSlidesPerRow % ++nSlideCtr == 0)
                            {
                                xOffset = slidePadding.Left + GroupPadding.Left;
                                yOffset += slidePadding.Vertical + slideSize.Height;
                            }
                            else
                                xOffset += slidePadding.Horizontal + slideSize.Width;
                        }
                        this.Size = new Size(GroupPadding.Horizontal + (nSlidesPerRow * slideSize.Width), GroupPadding.Bottom + yOffset + slideSize.Height + slidePadding.Bottom);
                        SlideGroup.Location = new Point(0, 0);
                        SlideGroup.Size = this.Size;
                        Controls.Add(SlideGroup);*/
        }
    }
}
