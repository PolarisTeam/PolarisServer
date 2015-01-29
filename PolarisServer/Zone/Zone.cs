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

        public Zone(string name, ZoneType type)
        {
            Name = name;
            Type = type;
        }

        public string Name { get; set; }
        private ZoneType Type { get; set; }
    }
}