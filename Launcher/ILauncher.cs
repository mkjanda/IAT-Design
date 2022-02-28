using System.Runtime.InteropServices;

namespace Launcher
{
    [ComImport()]
    [ComVisible(true)]
    [Guid("19D42AB0-9C98-4115-B725-15A5B04BC3B0")]
    public interface ILauncher
    {
        void Launch();
    }
}
