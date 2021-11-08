using System;
using System.Drawing;
using System.Collections.Generic;

namespace IATClient
{
    public class CDualKeyLayout
    {
        private static readonly object genLock = new object();
        private Uri _Key1Uri, _Key2Uri = null, _ConjunctionUri;
        private readonly object lockObj = new object();
        public int VertPadding { get; set; } = 10;
        private readonly DIDualKey LeftValue; 
        private readonly DIDualKey RightValue;
        private bool LayoutSuspended = false;

        public void SuspendLayout()
        {
            LayoutSuspended = true;
        }
        public void ResumeLayout(bool immediate)
        {
            LayoutSuspended = false;
            if (immediate)
                Invalidate();
        }
        public void Invalidate()
        {
            if (LayoutSuspended)
                return;
            PerformLayout();
        }

        public Uri Key1Uri
        {
            get
            {
                return _Key1Uri;
            }
            set
            {
                if (Key1Uri != null)
                {
                    CIATKey oldKey = CIAT.SaveFile.GetIATKey(Key1Uri);
                    LeftValue[oldKey.LeftValueUri] = null;
                    RightValue[oldKey.RightValueUri] = null;
                }
                _Key1Uri = value;
                if (_Key1Uri != null) { 
                    CIATKey newKey = CIAT.SaveFile.GetIATKey(value);
                    LeftValue[newKey.LeftValueUri] = Rectangle.Empty;
                    RightValue[newKey.RightValueUri] = Rectangle.Empty;
                    if (!LayoutSuspended)
                        PerformLayout();
                }
            }
        }

        public Uri Key2Uri
        {
            get
            {
                return _Key2Uri;
            }
            set
            {
                if (value == null)
                    return;
                if (Key2Uri != null)
                {
                    CIATKey oldKey = CIAT.SaveFile.GetIATKey(Key2Uri);
                    LeftValue[oldKey.LeftValueUri] = null;
                    RightValue[oldKey.RightValueUri] = null;
                }
                _Key2Uri = value;
                if (_Key2Uri != null)
                {
                    CIATKey newKey = CIAT.SaveFile.GetIATKey(value);
                    LeftValue[newKey.LeftValueUri] = Rectangle.Empty;
                    RightValue[newKey.RightValueUri] = Rectangle.Empty;
                    if (!LayoutSuspended)
                        PerformLayout();
                }
           }
        }

        public Uri ConjunctionUri
        {
            get
            {
                return _ConjunctionUri;
            }
            set
            {
                if (value == null)
                    return;
                Uri cUri = _ConjunctionUri;
                _ConjunctionUri = value;
                if (cUri != null)
                {
                    LeftValue[cUri] = null;
                    RightValue[cUri] = null;
                }
                LeftValue[ConjunctionUri] = Rectangle.Empty;
                RightValue[ConjunctionUri] = Rectangle.Empty;
                if (!LayoutSuspended)
                    PerformLayout();
            }
        }


        public DIBase DITopLeft
        {
            get
            {
                if (Key1Uri == null)
                    return null;
                return CIAT.SaveFile.GetIATKey(Key1Uri).LeftValue;
            }
        }

        public DIBase DITopRight
        {
            get
            {
                if (Key1Uri == null)
                    return null;
                return CIAT.SaveFile.GetIATKey(Key1Uri).RightValue;
            }
        }

        public DIBase DIBottomLeft
        {
            get
            {
                if (Key2Uri == null)
                    return null;
                return CIAT.SaveFile.GetIATKey(Key2Uri).LeftValue;
            }
        }

        public DIBase DIBottomRight
        {
            get
            {
                if (Key2Uri == null)
                    return null;
                return CIAT.SaveFile.GetIATKey(Key2Uri).RightValue;
            }
        }

        public CDualKeyLayout(DIDualKey leftDisplay, DIDualKey rightDisplay)
        {
            SuspendLayout();
            LeftValue = leftDisplay;
            RightValue = rightDisplay;
        }

        public void PerformLayout()
        {
            try
            {
                lock (genLock)
                {
                    if ((Key1Uri == null) || (Key2Uri == null) || (ConjunctionUri == null))
                        return;
                    DIConjunction conjunction = CIAT.SaveFile.GetDI(ConjunctionUri) as DIConjunction;
                    DIBase diUpperLeft = null, diUpperRight = null, diLowerLeft = null, diLowerRight = null;
                    Size bounds = DIType.DualKey.GetBoundingSize();
                    bool ULFixed, LLFixed, URFixed, LRFixed;
                    Size URSize = Size.Empty, ULSize = Size.Empty, LLSize = Size.Empty, LRSize = Size.Empty;
                    double arBounds, arResponseValue;
                    Point ptConjunction;

                    // determine the size of each non fixed-sized display item if scaled to fit the response value rectangle
                    arBounds = (double)bounds.Width / (double)bounds.Height;

                    if (Key1Uri == null)
                    {
                        ULSize = Size.Empty;
                        URSize = Size.Empty;
                        ULFixed = false;
                        URFixed = false;
                    }
                    else
                    {
                        diUpperLeft = CIAT.SaveFile.GetIATKey(Key1Uri).LeftValue;
                        if (diUpperLeft.Type == DIType.Null)
                        {
                            ULSize = Size.Empty;
                            ULFixed = false;
                        }
                        else
                        {
                            if (diUpperLeft.Type == DIType.ResponseKeyText)
                            {
                                ULSize = diUpperLeft.AbsoluteBounds.Size;
                                ULFixed = true;
                            }
                            else if (diUpperLeft.Type == DIType.ResponseKeyImage)
                            {
                                arResponseValue = (double)diUpperLeft.IImage.OriginalSize.Width / (double)diUpperLeft.IImage.OriginalSize.Height;
                                if (arBounds > arResponseValue)
                                    ULSize = new Size((int)(bounds.Height * arResponseValue), bounds.Height);
                                else
                                    ULSize = new Size(bounds.Width, (int)(bounds.Height / arResponseValue));
                                ULFixed = false;
                            }
                            else
                                ULFixed = true;
                        }
                        diUpperRight = CIAT.SaveFile.GetIATKey(Key1Uri).RightValue;
                        if (diUpperRight.Type == DIType.Null)
                        {
                            URSize = Size.Empty;
                            URFixed = false;
                        }
                        else
                        {
                            if (diUpperRight.Type == DIType.ResponseKeyText)
                            {
                                URSize = diUpperRight.AbsoluteBounds.Size;
                                URFixed = true;
                            }
                            else if (diUpperRight.Type == DIType.ResponseKeyImage)
                            {
                                arResponseValue = (double)diUpperRight.IImage.OriginalSize.Width / (double)diUpperRight.IImage.OriginalSize.Height;
                                if (arBounds > arResponseValue)
                                    URSize = new Size((int)(bounds.Height * arResponseValue), bounds.Height);
                                else
                                    URSize = new Size(bounds.Width, (int)(bounds.Height / arResponseValue));
                                URFixed = false;
                            }
                            else
                                URFixed = true;
                        }
                    }
                    if (Key2Uri == null)
                    {
                        LLSize = Size.Empty;
                        LRSize = Size.Empty;
                        LLFixed = false;
                        LRFixed = false;
                    }
                    else
                    {
                        diLowerLeft = CIAT.SaveFile.GetIATKey(Key2Uri).LeftValue;
                        if (diLowerLeft.Type == DIType.Null)
                        {
                            LLSize = Size.Empty;
                            LLFixed = false;
                        }
                        else
                        {
                            if (diLowerLeft.Type == DIType.ResponseKeyText)
                            {
                                LLSize = diLowerLeft.AbsoluteBounds.Size;
                                LLFixed = true;
                            }
                            else if (diLowerLeft.Type == DIType.ResponseKeyImage)
                            {
                                arResponseValue = (double)diLowerLeft.IImage.OriginalSize.Width / (double)diLowerLeft.IImage.OriginalSize.Height;
                                if (arBounds > arResponseValue)
                                    LLSize = new Size((int)(bounds.Height * arResponseValue), bounds.Height);
                                else
                                    LLSize = new Size(bounds.Width, (int)(bounds.Height / arResponseValue));
                                LLFixed = false;
                            }
                            else
                                LLFixed = true;
                        }
                        diLowerRight = CIAT.SaveFile.GetIATKey(Key2Uri).RightValue;
                        if (diLowerRight.Type == DIType.Null)
                        {
                            LRSize = Size.Empty;
                            LRFixed = false;
                        }
                        else
                        {
                            if (diLowerRight.Type == DIType.ResponseKeyText)
                            {
                                LRSize = diLowerRight.AbsoluteBounds.Size;
                                LRFixed = true;
                            }
                            else if (diLowerRight.Type == DIType.ResponseKeyImage)
                            {
                                arResponseValue = (double)diLowerRight.IImage.OriginalSize.Width / (double)diLowerRight.IImage.OriginalSize.Height;
                                if (arBounds > arResponseValue)
                                    LRSize = new Size((int)(bounds.Height * arResponseValue), bounds.Height);
                                else
                                    LRSize = new Size(bounds.Width, (int)(bounds.Height / arResponseValue));
                                LRFixed = false;
                            }
                            else
                                LRFixed = true;
                        }
                    }

                    lock (lockObj)
                    {
                        Double arUL, arLL, arUR, arLR, arUpper, arLower, Proportion;
                        arUL = (ULSize == Size.Empty) ? 0 : ((double)ULSize.Width / (double)ULSize.Height);
                        arLL = (LLSize == Size.Empty) ? 0 : ((double)LLSize.Width / (double)LLSize.Height);
                        arUR = (URSize == Size.Empty) ? 0 : ((double)URSize.Width / (double)URSize.Height);
                        arLR = (LRSize == Size.Empty) ? 0 : ((double)LRSize.Width / (double)LRSize.Height);

                        if ((arUL == 0) && (arUR == 0))
                            arUpper = Double.NaN;
                        else if (!ULFixed && !URFixed)
                            arUpper = (Math.Abs(arUL - 1) < Math.Abs(arUR - 1)) ? arUL : arUR;
                        else if (!ULFixed)
                            arUpper = arUL;
                        else if (!URFixed)
                            arUpper = arUR;
                        else
                            arUpper = (Math.Abs(arUL - 1) < Math.Abs(arUR - 1)) ? arUL : arUR;

                        if ((arLL == 0) && (arLR == 0))
                            arLower = Double.NaN;
                        else if (!LLFixed && !LRFixed)
                            arLower = (Math.Abs(arLL - 1) < Math.Abs(arLR - 1)) ? arLL : arLR;
                        else if (!LRFixed)
                            arLower = arLL;
                        else if (!LRFixed)
                            arLower = arLR;
                        else
                            arLower = (Math.Abs(arLL - 1) < Math.Abs(arLR - 1)) ? arLL : arLR; 

                        if ((LRSize.IsEmpty || LLSize.IsEmpty) && (URSize.IsEmpty || ULSize.IsEmpty))
                        {
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, (bounds.Height - conjunction.AbsoluteBounds.Height) >> 1);
                        }
                        else if (!ULFixed && !LLFixed && !URFixed && !LRFixed)
                        {
                            if (Double.IsNaN(arLower))
                            {
                                ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, bounds.Height - conjunction.AbsoluteBounds.Height - VertPadding);
                            }
                            else if (Double.IsNaN(arUpper))
                            {
                                ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, VertPadding);
                            }
                            else
                            {
                                Proportion = Math.Log(arUpper / arLower, bounds.Height * bounds.Width);
                                int nOffset = (int)((bounds.Height >> 1) * Proportion);
                                ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, ((bounds.Height - conjunction.AbsoluteBounds.Height) >> 1) + nOffset);
                            }
                        }
                        else if (!ULFixed && !LLFixed && !URFixed)
                        {
                            Proportion = Math.Log(arUpper / arLower, bounds.Height * bounds.Width);
                            int nOffset = (int)((bounds.Height >> 1) * Proportion);
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, ((bounds.Height - conjunction.AbsoluteBounds.Height) >> 1) + nOffset);
                            if (bounds.Height - ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding << 1) < LRSize.Height)
                                ptConjunction.Y = bounds.Height - conjunction.AbsoluteBounds.Height - (VertPadding << 1) - LRSize.Height;
                        }
                        else if (!ULFixed && !LLFixed && !LRFixed)
                        {
                            Proportion = Math.Log(arUpper / arLower, bounds.Height * bounds.Width);
                            int nOffset = (int)((bounds.Height >> 1) * Proportion);
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, ((bounds.Height - conjunction.AbsoluteBounds.Height) >> 1) + nOffset);
                            if (ptConjunction.Y - VertPadding < URSize.Height)
                                ptConjunction.Y = URSize.Height + VertPadding;
                        }
                        else if (!ULFixed && !URFixed && !LRFixed)
                        {
                            Proportion = Math.Log(arUpper / arLower, bounds.Height * bounds.Width);
                            int nOffset = (int)((bounds.Height >> 1) * Proportion);
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, ((bounds.Height - conjunction.AbsoluteBounds.Height) >> 1) + nOffset);
                            if (bounds.Height - ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding << 1) < LLSize.Height)
                                ptConjunction.Y = bounds.Height - conjunction.AbsoluteBounds.Height - (VertPadding << 1) - LLSize.Height;
                        }
                        else if (!LLFixed && !URFixed && !LRFixed)
                        {
                            Proportion = Math.Log(arUpper / arLower, bounds.Height * bounds.Width);
                            int nOffset = (int)((bounds.Height >> 1) * Proportion);
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, ((bounds.Height - conjunction.AbsoluteBounds.Height) >> 1) + nOffset);
                            if (ptConjunction.Y - VertPadding < ULSize.Height)
                                ptConjunction.Y = ULSize.Height + VertPadding;
                        }
                        else if (!ULFixed && !LLFixed)
                        {
                            Proportion = Math.Log(arUpper / arLower, bounds.Height * bounds.Width);
                            int nOffset = (int)((bounds.Height >> 1) * Proportion);
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, ((bounds.Height - conjunction.AbsoluteBounds.Height) >> 1) + nOffset);
                            if (bounds.Height - ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding << 1) < LRSize.Height)
                                ptConjunction.Y = bounds.Height - conjunction.AbsoluteBounds.Height - (VertPadding << 1) - LRSize.Height;
                            if (ptConjunction.Y - VertPadding < URSize.Height)
                                ptConjunction.Y = URSize.Height + VertPadding;
                        }
                        else if (!URFixed && !LRFixed)
                        {
                            Proportion = Math.Log(arUpper / arLower, bounds.Height * bounds.Width);
                            int nOffset = (int)((bounds.Height >> 1) * Proportion);
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, ((bounds.Height - conjunction.AbsoluteBounds.Height) >> 1) + nOffset);
                            if (bounds.Height - ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding << 1) < LLSize.Height)
                                ptConjunction.Y = bounds.Height - conjunction.AbsoluteBounds.Height - (VertPadding << 1) - LLSize.Height;
                            if (ptConjunction.Y - VertPadding < ULSize.Height)
                                ptConjunction.Y = ULSize.Height + VertPadding;
                        }
                        else if (!ULFixed && !LRFixed)
                        {
                            Proportion = Math.Log(arUpper / arLower, bounds.Height * bounds.Width);
                            int nOffset = (int)((bounds.Height >> 1) * Proportion);
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, ((bounds.Height - conjunction.AbsoluteBounds.Height) >> 1) + nOffset);
                            if (bounds.Height - ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding << 1) < LLSize.Height)
                                ptConjunction.Y = bounds.Height - conjunction.AbsoluteBounds.Height - (VertPadding << 1) - LLSize.Height;
                            if (ptConjunction.Y - VertPadding < URSize.Height)
                                ptConjunction.Y = URSize.Height + VertPadding;
                        }
                        else if (!URFixed && !LLFixed)
                        {
                            Proportion = Math.Log(arUpper / arLower, bounds.Height * bounds.Width);
                            int nOffset = (int)((bounds.Height >> 1) * Proportion);
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, ((bounds.Height - conjunction.AbsoluteBounds.Height) >> 1) + nOffset);
                            if (bounds.Height - ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding << 1) < LRSize.Height)
                                ptConjunction.Y = bounds.Height - conjunction.AbsoluteBounds.Height - (VertPadding << 1) - LRSize.Height;
                            if (ptConjunction.Y - VertPadding < ULSize.Height)
                                ptConjunction.Y = ULSize.Height + VertPadding;
                        }
                        else if (!ULFixed || !URFixed)
                        {
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, bounds.Height - conjunction.AbsoluteBounds.Height - VertPadding);
                            ptConjunction.Y -= (LLSize.Height > LRSize.Height) ? LLSize.Height : LRSize.Height;
                        }
                        else if (!LLFixed || !LRFixed)
                        {
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, VertPadding);
                            ptConjunction.Y += (ULSize.Height > URSize.Height) ? ULSize.Height : URSize.Height;
                        }
                        else
                        {
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, (bounds.Height - conjunction.AbsoluteBounds.Height) >> 1);
                            int upperHeight = Math.Max(ULSize.Height, URSize.Height);
                            int lowerHeight = Math.Max(LLSize.Height, LRSize.Height);
                            ptConjunction.Y += (upperHeight - lowerHeight) / 2; 
/*
                            if ((diUpperLeft.Type == DIType.ResponseKeyText) && (diUpperRight.Type == DIType.ResponseKeyText)
                                && (diLowerLeft.Type == DIType.ResponseKeyText) && (diLowerRight.Type == DIType.ResponseKeyText))
                            {
                                ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1,
                                    (bounds.Height - conjunction.AbsoluteBounds.Height) >> 1);

                            }
                            else if ((diUpperLeft.Type == DIType.ResponseKeyText) && (diUpperRight.Type == DIType.ResponseKeyText))
                            {
                                ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1,
                                    bounds.Height - conjunction.AbsoluteBounds.Height - VertPadding);
                                ptConjunction.Y -= (LLSize.Height > LRSize.Height) ? LLSize.Height : LRSize.Height;
                            }
                            else if ((diLowerLeft.Type == DIType.ResponseKeyText) && (diLowerRight.Type == DIType.ResponseKeyText))
                            {
                                ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, VertPadding);
                                ptConjunction.Y += (ULSize.Height > URSize.Height) ? ULSize.Height : URSize.Height;
                            }
                            else
                            {
                                ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, VertPadding);
                                ptConjunction.Y += (ULSize.Height > URSize.Height) ? ULSize.Height : URSize.Height;
                            }
                            */
                        }

                        // scale the bounding rectangles of the response values
                        Nullable<Rectangle> LV1Rect = null, LV2Rect = null, RV1Rect = null, RV2Rect = null;
                        if (URFixed && ULFixed)
                        {
                            if (ULSize.Height > URSize.Height)
                            {
                                LV1Rect = new Rectangle(new Point((bounds.Width - diUpperLeft.AbsoluteBounds.Width) >> 1, ptConjunction.Y - (VertPadding >> 1) - ULSize.Height), diUpperLeft.AbsoluteBounds.Size);
                                RV1Rect = new Rectangle(new Point((bounds.Width - diUpperRight.AbsoluteBounds.Width) >> 1, ptConjunction.Y - (VertPadding >> 1) - ((ULSize.Height + URSize.Height) >> 1)),
                                    diUpperLeft.AbsoluteBounds.Size);
                            }
                            else
                            {
                                LV1Rect = new Rectangle(new Point((bounds.Width - diUpperLeft.AbsoluteBounds.Width) >> 1, ptConjunction.Y - (VertPadding >> 1) - ((URSize.Height + ULSize.Height) >> 1)),
                                    diUpperLeft.AbsoluteBounds.Size);
                                RV1Rect = new Rectangle(new Point((bounds.Width - diUpperRight.AbsoluteBounds.Width) >> 1, ptConjunction.Y - (VertPadding >> 1) - URSize.Height), diUpperRight.AbsoluteBounds.Size);
                            }
                        }
                        else
                        {
                            if (Double.IsNaN(arLower))
                            {
                                ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, bounds.Height - conjunction.AbsoluteBounds.Height - VertPadding);
                            }
                            else if (Double.IsNaN(arUpper))
                            {
                                ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, VertPadding);
                            }
                            else
                            {
                                Proportion = Math.Log(arUpper / arLower, bounds.Height * bounds.Width);
                                int nOffset = (int)((bounds.Height >> 1) * Proportion);
                                ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, ((bounds.Height - conjunction.AbsoluteBounds.Height) >> 1) + nOffset);
                            }
                            if ((URSize == Size.Empty) && (ULSize == Size.Empty))
                            {
                                LV1Rect = Rectangle.Empty;
                                RV1Rect = Rectangle.Empty;
                            }
                            else
                            {
                                int upperHeight = ptConjunction.Y - VertPadding;
                                if (!ULFixed)
                                    LV1Rect = new Rectangle((int)(bounds.Width - upperHeight * arUL) >> 1, VertPadding >> 1, (int)(upperHeight * arUL), upperHeight);
                                else
                                    LV1Rect = new Rectangle((int)(bounds.Width - diUpperLeft.AbsoluteBounds.Width) >> 1, (int)(ptConjunction.Y - Math.Max(diUpperLeft.AbsoluteBounds.Height, upperHeight * arUL)),
                                        diUpperLeft.AbsoluteBounds.Width, diUpperLeft.AbsoluteBounds.Height);
                                if (!URFixed)
                                    RV1Rect = new Rectangle((int)(bounds.Width - upperHeight * arUR) >> 1, VertPadding >> 1, (int)(upperHeight * arUR), upperHeight);
                                else
                                    RV1Rect = new Rectangle((int)(bounds.Width - diUpperRight.AbsoluteBounds.Width) >> 1, (int)(ptConjunction.Y - Math.Max(diUpperRight.AbsoluteBounds.Height, upperHeight * arUR)),
                                        diUpperRight.AbsoluteBounds.Width, diUpperRight.AbsoluteBounds.Height);
                            }
                        }
                        if (LLFixed && LRFixed)
                        {
                            if (LLSize.Height > LRSize.Height)
                            {
                                LV2Rect = new Rectangle(new Point((bounds.Width - diLowerLeft.AbsoluteBounds.Width) >> 1, ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding >> 1)), diLowerLeft.AbsoluteBounds.Size);
                                RV2Rect = new Rectangle(new Point((bounds.Width - diLowerRight.AbsoluteBounds.Width) >> 1, ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding >> 1) + ((LLSize.Height - LRSize.Height) >> 1)),
                                    diLowerRight.AbsoluteBounds.Size);
                            }
                            else
                            {
                                LV2Rect = new Rectangle(new Point((bounds.Width - diLowerLeft.AbsoluteBounds.Width) >> 1, ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding >> 1) + ((LRSize.Height - LLSize.Height) >> 1)),
                                    diLowerLeft.AbsoluteBounds.Size);
                                RV2Rect = new Rectangle(new Point((bounds.Width - diLowerRight.AbsoluteBounds.Width) >> 1, ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding >> 1)), diLowerRight.AbsoluteBounds.Size);

                            }
                        }
                        else
                        {
                            if ((LRSize == Size.Empty) && (LLSize == Size.Empty))
                            {
                                LV2Rect = Rectangle.Empty;
                                RV2Rect = Rectangle.Empty;
                            }
                            else
                            {
                                int lowerHeight = bounds.Height - ptConjunction.Y - conjunction.AbsoluteBounds.Height - VertPadding;
                                int lowerTopOffset = ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding >> 1);
                                if (!LLFixed)
                                    LV2Rect = new Rectangle((int)(bounds.Width - lowerHeight * arLL) >> 1, lowerTopOffset, (int)(lowerHeight * arLL), lowerHeight);
                                else
                                    LV2Rect = new Rectangle((int)(bounds.Width - diLowerLeft.AbsoluteBounds.Width) >> 1, (int)(ptConjunction.Y + Math.Max(diLowerLeft.AbsoluteBounds.Height, lowerHeight * arLL)),
                                        diLowerLeft.AbsoluteBounds.Width, diLowerLeft.AbsoluteBounds.Height);
                                if (!LRFixed)
                                    RV2Rect = new Rectangle((int)(bounds.Width - lowerHeight * arLR) >> 1, lowerTopOffset, (int)(lowerHeight * arLR), lowerHeight);
                                else
                                    RV2Rect = new Rectangle((int)(bounds.Width - diLowerRight.AbsoluteBounds.Width) >> 1, (int)(ptConjunction.Y + Math.Max(diLowerRight.AbsoluteBounds.Height, lowerHeight * arLR)),
                                        diLowerRight.AbsoluteBounds.Width, diLowerRight.AbsoluteBounds.Height);
                            }
                        }
                        /*
                        // scale the auto-scaled response key images
                        if (Key1Uri != null)
                        {
                            if (diUpperLeft.Type == DIType.ResponseKeyImage)
                                (diUpperLeft as DIResponseKeyImage).Resize(LV1Rect.Value);
                            if (diUpperRight.Type == DIType.ResponseKeyImage)
                                (diUpperRight as DIResponseKeyImage).Resize(RV1Rect.Value);
                        }
                        if (Key2Uri != null)
                        {
                            if (diLowerLeft.Type == DIType.ResponseKeyImage)
                                (diLowerLeft as DIResponseKeyImage).Resize(LV2Rect.Value);
                            if (diLowerRight.Type == DIType.ResponseKeyImage)
                                (diLowerRight as DIResponseKeyImage).Resize(RV2Rect.Value);
                        }
                        */
                        Rectangle ConjunctionRect = new Rectangle(ptConjunction, conjunction.AbsoluteBounds.Size);

                        LeftValue.LockImageDictionary();
                        LeftValue.ClearComponents();
                        if (Key1Uri != null)
                            LeftValue[CIAT.SaveFile.GetIATKey(Key1Uri).LeftValueUri] = LV1Rect;
                        if (Key2Uri != null)
                            LeftValue[CIAT.SaveFile.GetIATKey(Key2Uri).LeftValueUri] = LV2Rect;
                        LeftValue[ConjunctionUri] = ConjunctionRect;
                        LeftValue.UnlockImageDictionary();

                        RightValue.LockImageDictionary();
                        RightValue.ClearComponents();
                        if (Key1Uri != null)
                            RightValue[CIAT.SaveFile.GetIATKey(Key1Uri).RightValueUri] = RV1Rect;
                        if (Key2Uri != null)
                            RightValue[CIAT.SaveFile.GetIATKey(Key2Uri).RightValueUri] = RV2Rect;
                        RightValue[ConjunctionUri] = ConjunctionRect;
                        RightValue.UnlockImageDictionary();
                        /*
                        if (diUpperLeft.Type == DIType.ResponseKeyImage)
                            diUpperLeft.ScheduleInvalidation();
                        if (diUpperRight.Type == DIType.ResponseKeyImage)
                            diUpperRight.ScheduleInvalidation();
                        if (diLowerLeft.Type == DIType.ResponseKeyImage)
                            diLowerLeft.ScheduleInvalidation();
                        if (diLowerRight.Type == DIType.ResponseKeyImage)
                            diLowerRight.ScheduleInvalidation();
                            */
                    }
                }
            }
            catch (KeyNotFoundException) { }
            LeftValue.ScheduleInvalidation();
            RightValue.ScheduleInvalidation();
        }
    }
}
