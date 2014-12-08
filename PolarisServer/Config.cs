using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;

namespace PolarisServer
{
    public class ConfigComment : Attribute
    {
        public string comment;

        public ConfigComment(string comment)
        {
            this.comment = comment;
        }
    }

    public class Config
    {
        private string configFile = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + Path.DirectorySeparatorChar + "PolarisServer.cfg";

        // Settings
        [ConfigComment("The address to bind to")]
        public IPAddress BindAddress = IPAddress.Loopback;

        [ConfigComment("Log the data sent and recieved from packets")]
        public bool VerbosePackets = false;

        [ConfigComment("Time in seconds to perform a ping of all connected clients to the server")]
        public double PingTime = 60;

        [ConfigComment("The prefix to check for to send a command from the client to the server")]
        public string CommandPrefix = "|";

        [ConfigComment("Name of the database which contains the Polaris data")]
        public string DatabaseName = "polaris";
        [ConfigComment("Address of the database server")]
        public string DatabaseAddress = "127.0.0.1";
        [ConfigComment("Username for logging into the database server")]
        public string DatabaseUsername = "polaris";
        [ConfigComment("Password for logging into the database server")]
        public string DatabasePassword = "polaris";

        public void Load()
        {
            try
            {
                // No config exists, save a default one
                if (!File.Exists(configFile))
                {
                    Save(true);
                    return;
                }

                FieldInfo[] fields = this.GetType().GetFields();
                string[] lines = File.ReadAllLines(configFile);

                foreach (string option in lines)
                {
                    // Blank Line
                    if (option.Length == 0)
                        continue;

                    // Comment
                    if (option.StartsWith("//"))
                        continue;

                    string[] split = option.Split('=');

                    // Trim trailing/leading space
                    for (int i = 0; i < split.Length; i++)
                        split[i] = split[i].Trim();

                    // Check length
                    if (split.Length != 2)
                    {
                        Logger.WriteWarning("[CFG] Config line found with improper split size");
                        continue;
                    }

                    FieldInfo field = fields.FirstOrDefault(o => o.Name == split[0]);
                    if (field != null)
                        ParseField(field, split[1]);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteException("Error loading configuration", ex);
            }

            // Some settings require manual refreshing
            SettingsChanged();

            Logger.WriteInternal("[CFG] Configuration loaded");
        }

        public void Save(bool silent = false)
        {
            try
            {
                List<string> data = new List<string>();
                FieldInfo[] fields = this.GetType().GetFields();

                foreach (FieldInfo field in fields)
                    SaveField(field, data);

                File.WriteAllLines(configFile, data);
            }
            catch (Exception ex)
            {
                Logger.WriteException("Error saving configuration", ex);
            }

            if (!silent)
                Logger.WriteInternal("[CFG] Configuration saved");
        }

        public void SettingsChanged()
        {
            PolarisApp.BindAddress = BindAddress;
            Logger.VerbosePackets = VerbosePackets;
            PolarisApp.Instance.server.pingTimer.Interval = 1000 * PingTime;
        }

        public bool SetField(string name, string value)
        {
            FieldInfo[] fields = this.GetType().GetFields();
            FieldInfo field = fields.FirstOrDefault(o => o.Name == name);

            if (field != null)
            {
                ParseField(field, value);
                return true;
            }
            else
                return false;
        }

        private void ParseField(FieldInfo field, string value)
        {
            // Bool
            if (field.GetValue(this).GetType() == typeof(bool))
                field.SetValue(this, bool.Parse(value));

            // Int32
            if (field.GetValue(this).GetType() == typeof(Int32))
                field.SetValue(this, int.Parse(value));

            // Float
            if (field.GetValue(this).GetType() == typeof(float))
                field.SetValue(this, float.Parse(value));

            // Double
            if (field.GetValue(this).GetType() == typeof(double))
                field.SetValue(this, double.Parse(value));

            // String
            if (field.GetValue(this).GetType() == typeof(string))
                field.SetValue(this, value);
            
            // IPAddress
            if (field.GetValue(this).GetType() == typeof(IPAddress))
                field.SetValue(this, IPAddress.Parse(value));

            // Add more handling for special/custom types as needed
        }

        private void SaveField(FieldInfo field, List<string> data)
        {
            // Comment
            Attribute[] attributes = (Attribute[])field.GetCustomAttributes(typeof(ConfigComment), false);
            if (attributes.Length > 0)
            {
                ConfigComment commentAttr = (ConfigComment)attributes[0];
                data.Add("// " + commentAttr.comment);
            }

            // IP Address
            if (field.GetValue(this).GetType() == typeof(IPAddress))
            {
                IPAddress address = (IPAddress)field.GetValue(this);
                data.Add(field.Name + " = " + address.ToString());
            }
            else // Basic field
                data.Add(field.Name + " = " + field.GetValue(this));

            data.Add(string.Empty); // Leave a blank line between options

            // Add more handling for special/custom types as needed
        }
    }
}
