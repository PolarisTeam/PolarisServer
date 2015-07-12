using System;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Models
{
    public enum EntityType : UInt16
    {
        Player = 0x4,
        Object = 0x6
    }

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct EntityHeader
    {
        [FieldOffset(0x0)]
        public UInt64 ID;
        [FieldOffset(0x8)]
        public EntityType EntityType; // Maybe...
        [FieldOffset(0xA)]
        public UInt16 Unknown_A;

        public EntityHeader(ulong id, EntityType type) : this()
        {
            this.ID = id;
            this.EntityType = type;
        }
    }
}
