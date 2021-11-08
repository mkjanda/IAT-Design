using System;
using System.Drawing;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using System.Linq;
using System.Threading.Tasks;

namespace IATClient
{
    public abstract class DIImage : DIBase
    {
        public String Description { get; set; } = String.Empty;

        private void IImageChanged(Images.ImageChangedEventArgs args)
        {
        }

        public DIImage() : base()
        {
        }

        public DIImage(Uri uri) : base(uri) 
        {
        }

        public DIImage(Images.IImage img) : base(img)
        {
            Description = String.Empty;
        }

        public virtual void LoadImageFromFile(String path)
        {
            lock (lockObject)
            {
                Regex r = new Regex(@"\.([A-Za-z]+)$");
                if (!r.IsMatch(path))
                {
                    MessageBox.Show("Invalid image file name", "Error");
                    return;
                }
                String ext = r.Match(path).Groups[1].Value;
                System.Drawing.Imaging.ImageFormat format = Images.ImageFormat.FromExtension(ext.ToLower()).Format;
                Image img = Image.FromFile(path);
                AbsoluteBounds = new Rectangle(new Point(0, 0), img.Size);
                img.Tag = Images.ImageMediaType.VariableSize;
                if (IImage != null)
                {
                    CIAT.SaveFile.DeleteRelationship(URI, IImage.URI);
                    IImage?.Dispose();
                    IImage = null;
                }
                SetImage(img, format);
                Description = System.IO.Path.GetFileName(path);
            }
            CIAT.ActivityLog.LogEvent(ActivityLog.EventType.ImageLoad, URI, 
                new String[] { "File", "Size" }, new String[] { path.Substring(path.LastIndexOf(Path.DirectorySeparatorChar) + 1), new FileInfo(path).Length.ToString() });
        }

        protected override void OnImageChanged(Images.ImageChangedEvent evt, Images.IImageMedia img, object arg)
        {
            base.OnImageChanged(evt, img, arg);
            if (evt == Images.ImageChangedEvent.Resized)
            {
                AbsoluteBounds = (Rectangle)arg;
            }
        }

        public override Size ItemSize
        {
            get
            {
                return this.IImage.Size;
            }
        }

        public override Rectangle AbsoluteBounds { get; protected set; }
 /*       {
            get
            {
                return AbsoluteBounds;
//                return new Rectangle(new Point((BoundingSize.Width - IImage.Size.Width) >> 1, (BoundingSize.Height - IImage.Size.Height) >> 1), IImage.Size);
            }
        }*/
        protected override void Invalidate()
        {
            if (!Resize())
                Validate();
        }

        public double AspectRatio
        {
            get
            {
                return (double)this.IImage.Size.Width / (double)this.IImage.Size.Height;
            }
        }
    }
}
