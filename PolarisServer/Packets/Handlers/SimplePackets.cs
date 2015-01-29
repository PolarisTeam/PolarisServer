using System;
using System.IO;

// This file is to hold all packet handlers that require no logic to respond to, or require less than 5 lines of logic.

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x03, 0x0C)]
    public class PingResponse : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            Logger.Write("[HI!] Recieved ping response from " + context.User.Username);
        }

        #endregion
    }

    [PacketHandlerAttr(0x11, 0x06)]
    public class DeleteCharacter : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            var reader = new PacketReader(data);
            var ID = reader.ReadInt32();

            Logger.Write("[CHR] {0} is deleting character with ID {1}", context.User.Username, ID);

            // Delete Character
            foreach (var character in PolarisApp.Instance.Database.Characters)
                if (character.CharacterID == ID)
                {
                    PolarisApp.Instance.Database.Characters.Remove(character);
                    PolarisApp.Instance.Database.ChangeTracker.DetectChanges();
                    break;
                }

            // Detect the deletion and save the Database
            if (PolarisApp.Instance.Database.ChangeTracker.HasChanges())
                PolarisApp.Instance.Database.SaveChanges();

            // Disconnect for now
            // TODO: What do we do after a deletion?
            context.Socket.Close();
        }

        #endregion
    }

    [PacketHandlerAttr(0x11, 0x0D)]
    public class PingTimestampResponse : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            var reader = new PacketReader(data, position, size);
            var clientTime = reader.ReadUInt64();

            var writer = new PacketWriter();
            writer.Write(clientTime);
            writer.Write(Helper.Timestamp(DateTime.UtcNow));
            context.SendPacket(0x11, 0xE, 0, writer.ToArray());
        }

        #endregion
    }

    [PacketHandlerAttr(0x11, 0x1D)]
    public class GuildInfoRequest : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            var reader = new PacketReader(data);
            reader.BaseStream.Seek(0xC, SeekOrigin.Begin);
            var ID = reader.ReadUInt32();

            foreach (var client in PolarisApp.Instance.server.Clients)
            {
                if (client.Character.CharacterID == ID)
                {
                    var infoPacket = new GuildInfoPacket(context.Character);
                    context.SendPacket(infoPacket);
                    Logger.Write("[NFO] Sent guild info to " + client.Character.CharacterID);
                    break;
                }
            }
        }

        #endregion
    }

    [PacketHandlerAttr(0x11, 0x2B)]
    public class LogOutRequest : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            context.Socket.Close();
        }

        #endregion
    }

    [PacketHandlerAttr(0x11, 0x41)]
    public class CreateCharacterOne : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            var writer = new PacketWriter();
            writer.Write((uint) 0);
            writer.Write((uint) 0);
            writer.Write((uint) 0);
            writer.Write((uint) 0);

            context.SendPacket(0x11, 0x42, 0x0, writer.ToArray());
        }

        #endregion
    }

    [PacketHandlerAttr(0x11, 0x54)]
    public class CreateCharacterTwo : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            var writer = new PacketWriter();
            writer.Write((uint) 0);

            context.SendPacket(0x11, 0x55, 0x0, writer.ToArray());
        }

        #endregion
    }
}