using System;
using System.Runtime.InteropServices;
using System.ComponentModel.DataAnnotations;

using PolarisServer.Packets;
using PolarisServer.Database;

namespace PolarisServer.Models
{
    public class Character
    {
        public enum Race : ushort
        {
            Human = 0,
            Newman,
            Cast,
            Dewman
        }

        public enum Gender : ushort
        {
            Male = 0,
            Female
        }

        public enum ClassType : byte
        {
            Hunter = 0,
            Fighter,
            Ranger,
            Gunner,
            Force,
            Techer,
            Braver,
            Bouncer,
        }
        
        [Flags]
        public enum ClassTypeField : byte
        {
            Hunter = 1,
            Fighter = 2,
            Ranger = 4,
            Gunner = 8,
            Force = 16,
            Techer = 32,
            Braver = 64,
            Bouncer = 128
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct HSVColor
        {
            public ushort hue, saturation, value;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Figure
        {
            public ushort x, y, z; // Great naming, SEGA
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct JobEntry
        {
            public ushort level;
            public ushort level2; // Usually the same as the above, what is this used for?
            public uint exp;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Entries
        {
            public JobEntry hunter, fighter, ranger, gunner, force, techer, braver, bouncer;
        }

        [StructLayout(LayoutKind.Sequential)]
        public unsafe struct JobParam
        {
            public fixed byte unknown_0[4];
            public ClassType mainClass;
            public ClassType subClass;
            public fixed byte uknown_6[2];
            public ClassTypeField enabledClasses;
            public fixed byte uknown_8[2];
            public byte padding0;

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
            public fixed byte padding[4];
            public ushort height;
            public fixed byte charData[80]; // Figure Data, needs more work
            public ushort accessoryData1;
            public ushort accessoryData2;
            public ushort accessoryData3;
            public ushort accessoryData4;
            public HSVColor costumeColor;
            public HSVColor mainColor;
            public HSVColor sub1Color;
            public HSVColor sub2Color;
            public HSVColor sub3Color;
            public HSVColor eyeColor;
            public HSVColor hairColor;
            public int modelID;
            public ushort mainParts;
            public ushort bodyPaint;
            public ushort emblem;
            public ushort eyePattern;
            public ushort eyelashes;
            public ushort eyebrows;
            public ushort face;
            public ushort facePaint1;
            public ushort hairstyle;
            public ushort accessory1;
            public ushort accessory2;
            public ushort accessory3;
            public ushort facePaint2;
            public ushort arms;
            public ushort legs;
            public ushort accessory4;
            public ushort costume;
            public Race race;
            public Gender gender;
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
    }


    public struct MysteryPositions
    {
        public float a, b, c, facingAngle, x, y, z;
    }
}
