using System;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x7, 0x0)]
    public class ChatHandler : PacketHandler
    {

        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context.Character == null)
                return;
                
            PacketReader reader = new PacketReader(data, position, size);
            reader.BaseStream.Seek(0xC, System.IO.SeekOrigin.Begin);
            UInt32 channel = reader.ReadUInt32();
            string message = reader.ReadUTF16(0x9d3f, 0x44);

            Logger.Write("[CHT] <{0}> <{1}>", context.Character.Name, message);

            PacketWriter writer = new PacketWriter();
            writer.WritePlayerHeader((uint)context.Character.CharacterID);
            writer.Write((uint)channel);
            writer.WriteUTF16(message, 0x9d3f, 0x44);

            data = writer.ToArray();

            foreach (Client c in Server.Instance.Clients)
            {
                if (c.Character == null)
                    continue;

                c.SendPacket(0x7, 0x0, 0x44, data);
            }
        }

        #endregion
    }
}

