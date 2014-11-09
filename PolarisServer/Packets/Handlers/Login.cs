using System;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x0)]
    public class Login : PacketHandler
    {
        public Login()
        {
        }

        public override void handlePacket(Client context, byte[] data, uint position, uint size)
        {
            // Mystery packet
            var mystery = new Packets.PacketWriter();
            mystery.Write((uint)100);
            //SendPacket (0x11, 0x49, 0, mystery.ToArray ());

            // Login response packet
            var resp = new Packets.PacketWriter();
            resp.Write((uint)0); // Status flag: 0=success, 1=error
            resp.WriteUTF16("This is an error", 0x8BA4, 0xB6);
            resp.Write((uint)200); // Player ID
            resp.Write((uint)0); // Unknown
            resp.Write((uint)0); // Unknown
            resp.WriteFixedLengthUTF16("B001-DarkFox", 0x20);
            for (int i = 0; i < 0xBC; i++)
                resp.Write((byte)0);
            context.SendPacket(0x11, 1, 4, resp.ToArray());

            // Settings packet
            var settings = new Packets.PacketWriter();
            settings.WriteASCII(
                System.IO.File.ReadAllText("settings.txt"),
                0x54AF, 0x100);
            context.SendPacket(0x2B, 2, 4, settings.ToArray());
        }
    }
}

