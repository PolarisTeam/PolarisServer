using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

using Newtonsoft.Json;

using PolarisServer.Database;
using PolarisServer.Models;

namespace PolarisServer.Object
{
    class ObjectManager
    {
        private static readonly ObjectManager instance = new ObjectManager();

        private List<Tuple<string, PSOObject>> object_list = new List<Tuple<string, PSOObject>>();

        private ObjectManager() { }

        public static ObjectManager Instance
        {
            get
            {
                return instance;
            }
        }

        public PSOObject[] GetObjectsForZone(string zone)
        {
            if (zone == "tpmap") // Return empty object array for an tp'd map for now (We spawn in a teleporter manually)
            {
                return new PSOObject[0];
            }

            if (!object_list.Exists(o => o.Item1 == zone)) // Make sure objects for a zone are only loaded once
            {
                // Collect from db
                using (var db = new PolarisEf())
                {
                    var dbObjects = from dbo in db.GameObjects
                                    where dbo.ZoneName == zone
                                    select dbo;

                    if (dbObjects.Count() > 0)
                    {
                        foreach (var dbObject in dbObjects)
                        {
                            var newObject = PSOObject.FromDBObject(dbObject);
                            object_list.Add(new Tuple<string, PSOObject>(zone, newObject));
                            Logger.WriteInternal("[OBJ] Loaded object {0} for zone {1} from the DB.", newObject.Name, zone);
                        }
                    }
                    // Fallback
                    else if (Directory.Exists("Resources/objects/" + zone))
                    {
                        Logger.WriteWarning("[OBJ] No objects defined for zone {0} in the database, falling back to filesystem!", zone);
                        var objectPaths = Directory.GetFiles("Resources/objects/" + zone);
                        Array.Sort(objectPaths);
                        foreach (var path in objectPaths)
                        {
                            if (Path.GetExtension(path) == ".bin")
                            {
                                var newObject = PSOObject.FromPacketBin(File.ReadAllBytes(path));
                                object_list.Add(new Tuple<string, PSOObject>(zone, newObject));
                                Logger.WriteInternal("[OBJ] Loaded object ID {0} with name {1} pos: ({2}, {3}, {4})", newObject.Header.ID, newObject.Name, newObject.Position.PosX,
                                    newObject.Position.PosY, newObject.Position.PosZ);
                            }
                            else if (Path.GetExtension(path) == ".json")
                            {
                                var newObject = JsonConvert.DeserializeObject<PSOObject>(File.ReadAllText(path));
                                object_list.Add(new Tuple<string, PSOObject>(zone, newObject));
                                Logger.WriteInternal("[OBJ] Loaded object ID {0} with name {1} pos: ({2}, {3}, {4})", newObject.Header.ID, newObject.Name, newObject.Position.PosX,
                                    newObject.Position.PosY, newObject.Position.PosZ);
                            }
                        }
                    }
                    else
                    {
                        Logger.WriteWarning("[OBJ] Filesystem directory for zone {0} does not exist!", zone);
                    }
                }
            }

            // TODO: returning an IEnumerable<PSOObject> looks more favorable than converting to an array
            return (from o in object_list
                    where o.Item1 == zone
                    select o.Item2).ToArray();
        }

        internal PSONPC[] getNPCSForZone(string zone)
        {
            if (!object_list.Exists(o => o.Item1 == zone && o.Item2 as PSONPC != null)) // Make sure NPCs for a zone are only loaded once
            {
                using (var db = new PolarisEf())
                {
                    var dbNpcs = from n in db.NPCs
                                 where n.ZoneName == zone
                                 select n;

                    foreach (NPC npc in dbNpcs)
                    {
                        PSONPC dNpc = new PSONPC();
                        dNpc.Header = new ObjectHeader((uint)npc.EntityID, EntityType.Object);
                        dNpc.Position = new PSOLocation(npc.RotX, npc.RotY, npc.RotZ, npc.RotW, npc.PosX, npc.PosY, npc.PosZ);
                        dNpc.Name = npc.NPCName;

                        object_list.Add(new Tuple<string, PSOObject>(zone, dNpc));
                    }
                }
            }

            // TODO: returning an IEnumerable<PSONPC> looks more favorable than converting to an array
            return (from o in object_list
                    where o.Item1 == zone
                    select o.Item2 as PSONPC).ToArray();
        }

        internal PSOObject getObjectByID(string zone, uint ID)
        {
            //FIXME: This has been commented out because we were getting object errors with possible shared objects? That or it was just object 1 which is an edge case.
            //if(!zoneObjects.ContainsKey(zone) || !zoneObjects[zone].ContainsKey(ID))
            //{
            //    throw new Exception(String.Format("Object ID {0} does not exist in {1}!", ID, zone));
            //}

            //return zoneObjects[zone][ID];

            
            // TODO: Per above old comments, do shared object errors still occur? Is an object 1 edge case still an issue?
            PSOObject obj = null;
            try
            {
                if (string.IsNullOrEmpty(zone)) // Lookup by ID alone when no zone is specified
                {
                    obj = object_list.SingleOrDefault(o => o.Item2.Header.ID == ID).Item2;
                }
                else
                {
                    obj = object_list.SingleOrDefault(o => o.Item1 == zone && o.Item2.Header.ID == ID).Item2;
                }
            }
            catch (InvalidOperationException)
            {
                Logger.WriteWarning("[OBJ] Multiple objects with ID {0} exist, unknown which is intended.", ID);
            }
            catch (NullReferenceException)
            {
                // SingleOrDefault returns null when no object is found, access .Item2 anyway and catch null to simplify things
                Logger.WriteWarning("[OBJ] Client requested object {0} which we don't know about. Investigate.", ID);
            }

            return obj != null ? obj : new PSOObject() { Header = new ObjectHeader(ID, EntityType.Object), Name = "Unknown" };
        }

        internal PSOObject getObjectByID(uint ID)
        {
            return getObjectByID(null, ID);
        }
    }
}
