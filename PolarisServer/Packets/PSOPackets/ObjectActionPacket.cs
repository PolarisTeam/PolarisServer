using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    class ObjectActionPacket : Packet
    {
        private EntityHeader headerA; // Destination player / object?
        private EntityHeader headerB; // Calling player / object?
        private EntityHeader headerC; // Maybe argument player / object?
        private EntityHeader headerD; // Probably another arg I guess?
        private string command;       // ASCII command

        public ObjectActionPacket(EntityHeader headerA, EntityHeader headerB, EntityHeader headerC, EntityHeader headerD, string command)
        {
            this.headerA = headerA;
            this.headerB = headerB;
            this.headerC = headerC;
            this.headerD = headerD;
            this.command = command;
        }

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();

            writer.WriteStruct(headerA);
            writer.WriteStruct(headerB);
            writer.Write((uint)0); // Padding
            writer.WriteStruct(headerC);
            writer.WriteStruct(headerD);
            writer.Write((uint)0); // Padding
            writer.WriteAscii(command, 0x5CCF, 0x15);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x4, 0x15, (PacketFlags.ENTITY_HEADER | PacketFlags.STREAM_PACKED));
        }
    }
}
