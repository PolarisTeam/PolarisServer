using System;
using System.IO;

using PolarisServer.Models;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x05)]
    public class CharacterCreate : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context.User == null)
                return;

            PacketReader reader = new PacketReader(data, position, size);

            reader.ReadBytes(12);   // 12 unknown bytes
            reader.ReadByte();      // VoiceType
            reader.ReadBytes(5);    // 5 unknown bytes
            reader.ReadUInt16();    // VoiceData
            string name = reader.ReadFixedLengthUTF16(16);

            reader.BaseStream.Seek(0x4, SeekOrigin.Current); // Padding
            Character.LooksParam looks = reader.ReadStruct<Character.LooksParam>();
            Character.JobParam jobs = reader.ReadStruct<Character.JobParam>();

            Logger.WriteInternal("[CHR] {0} is creating a new character named {1}.", context.User.Username, name);
            var newCharacter = new Character
            {
                Name = name,
                Jobs = jobs,
                Looks = looks,
                Player = context.User,
            };

            // Add to database
            PolarisApp.Instance.Database.Characters.Add(newCharacter);
            PolarisApp.Instance.Database.SaveChanges();

            // Assign character to player
            context.Character = newCharacter;
            
            // Set Player ID
            PacketWriter writer = new PacketWriter();
            writer.Write(0);
            writer.Write((uint)context.User.PlayerID);
            context.SendPacket(0x11, 0x07, 0, writer.ToArray());
            
            // Spawn
            context.SendPacket(new NoPayloadPacket(0x11, 0x3E));
        }

        #endregion
    }
}

