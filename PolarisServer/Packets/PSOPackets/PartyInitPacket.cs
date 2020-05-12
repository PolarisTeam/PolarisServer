using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    class PartyInitPacket : Packet
    {
        private Character[] members;
        public PartyInitPacket(Character[] members)
        {
            this.members = members;
        }

        public override byte[] Build()
        {
            if (members.Length < 1)
                return new byte[0];

            // xor: 0xD863, sub: 0xA9
            PacketWriter writer = new PacketWriter();
            writer.WriteBytes(0, 12); // Unknown 12 bytes, not obj header
            writer.WriteStruct(new ObjectHeader((uint)members[0].Player.PlayerId, EntityType.Player)); // Player receiving the thing
            writer.WriteStruct(members.Length); // Likely partymembercount

            for(int i = 0; i < members.Length; i++)
            {
                writer.WriteStruct(new ObjectHeader((uint)members[i].Player.PlayerId, EntityType.Player)); // Header of player
                writer.WriteUtf16(members[i].Name, 0xD863, 0xA9);
                writer.WriteUtf16(members[i].Player.Nickname, 0xD863, 0xA9);
                writer.Write((byte)members[i].Jobs.entries.hunter.level); // Active class level
                writer.Write((byte)0); // idk
                writer.Write((byte)4); // idk
                writer.Write((byte)0xFF); // idk
                writer.Write((byte)0); // idk
                writer.Write((byte)0); // idk
                writer.Write((byte)0); // idk
                writer.Write((byte)0xFF); // idk
                writer.Write((byte)0xFF); // idk
                writer.Write((byte)0); // idk
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.WriteBytes(0, 12);
                writer.Write(0);
                writer.Write(0);
            }

            for(int i = 0; i < 4 - members.Length; i++) // Empty entries
            {
                writer.WriteStruct(new ObjectHeader(0, 0)); // Header of player
                writer.WriteMagic(0, 0xD863, 0xA9);
                writer.WriteMagic(0, 0xD863, 0xA9);
                writer.Write((byte)0); // Active class level
                writer.Write((byte)0); // idk
                writer.Write((byte)0); // idk
                writer.Write((byte)0); // idk
                writer.Write((byte)0); // idk
                writer.Write((byte)0); // idk
                writer.Write((byte)0); // idk
                writer.Write((byte)0); // idk
                writer.Write((byte)0); // idk
                writer.Write((byte)0); // idk
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.Write((ushort)0);
                writer.WriteBytes(0, 12);
                writer.Write(0);
            }

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0xE, 0x2, PacketFlags.PACKED);
        }
    }
}
