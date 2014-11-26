using System;
using System.Data.Entity;

using MySql.Data.Entity;
using System.ComponentModel.DataAnnotations;

namespace PolarisServer.Database
{

    public class Thing
    {
        [Key]
        public string key { get; set; }
        public string value { get; set; }
    }
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class PolarisEF : DbContext
    {
        public DbSet<Thing> Things { get; set; }
        public PolarisEF() : base("server=localhost;database=polaris;username=polaris;password=polaris")
        {
        }
    }
}

