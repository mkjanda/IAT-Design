using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace IATClient
{
    class CServerConnection
    {
        private const String DataConnectionServlet = "DataProvider";
        private String _Host, _IATName;
        private int _Port;
        
        public String Host
        {
            get
            {
                return _Host;
            }
        }

        public int Port
        {
            get
            {
                return _Port;
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

        public CServerConnection(String host, int port)
        {
            _Host = host;
            _Port = port;
            _IATName = String.Empty;
        }

        public Socket Connect()
        {
            IPEndPoint hostEndPoint = null;
            IPAddress hostAddress = null;
            IPHostEntry hostInfo;
            IPAddress[] IPAddresses;
            Socket socketV4, socketV6;

            try
            {
                hostInfo = Dns.GetHostEntry(Host);
                IPAddresses = hostInfo.AddressList;
            }
            catch (Exception ex)
            {
                throw new ServerConnectionException(ServerExceptionType.CannotResolveHost, ex);
            }

            for (int ctr1 = 0; ctr1 < 600; ctr1++)
            {
                try
                {
                    socketV4 = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.IP);
                    for (int ctr = 0; ctr < IPAddresses.Length; ctr++)
                    {
                        hostAddress = IPAddresses[ctr];
                        hostEndPoint = new IPEndPoint(hostAddress, Port);


                        try
                        {
                            socketV4.Connect(hostEndPoint);
                        }
                        catch (SocketException)
                        {
                            continue;
                        }
                        if (socketV4.Connected)
                        {
                            return socketV4;
                        }
                    }
                }
                catch (Exception)
                {
                }
                try
                {
                    socketV6 = new Socket(AddressFamily.InterNetworkV6, SocketType.Stream, ProtocolType.IP);

                    for (int ctr = 0; ctr < IPAddresses.Length; ctr++)
                    {
                        hostAddress = IPAddresses[ctr];
                        hostEndPoint = new IPEndPoint(hostAddress, Port);


                        try
                        {
                            socketV6.Connect(hostEndPoint);
                        }
                        catch (SocketException)
                        {
                            continue;
                        }
                        if (socketV6.Connected)
                        {
                            return socketV6;
                        }
                    }
                }
                catch (Exception)
                {
                }
                System.Threading.Thread.Sleep(100);
            }
            throw new ServerConnectionException(ServerExceptionType.CannotConnectToHost);
        }

        public void ShakeHands(Socket socket)
        {
            try
            {
                byte[] hand = new byte[10];
                socket.Receive(hand, 10, SocketFlags.None);
                byte[] response = new byte[10];
                for (int ctr = 0; ctr < 10; ctr++)
                {
                    if (ctr % 2 == 0)
                    {
                        if (hand[ctr] == 0)
                            response[ctr] = 0;
                        else
                            response[ctr] = (byte)(~hand[ctr] + 0x01);
                    }
                    else
                        response[ctr] = (byte)(~hand[ctr]);
                }
                socket.Send(response, 10, SocketFlags.None);
            }
            catch (Exception ex)
            {
                throw new ServerConnectionException(ServerExceptionType.HandshakeFailed, ex);
            }
        }

        /*
        private static bool sendByteBuffer(MemoryStream memStream, Socket socket, int nTransSizeBytes)
        {
            try
            {
                MemoryStream headerStream = new MemoryStream();
                StreamWriter headerWriter = new StreamWriter(headerStream, Encoding.Unicode);
                String headerStr = String.Format("{0}\n", memStream.Length);
                headerWriter.Write(headerStr);
                headerWriter.Flush();
                while (headerStream.Length < nTransSizeBytes)
                    headerStream.WriteByte(0);
                socket.Send(headerStream.GetBuffer(), 0, nTransSizeBytes, SocketFlags.None);
                socket.Send(memStream.GetBuffer(), 0, (int)memStream.Length, SocketFlags.None);
                headerStream.Dispose();
                headerWriter.Close();
                headerWriter.Dispose();
                return true;
            }
            catch (SocketException ex)
            {
                throw new ServerConnectionException(ServerExceptionType.SocketTimeout, ex);
            }
            catch (ObjectDisposedException ex)
            {
                throw new ServerConnectionException(ServerExceptionType.SocketClosed, ex);
            }
            catch (Exception ex)
            {
                throw new ServerConnectionException(ServerExceptionType.TransmissionFailed, ex);
            }
        }

        public static bool sendByteBuffer(MemoryStream memStream, Socket socket)
        {
            return sendByteBuffer(memStream, socket, 20);
        }

        private static MemoryStream receiveByteBuffer(Socket socket, int nTransSizeBytes)
        {
            try
            {
                int nBytesReceived = 0;
                byte[] header = new byte[nTransSizeBytes];
                while (nBytesReceived < nTransSizeBytes)
                    nBytesReceived += socket.Receive(header, nBytesReceived, nTransSizeBytes - nBytesReceived, SocketFlags.None);
                char[] chHeader = new char[nTransSizeBytes];
                int bytesUsed, charsUsed;
                bool completed;
                System.Text.Encoding.Unicode.GetDecoder().Convert(header, 0, nTransSizeBytes, chHeader, 0, nTransSizeBytes, true, out bytesUsed, out charsUsed, out completed);
                String sHeader = new String(chHeader, 0, charsUsed);
                int nBytesExpected = Convert.ToInt32(sHeader);
                byte[] transmission = new byte[nBytesExpected];
                nBytesReceived = 0;
                while (nBytesReceived < nBytesExpected)
                    nBytesReceived += socket.Receive(transmission, nBytesReceived, nBytesExpected - nBytesReceived, SocketFlags.None);
                return new MemoryStream(transmission);
            }
            catch (SocketException ex)
            {
                throw new ServerConnectionException(ServerExceptionType.SocketTimeout, ex);
            }
            catch (ObjectDisposedException ex)
            {
                throw new ServerConnectionException(ServerExceptionType.SocketClosed, ex);
            }
            catch (Exception ex)
            {
                throw new ServerConnectionException(ServerExceptionType.ReceptionFailed, ex);
            }
        }


        public static MemoryStream receiveByteBuffer(Socket socket)
        {
            return receiveByteBuffer(socket, 20);
        }
        */
    }


}
