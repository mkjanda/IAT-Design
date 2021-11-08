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
                SII.SurveyImage.PreviewPanel = this;
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
                if (base.Height == value)
                    return;
                if (!base.IsHandleCreated)
                {
                    base.HandleCreated += (sender, args) =>
                    {
                        base.Height = value;
                        OnHeightChanged?.Invoke();
                    };
                }
                else
                {
                    base.BeginInvoke(new Action(() =>
                    {
                        base.Height = value;
                        OnHeightChanged?.Invoke();
                    }));
                }
            }
        }

        public void RecalcSize() { }

        public SurveyImageDisplay()
        {
            ImageBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.Resize += new EventHandler((sender, args) =>
            {
                if ((SII != null) && ((LastSize.Width != Size.Width) || (LastSize.Height != Size.Height)))
                {
                    SII.SurveyImage.IImage.Resize(new Size(this.Size.Width, SII.SurveyImage.IImage.OriginalSize.Height));
                    LastSize = Size;
                }
            });
            ImageBox.Click += new EventHandler((sender, args) =>
            {
                (Parent as SurveyDisplay)?.SelectionChanged(this, ModifierKeys);
            });
        }

        public override void SetImage(Images.IImageMedia image)
        {
            if (image == null)
            {
                ImageBox.SuspendLayout();
                Image i = ImageBox.Image;
                ImageBox.BackColor = BackColor;
                if (i != null)
                {
                    ImageBox.Image = null;
                    i.Dispose();
                }
                ImageBox.Image = null;
                ImageBox.ResumeLayout(false);
                Invalidate();
                return;
            }
            if (IsHandleCreated)
            {
                this.Invoke(new Action(() =>
                {
                    if (ImageBox.IsHandleCreated)
                    {
                        Image i = ImageBox.Image;
                        ImageBox.SuspendLayout();
                        ImageBox.BackColor = BackColor;
                        if (i != null)
                        {
                            ImageBox.Image = null;
                            i.Dispose();
                        }
                        ImageBox.Image = image.Image;
                        ImageBox.ResumeLayout(true);
                    }
                    else ImageBox.HandleCreated += (sender, args) =>
                    {
                        ImageBox.BackColor = BackColor;
                        ImageBox.Image = image.Image;

                    };
                }));
            }
            else HandleCreated += (sender, args) => {
                if (ImageBox.IsHandleCreated)
                {
                    Image i = ImageBox.Image;
                    ImageBox.SuspendLayout();
                    ImageBox.BackColor = BackColor;
                    if (i != null)
                    {
                        ImageBox.Image = null;
                        i.Dispose();
                    }
                    ImageBox.Image = image.Image;
                    ImageBox.ResumeLayout(true);
                }
                else ImageBox.HandleCreated += (s, a) =>
                {
                    ImageBox.BackColor = BackColor;
                    ImageBox.Image = image.Image;

                };
            };
        }
    }
}
