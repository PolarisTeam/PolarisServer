using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.IO;
using MySql.Data.Entity;
using PolarisServer.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace PolarisServer.Database
{
    public class ServerInfo
    {
        [Key, MaxLength(255)]
        public string Info { get; set; }

        public string Setting { get; set; }
    }

    public class Teleport
    {
        [Key, Column(Order = 1)]
        public string ZoneName { get; set; }

        [Key, Column(Order = 2)]
        public int ObjectID { get; set; }

        public float RotX { get; set; }
        public float RotY { get; set; }
        public float RotZ { get; set; }
        public float RotW { get; set; }

        public float PosX { get; set; }
        public float PosY { get; set; }
        public float PosZ { get; set; }
    }

    public class Player
    {
        [Key]
        public int PlayerId { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public string SettingsIni { get; set; }
    }

    [DbConfigurationType(typeof (MySqlEFConfiguration))]
    public class PolarisEf : DbContext
    {
        public PolarisEf()
            : base(
                string.Format("server={0};database={1};username={2};password={3}", PolarisApp.Config.DatabaseAddress,
                    PolarisApp.Config.DatabaseName, PolarisApp.Config.DatabaseUsername,
                    PolarisApp.Config.DatabasePassword))
        {
           
        }

        public void SetupDB()
        {
            try
            {
                foreach (
                    var f in
                        Directory.EnumerateFiles(Directory.GetCurrentDirectory() + "/Resources/sql/scripts/", "*.sql"))
                {
                    Logger.WriteInternal("[DB ] Executing database script {0}", f);
                    Database.ExecuteSqlCommand(File.ReadAllText(f));
                }
                var revision = ServerInfos.Find("Revision");
                if (revision == null)
                {
                    revision = new ServerInfo { Info = "Revision", Setting = "0" };
                    ServerInfos.Add(revision);

                    //TODO Possibly move this somewhere else?
                    Database.ExecuteSqlCommand("ALTER TABLE Players AUTO_INCREMENT=10000000");
                }
                SaveChanges();

                Logger.WriteInternal("[DB ] Loaded database with dataset revision {0}", revision.Setting);
            }
            catch (Exception ex)
            {
                Logger.WriteException("A database exception has occured", ex);
            }
        }

        public DbSet<ServerInfo> ServerInfos { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Character> Characters { get; set; }
        public DbSet<Teleport> Teleports { get; set; }
    }
}