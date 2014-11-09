using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;

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
		public abstract void handlePacket(Client context, byte[] data,  uint position, uint size);
	}

	public static class PacketHandlers
	{

		private static Dictionary<ushort, PacketHandler> loadedHandlers = new Dictionary<ushort, PacketHandler>();
		public static void loadPacketHandlers()
		{
			var classes = from t in Assembly.GetExecutingAssembly ().GetTypes ()
					where t.IsClass && t.Namespace == "PolarisServer.Packets.Handlers" && t.IsSubclassOf (typeof(PacketHandler))
				select t;
			foreach (Type t in classes.ToList()) 
			{
				Attribute[] attrs = (Attribute[])t.GetCustomAttributes (typeof(PacketHandlerAttr), false);
				if (attrs.Length > 0) {
					PacketHandlerAttr attr = (PacketHandlerAttr)attrs[0];
					Console.WriteLine ("[PKT] Loaded PacketHandler {0} for packet {1:X}-{2:X}.", t.Name, attr.type, attr.subtype);
					loadedHandlers.Add ((ushort)((attr.type << 8) | attr.subtype), (PacketHandler)Activator.CreateInstance (t));
				}
			}
		}
		/// <summary>
		/// Gets and creates a PacketHandler for a given packet type and subtype.
		/// </summary>
		/// <returns>An instance of a PacketHandler or null</returns>
		/// <param name="typeA">Type a.</param>
		/// <param name="typeB">Type b.</param>
		public static PacketHandler getHandlerFor(uint typeA, uint typeB)
		{
			ushort packetCode = (ushort)((typeA << 8) | typeB);  // Bitshifting is fun!
			PacketHandler handler = null;
			if (loadedHandlers.ContainsKey (packetCode))
				loadedHandlers.TryGetValue (packetCode, out handler);
			return handler; //TODO Test me!

		}
	}
}

