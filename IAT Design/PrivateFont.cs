using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace IATClient
{
    class PrivateFont
    {
        private static PrivateFontCollection PFC = new PrivateFontCollection();
        public static readonly PrivateFont Georgia = new PrivateFont("Georgia", "Georgia", Properties.Resources.font1);
        public static readonly PrivateFont Calibri = new PrivateFont("Calibri", "Calibri", Properties.Resources.font2);
        public static readonly PrivateFont Forte = new PrivateFont("Forte", "Forte", Properties.Resources.font3);
        public static readonly PrivateFont MVBoli = new PrivateFont("MV Boli", "MV Boli", Properties.Resources.font4);
        public static readonly PrivateFont Pristina = new PrivateFont("Pristina", "Pristina", Properties.Resources.font5);
        public static readonly PrivateFont SegoePrint = new PrivateFont("Segoe Print", "Segoe Print", Properties.Resources.font6);
        public static readonly PrivateFont SegoeScript = new PrivateFont("Segoe Script", "Segoe Script", Properties.Resources.font7);
        public String FamilyName { get; private set; }
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
