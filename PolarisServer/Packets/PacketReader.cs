using PolarisServer.Models;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

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

        public PacketReader(byte[] bytes, uint position, uint size)
            : base(new MemoryStream(bytes, (int) position, (int) size))
        {
        }

        public uint ReadMagic(uint xor, uint sub)
        {
            return (ReadUInt32() ^ xor) - sub;
        }

        public string ReadAscii(uint xor, uint sub)
        {
            var magic = ReadMagic(xor, sub);

            if (magic == 0)
            {
                return "";
            }
            var charCount = magic - 1;
            var padding = 4 - (charCount & 3);

            var data = ReadBytes((int) charCount);
            for (var i = 0; i < padding; i++)
                ReadByte();

            return Encoding.ASCII.GetString(data);
        }

        public string ReadFixedLengthAscii(uint charCount)
        {
            var data = ReadBytes((int) charCount);
            var str = Encoding.ASCII.GetString(data);

            var endAt = str.IndexOf('\0');
            if (endAt == -1)
                return str;
            return str.Substring(0, endAt);
        }

        public string ReadUtf16(uint xor, uint sub)
        {
            var magic = ReadMagic(xor, sub);

            if (magic == 0)
            {
                return "";
            }
            var charCount = magic - 1;
            var padding = (magic & 1);

            var data = ReadBytes((int) (charCount*2));
            ReadUInt16();
            if (padding != 0)
                ReadUInt16();

            return Encoding.GetEncoding("UTF-16").GetString(data);
        }

        public string ReadFixedLengthUtf16(int charCount)
        {
            var data = ReadBytes(charCount*2);
            var str = Encoding.GetEncoding("UTF-16").GetString(data);

            var endAt = str.IndexOf('\0');
            if (endAt == -1)
                return str;
            return str.Substring(0, endAt);
        }

        public T ReadStruct<T>() where T : struct
        {
            var structBytes = new byte[Marshal.SizeOf(typeof (T))];
            Read(structBytes, 0, structBytes.Length);

            return Helper.ByteArrayToStructure<T>(structBytes);
        }

        public MysteryPositions ReadEntityPosition()
        {
            MysteryPositions pos = new MysteryPositions()
            {
                A = Helper.FloatFromHalfPrecision(ReadUInt16()),
                B = Helper.FloatFromHalfPrecision(ReadUInt16()),
                C = Helper.FloatFromHalfPrecision(ReadUInt16()),
                FacingAngle = Helper.FloatFromHalfPrecision(ReadUInt16()),
                X = Helper.FloatFromHalfPrecision(ReadUInt16()),
                Y = Helper.FloatFromHalfPrecision(ReadUInt16()),
                Z = Helper.FloatFromHalfPrecision(ReadUInt16()),
            };

            return pos;
        }
    }
}