using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolarisServer.Models;
using PolarisServer.Packets.Handlers;

namespace PolarisServer.Packets.PSOPackets
{
    class MovementPacket : Packet
    {
        FullMovementData data;

        public MovementPacket(FullMovementData data)
        {
            this.data = data;
        }

        public override byte[] Build()
        {
            PacketWriter pw = new PacketWriter();
            pw.WriteStruct(data);
            return pw.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x4, 0x7, PacketFlags.ENTITY_HEADER);
        }
    }
}
