using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    class TransactionRequest : INamedXmlSerializable
    {
        public enum ETransaction { Unset, RetrieveResults, RetrieveItemSlides, RequestTransmission, AbortTransaction, 
            DoIATDeploy, VerifyIATDeployment, DeleteIAT, DeleteIATData, TransactionSuccess, TransactionFail, IATExists,
            RequestPacket, RequestEncryptionKey, RequestRetrievalReady, EstablishEncryption, RequestAdminPasswordVerification, RequestDataPasswordVerification,
            VerifyPassword, NoSuchIAT, ClientFrozen, ClientDeleted, RequestIATList, RequestItemSlideManifest, TransactionRequest,
            RequestEMailVerification, RequestNewVerificationEMail, RequestResultDescriptor, RequestRemainingResources, QueryDeploymentProgress,
            IATBeingDeployed, IATDeployHaltFailed, NoSuchClient, EMailAlreadyVerified, RequestServerReport, RetrieveIATItemSize, InsufficientDiskSpace,
            InsufficientIATS, RequestConnection, RequestResults, RequestIATUpload, PasswordValid, PasswordInvalid, RequestItemSlides, CannotRestoreBackup, BackupRestored,
            RequestIATRedeploy, MismatchedDataFormat, RequestReconnection
        };

        private ETransaction _Transaction;
        private String _ServerPassword;
        private String _IATName;
        private int _ClientID = -1;
        private String _ProductKey;
        private int _IntValue = 0;
        private long _LongValue = 0;
        private String _StringValue = String.Empty;
        private String _UserKey;
        private const int UserID = -1;
        private decimal _DecimalValue = -1;
        private int NumDoubleDigs = 2;
        private bool _IsLastTransaction = true;

        public ETransaction Transaction
        {
            get
            {
                return _Transaction;
            }
            set
            {
                _Transaction = value;
            }
        }

        public String UserKey
        {
            get
            {
                return _UserKey;
            }
            set
            {
                _UserKey = value;
            }
        }

        public decimal DecimalValue
        {
            get
            {
                return _DecimalValue;
            }
            set
            {
                _DecimalValue = value;
            }
        }

        public String ServerPassword
        {
            get
            {
                return _ServerPassword;
            }
            set
            {
                _ServerPassword = value;
            }
        }

        public String IATName
        {
            get
            {
                return _IATName;
            }
            set
            {
                _IATName = value;
            }
        }

        public String StringValue
        {
            get
            {
                return _StringValue;
            }
            set
            {
                _StringValue = value;
            }
        }

        public int ClientID
        {
            get
            {
                return _ClientID;
            }
        }
        /*
        public int UserID
        {
            get
            {
                return _UserID;
            }
        }
        */
        public String ProductKey
        {
            get
            {
                return _ProductKey;
            }
            set
            {
                _ProductKey = value;
            }
        }

        public int IntValue
        {
            get
            {
                return _IntValue;
            }
            set
            {
                _IntValue = value;
            }
        }

        public long LongValue
        {
            get
            {
                return _LongValue;
            }
            set
            {
                _LongValue = value;
            }
        }

        public bool IsLastTransaction
        {
            get
            {
                return _IsLastTransaction;
            }
            set
            {
                _IsLastTransaction = value;
            }
        }


        public TransactionRequest()
        {
            Transaction = ETransaction.Unset;
            ServerPassword = String.Empty;
            IATName = String.Empty;
            ProductKey = CActivation.ProductKey;
            UserKey = CActivation.UserKey;
            if (UserKey == null)
                UserKey = String.Empty;
        }

        public TransactionRequest(ETransaction tType, String ServerPass, String IATName)
        {
            Transaction = tType;
            ServerPassword = ServerPass;
            this.IATName = IATName;
            ProductKey = CActivation.ProductKey;
            UserKey = CActivation.UserKey;
            if (UserKey == null)
                UserKey = String.Empty; 
        }

        public void ReadXml(XmlReader reader)
        {
            CultureInfo ci = new CultureInfo("en-us");
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            Transaction = (ETransaction)Enum.Parse(typeof(ETransaction), reader.ReadElementString());
            ServerPassword = reader.ReadElementString();
            IATName = reader.ReadElementString();
            ProductKey = reader.ReadElementString();
            _ClientID = Convert.ToInt32(reader.ReadElementString());
            UserKey = reader.ReadElementString();
            IntValue = Convert.ToInt32(reader.ReadElementString());
            LongValue = Convert.ToInt64(reader.ReadElementString());
            DecimalValue = Convert.ToDecimal(reader.ReadElementString(), ci);
            StringValue = reader.ReadElementString();
            IsLastTransaction = Convert.ToBoolean(reader.ReadElementString());
            reader.ReadEndElement();
        }

        public String GetName()
        {
            return "TransactionRequest";
        }

        public void WriteXml(XmlWriter writer)
        {
            CultureInfo ci = new CultureInfo("en-us");
            Assembly thisAssembly = Assembly.GetAssembly(typeof(IATConfigMainForm));
            writer.WriteStartElement("TransactionRequest");
            writer.WriteAttributeString("ClientVersion", thisAssembly.GetName().Version.ToString());
            writer.WriteElementString("Transaction", Transaction.ToString());
            writer.WriteElementString("ServerPassword", ServerPassword.ToString());
            writer.WriteElementString("IATName", IATName);
            writer.WriteElementString("ProductKey", ProductKey);
            writer.WriteElementString("ClientID", ClientID.ToString());
            if (UserKey != null)
                writer.WriteElementString("UserKey", UserKey);
            else
                writer.WriteElementString("UserKey", "NOT SET");
            writer.WriteElementString("IntValue", IntValue.ToString());
            writer.WriteElementString("LongValue", LongValue.ToString());
            writer.WriteElementString("DecimalValue", DecimalValue.ToString("F" + NumDoubleDigs.ToString(), ci));
            writer.WriteElementString("StringValue", StringValue.ToString());
            writer.WriteElementString("IsLastTransaction", IsLastTransaction.ToString());
            writer.WriteEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
