using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace IATClient
{
    public partial class LayoutPanel : UserControl
    {
        protected abstract class CComboBoxItem
        {
            public abstract Size Measure();
            public abstract void Draw(Graphics g, Rectangle BoundingRect, System.Drawing.Color BackColor, System.Drawing.Color ForeColor);
        }

        protected class CStringItem : CComboBoxItem
        {
            private String _Text;
            private bool bSizeValid;
            private Size ItemSize;
            private Font _ItemFont;
            private bool _IsSelectable;

            public String Text
            {
                get
                {
                    return _Text;
                }
                set
                {
                    _Text = value;
                    InvalidateSize();
                }
            }

            public Font ItemFont
            {
                get
                {
                    return _ItemFont;
                }
                set
                {
                    _ItemFont = value;
                    InvalidateSize();
                }
            }

            public bool IsSelectable
            {
                get
                {
                    return _IsSelectable;
                }
                set
                {
                    _IsSelectable = value;
                }
            }

            private void InvalidateSize()
            {
                bSizeValid = false;
            }

            public CStringItem()
            {
                _Text = String.Empty;
                bSizeValid = false;
                ItemSize = new Size(0, 0);
                _IsSelectable = true;
                _ItemFont = null;
            }

            public CStringItem(String str, Font f, bool isSelectable)
            {
                _Text = str;
                _ItemFont = f;
                _IsSelectable = isSelectable;
                bSizeValid = false;
                ItemSize = new Size(0, 0);
            }

            public CStringItem(String str, Font f)
            {
                _Text = str;
                _ItemFont = f;
                _IsSelectable = true;
                bSizeValid = false;
                ItemSize = new Size(0, 0);
            }

            public override Size Measure()
            {
                if (bSizeValid)
                    return ItemSize;
                if (ItemFont == null)
                    return new Size(0, 0);
                ItemSize = System.Windows.Forms.TextRenderer.MeasureText(Text, ItemFont);
                bSizeValid = true;
                ItemSize += new Size(ItemFont.Height, ItemFont.Height >> 2);
                return ItemSize;
            }

            public override void Draw(Graphics g, Rectangle BoundingRect, System.Drawing.Color BackColor, System.Drawing.Color ForeColor)
            {
                if (IsSelectable)
                {
                    Brush BackBrush = new SolidBrush(BackColor);
                    Brush ForeBrush = new SolidBrush(ForeColor);
                    g.FillRectangle(BackBrush, BoundingRect);
                    g.DrawString(Text, ItemFont, ForeBrush, BoundingRect.Location
                        + new Size(ItemFont.Height >> 1, ItemFont.Height >> 3));
                    BackBrush.Dispose();
                    ForeBrush.Dispose();
                }
                else
                {
                    Brush BackBrush = new SolidBrush(BackColor);
                    g.FillRectangle(BackBrush, BoundingRect);
                    g.DrawString(Text, ItemFont, System.Drawing.SystemBrushes.GrayText, BoundingRect.Location
                        + new Size(ItemFont.Height >> 1, ItemFont.Height >> 3));
                    BackBrush.Dispose();
                }
            }

            public override string ToString()
            {
                return Text;
            }
        }

        protected class CColorItem : CComboBoxItem
        {
            public static System.Drawing.Color[] Colors = { System.Drawing.Color.Azure, System.Drawing.Color.Beige, System.Drawing.Color.Black, System.Drawing.Color.Blue, System.Drawing.Color.BlueViolet, System.Drawing.Color.Brown,
                                         System.Drawing.Color.Chartreuse, System.Drawing.Color.Chocolate, System.Drawing.Color.CornflowerBlue, System.Drawing.Color.Crimson,
                                         System.Drawing.Color.Cyan, System.Drawing.Color.ForestGreen, System.Drawing.Color.Fuchsia, System.Drawing.Color.Gold, System.Drawing.Color.Gray,
                                         System.Drawing.Color.Green, System.Drawing.Color.GreenYellow, System.Drawing.Color.HotPink, System.Drawing.Color.Indigo, System.Drawing.Color.Ivory,
                                         System.Drawing.Color.Lavender, System.Drawing.Color.LightYellow, System.Drawing.Color.LimeGreen, System.Drawing.Color.Navy, System.Drawing.Color.Orange, System.Drawing.Color.OrangeRed,
                                         System.Drawing.Color.Pink, System.Drawing.Color.PowderBlue, System.Drawing.Color.Purple, System.Drawing.Color.Red, System.Drawing.Color.SeaGreen,
                                         System.Drawing.Color.Silver, System.Drawing.Color.Transparent, System.Drawing.Color.White, System.Drawing.Color.Yellow };

            private System.Drawing.Color _ItemColor;
            private Font _ItemFont;
            private Size ItemSize;
            private bool bSizeValid, bColorRectValid;
            private int VerticalPadding;
            protected const double ColorRectAspectRatio = 4.0 / 3.0;
            protected Image ColorRect;

            public System.Drawing.Color ItemColor
            {
                get
                {
                    return _ItemColor;
                }
                set
                {
                    _ItemColor = value;
                    InvalidateColorRect();
                }
            }

            public Font ItemFont
            {
                get
                {
                    return _ItemFont;
                }
                set
                {
                    _ItemFont = value;
                    VerticalPadding = (int)(value.Height * .20);
                    InvalidateItemSize();
                    InvalidateColorRect();
                }
            }

            private void InvalidateItemSize()
            {
                bSizeValid = false;
            }

            private void InvalidateColorRect()
            {
                if (ColorRect != null)
                    ColorRect.Dispose();
                ColorRect = null;
                bColorRectValid = false;
            }

            protected void GenerateColorRect()
            {
                if (ItemFont == null)
                    return;
                ColorRect = new Bitmap((int)(ItemFont.Height * ColorRectAspectRatio), ItemFont.Height,
                    System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                Graphics g = Graphics.FromImage(ColorRect);
                Rectangle BoundaryRect = new Rectangle(new Point(0, 0), new Size((int)(ItemFont.Height * ColorRectAspectRatio), ItemFont.Height));
                g.DrawRectangle(Pens.Black, BoundaryRect);
                BoundaryRect.Inflate(-1, -1);
                Brush br = new SolidBrush(ItemColor);
                g.FillRectangle(br, BoundaryRect);
                br.Dispose();
                g.Dispose();
                bColorRectValid = true;
            }

            public CColorItem()
            {
                _ItemColor = System.Drawing.Color.Black;
                _ItemFont = null;
                ItemSize = new Size(0, 0);
                VerticalPadding = 0;
                bColorRectValid = bSizeValid = false;
                ColorRect = null;
            }

            public CColorItem(System.Drawing.Color c, Font f)
            {
                VerticalPadding = 0;
                _ItemColor = c;
                ItemFont = f;
                ItemSize = new Size(0, 0);
                bSizeValid = false;
                GenerateColorRect();
            }

            public override Size Measure()
            {
                if (!bSizeValid)
                {
                    Size TextSize = System.Windows.Forms.TextRenderer.MeasureText(ItemColor.Name, ItemFont);
                    ItemSize = TextSize;
                    ItemSize.Width += ColorRect.Width + ItemFont.Height;
                    ItemSize.Height += VerticalPadding << 1;
                    bSizeValid = true;
                }
                return ItemSize;
            }

            public override void Draw(Graphics g, Rectangle BoundingRect, System.Drawing.Color BackColor, System.Drawing.Color ForeColor)
            {
                Brush BackBrush = new SolidBrush(BackColor);
                Brush ForeBrush = new SolidBrush(ForeColor);
                g.FillRectangle(BackBrush, BoundingRect);
                g.DrawString(ItemColor.Name, ItemFont, ForeBrush, BoundingRect.Location + new Size((ItemFont.Height >> 1) + ColorRect.Width, VerticalPadding));
                g.DrawImage(ColorRect, BoundingRect.Location + new Size(0, VerticalPadding));
                BackBrush.Dispose();
                ForeBrush.Dispose();
            }

            public override string ToString()
            {
                return ItemColor.Name;
            }
        }

        private const int InteriorHeightMinValue = 200;
        private const int InteriorWidthMinValue = 200;
        private const int InteriorHeightMaxValue = 1000;
        private const int InteriorWidthMaxValue = 1200;
        private const int InteriorHeightIncrement = 25;
        private const int InteriorWidthIncrement = 25;
        private const double StimulusWidthMinFraction = 1.0 / 3.0;
        private const double StimulusHeightMinFraction = 1.0 / 3.0;
        private const double StimulusWidthMaxFraction = 3.0 / 2.0;
        private const double StimulusHeightMaxFraction = 3.0 / 2.0;
        private const int StimulusWidthIncrement = 10;
        private const int StimulusHeightIncrement = 10;
        private const double ResponseKeyWidthMinFraction = 1.0 / 10.0;
        private const double ResponseKeyHeightMinFraction = 1.0 / 10.0;
        private const double ResponseKeyWidthMaxFraction = 1.0 / 1.5;
        private const double ResponseKeyHeightMaxFraction = 1.0 / 2.0;
        private const int ResponseKeyWidthIncrement = 10;
        private const int ResponseKeyHeightIncrement = 10;
        private const double InstructionsWidthMinFraction = 1.0 / 3.0;
        private const double InstructionsHeightMinFraction = 1.0 / 10.0;
        private const double InstructionsWidthMaxFraction = 9.5 / 10.0;
        private const double InstructionsHeightMaxFraction = 2.0 / 5.0;
        private const int InstructionsWidthIncrement = 10;
        private const int InstructionsHeightIncrement = 5;
        private const double ErrorMarkWidthMinFraction = 1.0 / 15.0;
        private const double ErrorMarkHeightMinFraction = 1.0 / 15.0;
        private const double ErrorMarkWidthMaxFraction = 1.0 / 5.0;
        private const double ErrorMarkHeightMaxFraction = 1.0 / 5.0;
        private const int ErrorMarkWidthIncrement = 5;
        private const int ErrorMarkHeightIncrement = 5;
        public static Size IATLayoutPanelSize = new Size(787, 505);
        private const int TickGap = 10;
        private const int TickLength = 3;
        protected Font ComboBoxFont;
        protected bool UpdatingFromCode;
        protected CIATLayout LocalLayout;
        protected Graphics OutlineGraphics;
        protected float fScaleFactor;
        protected SolidBrush LayoutBrush, LayoutOverlapBrush;
        protected Pen LayoutPen, LayoutOverlapPen;
        protected Image LayoutImage;

        private IATConfigMainForm MainForm
        {
            get
            {
                if (Parent == null)
                    return null;
                else return (IATConfigMainForm)Parent;
            }
        }


        public Size InteriorSize
        {
            get
            {
                Size sz = new Size();
                sz.Width = Convert.ToInt32(((CStringItem)InteriorWidth.SelectedItem).Text);
                sz.Height = Convert.ToInt32(((CStringItem)InteriorHeight.SelectedItem).Text);
                return sz;
            }
        }

        public CIATLayout FinalLayout
        {
            get
            {
                return LocalLayout;
            }
        }

        private void InitSizeCombos()
        {
            // init interior size combos
            UpdatingFromCode = true;
            InteriorWidth.DrawMode = DrawMode.OwnerDrawFixed;
            InteriorWidth.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            InteriorWidth.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            InteriorWidth.SelectedIndexChanged += new EventHandler(LayoutPanel_InteriorSizeChanged);
            InteriorHeight.DrawMode = DrawMode.OwnerDrawVariable;
            InteriorHeight.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            InteriorHeight.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            InteriorHeight.SelectedIndexChanged += new EventHandler(LayoutPanel_InteriorSizeChanged);
            CStringItem strItem;
            int n = InteriorWidthMinValue;
            while (n <= InteriorWidthMaxValue)
            {
                strItem = new CStringItem(n.ToString(), ComboBoxFont, true);
                InteriorWidth.Items.Add(strItem);
                if (n == LocalLayout.InteriorSize.Width)
                    InteriorWidth.SelectedItem = strItem;
                n += InteriorWidthIncrement;
            }
            n = InteriorHeightMinValue;
            while (n <= InteriorHeightMaxValue)
            {
                strItem = new CStringItem(n.ToString(), ComboBoxFont, true);
                InteriorHeight.Items.Add(strItem);
                if (n == LocalLayout.InteriorSize.Height)
                    InteriorHeight.SelectedItem = strItem;
                n += InteriorHeightIncrement;
            }

            StimulusWidth.DrawMode = DrawMode.OwnerDrawVariable;
            StimulusWidth.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            StimulusWidth.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            StimulusWidth.SelectedIndexChanged += new EventHandler(LayoutPanel_StimulusSizeChanged);
            StimulusHeight.DrawMode = DrawMode.OwnerDrawVariable;
            StimulusHeight.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            StimulusHeight.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            StimulusHeight.SelectedIndexChanged += new EventHandler(LayoutPanel_StimulusSizeChanged);
            ResponseKeyWidth.DrawMode = DrawMode.OwnerDrawVariable;
            ResponseKeyWidth.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            ResponseKeyWidth.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            ResponseKeyWidth.SelectedIndexChanged += new EventHandler(LayoutPanel_ResponseKeySizeChanged);
            ResponseKeyHeight.DrawMode = DrawMode.OwnerDrawVariable;
            ResponseKeyHeight.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            ResponseKeyHeight.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            ResponseKeyHeight.SelectedIndexChanged += new EventHandler(LayoutPanel_ResponseKeySizeChanged);
            InstructionsWidth.DrawMode = DrawMode.OwnerDrawVariable;
            InstructionsWidth.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            InstructionsWidth.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            InstructionsWidth.SelectedIndexChanged += new EventHandler(LayoutPanel_InstructionsSizeChanged);
            InstructionsHeight.DrawMode = DrawMode.OwnerDrawVariable;
            InstructionsHeight.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            InstructionsHeight.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            InstructionsHeight.SelectedIndexChanged += new EventHandler(LayoutPanel_InstructionsSizeChanged);
            ErrorMarkWidth.DrawMode = DrawMode.OwnerDrawVariable;
            ErrorMarkWidth.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            ErrorMarkWidth.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            ErrorMarkWidth.SelectedIndexChanged += new EventHandler(LayoutPanel_ErrorMarkSizeChanged);
            ErrorMarkHeight.DrawMode = DrawMode.OwnerDrawVariable;
            ErrorMarkHeight.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            ErrorMarkHeight.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            ErrorMarkHeight.SelectedIndexChanged += new EventHandler(LayoutPanel_ErrorMarkSizeChanged);
            FillSizeCombos();
            SetSizeComboInitValues();
            CalcAllowableComboValues();
            UpdatingFromCode = false;
        }


        private void FillSizeCombo(ComboBox combo, double minValue, double maxValue, int increment)
        {
            CStringItem strItem;
            int currentValue;
            if (combo.SelectedItem != null)
            {
                currentValue = Convert.ToInt32(((CStringItem)combo.SelectedItem).Text);
                combo.Items.Clear();
            }
            else currentValue = int.MaxValue;
            int nMinValue = ((int)(minValue / increment)) * increment;
            int nMaxValue = ((int)(maxValue / increment)) * increment;
            int n = nMinValue;
            while (n <= nMaxValue)
            {
                strItem = new CStringItem(n.ToString(), ComboBoxFont, true);
                combo.Items.Add(strItem);
                n += increment;
            }
            if (currentValue != int.MaxValue)
                SetComboValue(combo, currentValue);
        }

        private void FillSizeCombos()
        {
            Size sz = InteriorSize;
            FillSizeCombo(StimulusWidth, sz.Width * StimulusWidthMinFraction, sz.Width * StimulusWidthMaxFraction, StimulusWidthIncrement);
            FillSizeCombo(StimulusHeight, sz.Height * StimulusHeightMinFraction, sz.Height * StimulusHeightMaxFraction, StimulusHeightIncrement);
            FillSizeCombo(ResponseKeyWidth, sz.Width * ResponseKeyWidthMinFraction, sz.Width * ResponseKeyWidthMaxFraction, ResponseKeyWidthIncrement);
            FillSizeCombo(ResponseKeyHeight, sz.Height * ResponseKeyHeightMinFraction, sz.Height * ResponseKeyHeightMaxFraction, ResponseKeyHeightIncrement);
            FillSizeCombo(InstructionsWidth, sz.Width * InstructionsWidthMinFraction, sz.Width * InstructionsWidthMaxFraction, InstructionsWidthIncrement);
            FillSizeCombo(InstructionsHeight, sz.Height * InstructionsHeightMinFraction, sz.Height * InstructionsHeightMaxFraction, InstructionsHeightIncrement);
            FillSizeCombo(ErrorMarkWidth, sz.Width * ErrorMarkWidthMinFraction, sz.Width * ErrorMarkWidthMaxFraction, ErrorMarkWidthIncrement);
            FillSizeCombo(ErrorMarkHeight, sz.Height * ErrorMarkHeightMinFraction, sz.Height * ErrorMarkHeightMaxFraction, ErrorMarkHeightIncrement);
        }

        private void InitColorCombos()
        {
            BackgroundColor.DrawMode = DrawMode.OwnerDrawVariable;
            BackgroundColor.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            BackgroundColor.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            BorderColor.DrawMode = DrawMode.OwnerDrawVariable;
            BorderColor.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            BorderColor.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            OutlineColor.DrawMode = DrawMode.OwnerDrawVariable;
            OutlineColor.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            OutlineColor.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            WebPageColorDrop.DrawMode = DrawMode.OwnerDrawVariable;
            WebPageColorDrop.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            WebPageColorDrop.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            for (int ctr = 0; ctr < CColorItem.Colors.Length; ctr++)
            {
                CColorItem ci = new CColorItem(CColorItem.Colors[ctr], System.Drawing.SystemFonts.DialogFont);
                BorderColor.Items.Add(ci);
                if (LocalLayout.BorderColor.Name == ci.ToString())
                    BorderColor.SelectedItem = ci;
                ci = new CColorItem(CColorItem.Colors[ctr], System.Drawing.SystemFonts.DialogFont);
                BackgroundColor.Items.Add(ci);
                if (LocalLayout.BackColor.Name == ci.ToString())
                    BackgroundColor.SelectedItem = ci;
                ci = new CColorItem(CColorItem.Colors[ctr], System.Drawing.SystemFonts.DialogFont);
                OutlineColor.Items.Add(ci);
                if (LocalLayout.OutlineColor.Name == ci.ToString())
                    OutlineColor.SelectedItem = ci;
                ci = new CColorItem(CColorItem.Colors[ctr], System.Drawing.SystemFonts.DialogFont);
                WebPageColorDrop.Items.Add(ci);
                if (LocalLayout.WebpageBackColor.Name == ci.ToString())
                    WebPageColorDrop.SelectedItem = ci;
            }
        }

        private void SetComboValue(ComboBox combo, int value)
        {
            int thisVal, nextVal;
            bool bValSet = false;
            thisVal = Convert.ToInt32(((CStringItem)combo.Items[0]).Text);
            if (value < thisVal)
            {
                combo.SelectedIndex = 0;
                return;
            }
            for (int ctr = 0; ctr < combo.Items.Count - 1; ctr++)
            {
                nextVal = Convert.ToInt32(((CStringItem)combo.Items[ctr + 1]).Text);
                if ((value >= thisVal) && (value < nextVal))
                {
                    combo.SelectedIndex = ctr;
                    bValSet = true;
                    break;
                }
                thisVal = nextVal;
            }
            if (!bValSet)
                combo.SelectedIndex = combo.Items.Count - 1;
        }

        private void SetSizeComboInitValues()
        {
            SetComboValue(StimulusWidth, LocalLayout.StimulusSize.Width);
            SetComboValue(StimulusHeight, LocalLayout.StimulusSize.Height);
            SetComboValue(ResponseKeyWidth, LocalLayout.KeyValueSize.Width);
            SetComboValue(ResponseKeyHeight, LocalLayout.KeyValueSize.Height);
            SetComboValue(InstructionsWidth, LocalLayout.InstructionsSize.Width);
            SetComboValue(InstructionsHeight, LocalLayout.InstructionsSize.Height);
            SetComboValue(ErrorMarkWidth, LocalLayout.ErrorSize.Width);
            SetComboValue(ErrorMarkHeight, LocalLayout.ErrorSize.Height);
        }

        private void SetMaxAllowedComboValue(ComboBox combo, int value)
        {
            int n;
            CStringItem strItem;
            bool ValTooBig = false;
            for (int ctr = 0; ctr < combo.Items.Count; ctr++)
            {
                strItem = (CStringItem)combo.Items[ctr];
                if (!ValTooBig)
                {
                    n = Convert.ToInt32(strItem.Text);
                    if (n > value)
                        ValTooBig = true;
                    else
                        strItem.IsSelectable = true;
                }
                if (ValTooBig)
                    strItem.IsSelectable = false;
            }
        }

        private void SetAllowedComboValueRange(ComboBox combo, int minValue, int maxValue)
        {
            int n;
            CStringItem strItem;
            for (int ctr = 0; ctr < combo.Items.Count; ctr++)
            {
                strItem = (CStringItem)combo.Items[ctr];
                n = Convert.ToInt32(strItem.Text);
                if ((n >= minValue) && (n <= maxValue))
                    strItem.IsSelectable = true;
                else
                    strItem.IsSelectable = false;
            }
        }

        private Size AbsoluteSize(DIBase di)
        {
            return di.IImage.AbsoluteBounds.Size;
        }

        private Size GetMaxKeySize()
        {
            List<Size> keySizes = new List<Size>();
            foreach (CIATKey key in CIAT.SaveFile.GetAllIATKeyUris().Select(u => CIAT.SaveFile.GetIATKey(u)))
            {
                keySizes.Add(AbsoluteSize(key.LeftValue));
                keySizes.Add(AbsoluteSize(key.RightValue));
            }
            Size maxSize = new Size(0, 0);
            foreach (Size sz in keySizes)
            {
                if (maxSize.Width < sz.Width)
                    maxSize.Width = sz.Width;
                if (maxSize.Height < sz.Height)
                    maxSize.Height = sz.Height;
            }
            return maxSize;
        }


        private void CalcAllowableComboValues()
        {
            SetMaxAllowedComboValue(StimulusWidth, LocalLayout.GetMaxStimulusWidth());
            SetMaxAllowedComboValue(StimulusHeight, LocalLayout.GetMaxStimulusHeight());
            Size sz = GetMaxKeySize();
            SetAllowedComboValueRange(ResponseKeyWidth, sz.Width, LocalLayout.GetMaxKeyValueWidth());
            SetAllowedComboValueRange(ResponseKeyHeight, sz.Height, LocalLayout.GetMaxKeyValueHeight());
            SetMaxAllowedComboValue(InstructionsWidth, LocalLayout.GetMaxInstructionsWidth());
            SetMaxAllowedComboValue(InstructionsHeight, LocalLayout.GetMaxInstructionsHeight());
            SetMaxAllowedComboValue(ErrorMarkWidth, LocalLayout.GetMaxErrorMarkWidth());
            SetMaxAllowedComboValue(ErrorMarkHeight, LocalLayout.GetMaxErrorMarkHeight());
        }

        private PointF Transform(PointF point)
        {
            PointF dest = new PointF();
            dest.X = ((point.X - (LocalLayout.TotalSize.Width >> 1)) * fScaleFactor) + (LayoutImage.Width >> 1);
            dest.Y = ((point.Y - (LocalLayout.TotalSize.Height >> 1)) * fScaleFactor) + (LayoutImage.Height >> 1);
            if (dest.X > LayoutImage.Width)
                dest.X = LayoutImage.Width;
            if (dest.Y > LayoutImage.Height)
                dest.Y = LayoutImage.Height;
            if (dest.X < 0)
                dest.X = 0;
            if (dest.Y < 0)
                dest.Y = 0;
            return dest;
        }

        private RectangleF Transform(RectangleF rect)
        {
            RectangleF transformedRect = new RectangleF(Transform(rect.Location), new SizeF(rect.Width * fScaleFactor, rect.Height * fScaleFactor));
            while (transformedRect.X + transformedRect.Width > LayoutImage.Width - 1)
            {
                transformedRect.Width -= .5F;
                transformedRect.X -= .5F;
            }
            while (transformedRect.Y + transformedRect.Height > LayoutImage.Height - 1)
            {
                transformedRect.Height -= .5F;
                transformedRect.Y -= .5F;
            }
            return transformedRect;
        }

        private void DrawLayoutRectangle(Rectangle rect, String label, System.Drawing.Color c)
        {
            Brush br = new SolidBrush(c);
            Pen pen = new Pen(br);
            RectangleF transformedRect = rect;
            transformedRect.Location += new SizeF(LocalLayout.BorderWidth, LocalLayout.BorderWidth);
            transformedRect = Transform(transformedRect);
            transformedRect.Location -= new SizeF(1, 1);
            float x1, x2, y1, y2;
            x1 = transformedRect.X;
            x2 = transformedRect.X + transformedRect.Width;
            y1 = transformedRect.Y;
            y2 = transformedRect.Y;
            OutlineGraphics.DrawLine(pen, x1, y1, x2, y2);
            float ticker = TickGap;
            while (ticker < x2 - x1)
            {
                OutlineGraphics.DrawLine(pen, x1 + ticker, y1, x1 + ticker - TickLength, y1 + TickLength);
                ticker += TickGap;
            }
            x1 = x2;
            y2 = transformedRect.Y + transformedRect.Height;
            OutlineGraphics.DrawLine(pen, x1, y1, x2, y2);
            ticker = TickGap;
            while (ticker < y2 - y1)
            {
                OutlineGraphics.DrawLine(pen, x1, y1 + ticker, x1 - TickLength, y1 + ticker - TickLength);
                ticker += TickGap;
            }
            x2 = transformedRect.X;
            y1 = y2;
            OutlineGraphics.DrawLine(pen, x1, y1, x2, y2);
            ticker = TickGap;
            while (ticker < x1 - x2)
            {
                OutlineGraphics.DrawLine(pen, x1 - ticker, y1, x1 - ticker + TickLength, y1 - TickLength);
                ticker += TickGap;
            }
            x1 = x2;
            y2 = transformedRect.Y;
            OutlineGraphics.DrawLine(pen, x1, y1, x2, y2);
            ticker = TickGap;
            while (ticker < y1 - y2)
            {
                OutlineGraphics.DrawLine(pen, x1, y1 - ticker, x1 + TickLength, y1 - ticker + TickLength);
                ticker += TickGap;
            }
            Size szText = System.Windows.Forms.TextRenderer.MeasureText(label, System.Drawing.SystemFonts.DialogFont);
            OutlineGraphics.DrawString(label, System.Drawing.SystemFonts.DialogFont, pen.Brush,
                new PointF(transformedRect.X + ((int)(transformedRect.Width - szText.Width) >> 1), transformedRect.Y + ((int)(transformedRect.Height - szText.Height) >> 1)));
            pen.Dispose();
            br.Dispose();
        }

        private void UpdateLayoutOutline()
        {
            OutlineGraphics = Graphics.FromImage(LayoutImage);
            OutlineGraphics.FillRectangle(System.Drawing.SystemBrushes.Control, new Rectangle(new Point(0, 0), new Size(LayoutImage.Width, LayoutImage.Height)));
            float OutlineAR = (float)LocalLayout.TotalSize.Width / (float)LocalLayout.TotalSize.Height;
            float CanvasAR = (float)LayoutImage.Width / (float)LayoutImage.Height;
            fScaleFactor = (OutlineAR > CanvasAR) ? (float)LayoutImage.Width / (float)LocalLayout.TotalSize.Width :
                (float)LayoutImage.Height / (float)LocalLayout.TotalSize.Height;
            Brush BorderBrush = new SolidBrush(Color.Yellow);
            Pen BorderPen = new Pen(BorderBrush);
            if (LocalLayout.BorderWidth > 0)
            {
                RectangleF OuterBorderRect = Transform(new Rectangle(0, 0, LocalLayout.TotalSize.Width, LocalLayout.TotalSize.Height));
                RectangleF InnerBorderRect = new Rectangle(0, 0, LocalLayout.TotalSize.Width, LocalLayout.TotalSize.Height);
                InnerBorderRect.Inflate(-LocalLayout.BorderWidth, -LocalLayout.BorderWidth);
                InnerBorderRect = Transform(InnerBorderRect);
                BorderPen.Width = 2;

                if (OuterBorderRect.Width > LayoutImage.Width)
                    OuterBorderRect.Width = LayoutImage.Width;
                if (OuterBorderRect.Height > LayoutImage.Height)
                    OuterBorderRect.Height = LayoutImage.Height;
                OutlineGraphics.DrawRectangle(BorderPen, OuterBorderRect.X, OuterBorderRect.Y, OuterBorderRect.Width, OuterBorderRect.Height);
                float x1 = OuterBorderRect.X, x2 = OuterBorderRect.X, y1 = OuterBorderRect.Y, y2 = OuterBorderRect.Y;
                float ticker = TickGap;
                while (ticker <= OuterBorderRect.Width + OuterBorderRect.Height)
                {
                    if (ticker < OuterBorderRect.Width)
                        x1 += TickGap;
                    else
                    {
                        y1 += TickGap - (OuterBorderRect.X + OuterBorderRect.Width - x1);
                        x1 = OuterBorderRect.X + OuterBorderRect.Width;
                    }
                    if (ticker < OuterBorderRect.Height)
                        y2 += TickGap;
                    else
                    {
                        x2 += TickGap - (OuterBorderRect.Y + OuterBorderRect.Height - y2);
                        y2 = OuterBorderRect.Y + OuterBorderRect.Height;
                    }
                    OutlineGraphics.DrawLine(BorderPen, x1, y1, x2, y2);
                    ticker += TickGap;
                }
                OutlineGraphics.DrawLine(BorderPen, OuterBorderRect.X, OuterBorderRect.Y + OuterBorderRect.Height, OuterBorderRect.X + OuterBorderRect.Width, OuterBorderRect.Y);
                OutlineGraphics.DrawRectangle(BorderPen, InnerBorderRect.X, InnerBorderRect.Y, InnerBorderRect.Width, InnerBorderRect.Height);
                InnerBorderRect.Inflate(-1, -1);
                OutlineGraphics.FillRectangle(System.Drawing.SystemBrushes.Control, InnerBorderRect);
            }
            uint overlap = LocalLayout.FindOverlap();
            DrawLayoutRectangle(LocalLayout.StimulusRectangle, Properties.Resources.sStimulusRectangleName,
                ((overlap & CIATLayout.Overlap.StimulusRectangle) != 0) ? System.Drawing.Color.Red : LocalLayout.BackColor);
            DrawLayoutRectangle(LocalLayout.LeftKeyValueRectangle, Properties.Resources.sLeftResponseKeyRectangleName,
                ((overlap & CIATLayout.Overlap.KeyValueRectangles) != 0) ? System.Drawing.Color.Red : LocalLayout.BackColor);
            DrawLayoutRectangle(LocalLayout.RightKeyValueRectangle, Properties.Resources.sRightResponseKeyRectangleName,
                ((overlap & CIATLayout.Overlap.KeyValueRectangles) != 0) ? System.Drawing.Color.Red : LocalLayout.BackColor);
            DrawLayoutRectangle(LocalLayout.ErrorRectangle, Properties.Resources.sErrorRectangleName,
                ((overlap & CIATLayout.Overlap.ErrorRectangle) != 0) ? System.Drawing.Color.Red : LocalLayout.BackColor);
            DrawLayoutRectangle(LocalLayout.InstructionsRectangle, Properties.Resources.sInstructionsRectangleName,
                ((overlap & CIATLayout.Overlap.InstructionRectangle) != 0) ? System.Drawing.Color.Red : LocalLayout.BackColor);
            BorderPen.Dispose();
            BorderBrush.Dispose();
            OutlineGraphics.Dispose();
        }

        private void TestForOverlap()
        {
            uint overlap = LocalLayout.FindOverlap();
            if (overlap == 0)
            {
                MainForm.ErrorMsg = String.Empty;
            }
            else if ((overlap & CIATLayout.Overlap.StimulusRectangle) != 0)
            {
                MainForm.ErrorMsg = Properties.Resources.sStimulusRectangleOverlapException;
            }
            else if ((overlap & CIATLayout.Overlap.KeyValueRectangles) != 0)
            {
                MainForm.ErrorMsg = Properties.Resources.sResponseKeyRectangleOverlapException;
            }
            else if ((overlap & CIATLayout.Overlap.InstructionRectangle) != 0)
            {
                MainForm.ErrorMsg = Properties.Resources.sInstructionsRectangleOverlapException;
            }
            else if ((overlap & CIATLayout.Overlap.ErrorRectangle) != 0)
            {
                MainForm.ErrorMsg = Properties.Resources.sErrorMarkRectangleOverlapException;
            }
            if (overlap != 0)
            {
                ApplyButton.Enabled = false;
                OKButton.Enabled = false;
            }
            else
            {
                ApplyButton.Enabled = true;
                OKButton.Enabled = true;
            }
        }

        private Image CreateLayoutBrushImage(int width, int height, System.Drawing.Color ForeColor)
        {
            Image img = new Bitmap(width, height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(img);
            g.FillRectangle(Brushes.Transparent, 0, 0, width, height);
            Brush br = new SolidBrush(ForeColor);
            Pen pen = new Pen(br);
            g.DrawLine(pen, 0, (height >> 1) - 1, width - 1, (height >> 1) - 1);
            g.DrawLine(pen, 0, height >> 1, width - 1, height >> 1);
            int nMarkLength = (height - 2) >> 1;
            g.DrawLine(pen, (width + nMarkLength) >> 1, 0, (width - nMarkLength) >> 1, nMarkLength - 1);
            g.Dispose();
            pen.Dispose();
            br.Dispose();
            return img;
        }

        public void InvalidateAllSizeCombos()
        {
            InteriorWidth.Invalidate();
            InteriorHeight.Invalidate();
            StimulusWidth.Invalidate();
            StimulusHeight.Invalidate();
            ResponseKeyWidth.Invalidate();
            ResponseKeyHeight.Invalidate();
            InstructionsWidth.Invalidate();
            InstructionsHeight.Invalidate();
            ErrorMarkWidth.Invalidate();
            ErrorMarkHeight.Invalidate();
        }

        public void InitBorderWidthCombo()
        {
            UpdatingFromCode = true;
            BorderWidth.DrawMode = DrawMode.OwnerDrawVariable;
            BorderWidth.MeasureItem += new MeasureItemEventHandler(LayoutPanel_MeasureComboItem);
            BorderWidth.DrawItem += new DrawItemEventHandler(LayoutPanel_DrawComboItem);
            for (int ctr = 0; ctr <= 20; ctr++)
            {
                CStringItem si = new CStringItem(ctr.ToString(), System.Drawing.SystemFonts.DialogFont);
                BorderWidth.Items.Add(si);
                if (ctr == LocalLayout.BorderWidth)
                    BorderWidth.SelectedIndex = ctr;
            }
            UpdatingFromCode = false;
        }

        public LayoutPanel()
        {
            InitializeComponent();
            LocalLayout = new CIATLayout(CIAT.SaveFile.Layout);
            ComboBoxFont = System.Drawing.SystemFonts.DialogFont;
            Instructions.Text = Properties.Resources.sLayoutPanelInstructions;
            this.Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom | AnchorStyles.Top;
            LayoutImage = null;
            ImageRadio.CheckedChanged += new EventHandler(ImageRadio_CheckedChanged);
            ColorRadio.CheckedChanged += new EventHandler(ColorRadio_CheckedChanged);
            BrowseButton.Click += new EventHandler(BrowseButton_Click);
            OKButton.Click += new EventHandler(OKButton_Click);
            ApplyButton.Click += new EventHandler(ApplyButton_Click);
            CancelButton.Click += new EventHandler(CancelButton_Click);
            this.WebPageColorDrop.SelectedIndexChanged += new EventHandler(WebPageColorDrop_SelectedIndexChanged);
        }

        private void OKButton_Click(object sender, EventArgs e)
        {
            CIAT.SaveFile.Layout = LocalLayout;
            MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
        }

        private void ApplyButton_Click(object sender, EventArgs e)
        {
            LocalLayout.InvalidateMarks();
            CIAT.SaveFile.Layout = LocalLayout;
            LocalLayout = new CIATLayout(LocalLayout);
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            MainForm.FormContents = IATConfigMainForm.EFormContents.Main;
        }

        private void ImageRadio_CheckedChanged(object sender, EventArgs e)
        {
            ImageLabel.Enabled = true;
            ImageFileName.Enabled = true;
            BrowseButton.Enabled = true;
            ImageRepeat.Enabled = true;
            ImagePositioningDrop.Enabled = true;
        }

        private void ColorRadio_CheckedChanged(object sender, EventArgs e)
        {
            ImageLabel.Enabled = false;
            ImageFileName.Enabled = false;
            BrowseButton.Enabled = false;
            ImageRepeat.Enabled = false;
            ImagePositioningDrop.Enabled = false;
            ImageFileName.Text = String.Empty;
        }

        private void BrowseButton_Click(object sender, EventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "Image Files|*.png;*.bmp;*.tiff;*.tif;*.gif;*.emf;*.exif;*.ico;*.jpg;*.jpeg;*.wmf|All Files|*.*";
            if (dlg.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    LocalLayout.WebpageBackgroundImageFilename = dlg.FileName;
                }
                catch (OutOfMemoryException ex)
                {
                    MessageBox.Show("The file you selected does not contain a valid image.", "Bad Image File");
                    return;
                }
            }
        }

        private void LayoutPanel_MeasureComboItem(object sender, MeasureItemEventArgs e)
        {
            if (e.Index == -1)
            {
                e.ItemHeight = 0;
                e.ItemWidth = 0;
                return;
            }
            CComboBoxItem item = (CComboBoxItem)((ComboBox)sender).Items[e.Index];
            Size sz = item.Measure();
            e.ItemWidth = sz.Width;
            e.ItemHeight = sz.Height;
        }

        private void LayoutPanel_DrawComboItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index == -1)
                return;
            CComboBoxItem item = (CComboBoxItem)((ComboBox)sender).Items[e.Index];
            item.Draw(e.Graphics, e.Bounds, e.BackColor, e.ForeColor);
        }

        private void LayoutPanel_InteriorSizeChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == -1)
                return;
            if (UpdatingFromCode)
                return;
            CStringItem width, height;
            width = (CStringItem)InteriorWidth.Items[InteriorWidth.SelectedIndex];
            height = (CStringItem)InteriorHeight.Items[InteriorHeight.SelectedIndex];
            LocalLayout.SuspendLayout();
            LocalLayout.InteriorSize = new Size(Convert.ToInt32(width.Text), Convert.ToInt32(height.Text));
            UpdatingFromCode = true;
            FillSizeCombos();
            UpdatingFromCode = false;
            // update the layout object from the combos
            width = (CStringItem)StimulusWidth.Items[StimulusWidth.SelectedIndex];
            height = (CStringItem)StimulusHeight.Items[StimulusHeight.SelectedIndex];
            LocalLayout.StimulusSize = new Size(Convert.ToInt32(width.Text), Convert.ToInt32(height.Text));
            width = (CStringItem)ResponseKeyWidth.Items[ResponseKeyWidth.SelectedIndex];
            height = (CStringItem)ResponseKeyHeight.Items[ResponseKeyHeight.SelectedIndex];
            LocalLayout.KeyValueSize = new Size(Convert.ToInt32(width.Text), Convert.ToInt32(height.Text));
            width = (CStringItem)ErrorMarkWidth.Items[ErrorMarkWidth.SelectedIndex];
            height = (CStringItem)ErrorMarkHeight.Items[ErrorMarkHeight.SelectedIndex];
            LocalLayout.ErrorSize = new Size(Convert.ToInt32(width.Text), Convert.ToInt32(height.Text));
            width = (CStringItem)InstructionsWidth.Items[InstructionsWidth.SelectedIndex];
            height = (CStringItem)InstructionsHeight.Items[InstructionsHeight.SelectedIndex];
            LocalLayout.InstructionsSize = new Size(Convert.ToInt32(width.Text), Convert.ToInt32(height.Text));
            LocalLayout.ResumeLayout();
            CalcAllowableComboValues();
            LayoutImagePanel.Invalidate();
            InvalidateAllSizeCombos();
            TestForOverlap();
        }

        private void LayoutPanel_StimulusSizeChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == -1)
                return;
            if (UpdatingFromCode)
                return;
            CStringItem width, height;
            width = (CStringItem)StimulusWidth.Items[StimulusWidth.SelectedIndex];
            height = (CStringItem)StimulusHeight.Items[StimulusHeight.SelectedIndex];
            LocalLayout.StimulusSize = new Size(Convert.ToInt32(width.Text), Convert.ToInt32(height.Text));
            CalcAllowableComboValues();
            LayoutImagePanel.Invalidate();
            InvalidateAllSizeCombos();
            TestForOverlap();
        }

        private void LayoutPanel_ResponseKeySizeChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == -1)
                return;
            if (UpdatingFromCode)
                return;
            CStringItem width, height;
            width = (CStringItem)ResponseKeyWidth.Items[ResponseKeyWidth.SelectedIndex];
            height = (CStringItem)ResponseKeyHeight.Items[ResponseKeyHeight.SelectedIndex];
            LocalLayout.KeyValueSize = new Size(Convert.ToInt32(width.Text), Convert.ToInt32(height.Text));
            CalcAllowableComboValues();
            LayoutImagePanel.Invalidate();
            InvalidateAllSizeCombos();
            TestForOverlap();
        }


        private void LayoutPanel_InstructionsSizeChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == -1)
                return;
            if (UpdatingFromCode)
                return;
            CStringItem width, height;
            LocalLayout.SuspendLayout();
            width = (CStringItem)InstructionsWidth.Items[InstructionsWidth.SelectedIndex];
            height = (CStringItem)InstructionsHeight.Items[InstructionsHeight.SelectedIndex];
            LocalLayout.InstructionsSize = new Size(Convert.ToInt32(width.Text), Convert.ToInt32(height.Text));
            LocalLayout.ResumeLayout();
            CalcAllowableComboValues();
            LayoutImagePanel.Invalidate();
            InvalidateAllSizeCombos();
            TestForOverlap();
        }

        private void LayoutPanel_ErrorMarkSizeChanged(object sender, EventArgs e)
        {
            if (((ComboBox)sender).SelectedIndex == -1)
                return;
            if (UpdatingFromCode)
                return;
            LocalLayout.SuspendLayout();
            CStringItem width, height;
            width = (CStringItem)ErrorMarkWidth.Items[ErrorMarkWidth.SelectedIndex];
            height = (CStringItem)ErrorMarkHeight.Items[ErrorMarkHeight.SelectedIndex];
            LocalLayout.ErrorSize = new Size(Convert.ToInt32(width.Text), Convert.ToInt32(height.Text));
            LocalLayout.ResumeLayout();
            CalcAllowableComboValues();
            LayoutImagePanel.Invalidate();
            InvalidateAllSizeCombos();
            TestForOverlap();
        }

        private void LayoutPanel_ParentChanged(object sender, EventArgs e)
        {
            if (Parent != null)
            {
                InitSizeCombos();
                LayoutBrush = new SolidBrush(System.Drawing.Color.Green);
                LayoutOverlapBrush = new SolidBrush(System.Drawing.Color.Red);
                LayoutPen = new Pen(LayoutBrush);
                LayoutOverlapPen = new Pen(LayoutOverlapBrush);
                InitColorCombos();
                InitBorderWidthCombo();
                if (LocalLayout.WebpageImage)
                    ImageRadio.Checked = true;
                else
                    ColorRadio.Checked = true;
                ImagePositioningDrop.DropDownStyle = ComboBoxStyle.DropDownList;
                ImagePositioningDrop.Items.Clear();
                ImagePositioningDrop.Items.Add(CIATLayout.EWebpageBackgroundImageOrientation.Tiled);
                ImagePositioningDrop.Items.Add(CIATLayout.EWebpageBackgroundImageOrientation.Centered);
                ImagePositioningDrop.Items.Add(CIATLayout.EWebpageBackgroundImageOrientation.Top);
                ImagePositioningDrop.Items.Add(CIATLayout.EWebpageBackgroundImageOrientation.Left);
                ImagePositioningDrop.Items.Add(CIATLayout.EWebpageBackgroundImageOrientation.Right);
                ImagePositioningDrop.Items.Add(CIATLayout.EWebpageBackgroundImageOrientation.Bottom);
                ImagePositioningDrop.Items.Add(CIATLayout.EWebpageBackgroundImageOrientation.UpperRight);
                ImagePositioningDrop.Items.Add(CIATLayout.EWebpageBackgroundImageOrientation.UpperLeft);
                ImagePositioningDrop.Items.Add(CIATLayout.EWebpageBackgroundImageOrientation.LowerRight);
                ImagePositioningDrop.Items.Add(CIATLayout.EWebpageBackgroundImageOrientation.LowerLeft);
                Invalidate();
            }
            else
            {
                UpdatingFromCode = true;
                LayoutPen.Dispose();
                LayoutOverlapPen.Dispose();
                LayoutBrush.Dispose();
                LayoutOverlapBrush.Dispose();
                OutlineGraphics.Dispose();
                InteriorHeight.Items.Clear();
                InteriorWidth.Items.Clear();
                StimulusHeight.Items.Clear();
                StimulusWidth.Items.Clear();
                ResponseKeyHeight.Items.Clear();
                ResponseKeyWidth.Items.Clear();
                ErrorMarkHeight.Items.Clear();
                ErrorMarkWidth.Items.Clear();
                InstructionsHeight.Items.Clear();
                InstructionsWidth.Items.Clear();
                BackgroundColor.Items.Clear();
                BorderColor.Items.Clear();
                OutlineColor.Items.Clear();
                BorderWidth.Items.Clear();
                UpdatingFromCode = false;
            }
        }

        private void BackgroundColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!UpdatingFromCode)
            {
                LocalLayout.BackColor = ((CColorItem)BackgroundColor.SelectedItem).ItemColor;
                LayoutImagePanel.Invalidate();
            }
        }

        private void BorderColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!UpdatingFromCode)
            {
                LocalLayout.BorderColor = ((CColorItem)BorderColor.SelectedItem).ItemColor;
                LayoutImagePanel.Invalidate();
            }
        }

        private void OutlineColor_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!UpdatingFromCode)
            {
                LocalLayout.OutlineColor = ((CColorItem)OutlineColor.SelectedItem).ItemColor;
                LayoutImagePanel.Invalidate();
            }
        }

        private void WebPageColorDrop_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!UpdatingFromCode)
            {
                LocalLayout.WebpageBackColor = ((CColorItem)WebPageColorDrop.SelectedItem).ItemColor;
            }
        }


        private void BorderWidth_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!UpdatingFromCode)
            {
                LocalLayout.BorderWidth = Convert.ToInt32(((CStringItem)BorderWidth.SelectedItem).Text);
                LayoutImagePanel.Invalidate();
            }
        }

        private void LayoutPanel_SizeChanged(object sender, EventArgs e)
        {
            if (Parent != null)
                LayoutImagePanel.Invalidate();
        }

        private void LayoutImagePanel_Paint(object sender, PaintEventArgs e)
        {
            LayoutImage = new Bitmap(LayoutImagePanel.ClientSize.Width, LayoutImagePanel.ClientSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            UpdateLayoutOutline();
            e.Graphics.DrawImage(LayoutImage, new Point(0, 0));
            LayoutImage.Dispose();
        }
    }
}
