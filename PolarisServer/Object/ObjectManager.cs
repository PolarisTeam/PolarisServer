using Newtonsoft.Json;
using PolarisServer.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace PolarisServer.Object
{
    class ObjectManager
    {
        private static readonly ObjectManager instance = new ObjectManager();

        private Dictionary<String, Dictionary<uint, PSOObject>> zoneObjects = new Dictionary<string, Dictionary<uint, PSOObject>>();

        private ObjectManager() {}

        public static ObjectManager Instance
        {
            get
            {
                return instance;
            }
        }

        public Dictionary<uint, PSOObject> getObjectsForZone(string zone)
        {
            if(!zoneObjects.ContainsKey(zone))
            {
                //TODO Maybe make some resource management class for this stuff?
                if(!Directory.Exists("Resources/objects/" + zone))
                {
                    throw new Exception(String.Format("Unable to get objects for Zone {0}, Object folder not present.", zone));
                }

                Dictionary<uint, PSOObject> objects = new Dictionary<uint, PSOObject>();
                var objectPaths = Directory.GetFiles("Resources/objects/" + zone);
                Array.Sort(objectPaths);
                foreach (var path in objectPaths)
                {
                    if (Path.GetExtension(path) == ".bin")
                    {
                        var newObject = PSOObject.FromPacketBin(File.ReadAllBytes(path));
                        objects.Add(newObject.Header.ID, newObject);
                        Logger.WriteInternal("[OBJ] Loaded object ID {0} with name {1} pos: ({2}, {3}, {4})", newObject.Header.ID, newObject.Name, newObject.Position.X,
                            newObject.Position.Y, newObject.Position.Z);
                    }
                    else if (Path.GetExtension(path) == ".json")
                    {
                        var newObject = JsonConvert.DeserializeObject<PSOObject>(File.ReadAllText(path));
                        objects.Add(newObject.Header.ID, newObject);
                        Logger.WriteInternal("[OBJ] Loaded object ID {0} with name {1} pos: ({2}, {3}, {4})", newObject.Header.ID, newObject.Name, newObject.Position.X,
                            newObject.Position.Y, newObject.Position.Z);
                    }
                }

                zoneObjects.Add(zone, objects);
                return objects;

            }
            else
            {
                return zoneObjects[zone];
            }
        }

        internal PSOObject getObjectByID(string zone, uint ID)
        {
            if(!zoneObjects.ContainsKey(zone) || !zoneObjects[zone].ContainsKey(ID))
            {
                throw new Exception(String.Format("Object ID {0} does not exist in {1}!", ID, zone));
            }

            return zoneObjects[zone][ID];
        }
    }
}
