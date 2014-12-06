using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

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
            uint encoded = (magic + sub) ^ xor;
            Write(encoded);
        }

        public void WriteASCII(string str, uint xor, uint sub)
        {
            if (str.Length == 0)
            {
                WriteMagic(0, xor, sub);
            }
            else
            {
                // Magic, followed by string, followed by null terminator,
                // followed by padding characters if needed.
                uint charCount = (uint)str.Length;
                uint padding = 4 - (charCount & 3);

                WriteMagic(charCount + 1, xor, sub);
                Write(Encoding.ASCII.GetBytes(str));
                for (int i = 0; i < padding; i++)
                    Write((byte)0);
            }
        }

        public void WriteUTF16(string str, uint xor, uint sub)
        {
            if (str.Length == 0)
            {
                WriteMagic(0, xor, sub);
            }
            else
            {
                // Magic, followed by string, followed by null terminator,
                // followed by a padding character if needed.
                uint charCount = (uint)str.Length + 1;
                uint padding = (charCount & 1);

                WriteMagic(charCount, xor, sub);
                Write(Encoding.GetEncoding("UTF-16").GetBytes(str));
                Write((ushort)0);
                if (padding != 0)
                    Write((ushort)0);
            }
        }

        public void WriteFixedLengthASCII(string str, int charCount)
        {
            int writeAmount = Math.Min(str.Length, charCount);
            int paddingAmount = charCount - writeAmount;

            if (writeAmount > 0)
            {
                string chopped;
                if (writeAmount != str.Length)
                    chopped = str.Substring(0, writeAmount);
                else
                    chopped = str;

                Write(Encoding.GetEncoding("ASCII").GetBytes(chopped));
            }

            if (paddingAmount > 0)
            {
                for (int i = 0; i < paddingAmount; i++)
                    Write((byte)0);
            }
        }

        public void WriteFixedLengthUTF16(string str, int charCount)
        {
            int writeAmount = Math.Min(str.Length, charCount);
            int paddingAmount = charCount - writeAmount;

            if (writeAmount > 0)
            {
                string chopped;
                if (writeAmount != str.Length)
                    chopped = str.Substring(0, writeAmount);
                else
                    chopped = str;

                Write(Encoding.GetEncoding("UTF-16").GetBytes(chopped));
            }

            if (paddingAmount > 0)
            {
                for (int i = 0; i < paddingAmount; i++)
                    Write((ushort)0);
            }
        }

        public void Write(MysteryPositions s)
        {
            Write(Helper.FloatToHalfPrecision(s.a));
            Write(Helper.FloatToHalfPrecision(s.b));
            Write(Helper.FloatToHalfPrecision(s.c));
            Write(Helper.FloatToHalfPrecision(s.facingAngle));
            Write(Helper.FloatToHalfPrecision(s.x));
            Write(Helper.FloatToHalfPrecision(s.y));
            Write(Helper.FloatToHalfPrecision(s.z));
        }

        public void WritePlayerHeader(uint id)
        {
            Write(id);
            Write((uint)0);
            Write((ushort)4);
            Write((ushort)0);
        }

        public unsafe void WriteStruct<T>(T structure) where T : struct
        {
            byte[] strArr = new byte[Marshal.SizeOf(structure)];

            fixed (byte* ptr = strArr)
            {
                Marshal.StructureToPtr(structure, (IntPtr)ptr, false);
            }

            Write(strArr);
        }

        public byte[] ToArray()
        {
            MemoryStream ms = (MemoryStream)BaseStream;
            return ms.ToArray();
        }
    }
}

