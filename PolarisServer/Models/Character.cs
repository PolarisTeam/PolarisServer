using System;
using System.Runtime.InteropServices;

namespace PolarisServer.Models
{
    public class Character
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct HSVColor
        {
            public ushort hue, saturation, value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JobEntry
        {
            public ushort level;
            public ushort unknown_2;
            public uint exp;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Entries
        {
            public JobEntry entry0, entry1, entry2, entry3, entry4, entry5, entry6, entry7;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct JobParam
        {
            public fixed byte unknown_0[4];
            public uint unknown_4;
            public Entries entries;

            public ushort unknown_48, unknown_4A;
            public uint unknown_4C;
            public ushort unknown_50, unknown_52;
            public uint unknown_54;
            public ushort unknown_58, unknown_5A;
            public uint unknown_5C;
            public ushort unknown_60, unknown_62;
            public uint unknown_64;
            public uint unknown_68;
            public ushort unknown_6C, unknown_6E;
            public fixed int unknown_70[4];
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct LooksParam
        {
            public fixed byte charData[86];
            public fixed ushort accData[3];
            public fixed byte unknown_4[6];
            public HSVColor costumeColor;
            public fixed byte unknown_5[12];
            public HSVColor skinColor;
            public HSVColor eyeColor;
            public fixed byte unknown_6[6];
            public HSVColor hairColor;
            public fixed byte unknown_7[2];
            public ushort bodyPaint;
            public byte emblem;
            public byte eyePattern;
            public ushort eyebrows;
            public byte eyelashes;
            public uint faceVariant;
            public byte makeupPattern1;
            public byte hairstyle;
            public ushort acc1;
            public ushort acc2;
            public ushort acc3;
            public byte makeupPattern2;
            public ushort acc4;
            public ushort costume;
            public fixed byte unknown_8[2];
            public byte race;
            public byte unknown_9;
            public byte female;
            public fixed byte unknown_10[5];
        }

        // Probably more info than this
        public uint CharacterId;
        public uint PlayerId;
        public string Name;
        public LooksParam Looks;
        public JobParam Job;

        public Character()
        {
        }
    }
}
