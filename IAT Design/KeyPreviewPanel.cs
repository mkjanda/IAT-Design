using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{

    partial class KeyPreviewPanel : UserControl
    {
        // buffers for the left and right preview panes and the preview window
        private Graphics LeftBufferGraphics, RightBufferGraphics;
        private Bitmap LeftBuffer, RightBuffer;
        private Brush EraseBrush;
        private DIBase LDI = null, RDI = null;
        private bool LDIChanged = false, RDIChanged = false;
        private PictureBox LeftResponsePreview, RightResponsePreview;
        private static Size _PreviewSize = new Size(225, 150);
        public static Size PreviewSize
        {
            get
            {
                return _PreviewSize;
            }
        }

        public KeyPreviewPanel()
        {
            InitializeComponent();
            LeftResponsePreview = new PictureBox();
            this.LeftResponsePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.LeftResponsePreview.Location = new System.Drawing.Point(3, 16);
            this.LeftResponsePreview.Name = "LeftResponsePreview";
            this.LeftResponsePreview.Size = PreviewSize;
            this.LeftResponsePreview.TabIndex = 0;
            this.LeftResponsePreview.TabStop = false;
            RightResponsePreview = new PictureBox();
            this.RightResponsePreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RightResponsePreview.Location = new System.Drawing.Point(3, 16);
            this.RightResponsePreview.Name = "RightResponsePreview";
            this.RightResponsePreview.Size = PreviewSize;
            this.RightResponsePreview.TabIndex = 1;
            this.RightResponsePreview.TabStop = false;
            LeftResponsePreviewGroup.Controls.Add(LeftResponsePreview);
            RightResponsePreviewGroup.Controls.Add(RightResponsePreview);
            Controls.Add(LeftResponsePreviewGroup);
            Controls.Add(RightResponsePreviewGroup);
            LeftResponsePreview.BackColor = CIAT.SaveFile.Layout.BackColor;
            LeftResponsePreview.Image = new Bitmap(LeftResponsePreview.ClientRectangle.Width, LeftResponsePreview.ClientRectangle.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
            RightResponsePreview.BackColor = CIAT.SaveFile.Layout.BackColor;
            RightResponsePreview.Image = new Bitmap(RightResponsePreview.ClientRectangle.Width, RightResponsePreview.ClientRectangle.Height, System.Drawing.Imaging.PixelFormat.Format32bppRgb);
        }
        /*      
              public void UpdateLeftPane(CDisplayItem di)
              {
                  LDI = di;
                  LDIChanged = true;
              }



              public void UpdateRightPane(CDisplayItem di)
              {
                  RDI = di;
                  RDIChanged = true;
              }



              /// <summary>
              /// Disposes of all graphics objects used by the control
              /// </summary>
              public void DisposeOfGraphics()
              {
                  if (LeftBuffer != null)
                  {
                      LeftBuffer.Dispose();
                      LeftBuffer = null;
                      RightBuffer.Dispose();
                      RightBuffer = null;
                      LeftBufferGraphics.Dispose();
                      LeftBufferGraphics = null;
                      RightBufferGraphics.Dispose();
                      RightBufferGraphics = null;
                      EraseBrush.Dispose();
                      EraseBrush = null;

                  }
              }
      /*        
              /// <summary>
              /// Constructs graphics objects used by the control
              /// </summary>
              public void ConstructGraphics()
              {
                  Graphics g = Graphics.FromHwnd(this.Handle);
                  LeftResponsePreview.BackColor = CIAT.SaveFile.Layout.BackColor;
                  LeftResponsePreview.Image = new Bitmap(LeftResponsePreview.ClientRectangle.Width, LeftResponsePreview.ClientRectangle.Height, g);
                  RightResponsePreview.BackColor = CIAT.SaveFile.Layout.BackColor;
                  RightResponsePreview.Image = new Bitmap(RightResponsePreview.ClientRectangle.Width, RightResponsePreview.ClientRectangle.Height, g);
                  LeftBufferGraphics = Graphics.FromImage(LeftResponsePreview.Image);
                  RightBufferGraphics = Graphics.FromImage(RightResponsePreview.Image);
                  EraseBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                  g.Dispose();
              }

              System.Windows.Forms.Timer UpdateTimer;
              */
        public void UpdateLeftPreview(Image img)
        {
            Graphics g = Graphics.FromImage(LeftResponsePreview.Image);
            if (img == null)
            {
                Brush br = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                g.FillRectangle(br, new Rectangle(0, 0, LeftResponsePreview.Image.Width, LeftResponsePreview.Image.Height));
                br.Dispose();
            }
            else
                g.DrawImage(img, new Point((PreviewSize.Width - img.Width) >> 1, (PreviewSize.Height - img.Height) >> 1));
            LeftResponsePreview.Invalidate();
            g.Dispose();
        }

        public void UpdateRightPreview(Image img)
        {
            Graphics g = Graphics.FromImage(RightResponsePreview.Image);
            if (img == null)
            {
                Brush br = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                g.FillRectangle(br, new Rectangle(0, 0, RightResponsePreview.Image.Width, RightResponsePreview.Image.Height));
                br.Dispose();
            }
            else
                g.DrawImage(img, new Point((PreviewSize.Width - img.Width) >> 1, (PreviewSize.Height - img.Height) >> 1));
            RightResponsePreview.Invalidate();
            g.Dispose();
        }

        /*
                public void UpdatePreview(object sender, EventArgs e)
                {
                    if (LDIChanged && (LDI == null))
                    {
                        Brush eraseBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                        LeftBufferGraphics.FillRectangle(eraseBrush, new Rectangle(0, 0, LeftResponsePreview.Image.Width, LeftResponsePreview.Image.Height));
                        eraseBrush.Dispose();
                        LeftResponsePreview.Invalidate();
                    }
                    else if (LDI == null) { }
                    else if (LDIChanged || !LDI.ComponentImageValid)
                    {
                        Brush eraseBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                        LeftBufferGraphics.FillRectangle(eraseBrush, new Rectangle(0, 0, LeftResponsePreview.Image.Width, LeftResponsePreview.Image.Height));
                        LDI.IATImage.Lock();
                        double arImage = (double)LDI.IATImage.ImageSize.Width / (double)LDI.IATImage.ImageSize.Height;
                        double arWin = (double)LeftResponsePreview.Width / (double)LeftResponsePreview.Height;
                        double scale;
                        if (arImage > arWin)
                            scale = (double)LeftResponsePreview.Width / (double)CIAT.SaveFile.Layout.KeyValueSize.Width;
                        else
                            scale = (double)LeftResponsePreview.Height / (double)CIAT.SaveFile.Layout.KeyValueSize.Height;
                        Size sz = new Size((int)(LDI.IATImage.ImageSize.Width * scale), (int)(LDI.IATImage.ImageSize.Height * scale));
                        Point loc = new Point((LeftResponsePreview.Width - sz.Width) >> 1, (LeftResponsePreview.Height - sz.Height) >> 1);
                        LeftBufferGraphics.DrawImage(LDI.IATImage.theImage, new Rectangle(loc, sz));
                        LDI.IATImage.Unlock();
                        eraseBrush.Dispose();
                        LDI.ValidateComponentImage();
                        LeftResponsePreview.Invalidate();

                    }
                    if (RDIChanged && (RDI == null))
                    {
                        Brush eraseBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                        RightBufferGraphics.FillRectangle(eraseBrush, new Rectangle(0, 0, RightResponsePreview.Image.Width, RightResponsePreview.Image.Height));
                        eraseBrush.Dispose();
                        RightResponsePreview.Invalidate();
                    }
                    else if (RDI == null) { }
                    else if (RDIChanged || !RDI.ComponentImageValid)
                    {
                        Brush eraseBrush = new SolidBrush(CIAT.SaveFile.Layout.BackColor);
                        RightBufferGraphics.FillRectangle(eraseBrush, new Rectangle(0, 0, RightResponsePreview.Image.Width, RightResponsePreview.Image.Height));
                        RDI.IATImage.Lock();
                        double arImage = (double)RDI.IATImage.ImageSize.Width / (double)RDI.IATImage.ImageSize.Height;
                        double arWin = (double)RightResponsePreview.Width / (double)RightResponsePreview.Height;
                        double scale;
                        if (arImage > arWin)
                            scale = (double)RightResponsePreview.Width / (double)CIAT.SaveFile.Layout.KeyValueSize.Width;
                        else
                            scale = (double)RightResponsePreview.Height / (double)CIAT.SaveFile.Layout.KeyValueSize.Height;
                        Size sz = new Size((int)(RDI.IATImage.ImageSize.Width * scale), (int)(RDI.IATImage.ImageSize.Height * scale));
                        Point loc = new Point((RightResponsePreview.Width - sz.Width) >> 1, (RightResponsePreview.Height - sz.Height) >> 1);
                        RightBufferGraphics.DrawImage(RDI.IATImage.theImage, new Rectangle(loc, sz));
                        RDI.IATImage.Unlock();
                        eraseBrush.Dispose();
                        RDI.ValidateComponentImage();
                        RightResponsePreview.Invalidate();
                    }
                    LDIChanged = false;
                    RDIChanged = false;
                }
                /*
                public void StartUpdateTimer()
                {
                    UpdateTimer = new System.Windows.Forms.Timer();
                    UpdateTimer.Tick += new EventHandler(UpdatePreview);
                    UpdateTimer.Interval = 10;
                    UpdateTimer.Start();
                }

                public void StopTimer()
                {
                    UpdateTimer.Stop();
                }
                 * */

    }
}
