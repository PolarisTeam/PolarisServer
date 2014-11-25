using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

using PolarisServer.Database;

namespace PolarisServer
{
    class PolarisApp
    {

        private static PolarisApp _Instance;
        public static PolarisApp Instance { get { return _Instance; } }
        private PolarisDB _Database;

        public static void Main(string[] args)
        {

            Console.WriteLine("Arf");
            _Instance = new PolarisApp();
            _Instance.Start();

        }

        public void Start()
        {
            Logger.WriteInternal("Server starting at " + DateTime.Now.ToString());
            Packets.Handlers.PacketHandlers.loadPacketHandlers();
            _Database = new PolarisDB();
            for (int i = 0; i < 10; i++)
            {
                new QueryServer(QueryMode.ShipList, 12099 + (100 * i));
            }
            new Server().Run();
        }

        public PolarisDB Database { get { return _Database; } }

    }
}
