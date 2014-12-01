using System;
using System.IO;
using System.Text;
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

        public PacketReader(byte[] bytes, uint position, uint size) : base(new MemoryStream(bytes, (int)position, (int)size))
        {
        }

        public uint ReadMagic(uint xor, uint sub)
        {
            return (ReadUInt32() ^ xor) - sub;
        }

        public string ReadASCII(uint xor, uint sub)
        {
            uint magic = ReadMagic(xor, sub);

            if (magic == 0)
            {
                return "";
            }
            else
            {
                uint charCount = magic - 1;
                uint padding = 4 - (charCount & 3);

                byte[] data = ReadBytes((int)charCount);
                for (int i = 0; i < padding; i++)
                    ReadByte();

                return Encoding.ASCII.GetString(data);
            }
        }

        public string ReadFixedLengthASCII(uint charCount)
        {
            byte[] data = ReadBytes((int)charCount);
            string str = Encoding.ASCII.GetString(data);

            int endAt = str.IndexOf('\0');
            if (endAt == -1)
                return str;
            else
                return str.Substring(0, endAt);
        }

        public string ReadUTF16(uint xor, uint sub)
        {
            uint magic = ReadMagic(xor, sub);

            if (magic == 0)
            {
                return "";
            }
            else
            {
                uint charCount = magic - 1;
                uint padding = (magic & 1);

                byte[] data = ReadBytes((int)(charCount * 2));
                ReadUInt16();
                if (padding != 0)
                    ReadUInt16();

                return Encoding.GetEncoding("UTF-16").GetString(data);
            }
        }

        public string ReadFixedLengthUTF16(int charCount)
        {
            byte[] data = ReadBytes(charCount * 2);
            string str = Encoding.GetEncoding("UTF-16").GetString(data);

            int endAt = str.IndexOf('\0');
            if (endAt == -1)
                return str;
            else
                return str.Substring(0, endAt);
        }

        public unsafe T ReadStruct<T>() where T : struct
        {
            byte[] structBytes = new byte[Marshal.SizeOf(typeof(T))];
            Read(structBytes, 0, structBytes.Length);

            return Helper.ByteArrayToStructure<T>(structBytes);
        }
    }
}

