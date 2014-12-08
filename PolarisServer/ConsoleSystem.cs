using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;

using PolarisServer.Models;
using PolarisServer.Packets;
using PolarisServer.Packets.Handlers;

namespace PolarisServer
{
    public delegate void ConsoleCommandDelegate(string[] args, int length, string full, Client client);

    public class ConsoleCommandArgument
    {
        public string Name { get; set; }
        public bool Optional { get; set; }
        
        public ConsoleCommandArgument(string name, bool optional)
        {
            this.Name = name;
            this.Optional = optional;
        }
    }

    public class ConsoleCommand
    {
        public ConsoleCommandDelegate Command { get; set; }
        public List<string> Names { get; set; }
        public List<ConsoleCommandArgument> Arguments { get; set; }
        public string Help { get; set; }

        public ConsoleCommand(ConsoleCommandDelegate cmd, params string[] cmdNames)
        {
            if (cmdNames == null || cmdNames.Length < 1)
                throw new NotSupportedException();

            this.Command = cmd;
            this.Names = new List<string>(cmdNames);
            this.Arguments = new List<ConsoleCommandArgument>();
        }

        public bool Run(string[] args, int length, string full, Client client)
        {
            try
            {
                Command(args, length, full, client);
            }
            catch (IndexOutOfRangeException ex)
            {
                Logger.WriteException("Invalid command parameter", ex);
                return false;
            }
            catch (Exception ex)
            {
                Logger.WriteException("error in command", ex);
                return false;
            }

            return true;
        }
    }

    /// <summary>
    /// Handles commands and drawing/updating the console
    /// </summary>
    public class ConsoleSystem
    {
        public Thread thread;

        ConsoleKeyInfo key = new ConsoleKeyInfo();
        List<ConsoleKey> controlKeys = new List<ConsoleKey>()
        {
            ConsoleKey.Backspace,
            ConsoleKey.Enter,
            
            ConsoleKey.UpArrow,
            ConsoleKey.DownArrow,
            ConsoleKey.LeftArrow,
            ConsoleKey.RightArrow,

            ConsoleKey.Home,
            ConsoleKey.End,
            
            ConsoleKey.Delete
        };

        public List<ConsoleCommand> commands = new List<ConsoleCommand>();

        List<string> history = new List<string>();
        int historyIndex = 0;

        public int width = 80;
        public int height = 24;

        int commandIndex = 0;
        string commandLine = string.Empty;

        string info = string.Empty;
        string prompt = string.Empty;

        int prevCount = 0;

        public System.Timers.Timer timer;
        public bool refreshDraw = false;
        public bool refreshPrompt = false;

        public ConsoleSystem()
        {
            Console.Title = "Polaris";
            Console.CursorVisible = true;
            Console.SetWindowSize(width, height);

            timer = new System.Timers.Timer(1000);
            timer.Elapsed += TimerRefresh;
            timer.Start();

            CreateCommands();
        }

        private void TimerRefresh(object sender, System.Timers.ElapsedEventArgs e)
        {
            refreshPrompt = true;
        }

        public static void StartThread()
        {
            while (true)
            {
                try
                {
                    PolarisApp.ConsoleSystem.Draw();
                    PolarisApp.ConsoleSystem.CheckInput();
                }
                catch (ThreadInterruptedException ex)
                {
                    Logger.WriteException("Thread inturrupted", ex);
                }
                catch (ThreadStateException ex)
                {
                    Logger.WriteException("Thread state error", ex);
                }
                catch (Exception ex)
                {
                    Logger.WriteException(string.Empty, ex);
                }
            }
        }

        public void CreateCommands()
        {
            // Help
            ConsoleCommand help = new ConsoleCommand(Help, "help");
            help.Help = "Displays help for all commands";
            commands.Add(help);

            // Config
            ConsoleCommand config = new ConsoleCommand(Config, "config", "c");
            config.Arguments.Add(new ConsoleCommandArgument("Save | Load | List | Option Name", false));
            config.Arguments.Add(new ConsoleCommandArgument("Value", true));
            config.Help = "Set a configuration variable to the specified value";
            commands.Add(config);

            // Clear
            ConsoleCommand clearLog = new ConsoleCommand(ClearLog, "clear", "cls");
            clearLog.Help = "Clears the current log buffer";
            commands.Add(clearLog);

            // Echo
            ConsoleCommand echo = new ConsoleCommand(Echo, "echo");
            echo.Help = "Echo the given text back into the Console";
            echo.Arguments.Add(new ConsoleCommandArgument("text", false));
            commands.Add(echo);
            
            // Announce
            ConsoleCommand announce = new ConsoleCommand(Announce, "announce", "a");
            announce.Arguments.Add(new ConsoleCommandArgument("Message", false));
            announce.Help = "Sends a message to all connected players";
            commands.Add(announce);

            // ClearPlayers
            ConsoleCommand clearPlayers = new ConsoleCommand(ClearPlayers, "clearplayers", "cp");
            clearPlayers.Arguments.Add(new ConsoleCommandArgument("Exempt Username", true));
            clearPlayers.Help = "Disconnects all players from the server";
            commands.Add(clearPlayers);

            // SpawnClone
            ConsoleCommand spawnClone = new ConsoleCommand(SpawnClone, "spawnclone");
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Username", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Player Name", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("X", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Y", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Z", false));
            spawnClone.Help = "Spawns a clone of your character";
            commands.Add(spawnClone);

            // SendPacket
            ConsoleCommand sendPacket = new ConsoleCommand(SendPacket, "sendpacket", "sendp");
            sendPacket.Arguments.Add(new ConsoleCommandArgument("Name", false));
            sendPacket.Arguments.Add(new ConsoleCommandArgument("Type", false));
            sendPacket.Arguments.Add(new ConsoleCommandArgument("SubType", false));
            sendPacket.Arguments.Add(new ConsoleCommandArgument("Flags", false));
            sendPacket.Arguments.Add(new ConsoleCommandArgument("Data ...", false));
            sendPacket.Help = "Sends a packet to a client";
            commands.Add(sendPacket);

            // SendPacketFile
            ConsoleCommand sendPacketFile = new ConsoleCommand(SendPacketFile, "sendpacketfile", "sendpf");
            sendPacketFile.Arguments.Add(new ConsoleCommandArgument("Username", false));
            sendPacketFile.Arguments.Add(new ConsoleCommandArgument("Filename", false));
            sendPacketFile.Help = "Sends the specified file's contents as a packet";
            commands.Add(sendPacketFile);

            // SendPacketDirectory
            ConsoleCommand sendPacketDirectory = new ConsoleCommand(SendPacketDirectory, "sendpacketdirectory", "sendpd");
            sendPacketDirectory.Arguments.Add(new ConsoleCommandArgument("Username", false));
            sendPacketDirectory.Arguments.Add(new ConsoleCommandArgument("Dirname", false));
            sendPacketDirectory.Help = "Sends the specified directory's contents as a packet";
            commands.Add(sendPacketDirectory);

            // Exit
            ConsoleCommand exit = new ConsoleCommand(Exit, "exit", "quit");
            exit.Help = "Close the Polaris Server";
            commands.Add(exit);
        }

        public void Draw()
        {
            // Draw log
            if (Logger.lines.Count > prevCount || refreshDraw)
            {
                Console.Clear();
                Console.SetCursorPosition(0, 1);

                for (int i = 0; i < Logger.lines.Count; i++)
                {
                    try
                    {
                        Console.ForegroundColor = Logger.lines[i].color;
                        if (Logger.lines[i].text.Length >= width)
                            Console.Write(Logger.lines[i].text);
                        else
                            Console.WriteLine(Logger.lines[i].text);
                        Console.ResetColor();
                    }
                    catch (ArgumentOutOfRangeException ex)
                    {
                        Logger.WriteException("Tried to draw past the log buffer", ex);
                    }
                }

                refreshDraw = false;
                refreshPrompt = true;
            }

            if (refreshPrompt)
            {
                // Build info string
                if (PolarisApp.Instance != null && PolarisApp.Instance.server != null)
                {
                    int clients = PolarisApp.Instance.server.Clients.Count;
                    float usage = Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024;
                    string time = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();

                    info = string.Format("Clients: {0} | Memory: {1} MB | {2}", clients, usage, time);
                }
                else
                    info = "Initializing...";

                // Build prompt string
                prompt = "Polaris> ";

                // Draw info
                Console.SetCursorPosition(0, height - 2);
                Console.Write(info);
                for (int i = 0; i < width - info.Length; i++)
                    Console.Write(" ");

                // Draw Prompt
                Console.SetCursorPosition(0, height - 1);
                Console.Write(prompt);
                Console.Write(commandLine);
                for (int i = 0; i < width - prompt.Length; i++)
                    Console.Write(" ");
                Console.SetCursorPosition(prompt.Length + commandIndex, height - 1);

                refreshPrompt = false;
            }

            prevCount = Logger.lines.Count;
        }

        public void CheckInput()
        {
            if (Console.KeyAvailable)
            {
                // Need to refresh the display
                refreshPrompt = true;

                // Read key
                bool validKey = true;
                key = Console.ReadKey(true);

                // Check to make sure this is a valid key to append to the command line
                foreach (ConsoleKey controlKey in controlKeys)
                {
                    if (key.Key == controlKey)
                    {
                        validKey = false;
                        break;
                    }
                }

                // Append key to the command line
                if (validKey)
                {
                    commandLine = commandLine.Insert(commandIndex, key.KeyChar.ToString());
                    commandIndex++;
                }

                // Backspace
                if (key.Key == ConsoleKey.Backspace && commandLine.Length > 0 && commandIndex > 0)
                {
                    commandLine = commandLine.Remove(commandIndex - 1, 1);
                    commandIndex--;
                }

                // Cursor movement
                if (key.Key == ConsoleKey.LeftArrow && commandLine.Length > 0 && commandIndex > 0)
                    commandIndex--;
                if (key.Key == ConsoleKey.RightArrow && commandLine.Length > 0 && commandIndex <= commandLine.Length - 1)
                    commandIndex++;
                if (key.Key == ConsoleKey.Home)
                    commandIndex = 0;
                if (key.Key == ConsoleKey.End)
                    commandIndex = commandLine.Length;

                // History
                if (key.Key == ConsoleKey.UpArrow && history.Count > 0)
                {
                    historyIndex--;

                    if (historyIndex < 0)
                        historyIndex = history.Count - 1;

                    commandLine = history[historyIndex];
                    commandIndex = history[historyIndex].Length;
                }
                if (key.Key == ConsoleKey.DownArrow && history.Count > 0)
                {
                    historyIndex++;

                    if (historyIndex > history.Count - 1)
                        historyIndex = 0;

                    commandLine = history[historyIndex];
                    commandIndex = history[historyIndex].Length;
                }

                // Run Command
                if (key.Key == ConsoleKey.Enter)
                {
                    bool valid = false;

                    // Stop if the command line is blank
                    if (string.IsNullOrEmpty(commandLine))
                        Logger.WriteWarning("[CMD] No command specified");
                    else
                    {
                        // Iterate commands
                        foreach (ConsoleCommand command in commands)
                        {
                            string full = commandLine;
                            string[] args = full.Split(' ');

                            foreach (string name in command.Names)
                                if (args[0].ToLower() == name.ToLower())
                                {
                                    command.Run(args, args.Length, full, null);
                                    valid = true;
                                    break;
                                }

                            if (valid)
                                break;
                        }

                        if (!valid)
                            Logger.WriteError("[CMD] {0} - Command not found", commandLine.Split(' ')[0].Trim('\r'));

                        // Add the command line to history and wipe it
                        history.Add(commandLine);
                        historyIndex = history.Count;
                        commandLine = string.Empty;
                    }

                    commandIndex = 0;
                }
            }
        }

        #region Command Handlers

        // TODO: Use that fancy popup box when sending help to a client
        private void Help(string[] args, int length, string full, Client client)
        {
            Logger.WriteCommand(client, "[CMD] List of Commands");

            foreach (ConsoleCommand command in commands)
            {
                if (command.Help != string.Empty)
                {
                    string help = string.Empty;

                    // Name
                    help += command.Names[0];

                    // Arguments
                    if (command.Arguments.Count > 0)
                        foreach (ConsoleCommandArgument argument in command.Arguments)
                            if (argument.Optional)
                                help += " [" + argument.Name + "]";
                            else
                                help += " <" + argument.Name + ">";

                    // Seperator
                    help += " - ";

                    // Help
                    help += command.Help + " ";

                    // Aliases
                    if (command.Names.Count > 1)
                    {
                        help += "[aliases: ";

                        for (int i = 1; i < command.Names.Count; i++)
                            if (i == command.Names.Count - 1)
                                help += command.Names[i];
                            else
                                help += command.Names[i] + ", ";

                        help += "]";
                    }

                    // Log it
                    Logger.WriteCommand(client, "[CMD] {0}", help);
                }
            }
        }

        private void Echo(string[] args, int length, string full, Client client)
        {
            string echo = string.Empty;
            for (int i = 1; i < args.Length; i++)
                echo += args[i];

            Logger.WriteCommand(client, "[CMD] " + echo);
        }

        private void Announce(string[] args, int length, string full, Client client)
        {
            string message = full.Split('"')[1].Split('"')[0].Trim('\"');
            SystemMessagePacket messagePacket = new SystemMessagePacket(message, SystemMessagePacket.MessageType.GoldenTicker);

            foreach (Client c in Server.Instance.Clients)
            {
                if (c.Character == null)
                    continue;

                c.SendPacket(messagePacket);
            }

            Logger.WriteCommand(client, "[CMD] Sent announcement to all players");
        }

        private void ClearLog(string[] args, int length, string full, Client client)
        {
            Logger.lines.Clear();
            refreshDraw = true;
        }

        private void Config(string[] args, int length, string full, Client client)
        {
            FieldInfo[] fields = PolarisApp.Config.GetType().GetFields();
            FieldInfo field = fields.FirstOrDefault(o => o.Name == args[1]);

            switch (args[1].ToLower())
            {
                case "save":
                    PolarisApp.Config.Save();
                    break;

                case "load":
                    PolarisApp.Config.Load();
                    break;

                case "list":
                    Logger.WriteCommand(client, "[CMD] Config Options");
                    foreach (FieldInfo f in fields)
                        Logger.WriteCommand(client, "[CMD] {0} = {1}", f.Name, f.GetValue(PolarisApp.Config));
                    break;

                default: // Set a config option
                    if (args.Length < 3)
                        Logger.WriteCommand(client, "[CMD] Too few arguments");
                    else if (field != null)
                    {
                        string value = string.Empty;

                        if (args[2].Contains('\"'))
                            value = full.Split('"')[1].Split('"')[0].Trim('\"');
                        else
                            value = args[2];

                        if (!PolarisApp.Config.SetField(args[1], value))
                            Logger.WriteCommand(client, "[CMD] Config option {0} could not be changed to {1}", args[1], value);
                        else
                        {
                            Logger.WriteCommand(client, "[CMD] Config option {0} changed to {1}", args[1], value);
                            PolarisApp.Config.SettingsChanged();
                        }
                    }
                    else
                        Logger.WriteCommand(client, "[CMD] Config option {0} not found", args[1]);
                    break;
            }
        }

        private void Exit(string[] args, int length, string full, Client client)
        {
            Environment.Exit(0);
        }

        private void ClearPlayers(string[] args, int length, string full, Client client)
        {
            // Temporary haxifications to pull your own connection
            int ID = -1;
            bool foundPlayer = false;

            if (args.Length > 1)
            {
                string name = args[1];

                // Find the player
                ID = Helper.FindPlayerByUsername(name);
                if (ID != -1)
                    foundPlayer = true;

                // Couldn't find the username
                if (!foundPlayer)
                {
                    Logger.WriteCommand(client, "[CMD] Could not find user " + name);
                    return;
                }
            }

            for (int i = 0; i < PolarisApp.Instance.server.Clients.Count; i++)
            {
                if (ID > -1 && i == ID)
                    continue;

                // This is probably not the right way to do this
                PolarisApp.Instance.server.Clients[i].Socket.Close();
                Logger.WriteCommand(client, "[CMD] Logged out user " + PolarisApp.Instance.server.Clients[i].User.Username);
            }

            Logger.WriteCommand(client, "[CMD] Logged out all players successfully");
        }

        private void SpawnClone(string[] args, int length, string full, Client client)
        {
            // Temporary haxifications to pull your own connection
            string name = args[1].Trim('\"');
            string playerName = args[2].Trim('\"');
            float x = float.Parse(args[3]);
            float y = float.Parse(args[4]);
            float z = float.Parse(args[5]);

            if (client == null)
            {
                int ID = 0;
                bool foundPlayer = false;
                
                // Find the player
                ID = Helper.FindPlayerByUsername(name);
                if (ID != -1)
                    foundPlayer = true;

                // Couldn't find the username
                if (!foundPlayer)
                {
                    Logger.WriteError("[CMD] Could not find user " + name);
                    return;
                }

                client = PolarisApp.Instance.server.Clients[ID];
            }
            
            // Default coordinates
            if (x == 0)
                x = -0.417969f;
            if (y == 0)
                y = 0.000031f;
            if (z == 0)
                z = 134.375f;

            var fakePlayer = new Database.Player();
            fakePlayer.Username = name;
            fakePlayer.Nickname = playerName;
            fakePlayer.PlayerID = 12345678 + new Random().Next();

            var fakeChar = new Character();
            fakeChar.CharacterID = 12345678 + new Random().Next();
            fakeChar.Player = fakePlayer;
            fakeChar.Name = playerName;

            fakeChar.Looks = client.Character.Looks;
            fakeChar.Jobs = client.Character.Jobs;

            var fakePacket = new Packets.CharacterSpawnPacket(fakeChar);
            fakePacket.Position.facingAngle = 0f;
            fakePacket.Position.x = x;
            fakePacket.Position.y = y;
            fakePacket.Position.z = z;
            fakePacket.IsItMe = false;
            client.SendPacket(fakePacket);

            Logger.WriteCommand(client, "[CMD] Spawned a clone of {0} named {1}", name, playerName);
        }

        private void SendPacket(string[] args, int length, string full, Client client)
        {
            string name = args[1].Trim('\"');
            byte type = byte.Parse(args[2]);
            byte subType = byte.Parse(args[3]);
            byte flags = byte.Parse(args[4]);
            byte[] data = new byte[args.Length - 5];
            bool foundPlayer = false;

            // Find the player
            int ID = 0;
            ID = Helper.FindPlayerByUsername(name);
            if (ID != -1)
                foundPlayer = true;

            // Couldn't find the username
            if (!foundPlayer)
            {
                Logger.WriteCommand(client, "[CMD] Could not find user " + name);
                return;
            }

            if (args.Length >= 5)
            {
                int packetSize = 4;
                while (++packetSize < args.Length)
                    data[packetSize - 5] = byte.Parse(args[packetSize]);
            }

            // Send packet
            PolarisApp.Instance.server.Clients[ID].SendPacket(type, subType, flags, data);

            Logger.WriteCommand(client, "[CMD] Sent packet {0:X}-{1:X} with flags {2} to {3}", type, subType, flags, name);
        }

        private void SendPacketDirectory(string[] args, int length, string full, Client client)
        {
            string name = args[1].Trim('\"');
            int ID = 0;
            string dirname = args[2].Trim('\"');
            bool foundPlayer = false;

            // Find the player
            ID = Helper.FindPlayerByUsername(name);
            if (ID != -1)
                foundPlayer = true;

            // Couldn't find the username
            if (!foundPlayer)
            {
                Logger.WriteCommand(client, "[CMD] Could not find user " + name);
                return;
            }

            // Pull packets from the specified directory
            var packetList = Directory.GetFiles(dirname);
            Array.Sort(packetList);

            foreach (var path in packetList)
            {
                int index = -1;
                byte[] data = File.ReadAllBytes(path);
                byte[] packet = new byte[data.Length - 8];

                // Strip the header out
                while (++index < data.Length - 8)
                    packet[index] = data[index + 8];

                // Send packet
                PolarisApp.Instance.server.Clients[ID].SendPacket(data[4], data[5], data[6], packet);

                Logger.WriteCommand(client, "[CMD] Sent contents of {0} as packet {1:X}-{2:X} with flags {3} to {4}", path, data[4], data[5], data[6], name);
            }
        }

        private void SendPacketFile(string[] args, int length, string full, Client client)
        {
            string name = args[1].Trim('\"');
            int ID = 0;
            string filename = args[2].Trim('\"');
            bool foundPlayer = false;

            // Find the player
            ID = Helper.FindPlayerByUsername(name);
            if (ID != -1)
                foundPlayer = true;

            // Couldn't find the username
            if (!foundPlayer)
            {
                Logger.WriteError("[CMD] Could not find user " + name);
                return;
            }

            // Pull packet from the specified file
            int index = -1;
            byte[] data = File.ReadAllBytes(filename);
            byte[] packet = new byte[data.Length - 8];

            // Strip the header out
            while (++index < data.Length - 8)
                packet[index] = data[index + 8];

            // Send packet
            PolarisApp.Instance.server.Clients[ID].SendPacket(data[4], data[5], data[6], packet);

            Logger.WriteCommand(client, "[CMD] Sent contents of {0} as packet {1:X}-{2:X} with flags {3} to {4}", filename, data[4], data[5], data[6], name);
        }

        #endregion
    }
}
