using PolarisServer.Models;

namespace PolarisServer.Packets
{
    public abstract class Packet
    {
        public abstract byte[] Build();
        public abstract PacketHeader GetHeader();
    }
}