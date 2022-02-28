using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace IATClient
{
    class FontSelectCombo : ComboBox
    {
        public FontSelectCombo()
        {
            this.DropDownStyle = ComboBoxStyle.DropDownList;
            this.DrawMode = DrawMode.OwnerDrawVariable;
            this.MeasureItem += new MeasureItemEventHandler(MyMeasureItem);
            this.DrawItem += new DrawItemEventHandler(MyDrawItem);
            this.Items.AddRange(CFontFile.AvailableFonts);
        }

        private void MyMeasureItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            CFontFile.FontData fd = (CFontFile.FontData)this.Items[e.Index];
            e.ItemWidth = fd.szFontLabel.Width;
            e.ItemHeight = fd.szFontLabel.Height;

        }

        private void MyDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0)
                return;
            CFontFile.FontData fd = (CFontFile.FontData)this.Items[e.Index];
            e.Graphics.DrawImage(fd.FontImage, new PointF(e.Bounds.X, e.Bounds.Y));
        }

        public void SelectFontFamily(String fontFamily)
        {
            this.SelectedIndex = this.Items.IndexOf(CFontFile.AvailableFonts.First(fd => fd.FamilyName == fontFamily));
        }
    }
}
