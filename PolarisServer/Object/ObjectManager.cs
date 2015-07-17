using Newtonsoft.Json;
using PolarisServer.Database;
using PolarisServer.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace PolarisServer.Object
{
    class ObjectManager
    {
        private static readonly ObjectManager instance = new ObjectManager();

        private Dictionary<String, Dictionary<ulong, PSOObject>> zoneObjects = new Dictionary<string, Dictionary<ulong, PSOObject>>();

        private Dictionary<ulong, PSOObject> allTheObjects = new Dictionary<ulong, PSOObject>();

        private ObjectManager() { }

        public static ObjectManager Instance
        {
            get
            {
                return instance;
            }
        }

        public PSOObject[] getObjectsForZone(string zone)
        {
            if (zone == "tpmap")
            {
                return new PSOObject[0];
            }
            if (!zoneObjects.ContainsKey(zone))
            {
                //TODO Maybe make some resource management class for this stuff?
                if (!Directory.Exists("Resources/objects/" + zone))
                {
                    throw new Exception(String.Format("Unable to get objects for Zone {0}, Object folder not present.", zone));
                }

                Dictionary<ulong, PSOObject> objects = new Dictionary<ulong, PSOObject>();
                var objectPaths = Directory.GetFiles("Resources/objects/" + zone);
                Array.Sort(objectPaths);
                foreach (var path in objectPaths)
                {
                    if (Path.GetExtension(path) == ".bin")
                    {
                        var newObject = PSOObject.FromPacketBin(File.ReadAllBytes(path));
                        objects.Add(newObject.Header.ID, newObject);
                        allTheObjects.Add(newObject.Header.ID, newObject);
                        Logger.WriteInternal("[OBJ] Loaded object ID {0} with name {1} pos: ({2}, {3}, {4})", newObject.Header.ID, newObject.Name, newObject.Position.PosX,
                            newObject.Position.PosY, newObject.Position.PosZ);
                    }
                    else if (Path.GetExtension(path) == ".json")
                    {
                        var newObject = JsonConvert.DeserializeObject<PSOObject>(File.ReadAllText(path));
                        objects.Add(newObject.Header.ID, newObject);
                        allTheObjects.Add(newObject.Header.ID, newObject);
                        Logger.WriteInternal("[OBJ] Loaded object ID {0} with name {1} pos: ({2}, {3}, {4})", newObject.Header.ID, newObject.Name, newObject.Position.PosX,
                            newObject.Position.PosY, newObject.Position.PosZ);
                    }
                }

                zoneObjects.Add(zone, objects);
                return objects.Values.ToArray();

            }
            else
            {
                return zoneObjects[zone].Values.ToArray();
            }
        }

        internal PSONPC[] getNPCSForZone(string zone)
        {
            List<PSONPC> npcs = new List<PSONPC>();
            using (var db = new PolarisEf())
            {
                var dbNpcs = from n in db.NPCs
                             where n.ZoneName == zone
                             select n;

                foreach(NPC npc in dbNpcs)
                {
                    PSONPC dNpc = new PSONPC();
                    dNpc.Header = new ObjectHeader(npc.EntityID, EntityType.Object);
                    dNpc.Position = new PSOLocation(npc.RotX, npc.RotY, npc.RotZ, npc.RotW, npc.PosX, npc.PosY, npc.PosZ);
                    dNpc.Name = npc.NPCName;

                    npcs.Add(dNpc);
                    if (!zoneObjects[zone].ContainsKey(dNpc.Header.ID))
                    {
                        zoneObjects[zone].Add(dNpc.Header.ID, dNpc);
                    }
                    if (!allTheObjects.ContainsKey(dNpc.Header.ID))
                        allTheObjects.Add(dNpc.Header.ID, dNpc);
                }
            }

                return npcs.ToArray();
        }

        internal PSOObject getObjectByID(string zone, uint ID)
        {
            //if(!zoneObjects.ContainsKey(zone) || !zoneObjects[zone].ContainsKey(ID))
            //{
            //    throw new Exception(String.Format("Object ID {0} does not exist in {1}!", ID, zone));
            //}

            //return zoneObjects[zone][ID];
            return getObjectByID(ID);
        }

        internal PSOObject getObjectByID(uint ID)
        {
            if (!allTheObjects.ContainsKey(ID))
            {
                Logger.WriteWarning("[OBJ] Client requested object {0} which we don't know about. Investigate.", ID);
                return new PSOObject() { Header = new ObjectHeader(ID, EntityType.Object), Name = "Unknown" };
            }

            return allTheObjects[ID];
        }
    }
}
