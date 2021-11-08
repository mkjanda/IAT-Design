using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace IATClient
{
    public class PurchaseOrder
    {
        int _NumTests = 0, _NumAdministrations = 0, _DiskSpace = 0, _Total = 0;

        public int NumTests
        {
            get
            {
                return _NumTests;
            }
            set
            {
                _NumTests = value;
            }
        }

        public int NumAdministrations
        {
            get
            {
                return _NumAdministrations;
            }
            set
            {
                _NumAdministrations = value;
            }
        }

        public int DiskSpace
        {
            get
            {
                return _DiskSpace;
            }
            set
            {
                _DiskSpace = value;
            }
        }

        public int Total
        {
            get
            {
                return _Total;
            }
            set
            {
                _Total = value;
            }
        }

        public PurchaseOrder() { }

        public void WriteXml(XmlWriter xWriter)
        {
            xWriter.WriteStartElement("PurchaseOrder");
            xWriter.WriteElementString("NumTests", NumTests.ToString());
            xWriter.WriteElementString("NumAdministrations", NumAdministrations.ToString());
            xWriter.WriteElementString("DiskSpace", DiskSpace.ToString());
            xWriter.WriteElementString("Total", Total.ToString());
            xWriter.WriteEndElement();
        }
    }
}
