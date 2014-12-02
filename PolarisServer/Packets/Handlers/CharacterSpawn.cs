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

            // Set Area
            var setAreaPacket = System.IO.File.ReadAllBytes("testSetAreaPacket.bin");
            context.SendPacket(3, 0x24, 4, setAreaPacket);

            // Set Player ID
            var setPlayerID = new PacketWriter();
            setPlayerID.WritePlayerHeader((uint)context.User.PlayerID);
            context.SendPacket(6, 0, 0, setPlayerID.ToArray());

            // Spawn Lobby Objects
            if (System.IO.Directory.Exists("objects/lobby"))
            {
                var objectPaths = System.IO.Directory.GetFiles("objects/lobby");
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

            // Spawn Character
            context.SendPacket(new CharacterSpawnPacket(context.Character));

            // Unlock Controls
            context.SendPacket(new NoPayloadPacket(3, 0x2B));

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
            context.SendPacket(0x23, 0x7, 0, memSetPacket);

            Logger.Write("[CHR] {0}'s character has respawned", context.User.Username);
        }
    }
}
