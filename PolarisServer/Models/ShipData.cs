using System;
using System.Runtime.InteropServices;

namespace PolarisServer.Models
{
    public enum ShipStatus : ushort
    {
        SHIP_UNKNOWN = 0,
        SHIP_ONLINE,
        SHIP_BUSY,
        SHIP_FULL,
        SHIP_OFFLINE
    }

    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Auto)]
    public unsafe struct ShipEntry
    {
        public UInt32 number;

        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 16)]
        public string name;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] ip;

        public UInt32 zero;
        public ShipStatus status;
        public UInt16 order;
        public UInt32 unknown;
    }
}
