using PolarisServer.Models;
using PolarisServer.Object;
using PolarisServer.Packets.PSOPackets;
using PolarisServer.Zone;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x3, 0x34)]
    class ReturnToLobbyHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            PacketReader reader = new PacketReader(data);

            reader.ReadUInt64(); // Skip 8 bytes
            if(reader.ReadUInt32() != 0x10)
            {
                Logger.WriteWarning("[WRN] Packet 0x3 0x34's first value was not 0x10! Investigate.");
            }

            uint partOfLobby = reader.ReadUInt32();
            PSOLocation destination;
            if(partOfLobby == 0) // Gate area
            {
                destination = new PSOLocation(0f, 1f, 0f, 0f, -0.22f, 2.4f, 198.75f);
            }
            else // Shop area
            {
                destination = new PSOLocation(0f, 1f, 0f, 20f, 0.20f, 1.23f, -175.25f);
            }
            Map lobbyMap = ZoneManager.Instance.MapFromInstance("lobby", "lobby");
            lobbyMap.SpawnClient(context, destination);
            
        }
    }
}
