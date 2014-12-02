using System;
using System.Net.Sockets;

namespace PolarisServer.Network
{
    public class SocketClient
    {
        private SocketServer server;
        private TcpClient socket;

        public TcpClient Socket { get { return socket; } }

        private byte[] readBuffer;

        public delegate void DataReceivedDelegate(byte[] data, int size);
        public event DataReceivedDelegate DataReceived;

        public delegate void ConnectionLostDelegate();
        public event ConnectionLostDelegate ConnectionLost;

        public SocketClient(SocketServer server, TcpClient socket)
        {
            this.server = server;
            this.socket = socket;

            readBuffer = new byte[1024 * 16];
        }

        public bool OnReadable()
        {
            try
            {
                int read = socket.Client.Receive(readBuffer);
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
            socket.Close();
        }
    }
}

