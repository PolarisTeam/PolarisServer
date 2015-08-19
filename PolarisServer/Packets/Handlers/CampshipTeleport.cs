using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolarisServer.Models;
using PolarisServer.Zone;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x3, 0x12)]
    class CampshipTeleport : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            if (context.currentParty.currentQuest == null)
                return;

            var instanceName = String.Format("{0}-{1}", context.currentParty.currentQuest.name, context.User.Nickname);
            ZoneManager.Instance.NewInstance(instanceName, new Map("campship", 150, 0, Map.MapType.Campship, 0));
            // todo: add next map

            Map campship = ZoneManager.Instance.MapFromInstance("campship", instanceName);

            campship.SpawnClient(context, new PSOLocation(0, 1, 0, 0, 0, 0, 0), context.currentParty.currentQuest.name);
        }
    }

    [PacketHandlerAttr(0x4, 0x13)]
    class CampshipTeleportDown : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            if (context.currentParty.currentQuest == null)
                return;

            // TODO: WTF terribad hax?
            if (context.CurrentLocation.PosZ >= 20)
            {
                var instanceName = String.Format("{0}-{1}", context.currentParty.currentQuest.name, context.User.Nickname);
                ZoneManager.Instance.NewInstance(instanceName, new Map("forest", 310, 0, Map.MapType.Quest, Map.MapFlags.EnableMap));

                Map forest = ZoneManager.Instance.MapFromInstance("campship", instanceName);
                forest.SpawnClient(context, new PSOLocation(0, 1, 0, 0, 0, 0, 0));
            }
        }
    }
}
