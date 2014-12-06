using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PolarisServer.Packets.Handlers
{
    public class PacketHandlerAttr : Attribute
    {
        public uint type, subtype;

        public PacketHandlerAttr(uint type, uint subtype)
        {
            this.type = type;
            this.subtype = subtype;
        }
    }

    public abstract class PacketHandler
    {
        public abstract void HandlePacket(Client context, byte[] data, uint position, uint size);
    }

    public static class PacketHandlers
    {
        private static Dictionary<ushort, PacketHandler> handlers = new Dictionary<ushort, PacketHandler>();

        public static void LoadPacketHandlers()
        {
            var classes = from t in Assembly.GetExecutingAssembly().GetTypes()
                          where t.IsClass && t.Namespace == "PolarisServer.Packets.Handlers" && t.IsSubclassOf(typeof(PacketHandler))
                          select t;
            
            foreach (Type t in classes.ToList())
            {
                Attribute[] attrs = (Attribute[])t.GetCustomAttributes(typeof(PacketHandlerAttr), false);
                
                if (attrs.Length > 0)
                {
                    PacketHandlerAttr attr = (PacketHandlerAttr)attrs[0];
                    Logger.WriteInternal("[PKT] Loaded PacketHandler {0} for packet {1:X}-{2:X}.", t.Name, attr.type, attr.subtype);
                    if (!handlers.ContainsKey(Helper.PacketTypeToUShort(attr.type, attr.subtype)))
                        handlers.Add(Helper.PacketTypeToUShort(attr.type, attr.subtype), (PacketHandler)Activator.CreateInstance(t));
                }
            }
        }

        /// <summary>
        /// Gets and creates a PacketHandler for a given packet type and subtype.
        /// </summary>
        /// <returns>An instance of a PacketHandler or null</returns>
        /// <param name="type">Type a.</param>
        /// <param name="subtype">Type b.</param>
        public static PacketHandler GetHandlerFor(uint type, uint subtype)
        {
            ushort packetCode = Helper.PacketTypeToUShort(type, subtype);
            PacketHandler handler = null;
            
            if (handlers.ContainsKey(packetCode))
                handlers.TryGetValue(packetCode, out handler);
            
            return handler;
        }

        public static PacketHandler[] GetLoadedHandlers()
        {
            return handlers.Values.ToArray();
        }
    }
}

