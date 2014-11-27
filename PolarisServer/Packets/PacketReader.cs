using System;
using System.IO;
using System.Runtime.InteropServices;

namespace PolarisServer.Packets
{
    public class PacketReader : BinaryReader
    {
        public PacketReader(Stream s) : base(s)
        {
        }

        public PacketReader(byte[] bytes) : base(new MemoryStream(bytes))
        {
        }

        public unsafe T ReadStruct<T>() where T : struct
        {
            byte[] structBytes = ReadBytes(Marshal.SizeOf(typeof(T)));

            return Helper.ByteArrayToStructure<T>(structBytes);
        }
    }
}

