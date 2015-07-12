using PolarisServer.Models;
using PolarisServer.Object;
using PolarisServer.Packets.PSOPackets;
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

            // TODO: Delete this code and do this COMPLETELY correctly!!!
            if (command == "Transfer")
            {
                // Send forwarded
                context.SendPacket(new ObjectActionPacket(dstObject, srcObject, new EntityHeader(), new EntityHeader(), "Forwarded"));
                // Send teleport
                PSOLocation dstLoc = new PSOLocation();
                dstLoc.FacingAngle = 0f;
                dstLoc.X = -8f;
                dstLoc.Y = 5f;
                dstLoc.Z = -143f;
                dstLoc.B = 1f;
                //dstLoc.A = 1f;
                //dstLoc.C = 1f;
                context.SendPacket(new TeleportTransferPacket(ObjectManager.Instance.getObjectByID("lobby", srcObject.ID), dstLoc));
            }
        }
    }
}
