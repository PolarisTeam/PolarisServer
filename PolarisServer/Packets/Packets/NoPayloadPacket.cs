using System;

namespace PolarisServer.Packets
{
    public class NoPayloadPacket : Packet
    {
        byte type, subtype;

        public NoPayloadPacket(byte type, byte subtype)
        {
        }

        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            return new byte[0];
        }

        public override PolarisServer.Models.PacketHeader GetHeader()
        {
            return new PolarisServer.Models.PacketHeader
            {
                type = type,
                subtype = subtype
            };
        }

        #endregion
    }
}

