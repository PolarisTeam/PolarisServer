
using System;
using System.Runtime.InteropServices;

namespace PolarisServer.Models
{
    public enum ShipStatus : ushort
    {
        Unknown = 0,
        Online,
        Busy,
        Full,
        Offline
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
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
