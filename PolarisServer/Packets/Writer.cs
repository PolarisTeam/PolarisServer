using System;
using System.IO;
using System.Text;
using System.Runtime.InteropServices;

namespace PolarisServer.Packets
{
    public class Writer : BinaryWriter
    {
        public Writer()
            : base(new MemoryStream())
        {
        }

        public Writer(Stream s)
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

        public void WriteStruct<T>(T structure) where T : struct
        {
            var size = Marshal.SizeOf(structure);
            byte[] strArr = new byte[size];
            unsafe
            {
                IntPtr structPtr = Marshal.AllocHGlobal(size);

                Marshal.StructureToPtr(structure, structPtr, true);
                Marshal.Copy(structPtr, strArr, 0, size);
                Marshal.FreeHGlobal(structPtr);
            }
            Write(strArr);

        }

        public byte[] ToArray()
        {
            var ms = (MemoryStream)BaseStream;
            return ms.ToArray();
        }
    }
}

