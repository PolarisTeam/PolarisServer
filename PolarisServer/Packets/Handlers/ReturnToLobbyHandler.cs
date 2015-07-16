using PolarisServer.Models;
using PolarisServer.Object;
using PolarisServer.Packets.PSOPackets;
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
        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
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

            var setAreaPacket = File.ReadAllBytes("Resources/testSetAreaPacket.bin");
            context.SendPacket(0x03, 0x24, 4, setAreaPacket);

            foreach (Client c in Server.Instance.Clients)
            {
                if (c == context || c.Character == null)
                    continue;

                if (c.CurrentZone == context.CurrentZone)
                {
                    PacketWriter writer2 = new PacketWriter();
                    writer2.WriteStruct(new ObjectHeader((uint)c.User.PlayerId, EntityType.Player));
                    writer2.WriteStruct(new ObjectHeader((uint)context.User.PlayerId, EntityType.Player));
                    c.SendPacket(0x4, 0x3B, 0x40, writer2.ToArray());
                }
            }

            context.CurrentZone = "lobby";

            // Set Player ID
            var setPlayerId = new PacketWriter();
            setPlayerId.WritePlayerHeader((uint)context.User.PlayerId);
            context.SendPacket(0x06, 0x00, 0, setPlayerId.ToArray());

            // Lobby Objects
            PSOObject[] lobbyObjects = ObjectManager.Instance.getObjectsForZone("lobby").Values.ToArray();

            foreach (PSOObject obj in lobbyObjects)
            {
                context.SendPacket(0x08, 0x0B, 0x0, obj.GenerateSpawnBlob());
            }

            // Lobby NPCs
            PSONPC[] lobbyNpcs = ObjectManager.Instance.getNPCSForZone("lobby");

            foreach (PSONPC npc in lobbyNpcs)
            {
                context.SendPacket(0x08, 0xC, 0x4, npc.GenerateSpawnBlob());
            }

            // Spawn Character
            context.SendPacket(new CharacterSpawnPacket(context.Character, destination));
            context.CurrentLocation = destination;

            // Unlock Controls
            context.SendPacket(new NoPayloadPacket(0x03, 0x2B));

            // Spawn on other player's clients
            var spawnPacket = new CharacterSpawnPacket(context.Character, destination) { IsItMe = false };
            foreach (var c in Server.Instance.Clients.Where(c => c != context).Where(c => c.Character != null).Where(c => c.CurrentZone == "lobby"))
            {
                c.SendPacket(spawnPacket);

                var remoteChar = new CharacterSpawnPacket(c.Character, c.CurrentLocation) { IsItMe = false };
                context.SendPacket(remoteChar);
            }
            
        }
    }
}
