using System;
using System.Collections.Generic;
using System.Linq;
using System.Drawing;
using System.IO;
using System.Drawing.Text;
using System.Runtime.InteropServices;

namespace IATClient
{
    class PrivateFont
    {
        private static PrivateFontCollection PFC = new PrivateFontCollection();
        public static readonly PrivateFont Okashi= new PrivateFont("Okashi", "Okashi^", Properties.Resources.font1);
        public static readonly PrivateFont Amagro= new PrivateFont("Amagro", "Amagro bold", Properties.Resources.font2);
        public static readonly PrivateFont Disney = new PrivateFont("Disney", "New Walt Disney Font", Properties.Resources.font5);
        public static readonly PrivateFont Chiseled = new PrivateFont("Chiseled", "RACE1 Brannt Chiseled NCV", Properties.Resources.font4);
        public static readonly PrivateFont Kaushan = new PrivateFont("Kaushan", "Kaushan Script", Properties.Resources.font6);
        public static readonly PrivateFont Goxaqo = new PrivateFont("Goxaqo", "Goxaqo Personal Use", Properties.Resources.font3);
        public static readonly PrivateFont JosefinSans = new PrivateFont("Josefin Sans", "Josefin Sans", Properties.Resources.font7);
        public static readonly PrivateFont Lora = new PrivateFont("Lora", "Lora", Properties.Resources.font8);
        public static readonly PrivateFont Typewriter = new PrivateFont("Typewriter", "Another Typewriter", Properties.Resources.font9);
        public static readonly PrivateFont Bree = new PrivateFont("Lobster", "Lobster Two", Properties.Resources.font10);
        public String FamilyName { get; private set;  }
        public String DisplayName { get; private set; }
        public FontFamily FontFamily { get; private set; }

        public int GetFontHeight(double pts)
        {
            double designHeight = FontFamily.GetCellAscent(FontStyle.Regular) + FontFamily.GetCellDescent(FontStyle.Regular);
            double emHeight = FontFamily.GetEmHeight(FontStyle.Regular);
            double designUnit = emHeight / pts;
            return (int)(designHeight / designUnit);
        }

        public static List<PrivateFont> Fonts { get; private set; }



        [DllImport("gdi32.dll")]
        private static extern IntPtr AddFontMemResourceEx(IntPtr pbFont, uint cbFont, IntPtr pdv, [In] ref uint pcFonts);
        private PrivateFont(String displayName, String famName, String fontData)
        {
            var memStream = new MemoryStream(Convert.FromBase64String(fontData));
            IntPtr data = Marshal.AllocCoTaskMem((int)memStream.Length);
            Marshal.Copy(memStream.ToArray(), 0, data, (int)memStream.Length);
            uint cFonts = 0;
            AddFontMemResourceEx(data, (uint)memStream.Length, IntPtr.Zero, ref cFonts);
            PFC.AddMemoryFont(data, (int)memStream.Length);
            Marshal.FreeCoTaskMem(data);
            memStream.Dispose();
            DisplayName = displayName;
            if (Fonts == null)
                Fonts = new List<PrivateFont>();
            Fonts.Add(this);
            Fonts.Sort((x, y) => String.Compare(x.DisplayName, y.DisplayName));
            FamilyName = famName;
            FontFamily = PFC.Families.Where(f => f.Name == FamilyName).FirstOrDefault();
        }


    }
}
