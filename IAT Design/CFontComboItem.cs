using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class CFontComboItem
    {
        private Image _FontFaceImage;
        private String _FontFaceName;

        public CFontComboItem()
        {
            _FontFaceImage = null;
            _FontFaceName = String.Empty;
        }

        public String FontFaceName
        {
            get 
            {
                return _FontFaceName;
            }
            set 
            {
                _FontFaceName = value;
            }
        }

        public Image FontFaceImage
        {
            get
            {
                return _FontFaceImage;
            }
            set
            {
                if (_FontFaceImage != null)
                    _FontFaceImage.Dispose();
                _FontFaceImage = value;
            }
        }

        public void Draw(Graphics g, Brush backBrush, Rectangle bounds)
        {
            g.FillRectangle(backBrush, bounds);
            if (FontFaceImage != null)
                g.DrawImage(FontFaceImage, bounds.Location);
        }

        public Size Measure()
        {
            if (FontFaceImage == null)
                return Size.Empty;
            else
                return FontFaceImage.Size;
        }

        static CFontComboItem[] GenerateFontComboItems()
        {
            CFontComboItem[] comboItems = new CFontComboItem[CFontFile.AvailableFonts.Length];
            for (int ctr = 0; ctr < CFontFile.AvailableFonts.Length; ctr++)
            {
                CFontComboItem ci = new CFontComboItem();
                ci.FontFaceImage = CFontFile.AvailableFonts[ctr].FontImage;
                ci.FontFaceName = CFontFile.AvailableFonts[ctr].FamilyName;
            }
            return comboItems;
        }
    }
}
