using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Xml;
using System.Threading;

namespace IATClient
{
    public class CMultiLineTextDisplayItem : CTextDisplayItem, IStoredInXml, IDisposable
    {

        /// <summary>
        /// An enumeration of the various text-justifications
        /// </summary>
        public enum EJustification { left, right, center };

        private float LinePadding
        {
            get
            {
                return PhraseFontSize / 3.14159F;
            }
        }



        // the text justifiaction
        private EJustification _TextJustification;

        public EJustification TextJustification
        {
            get
            {
                return _TextJustification;
            }
            set
            {
                lock (LockObject)
                {
                    _TextJustification = value;
                    if (!MultipleUpdating)
                    {
                        ((INonUserImage)IATImage).Invalidate();
                        _ComponentImageValid = false;
                    }
                }
            }
        }

        // the line spacing of the multi line text display object
        private float _LineSpacing;

        /// <summary>
        /// gets or sets the line spacing
        /// </summary>
        public float LineSpacing
        {
            get
            {
                return _LineSpacing;
            }
            set
            {
                Lock();
                _LineSpacing = value;
                if (!MultipleUpdating)
                {
                    ((INonUserImage)IATImage).Invalidate();
                    _ComponentImageValid = false;
                }
                Unlock();
            }
        }

        public override Rectangle GetBoundingRectangle()
        {
            switch (UsedAs)
            {
                case EUsedAs.iatBlockInstructions:
                    return CIAT.Layout.InstructionsRectangle;

                case EUsedAs.keyedInstructionScreen:
                    return CIAT.Layout.KeyInstructionScreenTextAreaRectangle;

                case EUsedAs.mockItemInstructions:
                    return CIAT.Layout.MockItemInstructionsRectangle;

                case EUsedAs.textInstructionScreen:
                    return CIAT.Layout.InstructionScreenTextAreaRectangle;

                default:
                    throw new Exception("Unrecognized multi-line display item type encountered in CMultiLineTextDisplayItem::GetBoundingRectangle");
            }
        }

        /// <summary>
        /// The default constructor
        /// </summary>
        public CMultiLineTextDisplayItem(EUsedAs usedAs) : base(usedAs, EType.multiLineTextDisplayItem)
        {
            _TextJustification = EJustification.center;
            _LineSpacing = 1;
        }

        /// <summary>
        /// The copy constructor
        /// </summary>
        /// <param name="o">The object to be copied</param>
        public CMultiLineTextDisplayItem(CMultiLineTextDisplayItem o) : base(o, EType.multiLineTextDisplayItem)
        {
            _TextJustification = o.TextJustification;
            _LineSpacing = o.LineSpacing;
        }

        /// <summary>
        /// Gets the index where a line should be wrapped in the Instruction boundary
        /// </summary>
        /// <param name="str">The string that might need to be wrapped</param>
        /// <param name="font">The font the string is to be rendered in</param>
        /// <returns>either the index where the string should be truncated, -1 if the string needs no truncation, or 0 if
        /// the string contains no whitespace characters prior to the truncation point</returns>
        private int GetLineWrapIndex(Graphics g, String str, Font font)
        {
            int nWidth = GetBoundingRectangle().Width;
            SizeF sz = g.MeasureString(str, font);
            if (sz.Width < nWidth)
                return -1;
            int ctr = 0;
            int strLen = str.Length;
            // find the truncation point
            while (g.MeasureString(str.Substring(0, ++ctr), font).Width < nWidth);

            // look for white space
            while ((!Char.IsWhiteSpace(str[--ctr])) && (ctr > 0));
            if (ctr == 0)
                return 0;
            
            // look for end of white space
            while ((Char.IsWhiteSpace(str[--ctr])) && (ctr > 0));
            if (ctr == 0)
                return 0;

            // store found wrap index
            int nWrapIndex = ctr + 1;

            // look for preceding linebreaks
            ctr = 0;
            while (ctr < nWrapIndex)
                if ((str[ctr] == '\n') || (str[ctr] == '\r'))
                    return ctr;
                else
                    ctr++;

            return nWrapIndex;
        }

        /// <summary>
        /// Gets the point where a string should be terminated, heedless of wrapping between words
        /// </summary>
        /// <param name="str">The string to be truncated</param>
        /// <param name="font">The font the string is to be rendered in</param>
        /// <returns>The index of the last character in the string</returns>
        private int GetTruncationIndex(String str, Font font)
        {
            int nWidth = GetBoundingRectangle().Width;
            int ctr = 0;
            int strLen = str.Length;
            while (System.Windows.Forms.TextRenderer.MeasureText(str.Substring(0, ++ctr), font).Width < nWidth);
            return ctr - 1;
        }


        protected override Size GetItemSize()
        {
            switch (UsedAs)
            {
                case EUsedAs.iatBlockInstructions:
                    return CIAT.Layout.InstructionsSize;

                case EUsedAs.keyedInstructionScreen:
                    return CIAT.Layout.KeyInstructionScreenTextAreaRectangle.Size;

                case EUsedAs.mockItemInstructions:
                    return CIAT.Layout.MockItemInstructionsRectangle.Size;

                case EUsedAs.textInstructionScreen:
                    return CIAT.Layout.InstructionScreenTextAreaRectangle.Size;
            }
            throw new Exception("Unexpected multi-line text display item type encountered.");
        }

        public override Image GenerateImage()
        {
            Lock();
            Image img = new Bitmap(GetItemSize().Width, GetItemSize().Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(img);
            Brush backBr = new SolidBrush(CIAT.Layout.BackColor);
            g.FillRectangle(backBr, new Rectangle(0, 0, img.Width, img.Height));
            backBr.Dispose();
            DisplayLines(g);
            g.Dispose();
            Unlock();
            return img;
        }

        /// <summary>
        /// Parses the text of Instructions into CTextDisplayItem objects and adds them to LinesOfText
        /// </summary>
        private List<String> ParseText(Graphics g)
        {
            List<String> LineList = new List<String>();
            String str = Phrase;
            char []delims = {'\n', '\r'};
            string[] lines = str.Split(delims, StringSplitOptions.RemoveEmptyEntries);
            for (int ctr = 0; ctr < lines.Length; ctr++)
            {
                int nLength;
                Font font = PhraseFont;
                while ((nLength = GetLineWrapIndex(g, lines[ctr], font)) != -1)
                {
                    if (nLength == 0)
                        nLength = GetTruncationIndex(lines[ctr], font);
                    LineList.Add(lines[ctr].Substring(0, nLength));
                    lines[ctr] = lines[ctr].Substring(nLength);
                    lines[ctr] = lines[ctr].TrimStart();
                }
                if (lines[ctr] != String.Empty)
                {
                    LineList.Add(lines[ctr]);
                }
            }
            return LineList;
        }
        
        private void DisplayLines(Graphics g)
        {
            if (Phrase == String.Empty)
                return;
            List<String> ParsedInstructions = ParseText(g);
            Brush br = new SolidBrush(PhraseColor);
            PointF ptDraw = new PointF(0, 0);
            SizeF szText;
            for (int ctr = 0; ctr < ParsedInstructions.Count; ctr++)
            {
                szText = g.MeasureString(ParsedInstructions[ctr], PhraseFont);
                if (TextJustification == EJustification.center)
                    ptDraw.X = ((GetItemSize().Width - szText.Width) / 2);
                else if (TextJustification == EJustification.left)
                    ptDraw.X = 0;
                else if (TextJustification == EJustification.right)
                    ptDraw.X = GetItemSize().Width - szText.Width;
                g.DrawString(ParsedInstructions[ctr], PhraseFont, br, ptDraw);
                ptDraw.Y += LineSpacing * PhraseFont.GetHeight();
            }
            br.Dispose();
        }

        /// <summary>
        /// Writes the object's data to an XmlTextWriter
        /// </summary>
        /// <param name="writer">The XmlTextWriter object to use for output</param>
        public override void WriteToXml(XmlTextWriter writer)
        {
            writer.WriteStartElement("IATMultiLineTextDisplayItem");
            writer.WriteElementString("Text", Phrase);
            writer.WriteElementString("FontFamily", PhraseFontFamily);
            writer.WriteElementString("FontSize", PhraseFontSize.ToString());
            writer.WriteElementString("TextColorName", PhraseColor.Name);
            writer.WriteElementString("TextJustification", ((int)TextJustification).ToString());
            writer.WriteElementString("LineSpacing", LineSpacing.ToString());
            writer.WriteEndElement();
        }

        /// <summary>
        /// Loads the object's data from the passed XmlNode
        /// </summary>
        /// <param name="node">The XmlNode object to load data from</param>
        /// <returns>"true" on success, "false" otherwise</returns>
        public override bool LoadFromXml(XmlNode node)
        {
            // check the node name
            if (node.Name != "IATMultiLineTextDisplayItem")
                return false;

            if (node.ChildNodes.Count != 6)
                return false;

            _Phrase = node.ChildNodes[0].InnerText;
            _PhraseFontFamily = node.ChildNodes[1].InnerText;
            _PhraseFontSize = Convert.ToSingle(node.ChildNodes[2].InnerText);
            _PhraseColor = System.Drawing.Color.FromName(node.ChildNodes[3].InnerText);
            _TextJustification = (EJustification)Convert.ToInt32(node.ChildNodes[4].InnerText);
            _LineSpacing = Convert.ToSingle(node.ChildNodes[5].InnerText);
            PhraseFontInvalidated = true;
            ((INonUserImage)IATImage).InvalidateNow();
//            ConstructInstructionsImage();
            // success
            return true;
        }
    }
}
