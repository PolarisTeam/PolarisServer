using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Net;
using System.Net.Sockets;
using System.Threading;

using PolarisServer.Database;

namespace PolarisServer
{
    class PolarisApp
    {
        private static PolarisApp _Instance;
        public static PolarisApp Instance { get { return _Instance; } }
        public PolarisEF Database { get { return _Database; } }
        private PolarisEF _Database;
        public static IPAddress BindAddress = IPAddress.Parse("127.0.0.1");
        public List<QueryServer> queryServers = new List<QueryServer>();
        public Server server;

        // Console System
        public static ConsoleSystem ConsoleSystem;
        public static Thread ConsoleDrawThread;
        public static Thread ConsoleInputThread;

        public static void Main(string[] args)
        {
            ConsoleSystem = new ConsoleSystem();
            ConsoleDrawThread = new Thread(new ThreadStart(ConsoleSystem.StartDrawing));
            ConsoleInputThread = new Thread(new ThreadStart(ConsoleSystem.StartInput));
            ConsoleDrawThread.Name = "Draw";
            ConsoleDrawThread.Start();
            ConsoleInputThread.Name = "Input";
            ConsoleInputThread.Start();
            
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
                for (int i = 0; i < args.Length; i++)
                    Logger.WriteError("[CMD] {0} -> {1}", i, args[i].ToString());
                Logger.WriteException("An error has occurred while parsing command line parameters", ex);
            }

            Logger.Write("Arf. Polaris Server version GIT.\nCreated by PolarisTeam (http://github.com/PolarisTeam) and licenced under AGPL.");
            System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseIfModelChanges<PolarisEF>());
            _Instance = new PolarisApp();
            _Instance.Start();
        }

        public void Start()
        {
            Logger.WriteInternal("Server starting at " + DateTime.Now.ToString());
            
            Packets.Handlers.PacketHandlers.LoadPacketHandlers();
            
            Logger.WriteInternal("[DB ] Loading database...");
            _Database = new PolarisEF();
            
            for (int i = 0; i < 10; i++)
                queryServers.Add(new QueryServer(QueryMode.ShipList, 12099 + (100 * i)));

            server = new Server();
            server.Run();
        }
    }
}
