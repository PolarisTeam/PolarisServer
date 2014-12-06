using System;
using System.Collections.Generic;
using System.Timers;

using PolarisServer.Network;
using PolarisServer.Packets;

namespace PolarisServer
{
    public class Server
    {
        private List<Client> clients;
        private SocketServer server;

        public List<Client> Clients { get { return clients; } }
        public static Server Instance { get; private set; }
        public DateTime StartTime { get; private set; }
        public Timer pingTimer;

        public Server()
        {
            clients = new List<Client>();
            server = new SocketServer(12205);
            server.NewClient += HandleNewClient;
            Instance = this;
            StartTime = DateTime.Now;

            pingTimer = new Timer(1000 * PolarisApp.Config.PingTime); // 1 Minute default
            pingTimer.Elapsed += PingClients;
            pingTimer.Start();

            new QueryServer(QueryMode.BlockBalance, 12200);
        }

        public void Run()
        {
            while (true)
            {
                // Run the underlying SocketServer
                server.Run();

                // Check Clients to make sure they still exist
                foreach (Client client in clients)
                    if (client.IsClosed)
                    {
                        clients.Remove(client);
                        break;
                    }
            }
        }

        void HandleNewClient(SocketClient client)
        {
            Client newClient = new Client(this, client);
            clients.Add(newClient);
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
    }
}
