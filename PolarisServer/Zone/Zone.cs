using System;

namespace PolarisServer.Zone
{
    public class Zone
    {
        public enum ZoneType
        {
            Lobby,
            MyRoom,
            TeamRoom,
            StoryQuest,
            SinglePartyQuest,
            MultiPartyQuest,
            Other
        };

        public string Name { get; set; }

        private ZoneType Type { get; set; }
        public Zone(string name, ZoneType type)
        {
            this.Name = name;
            this.Type = type;
        }
    }
}

