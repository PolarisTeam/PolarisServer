using System;
using System.Linq;
using PolarisServer.Packets;
using PolarisServer.Models;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x2B)]
    public class LogOutRequest : PacketHandler
    {
        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            // DOUBLE SOCKET UP IN HERE
            context.Socket.Close();
        }
    }

    [PacketHandlerAttr(0x11, 0x41)]
    public class CreateCharacterOne : PacketHandler
    {
        public override void handlePacket(Client context, byte[] data, uint position, uint size)
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
        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            PacketWriter writer = new PacketWriter();
            writer.Write((uint)0);

            context.SendPacket(0x11, 0x55, 0x0, writer.ToArray());
        }
    }

    [PacketHandlerAttr(0x11, 2)]
    public class RequestCharacterList : PacketHandler
    {
        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context.User == null)
                return;

            var db = PolarisApp.Instance.Database;
            var chars = from c in db.Characters
                    where c.Player.PlayerID == context.User.PlayerID
                                 select c;

            PacketWriter writer = new PacketWriter();
            writer.Write((uint) chars.Count());

            foreach (var ch in chars)
            {
                writer.Write((uint)ch.CharacterID);
                writer.Write((uint)context.User.PlayerID);
                for (int i = 0; i < 0xC; i++)
                    writer.Write((byte)0);

                writer.WriteFixedLengthUTF16(ch.Name, 16);
                writer.Write((uint)0);

                writer.WriteStruct(ch.Looks);
                writer.WriteStruct(ch.Jobs);
                for (int i = 0; i < 0x44; i++)
                    writer.Write((byte)0);
            }

            // Ninji note: This packet may be followed by extra data,
            // after a fixed-length array of character data structures.
            // Needs more investigation at some point.

            context.SendPacket(0x11, 0x3, 0x0, writer.ToArray());
        }
    }
}

