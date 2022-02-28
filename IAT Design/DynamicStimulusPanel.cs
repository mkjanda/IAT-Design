using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class DynamicStimulusPanel : Panel
    {
        protected List<CIATItem> ItemList = new List<CIATItem>();
        protected List<ScrollingPreviewPanelPane> StimulusPanes = new List<ScrollingPreviewPanelPane>();
        protected List<RadioButton> LeftRadios = new List<RadioButton>();
        protected List<RadioButton> RightRadios = new List<RadioButton>();
        protected Size ThumbnailSize = Images.ImageManager.ThumbnailSize;
        public DynamicStimulusPanel()
        {

        }
    }
}
