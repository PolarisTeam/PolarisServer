using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Timers;
using Timer = System.Timers.Timer;

using PolarisServer.Database;
using PolarisServer.Models;
using PolarisServer.Object;
using PolarisServer.Packets;
using PolarisServer.Packets.PSOPackets;
using PolarisServer.Zone;

namespace PolarisServer
{
    public delegate void ConsoleCommandDelegate(string[] args, int length, string full, Client client);

    public class ConsoleCommandArgument
    {
        public ConsoleCommandArgument(string name, bool optional)
        {
            Name = name;
            Optional = optional;
        }

        public string Name { get; set; }
        public bool Optional { get; set; }
    }

    public class ConsoleCommand
    {
        public ConsoleCommand(ConsoleCommandDelegate cmd, params string[] cmdNames)
        {
            if (cmdNames == null || cmdNames.Length < 1)
                throw new NotSupportedException();

            Command = cmd;
            Names = new List<string>(cmdNames);
            Arguments = new List<ConsoleCommandArgument>();
        }

        public ConsoleCommandDelegate Command { get; set; }
        public List<string> Names { get; set; }
        public List<ConsoleCommandArgument> Arguments { get; set; }
        public string Help { get; set; }

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
    ///     Handles commands and drawing/updating the console
    /// </summary>
    public class ConsoleSystem
    {
        private readonly object _consoleLock = new object();

        private readonly List<ConsoleKey> _controlKeys = new List<ConsoleKey>
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

        private const string Prompt = "Polaris> ";

        private string _commandLine = string.Empty;
        private int _commandIndex;
        private int _commandRowInConsole;

        private readonly List<string> _history = new List<string>();
        private int _historyIndex;

        private int _lastDrawnCommandLineSize;
        private int _maxCommandLineSize = -1;

        public Thread Thread;

        public List<ConsoleCommand> Commands = new List<ConsoleCommand>();
        private ConsoleKeyInfo _key;
        
        // ReSharper disable once InconsistentNaming
        public Timer timer;

        public ConsoleSystem()
        {
            Console.Title = "Polaris";
            Console.CursorVisible = true;
            Console.Clear();

            Width = Console.WindowWidth;
            Height = Console.WindowHeight;
            _maxCommandLineSize = Width - Prompt.Length;

            timer = new Timer(1000);
            timer.Elapsed += TimerRefresh;
            timer.Start();

            CreateCommands();
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public void SetSize(int width, int height)
        {
            try
            {
                Console.SetWindowSize(width, height);

                Width = width;
                Height = height;
                _maxCommandLineSize = width - Prompt.Length;
            }
            catch (Exception)
            {
                Logger.WriteWarning("[WRN] Failed to set console size to ({0},{1}).", width, height);
            }
        }

        public void AddLine(ConsoleColor color, string text)
        {
            lock (_consoleLock)
            {
                BlankDrawnCommandLine();

                var useColors = PolarisApp.Config.UseConsoleColors;
                var saveColor = Console.ForegroundColor;

                if (useColors)
                    Console.ForegroundColor = color;
                Console.WriteLine(text);

                if (useColors)
                    Console.ForegroundColor = saveColor;

                _commandRowInConsole = Console.CursorTop;

                RefreshCommandLine();
                FixCursorPosition();
            }
        }

        private void BlankDrawnCommandLine()
        {
            if (_lastDrawnCommandLineSize > 0)
            {
                Console.SetCursorPosition(0, _commandRowInConsole);

                for (int i = 0; i < _lastDrawnCommandLineSize; i++)
                    Console.Write(' ');

                _lastDrawnCommandLineSize = 0;

                Console.CursorLeft = 0;
            }
        }

        private void RefreshCommandLine()
        {
            BlankDrawnCommandLine();

            Console.Write(Prompt);
            Console.Write(_commandLine);

            _lastDrawnCommandLineSize = Prompt.Length + _commandLine.Length;
        }

        private void FixCursorPosition()
        {
            Console.SetCursorPosition(Prompt.Length + _commandIndex, _commandRowInConsole);
        }

        private string AssembleInfoBar()
        {
            if (PolarisApp.Instance != null && PolarisApp.Instance.Server != null)
            {
                var clients = PolarisApp.Instance.Server.Clients.Count;
                float usage = Process.GetCurrentProcess().PrivateMemorySize64 / 1024 / 1024;

                var time = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString();

                return string.Format("Clients: {0} | Memory: {1} MB | {2}", clients, usage, time);
            }

            return "Initializing...";
        }

        private void TimerRefresh(object sender, ElapsedEventArgs e)
        {
            lock (_consoleLock)
            {
                Console.Title = "Polaris - " + AssembleInfoBar();
            }
        }

        public static void StartThread()
        {
            while (true)
            {
                try
                {
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
            var help = new ConsoleCommand(Help, "help") { Help = "Displays help for all commands" };
            Commands.Add(help);

            // Config
            var config = new ConsoleCommand(Config, "config", "c");
            config.Arguments.Add(new ConsoleCommandArgument("Save | Load | List | Option Name", false));
            config.Arguments.Add(new ConsoleCommandArgument("Value", true));
            config.Help = "Set a configuration variable to the specified value";
            Commands.Add(config);

            // Clear
            var clearLog = new ConsoleCommand(ClearLog, "clear", "cls") { Help = "Clears the current log buffer" };
            Commands.Add(clearLog);

            // Echo
            var echo = new ConsoleCommand(Echo, "echo") { Help = "Echo the given text back into the Console" };
            echo.Arguments.Add(new ConsoleCommandArgument("text", false));
            Commands.Add(echo);

            var lua = new ConsoleCommand(RunLUA, "lua");
            lua.Arguments.Add(new ConsoleCommandArgument("user", true));
            lua.Arguments.Add(new ConsoleCommandArgument("lua", false));
            Commands.Add(lua);

            // Announce
            var announce = new ConsoleCommand(Announce, "announce", "a");
            announce.Arguments.Add(new ConsoleCommandArgument("Message", false));
            announce.Help = "Sends a message to all connected players";
            Commands.Add(announce);

            // ClearPlayers
            var clearPlayers = new ConsoleCommand(ClearPlayers, "clearplayers", "cp");
            clearPlayers.Arguments.Add(new ConsoleCommandArgument("Exempt Username", true));
            clearPlayers.Help = "Disconnects all players from the server";
            Commands.Add(clearPlayers);

            // SpawnClone
            var spawnClone = new ConsoleCommand(SpawnClone, "spawnclone");
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Username", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Player Name", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("X", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Y", false));
            spawnClone.Arguments.Add(new ConsoleCommandArgument("Z", false));
            spawnClone.Help = "Spawns a clone of your character";
            Commands.Add(spawnClone);

            // SendPacket
            var sendPacket = new ConsoleCommand(SendPacket, "sendpacket", "sendp");
            sendPacket.Arguments.Add(new ConsoleCommandArgument("Name", false));
            sendPacket.Arguments.Add(new ConsoleCommandArgument("Type", false));
            sendPacket.Arguments.Add(new ConsoleCommandArgument("SubType", false));
            sendPacket.Arguments.Add(new ConsoleCommandArgument("Flags", false));
            sendPacket.Arguments.Add(new ConsoleCommandArgument("Data ...", false));
            sendPacket.Help = "Sends a packet to a client";
            Commands.Add(sendPacket);

            // SendPacketFile
            var sendPacketFile = new ConsoleCommand(SendPacketFile, "sendpacketfile", "sendpf");
            sendPacketFile.Arguments.Add(new ConsoleCommandArgument("Username", false));
            sendPacketFile.Arguments.Add(new ConsoleCommandArgument("Filename", false));
            sendPacketFile.Help = "Sends the specified file's contents as a packet";
            Commands.Add(sendPacketFile);

            // SendPacketDirectory
            var sendPacketDirectory = new ConsoleCommand(SendPacketDirectory, "sendpacketdirectory", "sendpd");
            sendPacketDirectory.Arguments.Add(new ConsoleCommandArgument("Username", false));
            sendPacketDirectory.Arguments.Add(new ConsoleCommandArgument("Dirname", false));
            sendPacketDirectory.Help = "Sends the specified directory's contents as a packet";
            Commands.Add(sendPacketDirectory);

            // SendPacketDirectory Slow
            var sendPacketDirectorySlow = new ConsoleCommand(SendPacketDirectorySlow, "sendpacketdirectoryslow", "sendpds");
            sendPacketDirectorySlow.Arguments.Add(new ConsoleCommandArgument("Username", false));
            sendPacketDirectorySlow.Arguments.Add(new ConsoleCommandArgument("Dirname", false));
            sendPacketDirectorySlow.Arguments.Add(new ConsoleCommandArgument("Sleeptime", false));
            sendPacketDirectorySlow.Help = "Sends the specified directory's contents as a packet (With delay between packets)";
            Commands.Add(sendPacketDirectorySlow);

            var teleportPlayer = new ConsoleCommand(TeleportPlayer, "teleportplayer", "tp");

            teleportPlayer.Arguments.Add(new ConsoleCommandArgument("RotX", false));
            teleportPlayer.Arguments.Add(new ConsoleCommandArgument("RotY", false));
            teleportPlayer.Arguments.Add(new ConsoleCommandArgument("RotZ", false));
            teleportPlayer.Arguments.Add(new ConsoleCommandArgument("RotW", false));

            teleportPlayer.Arguments.Add(new ConsoleCommandArgument("PosX", false));
            teleportPlayer.Arguments.Add(new ConsoleCommandArgument("PosY", false));
            teleportPlayer.Arguments.Add(new ConsoleCommandArgument("PosZ", false));

            teleportPlayer.Arguments.Add(new ConsoleCommandArgument("Username", true));

            teleportPlayer.Help = "Teleports a player to the given position.";
            Commands.Add(teleportPlayer);

            var teleportPlayer2 = new ConsoleCommand(TeleportPlayer_POS, "teleportplayerpos", "tpp");

            teleportPlayer2.Arguments.Add(new ConsoleCommandArgument("PosX", false));
            teleportPlayer2.Arguments.Add(new ConsoleCommandArgument("PosY", false));
            teleportPlayer2.Arguments.Add(new ConsoleCommandArgument("PosZ", false));

            teleportPlayer2.Arguments.Add(new ConsoleCommandArgument("Username", true));

            teleportPlayer2.Help = "Teleports a player to the given position. (pos only)";
            Commands.Add(teleportPlayer2);

            var changeThezone = new ConsoleCommand(ChangeArea, "areachange", "map");
            changeThezone.Arguments.Add(new ConsoleCommandArgument("username", false));
            changeThezone.Arguments.Add(new ConsoleCommandArgument("zoneID", false));
            changeThezone.Arguments.Add(new ConsoleCommandArgument("mapNumber", false));
            changeThezone.Arguments.Add(new ConsoleCommandArgument("flags", false));
            changeThezone.Arguments.Add(new ConsoleCommandArgument("seed", false));
            changeThezone.Arguments.Add(new ConsoleCommandArgument("sizeX", false));
            changeThezone.Arguments.Add(new ConsoleCommandArgument("sizeY", false));
            changeThezone.Arguments.Add(new ConsoleCommandArgument("templateNum", false));

            teleportPlayer.Help = "Spawns you elsewhere.";
            Commands.Add(changeThezone);

            var SpawnObjectCommand = new ConsoleCommand(SpawnObject, "spawnobject", "sobj");
            SpawnObjectCommand.Arguments.Add(new ConsoleCommandArgument("username", true));
            SpawnObjectCommand.Arguments.Add(new ConsoleCommandArgument("objectName", false));
            SpawnObjectCommand.Arguments.Add(new ConsoleCommandArgument("entityID", false));

            SpawnObjectCommand.Arguments.Add(new ConsoleCommandArgument("RotX", false));
            SpawnObjectCommand.Arguments.Add(new ConsoleCommandArgument("RotY", false));
            SpawnObjectCommand.Arguments.Add(new ConsoleCommandArgument("RotZ", false));
            SpawnObjectCommand.Arguments.Add(new ConsoleCommandArgument("RotW", false));
            SpawnObjectCommand.Arguments.Add(new ConsoleCommandArgument("PosX", false));
            SpawnObjectCommand.Arguments.Add(new ConsoleCommandArgument("PosY", false));
            SpawnObjectCommand.Arguments.Add(new ConsoleCommandArgument("PosZ", false));

            SpawnObjectCommand.Help = "Spawns an object. (Does not contain object arguments/flags.";
            Commands.Add(SpawnObjectCommand);

            var ImportNPC = new ConsoleCommand(ImportNPCs, "importnpc");
            ImportNPC.Arguments.Add(new ConsoleCommandArgument("zone", false));
            ImportNPC.Arguments.Add(new ConsoleCommandArgument("npcfolder", false));
            ImportNPC.Help = "Imports a folder of NPC spawn packets into the database.";
            Commands.Add(ImportNPC);

            var ImportObject = new ConsoleCommand(ImportObjects, "importobjects");
            ImportObject.Arguments.Add(new ConsoleCommandArgument("zone", false));
            ImportObject.Arguments.Add(new ConsoleCommandArgument("objectfolder", false));
            ImportObject.Help = "Imports a folder of object spawn packets into the database.";
            Commands.Add(ImportObject);

            var tellLoc = new ConsoleCommand(TellPosition, "pos");
            tellLoc.Help = "Tells you your current location. (Need to be a client to use this.)";
            Commands.Add(tellLoc);

            // Exit
            var exit = new ConsoleCommand(Exit, "exit", "quit") { Help = "Close the Polaris Server" };
            Commands.Add(exit);
        }

        public void CheckInput()
        {
            // Read key
            _key = Console.ReadKey(true);

            // Check to make sure this is a valid key to append to the command line
            var validKey = _controlKeys.All(controlKey => _key.Key != controlKey);

            // Append key to the command line
            if (validKey && (_commandLine.Length + 1) < _maxCommandLineSize)
            {
                _commandLine = _commandLine.Insert(_commandIndex, _key.KeyChar.ToString());
                _commandIndex++;
            }

            // Backspace
            if (_key.Key == ConsoleKey.Backspace && _commandLine.Length > 0 && _commandIndex > 0)
            {
                _commandLine = _commandLine.Remove(_commandIndex - 1, 1);
                _commandIndex--;
            }

            // Cursor movement
            if (_key.Key == ConsoleKey.LeftArrow && _commandLine.Length > 0 && _commandIndex > 0)
                _commandIndex--;
            if (_key.Key == ConsoleKey.RightArrow && _commandLine.Length > 0 && _commandIndex <= _commandLine.Length - 1)
                _commandIndex++;
            if (_key.Key == ConsoleKey.Home)
                _commandIndex = 0;
            if (_key.Key == ConsoleKey.End)
                _commandIndex = _commandLine.Length;

            // History
            if (_key.Key == ConsoleKey.UpArrow && _history.Count > 0)
            {
                _historyIndex--;

                if (_historyIndex < 0)
                    _historyIndex = _history.Count - 1;

                _commandLine = _history[_historyIndex];
                _commandIndex = _history[_historyIndex].Length;
            }
            if (_key.Key == ConsoleKey.DownArrow && _history.Count > 0)
            {
                _historyIndex++;

                if (_historyIndex > _history.Count - 1)
                    _historyIndex = 0;

                _commandLine = _history[_historyIndex];
                _commandIndex = _history[_historyIndex].Length;
            }

            // Run Command
            if (_key.Key == ConsoleKey.Enter)
            {
                var valid = false;

                // Stop if the command line is blank
                if (string.IsNullOrEmpty(_commandLine))
                    Logger.WriteWarning("[CMD] No command specified");
                else
                {
                    // Iterate commands
                    foreach (var command in Commands)
                    {
                        var full = _commandLine;
                        var args = full.Split(' ');

                        if (command.Names.Any(name => args[0].ToLower() == name.ToLower()))
                        {
                            command.Run(args, args.Length, full, null);
                            valid = true;
                        }

                        if (valid)
                            break;
                    }

                    if (!valid)
                        Logger.WriteError("[CMD] {0} - Command not found", _commandLine.Split(' ')[0].Trim('\r'));

                    // Add the command line to history and wipe it
                    _history.Add(_commandLine);
                    _historyIndex = _history.Count;
                    _commandLine = string.Empty;
                }

                _commandIndex = 0;
            }

            lock (_consoleLock)
            {
                RefreshCommandLine();
                FixCursorPosition();
            }
        }

        #region Command Handlers

        // TODO: Use that fancy popup box when sending help to a client
        private void Help(string[] args, int length, string full, Client client)
        {
            Logger.WriteCommand(client, "[CMD] List of Commands");

            foreach (var command in Commands)
            {
                if (command.Help != string.Empty)
                {
                    var help = string.Empty;

                    // Name
                    help += command.Names[0];

                    // Arguments
                    if (command.Arguments.Count > 0)
                        foreach (var argument in command.Arguments)
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

                        for (var i = 1; i < command.Names.Count; i++)
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
            var echo = string.Empty;
            for (var i = 1; i < args.Length; i++)
                echo += args[i];

            Logger.WriteCommand(client, "[CMD] " + echo);
        }

        private void Announce(string[] args, int length, string full, Client client)
        {
            var message = full.Split('"')[1].Split('"')[0].Trim('\"');
            var messagePacket = new SystemMessagePacket(message, SystemMessagePacket.MessageType.GoldenTicker);

            foreach (var c in Server.Instance.Clients)
            {
                if (c.Character == null)
                    continue;

                c.SendPacket(messagePacket);
            }

            Logger.WriteCommand(client, "[CMD] Sent announcement to all players");
        }

        private void ClearLog(string[] args, int length, string full, Client client)
        {
            // What do, how do we deal with this now --Ninji
        }

        private void Config(string[] args, int length, string full, Client client)
        {
            var fields = PolarisApp.Config.GetType().GetFields();
            var field = fields.FirstOrDefault(o => o.Name == args[1]);

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
                    foreach (var f in fields)
                        Logger.WriteCommand(client, "[CMD] {0} = {1}", f.Name, f.GetValue(PolarisApp.Config));
                    break;

                default: // Set a config option
                    if (args.Length < 3)
                        Logger.WriteCommand(client, "[CMD] Too few arguments");
                    else if (field != null)
                    {
                        var value = args[2].Contains('\"') ? full.Split('"')[1].Split('"')[0].Trim('\"') : args[2];

                        if (!PolarisApp.Config.SetField(args[1], value))
                            Logger.WriteCommand(client, "[CMD] Config option {0} could not be changed to {1}", args[1],
                                value);
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
            var id = -1;
            var foundPlayer = false;

            if (args.Length > 1)
            {
                var name = args[1];

                // Find the player
                id = Helper.FindPlayerByUsername(name);
                if (id != -1)
                    foundPlayer = true;

                // Couldn't find the username
                if (!foundPlayer)
                {
                    Logger.WriteCommand(client, "[CMD] Could not find user " + name);
                    return;
                }
            }

            for (var i = 0; i < PolarisApp.Instance.Server.Clients.Count; i++)
            {
                if (id > -1 && i == id)
                    continue;

                // This is probably not the right way to do this
                PolarisApp.Instance.Server.Clients[i].Socket.Close();
                Logger.WriteCommand(client,
                    "[CMD] Logged out user " + PolarisApp.Instance.Server.Clients[i].User.Username);
            }

            Logger.WriteCommand(client, "[CMD] Logged out all players successfully");
        }

        private void SpawnClone(string[] args, int length, string full, Client client)
        {
            // Temporary haxifications to pull your own connection
            var name = args[1].Trim('\"');
            var playerName = args[2].Trim('\"');
            var x = float.Parse(args[3]);
            var y = float.Parse(args[4]);
            var z = float.Parse(args[5]);

            if (client == null)
            {
                var foundPlayer = false;

                // Find the player
                var id = Helper.FindPlayerByUsername(name);
                if (id != -1)
                    foundPlayer = true;

                // Couldn't find the username
                if (!foundPlayer)
                {
                    Logger.WriteError("[CMD] Could not find user " + name);
                    return;
                }

                client = PolarisApp.Instance.Server.Clients[id];
            }

            // Default coordinates
            if (x == 0)
                x = -0.417969f;
            if (y == 0)
                y = 0.000031f;
            if (z == 0)
                z = 134.375f;

            var fakePlayer = new Player
            {
                Username = name,
                Nickname = playerName,
                PlayerId = (12345678 + new Random().Next())
            };

            var fakeChar = new Character
            {
                CharacterId = 12345678 + new Random().Next(),
                Player = fakePlayer,
                Name = playerName,
                Looks = client.Character.Looks,
                Jobs = client.Character.Jobs
            };


            var fakePacket = new CharacterSpawnPacket(fakeChar, new PSOLocation(0f, 1f, 0f, 0f, x, y, z))
            {
                IsItMe = false
            };
            client.SendPacket(fakePacket);

            Logger.WriteCommand(client, "[CMD] Spawned a clone of {0} named {1}", name, playerName);
        }

        private void SendPacket(string[] args, int length, string full, Client client)
        {
            var name = args[1].Trim('\"');
            var type = byte.Parse(args[2]);
            var subType = byte.Parse(args[3]);
            var flags = byte.Parse(args[4]);
            var data = new byte[args.Length - 5];
            var foundPlayer = false;

            // Find the player
            var id = Helper.FindPlayerByUsername(name);
            if (id != -1)
                foundPlayer = true;

            // Couldn't find the username
            if (!foundPlayer)
            {
                Logger.WriteCommand(client, "[CMD] Could not find user " + name);
                return;
            }

            if (args.Length >= 5)
            {
                var packetSize = 4;
                while (++packetSize < args.Length)
                    data[packetSize - 5] = byte.Parse(args[packetSize]);
            }

            // Send packet
            PolarisApp.Instance.Server.Clients[id].SendPacket(type, subType, flags, data);

            Logger.WriteCommand(client, "[CMD] Sent packet {0:X}-{1:X} with flags {2} to {3}", type, subType, flags,
                name);
        }

        private void SendPacketDirectory(string[] args, int length, string full, Client client)
        {
            var name = args[1].Trim('\"');
            var dirname = args[2].Trim('\"');
            var foundPlayer = false;

            // Find the player
            var id = Helper.FindPlayerByUsername(name);
            if (id != -1)
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
                var index = -1;
                var data = File.ReadAllBytes(path);
                var packet = new byte[data.Length - 8];

                // Strip the header out
                while (++index < data.Length - 8)
                    packet[index] = data[index + 8];

                // Send packet
                PolarisApp.Instance.Server.Clients[id].SendPacket(data[4], data[5], data[6], packet);

                Logger.WriteCommand(client, "[CMD] Sent contents of {0} as packet {1:X}-{2:X} with flags {3} to {4}",
                    path, data[4], data[5], data[6], name);
            }
        }

        private void SendPacketDirectorySlow(string[] args, int length, string full, Client client)
        {
            var name = args[1].Trim('\"');
            var dirname = args[2].Trim('\"');
            var delay = Int32.Parse(args[3]);
            var foundPlayer = false;

            // Find the player
            var id = Helper.FindPlayerByUsername(name);
            if (id != -1)
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
                var index = -1;
                var data = File.ReadAllBytes(path);
                var packet = new byte[data.Length - 8];

                // Strip the header out
                while (++index < data.Length - 8)
                    packet[index] = data[index + 8];

                // Send packet
                PolarisApp.Instance.Server.Clients[id].SendPacket(data[4], data[5], data[6], packet);

                Logger.WriteCommand(client, "[CMD] Sent contents of {0} as packet {1:X}-{2:X} with flags {3} to {4}",
                    path, data[4], data[5], data[6], name);
                Thread.Sleep(delay);
            }
        }

        private void SendPacketFile(string[] args, int length, string full, Client client)
        {
            var name = args[1].Trim('\"');
            var filename = args[2].Trim('\"');
            var foundPlayer = false;

            // Find the player
            var id = Helper.FindPlayerByUsername(name);
            if (id != -1)
                foundPlayer = true;

            // Couldn't find the username
            if (!foundPlayer)
            {
                Logger.WriteError("[CMD] Could not find user " + name);
                return;
            }

            // Pull packet from the specified file
            var index = -1;
            var data = File.ReadAllBytes(filename);
            var packet = new byte[data.Length - 8];

            // Strip the header out
            while (++index < data.Length - 8)
                packet[index] = data[index + 8];

            // Send packet
            PolarisApp.Instance.Server.Clients[id].SendPacket(data[4], data[5], data[6], packet);

            Logger.WriteCommand(client, "[CMD] Sent contents of {0} as packet {1:X}-{2:X} with flags {3} to {4}",
                filename, data[4], data[5], data[6], name);
        }

        private void TeleportPlayer(string[] args, int length, string full, Client client)
        {
            var foundPlayer = false;
            var id = 0;
            if (client != null)
            {
                id = client.User.PlayerId;
                foundPlayer = true;
            }
                
            else
            {
                var name = args[8].Trim('\"');

                Helper.FindPlayerByUsername(name);
                if (id != -1)
                    foundPlayer = true;
                client = PolarisApp.Instance.Server.Clients[id];
            }
            

            // Couldn't find the username
            if (!foundPlayer)
            {
                Logger.WriteError("[CMD] Could not find user.");
                return;
            }

            PSOLocation destination = new PSOLocation(float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]),
                float.Parse(args[5]), float.Parse(args[6]), float.Parse(args[7]));


            client.SendPacket(new TeleportTransferPacket(ObjectManager.Instance.getObjectByID("lobby", 443), destination));

        }

        private void TeleportPlayer_POS(string[] args, int length, string full, Client client)
        {
            var foundPlayer = false;
            var id = 0;
            if (client != null)
            {
                id = client.User.PlayerId;
                foundPlayer = true;
            }
            else
            {
                var name = args[4].Trim('\"');

                Helper.FindPlayerByUsername(name);
                if (id != -1)
                    foundPlayer = true;

                client = PolarisApp.Instance.Server.Clients[id];
            }


            // Couldn't find the username
            if (!foundPlayer)
            {
                Logger.WriteError("[CMD] Could not find user.");
                return;
            }

            PSOLocation destination = new PSOLocation(0f, 1f, 0f, 0f,
                float.Parse(args[1]), float.Parse(args[2]), float.Parse(args[3]));


            client.SendPacket(new TeleportTransferPacket(ObjectManager.Instance.getObjectByID("lobby", 443), destination));

        }

        private void ChangeArea(string[] args, int length, string full, Client client)
        {
            var name = args[1].Trim('\"');
            var foundPlayer = false;


            var id = Helper.FindPlayerByUsername(name);
            if (id != -1)
                foundPlayer = true;

            // Couldn't find the username
            if (!foundPlayer)
            {
                Logger.WriteError("[CMD] Could not find user " + name);
                return;
            }

            Client context = PolarisApp.Instance.Server.Clients[id];

            Map dstMap = null;

            if (!ZoneManager.Instance.InstanceExists(String.Format("tpinstance_{0}_{1}", Int32.Parse(args[3]), Int32.Parse(args[8]))))
            {
                dstMap = new Map("tpmap", Int32.Parse(args[3]), Int32.Parse(args[8]), (Map.MapType)Int32.Parse(args[2]), (Map.MapFlags)Int32.Parse(args[4]))
                { GenerationArgs = new Map.GenParam() { seed = UInt32.Parse(args[5]), xsize = UInt32.Parse(args[6]), ysize = UInt32.Parse(args[7]) } };
                ZoneManager.Instance.NewInstance(String.Format("tpinstance_{0}", Int32.Parse(args[3])), dstMap);
            } else
            {
                dstMap = ZoneManager.Instance.MapFromInstance("tpmap", String.Format("tpinstance_{0}_{1}", Int32.Parse(args[3]), Int32.Parse(args[8])));
            }

            dstMap.SpawnClient(context, dstMap.GetDefaultLocation());



            //PSOLocation destination = new PSOLocation(float.Parse(args[2]), float.Parse(args[3]), float.Parse(args[4]), float.Parse(args[5]),float.Parse(args[6]), float.Parse(args[7]), float.Parse(args[8]));


            //PolarisApp.Instance.Server.Clients[id].SendPacket(new TeleportTransferPacket(ObjectManager.Instance.getObjectByID("lobby", 443), destination));

            context.SendPacket(0x8, 0xB, 0x0, ObjectManager.Instance.getObjectByID(443).GenerateSpawnBlob());

            //var objects = ObjectManager.Instance.getObjectsForZone("casino").Values;
            //foreach (var obj in objects)
            //{
            //    context.SendPacket(0x8, 0xB, 0x0, obj.GenerateSpawnBlob());
            //}



            context.SendPacket(new NoPayloadPacket(0x03, 0x2B));

        }

        private void SpawnObject(string[] args, int length, string full, Client client)
        {
            if(client == null)
            {
                var id = Helper.FindPlayerByUsername(args[1]);
                if (id == -1)
                    return;

                client = PolarisApp.Instance.Server.Clients[id];
            }
            else
            {
                string[] newargs = new string[args.Length + 1];
                newargs[0] = "";
                newargs[1] = "";
                Array.Copy(args, 1, newargs, 2, 9);
                args = newargs;
            }
            PSOObject obj = new PSOObject();
            obj.Name = args[2];
            obj.Header = new ObjectHeader((uint)Int32.Parse(args[3]), EntityType.Object);
            obj.Position = new PSOLocation(float.Parse(args[4]), float.Parse(args[5]), float.Parse(args[6]), float.Parse(args[7]), float.Parse(args[8]), float.Parse(args[9]), float.Parse(args[10]));
            obj.Things = new PSOObject.PSOObjectThing[0];

            client.SendPacket(0x8, 0xB, 0x0, obj.GenerateSpawnBlob());
        }

        private void RunLUA(string[] args, int length, string full, Client client)
        {
            if (client == null)
            {
                var id = Helper.FindPlayerByUsername(args[1]);
                if (id == -1)
                    return;

                client = PolarisApp.Instance.Server.Clients[id];
            }
            else
            {
                string[] newargs = new string[args.Length + 1];
                newargs[0] = "";
                newargs[1] = "";
                Array.Copy(args, 1, newargs, 2, args.Length - 1);
                args = newargs;
            }

            PacketWriter luaPacket = new PacketWriter();
            luaPacket.Write((UInt16)1);
            luaPacket.Write((UInt16)1);
            luaPacket.WriteAscii(String.Join(" ", args, 2, args.Length - 2), 0xD975, 0x2F);

            client.SendPacket(0x10, 0x3, 0x4, luaPacket.ToArray());
        }

        private void ImportNPCs(string[] args, int length, string full, Client client)
        {
            string zone = args[1];
            string folder = args[2];

            var packetList = Directory.GetFiles(folder);
            Array.Sort(packetList);

            List<NPC> newNPCs = new List<NPC>();
            foreach (var path in packetList)
            {
                var data = File.ReadAllBytes(path);
                PacketReader reader = new PacketReader(data);
                PacketHeader header = reader.ReadStruct<PacketHeader>();
                if (header.Type != 0x8 || header.Subtype != 0xC)
                {
                    Logger.WriteWarning("[WRN] File {0} not an NPC spawn packet, skipping.", path);
                    continue;
                }

                NPC newNPC = new NPC();
                newNPC.EntityID = (int)reader.ReadStruct<ObjectHeader>().ID;
                var pos = reader.ReadEntityPosition();
                newNPC.RotX = pos.RotX;
                newNPC.RotY = pos.RotY;
                newNPC.RotZ = pos.RotZ;
                newNPC.RotW = pos.RotW;

                newNPC.PosX = pos.PosX;
                newNPC.PosY = pos.PosY;
                newNPC.PosZ = pos.PosZ;
                reader.ReadInt16();
                newNPC.NPCName = reader.ReadFixedLengthAscii(0x20);
                newNPC.ZoneName = zone;
                newNPCs.Add(newNPC);
                Logger.WriteInternal("[NPC] Adding new NPC {0} to the database for zone {1}", newNPC.NPCName, zone);
            }

            using (var db = new PolarisEf())
            {
                db.NPCs.AddRange(newNPCs);
                db.SaveChanges();
            }
                
        }

        private void ImportObjects(string[] args, int length, string full, Client client)
        {
            string zone = args[1];
            string folder = args[2];

            var packetList = Directory.GetFiles(folder);
            Array.Sort(packetList);

            List<GameObject> newObjects = new List<GameObject>();
            foreach (var path in packetList)
            {
                var data = File.ReadAllBytes(path);
                PacketReader reader = new PacketReader(data);
                PacketHeader header = reader.ReadStruct<PacketHeader>();
                if (header.Type != 0x8 || header.Subtype != 0xB)
                {
                    Logger.WriteWarning("[WRN] File {0} not an Object spawn packet, skipping.", path);
                    continue;
                }

                GameObject newObj = new GameObject();
                newObj.ObjectID = (int)reader.ReadStruct<ObjectHeader>().ID;
                var pos = reader.ReadEntityPosition();
                newObj.RotX = pos.RotX;
                newObj.RotY = pos.RotY;
                newObj.RotZ = pos.RotZ;
                newObj.RotW = pos.RotW;
                   
                newObj.PosX = pos.PosX;
                newObj.PosY = pos.PosY;
                newObj.PosZ = pos.PosZ;
                reader.ReadInt16();
                newObj.ObjectName = reader.ReadFixedLengthAscii(0x2C);
                var objHeader = reader.ReadStruct<ObjectHeader>(); // Seems to always be blank...
                if (objHeader.ID != 0)
                    Logger.WriteWarning("[OBJ] It seems object {0} has a nonzero objHeader! ({1}) Investigate.", newObj.ObjectName, objHeader.ID);
                newObj.ZoneName = zone;
                var thingCount = reader.ReadUInt32();
                newObj.ObjectFlags = new byte[thingCount * 4];
                for (int i = 0; i < thingCount; i++)
                {
                    Buffer.BlockCopy(BitConverter.GetBytes(reader.ReadUInt32()), 0, newObj.ObjectFlags, i * 4, 4); // This should work
                }
                newObjects.Add(newObj);
                Logger.WriteInternal("[OBJ] Adding new Object {0} to the database for zone {1}", newObj.ObjectName, zone);
            }

            using (var db = new PolarisEf())
            {
                db.GameObjects.AddRange(newObjects);
                db.SaveChanges();
            }

        }

        private void TellPosition(string[] args, int length, string full, Client client)
        {
            if (client == null)
            {
                Logger.WriteError("[CMD] You need to be a client to run this command!");
            }

            Logger.WriteCommand(client, "[CMD] {0}", client.CurrentLocation.ToString());
        }

        #endregion
    }
}
