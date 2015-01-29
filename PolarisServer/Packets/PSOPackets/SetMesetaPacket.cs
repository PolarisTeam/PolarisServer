using System;
using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    public class SetMesetaPacket : Packet
    {
        public Int64 NewAmount = 0;

        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            writer.Write(NewAmount);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader
            {
                Type = 0x0F,
                Subtype = 0x14,
                Flags1 = 0
            };
        }

        #endregion
    }
}