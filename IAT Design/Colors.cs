using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace IATClient
{
    public class NamedColor
    {
        public readonly static NamedColor Black = new NamedColor()
        {
            Name = "Black",
            Color = System.Drawing.Color.Black
        };
        public readonly static NamedColor FlameScarlet = new NamedColor()
        {
            Name = "Flame Scarlet",
            Color = System.Drawing.Color.FromArgb(205, 33, 42)
        };
        public readonly static NamedColor Firefly = new NamedColor()
        {
            Name = "Firefly",
            Color = System.Drawing.Color.FromArgb(209, 206, 32)
        };
        public readonly static NamedColor SilverSconce = new NamedColor()
        {
            Name = "Silver Sconce",
            Color = System.Drawing.Color.FromArgb(161, 159, 165)
        };
        public readonly static NamedColor White = new NamedColor()
        {
            Name = "White",
            Color = System.Drawing.Color.White
        };
        public readonly static NamedColor UltraViolet = new NamedColor()
        {
            Name = "Ultra Violet",
            Color = System.Drawing.Color.FromArgb(95, 75, 139)
        };
        public readonly static NamedColor KnockoutPink = new NamedColor()
        {
            Name = "Knockout Pink",
            Color = System.Drawing.Color.FromArgb(255, 62, 165)
        };
        public readonly static NamedColor Emerald = new NamedColor()
        {
            Name = "Emerald",
            Color = System.Drawing.Color.FromArgb(0, 148, 115)
        };
        public readonly static NamedColor SunsetGold = new NamedColor()
        {
            Name = "Sunset Gold",
            Color = System.Drawing.Color.FromArgb(247, 196, 148)
        };
        public readonly static NamedColor RadiantOrchid = new NamedColor()
        {
            Name = "Radiant Orchid",
            Color = System.Drawing.Color.FromArgb(174, 93, 153)
        };
        public readonly static NamedColor AngelBlue = new NamedColor()
        {
            Name = "Angel Blue",
            Color = System.Drawing.Color.FromArgb(131, 198, 207)
        };
        public readonly static NamedColor AcidLime = new NamedColor()
        {
            Name = "Acid Lime",
            Color = System.Drawing.Color.FromArgb(187, 223, 50)
        };
        public readonly static NamedColor Bluebird = new NamedColor()
        {
            Name = "Bluebird",
            Color = System.Drawing.Color.FromArgb(0, 161, 180)
        };
        public readonly static NamedColor StarSapphire = new NamedColor()
        {
            Name = "Star Sapphire",
            Color = System.Drawing.Color.FromArgb(69, 104, 154)
        };
        public readonly static NamedColor EmberGlow = new NamedColor()
        {
            Name = "Ember Glow",
            Color = System.Drawing.Color.FromArgb(234, 103, 89)
        };
        public readonly static NamedColor ScarletIbis = new NamedColor()
        {
            Name = "Scarlet Ibis",
            Color = System.Drawing.Color.FromArgb(244, 85, 32)
        };
        public readonly static NamedColor Greenery = new NamedColor()
        {
            Name = "Greenery",
            Color = System.Drawing.Color.FromArgb(136, 176, 75)
        };
        public static readonly NamedColor CallisteGreen = new NamedColor()
        {
            Name = "Calliste Green",
            Color = Color.FromArgb(117, 122, 78)
        };
        public static readonly NamedColor BlackenedPearl = new NamedColor()
        {
            Name = "Blackened Pearl",
            Color = Color.FromArgb(77, 75, 80)
        };
        public static readonly NamedColor Citrus = new NamedColor()
        {
            Name = "Pale Gold",
            Color = Color.FromArgb(189, 152, 101)
        };
        public static readonly NamedColor Raspberry = new NamedColor()
        {
            Name = "Raspberry",
            Color = Color.FromArgb(211, 46, 94)
        };
        public static readonly NamedColor Oriole = new NamedColor()
        {
            Name = "Oriole",
            Color = Color.FromArgb(255, 121, 19)
        };
        public static readonly NamedColor Bamboo = new NamedColor() { Name = "Bamboo", Color = Color.FromArgb(210, 176, 76) };
        public static readonly NamedColor Butterum = new NamedColor() { Name = "Butterum", Color = Color.FromArgb(198, 143, 101) };

        public String Name { get; private set; }
        public Color Color { get; private set; }
        private static List<NamedColor> _All;
        private NamedColor()
        {
            if (_All == null)
                _All = new List<NamedColor>();
            _All.Add(this);
        }
        public String ToString()
        {
            return Name;
        }
        public static List<NamedColor> All { get { return _All.OrderBy(nc => nc.Name).ToList(); } }
        public static NamedColor GetNamedColor(System.Drawing.Color color)
        {
            var c = All.Where((c) => c.Color.ToArgb() == color.ToArgb()).FirstOrDefault();
            if (c == null)
                return NamedColor.White;
            return c;
        }
        public static NamedColor GetNamedColor(String name)
        {
            var c = All.Where((c) => c.Name == name).FirstOrDefault();
            if (c == null)
                return NamedColor.White;
            return c;
        }

    }
}
