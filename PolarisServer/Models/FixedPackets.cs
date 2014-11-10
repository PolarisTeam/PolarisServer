using System;
using System.Runtime.InteropServices;
using PolarisServer.Models;

namespace PolarisServer.Models
{
    public struct PacketHeader
    {
        UInt32 size;
        byte type;
        byte subtype;
        byte flags1;
        byte flags2;

        public PacketHeader(int size, byte type, byte subtype, byte flags1, byte flags2)
        {
            this.size = (uint)size;
            this.type = type;
            this.subtype = subtype;
            this.flags1 = flags1;
            this.flags2 = flags2;
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct CharacterListPacket
    {
        public UInt32 numberOfCharacters, characterId, playerID;
        public fixed byte unknown_13[0xC];
        public fixed ushort name[16];
        public UInt32 padding;
        public Character.LooksParam looks;
        public Character.JobParam jobs;

    }
}

