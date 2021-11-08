using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace IATClient
{
    class CCapsuleBackground
    {
        private Color FaceColor, LightFaceColor, DarkFaceColor;
        private static Color TransColor = Color.Transparent;
        private Size _CapsuleSize;
        private const float HeightRounded = .1F;
        private const float WidthRounded = .1F;
        private const float BorderRatio = .05F;

        public Size CapsuleSize
        {
            get
            {
                return _CapsuleSize;
            }
        }

        public int VertBorderWidth
        {
            get
            {
                return (int)(BorderRatio * CapsuleSize.Height);
            }
        }

        public int HorizBorderWidth
        {
            get
            {
                return (int)(BorderRatio * CapsuleSize.Width);
            }
        }


        public Rectangle FaceRect
        {
            get
            {
                Rectangle r = new Rectangle((int)(CapsuleSize.Width * BorderRatio), (int)(CapsuleSize.Height * BorderRatio), (int)(CapsuleSize.Width - 2 * BorderRatio), (int)(CapsuleSize.Height - 2 * BorderRatio));
                r.Inflate(-(int)(CapsuleSize.Width * WidthRounded), -(int)(CapsuleSize.Height * HeightRounded));
                return r;
            }
        }

        private void DrawTopBorder(Graphics g)
        {
            CColorGradient grad = new CColorGradient(DarkFaceColor, FaceColor, FaceRect.Top);
            Pen p;
            Brush br;
            int y = 0;
            while (y < FaceRect.Top)
            {
                br = new SolidBrush(grad.CurrentColor);
                p = new Pen(br);
                g.DrawLine(p, new Point(FaceRect.Left, y), new Point(FaceRect.Right, y));
                p.Dispose();
                br.Dispose();
                y++;
                grad.NextGrad();
            }
        }

        private void DrawBottomBorder(Graphics g)
        {
            CColorGradient grad = new CColorGradient(DarkFaceColor, FaceColor, CapsuleSize.Height - FaceRect.Bottom);
            Pen p;
            Brush br;
            int y = CapsuleSize.Height - 1;
            while (y > FaceRect.Bottom)
            {
                br = new SolidBrush(grad.CurrentColor);
                p = new Pen(br);
                g.DrawLine(p, new Point(FaceRect.Left, y), new Point(FaceRect.Right, y));
                p.Dispose();
                br.Dispose();
                y--;
                grad.NextGrad();
            }
        }

        private void DrawLeftBorder(Graphics g)
        {
            CColorGradient grad = new CColorGradient(DarkFaceColor, FaceColor, FaceRect.Left);
            Pen p;
            Brush br;
            int x = 0;
            while (x < FaceRect.Left)
            {
                br = new SolidBrush(grad.CurrentColor);
                p = new Pen(br);
                g.DrawLine(p, new Point(x, FaceRect.Top), new Point(x, FaceRect.Bottom));
                p.Dispose();
                br.Dispose();
                x++;
                grad.NextGrad();
            }
        }

        private void DrawRightBorder(Graphics g)
        {
            CColorGradient grad = new CColorGradient(DarkFaceColor, FaceColor, CapsuleSize.Width - FaceRect.Right);
            Pen p;
            Brush br;
            int x = CapsuleSize.Width - 1;
            while (x > FaceRect.Right)
            {
                br = new SolidBrush(grad.CurrentColor);
                p = new Pen(br);
                g.DrawLine(p, new Point(x, FaceRect.Top), new Point(x, FaceRect.Bottom));
                p.Dispose();
                br.Dispose();
                x--;
                grad.NextGrad();
            }
        }

        private void DrawTopLeftCorner(Graphics g)
        {

        }



        public CCapsuleBackground(Size sz, Color face, Color light, Color dark)
        {
            FaceColor = face;
            LightFaceColor = light;
            DarkFaceColor = dark;
            _CapsuleSize = sz;
        }

        public Image GetBackgroundImage()
        {
            Bitmap img = new Bitmap(CapsuleSize.Width, CapsuleSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(img);
            Brush TransBrush = new SolidBrush(TransColor);
            g.FillRectangle(TransBrush, new Rectangle(0, 0, CapsuleSize.Width, CapsuleSize.Height));

        }
    }
}
