using System;
using System.Collections.Generic;
using System.IO;

using PolarisServer.Models;
using PolarisServer.Object;
using PolarisServer.Packets;
using PolarisServer.Packets.PSOPackets;

namespace PolarisServer.Zone
{
    public class Map
    {
        public string Name { get; set; }
        public MapType Type { get; set; }
        public PSOObject[] Objects { get; set; }
        public PSONPC[] NPCs { get; set; }
        public ObjectHeader MapHeader { get; set; }

        public int MapID { get; set; }
        public int VariantID { get; set; }

        public MapFlags Flags { get; set; }

        public List<Client> Clients { get; set; }

        public GenParam GenerationArgs { get; set; }

        public string InstanceName { get; set; }

        public enum MapType : int
        {
            Lobby = 4,
            Casino = 11,
            MyRoom = 8,
            TeamRoom = 7,
            ChallengeLobby = 5,
            Campship = 1,
            Quest = 0,
            Other = ~0
        };

        [Flags]
        public enum MapFlags
        {
            None = 0,
            MultiPartyArea = 1,
            Unknown1 = 2,
            EnableMap = 4
        }

        public Map(string name, int id, int variant, MapType type, MapFlags flags)
        {
            Name = name;
            MapID = id;
            VariantID = variant;
            Type = type;
            Flags = flags;
            Clients = new List<Client>();
            GenerationArgs = new GenParam();

            Objects = ObjectManager.Instance.GetObjectsForZone(Name);
            NPCs = ObjectManager.Instance.getNPCSForZone(Name);
        }

        public PSOLocation GetDefaultLocation()
        {
            PSOLocation location;

            switch (Type)
            {
                case MapType.Lobby:
                    location = new PSOLocation(0f, 1f, 0f, 0f, -0.417969f, 0f, 137.375f);
                    break;

                case MapType.Casino:
                    location = new PSOLocation(0, 1f, 0, 0, 2, 6, 102);
                    break;

                default:
                    location = new PSOLocation(0f, 1f, 0f, 0f, 0f, 0f, 0f);
                    break;
            }

            return location;
        }

        /// <summary>
        /// Spawns a client into a map at a given location
        /// </summary>
        /// <param name="c">Client to spawn into map</param>
        /// <param name="location">Location to spawn client at</param>
        /// <param name="questOveride">If this also sets the quest data, specify the quest name here to load the spawn from the bin rather then generate it.</param>
        public void SpawnClient(Client c, PSOLocation location, string questOveride = "")
        {
            if (Clients.Contains(c))
            {
                return;
            }

            // Set area
            if (questOveride != "") // TODO: This is a temporary hack, fix me!!
            {
                var setAreaPacket = File.ReadAllBytes("Resources/quests/" + questOveride + ".bin");
                c.SendPacket(0x03, 0x24, 4, setAreaPacket);
            }
            else
            {
                PacketWriter writer = new PacketWriter();
                writer.WriteStruct(new ObjectHeader(1, EntityType.Map));
                writer.WriteStruct(new ObjectHeader((uint)c.User.PlayerId, EntityType.Player));
                writer.Write(0x34f9); // 8 Zeros
                writer.Write(0); // 8 Zeros
                writer.Write(~(uint)Type); // F4 FF FF FF
                writer.Write(MapID); // Map ID maybe
                writer.Write((uint)Flags);
                writer.Write(GenerationArgs.seed); // 81 8F E6 19 (Maybe seed)
                writer.Write(VariantID); // Randomgen enable / disable maybe
                writer.Write(GenerationArgs.xsize); // X Size
                writer.Write(GenerationArgs.ysize); // Y Size
                writer.Write(0);
                writer.Write(0);
                writer.Write(~0); // FF FF FF FF FF FF FF FF
                writer.Write(0x0c01);

                c.SendPacket(0x3, 0x0, 0x0, writer.ToArray());
            }

            if (c.CurrentZone != null)
            {
                c.CurrentZone.RemoveClient(c);
            }

            var setPlayerId = new PacketWriter();
            setPlayerId.WritePlayerHeader((uint)c.User.PlayerId);
            c.SendPacket(0x06, 0x00, 0, setPlayerId.ToArray());

            // Spawn Character
            c.SendPacket(new CharacterSpawnPacket(c.Character, location));
            c.CurrentLocation = location;
            c.CurrentZone = this;

            // Objects
            foreach (PSOObject obj in Objects)
            {
                c.SendPacket(0x08, 0x0B, 0x0, obj.GenerateSpawnBlob());
            }

            // NPCs
            foreach (PSONPC npc in NPCs)
            {
                c.SendPacket(0x08, 0xC, 0x4, npc.GenerateSpawnBlob());
            }

            // Spawn for others, Spawn others for me
            CharacterSpawnPacket spawnMe = new CharacterSpawnPacket(c.Character, location, false);
            foreach (Client other in Clients)
            {
                other.SendPacket(spawnMe);
                c.SendPacket(new CharacterSpawnPacket(other.Character, other.CurrentLocation, false));
            }

            // Unlock Controls
            c.SendPacket(new NoPayloadPacket(0x03, 0x2B)); // Inital spawn only, move this!

            Clients.Add(c);

            Logger.Write("[MAP] {0} has spawned in {1}.", c.User.Username, Name);

            if (InstanceName != null && ZoneManager.Instance.playerCounter.ContainsKey(InstanceName))
            {
                ZoneManager.Instance.playerCounter[InstanceName] += 1;
            }

        }

        public void RemoveClient(Client c)
        {
            if (!Clients.Contains(c))
                return;

            c.CurrentZone = null;
            Clients.Remove(c);

            foreach (Client other in Clients)
            {
                PacketWriter writer = new PacketWriter();
                writer.WriteStruct(new ObjectHeader((uint)other.User.PlayerId, EntityType.Player));
                writer.WriteStruct(new ObjectHeader((uint)c.User.PlayerId, EntityType.Player));
                other.SendPacket(0x4, 0x3B, 0x40, writer.ToArray());
            }

            if (InstanceName != null && ZoneManager.Instance.playerCounter.ContainsKey(InstanceName))
            {
                ZoneManager.Instance.playerCounter[InstanceName] -= 1;
                if (ZoneManager.Instance.playerCounter[InstanceName] <= 0)
                {
                    ZoneManager.Instance.playerCounter.Remove(InstanceName);
                    ZoneManager.Instance.instances.Remove(InstanceName);
                }
            }
        }

        public class GenParam
        {
            public int seed, xsize, ysize;
        }
    }
}