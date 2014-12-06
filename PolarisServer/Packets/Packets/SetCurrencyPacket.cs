using System;
using System.IO;

using PolarisServer.Models;

namespace PolarisServer.Packets
{
    public class SetCurrencyPacket : Packet
    {
        public int newACAmount = 0;
        public int newFUNAmount = 0;

        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();

            // AC
            writer.Write(newACAmount);

            // Padding?
            for (int i = 0; i < 20; i++)
                writer.Write((byte)0);

            // FUN
            writer.Write(newFUNAmount);

            // Padding?
            for (int i = 0; i < 4; i++)
                writer.Write((byte)0);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x11, 0x1C);
        }

        #endregion
    }
}
