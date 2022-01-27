using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;

namespace IATClient.Messages
{

    public class ActivationResponse : INamedXmlSerializable
    {
        public enum EResult
        {
            Unset, NoSuchClient, InvalidRequest, ServerFailure, NoActivationsRemaining, InvalidProductCode, ClientFrozen, ClientDeleted, CannotConnect, Success,
            EmailAlreadyVerified
        };
        private EResult _Result;
        private String _VerificationCode, _ProductKey, _Province, _PostalCode, _Phone, _Name, _EMail, _Country, _City, _Address1, _Address2;
        private int _UserNum;

        public ActivationResponse() { }

        public String VerificationCode
        {
            get
            {
                return _VerificationCode;
            }
        }

        public String ProductKey
        {
            get
            {
                return _ProductKey;
            }
        }

        public String ClientProvince
        {
            get
            {
                return _Province;
            }
        }

        public String ClientPostalCode
        {
            get
            {
                return _PostalCode;
            }
        }

        public String ClientPhone
        {
            get
            {
                return _Phone;
            }
        }

        public String ClientName
        {
            get
            {
                return _Name;
            }
        }

        public String ClientEMail
        {
            get
            {
                return _EMail;
            }
        }

        public String ClientCity
        {
            get
            {
                return _City;
            }
        }

        public String ClientCountry
        {
            get
            {
                return _Country;
            }
        }

        public String ClientAddress1
        {
            get
            {
                return _Address1;
            }
        }

        public String ClientAddress2
        {
            get
            {
                return _Address2;
            }
        }

        public EResult Result
        {
            get
            {
                return _Result;
            }
            set
            {
                _Result = value;
            }
        }

        public String GetName()
        {
            return "ActivationResponse";
        }

        public void WriteXml(XmlWriter xWriter)
        {
            throw new NotImplementedException();
        }

        public void ReadXml(XmlReader xReader)
        {
            xReader.ReadStartElement("ActivationResponse");
            _Result = (EResult)Enum.Parse(typeof(EResult), xReader.ReadElementString("ActivationResult"));
            _ProductKey = xReader.ReadElementString("ProductKey");
            _VerificationCode = xReader.ReadElementString("VerificationCode");
            _Name = xReader.ReadElementString("ClientName");
            _EMail = xReader.ReadElementString("ClientEMail");
            _Phone = xReader.ReadElementString("Phone");
            _Address1 = xReader.ReadElementString("Address1");
            _Address2 = xReader.ReadElementString("Address2");
            _City = xReader.ReadElementString("City");
            _Province = xReader.ReadElementString("Province");
            _PostalCode = xReader.ReadElementString("PostalCode");
            _Country = xReader.ReadElementString("Country");
            xReader.ReadEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
