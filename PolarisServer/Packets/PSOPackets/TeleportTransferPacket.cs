using System;
using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    class TeleportTransferPacket : Packet
    {
        private PSOObject src;
        private PSOLocation dst;

        public TeleportTransferPacket(PSOObject srcTeleporter, PSOLocation destination)
        {
            src = srcTeleporter;
            dst = destination;
        }

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write(new byte[12]);
            writer.WriteStruct(src.Header);
            writer.WritePosition(dst);
            writer.Write(new byte[2]);
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x4, 0x2, PacketFlags.EntityHeader);
        }
    }
}
