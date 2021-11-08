using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace IATClient
{
    public class DILambdaGenerated : DIGenerated
    {
        private Action<Bitmap> GenerateImage;

        public DILambdaGenerated(Action<Bitmap> generateImage)
        {
            GenerateImage = generateImage;
        }

        protected override bool Generate()
        {
            if (Broken || IsDisposed)
                return true;
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FullWindow);
            GenerateImage(bmp);
            SetImage(bmp, System.Drawing.Imaging.ImageFormat.Png);
            return true;
        }

        public override void Save()
        {
        }

        protected override void DoLoad(Uri uri)
        {
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
