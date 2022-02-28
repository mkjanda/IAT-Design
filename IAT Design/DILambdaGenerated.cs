using System;
using System.Drawing;

namespace IATClient
{
    public class DILambdaGenerated : DIGenerated
    {
        private Action<Bitmap> GenerateImage;

        public DILambdaGenerated(Action<Bitmap> generateImage)
        {
            GenerateImage = generateImage;
            Generate();
        }

        protected override Bitmap Generate()
        {
            if (Broken || IsDisposed)
                return null;
            Bitmap bmp = CIAT.ImageManager.RequestBitmap(Images.ImageMediaType.FullWindow);
            GenerateImage(bmp);
            return bmp;
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
