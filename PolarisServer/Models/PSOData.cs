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

    public struct ObjectHeader
    {
        public UInt32 ID;
        public UInt32 padding; // Always is padding
        public EntityType EntityType; // Maybe...
        public UInt16 Unknown_A;

        public ObjectHeader(uint id, EntityType type) : this()
        {
            this.ID = id;
            this.EntityType = type;
        }
    }
}
