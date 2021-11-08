using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    public class ColorPickerEventArgs {
        private Color _Color;
        public ColorPickerEventArgs(Color c) {
            _Color = c;
        }
        public Color Color {
            get {
                return _Color;
            }
        }
    }



    public class ColorPicker : UserControl
    {
        public class ColorSelection : Enumeration
        {
            private static readonly List<ColorSelection> _Colors = new List<ColorSelection>(new ColorSelection[]
            {
                new ColorSelection(6, "Living Coral", Color.FromArgb(255, 111, 97)),
                new ColorSelection(1, "Storm Gray", Color.FromArgb(181, 186, 182)),
                new ColorSelection(2, "Forest Biome", Color.FromArgb(52, 86, 84)),
                new ColorSelection(3, "Martini Olive", Color.FromArgb(113, 106, 77)),
                new ColorSelection(4, "Golden Lime", Color.FromArgb(154, 151, 56)),
                new ColorSelection(5, "Mauvewood", Color.FromArgb(167, 93, 103)),
                new ColorSelection(6, "Twill", Color.FromArgb(167, 155, 130)),
                new ColorSelection(7, "Sulphur Spring", Color.FromArgb(213, 215, 23)),
                new ColorSelection(8, "Chive Blossom", Color.FromArgb(125, 93, 153)),
                new ColorSelection(9, "Vivacious", Color.FromArgb(163, 40, 87)),
                new ColorSelection(10, "Barrier Reef", Color.FromArgb(0, 132, 161)),
                new ColorSelection(11, "Deep Lake", Color.FromArgb(0, 101, 107)),
                new ColorSelection(12, "Ibiza Blue", Color.FromArgb(0, 124, 183)),
                new ColorSelection(13, "Pink Lemonade", Color.FromArgb(238, 109, 138)),
                new ColorSelection(14, "Blue Depths", Color.FromArgb(38, 48, 86)),
                new ColorSelection(15, "Turkish Sea", Color.FromArgb(25, 81, 144)),
                new ColorSelection(16, "Viridian Green", Color.FromArgb(0, 148, 153)),
                new ColorSelection(17, "Turtle Green", Color.FromArgb(129, 137, 78)),
                new ColorSelection(18, "Sea Pink", Color.FromArgb(222, 152, 171)),
                new ColorSelection(19, "Vibrant Yellow", Color.FromArgb(255, 218, 41)),
                new ColorSelection(20, "Limpet Shell", Color.FromArgb(152, 221, 222))
            });

            static ColorSelection()
            {

            }
                
            private ColorSelection(int ndx, String name, Color color) : base(ndx, name)
            {
                this.Color = color;
            }
            public Color Color { get; private set; }
            public static IReadOnlyList<ColorSelection> Colors
            {
                get
                {
                    return _Colors.AsReadOnly();
                }
            }
        }


        private List<Color> ColorList = new List<Color>();
        private static Size ButtonSize = new Size(30, 30);
        public Color SelectedColor { get; set; }
        private List<Button> ColorButtons = new List<Button>();
        private bool bCollapsed = true;
        public event EventHandler<ColorPickerEventArgs> SelectionMade = null;
        public Size ExpandedSize { get; set; }

        public ColorPicker(Color initialColor)
        {
/*            ColorList.AddRange(new Color []{ Color.AliceBlue,  Color.AntiqueWhite,  Color.Aqua,  Color.Aquamarine,  Color.Azure,  Color.Beige,  Color.Bisque,  Color.Black,  Color.BlanchedAlmond,  
                Color.Blue,  Color.BlueViolet,  Color.Brown,  Color.BurlyWood,  Color.CadetBlue,  Color.Chartreuse,  Color.Chocolate,  Color.Coral,  Color.CornflowerBlue,  Color.Cornsilk,  
                Color.Crimson,  Color.Cyan,  Color.DarkBlue,  Color.DarkCyan,  Color.DarkGoldenrod,  Color.DarkGray,  Color.DarkGreen,  Color.DarkKhaki,  Color.DarkMagenta,  Color.DarkOliveGreen,  
                Color.DarkOrange,  Color.DarkOrchid,  Color.DarkRed,  Color.DarkSalmon,  Color.DarkSeaGreen,  Color.DarkSlateBlue,  Color.DarkSlateGray,  Color.DarkTurquoise,  Color.DarkViolet,  
                Color.DeepPink,  Color.DeepSkyBlue,  Color.DimGray,  Color.DodgerBlue,  Color.Firebrick,  Color.FloralWhite,  Color.ForestGreen,  Color.Fuchsia,  Color.Gainsboro,  Color.GhostWhite,  
                Color.Gold,  Color.Goldenrod,  Color.Gray,  Color.Green,  Color.GreenYellow,  Color.Honeydew,  Color.HotPink,  Color.IndianRed,  Color.Indigo,  Color.Ivory,  Color.Khaki,  
                Color.Lavender,  Color.LavenderBlush,  Color.LawnGreen,  Color.LemonChiffon,  Color.LightBlue,  Color.LightCoral,  Color.LightCyan,  Color.LightGoldenrodYellow,  Color.LightGray,  
                Color.LightGreen,  Color.LightSalmon,  Color.LightSeaGreen,  Color.LightSkyBlue,  Color.LightSlateGray,  Color.LightSteelBlue,  Color.LightYellow,  Color.Lime,  Color.LimeGreen,  
                Color.Linen,  Color.Magenta,  Color.Maroon,  Color.MediumAquamarine,  Color.MediumBlue,  Color.MediumOrchid,  Color.MediumPurple,  Color.MediumSeaGreen,  Color.MediumSlateBlue,  
                Color.MediumSpringGreen,  Color.MediumTurquoise,  Color.MediumVioletRed,  Color.MidnightBlue,  Color.MintCream,  Color.MistyRose,  Color.Moccasin,  Color.NavajoWhite,  Color.Navy,  
                Color.OldLace,  Color.Olive,  Color.OliveDrab,  Color.Orange,  Color.OrangeRed,  Color.Orchid,  Color.PaleGoldenrod,  Color.PaleGreen,  Color.PaleTurquoise,  Color.PaleVioletRed,  
                Color.PapayaWhip,  Color.PeachPuff,  Color.Peru,  Color.Pink,  Color.Plum,  Color.PowderBlue,  Color.Purple,  Color.Red,  Color.RosyBrown,  Color.RoyalBlue,  Color.SaddleBrown,  
                Color.Salmon,  Color.SandyBrown,  Color.SeaGreen,  Color.SeaShell,  Color.Sienna,  Color.Silver,  Color.SkyBlue,  Color.SlateBlue,  Color.SlateGray,  Color.Snow,  Color.SpringGreen,  
                Color.SteelBlue,  Color.Tan,  Color.Teal,  Color.Thistle,  Color.Tomato,  Color.Turquoise,  Color.Violet,  Color.Wheat,  Color.White,  Color.WhiteSmoke,  Color.Yellow,  Color.YellowGreen });
 */
            int ndx = 0;
            int width = 0, height = 0;
            foreach (ColorSelection cs in ColorSelection.Colors)
            {
                RadioButton rb = new RadioButton() {
                    Appearance = Appearance.Button,
                    Size = ButtonSize,
                    BackColor = cs.Color
                };
                Label l = new Label()
                {
                    Text = cs.Name
                };
                rb.Click += (s, e) => { if (rb.Checked) SelectedColor = rb.BackColor; SelectionMade?.Invoke(this, new ColorPickerEventArgs(rb.BackColor)); };
                rb.Location = new Point(0, ndx * ButtonSize.Height);
                this.Controls.Add(rb);
                l.Size = TextRenderer.MeasureText(l.Text, l.Font);
                l.Location = new Point(rb.Right + 3, ndx++ * ButtonSize.Height + ((rb.Height - l.Height) >> 1));
                this.Controls.Add(l);
                width = (l.Right > width) ? l.Right : width;
                height += ButtonSize.Height;
            }
            this.Size = new Size(width, height);

            /*(
            int nCols = (int)Math.Ceiling((double)controlSize.Width / ButtonSize.Width);
            int col = 0, row = 0;
            foreach (Button b in ColorButtons)
            {
                b.Location = new Point(col * ButtonSize.Width, row * ButtonSize.Height);
                if (++col >= nCols)
                {
                    col = 0;
                    row++;
                }
                this.Controls.Add(b);
            }
            this.BackColor = initialColor;
            this.Size = new Size(controlSize.Width, row * ButtonSize.Height);
            */
        }
    }


}
