using System;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public class SurveyImageDisplay : ImageDisplay, ISurveyItemDisplay, ISurveyImageDisplay
    {
        public bool IsUnique { get { return false; } set { throw new InvalidOperationException(); } }
        public bool Selected { get; set; }
        public bool Active { get; set; }
        private CSurvey Survey { get; set; }
        public Action OnHeightChanged { get; set; } = null;
        private CSurveyItemImage SII = null;
        private Size LastSize { get; set; } = Size.Empty;
        public CSurveyItem SurveyItem
        {
            get
            {
                if (SII == null)
                    SII = new CSurveyItemImage(this.Tag as DISurveyImage);
                return SII;
            }
            set
            {
                SII = value as CSurveyItemImage;
                var ar = (double)this.Width / (double)this.Height;
                var origImg = SII.SurveyImage.IImage.OriginalImage.Img;
                var siiAr = (double)SII.SurveyImage.IImage.OriginalSize.Width / SII.SurveyImage.IImage.OriginalSize.Height;
                SII.SurveyImage.PreviewPanel = this;
                SII.SurveyImage.IImage.Resize(new Size(origImg.Width, (int)(origImg.Width / siiAr)));
            }
        }

        public new int Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                if (!base.IsHandleCreated)
                {
                    base.HandleCreated += (sender, args) =>
                    {
                        if (base.Height == value)
                            return;
                        base.Height = value;
                    };
                }
                else
                {
                    this.BeginInvoke(new Action(() =>
                    {
                        if (base.Height == value)
                            return;
                        base.Height = value;
                    }));
                }
            }
        }


        public new int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                if (base.Width == value)
                    return;
                base.Width = value;
            }
        }

        public Task<int> RecalcSize(bool children)
        {
            if (SII == null)
                return Task.Run(() => 0);
            Size siiSz = SII.SurveyImage.IImage.OriginalSize;
            double siiAr = (double)siiSz.Width / (double)siiSz.Height;
            this.Height = (int)(Math.Min(this.Width, SII.SurveyImage.IImage.Size.Width) / siiAr);
            (Parent as SurveyDisplay).RecalcSize(false);
            return Task.Run(() => ImageBox.Height);
        }

        public SurveyImageDisplay()
        {
            ImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            ImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            ImageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            ImageBox.BackColor = System.Drawing.Color.White;
            ImageBox.Click += new EventHandler((sender, args) =>
            {
                (Parent as SurveyDisplay)?.SelectionChanged(this, ModifierKeys);
            });
            this.HandleCreated += (sender, args) =>
            {
                this.Width = Parent.Width;
                if (SII != null)
                {
                    var ar = (double)this.Width / (double)this.Height;
                    var origImg = SII.SurveyImage.IImage.OriginalImage.Img;
                    var siiAr = (double)SII.SurveyImage.IImage.OriginalSize.Width / SII.SurveyImage.IImage.OriginalSize.Height;
                    SII.SurveyImage.PreviewPanel = this;
                    SII.SurveyImage.IImage.Resize(new Size(origImg.Width, (int)(origImg.Width / siiAr)));
                }
            };
            this.BackColor = Color.White;
        }


        public override void SetImage(Images.IImageMedia image)
        {
            RecalcSize(false);
            var img = image.Img;
            if (!this.IsHandleCreated)
            {
                this.HandleCreated += (sender, args) =>
                {
                    bool bResize = true;
                    if ((img != null) && (ImageBox.Image != null))
                        bResize = (ImageBox.Image.Size.Height != img.Size.Height) ? true : false;
                    ImageBox.Image = img;
                    //                if (bResize)
                    //                  this.Size = img.Size;
                };
            }
            else
            {
                this.BeginInvoke(new Action(() =>
                {
                    bool bResize = true;
                    if ((img != null) && (ImageBox.Image != null))
                        bResize = (ImageBox.Image.Size.Height != img.Size.Height) ? true : false;
                    ImageBox.Image = img;
                    //                    if (bResize)
                    //                      this.Size = img.Size;
                }));
            }
        }
    }
}
