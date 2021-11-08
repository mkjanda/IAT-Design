using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace IATClient
{

    public interface IImage
    {
        Image Image { get; }
        Size Size { get; }
        System.Drawing.Imaging.ImageFormat Format { get; }
        void Load();
        void Update(Image img);
        void Delete();
        void Commit();
    }


    public interface IImageSource
    {
        void Validate();
        void Invalidate();
 //       object LockObject { get; }
        bool IsValid { get; }
        IIATImage IATImage { get; }
 //       bool ImageExists { get; }
    }

    public interface IComponentImageSource : IImageSource
    {
        CComponentImage.ESourceType SourceType { get; }
    }

    public interface IIATImage : IDisposable
    {
        void InvalidateThumbnail();
        Size ImageSize { get; }
        void Resize(Size sz);
        void PerformResize(Size sz);
//        IIATImage CreateCopy();
        void DestroyThumbnail();
        IImage theImage { get; }
        Size OriginalImageSize { get; }
        bool IsUserImage { get; }
        Size BoundingSize { get; }
        void CreateThumbnail();
    }

    public interface IUserImage : IIATImage
    {
        String Description { get; }
        String FullFilePath { get; }
        void SetSizeCallback(ImageManager.ImageSizeCallback callback);
    }

    public interface INonUserImage : IIATImage, IImageSource
    {
        void Dispose(ImageManager.INonUserImageSource source);
    //    void Commit();
        void InvalidateSource(ImageManager.INonUserImageSource source);
        void Invalidate();
        void InvalidateNow();
    }

    interface ICompositeImage : IIATImage, IImageSource
    {
        void UpdateImage(Image img);
    }

    public delegate void ThumbnailNotification(Image img);
}
