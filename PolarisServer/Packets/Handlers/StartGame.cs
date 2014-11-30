using System;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x4)]
    public class StartGame : PacketHandler
    {
        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            var reader = new PacketReader(data, position, size);
            var charID = reader.ReadUInt32();

            if (context.User == null)
                return;

            var character = PolarisApp.Instance.Database.Characters.Find((int)charID);

            if (character == null || character.Player.PlayerID != context.User.PlayerID)
                return;

            context.Character = character;

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

        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context.User == null || context.Character == null)
                return;

            // Set Area
            var packet = System.IO.File.ReadAllBytes("testSetAreaPacket.bin");
            context.SendPacket(3, 0x24, 4, packet);

            // Set Player ID
            var setPlayerID = new PacketWriter();
            setPlayerID.WritePlayerHeader((uint)context.User.PlayerID);
            context.SendPacket(6, 0, 0, setPlayerID.ToArray());

            // Spawn Character
            context.SendPacket(new CharacterSpawnPacket(context.Character));

            // Unlock Controls
            context.SendPacket(new NoPayloadPacket(3, 0x2B));
        }
    }

    [PacketHandlerAttr(3, 0x10)]
    public class DoItMaybe : PacketHandler
    {
        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context.User == null || context.Character == null)
                return;
            context.SendPacket(new NoPayloadPacket(3, 0x23));
        }
    }
}

