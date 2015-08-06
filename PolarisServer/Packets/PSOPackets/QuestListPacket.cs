using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolarisServer.Models;
using System.Runtime.InteropServices;

namespace PolarisServer.Packets.PSOPackets
{
    class QuestListPacket : Packet
    {
        private QuestDefiniton[] questdefs;

        public QuestListPacket(QuestDefiniton[] questdefs)
        {
            this.questdefs = questdefs;
        }

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();
            writer.WriteMagic((uint)questdefs.Length, 0x1DB0, 0xC5);
            foreach (QuestDefiniton d in questdefs)
            {
                writer.WriteStruct(d);
            }
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0xB, 0x18, 0x4);
        }

        // Hoo boy, this is 468 bytes!
        // TODO: Map out this struct.
        // Most of this is WRONG!!! Needs serious investigation. 
        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public unsafe struct QuestDefiniton
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32 - 8)]
            public string dateOrSomething;
            public int field_18;
            public int field_1C;
            public int needsToBeNonzero;
            public int alsoGetsSetToDword;
            public UInt16 getsSetToWord;
            public UInt16 moreWordSetting;
            public int questNameString;
            public int field_30;
            public int field_34;
            public int field_38;
            public int field_3C;
            public int field_40;
            public int field_44;
            public int field_48;
            public int field_4C;
            public int field_50;
            public int field_54;
            public int field_58;
            public int field_5C;
            public int field_60;
            public int field_64;
            public int field_68;
            public int field_6C;
            public int field_70;
            public int field_74;
            public int field_78;
            public int field_7C;
            public int field_80;
            public int field_84;
            public int field_88;
            public int field_8C;
            public int field_90;
            public int field_94;
            public int field_98;
            public UInt16 field_9C;
            public char field_9E;
            public char field_9F;
            public int field_A0;
            public int field_A4;
            public int field_A8;
            public int field_AC;
            public int field_B0;
            public int field_B4;
            public int field_B8;
            public int field_BC;
            public int field_C0;
            public int field_C4;
            public int field_C8;
            public int field_CC;
            public int field_D0;
            public int field_D4;
            public int field_D8;
            public int field_DC;
            public int field_E0;
            public int field_E4;
            public int field_E8;
            public int field_EC;
            public UInt16 field_F0;
            public UInt16 field_F2;
            public UInt16 field_F4;
            public char field_F6;
            public char field_F7;
            public char field_F8;
            public char field_F9;
            public char field_FA;
            public char field_FB;
            public char field_FC;
            public char field_FD;
            public char field_FE;
            public char field_FF;
            public char field_100;
            public char field_101;
            public char field_102;
            public char field_103;
            public char field_104;
            public char field_105;
            public UInt16 field_106;
            public int field_108;
            public int field_10C;
            public short field_110;
            public byte playtime;
            public byte partyType;
            public QuestDefThing field_114_1;
            public QuestDefThing field_114_2;
            public QuestDefThing field_114_3;
            public QuestDefThing field_114_4;
            public QuestDefThing field_114_5;
            public QuestDefThing field_114_6;
            public QuestDefThing field_114_7;
            public QuestDefThing field_114_8;
            public QuestDefThing field_114_9;
            public QuestDefThing field_114_10;
            public QuestDefThing field_114_11;
            public QuestDefThing field_114_12;
            public QuestDefThing field_114_13;
            public QuestDefThing field_114_14;
            public QuestDefThing field_114_15;
            public QuestDefThing field_114_16;
        }

        public struct QuestDefThing
        {
            int field_0;
            int field_4;
            char field_8;
            char field_9;
            UInt16 field_A;
        }

        [Flags]
        public enum QuestBitfield1
        {
            MatterObjectiveQuest = 0x0001,
            NewQuest = 0x0100,
            ClientOrder = 0x0800,
            UnknownLevel = 0x1000
        }

        public enum PartyType
        {
            SoloQuest = 1,
            SinglePartyQuest = 2,
            MultiPartyQuest = 3
        }

        public enum EstimatedTime
        {
            Short = 1,
            Medium = 2,
            Long = 3
        }

        [Flags]
        public enum DifficultiesAvailable
        {
            Normal = 0x01,
            hard = 0x02,
            VeryHard = 0x04,
            SuperHard = 0x08,
            ExtraHard = 0x10,
            Dummy1 = 0x20,
            Dummy2 = 0x40,
            Dummy3 = 0x80,
        }

        [Flags]
        public enum DifficultiesCompleted
        {
            Normal = 0x01,
            hard = 0x02,
            VeryHard = 0x04,
            SuperHard = 0x08,
            ExtraHard = 0x10,
            Dummy1 = 0x20,
            Dummy2 = 0x40,
            Dummy3 = 0x80,
        }
    }
}
