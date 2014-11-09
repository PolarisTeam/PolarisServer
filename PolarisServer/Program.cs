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
            new Server().Run();
        }
    }
}
