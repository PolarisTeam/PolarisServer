using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace PolarisServer
{
    public static class Helper
    {
        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }

        public static byte[] StringToByteArray(String hex)
        {
            int NumberChars = hex.Length;
            byte[] bytes = new byte[NumberChars / 2];
            for (int i = 0; i < NumberChars; i += 2)
                bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);
            return bytes;
        }

        public static T ByteArrayToStructure<T>(byte[] bytes) where T: struct 
        {
            GCHandle handle = GCHandle.Alloc(bytes, GCHandleType.Pinned);
            T stuff = (T)Marshal.PtrToStructure(handle.AddrOfPinnedObject(),
                typeof(T));
            handle.Free();
            return stuff;
        }

        public static string ObjectToString(object obj)
        {
            string data = string.Empty;

            data += "{ ";
            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(obj))
                data += string.Format("{0} = {1}, ", descriptor.Name, descriptor.GetValue(obj));
            data = data.Remove(data.Length - 3);
            data += " }";

            return data;
        }
        
        #region Float Manipulation

        public static unsafe float UIntToFloat(uint input)
        {
            float* fp = (float*)(&input);
            return *fp;
        }
        public static unsafe uint FloatToUInt(float input)
        {
            uint* ip = (uint*)(&input);
            return *ip;
        }

        public static float FloatFromHalfPrecision(ushort value)
        {
            if ((value & 0x7FFF) != 0)
            {
                uint sign = (uint)((value & 0x8000) << 16);
                uint exponent = (uint)(((value & 0x7C00) >> 10) + 0x70) << 23;
                uint mantissa = (uint)((value & 0x3FF) << 13);
                return UIntToFloat(sign | exponent | mantissa);
            }
            else
            {
                return 0;
            }
        }

        public static ushort FloatToHalfPrecision(float value)
        {
            uint ivalue = FloatToUInt(value);
            if ((ivalue & 0x7FFFFFFF) != 0)
            {
                ushort sign = (ushort)((ivalue >> 16) & 0x8000);
                ushort exponent = (ushort)(((ivalue & 0x7F800000) >> 23) - 0x70);
                if ((exponent & 0xFFFFFFE0) != 0)
                {
                    return (ushort)((exponent >> 17) ^ 0x7FFF | sign);
                }
                else
                {
                    ushort a = (ushort)((ivalue & 0x7FFFFF) >> 13);
                    ushort b = (ushort)(exponent << 10);
                    return (ushort)(a | b | sign);
                }
            }
            else
            {
                return (ushort)(ivalue >> 16);
            }
        }

        #endregion

        public static int FindPlayerByUsername(string name)
        {
            for (int i = 0; i < PolarisApp.Instance.server.Clients.Count; i++)
                if (name.ToLower() == PolarisApp.Instance.server.Clients[i].User.Username.ToLower())
                    return i;

            return -1;
        }

        public static ushort PacketTypeToUShort(uint type, uint subtype)
        {
            return (ushort)((type << 8) | subtype);
        }

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
