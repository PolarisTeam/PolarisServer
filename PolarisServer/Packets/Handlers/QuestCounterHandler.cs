using System;

using PolarisServer.Packets.PSOPackets;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0xB, 0x30)]
    class QuestCounterHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            // Not sure what this does yet
            byte[] allTheQuests = new byte[408];

            for (int i = 0; i < allTheQuests.Length; i++)
                allTheQuests[i] = 0xFF;

            context.SendPacket(0xB, 0x22, 0x0, allTheQuests);
        }
    }

    [PacketHandlerAttr(0xB, 0x15)]
    class QuestCounterAvailableHander : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            PacketWriter writer = new PacketWriter();

            context.SendPacket(new QuestAvailablePacket());
        }
    }

    [PacketHandlerAttr(0xB, 0x17)]
    class QuestListRequestHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            // What am I doing
            QuestListPacket.QuestDefiniton[] defs = new PSOPackets.QuestListPacket.QuestDefiniton[1];
            for (int i = 0; i < defs.Length; i++)
            {
                defs[i].dateOrSomething = "2012/01/05";
                defs[i].needsToBeNonzero = 0x00000020;
                defs[i].getsSetToWord = 0x0000000B;
                defs[i].questNameString = 30010;
                defs[i].playTime = (byte)QuestListPacket.EstimatedTime.Short;
                defs[i].partyType = (byte)QuestListPacket.PartyType.SinglePartyQuest;
                defs[i].difficulties = (byte)QuestListPacket.Difficulties.Normal | (byte)QuestListPacket.Difficulties.hard | (byte)QuestListPacket.Difficulties.VeryHard | (byte)QuestListPacket.Difficulties.SuperHard;
                defs[i].requiredLevel = 1;
                // Not sure why but these need to be set for the quest to be enabled
                defs[i].field_FF = 0xF1;
                defs[i].field_101 = 1;
            }

            context.SendPacket(new QuestListPacket(defs));
            context.SendPacket(new NoPayloadPacket(0xB, 0x1B));
        }
    }

    [PacketHandlerAttr(0xB, 0x19)]
    class QuestDifficultyRequestHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            QuestDifficultyPacket.QuestDifficulty[] diffs = new QuestDifficultyPacket.QuestDifficulty[1];
            for (int i = 0; i < diffs.Length; i++)
            {
                diffs[i].dateOrSomething = "2012/01/05";
                diffs[i].something = 0x20;
                diffs[i].something2 = 0x0B;
                diffs[i].questNameString = 30010;

                // These are likely bitfields
                diffs[i].something3 = 0x00030301;
            }

            context.SendPacket(new QuestDifficultyPacket(diffs));

            // [K873] I believe this is the correct packet, but it causes an infinite send/recieve loop, we're probably just missing something else
            context.SendPacket(new NoPayloadPacket(0xB, 0x1C));
        }
    }

    [PacketHandlerAttr(0xE, 0xC)]
    class QuestDifficultyStartHandler : PacketHandler
    {
        // Go go maximum code duplication (for now)
        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            QuestListPacket.QuestDefiniton def = new QuestListPacket.QuestDefiniton();
            def.dateOrSomething = "2012/01/05";
            def.needsToBeNonzero = 0x00000020;
            def.getsSetToWord = 0x0000000B;
            def.questNameString = 30010;
            def.playTime = (byte)QuestListPacket.EstimatedTime.Short;
            def.partyType = (byte)QuestListPacket.PartyType.SinglePartyQuest;
            def.difficulties = (byte)QuestListPacket.Difficulties.Normal | (byte)QuestListPacket.Difficulties.hard | (byte)QuestListPacket.Difficulties.VeryHard | (byte)QuestListPacket.Difficulties.SuperHard;
            def.requiredLevel = 1;
            // Not sure why but these need to be set for the quest to be enabled
            def.field_FF = 0xF1;
            def.field_101 = 1;

            QuestDifficultyPacket.QuestDifficulty diff = new QuestDifficultyPacket.QuestDifficulty();
            diff.dateOrSomething = "2012/01/05";
            diff.something = 0x20;
            diff.something2 = 0x0B;
            diff.questNameString = 30010;

            // These are likely bitfields
            diff.something3 = 0x00030301;

            context.SendPacket(new SetQuestPacket(def, context.User));
            context.SendPacket(new QuestStartPacket(def, diff));
        }
    }
}
