using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    class CGridResultRow
    {
        private List<Rectangle> _CellRects = new List<Rectangle>();
        private List<String> _Results = new List<String>();
        private Rectangle _Dimensions = new Rectangle();
        private Padding _CellPadding, _RowPadding;

        public CGridResultRow(List<Rectangle> cellRects, List<String> results, Padding rowPadding, Padding cellPadding)
        {
            foreach (Rectangle r in cellRects)
            {
                r.Offset(new Point(rowPadding.Top, rowPadding.Left));
                _CellRects.Add(r);
            }
            _Results.AddRange(results);
            _RowPadding = rowPadding;
            _CellPadding = cellPadding;
        }


        public Size Size
        {
            get
            {
                return _Dimensions.Size;
            }
            set
            {
                _Dimensions.Size = value;
            }
        }

        public Point Location
        {
            get
            {
                return _Dimensions.Location;
            }
            set
            {
                _Dimensions.Location = value;
            }
        }

        public int Right
        {
            get
            {
                return _Dimensions.Right;
            }
        }

        public int Bottom
        {
            get
            {
                return _Dimensions.Bottom;
            }
        }

        public int Width
        {
            get
            {
                return _Dimensions.Width;
            }
        }

        public int Left
        {
            get
            {
                return _Dimensions.Left;
            }
        }

        public bool InClipRect(Rectangle clipRect)
        {
            if ((clipRect.Right <= _Dimensions.Left) || (clipRect.Left >= _Dimensions.Right) || (clipRect.Bottom <= _Dimensions.Top) || (clipRect.Top >= _Dimensions.Bottom))
                return false;
            return true;
        }

        public bool HitTest(Point pt)
        {
            if (_Dimensions.Contains(pt))
                return true;
            return false;
        }

        public void SetDimensions(Rectangle dimensions)
        {
            _Dimensions = new Rectangle(dimensions.Location, dimensions.Size);
        }

        public void Draw(Graphics g, Font resultFont)
        {
            for (int ctr = 0; ctr < _CellRects.Count; ctr++)
            {
                SizeF szText = g.MeasureString(_Results[ctr], resultFont, new SizeF(_CellRects[ctr].Width, _CellRects[ctr].Height), StringFormat.GenericDefault);
                String resultStr = _Results[ctr];
                SizeF lineSize = g.MeasureString(resultStr.Trim(), resultFont);
                int nLines = 0;
                while (lineSize.Width > _CellRects[ctr].Width)
                {
                    String str = resultStr.Substring(0, resultStr.Trim().LastIndexOf(' ')).Trim();
                    lineSize = g.MeasureString(str, resultFont);
                    while (lineSize.Width > _CellRects[ctr].Width)
                    {
                        str = str.Substring(0, str.LastIndexOf(' '));
                        lineSize = g.MeasureString(str, resultFont);
                    }
                    g.DrawString(str, resultFont, Brushes.Black, new PointF(_CellRects[ctr].Left + _RowPadding.Left + _CellPadding.Left, _Dimensions.Top + _CellRects[ctr].Top + _RowPadding.Top + (resultFont.Height * nLines++)));
                    resultStr = resultStr.Substring(str.Length + 1).Trim();
                    lineSize = g.MeasureString(resultStr, resultFont);
                }
                g.DrawString(resultStr, resultFont, Brushes.Black, new PointF(_CellRects[ctr].Left + _RowPadding.Left + _CellPadding.Left, _Dimensions.Top + _CellRects[ctr].Top + _RowPadding.Top + (nLines * resultFont.Height)));
                if (ctr == _CellRects.Count - 1)
                    g.DrawLine(Pens.Black, new Point(_Dimensions.Right + _RowPadding.Right, _Dimensions.Top - _RowPadding.Top), new Point(_Dimensions.Right + _RowPadding.Right, _Dimensions.Bottom + _RowPadding.Vertical));
                else
                    g.DrawLine(Pens.Black, new Point(_CellRects[ctr].Right, _Dimensions.Top - _RowPadding.Top), new Point(_CellRects[ctr].Right, _Dimensions.Bottom + _RowPadding.Vertical));
            }
            g.DrawLine(Pens.Black, new Point(_Dimensions.Left - _RowPadding.Left, _Dimensions.Bottom + _RowPadding.Vertical), new Point(_Dimensions.Right + _RowPadding.Right, _Dimensions.Bottom + _RowPadding.Vertical));
        }
    }
}
