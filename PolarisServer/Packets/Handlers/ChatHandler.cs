using System;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x07, 0x00)]
    public class ChatHandler : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte[] data, uint position, uint size)
        {
            if (context.Character == null)
                return;
            
            PacketReader reader = new PacketReader(data, position, size);
            reader.BaseStream.Seek(0xC, System.IO.SeekOrigin.Begin);
            UInt32 channel = reader.ReadUInt32();
            string message = reader.ReadUTF16(0x9D3F, 0x44);

            if (message.StartsWith(PolarisApp.Config.CommandPrefix))
            {
                bool valid = false;

                // Iterate commands
                foreach (ConsoleCommand command in PolarisApp.ConsoleSystem.commands)
                {
                    string full = message.Substring(1); // Strip the command chars
                    string[] args = full.Split(' ');

                    foreach (string name in command.Names)
                        if (args[0].ToLower() == name.ToLower())
                        {
                            command.Run(args, args.Length, full, context);
                            valid = true;
                            Logger.WriteCommand(null, "[CMD] {0} issued command {1}", context.User.Username, full);
                            break;
                        }

                    if (valid)
                        break;
                }
                
                if (!valid)
                    Logger.WriteClient(context, "[CMD] {0} - Command not found", message.Split(' ')[0].Trim('\r'));
            }
            else
            {
                Logger.Write("[CHT] <{0}> <{1}>", context.Character.Name, message);

                PacketWriter writer = new PacketWriter();
                writer.WritePlayerHeader((uint)context.User.PlayerID);
                writer.Write((uint)channel);
                writer.WriteUTF16(message, 0x9D3F, 0x44);

                data = writer.ToArray();

                foreach (Client c in Server.Instance.Clients)
                {
                    if (c.Character == null)
                        continue;

                    c.SendPacket(0x07, 0x00, 0x44, data);
                }
            }
        }

        #endregion
    }
}

