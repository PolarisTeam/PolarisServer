using PolarisServer.Database;
using System.Linq;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x02)]
    public class RequestCharacterList : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            if (context.User == null)
                return;

            var writer = new PacketWriter();

            using (var db = new PolarisEf())
            {
                var chars = from c in db.Characters
                            where c.Player.PlayerId == context.User.PlayerId
                            select c;


                writer.Write((uint)chars.Count());

                foreach (var ch in chars)
                {
                    writer.Write((uint)ch.CharacterId);
                    writer.Write((uint)context.User.PlayerId);
                    for (var i = 0; i < 0xC; i++)
                        writer.Write((byte)0);

                    writer.WriteFixedLengthUtf16(ch.Name, 16);
                    writer.Write((uint)0);

                    writer.WriteStruct(ch.Looks);
                    writer.WriteStruct(ch.Jobs);
                    for (var i = 0; i < 0x44; i++)
                        writer.Write((byte)0);
                } 
            }

            // Ninji note: This packet may be followed by extra data,
            // after a fixed-length array of character data structures.
            // Needs more investigation at some point.
            // --- 
            // CK note: Extra data is likely current equipment, playtime, etc.
            // All of that data is currently unaccounted for at the moment.

            context.SendPacket(0x11, 0x03, 0, writer.ToArray());
        }

        #endregion
    }
}