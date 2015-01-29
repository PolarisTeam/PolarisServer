using PolarisServer.Models;

namespace PolarisServer.Packets
{
    public class NoPayloadPacket : Packet
    {
        private readonly byte _subtype;
        private readonly byte _type;

        public NoPayloadPacket(byte type, byte subtype)
        {
            this._type = type;
            this._subtype = subtype;
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