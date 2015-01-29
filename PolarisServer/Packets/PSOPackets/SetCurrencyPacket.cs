using PolarisServer.Models;

namespace PolarisServer.Packets.PSOPackets
{
    public class SetCurrencyPacket : Packet
    {
        public int NewAcAmount = 0;
        public int NewFunAmount = 0;

        #region implemented abstract members of Packet

        public override byte[] Build()
        {
            var writer = new PacketWriter();

            // AC
            writer.Write(NewAcAmount);

            // Padding?
            for (var i = 0; i < 20; i++)
                writer.Write((byte) 0);

            // FUN
            writer.Write(NewFunAmount);

            // Padding?
            for (var i = 0; i < 4; i++)
                writer.Write((byte) 0);

            return writer.ToArray();
        }

        public override PacketHeader GetHeader()
        {
            return new PacketHeader(0x11, 0x1C);
        }

        #endregion
    }
}