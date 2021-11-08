using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    partial class IATItemPreviewPanel : UserControl
    {
        private static object lockObject = new object();

        // the memory bitmap used to render the preview to
        private Bitmap BufferBMP;
        private Image LastUpdateImage = null;
        // the graphics object associated with the memory buffer
//        private Graphics BufferGraphics, PreviewGraphics;
        private Rectangle PreviewRectangle;
        // the brush used to erase regions of BufferBMP
        private SolidBrush EraseBrush;

        private CIATBlock Block
        {
            get
            {
                return ((IATBlockPanel)Parent).Block;
            }
        }

        /// <summary>
        /// gets the stimulus rendered in the control
        /// </summary>
        public CDisplayItem Stimulus
        {
            get
            {
                CIATItem item = ((IATBlockPanel)Parent).ActiveItem;
                if (item == null)
                    return null;
                return item.Stimulus;
            }
        }

        public IIATImage StimulusImage
        {
            get
            {
                return ((IATBlockPanel)Parent).ActiveItem.Stimulus.IATImage;
            }
        }
                

        public CIATKey Key
        {
            get
            {
                return ((IATBlockPanel)Parent).ActiveKey;
            }
        }

        /// <summary>
        /// gets of sets the CMultiLineTextDisplayItem object rendered in the preview pane
        /// </summary>
        public CMultiLineTextDisplayItem Instructions
        {
            get 
            {
                return Block.Instructions;
            }
        }

        public void DisposeOfGraphics()
        {
            if (BufferBMP != null)
            {
                BufferBMP.Dispose();
                EraseBrush.Dispose();
                ItemPreview.Image.Dispose();
                BufferBMP = null;
            }
        }

        public void InitGraphics()
        {
            if (BufferBMP != null)
                return;
            Graphics g = Graphics.FromHwnd(ItemPreview.Handle);
            BufferBMP = new Bitmap(ItemPreview.ClientRectangle.Width, ItemPreview.ClientRectangle.Height, g);
            EraseBrush = new SolidBrush(CIAT.Layout.BackColor);
            g.FillRectangle(EraseBrush, ItemPreview.ClientRectangle);
            ItemPreview.SizeMode = PictureBoxSizeMode.CenterImage;
           
            ItemPreview.Image = new Bitmap(ItemPreview.ClientRectangle.Width, ItemPreview.ClientRectangle.Height, g);
            g.Dispose();
            Graphics PreviewGraphics = Graphics.FromImage(ItemPreview.Image);
            PreviewGraphics.FillRectangle(System.Drawing.SystemBrushes.Control, 0, 0, ItemPreview.Image.Width, ItemPreview.Image.Height);
            PreviewGraphics.Dispose();
            PreviewRectangle.X = (ItemPreview.Width - CIAT.Layout.InteriorSize.Width) >> 1;
            PreviewRectangle.Y = (ItemPreview.Height - CIAT.Layout.InteriorSize.Height) >> 1;
            PreviewRectangle.Width = CIAT.Layout.InteriorSize.Width;
            PreviewRectangle.Height = CIAT.Layout.InteriorSize.Height;

            /*
            double LayoutAR = (double)CIAT.Layout.InteriorSize.Width / (double)CIAT.Layout.InteriorSize.Height;
            double PreviewAR = (double)ItemPreview.ClientRectangle.Width / (double)ItemPreview.ClientRectangle.Height;
            if (LayoutAR > PreviewAR)
            {
                PreviewRectangle.X = 0;
                PreviewRectangle.Width = ItemPreview.Image.Width;
                PreviewRectangle.Height = (CIAT.Layout.InteriorSize.Height * ItemPreview.Image.Width) / CIAT.Layout.InteriorSize.Width;
                PreviewRectangle.Y = (ItemPreview.Image.Height - PreviewRectangle.Height) >> 1;
            }
            else
            {
                PreviewRectangle.Y = 0;
                PreviewRectangle.Height = ItemPreview.Image.Height;
                PreviewRectangle.Width = (CIAT.Layout.InteriorSize.Width * ItemPreview.Image.Height) / CIAT.Layout.InteriorSize.Height;
                PreviewRectangle.X = (ItemPreview.Image.Width - PreviewRectangle.Width) >> 1;
            }*/
        }

        public IATItemPreviewPanel()
        {
            InitializeComponent();
            BufferBMP = null;
            EraseBrush = null;
            ItemPreview.Image = null;
            ItemPreview.Dock = DockStyle.Fill;
        }

        private void IATItemPreviewPanel_ParentChanged(object sender, EventArgs e)
        {
            InitGraphics();
        }

        /// <summary>
        /// Dummy callback function
        /// </summary>
        /// <returns>false</returns>
        public static bool ThumbnailCallback()
        {
            return false;
        }
        /*
        /// <summary>
        /// Updates the image in the preview pane
        /// </summary>
        private void UpdatePreviewFromBuffer()
        {
            PreviewGraphics.DrawImage(BufferBMP, PreviewRectangle);
            ItemPreview.Invalidate();
        }
        /*
        /// <summary>
        /// Redraws the stimulus
        /// </summary>
        public void InvalidateStimulus()
        {
            // clear the stimulus rectangle
            if (!PerformingMultipleInvalidate)
                BufferGraphics.FillRectangle(EraseBrush, CIAT.Layout.StimulusRectangle);
            
            // if the stimulus is non-null, draw it
            if (Stimulus != null)
            {
                BufferGraphics.Clip = new Region(CIAT.Layout.StimulusRectangle);
                BufferGraphics.DrawImage(Stimulus.IATImage.theImage, CIAT.Layout.StimulusRectangle.Location + new Size((CIAT.Layout.StimulusSize.Width - Stimulus.IATImage.ImageSize.Width) >> 1,
                    (CIAT.Layout.StimulusSize.Height - Stimulus.IATImage.ImageSize.Height) >> 1));
                BufferGraphics.ResetClip();
            }

            // if not performing a multiple invalidation, update the preview image
            if (!PerformingMultipleInvalidate)
                UpdatePreviewFromBuffer();
        }
/*
        /// <summary>
        /// Forces a re-draw of the key values
        /// </summary>
        public void InvalidateKey()
        {
            // clear the background rectangles
            if (!PerformingMultipleInvalidate)
            {
                BufferGraphics.FillRectangle(EraseBrush, Rectangle.Inflate(CIAT.Layout.LeftKeyValueRectangle, CIAT.Layout.ResponseValueRectMargin, 
                    CIAT.Layout.ResponseValueRectMargin));
                BufferGraphics.FillRectangle(EraseBrush, Rectangle.Inflate(CIAT.Layout.RightKeyValueRectangle, CIAT.Layout.ResponseValueRectMargin,
                    CIAT.Layout.ResponseValueRectMargin));
            }

            // if key is non-null, draw it
            if (Key != null)
            {
                BufferGraphics.Clip = new Region(CIAT.Layout.LeftKeyValueRectangle);
                BufferGraphics.DrawImage(Key.LeftValue.IATImage.theImage, CIAT.Layout.LeftKeyValueRectangle.Location + 
                    new Size((CIAT.Layout.LeftKeyValueRectangle.Size.Width - Key.LeftValue.IATImage.ImageSize.Width) >> 1,
                        (CIAT.Layout.LeftKeyValueRectangle.Size.Height - Key.LeftValue.IATImage.ImageSize.Height) >> 1));
                BufferGraphics.Clip = new Region(CIAT.Layout.RightKeyValueRectangle);
                BufferGraphics.DrawImage(Key.RightValue.IATImage.theImage, CIAT.Layout.RightKeyValueRectangle.Location +
                    new Size((CIAT.Layout.RightKeyValueRectangle.Size.Width - Key.RightValue.IATImage.ImageSize.Width) >> 1,
                        (CIAT.Layout.RightKeyValueRectangle.Size.Height - Key.RightValue.IATImage.ImageSize.Height) >> 1));
                BufferGraphics.ResetClip();
            }

            // if not performing a multiple invalidation, updatte the preview image
            if (!PerformingMultipleInvalidate)
                UpdatePreviewFromBuffer();
        }
*/
        protected void InvalidateKeyedDirection()
        {
            Pen OutlinePen = new Pen(CIAT.Layout.OutlineColor, CIAT.Layout.ResponseValueRectMargin >> 1);
            Pen BackgroundPen = new Pen(EraseBrush, CIAT.Layout.ResponseValueRectMargin >> 1);
            Graphics g = Graphics.FromImage(ItemPreview.Image);

//            double keyAR = (double)CIAT.Layout.KeyValueSize.Width / (double)CIAT.Layout.KeyValueSize.Height;
//            double previewAR = (double)PreviewRectangle.Width / (double)PreviewRectangle.Height;
            Rectangle LeftKeyRectangle, RightKeyRectangle;
            LeftKeyRectangle = new Rectangle(PreviewRectangle.Location, new Size((int)(CIAT.Layout.KeyValueSize.Width * (double)PreviewRectangle.Height / (double)PreviewRectangle.Height),
                (int)(CIAT.Layout.KeyValueSize.Height * (double)PreviewRectangle.Width / (double)PreviewRectangle.Height)));
            RightKeyRectangle = new Rectangle(PreviewRectangle.Width - LeftKeyRectangle.Width, 0, LeftKeyRectangle.Width, LeftKeyRectangle.Height);
            
            // erase left response key outline
            LeftKeyRectangle.Inflate(CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.ResponseValueRectMargin);
            RightKeyRectangle.Inflate(CIAT.Layout.ResponseValueRectMargin, CIAT.Layout.ResponseValueRectMargin);
            g.DrawRectangle(BackgroundPen, LeftKeyRectangle);
            g.DrawRectangle(BackgroundPen, RightKeyRectangle);

            // draw response key outline where appropriate
            if (((IATBlockPanel)Parent).ActiveItem != null)
            {
                switch (((IATBlockPanel)Parent).ActiveItem.KeyedDir)
                {
                    case CIATItem.EKeyedDir.Left:
                        g.DrawRectangle(OutlinePen, LeftKeyRectangle);
                        break;

                    case CIATItem.EKeyedDir.Right:
                        g.DrawRectangle(OutlinePen, RightKeyRectangle);
                        break;
                }
            }

            OutlinePen.Dispose();
            BackgroundPen.Dispose();
        }
/*
        public void InvalidateInstructions()
        {
            // clear the background rectangle
            if (!PerformingMultipleInvalidate) 
                BufferGraphics.FillRectangle(EraseBrush, CIAT.Layout.InstructionsRectangle);

            // draw instructions
            BufferGraphics.Clip = new Region(CIAT.Layout.InstructionsRectangle);
            BufferGraphics.DrawImage(Instructions.IATImage.theImage, CIAT.Layout.InstructionsRectangle);
            BufferGraphics.ResetClip();
            
            // if not performing a multiple update, update the preview image
            if (!PerformingMultipleInvalidate)
                UpdatePreviewFromBuffer();
        }
*/
        /// <summary>
        /// Redraws all objects in the preview pane
        /// </summary>
        public void InvalidatePreview(Image img)
        {
            lock (lockObject)
            {
                Graphics g = Graphics.FromImage(ItemPreview.Image);
                if (img == null)
                {
                    if (LastUpdateImage == null)
                    {
                        Brush br = new SolidBrush(CIAT.Layout.BackColor);
                        g.FillRectangle(br, this.ClientRectangle);
                        br.Dispose();
                    }
                    else
                        g.DrawImage(LastUpdateImage, this.ClientRectangle);
                }
                else
                    g.DrawImage(img, PreviewRectangle);
                g.Dispose();
                InvalidateKeyedDirection();

                ItemPreview.Invalidate();
            }
        }
    }
}
