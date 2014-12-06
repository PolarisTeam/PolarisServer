using System;
using System.IO;

using PolarisServer.Models;

namespace PolarisServer.Packets
{
    class GuildInfoPacket : Packet
    {
        Character character;

        public GuildInfoPacket(Character character)
        {
            this.character = character;
        }

        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();

            writer.Write((byte)1); // Always 1?

            // Padding? Or the above is actually a uint
            for (int i = 0; i < 3; i++)
                writer.Write((byte)0);

            // Character ID
            writer.Write((uint)character.CharacterID);

            // Padding?
            for (int i = 0; i < 4; i++)
                writer.Write((byte)0);

            // Nickname
            // TODO: The above and below may be switched around, need more data
            writer.WriteFixedLengthUTF16(character.Player.Nickname, 16);

            // Padding?
            for (int i = 0; i < 36; i++)
                writer.Write((byte)0);

            // Player name
            writer.WriteFixedLengthUTF16(character.Name, 16);

            // Unknown?
            for (int i = 0; i < 24; i++)
                writer.Write((byte)0);

            // Team Name
            // We don't actually have team names anywhere, just dump a test here
            writer.WriteFixedLengthUTF16("Polaris Team", 16);

            // Unknown
            // Somewhere in here is likely a Team ID
            for (int i = 0; i < 32; i++)
                writer.Write((byte)0);
            
            return writer.ToArray();
        }

        public override Models.PacketHeader GetHeader()
        {
            return new Models.PacketHeader(0x1C, 0x1F, 0);
        }

        #endregion
    }
}
