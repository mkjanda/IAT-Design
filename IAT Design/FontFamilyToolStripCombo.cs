using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace IATClient
{
    class FontFamilyToolstripCombo : ToolStripControlHost
    {
        class FontDropDownItem
        {
            public CFontFile.FontData FontData { get; set; }
            private String _FamilyName;

            public FontDropDownItem(CFontFile.FontData fd)
            {
                FontData = fd;
                _FamilyName = fd.FamilyName;
            }

            public String FamilyName
            {
                get
                {
                    return this._FamilyName;
                }
            }

            public override String ToString()
            {
                return _FamilyName;
            }
        }

        List<FontDropDownItem> FontDataItems = new List<FontDropDownItem>();

        public FontFamilyToolstripCombo() : base(new ComboBox())
        {
            Initialize();
        }

        public ComboBox FontCombo
        {
            get
            {
                return Control as ComboBox;
            }
        }

        public void Initialize()
        {
            this.Size = new Size(170, 30);
            FontCombo.Dock = DockStyle.Fill;
            FontCombo.IntegralHeight = true;
            FontCombo.DropDownStyle = ComboBoxStyle.DropDownList;
            FontCombo.BackColor = Color.White;
            FontDataItems.Clear();
            FontCombo.DrawMode = DrawMode.OwnerDrawVariable;
            FontCombo.DropDownHeight = 400;
            FontCombo.MeasureItem += new MeasureItemEventHandler(FontCombo_MeasureItem);
            FontCombo.DrawItem += new DrawItemEventHandler(FontCombo_DrawItem);
            foreach (CFontFile.FontData fd in CFontFile.AvailableFonts)
            {
                FontDataItems.Add(new FontDropDownItem(fd));
                FontCombo.Items.Add(FontDataItems.Last());
            }
            FontCombo.DropDownWidth = 300;
            FontCombo.DisplayMember = String.Empty;
        }

        public String FamilyName
        {
            get
            {
                if (FontCombo.SelectedIndex < 0)
                    return String.Empty;
                return FontDataItems[FontCombo.SelectedIndex].FamilyName;
            }
            set
            {
                FontCombo.Text = value;
            }
        }

        public new String Text
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        private void FontCombo_MeasureItem(object sender, MeasureItemEventArgs e)
        {
            FontDropDownItem fd = FontDataItems[e.Index];
            e.ItemHeight = fd.FontData.szFontLabel.Height + 4;
            e.ItemWidth = fd.FontData.szFontLabel.Width + 3;
        }

        private void FontCombo_DrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            FontDropDownItem fd = FontDataItems[e.Index];
            if ((FontCombo.DroppedDown == false) || (e.State == DrawItemState.ComboBoxEdit))
            {
                double imgAr = fd.FontData.FontImage.Width / fd.FontData.FontImage.Height;
                double arBounds = e.Bounds.Width / e.Bounds.Height;
                e.Graphics.SmoothingMode = SmoothingMode.HighQuality;
                e.Graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                e.Graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;
                if (imgAr < arBounds)
                    e.Graphics.DrawImage(fd.FontData.FontImage, new RectangleF(e.Bounds.X, e.Bounds.Y, (float)((double)e.Bounds.Height / (double)fd.FontData.FontImage.Height) * fd.FontData.FontImage.Width, e.Bounds.Height));
                else
                    e.Graphics.DrawImage(fd.FontData.FontImage, new RectangleF(e.Bounds.X, e.Bounds.Y, e.Bounds.Width, (float)((double)e.Bounds.Width / (double)fd.FontData.FontImage.Width) * fd.FontData.FontImage.Height));
            }
            else
                e.Graphics.DrawImage(fd.FontData.FontImage, new Point(e.Bounds.X + 3, e.Bounds.Y + 2));
        }
    }
}
