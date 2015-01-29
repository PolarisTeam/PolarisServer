using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    public class NoPayloadPacket : Packet
    {
        private readonly byte _subtype;
        private readonly byte _type;

        public NoPayloadPacket(byte type, byte subtype)
        {
            _type = type;
            _subtype = subtype;
        }

        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            return new byte[0];
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader
            {
                Type = _type,
                Subtype = _subtype
            };
        }

        #endregion
    }
}