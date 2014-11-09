using System;
using System.Runtime.InteropServices;
using PolarisServer.Models;

namespace PolarisServer.Models
{
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

