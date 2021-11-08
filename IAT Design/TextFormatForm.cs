using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public partial class TextFormatForm : Form
    {
        private Dictionary<String, Color> ColorMap = new Dictionary<String, Color>();
/*

        private void InitColorDrop()
        {
            ColorMap["Alice Blue"] = Color.AliceBlue;
            ColorMap["Antique White"] = Color.AntiqueWhite;
            ColorMap["Aqua"] = Color.Aqua;
            ColorMap["Aquamarine"] = Color.Aquamarine;
            ColorMap["Azure"] = Color.Azure;
            ColorMap["Beige"] = Color.Beige;
            ColorMap["Bisque"] = Color.Bisque;
            ColorMap["Black"] = Color.Black;
            ColorMap["Blanched Almond"] = Color.BlanchedAlmond;
            ColorMap["Blue"] = Color.Blue;
            ColorMap["Blue Violet"] = Color.BlueViolet;
            ColorMap["Brown"] = Color.Brown;
            ColorMap["Burly Wood"] = Color.BurlyWood;
            ColorMap["Cadet Blue"] = Color.CadetBlue;
            ColorMap["Chartreuse"] = Color.Chartreuse;
            ColorMap["Chocolate"] = Color.Chocolate;
            ColorMap["Coral"] = Color.Coral;
            ColorMap["Cornflower Blue"] = Color.CornflowerBlue;
            ColorMap["Cornsilk"] = Color.Cornsilk;
            ColorMap["Crimson"] = Color.Crimson;
            ColorMap["Cyan"] = Color.Cyan;
            ColorMap["Dark Blue"] = Color.DarkBlue;
            ColorMap["Dark Cyan"] = Color.DarkCyan;
            ColorMap["Dark Goldenrod"] = Color.DarkGoldenrod;
            ColorMap["Dark Gray"] = Color.DarkGray;
            ColorMap["Dark Green"] = Color.DarkGreen;
            ColorMap["Dark Khaki"] = Color.DarkKhaki;
            ColorMap["Dark Magenta"] = Color.DarkMagenta;
            ColorMap["Dark Olive Green"] = Color.DarkOliveGreen;
            ColorMap["Dark Orange"] = Color.DarkOrange;
            ColorMap["Dark Orchid"] = Color.DarkOrchid;
            ColorMap["Dark Red"] = Color.DarkRed;
            ColorMap["Dark Salmon"] = Color.DarkSalmon;
            ColorMap["Dark Sea Green"] = Color.DarkSeaGreen;
            ColorMap["Dark Slate Blue"] = Color.DarkSlateBlue;
            ColorMap["Dark Slate Gray"] = Color.DarkSlateGray;
            ColorMap["Dark Turquoise"] = Color.DarkTurquoise;
            ColorMap["Dark Violet"] = Color.DarkViolet;
            ColorMap["Deep Pink"] = Color.DeepPink;
            ColorMap["Deep Sky Blue"] = Color.DeepSkyBlue;
            ColorMap["Dim Gray"] = Color.DimGray;
            ColorMap["Dodger Blue"] = Color.DodgerBlue;
            ColorMap["Firebrick"] = Color.Firebrick;
            ColorMap["Floral White"] = Color.FloralWhite;
            ColorMap["Forest Green"] = Color.ForestGreen;
            ColorMap["Fuchsia"] = Color.Fuchsia;
            ColorMap["Gainsboro"] = Color.Gainsboro;
            ColorMap["Ghost White"] = Color.GhostWhite;
            ColorMap["Gold"] = Color.Gold;
            ColorMap["Goldenrod"] = Color.Goldenrod;
            ColorMap["Gray"] = Color.Gray;
            ColorMap["Green"] = Color.Green;
            ColorMap["Green Yellow"] = Color.GreenYellow;
            ColorMap["Honeydew"] = Color.Honeydew;
            ColorMap["Hot Pink"] = Color.HotPink;
            ColorMap["Indian Red"] = Color.IndianRed;
            ColorMap["Indigo"] = Color.Indigo;
            ColorMap["Ivory"] = Color.Ivory;
            ColorMap["Khaki"] = Color.Khaki;
            ColorMap["Lavender"] = Color.Lavender;
            ColorMap["Lavender Blush"] = Color.LavenderBlush;
            ColorMap["Lawn Green"] = Color.LawnGreen;
            ColorMap["Lemon Chiffon"] = Color.LemonChiffon;
            ColorMap["Light Blue"] = Color.LightBlue;
            ColorMap["Light Coral"] = Color.LightCoral;
            ColorMap["Light Cyan"] = Color.LightCyan;
            ColorMap["Light Goldenrod Yellow"] = Color.LightGoldenrodYellow;
            ColorMap["Light Gray"] = Color.LightGray;
            ColorMap["Light Green"] = Color.LightGreen;
            ColorMap["Light Salmon"] = Color.LightSalmon;
            ColorMap["Light Sea Green"] = Color.LightSeaGreen;
            ColorMap["Light Sky Blue"] = Color.LightSkyBlue;
            ColorMap["Light Slate Gray"] = Color.LightSlateGray;
            ColorMap["Light Steel Blue"] = Color.LightSteelBlue;
            ColorMap["Light Yellow"] = Color.LightYellow;
            ColorMap["Lime"] = Color.Lime;
            ColorMap["Lime Green"] = Color.LimeGreen;
            ColorMap["Linen"] = Color.Linen;
            ColorMap["Magenta"] = Color.Magenta;
            ColorMap["Maroon"] = Color.Maroon;
            ColorMap["Medium Aquamarine"] = Color.MediumAquamarine;
            ColorMap["Medium Blue"] = Color.MediumBlue;
            ColorMap["Medium Orchid"] = Color.MediumOrchid;
            ColorMap["Medium Purple"] = Color.MediumPurple;
            ColorMap["Medium Sea Green"] = Color.MediumSeaGreen;
            ColorMap["Medium Slate Blue"] = Color.MediumSlateBlue;
            ColorMap["Medium Spring Green"] = Color.MediumSpringGreen;
            ColorMap["Medium Turquoise"] = Color.MediumTurquoise;
            ColorMap["Medium Violet Red"] = Color.MediumVioletRed;
            ColorMap["Midnight Blue"] = Color.MidnightBlue;
            ColorMap["Mint Cream"] = Color.MintCream;
            ColorMap["Misty Rose"] = Color.MistyRose;
            ColorMap["Moccassin"] = Color.Moccasin;
            ColorMap["Navajo White"] = Color.NavajoWhite;
            ColorMap["Navy"] = Color.Navy;
            ColorMap["Old Lace"] = Color.OldLace;
            ColorMap["Olive"] = Color.Olive;
            ColorMap["Olive Drab"] = Color.OliveDrab;
            ColorMap["Orange"] = Color.Orange;
            ColorMap["Orange Red"] = Color.OrangeRed;
            ColorMap["Orchid"] = Color.Orchid;
            ColorMap["Pale Goldenrod"] = Color.PaleGoldenrod;
            ColorMap["Pale Green"] = Color.PaleGreen;
            ColorMap["Pale Turquoise"] = Color.PaleTurquoise;
            ColorMap["Pale Violet Red"] = Color.PaleVioletRed;
            ColorMap["Papaya Whip"] = Color.PapayaWhip;
            ColorMap["Peach Puff"] = Color.PeachPuff;
            ColorMap["Peru"] = Color.Peru;
            ColorMap["Pink"] = Color.Pink;
            ColorMap["Plum"] = Color.Plum;
            ColorMap["Powder Blue"] = Color.PowderBlue;
            ColorMap["Purple"] = Color.Purple;
            ColorMap["Red"] = Color.Red;
            ColorMap["Rosy Brown"] = Color.RosyBrown;
            ColorMap["Royal Blue"] = Color.RoyalBlue;
            ColorMap["Saddle Brown"] = Color.SaddleBrown;
            ColorMap["Salmon"] = Color.Salmon;
            ColorMap["Sandy Brown"] = Color.SandyBrown;
            ColorMap["Sea Green"] = Color.SeaGreen;
            ColorMap["Sea Shell"] = Color.SeaShell;
            ColorMap["Sienna"] = Color.Sienna;
            ColorMap["Silver"] = Color.Silver;
            ColorMap["Sky Blue"] = Color.SkyBlue;
            ColorMap["Slate Blue"] = Color.SlateBlue;
            ColorMap["Slate Gray"] = Color.SlateGray;
            ColorMap["Snow"] = Color.Snow;
            ColorMap["Spring Green"] = Color.SpringGreen;
            ColorMap["Steel Blue"] = Color.SteelBlue;
            ColorMap["Tan"] = Color.Tan;
            ColorMap["Teal"] = Color.Teal;
            ColorMap["Thistle"] = Color.Thistle;
            ColorMap["Tomato"] = Color.Tomato;
            ColorMap["Turquoise"] = Color.Turquoise;
            ColorMap["Violet"] = Color.Violet;
            ColorMap["Wheat"] = Color.Wheat;
            ColorMap["White"] = Color.White;
            ColorMap["White Smoke"] = Color.WhiteSmoke;
            ColorMap["Yellow"] = Color.Yellow;
            ColorMap["Yellow Green"] = Color.YellowGreen;
            ColorCombo.Items.Clear();
            foreach (String str in ColorMap.Keys.OrderBy(x => x))
            {
                Bitmap bmp = new Bitmap(ColorCombo.Height, ColorCombo.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
                Graphics g = Graphics.FromImage(bmp);
                Brush br = new SolidBrush(ColorMap[str]);
                g.FillRectangle(br, new Rectangle(0, 0, bmp.Width, bmp.Height));
                g.DrawRectangle(Pens.Black, new Rectangle(0, 0, bmp.Width, bmp.Height));
                br.Dispose();
                g.Dispose();
                ColorCombo.Items.Add(str);
            }
        }
*/
        public TextFormatForm()
        {
            InitializeComponent();
            FontCombo.Items.AddRange(new object[] { "sans-serif", "serif" });
            FontSizeCombo.Items.AddRange(new object[] { "10", "12", "14", "16", "18", "20", "22", "24" });
//            ColorPicker cp = new ColorPicker();
 //           cp.SelectionMade += new EventHandler<ColorPickerEventArgs>((o, e) => {});

//            this.toolStrip1.Items.Add(new ToolStripControlHost(cp));
        }

        private void FontCombo_Click(object sender, EventArgs e)
        {

        }

       
    }
}
