using System;
using System.Linq;
using PolarisServer.Packets;
using PolarisServer.Models;

// This file is to hold all packet handlers that require no logic to respond to, or require less than 5 lines of logic.

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
        
}

