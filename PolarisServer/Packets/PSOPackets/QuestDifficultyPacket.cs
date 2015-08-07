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
                difficulty.unknown1 = 0x0101;
                difficulty.monster1 = 0xFFFFFFFF;
                difficulty.monster2 = 0xFFFFFFFF;
                difficulty.monster3 = 0xFFFFFFFF;

                questdiffs[i].difficulty1 = difficulty;
                questdiffs[i].difficulty2 = difficulty;
                questdiffs[i].difficulty3 = difficulty;
                questdiffs[i].difficulty4 = difficulty;
                questdiffs[i].difficulty5 = difficulty;
                questdiffs[i].difficulty6 = difficulty;
                questdiffs[i].difficulty7 = difficulty;
                questdiffs[i].difficulty8 = difficulty;
            }

            this.questdiffs = questdiffs;
        }

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();
            // TODO: This sub/xor needs to be checked, it's more than likely wrong
            // writer.WriteMagic((uint)questdiffs.Length, 0x1DB0, 0xC5);
            writer.Write((int)0x294D); // Just directly write the sub/xor for now
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
            public int questNameString;
            public int something3;
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
            public uint unknown1;
            public uint unknown2;
            public uint monster1;
            public uint monster1flags;
            public uint monster2;
            public uint monster2flags;
            public uint monster3;
            public uint monster3flags;
        }
    }
}
