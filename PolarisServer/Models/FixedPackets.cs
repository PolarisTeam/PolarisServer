using System;

namespace PolarisServer.Models
{
    public struct PacketHeader
    {
        public byte flags1;
        public byte flags2;
        public UInt32 size;
        public byte subtype;
        public byte type;

        public PacketHeader(int size, byte type, byte subtype, byte flags1, byte flags2)
        {
            this.size = (uint) size;
            this.type = type;
            this.subtype = subtype;
            this.flags1 = flags1;
            this.flags2 = flags2;
        }

        public PacketHeader(byte type, byte subtype) : this(type, subtype, 0)
        {
        }

        public PacketHeader(byte type, byte subtype, byte flags1) : this(0, type, subtype, flags1, 0)
        {
        }
    }
}