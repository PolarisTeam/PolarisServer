using System.Net.Sockets;

namespace PolarisServer.Network
{
    public class SocketClient
    {
        public delegate void ConnectionLostDelegate();

        public delegate void DataReceivedDelegate(byte[] data, int size);

        private readonly byte[] readBuffer;
        private readonly SocketServer server;

        public SocketClient(SocketServer server, TcpClient socket)
        {
            this.server = server;
            Socket = socket;

            readBuffer = new byte[1024*16];
        }

        public TcpClient Socket { get; private set; }
        public event DataReceivedDelegate DataReceived;
        public event ConnectionLostDelegate ConnectionLost;

        public bool OnReadable()
        {
            try
            {
                var read = Socket.Client.Receive(readBuffer);
                if (read == 0)
                {
                    // Connection failed, presumably
                    ConnectionLost();
                    server.NotifyConnectionClosed(this);
                    return false;
                }

                DataReceived(readBuffer, read);

                return true;
            }
            catch (SocketException)
            {
                ConnectionLost();
                server.NotifyConnectionClosed(this);
                return false;
            }
        }

        public void Close()
        {
            ConnectionLost();
            server.NotifyConnectionClosed(this);
            Socket.Close();
        }
    }
}