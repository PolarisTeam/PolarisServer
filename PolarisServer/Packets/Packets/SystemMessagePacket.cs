using System;

using PolarisServer.Models;

namespace PolarisServer.Packets
{
    public class SystemMessagePacket : Packet
    {
        public enum MessageType
        {
            GoldenTicker = 0,
            AdminMessage,
            AdminMessageInstant,
            SystemMessage,
            GenericMessage
        }

        string message;
        MessageType type;

        public SystemMessagePacket(string message, MessageType type)
        {
            this.message = message;
            this.type = type;
        }

        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();
            writer.WriteUTF16(message, 0x78F7, 0xA2);
            writer.Write((UInt32)type);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader
            {
                type = 0x19,
                subtype = 0x01,
                flags1 = 0x04
            };
        }
        
        #endregion
    }
}

