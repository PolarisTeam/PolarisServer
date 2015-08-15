using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    class SetQuestPacket : Packet
    {
        QuestListPacket.QuestDefiniton questdef;
        Database.Player p;

        public SetQuestPacket(QuestListPacket.QuestDefiniton questdef, Database.Player p)
        {
            this.questdef = questdef;
            this.p = p;
        }
        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();
            writer.Write(questdef.questNameString);
            writer.Write(0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)1);
            writer.WriteStruct(new ObjectHeader((uint)p.PlayerId, EntityType.Player));
            writer.Write(0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write((ushort)0);
            writer.Write(0);
            writer.Write(0);
            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0xE, 0x25);
        }
    }
}
