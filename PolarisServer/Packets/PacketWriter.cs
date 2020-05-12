using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using PolarisServer.Models;

namespace PolarisServer.Packets
{
    public class PacketWriter : BinaryWriter
    {
        public PacketWriter()
            : base(new MemoryStream())
        {
        }

        public PacketWriter(Stream s)
            : base(s)
        {
        }

        public void WriteMagic(uint magic, uint xor, uint sub)
        {
            var encoded = (magic + sub) ^ xor;
            Write(encoded);
        }

        public void WriteAscii(string str, uint xor, uint sub)
        {
            if (str.Length == 0)
            {
                WriteMagic(0, xor, sub);
            }
            else
            {
                // Magic, followed by string, followed by null terminator,
                // followed by padding characters if needed.
                var charCount = (uint) str.Length;
                var padding = 4 - (charCount & 3);

                WriteMagic(charCount + 1, xor, sub);
                Write(Encoding.ASCII.GetBytes(str));
                for (var i = 0; i < padding; i++)
                    Write((byte) 0);
            }
        }

        public void WriteUtf16(string str, uint xor, uint sub)
        {
            if (str.Length == 0)
            {
                WriteMagic(0, xor, sub);
            }
            else
            {
                // Magic, followed by string, followed by null terminator,
                // followed by a padding character if needed.
                var charCount = (uint) str.Length + 1;
                var padding = (charCount & 1);

                WriteMagic(charCount, xor, sub);
                Write(Encoding.GetEncoding("UTF-16").GetBytes(str));
                Write((ushort) 0);
                if (padding != 0)
                    Write((ushort) 0);
            }
        }

        public void WriteFixedLengthASCII(string str, int charCount)
        {
            var writeAmount = Math.Min(str.Length, charCount);
            var paddingAmount = charCount - writeAmount;

            if (writeAmount > 0)
            {
                var chopped = writeAmount != str.Length ? str.Substring(0, writeAmount) : str;

                Write(Encoding.GetEncoding("ASCII").GetBytes(chopped));
            }

            if (paddingAmount > 0)
            {
                for (var i = 0; i < paddingAmount; i++)
                    Write((byte) 0);
            }
        }

        public void WriteFixedLengthUtf16(string str, int charCount)
        {
            var writeAmount = Math.Min(str.Length, charCount);
            var paddingAmount = charCount - writeAmount;

            if (writeAmount > 0)
            {
                var chopped = writeAmount != str.Length ? str.Substring(0, writeAmount) : str;

                Write(Encoding.GetEncoding("UTF-16").GetBytes(chopped));
            }

            if (paddingAmount > 0)
            {
                for (var i = 0; i < paddingAmount; i++)
                    Write((ushort) 0);
            }
        }

        public void Write(PSOLocation s)
        {
            Write(Helper.FloatToHalfPrecision(s.RotX));
            Write(Helper.FloatToHalfPrecision(s.RotY));
            Write(Helper.FloatToHalfPrecision(s.RotZ));
            Write(Helper.FloatToHalfPrecision(s.RotW));
            Write(Helper.FloatToHalfPrecision(s.PosX));
            Write(Helper.FloatToHalfPrecision(s.PosY));
            Write(Helper.FloatToHalfPrecision(s.PosZ));
        }

        public void WritePlayerHeader(uint id)
        {
            Write(id);
            Write((uint) 0);
            Write((ushort) 4);
            Write((ushort) 0);
        }

        public unsafe void WriteStruct<T>(T structure) where T : struct
        {
            var strArr = new byte[Marshal.SizeOf(structure)];

            fixed (byte* ptr = strArr)
            {
                Marshal.StructureToPtr(structure, (IntPtr) ptr, false);
            }

            Write(strArr);
        }

        public byte[] ToArray()
        {
            var ms = (MemoryStream) BaseStream;
            return ms.ToArray();
        }

        internal void WriteBytes(byte b, uint count)
        {
            for(int i = 0; i < count; i++)
            {
                Write(b);
            }
        }
    }
}