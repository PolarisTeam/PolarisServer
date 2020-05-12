using System.Collections.Generic;

namespace PolarisServer.Zone
{
    public class ZoneManager
    {
        private static readonly ZoneManager instance = new ZoneManager();

        internal Dictionary<string, List<Map>> instances = new Dictionary<string, List<Map>>();

        internal Dictionary<string, int> playerCounter = new Dictionary<string, int>();

        private ZoneManager()
        {
            // Create lobby instance
            List<Map> lobbyMaps = new List<Map>(){ new Map("lobby", 106, 0, Map.MapType.Lobby, Map.MapFlags.None),
                new Map("casino", 104, 0, Map.MapType.Casino, Map.MapFlags.MultiPartyArea | Map.MapFlags.Unknown1) };

            instances.Add("lobby", lobbyMaps);
        }

        public static ZoneManager Instance
        {
            get
            {
                return instance;
            }
        }

        public Map MapFromInstance(string mapName, string instanceName)
        {
            if (!instances.ContainsKey(instanceName))
                throw new KeyNotFoundException();

            Map dstMap = null;
            foreach (Map m in instances[instanceName])
            {
                if (m.Name == mapName)
                    return m;
            }

            return dstMap;
        }

        public void NewInstance(string instanceName, Map initialMap)
        {
            if (instances.ContainsKey(instanceName))
            {
                return;
            }
            initialMap.InstanceName = instanceName;
            instances.Add(instanceName, new List<Map>() { initialMap });
            playerCounter.Add(instanceName, 0);
        }

        public bool InstanceExists(string instanceName)
        {
            return instances.ContainsKey(instanceName);
        }

        public void AddMapToInstance(string instance, Map m)
        {
            List<Map> maps = instances[instance];
            if (!maps.Contains(m))
                maps.Add(m);
        }
    }
}