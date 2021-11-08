using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;

namespace IATClient
{
    public class CColorGradient
    {
        private Color StartColor, EndColor;
        private Color _CurrentColor;
        private int aDiff, rDiff, gDiff, bDiff;

        public Color CurrentColor
        {
            get
            {
                return _CurrentColor;
            }
        }

        private int NumGrads;
        private int CurrGrad = 0;

        public CColorGradient(Color startColor, Color endColor, int nGrads)
        {
            StartColor = startColor;
            EndColor = endColor;
            CurrentColor = StartColor;
            aDiff = EndColor.A - StartColor.A;
            rDiff = EndColor.R - StartColor.R;
            bDiff = EndColor.B - StartColor.B;
            gDiff = EndColor.G - StartColor.G;
            NumGrads = nGrads;
        }

        public void NextGrad()
        {
            _CurrentColor = Color.FromArgb(StartColor.A + (aDiff * CurrGrad / NumGrads), StartColor.R + (rDiff * CurrGrad / NumGrads), StartColor.G + (gDiff * CurrGrad / NumGrads),
                StartColor.B + (bDiff * CurrGrad / NumGrads));
        }
    }
}
