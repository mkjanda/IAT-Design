using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;

using System.Text;
using System.Windows.Forms;

namespace IATClient
{
    partial class InstructionScreenPreview : UserControl, IDisposable
    {
        // the display mode of the preview control
        public enum EMode { None, Text, MockItem, Keyed };
        public EMode Mode;

        // Graphics objects
        protected Bitmap Buffer;
        protected Rectangle PreviewRectangle;
        protected Bitmap LastUpdateImage = null;
        // a flag to indicate if a multiple update is being performed
        protected bool PerformingMultipleUpdate;
        private object lockObject = new object();
/*
        public Image InstructionScreenImage
        {
            get
            {
                return Buffer;
            }
        }
        */
        public InstructionScreenPreview()
        {
            InitializeComponent();
            Mode = EMode.None;
            PerformingMultipleUpdate = false;
            PreviewRectangle = new Rectangle();
            CreateGraphicsObjects();
        }

        public void CreateGraphicsObjects()
        {
            if (Buffer != null)
                return;
            Buffer = new Bitmap(CIAT.SaveFile.Layout.InteriorSize.Width, CIAT.SaveFile.Layout.InteriorSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Brush EraseBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
            PreviewPane.Image = new Bitmap(PreviewPane.ClientRectangle.Width, PreviewPane.ClientRectangle.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            Graphics g = Graphics.FromImage(PreviewPane.Image);
            g.FillRectangle(EraseBrush, 0, 0, PreviewPane.Image.Width, PreviewPane.Image.Height);
            double LayoutAR = (double)CIAT.SaveFile.Layout.InteriorSize.Width / (double)CIAT.SaveFile.Layout.InteriorSize.Height;
            double PreviewAR = (double)PreviewPane.ClientRectangle.Width / (double)PreviewPane.ClientRectangle.Height;
            if (LayoutAR > PreviewAR)
            {
                PreviewRectangle.X = 0;
                PreviewRectangle.Width = PreviewPane.Image.Width;
                PreviewRectangle.Height = (CIAT.SaveFile.Layout.InteriorSize.Height * PreviewPane.Image.Width) / CIAT.SaveFile.Layout.InteriorSize.Width;
                PreviewRectangle.Y = (PreviewPane.Image.Height - PreviewRectangle.Height) >> 1;
            }
            else
            {
                PreviewRectangle.Y = 0;
                PreviewRectangle.Height = PreviewPane.Image.Height;
                PreviewRectangle.Width = (CIAT.SaveFile.Layout.InteriorSize.Width * PreviewPane.Image.Height) / CIAT.SaveFile.Layout.InteriorSize.Height;
                PreviewRectangle.X = (PreviewPane.Image.Width - PreviewRectangle.Width) >> 1;
            }
            EraseBrush.Dispose();
        }


        public void DisposeofGraphicsObjects()
        {
            if (Buffer == null)
                return;
            PreviewPane.Image.Dispose();
            PreviewPane.Image = null;
            Buffer.Dispose();
            Buffer = null;
        }
/*
        public void InvalidateAll(CInstructionScreen Screen)
        {
            PerformingMultipleUpdate = true;

            BufferGraphics.FillRectangle(EraseBrush, new Rectangle(new Point(0, 0), CIAT.SaveFile.Layout.InteriorSize));
            if (Screen != null)
            {
                InvalidateContinueInstructions(Screen.ContinueInstructions);
                if (Mode == EMode.Text)
                {
                    InvalidateTextInstructions(((CTextInstructionScreen)Screen).Instructions);
                }
                else if (Mode == EMode.MockItem)
                {
                    InvalidateMockItemStimulus((IStimulus)((CMockItemScreen)Screen).MockItem.Stimulus);
                    InvalidateMockItemInstructions(((CMockItemScreen)Screen).BriefInstructions);
                    CIATKey ResponseKey = null;
                    if (((CMockItemScreen)Screen).ResponseKeyName != String.Empty)
                        ResponseKey = CIATKey.KeyDictionary[((CMockItemScreen)Screen).ResponseKeyName];
                    InvalidateResponseKey(ResponseKey);
                    InvalidateMockItemKeyedDirOutline(((CMockItemScreen)Screen).MockItem.KeyedDir, ((CMockItemScreen)Screen).KeyedDirOutlined);
                    InvalidateMockItemInvalidResponse(((CMockItemScreen)Screen).InvalidResponseFlag);
                }
                else if (Mode == EMode.Keyed)
                {
                    CIATKey ResponseKey = null;
                    if (((CKeyInstructionScreen)Screen).ResponseKeyName != String.Empty)
                        ResponseKey = CIATKey.KeyDictionary[((CKeyInstructionScreen)Screen).ResponseKeyName];
                    InvalidateResponseKey(ResponseKey);
                    InvalidateTextInstructions(((CKeyInstructionScreen)Screen).Instructions);
                }
            }
            UpdatePreviewPane();
            PerformingMultipleUpdate = false;
        }

        public void InvalidateContinueInstructions(CTextDisplayItem di)
        {
            if (!PerformingMultipleUpdate)
                BufferGraphics.FillRectangle(EraseBrush, CIAT.SaveFile.Layout.ContinueInstructionsRectangle);

            if (di != null)
            {
                Size ItemSize = di.IATImage.ImageSize;
                BufferGraphics.Clip = new Region(CIAT.SaveFile.Layout.ContinueInstructionsRectangle);
                BufferGraphics.DrawImage(di.IATImage.theImage, CIAT.SaveFile.Layout.ContinueInstructionsRectangle.Location +
                    new Size((CIAT.SaveFile.Layout.ContinueInstructionsRectangle.Width - ItemSize.Width) >> 1, (CIAT.SaveFile.Layout.ContinueInstructionsRectangle.Height - ItemSize.Height) >> 1));
                BufferGraphics.ResetClip();
            }

            if (!PerformingMultipleUpdate)
                UpdatePreviewPane();
            
        }

        public void InvalidateTextInstructions(CMultiLineTextDisplayItem di)
        {
            if (!PerformingMultipleUpdate)
                BufferGraphics.FillRectangle(EraseBrush, di.GetBoundingRectangle());

            if (di != null)
            {
                BufferGraphics.Clip = new Region(di.GetBoundingRectangle());
                BufferGraphics.DrawImage(di.IATImage.theImage, CIAT.SaveFile.Layout.InstructionScreenTextAreaRectangle.Location);
                BufferGraphics.ResetClip();
            }

            if (!PerformingMultipleUpdate)
                UpdatePreviewPane();
        }

        public void Clear()
        {
            BufferGraphics.FillRectangle(EraseBrush, new Rectangle(new Point(0, 0), Buffer.Size));
        }

        public void InvalidateMockItemStimulus(IStimulus di)
        {
            if (!PerformingMultipleUpdate)
                BufferGraphics.FillRectangle(EraseBrush, CIAT.SaveFile.Layout.StimulusRectangle);

            if (di != null)
            {
                BufferGraphics.Clip = new Region(CIAT.SaveFile.Layout.StimulusRectangle);
                BufferGraphics.DrawImage(di.IATImage.theImage, CIAT.SaveFile.Layout.StimulusRectangle.Location +
                    new Size((CIAT.SaveFile.Layout.StimulusRectangle.Width - di.IATImage.ImageSize.Width) >> 1,
                        (CIAT.SaveFile.Layout.StimulusRectangle.Height - di.IATImage.ImageSize.Height) >> 1));
                BufferGraphics.ResetClip();
            }

            if (!PerformingMultipleUpdate)
                UpdatePreviewPane();
        }

        public void InvalidateMockItemInstructions(CMultiLineTextDisplayItem di)
        {
            if (!PerformingMultipleUpdate)
                BufferGraphics.FillRectangle(EraseBrush, CIAT.SaveFile.Layout.MockItemInstructionsRectangle);

            if (di != null)
            {
                BufferGraphics.Clip = new Region(CIAT.SaveFile.Layout.MockItemInstructionsRectangle);
                BufferGraphics.DrawImage(di.IATImage.theImage, CIAT.SaveFile.Layout.MockItemInstructionsRectangle.Location +
                    new Size((CIAT.SaveFile.Layout.MockItemInstructionsRectangle.Width - di.IATImage.ImageSize.Width) >> 1,
                        (CIAT.SaveFile.Layout.MockItemInstructionsRectangle.Height - di.IATImage.ImageSize.Height) >> 1));
                BufferGraphics.ResetClip();
            }

            if (!PerformingMultipleUpdate)
                UpdatePreviewPane();
        }

        public void InvalidateResponseKey(CIATKey Key)
        {
            if (!PerformingMultipleUpdate)
            {
                BufferGraphics.FillRectangle(EraseBrush, CIAT.SaveFile.Layout.LeftKeyValueRectangle);
                BufferGraphics.FillRectangle(EraseBrush, CIAT.SaveFile.Layout.RightKeyValueRectangle);
            }

            // if key is non-null, draw it
            if (Key != null)
            {
                BufferGraphics.Clip = new Region(CIAT.SaveFile.Layout.LeftKeyValueRectangle);
                BufferGraphics.DrawImage(Key.LeftValue.IATImage.theImage, CIAT.SaveFile.Layout.LeftKeyValueRectangle.Location +
                    new Size((CIAT.SaveFile.Layout.LeftKeyValueRectangle.Width - Key.LeftValue.IATImage.ImageSize.Width) >> 1,
                        (CIAT.SaveFile.Layout.LeftKeyValueRectangle.Height - Key.LeftValue.IATImage.ImageSize.Height) >> 1));
                BufferGraphics.Clip = new Region(CIAT.SaveFile.Layout.RightKeyValueRectangle);
                BufferGraphics.DrawImage(Key.RightValue.IATImage.theImage, CIAT.SaveFile.Layout.RightKeyValueRectangle.Location +
                    new Size((CIAT.SaveFile.Layout.RightKeyValueRectangle.Width - Key.RightValue.IATImage.ImageSize.Width) >> 1,
                        (CIAT.SaveFile.Layout.RightKeyValueRectangle.Height - Key.RightValue.IATImage.ImageSize.Height) >> 1));
                BufferGraphics.ResetClip();
            }

            // if not performing a multiple invalidation, updatte the preview image
            if (!PerformingMultipleUpdate)
                UpdatePreviewPane();
        }
        */
        public void InvalidateMockItemKeyedDirOutline(KeyedDirection keyedDir, bool dispOutline)
        {
            lock (lockObject)
            {
                Brush backBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                Brush OutlineBrush = new SolidBrush(CIAT.SaveFile.Layout.OutlineColor);
                Pen OutlinePen = new Pen(OutlineBrush, CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1);
                Pen BackgroundPen = new Pen(backBrush, CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1);

                // erase left response key outline
                Graphics g = Graphics.FromImage(PreviewPane.Image);
                g.Clip = new Region(Rectangle.Inflate(CIAT.SaveFile.Layout.LeftKeyValueRectangle,
                    CIAT.SaveFile.Layout.ResponseValueRectMargin, CIAT.SaveFile.Layout.ResponseValueRectMargin));
                g.DrawRectangle(BackgroundPen, Rectangle.Inflate(CIAT.SaveFile.Layout.LeftKeyValueRectangle,
                    CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1, CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1));
                g.ResetClip();

                // erase right respone key outline
                g.Clip = new Region(Rectangle.Inflate(CIAT.SaveFile.Layout.RightKeyValueRectangle,
                    CIAT.SaveFile.Layout.ResponseValueRectMargin, CIAT.SaveFile.Layout.ResponseValueRectMargin));
                g.DrawRectangle(BackgroundPen, Rectangle.Inflate(CIAT.SaveFile.Layout.RightKeyValueRectangle,
                    CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1, CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1));
                g.ResetClip();

                // draw response key outline where appropriate
                if (dispOutline)
                {
                    if (keyedDir == KeyedDirection.Left)
                    {
                        g.Clip = new Region(Rectangle.Inflate(CIAT.SaveFile.Layout.LeftKeyValueRectangle,
                            CIAT.SaveFile.Layout.ResponseValueRectMargin, CIAT.SaveFile.Layout.ResponseValueRectMargin));
                        g.DrawRectangle(OutlinePen, Rectangle.Inflate(CIAT.SaveFile.Layout.LeftKeyValueRectangle,
                            CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1, CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1));
                        g.ResetClip();
                    }
                    else if (keyedDir == KeyedDirection.Right)
                    {
                        g.Clip = new Region(Rectangle.Inflate(CIAT.SaveFile.Layout.RightKeyValueRectangle,
                            CIAT.SaveFile.Layout.ResponseValueRectMargin, CIAT.SaveFile.Layout.ResponseValueRectMargin));
                        g.DrawRectangle(OutlinePen, Rectangle.Inflate(CIAT.SaveFile.Layout.RightKeyValueRectangle,
                            CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1, CIAT.SaveFile.Layout.ResponseValueRectMargin >> 1));
                        g.ResetClip();
                    }
                }

                OutlinePen.Dispose();
                BackgroundPen.Dispose();
                backBrush.Dispose();
                OutlineBrush.Dispose();
            }

        }
        /*
        public void InvalidateMockItemInvalidResponse(bool dispInvalidResponseMark)
        {
            if (!PerformingMultipleUpdate)
                BufferGraphics.FillRectangle(EraseBrush, CIAT.SaveFile.Layout.ErrorRectangle);

            if (dispInvalidResponseMark)
            {
                BufferGraphics.DrawImage(CIAT.SaveFile.Layout.ErrorMarkBitmap, CIAT.SaveFile.Layout.ErrorRectangle.Location);
            }

            if (!PerformingMultipleUpdate)
                UpdatePreviewPane();
        }
*/
        public void InvalidatePreview(Image img)
        {
            lock (lockObject)
            {
                Graphics g = Graphics.FromImage(PreviewPane.Image);
                if (img == null)
                {
                    if (LastUpdateImage == null)
                    {
                        Brush br = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                        g.FillRectangle(br, 0, 0, PreviewPane.Image.Width, PreviewPane.Image.Height);
                        br.Dispose();
                    }
                    else
                        g.DrawImage(LastUpdateImage, PreviewRectangle);
                }
                else
                    g.DrawImage(img, PreviewRectangle);
                g.Dispose();
                PreviewPane.Invalidate();
                KeyedDirection mockKeyedDir = ((InstructionScreenPanel)Parent).GetVisibleKeyedDir();
                if (mockKeyedDir != KeyedDirection.None)
                    InvalidateMockItemKeyedDirOutline(mockKeyedDir, true);
            }
        }
/*
        private void UpdatePreviewPane()
        {
            PreviewGraphics.DrawImage(Buffer, PreviewRectangle);
            PreviewPane.Invalidate();
            if (OnPreviewUpdate != null)
                OnPreviewUpdate();
        }
 * */
    }
}
