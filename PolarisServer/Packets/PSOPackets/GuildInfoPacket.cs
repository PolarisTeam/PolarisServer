using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    internal class GuildInfoPacket : Packet
    {
        private readonly Character _character;

        public GuildInfoPacket(Character character)
        {
            _character = character;
        }

        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write((byte) 1); // Always 1?

            // Padding? Or the above is actually a uint
            for (var i = 0; i < 3; i++)
                writer.Write((byte) 0);

            // Character ID
            writer.Write((uint) _character.CharacterId);

            // Padding?
            for (var i = 0; i < 4; i++)
                writer.Write((byte) 0);

            // Nickname
            // TODO: The above and below may be switched around, need more data
            writer.WriteFixedLengthUtf16(_character.Player.Nickname, 16);

            // Padding?
            for (var i = 0; i < 36; i++)
                writer.Write((byte) 0);

            // Player name
            writer.WriteFixedLengthUtf16(_character.Name, 16);

            // Unknown?
            for (var i = 0; i < 24; i++)
                writer.Write((byte) 0);

            // Team Name
            // We don't actually have team names anywhere, just dump a test here
            writer.WriteFixedLengthUtf16("Polaris Team", 16);

            // Unknown
            // Somewhere in here is likely a Team ID
            for (var i = 0; i < 32; i++)
                writer.Write((byte) 0);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x1C, 0x1F, 0);
        }

        #endregion
    }
}