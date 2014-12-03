using System;
using System.IO;

using PolarisServer.Models;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x3E)]
    public class CharacterSpawn : PacketHandler
    {
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
            }

            // Set Area
            var setAreaPacket = File.ReadAllBytes("testSetAreaPacket.bin");
            context.SendPacket(0x03, 0x24, 4, setAreaPacket);

            // Set Player ID
            var setPlayerID = new PacketWriter();
            setPlayerID.WritePlayerHeader((uint)context.User.PlayerID);
            context.SendPacket(0x06, 0x00, 0, setPlayerID.ToArray());

            /* Spawn Lobby Objects
             * DON"T FORGET TO RE-ENABLE THIS BEFORE COMITTING!
             * turned off for less packet spam
            if (Directory.Exists("objects/lobby"))
            {
                var objectPaths = Directory.GetFiles("objects/lobby");
                Array.Sort(objectPaths);
                foreach (var path in objectPaths)
                {
                    context.SendPacket(System.IO.File.ReadAllBytes(path));
                }
            }
            else
            {
                Logger.WriteWarning("Directory 'objects/lobby' not found!");
            }
            */

            // Spawn Character
            context.SendPacket(new CharacterSpawnPacket(context.Character));

            // Unlock Controls
            context.SendPacket(new NoPayloadPacket(0x03, 0x2B));

            // Spawn on other player's clients
            var spawnPacket = new CharacterSpawnPacket(context.Character);
            spawnPacket.IsItMe = false;
            foreach (Client c in Server.Instance.Clients)
            {
                if (c == context)
                    continue;

                if (c.Character == null)
                    continue;

                c.SendPacket(spawnPacket);

                var remoteChar = new CharacterSpawnPacket(c.Character);
                remoteChar.IsItMe = false;
                context.SendPacket(remoteChar);
            }

            // memset packet - Enables menus
            // Also holds event items and likely other stuff too
            var memSetPacket = System.IO.File.ReadAllBytes("setMemoryPacket.bin");
            context.SendPacket(0x23, 0x07, 0, memSetPacket);

            // Give a blank palette
            context.SendPacket(new PalettePacket());

            Logger.Write("[CHR] {0}'s character has spawned", context.User.Username);
        }
    }
}
