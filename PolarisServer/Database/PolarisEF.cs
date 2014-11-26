using System;
using System.Data.Entity;

using MySql.Data.Entity;

namespace PolarisServer.Database
{

    public class Thing
    {
        public string key { get; set; }
        public object value { get; set; }
    }
    [DbConfigurationType(typeof(MySqlEFConfiguration))]
    public class PolarisEF : DbContext
    {
        public DbSet<Thing> Things { get; set; }
        public PolarisEF() : base("server=localhost;database=polaris;password=polaris")
        {
        }
    }
}

