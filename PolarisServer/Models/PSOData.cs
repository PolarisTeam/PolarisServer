using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Models
{
    struct EntityHeader
    {
        public UInt32 ID;
        public UInt32 Unknown_4;
        public UInt16 EntityType; // Maybe...
        public UInt16 Unknown_A;
    }
}
