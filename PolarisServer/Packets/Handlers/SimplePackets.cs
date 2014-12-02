using System;
using System.IO;
using System.Linq;
using PolarisServer.Packets;
using PolarisServer.Models;

// This file is to hold all packet handlers that require no logic to respond to, or require less than 5 lines of logic.

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x6)]
    public class DeleteCharacter : PacketHandler
    {
        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            PacketReader reader = new PacketReader(data);
            int ID = reader.ReadInt32();

            Logger.Write("[---] Player is deleting character with ID " + ID);

            // Delete Character
            foreach (Character character in PolarisApp.Instance.Database.Characters)
                if (character.CharacterID == ID)
                {
                    PolarisApp.Instance.Database.Characters.Remove(character);

                    // TODO: Currently this throws System.Data.Entity.Core.EntityException when called, not sure why
                    // See: http://puu.sh/ddWKc/b8d91751a9.txt
                    // PolarisApp.Instance.Database.SaveChanges();

                    break;
                }

            // Disconnect for now
            context.Socket.Close();
        }
    }

    // [PacketHandlerAttr(0x11, 0xD)]  // It seems both of these are used for some form of pinging, needs investigation
    [PacketHandlerAttr(0x11, 0x68)]    // One of them might just be a timestamp update - Kyle
    public class PingResponse : PacketHandler
    {
        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            Logger.Write("[HI!] Recieved a ping from " + context.User.Username);
        }
    }

    [PacketHandlerAttr(0x11, 0x2B)]
    public class LogOutRequest : PacketHandler
    {
        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            // DOUBLE SOCKET UP IN HERE
            context.Socket.Close();
        }
    }

    [PacketHandlerAttr(0x11, 0x41)]
    public class CreateCharacterOne : PacketHandler
    {
        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((uint)0);
            writer.Write((uint)0);
            writer.Write((uint)0);
            writer.Write((uint)0);

            context.SendPacket(0x11, 0x42, 0x0, writer.ToArray());
        }
    }

    [PacketHandlerAttr(0x11, 0x54)]
    public class CreateCharacterTwo : PacketHandler
    {
        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((uint)0);

            context.SendPacket(0x11, 0x55, 0x0, writer.ToArray());
        }
    }

}

