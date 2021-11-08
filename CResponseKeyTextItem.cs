using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Drawing;

namespace IATClient
{
    class CResponseKeyTextItem : CTextDisplayItem, IComponentImageSource
    {
        private System.Threading.Timer timer;
        private Control InvokeTarget = null;
        private bool bHaltFlag = false;

        public CComponentImage.ESourceType SourceType
        {
            get
            {
                return CComponentImage.ESourceType.responseKey;
            }
        }

        private bool HaltFlag
        {
            get
            {
                return bHaltFlag;
            }
            set
            {
                bHaltFlag = value;
            }
        }

        /*
        public void InvalidateKeyValue()
        {
            ((INonUserImage)IATImage).Invalidate(this);
        }
*/
        public CResponseKeyTextItem() : base(CTextDisplayItem.EUsedAs.responseKey) 
        {
        }

        private object InvalidateLock = new object();
        private bool _PreviewValid = false;

        public bool PreviewValid
        {
            get
            {
                lock (InvalidateLock)
                    return _PreviewValid;
            }
            set
            {
                lock (InvalidateLock)
                    _PreviewValid = value;
            }
        }

        private void InvalidateProc()
        {
            lock (InvalidateLock)
                PreviewValid = false;
        }

        public void SetInvokeTarget(Control invokeTarget)
        {
            InvokeTarget = invokeTarget;
        }
        /*
        public void OpenForEditing()
        {
            HaltFlag = false;
            if ((InvokeTarget == null) || (Update == null))
                throw new Exception("Cannot open CResponseKeyTextItem for dispaly with null value InvokeTarget or Update functions");
            ThreadStart proc = new ThreadStart(UpdateProc);
            Thread th = new Thread(proc);
            th.Start();
        }
        */
        private void TimerTick(Object o)
        {
        }
        /*
        private void UpdateProc()
        {
            while (!HaltFlag)
            {
                lock (InvalidateLock)
                {
                    if (!PreviewValid)
                    {
                        Image img = null;
                        IATImage.Lock();
                        img = new Bitmap(IATImage.theImage);
                        IATImage.Unlock();
                        Image previewImg = new Bitmap(CIAT.Layout.KeyValueSize.Width, CIAT.Layout.KeyValueSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        Graphics g2 = Graphics.FromImage(previewImg);
                        g2.DrawImage(img, new Point((previewImg.Width - img.Width) >> 1, (previewImg.Height - img.Height) >> 1));
                        g2.Dispose();
                        Image finalImg = new Bitmap(KeyPreviewPanel.PreviewSize.Width, KeyPreviewPanel.PreviewSize.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
                        Brush br = new SolidBrush(CIAT.Layout.BackColor);
                        Graphics g = Graphics.FromImage(finalImg);
                        g.FillRectangle(br, new Rectangle(0, 0, finalImg.Width, finalImg.Height));
                        double arText = (double)previewImg.Width / (double)previewImg.Height;
                        double arImg = (double)finalImg.Width / (double)finalImg.Height;
                        Size sz;
                        if (arText > arImg)
                            sz = new Size(finalImg.Width, (int)((double)finalImg.Width * ((double)previewImg.Height / (double)previewImg.Width)));
                        else
                            sz = new Size((int)((double)finalImg.Height * ((double)previewImg.Width / (double)previewImg.Height)), finalImg.Height);
                        g.DrawImage(previewImg, new Rectangle(new Point((finalImg.Width - sz.Width) >> 1, (finalImg.Height - sz.Height) >> 1), sz));
                        g.Dispose();
                        img.Dispose();
                        if (!HaltFlag)
                            InvokeTarget.Invoke(Update, finalImg);
                        PreviewValid = true;
                        finalImg.Dispose();
                        previewImg.Dispose();
                    }
                }
                Thread.Sleep(25);
            }
            if ((InvokeTarget.IsHandleCreated) && (!IATConfigMainForm.IsInShutdown))
                InvokeTarget.Invoke(Update, (Object)null);
        }
        */
        public void CloseForDisplay()
        {
            HaltFlag = true;
        }

        public override void Dispose()
        {
            HaltFlag = true;
            base.Dispose();
        }
    }
}
