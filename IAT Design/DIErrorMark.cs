using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
            IImage = new ImageManager.Image(Generate(), ImageFormat.Bmp, DIType.ErrorMark);
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
        protected override Bitmap Generate()
        {
            if (Broken || IsDisposed)
                return null;
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
                            fontSize += 5F;
                            try
                            {
                                g.FillRectangle(br, new Rectangle(0, 0, szBounds.Width + 50, szBounds.Height + 50));
                                using (Font markFont = new Font(MarkFontFamily, fontSize))
                                    g.DrawString(Mark, markFont, markBrush, new PointF(0, 0));
                                CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
                            }
                            catch (Exception ignoring)
                            {
                            }
                        } while ((AbsoluteBounds.Width <= szBounds.Width) && (AbsoluteBounds.Height <= szBounds.Height));
                        do
                        {
                            fontSize -= 4F;
                            try
                            {
                                g.FillRectangle(br, new Rectangle(0, 0, szBounds.Width + 50, szBounds.Height + 50));
                                using (Font markFont = new Font(MarkFontFamily, fontSize))
                                    g.DrawString(Mark, markFont, markBrush, new PointF(0, 0));
                                CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
                            }
                            catch (Exception ignoring)
                            {
                            }
                            if (fontSize < 0)
                                break;
                        } while ((AbsoluteBounds.Width >= szBounds.Width) || (AbsoluteBounds.Height >= szBounds.Height));
                        do
                        {
                            fontSize += 3F;
                            try
                            {
                                g.FillRectangle(br, new Rectangle(0, 0, szBounds.Width + 50, szBounds.Height + 50));
                                using (Font markFont = new Font(MarkFontFamily, fontSize))
                                    g.DrawString(Mark, markFont, markBrush, new PointF(0, 0));
                                CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
                            }
                            catch (Exception ignoring)
                            {
                            }
                        } while ((AbsoluteBounds.Width <= szBounds.Width) && (AbsoluteBounds.Height <= szBounds.Height));
                        do
                        {
                            fontSize -= 2F;
                            try
                            {
                                g.FillRectangle(br, new Rectangle(0, 0, szBounds.Width + 50, szBounds.Height + 50));
                                using (Font markFont = new Font(MarkFontFamily, fontSize))
                                    g.DrawString(Mark, markFont, markBrush, new PointF(0, 0));
                                CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
                            }
                            catch (Exception ignoring)
                            {
                            }
                            if (fontSize < 0)
                                break;
                        } while ((AbsoluteBounds.Width >= szBounds.Width) || (AbsoluteBounds.Height >= szBounds.Height));
                        do
                        {
                            try
                            {
                                g.FillRectangle(br, new Rectangle(0, 0, szBounds.Width + 50, szBounds.Height + 50));
                                using (Font markFont = new Font(MarkFontFamily, fontSize))
                                    g.DrawString(Mark, markFont, markBrush, new PointF(0, 0));
                                CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
                                fontSize += 1F;
                            }
                            catch (Exception ignoring)
                            {
                            }
                        } while ((AbsoluteBounds.Width <= szBounds.Width) && (AbsoluteBounds.Height <= szBounds.Height));
                        do
                        {
                            fontSize -= .5F;
                            try
                            {
                                g.FillRectangle(br, new Rectangle(0, 0, szBounds.Width + 50, szBounds.Height + 50));
                                using (Font markFont = new Font(MarkFontFamily, fontSize))
                                    g.DrawString(Mark, markFont, markBrush, new PointF(0, 0));
                                CalcAbsoluteBounds(bmp, CIAT.SaveFile.Layout.BackColor);
                            }
                            catch (Exception ignoring)
                            {
                            }
                            if (fontSize < 0)
                                break;
                        } while ((AbsoluteBounds.Width >= szBounds.Width) || (AbsoluteBounds.Height >= szBounds.Height));
                    }
                }
                Bitmap errorBmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.ErrorMark);
                using (Graphics g = Graphics.FromImage(errorBmp))
                {
                    g.FillRectangle(br, new Rectangle(new Point(0, 0), errorBmp.Size));
                    g.DrawImage(bmp, AbsoluteBounds.Location);
                }
                errorBmp.MakeTransparent(CIAT.SaveFile.Layout.BackColor);
                br.Dispose();
                return errorBmp;
            }
        }

        public override object Clone()
        {
            DIErrorMark o = new DIErrorMark();
            return o;
        }

        public override void Save()
        {
            XDocument xDoc = new XDocument();
            xDoc.Document.Add(new XElement(BaseType.ToString(), new XAttribute("rImageId", rImageId), new XAttribute("InvalidationState", InvalidationState.ToString())));
            xDoc.Root.Add(new XElement("AbsoluteBounds", new XElement("X", AbsoluteBounds.X.ToString()), new XElement("Y", AbsoluteBounds.Y.ToString()),
                new XElement("Width", AbsoluteBounds.Width.ToString()), new XElement("Height", AbsoluteBounds.Height.ToString())));
            Stream s = CIAT.SaveFile.GetWriteStream(this);
            xDoc.Save(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseWriteStreamLock();
        }

        protected override void DoLoad(Uri uri)
        {
            this.URI = uri;
            Stream s = CIAT.SaveFile.GetReadStream(this);
            XDocument xDoc = XDocument.Load(s);
            s.Dispose();
            CIAT.SaveFile.ReleaseReadStreamLock();
            rImageId = xDoc.Root.Attribute("rImageId").Value;
            SetImage(rImageId);
            var iStateAttr = xDoc.Element("InvalidationState");
            if (iStateAttr != null)
                InvalidationState = InvalidationStates.Parse(iStateAttr.Value);
            else
                InvalidationState = InvalidationStates.InvalidationReady;
            XElement absSize = xDoc.Root.Element("AbsoluteBounds");
            if (absSize == null)
            {
                ScheduleInvalidation();
                return;
            }
            else
            {
                int x = Convert.ToInt32(absSize.Element("X").Value);
                int y = Convert.ToInt32(absSize.Element("Y").Value);
                int width = Convert.ToInt32(absSize.Element("Width").Value);
                int height = Convert.ToInt32(absSize.Element("Height").Value);
                AbsoluteBounds = new Rectangle(x, y, width, height);
            }
        }
    }
}
