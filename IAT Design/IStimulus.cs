using System;

namespace IATClient
{
    public interface IStimulus : IDisposable, IThumbnailPreviewable, IPackagePart
    {
        String Description { get; }
        Images.IImage IImage { get; }
        DIType Type { get; }
        IUri IUri { get; }
        void ScheduleInvalidation();
    }
}
