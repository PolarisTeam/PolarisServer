using System;

namespace PolarisServer.Models
{
    public struct PacketHeader
    {
        public byte Flags1;
        public byte Flags2;
        public UInt32 Size;
        public byte Subtype;
        public byte Type;

        public PacketHeader(int size, byte type, byte subtype, byte flags1, byte flags2)
        {
            Size = (uint) size;
            Type = type;
            Subtype = subtype;
            Flags1 = flags1;
            Flags2 = flags2;
        }

        public PacketHeader(byte type, byte subtype) : this(type, subtype, 0)
        {
        }

        public PacketHeader(byte type, byte subtype, byte flags1) : this(0, type, subtype, flags1, 0)
        {
        }
    }
}