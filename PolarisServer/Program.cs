using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;

namespace PolarisServer
{
    class MainClass
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Arf");
            Logger.Write(string.Format("Server started at {0}", DateTime.Now.ToString()), LogType.Internal);
            Packets.Handlers.PacketHandlers.loadPacketHandlers();
            for (int i = 0; i < 10; i++)
            {
                new QueryServer(QueryMode.SHIP_LIST, 12099 + (100 * i));
            }
            new Server().Run();
        }
    }
}
