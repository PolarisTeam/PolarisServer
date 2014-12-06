using System;

using PolarisServer.Models;

namespace PolarisServer.Packets
{
    public class SetMesetaPacket : Packet
    {
        public Int64 newAmount = 0;
        
        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            PacketWriter writer = new PacketWriter();

            writer.Write(newAmount);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader
            {
                type = 0x0F,
                subtype = 0x14,
                flags1 = 0
            };
        }
        
        #endregion
    }
}
