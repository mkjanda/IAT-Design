using System;
using System.Collections.Generic;

using System.Text;
using System.Xml;

namespace IATClient
{
    class ActivationConfirmation : INamedXmlSerializable
    {
        private String _ClientName = String.Empty, _ProductKey = String.Empty, _ClientAddress1 = String.Empty, _ClientAddress2 = String.Empty, _ClientPhone = String.Empty;
        private String _ClientEMail = String.Empty, _ClientCity = String.Empty, _ClientPostalCode = String.Empty, _ClientCountry = String.Empty, _ClientProvince = String.Empty;
        private String _ConfirmationCode = String.Empty, _UserKey = String.Empty;

        public enum EActivationResult { ActivationNotAttempted, ServerFailure, NoActivationsRemaining, InvalidProductCode, Success };
        private EActivationResult _ActivationResult = EActivationResult.ActivationNotAttempted;

        public ActivationConfirmation()
        {
            _ProductKey = "FAILED";
        }

        public String UserKey
        {
            get
            {
                return _UserKey;
            }
        }

        public String ClientName
        {
            get
            {
                return _ClientName;
            }
        }

        public String ProductKey
        {
            get
            {
                return _ProductKey;
            }
        }

        public String ClientAddress1
        {
            get
            {
                return _ClientAddress1;
            }
        }

        public String ClientAddress2
        {
            get
            {
                return _ClientAddress2;
            }
        }

        public String ClientCity
        {
            get
            {
                return _ClientCity;
            }
        }

        public String ClientPhone
        {
            get
            {
                return _ClientPhone;
            }
        }

        public String ClientEMail
        {
            get
            {
                return _ClientEMail;
            }
        }

        public String ClientPostalCode
        {
            get
            {
                return _ClientPostalCode;
            }
        }

        public String ClientProvince
        {
            get
            {
                return _ClientProvince;
            }
        }

        public String ClientCountry
        {
            get
            {
                return _ClientCountry;
            }
        }

        public String ConfirmationCode
        {
            get
            {
                return _ConfirmationCode;
            }
        }

        public EActivationResult ActivationResult
        {
            get
            {
                return _ActivationResult;
            }
        }

        public String GetName()
        {
            return "ActivationConfirmation";
        }

        public void ReadXml(XmlReader reader)
        {
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            _ActivationResult = (EActivationResult)(Enum.Parse(typeof(EActivationResult), reader.ReadElementString("ActivationResult")));
            _ProductKey = reader.ReadElementString("ProductKey");
            _UserKey = reader.ReadElementString("UserKey");
            _ClientName = reader.ReadElementString("ClientName");
            _ClientAddress1 = reader.ReadElementString("Address1");
            _ClientAddress2 = reader.ReadElementString("Address2");
            if (_ClientAddress2 == "NONE")
                _ClientAddress2 = String.Empty;
            _ClientPhone = reader.ReadElementString("Phone");
            _ClientEMail = reader.ReadElementString("EMail");
            _ClientCity = reader.ReadElementString("City");
            _ClientProvince = reader.ReadElementString("Province");
            _ClientPostalCode = reader.ReadElementString("PostalCode");
            _ClientCountry = reader.ReadElementString("Country");
            reader.ReadEndElement();
        }

        public void WriteXml(XmlWriter writer)
        {
            throw new NotImplementedException();
        }
    }
}
