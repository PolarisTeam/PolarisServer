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

            reader.BaseStream.Seek(0x2C, SeekOrigin.Current);

            var macCount = reader.ReadMagic(0x5E6, 107);
            reader.BaseStream.Seek(0x1C * macCount, SeekOrigin.Current);

            reader.BaseStream.Seek(0x114, SeekOrigin.Current);

            var username = reader.ReadFixedLengthAscii(64);
            var password = reader.ReadFixedLengthAscii(64);


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

                // Mystery packet
                var mystery = new PacketWriter();
                mystery.Write((uint)100);
                // SendPacket(0x11, 0x49, 0, mystery.ToArray());

                // Login response packet
                var resp = new PacketWriter();
                resp.Write((uint)((user == null) ? 1 : 0)); // Status flag: 0=success, 1=error
                resp.WriteUtf16(error, 0x8BA4, 0xB6);

                if (user == null)
                {
                    for (var i = 0; i < 0xEC; i++)
                        resp.Write((byte)0);
                    context.SendPacket(0x11, 1, 4, resp.ToArray());
                    return;
                }

                // TODO: Explore this data! Some if it seems really important. (May contain level cap setting + more)

                resp.WriteStruct(new ObjectHeader((uint)user.PlayerId, EntityType.Player));
                resp.WriteFixedLengthUtf16("B001-Polaris", 0x20); // This is right
                // Set things to default values; Dunno these purposes yet.
                resp.Write(0x42700000); //0
                resp.Write(7);          //4
                resp.Write(0xA);        //8
                resp.Write(1);          //C
                resp.Write(0x41200000); //10
                resp.Write(0x40A00000); //14
                resp.Write(11);         //18
                resp.Write(0x3F800000); //1C (1 as a float)
                resp.Write(0x42960000); //20
                resp.Write(40);         //24
                resp.Write(0x41200000); //28
                resp.Write(1);          //2C?
                resp.Write(1120403456); //30

                //WHAT
                for(int i = 0; i < 10; i++)
                {
                    resp.Write(1065353216);
                }
                //ARE
                for(int i = 0; i < 21; i++)
                {
                    resp.Write(1120403456); 
                }
                //THESE?
                resp.Write(0x91A2B);    //B0
                resp.Write(0x91A2B);    //B4

                resp.Write(15);
                resp.Write(5);
                resp.Write(0);

                context.SendPacket(0x11, 1, 4, resp.ToArray());

                // Settings packet
                var settings = new PacketWriter();
                settings.WriteAscii(user.SettingsIni, 0x54AF, 0x100);
                context.SendPacket(0x2B, 2, 4, settings.ToArray());

                context.User = user;

            }

            if (PolarisApp.Config.motd != "")
            {
                context.SendPacket(new SystemMessagePacket(PolarisApp.Config.motd, SystemMessagePacket.MessageType.AdminMessageInstant));
            }

        }

        #endregion
    }
}