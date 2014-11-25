using System;
using MySql.Data.MySqlClient;

using PolarisServer;
using PolarisServer.Models;

namespace PolarisServer.Database
{
    public class DBCharacter : DBObject
    {
        public static Character GetCharacter()
        {
            return null;
        }

        public override void CreateTables(MySqlConnection connection)
        {
            new MySqlCommand("CREATE TABLE IF NOT EXISTS Characters(CharacterID INTEGER NOT NULL, PlayerID INTEGER NOT NULL, " +
                "Name TEXT, JobParam BLOB, LooksParam BLOB, PRIMARY KEY(CharacterID), FOREIGN KEY (PlayerID) REFRENCES Players(PlayerID))", connection).ExecuteNonQuery();
        }

    }
}

