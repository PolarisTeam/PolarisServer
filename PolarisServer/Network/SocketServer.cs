using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PolarisServer.Network
{
    public class SocketServer
    {
        public delegate void NewClientDelegate(SocketClient client);

        private readonly List<SocketClient> clients = new List<SocketClient>();
        private readonly TcpListener listener;
        private readonly List<Socket> readableSockets = new List<Socket>();
        private readonly Dictionary<Socket, SocketClient> socketMap = new Dictionary<Socket, SocketClient>();
        private int port;

        public SocketServer(int port)
        {
            this.port = port;

            listener = new TcpListener(IPAddress.Any, port);
            listener.Start();
        }

        public IList<SocketClient> Clients
        {
            get { return clients.AsReadOnly(); }
        }

        public event NewClientDelegate NewClient;

        public void Run()
        {
            try
            {
                // Compile a list of possibly-readable sockets
                readableSockets.Clear();
                readableSockets.Add(listener.Server);

                foreach (var client in clients)
                    readableSockets.Add(client.Socket.Client);

                Socket.Select(readableSockets, null, null, 1000000);

                foreach (var socket in readableSockets)
                {
                    if (socket == listener.Server)
                    {
                        // New connection
                        Logger.WriteInternal("[HI!] New connection!");

                        var c = new SocketClient(this, listener.AcceptTcpClient());

                        clients.Add(c);
                        socketMap.Add(c.Socket.Client, c);

                        NewClient(c);
                    }
                    else
                    {
                        // Readable data
                        if (socket.Connected)
                            socketMap[socket].OnReadable();
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

            socketMap.Remove(client.Socket.Client);
            clients.Remove(client);
        }
    }
}