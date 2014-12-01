using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

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
        List<ConsoleCommand> commands = new List<ConsoleCommand>();
        ConsoleKeyInfo key = new ConsoleKeyInfo();
        public int width = 120;
        public int height = 35;
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

            // Echo
            ConsoleCommand echo = new ConsoleCommand(Echo, "echo");
            echo.Help = "Echo the given text back into the Console";
            echo.Arguments.Add(new ConsoleCommandArgument("text", false));
            commands.Add(echo);

            // SpawnClone
            ConsoleCommand spawnClone = new ConsoleCommand(SpawnClone, "spawnclone");
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Name", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("X", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Y", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Z", false));
            spawnClone.Help = "Spawns a clone of your character";
            commands.Add(spawnClone);

            // SendNoPayload
            ConsoleCommand sendNoPayload = new ConsoleCommand(SendNoPayload, "sendnopayload", "sendnp");
            sendNoPayload.Arguments.Add(new ConsoleCommandArgument("Username", false));
            sendNoPayload.Arguments.Add(new ConsoleCommandArgument("Type", false));
            sendNoPayload.Arguments.Add(new ConsoleCommandArgument("SubType", false));
            sendNoPayload.Help = "Sends a packet with no payload";
            commands.Add(sendNoPayload);

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
            prompt = "Polaris> " + commandLine;

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
            for (int i = 0; i < width - prompt.Length; i++)
                Console.Write(" ");
            Console.SetCursorPosition(prompt.Length, height - 1);

            prevCount = Logger.lines.Count;
            infoUpdateCounter++;
        }

        public void CheckInput()
        {
            // Read key
            key = Console.ReadKey(true);

            // Append key to the command line
            if (key.Key != ConsoleKey.Backspace && key.Key != ConsoleKey.Enter)
                commandLine += key.KeyChar;

            // Backspace
            if (key.Key == ConsoleKey.Backspace && commandLine.Length > 0)
                commandLine = commandLine.Remove(commandLine.Length - 1);

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

                    // Empty the command line
                    commandLine = string.Empty;
                }
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

        private void Exit(string[] args, int length, string full)
        {
            Environment.Exit(0);
        }

        private void SpawnClone(string[] args, int length, string full)
        {
            // Temporary haxifications to pull your own connection
            Client client = PolarisApp.Instance.server.Clients[0];
            float x = float.Parse(args[2]);
            float y = float.Parse(args[3]);
            float z = float.Parse(args[4]);

            // Default coordinates
            if (x == 0)
                x = -0.417969f;
            if (y == 0)
                y = 0.000031f;
            if (z == 0)
                z = 134.375f;

            var fakePlayer = new Database.Player();
            fakePlayer.Username = args[1].Trim('\"');
            fakePlayer.Nickname = args[1].Trim('\"');
            fakePlayer.PlayerID = 12345678 + new Random().Next();

            var fakeChar = new Models.Character();
            fakeChar.CharacterID = 12345678 + new Random().Next();
            fakeChar.Player = fakePlayer;
            fakeChar.Name = args[1].Trim('\"');

            fakeChar.Looks = client.Character.Looks;
            fakeChar.Jobs = client.Character.Jobs;

            var fakePacket = new Packets.CharacterSpawnPacket(fakeChar);
            fakePacket.Position.facingAngle = 0f;
            fakePacket.Position.x = x;
            fakePacket.Position.y = y;
            fakePacket.Position.z = z;
            fakePacket.IsItMe = false;
            client.SendPacket(fakePacket);

            Logger.WriteCommand("[CMD] Spawned a clone named " + fakeChar.Name);
        }

        private void SendNoPayload(string[] args, int length, string full)
        {
            string name = args[1].Trim('\"');
            int ID = 0;
            byte type = byte.Parse(args[2]);
            byte subType = byte.Parse(args[3]);
            bool foundPlayer = false;

            // Find the player
            ID = new Helper().FindPlayerByUsername(name);
            if (ID != -1)
                foundPlayer = true;

            // Couldn't find the username
            if (!foundPlayer)
            {
                Logger.WriteError("[CMD] Could not find user " + name);
                return;
            }

            // Send packet
            PolarisApp.Instance.server.Clients[ID].SendPacket(new Packets.NoPayloadPacket(type, subType));

            Logger.WriteCommand("[CMD] Sent no payload packet to {0} of type {1:X}-{2:X}", name, type, subType);
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
            ID = new Helper().FindPlayerByUsername(name);
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
