using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public interface IResponseKeyDI
    {
        void AddKeyOwner(IPackagePart pp);
        void SetKeyOwners(List<IPackagePart> PPs);
        void ReleaseKeyOwner(IPackagePart pp);
        void ReleaseKeyOwners(List<IPackagePart> PPs);
        List<Uri> KeyOwners { get; }
        IImageDisplay PreviewPanel { get; set; }
        Action ValidateData { get; set; }
        DIType Type { get; }
        Uri URI { get; }
        Type BaseType { get; }
    }
}
