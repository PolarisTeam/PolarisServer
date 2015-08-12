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
            ポカポカDebug,
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
            UltimateDebug,
            NotSet
        }

        [Flags]
        public enum QuestTypeAvailable : UInt64
        {
            None = 0x0000000000000000,
            All = 0xFFFFFFFFFFFFFFFF,
            Extreme = 0x0000000000000002,
            StoryEP1 = 0x0000000000000004,
            Arks = 0x0000000000000008,
            Limited = 0x0000000000000010,
            ExtremeDebug = 0x0000000000000020,
            Blank1 = 0x0000000000000040,
            StoryEP2 = 0x0000000000000080,
            NetCafeLimited = 0x0000000000000100,
            ポカポカDebug = 0x0000000000000200,
            Blank2 = 0x0000000000000400,
            Advance = 0x0000000000000800,
            FreeField = 0x0000000000001000,
            FreeDebug = 0x0000000000002000,
            ArksDebug1 = 0x0000000000004000,
            StoryDebug = 0x0000000000008000,
            Challange = 0x0000010000010000,
            Emergency = 0x0000020000020000,
            EmergencyDebug = 0x0000040000040000,
            TimeAttack = 0x0000080000080000,
            TimeDebug = 0x0000000000100000,
            ArksDebug2 = 0x0000000000200000,
            ArksDebug3 = 0x0000000000400000,
            ArksDebug4 = 0x0000000000800000,
            ArksDebug5 = 0x0000000001000000,
            ArksDebug6 = 0x0000000002000000,
            ArksDebug7 = 0x0000000004000000,
            ArksDebug8 = 0x0000000008000000,
            ArksDebug9 = 0x0000000010000000,
            ArksDebug10 = 0x0000000020000000,
            Blank3 = 0x0000000040000000,
            StoryEP3 = 0x0000000080000000,
            Featured = 0x0000000100000000,
            Ultimate = 0x0000000200000000,
            UltimateDebug = 0x0000000400000000,
            NotSet = 0x0000000800000000,
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
