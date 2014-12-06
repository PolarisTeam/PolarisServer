using System;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x1, 0x2)]
    public class ClientDebug : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            byte[] tooManyZeros = new byte[UInt32.MaxValue - 8];

            // TODO: Test this lol
            context.SendPacket(0x00, 0x00, 0, tooManyZeros);
        }

        #endregion
    }
}

