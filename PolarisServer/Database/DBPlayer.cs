using System;
using MySql.Data.MySqlClient;

namespace PolarisServer.Database
{
    public class DBPlayer : DBObject
    {
        public DBPlayer()
        {
        }

        public override void CreateTables(MySqlConnection connection)
        {
            new MySqlCommand("CREATE TABLE IF NOT EXISTS Players(PlayerID INTEGER NOT NULL, LoginID TEXT NOT NULL, " +
                "Password TEXT NOT NULL, Nickname TEXT NOT NULL, Prefrences BLOB, PRIMARY KEY(PlayerID))", connection).ExecuteNonQuery();
        }
    }
}

