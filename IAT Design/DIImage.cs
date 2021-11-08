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
        public static readonly int MaxFileSize = 10485760;
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
                Image img = Image.FromFile(path);
//                AbsoluteBounds = new Rectangle(new Point(0, 0), img.Size);
                img.Tag = Images.ImageMediaType.VariableSize;
                if (IImage != null)
                {
                    //CIAT.SaveFile.DeleteRelationship(URI, IImage.URI);
                    IImage?.Dispose();
                    IImage = null;
                }
                this.IImage = new Images.ImageManager.Image(img, Images.ImageFormat.FromExtension(ext.ToLower()), Type);
                rImageId = CIAT.SaveFile.GetRelationship(CIAT.SaveFile.ImageMetaDataDocument, IImage);
                this.IImage.Changed += (evt, img, args) => OnImageEvent(evt, img, args);
                OnImageEvent(Images.ImageEvent.Updated, IImage, null);
                ScheduleInvalidation();
                Description = System.IO.Path.GetFileName(path);
            }
            CIAT.SaveFile.ActivityLog.LogEvent(ActivityLog.EventType.ImageLoad, URI, 
                new String[] { "File", "Size" }, new String[] { path.Substring(path.LastIndexOf(Path.DirectorySeparatorChar) + 1), new FileInfo(path).Length.ToString() });
        }

        protected override void OnImageEvent(Images.ImageEvent evt, Images.IImageMedia img, object arg)
        {
            base.OnImageEvent(evt, img, arg);
            if (evt == Images.ImageEvent.Resized)
            {
                AbsoluteBounds = (Rectangle)arg;
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
