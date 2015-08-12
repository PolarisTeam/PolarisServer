using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;

namespace PolarisServer.Models
{
    /*
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PSO2ItemConsumable
    {
        long guid;
        int ID;
        int subID;
        short unused1;
        short quantity;
        fixed int unused2[9];
    }

    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public unsafe struct PSO2ItemWeapon
    {
        long guid;
        int ID;
        int subID;
        byte flags;
        byte element;
        byte force;
        byte grind;
        byte grindPercent;
        byte unknown1;
        short unknown2;
        fixed short affixes[8];
        int potential;
        byte extend;
        byte unknown3;
        short unknown4;
        int unknown5;
        int unknown6;
    }
    */

    public enum ItemType
    {
        Consumable,
        Weapon,
        Costume,
        Unit,
        Room
    }

    [Flags]
    public enum ItemFlags
    {
        Locked = 0x01,
        BoundToOwner = 0x02
    }

    public enum ItemElement
    {
        None,
        Fire,
        Ice,
        Lightning,
        Wind,
        Light,
        Dark
    }

    public class PSO2Item
    {
        public const int Size = 0x38;

        MemoryStream stream;
        ItemType type = ItemType.Consumable;
        byte[] data = new byte[Size];

        public override string ToString()
        {
            return string.Format("Data: {0:X}", BitConverter.ToString(data)).Replace('-', ' ');
        }

        public PSO2Item(byte[] data)
        {
            SetData(data);
        }

        public byte[] GetData()
        {
            return data;
        }

        public void SetData(byte[] data)
        {
            this.data = data;

            stream = new MemoryStream(data, true);
        }

        public long GetGUID()
        {
            byte[] guid = new byte[sizeof(long)];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(guid, 0, sizeof(long));

            return BitConverter.ToInt64(guid, 0);
        }

        public void SetGUID(long guid)
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes(guid), 0, 8);
        }

        public int[] GetID()
        {
            byte[] ID = new byte[sizeof(int)];
            byte[] subID = new byte[sizeof(int)];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(ID, 0x08, sizeof(int));
            stream.Read(subID, 0x0C, sizeof(int));

            return new int[] { BitConverter.ToInt32(ID, 0), BitConverter.ToInt32(subID, 0) };
        }

        public void SetID(int ID, int subID)
        {
            stream.Seek(0, SeekOrigin.Begin);
            stream.Write(BitConverter.GetBytes(ID), 0x08, sizeof(int));
            stream.Write(BitConverter.GetBytes(subID), 0x0C, sizeof(int));
        }

        // ...
    }
}
