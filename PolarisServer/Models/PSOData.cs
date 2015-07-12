using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Models
{
    public enum EntityType
    {
        Player = 0x4,
        Object = 0x6
    }

    public struct EntityHeader
    {
        public UInt32 ID;
        public UInt32 Unknown_4;
        public UInt16 EntityType; // Maybe...
        public UInt16 Unknown_A;
    }
}
