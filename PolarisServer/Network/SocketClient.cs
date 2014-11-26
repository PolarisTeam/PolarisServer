using System;
using System.Net.Sockets;

namespace PolarisServer.Network
{
    public class SocketClient
    {
        private SocketServer _server;
        private TcpClient _socket;

        public TcpClient Socket { get { return _socket; } }

        private byte[] _readBuffer;

        public delegate void DataReceivedDelegate(byte[] data, int size);

        public event DataReceivedDelegate DataReceived;

        public delegate void ConnectionLostDelegate();

        public event ConnectionLostDelegate ConnectionLost;

        public SocketClient(SocketServer server, TcpClient socket)
        {
            _server = server;
            _socket = socket;

            _readBuffer = new byte[1024 * 16];
        }

        public bool OnReadable()
        {
            try
            {
                int read = _socket.Client.Receive(_readBuffer);
                if (read == 0)
                {
                    // Connection failed, presumably
                    ConnectionLost();
                    return false;
                }

                DataReceived(_readBuffer, read);

                return true;
            }
            catch (SocketException)
            {
                ConnectionLost();
                return false;
            }
        }
    }
}

