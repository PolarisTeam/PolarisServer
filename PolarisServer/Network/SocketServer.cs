using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PolarisServer.Network
{
    public class SocketServer
    {
        public delegate void NewClientDelegate(SocketClient client);

        private readonly List<SocketClient> _clients = new List<SocketClient>();
        private readonly TcpListener _listener;
        private readonly List<Socket> _readableSockets = new List<Socket>();
        private readonly Dictionary<Socket, SocketClient> _socketMap = new Dictionary<Socket, SocketClient>();
        private int _port;

        public SocketServer(int port)
        {
            _port = port;

            _listener = new TcpListener(IPAddress.Any, port);
            _listener.Start();
        }

        public IList<SocketClient> Clients
        {
            get { return _clients.AsReadOnly(); }
        }

        public event NewClientDelegate NewClient;

        public void Run()
        {
            try
            {
                // Compile a list of possibly-readable sockets
                _readableSockets.Clear();
                _readableSockets.Add(_listener.Server);

                foreach (var client in _clients)
                    _readableSockets.Add(client.Socket.Client);

                Socket.Select(_readableSockets, null, null, 1000000);

                foreach (var socket in _readableSockets)
                {
                    if (socket == _listener.Server)
                    {
                        // New connection
                        Logger.WriteInternal("[HI!] New connection!");

                        var c = new SocketClient(this, _listener.AcceptTcpClient());

                        _clients.Add(c);
                        _socketMap.Add(c.Socket.Client, c);

                        NewClient(c);
                    }
                    else
                    {
                        // Readable data
                        if (socket.Connected)
                            _socketMap[socket].OnReadable();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException("A socket error occurred", ex);
            }
        }

        internal void NotifyConnectionClosed(SocketClient client)
        {
            Console.WriteLine("Connection closed");

            _socketMap.Remove(client.Socket.Client);
            _clients.Remove(client);
        }
    }
}