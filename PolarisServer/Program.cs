using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using System.Security.Cryptography;
using PolarisServer.Database;
using PolarisServer.Packets.Handlers;

namespace PolarisServer
{
    internal class PolarisApp
    {
        // Will be using these around the app later [KeyPhact]
        public const string PolarisName = "Polaris Server";
        public const string PolarisShortName = "Polaris";
        public const string PolarisAuthor = "PolarisTeam (http://github.com/PolarisTeam)";
        public const string PolarisCopyright = "(C) 2014 PolarisTeam.";
        public const string PolarisLicense = "All licenced under AGPL.";
        public const string PolarisVersion = "v0.1.0-pre";
        public const string PolarisVersionName = "Corsac Fox";
        public static IPAddress BindAddress = IPAddress.Parse("127.0.0.1");
        public static Config Config;
        public static ConsoleSystem ConsoleSystem;
        public List<QueryServer> QueryServers = new List<QueryServer>();
        public Server Server;
        public static PolarisApp Instance { get; private set; }
        public PolarisEf Database { get; private set; }

        public static void Main(string[] args)
        {
            Config = new Config();

            ConsoleSystem = new ConsoleSystem {Thread = new Thread(ConsoleSystem.StartThread)};
            ConsoleSystem.Thread.Start();

            // Setup function exit handlers to guarentee Exit() is run before closing
            Console.CancelKeyPress += Exit;
            AppDomain.CurrentDomain.ProcessExit += Exit;

            try
            {
                for (var i = 0; i < args.Length; i++)
                {
                    switch (args[i].ToLower())
                    {
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
                // If it doesn't exist, generate a fresh keypair [CK]
                Logger.WriteWarning("[WRN] No privatekey.blob installed, generating new keypair...");
                RSACryptoServiceProvider rcsp = new RSACryptoServiceProvider();
                byte[] cspBlob = rcsp.ExportCspBlob(true);
                byte[] cspBlobPub = rcsp.ExportCspBlob(false);
                FileStream outFile = File.Create("privateKey.blob");
                FileStream outFilePub = File.Create("publicKey.blob");
                outFile.Write(cspBlob, 0, cspBlob.Length);
                outFile.Close();
                outFilePub.Write(cspBlobPub, 0, cspBlobPub.Length);
                outFilePub.Close();
            }

            // Fix up startup message [KeyPhact]
            Logger.Write(PolarisName + " - " + PolarisVersion + " (" + PolarisVersionName + ")");
            Logger.Write("By " + PolarisAuthor);
            Logger.Write(PolarisLicense);

            Thread.Sleep(1000);
            //System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseIfModelChanges<PolarisEF>());
            Instance = new PolarisApp();
            Instance.Start();
        }

        public void Start()
        {
            Logger.WriteInternal("Server starting at " + DateTime.Now);

            Server = new Server();

            Config.Load();

            PacketHandlers.LoadPacketHandlers();

            Logger.WriteInternal("[DB ] Loading database...");
            Database = new PolarisEf();

            for (var i = 0; i < 10; i++)
                QueryServers.Add(new QueryServer(QueryMode.ShipList, 12099 + (100*i)));

            Server.Run();
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