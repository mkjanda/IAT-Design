using System;
using System.Drawing;
using System.Windows.Forms;

namespace IATClient
{
    public interface IImageDisplay : IDisposable
    {
        void SetImage(Images.IImageMedia image);
        void SetImage(Bitmap bmp);
        event EventHandler HandleCreated;
        bool IsDisposed { get; }
        bool IsHandleCreated { get; }
        object Tag { get; set; }
        IAsyncResult BeginInvoke(Delegate d);
        int Height { get; set; }
        int Width { get; set; }
        Point Location { get; }
        void SuspendLayout();
        void ResumeLayout(bool b);
        Control.ControlCollection Controls { get; }
        IntPtr Handle { get; }
        Size Size { get; }
    }

    public interface ISurveyImageDisplay : IImageDisplay {
        new int Height { get; set; }
    }
}
