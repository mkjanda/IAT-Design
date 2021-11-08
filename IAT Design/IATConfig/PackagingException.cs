using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IATClient.IATConfig
{
    public class PackagingException : Exception
    {
        public PackagingException(String msg)
            : base(msg)
        { }
    }
}
