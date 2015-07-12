using PolarisServer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x4, 0x14)]
    class ObjectInteract : PacketHandler
    {
        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            PacketReader reader = new PacketReader(data);
            reader.ReadBytes(12); // Padding MAYBE???????????
            EntityHeader srcObject = reader.ReadStruct<EntityHeader>();
            byte[] someBytes = reader.ReadBytes(4); // Dunno what this is yet.
            EntityHeader dstObject = reader.ReadStruct<EntityHeader>(); // Could be wrong
            reader.ReadBytes(16); // Not sure what this is yet
            string command = reader.ReadAscii(0xD711, 0xCA);

            Logger.WriteInternal("[OBJ] Object ID {0} (Type {1}) wants to {2} onto Object ID {3} (Type {4}) !!!", srcObject.ID, srcObject.EntityType, command,
                dstObject.ID, dstObject.EntityType);
        }
    }
}
