using System;
using System.Runtime.Serialization;

namespace IATClient
{
    public enum ServerExceptionType
    {
        generic, CannotConnectToHost, CannotRetrieveHostPort, HandshakeFailed, CannotResolveHost,
        InvalidXMLFromHost, SocketTimeout, SocketClosed, TransmissionFailed, ReceptionFailed
    };

    class ServerConnectionException : Exception, ISerializable
    {
        private static string[] ExceptionMessages = {"A generic server connection error occurred.",
                                                        "Could not initiate a connection with the server.",
                                                        "Could not retrieve a port from the server to use for a socket connection.",
                                                        "The handshaking algorithm failed.",
                                                        "Cannot resolve the provided host name with the DNS server.",
                                                        "Could not deserialize transmission from host.",
                                                        "The connection with the host timed out.",
                                                        "The connection with the host was unexpectedly closed.",
                                                        "An error occurred while transmitting data to the server.",
                                                        "An error occurred while receiving data from the server."
                                                    };

        private ServerExceptionType _Type;

        public ServerExceptionType Type
        {
            get
            {
                return _Type;
            }
        }

        public ServerConnectionException()
        {
            _Type = ServerExceptionType.generic;
        }

        public ServerConnectionException(String message) : base(message)
        {
        }

        public ServerConnectionException(String message, Exception inner) : base(message, inner)
        {
        }

        protected ServerConnectionException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }

        public ServerConnectionException(ServerExceptionType type) : base(ExceptionMessages[(int)type])
        {
            _Type = type;
        }

        public ServerConnectionException(ServerExceptionType type, Exception innerException)
            : base(ExceptionMessages[(int)type], innerException)
        {
            _Type = type;
        }
    }
}