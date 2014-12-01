using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

using PolarisServer.Models;
using PolarisServer.Packets;

namespace PolarisServer
{
    public delegate void ConsoleCommandDelegate(string[] args, int length, string full);

    public class ConsoleCommandArgument
    {
        private string name;
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private bool optional;
        public bool Optional
        {
            get { return optional; }
            set { optional = value; }
        }

        public ConsoleCommandArgument(string name, bool optional)
        {
            this.name = name;
            this.optional = optional;
        }
    }

    public class ConsoleCommand
    {
        private ConsoleCommandDelegate command;
        public ConsoleCommandDelegate Command
        {
            get { return command; }
            set { command = value; }
        }
        private List<string> names;
        public List<string> Names
        {
            get { return names; }
            set { names = value; }
        }
        private List<ConsoleCommandArgument> arguments;
        public List<ConsoleCommandArgument> Arguments
        {
            get { return arguments; }
            set { arguments = value; }
        }
        private string help;
        public string Help
        {
            get { return help; }
            set { help = value; }
        }

        public ConsoleCommand(ConsoleCommandDelegate cmd, params string[] cmdNames)
        {
            if (cmdNames == null || cmdNames.Length < 1)
                throw new NotSupportedException();

            command = cmd;
            names = new List<string>(cmdNames);
            arguments = new List<ConsoleCommandArgument>();
        }

        public bool Run(string[] args, int length, string full)
        {
            try
            {
                command(args, length, full);
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
        // Input
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

        List<ConsoleCommand> commands = new List<ConsoleCommand>();

        List<string> history = new List<string>();
        int historyIndex = 0;

        public int width = 120;
        public int height = 35;

        int commandIndex = 0;
        string commandLine = string.Empty;

        string info = string.Empty;
        string prompt = string.Empty;

        int prevCount = 0;
        int infoUpdateCounter = 0;

        const int ThreadSleepTime = 20;
        public bool refreshDraw = false;

        public ConsoleSystem()
        {
            Console.Title = "Polaris";
            Console.CursorVisible = true;
            Console.SetWindowSize(width, height);

            CreateCommands();

            infoUpdateCounter = ThreadSleepTime;
        }

        public static void StartDrawing()
        {
            while (true)
            {
                try
                {
                    PolarisApp.ConsoleSystem.Draw();
                    Thread.Sleep(ThreadSleepTime);
                }
                catch (ThreadInterruptedException ex)
                {
                    Logger.WriteException("Thread inturrupted", ex);
                }
                catch (ThreadStateException ex)
                {
                    Logger.WriteException("Thread state error", ex);
                }
            }
        }

        public static void StartInput()
        {
            while (true)
            {
                try
                {
                    PolarisApp.ConsoleSystem.CheckInput();
                    Thread.Sleep(ThreadSleepTime);
                }
                catch (ThreadInterruptedException ex)
                {
                    Logger.WriteException("Thread inturrupted", ex);
                }
                catch (ThreadStateException ex)
                {
                    Logger.WriteException("Thread state error", ex);
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
            config.Arguments.Add(new ConsoleCommandArgument("Option", false));
            config.Arguments.Add(new ConsoleCommandArgument("Value", false));
            config.Help = "Set a configuration variable to the specified value";
            commands.Add(config);

            // Echo
            ConsoleCommand echo = new ConsoleCommand(Echo, "echo");
            echo.Help = "Echo the given text back into the Console";
            echo.Arguments.Add(new ConsoleCommandArgument("text", false));
            commands.Add(echo);

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

            // LoadLooks
            ConsoleCommand loadLooks = new ConsoleCommand(LoadLooks, "loadlooks");
            loadLooks.Arguments.Add(new ConsoleCommandArgument("Username", false));
            loadLooks.Arguments.Add(new ConsoleCommandArgument("Filename", false));
            loadLooks.Help = "Loads a binary file containing looks structure data";
            commands.Add(loadLooks);

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
            sendPacketFile.Arguments.Add(new ConsoleCommandArgument("Type", false));
            sendPacketFile.Arguments.Add(new ConsoleCommandArgument("SubType", false));
            sendPacketFile.Arguments.Add(new ConsoleCommandArgument("Flags", false));
            sendPacketFile.Arguments.Add(new ConsoleCommandArgument("Filename", false));
            sendPacketFile.Help = "Sends the specified files contents as a packet";
            commands.Add(sendPacketFile);

            // Exit
            ConsoleCommand exit = new ConsoleCommand(Exit, "exit", "quit");
            exit.Help = "Close the Polaris Server";
            commands.Add(exit);
        }

        public void Draw()
        {
            // Build info string
            if (infoUpdateCounter >= ThreadSleepTime)
            {
                if (PolarisApp.Instance != null && PolarisApp.Instance.server != null)
                {
                    int clients = PolarisApp.Instance.server.Clients.Count;
                    float usage = Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024;
                    string time = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();

                    info = string.Format("Clients: {0} | Memory: {1} MB | {2}", clients, usage, time);
                    infoUpdateCounter = 0;
                }
                else
                    info = "Initializing...";
            }

            // Build prompt string
            prompt = "Polaris> ";

            Console.SetCursorPosition(0, 3);

            // Draw log
            if (Logger.lines.Count > prevCount || refreshDraw)
            {
                Console.Clear();

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
            }

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

            prevCount = Logger.lines.Count;
            infoUpdateCounter++;
        }

        public void CheckInput()
        {
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
                                command.Run(args, args.Length, full);
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

        #region Command Handlers

        private void Help(string[] args, int length, string full)
        {
            Logger.WriteCommand("[CMD] List of Commands");

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
                    Logger.WriteCommand("[CMD] {0}", help);
                }
            }
        }

        private void Echo(string[] args, int length, string full)
        {
            string echo = string.Empty;
            for (int i = 1; i < args.Length; i++)
                echo += args[i];

            Logger.WriteCommand("[CMD] " + echo);
        }

        private void Config(string[] args, int length, string full)
        {
            switch (args[1].ToLower())
            {
                default:
                    Logger.WriteError("[CMD] Invalid configuration option");
                    break;
                
                case "verbosepackets":
                    Logger.VerbosePackets = !Logger.VerbosePackets;
                    Logger.WriteCommand("[CMD] Verbose packet logging " + (Logger.VerbosePackets ? "enabled" : "disabled"));
                    break;
            }
        }

        private void Exit(string[] args, int length, string full)
        {
            Environment.Exit(0);
        }

        private void ClearPlayers(string[] args, int length, string full)
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
                    Logger.WriteError("[CMD] Could not find user " + name);
                    return;
                }
            }

            for (int i = 0; i < PolarisApp.Instance.server.Clients.Count; i++)
            {
                if (ID > -1 && i == ID)
                    continue;

                // This is probably not the right way to do this
                PolarisApp.Instance.server.Clients[i].Socket.Close();
                Logger.WriteCommand("[CMD] Logged out user " + PolarisApp.Instance.server.Clients[i].User.Username);
            }

            Logger.WriteCommand("[CMD] Logged out all players successfully");
        }

        private void SpawnClone(string[] args, int length, string full)
        {
            // Temporary haxifications to pull your own connection
            int ID = 0;
            string name = args[1].Trim('\"');
            string playerName = args[2].Trim('\"');
            float x = float.Parse(args[3]);
            float y = float.Parse(args[4]);
            float z = float.Parse(args[5]);
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

            // Default coordinates
            if (x == 0)
                x = -0.417969f;
            if (y == 0)
                y = 0.000031f;
            if (z == 0)
                z = 134.375f;

            Client client = PolarisApp.Instance.server.Clients[ID];

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

            Logger.WriteCommand("[CMD] Spawned a clone of {0} named {1}", name, playerName);
        }

        private void LoadLooks(string[] args, int length, string full)
        {
            int ID = 0;
            string name = args[1].Trim('\"');
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

            byte[] bytes = File.ReadAllBytes(filename);
            Client client = PolarisApp.Instance.server.Clients[ID];
            client.Character.Looks = new Character.LooksParam();
            client.Character.Looks = Helper.ByteArrayToStructure<Character.LooksParam>(bytes);

            // Setup packets
            var areaPacket = File.ReadAllBytes("testSetAreaPacket.bin");
            var playerID = new PacketWriter();
            playerID.WritePlayerHeader((uint)client.User.PlayerID);
            
            // Send packets
            client.SendPacket(new NoPayloadPacket(0x3, 0x4)); // Loading screen
            client.SendPacket(0x6, 0x00, 0, playerID.ToArray()); // Set player ID
            client.SendPacket(0x03, 0x24, 4, areaPacket); // Setup area
            client.SendPacket(new CharacterSpawnPacket(client.Character)); // Spawn
            client.SendPacket(new NoPayloadPacket(0x03, 0x2B)); // Enable Controls

            Logger.WriteHex("[CMD] Looks Data from file: ", File.ReadAllBytes(filename));
            Logger.WriteCommand("[CMD] Loaded looks from {0} onto {1}", filename, name);
        }

        private void SendPacket(string[] args, int length, string full)
        {
            string name = args[1].Trim('\"');
            int ID = 0;
            byte type = byte.Parse(args[2]);
            byte subType = byte.Parse(args[3]);
            byte flags = byte.Parse(args[4]);
            byte[] data = new byte[args.Length - 5];
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

            if (args.Length >= 5)
            {
                int packetSize = 4;
                while (++packetSize < args.Length)
                    data[packetSize - 5] = byte.Parse(args[packetSize]);
            }

            // Send packet
            PolarisApp.Instance.server.Clients[ID].SendPacket(type, subType, flags, data);

            Logger.WriteCommand("[CMD] Sent packet {0:X}-{1:X} with flags {2} to {3}", type, subType, flags, name);
            if (args.Length < 6)
                Logger.WriteCommand("[CMD] No data sent, sending basic payload");
            else
                Logger.WriteHex("[CMD] Sent packet data: ", data);
        }

        private void SendPacketFile(string[] args, int length, string full)
        {
            string name = args[1].Trim('\"');
            int ID = 0;
            byte type = byte.Parse(args[2]);
            byte subType = byte.Parse(args[3]);
            byte flags = byte.Parse(args[4]);
            string filename = args[5].Trim('\"');
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
            var packet = File.ReadAllBytes(filename);

            // Send packet
            PolarisApp.Instance.server.Clients[ID].SendPacket(type, subType, flags, packet);

            Logger.WriteCommand("[CMD] Sent contents of {0} as packet {1:X}-{2:X} with flags {3} to {4}", filename, type, subType, flags, name);
        }

        #endregion
    }
}
