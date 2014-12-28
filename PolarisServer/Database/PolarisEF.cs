using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

using MySql.Data.Entity;

using PolarisServer.Models;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.Infrastructure;
using System.ComponentModel;

namespace PolarisServer.Database
{
    public class ServerInfo
    {
        [Key, MaxLength(255)]
        public string key { get; set; }

        public string value { get; set; }
    }

    public class Player
    {
        [Key]
        public int PlayerID { get; set; }

        public string Username { get; set; }
        public string Password { get; set; }
        public string Nickname { get; set; }
        public string SettingsINI { get; set; }
    }

    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class PolarisEF : DbContext
    {
        public DbSet<ServerInfo> ServerInfos { get; set; }
        public DbSet<Player> Players { get; set; }
        public DbSet<Character> Characters { get; set; }

        public PolarisEF()
            : base(string.Format("server={0};database={1};username={2};password={3}", PolarisApp.Config.DatabaseAddress, PolarisApp.Config.DatabaseName, PolarisApp.Config.DatabaseUsername, PolarisApp.Config.DatabasePassword))
        {
            try
            {
                this.Database.CreateIfNotExists();
                ServerInfo revision = this.ServerInfos.Find("Revision");
                if (revision == null)
                {
                    revision = new ServerInfo { key = "Revision", value = "0" };
                    this.ServerInfos.Add(revision);

                    //TODO Possibly move this somewhere else?
                    this.Database.ExecuteSqlCommand("ALTER TABLE Players AUTO_INCREMENT=10000000");
                }
                this.SaveChanges();

                Logger.WriteInternal("[DB ] Loaded database with dataset revision {0}", revision.value);
            }
            catch (Exception ex)
            {
                Logger.WriteException("A database exception has occured", ex);
            }
        }
    }
}
