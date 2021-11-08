using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IATClient
{
    public interface IThumbnailPreviewable
    {
        IImageDisplay ThumbnailPreviewPanel { get; set; }
    }
}
