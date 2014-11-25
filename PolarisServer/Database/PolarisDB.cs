using System;
using MySql.Data.MySqlClient;

namespace PolarisServer.Database
{
    public class PolarisDB
    {
        private MySqlConnection connection;

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
                PolarisServer.Logger.WriteInternal("[---] Connected to MySQL server with version {0}", connection.ServerVersion);
            }
            catch(MySqlException ex)
            {
                PolarisServer.Logger.WriteError("[ERR] A MySQL Error has occured! {0}", ex.GetType());
            }


        }

    }
}

