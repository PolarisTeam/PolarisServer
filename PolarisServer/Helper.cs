using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace PolarisServer
{
    public static class Helper
    {
        public static string ByteArrayToString(byte[] ba)
        {
            var hex = new StringBuilder(ba.Length * 2);
            foreach (var b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] StringToByteArray(String hex)
        {
            var numberChars = hex.Length;
            var bytes = new byte[numberChars / 2];
            for (var i = 0; i < numberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static T ByteArrayToStructure<T>(byte[] bytes) where T : struct
        {
            var handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            var stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                typeof(T));
            handle.Free();
            return stuff;
        }

        public static string ObjectToString(object obj)
        {
            var data = string.Empty;

            data += "{ ";
            data = TypeDescriptor.GetProperties(obj).Cast<PropertyDescriptor>().Aggregate(data, (current, descriptor) => current + string.Format("{0} = {1}, ", descriptor.Name, descriptor.GetValue(obj)));
            data = data.Remove(data.Length - 3);
            data += " }";

            return data;
        }

        public static int FindPlayerByUsername(string name)
        {
            for (var i = 0; i < PolarisApp.Instance.Server.Clients.Count; i++)
                if (name.ToLower() == PolarisApp.Instance.Server.Clients[i].User.Username.ToLower())
                    return i;

            return -1;
        }

        public static ushort PacketTypeToUShort(uint type, uint subtype)
        {
            return (ushort)((type << 8) | subtype);
        }

        #region Float Manipulation

        public static unsafe float UIntToFloat(uint input)
        {
            var fp = (float*)(&input);
            return *fp;
        }

        public static unsafe uint FloatToUInt(float input)
        {
            var ip = (uint*)(&input);
            return *ip;
        }

        public static float FloatFromHalfPrecision(ushort value)
        {
            if ((value & 0x7FFF) != 0)
            {
                var sign = (uint)((value & 0x8000) << 16);
                var exponent = (uint)(((value & 0x7C00) >> 10) + 0x70) << 23;
                var mantissa = (uint)((value & 0x3FF) << 13);
                return UIntToFloat(sign | exponent | mantissa);
            }
            return 0;
        }

        public static ushort FloatToHalfPrecision(float value)
        {
            var ivalue = FloatToUInt(value);
            if ((ivalue & 0x7FFFFFFF) != 0)
            {
                var sign = (ushort)((ivalue >> 16) & 0x8000);
                var exponent = (ushort)(((ivalue & 0x7F800000) >> 23) - 0x70);
                if ((exponent & 0xFFFFFFE0) != 0)
                {
                    return (ushort)((exponent >> 17) ^ 0x7FFF | sign);
                }
                var a = (ushort)((ivalue & 0x7FFFFF) >> 13);
                var b = (ushort)(exponent << 10);
                return (ushort)(a | b | sign);
            }
            return (ushort)(ivalue >> 16);
        }

        #endregion

        #region Timestamps

        public static long Timestamp(DateTime time)
        {
            return time.ToFileTimeUtc() / 10000;
        }

        public static DateTime Timestamp(long stamp)
        {
            return DateTime.FromFileTimeUtc(stamp * 10000);
        }

        #endregion
    }
}