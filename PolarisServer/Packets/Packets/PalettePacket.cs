using System;

using PolarisServer.Models;

namespace PolarisServer.Packets
{
    public class PalettePacket : Packet
    {
        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();

            // Enable flag
            writer.Write((byte)1);

            // Blank out the rest (skills)
            for (int i = 0; i < 1091; i++)
                writer.Write((byte)0);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x21, 0x01);
        }

        #endregion
    }
}
