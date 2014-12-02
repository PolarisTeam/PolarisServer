using System;
using System.Collections.Generic;

using PolarisServer.Network;

namespace PolarisServer
{
    public class Server
    {
        private List<Client> clients;
        private Network.SocketServer server;

        public List<Client> Clients { get { return clients; } }
        public static Server Instance { get; private set; }
        public DateTime StartTime { get; private set; }

        public Server()
        {
            clients = new List<Client>();
            server = new Network.SocketServer(12205);
            server.NewClient += HandleNewClient;
            Instance = this;
            StartTime = DateTime.Now;

            new QueryServer(QueryMode.BlockBalance, 12200);
        }

        public void Run()
        {
            server.Run();
        }

        void HandleNewClient(SocketClient client)
        {
            var c = new Client(this, client);
            clients.Add(c);
        }
    }
}
