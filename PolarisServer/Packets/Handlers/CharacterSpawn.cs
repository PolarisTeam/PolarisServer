using System;
using System.IO;

using PolarisServer.Models;

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
                PacketReader reader = new PacketReader(data);

                reader.BaseStream.Seek(0x38, SeekOrigin.Begin);
                context.Character.Looks = reader.ReadStruct<Character.LooksParam>();
                context.Character.Jobs = reader.ReadStruct<Character.JobParam>();

                PolarisApp.Instance.Database.ChangeTracker.DetectChanges();
            }

            // Set Area
            byte[] setAreaPacket = File.ReadAllBytes("Resources/testSetAreaPacket.bin");
            context.SendPacket(0x03, 0x24, 4, setAreaPacket);

            // Set Player ID
            PacketWriter setPlayerID = new PacketWriter();
            setPlayerID.WritePlayerHeader((uint)context.User.PlayerID);
            context.SendPacket(0x06, 0x00, 0, setPlayerID.ToArray());

            // Spawn Lobby Objects
            if (Directory.Exists("Resources/objects/lobby"))
            {
                string[] objectPaths = Directory.GetFiles("Resources/objects/lobby");
                Array.Sort(objectPaths);
                foreach (var path in objectPaths)
                {
                    context.SendPacket(File.ReadAllBytes(path));
                }
            }
            else
            {
                Logger.WriteWarning("Directory 'Resources/objects/lobby' not found!");
            }

            // Spawn Character
            context.SendPacket(new CharacterSpawnPacket(context.Character));

            // Unlock Controls
            context.SendPacket(new NoPayloadPacket(0x03, 0x2B));

            // Spawn on other player's clients
            CharacterSpawnPacket spawnPacket = new CharacterSpawnPacket(context.Character);
            spawnPacket.IsItMe = false;
            foreach (Client c in Server.Instance.Clients)
            {
                if (c == context)
                    continue;

                if (c.Character == null)
                    continue;

                c.SendPacket(spawnPacket);

                CharacterSpawnPacket remoteChar = new CharacterSpawnPacket(c.Character);
                remoteChar.IsItMe = false;
                context.SendPacket(remoteChar);
            }

            // memset packet - Enables menus
            // Also holds event items and likely other stuff too
            byte[] memSetPacket = File.ReadAllBytes("Resources/setMemoryPacket.bin");
            context.SendPacket(0x23, 0x07, 0, memSetPacket);

            // Give a blank palette
            context.SendPacket(new PalettePacket());

            Logger.Write("[CHR] {0}'s character {1} has spawned", context.User.Username, context.Character.Name);
        }

        #endregion
    }
}
