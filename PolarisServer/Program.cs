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
        private PolarisDB _Database;

        public static void Main(string[] args)
        {

            Console.WriteLine("Arf. Polaris Server version GIT.\nCreated by PolarisTeam (http://github.com/PolarisTeam) and licenced under AGPL.");
            System.Data.Entity.Database.SetInitializer(new DropCreateDatabaseIfModelChanges<PolarisEF>());
            using (var database = new PolarisEF())
            {
                try
                {
                    database.Database.CreateIfNotExists();


                    if(database.Things.Find("Revision") != null)
                        database.Things.Remove(database.Things.Find("Revision"));
                    database.Things.Add(new Thing {key = "Revision", value = "0"});

                    database.SaveChanges();
                }
                catch (Exception ex)
                {
                    Logger.WriteError("[ERR] DB Expection occured! {0}: {1}", ex.GetType(), ex.ToString());
                    if (ex.InnerException != null)
                    {
                        Logger.WriteError("[ERR] Inner exception occured! {0}: {1}", ex.InnerException.GetType(), ex.InnerException.ToString());
                    }

                }
            }
            _Instance = new PolarisApp();
            _Instance.Start();

        }

        public void Start()
        {
            Logger.WriteInternal("Server starting at " + DateTime.Now.ToString());
            Packets.Handlers.PacketHandlers.loadPacketHandlers();
            //_Database = new PolarisDB();
            for (int i = 0; i < 10; i++)
            {
                new QueryServer(QueryMode.ShipList, 12099 + (100 * i));
            }
            new Server().Run();
        }

        public PolarisDB Database { get { return _Database; } }

    }
}
