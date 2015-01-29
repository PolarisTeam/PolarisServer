using System;

namespace PolarisServer.Models
{
    public struct EntityHeader
    {
        public UInt16 EntityType; // Maybe...
        public UInt32 ID;
        public UInt32 Unknown_4;
        public UInt16 Unknown_A;
    }
}