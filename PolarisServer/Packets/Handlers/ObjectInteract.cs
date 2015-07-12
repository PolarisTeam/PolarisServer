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

            PSOObject srcObj = ObjectManager.Instance.getObjectByID("lobby", srcObject.ID);

            Logger.WriteInternal("[OBJ] {0} (ID {1}) <{2}> --> Ent {3} (ID {4})", srcObj.Name, srcObj.Header.ID, command, (EntityType)dstObject.EntityType, dstObject.ID);

            // TODO: Delete this code and do this COMPLETELY correctly!!!
            if (command == "Transfer")
            {
                // Send forwarded
                context.SendPacket(new ObjectActionPacket(dstObject, srcObject, new EntityHeader(), new EntityHeader(), "Forwarded"));
                // Send teleport
                PSOLocation dstLoc = new PSOLocation();
                dstLoc.PosX = -8f;
                dstLoc.PosY = 50f;
                dstLoc.PosZ = -143f;
                //dstLoc.RotX = -0.7158228f;
                dstLoc.RotY = 1f;
                //dstLoc.RotZ = 0f;
                dstLoc.RotW = 0.698282f;
                context.SendPacket(new TeleportTransferPacket(ObjectManager.Instance.getObjectByID("lobby", srcObject.ID), dstLoc));
            }
        }
    }
}
