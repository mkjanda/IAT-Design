using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using IATClient.Images;

namespace IATClient
{
    public class DIErrorMark : DIGenerated
    {
        private readonly String Mark = "X";
        private readonly FontFamily MarkFontFamily = FontFamily.GenericSansSerif;
        private readonly Color MarkColor = Color.Red;
        public DIErrorMark()
        {
            ScheduleInvalidation();
        }

        public DIErrorMark(Uri uri) : base(uri)
        {
            ResumeLayout(false);
            ScheduleInvalidation();
        }

        public override IUri IUri
        {
            get
            {
                return CIATLayout.IErrorMarkUri;
            }
        }
        /*
        private Rectangle AbsoluteClip(Bitmap bmp)
        {
            Bitmap clone = bmp.Clone() as Bitmap;
            System.Drawing.Imaging.BitmapData bmpData = clone.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr dataPtr = bmpData.Scan0;
            Int32[] data = new Int32[bmpData.Stride * bmp.Height / 4];
            System.Runtime.InteropServices.Marshal.Copy(dataPtr, data, 0, bmpData.Stride * bmp.Height / 4);
            Rectangle absoluteBounds = new Rectangle();
            int y = 0, x = 0;
            bool bClear = false;
            while (data[y * bmpData.Stride / 4 + x++] != MarkColor.ToArgb())
            {
                if (x >= bmpData.Stride / 4) {
                    y++;
                    x = 0;
                }
                if (y >= bmp.Height)
                {
                    bClear = true;
                    break;
                }
            }
            if (bClear)
                return new Rectangle(0, 0, 1, 1);
            absoluteBounds.Y = y;
            y = bmp.Height - 1; x = 0;
            while (data[y * bmpData.Stride / 4 + x++] != MarkColor.ToArgb()) {
                if (x >= bmpData.Stride / 4)
                {
                    y--;
                    x = 0;
                }
            }
            absoluteBounds.Height = y - absoluteBounds.Y;
            y = 0; x = 0;
            while (data[y++ * bmpData.Stride / 4 + x] != MarkColor.ToArgb()) {
                if (y >= bmp.Height)
                {
                    y = 0;
                    x++;
                }
            }
            absoluteBounds.X = x;
            y = 0; x = bmpData.Stride / 4 - 1;
            while (data[y++ * bmpData.Stride / 4 + x] != MarkColor.ToArgb())
            {
                if (y >= bmp.Height)
                {
                    y = 0;
                    x--;
                }
            }
            absoluteBounds.Width = x - absoluteBounds.X;
            clone.UnlockBits(bmpData);
            clone.Dispose();
            return absoluteBounds;
        }
        */
        protected override bool Generate()
        {
            if (Broken || IsDisposed)
                return true;
            Size szBounds;
            szBounds = DIType.ErrorMark.GetBoundingSize();
            float fontSize = 36F;
            Brush br = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            using (Bitmap bmp = new Bitmap(szBounds.Width + 50, szBounds.Height + 50, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
                using (Graphics g = Graphics.FromImage(bmp))
                {
                    using (Brush markBrush = new SolidBrush(MarkColor))
                    {
                        do
                        {
                            try
                            {
                                g.FillRectangle(br, new Rectangle(0, 0, szBounds.Width + 50, szBounds.Height + 50));
                                using (Font markFont = new Font(MarkFontFamily, fontSize))
                                    g.DrawString(Mark, markFont, markBrush, new PointF(0, 0));
                                CalcAbsoluteBounds(bmp);
                                fontSize += 5F;
                            }
                            catch (Exception ignoring)
                            {
                            }
                        } while ((AbsoluteBounds.Width <= szBounds.Width) && (AbsoluteBounds.Height <= szBounds.Height));
                        do
                        {
                            try
                            {
                                g.FillRectangle(br, new Rectangle(0, 0, szBounds.Width + 50, szBounds.Height + 50));
                                using (Font markFont = new Font(MarkFontFamily, fontSize))
                                    g.DrawString(Mark, markFont, markBrush, new PointF(0, 0));
                                CalcAbsoluteBounds(bmp);
                                fontSize -= .5F;
                            }
                            catch (Exception ignoring)
                            {
                            }
                        } while ((AbsoluteBounds.Width >= szBounds.Width) || (AbsoluteBounds.Height >= szBounds.Height));
                    }
                }
                Bitmap errorBmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.ErrorMark);
                using (Graphics g = Graphics.FromImage(errorBmp))
                {
                    g.FillRectangle(br, 0, 0, errorBmp.Width, errorBmp.Height);
                    g.DrawImage(bmp, new Rectangle(0, 0, DIType.ErrorMark.GetBoundingSize().Width, DIType.ErrorMark.GetBoundingSize().Height), AbsoluteBounds, GraphicsUnit.Pixel);
                }
                errorBmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                AbsoluteBounds = new Rectangle(new Point(0, 0), Images.ImageMediaType.ErrorMark.ImageSize);
                SetImage(errorBmp, System.Drawing.Imaging.ImageFormat.Png);
            }
            br.Dispose();
            return true;
        }

        public override object Clone()
        {
            DIErrorMark o = new DIErrorMark();
            return o;
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            String ImageRelId = (this.IImage != null) ? CIAT.SaveFile.GetRelationshipsByType(this.URI, BaseType, this.IImage.BaseType).First().Id : String.Empty;
            xDoc.Document.Add(new XElement(BaseType.ToString(), new XAttribute("rId", ImageRelId)));
            xDoc.Root.Add(new XElement("Size", new XElement("Width", ItemSize.Width.ToString()), new XElement("Height", ItemSize.Height.ToString())));
            using (Stream s = CIAT.SaveFile.GetWriteStream(this))
            {
                xDoc.Save(s);
            }
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            String ImageRelId = xDoc.Root.Attribute("rId").Value;
            if (ImageRelId == String.Empty)
                Generate();
            else
            {
                try
                {
                    SetImage(CIAT.SaveFile.GetRelationship(this, ImageRelId).TargetUri);
                }
                catch (Exception ignored) { }
            }
            XElement szElem = xDoc.Root.Element("Size");
            ItemSize = new Size(Convert.ToInt32(szElem.Element("Width").Value), Convert.ToInt32(szElem.Element("Height").Value));
        }
    }
}
