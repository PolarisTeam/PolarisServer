using System;
using System.IO;
using System.Linq;
using Newtonsoft.Json;
using PolarisServer.Models;
using PolarisServer.Packets.PSOPackets;
using PolarisServer.Object;
using PolarisServer.Database;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x3E)]
    public class CharacterSpawn : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context.User == null || context.Character == null)
                return;

            // Looks/Jobs
            if (size > 0)
            {
                var reader = new PacketReader(data);

                reader.BaseStream.Seek(0x38, SeekOrigin.Begin);
                context.Character.Looks = reader.ReadStruct<Character.LooksParam>();
                context.Character.Jobs = reader.ReadStruct<Character.JobParam>();

                using(var db = new PolarisEf())
                    db.ChangeTracker.DetectChanges();
            }

            // Set Area
            var setAreaPacket = File.ReadAllBytes("Resources/testSetAreaPacket.bin");
            context.SendPacket(0x03, 0x24, 4, setAreaPacket);

            context.CurrentZone = "lobby";

            // Set Player ID
            var setPlayerId = new PacketWriter();
            setPlayerId.WritePlayerHeader((uint) context.User.PlayerId);
            context.SendPacket(0x06, 0x00, 0, setPlayerId.ToArray());

            // Lobby Objects
            PSOObject[] lobbyObjects = ObjectManager.Instance.getObjectsForZone("lobby").Values.ToArray();

            foreach(PSOObject obj in lobbyObjects)
            {
                context.SendPacket(0x08, 0x0B, 0x0, obj.GenerateSpawnBlob());
            }

            // Spawn Character
            context.SendPacket(new CharacterSpawnPacket(context.Character, new PSOLocation(0f, 1f, 0f, 0f, -0.417969f, 0f, 137.375f)));
            context.CurrentLocation = new PSOLocation(0f, 1f, 0f, 0f, -0.417969f, 0f, 137.375f);

            // Unlock Controls
            context.SendPacket(new NoPayloadPacket(0x03, 0x2B));

            // Spawn on other player's clients
            var spawnPacket = new CharacterSpawnPacket(context.Character, new PSOLocation(0f, 1f, 0f, 0f, -0.417969f, 0f, 137.375f)) {IsItMe = false};
            foreach (var c in Server.Instance.Clients.Where(c => c != context).Where(c => c.Character != null).Where(c => c.CurrentZone == "lobby"))
            {
                c.SendPacket(spawnPacket);

                var remoteChar = new CharacterSpawnPacket(c.Character, c.CurrentLocation) {IsItMe = false};
                context.SendPacket(remoteChar);
            }

            // memset packet - Enables menus
            // Also holds event items and likely other stuff too
            var memSetPacket = File.ReadAllBytes("Resources/setMemoryPacket.bin");
            context.SendPacket(0x23, 0x07, 0, memSetPacket);

            // Give a blank palette
            context.SendPacket(new PalettePacket());

            Logger.Write("[CHR] {0}'s character {1} has spawned", context.User.Username, context.Character.Name);
        }

        #endregion
    }
}