using System;
using System.Drawing;
using System.Threading.Tasks;

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
                Size siiSz = SII.SurveyImage.IImage.OriginalSize;
                double siiAr = (double)siiSz.Width / (double)siiSz.Height;
                if (siiSz.Width > Width)
                {
                    this.Height = (int)(this.Width / siiAr);
                }
                SII.SurveyImage.PreviewPanel = this;
                SII.SurveyImage.IImage.Resize(Size);

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
                    if (base.Height == value)
                        return;
                    base.Height = value;
                }
            }
        }

        public new int Width
        {
            get
            {
                return base.Width;
            }
            set {
                if (base.Width == value)
                    return;
                base.Width = value;
            }
        }

        public Task<int> RecalcSize(bool recalcChildren)
        {
            Size siiSz = SII.SurveyImage.IImage.OriginalSize;
            double siiAr = (double)siiSz.Width / (double)siiSz.Height;
            if (siiSz.Width > Width)
            {
                this.Height = (int)(this.Width / siiAr);
            }
            SII.SurveyImage.PreviewPanel = this;
            SII.SurveyImage.IImage.Resize(Size);
            return Task.Run(() => ImageBox.Height);
        }

        public SurveyImageDisplay()
        {
            ImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            ImageBox.Dock = System.Windows.Forms.DockStyle.Fill;
            ImageBox.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.Resize += new EventHandler((sender, args) =>
            {
                RecalcSize(true);
            });
            ImageBox.Click += new EventHandler((sender, args) =>
            {
                (Parent as SurveyDisplay)?.SelectionChanged(this, ModifierKeys);
            });
            this.BackColor = Color.White;
        }
        

        public override void SetImage(Images.IImageMedia image)
        {
            var img = image.Img;
            this.BeginInvoke(new Action(() =>
            {
                bool bResize = true;
                if ((img != null) && (ImageBox.Image != null))
                    bResize = (ImageBox.Image.Size.Height != img.Size.Height) ? true : false;
                ImageBox.Image = img;
                if (bResize)
                    this.Size = img.Size;
            }));
        } 
    }
}
