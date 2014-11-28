using System;
using PolarisServer.Packets;
using PolarisServer.Models;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x2)]
    public class CharacterListRequest : PacketHandler
    {
        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (size > 0)
                throw new NotImplementedException();

            //TODO handle this better
            PacketWriter writer = new PacketWriter();
            CharacterListPacket cList = new CharacterListPacket();
            cList.numberOfCharacters = 0;
            writer.WriteStruct(cList);

            context.SendPacket(0x11, 0x3, 0x0, writer.ToArray());
        }
    }

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
}

