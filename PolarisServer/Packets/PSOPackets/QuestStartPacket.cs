using System;
using System.Runtime.InteropServices;

using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    class QuestStartPacket : Packet
    {
        QuestListPacket.QuestDefiniton data;
        QuestDifficultyPacket.QuestDifficulty difficulty;

        public QuestStartPacket(QuestListPacket.QuestDefiniton data, QuestDifficultyPacket.QuestDifficulty difficulty)
        {
            this.data = data;
            this.difficulty = difficulty;
        }

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();

            writer.Write(0x753A); // Magic, needs proper sub/xor
            writer.Write((int)0);
            writer.WriteStruct<QuestListPacket.QuestDefiniton>(data);
            writer.WriteStruct<QuestDifficultyPacket.QuestDifficulty>(difficulty);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0xB, 0x31, 0x4);
        }
    }
}
