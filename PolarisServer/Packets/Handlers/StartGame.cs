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

            var character = PolarisApp.Instance.Database.Characters.Find(charID);

            if (character == null || character.Player.PlayerID != context.User.PlayerID)
                return;

            context.Charcter = character;

            // Transition to the loading screen
            context.SendPacket(new NoPayloadPacket(0x3, 0x4));

            // TODO Set area, Set character, possibly more. See PolarisLegacy for more.
        }
    }
}

