using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using System.Xml;
using System.Xml.Schema;

namespace IATClient
{
    class TransactionRequest : INamedXmlSerializable
    {
        public enum ETransaction
        {
            Unset, RetrieveResults, RetrieveItemSlides, RequestTransmission, AbortTransaction,
            DoIATDeploy, VerifyIATDeployment, DeleteIAT, DeleteIATData, TransactionSuccess, TransactionFail, IATExists,
            RequestPacket, RequestEncryptionKey, RequestRetrievalReady, EstablishEncryption, RequestAdminPasswordVerification, RequestDataPasswordVerification,
            VerifyPassword, NoSuchIAT, ClientFrozen, ClientDeleted, RequestIATList, RequestItemSlideManifest, TransactionRequest,
            RequestEMailVerification, RequestNewVerificationEMail, RequestResultDescriptor, RequestRemainingResources, QueryDeploymentProgress,
            IATBeingDeployed, IATDeployHaltFailed, NoSuchClient, EMailAlreadyVerified, RequestServerReport, RetrieveIATItemSize, InsufficientDiskSpace,
            InsufficientIATS, RequestConnection, RequestResults, RequestIATUpload, PasswordValid, PasswordInvalid, RequestItemSlides, CannotRestoreBackup, BackupRestored,
            RequestIATRedeploy, MismatchedDataFormat, RequestReconnection, QueryRemainingIATS, RemainingIATS, RequestDataUpload, TestBeingDeployed, HaltTestDeployment,
            DeploymentHalted, ItemSlideDownloadReady, DeploymentDescriptorMismatch, CannotCreateBackup, EncryptionKeysReceived, DeploymentFileManifestReceived,
            ItemSlideManifestReceived, TokenDefinitionReceived, AbortDeployment, QueryPublicityIAT, PublicityIAT, TestFilesMissing, DeploymentAbortFailed,
            ResultsReady, NoActivationsRemain, EmailVerificationMismatch
        };

        public Dictionary<String, int> IntValues { get; private set; } = new Dictionary<String, int>();
        public Dictionary<String, long> LongValues { get; private set; } = new Dictionary<String, long>();
        public Dictionary<String, String> StringValues { get; private set; } = new Dictionary<string, string>();
        private const int UserID = -1;
        private bool _IsLastTransaction = true;

        public ETransaction Transaction { get; set; }
        public String ActivationKey { get; set; }
        public String IATName { get; set; }
        public int ClientID { get; private set; }
        public String ProductKey { get; set; }
        public bool IsLastTransaction { get; set; }

        public TransactionRequest()
        {
            Transaction = ETransaction.Unset;
            IATName = String.Empty;
            ProductKey = LocalStorage.Activation[LocalStorage.Field.ProductKey];
            ActivationKey = LocalStorage.Activation[LocalStorage.Field.ActivationKey];
        }

        public TransactionRequest(ETransaction tType)
        {
            Transaction = tType;
            IATName = String.Empty;
            ProductKey = LocalStorage.Activation[LocalStorage.Field.ProductKey];
            ActivationKey = LocalStorage.Activation[LocalStorage.Field.ActivationKey];
        }

        public TransactionRequest(ETransaction tType, String IATName)
        {
            Transaction = tType;
            this.IATName = IATName;
            ProductKey = LocalStorage.Activation[LocalStorage.Field.ProductKey];
            ActivationKey = LocalStorage.Activation[LocalStorage.Field.ActivationKey];
        }

        public void ReadXml(XmlReader reader)
        {
            CultureInfo ci = new CultureInfo("en-us");
            if (Convert.ToBoolean(reader["HasException"]))
                throw new CXmlSerializationException(reader);
            reader.ReadStartElement();
            Transaction = (ETransaction)Enum.Parse(typeof(ETransaction), reader.ReadElementString());
            IATName = reader.ReadElementString();
            ProductKey = reader.ReadElementString();
            ClientID = Convert.ToInt32(reader.ReadElementString());
            if (reader.LocalName == "ActivationKey")
            {
                ActivationKey = reader.ReadElementString();
                if ((ActivationKey != LocalStorage.Activation[LocalStorage.Field.ActivationKey]) && (ActivationKey != String.Empty))
                    LocalStorage.Activation[LocalStorage.Field.ActivationKey] = ActivationKey;
            }
            while (reader.Name == "IntValue")
            {
                String name = reader["name"];
                IntValues[name] = Convert.ToInt32(reader.ReadElementString());
            }
            while (reader.Name == "LongValue")
            {
                String name = reader["name"];
                LongValues[name] = Convert.ToInt64(reader.ReadElementString());
            }
            while (reader.Name == "StringValue")
            {
                String name = reader["name"];
                StringValues[name] = reader.ReadElementString();
            }
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
            if (Transaction == ETransaction.RequestConnection)
                StringValues["email"] = LocalStorage.Activation[LocalStorage.Field.UserEmail];
            writer.WriteStartElement("TransactionRequest");
            writer.WriteAttributeString("ClientVersion", thisAssembly.GetName().Version.ToString());
            writer.WriteElementString("Transaction", Transaction.ToString());
            writer.WriteElementString("IATName", IATName);
            writer.WriteElementString("ProductKey", ProductKey);
            writer.WriteElementString("ClientID", ClientID.ToString());
            if (ActivationKey != null)
                writer.WriteElementString("ActivationKey", ActivationKey);
            foreach (String key in IntValues.Keys)
            {
                writer.WriteStartElement("IntValue");
                writer.WriteAttributeString("name", key);
                writer.WriteString(IntValues[key].ToString());
                writer.WriteEndElement();
            }
            foreach (String key in LongValues.Keys)
            {
                writer.WriteStartElement("LongValue");
                writer.WriteAttributeString("name", key);
                writer.WriteString(LongValues[key].ToString());
                writer.WriteEndElement();
            }
            foreach (String key in StringValues.Keys)
            {
                writer.WriteStartElement("StringValue");
                writer.WriteAttributeString("name", key);
                writer.WriteString(StringValues[key]);
                writer.WriteEndElement();
            }
            writer.WriteElementString("IsLastTransaction", IsLastTransaction.ToString());
            writer.WriteEndElement();
        }

        public XmlSchema GetSchema()
        {
            return null;
        }
    }
}
