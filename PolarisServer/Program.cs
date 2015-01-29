using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using PolarisServer.Database;
using PolarisServer.Packets.Handlers;

namespace PolarisServer
{
    internal class PolarisApp
    {
        // Will be using these around the app later [KeyPhact]
        public const string POLARIS_NAME = "Polaris Server";
        public const string POLARIS_SHORT_NAME = "Polaris";
        public const string POLARIS_AUTHOR = "PolarisTeam (http://github.com/PolarisTeam)";
        public const string POLARIS_COPYRIGHT = "(C) 2014 PolarisTeam.";
        public const string POLARIS_LICENSE = "All licenced under AGPL.";
        public const string POLARIS_VERSION = "v0.1.0-pre";
        public const string POLARIS_VERSION_NAME = "Corsac Fox";
        public static IPAddress BindAddress = IPAddress.Parse("127.0.0.1");
        public static Config Config;
        public static ConsoleSystem ConsoleSystem;
        public List<QueryServer> queryServers = new List<QueryServer>();
        public Server server;
        public static PolarisApp Instance { get; private set; }
        public PolarisEF Database { get; private set; }

        public static void Main(string[] args)
        {
            Config = new Config();

            ConsoleSystem = new ConsoleSystem();
            ConsoleSystem.thread = new Thread(ConsoleSystem.StartThread);
            ConsoleSystem.thread.Start();

            // Setup function exit handlers to guarentee Exit() is run before closing
            Console.CancelKeyPress += Exit;
            AppDomain.CurrentDomain.ProcessExit += Exit;

            try
            {
                for (var i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
                        default:
                            break;

                        case "-b":
                        case "--bind-address":
                            if (++i < args.Length)
                                BindAddress = IPAddress.Parse(args[i]);
                            break;

                        case "-s":
                        case "--size":
                            var splitArgs = args[++i].Split(',');
                            var width = int.Parse(splitArgs[0]);
                            var height = int.Parse(splitArgs[1]);
                            if (width < ConsoleSystem.Width)
                            {
                                Logger.WriteWarning("[ARG] Capping console width to {0} columns", ConsoleSystem.Width);
                                width = ConsoleSystem.Width;
                            }
                            if (height < ConsoleSystem.Height)
                            {
                                Logger.WriteWarning("[ARG] Capping console height to {0} rows", ConsoleSystem.Height);
                                height = ConsoleSystem.Height;
                            }
                            ConsoleSystem.SetSize(width, height);
                            break;
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException("An error has occurred while parsing command line parameters", ex);
            }

            // Check for settings.txt [AIDA]
            if (!File.Exists("Resources/settings.txt"))
            {
                // If it doesn't exist, throw an error and quit [AIDA]
                Logger.WriteError("[ERR] Failed to load settings.txt. Press any key to quit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // Check for Private Key BLOB [AIDA]
            if (!File.Exists("privateKey.blob"))
            {
                // If it doesn't exist, throw an error and quit [AIDA]
                Logger.WriteError("[ERR] Failed to load privateKey.blob. Press any key to quit.");
                Console.ReadKey();
                Environment.Exit(0);
            }

            // Fix up startup message [KeyPhact]
            Logger.Write(POLARIS_NAME + " - " + POLARIS_VERSION + " (" + POLARIS_VERSION_NAME + ")");
            Logger.Write("By " + POLARIS_AUTHOR);
            Logger.Write(POLARIS_LICENSE);

            Thread.Sleep(1000);
            //System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseIfModelChanges<PolarisEF>());
            Instance = new PolarisApp();
            Instance.Start();
        }

        public void Start()
        {
            Logger.WriteInternal("Server starting at " + DateTime.Now);

            server = new Server();

            Config.Load();

            PacketHandlers.LoadPacketHandlers();

            Logger.WriteInternal("[DB ] Loading database...");
            Database = new PolarisEF();

            for (var i = 0; i < 10; i++)
                queryServers.Add(new QueryServer(QueryMode.ShipList, 12099 + (100*i)));

            server.Run();
        }

        private static void Exit(object sender, EventArgs e)
        {
            // Save the configuration
            Config.Save();

            // Save the database
            if (Instance != null && Instance.Database != null)
                Instance.Database.SaveChanges();
        }
    }
}