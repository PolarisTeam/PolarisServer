using System;

namespace PolarisServer.Models
{
    public struct EntityHeader
    {
        public UInt16 EntityType; // Maybe...
        public UInt32 Id;
        public UInt32 Unknown4;
        public UInt16 UnknownA;
    }
}