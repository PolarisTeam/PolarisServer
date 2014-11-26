using System;
using System.Data.Entity;
using System.ComponentModel.DataAnnotations;

using MySql.Data.Entity;

using PolarisServer.Models;

namespace PolarisServer.Database
{

    public class ServerInfo
    {
        [Key]
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

        public PolarisEF() : base("server=localhost;database=polaris;username=polaris;password=polaris")
        {
            try
            {
                ServerInfo revision = this.ServerInfos.Find("Revision");
                if(revision == null)
                {
                    revision = new ServerInfo { key = "Revision", value = "0" };
                    this.ServerInfos.Add(revision);

                }
                this.SaveChanges();

                Logger.WriteInternal("[DB ] Loaded database with dataset revsision {0}", revision.value);
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
            
    }
}

