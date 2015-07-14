using PolarisServer.Models;
using PolarisServer.Object;
using PolarisServer.Packets.PSOPackets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x3, 0x35)]
    class CasinoTeleportHandler : PacketHandler
    {
        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context.User == null)
                return;

            // Dunno what these are yet.
            context.SendPacket(0x11, 0xA, 0x0, BitConverter.GetBytes(context.User.PlayerId));
            context.SendPacket(0x1E, 0xC, 0x0, BitConverter.GetBytes(101));

            // Has a bunch of data, Likely a Set Area packet variation!
            PacketWriter writer = new PacketWriter();
            writer.Write(1); // ???
            writer.Write(0); // ???
            writer.Write(5);
            writer.WriteStruct(new EntityHeader((ulong)context.User.PlayerId, EntityType.Player));
            writer.Write(new byte[8]); // 8 Zeros

            //writer.Write((uint)4294967284); // F4 FF FF FF
            //writer.Write(104);
            //writer.Write(3);
            //writer.Write((uint)434540417);
            //writer.Write(new byte[16]);
            //writer.Write((ulong)18446744073709551615); // FF FF FF FF FF FF FF FF
            //writer.Write(0);

            context.CurrentZone = "casino";

            context.SendPacket(0x3, 0x0, 0x0, writer.ToArray());


            // No payload
            context.SendPacket(new NoPayloadPacket(0x3, 0x2a));

            var setPlayerId = new PacketWriter();
            setPlayerId.WritePlayerHeader((uint)context.User.PlayerId);
            context.SendPacket(0x06, 0x00, 0, setPlayerId.ToArray());

            context.SendPacket(new CharacterSpawnPacket(context.Character, new PSOLocation(0, 1f, 0, 0, 2, 6, 102)));

            var spawnPacket = new CharacterSpawnPacket(context.Character, new PSOLocation(0, 1f, 0, 0, 2, 6, 102)) { IsItMe = false };
            foreach (var c in Server.Instance.Clients.Where(c => c != context).Where(c => c.Character != null).Where(c => c.CurrentZone == "casino"))
            {
                c.SendPacket(spawnPacket);

                var remoteChar = new CharacterSpawnPacket(c.Character, c.CurrentLocation) { IsItMe = false };
                context.SendPacket(remoteChar);
            }

            var objects = ObjectManager.Instance.getObjectsForZone("casino").Values;
            foreach (var obj in objects)
            {
                context.SendPacket(0x8, 0xB, 0x0, obj.GenerateSpawnBlob());
            }

            

            context.SendPacket(new NoPayloadPacket(0x03, 0x2B));

        }
    }
}
