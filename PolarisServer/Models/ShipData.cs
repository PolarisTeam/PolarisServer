using System;
using System.Runtime.InteropServices;

namespace PolarisServer.Models
{
    public enum ShipStatus
    {
        SHIP_UNKNOWN = 0,
        SHIP_ONLINE,
        SHIP_BUSY,
        SHIP_FULL,
        SHIP_OFFLINE
    }

    [StructLayout(LayoutKind.Sequential)]
    public unsafe struct ShipEntry
    {
        public UInt32 number;
        public fixed char name[16];
        public fixed byte ip[4];
        UInt32 zero;
        public UInt16 status;
        public UInt16 order;
        public UInt32 unknown;
    }
}
