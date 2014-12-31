using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;

using PolarisServer.Database;
using PolarisServer.Packets.Handlers;

namespace PolarisServer
{
    class PolarisApp
    {
        private static PolarisApp instance;
        public static PolarisApp Instance { get { return instance; } }

        public PolarisEF Database { get { return database; } }
        private PolarisEF database;

        public static IPAddress BindAddress = IPAddress.Parse("127.0.0.1");
        public List<QueryServer> queryServers = new List<QueryServer>();

        public Server server;

        public static Config Config;
        public static ConsoleSystem ConsoleSystem;

        public static void Main(string[] args)
        {
            Config = new Config();

            ConsoleSystem = new ConsoleSystem();
            ConsoleSystem.thread = new Thread(new ThreadStart(ConsoleSystem.StartThread));
            ConsoleSystem.thread.Start();

            // Setup function exit handlers to guarentee Exit() is run before closing
            Console.CancelKeyPress += Exit;
            AppDomain.CurrentDomain.ProcessExit += Exit;

            try
            {
                for (int i = 0; i < args.Length; i++)
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
                            string[] splitArgs = args[++i].Split(',');
                            int width = int.Parse(splitArgs[0]);
                            int height = int.Parse(splitArgs[1]);
                            if (width < ConsoleSystem.width)
                            {
                                Logger.WriteWarning("[ARG] Capping console width to {0} columns", ConsoleSystem.width);
                                width = ConsoleSystem.width;
                            }
                            if (height < ConsoleSystem.height)
                            {
                                Logger.WriteWarning("[ARG] Capping console height to {0} rows", ConsoleSystem.height);
                                height = ConsoleSystem.height;
                            }
                            ConsoleSystem.width = width;
                            ConsoleSystem.height = height;
                            Console.SetWindowSize(ConsoleSystem.width, ConsoleSystem.height);
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

            Logger.Write("Polaris Server v0.1.0-pre \"Corsac Fox\".\nCreated by PolarisTeam (http://github.com/PolarisTeam) and licenced under AGPL."); // also, arf.
            Thread.Sleep(1000);
            //System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseIfModelChanges<PolarisEF>());
            instance = new PolarisApp();
            instance.Start();
        }

        public void Start()
        {
            Logger.WriteInternal("Server starting at " + DateTime.Now.ToString());

            server = new Server();

            Config.Load();

            PacketHandlers.LoadPacketHandlers();

            Logger.WriteInternal("[DB ] Loading database...");
            database = new PolarisEF();

            for (int i = 0; i < 10; i++)
                queryServers.Add(new QueryServer(QueryMode.ShipList, 12099 + (100 * i)));

            server.Run();
        }

        static void Exit(object sender, EventArgs e)
        {
            // Save the configuration
            Config.Save();

            // Save the database
            if (instance != null && instance.database != null)
                instance.database.SaveChanges();
        }
    }
}
