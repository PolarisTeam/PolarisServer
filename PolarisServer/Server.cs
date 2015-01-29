using System;
using System.Collections.Generic;
using System.Timers;
using PolarisServer.Network;
using PolarisServer.Packets.PSOPackets;

namespace PolarisServer
{
    public class Server
    {
        private readonly SocketServer _server;
        public Timer PingTimer;

        public Server()
        {
            Clients = new List<Client>();
            _server = new SocketServer(12205);
            _server.NewClient += HandleNewClient;
            Instance = this;
            StartTime = DateTime.Now;

            PingTimer = new Timer(1000*PolarisApp.Config.PingTime); // 1 Minute default
            PingTimer.Elapsed += PingClients;
            PingTimer.Start();

            new QueryServer(QueryMode.BlockBalance, 12200);
        }

        public List<Client> Clients { get; private set; }
        public static Server Instance { get; private set; }
        public DateTime StartTime { get; private set; }

        public void Run()
        {
            while (true)
            {
                // Run the underlying SocketServer
                _server.Run();

                // Check Clients to make sure they still exist
                foreach (var client in Clients)
                    if (client.IsClosed)
                    {
                        Clients.Remove(client);
                        break;
                    }
            }
        }

        private void HandleNewClient(SocketClient client)
        {
            var newClient = new Client(this, client);
            Clients.Add(newClient);
        }

        private void PingClients(object sender, ElapsedEventArgs e)
        {
            // Ping!
            // TODO: Disconnect a client if we don't get a response in a certain amount of time
            foreach (var client in Clients)
            {
                if (client != null)
                {
                    Logger.Write("[HEY] Pinging " + client.User.Username);
                    client.SendPacket(new NoPayloadPacket(0x03, 0x0B));
                }
            }
        }
    }
}