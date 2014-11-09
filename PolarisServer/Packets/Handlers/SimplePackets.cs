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
}

