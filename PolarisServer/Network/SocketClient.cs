using System.Net.Sockets;

namespace PolarisServer.Network
{
    public class SocketClient
    {
        public delegate void ConnectionLostDelegate();

        public delegate void DataReceivedDelegate(byte[] data, int size);

        private readonly byte[] _readBuffer, _writeBuffer;
        private readonly SocketServer _server;
        private int _writePosition = 0;

        public SocketClient(SocketServer server, TcpClient socket)
        {
            _server = server;
            Socket = socket;

            _readBuffer = new byte[1024 * 16];
            _writeBuffer = new byte[1024 * 1024]; // too high? too low? not sure
        }

        public TcpClient Socket { get; private set; }
        public event DataReceivedDelegate DataReceived;
        public event ConnectionLostDelegate ConnectionLost;

        public bool NeedsToWrite { get { return (_writePosition > 0); } }

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

        public bool OnWritable()
        {
            try
            {
                var write = Socket.Client.Send(_writeBuffer, 0, _writePosition, SocketFlags.None);
                if (write == 0)
                {
                    // Connection failed, presumably
                    ConnectionLost();
                    _server.NotifyConnectionClosed(this);
                    return false;
                }

                System.Array.Copy(_writeBuffer, write, _writeBuffer, 0, _writePosition - write);
                _writePosition -= write;

                return true;
            }
            catch (SocketException)
            {
                ConnectionLost();
                _server.NotifyConnectionClosed(this);
                return false;
            }
        }

        public void Write(byte[] blob)
        {
            if ((_writePosition + blob.Length) > _writeBuffer.Length)
            {
                // Buffer exceeded!
                throw new System.Exception("too much data in write queue");
            }

            System.Array.Copy(blob, 0, _writeBuffer, _writePosition, blob.Length);
            _writePosition += blob.Length;
        }

        public void Close()
        {
            ConnectionLost();
            _server.NotifyConnectionClosed(this);
            Socket.Close();
        }
    }
}