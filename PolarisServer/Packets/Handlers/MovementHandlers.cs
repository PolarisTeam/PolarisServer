using System;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x04, 0x07)]
    public class MovementHandler : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            byte[] bytes = BitConverter.GetBytes((uint)context.User.PlayerID);

            data[0] = bytes[0];
            data[1] = bytes[1];
            data[2] = bytes[2];
            data[3] = bytes[3];

            foreach (Client c in Server.Instance.Clients)
            {
                if (c == context)
                    continue;

                if (c.Character == null)
                    continue;

                c.SendPacket(0x04, 0x71, 0x40, data);
            }
        }

        #endregion
    }

    [PacketHandlerAttr(0x04, 0x71)]
    public class MovementEndHandler : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            byte[] bytes = BitConverter.GetBytes((uint)context.User.PlayerID);

            data[0] = bytes[0];
            data[1] = bytes[1];
            data[2] = bytes[2];
            data[3] = bytes[3];

            foreach (Client c in Server.Instance.Clients)
            {
                if (c == context)
                    continue;

                if (c.Character == null)
                    continue;

                c.SendPacket(0x04, 0x71, 0x40, data);
            }
        }

        #endregion
    }

    public class MovementEventHandler : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            byte[] bytes = BitConverter.GetBytes((uint)context.User.PlayerID);

            data[0] = bytes[0];
            data[1] = bytes[1];
            data[2] = bytes[2];
            data[3] = bytes[3];

            foreach (Client c in Server.Instance.Clients)
            {
                if (c.Character == null)
                    continue;

                c.SendPacket(0x04, 0x80, 0x44, data);
            }
        }

        #endregion
    }

}
