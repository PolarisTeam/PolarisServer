using System;
using MySql.Data.MySqlClient;
using System.Collections.Generic;

using PolarisServer.Models;

namespace PolarisServer.Database
{
    public class PolarisDB
    {
        private MySqlConnection connection;
        private string dbRevision;


        public PolarisDB()
        {
            ConnectToDB();
        }

        private void ConnectToDB()
        {
            if (connection != null)
                return;
            try
            {
                //TODO Grab these values from a config file.
                connection = new MySqlConnection("server=localhost;userid=polaris;password=polaris;database=polaris");
                connection.Open();

                GenerateTables();

                PolarisServer.Logger.WriteInternal("[---] Connected to MySQL server with version {0}", connection.ServerVersion);

                object revObj = GetServerInfo("Revision");

                if (revObj == null)
                {
                    revObj = 0;
                    AddServerinfo("Revision", revObj);
                }

                dbRevision = Convert.ToString(revObj);

                PolarisServer.Logger.WriteInternal("[---] MySQL server has base data revision {0} loaded.", dbRevision);
            }
            catch (MySqlException ex)
            {
                PolarisServer.Logger.WriteError("[ERR] A MySQL Error has occured! {0}: {1}", ex.GetType(), ex.ToString());
            }
        }

        private void GenerateTables()
        {
            new MySqlCommand("CREATE TABLE IF NOT EXISTS ServerInfo(name TEXT NOT NULL PRIMARY KEY, data BLOB)", connection).ExecuteNonQuery();
            new MySqlCommand("CREATE TABLE IF NOT EXISTS Players(PlayerID INTEGER NOT NULL, LoginID TEXT NOT NULL, " +
                "Password TEXT NOT NULL, Nickname TEXT NOT NULL, Prefrences BLOB, PRIMARY KEY(PlayerID))", connection).ExecuteNonQuery();
            new MySqlCommand("CREATE TABLE IF NOT EXISTS Characters(CharacterID INTEGER NOT NULL, PlayerID INTEGER NOT NULL, " +
                "Name TEXT, JobParam BLOB, LooksParam BLOB, PRIMARY KEY(CharacterID), FOREIGN KEY (PlayerID) REFRENCES Players(PlayerID))", connection).ExecuteNonQuery();
        }

        public void AddServerinfo(string name, object value)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand("REPLACE INTO ServerInfo(name, data) VALUES(@name, @data)", connection);
                cmd.Parameters.AddWithValue("name", name);
                cmd.Parameters.AddWithValue("data", value);

                cmd.ExecuteNonQuery();
            }
            catch (MySqlException ex)
            {
                PolarisServer.Logger.WriteError("[ERR] A MySQL Error has occured! {0}: {1}", ex.GetType(), ex.ToString());
            }
        }

        public object GetServerInfo(string name)
        {
            try
            {
                MySqlCommand cmd = new MySqlCommand("SELECT data FROM ServerInfo WHERE name = @name", connection);
                cmd.Parameters.AddWithValue("name", name);

                return cmd.ExecuteScalar();
            }
            catch (MySqlException ex)
            {
                PolarisServer.Logger.WriteError("[ERR] A MySQL Error has occured! {0}: {1}", ex.GetType(), ex.ToString());
            }

            return null;
        }

        public Character[] GetCharacters(int PlayerID)
        {
            List<Character> characters = new List<Character>();
            MySqlCommand cmd = new MySqlCommand("SELECT * FROM Characters WHERE PlayerID = @pid", connection);
            cmd.Parameters.AddWithValue("pid", PlayerID);

            var Reader = cmd.ExecuteReader();

            while (Reader.Read())
            {
                var character = new Character();
                character.CharacterId = Convert.ToUInt32(Reader["CharacterID"]);
                character.Name = Convert.ToString(Reader["Name"]);

                //Character.Job = (Character.JobParam)["JobParam"];
                //Character.Looks = (Character.LooksParam)Reader["LooksParam"];

                characters.Add(character);
            }

            return characters.ToArray();
        }
    }
}

