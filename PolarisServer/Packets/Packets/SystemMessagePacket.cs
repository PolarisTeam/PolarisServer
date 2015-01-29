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

        private readonly string _message;
        private readonly MessageType _type;

        public SystemMessagePacket(string message, MessageType type)
        {
            this._message = message;
            this._type = type;
        }

        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            var writer = new PacketWriter();
            writer.WriteUtf16(_message, 0x78F7, 0xA2);
            writer.Write((UInt32) _type);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader
            {
                Type = 0x19,
                Subtype = 0x01,
                Flags1 = 0x04
            };
        }

        #endregion
    }
}