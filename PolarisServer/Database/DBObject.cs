using System;
using MySql.Data.MySqlClient;

namespace PolarisServer.Database
{
    public abstract class DBObject
    {
        public DBObject()
        {
        }

        public abstract void CreateTables(MySqlConnection connection);
    }
}

