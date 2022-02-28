using System;

namespace IATClient.IATConfig
{
    public class PackagingException : Exception
    {
        public PackagingException(String msg)
            : base(msg)
        { }
    }
}
