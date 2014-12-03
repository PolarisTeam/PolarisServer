using System;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;

using PolarisServer.Packets;
using PolarisServer.Database;



namespace PolarisServer.Models
{
    public class Character
    {
        public enum ClassType
        {
            Hunter = 0,
            Ranger,
            Fighter,
            Gunner,
            Techer,
            Braver,
            Bouncer,
        }
        
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
            public uint mainClass; // Main Class
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
            public fixed byte unknown0[100];
            public HSVColor SkinColor;
            public HSVColor unknownColor0; // Cast Color parts maybe?
            public HSVColor eyeColor;
            public fixed byte unknown1[8]; // Cast Color parts maybe?
            public HSVColor hairColor;
            public fixed byte unknown2[12];
            public fixed byte unknown3[2]; // Seems to fuck around with the skin texture
            public fixed byte unknown4[4];
            public ushort hairstyle;
            public fixed byte unknown5[18];
            public ushort female;
        }

        // Probably more info than this
        [Key]
        public int CharacterID { get; set; }

        public virtual Player Player { get; set; }
        public string Name { get; set; } 

        public byte[] LooksBinary
        {
            get
            {
                PacketWriter w = new PacketWriter();
                w.WriteStruct(Looks);
                return w.ToArray();
            }

            set
            {
                Looks = Helper.ByteArrayToStructure<LooksParam>(value);
            }

        }

        public byte[] JobsBinary
        {
            get
            {
                PacketWriter w = new PacketWriter();
                w.WriteStruct(Jobs);
                return w.ToArray();
            }

            set
            {
                Jobs = Helper.ByteArrayToStructure<JobParam>(value);
            }

        }
            
        public LooksParam Looks { get; set; }
        public JobParam Jobs { get; set; }

        public Character()
        {
        }
    }


    public struct MysteryPositions
    {
        public float a, b, c, facingAngle, x, y, z;
    }
}
