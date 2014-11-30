using System;

namespace PolarisServer
{
    public abstract class Packet
    {
        public abstract byte[] Build();
    }
}

