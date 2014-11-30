using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Data.Entity;

using PolarisServer.Database;

namespace PolarisServer
{
    class PolarisApp
    {

        private static PolarisApp _Instance;
        public static PolarisApp Instance { get { return _Instance; } }
        private PolarisEF _Database;
		public static IPAddress BindAddress = IPAddress.Parse("127.0.0.1");

		public static void Main(string[] args)
		{
			try
			{
                throw new Exception("Test");

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
					}
				}
			}
			catch (Exception ex)
			{
                Logger.WriteException("An error has occurred while parsing command line parameters", ex);
			}

            Console.WriteLine("Arf. Polaris Server version GIT.\nCreated by PolarisTeam (http://github.com/PolarisTeam) and licenced under AGPL.");
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
            {
                new QueryServer(QueryMode.ShipList, 12099 + (100 * i));
            }
            new Server().Run();
        }

        public PolarisEF Database { get { return _Database; } }

    }
}
