using System;

using PolarisServer.Models;

namespace PolarisServer.Packets
{
    public class CharacterSpawnPacket : Packet
    {
        private Character character = null;
        public MysteryPositions Position;
        public bool IsItMe = true;

        public CharacterSpawnPacket(Character character)
        {
            this.character = character;

            Position.a = 0.000031f;
            Position.b = 1.0f;
            Position.c = 0.000031f;
            Position.facingAngle = -0.000031f;

            Position.x = -0.417969f;
            Position.y = 0.000031f;
            Position.z = 134.375f;
        }

        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();

            // Player header
            writer.WritePlayerHeader((uint)character.Player.PlayerID);

            // Spawn position
            writer.Write(Position);

            writer.Write((ushort)0); // padding?
            writer.WriteFixedLengthASCII("Character", 32);
            writer.Write((ushort)1); // 0x44
            writer.Write((ushort)0); // 0x46
            writer.Write((uint)602); // 0x48
            writer.Write((uint)1); // 0x4C
            writer.Write((uint)53); // 0x50
            writer.Write((uint)0); // 0x54
            writer.Write((uint)(IsItMe ? 47 : 39)); // 0x58
            writer.Write((ushort)559); // 0x5C
            writer.Write((ushort)306); // 0x5E
            writer.Write((uint)character.Player.PlayerID); // player ID copy
            writer.Write((uint)0); // "char array ugggghhhhh" according to PolarisLegacy
            writer.Write((uint)0); // "voiceParam_unknown4"
            writer.Write((uint)0); // "voiceParam_unknown8"
            writer.WriteFixedLengthUTF16(character.Name, 16);
            writer.Write((uint)0); // 0x90
            writer.WriteStruct(character.Looks);
            writer.WriteStruct(character.Jobs);
            writer.WriteFixedLengthUTF16("", 32); // title?
            writer.Write((uint)0); // 0x204
            writer.Write((uint)0); // gmflag?
            writer.WriteFixedLengthUTF16(character.Player.Nickname, 16); // nickname, maybe not 16 chars?
            for (int i = 0; i < 64; i++)
                writer.Write((byte)0);

            return writer.ToArray();
        }
        
        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x08, 0x04);
        }

        #endregion
    }
}


