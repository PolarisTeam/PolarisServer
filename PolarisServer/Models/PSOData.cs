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
        Map = 0x5,
        Object = 0x6
    }

    [StructLayout(LayoutKind.Explicit, Size = 12)]
    public struct ObjectHeader
    {
        [FieldOffset(0x0)]
        public UInt64 ID;
        [FieldOffset(0x8)]
        public EntityType EntityType; // Maybe...
        [FieldOffset(0xA)]
        public UInt16 Unknown_A;

        public ObjectHeader(ulong id, EntityType type) : this()
        {
            this.ID = id;
            this.EntityType = type;
        }
    }
}
