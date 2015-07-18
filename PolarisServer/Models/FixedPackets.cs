using System;
using System.Runtime.InteropServices;
using PolarisServer.Models;

namespace PolarisServer.Models
{
    public struct PacketHeader
    {
        public UInt32 Size;
        public byte Type;
        public byte Subtype;
        public byte Flags1;
        public byte Flags2;

        public PacketHeader(int size, byte type, byte subtype, byte flags1, byte flags2)
        {
            this.Size = (uint)size;
            this.Type = type;
            this.Subtype = subtype;
            this.Flags1 = flags1;
            this.Flags2 = flags2;
        }

        public PacketHeader(byte type, byte subtype) : this(type, subtype, (byte)0)
        {
        }

        public PacketHeader(byte type, byte subtype, byte flags1) : this(0, type, subtype, flags1, 0)
        {
        }

        public PacketHeader(byte type, byte subtype, PacketFlags packetFlags) : this(type, subtype, (byte)packetFlags)
        {
        }
    }

    [Flags]
    public enum PacketFlags : byte
    {
        NONE,
        STREAM_PACKED = 0x4,
        FLAG_10 = 0x10,
        FULL_MOVEMENT = 0x20,
        ENTITY_HEADER = 0x40
    }

}

