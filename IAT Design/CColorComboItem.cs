using System;
using System.Collections.Generic;

using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class CColorComboItem
    {
        private Image ColorRectImage;
        private System.Drawing.Color _ItemColor;
        private Font NameFont;
        private Size ColorRectSize;
        private System.Drawing.Color _NameColor;

        static System.Drawing.Color[] Colors = {System.Drawing.Color.Azure, System.Drawing.Color.Beige, System.Drawing.Color.Black, System.Drawing.Color.Blue, System.Drawing.Color.BlueViolet, System.Drawing.Color.Brown,
                                        System.Drawing.Color.Chartreuse, System.Drawing.Color.Chocolate, System.Drawing.Color.CornflowerBlue, System.Drawing.Color.Crimson,
                                        System.Drawing.Color.Cyan, System.Drawing.Color.ForestGreen, System.Drawing.Color.Fuchsia, System.Drawing.Color.Gold, System.Drawing.Color.Gray,
                                        System.Drawing.Color.Green, System.Drawing.Color.GreenYellow, System.Drawing.Color.HotPink, System.Drawing.Color.Indigo, System.Drawing.Color.Ivory,
                                        System.Drawing.Color.Lavender, System.Drawing.Color.LightYellow, System.Drawing.Color.LimeGreen, System.Drawing.Color.Navy, System.Drawing.Color.Orange, System.Drawing.Color.OrangeRed,
                                        System.Drawing.Color.Pink, System.Drawing.Color.PowderBlue, System.Drawing.Color.Purple, System.Drawing.Color.Red, System.Drawing.Color.SeaGreen,
                                        System.Drawing.Color.Silver, System.Drawing.Color.White, System.Drawing.Color.Yellow };

        public System.Drawing.Color ItemColor
        {
            get 
            {
                return _ItemColor;
            }
        }

        public System.Drawing.Color NameColor
        {
            get
            {
                return _NameColor;
            }
            set
            {
                _NameColor = value;
            }
        }

        protected CColorComboItem(System.Drawing.Color c, Font f, Size szRect)
        {
            _ItemColor = c;
            ColorRectSize = szRect;
            ColorRectImage = new Bitmap(ColorRectSize.Width, ColorRectSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(ColorRectImage);
            g.DrawRectangle(Pens.Black, new Rectangle(0, 0, ColorRectSize.Width - 1, ColorRectSize.Height - 1));
            Brush br = new SolidBrush(c);
            g.FillRectangle(br, new Rectangle(1, 1, ColorRectSize.Width - 2, ColorRectSize.Height - 2));
            g.Dispose();
            NameFont = f;
            NameColor = System.Drawing.SystemColors.ControlText;
        }

        public void Draw(Graphics g, Brush backBrush, Rectangle bounds)
        {
            Point ptColorRect = bounds.Location;
            Point ptColorName = bounds.Location + new Size(ColorRectSize.Width + (NameFont.Height >> 1), 0);
            if (ColorRectSize.Height < NameFont.Height)
                ptColorRect.Y += (NameFont.Height - ColorRectSize.Height) >> 1;
            else
                ptColorName.Y += (ColorRectSize.Height - NameFont.Height) >> 1;

            g.FillRectangle(backBrush, bounds);
            g.DrawImage(ColorRectImage, ptColorRect);
            Brush textBrush = new SolidBrush(NameColor);
            g.DrawString(ItemColor.Name, NameFont, textBrush, ptColorName);
            textBrush.Dispose();
        }

        public Size Measure()
        {
            Size szText = TextRenderer.MeasureText(ItemColor.Name, NameFont);
            Size sz = new Size(ColorRectSize.Width + (NameFont.Height >> 1) + szText.Width, (ColorRectSize.Height > NameFont.Height) ? ColorRectSize.Height : NameFont.Height);
            return sz;
        }

        public static CColorComboItem[] GenerateColorComboItems(Font LabelFont, Size ColorRectSize)
        {
            CColorComboItem[] result = new CColorComboItem[Colors.Length];
            for (int ctr = 0; ctr < Colors.Length; ctr++)
                result[ctr] = new CColorComboItem(Colors[ctr], LabelFont, ColorRectSize);
            return result;
        }
    }
}
