using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Threading;
using System.IO;

namespace IATClient
{
    [Serializable]
    public class CTextDisplayItem : CDisplayItem, IStimulus, IDisposable, ImageManager.INonUserImageSource 
    {
        public enum EUsedAs { stimulus, continueInstructions, responseKey, conjunction, mockItemInstructions, iatBlockInstructions, textInstructionScreen, keyedInstructionScreen };

        private INonUserImage _IATImage;

        public override IIATImage IATImage
        {
            get
            {
                return _IATImage;
            }
        }

        public override CComponentImage.ESourceType SourceType
        {
            get
            {
                return CComponentImage.ESourceType.text;
            }
        }
        
        private EUsedAs _UsedAs;

        public EUsedAs UsedAs
        {
            get
            {
                return _UsedAs;
            }
        }

        protected bool _ComponentImageValid = false;

        /// <summary>
        /// gets a description of the display item
        /// </summary>
        public string Description
        {
            get 
            {
                return Phrase;
            }
        }


        // the minimum accepted phrase font size
        const float MinPhraseFontSize = 8;

        // the phrase that represents the IATItem
        protected String _Phrase;

        /// <summary>
        /// gets or sets the phrase that represents the item
        /// </summary>
        public virtual String Phrase
        {
            get
            {
                Lock();
                String str = _Phrase;
                Unlock();
                return str;
            }
            set
            {
                Lock();
                BoundingRectInvalidated = true;
                _Phrase = value;
                Invalidate();
                Unlock();
            }
        }


        public override bool ImageExists
        {
            get
            {
                return true;
            }
        }

        public override bool IsValid { get { return _ComponentImageValid; } }

        public override void Validate()
        {
            Lock();
            _ComponentImageValid = true;
            Unlock();
        }

        public override void Invalidate()
        {
            _ComponentImageValid = false;
            if (IATImage != null)
                _IATImage.Invalidate();
        }

        protected bool MultipleUpdating = false;

        public void BeginMultipleUpdate()
        {
            Lock();
            MultipleUpdating = true;
        }

        public void EndMultipleUpdate()
        {
            MultipleUpdating = false;
            if (IATImage != null)
                _IATImage.InvalidateSource(this);
            Unlock();
        }

        // the font-family the phrase is to be rendered in
        protected String _PhraseFontFamily;

        private void SetFont(CIATPreferences.CFontPreferences preferences)
        {
                BeginMultipleUpdate();
                PhraseFontFamily = preferences.FontFamily;
                PhraseFontSize = preferences.FontSize;
                PhraseColor = preferences.FontColor;
                EndMultipleUpdate();
        }

        /// <summary>
        /// gets or sets the font family the phrase is to be rendered in
        /// </summary>
        public String PhraseFontFamily
        {
            get
            {
                Lock();
                String str = _PhraseFontFamily;
                Unlock();
                return str;
            }
            set
            {
                Lock();
                BoundingRectInvalidated = true;
                PhraseFontInvalidated = true;
                _PhraseFontFamily = value;
                Invalidate();
                Unlock();
            }
        }

        // the size of the font the phrase is to be rendered in
        protected float _PhraseFontSize;

        /// <summary>
        /// gets or sets the size of the font the phrase is to be rendered in
        /// </summary>
        public float PhraseFontSize
        {
            get
            {
                return _PhraseFontSize;
            }
            set
            {
                Lock();
                BoundingRectInvalidated = true;
                PhraseFontInvalidated = true;
                _PhraseFontSize = value;
                Invalidate();
                Unlock();
            }
        }

        // the color the phrase is to be rendered in
        protected System.Drawing.Color _PhraseColor;

        /// <summary>
        /// gets or sets the color the phrase is to be rendered in
        /// </summary>
        public System.Drawing.Color PhraseColor
        {
            get
            {
                Lock();
                System.Drawing.Color c = _PhraseColor;
                Unlock();
                return c;
            }
            set
            {
                Lock();
                _PhraseColor = value;
                Invalidate();
                Unlock();
            }
        }

        // flags indicating if the phrase font or bounding rectangle have been invalidated
        protected bool PhraseFontInvalidated, BoundingRectInvalidated;

        // the phrase font
        private Font _PhraseFont;
        
        /// <summary>
        /// gets the Font object the phrase is to be rendered in
        /// </summary>
        public Font PhraseFont
        {
            get
            {
                Lock();
                if (_PhraseFont == null)
                    ConstructPhraseFont();
                else if (PhraseFontInvalidated)
                {
                    _PhraseFont.Dispose();
                    ConstructPhraseFont();
                }
                PhraseFontInvalidated = false;
                Font f = _PhraseFont;
                Unlock();
                return f;
            }
        }

        private void ConstructPhraseFont()
        {
            _PhraseFont = new Font(_PhraseFontFamily, _PhraseFontSize * .975F);
        }


        public static bool Equals(CTextDisplayItem a, CTextDisplayItem b)
        {
            if (a.Phrase != b.Phrase)
                return false;
            if (a.PhraseColor != b.PhraseColor)
                return false;
            if (a.PhraseFontSize != b.PhraseFontSize)
                return false;
            if (a.PhraseFontFamily != b.PhraseFontFamily)
                return false;
            return true;
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public CTextDisplayItem(EUsedAs usedAs)
            : base(CDisplayItem.EType.text)
        {
            _Phrase = String.Empty;
            _PhraseFontFamily = String.Empty;
            _PhraseFontSize = -1;
            _PhraseColor = System.Drawing.Color.White;
            _PhraseFont = null;
            PhraseFontInvalidated = true;
            BoundingRectInvalidated = true;
            _BoundingRectangle = new Rectangle(0, 0, 0, 0);
            _UsedAs = usedAs;
            switch (usedAs)
            {
                case EUsedAs.conjunction:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.Conjunction]);
                    break;

                case EUsedAs.continueInstructions:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.ContinueInstructions]);
                    break;

                case EUsedAs.iatBlockInstructions:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.BlockInstructions]);
                    break;

                case EUsedAs.keyedInstructionScreen:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.KeyedInstructions]);
                    break;

                case EUsedAs.mockItemInstructions:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.MockItemInstructions]);
                    break;

                case EUsedAs.responseKey:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextResponseKey]);
                    break;

                case EUsedAs.stimulus:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextStimulus]);
                    break;

                case EUsedAs.textInstructionScreen:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextInstructions]);
                    break;
            }
            _IATImage = CIAT.ImageManager.AddNonUserImage(this);
            if (usedAs == EUsedAs.stimulus)
                _IATImage.CreateThumbnail();
        }

        public CTextDisplayItem(EUsedAs usedAs, CDisplayItem.EType type) : base(type)
        {
            _UsedAs = usedAs;
            _Phrase = String.Empty;
            _PhraseFontFamily = String.Empty;
            _PhraseFontSize = -1;
            _PhraseColor = System.Drawing.Color.White;
            _PhraseFont = null;
            PhraseFontInvalidated = true;
            BoundingRectInvalidated = true;
            _BoundingRectangle = new Rectangle(0, 0, 0, 0);
            _UsedAs = usedAs;
            switch (usedAs)
            {
                case EUsedAs.conjunction:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.Conjunction]);
                    break;

                case EUsedAs.continueInstructions:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.ContinueInstructions]);
                    break;

                case EUsedAs.iatBlockInstructions:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.BlockInstructions]);
                    break;

                case EUsedAs.keyedInstructionScreen:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.KeyedInstructions]);
                    break;

                case EUsedAs.mockItemInstructions:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.MockItemInstructions]);
                    break;

                case EUsedAs.responseKey:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextResponseKey]);
                    break;

                case EUsedAs.stimulus:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextStimulus]);
                    break;

                case EUsedAs.textInstructionScreen:
                    SetFont(CIAT.Preferences.FontPreferences[CIATPreferences.CFontPreferences.EUsedFor.TextInstructions]);
                    break;
            }
            _IATImage = CIAT.ImageManager.AddNonUserImage(this);
            if (usedAs == EUsedAs.stimulus)
                _IATImage.CreateThumbnail();
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="o">The object to be copied</param>
        public CTextDisplayItem(CTextDisplayItem o)
            : base(CDisplayItem.EType.text)
        {
            _Phrase = o.Phrase;
            _PhraseFontFamily = o.PhraseFontFamily;
            _PhraseFontSize = o.PhraseFontSize;
            _PhraseColor = o.PhraseColor;
            _PhraseFont = null;
            PhraseFontInvalidated = true;
            BoundingRectInvalidated = true;
            _BoundingRectangle = new Rectangle(0, 0, 0, 0);
            _UsedAs = o.UsedAs;
            _IATImage = CIAT.ImageManager.AddNonUserImage(this, (INonUserImage)o.IATImage);
        }

        public CTextDisplayItem(CTextDisplayItem o, EType type)
            : base(type)
        {
            _Phrase = o.Phrase;
            _PhraseFontFamily = o.PhraseFontFamily;
            _PhraseFontSize = o.PhraseFontSize;
            _PhraseColor = o.PhraseColor;
            _PhraseFont = null;
            PhraseFontInvalidated = true;
            BoundingRectInvalidated = true;
            _BoundingRectangle = new Rectangle(0, 0, 0, 0);
            _UsedAs = o.UsedAs;
            _IATImage = CIAT.ImageManager.AddNonUserImage(this, (INonUserImage)o.IATImage);
        }


        /// <summary>
        /// Determines if the object's data is valid
        /// </summary>
        /// <returns>"true" if the object contains valid data, otherwise "false"</returns>
        public override bool IsDefined()
        {
            Lock();
            bool bResult;
            if ((PhraseFontFamily == String.Empty) || (PhraseFontSize <= MinPhraseFontSize))
                bResult = false;
            bResult = true;
            Unlock();
            return bResult;
        }


        public override void WriteToXml(XmlTextWriter writer)
        {
            if (!IsDefined())
                throw new Exception();
            writer.WriteStartElement("DisplayItem");
            writer.WriteStartAttribute("Type");
            writer.WriteString(sText);
            writer.WriteEndAttribute();
            writer.WriteElementString("Phrase", Phrase);
            writer.WriteElementString("FontFamily", PhraseFontFamily);
            writer.WriteElementString("FontSize", PhraseFontSize.ToString());
            writer.WriteElementString("ColorName", PhraseColor.Name);
            writer.WriteEndElement();
        }

        /// <summary>
        /// Loads the object's data from an XmlNode
        /// </summary>
        /// <param name="node">The XmlNode object to load data from</param>
        /// <returns>"true" on success, "false" on error</returns>
        public override bool LoadFromXml(XmlNode node)
        {
            // ensure the node has the correct number of child children
            if (node.ChildNodes.Count != 4)
                return false;
            
            // load the phrase, font family, font size, and font color
            _Phrase = node.ChildNodes[0].InnerText;
            _PhraseFontFamily = node.ChildNodes[1].InnerText;
            _PhraseFontSize = Convert.ToSingle(node.ChildNodes[2].InnerText);
            _PhraseColor = System.Drawing.Color.FromName(node.ChildNodes[3].InnerText);
            ((INonUserImage)IATImage).InvalidateNow();
            // success
            return true;
        }


        // stores the previously calculated size of the item
        protected Rectangle _BoundingRectangle;

        /// <summary>
        /// Calculates the size of the item
        /// </summary>
        /// <returns>The size of the item</returns>
        protected override Size GetItemSize()
        {
            return System.Windows.Forms.TextRenderer.MeasureText(Phrase, PhraseFont);
        }
        /*
        public void GetAbsoluteBoundingRectangle()
        {
            Lock();
            if (Prase == String.Empty)
            {
                Unlock();
                return Rectangle.Empty;
            }

            Size szBounds = System.Windows.Forms.TextRenderer.MeasureText(Phrase, PhraseFont);
            Bitmap bmp = new Bitmap(szBounds.Width, szBounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(bmp);
            g.FillRectangle(Brushes.Black, new Rectangle(0, 0, szBounds.Width, szBounds.Height));
            g.DrawString(Phrase, PhraseFont, Brushes.White, new Point(0, 0));
            Rectangle bounds = new Rectangle(new Point(0, 0), szBounds);
            bool LineIsBlank = true;
            int ctr1 = 0, ctr2;
            while ((LineIsBlank) && (ctr1 < bmp.Height))
            {
                for (ctr2 = 0; ctr2 < szBounds.Width; ctr2++)
                    if (bmp.GetPixel(ctr2, ctr1).ToArgb() != System.Drawing.Color.Black.ToArgb())
                    {
                        LineIsBlank = false;
                        bounds.Y += ctr1;
                        bounds.Height -= ctr1;
                        break;
                    }
                ctr1++;
            }
            if (LineIsBlank)
            {
                Unlock();
                return Rectangle.Empty;
            }
            ctr1 = 0;
            LineIsBlank = true;
            while (LineIsBlank)
            {
                for (ctr2 = 0; ctr2 < szBounds.Width; ctr2++)
                    if (bmp.GetPixel(ctr2, szBounds.Height - ctr1 - 1).ToArgb() != System.Drawing.Color.Black.ToArgb())
                    {
                        LineIsBlank = false;
                        bounds.Height -= ctr1;
                        break;
                    }
                ctr1++;
            }
            ctr1 = 0;
            LineIsBlank = true;
            while (LineIsBlank)
            {
                for (ctr2 = 0; ctr2 < szBounds.Height; ctr2++)
                    if (bmp.GetPixel(ctr1, ctr2).ToArgb() != System.Drawing.Color.Black.ToArgb())
                    {
                        LineIsBlank = false;
                        bounds.X += ctr1;
                        bounds.Width -= ctr1;
                        break;
                    }
                ctr1++;
            }
            ctr1 = 0;
            LineIsBlank = true;
            while (LineIsBlank)
            {
                for (ctr2 = 0; ctr2 < szBounds.Height; ctr2++)
                    if (bmp.GetPixel(szBounds.Width - ctr1 - 1, ctr2).ToArgb() != System.Drawing.Color.Black.ToArgb())
                    {
                        LineIsBlank = false;
                        bounds.Width -= ctr1;
                        break;
                    }
                ctr1++;
            }
            bmp.Dispose();
            g.Dispose();
            Unlock();
            return bounds;
        }
        */

        /// <summary>
        /// returns the minimal bounding rectangle of the text display item
        /// </summary>
        /// <returns></returns>
        public virtual Rectangle GetBoundingRectangle()
        {
            Lock();
            if (Phrase == String.Empty)
            {
                _BoundingRectangle = new Rectangle(0, 0, 0, 0);
                BoundingRectInvalidated = false;
                Unlock();
                return Rectangle.Empty;
            }
            if (!BoundingRectInvalidated)
            {
                Unlock();
                return _BoundingRectangle;
            }
            String phrase = Phrase;
            Font phraseFont = new Font(PhraseFontFamily, PhraseFontSize);
            Color fontColor = PhraseColor;
            Size szBounds = System.Windows.Forms.TextRenderer.MeasureText(phrase, phraseFont);
            Bitmap tempBmp = new Bitmap(szBounds.Width, szBounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(tempBmp);
            Brush backBr = new SolidBrush(CIAT.Layout.BackColor);
            g.FillRectangle(backBr, new Rectangle(0, 0, szBounds.Width, szBounds.Height));
            Brush PhraseBrush = new SolidBrush(fontColor);
            g.DrawString(phrase, phraseFont, PhraseBrush, new Point(0, 0));
            PhraseBrush.Dispose();
            backBr.Dispose();
            g.Dispose();
            Image clippedImg = AbsoluteClip(tempBmp, CIAT.Layout.BackColor);
            tempBmp.Dispose();
            _BoundingRectangle = new Rectangle(new Point(0, 0), new Size(clippedImg.Width, clippedImg.Height));
            BoundingRectInvalidated = false;
            clippedImg.Dispose();
            Unlock();
            return _BoundingRectangle;
        }       

        /// <summary>
        /// Displays the item in the given graphics context at the given location
        /// </summary>
        /// <param name="g">The graphics context</param>
        /// <param name="location">The location to display the item</param>
        /// <returns>"true" on success, "false" on error</returns>
        
        /*
        public override bool Display(Graphics g, Point location)
        {
            if (!IsValid())
                return false;
            Brush br = new SolidBrush(PhraseColor);
            g.DrawString(Phrase, PhraseFont, br, location);
            br.Dispose();
            return true;
        }
        */

        public Size GetContainerSize()
        {
            if (UsedAs == EUsedAs.continueInstructions)
                return CIAT.Layout.ContinueInstructionsSize;
            else if (UsedAs == EUsedAs.iatBlockInstructions)
                return CIAT.Layout.InstructionsSize;
            else if (UsedAs == EUsedAs.keyedInstructionScreen)
                return CIAT.Layout.KeyInstructionScreenTextAreaRectangle.Size;
            else if (UsedAs == EUsedAs.responseKey)
                return CIAT.Layout.KeyValueSize;
            else if (UsedAs == EUsedAs.stimulus)
                return CIAT.Layout.StimulusSize;
            else if (UsedAs == EUsedAs.conjunction)
                return CIAT.Layout.KeyValueSize;
            else if (UsedAs == EUsedAs.textInstructionScreen)
                return CIAT.Layout.InstructionScreenTextAreaRectangle.Size;
            else if (UsedAs == EUsedAs.mockItemInstructions)
                return CIAT.Layout.MockItemInstructionsRectangle.Size;
            else
                throw new Exception("Cannot retrieve bounding rectangle for text display item");
        }

        protected Image AbsoluteClip(Image img, Color backColor)
        {
            Bitmap bmp = new Bitmap(img);
            System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, img.Width, img.Height), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            IntPtr bmpPtr = bmpData.Scan0;
            byte[] bmpBytes = new byte[Math.Abs(bmpData.Stride * bmpData.Height)];
            System.Runtime.InteropServices.Marshal.Copy(bmpPtr, bmpBytes, 0, bmpBytes.Length);
            byte[] backColorBytes = new byte[] { backColor.R, backColor.G, backColor.B, backColor.A };
            int top = 0, left = 0, bottom = img.Height, right = img.Width;
            int ctr = 0;
            while ((bmpBytes[ctr] ^ backColorBytes[0]) + (bmpBytes[ctr + 1] ^ backColorBytes[1]) + (bmpBytes[ctr + 2] ^ backColorBytes[2]) + (bmpBytes[ctr + 3] ^ backColorBytes[3]) == 0)
            {
                ctr += 4;
                if (ctr % bmpData.Stride == 0)
                    top++;
            }
            ctr = bmpBytes.Length - bmpData.Stride;
            while ((bmpBytes[ctr] ^ backColorBytes[0]) + (bmpBytes[ctr + 1] ^ backColorBytes[1]) + (bmpBytes[ctr + 2] ^ backColorBytes[2]) + (bmpBytes[ctr + 3] ^ backColorBytes[3]) == 0)
            {
                ctr += 4;
                if (ctr % bmpData.Stride == 0)
                {
                    bottom--;
                    ctr -= bmpData.Stride * 2;
                }
            }
            ctr = 0;
            while ((bmpBytes[ctr] ^ backColorBytes[0]) + (bmpBytes[ctr + 1] ^ backColorBytes[1]) + (bmpBytes[ctr + 2] ^ backColorBytes[2]) + (bmpBytes[ctr + 3] ^ backColorBytes[3]) == 0)
            {
                ctr += bmpData.Stride;
                if (ctr >= bmpBytes.Length)
                    ctr = ++left << 2;
            }
            ctr = bmpData.Stride - 4;
            while ((bmpBytes[ctr] ^ backColorBytes[0]) + (bmpBytes[ctr + 1] ^ backColorBytes[1]) + (bmpBytes[ctr + 2] ^ backColorBytes[2]) + (bmpBytes[ctr + 3] ^ backColorBytes[3]) == 0)
            {
                ctr += bmpData.Stride;
                if (ctr >= bmpBytes.Length)
                {
                    ctr = bmpData.Stride - ((img.Width - --right) << 2);
                }
            }
            bmp.UnlockBits(bmpData);
            MemoryStream bmpDataStream = new MemoryStream();
            for (ctr = top; ctr < bottom; ctr++)
                bmpDataStream.Write(bmpBytes, ctr * bmpData.Stride + (left << 2), (right - left) << 2);
            bmp.Dispose();
            Bitmap result = new Bitmap(right - left, bottom - top, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            System.Drawing.Imaging.BitmapData resultData = result.LockBits(new Rectangle(0, 0, result.Width, result.Height), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            byte[] resultBytes = bmpDataStream.ToArray();
            IntPtr resultBmpPtr = resultData.Scan0;
            System.Runtime.InteropServices.Marshal.Copy(resultBytes, 0, resultBmpPtr, resultBytes.Length);
            result.UnlockBits(resultData);
            return result;
        }

    

        public virtual Image GenerateImage()
        {
            Lock();
            String phrase = Phrase;
            if (phrase.Trim() == String.Empty)
            {
                Bitmap emptyImg = new Bitmap(1, 1, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
                emptyImg.SetPixel(0, 0, CIAT.Layout.BackColor);
                Unlock();
                return emptyImg;
            }
            Font phraseFont = new Font(PhraseFontFamily, PhraseFontSize);
            Color fontColor = PhraseColor;
            Size szBounds = System.Windows.Forms.TextRenderer.MeasureText(phrase, phraseFont);
            Bitmap tempBmp = new Bitmap(szBounds.Width, szBounds.Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
            Graphics g = Graphics.FromImage(tempBmp);
            Brush backBr = new SolidBrush(CIAT.Layout.BackColor);
            g.FillRectangle(backBr, new Rectangle(0, 0, szBounds.Width, szBounds.Height));
            Brush PhraseBrush = new SolidBrush(fontColor);
            g.DrawString(phrase, phraseFont, PhraseBrush, new Point(0, 0));
            PhraseBrush.Dispose();
            backBr.Dispose();
            g.Dispose();
            Image clippedImg = AbsoluteClip(tempBmp, CIAT.Layout.BackColor);
            tempBmp.Dispose();
            _BoundingRectangle = new Rectangle(new Point(0, 0), new Size(clippedImg.Width, clippedImg.Height));
            BoundingRectInvalidated = false;
            Unlock();
            return clippedImg;
        }

        public Image TryGenerateImage()
        {
            if (!TryLock())
                return null;
            Image img = GenerateImage();
            _ComponentImageValid = false;
            Unlock();
            return img;
        }

        /// <summary>
        /// Displays a the object with the given font and brush
        /// </summary>
        /// <param name="g">The graphics object</param>
        /// <param name="f">The font</param>
        /// <param name="br">The brush</param>
        /// <param name="location">The location of the upper left hand corner</param>
        /// <returns>"true" on success, otherwise false</returns>
        /*
        public bool Display(Graphics g, Font f, Brush br, Point location)
        {
            if (!IsValid())
                return false;
            g.DrawString(Phrase, f, br, location);
            return true;
        }
        */
        /// <summary>
        /// Displays the object as a stimulus
        /// </summary>
        /// <param name="g">The graphics context</param>
        /// <param name="bounds">The bounding rectangle</param>
        /// <returns>"true" on success, otherwise "false"</returns>
        /// 
        /*
        public bool DisplayAsStimulus(Graphics g)
        {
            if (!IsValid())
                return false;
            Point ptDraw = new Point(CIAT.Layout.StimulusRectangle.Left + ((CIAT.Layout.StimulusSize.Width - ItemSize.Width) >> 1), 
                CIAT.Layout.StimulusRectangle.Top + CIAT.Layout.TextStimulusPaddingTop);
            Brush br = new SolidBrush(PhraseColor);
            g.DrawString(Phrase, PhraseFont, br, ptDraw);
            br.Dispose();
            return true;
        }
        */
        public Bitmap GenerateStimulusImage()
        {
            Lock();
            Bitmap theImage = new Bitmap(CIAT.Layout.StimulusRectangle.Width, CIAT.Layout.StimulusRectangle.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(theImage);
            Brush backBrush = new SolidBrush(CIAT.Layout.BackColor);
            g.FillRectangle(backBrush, new Rectangle(0, 0, theImage.Width - 1, theImage.Height - 1));
            backBrush.Dispose();
            Brush textBrush = new SolidBrush(PhraseColor);
            SizeF szPhrase = g.MeasureString(Phrase, PhraseFont);
            g.DrawString(Phrase, PhraseFont, textBrush, new PointF((theImage.Width - szPhrase.Width) / 2, (theImage.Height - szPhrase.Height) / 2));
            textBrush.Dispose();
            g.Dispose();
            Unlock();
            return theImage;
        }
        
        /// <summary>
        /// Disposes of the font if necessary
        /// </summary>
        public override void Dispose()
        {
            Lock();
            base.Dispose();
            if (_PhraseFont != null)
                _PhraseFont.Dispose();
            Unlock();
        }
    }
}
