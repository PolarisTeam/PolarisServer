using System;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using MySql.Data.Entity;

namespace PolarisServer.Database
{

    public class Thing
    {
        [Key]
        public string key { get; set; }
        public object value { get; set; }
    }
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class PolarisEF : DbContext
    {
        public DbSet<Thing> Things { get; set; }
        public PolarisEF() : base("username=polaris;server=localhost;database=polaris;password=polaris")
        {
        }
    }
}

