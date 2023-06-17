using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.Threading;

namespace IATClient
{
    public class CDualKeyLayout
    {
        private Uri _Key1Uri, _Key2Uri = null, _ConjunctionUri;
        private readonly object lockObj = new object();
        public int VertPadding { get; set; } = (int)((double)CIAT.SaveFile.Layout.KeyValueSize.Height / 10F);
        private readonly DIDualKey LeftValue;
        private readonly DIDualKey RightValue;

        public Uri Key1Uri
        {
            get
            {
                return _Key1Uri;
            }
            set
            {
                if (_Key1Uri != null)
                {
                    CIATKey oldKey = CIAT.SaveFile.GetIATKey(Key1Uri);
                    LeftValue[oldKey.LeftValueUri] = null;
                    RightValue[oldKey.RightValueUri] = null;
                }
                if ((value != null) && !value.Equals(_Key1Uri))
                {
                    CIATKey newKey = CIAT.SaveFile.GetIATKey(value);
                    LeftValue[newKey.LeftValueUri] = Rectangle.Empty;
                    RightValue[newKey.RightValueUri] = Rectangle.Empty;
                    _Key1Uri = value;
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
                if ((value != null) && !value.Equals(_Key2Uri))
                {
                    CIATKey newKey = CIAT.SaveFile.GetIATKey(value);
                    LeftValue[newKey.LeftValueUri] = Rectangle.Empty;
                    RightValue[newKey.RightValueUri] = Rectangle.Empty;
                    _Key2Uri = value;
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
            LeftValue = leftDisplay;
            RightValue = rightDisplay;
            Status = Idle;
        }
        private readonly CancellationTokenSource CompositeCancellationTokenSource = new CancellationTokenSource();
        private readonly CancellationTokenSource ComponentCancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent LayoutBusy = new ManualResetEvent(true);
        private readonly ManualResetEvent ComponentValidationEvent = new ManualResetEvent(false);
        private readonly ManualResetEvent CompositeValidationEvent = new ManualResetEvent(false);
        private readonly object Idle = new object(), LayoutRunning = new object(), LayoutQueued = new object();
        private ConcurrentDictionary<Uri, ManualResetEvent> ValidationEvents = new ConcurrentDictionary<Uri, ManualResetEvent>();
        private ConcurrentDictionary<Uri, Func<bool>> ValidationLocks = new ConcurrentDictionary<Uri, Func<bool>>();
        private int ComponentCounter = 0, CompositeCounter = 0;
        private object Status;

        public void CompositeValidated(Uri u)
        {
            if (ValidationLocks.TryRemove(u, out _))
                if (--CompositeCounter == 0)
                {
                    CompositeValidationEvent.Set();
                    CompositeCancellationTokenSource.Cancel();
                }
        }

        public void ComponentValidated(Uri u)
        {
            if (ValidationLocks.TryRemove(u, out _))
                if (--ComponentCounter == 0)
                {
                    ComponentValidationEvent.Set();
                    //         ComponentCancellationTokenSource.Cancel();
                }
        }

        public void PerformLayout()
        {
            if ((Key1Uri == null) || (Key2Uri == null) || (ConjunctionUri == null))
                return;
            if (Interlocked.CompareExchange(ref Status, LayoutRunning, Idle).Equals(Idle))
            {
                LayoutBusy.Reset();
            }
            else if (Interlocked.CompareExchange(ref Status, LayoutQueued, LayoutRunning).Equals(LayoutRunning))
            {
                LayoutBusy.WaitOne();
                LayoutBusy.Reset();
                Interlocked.Exchange(ref Status, LayoutRunning);
            }
            else
                return;
            try
            {
                ComponentCounter = 0;
                DIConjunction conjunction = CIAT.SaveFile.GetDI(ConjunctionUri) as DIConjunction;
                ComponentCounter++;
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
                        ComponentCounter++;
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
                                ULSize = new Size(bounds.Width, (int)(bounds.Width / arResponseValue));
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
                        ComponentCounter++;
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
                                URSize = new Size(bounds.Width, (int)(bounds.Width / arResponseValue));
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
                        ComponentCounter++;
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
                        ComponentCounter++;
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
                                LRSize = new Size(bounds.Width, (int)(bounds.Width / arResponseValue));
                            LRFixed = false;
                        }
                        else
                            LRFixed = true;
                    }
                }

                lock (lockObj)
                {
                    Nullable<Rectangle> LV1Rect = null, LV2Rect = null, RV1Rect = null, RV2Rect = null;
                    Double arUL, arLL, arUR, arLR, arUpper, arLower, Proportion;
                    arUL = (ULSize == Size.Empty) ? 0 : ((double)ULSize.Width / (double)ULSize.Height);
                    arLL = (LLSize == Size.Empty) ? 0 : ((double)LLSize.Width / (double)LLSize.Height);
                    arUR = (URSize == Size.Empty) ? 0 : ((double)URSize.Width / (double)URSize.Height);
                    arLR = (LRSize == Size.Empty) ? 0 : ((double)LRSize.Width / (double)LRSize.Height);

                    if ((arUL == 0) && (arUR == 0))
                        arUpper = Double.NaN;
                    else if (!ULFixed && !URFixed)
                        arUpper = (Math.Abs(arUL) < Math.Abs(arUR)) ? arUL : arUR;
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
                        ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, (bounds.Height - VertPadding) >> 1);

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
                    else if (!ULFixed && !URFixed)
                    {
                        int bottomHeight = Math.Max(LLSize.Height, LRSize.Height);
                        ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, bounds.Height - (VertPadding + bottomHeight));
                        LV2Rect = new Rectangle(new Point((bounds.Width - LLSize.Width) >> 1,
                            ptConjunction.Y + (VertPadding >> 1) + conjunction.AbsoluteBounds.Height), LLSize);
                        RV2Rect = new Rectangle(new Point((bounds.Width - LRSize.Width) >> 1,
                            ptConjunction.Y + (VertPadding >> 1) + conjunction.AbsoluteBounds.Height), LRSize);
                        int upperHeight = ptConjunction.Y - VertPadding;
                        double arKeyValueBounds = (double)bounds.Width / (double)upperHeight;
                        Size szLRKey, szLLKey;
                        double arKey = (double)ULSize.Width / (double)ULSize.Height;
                        if (arKeyValueBounds > arKey)
                            szLLKey = new Size((int)((double)upperHeight * arKey), upperHeight);
                        else
                            szLLKey = new Size(bounds.Width, (int)((double)bounds.Width / arKey));
                        arKey = (double)URSize.Width / (double)URSize.Height;
                        if (arKeyValueBounds > arKey)
                            szLRKey = new Size((int)((double)upperHeight * arKey), upperHeight);
                        else
                            szLRKey = new Size(bounds.Width, (int)((double)bounds.Width / arKey));
                        LV1Rect = new Rectangle(new Point((bounds.Width - szLLKey.Width) >> 1, (VertPadding + upperHeight - szLLKey.Height) >> 1), szLLKey);
                        RV1Rect = new Rectangle(new Point((bounds.Width - szLRKey.Width) >> 1, (VertPadding + upperHeight - szLRKey.Height) >> 1), szLRKey);

                        /*                      Size szLLKey;
                                                double lScale;
                                                arKey = (double)URSize.Width / (double)URSize.Height;
                                                Size szLRKey;
                                                double rScale;
                                                if (arKeyValueBounds > arKey)
                                                {
                                                    szLRKey = new Size((int)((double)upperHeight * arKey), upperHeight);
                                                    rScale = (double)upperHeight / (double)diUpperRight.IImage.OriginalSize.Height;
                                                }
                                                else
                                                {
                                                    szLRKey = new Size(bounds.Width, (int)((double)upperHeight / arKey));
                                                    rScale = (double)bounds.Width / (double)diUpperRight.IImage.OriginalSize.Width;
                                                }
                                                if (lScale < rScale)
                                                {
                                                    LV1Rect = new Rectangle(new Point((bounds.Width - szLLKey.Width) >> 1, (VertPadding + upperHeight - szLLKey.Height) >> 1), szLLKey);
                                                    int rvWidth = (int)((double)szLRKey.Width * lScale / rScale);
                                                    int rvHeight = (int)((double)szLRKey.Height * lScale / rScale);
                                                    RV1Rect = new Rectangle((int)((double)((bounds.Width - szLLKey.Width) >> 1) * (double)lScale / (double)rScale),
                                                        (int)((double)((VertPadding + upperHeight - szLRKey.Height) >> 1) * (double)lScale / (double)rScale),
                                                        rvWidth, rvHeight);
                                                } else
                                                {
                                                    int lvWidth = (int)((double)szLRKey.Width * rScale / lScale);
                                                    int lvHeight = (int)((double)szLRKey.Height * rScale / lScale);
                                                    LV1Rect = new Rectangle((int)((double)((bounds.Width - szLLKey.Width) >> 1) * (double)rScale / (double)lScale),
                                                        (int)((double)((VertPadding + upperHeight - szLLKey.Height) >> 1) * (double)rScale / (double)lScale),
                                                        lvWidth, lvHeight);
                                                }
                        */

                    }
                    else if (!LLFixed && !LRFixed)
                    {
                        ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, Math.Max(ULSize.Height + VertPadding, URSize.Height + VertPadding));
                        int llrHeight = bounds.Height - ptConjunction.Y - VertPadding - conjunction.AbsoluteBounds.Height;
                        if (llrHeight > LLSize.Height)
                            LLSize = new Size((int)((double)llrHeight * arLL), llrHeight);
                        if (llrHeight > LRSize.Height)
                            LRSize = new Size((int)((double)llrHeight * arLR), llrHeight);
                        LV1Rect = new Rectangle(new Point((bounds.Width - ULSize.Width) >> 1, ptConjunction.Y - (VertPadding >> 1) - ULSize.Height), ULSize);
                        RV1Rect = new Rectangle(new Point((bounds.Width - URSize.Width) >> 1, ptConjunction.Y - (VertPadding >> 1) - URSize.Height), URSize);
                        LV2Rect = new Rectangle(new Point((bounds.Width - LLSize.Width) >> 1, ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding >> 2)),
                            LLSize);
                        RV2Rect = new Rectangle(new Point((bounds.Width - LRSize.Width) >> 1, ptConjunction.Y + conjunction.AbsoluteBounds.Height + (VertPadding >> 2)),
                            LRSize);
                        if (LRSize.Height > LLSize.Height)
                            LV2Rect = new Rectangle(new Point(LV2Rect.Value.X, LV2Rect.Value.Y + ((LRSize.Height - LLSize.Height) >> 1)), LLSize);
                        else if (ULSize.Height > URSize.Height)
                            RV2Rect = new Rectangle(new Point(RV2Rect.Value.X, RV2Rect.Value.Y + ((LLSize.Height - LRSize.Height) >> 1)), LRSize);
                    }
                    else
                    {
                        int upperHeight = Math.Max(diUpperLeft.AbsoluteBounds.Height, diUpperRight.AbsoluteBounds.Height);
                        int lowerHeight = Math.Max(diLowerLeft.AbsoluteBounds.Height, diLowerRight.AbsoluteBounds.Height);
                        int cHeight = conjunction.AbsoluteBounds.Height + VertPadding;
                        LV1Rect = new Rectangle((bounds.Width - diUpperLeft.AbsoluteBounds.Width) >> 1, (bounds.Height - (upperHeight + cHeight + lowerHeight)) >> 1,
                            diUpperLeft.AbsoluteBounds.Width, diUpperLeft.AbsoluteBounds.Height);
                        LV2Rect = new Rectangle((bounds.Width - diLowerLeft.AbsoluteBounds.Width) >> 1, (bounds.Height + upperHeight + cHeight - lowerHeight) >> 1,
                            diLowerLeft.AbsoluteBounds.Width, diLowerLeft.AbsoluteBounds.Height);
                        RV1Rect = new Rectangle((bounds.Width - diUpperRight.AbsoluteBounds.Width) >> 1, (bounds.Height - (upperHeight + cHeight + lowerHeight)) >> 1,
                            diUpperRight.AbsoluteBounds.Width, diUpperRight.AbsoluteBounds.Height);
                        RV2Rect = new Rectangle((bounds.Width - diLowerRight.AbsoluteBounds.Width) >> 1, (bounds.Height - lowerHeight + upperHeight + cHeight) >> 1,
                            diLowerRight.AbsoluteBounds.Width, diLowerRight.AbsoluteBounds.Height);
                        ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, Math.Max(LV1Rect.Value.Bottom, RV1Rect.Value.Bottom) + VertPadding / 2);
                        /*
                        int upperHeight = Math.Max(ULSize.Height, URSize.Height);
                        int lowerHeight = Math.Max(LLSize.Height, LRSize.Height);
                        ptConjunction.Y += (upperHeight - lowerHeight) / 2;

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
                    /*
                    // scale the bounding rectangles of the response values

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
                            ptConjunction = new Point((bounds.Width - conjunction.AbsoluteBounds.Width) >> 1, bounds.Height - conjunction.AbsoluteBounds.Height - VertPadding);
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
                            {
                                Rectangle lv1Rect = new Rectangle((int)(bounds.Width - upperHeight * arUL) >> 1, VertPadding >> 1, (int)(upperHeight * arUL), upperHeight);
                                if (lv1Rect.Width > bounds.Width - (VertPadding >> 1))
                                {
                                    lv1Rect.Width = bounds.Width - (VertPadding >> 1);
                                    lv1Rect.Height = (int)(lv1Rect.Width / arUR);
                                    lv1Rect.X = (bounds.Width - lv1Rect.Width) >> 1;
                                    LV1Rect = lv1Rect;
                                }
                            }
                            else
                                LV1Rect = new Rectangle((int)(bounds.Width - diUpperLeft.AbsoluteBounds.Width) >> 1, (int)(ptConjunction.Y - Math.Max(diUpperLeft.AbsoluteBounds.Height, upperHeight * arUL)),
                                    diUpperLeft.AbsoluteBounds.Width, diUpperLeft.AbsoluteBounds.Height);
                            if (!URFixed)
                            {
                                Rectangle rv1Rect = new Rectangle((int)(bounds.Width - upperHeight * arUR) >> 1, VertPadding >> 1, (int)(upperHeight * arUR), upperHeight);
                                if (rv1Rect.Width > bounds.Width - (VertPadding >> 1))
                                {
                                    rv1Rect.Width = bounds.Width - (VertPadding >> 1);
                                    rv1Rect.Height = (int)(rv1Rect.Width / arUR);
                                    rv1Rect.X = (bounds.Width - rv1Rect.Width) >> 1;
                                    RV1Rect = rv1Rect;
                                }
                            }
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
                            {
                                Rectangle lv2Rect = new Rectangle((int)(bounds.Width - lowerHeight * arLL) >> 1, lowerTopOffset, (int)(lowerHeight * arLL), lowerHeight);
                                if (lv2Rect.Width > bounds.Width - (VertPadding >> 1))
                                {
                                    lv2Rect.Width = bounds.Width - (VertPadding >> 1);
                                    lv2Rect.Height = (int)(lv2Rect.Width / arLL);
                                    lv2Rect.X = (bounds.Width - lv2Rect.Width) >> 1;
                                }
                                LV2Rect = lv2Rect;
                            }
                            else
                                LV2Rect = new Rectangle((int)(bounds.Width - diLowerLeft.AbsoluteBounds.Width) >> 1, (int)(ptConjunction.Y + Math.Max(diLowerLeft.AbsoluteBounds.Height, lowerHeight * arLL)),
                                    diLowerLeft.AbsoluteBounds.Width, diLowerLeft.AbsoluteBounds.Height);
                            if (!LRFixed)
                            {
                                Rectangle rv2Rect = new Rectangle((int)(bounds.Width - lowerHeight * arLR) >> 1, lowerTopOffset, (int)(lowerHeight * arLR), lowerHeight);
                                if (rv2Rect.Width > bounds.Width - (VertPadding >> 1))
                                {
                                    rv2Rect.Width = bounds.Width - (VertPadding >> 1);
                                    rv2Rect.Height = (int)(rv2Rect.Width / arLR);
                                    rv2Rect.X = (bounds.Width - rv2Rect.Width) >> 1;
                                }
                                RV2Rect = rv2Rect;
                            }
                            else
                                RV2Rect = new Rectangle((int)(bounds.Width - diLowerRight.AbsoluteBounds.Width) >> 1, (int)(ptConjunction.Y + Math.Max(diLowerRight.AbsoluteBounds.Height, lowerHeight * arLR)),
                                    diLowerRight.AbsoluteBounds.Width, diLowerRight.AbsoluteBounds.Height);
                        }
                    }


                    // scale the auto-scaled response key images
                    /*
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
                    LeftValue.ClearComponents();
                    RightValue.ClearComponents();
                    var UL = CIAT.SaveFile.GetIATKey(Key1Uri).LeftValue;
                    var UR = CIAT.SaveFile.GetIATKey(Key1Uri).RightValue;
                    var LL = CIAT.SaveFile.GetIATKey(Key2Uri).LeftValue;
                    var LR = CIAT.SaveFile.GetIATKey(Key2Uri).RightValue;
                    var C = CIAT.SaveFile.GetDI(ConjunctionUri);
                    UL.SuspendLayout();
                    UR.SuspendLayout();
                    LL.SuspendLayout();
                    C.SuspendLayout();
                    LR.SuspendLayout();
                    LeftValue.SuspendLayout();
                    RightValue.SuspendLayout();
                    LeftValue.ClearComponents();
                    if (Key1Uri != null)
                        LeftValue[UL.URI] = LV1Rect;
                    if (Key2Uri != null)
                        LeftValue[LL.URI] = LV2Rect;
                    LeftValue[ConjunctionUri] = ConjunctionRect;

                    RightValue.ClearComponents();
                    if (Key1Uri != null)
                        RightValue[UR.URI] = RV1Rect;
                    if (Key2Uri != null)
                        RightValue[LR.URI] = RV2Rect;
                    RightValue[ConjunctionUri] = ConjunctionRect;
                    try
                    {
                        ValidationLock validationLock = new ValidationLock(new DIBase[] { UL as DIBase, UR as DIBase, LL as DIBase,
                            LR as DIBase, C as DIBase });
                        validationLock.InvalidationEvent.Reset();
                        UL.ResumeLayout(false);
                        UR.ResumeLayout(false);
                        LL.ResumeLayout(false);
                        LR.ResumeLayout(false);
                        C.ResumeLayout(false);
                        UL.ScheduleInvalidation();
                        UR.ScheduleInvalidation();
                        LL.ScheduleInvalidation();
                        LR.ScheduleInvalidation();
                        C.ScheduleInvalidation();
                        validationLock.InvalidationEvent.Set();
                        validationLock.ValidationEvent.WaitOne(1500);
                        validationLock = new ValidationLock(new DIBase[] { LeftValue, RightValue });
                        LeftValue.ResumeLayout(false);
                        RightValue.ResumeLayout(false);
                        LeftValue.ScheduleInvalidation();
                        RightValue.ScheduleInvalidation();
                        validationLock.InvalidationEvent.Set();
                        validationLock.ValidationEvent.WaitOne(1500);
                    }
                    catch (Exception ex)
                    {
                        ErrorReporter.ReportError(new CReportableException("Error Generating Dual Key", ex));
                    }
                    finally
                    {
                        LayoutBusy.Set();
                    }
                }

            }
            catch (Exception ex)
            {
                LayoutBusy.Set();
                throw ex;
            }
            //      LeftValue.ScheduleInvalidation();
            //    RightValue.ScheduleInvalidation();
        }
    }
}
