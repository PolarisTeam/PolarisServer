using System;
using System.Collections.Generic;

using PolarisServer.Network;
using PolarisServer.Packets;

namespace PolarisServer
{
    public class Server
    {
        private List<Client> clients;
        private Network.SocketServer server;

        public List<Client> Clients { get { return clients; } }
        public static Server Instance { get; private set; }
        public DateTime StartTime { get; private set; }
        private System.Timers.Timer pingTimer = new System.Timers.Timer(1000 * 60); // 1 Minute

        public Server()
        {
            clients = new List<Client>();
            server = new Network.SocketServer(12205);
            server.NewClient += HandleNewClient;
            Instance = this;
            StartTime = DateTime.Now;

            pingTimer.Elapsed += PingClients;
            pingTimer.Start();

            new QueryServer(QueryMode.BlockBalance, 12200);
        }

        void PingClients(object sender, System.Timers.ElapsedEventArgs e)
        {
            // Ping!
            // TODO: Disconnect a client if we don't get a response in a certain amount of time
            foreach (Client client in clients)
            {
                if (client != null)
                {
                    Logger.Write("[HEY] Pinging " + client.User.Username);
                    client.SendPacket(new NoPayloadPacket(0x03, 0x0B));
                }
            }
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
