using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PolarisServer.Network
{
    public class SocketServer
    {
        private int _port;
        private List<SocketClient> _clients = new List<SocketClient>();
        private Dictionary<Socket, SocketClient> _socketMap = new Dictionary<Socket, SocketClient>();

        public IList<SocketClient> Clients { get { return _clients.AsReadOnly(); } }

        public delegate void NewClientDelegate(SocketClient client);

        public event NewClientDelegate NewClient;

        public SocketServer(int port)
        {
            _port = port;
        }

        public void Run()
        {
            TcpListener listener = new TcpListener(IPAddress.Any, _port);
            listener.Start();

            List<Socket> readableSockets = new List<Socket>();

            while (true)
            {
                // Compile a list of possibly-readable sockets
                readableSockets.Clear();
                readableSockets.Add(listener.Server);
                foreach (SocketClient client in _clients)
                    readableSockets.Add(client.Socket.Client);

                Socket.Select(readableSockets, null, null, 1000000);

                foreach (Socket socket in readableSockets)
                {
                    if (socket == listener.Server)
                    {
                        // New connection
                        Logger.Write("New connection!");

                        SocketClient c = new SocketClient(this, listener.AcceptTcpClient());

                        _clients.Add(c);
                        _socketMap.Add(c.Socket.Client, c);

                        NewClient(c);
                    }
                    else
                    {
                        // Readable data
                        SocketClient c = _socketMap[socket];

                        if (!c.OnReadable())
                        {
                            // Connection failed, remove it from here
                            Console.WriteLine("Connection closed");

                            _clients.Remove(c);
                            _socketMap.Remove(socket);
                        }
                    }
                }
            }
        }
    }
}

