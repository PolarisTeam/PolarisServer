using PolarisServer.Packets.PSOPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Packets.Handlers
{

    [PacketHandlerAttr(0x2F, 0x6)]
    class SymbolArtListHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            context.SendPacket(new SymbolArtList(new Models.ObjectHeader((uint)context.User.PlayerId, Models.EntityType.Player)));
        }
    }
}
