using System;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x1, 0x2)]
    public class ClientDebug : PacketHandler
    {
        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (!(data[7] == 0x3 && data[8] == 0x4))
                return;

            byte[] tooManyZeros = new byte[Int32.MaxValue - 8];

            // TODO: Test this lol
            context.SendPacket(0x0, 0x0, 0x0, tooManyZeros);
        }
    }
}

