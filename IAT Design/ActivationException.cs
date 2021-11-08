using System;
using System.Collections.Generic;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace IATClient
{
    [Serializable]
    [XmlRoot(ElementName = "ActivationException")]
    public class ActivationException
    {
        [XmlElement(ElementName = "ProductKey", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public String ProductKey { get; set; } = LocalStorage.Activation[LocalStorage.Field.ProductKey];

        [XmlElement(ElementName = "ActivationKey", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public String ActivationKey { get; set; } = LocalStorage.Activation[LocalStorage.Field.ActivationKey];

        [XmlElement(ElementName = "Email", Form = XmlSchemaForm.Unqualified, IsNullable = true)]
        public String Email { get; set; } = LocalStorage.Activation[LocalStorage.Field.UserEmail];

        [XmlElement(ElementName = "Version", Form = XmlSchemaForm.Unqualified)]
        public String Version { get; set; } = LocalStorage.Activation[LocalStorage.Field.Version];

        [Serializable]
        [XmlRoot("Exception")]
        public class CException
        {
            [Serializable]
            [XmlRoot("InnerException")]
            public class CInnerException
            {
                public CInnerException() { }

                public CInnerException(String Message, List<String> StackTrace)
                {
                    _ExceptionMessage = Message;
                    _StackTrace = StackTrace;
                }

                private String _ExceptionMessage;
                private List<String> _StackTrace = new List<String>();

                [XmlElement(ElementName = "ExceptionMessage", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
                public String ExceptionMessage
                {
                    get
                    {
                        return _ExceptionMessage;
                    }
                    set
                    {
                        _ExceptionMessage = value;
                    }
                }

                [XmlElement(ElementName = "StackTraceElement", Form = XmlSchemaForm.Unqualified)]
                public List<String> StackTrace
                {
                    get
                    {
                        return _StackTrace;
                    }
                    set
                    {
                        _StackTrace = value;
                    }
                }
            }

            public CException() { }

            public CException(Exception ex)
            {
                _ExceptionMessage = ex.Message;
                _StackTrace = new List<String>(ex.StackTrace.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries));
                Exception innerEx = ex.InnerException;
                while (innerEx != null)
                {
                    InnerExceptions.Add(new CInnerException(innerEx.Message, new List<String>(innerEx.StackTrace.Split(new char[] { '\n' }, StringSplitOptions.RemoveEmptyEntries))));
                    innerEx = innerEx.InnerException;
                }
            }

            private String _ExceptionMessage;
            private List<String> _StackTrace;

            [XmlElement(ElementName = "ExceptionMessage")]
            public String ExceptionMessage
            {
                get
                {
                    return _ExceptionMessage;
                }
                set
                {
                    _ExceptionMessage = value;
                }
            }

            [XmlElement(ElementName = "StackTrace")]
            public List<String> StackTrace
            {
                get
                {
                    return _StackTrace;
                }
                set
                {
                    _StackTrace = value;
                }
            }

            [XmlElement("InnerExceptions")]
            public List<CInnerException> InnerExceptions { get; set; } = new List<CInnerException>();
        }

        [XmlElement(ElementName = "Exception", Type = typeof(CException))]
        public CException Exception { get; set; }

        public ActivationException() { }

        public ActivationException(Exception ex)
        {
            Exception = new CException(ex);
        }
    }
}
