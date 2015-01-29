using System.Net.Sockets;

namespace PolarisServer.Network
{
    public class SocketClient
    {
        public delegate void ConnectionLostDelegate();

        public delegate void DataReceivedDelegate(byte[] data, int size);

        private readonly byte[] _readBuffer;
        private readonly SocketServer _server;

        public SocketClient(SocketServer server, TcpClient socket)
        {
            _server = server;
            Socket = socket;

            _readBuffer = new byte[1024*16];
        }

        public TcpClient Socket { get; private set; }
        public event DataReceivedDelegate DataReceived;
        public event ConnectionLostDelegate ConnectionLost;

        public bool OnReadable()
        {
            try
            {
                var read = Socket.Client.Receive(_readBuffer);
                if (read == 0)
                {
                    // Connection failed, presumably
                    ConnectionLost();
                    _server.NotifyConnectionClosed(this);
                    return false;
                }

                DataReceived(_readBuffer, read);

                return true;
            }
            catch (SocketException)
            {
                ConnectionLost();
                _server.NotifyConnectionClosed(this);
                return false;
            }
        }

        public void Close()
        {
            ConnectionLost();
            _server.NotifyConnectionClosed(this);
            Socket.Close();
        }
    }
}