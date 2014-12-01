using System;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x4)]
    public class StartGame : PacketHandler
    {
        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            var reader = new PacketReader(data, position, size);
            var charID = reader.ReadUInt32();

            if (context.User == null)
                return;

            if (context.Character == null) // On character create, this is already set.
            {
                var character = PolarisApp.Instance.Database.Characters.Find((int)charID);

                if (character == null || character.Player.PlayerID != context.User.PlayerID)
                    return;

                context.Character = character;
            }

            // Transition to the loading screen
            context.SendPacket(new NoPayloadPacket(0x3, 0x4));

            // TODO Set area, Set character, possibly more. See PolarisLegacy for more.
        }
    }

    [PacketHandlerAttr(3, 3)]
    public class InitialLoad : PacketHandler
    {
        // Ninji note: 3-3 may not be the correct place to do this
        // Once we have better state tracking, we should make sure that
        // 3-3 only does anything at the points where the client is supposed
        // to be sending it, etc etc

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

            /* Spawn more characters just because we can
             * Don't do this, ever --Ninji
             * Moved this into a testing command for the hell of it --Kyle
            for (int i = 0; i < 50; i++)
            {
                var fakePlayer = new Database.Player();
                fakePlayer.Username = string.Format("Fake Player {0}", i);
                fakePlayer.Nickname = string.Format("Fake Name {0}", i);
                fakePlayer.PlayerID = 12345678 + i;
                
                var fakeChar = new Models.Character();
                fakeChar.CharacterID = 1234567 + i;
                fakeChar.Player = fakePlayer;
                fakeChar.Name = string.Format("Fake Char {0}", i);
                fakeChar.Looks = context.Character.Looks;
                fakeChar.Jobs = context.Character.Jobs;
                
                var fakePacket = new CharacterSpawnPacket(fakeChar);
                fakePacket.Position.facingAngle = (0.2f * i);
                fakePacket.Position.x = -0.417969f + (float)(Math.Sin(i) * 8.0);
                fakePacket.Position.y = 0.000031f;
                fakePacket.Position.z = 134.375f + (float)(Math.Cos(i) * 8.0);
                fakePacket.IsItMe = false;
                context.SendPacket(fakePacket);
            } */

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

        }
    }

    [PacketHandlerAttr(3, 0x10)]
    public class DoItMaybe : PacketHandler
    {
        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context.User == null || context.Character == null)
                return;
            context.SendPacket(new NoPayloadPacket(3, 0x23));
        }
    }
}

