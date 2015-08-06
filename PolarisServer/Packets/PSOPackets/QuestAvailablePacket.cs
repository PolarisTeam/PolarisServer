using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolarisServer.Models;
using System.Runtime.InteropServices;

namespace PolarisServer.Packets.PSOPackets
{
    class QuestAvailablePacket : Packet
    {
        public enum QuestType
        {
            Extreme,
            StoryEP1,
            Arks,
            Limited,
            ExtremeDebug,
            Blank1,
            StoryEP2,
            NetCafeLimited,
            MoonRunesDebug1,
            Blank2,
            Advance,
            FreeField,
            FreeDebug,
            ArksDebug1,
            StoryDebug,
            Challenge,
            Emergency,
            EmergencyDebug,
            TimeAttack,
            TimeDebug,
            ArksDebug2,
            ArksDebug3,
            ArksDebug4,
            ArksDebug5,
            ArksDebug6,
            ArksDebug7,
            ArksDebug8,
            ArksDebug9,
            ArksDebug10,
            Blank3,
            StoryEP3,
            Featured,
            Ultimate,
            MoonRunesDebug2,
            NotSet
        }

        [Flags]
        public enum QuestTypeAvailable : UInt64
        {
            None = 0x0000000000000000,
            All = 0xFFFFFFFFFFFFFFFF,
            Limited = 0x1000000000000000,
            ExtremeDebug = 0x2000000000000000,
            Blank1 = 0x4000000000000000,
            StoryEP2 = 0x8000000000000000,
            Extreme = 0x0200000000000000,
            StoryEP1 = 0x0400000000000000,
            Arks = 0x0800000000000000,
            FreeField = 0x0010000000000000,
            FreeDebug = 0x0020000000000000,
            ArksDebug1 = 0x0040000000000000,
            StoryDebug = 0x0080000000000000,
            NetCafeLimited = 0x0001000000000000,
            MoonRunesDebug1 = 0x0002000000000000,
            Blank2 = 0x0004000000000000,
            Advance = 0x0008000000000000,
            TimeDebug = 0x0000100000000000,
            ArksDebug2 = 0x0000200000000000,
            ArksDebug3 = 0x0000400000000000,
            ArksDebug4 = 0x0000800000000000,
            Challange = 0x0000010000000000,
            Emergency = 0x0000020000000000,
            EmergencyDebug = 0x0000040000000000,
            TimeAttack = 0x0000080000000000,
            ArksDebug9 = 0x0000001000000000,
            ArksDebug10 = 0x0000002000000000,
            Blank3 = 0x0000004000000000,
            StoryEP3 = 0x0000008000000000,
            ArksDebug5 = 0x0000000100000000,
            ArksDebug6 = 0x0000000200000000,
            ArksDebug7 = 0x0000000400000000,
            ArksDebug8 = 0x0000000800000000,
            Featured = 0x0000000001000000,
            Ultimate = 0x0000000002000000,
            MoonRunesDebug2 = 0x0000000004000000,
            NotSet = 0x0000000008000000,
        }

        public short[] amount = new short[Enum.GetValues(typeof(QuestType)).Length];
        QuestTypeAvailable available = QuestTypeAvailable.All;

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();

            // Filler/Padding?
            writer.Write((UInt16)0);

            // Amounts
            for (int i = 0; i < amount.Length; i++)
            {
                amount[i] = (short)(i + 1); // Just for testing
                writer.Write(amount[i]);
            }

            // Padding/Blank entries?
            for (int i = 0; i < 2; i++)
                writer.Write((int)0);

            // Available Bitfield
            writer.Write((UInt64)available);

            // Filler/Padding?
            for (int i = 0; i < 2; i++)
                writer.Write((int)0);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0xB, 0x16);
        }
    }
}
