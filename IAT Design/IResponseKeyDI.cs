using System;
using System.Collections.Generic;

namespace IATClient
{
    public interface IResponseKeyDI
    {
        void AddKeyOwner(IPackagePart pp);
        void SetKeyOwners(List<CIATKey> PPs);
        void ReleaseKeyOwner(IPackagePart pp);
        void ReleaseKeyOwners(List<CIATKey> PPs);
        List<CIATKey> KeyOwners { get; }
        IImageDisplay PreviewPanel { get; set; }
        Action ValidateData { get; set; }
        DIType Type { get; }
        Uri URI { get; }
        Type BaseType { get; }
        void ScheduleInvalidation();
    }
}
