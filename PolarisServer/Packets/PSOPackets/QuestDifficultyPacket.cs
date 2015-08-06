using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolarisServer.Models;
using System.Runtime.InteropServices;

namespace PolarisServer.Packets.PSOPackets
{
    class QuestDifficultyPacket : Packet
    {
        private QuestDifficulty[] questdiffs;

        public QuestDifficultyPacket(QuestDifficulty[] questdiffs)
        {
            // Setup dummy difficulty entries
            for (int i = 0; i < questdiffs.Length; i++)
            {
                QuestDifficultyEntry difficulty = new QuestDifficultyEntry();
                difficulty.field_4 = 0x268BCAE6;
                difficulty.field_8 = 0x02;
                difficulty.field_C = 0x29E4B5D8;
                difficulty.field_10 = 0x02;
                difficulty.field_14 = 0x29FE98B4;
                difficulty.field_18 = 0x02;
                difficulty.field_1C = 0x1514;

                QuestDifficultyEntry blankDifficulty = new QuestDifficultyEntry();
                blankDifficulty.field_4 = 0xFFFFFFFF;
                blankDifficulty.field_C = 0xFFFFFFFF;
                blankDifficulty.field_14 = 0xFFFFFFFF;

                questdiffs[i].difficulty1 = difficulty;
                questdiffs[i].difficulty2 = difficulty;
                questdiffs[i].difficulty3 = difficulty;
                questdiffs[i].difficulty4 = difficulty;
                questdiffs[i].difficulty5 = blankDifficulty;
                questdiffs[i].difficulty6 = blankDifficulty;
                questdiffs[i].difficulty7 = blankDifficulty;
                questdiffs[i].difficulty8 = blankDifficulty;
            }

            this.questdiffs = questdiffs;
        }

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();
            // TODO: This sub/xor needs to be checked, it's more than likely wrong
            writer.WriteMagic((uint)questdiffs.Length, 0x1DB0, 0xC5);
            foreach (QuestDifficulty d in questdiffs)
            {
                writer.WriteStruct(d);
            }
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0xB, 0x1A, 0x4);
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi, Pack = 1)]
        public unsafe struct QuestDifficulty
        {
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 32 - 8)]
            public string dateOrSomething;
            public int field_24;
            public int field_28;
            public int something;
            public int field_30;
            public int something2;
            public int something3;
            public int something4;
            public int something5;
            public QuestDifficultyEntry difficulty1;
            public QuestDifficultyEntry difficulty2;
            public QuestDifficultyEntry difficulty3;
            public QuestDifficultyEntry difficulty4;
            public QuestDifficultyEntry difficulty5;
            public QuestDifficultyEntry difficulty6;
            public QuestDifficultyEntry difficulty7;
            public QuestDifficultyEntry difficulty8;
        }

        public struct QuestDifficultyEntry
        {
            public uint field_0;
            public uint field_4;
            public uint field_8;
            public uint field_C;
            public uint field_10;
            public uint field_14;
            public uint field_18;
            public uint field_1C;
        }
    }
}
