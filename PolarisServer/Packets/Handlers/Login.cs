using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using PolarisServer.Database;
using PolarisServer.Packets.PSOPackets;
using PolarisServer.Models;

namespace PolarisServer.Packets.Handlers
{
    [PacketHandlerAttr(0x11, 0x0)]
    public class Login : PacketHandler
    {
        #region implemented abstract members of PacketHandler

        public override void HandlePacket(Client context, byte flags, byte[] data, uint position, uint size)
        {
            var reader = new PacketReader(data, position, size);
            reader.BaseStream.Seek(0x2E0, SeekOrigin.Current);
            var username = reader.ReadFixedLengthAscii(0x60);
            var password = reader.ReadFixedLengthAscii(0x60);

            // What am I doing here even
            using (var db = new PolarisEf())
            {
                var users = from u in db.Players
                            where u.Username.ToLower().Equals(username.ToLower())
                            select u;


                var error = "";
                Player user;

                if (!users.Any())
                {
                    // Check if there is an empty field
                    if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                    {
                        error = "Username and password fields must not be empty.";
                        user = null;
                    }
                    // Check for special characters
                    else if (!Regex.IsMatch(username, "^[a-zA-Z0-9 ]*$", RegexOptions.IgnoreCase))
                    {
                        error = "Username must not contain special characters.\nPlease use letters and numbers only.";
                        user = null;
                    }
                    else // We're all good!
                    {
                        // Insert new player into database
                        user = new Player
                        {
                            Username = username.ToLower(),
                            Password = BCrypt.Net.BCrypt.HashPassword(password),
                            Nickname = username.ToLower(),
                            // Since we can't display the nickname prompt yet, just default it to the username
                            SettingsIni = File.ReadAllText("Resources/settings.txt")
                        };
                        db.Players.Add(user);
                        db.SaveChanges();

                        // context.SendPacket(0x11, 0x1e, 0x0, new byte[0x44]); // Request nickname
                    }
                }
                else
                {
                    user = users.First();
                    if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
                    {
                        error = "Incorrect password.";
                        user = null;
                    }
                }

                /* Mystery packet
                var mystery = new PacketWriter();
                mystery.Write((uint)100);
                SendPacket(0x11, 0x49, 0, mystery.ToArray()); */

                // Settings packet
                var settings = new PacketWriter();
                settings.WriteAscii(user.SettingsIni, 0x54AF, 0x100);
                context.SendPacket(0x2B, 2, 4, settings.ToArray());

                if (user == null)
                    return;

                context.User = user;

                // Login response packet
                context.SendPacket(new LoginDataPacket("Polaris Block 1", error, (user == null) ? (uint)0 : (uint)user.PlayerId));

            }

            if (PolarisApp.Config.motd != "")
            {
                context.SendPacket(new SystemMessagePacket(PolarisApp.Config.motd, SystemMessagePacket.MessageType.AdminMessageInstant));
            }

        }

        #endregion
    }
}