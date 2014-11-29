using System;
using System.Text;
using PolarisServer.Models;

namespace PolarisServer.Packets.Handlers
{

    [PacketHandlerAttr(0x11, 0x5)]
    public class CharacterCreate : PacketHandler
    {
        public CharacterCreate()
        {
        }
            

        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context.User == null)
                return;

            PacketReader reader = new PacketReader(data, position, size);

            reader.ReadBytes(12);   // 12 unknown bytes
            reader.ReadByte();      // VoiceType
            reader.ReadBytes(5);    // 5 unknown bytes
            reader.ReadUInt16();    // VoiceData
            string name = Encoding.Unicode.GetString(reader.ReadBytes(32));

            reader.BaseStream.Seek(0x4, System.IO.SeekOrigin.Current); // Padding

            Character.LooksParam looks = reader.ReadStruct<Character.LooksParam>();
            Character.JobParam jobs = reader.ReadStruct<Character.JobParam>();

            Logger.WriteInternal("{0} is creating a new character named {1}.", context.User.Username, name);

            PolarisApp.Instance.Database.Characters.Add(new Character
                {
                    Name = name,
                    Jobs = jobs,
                    Looks = looks,
                    Player = context.User,
                });
        }
    }
}

