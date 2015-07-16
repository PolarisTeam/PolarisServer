using PolarisServer.Models;
using PolarisServer.Object;
using PolarisServer.Packets.PSOPackets;
using PolarisServer.Zone;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x3, 0x35)]
    class CasinoTeleportHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            if (context.User == null)
                return;

            // Dunno what these are yet.
            context.SendPacket(0x11, 0xA, 0x0, BitConverter.GetBytes(context.User.PlayerId));
            context.SendPacket(0x1E, 0xC, 0x0, BitConverter.GetBytes(101));

            Map casinoMap = ZoneManager.Instance.MapFromInstance("casino", "lobby");
            casinoMap.SpawnClient(context, casinoMap.GetDefaultLoaction());

        }
    }
}
